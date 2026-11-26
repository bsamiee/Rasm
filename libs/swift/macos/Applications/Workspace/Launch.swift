public import AppKit
public import AppleEvents
import AsyncAlgorithms
import CasePathsCore
public import Encoding
import Files
import Processes
import Results
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct InstanceSnapshot: Encodable, Sendable {
    private enum Key: CodingKey {
        case pid, instance, identifier, url, activationPolicy, language, state, listening, listeningFailure
    }

    public let instance: Instance
    public let identifier: String
    public let url: URL
    public let language: Locale.Language?
    public let state: InstanceState

    public init?(_ instance: Instance, state: InstanceState, language: Locale.Language? = nil) {
        guard let url = instance.application.bundleURL, let identifier = instance.application.bundleIdentifier else { return nil }
        (self.instance, self.identifier, self.url, self.language, self.state) = (instance, identifier, url, language, state)
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(instance.process.pid, forKey: .pid)
        try fields.encode(instance.process, forKey: .instance)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encode(url, forKey: .url)
        try fields.encode(instance.application.activationPolicy.name, forKey: .activationPolicy)
        let minimal: String? = language?.minimalIdentifier
        try fields.encodeIfPresent(minimal, forKey: .language)
        try fields.encode(state, forKey: .state)
        let listening = state[case: \.running]?.listening
        try fields.encode(listening?[case: \.success] ?? [], forKey: .listening)
        if case .failure(let error) = listening { try fields.encode(error.description, forKey: .listeningFailure) }
    }
}

public struct LaunchedInstance: Encodable, Sendable {
    private enum Key: CodingKey {
        case pid, instance, identifier, created, replaced
    }

    public let instance: Instance
    public let identifier: String
    public let created: Bool
    public let replaced: Bool

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(instance.process.pid, forKey: .pid)
        try fields.encode(instance.process, forKey: .instance)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encode(created, forKey: .created)
        if replaced { try fields.encode(true, forKey: .replaced) }
    }
}

