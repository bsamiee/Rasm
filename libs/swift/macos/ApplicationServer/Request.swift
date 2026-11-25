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

let identifierKey = kMDItemCFBundleIdentifier as String

// --- [MODELS] --------------------------------------------------------------------------

public enum Request: Decodable, Sendable {
    public struct Application: Decodable, Sendable {
        public let url: URL
        public let instance: ProcessInstance?
    }

    case installations(identifiers: [String]?, prefix: String?)
    case instances(identifiers: [String])
    case events(identifiers: [String])
    case pressure
    case element(instance: ProcessInstance, root: Element.ID?, depth: Int)
    case handler(document: URL)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool, opening: [String: AppleEventValue], wait: Bool)
    case act(instance: ProcessInstance, actions: [ElementAction])
    case quit(instance: ProcessInstance, mode: Instance.QuitMode)
    case dictionary(url: URL)
    case execute(instance: ProcessInstance, arguments: [String: AppleEventValue])
    case capture(instance: ProcessInstance, source: Source?, longestSide: LongestSide?)
    case preview(file: URL, longestSide: LongestSide)
    case diagnostics(application: Application?, folders: [LogFolder], since: Double?, limit: Int)
}

private struct ActResult: Encodable, Sendable {
    let outcomes: [Exit<CGWindowID?>]
    let state: InstanceState
}

private struct Preview: Encodable, Sendable {
    let width: Int
    let height: Int
    let preview: Data
}

private struct Diagnostics: Encodable, Sendable {
    let reports: [DiagnosticReport]
    let logs: [LogLine]
    let unified: Exit<[LogEntry]>
    let unreadable: [String]
}

private enum Call<Extra: Decodable & Sendable>: Decodable, Sendable {
    case extra(Extra)
    case request(Request)

