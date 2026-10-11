import Foundation
import Subprocess
import System

// --- [TYPES] ---------------------------------------------------------------------------

nonisolated protocol DeadlineFailure: Error {
    static var timedOut: Self { get }
    static var cancelled: Self { get }
}

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum ProcessFailure: DeadlineFailure, LocalizedError {
    case launch(SubprocessError)
    case io(any Error)
    case exit(TerminationStatus)
    case timedOut
    case cancelled
    case descriptorNotInherited(Int32)

    var errorDescription: String? {
        switch self {
            case .launch(let error): "Could not start process: \(error.description)"
            case .io(let error): "Process I/O failed: \(error.localizedDescription)"
            case .exit(.exited(let code)): "Process exited with status \(code)"
            case .exit(.signaled(let signal)): "Process stopped by signal \(signal)"
            case .timedOut: "Process timed out"
            case .cancelled: "Canceled"
            case .descriptorNotInherited(let descriptor): "posix_spawn could not inherit descriptor \(descriptor)"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum ProcessRun {
    static func withDeadline<Value: Sendable, Failure: DeadlineFailure>(
        _ deadline: Duration,
        _ work: @escaping @Sendable () async -> Result<Value, Failure>,
    ) async -> Result<Value, Failure> {
        await withTaskGroup { group in
            group.addTask(name: "work") { await work() }
            group.addTask(name: "deadline") {
                await Result { try await Task.sleep(for: deadline) }
                    .mapError { _ in Failure.cancelled }
                    .flatMap { _ in .failure(.timedOut) }
            }
            let first: Result<Value, Failure> = await group.next() ?? .failure(.cancelled)
            group.cancelAll()
            return first
        }
    }

    static func stream<Value: Sendable>(
        _ invocation: Configuration,
        deadline: Duration?,
        _ body:
            @escaping @Sendable (Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>)
            async -> Result<Value, ProcessFailure>,
    ) async -> Result<(Value, TerminationStatus), ProcessFailure> {
        let work: @Sendable () async -> Result<(Value, TerminationStatus), ProcessFailure> = {
            guard !Task.isCancelled else { return .failure(.cancelled) }
            let outcome:
                Result<
                    ExecutionResult<Result<Value, ProcessFailure>, SequenceOutput, DiscardedOutput>, any Error
                > = await Result {
                    try await Subprocess.run(
                        invocation,
                        input: .inputWriter,
                        output: .sequence,
                        error: .discarded,
                        body: body,
                    )
                }
            return outcome.mapError(failure).flatMap { result in
                result.closureResult.map { value in (value, result.terminationStatus) }
            }
        }
        return switch deadline {
            case .some(let deadline): await withDeadline(deadline, work)
            case .none: await work()
        }
    }

    static func collect<Input: InputProtocol>(
        _ invocation: Configuration,
        deadline: Duration,
        input: Input = .none,
    ) async -> Result<ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>>, ProcessFailure> {
        await withDeadline(deadline) {
            guard !Task.isCancelled else { return .failure(.cancelled) }
            let outcome:
                Result<
                    ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>>, any Error
                > = await Result {
                    try await Subprocess.run(
                        invocation,
                        input: input,
                        output: .string(limit: 1 << 20),
                        error: .string(limit: 1 << 20),
                    )
                }
            return outcome.mapError(failure)
        }
    }

    static func failure(_ error: any Error) -> ProcessFailure {
        switch error {
            case is CancellationError: .cancelled
            case let error as ProcessFailure: error
            case let error as SubprocessError: .launch(error)
            default: .io(error)
        }
    }

    static func configuration(
        executable: Executable,
        arguments: [String],
        environment: [String: String],
        workingDirectory: URL,
    ) -> Configuration {
        var options: PlatformOptions = PlatformOptions()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
        return Configuration(
            executable: executable,
            arguments: Arguments(arguments),
            environment: .custom(
                Dictionary(
                    uniqueKeysWithValues: environment.map { key, value in
                        (Environment.Key(stringLiteral: key), value)
                    }
                )
            ),
            workingDirectory: FilePath(workingDirectory.path),
            platformOptions: options,
        )
    }
}
