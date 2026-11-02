import CustomDump
import Darwin
import Foundation
import PropertyBased
import Subprocess
import System
import Testing

@testable import Processes

// --- [TYPES] ---------------------------------------------------------------------------

typealias Terminations = AsyncStream<Result<(status: TerminationStatus, detail: ExitDetail), Errno>>

// --- [CONSTANTS] -----------------------------------------------------------------------

private let detailMask: Int32 = NOTE_EXIT_DECRYPTFAIL | NOTE_EXIT_MEMORY | NOTE_EXIT_CSERROR
private let signals: [Signal] = [.kill, .terminate, .terminalClosed, .interrupt, .userDefinedOne, .userDefinedTwo, .alarm]

// --- [MODELS] --------------------------------------------------------------------------

struct Exit: Equatable {
    let status: TerminationStatus
    let detail: ExitDetail
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func elements(of stream: Terminations) async -> [Result<Exit, Errno>] {
    var elements: [Result<Exit, Errno>] = []
    for await element: Result<(status: TerminationStatus, detail: ExitDetail), Errno> in stream {
        elements.append(element.map { exit in Exit(status: exit.status, detail: exit.detail) })
    }
    return elements
}

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

    @Test
    func detailBitsNameTheKernelReasons() {
        expectNoDifference(ExitDetail.decryptFail.rawValue, NOTE_EXIT_DECRYPTFAIL)
        expectNoDifference(ExitDetail.memory.rawValue, NOTE_EXIT_MEMORY)
        expectNoDifference(ExitDetail.csError.rawValue, NOTE_EXIT_CSERROR)
        expectNoDifference(ExitDetail(rawValue: detailMask), [.decryptFail, .memory, .csError])
    }
}

@Suite(.timeLimit(.minutes(1)))
struct ProcessTerminations {
    @Test(arguments: [Int32(0), 7, 255, 300])
    func childExitYieldsOneTruncatedExitCodeThenFinishes(code: Int32) async throws {
        let (observed, status): (value: [Result<Exit, Errno>], status: TerminationStatus) = try await child(running: "echo ready; read _; exit \(code)") { process, _, execution in
            let stream: Terminations = termination(of: process)
            try await execution.standardInputWriter.finish()
            return await elements(of: stream)
        }
        expectNoDifference(observed, [.success(Exit(status: .exited(code & 0xFF), detail: []))])
        expectNoDifference(status, .exited(code & 0xFF))
    }

    @Test(arguments: signals)
    func childKilledBySignalYieldsThatSignal(signal: Signal) async throws {
        let (observed, status): (value: [Result<Exit, Errno>], status: TerminationStatus) = try await child(running: "echo ready; exec /bin/cat") { process, _, execution in
            let stream: Terminations = termination(of: process)
            try execution.send(signal: signal)
            return await elements(of: stream)
        }
        expectNoDifference(observed, [.success(Exit(status: .signaled(signal.rawValue), detail: []))])
        expectNoDifference(status, .signaled(signal.rawValue))
    }

    @Test
    func stopAndContinueYieldNothingBeforeTheExit() async throws {
        let (observed, stop): (value: [Result<Exit, Errno>], status: TerminationStatus) = try await child(running: "echo ready; read _; exit 9") { process, _, execution in
            let stream: Terminations = termination(of: process)
            try execution.send(signal: .suspend)
            let stopped: Int32 = await stopStatus(of: process)
            expectNoDifference([stopped & 0o177, (stopped >> 8) & 0xFF], [0o177, SIGSTOP])
            try execution.send(signal: .resume)
            try await execution.standardInputWriter.finish()
            return await elements(of: stream)
        }
        expectNoDifference(observed, [.success(Exit(status: .exited(9), detail: []))])
        expectNoDifference(stop, .exited(9))
    }

    @Test
    func grandchildExitCodeReachesAWatcherThatIsNotItsParent() async throws {
        let (observed, _): (value: [Result<Exit, Errno>], status: TerminationStatus) = try await child(running: "/bin/sh -c 'read _; exit 5' <&0 & echo $!; wait") { _, line, execution in
            let parsed: pid_t? = pid_t(line)
            let grandchild: pid_t = try #require(parsed)
            let stream: Terminations = termination(of: grandchild)
            try await execution.standardInputWriter.finish()
            return await elements(of: stream)
        }
        expectNoDifference(observed, [.success(Exit(status: .exited(5), detail: []))])
    }

    @Test
    func grandchildSignalReachesAWatcherThatIsNotItsParent() async throws {
        let (observed, _): (value: [Result<Exit, Errno>], status: TerminationStatus) = try await child(running: "/bin/sleep 600 & echo $!; read _; kill -TERM $!; wait") { _, line, execution in
            let parsed: pid_t? = pid_t(line)
            let grandchild: pid_t = try #require(parsed)
            let stream: Terminations = termination(of: grandchild)
            try await execution.standardInputWriter.finish()
            return await elements(of: stream)
        }
        expectNoDifference(observed, [.success(Exit(status: .signaled(SIGTERM), detail: []))])
    }

    @Test
    func reapedProcessYieldsNoSuchProcessThenFinishes() async throws {
        let process: pid_t = try await reapedProcess()
        let observed: [Result<Exit, Errno>] = await elements(of: termination(of: process))
        expectNoDifference(observed, [.failure(.noSuchProcess)])
    }
}
