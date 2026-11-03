import AsyncAlgorithms
import CustomDump
import Darwin
import Processes
import Subprocess
import System
import Testing

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct ProcessStatuses {
    @Test(arguments: [("echo ready; exec /bin/cat", ProcessStatus.sleeping), ("echo ready; while :; do :; done", .running)])
    func blockedReaderIsSleepingAndBusyLoopIsRunning(script: String, status: ProcessStatus) async throws {
        _ = try await child(running: script) { process, _, execution in
            _ = try await AsyncTimerSequence(interval: .milliseconds(10), clock: .continuous).first { _ in try ProcessStatus(of: process) == status }
            try execution.send(signal: .kill)
        }
    }

    @Test
    func stoppedChildIsStoppedUntilContinued() async throws {
        _ = try await child(running: "echo ready; read _") { process, _, execution in
            try execution.send(signal: .suspend)
            _ = await stopStatus(of: process)
            expectNoDifference(try ProcessStatus(of: process), .stopped)
            try execution.send(signal: .resume)
            try await execution.standardInputWriter.finish()
        }
    }

    @Test
    func callingProcessIsRunning() throws {
        expectNoDifference(try ProcessStatus(of: getpid()), .running)
    }

    @Test
    func reapedProcessThrowsNoSuchProcess() async throws {
        let process: pid_t = try await reapedProcess()
        #expect(throws: Errno.noSuchProcess) { try ProcessStatus(of: process) }
    }
}
