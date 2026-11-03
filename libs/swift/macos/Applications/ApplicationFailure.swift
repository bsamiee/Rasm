public import AppleEvents
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
        case malformed(String)
        case unanswered(ElementAction?)
        case noProcess
        case visibility(hidden: Bool, limit: Duration)
        case unreadable(FailedRead)
        case unresolved
        case foreign(pid_t)
        case activating
        case disabled
        case notSettable(String)
        case refused(AXError?)
        case noIdentifier
        case noPath
        case noHandler
        case noWindow
        case noDisplay
        case noImage
        case noExtension
        case noEncoder
        case noReport(Duration)
        case event(AppleEventFailure)
        case system(Errno)
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

    public var name: String { Mirror(reflecting: reason).children.first?.label ?? String(describing: reason) }

    public var errorDescription: String? {
        switch reason {
            case .untrusted: "\(subject) lacks Accessibility access, grant it in System Settings > Privacy & Security > Accessibility"
            case .malformed(let usage): usage
            case .unanswered(.none): "\(subject) did not answer within messaging timeout"
            case .unanswered(.some): "\(subject) got no answer within messaging timeout, action can have run"
            case .noProcess: "Process \(subject) exited"
            case .visibility(let hidden, let limit): "Process \(subject) stays \(hidden ? "hidden" : "shown") \(limit.formatted(.units(width: .wide))) after \(hidden ? "unhide" : "hide")"
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error.rawValue)"
            case .unresolved: "\(subject) no longer exists in its app, read the app again for a current id"
            case .foreign(let process): "\(subject) belongs to a process other than \(process)"
            case .activating: "\(subject) not sent, raising a window takes focus from the frontmost app"
            case .disabled: "\(subject) not sent to disabled element"
            case .notSettable(let attribute): "\(subject) has no settable \(attribute)"
            case .refused(.some(let error)): "\(subject) refused with AXError \(error.rawValue)"
            case .refused(.none): "\(subject) refused"
            case .noIdentifier: "\(subject) has no \(kCFBundleIdentifierKey as String)"
            case .noPath: "\(subject) has no \(NSMetadataItemPathKey)"
            case .noHandler: "No application opens \(subject)"
            case .noWindow: "\(subject) has no on-screen window"
            case .noDisplay: "No display shows \(subject)"
            case .noImage: "\(subject) holds no SDR image"
            case .noExtension: "\(subject) has no filename extension"
            case .noEncoder: "ImageIO has no \(subject) encoder"
            case .noReport(let limit): "Process \(subject) wrote no crash report within \(limit.formatted(.units(width: .wide)))"
            case .event(let failure): failure.errorDescription
            case .system(let error): "\(subject): \(error.description)"
            case .host(let error): "\(subject): \(error.localizedDescription)"
            case .cancelled: "Canceled \(subject)"
            case .unmatched(let miss): miss.errorDescription
            case .unwatched(let failure): failure.errorDescription
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .noProcess: [NSUnderlyingErrorKey: Errno.noSuchProcess]
            case .system(let error): [NSUnderlyingErrorKey: error]
            case .event(let failure): [NSUnderlyingErrorKey: failure]
            case .host(let error): [NSUnderlyingErrorKey: error]
            case .unwatched(let failure): [NSUnderlyingErrorKey: failure]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .untrusted, .malformed, .unanswered, .visibility, .unreadable, .unresolved, .foreign, .activating, .disabled, .notSettable, .refused, .noIdentifier, .noPath,
                .noHandler, .noWindow, .noDisplay, .noImage, .noExtension, .noEncoder, .noReport, .cancelled:
                [:]
        }
    }
}

extension ApplicationFailure: Equatable {
    public static func == (left: Self, right: Self) -> Bool {
        left.name == right.name && left.subject == right.subject && left.errorDescription == right.errorDescription
    }
}

extension ApplicationFailure: Encodable {
    private enum Key: CodingKey {
        case reason, subject, message, pid, error, attribute, reads, role, label, candidates, limit, status
    }

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encode(name, forKey: .reason)
        try fields.encode(subject, forKey: .subject)
        try fields.encodeIfPresent(errorDescription, forKey: .message)
        switch reason {
            case .foreign(let process): try fields.encode(process, forKey: .pid)
            case .refused(.some(let error)): try fields.encode(error.rawValue, forKey: .error)
            case .notSettable(let attribute): try fields.encode(attribute, forKey: .attribute)
            case .unreadable(let read):
                try fields.encode(read.reads, forKey: .reads)
                try fields.encodeIfPresent(read.role, forKey: .role)
                try fields.encodeIfPresent(read.label, forKey: .label)
                try fields.encode(read.error.rawValue, forKey: .error)
            case .unmatched(let miss): try fields.encode(miss.candidates, forKey: .candidates)
            case .visibility(_, let limit), .noReport(let limit): try fields.encode(limit / .seconds(1), forKey: .limit)
            case .event(let failure): try fields.encodeIfPresent(failure.status, forKey: .status)
            case .system(let error): try fields.encode(error.rawValue, forKey: .error)
            case .untrusted, .malformed, .unanswered, .noProcess, .unresolved, .activating, .disabled, .refused(.none), .noIdentifier, .noPath, .noHandler, .noWindow,
                .noDisplay, .noImage, .noExtension, .noEncoder, .host, .cancelled, .unwatched:
                break
        }
    }
}

extension ApplicationFailure {
    init(_ error: AXError, subject: String, process: pid_t?, answering action: ElementAction? = nil, otherwise: () -> Self) {
        let exited: pid_t? = process.flatMap { process in Result { () throws(Errno) in try ProcessStatus(of: process) } == .failure(.noSuchProcess) ? process : nil }
        self =
            switch (error, exited) {
                case (.apiDisabled, _) where !AXIsProcessTrusted(): Self(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
                case (.cannotComplete, .some(let process)), (.invalidUIElement, .some(let process)): Self(subject: String(process), reason: .noProcess)
                case (.cannotComplete, .none): Self(subject: subject, reason: .unanswered(action))
                case (_, _): otherwise()
            }
    }

    init(_ error: Errno, process: pid_t) {
        self = Self(subject: String(process), reason: error == .noSuchProcess ? .noProcess : .system(error))
    }
}
