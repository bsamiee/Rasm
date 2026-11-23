public import Applications
import CasePathsCore
public import Configuration
public import Foundation
import Logging
import Synchronization
public import System

// --- [TYPES] ---------------------------------------------------------------------------

public enum Folder: Sendable {
    case logs
    case captures
    case media
    case state
}

// --- [SERVICES] ------------------------------------------------------------------------

final class UnrecognizedValues: AccessReporter {
    private let accepted: [String: [String]]
    private let raw: ConfigReader
    private let state = Mutex<(reported: Set<String>, pending: [(variable: String, value: String, accepted: String)])>(([], []))

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
    public static let current = Self()

    public let identifier: String
    public let name: String
    public let version: String?
    public let settings: ConfigReader
    let environment: [String: String]
    let stored: MutableInMemoryProvider
    let unrecognized: UnrecognizedValues

    public init(bundle: Bundle = .main, environment: [String: String] = ProcessInfo.processInfo.environment) {
        let identifier = bundle.bundleIdentifier ?? ProcessInfo.processInfo.processName
        let name = identifier.split(separator: ".").last.map { component in String(component) } ?? identifier
        (self.identifier, self.name, self.environment) = (identifier, name, environment)
        version = bundle.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        stored = MutableInMemoryProvider(name: identifier, initialValues: Self.values(UserDefaults.standard.persistentDomain(forName: identifier) ?? [:]))
        let providers: [any ConfigProvider] = [EnvironmentVariablesProvider(environmentVariables: environment).prefixKeys(with: ConfigKey([name])), stored]
        unrecognized = UnrecognizedValues(accepted: ["log.target": Target.allCases.map(\.rawValue)], raw: ConfigReader(providers: providers))
        settings = ConfigReader(providers: providers, accessReporter: unrecognized)
    }

    public func directory(_ folder: Folder, of application: String? = nil, default stated: URL? = nil) -> URL {
        let setting: (ConfigKey) -> URL? = { key in settings.string(forKey: key).map { path in URL(filePath: path, directoryHint: .isDirectory) } }
        return switch (folder, setting("output.dir")) {
            case (.state, _): setting("state.dir") ?? stated ?? .applicationSupportDirectory.appending(path: identifier, directoryHint: .isDirectory)
            case (_, .some(let output)): output.appending(components: application ?? name, String(describing: folder), directoryHint: .isDirectory)
            case (.logs, .none): .libraryDirectory.appending(components: "Logs", name, directoryHint: .isDirectory)
            case (_, .none): .cachesDirectory.appending(components: name, String(describing: folder), directoryHint: .isDirectory)
        }
    }

    public func store(_ value: any Sendable?, forKey key: ConfigKey) {
        UserDefaults.standard.set(value, forKey: key.description)
        stored.setValue(value.flatMap { value in Self.values([key.description: value]).values.first }, forKey: AbsoluteConfigKey(key))
    }

    public func require(_ grants: [Grant]) throws(ApplicationFailure) {
        for grant in grants where !grant.held {
            let requested = ConfigKey("grants.requested.\(grant.rawValue)")
            if settings.bool(forKey: requested) != true {
                grant.request()
                store(true, forKey: requested)
            }
            try grant.check()
        }
    }

    static func values(_ domain: [String: Any]) -> [AbsoluteConfigKey: ConfigValue] {
        domain.reduce(into: [:]) { values, entry in
            let content: ConfigContent? =
                switch entry.value {
                    case let text as String: .string(text)
                    case let texts as [String]: .stringArray(texts)
                    case let data as Data: .bytes(Array(data))
                    case let number as NSNumber where CFGetTypeID(number) == CFBooleanGetTypeID(): .bool(number.boolValue)
                    case let number as NSNumber where CFNumberIsFloatType(number): .double(number.doubleValue)
                    case let number as NSNumber: .int(number.intValue)
                    default: nil
                }
            values[AbsoluteConfigKey(ConfigKey(entry.key))] = content.map { content in ConfigValue(content, isSecret: false) }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func disclaimResponsibility() throws(Errno) {
    func check(_ status: Int32) throws(Errno) { if status != 0 { throw Errno(rawValue: status) } }
    guard responsiblePID(getpid()) != getpid() else { return }
    guard let executable = Bundle.main.executablePath else { throw .noSuchFileOrDirectory }
    var attributes: posix_spawnattr_t?
    try check(unsafe posix_spawnattr_init(&attributes))
    defer { _ = unsafe posix_spawnattr_destroy(&attributes) }
    try check(unsafe disclaim(&attributes, 1))
    try check(unsafe posix_spawnattr_setflags(&attributes, Int16(POSIX_SPAWN_SETEXEC)))
    try check(unsafe posix_spawn(nil, executable, nil, &attributes, CommandLine.unsafeArgv, environ))
}

@_silgen_name("responsibility_get_pid_responsible_for_pid")
private func responsiblePID(_: pid_t) -> pid_t
@_silgen_name("responsibility_spawnattrs_setdisclaim")
private func disclaim(_: UnsafeMutablePointer<posix_spawnattr_t?>, _: Int32) -> Int32
