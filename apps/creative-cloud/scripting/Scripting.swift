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
    case dictionary(application: URL)
    case execute(application: URL, command: String, arguments: [String: Descriptor])
    case file(url: URL)

    func perform() -> Result<any Encodable, Failures> {
        switch self {
            case .applications:
                Application.all().map(\.self)
            case .dictionary(let url):
                Application.at(url)
                    .flatMap(ScriptingDictionary.definition(of:))
                    .flatMap { definition in
                        String(bytes: definition, encoding: .utf8).map(Result.success) ?? .failure(Failures(.unreadable(code: CocoaError.fileReadInapplicableStringEncoding.rawValue)))
                    }
            case .execute(let url, let command, let arguments):
                Application.at(url).flatMap { application in application.execute(command, arguments: arguments) }.map(\.self)
            case .file(let url):
                Result { try File(url: url) }.mapError { error in Failures(.unreadable(code: (error as NSError).code)) }.map(\.self)
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum Scripting {
    static func main() throws {
        switch try JSONDecoder().decode(Request.self, from: FileHandle.standardInput.readToEnd() ?? Data()).perform() {
            case .success(let value):
                try FileHandle.standardOutput.write(contentsOf: JSONEncoder().encode(value))
            case .failure(let failures):
                try FileHandle.standardOutput.write(contentsOf: JSONEncoder().encode([failures.first] + failures.remaining))
                exit(EXIT_FAILURE)
        }
    }
}
