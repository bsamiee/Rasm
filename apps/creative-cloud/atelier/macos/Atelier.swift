import AppKit
import Foundation
import RasmMacOS
import UniformTypeIdentifiers
import os

// --- [MODELS] --------------------------------------------------------------------------

extension Result where Success: Encodable, Failure: Encodable {
    private enum Key: String, CodingKey {
        case success, failure
        case tag = "_tag"
    }

    func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .success(let value):
                try fields.encode("Success", forKey: .tag)
                try fields.encode(value, forKey: .success)
            case .failure(let error):
                try fields.encode("Failure", forKey: .tag)
                try fields.encode(error, forKey: .failure)
        }
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

    private enum Key: CodingKey { case reply, artifacts }

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
        for read: Result<Artifact, Failure> in artifacts { try read.encode(to: reads.superEncoder()) }
    }
}

struct Launched: Encodable {
    let pid: pid_t
}

struct InstanceStatus: Encodable {
    private enum Key: CodingKey { case pid, status }

    let pid: pid_t
    let status: Result<ApplicationStatus, Failure>

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode(pid, forKey: .pid)
        try status.encode(to: container.superEncoder(forKey: .status))
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
    case stream(any AsyncSequence<Result<any Encodable & Sendable, AggregateError<Failure>>, Never>)
}

enum Request: Decodable {
    struct Target: Decodable {
        let url: URL
        let pid: pid_t?
    }

    struct Press: Decodable {
        let element: CGWindowID
        let actions: [ElementAction]
    }

    struct Definition: Decodable {
        let url: URL
        let selection: ScriptingDictionary.Selection
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
    case statuses
    case launch(Target)
    case activate(Target)
    case status(Target)
    case press(Target, Press)
    case terms(Target)
    case definition(Definition)
    case execute(Target, Execute)
    case screenshot(Target, ScreenshotOptions)
    case faces
    case face(Named)

    private enum Method: String, Decodable { case installations, statuses, launch, activate, status, press, terms, definition, execute, screenshot, faces, face }
    private enum Key: CodingKey { case method, params }

    init(from decoder: any Decoder) throws {
        let call: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        self =
            switch try call.decode(Method.self, forKey: .method) {
                case .installations: .installations
                case .statuses: .statuses
                case .launch: try .launch(call.decode(Target.self, forKey: .params))
                case .activate: try .activate(call.decode(Target.self, forKey: .params))
                case .status: try .status(call.decode(Target.self, forKey: .params))
                case .press: try .press(call.decode(Target.self, forKey: .params), call.decode(Press.self, forKey: .params))
                case .terms: try .terms(call.decode(Target.self, forKey: .params))
                case .definition: try .definition(call.decode(Definition.self, forKey: .params))
                case .execute: try .execute(call.decode(Target.self, forKey: .params), call.decode(Execute.self, forKey: .params))
                case .screenshot: try .screenshot(call.decode(Target.self, forKey: .params), call.decode(ScreenshotOptions.self, forKey: .params))
                case .faces: .faces
                case .face: try .face(call.decode(Named.self, forKey: .params))
            }
    }

    func perform(definitions: ScriptingDefinitions) async -> Result<Response, AggregateError<Failure>> {
        let deadline: Duration = .seconds(60)
        func running(_ target: Target) -> Result<NSRunningApplication, AggregateError<Failure>> {
            Adobe.installation(at: target.url).flatMap { installation in installation.runningApplication(target.pid).aggregated(Failure.discovery) }
        }
        func reached(_ target: Target) async -> Result<(installation: Installation, instance: NSRunningApplication), AggregateError<Failure>> {
            await Adobe.installation(at: target.url).bind { installation in
                switch (target.pid, installation.instances.isEmpty) {
                    case (.none, true):
                        await NSWorkspace.shared.launchApplication(at: installation.url, within: deadline, createsNewApplicationInstance: false, activates: false)
                            .aggregated(Failure.launch)
                            .flatMap { instance in Installation.at(installation.url, among: [instance]).map { launched in (launched, instance) }.aggregated(Failure.discovery) }
                    case (.some, _), (.none, false): installation.runningApplication(target.pid).map { instance in (installation, instance) }.aggregated(Failure.discovery)
                }
            }
        }
        func activated(_ target: Target) async -> Result<NSRunningApplication, AggregateError<Failure>> {
            await reached(target).bind { reached in await NSWorkspace.shared.activateApplication(at: reached.installation.url, within: deadline).aggregated(Failure.activation) }
        }
        return switch self {
            case .installations:
                await .success(
                    .stream(
                        NSWorkspace.shared.applications(matching: .identifierPrefix(Adobe.identifierPrefix), in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask)).map {
                            listing -> Result<any Encodable & Sendable, AggregateError<Failure>> in listing.map(\.self).aggregated(Failure.discovery)
                        }
                    )
                )
            case .statuses:
                await .success(
                    .stream(
                        NSWorkspace.shared.instanceEvents(of: Adobe.owns).compactMap { event -> Result<any Encodable & Sendable, AggregateError<Failure>>? in
                            if case .status(let instance, let read) = event { .success(InstanceStatus(pid: instance.processIdentifier, status: read.mapError(Failure.element))) } else { nil }
                        }
                    )
                )
            case .launch(let target):
                await Adobe.installation(at: target.url).bind { installation in
                    await NSWorkspace.shared.launchApplication(at: installation.url, within: deadline, createsNewApplicationInstance: false, activates: false)
                        .map { instance in Response.value(Launched(pid: instance.processIdentifier)) }
                        .aggregated(Failure.launch)
                }
            case .activate(let target):
                await activated(target).map { _ in Response.value(nil) }
            case .status(let target):
                running(target).flatMap { instance in instance.status().aggregated(Failure.element) }.map(Response.value)
            case .press(let target, let press):
                running(target)
                    .flatMap { instance in
                        press.actions
                            .reduce(Result<Void, ElementFailure>.success(())) { performed, action in performed.flatMap { instance.perform(action, on: press.element) } }
                            .aggregated(Failure.element)
                    }
                    .map { Response.value(nil) }
            case .terms(let target):
                await Adobe.installation(at: target.url)
                    .bind { installation in await definitions.dictionary(of: installation, within: deadline).aggregated(Failure.definition) }
                    .map { dictionary in
                        Response.value(
                            Dictionary(
                                uniqueKeysWithValues: ScriptingDictionary.Kind.allCases.map { kind in
                                    let listed: [Entry] =
                                        switch kind {
                                            case .command: dictionary.commands.map(Entry.init)
                                            case .suite: dictionary.suites.map(Entry.init)
                                            case .class: dictionary.classes.map(Entry.init)
                                            case .recordType: dictionary.recordTypes.map(Entry.init)
                                            case .valueType: dictionary.valueTypes.map(Entry.init)
                                            case .enumeration: dictionary.enumerations.map(Entry.init)
                                        }
                                    return (kind.rawValue, listed)
                                }
                            )
                        )
                    }
            case .definition(let definition):
                await Adobe.installation(at: definition.url)
                    .bind { installation in await definitions.dictionary(of: installation, within: deadline).aggregated(Failure.definition) }
                    .flatMap { dictionary in dictionary.text(selecting: definition.selection).aggregated(Failure.definition) }
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
                await activated(target)
                    .bind { instance in await instance.screenshot(options).aggregated(Failure.screenshot) }
                    .map(Response.value)
            case .faces:
                .success(.value(InstalledFace.installed()))
            case .face(let named):
                InstalledFace.named(named.name).map(Response.value).aggregated(Failure.unmatched)
        }
    }
}

