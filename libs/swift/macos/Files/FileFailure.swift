public import CasePaths
public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public struct FileFailure: CustomNSError, LocalizedError {
    @CasePathable
    public enum Reason: Sendable {
        case unwatched
        case held
        case compromised
        case filesystem(any Error)
    }

    public let subject: String
    public let reason: Reason

    init(file: URL, reason: Reason) {
        subject = file.path(percentEncoded: false)
        self.reason = reason
    }

    public var errorDescription: String? {
        switch reason {
            case .unwatched: "FSEvents stream on \(subject) failed to start"
            case .held: "\(subject) is held"
            case .compromised: "\(subject) changed while held"
            case .filesystem(let error): "\(subject): \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .filesystem(let error): [NSUnderlyingErrorKey: error]
            case .unwatched, .held, .compromised: [:]
        }
    }
}
