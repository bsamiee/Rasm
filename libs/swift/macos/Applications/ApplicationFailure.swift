package import AppleEvents
package import ApplicationServices
package import CasePaths
package import Encoding
package import Foundation
import Processes
package import Results
package import System

// --- [ERRORS] --------------------------------------------------------------------------

package struct ApplicationFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    package enum Reason: CaseLabeled, Equatable, Sendable {
        case noGrant(Grant)
        case notResponsible(Grant, pid_t)
        case reused(pid_t)
        case quarantined(Installation.Quarantine, NSError)
        case unanswered(Step?)
        case stale(reached: Int, candidates: [String])
        case disabled
        case unchanged(Fact.Scalar?)
        case unseen
        case refused
        case symbolicHotKey
        case notFrontmost(pid_t?)
        case notKeyFocus(pid_t?)
        case covered(window: CGWindowID, owner: pid_t)
        case unshared
        case notText(Fact.Scalar)
        case pastDeadline
        case permittedFrontHeld
        case noIdentifier
        case noPath
        case noExecutable
        case noWindow
        case noImage
        case noEncoder
        case notOnConsole
        case screenLocked
        case displayAsleep
        case requiresApproval
        case accessibility(AXError)
        case windowServer(CGError)
        case event(AppleEventFailure)
        case system(Errno)
        case foundation(NSError)
        case cancelled
        case unmatched(Unmatched)
    }

    let subject: String
    package let reason: Reason

    package init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    package var errorDescription: String? {
        switch reason {
            case .noGrant(let grant): "\(subject) lacks the \(grant.rawValue) grant, allow it in System Settings > Privacy & Security at \(grant.settings)"
            case .notResponsible(let grant, let process):
                "\(subject) lacks the \(grant.rawValue) grant as process \(process) is responsible for it and holds none, start \(subject) responsible for itself"
            case .reused(let process): "Arguments and environment reach a new process alone and \(subject) runs as process \(process), close it first"
            case .quarantined(let quarantine, let error):
                "\(subject) did not launch (\(error.localizedDescription)) and holds the quarantine attribute Gatekeeper checks before a first launch: \(quarantine)"
            case .unanswered(.none): "\(subject) did not answer within messaging timeout"
            case .unanswered(.some): "\(subject) got no answer within messaging timeout, the action may have run"
            case .stale(let reached, let candidates) where candidates.isEmpty: "Reference \(subject) matches nothing open after \(reached) steps of its locator, read the application again"
            case .stale(let reached, let candidates): "Reference \(subject) matches nothing open after \(reached) steps of its locator, where \(candidates.formatted()) are, read the application again"
            case .disabled: "Element \(subject) is disabled"
            case .unchanged(.some(let value)): "\(subject) read back \(value) in place of the written value"
            case .unchanged(.none): "\(subject) read back a value other than the written one"
            case .unseen: "\(subject) changed nothing the application shows within its answer time"
            case .refused: "\(subject) refused"
            case .symbolicHotKey: "\(subject) refused, the system takes it as a symbolic hot key of System Settings > Keyboard > Keyboard Shortcuts"
            case .notFrontmost(let front): "\(subject) refused, the system route posts pointer events to the frontmost process, which is \(front.map(String.init) ?? "none")"
            case .notKeyFocus(let keyFocus): "\(subject) refused, the system route posts keys to the key-focus process, which is \(keyFocus.map(String.init) ?? "none")"
            case .covered(let window, let owner): "\(subject) refused, window \(window) of process \(owner) lies over the point"
            case .unshared: "\(subject) refused, its \(kCGWindowSharingState as String) is none"
            case .notText(let value): "\(subject) takes text, not \(value)"
            case .pastDeadline: "\(subject) runs past the reply deadline of \(replyDeadline.formatted(.units(allowed: [.seconds])))"
            case .permittedFrontHeld:
                "\(subject) refused, a system prompt such as the lock screen or an authorization dialog holds the window server's permitted front list, finish that prompt and retry"
            case .noIdentifier: "\(subject) has no \(kCFBundleIdentifierKey as String)"
            case .noPath: "\(subject) has no file path"
            case .noExecutable: "\(subject) names no executable"
            case .noWindow: "\(subject) has no window"
            case .noImage: "\(subject) yielded no image"
            case .noEncoder: "No encoder writes \(subject)"
            case .notOnConsole: "\(subject) refused, the session is not on the console"
            case .screenLocked: "\(subject) refused, the screen is locked"
            case .displayAsleep: "\(subject) refused, the display is asleep"
            case .requiresApproval: "\(subject) requires approval in System Settings > General > Login Items"
            case .accessibility(let error): "\(subject) failed with AXError \(error.rawValue)"
            case .windowServer(let error): "\(subject) failed with CGError \(error.rawValue)"
            case .event(let failure): failure.errorDescription
            case .system(let error): "\(subject): \(error.description)"
            case .foundation(let error): "\(subject): \(error.localizedDescription)"
            case .cancelled: "Canceled \(subject)"
            case .unmatched(let miss): miss.errorDescription
        }
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .event(let failure): [NSUnderlyingErrorKey: failure]
            case .system(let error): [NSUnderlyingErrorKey: error]
            case .foundation(let error), .quarantined(_, let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .noGrant, .notResponsible, .reused, .unanswered, .stale, .disabled, .unchanged, .unseen, .refused, .symbolicHotKey, .notFrontmost, .notKeyFocus, .covered,
                .unshared, .notText, .pastDeadline, .permittedFrontHeld, .noIdentifier, .noPath, .noExecutable, .noWindow, .noImage, .noEncoder, .notOnConsole, .screenLocked,
                .displayAsleep, .requiresApproval, .accessibility, .windowServer, .cancelled:
                [:]
        }
    }
}

