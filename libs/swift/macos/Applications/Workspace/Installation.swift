public import AppKit
import Files
import OrderedCollections
import UniformTypeIdentifiers

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
    struct DocumentType: Decodable {
        private enum CodingKeys: String, CodingKey {
            case role = "CFBundleTypeRole"
            case extensions = "CFBundleTypeExtensions"
            case contentTypes = "LSItemContentTypes"
        }

        let role: String?
        let extensions: [String]?
        let contentTypes: [String]?
    }

    struct URLType: Decodable {
        private enum CodingKeys: String, CodingKey { case schemes = "CFBundleURLSchemes" }

        let schemes: [String]?
    }

    struct ExportedType: Decodable {
        private enum CodingKeys: String, CodingKey { case identifier = "UTTypeIdentifier" }

        let identifier: String
    }

    private enum CodingKeys: String, CodingKey {
        case identifier = "CFBundleIdentifier"
        case bundleName = "CFBundleName"
        case version = "CFBundleShortVersionString"
        case build = "CFBundleVersion"
        case scriptingDefinitionFile = "OSAScriptingDefinition"
        case scriptingDefinitionIsDynamic = "OSAScriptingDefinitionIsDynamic"
        case agent = "LSUIElement"
        case backgroundOnly = "LSBackgroundOnly"
        case documentTypes = "CFBundleDocumentTypes"
        case urlTypes = "CFBundleURLTypes"
        case exportedTypes = "UTExportedTypeDeclarations"
        case multipleInstancesProhibited = "LSMultipleInstancesProhibited"
        case environment = "LSEnvironment"
        case claim = "ApplicationServerClaim"
    }

    let identifier: String?
    let bundleName: String?
    let version: String?
    let build: String?
    let scriptingDefinitionFile: String?
    let scriptingDefinitionIsDynamic: InfoFlag?
    let agent: InfoFlag?
    let backgroundOnly: InfoFlag?
    let documentTypes: [DocumentType]?
    let urlTypes: [URLType]?
    let exportedTypes: [ExportedType]?
    let multipleInstancesProhibited: InfoFlag?
    let environment: [String: String]?
    let claim: Installation.Claim?

    var activationPolicy: NSApplication.ActivationPolicy {
        switch (backgroundOnly?.value, agent?.value) {
            case (true, _): .prohibited
            case (_, true): .accessory
            default: .regular
        }
    }
}

public struct Installation: Encodable, Hashable, Sendable {
    public struct DocumentType: Encodable, Hashable, Sendable {
        public let role: String?
        public let extensions: [String]
    }

    public struct ExportedType: Encodable, Hashable, Sendable {
        public let identifier: String
        public let extensions: [String]
    }

    public struct Claim: Codable, Hashable, Sendable {
        public let prefixes: [String]
        public let server: String
        public let scope: String
    }

    public let url: URL
    public let executable: URL?
    public let identifier: String
    public let name: String
    public let bundleName: String?
    public let version: String?
    public let build: String?
    public let scriptable: Bool
    public let scriptingDefinitionFile: String?
    public let scriptingDefinitionIsDynamic: Bool
    public let activationPolicy: String
    public let documentTypes: [DocumentType]?
    public let urlSchemes: [String]?
    public let exportedTypes: [ExportedType]?
    public let multipleInstancesProhibited: Bool?
    public let environment: [String: String]?
    public let architectures: [String]?
    public let claim: Claim?
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
        let bundle = url.resolvingSymlinksInPath()
        let path = bundle.path(percentEncoded: false)
        let (info, values): (BundleInfo?, URLResourceValues)
        do {
            (info, values) = try (
                ifPresent { try Data(contentsOf: bundle.appending(components: "Contents", "Info.plist")) }.map { plist in try PropertyListDecoder().decode(BundleInfo.self, from: plist) },
                bundle.resourceValues(forKeys: [.applicationIsScriptableKey]),
            )
        } catch {
            throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
        }
        guard let identifier = info?.identifier else { throw ApplicationFailure(subject: path, reason: .noIdentifier) }
        let extensions: (String) -> [String] = { type in UTType(type)?.tags[.filenameExtension] ?? [] }
        let loaded = Bundle(url: bundle)
        self.init(
            url: bundle,
            executable: loaded?.executableURL,
            identifier: identifier,
            name: FileManager.default.displayName(atPath: path),
            bundleName: info?.bundleName,
            version: info?.version,
            build: info?.build,
            scriptable: values.applicationIsScriptable == true,
            scriptingDefinitionFile: info?.scriptingDefinitionFile,
            scriptingDefinitionIsDynamic: info?.scriptingDefinitionIsDynamic?.value == true,
            activationPolicy: (info?.activationPolicy ?? .regular).name,
            documentTypes: info?.documentTypes.map { types in
                OrderedDictionary<String?, [BundleInfo.DocumentType]>(grouping: types, by: \.role).map { role, declared in
                    DocumentType(
                        role: role,
                        extensions: OrderedSet(declared.flatMap { type in (type.extensions ?? []) + (type.contentTypes ?? []).flatMap(extensions) }.map { tag in tag.lowercased() }).elements,
                    )
                }
            },
            urlSchemes: info?.urlTypes.map { types in OrderedSet(types.flatMap { type in type.schemes ?? [] }).elements },
            exportedTypes: info?.exportedTypes?.map { type in ExportedType(identifier: type.identifier, extensions: extensions(type.identifier)) },
            multipleInstancesProhibited: info?.multipleInstancesProhibited?.value,
            environment: info?.environment,
            architectures: loaded?.executableArchitectures?.map { architecture in
                switch architecture.intValue {
                    case NSBundleExecutableArchitectureARM64: "arm64"
                    case NSBundleExecutableArchitectureX86_64: "x86_64"
                    case let other: String(other)
                }
            },
            claim: info?.claim,
        )
    }

    public init(opening document: URL) throws(ApplicationFailure) {
        let subject = document.isFileURL ? document.path(percentEncoded: false) : document.absoluteString
        if document.isFileURL {
            do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
        }
        guard let handler = NSWorkspace.shared.urlForApplication(toOpen: document) else { throw ApplicationFailure(subject: subject, reason: .noHandler) }
        try self.init(at: handler)
    }

    public func runs(_ instance: NSRunningApplication) -> Bool {
        instance.bundleURL?.resolvingSymlinksInPath().pathComponents == url.pathComponents
    }
}
