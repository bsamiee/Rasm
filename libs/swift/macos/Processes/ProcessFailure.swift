public import Foundation
public import Results
public import Subprocess

// --- [ERRORS] --------------------------------------------------------------------------

public enum ProcessFailure: DeadlineFailure, CustomNSError, LocalizedError {
    case subprocess(SubprocessError)
    case io(any Error)
    case exit(TerminationStatus)
    case deadline(Deadline)

    public init(_ error: any Error) {
        self =
            switch error {
                case is CancellationError: .deadline(.cancelled)
                case let error as Self: error
                case let error as SubprocessError: .subprocess(error)
                default: .io(error)
            }
    }

    public var errorDescription: String? {
        switch self {
            case .subprocess(let error): error.description
            case .io(let error): "Process I/O failed: \(error.localizedDescription)"
            case .exit(.exited(let code)): "Process exited with status \(code)"
            case .exit(.signaled(let signal)): "Process stopped by signal \(signal)"
            case .deadline(.timedOut(let timeout)): "Process did not finish within \(timeout.formatted(.units(width: .wide)))"
            case .deadline(.cancelled): "Canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .subprocess(let error as any Error), .io(let error): [NSUnderlyingErrorKey: error]
            case .exit, .deadline: [:]
        }
    }
}
