import Applications
import CasePathsCore
import CoreGraphics
import Foundation
import ScreenCapture
import Synchronization
import Testing

// --- [SERVICES] ------------------------------------------------------------------------

final class Captures: Sendable {
    @TaskLocal static var current: Captures?

    let shots = Mutex<[Screenshot]>([])
    let failed = Atomic<Bool>(false)

    func take(in watch: Watch) async {
        let numbers: [CGWindowID] = await watch.current[case: \.running]?.windows.filter(\.onScreen).map(\.number) ?? []
        guard !numbers.isEmpty else { return }
        let taken: [Result<Screenshot, ApplicationFailure>]
        do throws(ApplicationFailure) {
            taken = try await watch.instance.capture(.windows(numbers: numbers), in: watch)
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
