import CustomDump
import Foundation
import InMemoryLogging
import Logging
import MetricKit
import Testing
@testable import Telemetry

// --- [CONSTANTS] -----------------------------------------------------------------------

private let tree = #"{"callStackThreads":[],"callStackPerThread":true,"binaryInfo":[]}"#
private let environment =
    #""timeRange":{"begin":0,"end":1},"environment":{"regionFormat":"US","osVersion":{"platform":"macOS","number":"27.2","buildNumber":"27C5"},"deviceType":"Mac16,1","#
    + #""platformArchitecture":"arm64e","lowPowerModeEnabled":false,"isTestFlightApp":false,"applicationVersion":"1.4","applicationBuildVersion":"14","pid":312,"#
    + #""bundleIdentifier":"app.rasm.relay","signpostData":[],"states":[]}"#
private let diagnostics = [
    #"{"crashDiagnostic":{"callStackTree":\#(tree),"exceptionType":1,"exceptionCode":24,"signal":11},\#(environment)}"#,
    #"{"hangDiagnostic":{"callStackTree":\#(tree),"hangDuration":{"value":2.5,"unit":"s"}},\#(environment)}"#,
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct MetricKitReportsTests {
    @Test
    func logsEachReportAtItsOwnLevelWithItsBodyOrItsEncodingFailure() async {
        let store = InMemoryLogHandler()
        let reports = MetricKitReports(to: Logger(label: String(reflecting: Self.self)) { _ in store })
        let (stream, continuation) = AsyncStream.makeStream(of: Double.self)
        continuation.yield(2.5)
        continuation.yield(.nan)
        continuation.finish()
        await reports.log(stream) { _ in (.critical, "MetricKit hang report", "1.4", 312) }
        expectNoDifference(store.entries.map { entry in "\(entry.level) \(entry.message)" }, ["critical MetricKit hang report", "critical MetricKit hang report"])
        expectNoDifference(
            store.entries.map { entry in entry.metadata.mapValues(\.description) },
            [["service.version": "1.4", "process.pid": "312", "report": "2.5"], ["service.version": "1.4", "process.pid": "312"]],
        )
        expectNoDifference(store.entries.map { entry in entry.error.map { error in String(reflecting: type(of: error)) } }, [nil, "Swift.EncodingError"])
    }

    @Test
    func leavesCrashesToTheCrashReportReaderAndLogsEveryOtherDiagnosticWithItsMeasureAndTimeRange() async throws {
        let store = InMemoryLogHandler()
        let reports = MetricKitReports(to: Logger(label: String(reflecting: Self.self)) { _ in store })
        let decoded = try diagnostics.map { json in try JSONDecoder().decode(MetricKit.DiagnosticReport.self, from: Data(json.utf8)) }
        let hang = try #require(decoded.lazy.compactMap { (report: MetricKit.DiagnosticReport) -> HangDiagnostic? in if case .hang(let hang) = report.result { hang } else { nil } }.first)
        let (stream, continuation) = AsyncStream.makeStream(of: MetricKit.DiagnosticReport.self)
        for report in decoded { continuation.yield(report) }
        continuation.finish()
        await reports.log(stream)
        expectNoDifference(store.entries.map { entry in "\(entry.level) \(entry.message)" }, ["error Hang of \(hang.hangDuration.formatted()) reported by MetricKit"])
        expectNoDifference(store.entries.map { entry in entry.metadata.keys.sorted() }, [["process.pid", "service.version", "time_range"]])
        expectNoDifference(
            store.entries.map { entry in entry.metadata.filter { key, _ in key != "time_range" }.mapValues(\.description) },
            [["process.pid": "312", "service.version": "1.4"]],
        )
    }
}
