public import AppIntents
public import AppKit
import Files
import MachO.dyld.utils
import OrderedCollections
import System
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
        case executable = "CFBundleExecutable"
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
    let executable: String?
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

private struct ActionsData: Decodable {
    struct LocalizedString: Decodable {
        let key: String
        let table: String?
        let defaultValue: String?
    }

    struct Parameter: Decodable {
        let name: String
        let title: LocalizedString?
        let parameterDescription: LocalizedString?
        let isOptional: Bool
    }

    struct Action: Decodable {
        struct DescriptionMetadata: Decodable {
            let descriptionText: LocalizedString?
        }

        let identifier: String
        let title: LocalizedString?
        let descriptionMetadata: DescriptionMetadata?
        let openAppWhenRun: Bool
        let supportedModes: Int?
        let isDiscoverable: Bool?
        let parameters: [Parameter]
    }

    let actions: [String: Action]
}

private struct PackageData: Decodable {
    let includes: [String]
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

    public struct Quarantine: CustomStringConvertible, Encodable, Hashable, Sendable {
        public let agentName: String?
        public let type: String?
        public let originURL: URL?
        public let timeStamp: Date?

        public var description: String {
            let facts: [String?] = [
                type, agentName.map { agent in "from \(agent)" }, originURL.map { origin in "of \(origin)" }, timeStamp.map { stamp in "at \(stamp.ISO8601Format())" },
            ]
            return facts.compactMap(\.self).joined(separator: " ")
        }
    }

    public struct Claim: Codable, Hashable, Sendable {
        public let prefixes: [String]
        public let server: String
        public let scope: String

        public var predicate: NSPredicate {
            NSCompoundPredicate(orPredicateWithSubpredicates: prefixes.map { prefix in NSPredicate(format: "%K BEGINSWITH %@", NSMetadataItemCFBundleIdentifierKey, prefix) })
        }
    }

    public struct AppIntent: Encodable, Hashable, Sendable {
        public struct Parameter: Encodable, Hashable, Sendable {
            public let name: String
            public let title: String?
            public let description: String?
            public let isOptional: Bool
        }

        private enum CodingKeys: CodingKey {
            case identifier, title, description, openAppWhenRun, supportedModes, isDiscoverable, parameters
        }

        private static let modes: KeyValuePairs<String, IntentModes> = ["background": .background, "foreground": .foreground, "deferred": .foreground(.deferred), "dynamic": .foreground(.dynamic)]

        public let identifier: String
        public let title: String?
        public let description: String?
        public let openAppWhenRun: Bool
        public let supportedModes: IntentModes
        public let isDiscoverable: Bool
        public let parameters: [Parameter]

        public func encode(to encoder: any Encoder) throws {
            var container = encoder.container(keyedBy: CodingKeys.self)
            try container.encode(identifier, forKey: .identifier)
            try container.encodeIfPresent(title, forKey: .title)
            try container.encodeIfPresent(description, forKey: .description)
            try container.encode(openAppWhenRun, forKey: .openAppWhenRun)
            try container.encode(Self.modes.filter { _, mode in supportedModes.contains(mode) }.map(\.key), forKey: .supportedModes)
            try container.encode(isDiscoverable, forKey: .isDiscoverable)
            try container.encode(parameters, forKey: .parameters)
        }
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
    public let quarantine: Quarantine?
    public let claim: Claim?
    public let appIntents: [AppIntent]
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
                bundle.resourceValues(forKeys: [.localizedNameKey, .applicationIsScriptableKey, .quarantinePropertiesKey]),
            )
        } catch {
            throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
        }
        guard let info, let identifier = info.identifier else { throw ApplicationFailure(subject: path, reason: .noIdentifier) }
        let extensions: (String) -> [String] = { type in UTType(type)?.tags[.filenameExtension] ?? [] }
        let executable: URL? = info.executable.map { name in bundle.appending(components: "Contents", "MacOS", name) }
        let architectures: [String]? = try executable.map { executable throws(ApplicationFailure) in
            var slices: [String] = []
            let status: Int32 = unsafe macho_for_each_slice(executable.path(percentEncoded: false)) { slice, _, _, _ in
                if let name = unsafe macho_arch_name_for_mach_header(slice) { slices.append(unsafe String(cString: name)) }
            }
            switch status {
                case 0: return slices
                case EFTYPE: return []
                default: throw ApplicationFailure(subject: executable.path(percentEncoded: false), reason: .system(Errno(rawValue: status)))
            }
        }
        let documentTypes: [DocumentType]? = info.documentTypes.map { (types: [BundleInfo.DocumentType]) -> [DocumentType] in
            OrderedDictionary<String?, [BundleInfo.DocumentType]>(grouping: types, by: \.role).map { (role: String?, declared: [BundleInfo.DocumentType]) -> DocumentType in
                let tags: [String] = declared.flatMap { (type: BundleInfo.DocumentType) -> [String] in (type.extensions ?? []) + (type.contentTypes ?? []).flatMap(extensions) }
                return DocumentType(role: role, extensions: OrderedSet(tags.map { tag in tag.lowercased() }).elements)
            }
        }
        let quarantine: Quarantine? = values.quarantineProperties.map { (properties: [String: Any]) -> Quarantine in
            Quarantine(
                agentName: properties[kLSQuarantineAgentNameKey as String] as? String,
                type: properties[kLSQuarantineTypeKey as String] as? String,
                originURL: properties[kLSQuarantineOriginURLKey as String] as? URL,
                timeStamp: properties[kLSQuarantineTimeStampKey as String] as? Date,
            )
        }
        self.init(
            url: bundle,
            executable: executable,
            identifier: identifier,
            name: values.localizedName ?? path,
            bundleName: info.bundleName,
            version: info.version,
            build: info.build,
            scriptable: values.applicationIsScriptable == true,
            scriptingDefinitionFile: info.scriptingDefinitionFile,
            scriptingDefinitionIsDynamic: info.scriptingDefinitionIsDynamic?.value == true,
            activationPolicy: info.activationPolicy.name,
            documentTypes: documentTypes,
            urlSchemes: info.urlTypes.map { (types: [BundleInfo.URLType]) -> [String] in OrderedSet(types.flatMap { type in type.schemes ?? [] }).elements },
            exportedTypes: info.exportedTypes?.map { (type: BundleInfo.ExportedType) -> ExportedType in ExportedType(identifier: type.identifier, extensions: extensions(type.identifier)) },
            multipleInstancesProhibited: info.multipleInstancesProhibited?.value,
            environment: info.environment,
            architectures: architectures,
            quarantine: quarantine,
            claim: info.claim,
            appIntents: try Bundle(url: bundle).map { application throws(ApplicationFailure) in try AppIntent.declared(by: application) } ?? [],
        )
    }

    public func runs(_ instance: NSRunningApplication) -> Bool {
        instance.bundleIdentifier == identifier && instance.bundleURL?.resolvingSymlinksInPath().pathComponents == url.pathComponents
    }
}

