import AppKit
import CoreText
import Foundation
import RasmMacOS
import System
import UniformTypeIdentifiers
import os

// --- [MODELS] --------------------------------------------------------------------------

struct Face: Encodable {
    let postscriptName: String
    let family: String
    let style: String
    let fullName: String
    let file: String

    static func installed() -> [Self] {
        let collection: CTFontCollection = CTFontCollectionCreateFromAvailableFonts([kCTFontCollectionRemoveDuplicatesOption as String: true] as CFDictionary)
        return (CTFontCollectionCreateMatchingFontDescriptors(collection) as? [CTFontDescriptor] ?? []).compactMap { descriptor in
            switch (
                CTFontDescriptorCopyAttribute(descriptor, kCTFontNameAttribute) as? String, CTFontDescriptorCopyAttribute(descriptor, kCTFontFamilyNameAttribute) as? String,
                CTFontDescriptorCopyAttribute(descriptor, kCTFontStyleNameAttribute) as? String, CTFontDescriptorCopyAttribute(descriptor, kCTFontDisplayNameAttribute) as? String,
                CTFontDescriptorCopyAttribute(descriptor, kCTFontURLAttribute) as? URL,
            ) {
                case (.some(let postscriptName), .some(let family), .some(let style), .some(let fullName), .some(let file)):
                    Self(postscriptName: postscriptName, family: family, style: style, fullName: fullName, file: file.path(percentEncoded: false))
                default: nil
            }
        }
    }
}

enum Reply {
    case value(any Encodable)
    case stream(AsyncStream<Status>)
}

enum Request: Decodable {
    case installations
    case launch(url: URL)
    case status(url: URL)
    case watch(url: URL)
    case press(url: URL, element: CGWindowID, button: String)
    case definition(application: URL, selection: ScriptingDictionary.Selection?)
    case terms(application: URL, kind: String)
    case execute(application: URL, command: String, arguments: [String: JSONValue])
    case contentType(url: URL)
    case screenshot(application: URL, window: CGWindowID?)
    case faces

    private enum Method: String, Decodable { case installations, launch, status, watch, press, definition, terms, execute, contentType, screenshot, faces }
    private enum Key: CodingKey { case method, params, application, command, arguments, url, window, selection, kind, element, button }

    init(from decoder: any Decoder) throws {
        let call: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let params: () throws -> KeyedDecodingContainer<Key> = { try call.nestedContainer(keyedBy: Key.self, forKey: .params) }
        self =
            switch try call.decode(Method.self, forKey: .method) {
                case .installations: .installations
                case .launch: try .launch(url: params().decode(URL.self, forKey: .url))
                case .status: try .status(url: params().decode(URL.self, forKey: .url))
                case .watch: try .watch(url: params().decode(URL.self, forKey: .url))
                case .press:
                    try .press(url: params().decode(URL.self, forKey: .url), element: params().decode(CGWindowID.self, forKey: .element), button: params().decode(String.self, forKey: .button))
                case .definition:
                    try .definition(application: params().decode(URL.self, forKey: .application), selection: params().decodeIfPresent(ScriptingDictionary.Selection.self, forKey: .selection))
                case .terms: try .terms(application: params().decode(URL.self, forKey: .application), kind: params().decode(String.self, forKey: .kind))
                case .execute:
                    try .execute(
                        application: params().decode(URL.self, forKey: .application),
                        command: params().decode(String.self, forKey: .command),
                        arguments: params().decode([String: JSONValue].self, forKey: .arguments),
                    )
                case .contentType: try .contentType(url: params().decode(URL.self, forKey: .url))
                case .screenshot: try .screenshot(application: params().decode(URL.self, forKey: .application), window: params().decodeIfPresent(CGWindowID.self, forKey: .window))
                case .faces: .faces
            }
    }

