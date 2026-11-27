import AppKit
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
        case reused
        case unclaimed
        case quarantined(Installation.Quarantine, NSError)
        case unanswered(Step?)
        case stale(candidates: [String])
        case disabled
        case secure
        case unchanged(Fact.Scalar?)
        case unseen
        case refused
        case undelivered(routes: [Rung: Self], focus: String?)
        case symbolicHotKey
        case notFrontmost(pid_t?)
        case notKeyFocus(pid_t?)
        case covered(by: pid_t)
        case unshared
        case notText(Fact.Scalar)
        case pastDeadline
        case permittedFrontHeld
        case noIdentifier
        case noPath
        case noExecutable
        case noWindow
        case absent
        case noBackingStore
        case spansDisplays
        case noImage
        case noEncoder(extensions: [String])
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
        let named = { (process: pid_t?) -> String? in process.flatMap(NSRunningApplication.init(processIdentifier:))?.localizedName }
        let meaning = { (error: CGError) -> String in
            switch error {
                case .failure: "the window server hit a system error"
                case .illegalArgument: "the window server refused an argument as illegal"
                case .invalidConnection: "the window server connection is no longer valid"
                case .invalidContext: "this server holds no window server connection yet"
                case .cannotComplete: "the window server could not complete it"
                case .notImplemented: "the window server does not implement it"
                case .rangeCheck: "a value lies outside the range the window server takes"
                case .typeCheck: "a value has a type the window server does not take"
                case .invalidOperation: "the window server refused the operation"
                case .noneAvailable: "the window server has none available"
                default: "the window server returned an error it does not name"
            }
        }
        let cause = { (rung: Rung, reason: Reason) -> String in
            switch reason {
                case .covered(let owner): "the \(rung) is covered by a window of \(named(owner) ?? "another application")"
                case .notFrontmost(let front): "the \(rung) finds \(named(front) ?? "no application") in front"
                case .notKeyFocus(let keyFocus): "the \(rung) finds \(named(keyFocus) ?? "no application") holding key focus"
                case .symbolicHotKey: "the \(rung) is taken by a system shortcut"
                case .noWindow: "the \(rung) has no window to address"
                case .absent: "the \(rung) lost its window"
                case .unseen: "the \(rung) did not see the application come forward"
                case .windowServer(let error): "the \(rung) failed, \(meaning(error))"
                default: "the \(rung) was refused"
            }
        }
        let listed = { (routes: [Rung: Reason]) -> String in
            Rung.allCases.compactMap { (rung: Rung) -> String? in routes[rung].map { (held: Reason) -> String in cause(rung, held) } }.joined(separator: ", ")
        }
        return switch reason {
            case .noGrant(let grant): "\(subject) is off in System Settings > Privacy & Security > \(grant.pane), turn it on at \(grant.settings)"
            case .notResponsible(let grant, let process):
                "\(subject) runs under \(named(process) ?? "its parent process"), which is off in System Settings > Privacy & Security > \(grant.pane), turn it on at \(grant.settings)"
            case .reused: "\(subject) is already running and only a new instance takes arguments and environment, close it first or open a new instance"
            case .unclaimed: "\(subject) is outside the applications this server drives, call the tools of the server that drives it"
            case .quarantined(let quarantine, let error):
                "\(subject) did not launch while quarantined (\(quarantine)), approve its first launch in System Settings > Privacy & Security and open it again: \(error.localizedDescription)"
            case .unanswered(.none): "\(subject) got no answer in time"
            case .unanswered(.some): "\(subject) got no answer in time and may have run, inspect the application to see whether it did"
            case .stale(let candidates) where candidates.isEmpty: "\(subject) matches nothing open, inspect the application again"
            case .stale(let candidates): "\(subject) matches nothing open, the nearest open container holds \(candidates.formatted()), inspect the application again"
            case .disabled: "\(subject) is disabled"
            case .secure: "\(subject) is a secure field the person fills, no step can"
            case .unchanged(.some(let value)): "\(subject) read back \(value) in place of the written value"
            case .unchanged(.none): "\(subject) read back a value other than the written one"
            case .unseen: "\(subject) changed nothing the application shows within its answer time, view the application to see its state"
            case .refused: "\(subject) refused"
            case .undelivered(let routes, .some(let focus)):
                "no route reached the application: \(listed(routes)); \(focus), so inspect the application again and act on the control by its reference"
            case .undelivered(let routes, .none): "no route reached the application: \(listed(routes)), so inspect the application again and act on the control by its reference"
            case .symbolicHotKey: "\(subject) refused, the system takes it as a shortcut of System Settings > Keyboard > Keyboard Shortcuts"
            case .notFrontmost(let front): "\(subject) refused, the system route posts pointer events to the frontmost application, \(named(front) ?? "none")"
            case .notKeyFocus(let keyFocus): "\(subject) refused, the system route posts keys to the application holding key focus, \(named(keyFocus) ?? "none")"
            case .covered(let owner): "\(subject) refused, a window of \(named(owner) ?? "another application") lies over the point"
            case .unshared: "\(subject) is closed to screen capture by its application"
            case .notText(let value): "\(subject) takes text, not \(value)"
            case .pastDeadline: "\(subject) runs past the reply deadline of \(replyDeadline.formatted(.units(allowed: [.seconds]))), lower the count or the interval"
            case .permittedFrontHeld: "\(subject) refused, a system prompt such as the lock screen or an authorization dialog holds the front, finish that prompt and call again"
            case .noIdentifier: "\(subject) declares no \(kCFBundleIdentifierKey as String) in its Info.plist"
            case .noPath: "\(subject) has no file path"
            case .noExecutable: "\(subject) names no executable"
            case .noWindow: "\(subject) has no window"
            case .absent: "\(subject) is no longer open, inspect the application again"
            case .noBackingStore: "\(subject) is off screen and the window server holds no image of it, show it through the application first"
            case .spansDisplays: "\(subject) spans more than one display and a recording covers one, record the window alone or a region within one display"
            case .noImage: "\(subject) yielded no image"
            case .noEncoder(let extensions) where extensions.isEmpty: "\(subject) has no encoder"
            case .noEncoder(let extensions): "\(subject) has no encoder, give the file the extension \(extensions.formatted(.list(type: .or)))"
            case .notOnConsole: "\(subject) refused, the session is not on the console, call again once it is"
            case .screenLocked: "\(subject) refused, the screen is locked, call again once it is unlocked"
            case .displayAsleep: "\(subject) refused, the display is asleep, call again once it wakes"
            case .requiresApproval: "\(subject) waits for approval in System Settings > General > Login Items & Extensions, turn it on there"
            case .accessibility(.failure): "\(subject) failed, the Accessibility API hit a system error"
            case .accessibility(.illegalArgument): "\(subject) failed, the application refused an argument as illegal"
            case .accessibility(.invalidUIElement): "\(subject) failed, the element is gone, inspect the application again"
            case .accessibility(.invalidUIElementObserver): "\(subject) failed, its notification observer is no longer valid"
            case .accessibility(.cannotComplete): "\(subject) failed, the application is busy or did not answer"
            case .accessibility(.attributeUnsupported): "\(subject) failed, the element has no such attribute"
            case .accessibility(.actionUnsupported): "\(subject) failed, the element has no such action"
            case .accessibility(.notificationUnsupported): "\(subject) failed, the element sends no such notification"
            case .accessibility(.notImplemented): "\(subject) failed, the application does not implement the Accessibility API"
            case .accessibility(.notificationAlreadyRegistered): "\(subject) failed, the notification is already registered"
            case .accessibility(.notificationNotRegistered): "\(subject) failed, the notification is not registered"
            case .accessibility(.apiDisabled): "\(subject) failed, the Accessibility API is off for this server"
            case .accessibility(.noValue): "\(subject) failed, the element holds no such value"
            case .accessibility(.parameterizedAttributeUnsupported): "\(subject) failed, the element has no such parameterized attribute"
            case .accessibility(.notEnoughPrecision): "\(subject) failed, the value lacks the precision asked for"
            case .accessibility: "\(subject) failed, the Accessibility API returned an error it does not name"
            case .windowServer(let error): "\(subject) failed, \(meaning(error))"
            case .event(let failure): failure.errorDescription
            case .system(let error) where error == .noSuchProcess: "\(subject) failed, the process has exited"
            case .system(let error): "\(subject) failed: \(error.description)"
            case .foundation(let error): "\(subject) failed: \(error.localizedDescription)"
            case .cancelled: "\(subject) was canceled"
            case .unmatched(let miss): miss.errorDescription
        }
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .event(let failure): [NSUnderlyingErrorKey: failure]
            case .system(let error): [NSUnderlyingErrorKey: error]
            case .foundation(let error), .quarantined(_, let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .noGrant, .notResponsible, .reused, .unclaimed, .unanswered, .stale, .disabled, .secure, .unchanged, .unseen, .refused, .undelivered, .symbolicHotKey, .notFrontmost, .notKeyFocus,
                .covered, .unshared, .notText, .pastDeadline, .permittedFrontHeld, .noIdentifier, .noPath, .noExecutable, .noWindow, .absent, .noBackingStore, .spansDisplays, .noImage, .noEncoder,
                .notOnConsole, .screenLocked, .displayAsleep, .requiresApproval, .accessibility, .windowServer, .cancelled:
                [:]
        }
    }
}

