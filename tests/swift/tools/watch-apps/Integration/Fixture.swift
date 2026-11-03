import ApplicationServices
import CasePaths
import CustomDump
import Foundation
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [MODELS] --------------------------------------------------------------------------

struct Fixture {
    let identifier: String
    let name: String

    init() throws {
        let bundle: URL = try #require(TestApplication.current)
        let info: Bundle = try #require(Bundle(url: bundle))
        identifier = try #require(info.bundleIdentifier)
        name = try #require(info.object(forInfoDictionaryKey: kCFBundleNameKey as String) as? String)
    }
}

struct Button: Hashable {
    static let alert: Set<Self> = [
        Self(identifier: "action-button-1", label: "OK", attribute: kAXDefaultButtonAttribute), Self(identifier: "action-button-2", label: "Cancel", attribute: kAXCancelButtonAttribute),
    ]

    let identifier: String?
    let label: String?
    let attribute: String?
}

struct Answer: Equatable {
    let role: String?
    let modal: Bool?
    let description: String?
    let identifier: String?
    let requiresAnswer: Bool?
    let buttons: Set<Button>

    init(role: String?, modal: Bool?, description: String?, identifier: String?, requiresAnswer: Bool?, buttons: Set<Button>) {
        self.role = role
        self.modal = modal
        self.description = description
        self.identifier = identifier
        self.requiresAnswer = requiresAnswer
        self.buttons = buttons
    }

    init(_ element: WatchAppsLine.Element, modal: Bool = false) {
        role = element.role
        self.modal = modal ? element.modal : nil
        description = element.description
        identifier = element.identifier
        requiresAnswer = element.requiresAnswer
        buttons = Set(
            (element.detail?.controls ?? []).filter { control in control.role == kAXButtonRole && control.subrole == nil }
                .map { control in Button(identifier: control.identifier, label: control.label, attribute: control.attribute) }
        )
    }
}

enum Phase: Hashable {
    case launching
    case responding
    case answering
    case unresponsive(WatchAppsLine.ProcessStatus)
    case exited
    case failed
    case captured
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WatchAppsLine {
    func elements(of pid: Int32) -> [WatchAppsLine.Element]? { entry.instances?[pid]?.running?.status.responding?.elements }

    func windows(of pid: Int32) -> [WatchAppsLine.Window]? { entry.instances?[pid]?.running?.status.windows }

    func prompts(besides application: String) -> Bool { key != application && key != "system" && entry.instances?.isEmpty == false }
}

extension WatchAppsLine.Instance {
    var phase: Phase {
        switch self {
            case .running(let running):
                switch running.status {
                    case .launching: .launching
                    case .responding(let elements, _) where elements.answers.isEmpty: .responding
                    case .responding: .answering
                    case .unresponsive(let process, _): .unresponsive(process)
                }
            case .failed: .failed
            case .exited: .exited
            case .captured: .captured
        }
    }
}

extension [WatchAppsLine.Element] {
    var answers: [WatchAppsLine.Element] { filter { element in element.requiresAnswer == true } }

    var menuBar: WatchAppsLine.Element? { first { element in element.role == kAXMenuBarRole } }

    func titled(_ title: String) -> WatchAppsLine.Element? { first { element in element.title == title } }
}

extension WatchAppsLine.Element {
    func control(_ identifier: String) -> WatchAppsLine.Control? { detail?.controls?.first { control in control.identifier == identifier } }

    func control(labeled label: String) -> WatchAppsLine.Control? { detail?.controls?.first { control in control.label == label } }

    func item(_ scenario: Scenario) -> WatchAppsLine.Control? { control(labeled: String(describing: Scenario.self))?.items?.first { item in item.label == scenario.rawValue } }
}

extension AsyncStream<WatchAppsLine>.Iterator {
    mutating func next(sourceLocation: SourceLocation = #_sourceLocation, where matches: (WatchAppsLine) -> Bool) async throws -> WatchAppsLine {
        let seen: [WatchAppsLine] = await collect { seen in seen.last.map(matches) == true }
        let matched: WatchAppsLine? = seen.last.flatMap { line in matches(line) ? line : nil }
        return try #require(matched, "watch output ended after \(seen.count) lines with none matching, last \(seen.suffix(3))", sourceLocation: sourceLocation)
    }

