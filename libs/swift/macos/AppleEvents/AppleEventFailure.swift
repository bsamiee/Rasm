package import CasePaths
package import Foundation
package import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

package let privacySettings: String = "x-apple.systempreferences:com.apple.settings.PrivacySecurity.extension"

// --- [ERRORS] --------------------------------------------------------------------------

package struct AppleEventFailure: ModuleError, Equatable {
    package struct Status: Equatable, Sendable, StatusCode {
        package let code: OSStatus

        package init(_ code: OSStatus) { self.code = code }

        package var name: String { Self.constant(of: code) }
    }

    @CasePathable
    package enum Reason: Equatable, Sendable {
        case undeclared
        case osa(Status)
        case foundation(NSError)
        case unmatched(Unmatched)
        case unencodable
        case activating
        case unconsented(Status, settings: String)
        case undelivered(Status)
        case unanswered(Status)
        case replied(Status, message: String?, offending: String?)
        case cancelled
    }

    package let subject: String
    package let reason: Reason

    package init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .foundation(let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .osa(let status), .unconsented(let status, _), .undelivered(let status), .unanswered(let status), .replied(let status, _, _):
                [NSUnderlyingErrorKey: NSError(domain: NSOSStatusErrorDomain, code: Int(status.code))]
            case .undeclared, .unencodable, .activating, .cancelled: [:]
        }
    }
}
