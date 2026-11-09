public import AppleEvents
public import Foundation
public import EffectRPC
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct Artifact: Encodable, Sendable {
    public let uri: URL
    public let mimeType: String?
    public let data: Data
}

public struct Executed: Encodable, Sendable {
    public let reply: AppleEventValue
    public let artifacts: [Outcome<Artifact>]

    public init(reply: AppleEventValue, reading files: [URL]) {
        self.reply = reply
        artifacts = files.map { file in
            Outcome(Result { try Artifact(uri: file, mimeType: file.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, data: Data(contentsOf: file)) })
        }
    }
}
