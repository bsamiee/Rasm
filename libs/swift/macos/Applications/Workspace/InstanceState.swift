public import AppKit
import AppleEvents
import AsyncAlgorithms
public import CasePaths
import CasePathsCore
public import OrderedCollections
public import Processes
import Subprocess
import Synchronization
public import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reportLimit: Duration = .seconds(60)

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: Equatable, Sendable {
    public struct Running: Equatable, Sendable {
        @CasePathable
        public enum Status: Equatable, Sendable {
            case launching
            case responding
            case unresponsive(ProcessStatus)

            public var settled: Bool { self != .launching }

            public var name: String {
                switch self {
                    case .launching: "launching"
                    case .responding: "responding"
                    case .unresponsive: "unresponsive"
                }
            }
        }

        public let status: Status
        public let active: Bool
        public let windows: [Window]
        public let prompts: [Window]
        public let menus: [Element]
        public let usage: Result<ResourceUsage, Errno>?

        public var asking: [Window] { (windows + prompts).filter(\.asking) }
    }

    public struct Window: Equatable, Sendable {
        @CasePathable
        public enum Description: Equatable, Sendable {
            case element(Element)
            case undescribed
            case blocking
        }

        public let number: CGWindowID
        public let title: String?
        public let frame: CGRect
        public let layer: Int
        public let onScreen: Bool
        public let description: Description
        let arrived: Bool

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
                case .undescribed: arrived
            }
        }
    }

    case running(Running)
    case failed(ApplicationFailure)
    case exited(Result<ProcessExit, Errno>, report: Result<CrashReport, ApplicationFailure>?)
}

extension InstanceState.Window {
    init?(_ row: [String: Any]) {
        guard let number: CGWindowID = row[kCGWindowNumber as String] as? CGWindowID, let layer: Int = row[kCGWindowLayer as String] as? Int, layer >= 0,
            let frame: CGRect = (row[kCGWindowBounds as String] as? [String: Any]).flatMap({ bounds in CGRect(dictionaryRepresentation: bounds as CFDictionary) })
        else { return nil }
        self.init(
            number: number,
            title: (row[kCGWindowName as String] as? String).flatMap { title in title.isEmpty ? nil : title },
            frame: frame,
            layer: layer,
            onScreen: row[kCGWindowIsOnscreen as String] as? Bool ?? false,
            description: .undescribed,
            arrived: false,
        )
    }

    static func owned(by process: pid_t) -> [Self] {
        (CGWindowListCopyWindowInfo(.optionAll, kCGNullWindowID) as? [[String: Any]] ?? []).filter { row in row[kCGWindowOwnerPID as String] as? pid_t == process }.compactMap(Self.init)
    }

    static func listed(number: CGWindowID) -> Self? {
        (CGWindowListCopyWindowInfo(.optionIncludingWindow, number) as? [[String: Any]] ?? []).lazy.compactMap(Self.init).first
    }

    func described(_ description: Description, arrived: Bool) -> Self {
        Self(number: number, title: title, frame: frame, layer: layer, onScreen: onScreen, description: description, arrived: arrived)
    }
}

extension InstanceState.Running {
    fileprivate var printed: (status: Status, active: Bool, asking: [CGWindowID], menus: [Element.ID]) {
        (status == .unresponsive(.running) ? .unresponsive(.sleeping) : status, active, asking.map(\.number), menus.filter { menu in menu.role == kAXMenuRole }.map(\.id))
    }

    fileprivate func printsSame(as other: Self) -> Bool {
        let rows: [InstanceState.Window] = other.windows.filter(\.presented)
        let sameRows: Bool = windows.filter(\.presented).elementsEqual(rows) { (left: InstanceState.Window, right: InstanceState.Window) -> Bool in
            let sameTitle: Bool = left.description[case: \.element]?.title == right.description[case: \.element]?.title
            return left.number == right.number && left.title == right.title && sameTitle
        }
        return sameRows && printed == other.printed
    }
}

extension InstanceState {
    func printsSame(as other: Self) -> Bool {
        switch (self, other) {
            case (.running(let previous), .running(let next)): previous.printsSame(as: next)
            case (_, _): self == other
        }
    }
}

extension InstanceState: Encodable {
    private enum Key: CodingKey {
        case running, failed, exited, status, signal, detail, statusFailure, report, reportFailure, reporting
    }

    public func encode(to encoder: any Encoder) throws {
        var state: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .running(let running): try state.encode(running, forKey: .running)
            case .failed(let failure): try state.encode(failure, forKey: .failed)
            case .exited(let termination, let report):
                var fields: KeyedEncodingContainer<Key> = state.nestedContainer(keyedBy: Key.self, forKey: .exited)
                switch termination {
                    case .success(let ended):
                        switch ended.status {
                            case .exited(let status): try fields.encode(status, forKey: .status)
                            case .signaled(let signal): try fields.encode(signal, forKey: .signal)
                        }
                        if !ended.detail.isEmpty { try fields.encode(ended.detail, forKey: .detail) }
                        if report == nil, ended.reports { try fields.encode(true, forKey: .reporting) }
                    case .failure(let error): try fields.encode(error.description, forKey: .statusFailure)
                }
                switch report {
                    case .some(.success(let report)): try fields.encode(report, forKey: .report)
                    case .some(.failure(let failure)): try fields.encode(failure.name, forKey: .reportFailure)
                    case .none: break
                }
        }
    }
}

