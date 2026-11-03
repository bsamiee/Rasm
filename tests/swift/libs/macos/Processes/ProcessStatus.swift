import AsyncAlgorithms
import CustomDump
import Darwin
import Processes
import Subprocess
import System
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let statuses: [String: ProcessStatus] = ["R": .running, "S": .sleeping, "I": .sleeping, "U": .sleeping, "T": .stopped]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func settled(_ process: pid_t, reading expected: ProcessStatus) async throws {
    for await _: ContinuousClock.Instant in AsyncTimerSequence(interval: .milliseconds(10), clock: .continuous) {
        guard try ProcessStatus(of: process) != expected else { break }
    }
    let state: String = try await processState(of: process)
    expectNoDifference(statuses[state], expected)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct ProcessStatuses {
    @Test
    func blockedReaderIsSleeping() async throws {
        _ = try await child(running: "echo ready; exec /bin/cat") { process, _, execution in
            try await settled(process, reading: .sleeping)
            try execution.send(signal: .kill)
        }
    }

    @Test
    func busyLoopIsRunning() async throws {
        _ = try await child(running: "echo ready; while :; do :; done") { process, _, execution in
            try await settled(process, reading: .running)
            try execution.send(signal: .kill)
        }
    }

    @Test
    func stoppedChildIsStoppedUntilContinued() async throws {
        _ = try await child(running: "echo ready; read _") { process, _, execution in
            try execution.send(signal: .suspend)
            _ = await stopStatus(of: process)
            try await settled(process, reading: .stopped)
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
