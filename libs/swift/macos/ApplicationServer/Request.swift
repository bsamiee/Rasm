public import AppKit
public import AppleEvents
public import Applications
import CoreServices
import CurrentProcess
public import Files
public import JSONRPC
import Logging
public import Processes
import Results
public import ScreenCapture
import Telemetry

// --- [CONSTANTS] -----------------------------------------------------------------------

private let identifierKey = kMDItemCFBundleIdentifier as String

// --- [MODELS] --------------------------------------------------------------------------

public enum Request: Decodable, Sendable {
    public struct Subject: Decodable, Sendable {
        public let url: URL
        public let instance: ProcessInstance?
    }

    case installations(identifiers: [String]?, prefix: String?)
    case instances(identifiers: [String])
    case events(identifiers: [String])
    case pressure
    case nodes(instance: ProcessInstance, root: Element.ID?, depth: Int)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool)
    case act(instance: ProcessInstance, actions: [ElementAction])
    case quit(instance: ProcessInstance, force: Bool)
    case terms(url: URL)
    case definition(url: URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(instance: ProcessInstance, command: String?, arguments: [String: AppleEventValue], artifacts: [URL])
    case capture(instance: ProcessInstance, source: Source?, longestSide: Int?)
    case diagnostics(application: Subject?, folders: [LogFolder], since: Double?, limit: Int)
}

private struct Acted: Encodable, Sendable {
    let outcomes: [Outcome<CGWindowID?>]
    let state: InstanceState
}

private struct Diagnosed: Encodable, Sendable {
    let reports: [DiagnosticReport]
    let logs: [LogLine]
    let unified: Outcome<[Processes.LogEvent]>
    let unreadable: [String]
}

private enum Served<Extra: Decodable & Sendable>: Decodable, Sendable {
    case extra(Extra)
    case request(Request)

