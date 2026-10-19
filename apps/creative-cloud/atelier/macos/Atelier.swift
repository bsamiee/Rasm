import AppKit
import Foundation
import RasmMacOS
import UniformTypeIdentifiers
import os

// --- [MODELS] --------------------------------------------------------------------------

struct Release: Encodable {
    private enum Key: CodingKey { case channel }

    static let identifierPrefix: String = "com.adobe."

    let installation: Installation
    let channel: String?

    static func at(_ url: URL) -> Result<Self, AggregateError<Failure>> {
        NSWorkspace.shared.installedApplication(at: url.resolvingSymlinksInPath())
            .aggregated(Failure.discovery)
            .flatMap { installation in installation.identifier.hasPrefix(identifierPrefix) ? .success(Self(installation)) : .failure(AggregateError(first: .notApplication(url))) }
    }

    static func all() -> Result<[Self], AggregateError<Failure>> {
        NSWorkspace.shared.installedApplications(matching: "kMDItemCFBundleIdentifier == '\(identifierPrefix)*'", in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask))
            .map { installations in installations.map(Self.init) }
            .aggregated(Failure.discovery)
    }

    func encode(to encoder: any Encoder) throws {
        try installation.encode(to: encoder)
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encodeIfPresent(channel, forKey: .channel)
    }
}

extension Release {
    init(_ installation: Installation) {
        let bundle: Bundle? = Bundle(url: installation.url)
        self.installation = installation
        channel =
            (bundle?.object(forInfoDictionaryKey: "RELEASECHANNEL") as? String)?.lowercased()
            ?? (bundle?.object(forInfoDictionaryKey: "CFBundleName") as? String).flatMap { name in name.wholeMatch(of: /.*\((?<channel>[^)]+)\)/)?.channel.lowercased() }
    }
}

struct Executed: Encodable {
    struct Artifact: Encodable {
        let uri: URL
        let mimeType: String?
        let data: Data
    }

    private enum Key: String, CodingKey {
        case reply, artifacts, success, failure
        case tag = "_tag"
    }

    let reply: Native
    let artifacts: [Result<Artifact, Failure>]

    init(reply: Native, reading files: [URL]) {
        self.reply = reply
        artifacts = files.map { file in
            Result { try Artifact(uri: file, mimeType: file.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, data: Data(contentsOf: file)) }.mapError { error in
                .unreadable(file, error)
            }
        }
    }

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode(reply, forKey: .reply)
        var reads: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer(forKey: .artifacts)
        for read: Result<Artifact, Failure> in artifacts {
            var entry: KeyedEncodingContainer<Key> = reads.nestedContainer(keyedBy: Key.self)
            switch read {
                case .success(let artifact):
                    try entry.encode("Success", forKey: .tag)
                    try entry.encode(artifact, forKey: .success)
                case .failure(let failure):
                    try entry.encode("Failure", forKey: .tag)
                    try entry.encode(failure, forKey: .failure)
            }
        }
    }
}

enum Reply {
    case value(any Encodable)
    case stream(AsyncStream<ApplicationStatus>)
}

enum Request: Decodable {
    struct Target: Decodable {
        let application: URL
    }

    struct Press: Decodable {
        let application: URL
        let element: CGWindowID
        let action: ElementAction
    }

    struct Definition: Decodable {
        let application: URL
        let selection: ScriptingDictionary.Selection?
    }

    struct Terms: Decodable {
        let application: URL
        let kind: Kind
    }

    struct Execute: Decodable {
        let application: URL
        let command: String
        let arguments: [String: Native]
        let artifacts: [URL]
    }

    struct Capture: Decodable {
        let application: URL
        let window: CGWindowID?
    }

    struct Named: Decodable {
        let name: String
    }

    case installations
    case launch(Target)
    case status(Target)
    case watch(Target)
    case press(Press)
    case definition(Definition)
    case terms(Terms)
    case execute(Execute)
    case screenshot(Capture)
    case faces
    case face(Named)

    private enum Method: String, Decodable { case installations, launch, status, watch, press, definition, terms, execute, screenshot, faces, face }
    private enum Key: CodingKey { case method, params }

