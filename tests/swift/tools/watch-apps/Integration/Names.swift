import CasePaths
import CustomDump
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let absent: String = "dev.rasm.absent"

// --- [MODELS] --------------------------------------------------------------------------

private enum Printed: Hashable {
    case fail(key: String)
    case instances(key: String)
    case system

    init(_ line: WatchAppsLine) {
        self =
            switch line.entry {
                case .fail: .fail(key: line.key)
                case .instances: .instances(key: line.key)
                case .system: .system
            }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Names {
        @Test(arguments: ["read", "capture", "launch"])
        func unknownNamePrintsOneFailLineKeyedByName(command: String) async throws {
            let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: [command, absent],
                as: WatchAppsLine.self,
            )
            expectNoDifference(run.lines.map(Printed.init), [.fail(key: absent)])
            expectNoDifference(run.status, .exited(1))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func unknownNameFailsWhileOtherNamesRun() async throws {
            let fixture: Fixture = try Fixture()
            let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["read", absent, fixture.identifier],
                as: WatchAppsLine.self,
            )
            expectNoDifference(Set(run.lines.map(Printed.init)), [.fail(key: absent), .instances(key: fixture.identifier)])
            expectNoDifference(run.lines.count, 2)
            expectNoDifference(run.status, .exited(1))
        }

        @Test(.timeLimit(.minutes(1)))
        func watchExitsOneWhenNoNameResolves() async throws {
            let names: [String] = [absent, "\(absent).second"]
            let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["watch"] + names,
                as: WatchAppsLine.self,
            )
            let printed: [Printed] = run.lines.map(Printed.init).filter { line in line != .system }
            let failures: [Printed] = names.map(Printed.fail(key:))
            expectNoDifference(printed, failures)
            expectNoDifference(run.status, .exited(1))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func unknownNameFailsWhileWatchFollowsOthers() async throws {
            let fixture: Fixture = try Fixture()
            let missing: String = "\(absent).\(fixture.identifier)"
            let (pid, transcript): (Int32, [WatchAppsLine]) = try await watching([missing, fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                return pid
            }
            let failed: [Bool] = transcript.filter { line in line.key == missing }.map { line in line.entry.fail != nil }
            let start: WatchAppsLine.Entry? = transcript.first { line in line.key == fixture.identifier }?.entry
            let end: [Phase] = phases(of: pid, in: Array(transcript.suffix(1)))
            expectNoDifference(failed, [true])
            expectNoDifference(start, .instances([:]))
            expectNoDifference(end, [.responding])
            expectNoDifference(pids(in: Array(transcript.suffix(1))), [[pid]])
        }
    }
}
