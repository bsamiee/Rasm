public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public struct JSONRPCFailure: CustomNSError, LocalizedError {
    public enum Reason: Sendable {
        case transport(any Error)
        case closed
        case unreadable(any Error)
        case unencodable(any Error)
        case refused(JSONRPCError)
        case timedOut(Duration)
        case cancelled(CancellationError)
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
            case .cancelled: "\(subject) was canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .transport(let error), .unreadable(let error), .unencodable(let error), .refused(let error as any Error): [NSUnderlyingErrorKey: error]
            case .closed, .timedOut, .cancelled: [:]
        }
    }
}

extension JSONRPCFailure {
    init(subject: String, transport error: any Error) { self.init(subject: subject, reason: (error as? CancellationError).map(Reason.cancelled) ?? .transport(error)) }
}
