package import CasePaths
package import Encoding
package import Foundation
import Security
package import Subprocess
package import System

// --- [ERRORS] --------------------------------------------------------------------------

package struct ProcessFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    package enum Reason: CaseLabeled, Equatable, Sendable {
        case subprocess(SubprocessError)
        case exit(TerminationStatus, standardError: String)
        case timedOut(Duration)
        case cancelled
        case notFound
        case corrupt(NSError)
        case system(Errno)
        case mach(kern_return_t)
        case notify(UInt32)
        case authorization(right: String, OSStatus)
        case foundation(NSError)
    }

    package let subject: String
    package let reason: Reason

    package init(subject: String, reason: Reason) {
        (self.subject, self.reason) = (subject, reason)
    }

    package init(subject: String, _ error: any Error) {
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

    package var errorDescription: String? {
        switch reason {
            case .subprocess(let error): "\(subject) \(error.description)"
            case .exit(let status, let standardError):
                standardError.split(whereSeparator: \.isNewline).last { line in !line.allSatisfy(\.isWhitespace) }.map { line in "\(subject) \(status), \(line.trimmingCharacters(in: .whitespaces))" }
                    ?? "\(subject) \(status)"
            case .timedOut(let timeout): "\(subject) timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "\(subject) canceled"
            case .notFound: "\(subject) not found"
            case .corrupt(let error): "\(subject) is corrupt: \(((error as any Error) as? DecodingError)?.failureReason ?? error.localizedDescription)"
            case .system(let error as any Error), .foundation(let error as any Error): "\(subject) failed: \(error.localizedDescription)"
            case .mach(let status): unsafe "\(subject) failed: \(String(cString: mach_error_string(status)))"
            case .notify(let status): "\(subject) failed with notify status \(status)"
            case .authorization(let right, errAuthorizationInteractionNotAllowed):
                "\(subject) needs the \(right) right, which asks for a password, so it ran no tool; `security authorizationdb write \(right) allow` grants it without one"
            case .authorization(let right, let status):
                "\(subject) did not get the \(right) right (\((SecCopyErrorMessageString(status, nil) as String?) ?? String(status))), `security authorizationdb write \(right) allow` grants it"
        }
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .subprocess(let error as any Error), .corrupt(let error as any Error), .system(let error as any Error), .foundation(let error as any Error): [NSUnderlyingErrorKey: error]
            case .mach(let status): [NSUnderlyingErrorKey: NSError(domain: NSMachErrorDomain, code: Int(status))]
            case .authorization(_, let status): [NSUnderlyingErrorKey: NSError(domain: NSOSStatusErrorDomain, code: Int(status))]
            case .exit, .timedOut, .cancelled, .notFound, .notify: [:]
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension DecodingError {
    fileprivate var failureReason: String {
        let (keys, problem): ([any CodingKey], String) =
            switch self {
                case .keyNotFound(let key, let context): (context.codingPath + [key], "missing")
                case .valueNotFound(_, let context), .typeMismatch(_, let context), .dataCorrupted(let context):
                    (context.codingPath, context.underlyingError.flatMap { error in (error as NSError).userInfo[NSDebugDescriptionErrorKey] as? String } ?? context.debugDescription)
                @unknown default: ([], localizedDescription)
            }
        let path = keys.map { key in key.intValue.map { index in "[\(index)]" } ?? ".\(key.stringValue)" }.joined().trimmingPrefix(".")
        return path.isEmpty ? problem : "key \(path): \(problem)"
    }
}
