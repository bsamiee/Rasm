public import AppleEvents
public import Foundation
public import JSONRPC
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct Artifact: Encodable, Sendable {
    public let uri: URL
    public let mimeType: String?
    public let data: Data
    public let width: Int?
    public let height: Int?

    init(reading file: URL, width: Int? = nil, height: Int? = nil) throws {
        (uri, mimeType, data, self.width, self.height) = (file, try file.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, try Data(contentsOf: file), width, height)
    }
}

public struct Executed: Encodable, Sendable {
    public let reply: AppleEventValue
    public let artifacts: [Outcome<Artifact>]

    public init(reply: AppleEventValue, reading files: [URL]) {
        self.reply = reply
        artifacts = files.map { file in Outcome(Result { try Artifact(reading: file) }) }
    }
}
