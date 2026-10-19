public import Foundation
public import Subprocess
import System

// --- [TYPES] ---------------------------------------------------------------------------

public protocol DeadlineFailure: Error {
    static var timedOut: Self { get }
    static var cancelled: Self { get }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ProcessFailure: DeadlineFailure, LocalizedError {
    case launch(SubprocessError)
    case io(any Error)
    case exit(TerminationStatus)
    case timedOut
    case cancelled

    public var errorDescription: String? {
        switch self {
            case .launch(let error): "Could not start process: \(error.description)"
            case .io(let error): "Process I/O failed: \(error.localizedDescription)"
            case .exit(.exited(let code)): "Process exited with status \(code)"
            case .exit(.signaled(let signal)): "Process stopped by signal \(signal)"
            case .timedOut: "Process timed out"
            case .cancelled: "Canceled"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum ProcessRun {
    public static func withDeadline<Value: Sendable, Failure: DeadlineFailure>(
        _ deadline: Duration?,
        _ work: @escaping @Sendable () async -> Result<Value, Failure>,
    ) async -> Result<Value, Failure> {
        if let deadline {
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
        } else {
            await work()
        }
    }

    public static func run<Value: Sendable>(
        deadline: Duration?,
        _ run: @escaping @Sendable () async throws -> Value,
    ) async -> Result<Value, ProcessFailure> {
        await withDeadline(deadline) {
            await !Task.isCancelled ? Result { try await run() }.mapError(failure) : .failure(.cancelled)
        }
    }

    public static func failure(_ error: any Error) -> ProcessFailure {
        switch error {
            case is CancellationError: .cancelled
            case let error as ProcessFailure: error
            case let error as SubprocessError: .launch(error)
            default: .io(error)
        }
    }

    public static func configuration(
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
            workingDirectory: FilePath(workingDirectory.path(percentEncoded: false)),
            platformOptions: options,
        )
    }
}
