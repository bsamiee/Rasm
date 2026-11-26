public import AppKit
public import AppleEvents
public import Applications
import CurrentProcess
public import Files
public import JSONRPC
import Logging
public import Processes
import Results
public import ScreenCapture
import Telemetry

// --- [MODELS] --------------------------------------------------------------------------

public enum Request: Decodable, Sendable {
    public struct Application: Decodable, Sendable {
        public let url: URL
        public let instance: ProcessInstance?
    }

    case installations(identifiers: [String]?, prefix: String?)
    case instances(identifiers: [String]?)
    case events(identifiers: [String]?)
    case observations(identifiers: [String]?)
    case pressure
    case session
    case pasteboard(text: String?)
    case watch(instance: ProcessInstance)
    case model(instance: ProcessInstance, scope: Model.Scope)
    case intern(instance: ProcessInstance, locators: [Locator])
    case handlers(document: URL)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool, opening: [String: AppleEventValue], wait: Bool)
    case act(instance: ProcessInstance, actions: [ElementAction])
    case quit(instance: ProcessInstance, mode: Instance.QuitMode)
    case dictionary(url: URL)
    case execute(instance: ProcessInstance, arguments: [String: AppleEventValue])
    case capture(instance: ProcessInstance, source: Source, longestSide: LongestSide?)
    case record(instance: ProcessInstance, source: Source, seconds: Double, longestSide: LongestSide?)
    case scale(image: Data, longestSide: LongestSide)
    case diagnostics(application: Application?, folders: [LogFolder], since: Double?, limit: Int, find: String?)
}

private struct ActResult: Encodable, Sendable {
    let outcomes: [Exit<ElementAction.Delivery>]
    let state: InstanceState
    let written: Exit<[URL]>
    let diagnostics: Diagnostics
}

private struct ExecuteResult: Encodable, Sendable {
    let reply: AppleEventValue
    let written: Exit<[URL]>
}

