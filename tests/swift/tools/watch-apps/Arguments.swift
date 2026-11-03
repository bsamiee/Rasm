import CustomDump
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

// --- [CONSTANTS] -----------------------------------------------------------------------

private let helps: [(arguments: [String], names: [String])] = [
    (["--help"], ["watch", "launch", "show", "read", "capture", "perform", "set", "quit"]),
    (["watch", "--help"], ["<names>"]),
    (["launch", "--help"], ["<names>", "--new", "--document", "--arguments", "--environment"]),
    (["show", "--help"], ["<names>", "--pid"]),
    (["read", "--help"], ["<names>", "--pid"]),
    (["capture", "--help"], ["<names>", "--pid", "--window", "--longest-side"]),
    (["perform", "--help"], ["<id>", "<action>"]),
    (["set", "--help"], ["<id>", "<value>"]),
    (["quit", "--help"], ["<names>", "--pid", "--force"]),
]

private let malformed: [[String]] = { (absent: String) -> [[String]] in
    [
        ["undeclared"], ["watch"], ["read", absent, "--undeclared"], ["read", absent, "--pid", "text"], ["read", absent, "\(absent).second", "--pid", "1"],
        ["capture", absent, "--longest-side", "0"], ["launch", absent, "--environment", "NAME"], ["launch", absent, "--document", "/\(absent)/absent.txt"],
        ["perform", "not base64!", "AXPress"], ["set", "", "text"], ["quit", absent, "--pid", "text"],
    ]
}("dev.rasm.absent")

// --- [MODELS] --------------------------------------------------------------------------

private struct Printed: Equatable {
    let key: String
    let fails: Bool

    init(key: String, fails: Bool) {
        self.key = key
        self.fails = fails
    }

    init(_ line: WatchAppsLine) {
        key = line.key
        fails = if case .fail = line.entry { true } else { false }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Arguments {
    @Test(arguments: helps)
    func helpExitsZeroNamingEachFlag(arguments: [String], names: [String]) async throws {
        let help: (standardOutput: String, status: TerminationStatus, standardError: String) = try await output(of: .product(named: "watch-apps"), arguments: arguments)
        let missing: [String] = names.filter { name in !help.standardOutput.contains(name) }
        let lines: [Substring] = help.standardOutput.split(whereSeparator: \.isNewline)
        expectNoDifference(help.status, .exited(0))
        expectNoDifference(missing, [])
        expectNoDifference(lines.filter { line in line.hasPrefix("{") }, [])
    }

    @Test(arguments: malformed)
    func malformedArgumentsPrintOneFailLineKeyedByWatchApps(arguments: [String]) async throws {
        let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
            of: .product(named: "watch-apps"),
            arguments: arguments,
            as: WatchAppsLine.self,
        )
        expectNoDifference(run.lines.map(Printed.init), [Printed(key: "watch-apps", fails: true)])
        expectNoDifference(run.status, .exited(1))
    }
}
