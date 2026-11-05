import AsyncAlgorithms
import CustomDump
import Darwin
import Subprocess
import System
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let reporting: [(status: TerminationStatus, detail: ExitDetail, reports: Bool)] = [
    (status: .signaled(SIGSEGV), detail: [], reports: true), (status: .signaled(SIGKILL), detail: [], reports: false), (status: .signaled(SIGKILL), detail: .csError, reports: true),
    (status: .exited(0), detail: [], reports: false),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct ProcessStatuses {
    @Test
    func mainThreadUserTimeRisesOnlyForBusyChild() async throws {
        _ = try await child(running: "echo ready; while :; do :; done") { process, _, execution in
            let start: Duration = try mainThreadUserTime(of: process)
            _ = try await AsyncTimerSequence(interval: .milliseconds(10), clock: .continuous).first { _ in try mainThreadUserTime(of: process) > start }
            try execution.send(signal: .kill)
        }
    }

    @Test(arguments: reporting)
    func exitReportsOnCrashSignalsAndCodeSigningKills(status: TerminationStatus, detail: ExitDetail, reports: Bool) {
        expectNoDifference(ProcessExit(status: status, detail: detail).reports, reports)
    }

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
