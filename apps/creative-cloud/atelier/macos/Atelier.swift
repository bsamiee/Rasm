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
            .flatMap { installation in installation.identifier.hasPrefix(identifierPrefix) ? .success(Self(installation)) : .failure(AggregateError(first: .notAdobe(url))) }
    }

    static func all() -> Result<[Self], AggregateError<Failure>> {
        NSWorkspace.shared.installedApplications(matching: .identifierPrefix(identifierPrefix), in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask))
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

struct Entry: Encodable {
    let name: String
    let code: String
}

extension Entry {
    init(_ term: Term) {
        name = term.name
        code = term.code.sdef
    }

    init(_ command: Command) {
        name = command.name
        code = command.code
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

    let reply: AppleEventValue
    let artifacts: [Result<Artifact, Failure>]

    init(reply: AppleEventValue, reading files: [URL]) {
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

struct JSONError: Encodable {
    private enum Key: CodingKey { case name, message, cause }

    let error: any Error

    func encode(to encoder: any Encoder) throws {
        let mirror: Mirror = Mirror(reflecting: error)
        let (name, message): (String, String) =
            mirror.displayStyle == .enum
            ? (mirror.children.first?.label ?? String(describing: error), (error as? any LocalizedError)?.errorDescription ?? String(describing: error))
            : ((error as NSError).domain, error.localizedDescription)
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encode(name, forKey: .name)
        try fields.encode(message, forKey: .message)
        try fields.encodeIfPresent((error as NSError).underlyingErrors.first.map { cause in Self(error: cause) }, forKey: .cause)
    }
}

enum Response {
    case value(any Encodable?)
    case stream(any AsyncSequence<Result<ApplicationStatus, AggregateError<Failure>>, Never>)
}

enum Request: Decodable {
    struct Target: Decodable {
        let application: URL
        let pid: pid_t?
    }

    struct Press: Decodable {
        let element: CGWindowID
        let action: ElementAction
    }

    struct Definition: Decodable {
        let application: URL
        let selection: ScriptingDictionary.Selection?
    }

    struct Terms: Decodable {
        let application: URL
        let kind: ScriptingDictionary.Kind
    }

    struct Execute: Decodable {
        let command: String
        let arguments: [String: AppleEventValue]
        let artifacts: [URL]
    }

    struct Named: Decodable {
        let name: String
    }

    case installations
    case launch(Target)
    case status(Target)
    case watch(Target)
    case press(Target, Press)
    case definition(Definition)
    case terms(Terms)
    case execute(Target, Execute)
    case screenshot(Target, ScreenshotOptions)
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
                case .press: try .press(call.decode(Target.self, forKey: .params), call.decode(Press.self, forKey: .params))
                case .definition: try .definition(call.decode(Definition.self, forKey: .params))
                case .terms: try .terms(call.decode(Terms.self, forKey: .params))
                case .execute: try .execute(call.decode(Target.self, forKey: .params), call.decode(Execute.self, forKey: .params))
                case .screenshot: try .screenshot(call.decode(Target.self, forKey: .params), call.decode(ScreenshotOptions.self, forKey: .params))
                case .faces: .faces
                case .face: try .face(call.decode(Named.self, forKey: .params))
            }
    }

    func perform(definitions: ScriptingDefinitions) async -> Result<Response, AggregateError<Failure>> {
        let deadline: Duration = .seconds(60)
        @Sendable func refused(_ error: ElementFailure) -> AggregateError<Failure> {
            if case .untrusted = error { Accessibility.prompt() }
            return AggregateError(first: .element(error))
        }
        func running(_ target: Target) -> Result<NSRunningApplication, AggregateError<Failure>> {
            Release.at(target.application).flatMap { release in release.installation.runningApplication(target.pid).aggregated(Failure.discovery) }
        }
        func reached(_ target: Target) async -> Result<(installation: Installation, instance: NSRunningApplication), AggregateError<Failure>> {
            await Release.at(target.application).bind { release in
                switch (target.pid, release.installation.instances.isEmpty) {
                    case (.none, true): await release.installation.launched(within: deadline, createsNewApplicationInstance: false).aggregated(Failure.launch)
                    case (.some, _), (.none, false): release.installation.runningApplication(target.pid).map { instance in (release.installation, instance) }.aggregated(Failure.discovery)
                }
            }
        }
        return switch self {
            case .installations:
                Release.all().map(Response.value)
            case .launch(let target):
                await Release.at(target.application).bind { release in
                    await release.installation.launched(within: deadline, createsNewApplicationInstance: false)
                        .map { launched in Response.value(Release(installation: launched.installation, channel: release.channel)) }
                        .aggregated(Failure.launch)
                }
            case .status(let target):
                running(target).flatMap { instance in instance.status().mapError(refused) }.map(Response.value)
            case .watch(let target):
                running(target).map { instance in Response.stream(StatusChanges(instance).map { next in next.mapError(refused) }) }
            case .press(let target, let press):
                running(target).flatMap { instance in instance.perform(press.action, on: press.element).mapError(refused) }.map { Response.value(nil) }
            case .definition(let definition):
                await Release.at(definition.application)
                    .bind { release in await definitions.dictionary(of: release.installation, within: deadline).aggregated(Failure.definition) }
                    .flatMap { dictionary in dictionary.text(selecting: definition.selection).aggregated(Failure.definition) }
                    .map(Response.value)
            case .terms(let terms):
                await Release.at(terms.application)
                    .bind { release in await definitions.dictionary(of: release.installation, within: deadline).aggregated(Failure.definition) }
                    .map { dictionary in
                        switch terms.kind {
                            case .command: dictionary.commands.map(Entry.init)
                            case .suite: dictionary.suites.map(Entry.init)
                            case .class: dictionary.classes.map(Entry.init)
                            case .recordType: dictionary.recordTypes.map(Entry.init)
                            case .valueType: dictionary.valueTypes.map(Entry.init)
                            case .enumeration: dictionary.enumerations.map(Entry.init)
                        }
                    }
                    .map(Response.value)
            case .execute(let target, let call):
                await reached(target)
                    .bind { reached in
                        await definitions.dictionary(of: reached.installation, within: deadline).aggregated(Failure.definition).map { dictionary in (reached.instance, dictionary) }
                    }
                    .flatMap { instance, dictionary in
                        dictionary.event(call.command, arguments: call.arguments, target: NSAppleEventDescriptor(processIdentifier: instance.processIdentifier))
                            .mapError { errors in errors.map(Failure.dictionary) }
                            .flatMap { event in event.sent().aggregated(Failure.event) }
                            .map { reply in Response.value(Executed(reply: AppleEventValue(reply, in: dictionary), reading: call.artifacts)) }
                    }
            case .screenshot(let target, let options):
                await reached(target)
                    .bind { reached in await reached.instance.activated(within: deadline).aggregated(Failure.activation) }
                    .bind { instance in await instance.screenshot(options).aggregated(Failure.screenshot) }
                    .map(Response.value)
            case .faces:
                .success(.value(InstalledFace.installed()))
            case .face(let named):
                Result { () throws(AggregateError<Failure>) in
                    guard let face: InstalledFace = InstalledFace.named(named.name) else {
                        throw AggregateError(first: .unmatched(Unmatched(subject: "installed face", query: named.name, candidates: InstalledFace.installed().map(\.postscriptName))))
                    }
                    return .value(face)
                }
        }
    }
}

enum FromClient: Decodable {
    private enum Control: String {
        case ping = "@effect/rpc/Ping"
        case interrupt = "@effect/rpc/Interrupt"
        case ack = "@effect/rpc/Ack"
        case eof = "@effect/rpc/Eof"
    }

