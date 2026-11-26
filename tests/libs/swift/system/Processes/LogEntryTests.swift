import CustomDump
import Foundation
import OSLog
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let record = #"""
    {"timezoneName":"","messageType":"Default","eventType":"logEvent","subsystem":"com.apple.coreanimation","category":"Brightness","processImagePath":"\/System\/Library\/PrivateFrameworks\/SkyLight.framework\/Versions\/A\/Resources\/WindowServer","senderImagePath":"\/System\/Library\/Frameworks\/QuartzCore.framework\/Versions\/A\/QuartzCore","timestamp":"2026-11-26 04:14:55.319344-0600","eventMessage":"Display 1 commitBrightness","processID":640}
    """#
private let logger = Logger(subsystem: String(reflecting: LogEntryTests.self), category: "LogEntry")

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct LogEntryTests {
    @Test
    func decodesLogRecordIntoOSLogFields() throws {
        let entry = try JSONDecoder().decode(LogEntry.self, from: Data(record.utf8))
        let written = try #require(
            Calendar(identifier: .gregorian).date(
                from: DateComponents(timeZone: TimeZone(secondsFromGMT: -6 * 3600), year: 2026, month: 11, day: 26, hour: 4, minute: 14, second: 55, nanosecond: 319_344_000)
            )
        )
        let drift: TimeInterval = abs(entry.date.timeIntervalSince(written))
        #expect(drift.isLess(than: 1e-6))
        expectNoDifference(entry.level, .notice)
        expectNoDifference(entry.processIdentifier, 640)
        expectNoDifference(entry.composedMessage, "Display 1 commitBrightness")
        let encoded = try #require(try JSONSerialization.jsonObject(with: JSONEncoder().encode(entry)) as? [String: Any])
        expectNoDifference(encoded["messageType"] as? String, "Default")
    }

    @Test
    func readsOwnErrorsAndFaultsNewestFirst() async throws {
        let token = UUID().uuidString
        let start = Date.now.addingTimeInterval(-1)
        logger.error("error \(token, privacy: .public)")
        logger.fault("fault \(token, privacy: .public)")
        let entries = try await LogEntry.history(of: .process(getpid()), since: start, limit: 10, containing: token)
        expectNoDifference(entries.map(\.level), [.fault, .error])
        expectNoDifference(entries.map(\.composedMessage), ["fault \(token)", "error \(token)"])
    }

    @Test(.timeLimit(.minutes(1)))
    func streamsOnlyRequestedLevels() async throws {
        let token = UUID().uuidString
        let writing = Task {
            while !Task.isCancelled {
                logger.info("info \(token, privacy: .public)")
                logger.fault("fault \(token, privacy: .public)")
                try await Task.sleep(for: .milliseconds(200))
            }
        }
        defer { writing.cancel() }
        let first = try #require(
            await LogEntry.stream(of: .process(getpid()), levels: [.fault]).first { line in if case .success(let entry) = line { entry.composedMessage?.contains(token) == true } else { false } }
        )
        let entry = try first.get()
        expectNoDifference(entry.level, .fault)
        expectNoDifference(entry.processIdentifier, getpid())
    }
}
