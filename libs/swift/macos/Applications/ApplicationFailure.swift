public import AppleEvents
public import ApplicationServices
public import CasePaths
public import Encoding
public import Files
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
        case unreadable(FailedRead)
        case unresolved
        case foreign(pid_t)
        case disabled
        case unchanged(JSONValue?)
        case refused(AXError?)
        case noIdentifier
        case noPath
        case noHandler
        case noWindow
        case noImage
        case noExtension
        case noEncoder
        case event(AppleEventFailure)
        case system(Errno)
        case foundation(NSError)
        case cancelled
        case unmatched(Unmatched)
        case unwatched(FileFailure)
        case malformed(String)
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
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \(String(reflecting: label))" } ?? "") failed with AXError \(read.error.rawValue)"
            case .unresolved: "\(subject) no longer exists in its app, read the app again for a current id"
            case .foreign(let process): "\(subject) belongs to a process other than \(process)"
            case .disabled: "Element \(subject) is disabled"
            case .unchanged(let value): "\(subject) left \(kAXValueAttribute) at \(value.map { value in "\(value)" } ?? "unset")"
            case .refused(.some(let error)): "\(subject) refused with AXError \(error.rawValue)"
            case .refused(.none): "\(subject) refused"
            case .noIdentifier: "\(subject) has no \(kCFBundleIdentifierKey as String)"
            case .noPath: "\(subject) has no \(NSMetadataItemPathKey)"
            case .noHandler: "No application opens \(subject)"
            case .noWindow: "\(subject) has no window"
            case .noImage: "\(subject) holds no SDR image"
            case .noExtension: "\(subject) has no filename extension"
            case .noEncoder: "No encoder writes \(subject)"
            case .event(let failure): failure.errorDescription
            case .system(let error): "\(subject): \(error.description)"
            case .foundation(let error): "\(subject): \(error.localizedDescription)"
            case .cancelled: "Canceled \(subject)"
            case .unmatched(let miss): miss.errorDescription
            case .unwatched(let failure): failure.errorDescription
            case .malformed(let message): message
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .noProcess: [NSUnderlyingErrorKey: Errno.noSuchProcess]
            case .system(let error): [NSUnderlyingErrorKey: error]
            case .event(let failure): [NSUnderlyingErrorKey: failure]
            case .foundation(let error): [NSUnderlyingErrorKey: error]
            case .unwatched(let failure): [NSUnderlyingErrorKey: failure]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .noGrant, .notResponsible, .unanswered, .reused, .unreadable, .unresolved, .foreign, .disabled, .unchanged, .refused, .noIdentifier, .noPath, .noHandler, .noWindow,
                .noImage, .noExtension, .noEncoder, .cancelled, .malformed:
                [:]
        }
    }
}

extension ApplicationFailure: Encodable {
    private enum Key: CodingKey {
        case reason, subject, message, grant, pid, error, value, candidates, status
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
            case .foreign(let process), .reused(let process): try fields.encode(process, forKey: .pid)
            case .refused(.some(let error)): try fields.encode(error.rawValue, forKey: .error)
            case .unchanged(let value): try fields.encodeIfPresent(value, forKey: .value)
            case .unreadable(let read): try read.encode(to: encoder)
            case .unmatched(let miss): try fields.encode(miss.candidates, forKey: .candidates)
            case .event(let failure): try fields.encodeIfPresent(failure.status, forKey: .status)
            case .system(let error): try fields.encode(error.rawValue, forKey: .error)
            case .unanswered, .noProcess, .unresolved, .disabled, .refused(.none), .noIdentifier, .noPath, .noHandler, .noWindow, .noImage, .noExtension, .noEncoder,
                .foundation,
                .cancelled, .unwatched, .malformed:
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
