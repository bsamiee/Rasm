public import AppKit
import Carbon.OpenScripting
public import Results

// --- [MODELS] --------------------------------------------------------------------------

public enum InstanceEvent: Sendable {
    case launched(NSRunningApplication)
    case terminated(NSRunningApplication, status: Int?)
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
    public func launchApplication(at url: URL, within timeout: Duration) async -> Result<NSRunningApplication, LaunchFailure> {
        let configuration: OpenConfiguration = OpenConfiguration()
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
        let center: NotificationCenter = notificationCenter
        let opening: [NSRunningApplication] = runningApplications.filter { instance in instance.launchedWithWindows && matching(instance) }
        let changes: AsyncStream<InstanceEvent> = AsyncStream { continuation in
            let tasks: [Task<Void, Never>] = [Self.didLaunchApplicationNotification, Self.didTerminateApplicationNotification].map { name in
                Task { @MainActor [notes = center.notifications(named: name)] in
                    for await note: Notification in notes {
                        if let instance: NSRunningApplication = note.userInfo?[Self.applicationUserInfoKey] as? NSRunningApplication, instance.createsWindows, matching(instance) {
                            continuation.yield(name == Self.didLaunchApplicationNotification ? .launched(instance) : .terminated(instance, status: note.userInfo?["NSWorkspaceExitStatusKey"] as? Int))
                        }
                    }
                }
            }
            continuation.onTermination = { _ in
                for task: Task<Void, Never> in tasks { task.cancel() }
            }
        }
        return AsyncStream { continuation in
            let following: Task<Void, Never> = Task { @MainActor in
                await withDiscardingTaskGroup { group in
                    func observe(_ instance: NSRunningApplication) {
                        group.addTask(executorPreference: accessibility) { @concurrent in
                            for await next: Result<ApplicationStatus, ElementFailure> in StatusChanges(instance) { continuation.yield(.status(instance, next)) }
                        }
                    }
                    for instance: NSRunningApplication in opening { observe(instance) }
                    for await change: InstanceEvent in changes {
                        continuation.yield(change)
                        if case .launched(let instance) = change { observe(instance) }
                    }
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }
}

extension NSRunningApplication {
    public func finishedLaunching(within timeout: Duration) async -> Result<Void, LaunchFailure> {
        let (states, sink): (AsyncStream<Result<Void, LaunchFailure>>, AsyncStream<Result<Void, LaunchFailure>>.Continuation) = AsyncStream.makeStream()
        let observations: [NSKeyValueObservation] = [\.isFinishedLaunching, \.isTerminated].map { keyPath in
            observe(keyPath, options: [.initial, .new]) { instance, _ in
                if instance.isFinishedLaunching || instance.isTerminated { sink.yield(instance.isFinishedLaunching ? .success(()) : .failure(.terminated)) }
            }
        }
        defer {
            for observation: NSKeyValueObservation in observations { observation.invalidate() }
            sink.finish()
        }
        return await withTimeout(timeout) { await states.first { _ in true } ?? .failure(.deadline(.cancelled)) }
    }
}
