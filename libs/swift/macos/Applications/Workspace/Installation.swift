public import AppIntents
public import AppKit
import CasePathsCore
import Files
import MachO.dyld.utils
import OrderedCollections
import Synchronization
import System
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

private struct InfoKey: CodingKey, ExpressibleByStringLiteral {
    let stringValue: String
    var intValue: Int? { nil }

    init(stringValue: String) { self.stringValue = stringValue }
    init(stringLiteral key: String) { stringValue = key }
    init(_ key: CFString) { stringValue = key as String }
    init?(intValue _: Int) { nil }
}

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
        let identifier: String

        init(from decoder: any Decoder) throws {
            identifier = try decoder.container(keyedBy: InfoKey.self).decode(String.self, forKey: InfoKey(kUTTypeIdentifierKey))
        }
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

    init(from decoder: any Decoder) throws {
        let info = try decoder.container(keyedBy: InfoKey.self)
        identifier = try info.decodeIfPresent(String.self, forKey: InfoKey(kCFBundleIdentifierKey))
        executable = try info.decodeIfPresent(String.self, forKey: InfoKey(kCFBundleExecutableKey))
        bundleName = try info.decodeIfPresent(String.self, forKey: InfoKey(kCFBundleNameKey))
        version = try info.decodeIfPresent(String.self, forKey: "CFBundleShortVersionString")
        build = try info.decodeIfPresent(String.self, forKey: InfoKey(kCFBundleVersionKey))
        scriptingDefinitionFile = try info.decodeIfPresent(String.self, forKey: "OSAScriptingDefinition")
        scriptingDefinitionIsDynamic = try info.decodeIfPresent(InfoFlag.self, forKey: "OSAScriptingDefinitionIsDynamic")
        agent = try info.decodeIfPresent(InfoFlag.self, forKey: "LSUIElement")
        backgroundOnly = try info.decodeIfPresent(InfoFlag.self, forKey: "LSBackgroundOnly")
        documentTypes = try info.decodeIfPresent([DocumentType].self, forKey: "CFBundleDocumentTypes")
        urlTypes = try info.decodeIfPresent([URLType].self, forKey: "CFBundleURLTypes")
        exportedTypes = try info.decodeIfPresent([ExportedType].self, forKey: InfoKey(kUTExportedTypeDeclarationsKey))
        multipleInstancesProhibited = try info.decodeIfPresent(InfoFlag.self, forKey: "LSMultipleInstancesProhibited")
        environment = try info.decodeIfPresent([String: String].self, forKey: "LSEnvironment")
        claim = try info.decodeIfPresent(Installation.Claim.self, forKey: "ApplicationServerClaim")
    }
}

private struct ActionsData: Decodable {
    struct LocalizedString: Decodable {
        let key: String
        let table: String?
        let defaultValue: String?

        func string(in bundle: Bundle) -> String { bundle.localizedString(forKey: key, value: defaultValue, table: table) }
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

        func intent(in bundle: Bundle) -> Installation.AppIntent {
            Installation.AppIntent(
                identifier: identifier,
                title: title?.string(in: bundle),
                description: descriptionMetadata?.descriptionText?.string(in: bundle),
                openAppWhenRun: openAppWhenRun,
                supportedModes: supportedModes.map(IntentModes.init(rawValue:)) ?? (openAppWhenRun ? .foreground : .background),
                isDiscoverable: isDiscoverable ?? true,
                parameters: parameters.map { parameter in
                    Installation.AppIntent.Parameter(
                        name: parameter.name,
                        title: parameter.title?.string(in: bundle),
                        description: parameter.parameterDescription?.string(in: bundle),
                        isOptional: parameter.isOptional,
                    )
                },
            )
        }
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

        init(_ properties: [String: Any]) {
            agentName = properties[kLSQuarantineAgentNameKey as String] as? String
            type = properties[kLSQuarantineTypeKey as String] as? String
            originURL = properties[kLSQuarantineOriginURLKey as String] as? URL
            timeStamp = properties[kLSQuarantineTimeStampKey as String] as? Date
        }

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

    private struct Resources: Sendable {
        let scriptable: Bool
        let quarantine: Quarantine?
    }

    private final class Deferred: Hashable, Sendable {
        struct Held {
            var name: String?
            var resources: Result<Resources, ApplicationFailure>?
            var architectures: Result<[String]?, ApplicationFailure>?
            var appIntents: Result<[AppIntent], ApplicationFailure>?
        }

        private let held = Mutex(Held())

        static func == (_: Deferred, _: Deferred) -> Bool { true }

        func hash(into _: inout Hasher) {}

        func read<Value: Sendable>(_ fact: WritableKeyPath<Held, Value?>, by reading: () -> Value) -> Value {
            held.withLock { held in
                let value = held[keyPath: fact] ?? reading()
                held[keyPath: fact] = value
                return value
            }
        }
    }

