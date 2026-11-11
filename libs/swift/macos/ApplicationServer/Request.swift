public import AppKit
public import AppleEvents
public import Applications
import CoreServices
public import EffectRPC
public import Processes
import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

private let identifierKey: String = kMDItemCFBundleIdentifier as String

// --- [MODELS] --------------------------------------------------------------------------

public enum Request: Decodable, Sendable {
    case installations(identifiers: [String]?, prefix: String?)
    case instances(identifiers: [String]?, prefix: String?)
    case pressure
    case applications(toOpen: URL)
    case read(instance: ProcessInstance, window: CGWindowID?)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool)
    case perform(instance: ProcessInstance, actions: [ElementAction])
    case quit(instance: ProcessInstance, force: Bool)
    case terms(url: URL)
    case definition(url: URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(instance: ProcessInstance, command: String, arguments: [String: AppleEventValue], artifacts: [URL])
    case screenshot(instance: ProcessInstance, window: CGWindowID?, longestSide: Int?, directory: URL)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func selecting(_ identifiers: [String]?, _ prefix: String?, within admitting: NSPredicate) -> NSPredicate {
    let selected =
        (identifiers ?? []).map { identifier in NSPredicate(format: "%K == %@", identifierKey, identifier) }
        + [prefix].compactMap(\.self).map { prefix in NSPredicate(format: "%K BEGINSWITH %@", identifierKey, prefix) }
    return selected.isEmpty ? admitting : NSCompoundPredicate(andPredicateWithSubpredicates: [admitting, NSCompoundPredicate(orPredicateWithSubpredicates: selected)])
}

private func admitted(_ identifier: String?, by admitting: NSPredicate) -> Bool {
    identifier.map { identifier in admitting.evaluate(with: [identifierKey: identifier]) } == true
}

private func installation(at url: URL, admitting: () -> NSPredicate) throws(ApplicationFailure) -> Installation {
    let installation = try Installation(at: url)
    let admission = admitting()
    guard admitted(installation.identifier, by: admission) else {
        throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .unmatched(Unmatched(subject: admission.predicateFormat, query: installation.identifier, candidates: [])))
    }
    return installation
}

private func running(_ process: ProcessInstance, admitting: () -> NSPredicate) throws(ApplicationFailure) -> Instance {
    guard let application = NSRunningApplication(processIdentifier: process.pid) else { throw ApplicationFailure(subject: process.rawValue, reason: .noProcess) }
    let instance = try Instance(application)
    guard instance.process == process else { throw ApplicationFailure(subject: process.rawValue, reason: .noProcess) }
    let admission = admitting()
    guard admitted(application.bundleIdentifier, by: admission) else {
        throw ApplicationFailure(subject: process.rawValue, reason: .unmatched(Unmatched(subject: admission.predicateFormat, query: application.bundleIdentifier, candidates: [])))
    }
    return instance
}

extension Request {
    public func perform(admitting: @escaping @Sendable () -> NSPredicate, refusing: @Sendable (Installation) -> Set<UInt64>, definitions: ScriptingDefinitions) async throws -> Response {
        switch self {
            case .installations(let identifiers, let prefix):
                return await .stream(
                    Installation.search(selecting(identifiers, prefix, within: admitting()), in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask))
                        .map { listing -> any Encodable & Sendable in Outcome(listing) }
                )
            case .instances(let identifiers, let prefix):
                return .stream(
                    NSWorkspace.shared.instanceStates { instance in admitted(instance.bundleIdentifier, by: selecting(identifiers, prefix, within: admitting())) }
                        .map { states -> any Encodable & Sendable in states.compactMap { instance, state in RunningInstance(instance, state: state) } }
                )
            case .pressure:
                return .stream(SystemPressure.changes().map { pressure -> any Encodable & Sendable in Outcome(pressure) })
            case .applications(let file):
                return .value(NSWorkspace.shared.urlsForApplications(toOpen: file))
            case .read(let process, let window):
                return try await .value(running(process, admitting: admitting).reading(window: window))
            case .launch(let url, let documents, let arguments, let environment, let new):
                return try .stream(
                    NSWorkspace.shared.launching(installation(at: url, admitting: admitting), opening: documents, arguments: arguments, environment: environment, newInstance: new)
                        .map(\.self)
                )
            case .perform(let process, let actions):
                let instance = try running(process, admitting: admitting)
                var next: InstanceState?
                for action in actions { next = try await action.perform(in: process.pid).state }
                let settled = if let next { next } else { await instance.state() }
                return .value(settled)
            case .quit(let process, let force):
                var last: InstanceState?
                for await outcome in try running(process, admitting: admitting).quit(force: force) { last = try outcome.get() }
                return .value(last)
            case .terms(let url):
                return try await .value(definitions.dictionary(of: installation(at: url, admitting: admitting)))
            case .definition(let url, let kind, let name):
                return try await .value(definitions.dictionary(of: installation(at: url, admitting: admitting)).text(of: kind, named: name))
            case .execute(let process, let command, let arguments, let artifacts):
                let instance = try running(process, admitting: admitting)
                guard let url = instance.application.bundleURL else { throw ApplicationFailure(subject: process.rawValue, reason: .noPath) }
                let scripted = try installation(at: url, admitting: admitting)
                let reply = try await definitions.dictionary(of: scripted)
                    .reply(to: command, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: process.pid), refusing: refusing(scripted))
                return .value(Executed(reply: reply, reading: artifacts))
            case .screenshot(let process, let window, let longestSide, let directory):
                let shots = try await running(process, admitting: admitting).capturing(into: directory, window: window, longestSide: longestSide)
                return .value(Dictionary(uniqueKeysWithValues: shots.map { number, shot in (String(number), Outcome(shot)) }))
        }
    }
}
