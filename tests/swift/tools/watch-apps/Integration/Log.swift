import CustomDump
import Encoding
import Foundation
import Subprocess
import TestSupport
import Testing

@testable import AppKitFixture
@testable import watch_apps

// --- [OPERATIONS] ----------------------------------------------------------------------

private func logged(_ environment: [String], _ arguments: [String]) async throws -> (lines: [JSONValue], status: TerminationStatus) {
    let run: (lines: [JSONValue], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .name("env"),
        arguments: environment + [try builtProduct(named: product).path(percentEncoded: false)] + arguments,
    )
    return (run.lines, run.status)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Logs {
        @Test(.application(FixtureApplication.self, launches: false), .testDirectory)
        func logKeepsWholeStatesAndPrintsEachRunOfItsSession() async throws {
            let fixture: Fixture = try Fixture()
            let log: String = "\(logVariable)=\(try #require(TestDirectory.current).appending(component: "log").path(percentEncoded: false))"
            let session: String = UUID().uuidString
            let mine: [String] = [log, "\(sessionVariable)=\(session)"]
            let launched: (lines: [JSONValue], status: TerminationStatus) = try await logged(mine, fixture.launching())
            let pid: String = try #require(launched.lines.lazy.compactMap(fixture.launched).first)
            _ = try await logged([log, "\(sessionVariable)=other"], ["read", "--pid", pid])
            let folded: (lines: [JSONValue], status: TerminationStatus) = try await logged(mine, ["log"])
            let run: JSONValue = try #require(folded.lines.first)
            let written: String = try #require(run["pid"]?.argument)
            let records: (lines: [JSONValue], status: TerminationStatus) = try await logged(mine, ["log", "--pid", written])
            let created: [JSONValue] = records.lines.compactMap { record in record["line", fixture.identifier, pid, "launched", "created"] }
            expectNoDifference(folded.lines.count, 1)
            expectNoDifference(run["session"], .string(session))
            expectNoDifference(run["exit"], .object(["status": .number(0)]))
            _ = try #require(run["line", fixture.identifier, pid, "running", "windows"]?.rows.first?["frame"], "log holds whole rows where lines hold numbers")
            _ = try #require(records.lines.first?["argv"], "first record of a run holds its arguments")
            expectNoDifference(created, [.bool(true)])
            expectNoDifference(records.lines.last?["exit"], .object(["status": .number(0)]))
            expectNoDifference(records.status, .exited(0))
        }

        @Test
        func logWithoutItsVariablePrintsOneFailLine() async throws {
            let run: (lines: [JSONValue], status: TerminationStatus) = try await logged(["-u", logVariable], ["log"])
            let subjects: [JSONValue?] = run.lines.map { line in line[product, "fail", "subject"] }
            expectNoDifference(subjects, [.string(logVariable)])
            expectNoDifference(run.status, .exited(1))
        }
    }
}
