public import AppKit
import AsyncAlgorithms
public import CasePaths
public import OrderedCollections
public import Processes
public import Subprocess
import Synchronization
public import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reportingSignals: Set<TerminationStatus> = Set([SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGEMT, SIGFPE, SIGBUS, SIGSEGV, SIGSYS, SIGXCPU, SIGXFSZ].map(TerminationStatus.signaled))

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: Sendable {
    case running(ApplicationStatus, usage: Result<ResourceUsage, Errno>?)
    case failed(ApplicationFailure)
    case exited(Result<TerminationStatus, Errno>, report: Result<CrashReport, ApplicationFailure>?)

    public var isLaunching: Bool {
        status?.is(\.launching) == true
    }

    public var isResponding: Bool {
        status?.is(\.responding) == true
    }

    public var requiresAnswer: Bool {
        status?[case: \.responding]?.elements.contains(where: \.requiresAnswer) == true
    }

    private var status: ApplicationStatus? {
        self[case: \.running]?.0
    }
}

extension InstanceState: Encodable {
    private enum Key: CodingKey {
        case running, failed, exited, status, usage, usageFailure, reason, signal, statusFailure, report, reportFailure
    }

    public func encode(to encoder: any Encoder) throws {
        var state: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .running(let status, let usage):
                var fields: KeyedEncodingContainer<Key> = state.nestedContainer(keyedBy: Key.self, forKey: .running)
                try fields.encode(status, forKey: .status)
                switch usage {
                    case .some(.success(let usage)): try fields.encode(usage, forKey: .usage)
                    case .some(.failure(let error)): try fields.encode(error.description, forKey: .usageFailure)
                    case .none: break
                }
            case .failed(let failure):
                var fields: KeyedEncodingContainer<Key> = state.nestedContainer(keyedBy: Key.self, forKey: .failed)
                try fields.encode(failure.localizedDescription, forKey: .reason)
            case .exited(let termination, let report):
                var fields: KeyedEncodingContainer<Key> = state.nestedContainer(keyedBy: Key.self, forKey: .exited)
                switch termination {
                    case .success(.exited(let status)): try fields.encode(status, forKey: .status)
                    case .success(.signaled(let signal)): try fields.encode(signal, forKey: .signal)
                    case .failure(let error): try fields.encode(error.description, forKey: .statusFailure)
                }
                switch report {
                    case .some(.success(let report)): try fields.encode(report, forKey: .report)
                    case .some(.failure(let failure)): try fields.encode(failure.localizedDescription, forKey: .reportFailure)
                    case .none: break
                }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    func state(of status: Result<ApplicationStatus, ApplicationFailure>, sampling sampled: Bool) -> InstanceState {
        switch status {
            case .success(let status): .running(status, usage: sampled || status.is(\.unresponsive) ? Result { () throws(Errno) in try ResourceUsage(of: processIdentifier) } : nil)
            case .failure(let failure): .failed(failure)
        }
    }

    public func state(walking walked: @Sendable (Element) -> Bool) async -> InstanceState {
        state(of: await status(walking: walked).status, sampling: true)
    }

    public func states(
        walking walked: @escaping @Sendable (Element) -> Bool,
        notifications: Set<String>,
        polling interval: Duration,
        reportWithin limit: Duration = .seconds(60),
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<InstanceState, Never> {
        AsyncStream { continuation in
            let (ticks, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
            let (held, hold): (AsyncStream<InstanceState>, AsyncStream<InstanceState>.Continuation) = AsyncStream.makeStream()
            let observer: Result<Observer, ApplicationFailure> = Result { () throws(ApplicationFailure) in try Observer(self, notifications: notifications, ticks: tick) }
            let watching: Task<Void, Never> = Task {
                for await change: Result<ApplicationStatus, ApplicationFailure> in self.statusChanges(walking: walked, observer: observer, ticks: ticks, polling: interval, clock: clock)
                where !Task.isCancelled {
                    hold.yield(self.state(of: change, sampling: false))
                }
                hold.finish()
            }
            let settling: Task<Void, Never> = Task {
                for await state: InstanceState in held.debounce(for: interval, clock: clock) where !Task.isCancelled { continuation.yield(state) }
            }
            let following: Task<Void, Never> = Task {
                let exit: Result<ProcessExit, Errno>? = await termination(of: self.processIdentifier).first { _ in true }
                watching.cancel()
                settling.cancel()
                await watching.value
                await settling.value
                guard let exit: Result<ProcessExit, Errno> else { return continuation.finish() }
                continuation.yield(.exited(exit.map(\.status), report: nil))
                if case .success(let ended) = exit, ended.detail.contains(.csError) || reportingSignals.contains(ended.status) {
                    continuation.yield(
                        .exited(.success(ended.status), report: await CrashReport.written(by: self.processIdentifier, since: self.launchDate ?? .distantPast, within: limit, clock: clock))
                    )
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in
                watching.cancel()
                settling.cancel()
                following.cancel()
                observer[case: \.success]?.stop()
            }
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

    @MainActor
    public func instanceStates(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        notifications: Set<String>,
        polling interval: Duration,
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
                        group.addTask {
                            for await state: InstanceState in instance.states(walking: \.requiresAnswer, notifications: notifications, polling: interval, clock: clock) {
                                change.yield((instance, state))
                            }
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
    }
}
