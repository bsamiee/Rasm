public import Foundation
public import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

public enum ProcessFailure: DeadlineFailure, LocalizedError {
    case launch(SubprocessError)
    case io(any Error)
    case exit(TerminationStatus)
    case timedOut(Duration)
    case cancelled

    public var errorDescription: String? {
        switch self {
            case .launch(let error): "Could not start process: \(error.description)"
            case .io(let error): "Process I/O failed: \(error.localizedDescription)"
            case .exit(.exited(let code)): "Process exited with status \(code)"
            case .exit(.signaled(let signal)): "Process stopped by signal \(signal)"
            case .timedOut(let deadline): "Process did not finish within \(deadline.formatted(.units(width: .wide)))"
            case .cancelled: "Canceled"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum ProcessRun {
    static let outputLimit: Int = 1 << 20

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
