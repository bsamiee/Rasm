public import AppKit
import AsyncAlgorithms
public import CasePaths
import CasePathsCore
public import Encoding
public import OrderedCollections
public import Processes
import Subprocess
import Synchronization
public import System

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: Equatable, Sendable {
    public struct Running: Equatable, Sendable {
        @CasePathable
        public enum Status: CaseLabeled, Equatable, Sendable {
            case launching
            case responding
            case unresponsive(ProcessStatus)

            public var isIdle: Bool {
                switch self {
                    case .launching, .unresponsive(.running), .unresponsive(.runnable): false
                    case .responding, .unresponsive(.sleeping), .unresponsive(.stopped): true
                }
            }
        }

        public let status: Status
        public let active: Bool
        public let windows: [Window]
        public let prompts: [Window]
        public let menus: [TopLevelElement]
        public let usage: Result<ResourceUsage, Errno>?
        public let listening: Result<[in_port_t], Errno>

        public var asking: [Window] { (windows + prompts).filter(\.asking) }
    }

    public struct Window: Equatable, Sendable {
        @CasePathable
        public enum Description: CaseLabeled, Equatable, Sendable {
            case element(TopLevelElement)
            case undescribed
            case blocking

            public func encode(to encoder: any Encoder) throws {
                switch self {
                    case .element(let element): try element.kind.encode(to: encoder)
                    case .undescribed, .blocking: try caseLabel.encode(to: encoder)
                }
            }
        }

        public let number: CGWindowID
        public let title: String?
        public let frame: CGRect
        public let layer: Int
        public let onScreen: Bool
        public let description: Description
        let hasArrived: Bool

        public var asking: Bool {
            switch description {
                case .element(let element): element.asking
                case .undescribed: false
                case .blocking: true
            }
        }

        public var presented: Bool {
            switch description {
                case .element, .blocking: true
                case .undescribed: hasArrived && onScreen
            }
        }
    }

    public struct Ended: Equatable, Sendable {
        public let exit: Result<ProcessExit, Errno>
        public let report: Result<DiagnosticReport, ProcessFailure>?
    }

    case running(Running)
    case failed(ApplicationFailure)
    case exited(Ended)
}

extension InstanceState.Window {
    init?(_ row: [String: Any]) {
        guard let number = row[kCGWindowNumber as String] as? CGWindowID, let layer = row[kCGWindowLayer as String] as? Int, layer >= 0,
            let frame = (row[kCGWindowBounds as String] as? [String: Any]).flatMap({ bounds in CGRect(dictionaryRepresentation: bounds as CFDictionary) })
        else { return nil }
        self.init(
            number: number,
            title: (row[kCGWindowName as String] as? String).flatMap { title in title.isEmpty ? nil : title },
            frame: frame,
            layer: layer,
            onScreen: row[kCGWindowIsOnscreen as String] as? Bool ?? false,
            description: .undescribed,
            hasArrived: false,
        )
    }

    static func all(ownedBy process: pid_t) -> [Self] {
        (CGWindowListCopyWindowInfo(.optionAll, kCGNullWindowID) as? [[String: Any]] ?? []).filter { row in row[kCGWindowOwnerPID as String] as? pid_t == process }.compactMap(Self.init)
    }

    static func read(number: CGWindowID) -> Self? {
        (CGWindowListCopyWindowInfo(.optionIncludingWindow, number) as? [[String: Any]] ?? []).lazy.compactMap(Self.init).first
    }

    func described(_ description: Description, hasArrived: Bool) -> Self {
        Self(number: number, title: title, frame: frame, layer: layer, onScreen: onScreen, description: description, hasArrived: hasArrived)
    }
}

extension InstanceState: Encodable {
    private enum Key: CodingKey {
        case running, failed, exited
    }

