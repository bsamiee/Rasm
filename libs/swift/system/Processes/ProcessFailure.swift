public import CasePaths
public import Encoding
public import Foundation
public import Subprocess
public import System

// --- [ERRORS] --------------------------------------------------------------------------

public struct ProcessFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: CaseLabeled, Equatable, Sendable {
        case subprocess(SubprocessError)
        case exit(TerminationStatus, standardError: String)
        case timedOut(Duration)
        case cancelled
        case notFound
        case corrupt(NSError)
        case system(Errno)
        case mach(kern_return_t)
        case notify(UInt32)
        case foundation(NSError)
    }

    public let subject: String
    public let reason: Reason

    public init(subject: String, reason: Reason) {
        (self.subject, self.reason) = (subject, reason)
    }

    public init(subject: String, _ error: any Error) {
        self =
            switch error {
                case is CancellationError: Self(subject: subject, reason: .cancelled)
                case let error as Self: error
                case let error as SubprocessError: Self(subject: subject, reason: .subprocess(error))
                case let error as DecodingError: Self(subject: subject, reason: .corrupt(error as NSError))
                case let error as Errno: Self(subject: subject, reason: .system(error))
                default: Self(subject: subject, reason: .foundation(error as NSError))
            }
    }

    public var errorDescription: String? {
        switch reason {
            case .subprocess(let error): "\(subject) \(error.description)"
            case .exit(let status, let standardError):
                standardError.split(whereSeparator: \.isNewline).last { line in !line.allSatisfy(\.isWhitespace) }.map { line in "\(subject) \(status), \(line.trimmingCharacters(in: .whitespaces))" }
                    ?? "\(subject) \(status)"
            case .timedOut(let timeout): "\(subject) timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "\(subject) canceled"
            case .notFound: "\(subject) not found"
            case .corrupt(let error): "\(subject) is corrupt: \(error.localizedDescription)"
            case .system(let error as any Error), .foundation(let error as any Error): "\(subject) failed: \(error.localizedDescription)"
            case .mach(let status): unsafe "\(subject) failed: \(String(cString: mach_error_string(status)))"
            case .notify(let status): "\(subject) failed with notify status \(status)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .subprocess(let error as any Error), .corrupt(let error as any Error), .system(let error as any Error), .foundation(let error as any Error): [NSUnderlyingErrorKey: error]
            case .mach(let status): [NSUnderlyingErrorKey: NSError(domain: NSMachErrorDomain, code: Int(status))]
            case .exit, .timedOut, .cancelled, .notFound, .notify: [:]
        }
    }
}