    private enum CodingKeys: CodingKey {
        case url, executable, identifier, name, bundleName, version, build, scriptable, scriptingDefinitionFile, activationPolicy, documentTypes, urlSchemes, exportedTypes
        case multipleInstancesProhibited, environment, architectures, quarantine, claim, appIntents, unread
    }

    public let url: URL
    public let executable: URL?
    public let identifier: String
    public let bundleName: String?
    public let version: String?
    public let build: String?
    public let scriptingDefinitionFile: String?
    public let scriptingDefinitionIsDynamic: Bool
    public let activationPolicy: NSApplication.ActivationPolicy
    public let documentTypes: [DocumentType]?
    public let urlSchemes: [String]?
    public let exportedTypes: [ExportedType]?
    public let multipleInstancesProhibited: Bool?
    public let environment: [String: String]?
    public let claim: Claim?
    private let deferred = Deferred()

    public var name: String { deferred.read(\.name) { FileManager.default.displayName(atPath: url.path(percentEncoded: false)) } }

    public var scriptable: Bool { get throws(ApplicationFailure) { try resources.get().scriptable } }

    public var quarantine: Quarantine? { get throws(ApplicationFailure) { try resources.get().quarantine } }

    private var resources: Result<Resources, ApplicationFailure> {
        deferred.read(\.resources) { () -> Result<Resources, ApplicationFailure> in
            let values: URLResourceValues
            do {
                values = try url.resourceValues(forKeys: [.applicationIsScriptableKey, .quarantinePropertiesKey])
            } catch {
                return .failure(ApplicationFailure(subject: url.path(percentEncoded: false), reason: .foundation(error as NSError)))
            }
            let quarantine: Quarantine? = values.quarantineProperties.map(Quarantine.init)
            return .success(Resources(scriptable: values.applicationIsScriptable == true, quarantine: quarantine))
        }
    }

    public var architectures: [String]? {
        get throws(ApplicationFailure) {
            try deferred.read(\.architectures) { () -> Result<[String]?, ApplicationFailure> in
                guard let path = executable?.path(percentEncoded: false) else { return .success(nil) }
                var slices: [String] = []
                let status: Int32 = unsafe macho_for_each_slice(path) { slice, _, _, _ in
                    if let name = unsafe macho_arch_name_for_mach_header(slice) { slices.append(unsafe String(cString: name)) }
                }
                return switch status {
                    case 0: .success(slices)
                    case EFTYPE: .success([])
                    default: .failure(ApplicationFailure(subject: path, reason: .system(Errno(rawValue: status))))
                }
            }
            .get()
        }
    }

