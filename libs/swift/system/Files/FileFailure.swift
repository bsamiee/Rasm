package import CasePaths
import Foundation
package import Results

// --- [ERRORS] --------------------------------------------------------------------------

package struct FileFailure: ModuleError {
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

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .filesystem(let error): [NSUnderlyingErrorKey: error]
            case .unwatched, .held, .compromised: [:]
        }
    }
}
