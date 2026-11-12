public import AppKit
import Files

// --- [MODELS] --------------------------------------------------------------------------

private struct InfoFlag: Decodable {
    let value: Bool

    init(from decoder: any Decoder) throws {
        let container = try decoder.singleValueContainer()
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
    let version: String?
    let build: String?
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
    public let url: URL
    public let executable: URL?
    public let identifier: String
    public let name: String
    public let version: String?
    public let build: String?
    public let scriptable: Bool
    public let scriptingDefinitionFile: String?
    public let activationPolicy: NSApplication.ActivationPolicy
}

extension NSApplication.ActivationPolicy: @retroactive Encodable {
    public var name: String {
        switch self {
            case .regular: "regular"
            case .accessory: "accessory"
            case .prohibited: "prohibited"
            @unknown default: String(rawValue)
        }
    }

    public func encode(to encoder: any Encoder) throws {
        var label = encoder.singleValueContainer()
        try label.encode(name)
    }
}

extension Installation {
    public init(at url: URL) throws(ApplicationFailure) {
        let bundle = url.resolvingSymlinksInPath()
        let path = bundle.path(percentEncoded: false)
        let (info, values): (BundleInfo?, URLResourceValues)
        do {
            (info, values) = try (
                ifPresent { try Data(contentsOf: bundle.appending(components: "Contents", "Info.plist")) }.map { plist in try PropertyListDecoder().decode(BundleInfo.self, from: plist) },
                bundle.resourceValues(forKeys: [.applicationIsScriptableKey]),
            )
        } catch {
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
        guard let identifier = info?.identifier else { throw ApplicationFailure(subject: path, reason: .noIdentifier) }
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

    public static func preferred(of name: String, among installations: [Self]) throws(ApplicationFailure) -> Self {
        let shared = Set(installations.map(\.identifier))
        let registered =
            installations.count > 1 && shared.count == 1
            ? shared.first.flatMap(NSWorkspace.shared.urlForApplication(withBundleIdentifier:))?.resolvingSymlinksInPath().pathComponents : nil
        guard let chosen = installations.count == 1 ? installations.first : installations.first(where: { installation in installation.url.pathComponents == registered }) else {
            throw ApplicationFailure(subject: name, reason: .unnamed(installations.map(\.url)))
        }
        return chosen
    }

    public func runs(_ instance: NSRunningApplication) -> Bool {
        instance.bundleURL?.resolvingSymlinksInPath().pathComponents == url.pathComponents
    }
}
