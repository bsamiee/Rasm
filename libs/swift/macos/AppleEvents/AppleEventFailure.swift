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
        case xml(any Error)
        case unmatched(Unmatched)
        case unencodable
        case undated(any Error)
        case activating
        case undelivered(OSStatus)
        case unanswered(OSStatus)
        case replied(OSStatus, String?)
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .requiresRunning: "\(subject) has no OSAScriptingDefinition, launch it to read its dictionary"
            case .osa(let status): "OSACopyScriptingDefinitionFromURL failed on \(subject) (\(status))"
            case .xml(let error): "Scripting definition XML unreadable at \(subject). \(error.localizedDescription)"
            case .unmatched(let miss): miss.errorDescription
            case .unencodable: "Value has no \(subject) descriptor"
            case .undated: "\(subject) is no ISO 8601 date"
            case .activating: "Activating command \(subject) refused"
            case .undelivered(OSStatus(errAEEventNotPermitted)): "Automation permission denied for \(subject). Grant in System Settings > Privacy & Security > Automation"
            case .undelivered(let status): "Apple Event \(subject) undelivered (\(status))"
            case .unanswered(let status): "Application sent no reply to \(subject) (\(status)). Event may have run"
            case .replied(let status, let message): "\(message ?? "Application replied to \(subject) with error") (\(status))"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .xml(let error), .undated(let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .requiresRunning, .osa, .unencodable, .activating, .undelivered, .unanswered, .replied: [:]
        }
    }
}