    init(from decoder: any Decoder) throws {
        do { self = .extra(try Extra(from: decoder)) } catch { self = .request(try Request(from: decoder)) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func matches(_ identifier: String?, _ predicate: NSPredicate) -> Bool {
    identifier.map { identifier in predicate.evaluate(with: [identifierKey: identifier]) } == true
}

private func unmatchedFailure(_ subject: String, _ predicate: NSPredicate, _ identifier: String?) -> ApplicationFailure {
    ApplicationFailure(subject: subject, reason: .unmatched(Unmatched(subject: predicate.predicateFormat, query: identifier, candidates: [])))
}

private func installation(at url: URL, matching: () -> NSPredicate) throws(ApplicationFailure) -> Installation {
    let (installation, predicate) = (try Installation(at: url), matching())
    guard matches(installation.identifier, predicate) else { throw unmatchedFailure(url.path(percentEncoded: false), predicate, installation.identifier) }
    return installation
}

private func instance(of process: ProcessInstance, matching: () -> NSPredicate) throws(ApplicationFailure) -> Instance {
    guard let application = NSRunningApplication(processIdentifier: process.pid) else { throw ApplicationFailure(subject: process.rawValue, reason: .noProcess) }
    let instance = try Instance(application)
    guard instance.process == process else { throw ApplicationFailure(subject: process.rawValue, reason: .noProcess) }
    let predicate = matching()
    guard matches(application.bundleIdentifier, predicate) else { throw unmatchedFailure(process.rawValue, predicate, application.bundleIdentifier) }
    return instance
}

private func selection(of identifiers: [String], matching: () -> NSPredicate) -> @Sendable (NSRunningApplication) -> Bool {
    let predicate = matching()
    let held = Set(identifiers.filter { identifier in matches(identifier, predicate) })
    return { application in application.bundleIdentifier.map(held.contains) == true }
}

extension Request {
    func perform(matching: @escaping @Sendable () -> NSPredicate, dictionaries: ScriptingDictionaries) async throws -> Response {
        switch self {
            case .installations(let identifiers, let prefix):
                let subpredicates =
                    (identifiers ?? []).map { identifier in NSPredicate(format: "%K == %@", identifierKey, identifier) }
                    + [prefix].compactMap(\.self).map { prefix in NSPredicate(format: "%K BEGINSWITH %@", identifierKey, prefix) }
                let predicate = subpredicates.isEmpty ? matching() : NSCompoundPredicate(andPredicateWithSubpredicates: [matching(), NSCompoundPredicate(orPredicateWithSubpredicates: subpredicates)])
                return await .stream(
                    Installation.search(predicate, in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)).map { listing -> any Encodable & Sendable in Exit(listing) }
                )
            case .instances(let identifiers):
                return .stream(
                    NSWorkspace.shared.instanceStates(of: selection(of: identifiers, matching: matching))
                        .map { states -> any Encodable & Sendable in states.compactMap { instance, state in InstanceSnapshot(instance, state: state) } }
                )
            case .events(let identifiers):
                return .stream(NSWorkspace.shared.instanceEvents(of: selection(of: identifiers, matching: matching)).map(\.self))
            case .pressure:
                return .stream(SystemPressure.changes().map { pressure -> any Encodable & Sendable in Exit(pressure) })
            case .element(let process, let root, let depth):
                return try await .value(instance(of: process, matching: matching).element(from: root, depth: depth))
            case .handler(let document):
                return try .value(installation(at: Installation(opening: document).url, matching: matching))
            case .launch(let url, let documents, let arguments, let environment, let new, let opening, let wait):
                return try .stream(
                    NSWorkspace.shared.launchEvents(
                        installation(at: url, matching: matching),
                        in: dictionaries,
                        opening: documents,
                        with: opening,
                        arguments: arguments,
                        environment: environment,
                        newInstance: new,
                        waiting: wait,
                    )
                    .map(\.self)
                )
            case .act(let process, let actions):
                let acted = try await instance(of: process, matching: matching).act(actions)
                return .value(ActResult(outcomes: acted.outcomes.map(Exit.init), state: acted.state))
            case .quit(let process, let mode):
                let instance = try instance(of: process, matching: matching)
                let scripted =
                    try [.saving, .discarding].contains(mode) ? instance.application.bundleURL.map { url throws(ApplicationFailure) in try installation(at: url, matching: matching) } : nil
                let dictionary: ScriptingDictionary? = if let scripted, scripted.scriptable { try await dictionaries.dictionary(of: scripted) } else { nil }
                var last: InstanceState?
                for await outcome in instance.quit(mode, in: dictionary) { last = try outcome.get() }
                return .value(last)
            case .dictionary(let url):
                return try await .value(dictionaries.dictionary(of: installation(at: url, matching: matching)))
            case .execute(let process, let arguments):
                guard let url = try instance(of: process, matching: matching).application.bundleURL else { throw ApplicationFailure(subject: process.rawValue, reason: .noPath) }
                return try await .value(
                    dictionaries.dictionary(of: installation(at: url, matching: matching)).send(nil, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: process.pid))
                )
            case .capture(let process, let source, let longestSide):
                let instance = try instance(of: process, matching: matching)
                let screenshots = try await instance.capture(source, to: Program.current.directory(.captures, of: instance.application.localizedName), preview: longestSide)
                return .value(Dictionary(uniqueKeysWithValues: screenshots.map { number, screenshot in (String(number), Exit(screenshot)) }))
            case .preview(let file, let longestSide):
                let data: Data
                do { data = try Data(contentsOf: file) } catch { throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .foundation(error as NSError)) }
                guard let image = CGImageSourceCreateWithData(data as CFData, nil).flatMap({ source in CGImageSourceCreateImageAtIndex(source, 0, nil) }) else {
                    throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .noImage)
                }
                return try .value(Preview(width: image.width, height: image.height, preview: image.preview(longestSide: longestSide, in: CIContext())))
            case .diagnostics(let application, let folders, let since, let limit):
                let instance = try application?.instance.map { process throws(ApplicationFailure) in try instance(of: process, matching: matching) }
                let bundle = try application.map { application throws(ApplicationFailure) in Bundle(url: try installation(at: application.url, matching: matching).url) } ?? Bundle.main
                let (owns, source): ((DiagnosticReport) -> Bool, LogEntry.Source?) =
                    switch (instance?.process, bundle, bundle?.executableURL) {
                        case (.some(let process), _, _): ({ report in report.isWritten(by: process) }, .process(process.pid))
                        case (.none, .some(let bundle), .some(let executable)): ({ report in report.isWritten(by: bundle) }, .executable(executable))
                        case (.none, .some(let bundle), .none): ({ report in report.isWritten(by: bundle) }, nil)
                        case (.none, .none, _): ({ _ in false }, nil)
                    }
                let earliest = [since.map { since in Date.now.addingTimeInterval(-since) }, instance?.application.launchDate].compactMap(\.self).max()
                let reports = DiagnosticReport.reports(since: earliest ?? .distantPast, limit: limit, where: owns)
                let logs = LogLine.newest(in: folders, since: earliest, limit: limit)
                let unified = await Result { () async throws(any Error) in
                    guard let source else { throw ApplicationFailure(subject: application?.url.path(percentEncoded: false) ?? Program.current.identifier, reason: .noPath) }
                    return try await LogEntry.history(of: source, since: earliest, limit: limit)
                }
                return .value(
                    Diagnostics(
                        reports: reports.reports,
                        logs: logs.lines,
                        unified: Exit(unified),
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
    matching: @escaping @Sendable () -> NSPredicate,
    cachedDefinition: @escaping @Sendable (Installation) -> URL?,
    extra: @escaping @Sendable (Extra) async throws -> Response = { (_: Never) -> Response in },
) async throws(JSONRPCFailure) {
    let logger = Logger(label: String(reflecting: Request.self))
    let dictionaries = ScriptingDictionaries(cachedDefinition: cachedDefinition)
    let calls = DispatchQueue(label: "\(Request.self).calls", attributes: .concurrent)
    let output = DispatchSerialQueue(label: "\(Request.self).output")
    try await JSONRPC.serve(
        reading: lines(),
        writing: { line in try await withTaskExecutorPreference(output) { try write(line) } },
        onDrop: { failure in logger.debug("Dropped request", error: failure) },
        { (call: Call<Extra>) in
            try await withTaskExecutorPreference(calls) {
                switch call {
                    case .extra(let request): try await extra(request)
                    case .request(let request): try await request.perform(matching: matching, dictionaries: dictionaries)
                }
            }
        },
    )
}
