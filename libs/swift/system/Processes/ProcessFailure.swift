package import CasePaths
package import Foundation
package import Results
package import Subprocess
package import System

// --- [ERRORS] --------------------------------------------------------------------------

package struct ProcessFailure: ModuleError, Equatable {
    package struct Status: Equatable, Sendable, StatusCode {
        let code: Int32
        package let name: String

        package init(kernel code: kern_return_t) {
            (self.code, name) = (code, unsafe String(cString: mach_error_string(code)))
        }

        package init(security code: OSStatus) {
            (self.code, name) = (code, Self.constant(of: code))
        }
    }

    @CasePathable
    package enum Reason: Equatable, Sendable {
        case subprocess(SubprocessError)
        case exit(TerminationStatus, standardError: String)
        case timedOut(Duration)
        case cancelled
        case notFound
        case corrupt(NSError)
        case system(Errno)
        case mach(Status)
        case notify(UInt32)
        case authorization(right: String, Status)
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

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .subprocess(let error as any Error), .corrupt(let error as any Error), .system(let error as any Error), .foundation(let error as any Error): [NSUnderlyingErrorKey: error]
            case .mach(let status): [NSUnderlyingErrorKey: NSError(domain: NSMachErrorDomain, code: Int(status.code))]
            case .authorization(_, let status): [NSUnderlyingErrorKey: NSError(domain: NSOSStatusErrorDomain, code: Int(status.code))]
            case .exit, .timedOut, .cancelled, .notFound, .notify: [:]
        }
    }
}
