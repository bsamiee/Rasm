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
        defer {
            do { try FileManager.default.removeItem(at: file) } catch { Issue.record(error) }
        }
        let failure = await #expect(throws: ProcessFailure.self) {
            try await Configuration(
                executable: .path("/bin/sh"),
                arguments: ["-c", "sleep 600 & echo $! > \"$0\"; wait", file.path(percentEncoded: false)],
                platformOptions: .session,
            )
            .run(output: .discarded, within: .seconds(1))
        }
        expectNoDifference(failure?.reason, .timedOut(.seconds(1)))
        let grandchild = try #require(pid_t(try String(contentsOf: file, encoding: .utf8).trimmingCharacters(in: .newlines)))
        let (exits, exited) = AsyncStream<Void>.makeStream()
        let source = DispatchSource.makeProcessSource(identifier: grandchild, eventMask: .exit, queue: .global())
        source.setEventHandler { exited.yield() }
        source.activate()
        let ended: Bool = await exits.contains { _ in true }
        source.cancel()
        if !ended { kill(grandchild, SIGKILL) }
        #expect(ended, "process \(grandchild) of the timed-out session outlived its teardown")
    }
}
