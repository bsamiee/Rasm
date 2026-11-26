public import CasePaths
public import Encoding
public import Foundation
public import Subprocess

// --- [ERRORS] --------------------------------------------------------------------------

public struct ProcessFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: CaseLabeled, Sendable {
        case subprocess(SubprocessError)
        case io(any Error)
        case exit(TerminationStatus, standardError: String)
        case timedOut(Duration)
        case cancelled(CancellationError)
        case unlisted
        case noMarker
        case notify(UInt32)
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .subprocess(let error): "\(subject) \(error.description)"
            case .io(let error): "\(subject) I/O failed: \(error.localizedDescription)"
            case .exit(.exited(let code), let standardError): Self.describeExit("\(subject) exited with status \(code)", with: standardError)
            case .exit(.signaled(let signal), let standardError): Self.describeExit("\(subject) terminated by signal \(signal)", with: standardError)
            case .timedOut(let timeout): "\(subject) timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "\(subject) canceled"
            case .unlisted: "getpwuid found no user database entry for \(subject)"
            case .noMarker: "Login shell \(subject) printed no environment"
            case .notify(let status): "\(subject) failed with notify status \(status)"
        }
    }

    private static func describeExit(_ head: String, with standardError: String) -> String {
        standardError.split(whereSeparator: \.isNewline).last.map { line in "\(head), \(line.trimmingCharacters(in: .whitespaces))" } ?? head
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .subprocess(let error as any Error), .io(let error): [NSUnderlyingErrorKey: error]
            case .exit, .timedOut, .cancelled, .unlisted, .noMarker, .notify: [:]
        }
    }

    public static func == (left: Self, right: Self) -> Bool { (left.subject, left.errorDescription) == (right.subject, right.errorDescription) }
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