extension ApplicationFailure: Encodable {
    private enum Key: CodingKey {
        case reason, subject, message, grant, settings, pid, window, quarantine, reached, candidates, value, error, status
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(reason, forKey: .reason)
        try fields.encode(subject, forKey: .subject)
        try fields.encodeIfPresent(errorDescription, forKey: .message)
        switch reason {
            case .noGrant(let grant):
                try fields.encode(grant, forKey: .grant)
                try fields.encode(grant.settings, forKey: .settings)
            case .notResponsible(let grant, let process):
                try fields.encode(grant, forKey: .grant)
                try fields.encode(process, forKey: .pid)
            case .reused(let process): try fields.encode(process, forKey: .pid)
            case .quarantined(let quarantine, let error):
                try fields.encode(quarantine, forKey: .quarantine)
                try fields.encode(error.code, forKey: .error)
            case .stale(let reached, let candidates):
                try fields.encode(reached, forKey: .reached)
                try fields.encode(candidates, forKey: .candidates)
            case .unchanged(let value): try fields.encodeIfPresent(value, forKey: .value)
            case .notText(let value): try fields.encode(value, forKey: .value)
            case .notFrontmost(let process), .notKeyFocus(let process): try fields.encodeIfPresent(process, forKey: .pid)
            case .covered(let window, let owner):
                try fields.encode(window, forKey: .window)
                try fields.encode(owner, forKey: .pid)
            case .accessibility(let error): try fields.encode(error.rawValue, forKey: .error)
            case .windowServer(let error): try fields.encode(error.rawValue, forKey: .error)
            case .event(let failure): try fields.encodeIfPresent(failure.status, forKey: .status)
            case .system(let error): try fields.encode(error.rawValue, forKey: .error)
            case .unmatched(let miss): try fields.encode(miss.candidates, forKey: .candidates)
            case .unanswered, .unseen, .disabled, .refused, .symbolicHotKey, .unshared, .pastDeadline, .permittedFrontHeld, .noIdentifier, .noPath, .noExecutable, .noWindow, .noImage,
                .noEncoder, .notOnConsole, .screenLocked, .displayAsleep, .requiresApproval, .foundation, .cancelled:
                break
        }
    }
}

extension ApplicationFailure {
    init(_ error: AXError, subject: String, process: pid_t?, answering action: Step? = nil, otherwise: () -> Self) {
        let exited: pid_t? = process.flatMap { process in
            [.cannotComplete, .invalidUIElement].contains(error) && Result(catching: { () throws(Errno) in try ProcessInstance(of: process) }) == .failure(.noSuchProcess) ? process : nil
        }
        self =
            switch (error, exited) {
                case (.apiDisabled, _): Grant.accessibility.missing ?? otherwise()
                case (.cannotComplete, .some(let process)), (.invalidUIElement, .some(let process)): Self(subject: String(process), reason: .system(.noSuchProcess))
                case (.cannotComplete, .none): Self(subject: subject, reason: .unanswered(action))
                case (.invalidUIElement, .none): Self(subject: subject, reason: .stale(reached: 0, candidates: []))
                case (_, _): otherwise()
            }
    }
}
