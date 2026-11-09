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
        case backgroundOnly = "LSBackgroundOnly"
    }

    let identifier: String?
    let version: Installation.Version?
    let build: Installation.Version?
    let scriptingDefinitionFile: String?
    let agent: InfoFlag?
    let backgroundOnly: InfoFlag?

    var activationPolicy: NSApplication.ActivationPolicy {
        switch (backgroundOnly?.value, agent?.value) {
            case (true, _): .prohibited
            case (_, true): .accessory
            default: .regular
        }
    }
}

public struct Installation: Encodable, Hashable, Sendable {
    public struct Version: RawRepresentable, Codable, Hashable, Comparable, Sendable {
        public let rawValue: String

        private var parts: (release: [Int], prerelease: Substring?) {
            let end: String.Index = rawValue.firstIndex { character in !character.isASCII || !(character.isNumber || character == ".") } ?? rawValue.endIndex
            let numbers: [Int] = rawValue[..<end].split(separator: ".").compactMap { component in Int(component) }
            let suffix: Substring = rawValue[end...].drop { character in character == "-" }
            return (Array(numbers.reversed().drop { component in component == 0 }.reversed()), suffix.isEmpty ? nil : suffix)
        }

        public init(rawValue: String) { self.rawValue = rawValue }

        public static func == (lhs: Self, rhs: Self) -> Bool { lhs.parts == rhs.parts }

        public static func < (lhs: Self, rhs: Self) -> Bool {
            switch (lhs.parts, rhs.parts) {
                case (let left, let right) where left.release != right.release: left.release.lexicographicallyPrecedes(right.release)
                case ((_, .some(let left)), (_, .some(let right))): left.compare(right, options: .numeric) == .orderedAscending
                case ((_, .some), (_, .none)): true
                case ((_, .none), _): false
            }
        }

        public func hash(into hasher: inout Hasher) {
            hasher.combine(parts.release)
            hasher.combine(parts.prerelease)
        }
    }

    private enum CodingKeys: CodingKey {
        case url
        case executable
        case identifier
        case name
        case version
        case build
        case scriptable
        case scriptingDefinitionFile
        case activationPolicy
    }

    public let url: URL
    public let executable: URL?
    public let identifier: String
    public let name: String
    public let version: Version?
    public let build: Version?
    public let scriptable: Bool
    public let scriptingDefinitionFile: String?
    public let activationPolicy: NSApplication.ActivationPolicy

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<CodingKeys> = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(url, forKey: .url)
        try fields.encodeIfPresent(executable, forKey: .executable)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encode(name, forKey: .name)
        try fields.encodeIfPresent(version, forKey: .version)
        try fields.encodeIfPresent(build, forKey: .build)
        try fields.encode(scriptable, forKey: .scriptable)
        try fields.encodeIfPresent(scriptingDefinitionFile, forKey: .scriptingDefinitionFile)
        try fields.encode(activationPolicy.name, forKey: .activationPolicy)
    }
}

extension NSApplication.ActivationPolicy {
    public var name: String {
        switch self {
            case .regular: "regular"
            case .accessory: "accessory"
            case .prohibited: "prohibited"
            @unknown default: String(rawValue)
        }
    }
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
            executable: Bundle(url: bundle)?.executableURL,
            identifier: identifier,
            name: FileManager.default.displayName(atPath: path),
            version: info?.version,
            build: info?.build,
            scriptable: values.applicationIsScriptable == true,
            scriptingDefinitionFile: info?.scriptingDefinitionFile,
            activationPolicy: info?.activationPolicy ?? .regular,
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
