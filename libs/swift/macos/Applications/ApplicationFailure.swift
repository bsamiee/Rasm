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
        case hidden(Duration)
        case unreadable(FailedRead)
        case absent(String)
        case unresolved
        case foreign(pid_t)
        case closed
        case activating
        case disabled
        case refused(AXError)
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
            case .unanswered(.some): "\(subject) got no answer within messaging timeout, action can have run"
            case .noProcess: "Process \(subject) exited"
            case .hidden(let limit): "Process \(subject) stays hidden \(limit.formatted(.units(width: .wide))) after unhide"
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error.rawValue)"
            case .absent(let missing): "\(subject) has no \(missing)"
            case .unresolved: "\(subject) no longer exists in its app, read the app again for a current id"
            case .foreign(let process): "\(subject) belongs to a process other than \(process)"
            case .closed: "\(subject) sits in a closed window, read the app again for a current id"
            case .activating: "\(subject) not sent, raising a window takes focus from the frontmost app"
            case .disabled: "\(subject) not sent to disabled element"
            case .refused(let error): "\(subject) refused with AXError \(error.rawValue)"
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
            case .untrusted, .unanswered, .hidden, .unreadable, .absent, .unresolved, .foreign, .closed, .activating, .disabled, .refused, .cancelled: [:]
        }
    }
}

extension ApplicationFailure {
    init(_ error: AXError, subject: String, process: pid_t?, answering action: ElementAction? = nil, otherwise: () -> Self) {
        let exited: pid_t? = process.flatMap { process in Result { () throws(Errno) in try ProcessStatus(of: process) } == .failure(.noSuchProcess) ? process : nil }
        self =
            switch (error, exited) {
                case (.apiDisabled, _): Self(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
                case (.cannotComplete, .some(let process)), (.invalidUIElement, .some(let process)): Self(subject: String(process), reason: .noProcess)
                case (.cannotComplete, .none): Self(subject: subject, reason: .unanswered(action))
                case (_, _): otherwise()
            }
    }

    init(_ error: Errno, process: pid_t) {
        self = Self(subject: String(process), reason: error == .noSuchProcess ? .noProcess : .process(error))
    }
}
