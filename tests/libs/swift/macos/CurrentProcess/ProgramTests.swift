import Configuration
import CustomDump
import Foundation
import InMemoryLogging
import Logging
import Testing

@testable import CurrentProcess

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ProgramTests {
    @Test
    func readsEachPreferenceAsTheConfigurationTypeItsPropertyListTypeStates() {
        let domain: [String: Any] = [
            "flag": true, "count": 3, "ratio": 0.5, "name": "Untitled", "names": ["a", "b"], "blob": Data([1, 2]), "since": Date(timeIntervalSince1970: 0),
        ]
        let read: [AbsoluteConfigKey: ConfigContent] = Program.values(domain).mapValues { value in value.content }
        let expected: [AbsoluteConfigKey: ConfigContent] = [
            "flag": .bool(true), "count": .int(3), "ratio": .double(0.5), "name": .string("Untitled"), "names": .stringArray(["a", "b"]), "blob": .bytes([1, 2]),
        ]
        expectNoDifference(read, expected)
    }

    @Test
    func logsEachUnreadableSettingOnceWithTheValuesItAccepts() {
        let provider = InMemoryProvider(values: ["log.size": "big", "log.target": "syslog"])
        let reporter = UnrecognizedValues(accepted: ["log.target": LogTarget.allCases.map(\.rawValue)], raw: ConfigReader(providers: [provider]))
        let settings = ConfigReader(providers: [provider], accessReporter: reporter)
        let store = InMemoryLogHandler()
        let logger = Logger(label: String(reflecting: Self.self)) { _ in store }
        _ = (settings.int(forKey: "log.size"), settings.string(forKey: "log.target", as: LogTarget.self), settings.int(forKey: "log.size"))
        reporter.log(to: logger)
        reporter.log(to: logger)
        let expected: [Logger.Metadata] = [
            ["variable": "log.size", "value": "big", "accepted": "int"], ["variable": "log.target", "value": "syslog", "accepted": "file, stderr"],
        ]
        expectNoDifference(store.entries.map(\.metadata), expected)
        expectNoDifference(store.entries.map(\.level), [.warning, .warning])
    }
}
