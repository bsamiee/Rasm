public import AppKit
import Files
import Results

// --- [MODELS] --------------------------------------------------------------------------

private struct InfoFlag: Decodable {
    let value: Bool

    init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        value = try Result { try container.decode(Bool.self) }
            .flatMapError { error in if case DecodingError.typeMismatch = error { Result { try NSString(string: container.decode(String.self)).boolValue } } else { .failure(error) } }
            .get()
    }
}

private struct BundleInfo: Decodable {
    private enum CodingKeys: String, CodingKey {
        case identifier = "CFBundleIdentifier"
        case version = "CFBundleShortVersionString"
        case scriptingDefinitionFile = "OSAScriptingDefinition"
        case agent = "LSUIElement"
    }

    let identifier: String?
    let version: String?
    let scriptingDefinitionFile: String?
    let agent: InfoFlag?
}

public struct Installation: Encodable, Hashable, Sendable {
    public let url: URL
    public let identifier: String
    public let name: String
    public let version: String?
    public let scriptable: Bool
    public let scriptingDefinitionFile: String?
    public let agent: Bool
}

extension Installation {
    public init(at url: URL) throws(ApplicationFailure) {
        let bundle: URL = url.resolvingSymlinksInPath()
        let path: String = bundle.path(percentEncoded: false)
        let (info, values): (BundleInfo?, URLResourceValues) = try Result {
            try (
                ifPresent { try Data(contentsOf: bundle.appending(components: "Contents", "Info.plist")) }.map { plist in try PropertyListDecoder().decode(BundleInfo.self, from: plist) },
                bundle.resourceValues(forKeys: [.applicationIsScriptableKey]),
            )
        }
        .mapError { error in ApplicationFailure(subject: path, reason: .host(error)) }.get()
        guard let identifier: String = info?.identifier else { throw ApplicationFailure(subject: path, reason: .absent("CFBundleIdentifier")) }
        self.init(
            url: bundle,
            identifier: identifier,
            name: FileManager.default.displayName(atPath: path),
            version: info?.version,
            scriptable: values.applicationIsScriptable == true,
            scriptingDefinitionFile: info?.scriptingDefinitionFile,
            agent: info?.agent?.value == true,
        )
    }

    public static func single(of identifier: String, among installations: [Self]) throws(ApplicationFailure) -> Self {
        try Unmatched.single(
            installations.filter { installation in installation.identifier == identifier },
            subject: "installation of \(identifier)",
            query: nil,
            candidates: [],
        ) { installation in installation.url.path(percentEncoded: false) }
        .mapError { miss in ApplicationFailure(subject: identifier, reason: .unmatched(miss)) }.get()
    }

    public func runs(_ instance: NSRunningApplication) -> Bool {
        instance.bundleURL?.resolvingSymlinksInPath().path(percentEncoded: false) == url.path(percentEncoded: false)
    }
}
