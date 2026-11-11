public import AppleEvents
public import ApplicationServices
public import CasePaths
public import Files
public import Foundation
import Processes
public import Results
public import System

// --- [ERRORS] --------------------------------------------------------------------------

public struct ApplicationFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: Encodable, Equatable, Sendable {
        case untrusted
        case noCaptureAccess
        case unanswered(ElementAction?)
        case noProcess
        case unreadable(FailedRead)
        case unresolved
        case foreign(pid_t)
        case disabled
        case notSettable(String)
        case refused(AXError?)
        case noIdentifier
        case noPath
        case noHandler
        case noWindow
        case noImage
        case noExtension
        case noEncoder
        case noReport(Duration)
        case event(AppleEventFailure)
        case system(Errno)
        case host(NSError)
        case cancelled
        case unmatched(Unmatched)
        case unnamed([URL])
        case unwatched(FileFailure)
        case malformed(String)

        public func encode(to encoder: any Encoder) throws {
            var label = encoder.singleValueContainer()
            try label.encode(caseLabel(of: self))
        }
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
            case .noCaptureAccess: "\(subject) lacks Screen Recording access, grant it in System Settings > Privacy & Security > Screen & System Audio Recording"
            case .unanswered(.none): "\(subject) did not answer within messaging timeout"
            case .unanswered(.some): "\(subject) got no answer within messaging timeout, the action may have run"
            case .noProcess: "Process \(subject) exited"
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \(String(reflecting: label))" } ?? "") failed with AXError \(read.error.rawValue)"
            case .unresolved: "\(subject) no longer exists in its app, read the app again for a current id"
            case .foreign(let process): "\(subject) belongs to a process other than \(process)"
            case .disabled: "Element \(subject) is disabled"
            case .notSettable(let attribute): "\(subject) has no settable \(attribute)"
            case .refused(.some(let error)): "\(subject) refused with AXError \(error.rawValue)"
            case .refused(.none): "\(subject) refused"
            case .noIdentifier: "\(subject) has no \(kCFBundleIdentifierKey as String)"
            case .noPath: "\(subject) has no \(NSMetadataItemPathKey)"
            case .noHandler: "No application opens \(subject)"
            case .noWindow: "\(subject) has no window"
            case .noImage: "\(subject) holds no SDR image"
            case .noExtension: "\(subject) has no filename extension"
            case .noEncoder: "ImageIO has no \(subject) encoder"
            case .noReport(let limit): "Process \(subject) wrote no crash report within \(limit.formatted(.units(width: .wide)))"
            case .event(let failure): failure.errorDescription
            case .system(let error): "\(subject): \(error.description)"
            case .host(let error): "\(subject): \(error.localizedDescription)"
            case .cancelled: "Canceled \(subject)"
            case .unmatched(let miss): miss.errorDescription
            case .unnamed(let candidates) where candidates.isEmpty: "\(subject) names no application"
            case .unnamed: "\(subject) names no single application, pass one candidate"
            case .unwatched(let failure): failure.errorDescription
            case .malformed(let message): message
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
            case .untrusted, .noCaptureAccess, .unanswered, .unreadable, .unresolved, .foreign, .disabled, .notSettable, .refused, .noIdentifier, .noPath, .noHandler, .noWindow, .noImage,
                .noExtension, .noEncoder, .noReport, .cancelled, .unnamed, .malformed:
                [:]
        }
    }
}

extension ApplicationFailure: Encodable {
    private enum Key: CodingKey {
        case reason, subject, message, pid, error, attribute, reads, role, label, candidates, limit, status
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(reason, forKey: .reason)
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
            case .unnamed(let candidates): try fields.encode(candidates, forKey: .candidates)
            case .noReport(let limit): try fields.encode(limit / .seconds(1), forKey: .limit)
            case .event(let failure): try fields.encodeIfPresent(failure.status, forKey: .status)
            case .system(let error): try fields.encode(error.rawValue, forKey: .error)
            case .untrusted, .noCaptureAccess, .unanswered, .noProcess, .unresolved, .disabled, .refused(.none), .noIdentifier, .noPath, .noHandler, .noWindow, .noImage, .noExtension, .noEncoder,
                .host,
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
                case (.apiDisabled, _) where !AXIsProcessTrusted(): Self(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
                case (.cannotComplete, .some(let process)), (.invalidUIElement, .some(let process)): Self(subject: String(process), reason: .noProcess)
                case (.cannotComplete, .none): Self(subject: subject, reason: .unanswered(action))
                case (_, _): otherwise()
            }
    }

    init(_ error: Errno, process: String) {
        self = Self(subject: process, reason: error == .noSuchProcess ? .noProcess : .system(error))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func caseLabel(of value: some Any) -> String {
    Mirror(reflecting: value).children.first?.label ?? String(describing: value)
}
