public import CasePaths
public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public struct JSONRPCFailure: CustomNSError, LocalizedError {
    @CasePathable
    public enum Reason: Sendable {
        case transport(any Error)
        case closed
        case unreadable(DecodingError)
        case unencodable(EncodingError)
        case refused(JSONRPCError)
        case timedOut(Duration)
        case cancelled
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .transport(let error): error.localizedDescription
            case .closed: "Connection closed"
            case .unreadable: "Unreadable \(subject) message"
            case .unencodable: "Unencodable \(subject) message"
            case .refused(let error): "\(subject) error \(error.code) \(error.message)"
            case .timedOut(let timeout): "\(subject) timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "\(subject) canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
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
