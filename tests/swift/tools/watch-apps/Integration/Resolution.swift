import CasePaths
import CustomDump
import Foundation
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Resolution {
        @Test(.application(FixtureApplication.self, launches: false))
        func displayNameResolvesToLinesKeyedByBundleID() async throws {
            let fixture: Fixture = try Fixture()
            try await spotlightIndexed(fixture.bundle)
            for name: String in [fixture.name, fixture.name.lowercased(), fixture.name.uppercased()] {
                let read: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                    of: .product(named: "watch-apps"),
                    arguments: ["read", name],
                    as: WatchAppsLine.self,
                )
                expectNoDifference(read.lines.map(\.key), [fixture.identifier])
                expectNoDifference(read.lines.map(\.entry), [.instances([:])])
                expectNoDifference(read.status, .exited(0))
            }
            let launched: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture, named: fixture.name)
            expectNoDifference(Set(launched.lines.map(\.key)), [fixture.identifier])
        }

        @Test(.application(FixtureApplication.self, named: "AppKitFixture shared \(UUID().uuidString.prefix(8))", launches: false), .timeLimit(.minutes(1)))
        func sharedDisplayNameFailsListingEachBundleID() async throws {
            let first: Fixture = try Fixture()
            let test: Test = try #require(Test.current)
            try await TestApplication.application(FixtureApplication.self, named: first.name, launches: false).provideScope(for: test, testCase: Test.Case.current) {
                let second: Fixture = try Fixture()
                try await spotlightIndexed(first.bundle)
                try await spotlightIndexed(second.bundle)
                for command: String in ["read", "launch", "watch"] {
                    let refused: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                        of: .product(named: "watch-apps"),
                        arguments: [command, first.name],
                        as: WatchAppsLine.self,
                    )
                    let failures: [WatchAppsLine] = refused.lines.filter { line in line.key != "system" }
                    expectNoDifference(failures.map(\.key), [first.name])
                    let reason: String = try #require(failures.first?.entry.fail)
                    let unlisted: [String] = [first.identifier, second.identifier].filter { identifier in !reason.contains(identifier) }
                    expectNoDifference(unlisted, [], "\(reason)")
                    expectNoDifference(refused.status, .exited(1))
                }
                let read: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                    of: .product(named: "watch-apps"),
                    arguments: ["read", first.identifier, second.identifier],
                    as: WatchAppsLine.self,
                )
                expectNoDifference(read.lines.map(\.entry), [.instances([:]), .instances([:])])
            }
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func unknownNameFailsWhileWatchFollowsOthers() async throws {
            let fixture: Fixture = try Fixture()
            let absent: String = "dev.rasm.absent.\(fixture.identifier)"
            let watched: (outcome: Int32, lines: [TimedLine<WatchAppsLine>]) = try await watching([absent, fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                return pid
            }
            let transcript: [WatchAppsLine] = watched.lines.map(\.line)
            let failed: [Bool] = transcript.filter { line in line.key == absent }.map { line in line.entry.fail != nil }
            let start: WatchAppsLine.Entry? = transcript.first { line in line.key == fixture.identifier }?.entry
            let end: [Phase] = phases(of: watched.outcome, in: Array(transcript.suffix(1)))
            expectNoDifference(failed, [true])
            expectNoDifference(start, .instances([:]))
            expectNoDifference(end, [.responding])
            expectNoDifference(pids(in: Array(transcript.suffix(1))), [[watched.outcome]])
        }
    }
}
