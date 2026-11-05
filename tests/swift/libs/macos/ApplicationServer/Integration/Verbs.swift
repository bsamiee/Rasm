import AppKit
import ApplicationServer
import Applications
import CasePaths
import CasePathsCore
import CustomDump
import ScreenCaptureKit
import TestSupport
import Testing

@testable import AppKitFixture

// --- [OPERATIONS] ----------------------------------------------------------------------

private func launched(_ settings: Settings = Settings()) async throws -> (steps: [Launching], instance: NSRunningApplication) {
    try checkAccessibilityTrust()
    let installation: Installation = try Installation(at: try #require(TestApplication.current))
    var steps: [Launching] = []
    for await step: Launching in NSWorkspace.shared.launching(installation, environment: settings.environment) { steps.append(step) }
    let instance: NSRunningApplication? = steps.lazy.compactMap { step in if case .state(let running) = step { running.instance } else { nil } }.first
    return (steps, try #require(instance))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.serialized, .timeLimit(.minutes(2)))
struct Verbs {
    @Test(.application(FixtureApplication.self, launches: false))
    func launchingReportsCreatedFirstAndEndsSettledAndInactive() async throws {
        let (steps, instance): ([Launching], NSRunningApplication) = try await launched()
        guard case .launched(let first)? = steps.first else { throw ApplicationFailure(subject: "\(steps)", reason: .noProcess) }
        expectNoDifference([first.pid: first.created], [instance.processIdentifier: true])
        guard case .state(let last)? = steps.last, case .running(let running) = last.state else { throw ApplicationFailure(subject: "\(steps)", reason: .noProcess) }
        #expect(running.status.settled)
        #expect(!running.active)
    }

    @Test(.application(FixtureApplication.self, launches: false))
    func launchingEndsAtAnAskingDialogDuringLaunch() async throws {
        let (steps, _): ([Launching], NSRunningApplication) = try await launched(Settings(scenario: .launchModal))
        guard case .state(let last)? = steps.last, case .running(let running) = last.state else { throw ApplicationFailure(subject: "\(steps)", reason: .noProcess) }
        #expect(!running.asking.isEmpty)
    }

    @Test(.application(FixtureApplication.self, launches: false))
    func performingRefusesRaiseUnsentAndReturnsNextState() async throws {
        let (_, instance): ([Launching], NSRunningApplication) = try await launched()
        let window: Element = try #require(await instance.state { _ in true }[case: \.running]?.windows.lazy.compactMap { row in row.description[case: \.element] }.first)
        let refused: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await instance.performing(.perform(element: window.id, action: kAXRaiseAction)) }
        let named: Bool = refused.reason.is(\.refused)
        #expect(named, "raise failed \(refused.reason)")
        #expect(!instance.isActive, "fixture stays in background")
    }

    @Test(.application(FixtureApplication.self, launches: false))
    func capturingTakesEachPresentedRowAndDropsTransparentOnes() async throws {
        let (_, instance): ([Launching], NSRunningApplication) = try await launched()
        try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
        let shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = try await instance.capturing()
        #expect(!shots.isEmpty)
        #expect(shots.values.allSatisfy { shot in if case .success = shot { true } else { false } })
    }

    @Test(.application(FixtureApplication.self, launches: false))
    func quittingForceEndsOnSignal() async throws {
        let (_, instance): ([Launching], NSRunningApplication) = try await launched()
        var last: Result<InstanceState, ApplicationFailure>?
        for await step: Result<InstanceState, ApplicationFailure> in instance.quitting(force: true) { last = step }
        let state: InstanceState = try #require(last).get()
        #expect(state.is(\.exited))
    }
}
