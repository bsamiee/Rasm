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
    public let url: URL
    public let identifier: String
    public let name: String
    public let version: String?
    public let instances: [pid_t]
    public let scriptable: Bool

    public static func at(_ url: URL, among runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications) -> Result<Self, DiscoveryFailure> {
        let bundle: URL = url.resolvingSymlinksInPath()
        guard let info: Bundle = Bundle(url: bundle), let identifier: String = info.bundleIdentifier else { return .failure(.unbundled(bundle)) }
        return Result { try bundle.resourceValues(forKeys: [.applicationIsScriptableKey]) }
            .mapError { error in DiscoveryFailure.unreadable(bundle, error) }
            .map { values in
                Self(
                    url: bundle,
                    identifier: identifier,
                    name: FileManager.default.displayName(atPath: bundle.path(percentEncoded: false)),
                    version: info.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String,
                    instances: NSRunningApplication.instances(at: bundle, among: runningApplications).map(\.processIdentifier),
                    scriptable: values.applicationIsScriptable == true,
                )
            }
    }

    public func runningApplication(_ pid: pid_t?) -> Result<NSRunningApplication, DiscoveryFailure> {
        let live: [NSRunningApplication] = instances.compactMap(NSRunningApplication.init(processIdentifier:))
        let chosen: [NSRunningApplication] = live.filter { instance in pid.map { pid in instance.processIdentifier == pid } ?? true }
        let pids: [String] = live.map { instance in String(instance.processIdentifier) }
        return switch (chosen.first, chosen.count) {
            case (.some(let instance), 1): .success(instance)
            case (.none, _) where live.isEmpty: .failure(.notRunning(url))
            case (.none, _): .failure(.unmatched(Unmatched(subject: "running instance of \(name)", query: pid.map(String.init), candidates: pids)))
            case (.some, _): .failure(.unmatched(Unmatched(subject: "single running instance of \(name)", query: nil, candidates: pids)))
        }
    }
}

public enum InstanceChange: Sendable {
    case launched(NSRunningApplication)
    case terminated(NSRunningApplication, status: Int?)
}

