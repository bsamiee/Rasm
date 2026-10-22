public import AppKit
import AsyncAlgorithms
import Carbon.OpenScripting
import Combine
import Encoding
public import OrderedCollections
public import Results
public import Subprocess
public import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reportingSignals: Set<Int32> = [SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGEMT, SIGFPE, SIGBUS, SIGSEGV, SIGSYS]

// --- [MODELS] --------------------------------------------------------------------------

public enum InstanceState: Sendable {
    case running(Result<ApplicationStatus, ElementFailure>)
    case exited(Result<TerminationStatus, Errno>, report: Result<CrashReport, CrashReportFailure>?)
}

extension InstanceState: Encodable {
    private enum Key: CodingKey {
        case reason, status, statusFailure, signal, report, reportFailure
    }

    public func encode(to encoder: any Encoder) throws {
        switch self {
            case .running(.success(let status)): try status.encode(to: encoder)
            case .running(.failure(let error)): try encoder.encode(case: "failure", keyedBy: Key.self) { fields in try fields.encode(error.localizedDescription, forKey: .reason) }
            case .exited(let termination, let report):
                try encoder.encode(case: "exited", keyedBy: Key.self) { fields in
                    switch termination {
                        case .success(.exited(let status)): try fields.encode(status, forKey: .status)
                        case .success(.signaled(let signal)): try fields.encode(signal, forKey: .signal)
                        case .failure(let error): try fields.encode(error.description, forKey: .statusFailure)
                    }
                    switch report {
                        case .some(.success(let report)): try fields.encode(report, forKey: .report)
                        case .some(.failure(let error)): try fields.encode(error.localizedDescription, forKey: .reportFailure)
                        case .none: break
                    }
                }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum LaunchFailure: DeadlineFailure, CustomNSError, LocalizedError {
    case open(any Error)
    case terminated
    case deadline(Deadline)

    public var errorDescription: String? {
        switch self {
            case .open(let error): "Launch Services could not open application: \(error.localizedDescription)"
            case .terminated: "Application exited before it finished launching"
            case .deadline(.timedOut(let timeout)): "Application did not finish launching within \(timeout.formatted(.units(width: .wide)))"
            case .deadline(.cancelled): "Canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .open(let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    public func launchApplication(
        at url: URL,
        within timeout: Duration?,
        opening documents: [URL] = [],
        configuration: sending OpenConfiguration = OpenConfiguration(),
    ) async -> Result<NSRunningApplication, LaunchFailure> {
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
            let instance: NSRunningApplication = try await Result { try await open(documents, withApplicationAt: url, configuration: configuration) }.mapError(LaunchFailure.open).get()
            try await instance.finishedLaunching(within: timeout).get()
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
                held.states.removeAll { _, state in if case .exited = state { true } else { false } }
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
                let exit: Result<Int32, Errno>? = await withDiscardingTaskGroup { lifetime in
                    lifetime.addTask {
                        if !self.isFinishedLaunching { continuation.yield(.running(.success(.launching))) }
                        guard case .success = await self.finishedLaunching(within: nil) else { return }
                        switch self.statusChanges(walking: walked) {
                            case .success(let changes):
                                for await change: Result<ApplicationStatus, ElementFailure> in changes where !Task.isCancelled { continuation.yield(.running(change)) }
                            case .failure(let error): continuation.yield(.running(.failure(error)))
                        }
                    }
                    defer { lifetime.cancelAll() }
                    return await self.termination().first { _ in true }
                }
                guard let exit: Result<Int32, Errno> else { return continuation.finish() }
                let status: Result<TerminationStatus, Errno> = exit.map(TerminationStatus.init(waitStatus:))
                continuation.yield(.exited(status, report: nil))
                let reported: Bool =
                    switch (exit, status) {
                        case (.success(let data), .success(.signaled(let signal))): reportingSignals.contains(signal) || data & NOTE_EXIT_CSERROR != 0
                        case (.success(let data), .success(.exited)): data & NOTE_EXIT_CSERROR != 0
                        case (.failure, _), (_, .failure): false
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
                if let report: Result<CrashReport, CrashReportFailure> { continuation.yield(.exited(status, report: report)) }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }

    public func finishedLaunching(within timeout: Duration?) async -> Result<Void, LaunchFailure> {
        let finished: @Sendable () async -> Result<Void, LaunchFailure> = {
            await self.publisher(for: \.isFinishedLaunching).combineLatest(self.publisher(for: \.isTerminated)).compactMap { done, gone in done || gone ? done : nil }
                .buffer(size: 1, prefetch: .byRequest, whenFull: .dropOldest).values.first { _ in true }
                .map { done in done ? .success(()) : .failure(.terminated) } ?? .failure(.deadline(.cancelled))
        }
        return if let timeout: Duration {
            await withTimeout(timeout, finished)
        } else {
            await finished()
        }
    }

    private func termination() -> AsyncStream<Result<Int32, Errno>> {
        AsyncStream { continuation in
            let registered: Result<FileHandle, Errno> = Result { () throws(Errno) in
                let descriptor: Int32 = kqueue()
                guard descriptor >= 0 else { throw Errno(rawValue: errno) }
                let queue: FileHandle = FileHandle(fileDescriptor: descriptor, closeOnDealloc: true)
                var exit: kevent = unsafe kevent(
                    ident: UInt(processIdentifier),
                    filter: Int16(EVFILT_PROC),
                    flags: UInt16(EV_ADD | EV_ONESHOT),
                    fflags: NOTE_EXIT | UInt32(NOTE_EXITSTATUS) | UInt32(NOTE_EXIT_DETAIL),
                    data: 0,
                    udata: nil,
                )
                guard unsafe kevent(descriptor, &exit, 1, nil, 0, nil) == 0 else { throw Errno(rawValue: errno) }
                return queue
            }
            switch registered {
                case .success(let queue):
                    queue.readabilityHandler = { queue in
                        var exited: kevent = unsafe kevent()
                        var now: timespec = timespec()
                        let read: Int32 = unsafe kevent(queue.fileDescriptor, nil, 0, &exited, 1, &now)
                        continuation.yield(read < 0 ? .failure(Errno(rawValue: errno)) : .success(Int32(truncatingIfNeeded: unsafe exited.data)))
                        continuation.finish()
                    }
                    continuation.onTermination = { _ in queue.readabilityHandler = nil }
                case .failure(let error):
                    continuation.yield(.failure(error))
                    continuation.finish()
            }
        }
    }
}

extension TerminationStatus {
    init(waitStatus status: Int32) {
        self = status & _WSTOPPED == 0 ? .exited(status >> 8 & 0xff) : .signaled(status & _WSTOPPED)
    }
}
