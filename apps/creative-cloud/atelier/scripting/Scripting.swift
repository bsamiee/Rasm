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

enum Request: Decodable {
    case applications(running: Bool)
    case application(url: URL)
    case definition(application: URL, selection: ScriptingDictionary.Selection?)
    case terms(application: URL, kind: String?)
    case execute(application: URL, command: String, arguments: [String: JSONValue])
    case contentType(url: URL)
    case screenshot(application: URL, window: CGWindowID?)
    case faces

    private enum Method: String, Decodable { case applications, application, definition, terms, execute, contentType, screenshot, faces }
    private enum Key: CodingKey { case method, params, running, application, command, arguments, url, window, selection, kind }

    init(from decoder: any Decoder) throws {
        let call: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let params: () throws -> KeyedDecodingContainer<Key> = { try call.nestedContainer(keyedBy: Key.self, forKey: .params) }
        self =
            switch try call.decode(Method.self, forKey: .method) {
                case .applications: try .applications(running: params().decode(Bool.self, forKey: .running))
                case .application: try .application(url: params().decode(URL.self, forKey: .url))
                case .definition:
                    try .definition(application: params().decode(URL.self, forKey: .application), selection: params().decodeIfPresent(ScriptingDictionary.Selection.self, forKey: .selection))
                case .terms: try .terms(application: params().decode(URL.self, forKey: .application), kind: params().decodeIfPresent(String.self, forKey: .kind))
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

    func perform() async -> Result<any Encodable, AggregateError<Failure>> {
        switch self {
            case .applications(let running):
                Application.all(running: running).map(\.self)
            case .application(let url):
                Application.at(url).map(\.self)
            case .definition(let url, let selection):
                await Application.at(url).bind(ScriptingDictionary.definition(of:)).flatMap { document in ScriptingDictionary.definition(document, selecting: selection) }.map(\.self)
            case .terms(let url, let kind):
                await Application.at(url).bind(ScriptingDictionary.definition(of:)).flatMap { document in ScriptingDictionary.terms(document, kind: kind) }.map(\.self)
            case .execute(let url, let command, let arguments):
                await Application.at(url).bind { application in await application.execute(command, arguments: arguments) }.map(\.self)
            case .contentType(let url):
                Result { try url.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType ?? "application/octet-stream" }
                    .mapError { error in AggregateError(first: .unreadable(error)) }
                    .map(\.self)
            case .screenshot(let url, let window):
                await Application.at(url).bind { application in await application.open() }.bind { application in
                    await application.screenshot(windowID: window)
                        .map { result in Screenshot(window: result.window, windows: result.windows, image: result.image) }
                        .mapError { error in AggregateError(first: .unreadable(error)) }
                }.map(\.self)
            case .faces:
                .success(Face.installed())
        }
    }
}

struct Call: Decodable {
    private enum Key: CodingKey { case id }

    let id: Int
    let request: Result<Request, Answer.Fault>

    init(from decoder: any Decoder) throws {
        id = try decoder.container(keyedBy: Key.self).decode(Int.self, forKey: .id)
        request = Result { try Request(from: decoder) }.mapError(Answer.Fault.invalid)
    }

    func reply() async -> Result<Data, any Error> {
        let outcome: Result<any Encodable, Answer.Fault> = await request.bind { request in await request.perform().mapError(Answer.Fault.refused) }
        return Result { try JSONEncoder().encode(Answer(id: id, outcome: outcome)) }
            .flatMapError { error in Result { try JSONEncoder().encode(Answer(id: id, outcome: .failure(.unencodable(error)))) } }
    }
}

struct Answer: Encodable {
    enum Fault: Error {
        case invalid(any Error)
        case refused(AggregateError<Failure>)
        case unencodable(any Error)
    }

    private enum Key: String, CodingKey {
        case jsonrpc, id, result, error, code, message, failures
        case tag = "_tag"
    }

    let id: Int
    let outcome: Result<any Encodable, Fault>

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode("2.0", forKey: .jsonrpc)
        try container.encode(id, forKey: .id)
        switch outcome {
            case .success(let value):
                try container.encode(value, forKey: .result)
            case .failure(let fault):
                var error: KeyedEncodingContainer<Key> = container.nestedContainer(keyedBy: Key.self, forKey: .error)
                let (code, message, failures): (Int, String, [String]?) =
                    switch fault {
                        case .invalid(let cause): (-32_600, String(describing: cause), nil)
                        case .refused(let failures): (1, "Refused", failures.errors.map(\.localizedDescription))
                        case .unencodable(let cause): (-32_603, String(describing: cause), nil)
                    }
                try error.encode(code, forKey: .code)
                try error.encode(message, forKey: .message)
                if let failures {
                    try error.encode("ScriptingError", forKey: .tag)
                    try error.encode(failures, forKey: .failures)
                }
        }
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
        do {
            try takeResponsibility()
            _ = NSApplication.shared
            try await withThrowingDiscardingTaskGroup { group in
                group.addTask {
                    for await reply: Data in replies { try FileHandle.standardOutput.write(contentsOf: reply + Data("\n".utf8)) }
                }
                try await withThrowingDiscardingTaskGroup { requests in
                    for try await line: String in FileHandle.standardInput.bytes.lines {
                        requests.addTask(executorPreference: DispatchQueue.global()) {
                            switch await Result(catching: { try JSONDecoder().decode(Call.self, from: Data(line.utf8)) }).bind({ call in await call.reply() }) {
                                case .success(let reply): sink.yield(reply)
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
