import AppKit
import ApplicationServices
import CasePathsCore
import CustomDump
import Fixture
import Foundation
import Processes
import Subprocess
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    @Suite(.enabled(if: AXIsProcessTrusted(), "Watching the fixture from outside needs this test process to hold the Accessibility grant"))
    struct QuitTests {
        @Test
        func endsWithTheExitOfAnInstanceAnsweringTheQuitEvent() async throws {
            let state: InstanceState = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                return try await watch.quit(.asking, within: .seconds(10))
            }
            #expect(state.is(\.exited), "quit returned \(state) for an instance with no unsaved document")
        }

        @Test
        func endsWithTheCleanExitOfAnInstanceBusyClosingItsWindowsPastTheMessagingTimeout() async throws {
            let state: InstanceState = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.hangOnTermination(kind: .spinning(.seconds(3))))
                return try await watch.quit(.asking, within: .seconds(10))
            }
            expectNoDifference(state[case: \.exited]?.exit.map(\.status), .success(.exited(0)))
        }

        @Test
        func returnsTheUnresponsiveStateOfAHungInstanceBeforeTheReplyDeadline() async throws {
            let (state, waited): (InstanceState, Duration) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.hang(kind: .sleeping(.seconds(20))))
                _ = try await fixture.next(\.hanging)
                let started = ContinuousClock.now
                let state = try await watch.quit(.asking, within: .seconds(30))
                return (state, ContinuousClock.now - started)
            }
            expectNoDifference(state[case: \.running]?.status, .unresponsive(thread: .sleeping))
            #expect(waited < Duration.seconds(10), "quit waited \(waited) on an instance hung for 20 s under a 30 s reply deadline")
        }

        @Test
        func returnsTheRunningStateOnceTheSaveSheetItsQuitRaisedAsksBeforeTheReplyDeadline() async throws {
            let (state, asked, waited): (InstanceState, Bool, Duration) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.edit)
                let messages = await watch.messages()
                let asking = Task { await messages.contains { message in message[case: \.event]?.change.is(\.asking) == true } }
                let started = ContinuousClock.now
                let state = try await watch.quit(.asking, within: .seconds(30))
                let waited = ContinuousClock.now - started
                asking.cancel()
                return (state, await asking.value, waited)
            }
            #expect(state.is(\.running), "quit returned \(state) while the save sheet of its edited document asks")
            #expect(asked, "no asking event reported the save sheet the quit raised")
            #expect(waited < Duration.seconds(10), "quit waited \(waited) on a save sheet under a 30 s reply deadline")
        }

        @Test
        func endsAHungInstanceWithTheKillSignalWhenForcing() async throws {
            let state: InstanceState = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.hang(kind: .sleeping(.seconds(20))))
                _ = try await fixture.next(\.hanging)
                return try await watch.quit(.forcing, within: .seconds(10))
            }
            expectNoDifference(state[case: \.exited]?.exit.map(\.status), .success(.signaled(SIGKILL)))
        }
    }
}
