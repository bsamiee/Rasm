import AppKit
import AppleEvents
import Applications
import Carbon.OpenScripting
import CoreServices
import Fonts
import Foundation
import Processes
import Results
import UniformTypeIdentifiers
import os

// --- [TYPES] ---------------------------------------------------------------------------

enum Interference {
    case activation
    case interface
    case menuAction
    case script
}

// --- [MODELS] --------------------------------------------------------------------------

struct Outcome<Success: Encodable & Sendable, Failure: Encodable & Error>: Encodable, Sendable {
    private enum Key: String, CodingKey {
        case success, failure
        case tag = "_tag"
    }

    let result: Result<Success, Failure>

    func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch result {
            case .success(let value):
                try fields.encode("Success", forKey: .tag)
                try fields.encode(value, forKey: .success)
            case .failure(let error):
                try fields.encode("Failure", forKey: .tag)
                try fields.encode(error, forKey: .failure)
        }
    }
}

struct Executed: Encodable {
    struct Artifact: Encodable, Sendable {
        let uri: URL
        let mimeType: String?
        let data: Data
    }

    let reply: AppleEventValue
    let artifacts: [Outcome<Artifact, Encoded>]

    init(reply: AppleEventValue, reading files: [URL]) {
        self.reply = reply
        artifacts = files.map { file in
            Outcome(
                result: Result { try Artifact(uri: file, mimeType: file.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, data: Data(contentsOf: file)) }
                    .mapError { error in Encoded(Refusal.unreadable(file, error)) }
            )
        }
    }
}

struct Captured: Encodable {
    let image: Data
    let width: Int
    let height: Int
}

struct InstanceStatus: Encodable {
    let pid: pid_t
    let status: Outcome<ApplicationStatus, Encoded>
}

struct Encoded: Error, Encodable {
    private enum Key: String, CodingKey {
        case name, message, cause, subject, query, candidates
        case tag = "_tag"
    }

    let error: any Error

    init(_ error: any Error) {
        self.error = error
    }

    func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch error {
            case let miss as Unmatched, ElementFailure.unmatched(let miss), DefinitionFailure.unmatched(let miss), DictionaryFailure.unmatched(let miss):
                try fields.encode("Unmatched", forKey: .tag)
                try fields.encode(miss.subject, forKey: .subject)
                try fields.encodeIfPresent(miss.query, forKey: .query)
                try fields.encode(miss.candidates, forKey: .candidates)
            default:
                let mirror: Mirror = Mirror(reflecting: error)
                let (name, message): (String, String) =
                    mirror.displayStyle == .enum
                    ? (mirror.children.first?.label ?? String(describing: error), (error as? any LocalizedError)?.errorDescription ?? String(describing: error))
                    : ((error as NSError).domain, error.localizedDescription)
                try fields.encode(name, forKey: .name)
                try fields.encode(message, forKey: .message)
                try fields.encodeIfPresent((error as NSError).underlyingErrors.first.map(Self.init), forKey: .cause)
        }
    }
}

enum Values {
    case any
    case among(Set<String?>)
    case except(String)

    func contain(_ descriptor: NSAppleEventDescriptor?) -> Bool {
        let value: String? = descriptor.map { descriptor in
            descriptor.descriptorType == DescType(typeEnumerated)
                ? descriptor.enumCodeValue.sdef
                : descriptor.coerce(toDescriptorType: DescType(typeBoolean)).map { boolean in DescType(boolean.booleanValue ? typeTrue : typeFalse).sdef } ?? descriptor.descriptorType.sdef
        }
        return switch self {
            case .any: true
            case .among(let refused): refused.contains(value)
            case .except(let safe): value != safe
        }
    }
}

enum Clause {
    case always
    case parameter(String, Values)
    case property(String, of: Set<String>, Values)
    case direct(of: Set<String>)

