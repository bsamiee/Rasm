import AppKit
import AppleEvents
import Applications
import AsyncAlgorithms
import Encoding
import Files
import JSONRPC
import Processes
import Results
import System

// --- [MODELS] --------------------------------------------------------------------------

enum Request: Decodable, Sendable {
    struct Application: Decodable, Sendable {
        let url: URL
        let instance: ProcessInstance?
    }

    struct Record: Decodable, Sendable {
        let source: Source
        let longestSide: LongestSide?
        let keyFrameLimit: KeyFrameLimit
    }

    enum Mode: Decodable, Sendable {
        case still(marks: [Mark])
        case recording(keyFrameLimit: KeyFrameLimit, duration: Duration, movie: URL?)

        private enum CodingKeys: CodingKey {
            case still, recording
        }

        private enum Field: String, CodingKey {
            case tag = "_tag"
            case marks, keyFrameLimit, duration, movie
        }

        init(from decoder: any Decoder) throws {
            let fields = try decoder.container(keyedBy: Field.self)
            let tag: String = try fields.decode(String.self, forKey: .tag)
            self =
                switch CodingKeys(stringValue: tag) {
                    case .still: try .still(marks: fields.decode([Mark].self, forKey: .marks))
                    case .recording:
                        try .recording(
                            keyFrameLimit: fields.decode(KeyFrameLimit.self, forKey: .keyFrameLimit),
                            duration: .milliseconds(fields.decode(Double.self, forKey: .duration)),
                            movie: fields.decodeIfPresent(URL.self, forKey: .movie),
                        )
                    case nil: throw DecodingError.dataCorruptedError(forKey: .tag, in: fields, debugDescription: "\(tag) names no \(Self.self)")
                }
        }
    }

    case subscribe
    case installations(name: String)
    case installation(url: URL)
    case pasteboard(content: Bool)
    case watch(instance: ProcessInstance, locators: [Locator]?)
    case release(instance: ProcessInstance)
    case model(instance: ProcessInstance, scope: Model.Scope, known: Model.Known?)
    case intern(instance: ProcessInstance, locators: [Locator])
    case installationsOpening(document: URL)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool, opening: [String: AppleEventValue])
    case act(instance: ProcessInstance, steps: [Step], record: Record?)
    case activate(instance: ProcessInstance)
    case quit(instance: ProcessInstance, mode: Watch.QuitMode)
    case dictionary(url: URL)
    case execute(instance: ProcessInstance, arguments: [String: AppleEventValue])
    case capture(instance: ProcessInstance, source: Source, longestSide: LongestSide?, mode: Mode)
    case scale(image: Data, longestSide: LongestSide)
    case diagnostics(application: Application?, folders: [LogFolder], since: Double?, limit: Int, find: String?)
}

private enum Pushed: CaseTagged, Sendable {
    case installations(Installation.Listing)
    case instances(read: EncodedResult<[InstanceSnapshot]>)
    case changed(read: EncodedResult<InstanceEvent>)
    case observed(Watch.Report)
    case session(read: EncodedResult<Session>)
    case pressure(read: EncodedResult<SystemPressure>)
}

private struct ActResult: Encodable, Sendable {
    struct Diagnosis: Encodable, Sendable {
        let usage: EncodedResult<ResourceUsage.Interval>
        let load: EncodedResult<SystemLoad.Interval>
    }

    let outcomes: [EncodedResult<Step.Delivery>]
    let state: InstanceState
    let diagnosis: Diagnosis?
    let written: EncodedResult<[URL]>
    let pasteboard: EncodedResult<Applications.Pasteboard>?
    let frames: EncodedResult<[KeyFrame]>?
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
    private enum CodingKeys: CodingKey {
        case reports, samples, logs, unified, unreadable
    }