    private enum Key: CodingKey { case id, method, params, requestId }

    case request(id: Int, Result<Request, any Error>)
    case interrupt(requestId: Int)
    case ping
    case ack
    case eof

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        self =
            switch Control(rawValue: try container.decode(String.self, forKey: .method)) {
                case .ping: .ping
                case .interrupt: try .interrupt(requestId: container.nestedContainer(keyedBy: Key.self, forKey: .params).decode(Int.self, forKey: .requestId))
                case .ack: .ack
                case .eof: .eof
                case .none: try .request(id: container.decode(Int.self, forKey: .id), Result { try Request(from: decoder) })
            }
    }
}

enum FromServer: Encodable {
    enum Reason: Encodable {
        private enum Key: String, CodingKey {
            case error, defect
            case tag = "_tag"
        }

        case fail(Failure)
        case die(any Error)

        func encode(to encoder: any Encoder) throws {
            var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            switch self {
                case .fail(let failure):
                    try container.encode("Fail", forKey: .tag)
                    try container.encode(failure, forKey: .error)
                case .die(let error):
                    try container.encode("Die", forKey: .tag)
                    try container.encode(JSONError(error: error), forKey: .defect)
            }
        }
    }

    private enum Key: String, CodingKey {
        case jsonrpc, id, method, result, chunk, error, code, message, data
        case tag = "_tag"
    }