extension Installation.AppIntent {
    static func declared(by application: Bundle) throws(ApplicationFailure) -> [Self] {
        let embedded: (URL?, UTType) throws(ApplicationFailure) -> [URL] = { folder, type throws(ApplicationFailure) in
            do {
                let entries: [URL] = try folder.flatMap { folder in try ifPresent { try FileManager.default.contentsOfDirectory(at: folder, includingPropertiesForKeys: [.contentTypeKey]) } } ?? []
                return try entries.filter { entry in try entry.resourceValues(forKeys: [.contentTypeKey]).contentType?.conforms(to: type) == true }
            } catch {
                throw ApplicationFailure(subject: folder?.path(percentEncoded: false) ?? application.bundlePath, reason: .foundation(error as NSError))
            }
        }
        let extensions: [URL] = try [application.builtInPlugInsURL, application.bundleURL.appending(components: "Contents", "Extensions")].map { folder throws(ApplicationFailure) in
            try embedded(folder, .applicationExtension)
        }.flatMap(\.self)
        let frameworks: [String: URL] = try Dictionary(
            [application.privateFrameworksURL, application.sharedFrameworksURL].map { folder throws(ApplicationFailure) in try embedded(folder, .framework) }.flatMap(\.self).map { framework in
                (framework.deletingPathExtension().lastPathComponent, framework)
            },
            uniquingKeysWith: { first, _ in first },
        )
        var pending = OrderedSet([application.bundleURL] + extensions)
        var declared = OrderedSet<Self>()
        var next = pending.startIndex
        while next < pending.endIndex {
            let (intents, includes) = try read(pending[next])
            declared.append(contentsOf: intents)
            pending.append(contentsOf: includes.compactMap { include in Scanner(string: include).scanInt().flatMap { length in frameworks[String(include.drop(while: \.isNumber).prefix(length))] } })
            next += 1
        }
        return declared.elements
    }

    private static func read(_ bundle: URL) throws(ApplicationFailure) -> (intents: [Self], includes: [String]) {
        guard let owner = Bundle(url: bundle), let folder = owner.resourceURL?.appending(component: "Metadata.appintents") else { return ([], []) }
        let (actions, package): (ActionsData?, PackageData?)
        do {
            (actions, package) = try (
                ifPresent { try Data(contentsOf: folder.appending(component: "extract.actionsdata")) }.map { data in try JSONDecoder().decode(ActionsData.self, from: data) },
                ifPresent { try Data(contentsOf: folder.appending(component: "extract.packagedata")) }.map { data in try JSONDecoder().decode(PackageData.self, from: data) },
            )
        } catch {
            throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .foundation(error as NSError))
        }
        let localized: (ActionsData.LocalizedString) -> String = { string in owner.localizedString(forKey: string.key, value: string.defaultValue, table: string.table) }
        let intents: [Self] = (actions?.actions.values.sorted(using: KeyPathComparator(\.identifier)) ?? []).map { (action: ActionsData.Action) -> Self in
            Self(
                identifier: action.identifier,
                title: action.title.map(localized),
                description: action.descriptionMetadata?.descriptionText.map(localized),
                openAppWhenRun: action.openAppWhenRun,
                supportedModes: action.supportedModes.map(IntentModes.init(rawValue:)) ?? (action.openAppWhenRun ? .foreground : .background),
                isDiscoverable: action.isDiscoverable ?? true,
                parameters: action.parameters.map { parameter in
                    Parameter(name: parameter.name, title: parameter.title.map(localized), description: parameter.parameterDescription.map(localized), isOptional: parameter.isOptional)
                },
            )
        }
        return (intents, package?.includes ?? [])
    }
}
