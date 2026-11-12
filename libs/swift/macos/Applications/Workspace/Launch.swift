public import AppKit
import Files
import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct RunningInstance: Encodable, Sendable {
    private enum Key: CodingKey {
        case pid, instance, identifier, url, activationPolicy, state
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
        try fields.encode(instance.application.activationPolicy, forKey: .activationPolicy)
        try fields.encode(state, forKey: .state)
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

public enum Launching: Encodable, Sendable {
    private enum Key: CodingKey {
        case launched, state, prompts, failed
    }

    case launched(Launched)
    case state(RunningInstance)
    case prompts([RunningInstance])
    case failed(ApplicationFailure)

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        switch self {
            case .launched(let launched): try fields.encode(launched, forKey: .launched)
            case .state(let state): try fields.encode(state, forKey: .state)
            case .prompts(let prompts): try fields.encode(prompts, forKey: .prompts)
            case .failed(let failure): try fields.encode(failure, forKey: .failed)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public init(opening document: URL) throws(ApplicationFailure) {
        let subject = document.isFileURL ? document.path(percentEncoded: false) : document.absoluteString
        if document.isFileURL {
            do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: subject, reason: .host(error as NSError)) }
        }
        guard let handler = NSWorkspace.shared.urlForApplication(toOpen: document) else { throw ApplicationFailure(subject: subject, reason: .noHandler) }
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
    ) async throws(ApplicationFailure) -> (instance: Instance, created: Bool) {
        let url = installation.url
        let running = runningApplications.filter(installation.runs).compactMap { application in try? Instance(application) }
        let creating = newInstance || running.isEmpty
        if let reused = running.first, !creating, documents.isEmpty { return (reused, false) }
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
        opening documents: [URL] = [],
        arguments: [String] = [],
        environment: [String: String] = [:],
        newInstance: Bool = false,
        waiting: Bool = false,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<Launching, Never> & Sendable {
        AsyncStream { output in
            let prompting = Task {
                for await held in Self.shared.systemPrompts(naming: [installation.name], clock: clock) {
                    output.yield(.prompts(held.compactMap { instance, state in RunningInstance(instance, state: state) }))
                }
            }
            let following = Task {
                defer {
                    prompting.cancel()
                    output.finish()
                }
                do throws(ApplicationFailure) {
                    let (instance, created) = try await Self.shared.launch(
                        installation,
                        opening: documents,
                        arguments: arguments,
                        environment: environment,
                        newInstance: newInstance,
                    )
                    prompting.cancel()
                    let replaced = !created && Result(catching: { () throws(Errno) in try instance.process.runsInstalledCode() }) == .success(false)
                    output.yield(.launched(Launched(instance: instance, identifier: installation.identifier, created: created, replaced: replaced)))
                    for await state in instance.states(clock: clock) {
                        if let running = RunningInstance(instance, state: state) { output.yield(.state(running)) }
                        switch state {
                            case .running(let running) where !waiting && (!created || running.status.settled || !running.asking.isEmpty): return
                            case .failed where !waiting: return
                            case .running, .failed, .exited: continue
                        }
                    }
                } catch {
                    output.yield(.failed(error))
                }
            }
            output.onTermination = { _ in
                prompting.cancel()
                following.cancel()
            }
        }
    }
}
