package import CasePaths
import CoreServices
package import Encoding
package import Foundation
package import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

package let privacySettings: String = "x-apple.systempreferences:com.apple.settings.PrivacySecurity.extension"

// --- [ERRORS] --------------------------------------------------------------------------

package struct AppleEventFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    package enum Reason: CaseLabeled, Equatable, Sendable {
        case undeclared
        case osa(OSStatus)
        case foundation(NSError)
        case unmatched(Unmatched)
        case unencodable
        case activating
        case unconsented(OSStatus)
        case undelivered(OSStatus)
        case unanswered(OSStatus)
        case replied(OSStatus, String?)
        case cancelled
    }

    package let subject: String
    package let reason: Reason

    package init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    package var errorDescription: String? {
        switch reason {
            case .undeclared:
                "\(subject) holds no OSAScriptingDefinition file, sdef or aete resource named for its executable, or script suite, runs no instance, and has no cached dictionary, open it to read its dictionary"
            case .osa(let status): "OSACopyScriptingDefinitionFromURL failed on \(subject) (\(status))"
            case .foundation(let error): "\(subject): \(error.localizedDescription)"
            case .unmatched(let miss): miss.errorDescription
            case .unencodable: "Value has no \(subject) descriptor"
            case .activating: "Activating command \(subject) refused"
            case .unconsented(OSStatus(errAEEventWouldRequireUserConsent)):
                "Automation consent for Apple Event \(subject) is undecided and went without a prompt, asking once adds its Automation switch at \(privacySettings)?Privacy_Automation"
            case .unconsented: "Automation consent for Apple Event \(subject) is denied, turn on its switch in Privacy & Security > Automation at \(privacySettings)?Privacy_Automation"
            case .undelivered(let status): "Apple Event \(subject) undelivered (\(status))"
            case .unanswered(let status): "Application sent no reply to \(subject) (\(status)), event may have run"
            case .replied(let status, .some(let message)): "Application replied to \(subject) with error \(status): \(message)"
            case .replied(let status, .none): "Application replied to \(subject) with error \(status)"
            case .cancelled: "Stopped waiting for the reply to \(subject), event may still run"
        }
    }

    package var status: OSStatus? {
        switch reason {
            case .osa(let status), .unconsented(let status), .undelivered(let status), .unanswered(let status), .replied(let status, _): status
            case .undeclared, .foundation, .unmatched, .unencodable, .activating, .cancelled: nil
        }
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .foundation(let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .undeclared, .osa, .unencodable, .activating, .unconsented, .undelivered, .unanswered, .replied, .cancelled:
                status.map { status in [NSUnderlyingErrorKey: NSError(domain: NSOSStatusErrorDomain, code: Int(status))] } ?? [:]
        }
    }
}

extension AppleEventFailure: Encodable {
    private enum Key: CodingKey {
        case reason, subject, message, status
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(reason, forKey: .reason)
        try fields.encode(subject, forKey: .subject)
        try fields.encodeIfPresent(errorDescription, forKey: .message)
        try fields.encodeIfPresent(status, forKey: .status)
    }
}
