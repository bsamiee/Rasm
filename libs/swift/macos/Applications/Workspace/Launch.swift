public import AppKit
import AsyncAlgorithms
import Processes
import Results
import System

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    public func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        configuration: sending OpenConfiguration = OpenConfiguration(),
    ) async throws(ApplicationFailure) -> (instance: NSRunningApplication, created: Bool) {
        let url: URL = installation.url
        let running: [NSRunningApplication] = runningApplications.filter(installation.runs)
        let creating: Bool = configuration.createsNewApplicationInstance || running.isEmpty
        if let reused: NSRunningApplication = running.first, !creating, documents.isEmpty { return (reused, false) }
        configuration.activates = false
        configuration.hides = creating && !installation.agent
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        if documents.isEmpty {
            configuration.appleEvent = NSAppleEventDescriptor(
                eventClass: AEEventClass(kCoreEventClass),
                eventID: kAEOpenApplication,
                targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            )
            configuration.appleEvent?.setParam(NSAppleEventDescriptor(enumCode: kAENo), forKeyword: keyAERestoreAppState)
        }
        do {
            let instance: NSRunningApplication =
                try await documents.isEmpty
                ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
            return (instance, !running.contains(instance))
        } catch {
            throw ApplicationFailure(subject: "Launch Services open \(url.path(percentEncoded: false))", reason: .host(error))
        }
    }
}

extension NSRunningApplication {
    public func states(until settled: @escaping @Sendable (InstanceState) -> Bool, notifications: Set<String>, polling interval: Duration) -> AsyncStream<InstanceState> {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task {
                for await state: InstanceState in states(walking: \.requiresAnswer, notifications: notifications, polling: interval) {
                    continuation.yield(state)
                    guard !settled(state) else { break }
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }

    public func shown(within limit: Duration, clock: some Clock<Duration> = ContinuousClock()) async throws(ApplicationFailure) {
        let (visibility, visible): (AsyncStream<Result<Bool, Errno>>, AsyncStream<Result<Bool, Errno>>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observation: NSKeyValueObservation = observe(\.isHidden, options: [.initial, .new]) { instance, _ in
            if !instance.isHidden { visible.yield(.success(true)) }
        }
        defer { observation.invalidate() }
        _ = unhide()
        let exits: some AsyncSequence<Result<Bool, Errno>, Never> & Sendable = termination(of: processIdentifier).map { exit in exit.map { _ in false } }
        let process: pid_t = processIdentifier
        try await withTimeout(limit, expired: ApplicationFailure(subject: String(process), reason: .hidden(limit)), clock: clock) { () async throws(ApplicationFailure) in
            switch await merge(visibility, exits).first(where: { _ in true }) {
                case .success(true): return
                case .success(false): throw ApplicationFailure(.noSuchProcess, process: process)
                case .failure(let error): throw ApplicationFailure(error, process: process)
                case nil: throw ApplicationFailure(subject: "unhide of process \(process)", reason: .cancelled(CancellationError()))
            }
        }
    }
}
