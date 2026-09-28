import Foundation
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

struct File: Encodable {
    let uri: URL
    let mimeType: String?
    let blob: Data

    init(url: URL) throws {
        guard url.isFileURL else { throw CocoaError(.fileReadUnsupportedScheme, userInfo: [NSURLErrorKey: url]) }
        uri = url
        mimeType = try url.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType
        blob = try Data(contentsOf: url)
    }
}

enum Request: Decodable {
    case applications
    case dictionary(application: Application)
    case execute(application: Application, command: String, arguments: [String: Descriptor])
    case file(url: URL)

    func perform() -> Result<any Encodable, Failure> {
        switch self {
            case .applications: Result { try Application.all() }.mapError(Failure.request)
            case .dictionary(let application):
                Result { try ScriptingDictionary.definition(of: application.url) }
                    .flatMap { definition in String(bytes: definition, encoding: .utf8).map(Result.success) ?? .failure(CocoaError(.fileReadInapplicableStringEncoding)) }
                    .mapError(Failure.request)
            case .execute(let application, let command, let arguments): application.execute(command, arguments: arguments)
            case .file(let url): Result { try File(url: url) }.mapError(Failure.request)
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: Error, Encodable {
    case request(any Error)
    case send(any Error)
    case reply(any Error, Descriptor?)

    enum CodingKeys: String, CodingKey {
        case tag = "_tag"
        case stage, domain, code, message, reply
    }

    func encode(to encoder: any Encoder) throws {
        let (stage, error, reply): (String, any Error, Descriptor?) =
            switch self {
                case .request(let error): ("request", error, nil)
                case .send(let error): ("send", error, nil)
                case .reply(let error, let reply): ("reply", error, reply)
            }
        let message: String =
            switch error {
                case DecodingError.dataCorrupted(let context), DecodingError.keyNotFound(_, let context), DecodingError.typeMismatch(_, let context), DecodingError.valueNotFound(_, let context):
                    context.debugDescription
                default:
                    error.localizedDescription
            }
        var container: KeyedEncodingContainer<CodingKeys> = encoder.container(keyedBy: CodingKeys.self)
        try container.encode("nativeError", forKey: .tag)
        try container.encode(stage, forKey: .stage)
        try container.encode((error as NSError).domain, forKey: .domain)
        try container.encode((error as NSError).code, forKey: .code)
        try container.encode(message, forKey: .message)
        try container.encodeIfPresent(reply, forKey: .reply)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Scripting {
    static func main() throws {
        let reply: Result<any Encodable, Failure> = Result { try JSONDecoder().decode(Request.self, from: FileHandle.standardInput.readToEnd() ?? Data()) }
            .mapError(Failure.request)
            .flatMap { request in request.perform() }
        switch reply {
            case .success(let value):
                try FileHandle.standardOutput.write(contentsOf: JSONEncoder().encode(value))
            case .failure(let failure):
                try FileHandle.standardOutput.write(contentsOf: JSONEncoder().encode(failure))
                exit(EXIT_FAILURE)
        }
    }
}
