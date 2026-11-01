public import AppKit
import AsyncAlgorithms
public import CasePaths
public import OrderedCollections
public import Processes
public import Subprocess
public import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reportingSignals: Set<TerminationStatus> = Set([SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGEMT, SIGFPE, SIGBUS, SIGSEGV, SIGSYS].map(TerminationStatus.signaled))

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: Sendable {
    case running(ApplicationStatus, usage: ResourceUsage?)
    case failed(ApplicationFailure)
    case exited(Result<TerminationStatus, Errno>, report: Result<CrashReport, ApplicationFailure>?)
}

extension InstanceState: Encodable {
    private enum Key: CodingKey {
        case running, failed, exited, status, usage, reason, signal, statusFailure, report, reportFailure
    }

    public func encode(to encoder: any Encoder) throws {
        var state: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .running(let status, let usage):
                var fields: KeyedEncodingContainer<Key> = state.nestedContainer(keyedBy: Key.self, forKey: .running)
                try fields.encode(status, forKey: .status)
                try fields.encodeIfPresent(usage, forKey: .usage)
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
    var usage: ResourceUsage? {
        Result { () throws(Errno) in try ResourceUsage(of: processIdentifier) }[case: \.success]
    }

    func state(of status: Result<ApplicationStatus, ApplicationFailure>, sampling sampled: Bool) -> InstanceState {
        switch status {
            case .success(let status): .running(status, usage: sampled || status.is(\.unresponsive) ? usage : nil)
            case .failure(let failure): .failed(failure)
        }
    }

    public func state(walking walked: @Sendable (Element) -> Bool) async -> InstanceState {
        state(of: await status(walking: walked), sampling: true)
    }

    public func states(
        walking walked: @escaping @Sendable (Element) -> Bool,
        notifications: Set<String>,
        polling interval: Duration,
        reportWithin limit: Duration = .seconds(60),
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<InstanceState, Never> {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task {
                let exit: Result<(status: TerminationStatus, detail: ExitDetail), Errno>? = await withDiscardingTaskGroup { lifetime in
                    lifetime.addTask {
                        for await change: Result<ApplicationStatus, ApplicationFailure> in self.statusChanges(walking: walked, notifications: notifications, polling: interval, clock: clock)
                        where !Task.isCancelled {
                            continuation.yield(self.state(of: change, sampling: false))
                        }
                    }
                    defer { lifetime.cancelAll() }
                    return await termination(of: self.processIdentifier).first { _ in true }
                }
                guard let exit: Result<(status: TerminationStatus, detail: ExitDetail), Errno> else { return continuation.finish() }
                let status: Result<TerminationStatus, Errno> = exit.map(\.status)
                continuation.yield(.exited(status, report: nil))
                if case .success(let (status, detail)) = exit, detail.contains(.csError) || reportingSignals.contains(status) {
                    continuation.yield(.exited(.success(status), report: await CrashReport.written(by: self.processIdentifier, within: limit, clock: clock)))
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }
}

extension NSWorkspace {
    public func instances(of matching: (NSRunningApplication) -> Bool) -> [NSRunningApplication] {
        runningApplications.filter { instance in instance.activationPolicy != .prohibited && matching(instance) }
    }

    @MainActor
    public func instanceStates(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        walking walked: @escaping @Sendable (Element) -> Bool,
        notifications: Set<String>,
        polling interval: Duration,
        reportWithin limit: Duration = .seconds(60),
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable {
        let watched: @Sendable (NSRunningApplication) -> Bool = { instance in instance.activationPolicy != .prohibited && matching(instance) }
        let (inserted, insert): (AsyncStream<NSRunningApplication>, AsyncStream<NSRunningApplication>.Continuation) = AsyncStream.makeStream()
        let initial: Set<NSRunningApplication> = Set(runningApplications.filter(watched))
        let observation: NSKeyValueObservation = observe(\.runningApplications, options: [.initial, .new]) { _, change in
            for instance: NSRunningApplication in change.newValue ?? [] where watched(instance) { insert.yield(instance) }
        }
        let changes: AsyncStream<(instance: NSRunningApplication, state: InstanceState)> = AsyncStream { change in
            let following: Task<Void, Never> = Task {
                await withDiscardingTaskGroup { group in
                    var followed: Set<NSRunningApplication> = []
                    for await instance: NSRunningApplication in inserted where followed.insert(instance).inserted {
                        group.addTask {
                            for await state: InstanceState in instance.states(walking: walked, notifications: notifications, polling: interval, reportWithin: limit, clock: clock) {
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
                held.states.removeAll { _, state in state.is(\.exited) }
                held.states[next.instance] = next.state
                held.waiting.remove(next.instance)
            }
            .drop { held in !held.waiting.isEmpty }
            .map(\.states),
        )
    }
}