    mutating func collect(until done: ([WatchAppsLine]) -> Bool, after seen: [WatchAppsLine] = []) async -> [WatchAppsLine] {
        if !done(seen), let line: WatchAppsLine = await next(isolation: #isolation) {
            await collect(until: done, after: seen + [line])
        } else {
            seen
        }
    }
}

func pids(in lines: [WatchAppsLine]) -> [[Int32]] { lines.map { line in (line.entry.instances.map { instances in Array(instances.keys) } ?? []).sorted() } }

func phases(of pid: Int32, in lines: [WatchAppsLine]) -> [Phase] { lines.compactMap { line in line.entry.instances?[pid]?.phase } }

func phases(in lines: some Sequence<WatchAppsLine>) -> Set<Phase> {
    Set(lines.compactMap(\.entry.instances).flatMap { instances in instances.values.map(\.phase) })
}

func show(_ fixture: Fixture, pid: Int32) async throws {
    let shown: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["show", fixture.identifier, "--pid", String(pid)],
        as: WatchAppsLine.self,
    )
    expectNoDifference(shown.lines, [])
    expectNoDifference(shown.status, .exited(0))
}

func expectRefusal(
    _ arguments: [String],
    keyedBy key: String,
    fileID: StaticString = #fileID,
    filePath: StaticString = #filePath,
    line: UInt = #line,
    column: UInt = #column,
) async throws {
    let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(of: .product(named: "watch-apps"), arguments: arguments, as: WatchAppsLine.self)
    let keys: [String] = run.lines.map(\.key)
    let failing: [Bool] = run.lines.map { printed in printed.entry.fail != nil }
    expectNoDifference(keys, [key], "\(arguments)", fileID: fileID, filePath: filePath, line: line, column: column)
    expectNoDifference(failing, [true], "\(arguments)", fileID: fileID, filePath: filePath, line: line, column: column)
    expectNoDifference(run.status, .exited(1), "\(arguments)", fileID: fileID, filePath: filePath, line: line, column: column)
}

func watching<Outcome: Sendable>(
    _ names: [String],
    _ body: (_ lines: inout AsyncStream<WatchAppsLine>.Iterator) async throws -> Outcome,
) async throws -> (outcome: Outcome, lines: [WatchAppsLine]) {
    let session: (outcome: Outcome, lines: [WatchAppsLine], status: TerminationStatus) = try await streamingJSONLines(
        of: .product(named: "watch-apps"),
        arguments: ["watch"] + names,
        as: WatchAppsLine.self,
    ) { lines in
        var iterator: AsyncStream<WatchAppsLine>.Iterator = lines.makeAsyncIterator()
        return try await body(&iterator)
    }
    let keyed: [String: [WatchAppsLine]] = Dictionary(grouping: session.lines, by: \.key)
    expectNoDifference(keyed.filter { _, lines in zip(lines, lines.dropFirst()).contains { pair in pair.0 == pair.1 } }, [:])
    return (session.outcome, session.lines)
}

func launch(
    _ fixture: Fixture,
    _ settings: Settings = Settings(),
    named name: String? = nil,
    flags: [String] = [],
    running: Set<Int32> = [],
) async throws -> (pid: Int32, lines: [WatchAppsLine]) {
    let launched: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["launch", name ?? fixture.identifier] + flags + settings.arguments.flatMap { argument in ["--arguments", argument] },
        as: WatchAppsLine.self,
    )
    let lines: [WatchAppsLine] = launched.lines
    expectNoDifference(launched.status, .exited(0))
    expectNoDifference(phases(in: lines.dropLast(1)).subtracting([.launching]), [])
    expectNoDifference(phases(in: lines.suffix(1)).subtracting([.launching]).isEmpty, false, "last line \(String(describing: lines.last))")
    let started: Set<Int32> = Set(try #require(lines.last?.entry.instances).keys).subtracting(running)
    expectNoDifference(started.count, 1)
    return (try #require(started.first), lines)
}

func status(of fixture: Fixture, pid: Int32) async throws -> WatchAppsLine.Status {
    let read: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["read", fixture.identifier, "--pid", String(pid)],
        as: WatchAppsLine.self,
    )
    expectNoDifference(read.status, .exited(0))
    expectNoDifference(read.lines.map(\.key), [fixture.identifier])
    return try #require(read.lines.first?.entry.instances?[pid]?.running?.status)
}

func read(_ fixture: Fixture, pid: Int32) async throws -> [WatchAppsLine.Element] {
    try #require(try await status(of: fixture, pid: pid).responding?.elements)
}

func perform(_ control: WatchAppsLine.Control, _ action: String = kAXPressAction) async throws {
    let performed: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["perform", control.id, action],
        as: WatchAppsLine.self,
    )
    expectNoDifference(performed.status, .exited(0))
    expectNoDifference(phases(in: performed.lines.suffix(1)).isDisjoint(with: [.responding, .answering, .exited]), false, "last line \(String(describing: performed.lines.last))")
}

func press(_ scenario: Scenario, in fixture: Fixture, pid: Int32) async throws {
    let elements: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
    let item: WatchAppsLine.Control = try #require(elements.menuBar?.item(scenario))
    try await perform(item)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.serialized, .timeLimit(.minutes(2)))
struct Integration {}
