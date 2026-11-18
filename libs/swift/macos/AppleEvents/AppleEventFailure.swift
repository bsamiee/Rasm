public import CasePaths
import CoreServices
public import Foundation
public import Results

// --- [ERRORS] --------------------------------------------------------------------------

public struct AppleEventFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: Equatable, Sendable {
        case dynamic
        case undeclared
        case osa(OSStatus)
        case host(NSError)
        case unmatched(Unmatched)
        case unencodable
        case activating
        case undelivered(OSStatus)
        case unanswered(OSStatus)
        case replied(OSStatus, String?)
    }

    public let subject: String
    public let reason: Reason

    package init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    public var errorDescription: String? {
        switch reason {
            case .dynamic: "\(subject) declares OSAScriptingDefinitionIsDynamic, runs no instance, and has no cached dictionary, open it to read its dictionary"
            case .undeclared: "\(subject) names no OSAScriptingDefinition, runs no instance, and has no cached dictionary, open it to read its dictionary"
            case .osa(let status): "OSACopyScriptingDefinitionFromURL failed on \(subject) (\(status))"
            case .host(let error): "\(subject): \(error.localizedDescription)"
            case .unmatched(let miss): miss.errorDescription
            case .unencodable: "Value has no \(subject) descriptor"
            case .activating: "Activating command \(subject) refused"
            case .undelivered(OSStatus(errAEEventNotPermitted)): "Automation permission denied for \(subject), grant it in System Settings > Privacy & Security > Automation"
            case .undelivered(let status): "Apple Event \(subject) undelivered (\(status))"
            case .unanswered(let status): "Application sent no reply to \(subject) (\(status)), event may have run"
            case .replied(let status, let message): "\(message ?? "Application replied to \(subject) with error") (\(status))"
        }
    }

    public var status: OSStatus? {
        switch reason {
            case .osa(let status), .undelivered(let status), .unanswered(let status), .replied(let status, _): status
            case .dynamic, .undeclared, .host, .unmatched, .unencodable, .activating: nil
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .host(let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .dynamic, .undeclared, .osa, .unencodable, .activating, .undelivered, .unanswered, .replied: [:]
        }
    }
}
