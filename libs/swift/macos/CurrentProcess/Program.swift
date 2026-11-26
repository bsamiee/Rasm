public import Applications
import CasePathsCore
public import Configuration
public import Foundation
import Logging
public import Results
import Security
import Synchronization
public import System

// --- [TYPES] ---------------------------------------------------------------------------

public enum Directory: Sendable {
    case logs
    case media
    case state
}

// --- [SERVICES] ------------------------------------------------------------------------

final class UnrecognizedValues: AccessReporter {
    private let accepted: [String: [String]]
    private let raw: ConfigReader
    private let state = Mutex<(reported: Set<String>, pending: [Logging.Logger.Metadata])>(([], []))

    init(accepted: [String: [String]], raw: ConfigReader) { (self.accepted, self.raw) = (accepted, raw) }

    func report(_ event: AccessEvent) {
        guard let failure = event.conversionError ?? event.providerResults.lazy.compactMap({ provider in provider.result[case: \.failure] }).first else { return }
        let key = event.metadata.key
        add(variable: key.description, value: raw.string(forKey: ConfigKey(key.components)) ?? String(describing: failure), accepted: accepted[key.description] ?? [event.metadata.valueType.rawValue])
    }

    func add(variable: String, value: String, accepted: [String]) {
        state.withLock { state in
            if state.reported.insert(variable).inserted { state.pending.append(["variable": .string(variable), "value": .string(value), "accepted": .string(accepted.joined(separator: ", "))]) }
        }
    }

    func log(to logger: Logging.Logger) {
        for metadata in state.withLock({ state in exchange(&state.pending, with: []) }) {
            logger.warning("Setting value unrecognized, default stands", metadata: metadata)
        }
    }
}

public struct Program: Sendable {
    public static let current = Self()

    public let identifier: String
    public let name: String
    public let version: String?
    public let settings: ConfigReader
    let stored: MutableInMemoryProvider
    let unrecognized: UnrecognizedValues

    private init() {
        identifier = Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName
        name = identifier.split(separator: ".").last.map { component in String(component) } ?? identifier
        version = Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        stored = MutableInMemoryProvider(name: identifier, initialValues: Self.values(UserDefaults.standard.persistentDomain(forName: identifier) ?? [:]))
        let providers: [any ConfigProvider] = [EnvironmentVariablesProvider().prefixKeys(with: ConfigKey([name])), stored]
        unrecognized = UnrecognizedValues(accepted: ["log.target": LogTarget.allCases.map(\.rawValue)], raw: ConfigReader(providers: providers))
        settings = ConfigReader(providers: providers, accessReporter: unrecognized)
    }

    public func directory(_ kind: Directory, of application: String? = nil) -> URL {
        let configured = settings.string(forKey: kind == .state ? "state.dir" : "output.dir").map { path in URL(filePath: path, directoryHint: .isDirectory) }
        return switch (kind, configured) {
            case (.state, .some(let state)): state
            case (.state, .none): .applicationSupportDirectory.appending(path: identifier, directoryHint: .isDirectory)
            case (_, .some(let output)): output.appending(components: application ?? name, String(describing: kind), directoryHint: .isDirectory)
            case (.logs, .none): .libraryDirectory.appending(components: "Logs", identifier, directoryHint: .isDirectory)
            case (_, .none): .cachesDirectory.appending(components: identifier, String(describing: kind), directoryHint: .isDirectory)
        }
    }

    public func store(_ value: any Sendable?, forKey key: ConfigKey) {
        UserDefaults.standard.set(value, forKey: key.description)
        stored.setValue(value.flatMap { value in Self.values([key.description: value]).values.first }, forKey: AbsoluteConfigKey(key))
    }

    public func require() throws(AggregateError<ApplicationFailure>) {
        let missing = Grant.allCases.filter { grant in !grant.isHeld }
        guard !missing.isEmpty else { return }
        var (code, requirement, identity): (SecStaticCode?, SecRequirement?, CFData?) = (nil, nil, nil)
        let found = unsafe SecStaticCodeCreateWithPath(Bundle.main.bundleURL as CFURL, [], &code)
        let designated = code.map { code in unsafe SecCodeCopyDesignatedRequirement(code, [], &requirement) } ?? found
        let read = requirement.map { requirement in unsafe SecRequirementCopyData(requirement, [], &identity) } ?? designated
        let identified = Result { () throws(AggregateError<ApplicationFailure>) in
            guard let identity else { throw AggregateError(first: ApplicationFailure(subject: identifier, reason: .foundation(NSError(domain: NSOSStatusErrorDomain, code: Int(read))))) }
            return identity as Data
        }
        for grant in missing {
            let requested: ConfigKey = ["grants", "requested", grant.rawValue]
            guard case .success(let identity) = identified, settings.bytes(forKey: requested).map(Data.init) != identity else { continue }
            grant.request()
            store(identity, forKey: requested)
        }
        _ = try combine(identified, traverse(missing) { grant in Result { () throws(ApplicationFailure) in try grant.check() }.aggregated(\.self) }).get()
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

@_silgen_name("responsibility_spawnattrs_setdisclaim")
private func disclaim(_: UnsafeMutablePointer<posix_spawnattr_t?>, _: Int32) -> Int32
