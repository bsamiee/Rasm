import AppKit
import Applications
import CasePaths
import CustomDump
import Processes
import ScreenCaptureKit
import Subprocess
import System
import TestSupport
import Testing

@testable import AppKitFixture

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Launches {
        @Test(.application(FixtureApplication.self, launches: false))
        func launchReusesRunningInstanceUnlessNewOneIsAskedAndLeavesEachRunning() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let first: (instance: NSRunningApplication, created: Bool) = try await NSWorkspace.shared.launch(installation, arguments: Settings(hidden: true).arguments)
            await launched(first.instance)
            let facts: [String: Bool] = [
                "created": first.created, "finished": first.instance.isFinishedLaunching, "agent": installation.agent, "runs": installation.runs(first.instance),
                "runs current": installation.runs(.current),
            ]
            expectNoDifference(facts, ["created": true, "finished": true, "agent": true, "runs": true, "runs current": false])
            let again: (instance: NSRunningApplication, created: Bool) = try await NSWorkspace.shared.launch(installation, arguments: Settings(hidden: true).arguments)
            expectNoDifference([again.instance.processIdentifier: again.created], [first.instance.processIdentifier: false])
            let second: (instance: NSRunningApplication, created: Bool) = try await NSWorkspace.shared.launch(installation, arguments: Settings(hidden: true).arguments, newInstance: true)
            await launched(second.instance)
            expectNoDifference(second.created, true)
            expectNoDifference(Set(NSWorkspace.shared.instances(of: installation.runs).map(\.processIdentifier)), [first.instance.processIdentifier, second.instance.processIdentifier])
            expectNoDifference([first.instance.isTerminated, second.instance.isTerminated], [false, false])
            let installed: [Installation] = try await Installation.installed(named: installation.identifier)
            expectNoDifference(installed, [installation])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func agentLaunchesShown() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, arguments: Settings().arguments).instance
            await launched(instance)
            expectNoDifference([installation.agent, instance.isHidden], [true, false])
        }

        @Test(.application(FixtureApplication.self, launches: false, activationPolicy: .regular), .timeLimit(.minutes(1)))
        func launchHidesStaysReadableAndCaptureLeavesHiddenInstanceHidden() async throws {
            try checkAccessibilityTrust()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let installation: Installation = try installation()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, arguments: Settings().arguments).instance
            await launched(instance)
            expectNoDifference([instance.activationPolicy == .regular, instance.isHidden, instance.isActive], [true, true, false])
            let elements: [Element] = try await read(instance)
            expectNoDifference(elements.filter { element in element.role == kAXWindowRole }.compactMap(\.title), ["Fixture 0"])
            let window: CGWindowID = try await numbered("Fixture 0", in: instance)
            let hiddenCapture: [CGWindowID] = try await instance.screenshot().windows
            expectNoDifference(hiddenCapture, [window])
            expectNoDifference([instance.isHidden, instance.isActive], [true, false])
            try await instance.setHidden(false)
            try await instance.setHidden(false)
            let captured: [CGWindowID] = try await instance.screenshot().windows
            expectNoDifference(captured, [window])
            expectNoDifference([instance.isHidden, instance.isActive], [false, false])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func selfEnforcedSingleInstanceAlertsInNewInstanceThenExits() async throws {
            let refusing: Settings = Settings(scenario: .singleInstance, hidden: true, status: 3)
            let (installation, first): (Installation, NSRunningApplication) = try await launched(refusing)
            let second: NSRunningApplication = try await NSWorkspace.shared.launch(installation, arguments: refusing.arguments, newInstance: true).instance
            let asked: [InstanceState] = try await observe(second, until: { seen in seen.last?.asking != nil })
            let alert: Element = try #require(asked.last?.asking)
            let refused: Bool = alert.detail?.text.contains("Another instance is running") == true
            #expect(refused, "alert text \(alert.detail?.text ?? [])")
            let ok: Control = try #require(alert.detail?.controls.first { control in control.identifier == "action-button-1" })
            let answered: [InstanceState] = try await observe(
                second,
                until: { _ in false },
                acting: { try await ElementAction.perform(element: ok.id, action: kAXPressAction).perform() },
            )
            expectNoDifference(phases(answered).last, .exited(.exited(3)))
            let remaining: [Element] = try await read(first)
            expectNoDifference(remaining.filter(\.requiresAnswer).map(\.role), [])
        }
    }
}