    public var appIntents: [AppIntent] {
        get throws(ApplicationFailure) { try deferred.read(\.appIntents) { Result { () throws(ApplicationFailure) in try AppIntent.declared(by: url) } }.get() }
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        let read = (resources: resources, architectures: Result { () throws(ApplicationFailure) in try architectures }, appIntents: Result { () throws(ApplicationFailure) in try appIntents })
        let resolved: Resources? = read.resources[case: \.success]
        let scriptable: Bool? = resolved?.scriptable
        let quarantine: Quarantine? = resolved?.quarantine
        let architectures: [String]? = read.architectures[case: \.success] ?? nil
        try fields.encode(url, forKey: .url)
        try fields.encodeIfPresent(executable, forKey: .executable)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encode(name, forKey: .name)
        try fields.encodeIfPresent(bundleName, forKey: .bundleName)
        try fields.encodeIfPresent(version, forKey: .version)
        try fields.encodeIfPresent(build, forKey: .build)
        try fields.encodeIfPresent(scriptable, forKey: .scriptable)
        try fields.encodeIfPresent(scriptingDefinitionFile, forKey: .scriptingDefinitionFile)
        try fields.encode(activationPolicy.name, forKey: .activationPolicy)
        try fields.encodeIfPresent(documentTypes, forKey: .documentTypes)
        try fields.encodeIfPresent(urlSchemes, forKey: .urlSchemes)
        try fields.encodeIfPresent(exportedTypes, forKey: .exportedTypes)
        try fields.encodeIfPresent(multipleInstancesProhibited, forKey: .multipleInstancesProhibited)
        try fields.encodeIfPresent(environment, forKey: .environment)
        try fields.encodeIfPresent(architectures, forKey: .architectures)
        try fields.encodeIfPresent(quarantine, forKey: .quarantine)
        try fields.encodeIfPresent(claim, forKey: .claim)
        try fields.encodeIfPresent(read.appIntents[case: \.success], forKey: .appIntents)
        let unread: [ApplicationFailure] = [read.resources.map { _ in true }, read.architectures.map { _ in true }, read.appIntents.map { _ in true }].compactMap { outcome in outcome[case: \.failure] }
        if !unread.isEmpty { try fields.encode(unread, forKey: .unread) }
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

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public init(at url: URL) throws(ApplicationFailure) {
        let bundle = url.resolvingSymlinksInPath()
        let path = bundle.path(percentEncoded: false)
        let info: BundleInfo?
        do {
            info = try ifPresent { try Data(contentsOf: bundle.appending(components: "Contents", "Info.plist")) }.map { plist in try PropertyListDecoder().decode(BundleInfo.self, from: plist) }
        } catch {
            throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
        }
        guard let info, let identifier = info.identifier else { throw ApplicationFailure(subject: path, reason: .noIdentifier) }
        let extensions: (String) -> [String] = { type in UTType(type)?.tags[.filenameExtension] ?? [] }
        let documentTypes: [DocumentType]? = info.documentTypes.map { (types: [BundleInfo.DocumentType]) -> [DocumentType] in
            OrderedDictionary<String?, [BundleInfo.DocumentType]>(grouping: types, by: \.role).map { (role: String?, declared: [BundleInfo.DocumentType]) -> DocumentType in
                let tags: [String] = declared.flatMap { (type: BundleInfo.DocumentType) -> [String] in (type.extensions ?? []) + (type.contentTypes ?? []).flatMap(extensions) }
                return DocumentType(role: role, extensions: OrderedSet(tags.map { tag in tag.lowercased() }).elements)
            }
        }
        let activationPolicy: NSApplication.ActivationPolicy =
            switch (info.backgroundOnly?.value, info.agent?.value) {
                case (true, _): .prohibited
                case (_, true): .accessory
                default: .regular
            }
        self.init(
            url: bundle,
            executable: info.executable.map { name in bundle.appending(components: "Contents", "MacOS", name) },
            identifier: identifier,
            bundleName: info.bundleName,
            version: info.version,
            build: info.build,
            scriptingDefinitionFile: info.scriptingDefinitionFile,
            scriptingDefinitionIsDynamic: info.scriptingDefinitionIsDynamic?.value == true,
            activationPolicy: activationPolicy,
            documentTypes: documentTypes,
            urlSchemes: info.urlTypes.map { (types: [BundleInfo.URLType]) -> [String] in OrderedSet(types.flatMap { type in type.schemes ?? [] }).elements },
            exportedTypes: info.exportedTypes?.map { (type: BundleInfo.ExportedType) -> ExportedType in ExportedType(identifier: type.identifier, extensions: extensions(type.identifier)) },
            multipleInstancesProhibited: info.multipleInstancesProhibited?.value,
            environment: info.environment,
            claim: info.claim,
        )
    }

    public func runs(_ instance: NSRunningApplication) -> Bool {
        instance.bundleIdentifier == identifier && instance.bundleURL?.resolvingSymlinksInPath().pathComponents == url.pathComponents
    }
}

extension Installation.AppIntent {
    static func declared(by url: URL) throws(ApplicationFailure) -> [Self] {
        guard let application = Bundle(url: url) else { return [] }
        let folders = [application.builtInPlugInsURL, url.appending(components: "Contents", "Extensions", directoryHint: .isDirectory)].compactMap(\.self)
        let frameworks = [application.privateFrameworksURL, application.sharedFrameworksURL].compactMap(\.self)
        let entries: [URL]
        do {
            entries = try folders.flatMap { folder in try ifPresent { try FileManager.default.contentsOfDirectory(at: folder, includingPropertiesForKeys: nil) } ?? [] }
        } catch {
            throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .foundation(error as NSError))
        }
        var pending = OrderedSet([application] + entries.filter { entry in UTType(filenameExtension: entry.pathExtension)?.conforms(to: .applicationExtension) == true }.compactMap(Bundle.init(url:)))
        var declared = OrderedSet<Self>()
        var next = pending.startIndex
        while next < pending.endIndex {
            let owner = pending[next]
            next += 1
            guard let metadata = owner.resourceURL?.appending(component: "Metadata.appintents", directoryHint: .isDirectory) else { continue }
            let (actions, package): (ActionsData?, PackageData?)
            do {
                (actions, package) = try (
                    ifPresent { try Data(contentsOf: metadata.appending(component: "extract.actionsdata")) }.map { data in try JSONDecoder().decode(ActionsData.self, from: data) },
                    ifPresent { try Data(contentsOf: metadata.appending(component: "extract.packagedata")) }.map { data in try JSONDecoder().decode(PackageData.self, from: data) },
                )
            } catch {
                throw ApplicationFailure(subject: metadata.path(percentEncoded: false), reason: .foundation(error as NSError))
            }
            declared.append(contentsOf: (actions?.actions.values.sorted(using: KeyPathComparator(\.identifier)) ?? []).map { action in action.intent(in: owner) })
            let modules = (package?.includes ?? []).compactMap { include in Scanner(string: include).scanInt().map { length in String(include.drop(while: \.isNumber).prefix(length)) } }
            pending.append(
                contentsOf: modules.compactMap { module in frameworks.lazy.compactMap { folder in Bundle(url: folder.appending(component: module).appendingPathExtension(for: .framework)) }.first }
            )
        }
        return declared.elements
    }
}
