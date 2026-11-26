import AppKit
import AppleEvents
import Applications
import CurrentProcess
import Files
import JSONRPC
import Processes
import Results
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

let replyDeadline: Duration = .seconds(120)

// --- [MODELS] --------------------------------------------------------------------------

enum Request: Decodable, Sendable {
    struct Application: Decodable, Sendable {
        let url: URL
        let instance: ProcessInstance?
    }

    struct Record: Decodable, Sendable {
        let source: Source
        let longestSide: LongestSide?
        let frames: Int
    }

    case installations(identifiers: [String]?, prefix: String?)
    case installation(url: URL)
    case instances(identifiers: [String]?)
    case events(identifiers: [String]?)
    case observations(identifiers: [String]?)
    case pressure
    case session
    case pasteboard(text: String?)
    case watch(instance: ProcessInstance)
    case model(instance: ProcessInstance, scope: Model.Scope, known: Model.Known?)
    case intern(instance: ProcessInstance, locators: [Locator])
    case handlers(document: URL)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool, opening: [String: AppleEventValue], wait: Bool)
    case act(instance: ProcessInstance, steps: [Step], record: Record?)
    case quit(instance: ProcessInstance, mode: Watch.QuitMode)
    case dictionary(url: URL)
    case execute(instance: ProcessInstance, arguments: [String: AppleEventValue])
    case capture(instance: ProcessInstance, source: Source, longestSide: LongestSide?, marks: [Mark])
    case record(instance: ProcessInstance, source: Source, longestSide: LongestSide?, frames: Int, seconds: Double, movie: URL?)
    case scale(image: Data, longestSide: LongestSide)
    case diagnostics(application: Application?, folders: [LogFolder], since: Double?, limit: Int, find: String?)
}

private struct ActResult: Encodable, Sendable {
    let outcomes: [EncodedResult<Step.Delivery>]
    let state: InstanceState
    let usage: EncodedResult<ResourceUsage.Interval>
    let load: EncodedResult<SystemLoad.Interval>
    let written: EncodedResult<[URL]>
    let diagnostics: Diagnostics
    let recording: EncodedResult<Recording>?
}

private struct ExecuteResult: Encodable, Sendable {
    let reply: AppleEventValue
    let written: EncodedResult<[URL]>
}

private struct Scaled: Encodable, Sendable {
    let png: Data
    let width: Int
    let height: Int
}

