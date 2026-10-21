public import Foundation
public import Results
public import Subprocess

// --- [ERRORS] --------------------------------------------------------------------------

public enum ProcessFailure: DeadlineFailure, CustomNSError, LocalizedError {
    case launch(SubprocessError)
    case io(any Error)
    case exit(TerminationStatus)
    case timedOut(Duration)
    case cancelled

    public init(_ error: any Error) {
        self =
            switch error {
                case is CancellationError: .cancelled
                case let error as Self: error
                case let error as SubprocessError: .launch(error)
                default: .io(error)
            }
    }

    public var errorDescription: String? {
        switch self {
            case .launch(let error): "Could not start process: \(error.description)"
            case .io(let error): "Process I/O failed: \(error.localizedDescription)"
            case .exit(.exited(let code)): "Process exited with status \(code)"
            case .exit(.signaled(let signal)): "Process stopped by signal \(signal)"
            case .timedOut(let timeout): "Process did not finish within \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "Canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .launch(let error as any Error), .io(let error): [NSUnderlyingErrorKey: error]
            case .exit, .timedOut, .cancelled: [:]
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension PlatformOptions {
    public static var session: Self {
        var options: Self = Self()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
        return options
    }
}
