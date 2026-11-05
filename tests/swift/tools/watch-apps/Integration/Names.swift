import CustomDump
import Encoding
import Foundation
import Subprocess
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let absent: String = "dev.rasm.absent"

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Names {
        @Test(arguments: ["read", "capture", "quit"])
        func nameNothingRunsUnderPrintsAnEmptyLineAndExitsZero(command: String) async throws {
            let run: (lines: [JSONValue], status: TerminationStatus) = try await tool([command, absent])
            expectNoDifference(run.lines, [.object([absent: .object([:])])])
            expectNoDifference(run.status, .exited(0))
        }

        @Test(.timeLimit(.minutes(1)))
        func watchExitsOneWhenNoNameResolves() async throws {
            let run: (lines: [JSONValue], status: TerminationStatus) = try await tool(["watch", absent])
            let reasons: [JSONValue?] = run.lines.filter { line in line["system"] == nil }.map { line in line[absent, "fail", "reason"] }
            expectNoDifference(reasons, [.string("unmatched")])
            expectNoDifference(run.status, .exited(1))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func watchFailsAnUninstalledNameFollowsTheOthersAndExitsOne() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: JSONValue, lines: [JSONValue], status: TerminationStatus) = try await streaming(["watch", absent, fixture.identifier]) { lines in
                let empty: JSONValue = try await lines.next { line in line[fixture.identifier] != nil }
                let pid: String = try await launch(fixture)
                _ = try await lines.responded(fixture, pid: pid)
                return empty
            }
            let reason: JSONValue? = watched.lines.lazy.compactMap { line in line[absent, "fail", "reason"] }.first
            expectNoDifference(watched.outcome, .object([fixture.identifier: .object([:])]))
            expectNoDifference(reason, .string("unmatched"))
            expectNoDifference(watched.status, .exited(1))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func eachNameFormReadsTheRunningInstance() async throws {
            let fixture: Fixture = try Fixture()
            let pid: String = try await launch(fixture)
            let bundle: String = try #require(TestApplication.current).path(percentEncoded: false)
            for form: String in [fixture.identifier, fixture.identifier.uppercased(), fixture.name, fixture.name.uppercased(), String(fixture.name.dropLast()), bundle] {
                let read: (lines: [JSONValue], status: TerminationStatus) = try await tool(["read", form])
                let pids: [String?] = read.lines.map { line in line[fixture.identifier]?.fields.keys.first }
                expectNoDifference(pids, [pid], form)
            }
        }
    }
}
