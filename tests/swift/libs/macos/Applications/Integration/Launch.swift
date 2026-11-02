import AppKit
import Applications
import CasePaths
import CustomDump
import Subprocess
import TestSupport
import Testing

@testable import AppKitFixture

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Launches {
        @Test(.application(FixtureApplication.self, launches: false))
        func launchReusesRunningInstanceUnlessNewOneIsAsked() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let first: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true)))
            try await first.finishedLaunching()
            let facts: [String: Bool] = ["finished": first.isFinishedLaunching, "agent": installation.agent, "runs": installation.runs(first), "runs current": installation.runs(.current)]
            expectNoDifference(facts, ["finished": true, "agent": true, "runs": true, "runs current": false])
            let again: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true)))
            expectNoDifference(again.processIdentifier, first.processIdentifier)
            let second: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true), new: true))
            try await second.finishedLaunching()
            expectNoDifference(Set([first, second].map(\.processIdentifier)).count, 2)
            expectNoDifference(Set(NSWorkspace.shared.instances(of: installation.runs).map(\.processIdentifier)), [first.processIdentifier, second.processIdentifier])
            let installed: [Installation] = try await Installation.installed(named: installation.identifier)
            expectNoDifference(installed, [installation])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func hiddenLaunchOfAgentIsRefusedAndOpensNothing() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) {
                try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(), hides: true))
            }
            let refused: Bool = failure.reason.is(\.hiddenAgent)
            #expect(refused, "hidden agent launch failed \(failure.reason)")
            expectNoDifference(NSWorkspace.shared.instances(of: installation.runs).map(\.processIdentifier), [])
        }

        @Test(.application(FixtureApplication.self, launches: false, activationPolicy: .regular))
        func hiddenLaunchStaysReadableButHasNoWindowToCapture() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(), hides: true))
            try await instance.finishedLaunching()
            expectNoDifference(instance.activationPolicy, .regular)
            let elements: [Element] = try await read(instance)
            expectNoDifference(elements.filter { element in element.role == kAXWindowRole }.compactMap(\.title), ["Fixture 0"])
            let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await instance.screenshot() }
            let absent: Bool = failure.reason.is(\.absent)
            #expect(absent, "hidden capture failed \(failure.reason)")
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, multipleInstancesProhibited: true))
        func prohibitedNewInstanceIsRefusedAndOpensNothing() async throws {
            let (installation, instance): (Installation, NSRunningApplication) = try await launched()
            #expect(installation.multipleInstancesProhibited, "bundle declares LSMultipleInstancesProhibited")
            let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) {
                try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true), new: true))
            }
            let prohibited: Bool = failure.reason.is(\.multipleInstancesProhibited)
            #expect(prohibited, "refused with \(failure.reason)")
            expectNoDifference(NSWorkspace.shared.instances(of: installation.runs).map(\.processIdentifier), [instance.processIdentifier])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func selfEnforcedSingleInstanceAlertsInNewInstanceThenExits() async throws {
            let refusing: Settings = Settings(scenario: .singleInstance, hidden: true, status: 3)
            let (installation, first): (Installation, NSRunningApplication) = try await launched(refusing)
            let second: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(refusing, new: true))
            let asked: [Observed] = try await observe(second, within: .seconds(20), until: { seen in seen.last?.state.asking != nil })
            let alert: Element = try #require(asked.last?.state.asking)
            let refused: Bool = alert.detail?.text.contains("Another instance is running") == true
            #expect(refused, "alert text \(alert.detail?.text ?? [])")
            let ok: Control = try #require(alert.detail?.controls.first { control in control.identifier == "action-button-1" })
            let answered: [Observed] = try await observe(
                second,
                within: .seconds(20),
                until: { _ in false },
                acting: { try await ElementAction.perform(element: ok.id, action: kAXPressAction).perform() },
            )
            expectNoDifference(phases(answered).last, .exited(.exited(3)))
            let remaining: [Element] = try await read(first)
            expectNoDifference(remaining.filter(\.requiresAnswer).map(\.role), [])
        }
    }
}
