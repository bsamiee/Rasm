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
        guard let url = instance.application.bundleURL?.resolvedDirectory, let identifier = instance.application.bundleIdentifier else { return nil }
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
        opening documents: [URL],
        arguments: [String],
        environment: [String: String],
        newInstance: Bool,
        within deadline: Duration,
        watches: Watches,
        reporting output: AsyncStream<LaunchEvent>.Continuation,
        clock: some Clock<Duration> & Sendable,
    ) async throws(ApplicationFailure) -> (watch: Watch, settled: InstanceState?) {
        let running = try instances(of: installation.runs)
        let reused: Instance? =
            switch (newInstance ? nil : running.first, arguments.isEmpty && environment.isEmpty, documents.isEmpty) {
                case (.some(let reused), false, _): throw ApplicationFailure(subject: installation.name, reason: .reused(reused.process.pid))
                case (.some(let reused), true, true): reused
                case (.some, true, false), (.none, _, _): nil
            }
        let creates: Bool = reused == nil && (newInstance || running.isEmpty)
        let configuration = OpenConfiguration()
        configuration.activates = false
        configuration.hides = creates
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.createsNewApplicationInstance = newInstance
        configuration.arguments = creates ? arguments + ["-ApplePersistenceIgnoreState", "YES"] : arguments
        configuration.environment = environment
        let before: Set<pid_t> = Set(running.map(\.process.pid))
        let requested = clock_gettime_nsec_np(CLOCK_REALTIME) / NSEC_PER_USEC
        return try await PermittedFrontList.shared.holding(
            excluding: { application in installation.runs(application) && !(creates && before.contains(application.processIdentifier)) },
            for: deadline,
            clock: clock,
        ) { () async throws(ApplicationFailure) -> (watch: Watch, settled: InstanceState?) in
            let opened: NSRunningApplication?
            do {
                opened =
                    try await reused != nil
                    ? nil
                    : documents.isEmpty ? openApplication(at: installation.url, configuration: configuration) : open(documents, withApplicationAt: installation.url, configuration: configuration)
            } catch {
                let quarantine: Result<Installation.Quarantine?, ApplicationFailure> = Result { () throws(ApplicationFailure) in try installation.quarantine }
                if case .failure(let unread) = quarantine { output.yield(.failed(unread)) }
                throw ApplicationFailure(
                    subject: installation.url.path(percentEncoded: false),
                    reason: quarantine[case: \.success].flatMap(\.self).map { quarantine in ApplicationFailure.Reason.quarantined(quarantine, error as NSError) } ?? .foundation(error as NSError),
                )
            }
            let executing: [ProcessInstance]
            do throws(Errno) {
                let path: FilePath? = try (opened?.processIdentifier == -1 ? installation.executable : nil).map { executable throws(Errno) in
                    FilePath(try executable.canonical().path(percentEncoded: false))
                }
                executing = try path.map { path throws(Errno) in try ProcessInstance.all { executable in executable == path } } ?? []
            } catch {
                throw ApplicationFailure(subject: installation.executable?.path(percentEncoded: false) ?? installation.name, reason: .system(error))
            }
            let started: ProcessInstance? = executing.first { process in process.started >= requested }
            let found: Instance? = (started ?? executing.first).flatMap { process in
                NSRunningApplication(processIdentifier: process.pid).map { application in Instance(application: application, process: process) }
            }
            let (instance, created): (Instance, Bool) =
                switch (reused, opened, found) {
                    case (.some(let reused), _, _): (reused, false)
                    case (.none, _, .some(let found)): (found, started != nil)
                    case (.none, .some(let opened), .none) where opened.processIdentifier != -1: (try Instance(opened), !before.contains(opened.processIdentifier))
                    case (.none, _, .none): throw ApplicationFailure(subject: installation.url.path(percentEncoded: false), reason: .system(.noSuchProcess))
                }
            let watch = await watches.watch(instance)
            let replaced = !created && Result(catching: { () throws(Errno) in try instance.process.runsInstalledCode() }) == .success(false)
            output.yield(.launched(LaunchedInstance(instance: instance, identifier: installation.identifier, created: created, replaced: replaced)))
            let states = await watch.states().removeDuplicates().flatMapLatest { (state: InstanceState) in
                let idle: Bool =
                    state[case: \.running].map { running in running.status.isIdle && (running.status != .unresponsive(thread: .sleeping) || instance.application.isFinishedLaunching) } == true
                let quiet = chain([()].async, AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in }).prefix(idle ? .max : 0)
                    .map { () in Result { () throws(Errno) in try ResourceUsage(of: instance.process) } }
                    .adjacentPairs()
                    .map { start, end in (state, Optional(end.flatMap { end in start.map(end.since) })) }
                return chain([(state, Result<ResourceUsage.Interval, Errno>?.none)].async, quiet)
            }
            let expiry = AsyncTimerSequence(interval: deadline, clock: clock).prefix(1).map { _ in (InstanceState, Result<ResourceUsage.Interval, Errno>?)?.none }
            var settled: InstanceState?
            for await element in merge(states.map(Optional.some), expiry) {
                guard case (let state, let quiet)? = element else { break }
                if quiet == nil, let snapshot = InstanceSnapshot(instance, state: state) { output.yield(.state(snapshot)) }
                settled = state
                let settles: Bool =
                    switch (state, quiet) {
                        case (.exited(let ended), _): ended.report != nil || ended.exit[case: \.success]?.producesReport != true
                        case (.running(let running), _) where !created || !running.asking.isEmpty: true
                        case (.failed, _), (.running, .some(.failure)): true
                        case (.running, .some(.success(let interval))): ![ResourceUsage.Interval.Cause.running, .runnable, .paging].contains(interval.cause)
                        case (.running, .none): false
                    }
                if settles { break }
            }
            if creates { instance.application.unhide() }
            return (watch, settled)
        }
        .get()
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
        let (events, output) = AsyncStream<LaunchEvent>.makeStream()
        let launching = Task {
            defer { output.finish() }
            let launched = await Result { () async throws(ApplicationFailure) in
                try await Self.shared.launch(
                    installation,
                    opening: parameters.isEmpty ? documents : [],
                    arguments: arguments,
                    environment: environment,
                    newInstance: newInstance,
                    within: deadline,
                    watches: watches,
                    reporting: output,
                    clock: clock,
                )
            }
            if case .failure(let error) = launched { output.yield(.failed(error)) }
            guard case .success(let (watch, settled)) = launched else { return }
            await watch.addOpenCommandDocuments(documents)
            let opening: Bool = !parameters.isEmpty && settled?[case: \.running]?.asking.isEmpty == true
            let files = AppleEventValue.list(documents.map { document in document.isFileURL ? .file(document) : .text(document.absoluteString) })
            let replies: [LaunchEvent] =
                await opening
                ? race {
                    let reply = await Result { () async throws(AggregateError<AppleEventFailure>) in
                        let dictionary = try await Result { () async throws(AppleEventFailure) in try await dictionaries.dictionary(of: installation) }.aggregated(\.self).get()
                        return try await dictionary.send(
                            .code(UInt64(kCoreEventClass) << 32 | UInt64(kAEOpenDocuments)),
                            arguments: parameters.merging(["direct": files]) { _, documents in documents },
                            target: NSAppleEventDescriptor(processIdentifier: watch.instance.process.pid),
                            within: deadline,
                        )
                    }
                    return switch reply {
                        case .success(let value): [.opened(reply: value)]
                        case .failure(let failures): failures.errors.map { failure -> LaunchEvent in .failed(ApplicationFailure(subject: installation.name, reason: .event(failure))) }
                    }
                } against: {
                    let interrupting = await watch.states().first { state in state.is(\.exited) || state[case: \.running]?.asking.isEmpty == false }
                    return [interrupting.flatMap { state in InstanceSnapshot(watch.instance, state: state) }.map(LaunchEvent.state)].compactMap(\.self)
                } : []
            for reply in replies { output.yield(reply) }
            guard waiting else { return }
            for await state in await watch.states() {
                if let snapshot = InstanceSnapshot(watch.instance, state: state) { output.yield(.state(snapshot)) }
                if state[case: \.exited].map({ ended in ended.report != nil || ended.exit[case: \.success]?.producesReport != true }) == true { return }
            }
        }
        output.onTermination = { _ in launching.cancel() }
        return events
    }
}
