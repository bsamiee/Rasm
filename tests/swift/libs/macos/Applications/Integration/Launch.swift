import AppKit
import CasePaths
import CustomDump
import Processes
import ScreenCaptureKit
import Subprocess
import System
import TestSupport
import Testing

@testable import AppKitFixture
@testable import Applications

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Launches {
        @Test(.application(FixtureApplication.self, launches: false))
        func launchReusesRunningInstanceUnlessNewOneIsAskedAndLeavesEachRunning() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let environment: [String: String] = Settings().environment
            let first: (instance: NSRunningApplication, created: Bool) = try await NSWorkspace.shared.launch(installation, environment: environment)
            await launched(first.instance)
            let facts: [String: Bool] = [
                "created": first.created, "finished": first.instance.isFinishedLaunching, "agent": installation.agent, "runs": installation.runs(first.instance),
                "runs current": installation.runs(.current),
            ]
            expectNoDifference(facts, ["created": true, "finished": true, "agent": false, "runs": true, "runs current": false])
            let again: (instance: NSRunningApplication, created: Bool) = try await NSWorkspace.shared.launch(installation, environment: environment)
            expectNoDifference([again.instance.processIdentifier: again.created], [first.instance.processIdentifier: false])
            let second: (instance: NSRunningApplication, created: Bool) = try await NSWorkspace.shared.launch(installation, environment: environment, newInstance: true)
            await launched(second.instance)
            expectNoDifference(second.created, true)
            expectNoDifference(Set(NSWorkspace.shared.instances(of: installation.runs).map(\.processIdentifier)), [first.instance.processIdentifier, second.instance.processIdentifier])
            expectNoDifference([first.instance.isTerminated, second.instance.isTerminated], [false, false])
            let installed: [Installation] = try await Installation.installed(named: installation.identifier)
            expectNoDifference(installed, [installation])
        }

        @Test(.application(FixtureApplication.self, launches: false, activationPolicy: .accessory))
        func agentLaunchesOrderedBehindEverything() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, environment: Settings().environment).instance
            await launched(instance)
            let running: InstanceState.Running = try #require(await instance.state()[case: \.running])
            expectNoDifference([installation.agent, running.active], [true, false])
        }

        @Test(.application(FixtureApplication.self, launches: false), .timeLimit(.minutes(1)))
        func backgroundLaunchStaysReadableAndCaptureLeavesItInactive() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let running: InstanceState.Running = try await read(instance)
            expectNoDifference(running.active, false)
            expectNoDifference(running.elements.filter { element in element.role == kAXWindowRole }.compactMap(\.title), ["Fixture 0"])
            let window: CGWindowID = try await numbered("Fixture 0", in: instance)
            let shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = try await instance.screenshots(of: [window])
            let shot: Result<SCScreenshotOutput, ApplicationFailure> = try #require(shots[window])
            _ = try #require(try shot.get().sdrImage)
            let after: InstanceState.Running = try await read(instance)
            expectNoDifference(after.active, false)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func selfEnforcedSingleInstanceAlertsInNewInstanceThenExits() async throws {
            let refusing: Settings = Settings(scenario: .singleInstance, status: 3)
            let (installation, first): (Installation, NSRunningApplication) = try await launched(refusing)
            let second: NSRunningApplication = try await NSWorkspace.shared.launch(installation, environment: refusing.environment, newInstance: true).instance
            let asked: [InstanceState] = try await observe(second, until: { seen in seen.last?.asking != nil })
            let alert: Element = try #require(asked.last?.asking)
            let refused: Bool = alert.detail?.text.contains("Another instance is running") == true
            #expect(refused, "alert text \(alert.detail?.text ?? [])")
            let ok: Control = try #require(alert.answers?.accept)
            let answered: [InstanceState] = try await observe(
                second,
                until: { _ in false },
                acting: { _ = try await ElementAction.perform(element: ok.id, action: kAXPressAction).perform(in: second.processIdentifier) },
            )
            expectNoDifference(phases(answered).last, .exited(.exited(3)))
            let remaining: InstanceState.Running = try await read(first)
            expectNoDifference(remaining.elements.filter(\.asking).map(\.role), [])
        }
    }
}
