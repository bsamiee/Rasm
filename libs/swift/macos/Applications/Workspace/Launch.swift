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

public struct RunningInstance: Encodable, Sendable {
    private enum Key: CodingKey {
        case pid, instance, identifier, url, activationPolicy, state, listening, listeningFailure
    }

    public let instance: Instance
    public let identifier: String
    public let url: URL
    public let state: InstanceState

    public init?(_ instance: Instance, state: InstanceState) {
        guard let url = instance.application.bundleURL, let identifier = instance.application.bundleIdentifier else { return nil }
        (self.instance, self.identifier, self.url, self.state) = (instance, identifier, url, state)
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(instance.process.pid, forKey: .pid)
        try fields.encode(instance.process, forKey: .instance)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encode(url, forKey: .url)
        try fields.encode(instance.application.activationPolicy.name, forKey: .activationPolicy)
        try fields.encode(state, forKey: .state)
        let listening = state[case: \.running]?.listening
        try fields.encode(listening?[case: \.success] ?? [], forKey: .listening)
        if case .failure(let error) = listening { try fields.encode(error.description, forKey: .listeningFailure) }
    }
}

public struct Launched: Encodable, Sendable {
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

public enum Launching: CaseKeyed, Sendable {
    case launched(Launched)
    case state(RunningInstance)
    case prompts([RunningInstance])
    case opened(AppleEventValue)
    case failed(ApplicationFailure)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSWorkspace {
    public func launch(
        _ installation: Installation,
        opening documents: [URL] = [],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
    ) async throws(ApplicationFailure) -> (instance: Instance, created: Bool) {
        let url = installation.url
        let running = instances(of: installation.runs)
        switch (newInstance ? nil : running.first, arguments.isEmpty && environment.isEmpty, documents.isEmpty) {
            case (.some(let reused), false, _): throw ApplicationFailure(subject: installation.name, reason: .reused(reused.process.pid))
            case (.some(let reused), true, true): return (reused, false)
            case (.some, true, false), (.none, _, _): break
        }
        let listing: (URL) throws(ApplicationFailure) -> Set<ProcessInstance> = { executable throws(ApplicationFailure) in
            do throws(Errno) {
                let path = FilePath(try executable.canonical().path(percentEncoded: false))
                return Set(try ProcessInstance.executing { running in running == path })
            } catch {
                throw ApplicationFailure(subject: executable.path(percentEncoded: false), reason: .system(error))
            }
        }
        let executable = installation.executable
        let before = try executable.map(listing) ?? []
        let configuration = OpenConfiguration()
        configuration.activates = false
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
        let opened: NSRunningApplication
        do {
            opened = try await documents.isEmpty ? openApplication(at: url, configuration: configuration) : open(documents, withApplicationAt: url, configuration: configuration)
        } catch {
            throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .host(error as NSError))
        }
        guard opened.processIdentifier == -1, let executable else { return (try Instance(opened), !running.map(\.application).contains(opened)) }
        let after = try listing(executable)
        let started = after.subtracting(before).first
        guard let process = started ?? after.first, let application = NSRunningApplication(processIdentifier: process.pid) else {
            throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .noProcess)
        }
        return (Instance(application: application, process: process), started != nil)
    }

    public func launching(
        _ installation: Installation,
        in definitions: ScriptingDefinitions,
        opening documents: [URL] = [],
        with parameters: [String: AppleEventValue] = [:],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
        waiting: Bool = false,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<Launching, Never> & Sendable {
        AsyncStream { output in
            let following = Task {
                await withDiscardingTaskGroup { group in
                    defer { group.cancelAll() }
                    group.addTask {
                        for await held in Self.shared.systemPrompts(naming: [installation.name], clock: clock) {
                            output.yield(.prompts(held.compactMap { instance, state in RunningInstance(instance, state: state) }))
                        }
                    }
                    do throws(ApplicationFailure) {
                        let (instance, created) = try await Self.shared.launch(
                            installation,
                            opening: parameters.isEmpty ? documents : [],
                            arguments: arguments,
                            environment: environment,
                            newInstance: newInstance,
                        )
                        group.cancelAll()
                        let replaced = !created && Result(catching: { () throws(Errno) in try instance.process.runsInstalledCode() }) == .success(false)
                        output.yield(.launched(Launched(instance: instance, identifier: installation.identifier, created: created, replaced: replaced)))
                        let followed = { (waiting: Bool) async -> InstanceState? in
                            var (grace, last): (Int?, InstanceState?) = (nil, nil)
                            for await next in merge(instance.states(clock: clock).map(Optional.some), AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in InstanceState?.none }) {
                                if let next, let running = RunningInstance(instance, state: next) { output.yield(.state(running)) }
                                last = next ?? last
                                switch next {
                                    case .some(.exited(let ended)) where ended.report != nil || ended.exit[case: \.success]?.reports != true: return last
                                    case .some(.running(let running)) where !waiting && (!created || !running.asking.isEmpty): return last
                                    case .some(.running(let running))
                                    where !waiting && running.status.settled && (running.status != .unresponsive(.sleeping) || instance.application.isFinishedLaunching):
                                        if running.windows.contains(where: \.presented) { return last }
                                        grace = grace ?? 0
                                    case .some(.failed) where !waiting: return last
                                    case .some: grace = nil
                                    case .none where grace == 1: return last
                                    case .none: grace = grace.map { ticks in ticks + 1 }
                                }
                            }
                            return last
                        }
                        guard let settled = await followed(waiting && parameters.isEmpty)?[case: \.running], settled.asking.isEmpty, !parameters.isEmpty else { return }
                        let files = AppleEventValue.list(documents.map { document in document.isFileURL ? .file(document) : .text(document.absoluteString) })
                        let events: [Launching] = await race {
                            let failed = { (failure: AppleEventFailure) in Launching.failed(ApplicationFailure(subject: installation.name, reason: .event(failure))) }
                            do throws(AppleEventFailure) {
                                let dictionary = try await definitions.dictionary(of: installation)
                                let reply = await Result { () async throws(AggregateError<AppleEventFailure>) in
                                    try await dictionary.reply(
                                        to: (UInt64(kCoreEventClass) << 32 | UInt64(kAEOpenDocuments)).sdef,
                                        arguments: parameters.merging(["direct": files]) { _, documents in documents },
                                        target: NSAppleEventDescriptor(processIdentifier: instance.process.pid),
                                    )
                                }
                                return switch reply {
                                    case .success(let value): [.opened(value)]
                                    case .failure(let failures): failures.errors.map(failed)
                                }
                            } catch {
                                return [failed(error)]
                            }
                        } against: {
                            let interrupting = await instance.states(clock: clock).first { state in state.is(\.exited) || state[case: \.running]?.asking.isEmpty == false }
                            return [interrupting.flatMap { state in RunningInstance(instance, state: state) }.map(Launching.state)].compactMap(\.self)
                        }
                        for event in events { output.yield(event) }
                        if waiting { _ = await followed(true) }
                    } catch {
                        output.yield(.failed(error))
                    }
                }
                output.finish()
            }
            output.onTermination = { _ in following.cancel() }
        }
    }
}
