public import AppKit
import AsyncAlgorithms
import Processes
import Results
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let appSwitches: KeyValuePairs<String, (added: String, unless: Set<String>)> = [
    "com.mcneel.rhinoceros": ("-nosplash", []),
    "org.blenderfoundation.blender": ("--no-window-focus", ["-b", "--background"]),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public init(opening document: URL) throws(ApplicationFailure) {
        let path: String = document.path(percentEncoded: false)
        do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: path, reason: .host(error)) }
        guard let handler: URL = NSWorkspace.shared.urlForApplication(toOpen: document) else { throw ApplicationFailure(subject: path, reason: .noHandler) }
        try self.init(at: handler)
    }
}

extension NSWorkspace {
    public func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
    ) async throws(ApplicationFailure) -> (instance: NSRunningApplication, created: Bool) {
        let url: URL = installation.url
        let running: [NSRunningApplication] = runningApplications.filter(installation.runs)
        let creating: Bool = newInstance || running.isEmpty
        if let reused: NSRunningApplication = running.first, !creating, documents.isEmpty { return (reused, false) }
        let configuration: OpenConfiguration = OpenConfiguration()
        configuration.activates = false
        configuration.hides = creating && !installation.agent
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.createsNewApplicationInstance = newInstance
        configuration.arguments =
            arguments
            + appSwitches.filter { prefix, switched in installation.identifier.hasPrefix(prefix) && switched.unless.union([switched.added]).isDisjoint(with: arguments) }.map(\.value.added)
        configuration.environment = environment
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
            throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .host(error))
        }
    }
}

extension NSRunningApplication {
    public func setHidden(_ hidden: Bool, clock: some Clock<Duration> = ContinuousClock()) async throws(ApplicationFailure) {
        let process: pid_t = processIdentifier
        let (visibility, visible): (AsyncStream<Result<Bool, Errno>>, AsyncStream<Result<Bool, Errno>>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observation: NSKeyValueObservation = observe(\.isHidden, options: [.initial, .new]) { instance, _ in
            if instance.isHidden == hidden { visible.yield(.success(true)) }
        }
        defer { observation.invalidate() }
        guard isHidden != hidden else { return }
        let exits: some AsyncSequence<Result<Bool, Errno>, Never> & Sendable = termination(of: process).map { exit in exit.map { _ in false } }
        _ = hidden ? hide() : unhide()
        let expired: ApplicationFailure = ApplicationFailure(subject: String(process), reason: .visibility(hidden: !hidden, limit: messagingTimeout))
        try await withTimeout(messagingTimeout, expired: expired, clock: clock) { () async throws(ApplicationFailure) in
            switch await merge(visibility, exits).first(where: { _ in true }) {
                case .success(true): return
                case .success(false): throw ApplicationFailure(.noSuchProcess, process: process)
                case .failure(let error): throw ApplicationFailure(error, process: process)
                case nil: throw ApplicationFailure(subject: String(process), reason: .cancelled(CancellationError()))
            }
        }
    }
}
