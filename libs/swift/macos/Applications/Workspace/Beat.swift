import AppKit
import ApplicationServices
import AsyncAlgorithms
import CasePaths
import CasePathsCore
import Processes
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let beatAttributes: [String] = [kAXWindowsAttribute, kAXFocusedWindowAttribute, kAXFrontmostAttribute]
private let modalSessionTag: UInt64 = 0x8000_0000

// --- [MODELS] --------------------------------------------------------------------------

struct Beat: Sendable {
    var state: InstanceState?
    var observer: Result<Observer, ApplicationFailure>?
    var status: InstanceState.Running.Status?
    var active: Bool?
    var tokens: [Data?]?
    var numbers: Set<CGWindowID> = []
    var described: [CGWindowID: Element] = [:]
    var menus: [Element] = []
    var answered: Set<CGWindowID>?
    var baseline: Set<CGWindowID>?
    var mainThread: Duration?
    var usage: Result<ResourceUsage, Errno>?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func modalSessions(among windows: some Sequence<CGWindowID>) -> Set<CGWindowID> {
    guard let skyLight: UnsafeMutableRawPointer = unsafe dlopen("/System/Library/PrivateFrameworks/SkyLight.framework/SkyLight", RTLD_NOW) else { return [] }
    func symbol<Function>(_ name: String) -> Function? { unsafe dlsym(skyLight, name).map { address in unsafe unsafeBitCast(address, to: Function.self) } }
    guard let connection: @convention(c) () -> Int32 = symbol("SLSMainConnectionID"),
        let query: @convention(c) (Int32, CFArray, Int32) -> Unmanaged<CFTypeRef>? = unsafe symbol("SLSWindowQueryWindows"),
        let results: @convention(c) (CFTypeRef) -> Unmanaged<CFTypeRef>? = unsafe symbol("SLSWindowQueryResultCopyWindows"),
        let advance: @convention(c) (CFTypeRef) -> Bool = symbol("SLSWindowIteratorAdvance"),
        let tags: @convention(c) (CFTypeRef) -> UInt64 = symbol("SLSWindowIteratorGetTags")
    else { return [] }
    return Set(
        windows.filter { number in
            if let found: CFTypeRef = unsafe query(connection(), [NSNumber(value: number)] as CFArray, 1)?.takeRetainedValue(),
                let iterator: CFTypeRef = unsafe results(found)?.takeRetainedValue(), advance(iterator)
            {
                tags(iterator) & modalSessionTag != 0
            } else {
                false
            }
        }
    )
}

private func blocking(_ number: CGWindowID, in beat: Beat, tagged: Bool, flat: Bool) -> InstanceState.Window.Description {
    switch (beat.described[number], tagged, flat && beat.answered.map { answered in !answered.contains(number) } == true) {
        case (.some(let element), _, _) where element.asking || !tagged: .element(element)
        case (_, true, _), (.none, false, true): .blocking
        case (_, false, _): .undescribed
    }
}

extension NSRunningApplication {
    var isDialogHelper: Bool { bundleIdentifier.map(dialogHelpers.contains) == true }

    private func processStatus() throws(ApplicationFailure) -> ProcessStatus {
        do { return try ProcessStatus(of: processIdentifier, mainThread: true) } catch { throw ApplicationFailure(error, process: processIdentifier) }
    }

    private func rows(_ listed: [InstanceState.Window], in beat: Beat, reading once: Bool, _ description: (CGWindowID) -> InstanceState.Window.Description) -> [InstanceState.Window] {
        let numbers: Set<CGWindowID> = Set(listed.map(\.number))
        return (listed + beat.described.keys.filter { number in !numbers.contains(number) }.compactMap(InstanceState.Window.listed(number:))).map { window in
            window.described(description(window.number), arrived: beat.baseline.map { baseline in !baseline.contains(window.number) } ?? once)
        }
    }

