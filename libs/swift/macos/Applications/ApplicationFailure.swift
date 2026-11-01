public import ApplicationServices
public import CasePaths
public import Files
public import Foundation
public import Results
import System

// --- [ERRORS] --------------------------------------------------------------------------

public struct ApplicationFailure: CustomNSError, LocalizedError {
    @CasePathable
    public enum Reason: Sendable {
        case untrusted
        case unanswered(ElementAction?)
        case noProcess
        case unreadable(FailedRead)
        case unobservable(AggregateError<(notification: String, error: AXError)>)
        case absent(String)
        case activating
        case refused(AXError)
        case hiddenAgent
        case host(any Error)
        case cancelled(CancellationError)
        case unmatched(Unmatched)
        case unwatched(FileFailure)
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .untrusted: "\(subject) lacks Accessibility access, grant it in System Settings > Privacy & Security > Accessibility"
            case .unanswered(.none): "Application did not answer within messaging timeout"
            case .unanswered(.some): "\(subject) got no answer within messaging timeout and can have run"
            case .noProcess: "Process \(subject) exited"
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error)"
            case .unobservable(let refusals):
                "Application refused Accessibility notifications \(refusals.errors.map { refusal in "\(refusal.notification) (AXError \(refusal.error.rawValue))" }.joined(separator: ", "))"
            case .absent(let missing): "\(subject) has no \(missing)"
            case .activating: "\(subject) not sent. AXRaise brings window forward"
            case .refused(let error): "Application refused \(subject) with AXError \(error.rawValue)"
            case .hiddenAgent: "LSUIElement agent \(subject) cannot unhide, launch it without hiding"
            case .host(let error): "\(subject): \(error.localizedDescription)"
            case .cancelled: "Canceled \(subject)"
            case .unmatched(let miss): miss.errorDescription
            case .unwatched(let failure): failure.errorDescription
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .noProcess: [NSUnderlyingErrorKey: Errno.noSuchProcess]
            case .host(let error): [NSUnderlyingErrorKey: error]
            case .unwatched(let failure): [NSUnderlyingErrorKey: failure]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .untrusted, .unanswered, .unreadable, .unobservable, .absent, .activating, .refused, .hiddenAgent, .cancelled: [:]
        }
    }
}
