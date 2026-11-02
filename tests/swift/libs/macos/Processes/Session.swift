import CasePaths
import CustomDump
import Darwin
import Foundation
import Processes
import Subprocess
import System
import TestSupport
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@concurrent
private func terminations(listedIn fifo: URL) async throws -> [[Result<Exit, Errno>]] {
    let handle: FileHandle = try FileHandle(forReadingFrom: fifo)
    let data: Data? = try handle.readToEnd()
    let text: String? = String(bytes: try #require(data), encoding: .utf8)
    let fields: [Substring] = try #require(text).split(whereSeparator: \.isWhitespace)
    let processes: [pid_t] = fields.compactMap { field in pid_t(field) }
    let listed: Bool = processes.count == 2
    try #require(listed, "listed \(fields)")
    let parent: Terminations = termination(of: processes[0])
    let child: Terminations = termination(of: processes[1])
    return [await elements(of: parent), await elements(of: child)]
}

private func shell(_ script: String) -> Configuration {
    Configuration(executable: .path("/bin/sh"), arguments: ["-c", script], platformOptions: .session())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct Sessions {
    @Test
    func runCapturesOutputErrorAndInput() async throws {
        let result: ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>> = try await shell("/bin/cat; echo oops >&2").run(
            output: .string(limit: 1024),
            within: .seconds(30),
            input: .string("line one\nline two\n"),
            error: .string(limit: 1024),
        )
        expectNoDifference(result.standardOutput, "line one\nline two\n")
        expectNoDifference(result.standardError, "oops\n")
        expectNoDifference(result.terminationStatus, .exited(0))
    }

    @Test
    func sessionChildLeadsItsOwnProcessGroup() async throws {
        let result: ExecutionResult<Void, StringOutput<UTF8>, DiscardedOutput> = try await shell("echo $$ $(/bin/ps -o pgid= -p $$)").run(output: .string(limit: 256), within: .seconds(30))
        let fields: [pid_t] = result.standardOutput.split(whereSeparator: \.isWhitespace).compactMap { field in pid_t(field) }
        expectNoDifference(fields, [result.processIdentifier.value, result.processIdentifier.value])
        #expect(getpgrp() != result.processIdentifier.value)
    }

    @Test
    func nonzeroExitThrowsTheExitStatus() async throws {
        let failure: ProcessFailure = try await #require(throws: ProcessFailure.self) { try await shell("exit 3").run(output: .discarded, within: .seconds(30)) }
        expectNoDifference(failure.reason[case: \.exit], .exited(3))
    }

    @Test
    func signalThrowsTheSignalStatus() async throws {
        let failure: ProcessFailure = try await #require(throws: ProcessFailure.self) { try await shell("kill -TERM $$").run(output: .discarded, within: .seconds(30)) }
        expectNoDifference(failure.reason[case: \.exit], .signaled(SIGTERM))
    }

    @Test
    func missingExecutableThrowsTheSubprocessError() async throws {
        let configuration: Configuration = Configuration(executable: .path("/nonexistent/rasm-tool"), platformOptions: .session())
        let failure: ProcessFailure = try await #require(throws: ProcessFailure.self) { try await configuration.run(output: .discarded, within: .seconds(30)) }
        expectNoDifference(failure.reason[case: \.subprocess]?.code, .executableNotFound)
    }

    @Test(.testDirectory)
    func timeoutThrowsTimedOutAndTerminatesTheWholeSession() async throws {
        let fifo: URL = try #require(TestDirectory.current).appending(path: "processes")
        try #require(unsafe mkfifo(fifo.path(percentEncoded: false), 0o600) == 0)
        async let observed: [[Result<Exit, Errno>]] = terminations(listedIn: fifo)
        let script: String = "/bin/sleep 600 & echo $$ $! > '\(fifo.path(percentEncoded: false))'; wait"
        let failure: ProcessFailure = try await #require(throws: ProcessFailure.self) { try await shell(script).run(output: .discarded, within: .seconds(2)) }
        expectNoDifference(failure.reason[case: \.timedOut], .seconds(2))
        let terminated: Result<Exit, Errno> = .success(Exit(status: .signaled(SIGTERM), detail: []))
        let terminations: [[Result<Exit, Errno>]] = try await observed
        expectNoDifference(terminations, [[terminated], [terminated]])
    }

    @Test
    func failureClassifiesItsCause() async throws {
        expectNoDifference(ProcessFailure(subject: "probe", CancellationError()).reason.is(\.cancelled), true)
        let io: ProcessFailure = ProcessFailure(subject: "probe", Errno.brokenPipe)
        expectNoDifference(io.reason.is(\.io), true)
        expectNoDifference(io.subject, "probe")
        expectNoDifference(io.errorUserInfo[NSUnderlyingErrorKey] as? Errno, .brokenPipe)
        let spawn: SubprocessError = try await #require(throws: SubprocessError.self) {
            try await Subprocess.run(.path("/nonexistent/rasm-tool"), platformOptions: .session(), output: .discarded)
        }
        expectNoDifference(ProcessFailure(subject: "probe", spawn).reason[case: \.subprocess]?.code, .executableNotFound)
    }
}
