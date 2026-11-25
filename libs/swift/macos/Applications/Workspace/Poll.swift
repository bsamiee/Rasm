import AppKit
import ApplicationServices
import AsyncAlgorithms
import CasePathsCore
import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

struct Poll: Sendable {
    var isSingleRead: Bool = false
    var state: InstanceState?
    var observer: Result<Observer, ApplicationFailure>?
    var status: InstanceState.Running.Status?
    var active: Bool?
    var tokens: [Data?]?
    var numbers: Set<CGWindowID> = []
    var described: [CGWindowID: TopLevelElement] = [:]
    var menus: [TopLevelElement] = []
    var answered: Set<CGWindowID>?
    var baseline: Set<CGWindowID>?
    var isSilent: Bool = false
    var usage: Result<ResourceUsage, Errno>?
    var listening: Result<[in_port_t], Errno> = .success([])
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Instance {
    private func rows(_ listed: [InstanceState.Window], in poll: Poll, _ description: (CGWindowID) -> InstanceState.Window.Description) -> [InstanceState.Window] {
        let numbers = Set(listed.map(\.number))
        return (listed + poll.described.keys.filter { number in !numbers.contains(number) }.compactMap(InstanceState.Window.read(number:))).map { window in
            window.described(description(window.number), hasArrived: poll.baseline.map { baseline in !baseline.contains(window.number) } ?? poll.isSingleRead)
        }
    }

    private func read(
        after held: Poll,
        listed: [InstanceState.Window],
        walking walked: (TopLevelElement) -> Bool,
        heartbeat: Bool,
        ticks: AsyncStream<Void>.Continuation?,
    ) throws(ApplicationFailure) -> Poll {
        var next = held
        let element = AXUIElement.application(process.pid)
        if let ticks {
            next.observer = next.observer ?? Result { () throws(ApplicationFailure) in try Observer(application, ticks: ticks) }
            try next.observer?.get().register(on: element)
        }
        let read = try element.requireValues([kAXWindowsAttribute, kAXFocusedWindowAttribute, kAXFrontmostAttribute])
        let tokens =
            (read.values[kAXWindowsAttribute] as? [CFTypeRef] ?? []).compactMap(AXUIElement.cast).map(\.token) + [read.values[kAXFocusedWindowAttribute].flatMap(AXUIElement.cast)?.token]
        let numbers = Set(listed.map(\.number))
        if !heartbeat || next.tokens != tokens || next.numbers != numbers {
            let helper = application.isDialogHelper
            let elements = try AXUIElement.candidates(read).map { candidate throws(ApplicationFailure) in
                try candidate.describe(walking: walked, helper: helper, among: numbers)
            }
            .compactMap(\.self)
            next.described = Dictionary(elements.compactMap { row in row.window.map { window in (window, row.element) } }) { first, second in first.asking ? first : second }
            let bars = try element.requireValues(menuBars)
            let barred = try menuBars.enumerated().map { index, bar throws(ApplicationFailure) in
                try bars.values[bar].flatMap(AXUIElement.cast)?.describe(walking: walked, helper: helper, among: numbers, bar: index)?.element
            }
            .compactMap(\.self)
            next.menus =
                try barred + element.menus([kAXMenuRole]).map { menu throws(ApplicationFailure) in try menu.describe(walking: walked, helper: helper, among: numbers)?.element }.compactMap(\.self)
        }
        let status: InstanceState.Running.Status = application.isFinishedLaunching || next.described.values.contains(where: \.asking) ? .responding : .launching
        next.status = status
        next.active = read.values[kAXFrontmostAttribute] as? Bool ?? application.isActive
        next.tokens = tokens
        next.numbers = numbers
        next.answered = numbers
        next.isSilent = false
        if !next.isSingleRead, status == .responding, next.baseline == nil { next.baseline = numbers }
        return conclude(next, status: status, listed: listed) { number in next.described[number].map(InstanceState.Window.Description.element) ?? .undescribed }
    }

    private func inferStatus(after held: Poll, thread: ProcessStatus, listed: [InstanceState.Window]) -> Poll {
        let status: InstanceState.Running.Status? =
            switch (application.isFinishedLaunching, held.isSilent, thread, held.status) {
                case (false, true, .sleeping, _): .unresponsive(thread)
                case (false, _, _, _): .launching
                case (true, false, _, .some(let answered)), (true, true, .runnable, .some(let answered)): answered
                case (true, false, .runnable, .none): nil
                case (true, _, _, _): .unresponsive(thread)
            }
        var next = held
        next.isSilent = true
        guard let status else { return next }
        next.status = status
        let modal = Int(CGWindowLevelForKey(.modalPanelWindow))
        let shown: [InstanceState.Window] = thread == .sleeping ? listed.filter(\.onScreen) : []
        let tagged = Set(shown.filter { window in window.layer == modal }.map(\.number))
        let parked = application.isFinishedLaunching && held.isSilent
        let fresh: Set<CGWindowID> = parked ? Set(shown.filter { window in held.answered?.contains(window.number) == false }.map(\.number)) : []
        return conclude(next, status: status, listed: listed) { number in
            switch (held.described[number], tagged.contains(number), fresh.contains(number)) {
                case (.some(let element), let isTagged, _) where element.asking || !isTagged: .element(element)
                case (_, true, _), (.none, false, true): .blocking
                case (_, false, _): .undescribed
            }
        }
    }

    private func conclude(
        _ poll: Poll,
        status: InstanceState.Running.Status,
        listed: [InstanceState.Window],
        _ description: (CGWindowID) -> InstanceState.Window.Description,
    ) -> Poll {
        var next = poll
        if status.is(\.unresponsive), next.baseline == nil, application.isFinishedLaunching { next.baseline = [] }
        next.usage =
            switch (poll.isSingleRead, status, poll.usage) {
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
                listening: next.listening,
            )
        )
        return next
    }

