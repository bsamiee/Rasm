public import CasePaths
import CoreServices
public import Foundation
public import Results

// --- [ERRORS] --------------------------------------------------------------------------

public struct AppleEventFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: Equatable, Sendable {
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

    public let subject: String
    public let reason: Reason

    package init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    public var errorDescription: String? {
        switch reason {
            case .undeclared:
                "\(subject) holds no OSAScriptingDefinition file, sdef or aete resource named for its executable, or script suite, runs no instance, and has no cached dictionary, open it to read its dictionary"
            case .osa(let status): "OSACopyScriptingDefinitionFromURL failed on \(subject) (\(status))"
            case .foundation(let error): "\(subject): \(error.localizedDescription)"
            case .unmatched(let miss): miss.errorDescription
            case .unencodable: "Value has no \(subject) descriptor"
            case .activating: "Activating command \(subject) refused"
            case .unconsented(OSStatus(errAEEventWouldRequireUserConsent)):
                "Automation consent for Apple Event \(subject) is undecided and the event went without a prompt, asking the user once adds its switch to System Settings > Privacy & Security > Automation"
            case .unconsented: "Automation consent for Apple Event \(subject) is denied, turn on its switch in System Settings > Privacy & Security > Automation"
            case .undelivered(let status): "Apple Event \(subject) undelivered (\(status))"
            case .unanswered(let status): "Application sent no reply to \(subject) (\(status)), event may have run"
            case .replied(let status, .some(let message)): "Application replied to \(subject) with error \(status): \(message)"
            case .replied(let status, .none): "Application replied to \(subject) with error \(status)"
            case .cancelled: "Stopped waiting for the reply to \(subject), event may still run"
        }
    }

    public var status: OSStatus? {
        switch reason {
            case .osa(let status), .unconsented(let status), .undelivered(let status), .unanswered(let status), .replied(let status, _): status
            case .undeclared, .foundation, .unmatched, .unencodable, .activating, .cancelled: nil
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .foundation(let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .undeclared, .osa, .unencodable, .activating, .unconsented, .undelivered, .unanswered, .replied, .cancelled:
                status.map { status in [NSUnderlyingErrorKey: NSError(domain: NSOSStatusErrorDomain, code: Int(status))] } ?? [:]
        }
    }
}
