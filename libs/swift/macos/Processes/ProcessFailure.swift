public import CasePaths
public import Foundation
public import Subprocess

// --- [ERRORS] --------------------------------------------------------------------------

public struct ProcessFailure: CustomNSError, LocalizedError {
    @CasePathable
    public enum Reason: Sendable {
        case subprocess(SubprocessError)
        case io(any Error)
        case exit(TerminationStatus)
        case timedOut(Duration)
        case cancelled(CancellationError)
        case unlisted
        case markerNotFound
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .subprocess(let error): "\(subject) \(error.description)"
            case .io(let error): "\(subject) I/O failed: \(error.localizedDescription)"
            case .exit(.exited(let code)): "\(subject) exited with status \(code)"
            case .exit(.signaled(let signal)): "\(subject) terminated by signal \(signal)"
            case .timedOut(let timeout): "\(subject) timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "\(subject) canceled"
            case .unlisted: "getpwuid found no user database entry for \(subject)"
            case .markerNotFound: "Login shell \(subject) printed no environment"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .subprocess(let error as any Error), .io(let error): [NSUnderlyingErrorKey: error]
            case .exit, .timedOut, .cancelled, .unlisted, .markerNotFound: [:]
        }
    }
}

extension ProcessFailure {
    public init(subject: String, _ error: any Error) {
        self =
            switch error {
                case let error as CancellationError: Self(subject: subject, reason: .cancelled(error))
                case let error as Self: error
                case let error as SubprocessError: Self(subject: subject, reason: .subprocess(error))
                default: Self(subject: subject, reason: .io(error))
            }
    }
}
