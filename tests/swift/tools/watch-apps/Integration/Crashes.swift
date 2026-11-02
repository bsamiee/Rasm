import AppKit
import CasePaths
import CustomDump
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Crashes {
        @Test(.application(FixtureApplication.self, launches: false), .timeLimit(.minutes(2)))
        func crashPrintsSignalThenReportWithMachException() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: Int32, lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier], within: .seconds(90)) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.fault, in: fixture, pid: pid)
                _ = await lines.collect { seen in seen.contains { line in line.entry.instances?[pid]?.exited?.report != nil || line.entry.instances?[pid]?.exited?.reportFailure != nil } }
                return pid
            }
            let pid: Int32 = watched.outcome
            let exits: [WatchAppsLine.Exit] = watched.lines.compactMap { timed in timed.line.entry.instances?[pid]?.exited }
            let report: WatchAppsLine.Report = try #require(exits.last?.report)
            let readable: Bool = FileManager.default.isReadableFile(atPath: report.url.path(percentEncoded: false))
            expectNoDifference(exits.map(\.signal), [SIGSEGV, SIGSEGV])
            expectNoDifference(exits.map { exit in exit.report != nil }, [false, true])
            expectNoDifference(report.pid, pid)
            expectNoDifference([report.bundleIdentifier, report.exception, report.url.pathExtension], [fixture.identifier, "EXC_BAD_ACCESS", "ips"])
            #expect(readable)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func systemAlertNamingAppPrintsPromptLineUntilClosed() async throws {
            let fixture: Fixture = try Fixture()
            let header: [CFString: String] = [kCFUserNotificationAlertHeaderKey: "\(fixture.name) quit unexpectedly."]
            let shown: (host: String, dialogs: [WatchAppsLine.Element]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let created: CFUserNotification? = CFUserNotificationCreate(nil, 0, kCFUserNotificationPlainAlertLevel, nil, header as CFDictionary)
                let alert: CFUserNotification = try #require(created)
                defer { CFUserNotificationCancel(alert) }
                let prompt: WatchAppsLine = try await lines.next { line in line.prompts(besides: fixture.identifier) }.line
                CFUserNotificationCancel(alert)
                _ = try await lines.next { line in line.key == prompt.key && line.entry == .instances([:]) }
                return (prompt.key, (prompt.entry.instances ?? [:]).values.compactMap(\.running?.status.responding?.elements).joined().map(\.self))
            }
            .outcome
            let unanswered: [String] = shown.dialogs.filter { dialog in dialog.requiresAnswer != true }.map(\.id)
            let unnamed: [String] = shown.dialogs.filter { dialog in !([dialog.title].compactMap(\.self) + (dialog.detail?.text ?? [])).contains { text in text.contains(fixture.name) } }.map(\.id)
            expectNoDifference(shown.host, "com.apple.UserNotificationCenter")
            expectNoDifference(shown.dialogs.isEmpty, false)
            expectNoDifference(unanswered, [])
            expectNoDifference(unnamed, [])
        }
    }
}
