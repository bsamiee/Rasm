import AppKit
import Applications
import CasePaths
import CustomDump
import Encoding
import Foundation
import OrderedCollections
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

// --- [SERVICES] ------------------------------------------------------------------------

final class Timeline: Sendable {
    let entries: Mutex<[Observed]> = Mutex([])
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Sendable
func answerCrashAlert(naming name: String) async {
    let pressed: Result<Void, TestFailure> = await Result { () async throws(TestFailure) in
        try await withTimeout(.seconds(20), expired: TestFailure(subject: "crash alert naming \(name)", reason: .missing)) { () async throws(TestFailure) in
            for await prompt: (host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>) in await NSWorkspace.shared.systemPrompts(
                naming: [name],
                notifications: [kAXWindowCreatedNotification],
                polling: heartbeat,
            ) {
                let buttons: [Control] = prompt.prompts.values.flatMap(\.elements).compactMap(\.detail).flatMap(\.controls).filter { control in
                    control.identifier == "action-button-2"
                }
                for button: Control in buttons {
                    do { try await ElementAction.perform(element: button.id, action: kAXPressAction).perform() } catch { Issue.record(error) }
                }
                guard buttons.isEmpty else { return }
            }
        }
    }
    if case .failure(let failure) = pressed { Issue.record(failure) }
}

func installation() throws -> Installation {
    try Installation(at: try #require(TestApplication.current))
}

func instance(of installation: Installation) throws -> NSRunningApplication {
    try #require(NSWorkspace.shared.instances(of: installation.runs).first)
}

func configuration(_ settings: Settings, new: Bool = false, hides: Bool = false) -> NSWorkspace.OpenConfiguration {
    let configuration: NSWorkspace.OpenConfiguration = NSWorkspace.OpenConfiguration()
    configuration.arguments = settings.arguments
    configuration.createsNewApplicationInstance = new
    configuration.hides = hides
    return configuration
}

func launched() async throws -> (installation: Installation, instance: NSRunningApplication) {
    try checkAccessibilityTrust()
    let installation: Installation = try installation()
    let instance: NSRunningApplication = try instance(of: installation)
    try await instance.finishedLaunching()
    expectNoDifference(instance.activationPolicy, .accessory)
    return (installation, instance)
}

func launched(_ settings: Settings) async throws -> (installation: Installation, instance: NSRunningApplication) {
    try checkAccessibilityTrust()
    let installation: Installation = try installation()
    let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(settings))
    try await instance.finishedLaunching()
    expectNoDifference(instance.activationPolicy, .accessory)
    return (installation, instance)
}

func read(_ instance: NSRunningApplication) async throws -> [Element] {
    let state: InstanceState = await instance.state { _ in true }
    return try #require(state[case: \.running]?.0[case: \.responding], "read \(line(state))")
}

func menu(_ title: String, in elements: [Element]) throws -> [Control] {
    let bar: Element = try #require(elements.first { element in element.role == kAXMenuBarRole })
    return try #require(bar.detail?.controls.first { control in control.label == title }?.items, "menu \(title)")
}

func run(_ scenario: Scenario, in instance: NSRunningApplication) async throws {
    let item: Control = try #require(try menu("Scenario", in: try await read(instance)).first { item in item.label == scenario.rawValue })
    try await ElementAction.perform(element: item.id, action: kAXPressAction).perform()
}

func expectElapsed(_ elapsed: Duration, in bounds: ClosedRange<Duration>, _ event: String, sourceLocation: SourceLocation = #_sourceLocation) {
    let bounded: Bool = bounds.contains(elapsed)
    #expect(bounded, "\(event) after \(elapsed), outside \(bounds)", sourceLocation: sourceLocation)
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

func line(_ state: InstanceState) -> String {
    let encoded: Result<Data, any Error> = Result { try JSONEncoder().encodeLine(state) }
    return switch encoded.map({ data in String(bytes: data, encoding: .utf8) }) {
        case .success(.some(let text)): text
        case .success(.none): summary(state)
        case .failure(let error): "\(summary(state)): \(error)"
    }
}

func summary(of status: Result<TerminationStatus, Errno>) -> String {
    switch status {
        case .success(.exited(let code)): "status \(code)"
        case .success(.signaled(let signal)): "signal \(signal)"
        case .failure(let error): "statusFailure \(error.description)"
    }
}

func summary(of report: Result<CrashReport, ApplicationFailure>) -> String {
    switch report {
        case .success(let report): "report \(report.exception ?? "none")"
        case .failure: "reportFailure"
    }
}

func summary(_ state: InstanceState) -> String {
    switch state {
        case .running(.launching, _): "launching"
        case .running(.responding, _): "responding"
        case .running(.unresponsive(let process), _): "unresponsive \(process.rawValue)"
        case .failed(let failure): "failed \(failure.localizedDescription)"
        case .exited(let status, let report): (["exited", summary(of: status)] + [report.map(summary)].compactMap(\.self)).joined(separator: " ")
    }
}

func summaries(_ seen: [Observed]) -> [String] {
    seen.map { observed in summary(observed.state) }.reduce(into: []) { kept, next in
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
    try finished.mapError { failure in TestFailure(subject: "\(failure.subject) after\n\(seen.map { observed in line(observed.state) }.joined(separator: "\n"))", reason: failure.reason) }.get()
    return seen
}

extension InstanceState {
    var elements: [Element] { self[case: \.running]?.0[case: \.responding] ?? [] }

    var usage: ResourceUsage? { self[case: \.running]?.usage }

    var asking: Element? { elements.first(where: \.requiresAnswer) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.serialized)
struct Integration {}