private struct Diagnostics: Encodable, Sendable {
    let reports: [DiagnosticReport]
    let logs: [LogLine]
    let unified: EncodedResult<[LogEntry]>
    let unreadable: [String]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSPredicate {
    fileprivate func admit(_ identifier: String?, as subject: String) throws(ApplicationFailure) {
        guard evaluate(bundleIdentifier: identifier) else {
            throw ApplicationFailure(subject: subject, reason: .unmatched(Unmatched(subject: predicateFormat, query: identifier, candidates: [])))
        }
    }
}

extension Installation {
    fileprivate init(at url: URL, admittedBy predicate: NSPredicate) throws(ApplicationFailure) {
        try self.init(at: url)
        try predicate.admit(identifier, as: url.path(percentEncoded: false))
    }
}

extension Instance {
    fileprivate init(_ process: ProcessInstance, admittedBy predicate: NSPredicate) throws(ApplicationFailure) {
        let running = try NSRunningApplication(processIdentifier: process.pid).map { application throws(ApplicationFailure) in try Self(application) }
        guard let running, running.process == process else { throw ApplicationFailure(subject: process.rawValue, reason: .system(.noSuchProcess)) }
        try predicate.admit(running.application.bundleIdentifier, as: process.rawValue)
        self = running
    }
}

private func selection(of identifiers: [String]?, matching: @escaping @Sendable () -> NSPredicate) -> @Sendable (NSRunningApplication) -> Bool {
    let predicate = matching()
    let held = identifiers.map { identifiers in Set(identifiers.filter { identifier in predicate.evaluate(bundleIdentifier: identifier) }) }
    return { application in held.map { held in application.bundleIdentifier.map(held.contains) == true } ?? matching().evaluate(bundleIdentifier: application.bundleIdentifier) }
}

extension Request {
    func perform(matching: @escaping @Sendable () -> NSPredicate, dictionaries: ScriptingDictionaries, watches: Watches) async throws -> Response {
        let predicate = matching()
        switch self {
            case .installations(let identifiers, let prefix):
                let subpredicates =
                    (identifiers ?? []).map { identifier in NSPredicate(format: "%K == %@", NSMetadataItemCFBundleIdentifierKey, identifier) }
                    + [prefix.map { prefix in NSPredicate(format: "%K BEGINSWITH %@", NSMetadataItemCFBundleIdentifierKey, prefix) }].compactMap(\.self)
                let wanted = subpredicates.isEmpty ? predicate : NSCompoundPredicate(andPredicateWithSubpredicates: [predicate, NSCompoundPredicate(orPredicateWithSubpredicates: subpredicates)])
                return await .stream(Installation.search(wanted, in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)).map(\.self))
            case .installation(let url):
                return try .value(Installation(at: url, admittedBy: predicate))
            case .instances(let identifiers):
                return .stream(
                    watches.states(of: selection(of: identifiers, matching: matching))
                        .map { states -> any Encodable & Sendable in EncodedResult(states.map { states in states.compactMap { instance, state in InstanceSnapshot(instance, state: state) } }) }
                )
            case .events(let identifiers):
                return .stream(watches.events(of: selection(of: identifiers, matching: matching)).map { event -> any Encodable & Sendable in EncodedResult(event) })
            case .observations(let identifiers):
                return .stream(watches.observations(of: selection(of: identifiers, matching: matching)).map(\.self))
            case .pressure:
                return .stream(SystemPressure.changes().map { pressure -> any Encodable & Sendable in EncodedResult(pressure) })
            case .session:
                return .stream(Session.changes().map { session -> any Encodable & Sendable in EncodedResult(session) })
            case .pasteboard(let text):
                if let text { try Pasteboard.write(text, to: .general) }
                return try await .value(Pasteboard(.general))
            case .watch(let process):
                let watch = try await watches.watch(Instance(process, admittedBy: predicate))
                let scriptable: Bool?
                do { scriptable = try watch.instance.application.bundleURL?.resourceValues(forKeys: [.applicationIsScriptableKey]).applicationIsScriptable } catch {
                    throw ApplicationFailure(subject: process.rawValue, reason: .foundation(error as NSError))
                }
                if scriptable == true {
                    Task { await NSAppleEventDescriptor(processIdentifier: process.pid).consent(to: AEEventClass(kAECoreSuite), AEEventID(kAEGetData), asking: true) }
                }
                guard let snapshot = InstanceSnapshot(watch.instance, state: await watch.current, language: watch.language) else {
                    throw ApplicationFailure(subject: process.rawValue, reason: watch.instance.application.bundleURL == nil ? .noPath : .noIdentifier)
                }
                return .value(snapshot)
            case .model(let process, let scope, let known):
                return try await .value(watches.watch(Instance(process, admittedBy: predicate)).model(scope, known: known))
            case .intern(let process, let locators):
                return try await .value(watches.watch(Instance(process, admittedBy: predicate)).intern(locators))
            case .handlers(let document):
                return try await .value(Installation.handlers(of: document, matching: predicate))
            case .launch(let url, let documents, let arguments, let environment, let new, let opening, let wait):
                return try .stream(
                    NSWorkspace.shared.launchEvents(
                        Installation(at: url, admittedBy: predicate),
                        in: dictionaries,
                        watches: watches,
                        within: replyDeadline,
                        opening: documents,
                        with: opening,
                        arguments: arguments,
                        environment: environment,
                        newInstance: new,
                        waiting: wait,
                    )
                    .map(\.self)
                )
            case .act(let process, let steps, let record):
                let instance = try Instance(process, admittedBy: predicate)
                try Session.current().require(process.rawValue)
                let watch = await watches.watch(instance)
                let began = await watch.recordFaults()
                let loaded = Result { () throws(ProcessFailure) in try SystemLoad() }
                let acting = { (stepping: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) in
                    try await FileEvent.written(await watch.documentFiles) { () async throws(ApplicationFailure) in
                        try await PermittedFrontList.shared.holding(excluding: { application in application.processIdentifier == process.pid }, for: replyDeadline) {
                            () async throws(ApplicationFailure) in try await watch.act(steps, stepping: stepping)
                        }.get()
                    }
                }
                let (outcome, recording): (Result<_, ApplicationFailure>, Result<Recording, ApplicationFailure>?) =
                    if let record {
                        await watch.record(record.source, keyFrames: record.frames, longestSide: record.longestSide, while: acting)
                    } else {
                        (await Result { () async throws(ApplicationFailure) in try await acting { _ in } }, nil)
                    }
                let load = loaded.flatMap { earlier in Result { () throws(ProcessFailure) in try SystemLoad.Interval(since: earlier) } }
                let (faults, reports) = (await watch.recordedFaults(since: began), DiagnosticReport.reports(since: began) { report in report.isWritten(by: process) })
                let (acted, written) = try outcome.get()
                let diagnostics = Diagnostics(
                    reports: reports.reports,
                    logs: [],
                    unified: EncodedResult(Result<[LogEntry], Never>.success(faults.entries)),
                    unreadable: (reports.failures + faults.failures).compactMap(\.errorDescription),
                )
                return .value(
                    ActResult(
                        outcomes: acted.outcomes.map(EncodedResult.init),
                        state: acted.state,
                        usage: EncodedResult(acted.usage),
                        load: EncodedResult(load),
                        written: EncodedResult(written),
                        diagnostics: diagnostics,
                        recording: recording.map(EncodedResult.init),
                    )
                )
            case .quit(let process, let mode):
                let instance = try Instance(process, admittedBy: predicate)
                let scripted = try [.saving, .discarding].contains(mode) ? instance.application.bundleURL.map { url throws(ApplicationFailure) in try Installation(at: url) } : nil
                let dictionary: ScriptingDictionary? = if let scripted, try scripted.scriptable { try await dictionaries.dictionary(of: scripted) } else { nil }
                let watch = await watches.watch(instance)
                return try await .value(
                    PermittedFrontList.shared.holding(excluding: { application in application.processIdentifier == process.pid }, for: replyDeadline) { () async throws(ApplicationFailure) in
                        try await watch.quit(mode, within: replyDeadline, in: dictionary)
                    }.get()
                )
            case .dictionary(let url):
                return try await .value(dictionaries.dictionary(of: Installation(at: url, admittedBy: predicate)))
            case .execute(let process, let arguments):
                let instance = try Instance(process, admittedBy: predicate)
                guard let url = instance.application.bundleURL else { throw ApplicationFailure(subject: process.rawValue, reason: .noPath) }
                let dictionary = try await dictionaries.dictionary(of: Installation(at: url))
                let documents = await watches.watch(instance).documentFiles
                let (reply, written) = try await PermittedFrontList.shared.holding(excluding: { application in application.processIdentifier == process.pid }, for: replyDeadline) {
                    () async throws(AggregateError<AppleEventFailure>) in
                    try await FileEvent.written(documents) { () async throws(AggregateError<AppleEventFailure>) in
                        try await dictionary.send(nil, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: process.pid), within: replyDeadline)
                    }
                }
                .get()
                return .value(ExecuteResult(reply: reply, written: EncodedResult(written)))
            case .capture(let process, let source, let longestSide, let marks):
                let instance = try Instance(process, admittedBy: predicate)
                try Session.current().require(process.rawValue)
                return try await .value(watches.watch(instance).capture(source, longestSide: longestSide, marks: marks).map(EncodedResult.init))
            case .record(let process, let source, let longestSide, let frames, let seconds, let movie):
                let instance = try Instance(process, admittedBy: predicate)
                try Session.current().require(process.rawValue)
                let recorded = await watches.watch(instance).record(source, keyFrames: frames, longestSide: longestSide, movie: movie) {
                    (_: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) in
                    do { try await Task.sleep(for: .seconds(seconds)) } catch { throw ApplicationFailure(subject: "recording of \(source) in process \(process.pid)", reason: .cancelled) }
                }
                try recorded.value.get()
                return try .value(recorded.recording.get())
            case .scale(let image, let longestSide):
                let scaled = try image.image(longestSide: longestSide)
                return try .value(Scaled(png: scaled.png(), width: scaled.width, height: scaled.height))
            case .diagnostics(let application, let folders, let since, let limit, let find):
                let instance = try application?.instance.map { process throws(ApplicationFailure) in try Instance(process, admittedBy: predicate) }
                let bundle: Bundle? =
                    try instance == nil
                    ? application.map { application throws(ApplicationFailure) in Bundle(url: try Installation(at: application.url, admittedBy: predicate).url) } ?? Bundle.main : nil
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
                        unified: EncodedResult(unified),
                        unreadable: reports.failures.compactMap(\.errorDescription) + logs.failures.compactMap(\.errorDescription),
                    )
                )
        }
    }
}
