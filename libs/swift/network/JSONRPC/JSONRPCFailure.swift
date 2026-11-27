package import CasePaths
import Foundation
package import Results

// --- [ERRORS] --------------------------------------------------------------------------

package struct JSONRPCFailure: ModuleError {
    @CasePathable
    package enum Reason: Sendable {
        case transport(any Error)
        case closed
        case unreadable(DecodingError)
        case unencodable(EncodingError)
        case refused(JSONRPCError)
        case timedOut(Duration)
        case cancelled
    }

    package let subject: String
    package let reason: Reason

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .transport(let error), .unreadable(let error as any Error), .unencodable(let error as any Error), .refused(let error as any Error): [NSUnderlyingErrorKey: error]
            case .closed, .timedOut, .cancelled: [:]
        }
    }
}

extension JSONRPCFailure {
    init(subject: String, cause: any Error) {
        self.subject = subject
        reason =
            switch cause {
                case let error as DecodingError: .unreadable(error)
                case let error as EncodingError: .unencodable(error)
                case is CancellationError: .cancelled
                default: .transport(cause)
            }
    }
}
