import CustomDump
import PropertyBased
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

// --- [CONSTANTS] -----------------------------------------------------------------------

private let absent: String = "dev.rasm.absent"

private let helps: [(arguments: [String], names: [String])] = [
    (["--help"], ["watch", "launch", "read", "capture", "perform", "set"]),
    (["watch", "--help"], ["<names>"]),
    (["launch", "--help"], ["<names>", "--new", "--hides", "--document", "--arguments", "--environment"]),
    (["read", "--help"], ["<names>", "--pid"]),
    (["capture", "--help"], ["<names>", "--pid", "--window", "--longest-side"]),
    (["perform", "--help"], ["<element>", "<action>"]),
    (["set", "--help"], ["<element>", "<value>"]),
]

private let malformed: [[String]] = [
    ["bogus"], ["watch"], ["launch"], ["read"], ["capture"], ["perform"], ["set"], ["perform", "AXPress"], ["read", absent, "--pid"], ["read", absent, "--pid", "1.5"],
    ["read", absent, "\(absent).second", "--pid", "1"], ["capture", absent, "\(absent).second", "--pid", "1"], ["capture", absent, "--longest-side", "0"],
    ["watch", absent, "--pid", "1"], ["read", absent, "--new"], ["launch", absent, "--document"], ["launch", absent, "--environment", "NAME"],
    ["perform", "element", "AXPress", "extra"], ["set", "element", "text", "extra"],
]

private let numericOptions: [(command: String, option: String)] = [("read", "--pid"), ("capture", "--pid"), ("capture", "--window"), ("capture", "--longest-side")]

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

    @Test
    func unknownOptionsAndNonNumericValuesPrintOneFailLineKeyedByWatchApps() async {
        let commands: [String] = ["watch", "launch", "read", "capture"]
        await propertyCheck(count: 30, input: Gen.lowercaseLetter.string(of: 1...12), Gen.element(of: commands), Gen.element(of: numericOptions.indices), Gen.bool) {
            letters,
            command,
            index,
            unknown in
            let option: (command: String, option: String) = numericOptions[try #require(index)]
            let arguments: [String] = unknown ? [try #require(command), absent, "--x\(letters)"] : [option.command, absent, option.option, letters]
            let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: arguments,
                as: WatchAppsLine.self,
            )
            expectNoDifference(run.lines.map(Printed.init), [Printed(key: "watch-apps", fails: true)], "\(arguments)")
            expectNoDifference(run.status, .exited(1))
        }
    }

    @Test
    func malformedElementIDsPrintOneFailLineAndExitOne() async {
        let invalidMarks: [Character] = Array(" !#$%&*,.:;?@^~")
        await propertyCheck(count: 40, input: Gen.letterOrNumber.string(of: 0...24), Gen.element(of: invalidMarks), Gen.bool) { text, mark, performs in
            let id: String = String(text.prefix(text.count / 2)) + String(try #require(mark)) + String(text.dropFirst(text.count / 2))
            let arguments: [String] = performs ? ["perform", id, "AXPress"] : ["set", id, "text"]
            let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: arguments,
                as: WatchAppsLine.self,
            )
            let printed: [Printed] = run.lines.map(Printed.init)
            let keyed: Bool = printed == [Printed(key: "watch-apps", fails: true)] || printed == [Printed(key: id, fails: true)]
            #expect(keyed, "\(arguments) printed \(run.lines)")
            expectNoDifference(run.status, .exited(1))
        }
    }
}
