public import CasePaths
public import Encoding
public import Foundation
public import IOKit
public import Subprocess
public import System

// --- [ERRORS] --------------------------------------------------------------------------

public struct ProcessFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: CaseLabeled, Equatable, Sendable {
        case subprocess(SubprocessError)
        case system(Errno)
        case foundation(NSError)
        case exit(TerminationStatus, standardError: String)
        case timedOut(Duration)
        case cancelled
        case unlisted
        case noMarker
        case notify(UInt32)
        case ioKit(IOReturn)
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .subprocess(let error): "\(subject) \(error.description)"
            case .system(let error as any Error), .foundation(let error as any Error): "\(subject) failed: \(error.localizedDescription)"
            case .exit(.exited(let code), let standardError): Self.describeExit("\(subject) exited with status \(code)", with: standardError)
            case .exit(.signaled(let signal), let standardError): Self.describeExit("\(subject) terminated by signal \(signal)", with: standardError)
            case .timedOut(let timeout): "\(subject) timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "\(subject) canceled"
            case .unlisted: "getpwuid found no user database entry for \(subject)"
            case .noMarker: "Login shell \(subject) printed no environment"
            case .notify(let status): "\(subject) failed with notify status \(status)"
            case .ioKit(let status): unsafe "\(subject) failed: \(String(cString: mach_error_string(status)))"
        }
    }

    private static func describeExit(_ head: String, with standardError: String) -> String {
        standardError.split(whereSeparator: \.isNewline).last.map { line in "\(head), \(line.trimmingCharacters(in: .whitespaces))" } ?? head
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .subprocess(let error as any Error), .system(let error as any Error), .foundation(let error as any Error): [NSUnderlyingErrorKey: error]
            case .exit, .timedOut, .cancelled, .unlisted, .noMarker, .notify, .ioKit: [:]
        }
    }
}

extension ProcessFailure {
    public init(subject: String, _ error: any Error) {
        self =
            switch error {
                case is CancellationError: Self(subject: subject, reason: .cancelled)
                case let error as Self: error
                case let error as SubprocessError: Self(subject: subject, reason: .subprocess(error))
                case let error as Errno: Self(subject: subject, reason: .system(error))
                default: Self(subject: subject, reason: .foundation(error as NSError))
            }
    }
}
