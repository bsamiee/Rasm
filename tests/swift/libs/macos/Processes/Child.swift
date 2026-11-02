import Darwin
import Foundation
import Processes
import Subprocess
import System
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

func child<Value: Sendable>(
    running script: String,
    _ body: (_ process: pid_t, _ firstLine: String, _ execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>) async throws -> Value,
) async throws -> (value: Value, status: TerminationStatus) {
    let result: ExecutionResult<Value, SequenceOutput, DiscardedOutput> = try await Subprocess.run(
        Configuration(executable: .path("/bin/sh"), arguments: ["-c", script], platformOptions: .session()),
        input: .inputWriter,
        output: .sequence,
        error: .discarded,
    ) { execution in
        var lines: SubprocessOutputSequence.StringSequence<UTF8>.AsyncIterator = execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"]), bufferingPolicy: .unbounded)
            .makeAsyncIterator()
        let first: String = try #require(try await lines.next())
        return try await body(execution.processIdentifier.value, first, execution)
    }
    return (result.closureResult, result.terminationStatus)
}

func processState(of process: pid_t) async throws -> String {
    let result: ExecutionResult<Void, StringOutput<UTF8>, DiscardedOutput> = try await Subprocess.run(
        Configuration(executable: .path("/bin/ps"), arguments: ["-o", "state=", "-p", "\(process)"], platformOptions: .session()),
        output: .string(limit: 64),
    )
    return String(result.standardOutput.prefix(1))
}

@concurrent
func stopStatus(of process: pid_t) async -> Int32 {
    var status: Int32 = 0
    _ = unsafe waitpid(process, &status, WUNTRACED)
    return status
}

func reapedProcess() async throws -> pid_t {
    try await child(running: "echo ready") { process, _, _ in process }.value
}