    func refuses(_ event: NSAppleEventDescriptor) -> Bool {
        let direct: NSAppleEventDescriptor? = event.paramDescriptor(forKeyword: AEKeyword(keyDirectObject))
        switch self {
            case .always:
                return true
            case .parameter(let keyword, let values):
                return values.contain(FourCharCode(sdef: keyword).flatMap(event.paramDescriptor(forKeyword:)))
            case .property(let property, let containers, let values):
                let data: NSAppleEventDescriptor? = event.paramDescriptor(forKeyword: AEKeyword(keyAEData))
                let assigned: NSAppleEventDescriptor? =
                    switch direct?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue {
                        case FourCharCode(sdef: property): data
                        case FourCharCode(pProperties): FourCharCode(sdef: property).flatMap { code in data?.forKeyword(code) }
                        case .some, .none: nil
                    }
                let container: String? = direct?.forKeyword(AEKeyword(keyAEContainer))?.forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue.sdef
                return direct?.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue == FourCharCode(formPropertyID) && assigned != nil
                    && (containers.isEmpty || container.map(containers.contains) == true) && values.contain(assigned)
            case .direct(let classes):
                return direct?.forKeyword(AEKeyword(keyAEDesiredClass)).map { want in classes.contains(want.typeCodeValue.sdef) } == true
        }
    }
}

enum Response {
    case value(any Encodable?)
    case stream(any AsyncSequence<Result<any Encodable & Sendable, AggregateError<any Error>>, Never>)
}

enum Request: Decodable {
    case installations
    case statuses
    case launch(URL)
    case status(pid_t)
    case press(pid_t, element: CGWindowID, actions: [ElementAction])
    case terms(URL)
    case definition(URL, kind: ScriptingDictionary.Kind, name: String)
    case execute(pid_t, command: String, arguments: [String: AppleEventValue], artifacts: [URL])
    case screenshot(pid_t, window: CGWindowID?, pixels: Int?)
    case faces
    case face(String)

    private enum Method: String, Decodable { case installations, statuses, launch, status, press, terms, definition, execute, screenshot, faces, face }
    private enum Key: CodingKey { case method, params }
    private enum Field: CodingKey { case url, pid, element, actions, kind, name, command, arguments, artifacts, window, pixels }

    init(from decoder: any Decoder) throws {
        let call: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let method: Method = try call.decode(Method.self, forKey: .method)
        let params: () throws -> KeyedDecodingContainer<Field> = { try call.nestedContainer(keyedBy: Field.self, forKey: .params) }
        self =
            switch method {
                case .installations: .installations
                case .statuses: .statuses
                case .launch: try .launch(params().decode(URL.self, forKey: .url))
                case .status: try .status(params().decode(pid_t.self, forKey: .pid))
                case .press:
                    try .press(
                        params().decode(pid_t.self, forKey: .pid),
                        element: params().decode(CGWindowID.self, forKey: .element),
                        actions: params().decode([ElementAction].self, forKey: .actions),
                    )
                case .terms: try .terms(params().decode(URL.self, forKey: .url))
                case .definition:
                    try .definition(params().decode(URL.self, forKey: .url), kind: params().decode(ScriptingDictionary.Kind.self, forKey: .kind), name: params().decode(String.self, forKey: .name))
                case .execute:
                    try .execute(
                        params().decode(pid_t.self, forKey: .pid),
                        command: params().decode(String.self, forKey: .command),
                        arguments: params().decode([String: AppleEventValue].self, forKey: .arguments),
                        artifacts: params().decode([URL].self, forKey: .artifacts),
                    )
                case .screenshot:
                    try .screenshot(
                        params().decode(pid_t.self, forKey: .pid),
                        window: params().decodeIfPresent(CGWindowID.self, forKey: .window),
                        pixels: params().decodeIfPresent(Int.self, forKey: .pixels),
                    )
                case .faces: .faces
                case .face: try .face(params().decode(String.self, forKey: .name))
            }
    }

