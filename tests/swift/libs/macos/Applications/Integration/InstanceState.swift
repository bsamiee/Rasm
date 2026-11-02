import AppKit
import Applications
import CustomDump
import Darwin
import Processes
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let slack: Duration = .milliseconds(1500)
private let hangs: [(scenario: Scenario, process: ProcessStatus)] = [(.hangBlocked, .sleeping), (.hangQuiet, .sleeping), (.hangBusy, .running)]
private let exits: [(scenario: Scenario, exited: String)] = [(.exit, "exited status 7"), (.terminate, "exited status 0")]

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct InstanceStates {
        @Test(.application(FixtureApplication.self, arguments: Settings(scenario: .slowLaunch, duration: .seconds(3)).arguments, answering: answerCrashAlert))
        func slowLaunchHoldsLaunchingUntilLaunchFinishes() async throws {
            try checkAccessibilityTrust()
            let instance: NSRunningApplication = try instance(of: try installation())
            let seen: [Observed] = try await observe(instance, within: .seconds(20), until: { seen in seen.last?.state.elements.contains { element in element.title == "Fixture 0" } == true })
            expectNoDifference(summaries(seen), ["launching", "responding"])
            let responded: Observed = try #require(seen.first { observed in summary(observed.state) == "responding" })
            expectElapsed(responded.instant - seen[0].instant, in: .seconds(1)...(.seconds(3) + slack), "responding of a 3 s launch")
            expectNoDifference(responded.state.elements.filter(\.requiresAnswer).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings(scenario: .launchModal).arguments, answering: answerCrashAlert))
        func launchModalShowsBeforeLaunchFinishesAndHoldsTheMenus() async throws {
            try checkAccessibilityTrust()
            let instance: NSRunningApplication = try instance(of: try installation())
            let asked: [Observed] = try await observe(instance, within: .seconds(20), until: { seen in seen.last?.state.elements.contains(where: \.requiresAnswer) == true })
            expectNoDifference(summaries(Array(asked.dropLast())).filter { kind in kind != "launching" }, [])
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

        @Test(
            .application(FixtureApplication.self, launches: false, answering: answerCrashAlert),
            arguments: hangs,
        )
        func hangTurnsUnresponsiveWithinAHeartbeatAndRecovers(scenario: Scenario, process: ProcessStatus) async throws {
            let hang: Duration = .seconds(5)
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings(hidden: true, duration: hang))
            let baseline: ResourceUsage = try #require(await instance.state { _ in false }.usage)
            let hung: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last.map { observed in summary(observed.state).hasPrefix("unresponsive") } == true },
                acting: { try await run(scenario, in: instance) },
            )
            let start: ContinuousClock.Instant = hung[0].instant
            let turned: Observed = try #require(hung.last)
            expectNoDifference(summaries(hung).last, "unresponsive \(process.rawValue)")
            expectElapsed(turned.instant - start, in: .milliseconds(900)...(heartbeat * 2 + slack), "unresponsive")
            let first: ResourceUsage = try #require(turned.state.usage, "the line turning unresponsive carries usage")
            let reread: InstanceState = await instance.state { _ in false }
            let second: ResourceUsage = try #require(reread.usage)
            expectNoDifference(summary(reread), "unresponsive \(process.rawValue)")
            let (rising, earlier): (Duration, Duration) = (second.cpu - first.cpu, first.cpu - baseline.cpu)
            switch process {
                case .running:
                    expectElapsed(earlier, in: .milliseconds(600)...hang, "busy cpu before the unresponsive line")
                    expectElapsed(rising, in: .milliseconds(500)...hang, "busy cpu between unresponsive lines")
                case .sleeping, .stopped:
                    expectElapsed(earlier, in: .zero...(.milliseconds(300)), "waiting cpu before the unresponsive line")
                    expectElapsed(rising, in: .zero...(.milliseconds(200)), "waiting cpu between unresponsive lines")
            }
            let recovered: [Observed] = try await observe(instance, within: .seconds(15), until: { seen in seen.last.map { observed in summary(observed.state) == "responding" } == true })
            let back: ContinuousClock.Instant = try #require(recovered.last?.instant)
            expectElapsed(back - start, in: (hang - .milliseconds(200))...(hang + heartbeat * 2 + slack), "responding again")
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func stoppedProcessReadsStoppedWithoutAMessageAndResumes() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let stopped: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last.map { observed in summary(observed.state) == "unresponsive stopped" } == true },
                acting: { try await run(.stop, in: instance) },
            )
            expectNoDifference(summaries(stopped), ["responding", "unresponsive stopped"])
            let turned: Observed = try #require(stopped.last)
            expectElapsed(turned.instant - stopped[0].instant, in: .zero...(heartbeat + slack), "stopped read")
            let sampled: Bool = turned.state.usage != nil
            #expect(sampled, "the line turning unresponsive carries usage")
            let resumed: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last.map { observed in summary(observed.state) == "responding" } == true },
                acting: { kill(instance.processIdentifier, SIGCONT) },
            )
            expectNoDifference(summaries(resumed), ["unresponsive stopped", "responding"])
            let back: Observed = try #require(resumed.last)
            expectElapsed(back.instant - resumed[0].instant, in: .zero...(heartbeat * 2 + slack), "responding after the resume")
        }

        @Test(
            .application(FixtureApplication.self, arguments: Settings(status: 7).arguments, answering: answerCrashAlert),
            arguments: exits,
        )
        func exitReadsItsStatusOnceAndEndsWithoutAReport(scenario: Scenario, exited: String) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [Observed] = try await observe(instance, within: .seconds(15), until: { _ in false }, acting: { try await run(scenario, in: instance) })
            expectNoDifference(summaries(seen), ["responding", exited])
            let last: Observed = try #require(seen.last)
            expectElapsed(last.instant - seen[0].instant, in: .zero...(.seconds(5)), "stream end")
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func killReadsTheSignalAndEndsWithoutWaitingForAReport() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [Observed] = try await observe(instance, within: .seconds(15), until: { _ in false }, acting: { kill(instance.processIdentifier, SIGKILL) })
            expectNoDifference(summaries(seen), ["responding", "exited signal 9"])
            let last: Observed = try #require(seen.last)
            expectElapsed(last.instant - seen[0].instant, in: .zero...(.seconds(5)), "stream end")
        }
    }
}
