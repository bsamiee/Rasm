package import CasePaths
package import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

package struct FileFailure: CustomNSError, LocalizedError {
    @CasePathable
    package enum Reason: Sendable {
        case unwatched
        case held
        case compromised
        case filesystem(any Error)
    }

    package let subject: String
    package let reason: Reason

    init(file: URL, reason: Reason) {
        subject = file.path(percentEncoded: false)
        self.reason = reason
    }

    package var errorDescription: String? {
        switch reason {
            case .unwatched: "FSEvents stream on \(subject) failed to start"
            case .held: "\(subject) is held"
            case .compromised: "\(subject) changed while held"
            case .filesystem(let error): "\(subject): \(error.localizedDescription)"
        }
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .filesystem(let error): [NSUnderlyingErrorKey: error]
            case .unwatched, .held, .compromised: [:]
        }
    }
}
