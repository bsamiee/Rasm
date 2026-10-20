public import AppKit
import Carbon.OpenScripting
import CoreServices

// --- [MODELS] --------------------------------------------------------------------------

public enum ApplicationMatch: Sendable {
    case identifier(String)
    case identifierPrefix(String)
    case name(String)

    var predicate: String {
        func literal(_ text: String) -> String { text.replacing(/[\\'*]/) { match in "\\\(match.output)" } }
        let (attribute, value): (CFString, String) =
            switch self {
                case .identifier(let identifier): (kMDItemCFBundleIdentifier, literal(identifier))
                case .identifierPrefix(let prefix): (kMDItemCFBundleIdentifier, literal(prefix) + "*")
                case .name(let name): (kMDItemDisplayName, literal(name))
            }
        return "\(attribute as String) == '\(value)'"
    }
}

public struct Installation: Encodable, Hashable, Sendable {
    public struct Instance: Encodable, Hashable, Sendable {
        public let pid: pid_t
        public let finishedLaunching: Bool

        init(_ instance: NSRunningApplication) {
            pid = instance.processIdentifier
            finishedLaunching = instance.isFinishedLaunching
        }
    }

    public let url: URL
    public let identifier: String
    public let name: String
    public let version: String?
    public let running: Instance?
    public let scriptable: Bool

    public var runningApplication: Result<NSRunningApplication, DiscoveryFailure> {
        running.flatMap { running in NSRunningApplication(processIdentifier: running.pid) }.map(Result.success) ?? .failure(.notRunning(url))
    }

    public func launched(within deadline: Duration) async -> Result<(installation: Self, instance: NSRunningApplication), LaunchFailure> {
        await NSWorkspace.shared.launchApplication(at: url, within: deadline).map { instance in
            (Self(url: url, identifier: identifier, name: name, version: version, running: Instance(instance), scriptable: scriptable), instance)
        }
    }
}

extension Installation {
    init?(_ item: MDItem, among runningApplications: [NSRunningApplication]) throws(DiscoveryFailure) {
        switch (MDItemCopyAttribute(item, kMDItemPath) as? String, MDItemCopyAttribute(item, kMDItemCFBundleIdentifier) as? String, MDItemCopyAttribute(item, kMDItemDisplayName) as? String) {
            case (.some(let path), .some(let bundle), .some(let display)):
                let url: URL = URL(filePath: path, directoryHint: .isDirectory)
                self.url = url
                identifier = bundle
                name = display
                version = MDItemCopyAttribute(item, kMDItemVersion) as? String
                running = NSRunningApplication.instances(at: url, among: runningApplications).first.map(Instance.init)
                scriptable = try Result { try url.resourceValues(forKeys: [.applicationIsScriptableKey]).applicationIsScriptable == true }.mapError { error in DiscoveryFailure.unreadable(url, error) }
                    .get()
            default: return nil
        }
    }
}

public enum InstanceChange: Sendable {
    case launched(NSRunningApplication)
    case terminated(NSRunningApplication, status: Int?)
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum DiscoveryFailure: LocalizedError {
    case query(String)
    case unindexed(URL)
    case unreadable(URL, any Error)
    case notRunning(URL)

    public var errorDescription: String? {
        switch self {
            case .query(let query): "Spotlight could not run the query \(query)"
            case .unindexed(let url): "Spotlight indexes no application bundle at \(url.path(percentEncoded: false))"
            case .unreadable(let url, let error): "Launch Services could not read \(url.path(percentEncoded: false)): \(error.localizedDescription)"
            case .notRunning(let url): "No running instance of \(url.path(percentEncoded: false)), launch it first"
        }
    }
}

public enum LaunchFailure: DeadlineFailure, LocalizedError {
    case open(any Error)
    case terminated
    case timedOut(Duration)
    case cancelled

    public var errorDescription: String? {
        switch self {
            case .open(let error): "Launch Services could not open the application: \(error.localizedDescription)"
            case .terminated: "The application exited before it finished launching"
            case .timedOut(let deadline): "The application did not finish launching within \(deadline.formatted(.units(width: .wide)))"
            case .cancelled: "Canceled"
        }
    }
}

public enum ActivationFailure: LocalizedError {
    case launch(LaunchFailure)
    case declined(frontmost: NSRunningApplication?)
    case terminated

    public var errorDescription: String? {
        switch self {
            case .launch(let error): error.localizedDescription
            case .declined(.some(let frontmost)): "macOS kept \(frontmost.localizedName ?? "process \(frontmost.processIdentifier)") in front and did not activate the application"
            case .declined(.none): "macOS left no application in front and did not activate the application"
            case .terminated: "The application exited before it became active"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    // --- [DISCOVERY]
    public func installedApplications(matching matches: [ApplicationMatch], in directories: [URL]?) -> Result<[Installation], DiscoveryFailure> {
        let text: String = "kMDItemContentType == 'com.apple.application-bundle' && (\(matches.map(\.predicate).joined(separator: " || ")))"
        let running: [NSRunningApplication] = runningApplications
        return Result { () throws(DiscoveryFailure) in
            guard let search: MDQuery = MDQueryCreate(nil, text as CFString, nil, [kMDItemCFBundleIdentifier, kMDItemVersion] as CFArray) else { throw .query(text) }
            if let directories { MDQuerySetSearchScope(search, directories as CFArray, 0) }
            guard MDQueryExecute(search, CFOptionFlags(kMDQuerySynchronous.rawValue)) else { throw .query(text) }
            return try (0..<MDQueryGetResultCount(search)).map { index throws(DiscoveryFailure) in
                try Installation(unsafe unsafeBitCast(MDQueryGetResultAtIndex(search, index), to: MDItem.self), among: running)
            }
            .compactMap(\.self)
        }
    }

    public func installedApplication(at url: URL) -> Result<Installation, DiscoveryFailure> {
        Result { () throws(DiscoveryFailure) in try MDItemCreateWithURL(nil, url as CFURL).flatMap { item throws(DiscoveryFailure) in try Installation(item, among: runningApplications) } }
            .flatMap { installation in installation.map(Result.success) ?? .failure(.unindexed(url)) }
    }

    // --- [LIFECYCLE]
    public func launchApplication(at url: URL, within deadline: Duration, activates: Bool = false) async -> Result<NSRunningApplication, LaunchFailure> {
        let configuration: OpenConfiguration = OpenConfiguration()
        configuration.activates = activates
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.appleEvent = NSAppleEventDescriptor(
            eventClass: AEEventClass(kASAppleScriptSuite),
            eventID: AEEventID(kASLaunchEvent),
            targetDescriptor: NSAppleEventDescriptor(applicationURL: url),
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        return await Result { try await openApplication(at: url, configuration: configuration) }
            .mapError(LaunchFailure.open)
            .bind { instance in await instance.reaching(\.isFinishedLaunching, within: deadline).map { instance } }
    }

    public func activateApplication(at url: URL, within deadline: Duration) async -> Result<NSRunningApplication, ActivationFailure> {
        await launchApplication(at: url, within: deadline, activates: true).mapError(ActivationFailure.launch).bind { instance in
            await instance.reaching(\.isActive, within: deadline).map { instance }
                .mapError { failure in
                    switch failure {
                        case .timedOut: .declined(frontmost: frontmostApplication)
                        case .terminated: .terminated
                        case .open, .cancelled: .launch(failure)
                    }
                }
        }
    }

    @MainActor
    public func instanceChanges(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> AsyncStream<InstanceChange> {
        let center: NotificationCenter = notificationCenter
        return AsyncStream { continuation in
            let tasks: [Task<Void, Never>] = [Self.didLaunchApplicationNotification, Self.didTerminateApplicationNotification].map { name in
                Task { @MainActor in
                    let observed: AsyncCompactMapSequence<NotificationCenter.Notifications, InstanceChange> = center.notifications(named: name).compactMap { note in
                        (note.userInfo?[Self.applicationUserInfoKey] as? NSRunningApplication).flatMap { instance in
                            switch (matching(instance), name) {
                                case (false, _): nil
                                case (true, Self.didLaunchApplicationNotification): .launched(instance)
                                case (true, _): .terminated(instance, status: note.userInfo?["NSWorkspaceExitStatusKey"] as? Int)
                            }
                        }
                    }
                    for await next: InstanceChange in observed { continuation.yield(next) }
                }
            }
            continuation.onTermination = { _ in
                for task: Task<Void, Never> in tasks { task.cancel() }
            }
        }
    }
}

extension NSRunningApplication {
    public static func instances(at url: URL, among runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications) -> [NSRunningApplication] {
        let path: String = url.path(percentEncoded: false)
        return runningApplications.filter { instance in instance.activationPolicy != .prohibited && instance.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == path }
    }

    public func reaching(_ state: any KeyPath<NSRunningApplication, Bool> & Sendable, within deadline: Duration) async -> Result<Void, LaunchFailure> {
        let instance: NSRunningApplication = self
        let (states, sink): (AsyncStream<Bool>, AsyncStream<Bool>.Continuation) = AsyncStream.makeStream()
        let observations: [NSKeyValueObservation] = [state, \.isTerminated].map { keyPath in
            instance.observe(keyPath, options: [.initial, .new]) { instance, _ in
                if instance[keyPath: state] || instance.isTerminated { sink.yield(instance[keyPath: state]) }
            }
        }
        defer {
            for observation: NSKeyValueObservation in observations { observation.invalidate() }
            sink.finish()
        }
        return await withDeadline(deadline) {
            switch await states.first(where: { _ in true }) {
                case .some(true): .success(())
                case .some(false): .failure(.terminated)
                case .none: .failure(.cancelled)
            }
        }
    }
}
