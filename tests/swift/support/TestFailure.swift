public import Foundation
public import Subprocess

// --- [ERRORS] --------------------------------------------------------------------------

public struct TestFailure: LocalizedError, CustomNSError {
    public enum Reason: Sendable {
        case missing
        case refused(OSStatus)
        case exited(TerminationStatus, standardError: String)
        case undecodable(DecodingError)
        case host(any Error)
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? { "\(reason): \(subject)" }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .undecodable(let cause): [NSUnderlyingErrorKey: cause]
            case .host(let cause): [NSUnderlyingErrorKey: cause]
            case .missing, .refused, .exited: [:]
        }
    }
}