    func perform() async -> Result<Reply, AggregateError<Failure>> {
        switch self {
            case .installations:
                Application.all().map(Reply.value)
            case .launch(let url):
                await Application.at(url).bind { application in await application.launch() }.map(Reply.value)
            case .status(let url):
                Application.at(url).flatMap { application in application.status() }.map(Reply.value)
            case .watch(let url):
                await Application.at(url).bind { application in await application.watch() }.map(Reply.stream)
            case .press(let url, let element, let button):
                Application.at(url).flatMap { application in application.press(button, on: element) }.map(Reply.value)
            case .definition(let url, let selection):
                await Application.at(url).bind(ScriptingDictionary.definition(of:)).flatMap { document in ScriptingDictionary.definition(document, selecting: selection) }.map(Reply.value)
            case .terms(let url, let kind):
                await Application.at(url).bind(ScriptingDictionary.definition(of:)).flatMap { document in ScriptingDictionary.terms(document, kind: kind) }.map(Reply.value)
            case .execute(let url, let command, let arguments):
                await Application.at(url).bind { application in await application.execute(command, arguments: arguments) }.map(Reply.value)
            case .contentType(let url):
                Result { try url.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType ?? "application/octet-stream" }
                    .mapError { error in AggregateError(first: .unreadable(error)) }
                    .map(Reply.value)
            case .screenshot(let url, let window):
                await Application.at(url).bind { application in await application.open() }.bind { application in
                    await application.screenshot(windowID: window)
                        .map { result in Screenshot(window: result.window, windows: result.windows, image: result.image) }
                        .mapError { error in AggregateError(first: .unreadable(error)) }
                }.map(Reply.value)
            case .faces:
                .success(.value(Face.installed()))
        }
    }
}

enum Message: Decodable {
    case call(id: Int, request: Result<Request, Answer.Fault>)
    case interrupt(id: Int)
    case other

    private enum Key: CodingKey { case id, method, params, requestId }

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        self =
            switch (try container.decodeIfPresent(Int.self, forKey: .id), try container.decode(String.self, forKey: .method)) {
                case (.some(let id), _): .call(id: id, request: Result { try Request(from: decoder) }.mapError(Answer.Fault.invalid))
                case (.none, "@effect/rpc/Interrupt"): try .interrupt(id: container.nestedContainer(keyedBy: Key.self, forKey: .params).decode(Int.self, forKey: .requestId))
                case (.none, _): .other
            }
    }
}

struct Answer: Encodable {
    enum Fault: Error {
        case invalid(any Error)
        case refused(AggregateError<Failure>)
        case unencodable(any Error)
    }

    enum Outcome {
        case result(any Encodable)
        case chunk(any Encodable)
        case end
        case fault(Fault)
    }

    private enum Key: String, CodingKey {
        case jsonrpc, id, result, chunk, error, code, message, data, defect
        case tag = "_tag"
    }