enum FromClient: Decodable {
    private enum Control: String {
        case interrupt = "@effect/rpc/Interrupt"
        case ack = "@effect/rpc/Ack"
    }

    private enum Key: CodingKey { case id, method, params, requestId }

    case request(id: Int, Result<Request, any Error>)
    case interrupt(requestId: Int)
    case ack

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        self =
            switch Control(rawValue: try container.decode(String.self, forKey: .method)) {
                case .interrupt: try .interrupt(requestId: container.nestedContainer(keyedBy: Key.self, forKey: .params).decode(Int.self, forKey: .requestId))
                case .ack: .ack
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
        case id, result, chunk, error, data
        case tag = "_tag"
    }

    case exit(requestId: Int, Result<any Encodable?, AggregateError<Reason>>)
    case chunk(requestId: Int, any Encodable)

    var requestId: Int {
        switch self {
            case .exit(let id, _), .chunk(let id, _): id
        }
    }

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode(requestId, forKey: .id)
        switch self {
            case .exit(_, .success(.some(let value))): try container.encode(value, forKey: .result)
            case .exit(_, .success(.none)): try container.encodeNil(forKey: .result)
            case .exit(_, .failure(let cause)):
                var error: KeyedEncodingContainer<Key> = container.nestedContainer(keyedBy: Key.self, forKey: .error)
                try error.encode("Cause", forKey: .tag)
                try error.encode(cause.errors, forKey: .data)
            case .chunk(_, let value):
                try container.encode(true, forKey: .chunk)
                var values: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer(forKey: .result)
                try values.encode(value)
        }
    }

    func line() -> Result<Data, any Error> {
        Result { try JSONEncoder().encode(self) }
            .flatMapError { error in Result { try JSONEncoder().encode(Self.exit(requestId: requestId, .failure(AggregateError(first: .die(error))))) } }
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
            case .notAdobe(let url): "\(url.path(percentEncoded: false)) is no application with an identifier starting \(Adobe.identifierPrefix)"
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
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum Adobe {
    static let identifierPrefix: String = "com.adobe."

    static func owns(_ instance: NSRunningApplication) -> Bool {
        instance.bundleIdentifier?.hasPrefix(identifierPrefix) == true
    }

    static func installation(at url: URL) -> Result<Installation, AggregateError<Failure>> {
        Installation.at(url)
            .aggregated(Failure.discovery)
            .flatMap { installation in installation.identifier.hasPrefix(identifierPrefix) ? .success(installation) : .failure(AggregateError(first: .notAdobe(url))) }
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
                case .success(.stream(let chunks)):
                    for await chunk: Result<any Encodable & Sendable, AggregateError<Failure>> in chunks {
                        switch chunk {
                            case .success(let value): send(.chunk(requestId: id, value))
                            case .failure(let cause):
                                send(.exit(requestId: id, .failure(cause.map(FromServer.Reason.fail))))
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
            Accessibility.prompt()
            try await withThrowingDiscardingTaskGroup { group in
                group.addTask {
                    for await line: Data in lines { try FileHandle.standardOutput.write(contentsOf: line) }
                }
                for try await line: String in FileHandle.standardInput.bytes.lines {
                    switch Result(catching: { try JSONDecoder().decode(FromClient.self, from: Data(line.utf8)) }) {
                        case .success(.request(let id, let request)): await calls.start(id) { await answer(id, request) }
                        case .success(.interrupt(let id)): await calls.interrupt(id)
                        case .success(.ack): break
                        case .failure(let error): logger?.debug("Dropped a line no reply can match: \(String(describing: error), privacy: .public)")
                    }
                }
                sink.finish()
            }
        } catch {
            logger?.fault("Stopped reading requests: \(String(describing: error), privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}