    public func encode(to encoder: any Encoder) throws {
        var state = encoder.container(keyedBy: Key.self)
        switch self {
            case .running(let running): try state.encode(running, forKey: .running)
            case .failed(let failure): try state.encode(failure, forKey: .failed)
            case .exited(let ended): try state.encode(ended, forKey: .exited)
        }
    }
}

extension InstanceState.Ended: Encodable {
    private enum Key: CodingKey {
        case status, signal, detail, statusFailure, report, reportFailure, reporting
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        switch exit {
            case .success(let ended):
                switch ended.status {
                    case .exited(let status): try fields.encode(status, forKey: .status)
                    case .signaled(let signal): try fields.encode(signal, forKey: .signal)
                }
                if !ended.detail.isEmpty { try fields.encode(ended.detail, forKey: .detail) }
                if report == nil, ended.producesReport { try fields.encode(true, forKey: .reporting) }
            case .failure(let error): try fields.encode(error.description, forKey: .statusFailure)
        }
        switch report {
            case .some(.success(let report)): try fields.encode(report, forKey: .report)
            case .some(.failure(let failure)): try fields.encode(failure.reason, forKey: .reportFailure)
            case .none: break
        }
    }
}

extension InstanceState.Running: Encodable {
    private enum Key: CodingKey {
        case active, menus, process, status, usage, usageFailure, windows
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(active, forKey: .active)
        if !menus.isEmpty { try fields.encode(menus, forKey: .menus) }
        try fields.encode(status, forKey: .status)
        try fields.encodeIfPresent(status[case: \.unresponsive], forKey: .process)
        switch usage {
            case .some(.success(let usage)): try fields.encode(usage, forKey: .usage)
            case .some(.failure(let error)): try fields.encode(error.description, forKey: .usageFailure)
            case .none: break
        }
        try fields.encode(asking + windows.filter { window in !window.asking }, forKey: .windows)
    }
}

extension InstanceState.Window: Encodable {
    private enum Key: CodingKey {
        case asking, element, frame, kind, layer, number, onScreen, presented, title
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        if asking { try fields.encode(true, forKey: .asking) }
        try fields.encode(frame, forKey: .frame)
        try fields.encode(layer, forKey: .layer)
        try fields.encode(number, forKey: .number)
        try fields.encode(onScreen, forKey: .onScreen)
        try fields.encodeIfPresent(title, forKey: .title)
        if !presented { try fields.encode(false, forKey: .presented) }
        try fields.encode(description, forKey: .kind)
        try fields.encodeIfPresent(description[case: \.element], forKey: .element)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension InstanceState {
    fileprivate func adding(prompts rows: [Window]) -> Self {
        self[case: \.running].map { running in
            .running(Running(status: running.status, active: running.active, windows: running.windows, prompts: rows, menus: running.menus, usage: running.usage, listening: running.listening))
        }
            ?? self
    }
}

extension Instance {
    fileprivate var promptNames: [String] { [application.localizedName].compactMap(\.self) }

    public func state(walking walked: @Sendable (TopLevelElement) -> Bool = \.asking) async -> InstanceState {
        var last = Poll(isSingleRead: true)
        while true {
            last = await poll(after: last, walking: walked)
            guard let read = last.state else { continue }
            guard !application.isDialogHelper else { return read }
            let prompts: [InstanceState.Window] = await NSWorkspace.shared.instances(of: \.isDialogHelper).async.reduce(into: []) { rows, helper in
                rows += await helper.state().prompts(naming: promptNames)
            }
            return read.adding(prompts: prompts)
        }
    }

    public func states(
        walking walked: @escaping @Sendable (TopLevelElement) -> Bool = \.asking,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let names = promptNames
        return lifetime(
            combineLatest(
                polls(walking: walked, clock: clock),
                chain([OrderedDictionary<Instance, InstanceState>()].async, NSWorkspace.shared.states(of: \.isDialogHelper, walking: \.asking, clock: clock)),
            ).map { state, helpers in state.adding(prompts: helpers.values.flatMap { helper in helper.prompts(naming: names) }) },
            clock: clock,
        )
    }