    let id: Int
    let outcome: Outcome

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode("2.0", forKey: .jsonrpc)
        try container.encode(id, forKey: .id)
        switch outcome {
            case .result(let value):
                try container.encode(value, forKey: .result)
            case .chunk(let value):
                try container.encode(true, forKey: .chunk)
                var values: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer(forKey: .result)
                try values.encode(value)
            case .end:
                break
            case .fault(let fault):
                let (code, tag, field, messages): (Int, String, Key, AggregateError<String>) =
                    switch fault {
                        case .invalid(let cause): (-32_600, "Die", .defect, AggregateError(first: String(describing: cause)))
                        case .refused(let failures): (0, "Fail", .error, failures.map(\.localizedDescription))
                        case .unencodable(let cause): (-32_603, "Die", .defect, AggregateError(first: String(describing: cause)))
                    }
                var error: KeyedEncodingContainer<Key> = container.nestedContainer(keyedBy: Key.self, forKey: .error)
                try error.encode("Cause", forKey: .tag)
                try error.encode(code, forKey: .code)
                try error.encode(messages.first, forKey: .message)
                var reasons: any UnkeyedEncodingContainer = error.nestedUnkeyedContainer(forKey: .data)
                for message: String in messages.errors {
                    var reason: KeyedEncodingContainer<Key> = reasons.nestedContainer(keyedBy: Key.self)
                    try reason.encode(tag, forKey: .tag)
                    var value: KeyedEncodingContainer<Key> = reason.nestedContainer(keyedBy: Key.self, forKey: field)
                    try value.encode(message, forKey: .message)
                }
        }
    }

    func line() -> Result<Data, any Error> {
        Result { try JSONEncoder().encode(self) }
            .flatMapError { error in Result { try JSONEncoder().encode(Self(id: id, outcome: .fault(.unencodable(error)))) } }
            .map { data in data + Data("\n".utf8) }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Streams {
    private var running: [Int: Task<Void, Never>] = [:]

    func start(_ id: Int, _ work: @escaping @Sendable () async -> Void) {
        running[id] = Task {
            await work()
        }
    }

    func end(_ id: Int) {
        running.removeValue(forKey: id)?.cancel()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("responsibility_get_responsible_for_pid")
private func responsibility(_ pid: pid_t, _: UnsafeMutablePointer<pid_t>?, _: UnsafeMutablePointer<UInt64>?, _ length: UnsafeMutablePointer<Int>, _ path: UnsafeMutablePointer<CChar>) -> Int32
@_silgen_name("responsibility_spawnattrs_setdisclaim")
private func disclaim(_ attributes: UnsafeMutablePointer<posix_spawnattr_t?>, _ disclaim: Int32) -> Int32

private func takeResponsibility() throws(Errno) {
    var executable: [CChar] = Array(repeating: 0, count: Int(MAXPATHLEN))
    var responsible: [CChar] = executable
    var length: Int = responsible.count
    guard unsafe proc_pidpath(getpid(), &executable, UInt32(executable.count)) > 0, unsafe responsibility(getpid(), nil, nil, &length, &responsible) == 0, unsafe strcmp(executable, responsible) != 0
    else { return }
    var attributes: posix_spawnattr_t?
    func check(_ status: Int32) throws(Errno) { if status != 0 { throw Errno(rawValue: status) } }
    try check(unsafe posix_spawnattr_init(&attributes))
    try check(unsafe disclaim(&attributes, 1))
    try check(unsafe posix_spawnattr_setflags(&attributes, Int16(POSIX_SPAWN_SETEXEC)))
    try check(unsafe posix_spawn(nil, executable, nil, &attributes, CommandLine.unsafeArgv, environ))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Scripting {
    static func main() async {
        let logger: Logger = Logger(subsystem: Bundle.identity, category: "Scripting")
        let (replies, sink): (AsyncStream<Data>, AsyncStream<Data>.Continuation) = AsyncStream.makeStream()
        let streams: Streams = Streams()
        let emit: @Sendable (Answer) -> Void = { answer in
            switch answer.line() {
                case .success(let data): sink.yield(data)
                case .failure(let error): logger.debug("Dropped a reply no line can carry: \(String(describing: error), privacy: .public)")
            }
        }
        do {
            try takeResponsibility()
            _ = NSApplication.shared
            try await withThrowingDiscardingTaskGroup { group in
                group.addTask {
                    for await reply: Data in replies { try FileHandle.standardOutput.write(contentsOf: reply) }
                }
                try await withThrowingDiscardingTaskGroup { requests in
                    for try await line: String in FileHandle.standardInput.bytes.lines {
                        requests.addTask(executorPreference: DispatchQueue.global()) {
                            switch Result(catching: { try JSONDecoder().decode(Message.self, from: Data(line.utf8)) }) {
                                case .success(.call(let id, let request)):
                                    switch await request.bind({ request in await request.perform().mapError(Answer.Fault.refused) }) {
                                        case .success(.value(let value)): emit(Answer(id: id, outcome: .result(value)))
                                        case .success(.stream(let statuses)):
                                            await streams.start(id) {
                                                for await status: Status in statuses { emit(Answer(id: id, outcome: .chunk(status))) }
                                                emit(Answer(id: id, outcome: .end))
                                                await streams.end(id)
                                            }
                                        case .failure(let fault): emit(Answer(id: id, outcome: .fault(fault)))
                                    }
                                case .success(.interrupt(let id)): await streams.end(id)
                                case .success(.other): break
                                case .failure(let error): logger.debug("Dropped a line no reply can match: \(String(describing: error), privacy: .public)")
                            }
                        }
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
