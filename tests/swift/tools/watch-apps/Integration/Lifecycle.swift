import ApplicationServices
import CustomDump
import Encoding
import Foundation
import Subprocess
import System
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let quits: [(scenario: Scenario?, flags: [String], status: Int32)] = [(nil, [], 0), (.sheet, [], 1), (.sheet, ["--force"], 0)]

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Lifecycle {
        @Test(
            .enabled(if: ProcessInfo.processInfo.environment["RASM_PROCESS_SWIFT_TEST"] != nil),
            .application(FixtureApplication.self, launches: false),
            .testDirectory,
            .timeLimit(.minutes(1)),
        )
        func rustProcessLeaseReleasesTheNativeApplication() async throws {
            let executable: String = try #require(ProcessInfo.processInfo.environment["RASM_PROCESS_SWIFT_TEST"])
            let fixture: Fixture = try Fixture()
            let evidence: URL = try #require(TestDirectory.current)
            let log: URL = evidence.appending(component: "watch-apps.jsonl")
            let result: ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>> = try await Subprocess.run(
                .path(FilePath(executable)),
                arguments: Arguments(["--ignored", "--exact", "swift_fixture_opens_documents_in_the_background_until_lease_eof", "--nocapture"]),
                environment: .inherit.updating([
                    "RASM_PROCESS_SWIFT_HELPER": try builtProduct(named: product).path(percentEncoded: false),
                    "RASM_PROCESS_SWIFT_BUNDLE": fixture.bundle.path(percentEncoded: false),
                    "RASM_PROCESS_SWIFT_DIRECTORY": evidence.path(percentEncoded: false),
                    "WATCH_APPS_LOG": log.path(percentEncoded: false),
                ]),
                output: .string(limit: 1 << 20),
                error: .string(limit: 1 << 20),
            )
            let transcript: URL = evidence.appending(component: "rust-appkit.log")
            try Data((result.standardOutput + result.standardError).utf8).write(to: transcript)
            Attachment.record(try await Attachment(contentsOf: transcript))
            Attachment.record(try await Attachment(contentsOf: evidence.appending(component: "rust-events.jsonl")))
            Attachment.record(try await Attachment(contentsOf: log))
            expectNoDifference(result.terminationStatus, .exited(0))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchPrintsLaunchedThenOutlineUntilSettled() async throws {
            let fixture: Fixture = try Fixture()
            let ended: (outcome: (pid: String, held: JSONValue?), lines: [JSONValue], status: TerminationStatus) = try await streaming(fixture.launching(Settings(scenario: .slowLaunch))) { lines in
                let pid: String = try await lines.launched(fixture)
                let held: JSONValue = try await lines.next { line in fixture.status(in: line, of: pid) != nil }
                try send(SIGUSR1, to: pid)
                await lines.drain()
                return (pid, fixture.status(in: held, of: pid))
            }
            let pid: String = ended.outcome.pid
            let rows: [JSONValue] = try await whole(fixture, pid: pid)["windows"]?.rows ?? []
            let windows: [JSONValue] = rows.filter { row in row["presented"] == nil }.compactMap { row in row["number"] }
            let settled: JSONValue = .object(["active": .bool(false), "asking": .array([]), "menus": .array([]), "status": .string("responding"), "windows": .array(windows)])
            expectNoDifference(ended.lines.first, .object([fixture.identifier: .object([pid: .object(["launched": .object(["created": .bool(true)])])])]))
            expectNoDifference(ended.outcome.held, .string("launching"))
            expectNoDifference(ended.lines.last?[fixture.identifier, pid, "running"], settled)
            expectNoDifference(ended.status, .exited(0))
        }

        @Test(.application(FixtureApplication.self, launches: false), .timeLimit(.minutes(1)), arguments: [false, true])
        func interruptedLaunchLeavesTheInstanceRunningAndFailsOnlyUnderWait(wait: Bool) async throws {
            let fixture: Fixture = try Fixture()
            let flags: [String] = wait ? ["--wait"] : []
            let ended: (outcome: String, lines: [JSONValue], status: TerminationStatus) = try await streaming(fixture.launching(Settings(scenario: .slowLaunch), flags: flags)) { lines in
                try await lines.launched(fixture)
            }
            try send(0, to: ended.outcome)
            expectNoDifference(ended.status, .exited(wait ? 1 : 0))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchEndsAtAnAskingOutlineAndWaitFollowsItsAnswerToTheExitStatus() async throws {
            let fixture: Fixture = try Fixture()
            let settings: Settings = Settings(scenario: .launchModal, status: 7)
            let ended: (outcome: String, lines: [JSONValue], status: TerminationStatus) = try await streaming(fixture.launching(settings, flags: ["--wait"])) { lines in
                let pid: String = try await lines.launched(fixture)
                let asked: JSONValue = try await lines.next { line in line[fixture.identifier, pid, "running", "asking"]?.rows.isEmpty == false }
                let row: JSONValue = try #require(asked[fixture.identifier, pid, "running", "asking"]?.rows.first)
                let walked: JSONValue = try await whole(fixture, pid: pid, flags: ["--window", try #require(row["number"]?.argument)])
                let answer: JSONValue? = walked["windows"]?.rows.first { window in window["asking"] == .bool(true) }?["element", "answers", "accept", "id"]
                expectNoDifference(row["kind"], .string("alert"))
                expectNoDifference(row["text"], .array([.string("Launch modal"), .string(Scenario.launchModal.rawValue)]))
                expectNoDifference(Set(row["options"]?.rows ?? []), [.string("OK"), .string("Cancel")])
                let performed: (lines: [JSONValue], status: TerminationStatus) = try await tool(["perform", try #require(answer?.argument), kAXPressAction])
                expectNoDifference(performed.status, .exited(0))
                _ = try await lines.responded(fixture, pid: pid)
                try await press(.exit, in: fixture, pid: pid)
                await lines.drain()
                return pid
            }
            expectNoDifference(ended.lines.last?[fixture.identifier, ended.outcome, "exited", "status"], .number(Double(settings.status)))
            expectNoDifference(ended.status, .exited(settings.status))
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: [Int32(0), 7])
        func launchWaitReturnsTheStatusOfAnExitDuringStartup(status: Int32) async throws {
            let fixture: Fixture = try Fixture()
            let ended: (lines: [JSONValue], status: TerminationStatus) = try await tool(fixture.launching(Settings(scenario: .exit, status: status), flags: ["--wait"]))
            let pid: String = try #require(ended.lines.lazy.compactMap(fixture.launched).first)
            expectNoDifference(ended.lines.last?[fixture.identifier, pid, "exited", "status"], .number(Double(status)))
            expectNoDifference(ended.status, .exited(status))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchWaitReusesTheRunningInstanceAndReturnsTheShellStatusOfItsSignal() async throws {
            let fixture: Fixture = try Fixture()
            let pid: String = try await launch(fixture)
            let ended: (outcome: Void, lines: [JSONValue], status: TerminationStatus) = try await streaming(fixture.launching(flags: ["--wait"])) { lines in
                _ = try await lines.responded(fixture, pid: pid)
                try send(SIGKILL, to: pid)
                await lines.drain()
            }
            expectNoDifference(ended.lines.first?[fixture.identifier, pid, "launched", "created"], .bool(false))
            expectNoDifference(ended.lines.last?[fixture.identifier, pid, "exited", "signal"], .number(Double(SIGKILL)))
            expectNoDifference(ended.status, .exited(128 + SIGKILL))
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: quits)
        func quitExitsZeroOnlyWhenTheInstanceExited(scenario: Scenario?, flags: [String], status: Int32) async throws {
            let fixture: Fixture = try Fixture()
            let pid: String = try await launch(fixture)
            if let scenario { try await press(scenario, in: fixture, pid: pid) }
            let quit: (lines: [JSONValue], status: TerminationStatus) = try await tool(["quit", "--pid", pid] + flags)
            let exited: Bool = quit.lines.last?[fixture.identifier, pid, "exited"] != nil
            expectNoDifference(exited, status == 0)
            expectNoDifference(quit.status, .exited(status))
        }

        @Test(.application(FixtureApplication.self, launches: false), .testDirectory, .timeLimit(.minutes(1)))
        func watcherExitsWhenItsParentExits() async throws {
            let fixture: Fixture = try Fixture()
            let release: String = try #require(TestDirectory.current).appending(path: "release").path(percentEncoded: false)
            try #require(unsafe mkfifo(release, 0o600) == 0)
            let parent: (outcome: Void, lines: [JSONValue], status: TerminationStatus) = try await streamingJSONLines(
                of: .name("sh"),
                arguments: ["-c", "\"$0\" watch \"$1\" & read -r _ < \"$2\"", try builtProduct(named: product).path(percentEncoded: false), fixture.identifier, release],
            ) { lines in
                var iterator: AsyncStream<JSONValue>.Iterator = lines.makeAsyncIterator()
                _ = try await iterator.next { line in line[fixture.identifier] != nil }
                let writer: FileDescriptor = try FileDescriptor.open(FilePath(release), .writeOnly)
                _ = try writer.closeAfter { try writer.writeAll("\n".utf8) }
                await iterator.drain()
            }
            expectNoDifference(parent.status, .exited(0))
            expectNoDifference(parent.lines.filter { line in line["system"] == nil }, [.object([fixture.identifier: .object([:])])])
        }
    }
}
