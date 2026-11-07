import ApplicationServices
import CustomDump
import Encoding
import Foundation
import Subprocess
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

let product: String = "watch-apps"

// --- [MODELS] --------------------------------------------------------------------------

struct Fixture {
    let bundle: URL
    let identifier: String
    let name: String

    init() throws {
        bundle = try #require(TestApplication.current)
        let info: Bundle = try #require(Bundle(url: bundle))
        identifier = try #require(info.bundleIdentifier)
        name = try #require(info.object(forInfoDictionaryKey: kCFBundleNameKey as String) as? String)
    }

    func launching(_ settings: Settings = Settings(), flags: [String] = []) -> [String] {
        ["launch", identifier] + flags + settings.environment.flatMap { name, value in ["--environment", "\(name)=\(value)"] }
    }

    func launched(in line: JSONValue) -> String? { line[identifier]?.fields.first { _, state in state["launched"] != nil }?.key }

    func status(in line: JSONValue, of pid: String) -> JSONValue? { line[identifier, pid, "running", "status"] }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension JSONValue {
    var fields: [String: Self] { if case .object(let fields) = self { fields } else { [:] } }

    var rows: [Self] { if case .array(let rows) = self { rows } else { [] } }

    var argument: String? {
        switch self {
            case .string(let text): text
            case .number(let number): String(Int(number))
            case .null, .bool, .array, .object: nil
        }
    }

    subscript(_ path: String...) -> Self? { path.reduce(Optional(self)) { value, key in value?.fields[key] } }
}

extension [JSONValue] {
    func row(_ key: String, is text: String) -> JSONValue? { first { row in row[key] == .string(text) } }
}

extension AsyncStream<JSONValue>.Iterator {
    mutating func next(where matches: (JSONValue) -> Bool) async throws -> JSONValue {
        var last: JSONValue?
        while let line: JSONValue = await next(isolation: #isolation) {
            if matches(line) { return line }
            last = line
        }
        throw TestFailure(subject: "matching line, output ended after \(String(describing: last))", reason: .missing)
    }

    mutating func drain() async {
        while await next(isolation: #isolation) != nil {}
    }

    mutating func launched(_ fixture: Fixture) async throws -> String {
        try #require(fixture.launched(in: try await next { line in fixture.launched(in: line) != nil }))
    }

    mutating func responded(_ fixture: Fixture, pid: String) async throws -> JSONValue {
        try await next { line in fixture.status(in: line, of: pid) == .string("responding") }
    }
}

func tool(_ arguments: [String]) async throws -> (lines: [JSONValue], status: TerminationStatus) {
    let run: (lines: [JSONValue], status: TerminationStatus, standardError: String) = try await jsonLines(of: .product(named: product), arguments: arguments)
    return (run.lines, run.status)
}

func streaming<Outcome: Sendable>(
    _ arguments: [String],
    _ body: (_ lines: inout AsyncStream<JSONValue>.Iterator) async throws -> Outcome,
) async throws -> (outcome: Outcome, lines: [JSONValue], status: TerminationStatus) {
    let session: (outcome: Outcome, lines: [JSONValue], status: TerminationStatus) = try await streamingJSONLines(of: .product(named: product), arguments: arguments) { lines in
        var iterator: AsyncStream<JSONValue>.Iterator = lines.makeAsyncIterator()
        return try await body(&iterator)
    }
    let keyed: [String?: [JSONValue]] = Dictionary(grouping: session.lines) { line in line.fields.keys.first }
    expectNoDifference(keyed.filter { _, lines in zip(lines, lines.dropFirst()).contains { pair in pair.0 == pair.1 } }, [:])
    return session
}

func launch(_ fixture: Fixture, _ settings: Settings = Settings(), flags: [String] = []) async throws -> String {
    let launched: (lines: [JSONValue], status: TerminationStatus) = try await tool(fixture.launching(settings, flags: flags))
    expectNoDifference(launched.status, .exited(0))
    return try #require(launched.lines.lazy.compactMap(fixture.launched).first)
}

func whole(_ fixture: Fixture, pid: String, flags: [String] = []) async throws -> JSONValue {
    let read: (lines: [JSONValue], status: TerminationStatus) = try await tool(["read", "--pid", pid] + flags)
    expectNoDifference(read.status, .exited(0))
    return try #require(read.lines.first?[fixture.identifier, pid, "running"])
}

func press(_ scenario: Scenario, in fixture: Fixture, pid: String) async throws {
    let bar: JSONValue? = try await whole(fixture, pid: pid)["menus"]?.rows.row("role", is: kAXMenuBarRole)
    let items: [JSONValue]? = bar?["detail", "controls"]?.rows.row("label", is: String(describing: Scenario.self))?["items"]?.rows
    let item: String = try #require(items?.row("label", is: scenario.rawValue)?["id"]?.argument, "menu item \(scenario)")
    let performed: (lines: [JSONValue], status: TerminationStatus) = try await tool(["perform", item, kAXPressAction])
    expectNoDifference(performed.status, .exited(0))
}

func send(_ signal: Int32, to pid: String) throws {
    expectNoDifference(kill(try #require(pid_t(pid)), signal), 0)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.serialized, .timeLimit(.minutes(2)))
struct Integration {}
