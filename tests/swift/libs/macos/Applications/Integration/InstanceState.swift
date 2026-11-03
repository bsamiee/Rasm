import AppKit
import Applications
import CasePaths
import CustomDump
import Darwin
import Processes
import Subprocess
import System
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let hangs: [(scenario: Scenario, process: ProcessStatus)] = [(.hangBlocked, .sleeping), (.hangQuiet, .sleeping), (.hangBusy, .running)]
private let exits: [(scenario: Scenario, status: Int32)] = [(.exit, 7), (.terminate, 0)]

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct InstanceStates {
        @Test(.application(FixtureApplication.self, launches: false))
        func slowLaunchHoldsLaunchingUntilLaunchFinishes() async throws {
            try checkAccessibilityTrust()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(try installation(), arguments: Settings(scenario: .slowLaunch).arguments).instance
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.elements.contains { element in element.title == "Fixture 0" } == true },
                acting: { kill(instance.processIdentifier, SIGUSR1) },
            )
            expectNoDifference([phases(seen).first, phases(seen).last], [.launching, .responding])
            expectNoDifference(seen.last?.elements.filter(\.requiresAnswer).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings(scenario: .launchModal).arguments))
        func launchModalShowsBeforeLaunchFinishesAndHoldsMenus() async throws {
            try checkAccessibilityTrust()
            let instance: NSRunningApplication = try #require(TestApplication.instance)
            let asked: [InstanceState] = try await observe(instance, until: { seen in seen.last?.elements.contains(where: \.requiresAnswer) == true })
            expectNoDifference(phases(Array(asked.dropLast())).filter { phase in phase != .launching }, [])
            let modal: Element = try #require(asked.last?.asking)
            expectNoDifference(
                try shape(modal),
                try json(#"{"role":"AXWindow","subrole":"AXDialog","modal":true,"identifier":"_NS:87","description":"alert","minimized":false,"actions":[],"requiresAnswer":true}"#),
            )
            expectNoDifference(modal.detail?.text, ["Launch modal", Scenario.launchModal.rawValue])
            expectNoDifference(try modal.detail.map { detail in Set(try detail.controls.map { control in try shape(control) }) }, try alertButtons())
            let items: [Control] = try menu(String(describing: Scenario.self), in: try await read(instance))
            expectNoDifference(items.filter(\.enabled).compactMap(\.label), [])
            let ok: Control = try #require(modal.detail?.controls.first { control in control.identifier == "action-button-1" })
            let answered: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.count > 1 && seen.last?.elements.contains { element in element.title == "Fixture 0" } == true },
                acting: { try await ElementAction.perform(element: ok.id, action: kAXPressAction).perform() },
            )
            expectNoDifference(answered.last?.elements.filter(\.requiresAnswer).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func windowAccessibilityOmitsListsUnderWindowsWithItsFacts() async throws {
            try #require(CGPreflightScreenCaptureAccess(), "window titles need Screen Recording grant")
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings())
            let described: CGWindowID = try await numbered("Fixture 0", in: instance)
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.windows?.contains { window in window.title == "Undescribed" } == true },
                acting: { try await run(.undescribed, in: instance) },
            )
            let state: InstanceState = try #require(seen.last)
            let listed: [Window] = try #require(state.windows)
            let undescribed: Window = try #require(listed.first { window in window.title == "Undescribed" })
            let size: CGSize = await NSWindow.frameRect(forContentRect: NSRect(origin: .zero, size: undescribedSize), styleMask: [.titled]).size
            expectNoDifference(state.elements.contains { element in element.title == "Undescribed" }, false)
            expectNoDifference(listed.contains { window in window.number == described }, false)
            expectNoDifference([undescribed.frame.width, undescribed.frame.height], [size.width, size.height])
            expectNoDifference([undescribed.layer, undescribed.onScreen ? 1 : 0], [0, 1])
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: hangs)
        func hangTurnsUnresponsiveWithMainThreadStatusUsageAndEveryWindowThenRecovers(scenario: Scenario, process: ProcessStatus) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings(hidden: true))
            let described: CGWindowID = try await numbered("Fixture 0", in: instance)
            let hung: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase.is(\.unresponsive) == true },
                acting: { try await run(scenario, in: instance) },
            )
            let turned: InstanceState = try #require(hung.last)
            expectNoDifference(phases(hung).last, .unresponsive(process))
            expectNoDifference(turned.windows?.contains { window in window.number == described }, true)
            _ = try #require(turned.usage, "unresponsive line holds usage").get()
            let recovered: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase == .responding },
                acting: { kill(instance.processIdentifier, SIGUSR1) },
            )
            expectNoDifference(recovered.last?.phase, .responding)
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func stoppedProcessReadsStoppedWithoutMessageAndResumes() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let stopped: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase == .unresponsive(.stopped) },
                acting: { try await run(.stop, in: instance) },
            )
            expectNoDifference(phases(stopped), [.responding, .unresponsive(.stopped)])
            let sampled: Bool = stopped.last?.usage?[case: \.success] != nil
            #expect(sampled, "unresponsive line holds usage")
            let resumed: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase == .responding },
                acting: { kill(instance.processIdentifier, SIGCONT) },
            )
            expectNoDifference(phases(resumed), [.unresponsive(.stopped), .responding])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings(status: 7).arguments), arguments: exits)
        func exitReadsItsStatusOnceAndEndsWithoutReport(scenario: Scenario, status: Int32) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(instance, until: { _ in false }, acting: { try await run(scenario, in: instance) })
            expectNoDifference(phases(seen), [.responding, .exited(.exited(status))])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func killReadsSignalAndEndsWithoutWaitingForReport() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(instance, until: { _ in false }, acting: { kill(instance.processIdentifier, SIGKILL) })
            expectNoDifference(phases(seen), [.responding, .exited(.signaled(SIGKILL))])
        }
    }
}