    func perform(definitions: ScriptingDefinitions) async -> Result<Response, AggregateError<any Error>> {
        let timeout: Duration = .seconds(60)
        func dictionary(of url: URL) async -> Result<ScriptingDictionary, AggregateError<any Error>> {
            await Result { () async throws(AggregateError<any Error>) in
                try await definitions.dictionary(of: Adobe.installation(at: url).get(), within: timeout).aggregated { error in error as any Error }.get()
            }
        }
        return switch self {
            case .installations:
                await .success(
                    .stream(
                        NSWorkspace.shared.applications(
                            matching: NSPredicate(format: "%K BEGINSWITH %@", kMDItemCFBundleIdentifier as String, Adobe.identifierPrefix),
                            in: FileManager.default.urls(for: .applicationDirectory, in: .allDomainsMask),
                        )
                        .map { listing -> Result<any Encodable & Sendable, AggregateError<any Error>> in .success(Outcome(result: listing.mapError(Encoded.init))) }
                    )
                )
            case .statuses:
                await .success(
                    .stream(
                        NSWorkspace.shared.instanceEvents(of: Adobe.scripted).compactMap { event -> Result<any Encodable & Sendable, AggregateError<any Error>>? in
                            if case .status(let instance, let read) = event {
                                .success(InstanceStatus(pid: instance.processIdentifier, status: Outcome(result: read.mapError(Encoded.init))))
                            } else {
                                nil
                            }
                        }
                    )
                )
            case .launch(let url):
                await Result { () async throws(AggregateError<any Error>) in
                    let installation: Installation = try Adobe.installation(at: url).get()
                    let instance: NSRunningApplication = try await NSWorkspace.shared.launchApplication(at: installation.url, within: timeout).aggregated { error in error as any Error }.get()
                    return Response.value(["pid": instance.processIdentifier])
                }
            case .status(let pid):
                await Result { () async throws(AggregateError<any Error>) in
                    try await Response.value(Adobe.running(pid).get().instance.status().aggregated { error in error as any Error }.get())
                }
            case .press(let pid, let element, let actions):
                await Result { () async throws(AggregateError<any Error>) in
                    let instance: NSRunningApplication = try Adobe.running(pid).get().instance
                    for action: ElementAction in actions { try await instance.perform(action, on: element).aggregated { error in error as any Error }.get() }
                    return Response.value(nil)
                }
            case .terms(let url):
                await dictionary(of: url).map(Response.value)
            case .definition(let url, let kind, let name):
                await dictionary(of: url).flatMap { dictionary in dictionary.text(of: kind, named: name).aggregated { error in error } }.map(Response.value)
            case .execute(let pid, let command, let arguments, let artifacts):
                await Result { () async throws(AggregateError<any Error>) in
                    let installation: Installation = try Adobe.running(pid).get().installation
                    let dictionary: ScriptingDictionary = try await definitions.dictionary(of: installation, within: timeout).aggregated { error in error as any Error }.get()
                    let event: NSAppleEventDescriptor = try dictionary.event(command, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: pid))
                        .mapError { errors in errors.map { error in error as any Error } }
                        .flatMap { event in Adobe.admitted(event, named: command, by: installation.identifier) }
                        .get()
                    let reply: NSAppleEventDescriptor = try await event.sent(within: timeout).aggregated { error in error as any Error }.get()
                    return Response.value(Executed(reply: AppleEventValue(reply, in: dictionary), reading: artifacts))
                }
            case .screenshot(let pid, let window, let pixels):
                await Result { () async throws(AggregateError<any Error>) in
                    let image: CGImage = try await Adobe.running(pid).get().instance.screenshot(of: window, longestSide: pixels).aggregated { error in error as any Error }.get().image
                    return try image.data(as: .png).map { data in Response.value(Captured(image: data, width: image.width, height: image.height)) }.aggregated { error in error as any Error }.get()
                }
            case .faces:
                .success(.value(InstalledFace.installed()))
            case .face(let name):
                InstalledFace.named(name).map(Response.value).aggregated { error in error }
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

        case fail(any Error)
        case die(any Error)

        func encode(to encoder: any Encoder) throws {
            var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            switch self {
                case .fail(let error):
                    try container.encode("Fail", forKey: .tag)
                    try container.encode(Encoded(error), forKey: .error)
                case .die(let error):
                    try container.encode("Die", forKey: .tag)
                    try container.encode(Encoded(error), forKey: .defect)
            }
        }
    }

    private enum Key: String, CodingKey {
        case id, result, chunk, error, data
        case tag = "_tag"
    }

