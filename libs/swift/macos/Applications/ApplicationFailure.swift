public import ApplicationServices
public import CasePaths
public import Files
public import Foundation
import Processes
public import Results
public import System

// --- [ERRORS] --------------------------------------------------------------------------

public struct ApplicationFailure: CustomNSError, LocalizedError {
    @CasePathable
    public enum Reason: Sendable {
        case untrusted
        case unanswered(ElementAction?)
        case noProcess
        case unreadable(FailedRead)
        case absent(String)
        case activating
        case refused(AXError)
        case hiddenAgent
        case process(Errno)
        case host(any Error)
        case cancelled(CancellationError)
        case unmatched(Unmatched)
        case unwatched(FileFailure)
    }

    public let subject: String
    public let reason: Reason

    public init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    public var errorDescription: String? {
        switch reason {
            case .untrusted: "\(subject) lacks Accessibility access, grant it in System Settings > Privacy & Security > Accessibility"
            case .unanswered(.none): "\(subject) did not answer within messaging timeout"
            case .unanswered(.some): "\(subject) got no answer within messaging timeout and can have run"
            case .noProcess: "Process \(subject) exited"
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error)"
            case .absent(let missing): "\(subject) has no \(missing)"
            case .activating: "\(subject) not sent. AXRaise brings window forward"
            case .refused(let error): "Application refused \(subject) with AXError \(error.rawValue)"
            case .hiddenAgent: "LSUIElement agent \(subject) cannot unhide, launch it without hiding"
            case .process(let error): "Process \(subject): \(error.description)"
            case .host(let error): "\(subject): \(error.localizedDescription)"
            case .cancelled: "Canceled \(subject)"
            case .unmatched(let miss): miss.errorDescription
            case .unwatched(let failure): failure.errorDescription
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .noProcess: [NSUnderlyingErrorKey: Errno.noSuchProcess]
            case .process(let error): [NSUnderlyingErrorKey: error]
            case .host(let error): [NSUnderlyingErrorKey: error]
            case .unwatched(let failure): [NSUnderlyingErrorKey: failure]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .untrusted, .unanswered, .unreadable, .absent, .activating, .refused, .hiddenAgent, .cancelled: [:]
        }
    }
}

extension ApplicationFailure {
    init(_ error: AXError, subject: String, process: pid_t?, answering action: ElementAction? = nil, otherwise: @autoclosure () -> Self) {
        self =
            switch (error, process.map { process in Result { () throws(Errno) in try ProcessStatus(of: process) } }) {
                case (.apiDisabled, _): Self(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
                case (.cannotComplete, .some(.failure(.noSuchProcess))), (.invalidUIElement, .some(.failure(.noSuchProcess))):
                    Self(subject: process.map(String.init) ?? subject, reason: .noProcess)
                case (.cannotComplete, _): Self(subject: subject, reason: .unanswered(action))
                case (_, _): otherwise()
            }
    }

    init(_ error: Errno, process: pid_t) {
        self = error == .noSuchProcess ? Self(subject: String(process), reason: .noProcess) : Self(subject: String(process), reason: .process(error))
    }
}
