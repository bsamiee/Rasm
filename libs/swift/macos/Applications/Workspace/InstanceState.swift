public import AppKit
import AsyncAlgorithms
public import CasePaths
public import OrderedCollections
public import Processes
import Subprocess
import Synchronization
public import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reportingSignals: Set<TerminationStatus> = Set([SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGEMT, SIGFPE, SIGBUS, SIGSEGV, SIGSYS, SIGXCPU, SIGXFSZ].map(TerminationStatus.signaled))
private let reportLimit: Duration = .seconds(60)

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: Equatable, Sendable {
    public struct Running: Equatable, Sendable {
        public let status: ApplicationStatus
        public let usage: Result<ResourceUsage, Errno>?
        public internal(set) var hidden: Bool?
        public internal(set) var created: Bool?
        public internal(set) var quitting: Bool = false
    }

    @CasePathable
    public enum Next: Encodable, Equatable, Sendable {
        public struct Button: Encodable, Equatable, Sendable {
            public let id: Data
            public let label: String
        }

        case wait
        case drive
        case answer(id: Data, buttons: [Button], accept: Data?, discard: Data?, cancel: Data?)
        case capture(windows: [CGWindowID])
        case resume
        case force
        case grant
        case report(url: URL)
    }

    case running(Running)
    case failed(ApplicationFailure)
    case exited(Result<ProcessExit, Errno>, report: Result<CrashReport, ApplicationFailure>?)

    public var isLaunching: Bool { status?.is(\.launching) == true }

    public var isUnresponsive: Bool { status?.is(\.unresponsive) == true }

    public var requiresAnswer: Bool { status?[case: \.responding]?.elements.contains(where: \.requiresAnswer) == true }

    private var status: ApplicationStatus? { self[case: \.running]?.status }

    public var next: Next? {
        switch self {
            case .running(let running):
                switch running.status {
                    case .launching: .wait
                    case .responding(let elements, let windows):
                        switch (elements.first(where: \.requiresAnswer), windows.filter(\.onScreen).map(\.number)) {
                            case (.some(let asking), _):
                                .answer(
                                    id: asking.id,
                                    buttons: (asking.detail?.controls ?? []).compactMap { control in
                                        control.role == kAXButtonRole ? control.label.map { label in Next.Button(id: control.id, label: label) } : nil
                                    },
                                    accept: asking.control(kAXDefaultButtonAttribute, otherwise: "OKButton"),
                                    discard: asking.control(identifier: "DontSaveButton"),
                                    cancel: asking.control(kAXCancelButtonAttribute, kAXCloseButtonAttribute, otherwise: "CancelButton"),
                                )
                            case (.none, let shown) where !shown.isEmpty: .capture(windows: shown)
                            case (.none, _): .drive
                        }
                    case .unresponsive(.stopped, _): .resume
                    case .unresponsive: running.quitting ? .force : .wait
                }
            case .failed(let failure):
                switch failure.reason {
                    case .untrusted: .grant
                    case .unanswered: .wait
                    case _: nil
                }
            case .exited(_, .some(.success(let report))): .report(url: report.url)
            case .exited: nil
        }
    }
}

extension Element {
    fileprivate func control(_ attributes: String..., otherwise identifier: String) -> Data? {
        attributes.lazy.compactMap { attribute in self.detail?.controls.first { control in control.attribute == attribute }?.id }.first ?? control(identifier: identifier)
    }

    fileprivate func control(identifier: String) -> Data? {
        (detail?.controls.first { control in control.identifier == identifier })?.id
    }
}

extension InstanceState: Encodable {
    private enum Key: CodingKey {
        case running, failed, exited, status, next, usage, usageFailure, hidden, created, signal, detail, statusFailure, report, reportFailure
    }

