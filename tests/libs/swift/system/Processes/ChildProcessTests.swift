import CustomDump
import Darwin
import Foundation
import Subprocess
import System
import Testing

@testable import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ChildProcessTests {
    @Test
    func failsWithTheExitStatusAndWholeStandardErrorOfAChildExitingNonzero() async throws {
        let failure = await #expect(throws: ProcessFailure.self) {
            try await Configuration(executable: .path("/bin/sh"), arguments: ["-c", "printf 'first\\nlast\\n \\n' >&2; exit 3"]).run(output: .discarded, within: .seconds(10))
        }
        expectNoDifference(failure?.reason, .exit(.exited(3), standardError: "first\nlast\n \n"))
        let description = try #require(failure?.errorDescription)
        #expect(description.hasSuffix(", last"))
    }

    @Test
    func reportsCancellationInPlaceOfTheSignalItsTeardownSent() async {
        let reason = await Task { () async -> ProcessFailure.Reason? in
            do throws(ProcessFailure) {
                try await Configuration(executable: .path("/bin/sleep"), arguments: ["60"]).run(input: .none, output: .discarded) { _ in
                    unsafe withUnsafeCurrentTask { task in unsafe task?.cancel() }
                }
                return nil
            } catch {
                return error.reason
            }
        }
        .value
        expectNoDifference(reason, .cancelled)
    }

    @Test(.timeLimit(.minutes(1)))
    func timeoutEndsEveryProcessOfTheSessionItsChildStarted() async throws {
        let file = URL.temporaryDirectory.appending(path: "\(UUID().uuidString).pid")
        defer { try? FileManager.default.removeItem(at: file) }
        let failure = await #expect(throws: ProcessFailure.self) {
            try await Configuration(
                executable: .path("/bin/sh"),
                arguments: ["-c", "sleep 60 & echo $! > \"$0\"; wait", file.path(percentEncoded: false)],
                platformOptions: .session,
            )
            .run(output: .discarded, within: .seconds(1))
        }
        expectNoDifference(failure?.reason, .timedOut(.seconds(1)))
        let written = try String(contentsOf: file, encoding: .utf8)
        let parsed = pid_t(written.trimmingCharacters(in: .newlines))
        let grandchild = try #require(parsed)
        let deadline = ContinuousClock.now + .seconds(5)
        while kill(grandchild, 0) == 0, ContinuousClock.now < deadline { try await Task.sleep(for: .milliseconds(20)) }
        #expect(kill(grandchild, 0) == -1 && errno == ESRCH)
    }
}
