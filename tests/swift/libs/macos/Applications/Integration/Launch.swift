import AppKit
import Applications
import CasePaths
import CustomDump
import Processes
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
        func launchReusesRunningInstanceUnlessNewOneIsAsked() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let first: Launch = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true)))
            try await first.instance.finishedLaunching()
            let facts: [String: Bool] = [
                "created": first.created, "finished": first.instance.isFinishedLaunching, "agent": installation.agent, "runs": installation.runs(first.instance),
                "runs current": installation.runs(.current),
            ]
            expectNoDifference(facts, ["created": true, "finished": true, "agent": true, "runs": true, "runs current": false])
            let again: Launch = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true)))
            expectNoDifference([again.instance.processIdentifier: again.created], [first.instance.processIdentifier: false])
            let second: Launch = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true), new: true))
            try await second.instance.finishedLaunching()
            expectNoDifference(second.created, true)
            expectNoDifference(Set(NSWorkspace.shared.instances(of: installation.runs).map(\.processIdentifier)), [first.instance.processIdentifier, second.instance.processIdentifier])
            let installed: [Installation] = try await Installation.installed(named: installation.identifier)
            expectNoDifference(installed, [installation])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func createdInstanceQuitsWhenItsLaunchEndsAndReusedOneStays() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            func ended(new: Bool) async throws -> (pid: pid_t, created: Bool) {
                let launch: Launch = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true), new: new))
                try await launch.instance.finishedLaunching()
                return (launch.instance.processIdentifier, launch.created)
            }
            let kept: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(hidden: true))).detached()
            try await kept.finishedLaunching()
            let reused: (pid: pid_t, created: Bool) = try await ended(new: false)
            let fresh: (pid: pid_t, created: Bool) = try await ended(new: true)
            expectNoDifference([reused.created, fresh.created], [false, true])
            let exit: Result<ProcessExit, Errno>? = await termination(of: fresh.pid).first { _ in true }
            let gone: Bool = exit.map { exit in exit[case: \.success] != nil || exit == .failure(.noSuchProcess) } ?? false
            #expect(gone, "created instance \(fresh.pid) ended with \(String(describing: exit))")
            expectNoDifference(reused.pid, kept.processIdentifier)
            #expect(!kept.isTerminated, "reused instance \(kept.processIdentifier) quit")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func agentLaunchesShown() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings())).detached()
            try await instance.finishedLaunching()
            expectNoDifference([installation.agent, instance.isHidden], [true, false])
        }

        @Test(.application(FixtureApplication.self, launches: false, activationPolicy: .regular))
        func launchHidesStaysReadableAndHasNoWindowToCapture() async throws {
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings())).detached()
            try await instance.finishedLaunching()
            expectNoDifference([instance.activationPolicy == .regular, instance.isHidden, instance.isActive], [true, true, false])
            let elements: [Element] = try await read(instance)
            expectNoDifference(elements.filter { element in element.role == kAXWindowRole }.compactMap(\.title), ["Fixture 0"])
            let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await instance.screenshot() }
            let absent: Bool = failure.reason.is(\.absent)
            #expect(absent, "hidden capture failed \(failure.reason)")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func selfEnforcedSingleInstanceAlertsInNewInstanceThenExits() async throws {
            let refusing: Settings = Settings(scenario: .singleInstance, hidden: true, status: 3)
            let (installation, first): (Installation, NSRunningApplication) = try await launched(refusing)
            let second: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(refusing, new: true)).detached()
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
