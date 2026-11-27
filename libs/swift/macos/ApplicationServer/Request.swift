import AppKit
import AppleEvents
import Applications
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
        let frames: KeyFrameLimit
    }

    case installations(identifiers: [String]?, prefix: String?, name: String?)
    case installation(url: URL)
    case instances(identifiers: [String]?)
    case events(identifiers: [String]?)
    case observations(identifiers: [String]?)
    case pressure
    case session
    case pasteboard(text: String?, content: Bool)
    case watch(instance: ProcessInstance, locators: [Locator]?)
    case locators(instance: ProcessInstance)
    case release(instance: ProcessInstance)
    case model(instance: ProcessInstance, scope: Model.Scope, known: Model.Known?)
    case intern(instance: ProcessInstance, locators: [Locator])
    case handlers(document: URL)
    case launch(url: URL, documents: [URL], arguments: [String], environment: [String: String], new: Bool, opening: [String: AppleEventValue], wait: Bool)
    case act(instance: ProcessInstance, steps: [Step], record: Record?)
    case activate(instance: ProcessInstance)
    case quit(instance: ProcessInstance, mode: Watch.QuitMode)
    case dictionary(url: URL)
    case execute(instance: ProcessInstance, arguments: [String: AppleEventValue])
    case capture(instance: ProcessInstance, source: Source, longestSide: LongestSide?, marks: [Mark])
    case record(instance: ProcessInstance, source: Source, longestSide: LongestSide?, frames: KeyFrameLimit, seconds: Double, movie: URL?)
    case scale(image: Data, longestSide: LongestSide)
    case diagnostics(application: Application?, folders: [LogFolder], since: Double?, limit: Int, find: String?)
}