public enum LaunchEvent: CaseTagged, Sendable {
    case launched(LaunchedInstance)
    case state(InstanceSnapshot)
    case opened(reply: AppleEventValue)
    case failed(ApplicationFailure)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
        clock: some Clock<Duration> = ContinuousClock(),
    ) async throws(ApplicationFailure) -> (instance: Instance, created: Bool, hold: UUID?) {
        let url = installation.url
        let running = try instances(of: installation.runs)
        switch (newInstance ? nil : running.first, arguments.isEmpty && environment.isEmpty, documents.isEmpty) {
            case (.some(let reused), false, _): throw ApplicationFailure(subject: installation.name, reason: .reused(reused.process.pid))
            case (.some(let reused), true, true): return (reused, false, nil)
            case (.some, true, false), (.none, _, _): break
        }
        let configuration = OpenConfiguration()
        configuration.activates = false
        configuration.hides = newInstance || running.isEmpty
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.createsNewApplicationInstance = newInstance
        configuration.arguments = arguments
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
        let before: Set<pid_t> = Set(running.map(\.process.pid))
        let hold: UUID? =
            try await configuration.hides
            ? PermittedFrontList.shared.hold(excluding: { application in installation.runs(application) && !before.contains(application.processIdentifier) }, clock: clock) : nil
        let requested = clock_gettime_nsec_np(CLOCK_REALTIME) / NSEC_PER_USEC
        let launched: Result<(instance: Instance, created: Bool), ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> (instance: Instance, created: Bool) in
            let opened: NSRunningApplication
            do {
                opened = try await documents.isEmpty ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
            } catch {
                throw ApplicationFailure(
                    subject: url.path(percentEncoded: false),
                    reason: installation.quarantine.map { quarantine in ApplicationFailure.Reason.quarantined(quarantine, error as NSError) } ?? .foundation(error as NSError),
                )
            }
            guard opened.processIdentifier == -1, let executable = installation.executable else { return (try Instance(opened), !before.contains(opened.processIdentifier)) }
            let processes: [ProcessInstance]
            do throws(Errno) {
                let path = FilePath(try executable.canonical().path(percentEncoded: false))
                processes = try ProcessInstance.all { executing in executing == path }
            } catch {
                throw ApplicationFailure(subject: executable.path(percentEncoded: false), reason: .system(error))
            }
            let started = processes.first { process in process.started >= requested }
            guard let process = started ?? processes.first, let application = NSRunningApplication(processIdentifier: process.pid) else {
                throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .system(.noSuchProcess))
            }
            return (Instance(application: application, process: process), started != nil)
        }
        if case .failure = launched { PermittedFrontList.shared.release(hold) }
        let (instance, created) = try launched.get()
        return (instance, created, hold)
    }

    public func launchEvents(
        _ installation: Installation,
        in dictionaries: ScriptingDictionaries,
        watches: Watches,
        within deadline: Duration,
        opening documents: [URL] = [],
        with parameters: [String: AppleEventValue] = [:],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
        waiting: Bool = false,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<LaunchEvent, Never> & Sendable {
        AsyncStream { output in
            let following = Task {
                defer { output.finish() }
                do throws(ApplicationFailure) {
                    let (instance, created, hold) = try await Self.shared.launch(
                        installation,
                        opening: parameters.isEmpty ? documents : [],
                        arguments: arguments,
                        environment: environment,
                        newInstance: newInstance,
                        clock: clock,
                    )
                    let watch = await watches.watch(instance)
                    await watch.addOpenCommandDocuments(documents)
                    let replaced = !created && Result(catching: { () throws(Errno) in try instance.process.runsInstalledCode() }) == .success(false)
                    output.yield(.launched(LaunchedInstance(instance: instance, identifier: installation.identifier, created: created, replaced: replaced)))
                    let follow = { (untilExit: Bool) async -> InstanceState? in
                        var (grace, last): (Int?, InstanceState?) = (nil, nil)
                        var held: Result<ResourceUsage, Errno> = Result { () throws(Errno) in try ResourceUsage(of: instance.process) }
                        for await next in merge(await watch.states().map(Optional.some), AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in InstanceState?.none }) {
                            if let next, let snapshot = InstanceSnapshot(instance, state: next) { output.yield(.state(snapshot)) }
                            let read: Result<ResourceUsage, Errno> = next == nil ? Result { () throws(Errno) in try ResourceUsage(of: instance.process) } : held
                            let cause: ResourceUsage.Interval.Cause? = read.flatMap { (after: ResourceUsage) -> Result<ResourceUsage.Interval, Errno> in held.map(after.since) }[case: \.success]?.cause
                            held = read
                            let moved = next?[case: \.running]?.windows != last?[case: \.running]?.windows
                            last = next ?? last
                            switch next {
                                case .some(.exited(let ended)) where ended.report != nil || ended.exit[case: \.success]?.producesReport != true: return last
                                case .some(.running(let running)) where !untilExit && (!created || !running.asking.isEmpty): return last
                                case .some(.running(let running))
                                where !untilExit && running.status.isIdle && (running.status != .unresponsive(thread: .sleeping) || instance.application.isFinishedLaunching):
                                    grace = moved ? 0 : grace ?? 0
                                case .some(.failed) where !untilExit: return last
                                case .some: grace = nil
                                case .none where grace == 1 && (cause == .idle || cause == nil): return last
                                case .none: grace = grace.map { ticks in min(ticks + 1, 1) }
                            }
                        }
                        return last
                    }
                    let settled = await follow(false)
                    if hold != nil { _ = instance.application.unhide() }
                    if case .failure(let error) = PermittedFrontList.shared.release(hold) { output.yield(.failed(error)) }
                    if let idle = settled?[case: \.running], idle.asking.isEmpty, !parameters.isEmpty {
                        let files = AppleEventValue.list(documents.map { document in document.isFileURL ? .file(document) : .text(document.absoluteString) })
                        let events: [LaunchEvent] = await race {
                            let reply = await Result { () async throws(AggregateError<AppleEventFailure>) in
                                let dictionary = try await Result { () async throws(AppleEventFailure) in try await dictionaries.dictionary(of: installation) }.aggregated(\.self).get()
                                return try await dictionary.send(
                                    .code(UInt64(kCoreEventClass) << 32 | UInt64(kAEOpenDocuments)),
                                    arguments: parameters.merging(["direct": files]) { _, documents in documents },
                                    target: NSAppleEventDescriptor(processIdentifier: instance.process.pid),
                                    within: deadline,
                                )
                            }
                            return switch reply {
                                case .success(let value): [.opened(reply: value)]
                                case .failure(let failures): failures.errors.map { failure -> LaunchEvent in .failed(ApplicationFailure(subject: installation.name, reason: .event(failure))) }
                            }
                        } against: {
                            let interrupting = await watch.states().first { state in state.is(\.exited) || state[case: \.running]?.asking.isEmpty == false }
                            return [interrupting.flatMap { state in InstanceSnapshot(instance, state: state) }.map(LaunchEvent.state)].compactMap(\.self)
                        }
                        for event in events { output.yield(event) }
                    }
                    if waiting { _ = await follow(true) }
                } catch {
                    output.yield(.failed(error))
                }
            }
            output.onTermination = { _ in following.cancel() }
        }
    }
}
