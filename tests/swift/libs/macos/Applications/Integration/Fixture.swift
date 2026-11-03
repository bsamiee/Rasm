import AppKit
import Applications
import CasePaths
import CustomDump
import Encoding
import Foundation
import Processes
import Results
import Subprocess
import Synchronization
import System
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

let notifications: Set<String> = [kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXTitleChangedNotification, kAXFocusedWindowChangedNotification, kAXUIElementDestroyedNotification]
let heartbeat: Duration = .seconds(1)

// --- [MODELS] --------------------------------------------------------------------------

struct Observed: Sendable {
    let instant: ContinuousClock.Instant
    let state: InstanceState
}

@CasePathable
enum Phase: Equatable, Sendable {
    case launching
    case responding
    case unresponsive(ProcessStatus)
    case failed
    case exited(TerminationStatus)
    case reported(TerminationStatus, exception: String?)
    case unread
}

// --- [SERVICES] ------------------------------------------------------------------------

final class Timeline: Sendable {
    let entries: Mutex<[Observed]> = Mutex([])
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func installation() throws -> Installation {
    try Installation(at: try #require(TestApplication.current))
}

func configuration(_ settings: Settings, new: Bool = false) -> NSWorkspace.OpenConfiguration {
    let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
    configuration.arguments = settings.arguments
    configuration.createsNewApplicationInstance = new
    return configuration
}

func launched(_ instance: NSRunningApplication, fileID: StaticString = #fileID, filePath: StaticString = #filePath, line: UInt = #line, column: UInt = #column) async {
    var last: InstanceState?
    for await state: InstanceState in instance.launchStates(notifications: notifications, polling: heartbeat) { last = state }
    expectNoDifference(last?.phase, .responding, fileID: fileID, filePath: filePath, line: line, column: column)
}

func launched() async throws -> (installation: Installation, instance: NSRunningApplication) {
    try checkAccessibilityTrust()
    let installation: Installation = try installation()
    let instance: NSRunningApplication = try #require(TestApplication.instance)
    await launched(instance)
    expectNoDifference(instance.activationPolicy, .accessory)
    return (installation, instance)
}

func launched(_ settings: Settings) async throws -> (installation: Installation, instance: NSRunningApplication) {
    try checkAccessibilityTrust()
    let installation: Installation = try installation()
    let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(settings)).instance
    await launched(instance)
    expectNoDifference(instance.activationPolicy, .accessory)
    return (installation, instance)
}

func read(_ instance: NSRunningApplication) async throws -> [Element] {
    let state: InstanceState = await instance.state { _ in true }
    let responding: [Element]? = state[case: \.running]?.0[case: \.responding]?.elements
    return try #require(responding, "read \(state.phase)")
}

func numbered(_ title: String, in instance: NSRunningApplication) async throws -> CGWindowID {
    let window: CGWindowID? = try await read(instance).first { element in element.title == title }?.window
    return try #require(window, "window \(title)")
}

func menu(_ title: String, in elements: [Element]) throws -> [Control] {
    let bar: Element = try #require(elements.first { element in element.role == kAXMenuBarRole })
    return try #require(bar.detail?.controls.first { control in control.label == title }?.items, "menu \(title)")
}

func run(_ scenario: Scenario, in instance: NSRunningApplication) async throws {
    let item: Control = try #require(try menu("Scenario", in: try await read(instance)).first { item in item.label == scenario.rawValue })
    try await ElementAction.perform(element: item.id, action: kAXPressAction).perform()
}

func json(_ text: String) throws -> JSONValue {
    try JSONDecoder().decode(JSONValue.self, from: Data(text.utf8))
}

func shape(_ value: some Encodable, dropping dropped: Set<String> = ["id", "window", "detail", "items"]) throws -> JSONValue {
    let encoded: JSONValue = try JSONDecoder().decode(JSONValue.self, from: JSONEncoder().encodeLine(value))
    return switch encoded {
        case .object(let fields): .object(fields.filter { key, _ in !dropped.contains(key) })
        case .null, .bool, .number, .string, .array: encoded
    }
}

func alertButtons() throws -> Set<JSONValue> {
    try [
        json(#"{"role":"AXButton","label":"OK","identifier":"action-button-1","attribute":"AXDefaultButton","enabled":true,"actions":["AXPress"]}"#),
        json(#"{"role":"AXButton","label":"Cancel","identifier":"action-button-2","attribute":"AXCancelButton","enabled":true,"actions":["AXPress"]}"#),
    ]
}

func phases(_ seen: [Observed]) -> [Phase] {
    seen.map(\.state.phase).reduce(into: []) { kept, next in
        if kept.last != next { kept.append(next) }
    }
}

func observe(
    _ instance: NSRunningApplication,
    within limit: Duration,
    until done: @escaping @Sendable ([Observed]) -> Bool,
    walking walked: @escaping @Sendable (Element) -> Bool = \.requiresAnswer,
    notifying watched: Set<String> = notifications,
    acting act: @escaping @Sendable () async throws -> Void = {},
) async throws -> [Observed] {
    let timeline: Timeline = Timeline()
    let clock: ContinuousClock = ContinuousClock()
    let finished: Result<Void, TestFailure> = await Result { () async throws(TestFailure) in
        try await withTimeout(limit, expired: TestFailure(subject: "states of \(instance.processIdentifier) within \(limit)", reason: .missing)) { () async throws(TestFailure) in
            for await state: InstanceState in instance.states(walking: walked, notifications: watched, polling: heartbeat) {
                let seen: [Observed] = timeline.entries.withLock { entries in
                    entries.append(Observed(instant: clock.now, state: state))
                    return entries
                }
                if seen.count == 1 {
                    do { try await act() } catch { throw TestFailure(subject: "act on \(instance.processIdentifier)", reason: .host(error)) }
                }
                if done(seen) { return }
            }
        }
    }
    let seen: [Observed] = timeline.entries.withLock(\.self)
    try finished.mapError { failure in TestFailure(subject: "\(failure.subject) after \(phases(seen))", reason: failure.reason) }.get()
    return seen
}

extension InstanceState {
    var elements: [Element] { self[case: \.running]?.0[case: \.responding]?.elements ?? [] }

    var windows: [Window]? {
        switch self {
            case .running(.responding(_, let windows), _), .running(.unresponsive(_, let windows), _): windows
            case .running(.launching, _), .failed, .exited: nil
        }
    }

    var usage: ResourceUsage? { self[case: \.running]?.usage }

    var asking: Element? { elements.first(where: \.requiresAnswer) }

    var phase: Phase {
        switch self {
            case .running(.launching, _): .launching
            case .running(.responding, _): .responding
            case .running(.unresponsive(let process, _), _): .unresponsive(process)
            case .failed: .failed
            case .exited(.failure, _): .unread
            case .exited(.success(let status), .none): .exited(status)
            case .exited(.success(let status), .some(.success(let report))): .reported(status, exception: report.exception)
            case .exited(.success(let status), .some(.failure)): .reported(status, exception: nil)
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.serialized, .timeLimit(.minutes(2)))
struct Integration {}