    case exit(requestId: Int, Result<any Encodable?, AggregateError<Reason>>)
    case chunk(requestId: Int, any Encodable)
    case pong

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode("2.0", forKey: .jsonrpc)
        switch self {
            case .exit(let id, .success(let value)):
                try container.encode(id, forKey: .id)
                if let value { try container.encode(value, forKey: .result) } else { try container.encodeNil(forKey: .result) }
            case .exit(let id, .failure(let cause)):
                let message: String =
                    switch cause.first {
                        case .fail(let failure): failure.localizedDescription
                        case .die(let error): String(describing: error)
                    }
                try container.encode(id, forKey: .id)
                var error: KeyedEncodingContainer<Key> = container.nestedContainer(keyedBy: Key.self, forKey: .error)
                try error.encode("Cause", forKey: .tag)
                try error.encode(0, forKey: .code)
                try error.encode(message, forKey: .message)
                try error.encode(cause.errors, forKey: .data)
            case .chunk(let id, let value):
                try container.encode(id, forKey: .id)
                try container.encode(true, forKey: .chunk)
                var values: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer(forKey: .result)
                try values.encode(value)
            case .pong:
                try container.encode("@effect/rpc/Pong", forKey: .method)
        }
    }

    func line() -> Result<Data, any Error> {
        Result { try JSONEncoder().encode(self) }
            .flatMapError { error in
                switch self {
                    case .exit(let id, _), .chunk(let id, _): Result { try JSONEncoder().encode(Self.exit(requestId: id, .failure(AggregateError(first: .die(error))))) }
                    case .pong: .failure(error)
                }
            }
            .map { data in data + Data("\n".utf8) }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: CustomNSError, LocalizedError, Encodable {
    case notAdobe(URL)
    case discovery(DiscoveryFailure)
    case launch(LaunchFailure)
    case activation(ActivationFailure)
    case element(ElementFailure)
    case screenshot(ScreenshotFailure)
    case definition(DefinitionFailure)
    case dictionary(DictionaryFailure)
    case event(AppleEventFailure)
    case unreadable(URL, any Error)
    case unmatched(Unmatched)

    private enum Key: String, CodingKey {
        case subject, query, candidates
        case tag = "_tag"
    }

    var errorDescription: String? {
        switch self {
            case .notAdobe(let url): "\(url.path(percentEncoded: false)) is no application with an identifier starting \(Release.identifierPrefix)"
            case .discovery(let error as any Error), .launch(let error as any Error), .activation(let error as any Error), .element(let error as any Error), .screenshot(let error as any Error),
                .definition(let error as any Error), .dictionary(let error as any Error), .event(let error as any Error), .unmatched(let error as any Error):
                error.localizedDescription
            case .unreadable(let file, let error): "\(file.path(percentEncoded: false)) could not be read: \(error.localizedDescription)"
        }
    }

    var errorUserInfo: [String: Any] {
        if case .unreadable(_, let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }

    func encode(to encoder: any Encoder) throws {
        switch self {
            case .unmatched(let miss), .discovery(.unmatched(let miss)), .element(.unmatched(let miss)), .definition(.unmatched(let miss)), .dictionary(.unmatched(let miss)):
                var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
                try container.encode("Unmatched", forKey: .tag)
                try container.encode(miss.subject, forKey: .subject)
                try container.encodeIfPresent(miss.query, forKey: .query)
                try container.encode(miss.candidates, forKey: .candidates)
            case .discovery(let error as any Error), .launch(let error as any Error), .activation(let error as any Error), .element(let error as any Error), .screenshot(let error as any Error),
                .definition(let error as any Error), .dictionary(let error as any Error), .event(let error as any Error):
                try JSONError(error: error).encode(to: encoder)
            case .notAdobe, .unreadable:
                try JSONError(error: self).encode(to: encoder)
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Calls {
    private var running: [Int: Task<Void, Never>] = [:]

    func start(_ id: Int, _ call: @escaping @concurrent @Sendable () async -> Void) {
        running[id] = Task {
            await call()
            running[id] = nil
        }
    }

    func interrupt(_ id: Int) {
        running.removeValue(forKey: id)?.cancel()
    }

    func interruptAll() async {
        let tasks: [Task<Void, Never>] = Array(running.values)
        for task: Task<Void, Never> in tasks { task.cancel() }
        for task: Task<Void, Never> in tasks { await task.value }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let logger: Logger? = Logger(category: "Atelier")
        let (lines, sink): (AsyncStream<Data>, AsyncStream<Data>.Continuation) = AsyncStream.makeStream()
        let calls: Calls = Calls()
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        let send: @Sendable (FromServer) -> Void = { message in
            switch message.line() {
                case .success(let data): sink.yield(data)
                case .failure(let error): logger?.debug("Dropped a reply no line can carry: \(String(describing: error), privacy: .public)")
            }
        }
        @concurrent @Sendable func answer(_ id: Int, _ request: Result<Request, any Error>) async {
            switch await request.aggregated(FromServer.Reason.die).bind({ request in await request.perform(definitions: definitions).mapError { failures in failures.map(FromServer.Reason.fail) } }) {
                case .success(.value(let value)): send(.exit(requestId: id, .success(value)))
                case .success(.stream(let statuses)):
                    for await next: Result<ApplicationStatus, AggregateError<Failure>> in statuses {
                        switch next {
                            case .success(let status): send(.chunk(requestId: id, status))
                            case .failure(let failures):
                                send(.exit(requestId: id, .failure(failures.map(FromServer.Reason.fail))))
                                return
                        }
                    }
                    send(.exit(requestId: id, .success(nil)))
                case .failure(let cause): send(.exit(requestId: id, .failure(cause)))
            }
        }
        do {
            try disclaimResponsibility()
            try Accessibility.messagingTimeout(1).get()
            try await withThrowingDiscardingTaskGroup { group in
                group.addTask {
                    for await line: Data in lines { try FileHandle.standardOutput.write(contentsOf: line) }
                }
                for try await line: String in FileHandle.standardInput.bytes.lines {
                    switch Result(catching: { try JSONDecoder().decode(FromClient.self, from: Data(line.utf8)) }) {
                        case .success(.request(let id, let request)): await calls.start(id) { await answer(id, request) }
                        case .success(.interrupt(let id)): await calls.interrupt(id)
                        case .success(.ping): send(.pong)
                        case .success(.ack), .success(.eof): break
                        case .failure(let error): logger?.debug("Dropped a line no reply can match: \(String(describing: error), privacy: .public)")
                    }
                }
                await calls.interruptAll()
                sink.finish()
            }
        } catch {
            logger?.fault("Stopped reading requests: \(String(describing: error), privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}