    func lifetime(
        _ running: some AsyncSequence<InstanceState, Never> & Sendable,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let cut = process.exits().map { _ in InstanceState?.none }
        let lived = merge(running.map(InstanceState?.some), cut).prefix { (state: InstanceState?) in state != nil }.compactMap(\.self)
        return chain(lived, process.exits().flatMap { exit in self.exitStates(after: exit, clock: clock) }).removeDuplicates()
    }

    private func exitStates(after exit: Result<ProcessExit, Errno>, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        [false, true].async.compactMap { reported -> InstanceState? in
            switch (exit, reported) {
                case (_, false): .exited(InstanceState.Ended(exit: exit, report: nil))
                case (.success(let ended), true) where ended.producesReport:
                    .exited(
                        InstanceState.Ended(exit: exit, report: await DiagnosticReport.first(writtenBy: self.process, since: self.application.launchDate ?? .distantPast, clock: clock))
                    )
                case (_, true): nil
            }
        }
    }

    public func reading(window: CGWindowID? = nil) async -> InstanceState {
        guard let window else { return await state { _ in true } }
        let id = await state()[case: \.running]?.windows.first { row in row.number == window }?.description[case: \.element]?.id
        return await state { element in element.id == id }
    }
}

extension NSWorkspace {
    public func instances(of matching: (NSRunningApplication) -> Bool) -> [Instance] {
        runningApplications.filter(matching).compactMap { application in try? Instance(application) }
    }

    public func instanceStates(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        walking walked: @escaping @Sendable (TopLevelElement) -> Bool = \.asking,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<OrderedDictionary<Instance, InstanceState>, Never> & Sendable {
        combineLatest(
            states(of: matching, walking: walked, clock: clock),
            chain([OrderedDictionary<Instance, InstanceState>()].async, states(of: \.isDialogHelper, walking: \.asking, clock: clock)),
        )
        .map { states, helpers in
            OrderedDictionary<Instance, InstanceState>(
                uniqueKeysWithValues: states.map { instance, state in (instance, state.adding(prompts: helpers.values.flatMap { helper in helper.prompts(naming: instance.promptNames) })) }
            )
        }
        .removeDuplicates()
    }

    func states(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        walking walked: @escaping @Sendable (TopLevelElement) -> Bool,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<OrderedDictionary<Instance, InstanceState>, Never> & Sendable {
        let (inserted, insert) = AsyncStream<Instance>.makeStream()
        let listed = Mutex<Set<Instance>?>(nil)
        let observation = observe(\.runningApplications, options: [.initial]) { workspace, _ in
            let running = workspace.instances(of: matching)
            listed.withLock { held in held = held ?? Set(running) }
            for instance in running { insert.yield(instance) }
        }
        let initial = listed.withLock { held in held ?? [] }
        let changes = AsyncStream<(instance: Instance, state: InstanceState)> { change in
            let following = Task {
                await withDiscardingTaskGroup { group in
                    var followed: Set<Instance> = []
                    for await instance in inserted where followed.insert(instance).inserted {
                        let states = instance.lifetime(instance.polls(walking: walked, clock: clock), clock: clock)
                        group.addTask {
                            for await state in states { change.yield((instance, state)) }
                        }
                    }
                }
            }
            change.onTermination = { _ in
                following.cancel()
                observation.invalidate()
            }
        }
        return chain(
            (initial.isEmpty ? [OrderedDictionary()] : []).async,
            changes.reductions(into: (states: OrderedDictionary<Instance, InstanceState>(), waiting: initial)) { held, next in
                if held.waiting.isEmpty { held.states.removeAll { _, state in state.is(\.exited) } }
                let awaited = held.waiting.remove(next.instance) != nil
                if !(awaited && next.state.is(\.exited)) { held.states[next.instance] = next.state }
            }
            .drop { held in !held.waiting.isEmpty }
            .map(\.states),
        )
        .removeDuplicates()
    }
}
