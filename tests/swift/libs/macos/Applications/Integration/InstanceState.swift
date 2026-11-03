import AppKit
import Applications
import CasePaths
import CustomDump
import Darwin
import Processes
import Subprocess
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
        @Test(.application(FixtureApplication.self, arguments: Settings(scenario: .slowLaunch, duration: .seconds(3)).arguments))
        func slowLaunchHoldsLaunchingUntilLaunchFinishes() async throws {
            try checkAccessibilityTrust()
            let instance: NSRunningApplication = try #require(TestApplication.instance)
            let seen: [Observed] = try await observe(instance, within: .seconds(20), until: { seen in seen.last?.state.elements.contains { element in element.title == "Fixture 0" } == true })
            expectNoDifference(phases(seen), [.launching, .responding])
            expectNoDifference(seen.last?.state.elements.filter(\.requiresAnswer).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings(scenario: .launchModal).arguments))
        func launchModalShowsBeforeLaunchFinishesAndHoldsMenus() async throws {
            try checkAccessibilityTrust()
            let instance: NSRunningApplication = try #require(TestApplication.instance)
            let asked: [Observed] = try await observe(instance, within: .seconds(20), until: { seen in seen.last?.state.elements.contains(where: \.requiresAnswer) == true })
            expectNoDifference(phases(Array(asked.dropLast())).filter { phase in phase != .launching }, [])
            let modal: Element = try #require(asked.last?.state.asking)
            expectNoDifference(
                try shape(modal),
                try json(#"{"role":"AXWindow","subrole":"AXDialog","modal":true,"identifier":"_NS:87","description":"alert","minimized":false,"actions":[],"requiresAnswer":true}"#),
            )
            expectNoDifference(modal.detail?.text, ["Launch modal", "launchModal"])
            expectNoDifference(try modal.detail.map { detail in Set(try detail.controls.map { control in try shape(control) }) }, try alertButtons())
            let items: [Control] = try menu("Scenario", in: try await read(instance))
            expectNoDifference(items.filter(\.enabled).compactMap(\.label), [])
            let ok: Control = try #require(modal.detail?.controls.first { control in control.identifier == "action-button-1" })
            let answered: [Observed] = try await observe(
                instance,
                within: .seconds(20),
                until: { seen in seen.count > 1 && seen.last?.state.elements.contains { element in element.title == "Fixture 0" } == true },
                acting: { try await ElementAction.perform(element: ok.id, action: kAXPressAction).perform() },
            )
            expectNoDifference(answered.last?.state.elements.filter(\.requiresAnswer).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func windowAccessibilityOmitsListsUnderWindowsWithItsFacts() async throws {
            try #require(CGPreflightScreenCaptureAccess(), "window titles need Screen Recording grant")
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings())
            let described: CGWindowID = try await numbered("Fixture 0", in: instance)
            let seen: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last?.state.windows?.contains { window in window.title == "Undescribed" } == true },
                acting: { try await run(.undescribed, in: instance) },
            )
            let state: InstanceState = try #require(seen.last).state
            let listed: [Window] = try #require(state.windows)
            let undescribed: Window = try #require(listed.first { window in window.title == "Undescribed" })
            let size: CGSize = await NSWindow.frameRect(forContentRect: NSRect(origin: .zero, size: undescribedSize), styleMask: [.titled]).size
            expectNoDifference(state.elements.contains { element in element.title == "Undescribed" }, false)
            expectNoDifference(listed.contains { window in window.number == described }, false)
            expectNoDifference([undescribed.frame.width, undescribed.frame.height], [size.width, size.height])
            expectNoDifference([undescribed.layer, undescribed.onScreen ? 1 : 0], [0, 1])
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: hangs)
        func hangTurnsUnresponsiveListingEveryWindowUsingCPUOfItsStatusThenRecovers(scenario: Scenario, process: ProcessStatus) async throws {
            let clock: ContinuousClock = ContinuousClock()
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings(hidden: true))
            let described: CGWindowID = try await numbered("Fixture 0", in: instance)
            let hung: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last?.state.phase.is(\.unresponsive) == true },
                acting: { try await run(scenario, in: instance) },
            )
            let turned: Observed = try #require(hung.last)
            expectNoDifference(phases(hung).last, .unresponsive(process))
            expectNoDifference(turned.state.windows?.contains { window in window.number == described }, true)
            let first: ResourceUsage = try #require(turned.state.usage, "unresponsive line holds usage")
            let sampled: ContinuousClock.Instant = clock.now
            let reread: InstanceState = await instance.state { _ in false }
            let second: ResourceUsage = try #require(reread.usage)
            let share: Double = (second.cpu - first.cpu) / (clock.now - sampled)
            expectNoDifference(reread.phase, .unresponsive(process))
            let matchesStatus: Bool = process == .running ? share > 0.25 : share < 0.25
            #expect(matchesStatus, "\(process) took \(share) of a core")
            let recovered: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last?.state.phase == .responding },
                acting: { kill(instance.processIdentifier, SIGUSR1) },
            )
            expectNoDifference(recovered.last?.state.phase, .responding)
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func stoppedProcessReadsStoppedWithoutMessageAndResumes() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let stopped: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last?.state.phase == .unresponsive(.stopped) },
                acting: { try await run(.stop, in: instance) },
            )
            expectNoDifference(phases(stopped), [.responding, .unresponsive(.stopped)])
            let sampled: Bool = stopped.last?.state.usage != nil
            #expect(sampled, "unresponsive line holds usage")
            let resumed: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last?.state.phase == .responding },
                acting: { kill(instance.processIdentifier, SIGCONT) },
            )
            expectNoDifference(phases(resumed), [.unresponsive(.stopped), .responding])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings(status: 7).arguments), arguments: exits)
        func exitReadsItsStatusOnceAndEndsWithoutReport(scenario: Scenario, status: Int32) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [Observed] = try await observe(instance, within: .seconds(15), until: { _ in false }, acting: { try await run(scenario, in: instance) })
            expectNoDifference(phases(seen), [.responding, .exited(.exited(status))])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func killReadsSignalAndEndsWithoutWaitingForReport() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [Observed] = try await observe(instance, within: .seconds(15), until: { _ in false }, acting: { kill(instance.processIdentifier, SIGKILL) })
            expectNoDifference(phases(seen), [.responding, .exited(.signaled(SIGKILL))])
        }
    }
}