    public func encode(to encoder: any Encoder) throws {
        var state: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .running(let running):
                var fields: KeyedEncodingContainer<Key> = state.nestedContainer(keyedBy: Key.self, forKey: .running)
                try fields.encode(running.status, forKey: .status)
                try fields.encodeIfPresent(next, forKey: .next)
                try fields.encodeIfPresent(running.hidden, forKey: .hidden)
                try fields.encodeIfPresent(running.created, forKey: .created)
                switch running.usage {
                    case .some(.success(let usage)): try fields.encode(usage, forKey: .usage)
                    case .some(.failure(let error)): try fields.encode(error.description, forKey: .usageFailure)
                    case .none: break
                }
            case .failed(let failure):
                let failed: any Encoder = state.superEncoder(forKey: .failed)
                try failure.encode(to: failed)
                var fields: KeyedEncodingContainer<Key> = failed.container(keyedBy: Key.self)
                try fields.encodeIfPresent(next, forKey: .next)
            case .exited(let termination, let report):
                var fields: KeyedEncodingContainer<Key> = state.nestedContainer(keyedBy: Key.self, forKey: .exited)
                switch termination {
                    case .success(let ended):
                        switch ended.status {
                            case .exited(let status): try fields.encode(status, forKey: .status)
                            case .signaled(let signal): try fields.encode(signal, forKey: .signal)
                        }
                        if !ended.detail.isEmpty { try fields.encode(ended.detail, forKey: .detail) }
                    case .failure(let error): try fields.encode(error.description, forKey: .statusFailure)
                }
                switch report {
                    case .some(.success(let report)): try fields.encode(report, forKey: .report)
                    case .some(.failure(let failure)): try fields.encode(failure.name, forKey: .reportFailure)
                    case .none: break
                }
                try fields.encodeIfPresent(next, forKey: .next)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    func state(of status: Result<ApplicationStatus, ApplicationFailure>, sampling sampled: Bool) -> InstanceState {
        switch status {
            case .success(let status):
                .running(
                    InstanceState.Running(
                        status: status,
                        usage: sampled || status.is(\.unresponsive) ? Result { () throws(Errno) in try ResourceUsage(of: processIdentifier) } : nil,
                        hidden: isHidden,
                    )
                )
            case .failure(let failure): .failed(failure)
        }
    }

    public func state(walking walked: @Sendable (Element) -> Bool) async -> InstanceState {
        state(of: await status(walking: walked).status, sampling: true)
    }

    func states(
        ending exits: AsyncStream<Result<ProcessExit, Errno>>,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<InstanceState, Never> & Sendable {
        AsyncStream { continuation in
            let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
            let observer: Result<Observer, ApplicationFailure> = Result { () throws(ApplicationFailure) in try Observer(self, ticks: tick) }
            let watching: Task<Void, Never> = Task {
                for await change: Result<ApplicationStatus, ApplicationFailure> in self.statusChanges(walking: \.requiresAnswer, observer: observer, ticks: ticks, clock: clock) {
                    continuation.yield(self.state(of: change, sampling: false))
                }
            }
            let following: Task<Void, Never> = Task {
                let exit: Result<ProcessExit, Errno>? = await exits.first { _ in true }
                watching.cancel()
                await watching.value
                guard let exit: Result<ProcessExit, Errno> else { return continuation.finish() }
                continuation.yield(.exited(exit, report: nil))
                if case .success(let ended) = exit, ended.detail.contains(.csError) || reportingSignals.contains(ended.status) {
                    continuation.yield(
                        .exited(exit, report: await CrashReport.written(by: self.processIdentifier, since: self.launchDate ?? .distantPast, within: reportLimit, clock: clock))
                    )
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in
                watching.cancel()
                following.cancel()
                observer[case: \.success]?.stop()
            }
        }
        .removeDuplicates()
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

    @MainActor
    public func instanceStates(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
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
                        let exits: AsyncStream<Result<ProcessExit, Errno>> = termination(of: instance.processIdentifier)
                        group.addTask {
                            for await state: InstanceState in instance.states(ending: exits, clock: clock) { change.yield((instance, state)) }
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