    init(from decoder: any Decoder) throws {
        let call: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        self =
            switch try call.decode(Method.self, forKey: .method) {
                case .installations: .installations
                case .launch: try .launch(call.decode(Target.self, forKey: .params))
                case .status: try .status(call.decode(Target.self, forKey: .params))
                case .watch: try .watch(call.decode(Target.self, forKey: .params))
                case .press: try .press(call.decode(Press.self, forKey: .params))
                case .definition: try .definition(call.decode(Definition.self, forKey: .params))
                case .terms: try .terms(call.decode(Terms.self, forKey: .params))
                case .execute: try .execute(call.decode(Execute.self, forKey: .params))
                case .screenshot: try .screenshot(call.decode(Capture.self, forKey: .params))
                case .faces: .faces
                case .face: try .face(call.decode(Named.self, forKey: .params))
            }
    }

    func perform(dictionaries: Dictionaries) async -> Result<Reply, AggregateError<Failure>> {
        func refused(_ error: ElementFailure) -> AggregateError<Failure> {
            if case .untrusted = error { Accessibility.prompt() }
            return AggregateError(first: .element(error))
        }
        func running(_ application: URL) -> Result<NSRunningApplication, AggregateError<Failure>> {
            Release.at(application).flatMap { release in release.installation.runningApplication.aggregated(Failure.discovery) }
        }
        return switch self {
            case .installations:
                Release.all().map(Reply.value)
            case .launch(let target):
                await Release.at(target.application).bind { release in
                    await release.installation.launched().map { launched in Reply.value(Release(installation: launched.installation, channel: release.channel)) }.aggregated(Failure.launch)
                }
            case .status(let target):
                running(target.application).flatMap { instance in instance.status().mapError(refused) }.map(Reply.value)
            case .watch(let target):
                await running(target.application).bind { instance in await instance.statusChanges().mapError(refused) }.map(Reply.stream)
            case .press(let press):
                running(press.application).flatMap { instance in instance.perform(press.action, on: press.element).mapError(refused) }.map(Reply.value)
            case .definition(let definition):
                await Release.at(definition.application).bind { release in await dictionaries.text(of: release.installation, selecting: definition.selection) }.map(Reply.value)
            case .terms(let terms):
                await Release.at(terms.application)
                    .bind { release in await dictionaries.dictionary(of: release.installation).aggregated(Failure.definition) }
                    .map { dictionary in Reply.value(dictionary.terms(terms.kind)) }
            case .execute(let call):
                await Release.at(call.application)
                    .bind { release in await release.installation.launched().aggregated(Failure.launch) }
                    .bind { launched in await dictionaries.dictionary(of: launched.installation).aggregated(Failure.definition).map { dictionary in (launched.instance, dictionary) } }
                    .flatMap { instance, dictionary in
                        dictionary.event(call.command, arguments: call.arguments, target: NSAppleEventDescriptor(processIdentifier: instance.processIdentifier))
                            .mapError { errors in errors.map(Failure.dictionary) }
                            .flatMap { event in event.sent().aggregated(Failure.event) }
                            .map { reply in Reply.value(Executed(reply: Native(reply, in: dictionary), reading: call.artifacts)) }
                    }
            case .screenshot(let capture):
                await Release.at(capture.application)
                    .bind { release in await NSWorkspace.shared.activateApplication(at: release.installation.url).aggregated(Failure.activation) }
                    .bind { instance in await instance.screenshot(ScreenshotOptions(target: .window(capture.window))).aggregated(Failure.screenshot) }
                    .map(Reply.value)
            case .faces:
                .success(.value(InstalledFace.installed()))
            case .face(let named):
                InstalledFace.named(named.name).map { face in .success(.value(face)) }
                    ?? .failure(AggregateError(first: .unmatchedFace(named.name, InstalledFace.installed().map(\.postscriptName))))
        }
    }
}

enum Message: Decodable {
    case call(id: Int, request: Result<Request, Answer.Fault>)
    case interrupt(id: Int)
    case ping
    case other

