import CustomDump
import PropertyBased
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

// --- [CONSTANTS] -----------------------------------------------------------------------

private let chess: String = "com.apple.Chess"

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

struct Names {
    @Test(arguments: [chess, "Chess", "chess", "CHESS", "Chéss", "CHÉSS"])
    func namesResolveToOneLineKeyedByBundleID(name: String) async throws {
        let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
            of: .product(named: "watch-apps"),
            arguments: ["read", name],
            as: WatchAppsLine.self,
        )
        expectNoDifference(run.lines.map(Printed.init), [.instances(key: chess)])
        expectNoDifference(run.status, .exited(0))
    }

    @Test
    func unknownNamesPrintOneFailLineKeyedByName() async {
        let commands: [String] = ["read", "capture", "launch"]
        await propertyCheck(count: 20, input: Gen.lowercaseLetter.string(of: 8...16), Gen.element(of: commands)) { letters, command in
            let name: String = "absent \(letters)"
            let command: String = try #require(command)
            let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: [command, name],
                as: WatchAppsLine.self,
            )
            expectNoDifference(run.lines.map(Printed.init), [.fail(key: name)])
            expectNoDifference(run.status, .exited(1))
        }
    }

    @Test
    func unknownNameFailsWhileOtherNamesRun() async throws {
        let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
            of: .product(named: "watch-apps"),
            arguments: ["read", "dev.rasm.absent", chess],
            as: WatchAppsLine.self,
        )
        expectNoDifference(Set(run.lines.map(Printed.init)), [.fail(key: "dev.rasm.absent"), .instances(key: chess)])
        expectNoDifference(run.lines.count, 2)
        expectNoDifference(run.status, .exited(1))
    }

    @Test(.timeLimit(.minutes(1)))
    func watchExitsOneWhenNoNameResolves() async throws {
        let names: [String] = ["dev.rasm.absent", "dev.rasm.absent.second"]
        let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
            of: .product(named: "watch-apps"),
            arguments: ["watch"] + names,
            as: WatchAppsLine.self,
        )
        let printed: [Printed] = run.lines.map(Printed.init)
        let failures: [Printed] = names.map(Printed.fail(key:))
        expectNoDifference(printed.filter { line in line != .system }, failures)
        expectNoDifference(run.status, .exited(1))
    }
}
