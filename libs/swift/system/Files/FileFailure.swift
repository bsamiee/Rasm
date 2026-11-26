public import CasePaths
public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public struct FileFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: Equatable, Sendable {
        case unwatched
        case held
        case compromised
        case filesystem(NSError)
    }

    public let subject: String
    public let reason: Reason

    init(file: URL, reason: Reason) {
        self.init(files: [file], reason: reason)
    }

    init(files: [URL], reason: Reason) {
        subject = files.map { file in file.path(percentEncoded: false) }.formatted()
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
