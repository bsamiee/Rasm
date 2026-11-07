import ApplicationServices
import Carbon.HIToolbox
import CustomDump
import Encoding
import Subprocess
import TestSupport
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let absent: String = "dev.rasm.absent"
private let absentProcess: String = String(Int32.max)

private let helps: [(arguments: [String], names: [String])] = [
    (["--help"], ["watch", "launch", "read", "capture", "perform", "set", "key", "quit", "log", "serve"]),
    (["watch", "--help"], ["<names>"]),
    (["launch", "--help"], ["<names>", "--new", "--wait", "--document", "--environment", "<argument>"]),
    (["read", "--help"], ["<names>", "--window"]),
    (["capture", "--help"], ["<names>", "--window", "--longest-side"]),
    (["perform", "--help"], ["<id>", "<action>"]),
    (["set", "--help"], ["<id>", "<value>"]),
    (["key", "--help"], ["<pid>", "<code>"]),
    (["quit", "--help"], ["<names>", "--force"]),
    (["log", "--help"], ["<pid>"]),
]

private let malformed: [[String]] = [
    ["undeclared"], ["watch"], ["read"], ["read", absent, "--undeclared"],
    ["capture", absent, "--longest-side", "0"], ["launch"], ["launch", absent, "--environment", "NAME"], ["launch", absent, "\(absent).second", "--wait"],
    ["launch", absent, "\(absent).second", "--document", "/\(absent)/absent.txt"], ["launch", "--wait", "--document", "/\(absent)/absent.txt"], ["perform", "not base64!", kAXPressAction],
    ["set", "not base64!", "text"], ["key", "text", String(kVK_Return)], ["key", "1", "text"],
]

private let failures: [(arguments: [String], key: String, reason: String)] = [
    (["launch", absent], absent, "unmatched"),
    (["launch", "--document", "/\(absent)/absent.txt"], "/\(absent)/absent.txt", "host"),
    (["key", absentProcess, String(kVK_Return)], absentProcess, "noProcess"),
]

private let refusals: [(arguments: [String], key: String, reason: String)] = failures + malformed.map { arguments in (arguments, "watch-apps", "malformed") }

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
        let run: (lines: [[String: [String: [String: JSONValue]]]], status: TerminationStatus, standardError: String) = try await jsonLines(of: .product(named: "watch-apps"), arguments: arguments)
        let reasons: [JSONValue?] = run.lines.map { line in line[key]?["fail"]?["reason"] }
        expectNoDifference(reasons, [.string(reason)])
        expectNoDifference(run.status, .exited(1))
    }
}
