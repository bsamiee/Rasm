package import AppKit
package import AppleEvents
import AsyncAlgorithms
import CasePathsCore
package import Encoding
import Files
package import Processes
import Results
package import System

// --- [MODELS] --------------------------------------------------------------------------

package struct InstanceSnapshot: Encodable, Sendable {
    private enum CodingKeys: CodingKey {
        case instance, identifier, name, url, language, launchDate, usage, state, listeningPorts
    }

    let instance: Instance
    let identifier: String
    let url: URL
    let language: Locale.Language?
    let usage: Result<ResourceUsage, Errno>?
    let state: InstanceState
    let listeningPorts: Result<[in_port_t], Errno>

    package init?(_ instance: Instance, state: InstanceState, language: Locale.Language? = nil, usage: Result<ResourceUsage, Errno>? = nil) {
        guard let url = instance.application.bundleURL?.resolvedDirectory, let identifier = instance.application.bundleIdentifier else { return nil }
        (self.instance, self.identifier, self.url, self.language, self.usage, self.state) = (instance, identifier, url, language, usage, state)
        listeningPorts = state.is(\.running) ? Result { () throws(Errno) -> [in_port_t] in try instance.process.listeningPorts() } : .success([])
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(instance.process, forKey: .instance)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encodeIfPresent(instance.application.localizedName, forKey: .name)
        try fields.encode(url, forKey: .url)
        let minimal: String? = language?.minimalIdentifier
        try fields.encodeIfPresent(minimal, forKey: .language)
        try fields.encodeIfPresent(instance.application.launchDate, forKey: .launchDate)
        let used: EncodedResult<ResourceUsage>? = usage.map { read in EncodedResult(read) }
        try fields.encodeIfPresent(used, forKey: .usage)
        try fields.encode(state, forKey: .state)
        try fields.encode(EncodedResult(listeningPorts), forKey: .listeningPorts)
    }
}

package enum LaunchEvent: CaseTagged, Sendable {
    case launched(instance: ProcessInstance, identifier: String, created: Bool, replacedOnDisk: Bool)
    case state(state: InstanceState)
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
        if !newInstance, !running.isEmpty, !arguments.isEmpty || !environment.isEmpty { throw ApplicationFailure(subject: installation.name, reason: .alreadyRunning) }
        let before: Set<pid_t> = Set(running.map(\.process.pid))
        let creates: Bool = newInstance || running.isEmpty
        let configuration = OpenConfiguration()
        configuration.activates = false
        configuration.hides = creates
        configuration.addsToRecentItems = false
        configuration.promptsUserIfNeeded = false
        configuration.allowsRunningApplicationSubstitution = false
        configuration.createsNewApplicationInstance = newInstance
        configuration.arguments = arguments + ["-ApplePersistenceIgnoreState", "YES"]
        configuration.environment = environment
        let ends = clock.now.advanced(by: deadline)
        let hold = try await PermittedFrontList.shared.holding(
            excluding: { application in installation.runs(application) && !(creates && before.contains(application.processIdentifier)) },
            { () async throws(ApplicationFailure) -> (watch: Watch, settled: InstanceState?) in
                let opened: NSRunningApplication
                do {
                    opened =
                        try await documents.isEmpty
                        ? openApplication(at: installation.url, configuration: configuration) : open(documents, withApplicationAt: installation.url, configuration: configuration)
                } catch {
                    let quarantine: Result<Installation.Quarantine?, ApplicationFailure> = Result { () throws(ApplicationFailure) in try installation.quarantine }
                    if case .failure(let unread) = quarantine { output.yield(.failed(unread)) }
                    throw ApplicationFailure(
                        subject: installation.url.path(percentEncoded: false),
                        reason: quarantine[case: \.success].flatMap(\.self).map { quarantine in ApplicationFailure.Reason.quarantined(quarantine, error as NSError) } ?? .foundation(error as NSError),
                    )
                }
                let executable: URL? = opened.processIdentifier == -1 ? installation.executable : nil
                let executing: [ProcessInstance]
                do throws(Errno) {
                    let canonical: FilePath? = try executable.map { executable throws(Errno) in FilePath(try executable.canonical().path(percentEncoded: false)) }
                    executing = try canonical.map { canonical throws(Errno) in try ProcessInstance.all { path in path == canonical } } ?? []
                } catch {
                    throw ApplicationFailure(subject: installation.executable?.path(percentEncoded: false) ?? installation.name, reason: .system(error))
                }
                let found: Instance? = executing.max { first, second in first.started < second.started }.flatMap { process in
                    NSRunningApplication(processIdentifier: process.pid).map { application in Instance(application: application, process: process) }
                }
                let instance: Instance =
                    switch (found, opened.processIdentifier) {
                        case (.some(let found), _): found
                        case (.none, -1): throw ApplicationFailure(subject: installation.url.path(percentEncoded: false), reason: .system(.noSuchProcess))
                        case (.none, _): try Instance(opened)
                    }
                let created: Bool = !before.contains(instance.process.pid)
                let watch = await watches.watch(instance)
                let replacedOnDisk = !created && Result(catching: { () throws(ProcessFailure) in try instance.process.runsInstalledCode() }) == .success(false)
                output.yield(.launched(instance: instance.process, identifier: installation.identifier, created: created, replacedOnDisk: replacedOnDisk))
                let states = await watch.states().removeDuplicates().flatMapLatest { (state: InstanceState) in
                    let status = state[case: \.running]?.status
                    let idle: Bool = status?.isIdle == true && (status != .unresponsive(thread: .sleeping) || instance.application.isFinishedLaunching)
                    return chain([(state, false)].async, AsyncTimerSequence(interval: messagingTimeout, clock: clock).prefix(idle ? 1 : 0).map { _ in (state, true) })
                }
                let expiry = AsyncTimerSequence(interval: clock.now.duration(to: ends), clock: clock).prefix(1).map { _ in (InstanceState, Bool)?.none }
                var settled: InstanceState?
                for await element in merge(states.map(Optional.some), expiry) {
                    guard case (let state, let quiet)? = element else { break }
                    if !quiet { output.yield(.state(state: state)) }
                    settled = state
                    let reported: Bool = state[case: \.exited].map { ended in ended.report != nil || !ended.exit.producesReport } == true
                    if quiet || reported || state.is(\.failed) || state[case: \.running]?.asking.isEmpty == false { break }
                }
                if creates {
                    let (shown, showing) = AsyncStream<Bool>.makeStream(bufferingPolicy: .bufferingNewest(1))
                    let observation = instance.application.observe(\.isHidden, options: [.initial, .new]) { _, change in showing.yield(change.newValue == false) }
                    _ = instance.application.unhide()
                    let held = AsyncTimerSequence(interval: clock.now.duration(to: ends.advanced(by: messagingTimeout)), clock: clock).map { _ in true }
                    _ = await merge(shown, held).first(where: \.self)
                    observation.invalidate()
                }
                return (watch, settled)
            },
        )
        if case .failure(let failure) = hold.released { output.yield(.failed(failure)) }
        return try hold.value.get()
    }

    package func launchEvents(
        _ installation: Installation,
        in dictionaries: ScriptingDictionaries,
        watches: Watches,
        within deadline: Duration = replyDeadline,
        opening documents: [URL] = [],
        with parameters: [String: AppleEventValue] = [:],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
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
                    return [interrupting.map(LaunchEvent.state(state:))].compactMap(\.self)
                } : []
            for reply in replies { output.yield(reply) }
        }
        output.onTermination = { _ in launching.cancel() }
        return events
    }
}
