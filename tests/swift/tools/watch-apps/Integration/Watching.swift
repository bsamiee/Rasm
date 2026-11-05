import CustomDump
import Encoding
import Foundation
import Subprocess
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let spells: [(scenario: Scenario, resuming: Int32, process: JSONValue?)] = [(.hangBusy, SIGUSR1, nil), (.stop, SIGCONT, .string("stopped"))]

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Watching {
        @Test(.application(FixtureApplication.self, launches: false))
        func watchPrintsAnEmptyNameThenAnOutlineLinePerChange() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: (before: [JSONValue], after: [JSONValue]), lines: [JSONValue], status: TerminationStatus) = try await streaming(["watch", fixture.identifier]) { lines in
                _ = try await lines.next { line in line[fixture.identifier] != nil }
                let pid: String = try await launch(fixture)
                let settled: JSONValue = try await lines.responded(fixture, pid: pid)
                let before: [JSONValue] = settled[fixture.identifier, pid, "running", "windows"]?.rows ?? []
                try await press(.window, in: fixture, pid: pid)
                let opened: JSONValue = try await lines.next { line in line[fixture.identifier, pid, "running", "windows"]?.rows.count == before.count + 1 }
                return (before, opened[fixture.identifier, pid, "running", "windows"]?.rows ?? [])
            }
            let dropped: Set<JSONValue> = Set(watched.outcome.before).subtracting(watched.outcome.after)
            expectNoDifference(watched.lines.first { line in line["system"] == nil }, .object([fixture.identifier: .object([:])]))
            expectNoDifference(dropped, [])
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: spells)
        func unresponsiveLineNamesItsProcessOnlyWhenStoppedAndRecovers(scenario: Scenario, resuming: Int32, process: JSONValue?) async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: JSONValue?, lines: [JSONValue], status: TerminationStatus) = try await streaming(["watch", fixture.identifier]) { lines in
                let pid: String = try await launch(fixture)
                _ = try await lines.responded(fixture, pid: pid)
                try await press(scenario, in: fixture, pid: pid)
                let spell: JSONValue = try await lines.next { line in fixture.status(in: line, of: pid) == .string("unresponsive") }
                try send(resuming, to: pid)
                _ = try await lines.responded(fixture, pid: pid)
                return spell[fixture.identifier, pid, "running", "process"]
            }
            expectNoDifference(watched.outcome, process)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func watchKeysASystemPromptNamingTheApplicationByItsHostUntilItCloses() async throws {
            let fixture: Fixture = try Fixture()
            let header: [CFString: String] = [kCFUserNotificationAlertHeaderKey: "\(fixture.name) quit unexpectedly."]
            let host: String = "com.apple.UserNotificationCenter"
            let watched: (outcome: JSONValue?, lines: [JSONValue], status: TerminationStatus) = try await streaming(["watch", fixture.identifier]) { lines in
                _ = try await lines.next { line in line[fixture.identifier] != nil }
                let created: CFUserNotification? = CFUserNotificationCreate(nil, 0, kCFUserNotificationPlainAlertLevel, nil, header as CFDictionary)
                let alert: CFUserNotification = try #require(created)
                defer { CFUserNotificationCancel(alert) }
                let prompt: JSONValue = try await lines.next { line in line[host] != nil }
                CFUserNotificationCancel(alert)
                let closed: JSONValue = .object([:])
                _ = try await lines.next { line in line[host] == closed }
                let held: JSONValue? = prompt[host]?.fields.values.first
                return held?["running", "asking"]?.rows.first?["kind"]
            }
            expectNoDifference(watched.outcome, .string("system"))
        }
    }
}