private struct ActResult: Encodable, Sendable {
    let outcomes: [EncodedResult<Step.Delivery>]
    let state: InstanceState
    let usage: EncodedResult<ResourceUsage.Interval>?
    let load: EncodedResult<SystemLoad.Interval>?
    let written: EncodedResult<[URL]>
    let pasteboard: EncodedResult<Applications.Pasteboard>?
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
    let samples: [URL]
    let logs: [LogLine]
    let unified: EncodedResult<[LogEntry]>
    let unreadable: [String]
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func selection(of identifiers: [String]?, matching: @escaping @Sendable () -> NSPredicate) -> @Sendable (NSRunningApplication) -> Bool {
    let predicate = matching()
    let held = identifiers.map { identifiers in Set(identifiers.filter { identifier in predicate.evaluate(bundleIdentifier: identifier) }) }
    return { application in
        held.map { held in application.bundleIdentifier.map(held.contains) == true }
            ?? matching().evaluate(bundleIdentifier: application.bundleIdentifier, displayName: application.localizedName, bundle: application.bundleURL)
    }
}

extension Request {
    func perform(matching: @escaping @Sendable () -> NSPredicate, dictionaries: ScriptingDictionaries, watches: Watches) async throws -> Response {
        let predicate = matching()
        switch self {
            case .installations(let identifiers, let prefix, let name):
                let bundles: [NSPredicate] =
                    (identifiers ?? []).map { identifier in NSPredicate(format: "%K == %@", NSMetadataItemCFBundleIdentifierKey, identifier) }
                    + [prefix.map { prefix in NSPredicate(format: "%K BEGINSWITH %@", NSMetadataItemCFBundleIdentifierKey, prefix) }].compactMap(\.self)
                let named: NSPredicate? = name.map { name in NSPredicate(format: "%K CONTAINS[cd] %@ OR %K CONTAINS[cd] %@", NSMetadataItemDisplayNameKey, name, NSMetadataItemFSNameKey, name) }
                let subpredicates: [NSPredicate?] = [predicate, bundles.isEmpty ? nil : NSCompoundPredicate(orPredicateWithSubpredicates: bundles), named]
                let wanted = NSCompoundPredicate(andPredicateWithSubpredicates: subpredicates.compactMap(\.self))
                let folders: [URL] = name == nil ? FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask) : []
                return await .stream(Installation.search(wanted, in: folders).map(\.self))
            case .installation(let url):
                return try .value(Installation(at: url, matching: predicate))
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
                return .stream(Session.changes(from: watches).map { session -> any Encodable & Sendable in EncodedResult(session) })
            case .pasteboard(let text, let content):
                if let text { try Pasteboard.write(text, to: .general) }
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
            case .locators(let process):
                return await .value(watches.locators(of: process))
            case .release(let process):
                await watches.release(process)
                return .value(nil)
            case .model(let process, let scope, let known):
                return try await .value(watches.watch(watches.instance(process, matching: matching)).model(scope, known: known))
            case .intern(let process, let locators):
                return try await .value(watches.watch(watches.instance(process, matching: matching)).intern(locators))
            case .handlers(let document):
                return try await .value(Installation.handlers(of: document, matching: predicate))
            case .launch(let url, let documents, let arguments, let environment, let new, let opening, let wait):
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
                        waiting: wait,
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
                        for: replyDeadline,
                        { () async in
                            guard let record else {
                                return (await Result { () async throws(ApplicationFailure) in try await watch.act(steps) }, Result<Recording, ApplicationFailure>?.none)
                            }
                            return await watch.record(record.source, keyFrames: record.frames, longestSide: record.longestSide) {
                                (began: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) in try await watch.act(steps, stepping: began)
                            } as (Result<_, ApplicationFailure>, Result<Recording, ApplicationFailure>?)
                        },
                    )
                }
                try hold.released.get()
                let (outcome, recording) = hold.value.get()
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
                        usage: acted.diagnosis.map { diagnosis in EncodedResult(diagnosis.usage) },
                        load: acted.diagnosis.map { diagnosis in EncodedResult(diagnosis.load) },
                        written: EncodedResult(written),
                        pasteboard: copied.map(EncodedResult.init),
                        recording: recording.map(EncodedResult.init),
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
                    for: replyDeadline,
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
                        for: replyDeadline,
                        { () async throws(AggregateError<AppleEventFailure>) in try await dictionary.send(nil, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: process.pid)) },
                    )
                }
                try hold.released.get()
                return try .value(ExecuteResult(reply: hold.value.get(), written: EncodedResult(written)))
            case .capture(let process, let source, let longestSide, let marks):
                let instance = try await watches.instance(process, matching: matching)
                try Session.require("Capture")
                return try await .value(watches.watch(instance).capture(source, longestSide: longestSide, marks: marks).map(EncodedResult.init))
            case .record(let process, let source, let longestSide, let frames, let seconds, let movie):
                let instance = try await watches.instance(process, matching: matching)
                try Session.require("Recording")
                let recorded = await watches.watch(instance).record(source, keyFrames: frames, longestSide: longestSide, movie: movie) {
                    (_: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) in
                    do { try await Task.sleep(for: .seconds(seconds)) } catch { throw ApplicationFailure(subject: "the recording", reason: .cancelled) }
                }
                try recorded.value.get()
                return try .value(recorded.recording.get())
            case .scale(let image, let longestSide):
                let scaled = try image.image(longestSide: longestSide)
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
                let (samples, unsampled): ([URL], [String]) =
                    switch Result(catching: { () throws(ProcessFailure) -> [URL] in try instance?.process.samples(since: earliest ?? .distantPast) ?? [] }) {
                        case .success(let found): (Array(found.prefix(limit)), [])
                        case .failure(let failure): ([], [failure.errorDescription].compactMap(\.self))
                    }
                let logs = LogLine.newest(in: folders, since: earliest, limit: limit) { line in find.map { text in line.text.localizedCaseInsensitiveContains(text) } ?? true }
                let unified = await Result { () async throws(any Error) -> [LogEntry] in
                    guard let source else { throw ApplicationFailure(subject: (application?.url ?? Bundle.main.bundleURL).path(percentEncoded: false), reason: .noExecutable) }
                    return try await LogEntry.history(of: source, since: earliest, limit: limit, containing: find)
                }
                return .value(
                    Diagnostics(
                        reports: reports.reports,
                        samples: samples,
                        logs: logs.lines,
                        unified: EncodedResult(unified),
                        unreadable: reports.failures.compactMap(\.errorDescription) + logs.failures.compactMap(\.errorDescription) + unsampled,
                    )
                )
        }
    }
}