    private enum Key: CodingKey { case id, method, params, requestId }

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        self =
            switch (try container.decodeIfPresent(Int.self, forKey: .id), try container.decode(String.self, forKey: .method)) {
                case (.some(let id), _): .call(id: id, request: Result { try Request(from: decoder) }.mapError(Answer.Fault.invalid))
                case (.none, "@effect/rpc/Interrupt"): try .interrupt(id: container.nestedContainer(keyedBy: Key.self, forKey: .params).decode(Int.self, forKey: .requestId))
                case (.none, "@effect/rpc/Ping"): .ping
                case (.none, _): .other
            }
    }
}

enum Answer: Encodable {
    enum Fault: Error {
        case invalid(any Error)
        case refused(AggregateError<Failure>)
        case unencodable(any Error)
    }

    enum Outcome: Encodable {
        case result(any Encodable)
        case chunk(any Encodable)
        case end
        case fault(Fault)

        func encode(to encoder: any Encoder) throws {
            var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            switch self {
                case .result(let value):
                    try container.encode(value, forKey: .result)
                case .chunk(let value):
                    try container.encode(true, forKey: .chunk)
                    var values: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer(forKey: .result)
                    try values.encode(value)
                case .end:
                    break
                case .fault(let fault):
                    let (code, tag, field, failures): (Int, String, Key, AggregateError<Failure>) =
                        switch fault {
                            case .invalid(let cause): (-32_600, "Die", .defect, AggregateError(first: .malformed(cause)))
                            case .refused(let failures): (0, "Fail", .error, failures)
                            case .unencodable(let cause): (-32_603, "Die", .defect, AggregateError(first: .unencodable(cause)))
                        }
                    var error: KeyedEncodingContainer<Key> = container.nestedContainer(keyedBy: Key.self, forKey: .error)
                    try error.encode("Cause", forKey: .tag)
                    try error.encode(code, forKey: .code)
                    try error.encode(failures.first.localizedDescription, forKey: .message)
                    var reasons: any UnkeyedEncodingContainer = error.nestedUnkeyedContainer(forKey: .data)
                    for failure: Failure in failures.errors {
                        var reason: KeyedEncodingContainer<Key> = reasons.nestedContainer(keyedBy: Key.self)
                        try reason.encode(tag, forKey: .tag)
                        try reason.encode(failure, forKey: field)
                    }
            }
        }
    }

    private enum Key: String, CodingKey {
        case jsonrpc, id, method, result, chunk, error, code, message, data, defect
        case tag = "_tag"
    }

