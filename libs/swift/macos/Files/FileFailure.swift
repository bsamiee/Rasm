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

extension FileFailure: Equatable {
    public static func == (left: Self, right: Self) -> Bool {
        let same =
            switch left.reason {
                case .unwatched: right.reason.is(\.unwatched)
                case .held: right.reason.is(\.held)
                case .compromised: right.reason.is(\.compromised)
                case .filesystem(let error):
                    right.reason[case: \.filesystem].map { other in ((other as NSError).domain, (other as NSError).code) == ((error as NSError).domain, (error as NSError).code) } == true
            }
        return left.subject == right.subject && same
    }
}