    let reports: [DiagnosticReport]
    let samples: [URL]
    let logs: [LogLine]
    let unified: EncodedResult<[LogEntry]>
    let unreadable: [any ModuleError]

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(reports, forKey: .reports)
        try fields.encode(samples, forKey: .samples)
        try fields.encode(logs, forKey: .logs)
        try fields.encode(unified, forKey: .unified)
        var failures = fields.nestedUnkeyedContainer(forKey: .unreadable)
        for failure in unreadable { try failures.encode(failure) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Request {
    func perform(matching: @escaping @Sendable () -> NSPredicate, dictionaries: ScriptingDictionaries, watches: Watches) async throws -> Response {
        let predicate = matching()
        switch self {
            case .subscribe:
                let claimed: @Sendable (NSRunningApplication) -> Bool = { application in
                    matching().evaluate(bundleIdentifier: application.bundleIdentifier, displayName: application.localizedName, bundle: application.bundleURL)
                }
                let listings = await Installation.search(predicate, in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)).map(Pushed.installations)
                let instances = watches.states(of: claimed).map { states in
                    Pushed.instances(read: EncodedResult(states.map { states in states.compactMap { instance, state in InstanceSnapshot(instance, state: state) } }))
                }
                let changes = watches.events(of: claimed).compactMap { (event: Result<InstanceEvent, ApplicationFailure>) -> Pushed? in
                    switch event.map(\.isPushed) {
                        case .success(false): nil
                        case .success(true), .failure: .changed(read: EncodedResult(event))
                    }
                }
                let reads = watches.observations(of: claimed).map(Pushed.observed)
                let sessions = Session.changes(from: watches).map { session in Pushed.session(read: EncodedResult(session)) }
                let pressures = SystemPressure.changes().map { pressure in Pushed.pressure(read: EncodedResult(pressure)) }
                return .stream(merge(merge(listings, instances, changes), merge(reads, sessions, pressures)).map(\.self))
            case .installations(let name):
                let named = NSPredicate(format: "%K CONTAINS[cd] %@ OR %K CONTAINS[cd] %@", NSMetadataItemDisplayNameKey, name, NSMetadataItemFSNameKey, name)
                let found = await Installation.search(NSCompoundPredicate(andPredicateWithSubpredicates: [predicate, named]), in: []).first { _ in true }
                return .value(found)
            case .installation(let url):
                return try .value(Installation(at: url, matching: predicate))
            case .pasteboard(let content):
                return try await .value(Pasteboard(.general, reading: content))
            case .watch(let process, let locators):
                let instance = try await watches.instance(process, matching: matching)
                let installation = try instance.application.bundleURL.map { url throws(ApplicationFailure) in try Installation(at: url) }
                let watch = await watches.watch(instance, locators: locators ?? [])
                if try installation?.scriptable == true {
                    Task { await NSAppleEventDescriptor(processIdentifier: process.pid).consent(to: AEEventClass(kAECoreSuite), AEEventID(kAEGetData), asking: true) }
                }
                let usage = Result { () throws(Errno) in try ResourceUsage(of: process) }
                guard let snapshot = InstanceSnapshot(instance, state: await watch.current, language: await watch.language, usage: usage) else {
                    throw ApplicationFailure(subject: "The instance", reason: installation == nil ? .noPath : .noIdentifier)
                }
                return .value(snapshot)
            case .release(let process):
                let table = await watches.locators(of: process)
                await watches.release(process)
                return .value(table)
            case .model(let process, let scope, let known):
                return try await .value(watches.watch(watches.instance(process, matching: matching)).model(scope, known: known))
            case .intern(let process, let locators):
                return try await .value(watches.watch(watches.instance(process, matching: matching)).intern(locators))
            case .installationsOpening(let document):
                return try await .value(Installation.opening(document, matching: predicate))
            case .launch(let url, let documents, let arguments, let environment, let new, let opening):
                return try .stream(
                    NSWorkspace.shared.launchEvents(
                        Installation(at: url, matching: predicate),
                        in: dictionaries,
                        watches: watches,
                        opening: documents,
                        with: opening,
                        arguments: arguments,
                        environment: environment,
                        newInstance: new,
                    )
                    .map(\.self)
                )
            case .act(let process, let steps, let record):
                let instance = try await watches.instance(process, matching: matching)
                try Session.require("Acting")
                let watch = await watches.watch(instance)
                let changeCount: Int = NSPasteboard.general.changeCount
                let (hold, written) = try await FileEvent.written(await watch.documentFiles) { () async throws(ApplicationFailure) in
                    try await PermittedFrontList.shared.holding(
                        excluding: { application in application.processIdentifier == process.pid },
                        { () async in
                            guard let record else {
                                return (await Result { () async throws(ApplicationFailure) in try await watch.act(steps) }, Result<[KeyFrame], ApplicationFailure>?.none)
                            }
                            return await watch.record(record.source, keyFrames: record.keyFrameLimit, longestSide: record.longestSide) {
                                (began: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) in try await watch.act(steps, stepping: began)
                            } as (Result<_, ApplicationFailure>, Result<[KeyFrame], ApplicationFailure>?)
                        },
                    )
                }
                try hold.released.get()
                let (outcome, frames) = hold.value.get()
                let acted = try outcome.get()
                let copied: Result<Applications.Pasteboard, ApplicationFailure>?
                do throws(ApplicationFailure) {
                    let read: Applications.Pasteboard? = try await Applications.Pasteboard(.general, since: changeCount, writer: process.pid)
                    copied = read.map { read in .success(read) }
                } catch {
                    copied = .failure(error)
                }
                return .value(
                    ActResult(
                        outcomes: acted.outcomes.map(EncodedResult.init),
                        state: acted.state,
                        diagnosis: acted.diagnosis.map { diagnosis in ActResult.Diagnosis(usage: EncodedResult(diagnosis.usage), load: EncodedResult(diagnosis.load)) },
                        written: EncodedResult(written),
                        pasteboard: copied.map(EncodedResult.init),
                        frames: frames.map(EncodedResult.init),
                    )
                )
            case .activate(let process):
                return try await .value(watches.watch(watches.instance(process, matching: matching)).forward())
            case .quit(let process, let mode):
                let instance = try await watches.instance(process, matching: matching)
                let installation = try [.saving, .discarding].contains(mode) ? instance.application.bundleURL.map { url throws(ApplicationFailure) in try Installation(at: url) } : nil
                let dictionary: ScriptingDictionary? = if let installation, try installation.scriptable { try await dictionaries.dictionary(of: installation) } else { nil }
                let watch = await watches.watch(instance)
                let hold = try await PermittedFrontList.shared.holding(
                    excluding: { application in application.processIdentifier == process.pid },
                    { () async throws(ApplicationFailure) in try await watch.quit(mode, in: dictionary) },
                )
                try hold.released.get()
                return try .value(hold.value.get())
            case .dictionary(let url):
                return try await .value(dictionaries.dictionary(of: Installation(at: url, matching: predicate)))
            case .execute(let process, let arguments):
                let instance = try await watches.instance(process, matching: matching)
                guard let url = instance.application.bundleURL else { throw ApplicationFailure(subject: "The instance", reason: .noPath) }
                let dictionary = try await dictionaries.dictionary(of: Installation(at: url))
                let (hold, written) = try await FileEvent.written(await watches.watch(instance).documentFiles) { () async throws(ApplicationFailure) in
                    try await PermittedFrontList.shared.holding(
                        excluding: { application in application.processIdentifier == process.pid },
                        { () async throws(AggregateError<AppleEventFailure>) in try await dictionary.send(nil, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: process.pid)) },
                    )
                }
                try hold.released.get()
                return try .value(ExecuteResult(reply: hold.value.get(), written: EncodedResult(written)))
            case .capture(let process, let source, let longestSide, .still(let marks)):
                let instance = try await watches.instance(process, matching: matching)
                try Session.require("Capture")
                return try await .value(watches.watch(instance).capture(source, longestSide: longestSide, marks: marks).map(EncodedResult.init))
            case .capture(let process, let source, let longestSide, .recording(let keyFrameLimit, let duration, let movie)):
                let instance = try await watches.instance(process, matching: matching)
                try Session.require("Recording")
                let recorded = await watches.watch(instance).record(source, keyFrames: keyFrameLimit, longestSide: longestSide, movie: movie) {
                    (_: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) in
                    do { try await Task.sleep(for: duration) } catch { throw ApplicationFailure(subject: "the recording", reason: .cancelled) }
                }
                try recorded.value.get()
                return try .value(recorded.frames.get().map { frame in EncodedResult(Result<KeyFrame, ApplicationFailure>.success(frame)) })
            case .scale(let image, let longestSide):
                let scaled = try image.scale(longestSide: longestSide)
                return try .value(Scaled(png: scaled.png(), width: scaled.width, height: scaled.height))
            case .diagnostics(let application, let folders, let since, let limit, let find):
                let instance: Instance? = if let process = application?.instance { try await watches.instance(process, matching: matching) } else { nil }
                let bundle: Bundle? =
                    try instance == nil
                    ? application.map { application throws(ApplicationFailure) in Bundle(url: try Installation(at: application.url, matching: predicate).url) } ?? Bundle.main : nil
                let (owns, source): ((DiagnosticReport) -> Bool, LogEntry.Source?) =
                    switch (instance?.process, bundle) {
                        case (.some(let process), _): ({ report in report.isWritten(by: process) }, .process(process.pid))
                        case (.none, .some(let bundle)): ({ report in report.isWritten(by: bundle) }, bundle.executableURL.map(LogEntry.Source.executable))
                        case (.none, .none): ({ _ in false }, nil)
                    }
                let earliest = [since.map { since in Date.now.addingTimeInterval(-since) }, instance?.application.launchDate].compactMap(\.self).max()
                let reports = DiagnosticReport.reports(since: earliest ?? .distantPast, limit: limit, where: owns)
                let (samples, unsampled): ([URL], [ProcessFailure]) =
                    switch Result(catching: { () throws(ProcessFailure) -> [URL] in try instance?.process.samples(since: earliest ?? .distantPast) ?? [] }) {
                        case .success(let found): (Array(found.prefix(limit)), [])
                        case .failure(let failure): ([], [failure])
                    }
                let logs = LogLine.newest(in: folders, since: earliest, limit: limit) { line in find.map { text in line.text.localizedCaseInsensitiveContains(text) } ?? true }
                let unified = await Result { () async throws(any Error) -> [LogEntry] in
                    guard let source else { throw ApplicationFailure(subject: (application?.url ?? Bundle.main.bundleURL).path(percentEncoded: false), reason: .noExecutable) }
                    return try await LogEntry.history(of: source, since: earliest, limit: limit, containing: find)
                }
                let unreadable: [[any ModuleError]] = [reports.failures, logs.failures, unsampled]
                return .value(Diagnostics(reports: reports.reports, samples: samples, logs: logs.lines, unified: EncodedResult(unified), unreadable: unreadable.flatMap(\.self)))
        }
    }
}
