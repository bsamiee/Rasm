import AppKit
import CustomDump
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let crashes: [(scenario: Scenario, signal: Int32, exception: String)] = [(.fault, SIGSEGV, "EXC_BAD_ACCESS"), (.abort, SIGABRT, "EXC_CRASH"), (.trap, SIGTRAP, "EXC_BREAKPOINT")]
// --- [OPERATIONS] ----------------------------------------------------------------------

private func crash(
    _ fixture: Fixture,
    by scenario: Scenario,
    until done: @escaping (_ pid: Int32, _ seen: [WatchAppsLine]) -> Bool,
) async throws -> (pid: Int32, seen: [TimedLine<WatchAppsLine>], lines: [TimedLine<WatchAppsLine>]) {
    let watched: (outcome: (pid: Int32, seen: [TimedLine<WatchAppsLine>]), lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier], within: .seconds(90)) { lines in
        let pid: Int32 = try await launch(fixture).pid
        _ = try await lines.next { line in line.instances?[pid]?.phase == .responding }
        try await press(scenario, in: fixture, pid: pid)
        return (pid, await lines.collect { seen in done(pid, seen) })
    }
    return (watched.outcome.pid, watched.outcome.seen, watched.lines)
}

private func reported(_ pid: Int32, in seen: [WatchAppsLine]) -> Bool {
    seen.contains { line in line.instances?[pid]?.exit?.report != nil || line.instances?[pid]?.exit?.reportFailure != nil }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension HostSession {
    struct Crashes {
        @Test(.application(FixtureApplication.self, launches: false, answering: answerCrashAlert), .timeLimit(.minutes(2)), arguments: crashes)
        func crashPrintsSignalThenReportWithMachException(scenario: Scenario, signal: Int32, exception: String) async throws {
            let fixture: Fixture = try Fixture()
            let crashed: (pid: Int32, seen: [TimedLine<WatchAppsLine>], lines: [TimedLine<WatchAppsLine>]) = try await crash(fixture, by: scenario) { pid, seen in
                reported(pid, in: seen)
            }
            let pid: Int32 = crashed.pid
            let ends: [TimedLine<WatchAppsLine>] = crashed.lines.filter { timed in timed.line.instances?[pid]?.exit != nil }
            let exits: [WatchAppsLine.Exit] = ends.compactMap { timed in timed.line.instances?[pid]?.exit }
            let report: WatchAppsLine.Report = try #require(exits.last?.report)
            let readable: Bool = FileManager.default.isReadableFile(atPath: report.url.path(percentEncoded: false))
            let delay: Duration = try #require(ends.last).instant - (try #require(ends.first)).instant
            let reportedInTime: Bool = delay <= .seconds(60)
            expectNoDifference(exits.map(\.signal), [signal, signal])
            expectNoDifference(exits.map { exit in exit.report != nil }, [false, true])
            expectNoDifference([report.pid], [pid])
            expectNoDifference([report.bundleIdentifier, report.exception, report.url.pathExtension], [fixture.identifier, exception, "ips"])
            #expect(readable)
            #expect(reportedInTime, "report after \(delay)")
            try FileManager.default.removeItem(at: report.url)
        }

        @Test(.application(FixtureApplication.self, launches: false, activationPolicy: .regular, answering: answerCrashAlert), .timeLimit(.minutes(2)))
        func crashAlertPrintsPromptNamingAppUntilAnswered() async throws {
            let fixture: Fixture = try Fixture()
            func prompt(in seen: [WatchAppsLine]) -> WatchAppsLine? { seen.last { line in promptHosts.contains(line.key) && line.instances?.isEmpty == false } }
            let crashed: (pid: Int32, seen: [TimedLine<WatchAppsLine>], lines: [TimedLine<WatchAppsLine>]) = try await crash(fixture, by: .fault) { pid, seen in
                reported(pid, in: seen) && prompt(in: seen) != nil
            }
            let shown: WatchAppsLine? = prompt(in: crashed.seen.map(\.line))
            let host: String = try #require(shown?.key)
            let dialogs: [WatchAppsLine.Element] = (shown?.instances ?? [:]).values.compactMap(\.elements).joined().map(\.self)
            let ignore: WatchAppsLine.Control = try #require(dialogs.lazy.compactMap { element in element.control(labeled: "Ignore") }.first)
            _ = try await watching([fixture.identifier], within: .seconds(30)) { lines in
                _ = try await lines.next { line in line.key == host && line.instances?.isEmpty == false }
                try await perform(ignore)
                _ = try await lines.next { line in line.key == host && line.entry == .instances([:]) }
            }
            let unanswered: [String] = dialogs.filter { dialog in dialog.requiresAnswer != true }.map(\.id)
            let unnamed: [String] = dialogs.filter { dialog in !([dialog.title].compactMap(\.self) + (dialog.detail?.text ?? [])).contains { text in text.contains(fixture.name) } }
                .map(\.id)
            let report: URL? = crashed.lines.compactMap { timed in timed.line.instances?[crashed.pid]?.exit?.report?.url }.last
            expectNoDifference(unanswered, [])
            expectNoDifference(unnamed, [])
            try FileManager.default.removeItem(at: try #require(report))
        }
    }
}