extension InstanceState.Running: Encodable {
    private enum Key: CodingKey {
        case active, menus, process, status, usage, usageFailure, windows
    }

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encode(active, forKey: .active)
        if !menus.isEmpty { try fields.encode(menus, forKey: .menus) }
        try fields.encode(status.name, forKey: .status)
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
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        if asking { try fields.encode(true, forKey: .asking) }
        try fields.encode(frame, forKey: .frame)
        try fields.encode(layer, forKey: .layer)
        try fields.encode(number, forKey: .number)
        try fields.encode(onScreen, forKey: .onScreen)
        try fields.encodeIfPresent(title, forKey: .title)
        if !presented { try fields.encode(false, forKey: .presented) }
        switch description {
            case .element(let element):
                try fields.encode(element.kind, forKey: .kind)
                try fields.encode(element, forKey: .element)
            case .undescribed: try fields.encode("undescribed", forKey: .kind)
            case .blocking: try fields.encode("blocking", forKey: .kind)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension InstanceState {
    fileprivate func prompted(_ rows: [Window]) -> Self {
        self[case: \.running].map { running in .running(Running(status: running.status, active: running.active, windows: running.windows, prompts: rows, menus: running.menus, usage: running.usage)) }
            ?? self
    }
}

extension NSRunningApplication {
    fileprivate var promptNames: [String] { [localizedName].compactMap(\.self) }

    public func state(walking walked: @Sendable (Element) -> Bool = \.asking) async -> InstanceState {
        let read: InstanceState = await beat(after: nil, walking: walked).state ?? .failed(ApplicationFailure(subject: String(processIdentifier), reason: .cancelled))
        guard !isDialogHelper else { return read }
        let prompts: [InstanceState.Window] = await NSWorkspace.shared.instances { instance in instance.isDialogHelper }.async.reduce(into: []) { rows, helper in
            rows += await helper.beat(after: nil, walking: \.asking).state?.prompting(promptNames) ?? []
        }
        return read.prompted(prompts)
    }

    public func states(
        walking walked: @escaping @Sendable (Element) -> Bool = \.asking,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let names: [String] = promptNames
        return lifetime(
            combineLatest(
                beats(walking: walked, clock: clock),
                chain([OrderedDictionary<NSRunningApplication, InstanceState>()].async, NSWorkspace.shared.tracked(of: \.isDialogHelper, walking: \.asking, clock: clock)),
            ).map {
                state,
                helpers in state.prompted(helpers.values.flatMap { helper in helper.prompting(names) })
            },
            clock: clock,
        )
    }

    fileprivate func lifetime(
        _ running: some AsyncSequence<InstanceState, Never> & Sendable,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<InstanceState, Never> & Sendable {
        chain([Result<ProcessExit, Errno>?.none].async, termination(of: processIdentifier).map(Optional.some))
            .flatMapLatest { exit in chain(running.prefix(exit == nil ? .max : 0), self.ended(exit, clock: clock)) }
            .removeDuplicates { previous, next in previous.printsSame(as: next) }
    }

    private func ended(_ exit: Result<ProcessExit, Errno>?, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        [false, true].async.compactMap { reported -> InstanceState? in
            switch (exit, reported) {
                case (.none, _): nil
                case (.some(let exit), false): .exited(exit, report: nil)
                case (.some(.success(let ended)), true) where ended.reports:
                    .exited(.success(ended), report: await CrashReport.written(by: self.processIdentifier, since: self.launchDate ?? .distantPast, within: reportLimit, clock: clock))
                case (.some, true): nil
            }
        }
    }

    public func quit(clock: some Clock<Duration> & Sendable = ContinuousClock()) -> some AsyncSequence<Result<InstanceState, ApplicationFailure>, Never> & Sendable {
        AsyncStream { output in
            let states: some AsyncSequence<InstanceState, Never> & Sendable = states(clock: clock)
            let following: Task<Void, Never> = Task {
                let (replies, reply): (AsyncStream<Result<InstanceState, ApplicationFailure>>, AsyncStream<Result<InstanceState, ApplicationFailure>>.Continuation) = AsyncStream.makeStream()
                var sending: Task<Void, Never>?
                defer { sending?.cancel() }
                for await event: Result<InstanceState, ApplicationFailure> in merge(states.map(Result.success), replies) {
                    output.yield(event)
                    switch event {
                        case .success(.exited):
                            if sending == nil { reply.finish() }
                        case .success(.running(let running)) where running.asking.isEmpty && !running.status.is(\.unresponsive):
                            guard sending == nil else { continue }
                            sending = Task { await requestQuit(answering: reply, clock: clock) }
                        case .failure, .success(.running), .success(.failed):
                            return output.finish()
                    }
                }
                output.finish()
            }
            output.onTermination = { _ in following.cancel() }
        }
    }

    private func requestQuit(answering reply: AsyncStream<Result<InstanceState, ApplicationFailure>>.Continuation, clock: some Clock<Duration> & Sendable) async {
        defer { reply.finish() }
        let request: NSAppleEventDescriptor = NSAppleEventDescriptor(
            eventClass: AEEventClass(kCoreEventClass),
            eventID: AEEventID(kAEQuitApplication),
            targetDescriptor: NSAppleEventDescriptor(processIdentifier: processIdentifier),
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        let subject: String = String(processIdentifier)
        do {
            _ = try await request.sent(timeout: NSAppleEventTimeOutDefault)
        } catch  where error.status == OSStatus(userCanceledErr) {
            _ = await Result { () async throws(any Error) in try await clock.sleep(for: messagingTimeout) }
            let read: InstanceState = await state()
            switch read {
                case .running(let running) where !running.asking.isEmpty || running.status.is(\.unresponsive): reply.yield(.success(read))
                case .failed: break
                case .running, .exited: reply.yield(.failure(ApplicationFailure(subject: subject, reason: .refused(nil))))
            }
        } catch  where error.status == OSStatus(procNotFound) {
            if kill(processIdentifier, SIGTERM) != 0 { reply.yield(.failure(ApplicationFailure(subject: subject, reason: .event(error)))) }
        } catch {
            reply.yield(.failure(ApplicationFailure(subject: subject, reason: .event(error))))
        }
    }
}

extension NSWorkspace {
    public func instances(of matching: (NSRunningApplication) -> Bool) -> [NSRunningApplication] {
        runningApplications.filter { instance in
            instance.activationPolicy != .prohibited && !instance.isTerminated && matching(instance)
                && Result { () throws(Errno) in try ProcessStatus(of: instance.processIdentifier) } != .failure(.noSuchProcess)
        }
    }

    public func instances(named name: String) -> [NSRunningApplication] {
        guard !name.hasPrefix("/") else {
            let path: String = URL(filePath: name, directoryHint: .isDirectory).resolvingSymlinksInPath().path(percentEncoded: false)
            return instances { instance in instance.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == path }
        }
        let options: String.CompareOptions = [.caseInsensitive, .diacriticInsensitive]
        let exact: [NSRunningApplication] = instances { instance in
            [instance.bundleIdentifier, instance.localizedName].contains { candidate in candidate?.compare(name, options: options) == .orderedSame }
        }
        return exact.isEmpty ? instances { instance in instance.activationPolicy == .regular && instance.localizedName?.range(of: name, options: options) != nil } : exact
    }

    public func instanceStates(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        walking walked: @escaping @Sendable (Element) -> Bool = \.asking,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable {
        combineLatest(
            tracked(of: matching, walking: walked, clock: clock),
            chain([OrderedDictionary<NSRunningApplication, InstanceState>()].async, tracked(of: \.isDialogHelper, walking: \.asking, clock: clock)),
        )
        .map { states, helpers in
            OrderedDictionary<NSRunningApplication, InstanceState>(
                uniqueKeysWithValues: states.map { instance, state in (instance, state.prompted(helpers.values.flatMap { helper in helper.prompting(instance.promptNames) })) }
            )
        }
        .removeDuplicates()
    }

    func tracked(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        walking walked: @escaping @Sendable (Element) -> Bool,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable {
        let (inserted, insert): (AsyncStream<NSRunningApplication>, AsyncStream<NSRunningApplication>.Continuation) = AsyncStream.makeStream()
        let listed: Mutex<Set<NSRunningApplication>?> = Mutex(nil)
        let observation: NSKeyValueObservation = observe(\.runningApplications, options: [.initial]) { workspace, _ in
            let running: [NSRunningApplication] = workspace.instances(of: matching)
            listed.withLock { held in held = held ?? Set(running) }
            for instance: NSRunningApplication in running { insert.yield(instance) }
        }
        let initial: Set<NSRunningApplication> = listed.withLock { held in held ?? [] }
        let changes: AsyncStream<(instance: NSRunningApplication, state: InstanceState)> = AsyncStream { change in
            let following: Task<Void, Never> = Task {
                await withDiscardingTaskGroup { group in
                    var followed: Set<NSRunningApplication> = []
                    for await instance: NSRunningApplication in inserted where followed.insert(instance).inserted {
                        let states: some AsyncSequence<InstanceState, Never> & Sendable = instance.lifetime(instance.beats(walking: walked, clock: clock), clock: clock)
                        group.addTask {
                            for await state: InstanceState in states { change.yield((instance, state)) }
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
            changes.reductions(into: (states: OrderedDictionary<NSRunningApplication, InstanceState>(), waiting: initial)) { held, next in
                if held.waiting.isEmpty { held.states.removeAll { _, state in state.is(\.exited) } }
                let awaited: Bool = held.waiting.remove(next.instance) != nil
                if !(awaited && next.state.is(\.exited)) { held.states[next.instance] = next.state }
            }
            .drop { held in !held.waiting.isEmpty }
            .map(\.states),
        )
        .removeDuplicates()
    }
}
