public import AppKit
import Files
import Results

// --- [MODELS] --------------------------------------------------------------------------

private struct InfoFlag: Decodable {
    let value: Bool

    init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        do { value = try container.decode(Bool.self) } catch DecodingError.typeMismatch {
            do { value = try container.decode(Double.self) != 0 } catch DecodingError.typeMismatch { value = try NSString(string: container.decode(String.self)).boolValue }
        }
    }
}

private struct BundleInfo: Decodable {
    private enum CodingKeys: String, CodingKey {
        case identifier = "CFBundleIdentifier"
        case version = "CFBundleShortVersionString"
        case build = "CFBundleVersion"
        case scriptingDefinitionFile = "OSAScriptingDefinition"
        case agent = "LSUIElement"
    }

    let identifier: String?
    let version: Installation.Version?
    let build: Installation.Version?
    let scriptingDefinitionFile: String?
    let agent: InfoFlag?
}

public struct Installation: Encodable, Hashable, Sendable {
    public struct Version: RawRepresentable, Codable, Hashable, Comparable, Sendable {
        public let rawValue: String

        private var components: [Int] {
            let numbers: [Int] = rawValue.split(separator: ".").map { (component: Substring) -> Int in Int(component.prefix(while: \.isNumber)) ?? 0 }
            return Array(numbers.reversed().drop { (component: Int) -> Bool in component == 0 }.reversed())
        }

        public init(rawValue: String) { self.rawValue = rawValue }

        public static func == (lhs: Self, rhs: Self) -> Bool { lhs.components == rhs.components }

        public static func < (lhs: Self, rhs: Self) -> Bool { lhs.components.lexicographicallyPrecedes(rhs.components) }

        public func hash(into hasher: inout Hasher) { hasher.combine(components) }
    }

    private enum CodingKeys: CodingKey {
        case url
        case identifier
        case name
        case version
        case build
        case scriptable
        case scriptingDefinitionFile
        case agent
    }

    public let url: URL
    public let identifier: String
    public let name: String
    public let version: Version?
    public let build: Version?
    public let scriptable: Bool
    public let scriptingDefinitionFile: String?
    public let agent: Bool
}

extension Installation {
    public init(at url: URL) throws(ApplicationFailure) {
        let bundle: URL = url.resolvingSymlinksInPath()
        let path: String = bundle.path(percentEncoded: false)
        let (info, values): (BundleInfo?, URLResourceValues)
        do {
            (info, values) = try (
                ifPresent { try Data(contentsOf: bundle.appending(components: "Contents", "Info.plist")) }.map { plist in try PropertyListDecoder().decode(BundleInfo.self, from: plist) },
                bundle.resourceValues(forKeys: [.applicationIsScriptableKey]),
            )
        } catch {
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
        guard let identifier: String = info?.identifier else { throw ApplicationFailure(subject: path, reason: .noIdentifier) }
        self.init(
            url: bundle,
            identifier: identifier,
            name: FileManager.default.displayName(atPath: path),
            version: info?.version,
            build: info?.build,
            scriptable: values.applicationIsScriptable == true,
            scriptingDefinitionFile: info?.scriptingDefinitionFile,
            agent: info?.agent?.value == true,
        )
    }

    public static func newest(of name: String, among installations: [Self]) throws(ApplicationFailure) -> Self {
        let version: Version? = installations.compactMap(\.version).max()
        return try Unmatched.single(
            installations.filter { installation in installation.version == version },
            subject: "newest installation",
            query: name,
            candidates: [],
        ) { installation in installation.url.path(percentEncoded: false) }
        .mapError { miss in ApplicationFailure(subject: name, reason: .unmatched(miss)) }.get()
    }

    public func runs(_ instance: NSRunningApplication) -> Bool {
        instance.bundleURL?.resolvingSymlinksInPath().pathComponents == url.pathComponents
    }
}
