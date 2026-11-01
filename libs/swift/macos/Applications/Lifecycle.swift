public import AppKit
import AsyncAlgorithms
import Carbon.OpenScripting
public import CasePaths
import Combine
public import OrderedCollections
import Processes
import Subprocess
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reportingStatuses: Set<TerminationStatus> = Set([SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGEMT, SIGFPE, SIGBUS, SIGSEGV, SIGSYS].map(TerminationStatus.signaled))

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: Encodable, Sendable {
    case running(status: ApplicationStatus)
    case failed(reason: String)
    case exited(status: Int32?, signal: Int32?, statusFailure: String?, report: CrashReport?, reportFailure: String?)

    public init(_ read: Result<ApplicationStatus, ElementFailure>) {
        self =
            switch read {
                case .success(let status): .running(status: status)
                case .failure(let error): .failed(reason: error.localizedDescription)
            }
    }

    init(_ termination: Result<TerminationStatus, Errno>, report: Result<CrashReport, CrashReportFailure>?) {
        let (status, signal, statusFailure): (Int32?, Int32?, String?) =
            switch termination {
                case .success(.exited(let status)): (status, nil, nil)
                case .success(.signaled(let signal)): (nil, signal, nil)
                case .failure(let error): (nil, nil, error.description)
            }
        let (written, reportFailure): (CrashReport?, String?) =
            switch report {
                case .some(.success(let report)): (report, nil)
                case .some(.failure(let error)): (nil, error.localizedDescription)
                case .none: (nil, nil)
            }
        self = .exited(status: status, signal: signal, statusFailure: statusFailure, report: written, reportFailure: reportFailure)
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

@CasePathable
public enum LaunchFailure: CustomNSError, LocalizedError {
    case hiddenAgent(URL)
    case open(any Error)
    case terminated
    case cancelled

    public var errorDescription: String? {
        switch self {
            case .hiddenAgent(let url): "LSUIElement agent \(url.path(percentEncoded: false)) cannot unhide, launch it without hiding"
            case .open(let error): "Launch Services could not open application. \(error.localizedDescription)"
            case .terminated: "Application exited during launch"
            case .cancelled: "Launch canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        self[case: \.open].map { error in [NSUnderlyingErrorKey: error] } ?? [:]
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    public func launchApplication(
        _ installation: Installation,
        opening documents: [URL] = [],
        configuration: sending OpenConfiguration = OpenConfiguration(),
    ) async -> Result<NSRunningApplication, LaunchFailure> {
        let url: URL = installation.url
        configuration.activates = false
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.appleEvent =
            documents.isEmpty
            ? NSAppleEventDescriptor(
                eventClass: AEEventClass(kASAppleScriptSuite),
                eventID: AEEventID(kASLaunchEvent),
                targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            ) : nil
        return await Result { () async throws(LaunchFailure) in
            guard !(installation.agent && configuration.hides) else { throw .hiddenAgent(url) }
            let instance: NSRunningApplication = try await Result { try await open(documents, withApplicationAt: url, configuration: configuration) }.mapError(LaunchFailure.open).get()
            try await instance.finishedLaunching().get()
            return instance
        }
    }

    @MainActor
    public func instanceStates(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        walking walked: @escaping @Sendable (Element) -> Bool,
    ) -> some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable {
        let watched: @Sendable (NSRunningApplication) -> Bool = { instance in instance.createsWindows && matching(instance) }
        let (inserted, insert): (AsyncStream<NSRunningApplication>, AsyncStream<NSRunningApplication>.Continuation) = AsyncStream.makeStream()
        let initial: Set<NSRunningApplication> = Set(runningApplications.filter(watched))
        let observation: NSKeyValueObservation = observe(\.runningApplications, options: [.initial, .new]) { _, change in
            for instance: NSRunningApplication in change.newValue ?? [] where watched(instance) { insert.yield(instance) }
        }
        let changes: AsyncStream<(instance: NSRunningApplication, state: InstanceState)> = AsyncStream { change in
            let following: Task<Void, Never> = Task {
                await withDiscardingTaskGroup { group in
                    for await instance: NSRunningApplication in inserted {
                        group.addTask { for await state: InstanceState in instance.states(walking: walked) { change.yield((instance, state)) } }
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

extension NSRunningApplication {
    public func states(walking walked: @escaping @Sendable (Element) -> Bool) -> AsyncStream<InstanceState> {
        AsyncStream { continuation in
            let reports: some AsyncSequence<Result<CrashReport, CrashReportFailure>, Never> = CrashReport.following()
            let following: Task<Void, Never> = Task {
                let exit: Result<(status: TerminationStatus, detail: ExitDetail), Errno>? = await withDiscardingTaskGroup { lifetime in
                    lifetime.addTask {
                        if !self.isFinishedLaunching { continuation.yield(.running(status: .launching)) }
                        guard case .success = await self.finishedLaunching() else { return }
                        switch self.statusChanges(walking: walked) {
                            case .success(let changes):
                                for await change: Result<ApplicationStatus, ElementFailure> in changes where !Task.isCancelled { continuation.yield(InstanceState(change)) }
                            case .failure(let error): continuation.yield(InstanceState(.failure(error)))
                        }
                    }
                    defer { lifetime.cancelAll() }
                    return await termination(of: self.processIdentifier).first { _ in true }
                }
                guard let exit: Result<(status: TerminationStatus, detail: ExitDetail), Errno> else { return continuation.finish() }
                let status: Result<TerminationStatus, Errno> = exit.map(\.status)
                continuation.yield(InstanceState(status, report: nil))
                let reported: Bool =
                    switch exit {
                        case .success(let (status, detail)): detail.contains(.csError) || reportingStatuses.contains(status)
                        case .failure: false
                    }
                let report: Result<CrashReport, CrashReportFailure>? =
                    reported
                    ? await reports.first { read in
                        switch read {
                            case .success(let report): report.pid == self.processIdentifier
                            case .failure(.unwatched): true
                            case .failure(.unreadable): false
                        }
                    } : nil
                if let report: Result<CrashReport, CrashReportFailure> { continuation.yield(InstanceState(status, report: report)) }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }

    public func finishedLaunching() async -> Result<Void, LaunchFailure> {
        await publisher(for: \.isFinishedLaunching).combineLatest(publisher(for: \.isTerminated)).compactMap { done, gone in done || gone ? done : nil }
            .buffer(size: 1, prefetch: .byRequest, whenFull: .dropOldest).values.first { _ in true }
            .map { done in done ? .success(()) : .failure(.terminated) } ?? .failure(.cancelled)
    }
}
