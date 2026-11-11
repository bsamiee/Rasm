import AppKit
import ApplicationServices
import AsyncAlgorithms
import CasePathsCore
import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

struct Beat: Sendable {
    var once: Bool = false
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
    var silent: Bool = false
    var usage: Result<ResourceUsage, Errno>?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    var isDialogHelper: Bool { bundleIdentifier.map(dialogHelpers.contains) == true }
}

extension Instance {
    private func rows(_ listed: [InstanceState.Window], in beat: Beat, _ description: (CGWindowID) -> InstanceState.Window.Description) -> [InstanceState.Window] {
        let numbers = Set(listed.map(\.number))
        return (listed + beat.described.keys.filter { number in !numbers.contains(number) }.compactMap(InstanceState.Window.listed(number:))).map { window in
            window.described(description(window.number), arrived: beat.baseline.map { baseline in !baseline.contains(window.number) } ?? beat.once)
        }
    }

    private func answering(
        after held: Beat,
        listed: [InstanceState.Window],
        walking walked: (Element) -> Bool,
        heartbeat: Bool,
        ticks: AsyncStream<Void>.Continuation?,
    ) throws(ApplicationFailure) -> Beat {
        var next = held
        let element = AXUIElement.application(process.pid)
        if let ticks {
            next.observer = next.observer ?? Result { () throws(ApplicationFailure) in try Observer(application, ticks: ticks) }
            try next.observer?.get().register(on: element)
        }
        let read = try element.values([kAXWindowsAttribute, kAXFocusedWindowAttribute, kAXFrontmostAttribute])
        let tokens =
            (read[kAXWindowsAttribute] as? [CFTypeRef] ?? []).compactMap(AXUIElement.checked).map(\.token) + [read[kAXFocusedWindowAttribute].flatMap(AXUIElement.checked)?.token]
        let numbers = Set(listed.map(\.number))
        if !heartbeat || next.tokens != tokens || next.numbers != numbers {
            let helper = application.isDialogHelper
            let elements = try AXUIElement.candidates(read).map { candidate throws(ApplicationFailure) in
                try candidate.described(walking: walked, helper: helper, among: listed)
            }
            .compactMap(\.self)
            next.described = Dictionary(elements.compactMap { row in row.window.map { window in (window, row.element) } }) { first, second in first.asking ? first : second }
            let bars = try element.values(menuBars)
            let barred = try menuBars.enumerated().map { index, bar throws(ApplicationFailure) in
                try bars[bar].flatMap(AXUIElement.checked)?.described(walking: walked, helper: helper, among: listed, bar: index)?.element
            }
            .compactMap(\.self)
            next.menus =
                try barred + element.applicationMenus().map { menu throws(ApplicationFailure) in try menu.described(walking: walked, helper: helper, among: listed)?.element }.compactMap(\.self)
        }
        let status: InstanceState.Running.Status = application.isFinishedLaunching || next.described.values.contains(where: \.asking) ? .responding : .launching
        next.status = status
        next.active = read[kAXFrontmostAttribute] as? Bool ?? application.isActive
        next.tokens = tokens
        next.numbers = numbers
        next.answered = numbers
        next.silent = false
        if !next.once, status == .responding, next.baseline == nil { next.baseline = numbers }
        return concluded(next, status: status, listed: listed) { number in next.described[number].map(InstanceState.Window.Description.element) ?? .undescribed }
    }

    private func silent(after held: Beat, thread: ProcessStatus, listed: [InstanceState.Window]) -> Beat {
        let status: InstanceState.Running.Status? =
            switch (application.isFinishedLaunching, held.silent, thread, held.status) {
                case (false, true, .sleeping, _): .unresponsive(thread)
                case (false, _, _, _): .launching
                case (true, false, _, .some(let answered)), (true, true, .runnable, .some(let answered)): answered
                case (true, false, .runnable, .none): nil
                case (true, _, _, _): .unresponsive(thread)
            }
        var next = held
        next.silent = true
        guard let status else { return next }
        next.status = status
        let modal = Int(CGWindowLevelForKey(.modalPanelWindow))
        let tagged = Set(listed.filter { window in window.layer == modal && window.onScreen }.map(\.number))
        let parked = application.isFinishedLaunching && held.silent && thread == .sleeping
        return concluded(next, status: status, listed: listed) { number in
            switch (held.described[number], tagged.contains(number), parked && held.answered.map { answered in !answered.contains(number) } == true) {
                case (.some(let element), let isTagged, _) where element.asking || !isTagged: .element(element)
                case (_, true, _), (.none, false, true): .blocking
                case (_, false, _): .undescribed
            }
        }
    }

    private func concluded(
        _ beat: Beat,
        status: InstanceState.Running.Status,
        listed: [InstanceState.Window],
        _ description: (CGWindowID) -> InstanceState.Window.Description,
    ) -> Beat {
        var next = beat
        if status.is(\.unresponsive), next.baseline == nil, application.isFinishedLaunching { next.baseline = [] }
        next.usage =
            switch (beat.once, status, beat.usage) {
                case (false, .unresponsive, .some(let usage)): usage
                case (false, .launching, _), (false, .responding, _): nil
                case (true, _, _), (false, .unresponsive, .none): Result { () throws(Errno) in try ResourceUsage(of: process) }
            }
        next.state = .running(
            InstanceState.Running(
                status: status,
                active: next.active ?? application.isActive,
                windows: rows(listed, in: next, description),
                prompts: [],
                menus: next.menus,
                usage: next.usage,
            )
        )
        return next
    }

    @concurrent
    func beat(
        after held: Beat,
        walking walked: @Sendable (Element) -> Bool,
        heartbeat: Bool = false,
        ticks: AsyncStream<Void>.Continuation? = nil,
    ) async -> Beat {
        await withTaskExecutorPreference(accessibility) { () -> Beat in
            let listed = InstanceState.Window.owned(by: process.pid).filter { window in !window.frame.isEmpty }
            do throws(ApplicationFailure) {
                let before: MainThread
                do throws(Errno) { before = try MainThread(of: process) } catch { throw ApplicationFailure(error, process: String(process.pid)) }
                guard !before.stopped else {
                    return concluded(held, status: .unresponsive(.stopped), listed: listed) { number in
                        held.described[number].map(InstanceState.Window.Description.element) ?? .undescribed
                    }
                }
                guard application.activationPolicy != .prohibited else {
                    return concluded(held, status: .responding, listed: listed) { _ in .undescribed }
                }
                let answer = Result { () throws(ApplicationFailure) in try answering(after: held, listed: listed, walking: walked, heartbeat: heartbeat, ticks: ticks) }
                guard case .failure(let failure) = answer, failure.reason.is(\.unanswered) else { return try answer.get() }
                let after: MainThread
                do throws(Errno) { after = try MainThread(of: process) } catch { throw ApplicationFailure(error, process: String(process.pid)) }
                return silent(after: held, thread: after.status(since: before), listed: listed)
            } catch {
                var failed = held
                failed.state = .failed(error)
                return failed
            }
        }
    }

    func beats(walking walked: @escaping @Sendable (Element) -> Bool, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let (ticks, tick) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let heartbeats = AsyncStream { (try? await clock.sleep(for: messagingTimeout)).map { true } }
        return merge(chain([()].async, ticks).map { _ in false }, heartbeats)
            .reductions(Beat()) { last, heartbeat in await self.beat(after: last, walking: walked, heartbeat: heartbeat, ticks: tick) }
            .compactMap(\.state)
            .prefix { state in state[case: \.failed]?.reason.is(\.noProcess) != true }
    }
}