private struct Scaled: Encodable, Sendable {
    let png: Data
    let width: Int
    let height: Int
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
    identifier.map { identifier in predicate.evaluate(with: [NSMetadataItemCFBundleIdentifierKey: identifier]) } == true
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

private func selection(of identifiers: [String]?, matching: @escaping @Sendable () -> NSPredicate) -> @Sendable (NSRunningApplication) -> Bool {
    let predicate = matching()
    let held = identifiers.map { identifiers in Set(identifiers.filter { identifier in matches(identifier, predicate) }) }
    return { application in application.bundleIdentifier.map { identifier in held?.contains(identifier) ?? matches(identifier, matching()) } == true }
}

extension Request {
    func perform(matching: @escaping @Sendable () -> NSPredicate, dictionaries: ScriptingDictionaries, watches: Watches) async throws -> Response {
        switch self {
            case .installations(let identifiers, let prefix):
                let subpredicates =
                    (identifiers ?? []).map { identifier in NSPredicate(format: "%K == %@", NSMetadataItemCFBundleIdentifierKey, identifier) }
                    + [prefix].compactMap(\.self).map { prefix in NSPredicate(format: "%K BEGINSWITH %@", NSMetadataItemCFBundleIdentifierKey, prefix) }
                let predicate = subpredicates.isEmpty ? matching() : NSCompoundPredicate(andPredicateWithSubpredicates: [matching(), NSCompoundPredicate(orPredicateWithSubpredicates: subpredicates)])
                return await .stream(
                    Installation.search(predicate, in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)).map(\.self)
                )
            case .instances(let identifiers):
                return .stream(
                    watches.states(of: selection(of: identifiers, matching: matching))
                        .map { states -> any Encodable & Sendable in states.compactMap { instance, state in InstanceSnapshot(instance, state: state) } }
                )
            case .events(let identifiers):
                return .stream(watches.events(of: selection(of: identifiers, matching: matching)).map(\.self))
            case .observations(let identifiers):
                return .stream(watches.observations(of: selection(of: identifiers, matching: matching)).map(\.self))
            case .pressure:
                return .stream(SystemPressure.changes().map { pressure -> any Encodable & Sendable in Exit(pressure) })
            case .session:
                return .stream(Session.changes().map(\.self))
            case .pasteboard(let text):
                return try await .value(Pasteboard(.general, writing: text))
            case .watch(let process):
                let instance = try instance(of: process, matching: matching)
                let watch = await watches.watch(instance)
                guard let snapshot = InstanceSnapshot(instance, state: await watch.current, language: watch.language) else { throw ApplicationFailure(subject: process.rawValue, reason: .noPath) }
                return .value(snapshot)
            case .model(let process, let scope):
                return try await .value(watches.watch(instance(of: process, matching: matching)).model(scope))
            case .intern(let process, let locators):
                return try await .value(watches.watch(instance(of: process, matching: matching)).intern(locators))
            case .handlers(let document):
                return try await .value(Installation.handlers(of: document, matching: matching()))
            case .launch(let url, let documents, let arguments, let environment, let new, let opening, let wait):
                return try .stream(
                    NSWorkspace.shared.launchEvents(
                        installation(at: url, matching: matching),
                        in: dictionaries,
                        watches: watches,
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
                let instance = try instance(of: process, matching: matching)
                try Session.current().require(process.rawValue)
                let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: "act on process \(process.pid)")
                defer { ProcessInfo.processInfo.endActivity(activity) }
                let watch = await watches.watch(instance)
                let began = await watch.recordFaults()
                let outcome = await Result { () async throws(ApplicationFailure) in
                    try await FileEvent.written(await watch.documentFiles) { () async throws(ApplicationFailure) in try await instance.act(actions, in: watch) }
                }
                let (faults, reports) = (await watch.recordedFaults(since: began), DiagnosticReport.reports(since: began) { report in report.isWritten(by: process) })
                let (acted, written) = try outcome.get()
                let diagnostics = Diagnostics(
                    reports: reports.reports,
                    logs: [],
                    unified: Exit(Result<[LogEntry], Never>.success(faults.entries)),
                    unreadable: (reports.failures + faults.failures).compactMap(\.errorDescription),
                )
                return .value(ActResult(outcomes: acted.outcomes.map(Exit.init), state: acted.state, written: Exit(written), diagnostics: diagnostics))
            case .quit(let process, let mode):
                let instance = try instance(of: process, matching: matching)
                let scripted = try [.saving, .discarding].contains(mode) ? instance.application.bundleURL.map { url throws(ApplicationFailure) in try Installation(at: url) } : nil
                let dictionary: ScriptingDictionary? = if let scripted, scripted.scriptable { try await dictionaries.dictionary(of: scripted) } else { nil }
                let states = await instance.quit(mode, watch: watches.watch(instance), in: dictionary)
                return try await .value(states.reduce(nil as InstanceState?) { _, outcome in try outcome.get() })
            case .dictionary(let url):
                return try await .value(dictionaries.dictionary(of: installation(at: url, matching: matching)))
            case .execute(let process, let arguments):
                let instance = try instance(of: process, matching: matching)
                guard let url = instance.application.bundleURL else { throw ApplicationFailure(subject: process.rawValue, reason: .noPath) }
                let dictionary = try await dictionaries.dictionary(of: Installation(at: url))
                let (reply, written) = try await FileEvent.written(await watches.watch(instance).documentFiles) { () async throws(AggregateError<AppleEventFailure>) in
                    try await dictionary.send(nil, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: process.pid))
                }
                return .value(ExecuteResult(reply: reply, written: Exit(written)))
            case .capture(let process, let source, let longestSide):
                let instance = try instance(of: process, matching: matching)
                try Session.current().require(process.rawValue)
                return try await .value(instance.capture(source, in: watches.watch(instance), longestSide: longestSide).map(Exit.init))
            case .record(let process, let source, let seconds, let longestSide):
                let instance = try instance(of: process, matching: matching)
                try Session.current().require(process.rawValue)
                return try await .value(instance.recording(source, in: watches.watch(instance), for: .seconds(seconds), longestSide: longestSide))
            case .scale(let image, let longestSide):
                let scaled = try image.image(longestSide: longestSide)
                return try .value(Scaled(png: scaled.png(), width: scaled.width, height: scaled.height))
            case .diagnostics(let application, let folders, let since, let limit, let find):
                let instance = try application?.instance.map { process throws(ApplicationFailure) in try instance(of: process, matching: matching) }
                let bundle = try application.map { application throws(ApplicationFailure) in Bundle(url: try installation(at: application.url, matching: matching).url) } ?? Bundle.main
                let (owns, source): ((DiagnosticReport) -> Bool, LogEntry.Source?) =
                    switch (instance?.process, bundle) {
                        case (.some(let process), _): ({ report in report.isWritten(by: process) }, .process(process.pid))
                        case (.none, .some(let bundle)): ({ report in report.isWritten(by: bundle) }, bundle.executableURL.map(LogEntry.Source.executable))
                        case (.none, .none): ({ _ in false }, nil)
                    }
                let earliest = [since.map { since in Date.now.addingTimeInterval(-since) }, instance?.application.launchDate].compactMap(\.self).max()
                let reports = DiagnosticReport.reports(since: earliest ?? .distantPast, limit: limit, where: owns)
                let logs = LogLine.newest(in: folders, since: earliest, limit: limit) { line in find.map { text in line.text.localizedCaseInsensitiveContains(text) } ?? true }
                let unified = await Result { () async throws(any Error) in
                    guard let source else { throw ApplicationFailure(subject: application?.url.path(percentEncoded: false) ?? Program.current.identifier, reason: .noExecutable) }
                    return try await LogEntry.history(of: source, since: earliest, limit: limit, containing: find)
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
    let watches = Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) })
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
                    case .request(let request): try await request.perform(matching: matching, dictionaries: dictionaries, watches: watches)
                }
            }
        },
    )
}
