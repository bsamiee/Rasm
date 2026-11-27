package import Applications
import CasePathsCore
import Configuration
package import Foundation
import Logging
package import Results
import SPI
import Security
import Synchronization
package import System

// --- [TYPES] ---------------------------------------------------------------------------

package enum Directory: Sendable {
    case logs
    case state
}

// --- [SERVICES] ------------------------------------------------------------------------

final class UnrecognizedValues: AccessReporter {
    private let raw: ConfigReader
    private let state = Mutex<(reported: Set<String>, pending: [Logging.Logger.Metadata], accepted: [AbsoluteConfigKey: [String]])>(([], [], [:]))

    init(raw: ConfigReader) { self.raw = raw }

    func key(_ key: ConfigKey, accepting values: [String]) -> ConfigKey {
        state.withLock { state in state.accepted[AbsoluteConfigKey(key)] = values }
        return key
    }

    func report(_ event: AccessEvent) {
        let (key, mismatch) = (event.metadata.key, event.providerResults.last?.result[case: \.failure])
        guard let failure = mismatch ?? event.conversionError else { return }
        let rule = mismatch == nil ? state.withLock { state in state.accepted[key] } : nil
        add(variable: key.description, value: raw.string(forKey: ConfigKey(key.components)) ?? String(describing: failure), accepted: rule ?? [event.metadata.valueType.rawValue])
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

package struct Program: Sendable {
    package static let current = Self()

    package let identifier: String
    let name: String
    package let version: String?
    let settings: ConfigReader
    let stored: MutableInMemoryProvider
    let unrecognized: UnrecognizedValues

    private init() {
        identifier = Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName
        name = identifier.split(separator: ".").last.map { component in String(component) } ?? identifier
        version = Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        stored = MutableInMemoryProvider(name: identifier, initialValues: [:])
        let providers: [any ConfigProvider] = [EnvironmentVariablesProvider().prefixKeys(with: ConfigKey([name])), stored]
        unrecognized = UnrecognizedValues(raw: ConfigReader(providers: providers))
        settings = ConfigReader(providers: providers, accessReporter: unrecognized)
        stored.setValues(preferences.mapValues(Optional.some))
    }

    var preferences: [AbsoluteConfigKey: ConfigValue] {
        (UserDefaults.standard.persistentDomain(forName: identifier) ?? [:]).reduce(into: [:]) { values, entry in
            values[AbsoluteConfigKey(ConfigKey(entry.key))] = ConfigValue(propertyList: entry.value)
        }
    }

    package func directory(_ kind: Directory) -> URL {
        switch kind {
            case .state:
                settings.string(forKey: "state.dir").map { (path: String) -> URL in URL(filePath: path, directoryHint: .isDirectory) }
                    ?? URL.applicationSupportDirectory.appending(path: identifier, directoryHint: .isDirectory)
            case .logs:
                settings.string(forKey: "output.dir").map { (path: String) -> URL in
                    URL(filePath: path, directoryHint: .isDirectory).appending(components: name, String(describing: kind), directoryHint: .isDirectory)
                }
                    ?? URL.libraryDirectory.appending(components: "Logs", identifier, directoryHint: .isDirectory)
        }
    }

    func store(_ value: any Sendable?, forKey key: ConfigKey) {
        UserDefaults.standard.set(value, forKey: key.description)
        stored.setValue(value.flatMap(ConfigValue.init(propertyList:)), forKey: AbsoluteConfigKey(key))
    }

    var signature: (requirement: Data, unique: Data) {
        get throws(ApplicationFailure) {
            var (running, code, designated, requirement, information): (SecCode?, SecStaticCode?, SecRequirement?, CFData?, CFDictionary?) = (nil, nil, nil, nil, nil)
            let copied: OSStatus = unsafe SecCodeCopySelf([], &running)
            let located: OSStatus = running.map { (running: SecCode) -> OSStatus in unsafe SecCodeCopyStaticCode(running, [], &code) } ?? errSecSuccess
            let designatedRead: OSStatus = code.map { (code: SecStaticCode) -> OSStatus in unsafe SecCodeCopyDesignatedRequirement(code, [], &designated) } ?? errSecSuccess
            let encoded: OSStatus = designated.map { (designated: SecRequirement) -> OSStatus in unsafe SecRequirementCopyData(designated, [], &requirement) } ?? errSecSuccess
            let informed: OSStatus = code.map { (code: SecStaticCode) -> OSStatus in unsafe SecCodeCopySigningInformation(code, [], &information) } ?? errSecSuccess
            let statuses: [OSStatus] = [copied, located, designatedRead, encoded, informed]
            let status: OSStatus = statuses.first { (status: OSStatus) -> Bool in status != errSecSuccess } ?? errSecSuccess
            let fields: [CFString: CFTypeRef] = information as? [CFString: CFTypeRef] ?? [:]
            guard status == errSecSuccess, let requirement, let unique = fields[kSecCodeInfoUnique] as? Data else {
                throw ApplicationFailure(subject: identifier, reason: .foundation(NSError(domain: NSOSStatusErrorDomain, code: Int(status == errSecSuccess ? errSecCSUnsigned : status))))
            }
            return (requirement as Data, unique)
        }
    }

    package func require() throws(AggregateError<ApplicationFailure>) {
        let missing = Grant.allCases.filter { grant in !grant.isHeld(asking: false) }
        guard !missing.isEmpty else { return }
        let signed = Result { () throws(ApplicationFailure) in try signature.requirement }.aggregated(\.self)
        let requirement: Data? = responsiblePID(getpid()) == getpid() ? signed[case: \.success] : nil
        for grant in missing {
            let requested: ConfigKey = ["grants", "requested", grant.rawValue]
            guard let requirement, settings.bytes(forKey: requested).map(Data.init) != requirement else { continue }
            _ = grant.isHeld(asking: true)
            store(requirement, forKey: requested)
        }
        _ = try combine(signed, traverse(missing) { grant in Result { () throws(ApplicationFailure) in try grant.check() }.aggregated(\.self) }).get()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ConfigValue {
    init?(propertyList value: Any) {
        let content: ConfigContent
        switch value {
            case let text as String: content = .string(text)
            case let texts as [String]: content = .stringArray(texts)
            case let data as Data: content = .bytes(Array(data))
            case let number as NSNumber where CFGetTypeID(number) == CFBooleanGetTypeID(): content = .bool(number.boolValue)
            case let number as NSNumber where CFNumberIsFloatType(number): content = .double(number.doubleValue)
            case let number as NSNumber: content = .int(number.intValue)
            default: return nil
        }
        self.init(content, isSecret: false)
    }
}

package func disclaimResponsibility() throws(Errno) {
    func check(_ status: Int32) throws(Errno) { if status != 0 { throw Errno(rawValue: status) } }
    guard responsiblePID(getpid()) != getpid() else { return }
    guard let executable = Bundle.main.executablePath else { throw .noSuchFileOrDirectory }
    var attributes: posix_spawnattr_t?
    try check(unsafe posix_spawnattr_init(&attributes))
    defer { _ = unsafe posix_spawnattr_destroy(&attributes) }
    try check(unsafe disclaim(&attributes, true))
    try check(unsafe posix_spawnattr_setflags(&attributes, Int16(POSIX_SPAWN_SETEXEC)))
    try check(unsafe posix_spawn(nil, executable, nil, &attributes, CommandLine.unsafeArgv, environ))
}
