import CustomDump
import Darwin
import Foundation
import Processes
import Subprocess
import System
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let statuses: [String: ProcessStatus] = ["R": .running, "S": .sleeping, "I": .sleeping, "U": .sleeping, "T": .stopped]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func expectStatusMatchesProcessState(of process: pid_t) async throws {
    let before: String = try await processState(of: process)
    let status: ProcessStatus = try ProcessStatus(of: process)
    let after: String = try await processState(of: process)
    let agreed: Bool = before == after
    try #require(agreed, "ps read \(before) then \(after)")
    expectNoDifference(status, statuses[before])
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct ProcessStatuses {
    @Test
    func blockedReaderIsSleeping() async throws {
        _ = try await child(running: "echo ready; exec /bin/cat") { process, _, execution in
            try await expectStatusMatchesProcessState(of: process)
            expectNoDifference(try ProcessStatus(of: process), .sleeping)
            try execution.send(signal: .kill)
        }
    }

    @Test
    func busyLoopIsRunning() async throws {
        _ = try await child(running: "echo ready; while :; do :; done") { process, _, execution in
            try await expectStatusMatchesProcessState(of: process)
            expectNoDifference(try ProcessStatus(of: process), .running)
            try execution.send(signal: .kill)
        }
    }

    @Test
    func stoppedChildIsStoppedUntilContinued() async throws {
        _ = try await child(running: "echo ready; read _") { process, _, execution in
            try execution.send(signal: .suspend)
            _ = await stopStatus(of: process)
            try await expectStatusMatchesProcessState(of: process)
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

    @Test
    func wireSpellsEachStatusAsTheReadersDecode() throws {
        let encoder: JSONEncoder = JSONEncoder()
        let spellings: [String?] = try [ProcessStatus.running, .sleeping, .stopped].map { status in String(bytes: try encoder.encode(status), encoding: .utf8) }
        expectNoDifference(spellings, [#""running""#, #""sleeping""#, #""stopped""#])
    }
}