    case reply(id: Int, outcome: Outcome)
    case pong

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode("2.0", forKey: .jsonrpc)
        switch self {
            case .reply(let id, let outcome):
                try container.encode(id, forKey: .id)
                try outcome.encode(to: encoder)
            case .pong:
                try container.encode("@effect/rpc/Pong", forKey: .method)
        }
    }

    func line() -> Result<Data, any Error> {
        Result { try JSONEncoder().encode(self) }
            .flatMapError { error in
                switch self {
                    case .reply(let id, _): Result { try JSONEncoder().encode(Self.reply(id: id, outcome: .fault(.unencodable(error)))) }
                    case .pong: .failure(error)
                }
            }
            .map { data in data + Data("\n".utf8) }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: LocalizedError, Encodable {
    case notApplication(URL)
    case discovery(DiscoveryFailure)
    case launch(LaunchFailure)
    case activation(ActivationFailure)
    case element(ElementFailure)
    case screenshot(ScreenshotFailure)
    case definition(DefinitionFailure)
    case dictionary(DictionaryFailure)
    case event(AppleEventFailure)
    case unreadable(URL, any Error)
    case unmatchedFace(String, [String])
    case malformed(any Error)
    case unencodable(any Error)

    private enum Key: String, CodingKey {
        case name, message, subject, query, candidates
        case tag = "_tag"
    }

    var errorDescription: String? {
        switch self {
            case .notApplication(let url): "\(url.path(percentEncoded: false)) is no application with an identifier starting \(Release.identifierPrefix)"
            case .discovery(let error as any Error), .launch(let error as any Error), .activation(let error as any Error), .element(let error as any Error), .screenshot(let error as any Error),
                .definition(let error as any Error), .dictionary(let error as any Error), .event(let error as any Error):
                error.localizedDescription
            case .unreadable(let file, let error): "\(file.path(percentEncoded: false)) could not be read: \(error.localizedDescription)"
            case .unmatchedFace(let name, _): "CoreText matches no installed face named \(name)"
            case .malformed(let cause), .unencodable(let cause): String(describing: cause)
        }
    }

    var name: String {
        let innermost: Any = sequence(first: self as Any) { value in
            Mirror(reflecting: value).children.first.map(\.value).flatMap { payload in payload is any LocalizedError && Mirror(reflecting: payload).displayStyle == .enum ? payload : nil }
        }
        .reduce(self as Any) { _, value in value }
        return Mirror(reflecting: innermost).children.first?.label ?? String(describing: innermost)
    }

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .unmatchedFace(let query, let candidates):
                try container.encode("Unmatched", forKey: .tag)
                try container.encode("installed face", forKey: .subject)
                try container.encode(query, forKey: .query)
                try container.encode(candidates, forKey: .candidates)
            case .notApplication, .discovery, .launch, .activation, .element, .screenshot, .definition, .dictionary, .event, .unreadable, .malformed, .unencodable:
                try container.encode(name, forKey: .name)
                try container.encode(localizedDescription, forKey: .message)
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Streams {
    private var running: [Int: Task<Void, Never>] = [:]
    private var accepting: Bool = true

    func start(_ id: Int, _ work: @escaping @Sendable () async -> Void) -> Task<Void, Never> {
        let task: Task<Void, Never> = Task { await work() }
        if accepting { running[id] = task } else { task.cancel() }
        return task
    }

    func end(_ id: Int) {
        running.removeValue(forKey: id)?.cancel()
    }

    func endAll() {
        accepting = false
        for task: Task<Void, Never> in running.values { task.cancel() }
        running.removeAll()
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let logger: Logger? = Logger(category: "Atelier")
        let (replies, sink): (AsyncStream<Data>, AsyncStream<Data>.Continuation) = AsyncStream.makeStream()
        let streams: Streams = Streams()
        let dictionaries: Dictionaries = Dictionaries()
        let emit: @Sendable (Answer) -> Void = { answer in
            switch answer.line() {
                case .success(let data): sink.yield(data)
                case .failure(let error): logger?.debug("Dropped a reply no line can carry: \(String(describing: error), privacy: .public)")
            }
        }
        do {
            try disclaimResponsibility()
            try Accessibility.messagingTimeout(1).get()
            try await withThrowingDiscardingTaskGroup { group in
                group.addTask {
                    for await reply: Data in replies { try FileHandle.standardOutput.write(contentsOf: reply) }
                }
                try await withThrowingDiscardingTaskGroup { requests in
                    for try await line: String in FileHandle.standardInput.bytes.lines {
                        requests.addTask(executorPreference: DispatchQueue.global()) {
                            switch Result(catching: { try JSONDecoder().decode(Message.self, from: Data(line.utf8)) }) {
                                case .success(.call(let id, let request)):
                                    switch await request.bind({ request in await request.perform(dictionaries: dictionaries).mapError(Answer.Fault.refused) }) {
                                        case .success(.value(let value)): emit(.reply(id: id, outcome: .result(value)))
                                        case .success(.stream(let statuses)):
                                            await streams.start(id) {
                                                for await status: ApplicationStatus in statuses { emit(.reply(id: id, outcome: .chunk(status))) }
                                                emit(.reply(id: id, outcome: .end))
                                                await streams.end(id)
                                            }
                                            .value
                                        case .failure(let fault): emit(.reply(id: id, outcome: .fault(fault)))
                                    }
                                case .success(.interrupt(let id)): await streams.end(id)
                                case .success(.ping): emit(.pong)
                                case .success(.other): break
                                case .failure(let error): logger?.debug("Dropped a line no reply can match: \(String(describing: error), privacy: .public)")
                            }
                        }
                    }
                    await streams.endAll()
                }
                sink.finish()
            }
        } catch {
            logger?.fault("Stopped reading requests: \(String(describing: error), privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}