    private func answering(
        after held: Beat,
        sampling: Bool,
        listed: [InstanceState.Window],
        walking walked: (Element) -> Bool,
        heartbeat: Bool,
        ticks: AsyncStream<Void>.Continuation?,
    ) throws(ApplicationFailure) -> Beat {
        var next: Beat = held
        let application: AXUIElement = AXUIElement.application(processIdentifier)
        if let ticks {
            next.observer = next.observer ?? Result { () throws(ApplicationFailure) in try Observer(self, ticks: ticks) }
            try next.observer?.get().register(on: application)
        }
        let read: [String: CFTypeRef] = try application.values(beatAttributes)
        let tokens: [Data?] =
            (read[kAXWindowsAttribute] as? [CFTypeRef] ?? []).compactMap(AXUIElement.checked).map(\.token) + [read[kAXFocusedWindowAttribute].flatMap(AXUIElement.checked)?.token]
        let numbers: Set<CGWindowID> = Set(listed.map(\.number))
        if !heartbeat || next.tokens != tokens || next.numbers != numbers {
            let helper: Bool = isDialogHelper
            let elements: [(window: CGWindowID?, element: Element)] = try AXUIElement.candidates(read).map { candidate throws(ApplicationFailure) in
                try candidate.described(walking: walked, helper: helper)
            }
            .compactMap(\.self)
            next.described = Dictionary(elements.compactMap { row in row.window.map { window in (window, row.element) } }) { first, second in first.asking ? first : second }
            let bars: [String: CFTypeRef] = try application.values(menuBars)
            let barred: [Element] = try menuBars.enumerated().map { index, bar throws(ApplicationFailure) in
                try bars[bar].flatMap(AXUIElement.checked)?.described(walking: walked, helper: helper, bar: index)?.element
            }
            .compactMap(\.self)
            next.menus = try barred + application.applicationMenus().map { menu throws(ApplicationFailure) in try menu.described(walking: walked, helper: helper)?.element }.compactMap(\.self)
        }
        let status: InstanceState.Running.Status = isFinishedLaunching || next.described.values.contains(where: \.asking) ? .responding : .launching
        next.status = status
        next.active = read[kAXFrontmostAttribute] as? Bool ?? isActive
        next.tokens = tokens
        next.numbers = numbers
        next.answered = numbers
        next.mainThread = nil
        if !sampling, status == .responding, next.baseline == nil { next.baseline = numbers }
        return concluded(next, status: status, sampling: sampling, listed: listed) { number in next.described[number].map(InstanceState.Window.Description.element) ?? .undescribed }
    }

    private func silent(after held: Beat, sampling: Bool, process: ProcessStatus, listed: [InstanceState.Window]) throws(ApplicationFailure) -> Beat {
        let time: Duration
        do { time = try mainThreadUserTime(of: processIdentifier) } catch { throw ApplicationFailure(error, process: processIdentifier) }
        let tagged: Set<CGWindowID> = modalSessions(among: listed.map(\.number))
        let flat: Bool = isFinishedLaunching && held.mainThread == time
        let status: InstanceState.Running.Status =
            switch (isFinishedLaunching, held.mainThread == nil, held.status) {
                case (false, _, _): .launching
                case (true, true, .some(let answered)): answered
                case (true, _, _): .unresponsive(process)
            }
        var next: Beat = held
        next.status = status
        next.mainThread = time
        return concluded(next, status: status, sampling: sampling, listed: listed) { number in blocking(number, in: held, tagged: tagged.contains(number), flat: flat) }
    }

    private func concluded(
        _ beat: Beat,
        status: InstanceState.Running.Status,
        sampling: Bool,
        listed: [InstanceState.Window],
        _ description: (CGWindowID) -> InstanceState.Window.Description,
    ) -> Beat {
        var next: Beat = beat
        if status.is(\.unresponsive), next.baseline == nil, isFinishedLaunching { next.baseline = [] }
        next.usage =
            switch (sampling, status, beat.usage) {
                case (false, .unresponsive, .some(let usage)): usage
                case (false, .launching, _), (false, .responding, _): nil
                case (true, _, _), (false, .unresponsive, .none): Result { () throws(Errno) in try ResourceUsage(of: processIdentifier) }
            }
        next.state = .running(
            InstanceState.Running(
                status: status,
                active: next.active ?? isActive,
                windows: rows(listed, in: next, reading: sampling, description),
                prompts: [],
                menus: next.menus,
                usage: next.usage,
            )
        )
        return next
    }

    @concurrent
    func beat(
        after last: Beat?,
        walking walked: @Sendable (Element) -> Bool,
        heartbeat: Bool = false,
        ticks: AsyncStream<Void>.Continuation? = nil,
    ) async -> Beat {
        await withTaskExecutorPreference(accessibility) { () -> Beat in
            let listed: [InstanceState.Window] = InstanceState.Window.owned(by: processIdentifier).filter { window in !window.frame.isEmpty }
            let held: Beat = last ?? Beat()
            let sampling: Bool = last == nil
            do throws(ApplicationFailure) {
                let process: ProcessStatus = try processStatus()
                guard process != .stopped else {
                    return concluded(held, status: .unresponsive(.stopped), sampling: sampling, listed: listed) { number in
                        held.described[number].map(InstanceState.Window.Description.element) ?? .undescribed
                    }
                }
                guard activationPolicy != .prohibited else {
                    return concluded(held, status: .responding, sampling: sampling, listed: listed) { _ in .undescribed }
                }
                do throws(ApplicationFailure) {
                    return try answering(after: held, sampling: sampling, listed: listed, walking: walked, heartbeat: heartbeat, ticks: ticks)
                } catch  where error.reason.is(\.unanswered) {
                    return try silent(after: held, sampling: sampling, process: process, listed: listed)
                }
            } catch {
                var failed: Beat = held
                failed.state = .failed(error)
                return failed
            }
        }
    }

    func beats(walking walked: @escaping @Sendable (Element) -> Bool, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        return merge(chain([()].async, ticks).map { _ in false }, AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in true })
            .reductions(Beat()) { last, heartbeat in await self.beat(after: last, walking: walked, heartbeat: heartbeat, ticks: tick) }
            .compactMap(\.state)
            .prefix { state in state[case: \.failed]?.reason.is(\.noProcess) != true }
    }
}
