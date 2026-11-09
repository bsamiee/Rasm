public import AppKit
public import AppleEvents
public import Applications
import AsyncAlgorithms
import CoreServices
public import EffectRPC
import OrderedCollections
import Processes
import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

private let identifierKey: String = kMDItemCFBundleIdentifier as String

// --- [MODELS] --------------------------------------------------------------------------

public enum Request: Decodable, Sendable {
    case installations(identifiers: [String]?, prefix: String?)
    case instances(identifiers: [String]?, prefix: String?)
    case pressure
    case applications(toOpen: URL)
    case read(pid: pid_t, window: CGWindowID?)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool)
    case perform(pid: pid_t, actions: [ElementAction])
    case quit(pid: pid_t, force: Bool)
    case terms(url: URL)
    case definition(url: URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(pid: pid_t, command: String, arguments: [String: AppleEventValue], artifacts: [URL])
    case screenshot(pid: pid_t, window: CGWindowID?, longestSide: Int?, directory: URL)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func selecting(_ identifiers: [String]?, _ prefix: String?, within admitting: NSPredicate) -> NSPredicate {
    let selected: [NSPredicate] =
        (identifiers ?? []).map { identifier in NSPredicate(format: "%K == %@", identifierKey, identifier) }
        + [prefix].compactMap(\.self).map { prefix in NSPredicate(format: "%K BEGINSWITH %@", identifierKey, prefix) }
    return selected.isEmpty ? admitting : NSCompoundPredicate(andPredicateWithSubpredicates: [admitting, NSCompoundPredicate(orPredicateWithSubpredicates: selected)])
}

private func admitted(_ identifier: String?, by admitting: NSPredicate) -> Bool {
    identifier.map { identifier in admitting.evaluate(with: [identifierKey: identifier]) } == true
}

private func installation(at url: URL, admitting: () -> NSPredicate) throws(ApplicationFailure) -> Installation {
    let installation: Installation = try Installation(at: url)
    let admission: NSPredicate = admitting()
    guard admitted(installation.identifier, by: admission) else {
        throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .unmatched(Unmatched(subject: admission.predicateFormat, query: installation.identifier, candidates: [])))
    }
    return installation
}

private func running(_ pid: pid_t, admitting: () -> NSPredicate) throws(ApplicationFailure) -> NSRunningApplication {
    guard let instance: NSRunningApplication = NSRunningApplication(processIdentifier: pid), !instance.isTerminated else {
        throw ApplicationFailure(subject: String(pid), reason: .noProcess)
    }
    let admission: NSPredicate = admitting()
    guard admitted(instance.bundleIdentifier, by: admission) else {
        throw ApplicationFailure(subject: String(pid), reason: .unmatched(Unmatched(subject: admission.predicateFormat, query: instance.bundleIdentifier, candidates: [])))
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
            case .read(let pid, let window):
                return try await .value(running(pid, admitting: admitting).reading(window: window))
            case .launch(let url, let documents, let arguments, let environment, let new):
                return try .stream(
                    NSWorkspace.shared.launching(installation(at: url, admitting: admitting), opening: documents, arguments: arguments, environment: environment, newInstance: new)
                        .map(\.self)
                )
            case .perform(let pid, let actions):
                let instance: NSRunningApplication = try running(pid, admitting: admitting)
                var next: InstanceState?
                for action: ElementAction in actions { next = try await action.perform(in: pid).state }
                let settled: InstanceState = if let next { next } else { await instance.state() }
                return .value(settled)
            case .quit(let pid, let force):
                var last: InstanceState?
                for await outcome: Result<InstanceState, ApplicationFailure> in try running(pid, admitting: admitting).quitting(force: force) { last = try outcome.get() }
                return .value(last)
            case .terms(let url):
                return try await .value(definitions.dictionary(of: installation(at: url, admitting: admitting)))
            case .definition(let url, let kind, let name):
                return try await .value(definitions.dictionary(of: installation(at: url, admitting: admitting)).text(of: kind, named: name))
            case .execute(let pid, let command, let arguments, let artifacts):
                let instance: NSRunningApplication = try running(pid, admitting: admitting)
                guard let url: URL = instance.bundleURL else { throw ApplicationFailure(subject: String(pid), reason: .noPath) }
                let scripted: Installation = try installation(at: url, admitting: admitting)
                let dictionary: ScriptingDictionary = try await definitions.dictionary(of: scripted)
                let event: NSAppleEventDescriptor = try dictionary.event(
                    dictionary.command(command, taking: Set(arguments.keys)),
                    arguments: arguments,
                    target: NSAppleEventDescriptor(processIdentifier: pid),
                    refusing: refusing(scripted),
                )
                return try await .value(Executed(reply: AppleEventValue(event.sent(), in: dictionary), reading: artifacts))
            case .screenshot(let pid, let window, let longestSide, let directory):
                let shots: [CGWindowID: Result<URL, ApplicationFailure>] = try await running(pid, admitting: admitting).capturing(into: directory, window: window, longestSide: longestSide)
                return .value(Dictionary(uniqueKeysWithValues: shots.map { number, shot in (String(number), Outcome(shot)) }))
        }
    }
}