    init(from decoder: any Decoder) throws {
        do { self = .extra(try Extra(from: decoder)) } catch { self = .request(try Request(from: decoder)) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func admitted(_ identifier: String?, by admitting: NSPredicate) -> Bool {
    identifier.map { identifier in admitting.evaluate(with: [identifierKey: identifier]) } == true
}

private func unmatched(_ subject: String, _ admission: NSPredicate, _ identifier: String?) -> ApplicationFailure {
    ApplicationFailure(subject: subject, reason: .unmatched(Unmatched(subject: admission.predicateFormat, query: identifier, candidates: [])))
}

private func installation(at url: URL, admitting: () -> NSPredicate) throws(ApplicationFailure) -> Installation {
    let (installation, admission) = (try Installation(at: url), admitting())
    guard admitted(installation.identifier, by: admission) else { throw unmatched(url.path(percentEncoded: false), admission, installation.identifier) }
    return installation
}

private func running(_ process: ProcessInstance, admitting: () -> NSPredicate) throws(ApplicationFailure) -> Instance {
    guard let application = NSRunningApplication(processIdentifier: process.pid) else { throw ApplicationFailure(subject: process.rawValue, reason: .noProcess) }
    let instance = try Instance(application)
    guard instance.process == process else { throw ApplicationFailure(subject: process.rawValue, reason: .noProcess) }
    let admission = admitting()
    guard admitted(application.bundleIdentifier, by: admission) else { throw unmatched(process.rawValue, admission, application.bundleIdentifier) }
    return instance
}

private func selected(_ identifiers: [String], admitting: () -> NSPredicate) -> @Sendable (NSRunningApplication) -> Bool {
    let admission = admitting()
    let held = Set(identifiers.filter { identifier in admitted(identifier, by: admission) })
    return { application in application.bundleIdentifier.map(held.contains) == true }
}

extension Request {
    func perform(admitting: @escaping @Sendable () -> NSPredicate, refusing: @Sendable (Installation) -> Set<UInt64>, definitions: ScriptingDefinitions) async throws -> Response {
        switch self {
            case .installations(let identifiers, let prefix):
                let selected =
                    (identifiers ?? []).map { identifier in NSPredicate(format: "%K == %@", identifierKey, identifier) }
                    + [prefix].compactMap(\.self).map { prefix in NSPredicate(format: "%K BEGINSWITH %@", identifierKey, prefix) }
                let predicate = selected.isEmpty ? admitting() : NSCompoundPredicate(andPredicateWithSubpredicates: [admitting(), NSCompoundPredicate(orPredicateWithSubpredicates: selected)])
                return await .stream(
                    Installation.search(predicate, in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)).map { listing -> any Encodable & Sendable in Outcome(listing) }
                )
            case .instances(let identifiers):
                return .stream(
                    NSWorkspace.shared.instanceStates(of: selected(identifiers, admitting: admitting))
                        .map { states -> any Encodable & Sendable in states.compactMap { instance, state in RunningInstance(instance, state: state) } }
                )
            case .events(let identifiers):
                return .stream(NSWorkspace.shared.instanceEvents(of: selected(identifiers, admitting: admitting)).map(\.self))
            case .pressure:
                return .stream(SystemPressure.changes().map { pressure -> any Encodable & Sendable in Outcome(pressure) })
            case .nodes(let process, let root, let depth):
                return try await .value(running(process, admitting: admitting).nodes(from: root, depth: depth))
            case .launch(let url, let documents, let arguments, let environment, let new):
                return try .stream(
                    NSWorkspace.shared.launching(installation(at: url, admitting: admitting), opening: documents, arguments: arguments, environment: environment, newInstance: new)
                        .map(\.self)
                )
            case .act(let process, let actions):
                let acted = try await running(process, admitting: admitting).act(actions)
                return .value(Acted(outcomes: acted.outcomes.map(Outcome.init), state: acted.state))
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
            case .capture(let process, let source, let longestSide):
                let instance = try running(process, admitting: admitting)
                let shots = try await instance.capture(source, to: Program.current.directory(.captures, of: instance.application.localizedName), preview: longestSide)
                return .value(Dictionary(uniqueKeysWithValues: shots.map { number, shot in (String(number), Outcome(shot)) }))
            case .diagnostics(let application, let folders, let since, let limit):
                let instance = try application?.instance.map { process throws(ApplicationFailure) in try running(process, admitting: admitting) }
                let bundle = try application.map { application throws(ApplicationFailure) in Bundle(url: try installation(at: application.url, admitting: admitting).url) } ?? Bundle.main
                let (owns, source): ((DiagnosticReport) -> Bool, Processes.LogEvent.Source) =
                    switch (instance?.process, bundle, bundle?.executableURL) {
                        case (.some(let process), _, _): ({ report in report.written(by: process) }, .process(process.pid))
                        case (.none, .some(let bundle), .some(let executable)): ({ report in report.written(by: bundle) }, .executable(executable))
                        case (.none, _, _): throw ApplicationFailure(subject: application?.url.path(percentEncoded: false) ?? Program.current.identifier, reason: .noPath)
                    }
                let earliest = [since.map { since in Date.now.addingTimeInterval(-since) }, instance?.application.launchDate].compactMap(\.self).max()
                let reports = DiagnosticReport.reports(since: earliest ?? .distantPast, limit: limit, where: owns)
                let logs = LogLine.newest(in: folders, since: earliest, limit: limit)
                let unified = await Result { () async throws(ProcessFailure) in try await Processes.LogEvent.history(of: source, since: earliest, limit: limit) }
                return .value(
                    Diagnosed(
                        reports: reports.reports,
                        logs: logs.lines,
                        unified: Outcome(unified),
                        unreadable: reports.failures.compactMap(\.errorDescription) + logs.failures.compactMap(\.errorDescription),
                    )
                )
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public func serve<Extra: Decodable & Sendable>(
    reading lines: @autoclosure @escaping @Sendable () -> any AsyncSequence<Data, any Error>,
    writing write: @escaping @Sendable (Data) throws -> Void,
    admitting: @escaping @Sendable () -> NSPredicate,
    refusing: @escaping @Sendable (Installation) -> Set<UInt64>,
    cached: @escaping @Sendable (Installation) -> URL?,
    extending: @escaping @Sendable (Extra) async throws -> Response = { (_: Never) -> Response in },
) async throws(JSONRPCFailure) {
    let logger = Logger(label: String(reflecting: Request.self))
    let definitions = ScriptingDefinitions(cached: cached)
    let calls = DispatchQueue(label: "\(Request.self).calls", attributes: .concurrent)
    let output = DispatchSerialQueue(label: "\(Request.self).output")
    try await JSONRPC.serve(
        reading: lines(),
        writing: { line in try await withTaskExecutorPreference(output) { try write(line) } },
        dropped: { failure in logger.debug("Dropped request", error: failure) },
        { (served: Served<Extra>) in
            try await withTaskExecutorPreference(calls) {
                switch served {
                    case .extra(let extra): try await extending(extra)
                    case .request(let request): try await request.perform(admitting: admitting, refusing: refusing, definitions: definitions)
                }
            }
        },
    )
}