public enum InstanceEvent: Sendable {
    case instance(InstanceChange)
    case status(NSRunningApplication, Result<ApplicationStatus, ElementFailure>)
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum DiscoveryFailure: CustomNSError, LocalizedError {
    case query(String)
    case unpathed
    case unbundled(URL)
    case unreadable(URL, any Error)
    case notRunning(URL)
    case unmatched(Unmatched)

    public var errorDescription: String? {
        switch self {
            case .query(let query): "Spotlight could not run the query \(query)"
            case .unpathed: "Spotlight listed an application bundle without a path"
            case .unbundled(let url): "\(url.path(percentEncoded: false)) holds no application bundle with an identifier"
            case .unreadable(let url, let error): "Launch Services could not read \(url.path(percentEncoded: false)): \(error.localizedDescription)"
            case .notRunning(let url): "No running instance of \(url.path(percentEncoded: false)), launch it first"
            case .unmatched(let miss): miss.errorDescription
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .unreadable(_, let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

public enum LaunchFailure: ApplicationWaitFailure, CustomNSError, LocalizedError {
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

    public var errorUserInfo: [String: Any] {
        if case .open(let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

public enum ActivationFailure: ApplicationWaitFailure, CustomNSError, LocalizedError {
    case launch(LaunchFailure)
    case timedOut(Duration)
    case terminated
    case cancelled

    public var errorDescription: String? {
        switch self {
            case .launch(let error): error.localizedDescription
            case .timedOut(let deadline): "macOS did not make the application active within \(deadline.formatted(.units(width: .wide)))"
            case .terminated: "The application exited before it became active"
            case .cancelled: "Canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .launch(let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    // --- [DISCOVERY]
    @MainActor
    public func applications(matching match: ApplicationMatch, _ more: ApplicationMatch..., in directories: [URL] = []) -> AsyncStream<Result<[Installation], DiscoveryFailure>> {
        let text: String = "kMDItemContentType == 'com.apple.application-bundle' && (\(([match] + more).map(\.predicate).joined(separator: " || ")))"
        let query: NSMetadataQuery = NSMetadataQuery()
        query.predicate = NSPredicate(fromMetadataQueryString: text)
        query.searchScopes = directories
        query.sortDescriptors = [NSMetadataItemDisplayNameKey, NSMetadataItemVersionKey].map { key in NSSortDescriptor(key: key, ascending: true) }
        query.operationQueue = .main
        let (events, sink): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream()
        let (listings, continuation): (AsyncStream<Result<[Installation], DiscoveryFailure>>, AsyncStream<Result<[Installation], DiscoveryFailure>>.Continuation) = AsyncStream.makeStream()
        let observation: NSKeyValueObservation = observe(\.runningApplications) { _, _ in sink.yield() }
        let notes: any NSObjectProtocol = NotificationCenter.default.addObserver(forName: nil, object: query, queue: nil) { _ in sink.yield() }
        let following: Task<Void, Never> = Task { @MainActor in
            defer {
                query.stop()
                observation.invalidate()
                NotificationCenter.default.removeObserver(notes)
                continuation.finish()
            }
            guard query.start() else {
                continuation.yield(.failure(.query(text)))
                return
            }
            _ = await events.reduce(nil as [Installation]?) { @MainActor last, _ in
                guard !query.isGathering else { return last }
                query.disableUpdates()
                defer { query.enableUpdates() }
                let running: [NSRunningApplication] = runningApplications
                let listing: Result<[Installation], DiscoveryFailure> = Result { () throws(DiscoveryFailure) in
                    try (0..<query.resultCount).map { index throws(DiscoveryFailure) in
                        guard let path: String = (query.result(at: index) as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String else { throw .unpathed }
                        return try Installation.at(URL(filePath: path, directoryHint: .isDirectory), among: running).get()
                    }
                }
                switch listing {
                    case .success(last):
                        return last
                    case .success(let rows):
                        continuation.yield(listing)
                        return rows
                    case .failure:
                        continuation.yield(listing)
                        return last
                }
            }
        }
        continuation.onTermination = { _ in following.cancel() }
        return listings
    }

    // --- [LIFECYCLE]
    public func launchApplication(at url: URL, within deadline: Duration, createsNewApplicationInstance: Bool, activates: Bool) async -> Result<NSRunningApplication, LaunchFailure> {
        let configuration: OpenConfiguration = OpenConfiguration()
        configuration.activates = activates
        configuration.createsNewApplicationInstance = createsNewApplicationInstance
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
        let end: ContinuousClock.Instant = .now + deadline
        return await launchApplication(at: url, within: deadline, createsNewApplicationInstance: false, activates: true).mapError(ActivationFailure.launch).bind { instance in
            await instance.reaching(\.isActive, within: ContinuousClock.now.duration(to: end)).map { instance }
        }
    }

    @MainActor
    public func instanceChanges(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> AsyncStream<InstanceChange> {
        let center: NotificationCenter = notificationCenter
        return AsyncStream { continuation in
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
    }

    @MainActor
    public func instanceEvents(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> AsyncStream<InstanceEvent> {
        let changes: AsyncStream<InstanceChange> = instanceChanges(of: matching)
        let opening: [NSRunningApplication] = runningApplications.filter { instance in instance.isFinishedLaunching && instance.createsWindows && matching(instance) }
        return AsyncStream { continuation in
            let following: Task<Void, Never> = Task { @MainActor in
                await withDiscardingTaskGroup { group in
                    func observe(_ instance: NSRunningApplication) {
                        group.addTask { @concurrent in
                            for await next: Result<ApplicationStatus, ElementFailure> in StatusChanges(instance) { continuation.yield(.status(instance, next)) }
                        }
                    }
                    for instance: NSRunningApplication in opening { observe(instance) }
                    for await change: InstanceChange in changes {
                        continuation.yield(.instance(change))
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
    var createsWindows: Bool { activationPolicy != .prohibited }

    public static func instances(at url: URL, among runningApplications: [NSRunningApplication] = NSWorkspace.shared.runningApplications) -> [NSRunningApplication] {
        let path: String = url.path(percentEncoded: false)
        return runningApplications.filter { instance in instance.createsWindows && instance.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == path }
    }

    public func reaching<Failure: ApplicationWaitFailure>(_ state: any KeyPath<NSRunningApplication, Bool> & Sendable, within deadline: Duration) async -> Result<Void, Failure> {
        let (states, sink): (AsyncStream<Bool>, AsyncStream<Bool>.Continuation) = AsyncStream.makeStream()
        let observations: [NSKeyValueObservation] = [state, \.isTerminated].map { keyPath in
            observe(keyPath, options: [.initial, .new]) { instance, _ in
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
