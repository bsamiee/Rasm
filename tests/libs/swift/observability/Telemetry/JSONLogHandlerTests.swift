import CustomDump
import Encoding
import Files
import Foundation
import InMemoryLogging
import Logging
import OSLog
import OTel
import Telemetry
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct JSONLogHandlerTests {
    @Test
    func writesAnOpenTelemetryRecordAndExportsPrivateFieldsMasked() throws {
        let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let file = RotatingFile(folder.appending(component: "records.log"))
        let export = InMemoryLogHandler()
        var terminal = InMemoryLogHandler()
        terminal.logLevel = .warning
        let label = String(reflecting: Self.self)
        let handler = JSONLogHandler(label: label, to: (file, export, terminal), filter: LogFilter(roots: []), resource: ["service.name": "tests"], trace: .default)
        var logger = Logging.Logger(label: label) { _ in handler }
        logger.logLevel = .trace
        logger.info("opened", metadata: ["trace_id": "4bf92f3577b34da6", "span_id": "00f067aa0ba902b7", "count": .stringConvertible(3), "path": "/Users/a/b", "tool": "\("view", privacy: .public)"])
        let lines = try String(decoding: Data(contentsOf: file.url), as: UTF8.self).split(separator: "\n").map { line in try JSONDecoder().decode([String: JSONValue].self, from: Data(line.utf8)) }
        let expected: [[String: JSONValue]] = [
            [
                "severity_text": .string("INFO"), "severity_number": .number(9), "body": .string("opened"), "scope": .string(label), "trace_id": .string("4bf92f3577b34da6"),
                "span_id": .string("00f067aa0ba902b7"), "attributes": .object(["count": .number(3), "path": .string("/Users/a/b"), "tool": .string("view")]),
                "resource": .object(["service.name": .string("tests")]),
            ]
        ]
        expectNoDifference(lines.map { line in line.filter { key, _ in key != "timestamp" } }, expected)
        let exported: [Logging.Logger.Metadata] = [["count": .stringConvertible(3), "path": "<private>", "tool": "view"]]
        expectNoDifference(export.entries.map(\.metadata).map { metadata in metadata.mapValues(\.description) }, exported.map { metadata in metadata.mapValues(\.description) })
        expectNoDifference(terminal.entries.count, 0)
    }

    @Test
    func writesEachLineToTheUnifiedLogUnderItsServiceWithItsPublicFieldsInTheClear() throws {
        let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)
        defer {
            do { if try folder.checkResourceIsReachable() { try FileManager.default.removeItem(at: folder) } } catch CocoaError.fileReadNoSuchFile {} catch { Issue.record(error) }
        }
        let service = "\(String(reflecting: Self.self)).\(UUID().uuidString)"
        let label = String(reflecting: Self.self)
        let file = RotatingFile(folder.appending(component: "records.log"))
        let handler = JSONLogHandler(label: label, to: (file, nil, nil), filter: LogFilter(roots: []), resource: ["service.name": .string(service)], trace: .default)
        let logger = Logging.Logger(label: label) { _ in handler }
        logger.notice("dropped", metadata: ["count": .stringConvertible(3), "path": "/Users/a/b", "tool": "\("view", privacy: .public)"])
        let logged = try OSLogStore(scope: .currentProcessIdentifier).getEntries(matching: NSPredicate(format: "subsystem == %@", service)).compactMap { entry in entry as? OSLogEntryLog }
        expectNoDifference(logged.map(\.category), [label])
        expectNoDifference(logged.map(\.level), [.notice])
        expectNoDifference(logged.map { entry in entry.composedMessage.hasPrefix("dropped count=3 tool=view") }, [true])
    }
}
