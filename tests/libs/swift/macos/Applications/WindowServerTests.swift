import AppKit
import AsyncAlgorithms
import CasePathsCore
import CoreGraphics
import CustomDump
import Foundation
import Synchronization
import System
import Testing

@testable import Applications

// --- [SERVICES] ------------------------------------------------------------------------

struct ExpiredClock: Clock {
    var now: ContinuousClock.Instant { .now }
    var minimumResolution: Duration { .zero }

    func sleep(until _: ContinuousClock.Instant, tolerance _: Duration?) throws {}
}

final class Captures: Sendable {
    @TaskLocal static var current: Captures?

    let shots = Mutex<[Screenshot]>([])
    let failed = Atomic<Bool>(false)

    func take(in watch: Watch) async {
        let numbers: [CGWindowID] = await watch.current[case: \.running]?.windows.filter(\.onScreen).map(\.number) ?? []
        guard !numbers.isEmpty else { return }
        let taken: [Result<Screenshot, ApplicationFailure>]
        do throws(ApplicationFailure) {
            taken = try await watch.capture(.windows(numbers: numbers))
        } catch {
            taken = [.failure(error)]
        }
        shots.withLock { shots in shots += taken.compactMap { shot in shot[case: \.success] } }
        for refusal in taken.compactMap({ shot in shot[case: \.failure] }) { Issue.record("Capturing windows \(numbers) answered \(refusal.localizedDescription)", severity: .warning) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

struct CapturesOnFailure: SuiteTrait, TestTrait, TestScoping {
    var isRecursive: Bool { true }

    func provideScope(for _: Test, testCase _: Test.Case?, performing function: @concurrent @Sendable () async throws -> Void) async throws {
        let captures = Captures()
        let outcome: Result<Void, any Error>
        do {
            outcome = .success(try await Captures.$current.withValue(captures) { try await function() })
        } catch {
            captures.failed.store(true, ordering: .relaxed)
            outcome = .failure(error)
        }
        if captures.failed.load(ordering: .relaxed) {
            for shot in captures.shots.withLock({ shots in shots }) { Attachment.record(shot.image, named: "window \(shot.frame)", as: .png) }
        }
        try outcome.get()
    }
}

@Suite(
    .serialized,
    .timeLimit(.minutes(1)),
    CapturesOnFailure(),
    .compactMapIssues { issue in
        if issue.isFailure { Captures.current?.failed.store(true, ordering: .relaxed) }
        return issue
    },
)
struct WindowServerTests {}

extension WindowServerTests {
    @Suite(.enabled(if: PermittedFrontList.entries == 0, "Counting entries a scope leaves needs the permitted front list empty of every other holder"))
    struct PermittedFrontListTests {
        @Test
        func leavesNoEntryWhenEachLockWaitExpiresAsItsScopeRegisters() async throws {
            let refusals: [ApplicationFailure.Reason] = try await PermittedFrontList.shared.holding(excluding: { _ in false }, for: .seconds(30)) { () async -> [ApplicationFailure.Reason] in
                var refusals: [ApplicationFailure.Reason] = []
                for _ in 0..<200 {
                    let outcome = await Result { () async throws(ApplicationFailure) in
                        try await PermittedFrontList.shared.holding(excluding: { _ in false }, for: .seconds(30), clock: ExpiredClock()) { () async in }.value.get()
                    }
                    if case .failure(let failure) = outcome { refusals.append(failure.reason) }
                }
                return refusals
            }
            .value.get()
            #expect(refusals.allSatisfy { reason in reason == .system(.wouldBlock) }, "Expired lock waits answered \(refusals)")
            expectNoDifference(PermittedFrontList.entries, 0, "Permitted front entries left after 200 scopes whose lock wait expired at once")
        }

        @Test
        func leavesNoEntryWhenItsScopeIsCancelledWhileHeld() async throws {
            let scope = Task {
                try await PermittedFrontList.shared.holding(excluding: { _ in false }, for: .seconds(30)) { () async throws in try await Task.sleep(for: .seconds(30)) }.value.get()
            }
            let held: Int? = await AsyncTimerSequence(interval: .milliseconds(5), clock: ContinuousClock()).prefix(400).map { _ in PermittedFrontList.entries }.first { entries in entries > 0 }
            scope.cancel()
            _ = await scope.result
            #expect(held != nil, "The scope never listed an entry within 2 s")
            expectNoDifference(PermittedFrontList.entries, 0, "Permitted front entries left after the scope was cancelled")
        }

        @Test
        func releasesItsHoldAtItsLifetimeWhileItsBodyRuns() async throws {
            let (during, ended): (Int, Int) = try await PermittedFrontList.shared.holding(excluding: { _ in false }, for: .milliseconds(200)) { () async throws -> (Int, Int) in
                let listed: Int = PermittedFrontList.entries
                try await Task.sleep(for: .seconds(1))
                return (listed, PermittedFrontList.entries)
            }
            .value.get()
            #expect(during > 0, "The scope listed no entry before its lifetime ended")
            expectNoDifference(ended, 0, "Permitted front entries 800 ms past a 200 ms lifetime while the body still ran")
        }
    }
}