extension ApplicationFailure: Encodable {
    private enum CodingKeys: CodingKey {
        case reason, subject, message, grant, settings
    }

    package func encode(to encoder: any Encoder) throws {
        let owned: Grant? = reason[case: \.noGrant]
        let parent: Grant? = reason[case: \.notResponsible].map { (grant: Grant, _: pid_t) -> Grant in grant }
        let grant: Grant? = owned ?? parent
        let settings: String? = grant.map(\Grant.settings)
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(reason, forKey: .reason)
        try fields.encode(subject, forKey: .subject)
        try fields.encodeIfPresent(errorDescription, forKey: .message)
        try fields.encodeIfPresent(grant, forKey: .grant)
        try fields.encodeIfPresent(settings, forKey: .settings)
    }
}

extension ApplicationFailure {
    init(_ error: AXError, subject: String, process: pid_t?, answering action: Step? = nil, otherwise: (() -> Self)? = nil) {
        let classified: Bool = [AXError.cannotComplete, AXError.invalidUIElement].contains(error)
        let probe = { (process: pid_t) -> Result<ProcessInstance, Errno> in Result(catching: { () throws(Errno) -> ProcessInstance in try ProcessInstance(of: process) }) }
        let probed: Result<ProcessInstance, Errno>? = classified ? process.map(probe) : nil
        let gone: Result<ProcessInstance, Errno> = .failure(.noSuchProcess)
        let exited: Bool = probed == gone
        let refused: () -> Self = otherwise ?? { Self(subject: subject, reason: .accessibility(error)) }
        self =
            switch (error, exited) {
                case (.apiDisabled, _): Grant.accessibility.missing ?? refused()
                case (.cannotComplete, true), (.invalidUIElement, true): Self(subject: subject, reason: .system(.noSuchProcess))
                case (.cannotComplete, false): Self(subject: subject, reason: .unanswered(action))
                case (.invalidUIElement, false): Self(subject: subject, reason: .stale(candidates: []))
                case (_, _): refused()
            }
    }
}
