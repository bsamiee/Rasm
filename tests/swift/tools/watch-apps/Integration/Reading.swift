import ApplicationServices
import Carbon.HIToolbox
import CustomDump
import Encoding
import Subprocess
import TestSupport
import Testing

@testable import AppKitFixture

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Reading {
        @Test(.application(FixtureApplication.self, launches: false))
        func readPrintsWholeStateWithIdsAndWindowWalksOneRow() async throws {
            let fixture: Fixture = try Fixture()
            let pid: String = try await launch(fixture, Settings(windows: 2))
            let state: JSONValue = try await whole(fixture, pid: pid)
            let described: [JSONValue] = (state["windows"]?.rows ?? []).filter { row in row["kind"] == .string("window") }
            let number: String = try #require(described.first?["number"]?.argument)
            let one: JSONValue = try await whole(fixture, pid: pid, flags: ["--window", number])
            let walked: [String?] = (one["windows"]?.rows ?? []).filter { row in row["element", "detail"] != nil }.map { row in row["number"]?.argument }
            _ = try #require(state["usage"], "read holds usage")
            _ = try #require(state["menus"]?.rows.first?["id"], "read holds menu ids")
            expectNoDifference(described.count, 2)
            expectNoDifference(described.filter { row in row["element", "id"] == nil || row["element", "detail"] == nil }, [])
            expectNoDifference(walked, [number])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func nameReadsEachInstanceOnItsOwnLineAndPidSelectsOne() async throws {
            let fixture: Fixture = try Fixture()
            let first: String = try await launch(fixture)
            let second: String = try await launch(fixture, flags: ["--new"])
            let all: (lines: [JSONValue], status: TerminationStatus) = try await tool(["read", fixture.identifier])
            let one: (lines: [JSONValue], status: TerminationStatus) = try await tool(["read", second])
            let read: [String] = all.lines.compactMap { line in line[fixture.identifier]?.fields.keys.first }
            expectNoDifference(read.sorted(), [first, second].sorted())
            expectNoDifference(one.lines.map { line in line[fixture.identifier]?.fields.keys.first }, [second])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func actionsPrintTheNextStateKeyedByOwnerOrOneFailKeyedById() async throws {
            let fixture: Fixture = try Fixture()
            let pid: String = try await launch(fixture)
            let window: JSONValue? = try await whole(fixture, pid: pid)["windows"]?.rows.first { row in row["element", "title"] == .string("Fixture 0") }?["element"]
            let controls: [JSONValue] = window?["detail", "controls"]?.rows ?? []
            let field: String = try #require(controls.row("identifier", is: "field")?["id"]?.argument)
            let button: String = try #require(controls.row("identifier", is: "press")?["id"]?.argument)
            let element: String = try #require(window?["id"]?.argument)
            for arguments: [String] in [["set", field, "typed text"], ["perform", button, kAXPressAction], ["key", pid, String(kVK_Return)]] {
                let acted: (lines: [JSONValue], status: TerminationStatus) = try await tool(arguments)
                expectNoDifference(acted.lines.last.flatMap { line in fixture.status(in: line, of: pid) }, .string("responding"), "\(arguments)")
                expectNoDifference(acted.status, .exited(0), "\(arguments)")
            }
            for (arguments, key): ([String], String) in [(["set", button, "text"], button), (["perform", element, kAXRaiseAction], element)] {
                let refused: (lines: [JSONValue], status: TerminationStatus) = try await tool(arguments)
                expectNoDifference(refused.lines.map { line in line[key, "fail"] != nil }, [true], "\(arguments)")
                expectNoDifference(refused.status, .exited(1), "\(arguments)")
            }
        }
    }
}
