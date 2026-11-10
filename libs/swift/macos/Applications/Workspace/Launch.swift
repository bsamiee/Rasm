public import AppKit
import AsyncAlgorithms
import OrderedCollections
import Processes
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct RunningInstance: Encodable, Sendable {
    private enum Key: CodingKey {
        case pid, identifier, url, activationPolicy, state
    }

    public let instance: NSRunningApplication
    public let identifier: String
    public let url: URL
    public let state: InstanceState

    public init?(_ instance: NSRunningApplication, state: InstanceState) {
        guard let url = instance.bundleURL, let identifier = instance.bundleIdentifier else { return nil }
        (self.instance, self.identifier, self.url, self.state) = (instance, identifier, url, state)
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(instance.processIdentifier, forKey: .pid)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encode(url, forKey: .url)
        try fields.encode(instance.activationPolicy.name, forKey: .activationPolicy)
        try fields.encode(state, forKey: .state)
    }
}

public struct Launched: Encodable, Sendable {
    public let pid: pid_t
    public let identifier: String
    public let created: Bool
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
        let path = document.path(percentEncoded: false)
        do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: path, reason: .host(error as NSError)) }
        guard let handler = NSWorkspace.shared.urlForApplication(toOpen: document) else { throw ApplicationFailure(subject: path, reason: .noHandler) }
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
        let url = installation.url
        let running = runningApplications.filter(installation.runs)
        let creating = newInstance || running.isEmpty
        if let reused = running.first, !creating, documents.isEmpty { return (reused, false) }
        let executable = installation.executable?.path(percentEncoded: false)
        let listing: (String) throws(ApplicationFailure) -> Set<pid_t> = { executable throws(ApplicationFailure) in
            do throws(Errno) { return try processes(executing: executable) } catch { throw ApplicationFailure(subject: executable, reason: .system(error)) }
        }
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
        guard opened.processIdentifier == -1, let executable else { return (opened, !running.contains(opened)) }
        let after = try listing(executable)
        let started = after.subtracting(before).first
        guard let instance = (started ?? after.first).flatMap(NSRunningApplication.init(processIdentifier:)) else {
            throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .noProcess)
        }
        return (instance, started != nil)
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
                    output.yield(.launched(Launched(pid: instance.processIdentifier, identifier: installation.identifier, created: created)))
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
