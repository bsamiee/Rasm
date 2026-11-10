import CasePathsCore
public import Configuration
import Files
public import Foundation
import Logging
import Synchronization

// --- [TYPES] ---------------------------------------------------------------------------

public enum Folder: Sendable {
    case log
    case cache
    case state
    case runtime

    var parent: URL {
        switch self {
            case .log: .libraryDirectory.appending(path: "Logs", directoryHint: .isDirectory)
            case .cache: .cachesDirectory
            case .state: .applicationSupportDirectory
            case .runtime: .temporaryDirectory
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

final class UnrecognizedValues: AccessReporter {
    private let accepted: [String: [String]]
    private let raw: ConfigReader
    private let state: Mutex<(reported: Set<String>, pending: [(variable: String, value: String, accepted: String)])> = .init(([], []))

    init(accepted: [String: [String]], raw: ConfigReader) { (self.accepted, self.raw) = (accepted, raw) }

    func report(_ event: AccessEvent) {
        guard let failure = event.conversionError ?? event.providerResults.lazy.compactMap({ provider in provider.result[case: \.failure] }).first else { return }
        let key = event.metadata.key
        add(variable: key.description, value: raw.string(forKey: ConfigKey(key.components)) ?? String(describing: failure), accepted: accepted[key.description] ?? [event.metadata.valueType.rawValue])
    }

    func add(variable: String, value: String, accepted: [String]) {
        state.withLock { state in
            if state.reported.insert(variable).inserted { state.pending.append((variable, value, accepted.joined(separator: ", "))) }
        }
    }

    func log(to logger: Logging.Logger) {
        let pending = state.withLock { state in
            defer { state.pending = [] }
            return state.pending
        }
        for entry in pending {
            logger.warning(
                "Setting value unrecognized, default stands",
                metadata: ["variable": .string(entry.variable), "value": .string(entry.value), "accepted": .string(entry.accepted)],
            )
        }
    }
}

public struct Program: Sendable {
    public static let current: Program = .init()

    public let identifier: String
    public let name: String
    public let version: String?
    public let settings: ConfigReader
    let environment: [String: String]
    let stored: MutableInMemoryProvider
    let unrecognized: UnrecognizedValues

    public init(bundle: Bundle = .main, name: String? = nil, environment: [String: String] = ProcessInfo.processInfo.environment) {
        let identifier = bundle.bundleIdentifier ?? ProcessInfo.processInfo.processName
        let name = name ?? identifier.split(separator: ".").last.map { component in String(component) } ?? identifier
        (self.identifier, self.name, self.environment) = (identifier, name, environment)
        version = bundle.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        stored = MutableInMemoryProvider(name: identifier, initialValues: Self.values(UserDefaults.standard.persistentDomain(forName: identifier) ?? [:]))
        let providers: [any ConfigProvider] = [EnvironmentVariablesProvider(environmentVariables: environment).prefixKeys(with: ConfigKey([name])), stored]
        unrecognized = UnrecognizedValues(accepted: ["log.target": Target.allCases.map(\.rawValue)], raw: ConfigReader(providers: providers))
        settings = ConfigReader(providers: providers, accessReporter: unrecognized)
    }

    public func directory(_ folder: Folder, default stated: URL? = nil) -> URL {
        settings.string(forKey: ConfigKey([String(describing: folder), "dir"])).map { path in URL(filePath: path, directoryHint: .isDirectory) }
            ?? stated ?? folder.parent.appending(path: identifier, directoryHint: .isDirectory)
    }

    static func values(_ domain: [String: Any]) -> [AbsoluteConfigKey: ConfigValue] {
        domain.reduce(into: [:]) { values, entry in
            let content: ConfigContent? =
                switch entry.value {
                    case let text as String: .string(text)
                    case let texts as [String]: .stringArray(texts)
                    case let number as NSNumber where CFGetTypeID(number) == CFBooleanGetTypeID(): .bool(number.boolValue)
                    case let number as NSNumber where CFNumberIsFloatType(number): .double(number.doubleValue)
                    case let number as NSNumber: .int(number.intValue)
                    default: nil
                }
            values[AbsoluteConfigKey(ConfigKey(entry.key))] = content.map { content in ConfigValue(content, isSecret: false) }
        }
    }
}
