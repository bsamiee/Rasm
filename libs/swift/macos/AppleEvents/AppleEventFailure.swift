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

    public var status: OSStatus? {
        switch reason {
            case .osa(let status), .undelivered(let status), .unanswered(let status), .replied(let status, _): status
            case .requiresRunning, .host, .unmatched, .unencodable, .activating: nil
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

extension AppleEventFailure: Equatable {
    public static func == (left: Self, right: Self) -> Bool {
        let same: Bool =
            switch left.reason {
                case .requiresRunning: right.reason.is(\.requiresRunning)
                case .osa(let status): right.reason[case: \.osa] == Optional(status)
                case .host(let error):
                    right.reason[case: \.host].map { other in ((other as NSError).domain, (other as NSError).code) == ((error as NSError).domain, (error as NSError).code) } == true
                case .unmatched(let miss): right.reason[case: \.unmatched] == Optional(miss)
                case .unencodable: right.reason.is(\.unencodable)
                case .activating: right.reason.is(\.activating)
                case .undelivered(let status): right.reason[case: \.undelivered] == Optional(status)
                case .unanswered(let status): right.reason[case: \.unanswered] == Optional(status)
                case .replied(let status, let message): right.reason[case: \.replied].map { other in other == (status, message) } == true
            }
        return left.subject == right.subject && same
    }
}
