import AppKit
import CasePaths
import CustomDump
import Darwin
import Processes
import Subprocess
import System
import TestSupport
import Testing

@testable import AppKitFixture
@testable import Applications

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
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(try installation(), environment: Settings(scenario: .slowLaunch).environment).instance
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.elements.contains { element in element.title == "Fixture 0" } == true },
                acting: { kill(instance.processIdentifier, SIGUSR1) },
            )
            expectNoDifference([phases(seen).first, phases(seen).last], [.launching, .responding])
            expectNoDifference(seen.last?.elements.filter(\.asking).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, environment: Settings(scenario: .launchModal).environment))
        func launchModalAsksBeforeLaunchFinishesAndHoldsMenus() async throws {
            try checkAccessibilityTrust()
            let instance: NSRunningApplication = try #require(TestApplication.instance)
            let asked: [InstanceState] = try await observe(instance, until: { seen in seen.last?.asking != nil })
            expectNoDifference(phases(Array(asked.dropLast())).filter { phase in phase != .launching }, [])
            let modal: Element = try #require(asked.last?.asking)
            expectNoDifference(modal.kind, .alert)
            expectNoDifference(modal.detail?.text, ["Launch modal", Scenario.launchModal.rawValue])
            let items: [Control] = try menu(String(describing: Scenario.self), in: try await read(instance))
            expectNoDifference(items.filter(\.enabled).compactMap(\.label), [])
            let ok: Control = try #require(modal.answers?.accept)
            let answered: InstanceState = try await ElementAction.perform(element: ok.id, action: kAXPressAction).perform(in: instance.processIdentifier)
            expectNoDifference(answered.elements.filter(\.asking).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func windowAccessibilityOmitsReadsUndescribedWithItsFacts() async throws {
            try #require(CGPreflightScreenCaptureAccess(), "window titles need Screen Recording grant")
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.rows.contains { row in row.title == "Undescribed" } == true },
                acting: { try await run(.undescribed, in: instance) },
            )
            let row: InstanceState.Window = try #require(seen.last?.rows.first { row in row.title == "Undescribed" })
            let size: CGSize = await NSWindow.frameRect(forContentRect: NSRect(origin: .zero, size: undescribedSize), styleMask: [.titled]).size
            expectNoDifference(row.description, .undescribed)
            #expect(row.presented, "row arrived after the first responding beat")
            expectNoDifference([row.frame.width, row.frame.height], [size.width, size.height])
            expectNoDifference(row.onScreen, false)
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: hangs)
        func hangTurnsUnresponsiveWithMainThreadStatusUsageAndNothingAskingThenRecovers(scenario: Scenario, process: ProcessStatus) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let described: CGWindowID = try await numbered("Fixture 0", in: instance)
            let hung: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase.is(\.unresponsive) == true },
                acting: { try await run(scenario, in: instance) },
            )
            let turned: InstanceState = try #require(hung.last)
            expectNoDifference(turned.rows.contains { row in row.number == described }, true)
            expectNoDifference(turned[case: \.running]?.asking.map(\.number), [])
            _ = try #require(turned.usage, "unresponsive state holds usage").get()
            let recovered: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase == .responding },
                acting: { kill(instance.processIdentifier, SIGUSR1) },
            )
            expectNoDifference(recovered.last?.phase, .responding)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func blockedThreadWithANewWindowReadsBlocking() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?[case: \.running]?.asking.isEmpty == false },
                acting: { try await run(.hangWindow, in: instance) },
            )
            let row: InstanceState.Window = try #require(seen.last?[case: \.running]?.asking.first)
            expectNoDifference(row.description, .blocking)
            let silent: Bool = seen.last?.phase.is(\.unresponsive) == true
            #expect(silent)
            kill(instance.processIdentifier, SIGUSR1)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func blockedModalSessionReadsBlockingInOneRead() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            _ = try await observe(instance, until: { seen in seen.last?.phase.is(\.unresponsive) == true }, acting: { try await run(.modalBlocked, in: instance) })
            let read: InstanceState = await instance.state()
            expectNoDifference(read[case: \.running]?.asking.map(\.description), [.blocking])
            kill(instance.processIdentifier, SIGUSR1)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func stoppedProcessReadsStoppedWithoutMessageAndResumes() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let stopped: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase == .unresponsive(.stopped) },
                acting: { try await run(.stop, in: instance) },
            )
            let sampled: Bool = stopped.last?.usage?[case: \.success] != nil
            #expect(sampled, "unresponsive state holds usage")
            let resumed: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.phase == .responding },
                acting: { kill(instance.processIdentifier, SIGCONT) },
            )
            expectNoDifference(phases(resumed), [.unresponsive(.stopped), .responding])
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: exits)
        func exitReadsItsStatusOnceAndEndsWithoutReport(scenario: Scenario, status: Int32) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings(status: 7))
            let seen: [InstanceState] = try await observe(instance, until: { _ in false }, acting: { try await run(scenario, in: instance) })
            expectNoDifference(phases(seen), [.responding, .exited(.exited(status))])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func killReadsSignalAndEndsWithoutWaitingForReport() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(instance, until: { _ in false }, acting: { kill(instance.processIdentifier, SIGKILL) })
            expectNoDifference(phases(seen), [.responding, .exited(.signaled(SIGKILL))])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func quitEndsOnExitAndStopsAtAnAskingDialog() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            _ = try await observe(instance, until: { seen in seen.last?.asking != nil }, acting: { try await run(.sheet, in: instance) })
            var asked: [Result<InstanceState, ApplicationFailure>] = []
            for await step: Result<InstanceState, ApplicationFailure> in instance.quit() { asked.append(step) }
            let asking: Element? = try asked.last?.get().asking
            let answerable: Bool = asking != nil
            #expect(answerable)
            expectNoDifference(instance.isTerminated, false)
            _ = try await ElementAction.perform(element: try #require(asking?.answers?.cancel).id, action: kAXPressAction).perform(in: instance.processIdentifier)
            var quit: [Result<InstanceState, ApplicationFailure>] = []
            for await step: Result<InstanceState, ApplicationFailure> in instance.quit() { quit.append(step) }
            expectNoDifference(try quit.last?.get().phase, .exited(.exited(0)))
        }
    }
}
