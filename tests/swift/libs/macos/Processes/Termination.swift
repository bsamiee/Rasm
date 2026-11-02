import AsyncAlgorithms
import CustomDump
import Darwin
import Foundation
import PropertyBased
import Subprocess
import System
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let detailMask: Int32 = NOTE_EXIT_DECRYPTFAIL | NOTE_EXIT_MEMORY | NOTE_EXIT_CSERROR
private let signals: [Signal] = [.kill, .terminate, .terminalClosed, .interrupt, .userDefinedOne, .userDefinedTwo, .alarm]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct WaitStatuses {
    @Test
    func exitStatusesDecodeTheirCodeAndIgnoreDetailBits() async {
        await propertyCheck(input: Gen.int32(in: 0...255), Gen.int32(in: 0...7)) { code, detail in
            let status: Int32 = (code << 8) | (detail << 16 & detailMask)
            expectNoDifference(TerminationStatus(waitStatus: status), .exited(code))
        }
    }

    @Test
    func signalStatusesDecodeTheirSignalWithOrWithoutCoreDump() async {
        await propertyCheck(input: Gen.int32(in: 1...126), Gen.bool, Gen.int32(in: 0...7)) { signal, core, detail in
            let status: Int32 = signal | (core ? WCOREFLAG : 0) | (detail << 16 & detailMask)
            expectNoDifference(TerminationStatus(waitStatus: status), .signaled(signal))
        }
    }
}

@Suite(.timeLimit(.minutes(1)))
struct ProcessTerminations {
    @Test(arguments: [Int32(0), 7, 255, 300])
    func childExitYieldsOneTruncatedExitCodeThenFinishes(code: Int32) async throws {
        let (observed, status): (value: [Result<ProcessExit, Errno>], status: TerminationStatus) = try await child(running: "echo ready; read _; exit \(code)") { process, _, execution in
            let stream: AsyncStream<Result<ProcessExit, Errno>> = termination(of: process)
            try await execution.standardInputWriter.finish()
            return await Array(stream)
        }
        expectNoDifference(observed, [.success(ProcessExit(status: .exited(code & 0xFF), detail: []))])
        expectNoDifference(status, .exited(code & 0xFF))
    }

    @Test(arguments: signals)
    func childKilledBySignalYieldsThatSignal(signal: Signal) async throws {
        let (observed, status): (value: [Result<ProcessExit, Errno>], status: TerminationStatus) = try await child(running: "echo ready; exec /bin/cat") { process, _, execution in
            let stream: AsyncStream<Result<ProcessExit, Errno>> = termination(of: process)
            try execution.send(signal: signal)
            return await Array(stream)
        }
        expectNoDifference(observed, [.success(ProcessExit(status: .signaled(signal.rawValue), detail: []))])
        expectNoDifference(status, .signaled(signal.rawValue))
    }

    @Test
    func stopAndContinueYieldNothingBeforeExit() async throws {
        let (observed, stop): (value: [Result<ProcessExit, Errno>], status: TerminationStatus) = try await child(running: "echo ready; read _; exit 9") { process, _, execution in
            let stream: AsyncStream<Result<ProcessExit, Errno>> = termination(of: process)
            try execution.send(signal: .suspend)
            let stopped: Int32 = await stopStatus(of: process)
            expectNoDifference([stopped & 0o177, (stopped >> 8) & 0xFF], [0o177, SIGSTOP])
            try execution.send(signal: .resume)
            try await execution.standardInputWriter.finish()
            return await Array(stream)
        }
        expectNoDifference(observed, [.success(ProcessExit(status: .exited(9), detail: []))])
        expectNoDifference(stop, .exited(9))
    }

    @Test
    func grandchildExitCodeReachesWatcherThatIsNotItsParent() async throws {
        let (observed, _): (value: [Result<ProcessExit, Errno>], status: TerminationStatus) = try await child(running: "/bin/sh -c 'read _; exit 5' <&0 & echo $!; wait") { _, line, execution in
            let parsed: pid_t? = pid_t(line)
            let grandchild: pid_t = try #require(parsed)
            let stream: AsyncStream<Result<ProcessExit, Errno>> = termination(of: grandchild)
            try await execution.standardInputWriter.finish()
            return await Array(stream)
        }
        expectNoDifference(observed, [.success(ProcessExit(status: .exited(5), detail: []))])
    }

    @Test
    func grandchildSignalReachesWatcherThatIsNotItsParent() async throws {
        let (observed, _): (value: [Result<ProcessExit, Errno>], status: TerminationStatus) = try await child(running: "/bin/sleep 600 & echo $!; read _; kill -TERM $!; wait") { _, line, execution in
            let parsed: pid_t? = pid_t(line)
            let grandchild: pid_t = try #require(parsed)
            let stream: AsyncStream<Result<ProcessExit, Errno>> = termination(of: grandchild)
            try await execution.standardInputWriter.finish()
            return await Array(stream)
        }
        expectNoDifference(observed, [.success(ProcessExit(status: .signaled(SIGTERM), detail: []))])
    }

    @Test
    func reapedProcessYieldsNoSuchProcessThenFinishes() async throws {
        let process: pid_t = try await reapedProcess()
        let observed: [Result<ProcessExit, Errno>] = await Array(termination(of: process))
        expectNoDifference(observed, [.failure(.noSuchProcess)])
    }
}
