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
        case absent
        case activating
        case refused(AXError)
        case unsettable
        case hiddenAgent
        case open(any Error)
        case terminated
        case cancelled(CancellationError)
        case noPath
        case noBundleIdentifier
        case file(any Error)
        case unmatched(Unmatched)
        case unwatched(FileFailure)
        case content(any Error)
        case noWindow
        case noDisplay
        case capture(any Error)
        case unencodable
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .untrusted: "\(subject) lacks Accessibility access, grant it in System Settings > Privacy & Security > Accessibility"
            case .unanswered(.none): "Application did not answer within messaging timeout"
            case .unanswered(.some): "\(subject) got no answer within messaging timeout and can have run"
            case .noProcess: Errno.noSuchProcess.description
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error)"
            case .unobservable(let refusals):
                "Application refused Accessibility notifications \(refusals.errors.map { refusal in "\(refusal.notification) (AXError \(refusal.error.rawValue))" }.joined(separator: ", "))"
            case .absent: "Application has no open \(subject)"
            case .activating: "\(subject) not sent. AXRaise brings window forward"
            case .refused(let error): "Application refused \(subject) with AXError \(error.rawValue)"
            case .unsettable: "\(kAXValueAttribute) of \(subject) is not settable"
            case .hiddenAgent: "LSUIElement agent \(subject) cannot unhide, launch it without hiding"
            case .open(let error): "Launch Services could not open \(subject). \(error.localizedDescription)"
            case .terminated: "Process \(subject) exited during launch"
            case .cancelled: "Canceled \(subject)"
            case .noPath: "\(subject) has no kMDItemPath"
            case .noBundleIdentifier: "\(subject) has no CFBundleIdentifier"
            case .file(let error): "\(subject): \(error.localizedDescription)"
            case .unmatched(let miss): miss.errorDescription
            case .unwatched(let failure): failure.errorDescription
            case .content(let error): "ScreenCaptureKit could not list shareable content of \(subject). \(error.localizedDescription)"
            case .noWindow: "Process \(subject) has no on-screen window"
            case .noDisplay: "No display shows windows of process \(subject)"
            case .capture(let error): "ScreenCaptureKit could not capture \(subject). \(error.localizedDescription)"
            case .unencodable: "ImageIO could not encode capture as \(subject)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .noProcess: [NSUnderlyingErrorKey: Errno.noSuchProcess]
            case .open(let error), .file(let error), .content(let error), .capture(let error): [NSUnderlyingErrorKey: error]
            case .unwatched(let failure): [NSUnderlyingErrorKey: failure]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .untrusted, .unanswered, .unreadable, .unobservable, .absent, .activating, .refused, .unsettable, .hiddenAgent, .terminated, .cancelled, .noPath,
                .noBundleIdentifier, .noWindow, .noDisplay, .unencodable:
                [:]
        }
    }
}