    private static let encoder: JSONEncoder = JSONEncoder()

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
        Result { try Self.encoder.encode(self) }
            .flatMapError { error in Result { try Self.encoder.encode(Self.exit(requestId: requestId, .failure(AggregateError(first: .die(error))))) } }
            .map { data in data + Data("\n".utf8) }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Refusal: CustomNSError, LocalizedError {
    case notAdobe(URL)
    case noProcess(pid_t)
    case unreadable(URL, any Error)
    case interferes(String, code: String, Interference)

    var errorDescription: String? {
        switch self {
            case .notAdobe(let url): "\(url.path(percentEncoded: false)) is no application with an identifier starting \(Adobe.identifierPrefix)"
            case .noProcess(let pid): "No running application bundle has pid \(pid)"
            case .unreadable(let file, let error): "\(file.path(percentEncoded: false)) could not be read: \(error.localizedDescription)"
            case .interferes(let command, let code, .activation): "\(command) (\(code)) activates the application or opens another one, nothing was sent"
            case .interferes(let command, let code, .interface): "\(command) (\(code)) opens a window, dialog, or panel with these arguments, nothing was sent"
            case .interferes(let command, let code, .menuAction): "\(command) (\(code)) runs a menu action, the Menu form runs it, nothing was sent"
            case .interferes(let command, let code, .script): "\(command) (\(code)) runs a script, execute_script runs it, nothing was sent"
        }
    }

    var errorUserInfo: [String: Any] {
        if case .unreadable(_, let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Calls {
    private var running: [Int: Task<Void, Never>] = [:]

    func start(_ id: Int, _ call: @escaping @concurrent @Sendable () async -> Void) {
        running[id] = Task(executorPreference: DispatchQueue(label: "Atelier.requests", attributes: .concurrent)) {
            await call()
            await self.interrupt(id)
        }
    }

    func interrupt(_ id: Int) {
        running.removeValue(forKey: id)?.cancel()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum Adobe {
    static let identifierPrefix: String = "com.adobe."

    static let routes: [String: Interference] = [
        "UIUIInvk": .menuAction, "CAROexec": .menuAction, "K2  dosc": .script, "miscDjxM": .script, "miscdosc": .script, "miscDoAc": .script, "miscfile": .script,
    ]

    static let refusals: [String: [(code: String, clause: Clause, interference: Interference)]] = [
        "com.adobe.InDesign": [
            ("UIUIshow", .always, .activation),
            ("K2LKrvlO", .always, .activation),
            ("K2LKrvlB", .always, .activation),
            ("TeXTSTed", .always, .interface),
            ("K2  TpsV", .always, .interface),
            ("K2  casc", .always, .interface),
            ("K2  tile", .always, .interface),
            ("K2  rstr", .direct(of: ["cwin", "Lwin", "Swin"]), .interface),
            ("aevtpdoc", .parameter("prno", .among(["true"])), .interface),
            ("K2  ePrB", .parameter("BPBD", .among(["true"])), .interface),
            ("CoReclos", .parameter("svop", .among([nil, "ask "])), .interface),
            ("aevtquit", .parameter("savo", .among([nil, "ask "])), .interface),
            ("coresetd", .property("pvis", of: ["CPNS", "pgsp", "cLbp"], .among(["true"])), .interface),
            ("coresetd", .property("UIAc", of: [], .except("eNvr")), .interface),
        ]
            + [
                "K2  expt", "InBKexpt", "InESexpt", "K2PIexpt", "K2XLexpt", "TeXTexpt", "K2  exAs", "InESexAs", "K2PIexAs", "K2XLexAs", "TeXTexAs", "K2PIplac", "InESplac", "TeXTplac",
                "K2  LCDr", "K2  LPGn", "K2PIcopl", "InEScopl", "TeXTcopl", "K2PIplli", "TeXTplli",
            ]
            .map { code in (code, .parameter("imot", .among(["true"])), .interface) },
        "com.adobe.illustratorBeta": [
            ("misckCPk", .always, .interface),
            ("ART5kLWC", .always, .interface),
            ("aevtodoc", .parameter("DLOG", .among([nil, "true"])), .interface),
            ("coresetd", .property("pUIL", of: [], .except("e45a")), .interface),
        ],
        "com.adobe.Photoshop": [
            ("8BIMpsop", .always, .interface),
            ("8BIMptmg", .always, .interface),
            ("PsCSodoc", .parameter("ShwD", .except("Nevr")), .interface),
            ("PsCSclos", .parameter("savo", .among([nil, "ask "])), .interface),
            ("coresetd", .property("AUiL", of: [], .except("Nevr")), .interface),
        ],
        "com.adobe.Acrobat.Pro": [
            ("coresetd", .property("pisf", of: [], .any), .activation),
            ("CARObfrt", .always, .activation),
            ("coreclos", .parameter("savo", .among(["ask "])), .interface),
            ("aevtquit", .parameter("savo", .among(["ask "])), .interface),
            ("coresetd", .property("pskw", of: [], .among(["fals"])), .interface),
        ],
        "com.adobe.AfterEffectsBeta.application": [
            ("aevtpdoc", .parameter("pdlg", .among(["true"])), .interface),
            ("coresetd", .property("pvis", of: [], .among(["true"])), .interface),
            ("coresetd", .property("pmnd", of: [], .among(["fals"])), .interface),
        ],
        "com.adobe.PremiereProBeta.27": [
            ("CR8RCPAE", .always, .interface),
            ("CR8RCRPr", .always, .activation),
        ],
    ]

    static func installation(at url: URL) -> Result<Installation, AggregateError<any Error>> {
        Installation.at(url)
            .aggregated { error in error }
            .flatMap { installation in installation.identifier.hasPrefix(identifierPrefix) ? .success(installation) : .failure(AggregateError(first: Refusal.notAdobe(url))) }
    }

    static func running(_ pid: pid_t) -> Result<(instance: NSRunningApplication, installation: Installation), AggregateError<any Error>> {
        switch NSRunningApplication(processIdentifier: pid).flatMap({ instance in instance.bundleURL.map { url in (instance, url) } }) {
            case .some((let instance, let url)): installation(at: url).map { installation in (instance, installation) }
            case .none: .failure(AggregateError(first: Refusal.noProcess(pid)))
        }
    }

    static func scripted(_ instance: NSRunningApplication) -> Bool {
        switch instance.bundleURL.map(installation(at:)) {
            case .some(.success(let installation)): installation.scriptable
            case .some(.failure), .none: false
        }
    }

    static func admitted(_ event: NSAppleEventDescriptor, named command: String, by identifier: String) -> Result<NSAppleEventDescriptor, AggregateError<any Error>> {
        let code: String = (UInt64(event.eventClass) << 32 | UInt64(event.eventID)).sdef
        let interference: Interference? = routes[code] ?? refusals[identifier, default: []].first { row in row.code == code && row.clause.refuses(event) }?.interference
        return interference.map { interference in .failure(AggregateError(first: Refusal.interferes(command, code: code, interference))) } ?? .success(event)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Atelier {
    static func main() async {
        let logger: Logger = Logger(category: "Atelier")
        let (lines, sink): (AsyncStream<Data>, AsyncStream<Data>.Continuation) = AsyncStream.makeStream()
        let calls: Calls = Calls()
        let definitions: ScriptingDefinitions = ScriptingDefinitions()
        let decoder: JSONDecoder = JSONDecoder()
        let send: @Sendable (FromServer) -> Void = { message in
            switch message.line() {
                case .success(let data): sink.yield(data)
                case .failure(let error): logger.debug("Dropped a reply no line can carry: \(String(describing: error), privacy: .public)")
            }
        }
        @concurrent @Sendable func answer(_ id: Int, _ request: Result<Request, any Error>) async {
            let outcome: Result<Response, AggregateError<FromServer.Reason>> = await Result { () async throws(AggregateError<FromServer.Reason>) in
                try await request.aggregated(FromServer.Reason.die).get().perform(definitions: definitions).mapError { failures in failures.map(FromServer.Reason.fail) }.get()
            }
            switch outcome {
                case .success(.value(let value)): send(.exit(requestId: id, .success(value)))
                case .success(.stream(let chunks)):
                    for await chunk: Result<any Encodable & Sendable, AggregateError<any Error>> in chunks {
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
            AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), 1)
            AXIsProcessTrustedWithOptions(["AXTrustedCheckOptionPrompt": true] as CFDictionary)
            try await withThrowingDiscardingTaskGroup { group in
                group.addTask(executorPreference: DispatchQueue(label: "Atelier.replies", attributes: .concurrent)) {
                    for await line: Data in lines { try FileHandle.standardOutput.write(contentsOf: line) }
                }
                var line: Data = Data()
                for try await byte: UInt8 in FileHandle.standardInput.bytes {
                    line.append(byte)
                    guard byte == UInt8(ascii: "\n") else { continue }
                    defer { line = Data() }
                    switch Result(catching: { try decoder.decode(FromClient.self, from: line) }) {
                        case .success(.request(let id, let request)): await calls.start(id) { await answer(id, request) }
                        case .success(.interrupt(let id)): await calls.interrupt(id)
                        case .success(.ack): break
                        case .failure(let error): logger.debug("Dropped a line no reply can match: \(String(describing: error), privacy: .public)")
                    }
                }
                sink.finish()
            }
        } catch {
            logger.fault("Stopped reading requests: \(String(describing: error), privacy: .public)")
            exit(EXIT_FAILURE)
        }
    }
}
