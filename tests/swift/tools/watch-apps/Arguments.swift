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

private let refusals: [(arguments: [String], key: String, reason: String)] = { (absent: String) -> [(arguments: [String], key: String, reason: String)] in
    [
        (["undeclared"], "watch-apps", "malformed"), (["watch"], "watch-apps", "malformed"), (["read", absent, "--undeclared"], "watch-apps", "malformed"),
        (["read", absent, "--pid", "text"], "watch-apps", "malformed"), (["read", absent, "\(absent).second", "--pid", "1"], "watch-apps", "malformed"),
        (["capture", absent, "--longest-side", "0"], "watch-apps", "malformed"), (["launch", absent, "--environment", "NAME"], "watch-apps", "malformed"),
        (["launch", "--document", "/\(absent)/absent.txt"], "/\(absent)/absent.txt", "host"), (["perform", "not base64!", "AXPress"], "watch-apps", "malformed"),
        (["set", "", "text"], "watch-apps", "malformed"), (["quit", absent, "--pid", "text"], "watch-apps", "malformed"),
    ]
}("dev.rasm.absent")

// --- [MODELS] --------------------------------------------------------------------------

private struct Printed: Equatable {
    let key: String
    let reason: String?

    init(key: String, reason: String?) {
        self.key = key
        self.reason = reason
    }

    init(_ line: WatchAppsLine) {
        key = line.key
        reason = if case .fail(let failure) = line.entry { failure.reason } else { nil }
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

    @Test(arguments: refusals)
    func refusedArgumentsPrintOneFailLineNamingItsReason(arguments: [String], key: String, reason: String) async throws {
        let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
            of: .product(named: "watch-apps"),
            arguments: arguments,
            as: WatchAppsLine.self,
        )
        expectNoDifference(run.lines.map(Printed.init), [Printed(key: key, reason: reason)])
        expectNoDifference(run.status, .exited(1))
    }
}