    @concurrent
    func poll(
        after held: Poll,
        walking walked: @Sendable (TopLevelElement) -> Bool,
        heartbeat: Bool = false,
        ticks: AsyncStream<Void>.Continuation? = nil,
    ) async -> Poll {
        await withTaskExecutorPreference(accessibilityQueue) { () -> Poll in
            let listed = InstanceState.Window.all(ownedBy: process.pid).filter { window in !window.frame.isEmpty }
            do throws(ApplicationFailure) {
                var held = held
                let before: MainThread
                do throws(Errno) { before = try MainThread(of: process) } catch { throw ApplicationFailure(error, process: String(process.pid)) }
                held.listening = Result { () throws(Errno) in try process.listeningPorts() }
                guard !before.stopped else {
                    return conclude(held, status: .unresponsive(.stopped), listed: listed) { number in
                        held.described[number].map(InstanceState.Window.Description.element) ?? .undescribed
                    }
                }
                guard application.activationPolicy != .prohibited else {
                    return conclude(held, status: .responding, listed: listed) { _ in .undescribed }
                }
                let answer = Result { () throws(ApplicationFailure) in try read(after: held, listed: listed, walking: walked, heartbeat: heartbeat, ticks: ticks) }
                guard case .failure(let failure) = answer, failure.reason.is(\.unanswered) else { return try answer.get() }
                let after: MainThread
                do throws(Errno) { after = try MainThread(of: process) } catch { throw ApplicationFailure(error, process: String(process.pid)) }
                return inferStatus(after: held, thread: after.status(since: before), listed: listed)
            } catch {
                var failed = held
                failed.state = .failed(error)
                return failed
            }
        }
    }

    func polls(walking walked: @escaping @Sendable (TopLevelElement) -> Bool, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let (ticks, tick) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let helper = application.isDialogHelper
        return merge(chain([()].async, ticks).map { _ in false }, AsyncStream { (try? await clock.sleep(for: messagingTimeout)).map { true } })
            .reductions(Poll()) { last, heartbeat in
                heartbeat && helper && last.state?[case: \.running]?.status == .responding
                    ? last : await self.poll(after: last, walking: walked, heartbeat: heartbeat, ticks: tick)
            }
            .compactMap(\.state)
            .prefix { state in state[case: \.failed]?.reason.is(\.noProcess) != true }
    }
}
