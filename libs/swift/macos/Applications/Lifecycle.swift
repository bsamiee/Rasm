public import AppKit
import Carbon.OpenScripting
import Combine
public import Results
public import Subprocess
public import System

// --- [MODELS] --------------------------------------------------------------------------

public enum InstanceEvent: Sendable {
    case launched(NSRunningApplication)
    case terminated(NSRunningApplication, status: Result<TerminationStatus, Errno>)
    case status(NSRunningApplication, Result<ApplicationStatus, ElementFailure>)

    public var instance: NSRunningApplication {
        switch self {
            case .launched(let instance), .terminated(let instance, _), .status(let instance, _): instance
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
            case .open(let error): "Launch Services could not open the application: \(error.localizedDescription)"
            case .terminated: "The application exited before it finished launching"
            case .deadline(.timedOut(let timeout)): "The application did not finish launching within \(timeout.formatted(.units(width: .wide)))"
            case .deadline(.cancelled): "Canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .open(let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    public func launchApplication(at url: URL, within timeout: Duration?, configuration: sending OpenConfiguration = OpenConfiguration()) async -> Result<NSRunningApplication, LaunchFailure> {
        configuration.activates = false
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.appleEvent = NSAppleEventDescriptor(
            eventClass: AEEventClass(kASAppleScriptSuite),
            eventID: AEEventID(kASLaunchEvent),
            targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        return await Result { () async throws(LaunchFailure) in
            let instance: NSRunningApplication = try await Result { try await openApplication(at: url, configuration: configuration) }.mapError(LaunchFailure.open).get()
            try await instance.finishedLaunching(within: timeout).get()
            return instance
        }
    }

    @MainActor
    public func instanceEvents(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> AsyncStream<InstanceEvent> {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task { @MainActor in
                let (inserted, insert): (AsyncStream<NSRunningApplication>, AsyncStream<NSRunningApplication>.Continuation) = AsyncStream.makeStream()
                let observation: NSKeyValueObservation = observe(\.runningApplications, options: [.initial, .new]) { _, change in
                    for instance: NSRunningApplication in change.newValue ?? [] where instance.createsWindows && matching(instance) { insert.yield(instance) }
                }
                await withDiscardingTaskGroup { group in
                    for await instance: NSRunningApplication in inserted {
                        group.addTask { for await event: InstanceEvent in instance.events() { continuation.yield(event) } }
                    }
                }
                observation.invalidate()
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }
}

extension NSRunningApplication {
    public func events() -> AsyncStream<InstanceEvent> {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task {
                let exit: Result<TerminationStatus, Errno>? = await withDiscardingTaskGroup { lifetime in
                    lifetime.addTask {
                        guard case .success = await self.finishedLaunching(within: nil) else { return }
                        continuation.yield(.launched(self))
                        switch self.statusChanges() {
                            case .success(let changes):
                                for await change: Result<ApplicationStatus, ElementFailure> in changes where !Task.isCancelled { continuation.yield(.status(self, change)) }
                            case .failure(let error): continuation.yield(.status(self, .failure(error)))
                        }
                    }
                    defer { lifetime.cancelAll() }
                    return await self.termination().first { _ in true }
                }
                if let exit: Result<TerminationStatus, Errno> { continuation.yield(.terminated(self, status: exit)) }
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

    private func termination() -> AsyncStream<Result<TerminationStatus, Errno>> {
        AsyncStream { continuation in
            let registered: Result<FileHandle, Errno> = Result { () throws(Errno) in
                let descriptor: Int32 = kqueue()
                guard descriptor >= 0 else { throw Errno(rawValue: errno) }
                let queue: FileHandle = FileHandle(fileDescriptor: descriptor, closeOnDealloc: true)
                var exit: kevent = unsafe kevent(
                    ident: UInt(processIdentifier),
                    filter: Int16(EVFILT_PROC),
                    flags: UInt16(EV_ADD | EV_ONESHOT),
                    fflags: NOTE_EXIT | UInt32(NOTE_EXITSTATUS),
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
                        continuation.yield(read < 0 ? .failure(Errno(rawValue: errno)) : .success(TerminationStatus(waitStatus: Int32(truncatingIfNeeded: unsafe exited.data))))
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
