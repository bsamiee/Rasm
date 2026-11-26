public import AppleEvents
public import ApplicationServices
public import CasePaths
public import Encoding
public import Foundation
import Processes
public import Results
public import System

// --- [ERRORS] --------------------------------------------------------------------------

public struct ApplicationFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: CaseLabeled, Equatable, Sendable {
        case noGrant(Grant)
        case notResponsible(Grant, pid_t)
        case unanswered(ElementAction?)
        case noProcess
        case reused(pid_t)
        case quarantined(Installation.Quarantine, NSError)
        case unreadable(attribute: String, error: AXError)
        case stale(reached: Int, candidates: [String])
        case disabled
        case unchanged(Fact.Scalar?)
        case refused(AXError?)
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
        case event(AppleEventFailure)
        case system(Errno)
        case foundation(NSError)
        case cancelled
        case unmatched(Unmatched)
    }

    public let subject: String
    public let reason: Reason

    public init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    public var errorDescription: String? {
        switch reason {
            case .noGrant(let grant): "\(subject) lacks the \(grant.rawValue) grant, allow it in System Settings > Privacy & Security"
            case .notResponsible(let grant, let process):
                "\(subject) lacks the \(grant.rawValue) grant as process \(process) is responsible for it and holds none, start \(subject) responsible for itself"
            case .unanswered(.none): "\(subject) did not answer within messaging timeout"
            case .unanswered(.some): "\(subject) got no answer within messaging timeout, the action may have run"
            case .noProcess: "Process \(subject) exited"
            case .reused(let process): "Arguments and environment reach a new process alone and \(subject) runs as process \(process), close it first"
            case .quarantined(let quarantine, let error):
                "\(subject) did not launch (\(error.localizedDescription)) and holds the quarantine attribute Gatekeeper checks before a first launch: \(quarantine)"
            case .unreadable(let attribute, let error): "\(attribute) on \(subject) failed with AXError \(error.rawValue)"
            case .stale(let reached, let candidates) where candidates.isEmpty: "Reference \(subject) matches nothing open after \(reached) steps of its locator, read the application again"
            case .stale(let reached, let candidates): "Reference \(subject) matches nothing open after \(reached) steps of its locator, where \(candidates.formatted()) are, read the application again"
            case .disabled: "Element \(subject) is disabled"
            case .unchanged(let value): "\(subject) left \(kAXValueAttribute) at \(value.map { value in "\(value)" } ?? "unset")"
            case .refused(.some(let error)): "\(subject) refused with AXError \(error.rawValue)"
            case .refused(.none): "\(subject) refused"
            case .noIdentifier: "\(subject) has no \(kCFBundleIdentifierKey as String)"
            case .noPath: "\(subject) has no file path"
            case .noExecutable: "\(subject) names no executable"
            case .noWindow: "\(subject) has no window"
            case .noImage: "\(subject) holds no SDR image"
            case .noEncoder: "No encoder writes \(subject)"
            case .notOnConsole: "\(subject) refused, the session is not on the console"
            case .screenLocked: "\(subject) refused, the screen is locked"
            case .displayAsleep: "\(subject) refused, the display is asleep"
            case .requiresApproval: "\(subject) requires approval in System Settings > General > Login Items"
            case .event(let failure): failure.errorDescription
            case .system(let error): "\(subject): \(error.description)"
            case .foundation(let error): "\(subject): \(error.localizedDescription)"
            case .cancelled: "Canceled \(subject)"
            case .unmatched(let miss): miss.errorDescription
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .noProcess: [NSUnderlyingErrorKey: Errno.noSuchProcess]
            case .system(let error): [NSUnderlyingErrorKey: error]
            case .event(let failure): [NSUnderlyingErrorKey: failure]
            case .foundation(let error), .quarantined(_, let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .noGrant, .notResponsible, .unanswered, .reused, .unreadable, .stale, .disabled, .unchanged, .refused, .noIdentifier, .noPath, .noExecutable,
                .noWindow, .noImage, .noEncoder, .notOnConsole, .screenLocked, .displayAsleep, .requiresApproval, .cancelled:
                [:]
        }
    }
}

extension ApplicationFailure: Encodable {
    private enum Key: CodingKey {
        case reason, subject, message, grant, pid, error, value, candidates, status, attribute, reached, quarantine
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(reason, forKey: .reason)
        try fields.encode(subject, forKey: .subject)
        try fields.encodeIfPresent(errorDescription, forKey: .message)
        switch reason {
            case .noGrant(let grant): try fields.encode(grant, forKey: .grant)
            case .notResponsible(let grant, let process):
                try fields.encode(grant, forKey: .grant)
                try fields.encode(process, forKey: .pid)
            case .reused(let process): try fields.encode(process, forKey: .pid)
            case .quarantined(let quarantine, let error):
                try fields.encode(quarantine, forKey: .quarantine)
                try fields.encode(error.code, forKey: .error)
            case .refused(.some(let error)): try fields.encode(error.rawValue, forKey: .error)
            case .unchanged(let value): try fields.encodeIfPresent(value, forKey: .value)
            case .unreadable(let attribute, let error):
                try fields.encode(attribute, forKey: .attribute)
                try fields.encode(error.rawValue, forKey: .error)
            case .stale(let reached, let candidates):
                try fields.encode(reached, forKey: .reached)
                try fields.encode(candidates, forKey: .candidates)
            case .unmatched(let miss): try fields.encode(miss.candidates, forKey: .candidates)
            case .event(let failure): try fields.encodeIfPresent(failure.status, forKey: .status)
            case .system(let error): try fields.encode(error.rawValue, forKey: .error)
            case .unanswered, .noProcess, .disabled, .refused(.none), .noIdentifier, .noPath, .noExecutable, .noWindow, .noImage,
                .noEncoder, .notOnConsole, .screenLocked, .displayAsleep, .requiresApproval, .foundation, .cancelled:
                break
        }
    }
}

extension ApplicationFailure {
    init(_ error: AXError, subject: String, process: pid_t?, answering action: ElementAction? = nil, otherwise: () -> Self) {
        let exited = process.flatMap { process in Result(catching: { () throws(Errno) in try ProcessInstance(of: process) }).is(\.failure) ? process : nil }
        self =
            switch (error, exited) {
                case (.apiDisabled, _) where !Grant.accessibility.isHeld: Grant.accessibility.missing
                case (.cannotComplete, .some(let process)), (.invalidUIElement, .some(let process)): Self(subject: String(process), reason: .noProcess)
                case (.cannotComplete, .none): Self(subject: subject, reason: .unanswered(action))
                case (_, _): otherwise()
            }
    }

    init(_ error: Errno, process: String) {
        self = Self(subject: process, reason: error == .noSuchProcess ? .noProcess : .system(error))
    }
}
