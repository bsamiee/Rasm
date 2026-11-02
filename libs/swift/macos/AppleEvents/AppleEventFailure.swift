public import CasePaths
import CoreServices
public import Foundation
public import Results

// --- [ERRORS] --------------------------------------------------------------------------

public struct AppleEventFailure: CustomNSError, LocalizedError {
    @CasePathable
    public enum Reason: Sendable {
        case requiresRunning
        case osa(OSStatus)
        case host(any Error)
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
            case .requiresRunning: "\(subject) has no OSAScriptingDefinition, open it to read its dictionary"
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

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .host(let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .requiresRunning, .osa, .unencodable, .activating, .undelivered, .unanswered, .replied: [:]
        }
    }
}
