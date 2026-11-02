import CustomDump
import Foundation
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

let promptHosts: Set<String> = ["com.apple.UserNotificationCenter", "com.apple.coreservices.uiagent", "com.apple.accessibility.universalAccessAuthWarn"]

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
}

struct Button: Hashable {
    let identifier: String?
    let label: String?
    let attribute: String?
}

struct Answer: Equatable {
    let role: String?
    let subrole: String?
    let modal: Bool?
    let description: String?
    let identifier: String?
    let requiresAnswer: Bool?
    let buttons: Set<Button>

    init(role: String?, subrole: String?, modal: Bool?, description: String?, identifier: String?, requiresAnswer: Bool?, buttons: Set<Button>) {
        self.role = role
        self.subrole = subrole
        self.modal = modal
        self.description = description
        self.identifier = identifier
        self.requiresAnswer = requiresAnswer
        self.buttons = buttons
    }

    init(_ element: WatchAppsLine.Element, subrole: Bool = false, modal: Bool = false) {
        role = element.role
        self.subrole = subrole ? element.subrole : nil
        self.modal = modal ? element.modal : nil
        description = element.description
        identifier = element.identifier
        requiresAnswer = element.requiresAnswer
        buttons = Set(
            (element.detail?.controls ?? []).filter { control in control.role == "AXButton" && control.subrole == nil }
                .map { control in Button(identifier: control.identifier, label: control.label, attribute: control.attribute) }
        )
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

enum Phase: Hashable {
    case launching
    case responding
    case answering
    case unresponsive(WatchAppsLine.ProcessStatus)
    case exited
    case failed
    case captured
}

extension WatchAppsLine {
    var instances: [Int32: WatchAppsLine.Instance]? { if case .instances(let instances) = entry { instances } else { nil } }

    var reason: String? { if case .fail(let reason) = entry { reason } else { nil } }

    func elements(of pid: Int32) -> [WatchAppsLine.Element]? { instances?[pid]?.elements }
}

extension WatchAppsLine.Instance {
    var running: WatchAppsLine.Running? { if case .running(let running) = self { running } else { nil } }

    var elements: [WatchAppsLine.Element]? { if case .responding(let elements) = running?.status { elements } else { nil } }

    var exit: WatchAppsLine.Exit? { if case .exited(let exit) = self { exit } else { nil } }

    var captured: WatchAppsLine.Capture? { if case .captured(let capture) = self { capture } else { nil } }

    var phase: Phase {
        switch self {
            case .running(let running):
                switch running.status {
                    case .launching: .launching
                    case .responding(let elements) where elements.answers.isEmpty: .responding
                    case .responding: .answering
                    case .unresponsive(let process): .unresponsive(process)
                }
            case .failed: .failed
            case .exited: .exited
            case .captured: .captured
        }
    }
}

extension [WatchAppsLine.Element] {
    var answers: [WatchAppsLine.Element] { filter { element in element.requiresAnswer == true } }

    var menuBar: WatchAppsLine.Element? { first { element in element.role == "AXMenuBar" } }

    func titled(_ title: String) -> WatchAppsLine.Element? { first { element in element.title == title } }
}

extension WatchAppsLine.Element {
    func control(_ identifier: String) -> WatchAppsLine.Control? { detail?.controls?.first { control in control.identifier == identifier } }

    func control(labeled label: String) -> WatchAppsLine.Control? { detail?.controls?.first { control in control.label == label } }

    func item(_ scenario: Scenario) -> WatchAppsLine.Control? { control(labeled: "Scenario")?.items?.first { item in item.label == scenario.rawValue } }
}

extension AsyncStream<TimedLine<WatchAppsLine>>.Iterator {
    mutating func next(sourceLocation: SourceLocation = #_sourceLocation, where matches: (WatchAppsLine) -> Bool) async throws -> TimedLine<WatchAppsLine> {
        let seen: [TimedLine<WatchAppsLine>] = await collect { seen in seen.last.map(matches) == true }
        let matched: TimedLine<WatchAppsLine>? = seen.last.flatMap { timed in matches(timed.line) ? timed : nil }
        return try #require(matched, "watch output ended after \(seen.count) lines with none matching", sourceLocation: sourceLocation)
    }

    mutating func collect(until done: ([WatchAppsLine]) -> Bool, after seen: [TimedLine<WatchAppsLine>] = []) async -> [TimedLine<WatchAppsLine>] {
        if !done(seen.map(\.line)), let timed: TimedLine<WatchAppsLine> = await next(isolation: #isolation) {
            await collect(until: done, after: seen + [timed])
        } else {
            seen
        }
    }
}

func pids(in lines: [WatchAppsLine]) -> [[Int32]] { lines.map { line in (line.instances.map { instances in Array(instances.keys) } ?? []).sorted() } }

func phases(of pid: Int32, in lines: [WatchAppsLine]) -> [Phase] { lines.compactMap { line in line.instances?[pid]?.phase } }

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
    let failing: [Bool] = run.lines.map { printed in printed.reason != nil }
    expectNoDifference(keys, [key], "\(arguments)", fileID: fileID, filePath: filePath, line: line, column: column)
    expectNoDifference(failing, [true], "\(arguments)", fileID: fileID, filePath: filePath, line: line, column: column)
    expectNoDifference(run.status, .exited(1), "\(arguments)", fileID: fileID, filePath: filePath, line: line, column: column)
}

func watching<Outcome: Sendable>(
    _ names: [String],
    within budget: Duration = .seconds(60),
    _ body: (_ lines: inout AsyncStream<TimedLine<WatchAppsLine>>.Iterator) async throws -> Outcome,
) async throws -> (outcome: Outcome, lines: [TimedLine<WatchAppsLine>]) {
    let session: (outcome: Outcome, lines: [TimedLine<WatchAppsLine>], status: TerminationStatus) = try await streamingJSONLines(
        of: .product(named: "watch-apps"),
        within: budget,
        arguments: ["watch"] + names,
        as: WatchAppsLine.self,
    ) { lines in
        var iterator: AsyncStream<TimedLine<WatchAppsLine>>.Iterator = lines.makeAsyncIterator()
        return try await body(&iterator)
    }
    let keyed: [String: [WatchAppsLine]] = Dictionary(grouping: session.lines.map(\.line), by: \.key)
    expectNoDifference(keyed.filter { _, lines in zip(lines, lines.dropFirst()).contains { pair in pair.0 == pair.1 } }, [:])
    return (session.outcome, session.lines)
}

func launch(_ fixture: Fixture, _ settings: Settings = Settings(), flags: [String] = [], running: Set<Int32> = []) async throws -> (pid: Int32, lines: [WatchAppsLine]) {
    let launched: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["launch", fixture.identifier] + flags + settings.arguments.flatMap { argument in ["--arguments", argument] },
        as: WatchAppsLine.self,
    )
    expectNoDifference(launched.status, .exited(0))
    let started: Set<Int32> = Set(try #require(launched.lines.last?.instances).keys).subtracting(running)
    expectNoDifference(started.count, 1)
    return (try #require(started.first), launched.lines)
}

func read(_ fixture: Fixture, pid: Int32) async throws -> [WatchAppsLine.Element] {
    let read: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["read", fixture.identifier, "--pid", String(pid)],
        as: WatchAppsLine.self,
    )
    expectNoDifference(read.status, .exited(0))
    expectNoDifference(read.lines.map(\.key), [fixture.identifier])
    let elements: [WatchAppsLine.Element]? = read.lines.first?.elements(of: pid)
    return try #require(elements)
}

func perform(_ control: WatchAppsLine.Control, _ action: String = "AXPress") async throws {
    let performed: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["perform", control.id, action],
        as: WatchAppsLine.self,
    )
    expectNoDifference(performed.lines, [])
    expectNoDifference(performed.status, .exited(0))
}

func press(_ scenario: Scenario, in fixture: Fixture, pid: Int32) async throws {
    let elements: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
    let item: WatchAppsLine.Control = try #require(elements.menuBar?.item(scenario))
    try await perform(item)
}

func answerCrashAlert(naming name: String) async {
    do {
        _ = try await watching([name], within: .seconds(20)) { lines in
            func prompting(_ line: WatchAppsLine) -> Bool { promptHosts.contains(line.key) && line.instances?.isEmpty == false }
            guard let prompt: TimedLine<WatchAppsLine> = await lines.collect(until: { seen in seen.last.map(prompting) == true }).last.flatMap({ timed in prompting(timed.line) ? timed : nil })
            else { return }
            let ignore: [WatchAppsLine.Control] = (prompt.line.instances ?? [:]).values.compactMap(\.elements).joined().compactMap { element in element.control(labeled: "Ignore") }
            for control: WatchAppsLine.Control in ignore { try await perform(control) }
        }
    } catch {
        Issue.record(error)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.serialized)
struct HostSession {}
