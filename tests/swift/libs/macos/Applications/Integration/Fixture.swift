import AppKit
import CasePaths
import CustomDump
import Encoding
import Foundation
import Processes
import Subprocess
import System
import TestSupport
import Testing

@testable import AppKitFixture
@testable import Applications

// --- [MODELS] --------------------------------------------------------------------------

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

// --- [OPERATIONS] ----------------------------------------------------------------------

func installation() throws -> Installation {
    try Installation(at: try #require(TestApplication.current))
}

func launched(_ instance: NSRunningApplication, fileID: StaticString = #fileID, filePath: StaticString = #filePath, line: UInt = #line, column: UInt = #column) async {
    let settled: InstanceState? = await instance.states(ending: termination(of: instance.processIdentifier)).first { state in !state.isLaunching }
    expectNoDifference(settled?.phase, .responding, fileID: fileID, filePath: filePath, line: line, column: column)
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
    let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, arguments: settings.arguments).instance
    await launched(instance)
    expectNoDifference(instance.activationPolicy, .accessory)
    return (installation, instance)
}

func read(_ instance: NSRunningApplication) async throws -> [Element] {
    let state: InstanceState = await instance.state { _ in true }
    let responding: [Element]? = state[case: \.running]?.status[case: \.responding]?.elements
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
    let item: Control = try #require(try menu(String(describing: Scenario.self), in: try await read(instance)).first { item in item.label == scenario.rawValue })
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

func phases(_ seen: [InstanceState]) -> [Phase] {
    seen.map(\.phase).reduce(into: []) { kept, next in
        if kept.last != next { kept.append(next) }
    }
}

func observe(
    _ instance: NSRunningApplication,
    until done: ([InstanceState]) -> Bool,
    acting act: () async throws -> Void = {},
) async throws -> [InstanceState] {
    var seen: [InstanceState] = []
    for await state: InstanceState in instance.states(ending: termination(of: instance.processIdentifier)) {
        seen.append(state)
        if seen.count == 1 { try await act() }
        if done(seen) { break }
    }
    return seen
}

extension InstanceState {
    var elements: [Element] { self[case: \.running]?.status[case: \.responding]?.elements ?? [] }

    var windows: [Window]? {
        switch self[case: \.running]?.status {
            case .launching(let windows), .responding(_, let windows), .unresponsive(_, let windows): windows
            case nil: nil
        }
    }

    var usage: Result<ResourceUsage, Errno>? { self[case: \.running]?.usage }

    var asking: Element? { elements.first(where: \.requiresAnswer) }

    var phase: Phase {
        switch self {
            case .running(let running):
                switch running.status {
                    case .launching: .launching
                    case .responding: .responding
                    case .unresponsive(let process, _): .unresponsive(process)
                }
            case .failed: .failed
            case .exited(.failure, _): .unread
            case .exited(.success(let exit), .none): .exited(exit.status)
            case .exited(.success(let exit), .some(.success(let report))): .reported(exit.status, exception: report.exception)
            case .exited(.success(let exit), .some(.failure)): .reported(exit.status, exception: nil)
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.serialized, .timeLimit(.minutes(2)))
struct Integration {}
