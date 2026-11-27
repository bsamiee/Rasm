package import AppleEvents
package import ApplicationServices
package import CasePaths
package import Foundation
import Processes
package import Results
package import System

// --- [ERRORS] --------------------------------------------------------------------------

package struct ApplicationFailure: ModuleError, Equatable {
    @CasePathable
    package enum Reason: Equatable, Sendable {
        case noGrant(pane: String, settings: String)
        case parentLacksGrant(pane: String, parent: String?, settings: String)
        case alreadyRunning
        case outsideClaim
        case quarantined(Installation.Quarantine, NSError)
        case unanswered
        case unansweredMayHaveRun(Step)
        case notOpen(nearest: [String])
        case disabled
        case secureField
        case readBack(Fact.Scalar?)
        case noChangeSeen
        case refused
        case undelivered(routes: [Route.Kind: Self], focus: String?)
        case systemShortcut
        case frontElsewhere(String?)
        case keyFocusElsewhere(String?)
        case covered(by: String?)
        case closedToCapture
        case notText(Fact.Scalar)
        case pastReplyDeadline(Duration)
        case frontListHeld(entries: Int)
        case noIdentifier
        case noPath
        case noExecutable
        case noWindow
        case outsideWindow
        case noDisplay
        case absent
        case noBackingStore
        case spansDisplays
        case noImage
        case noEncoder(extensions: [String])
        case notOnConsole
        case screenLocked
        case displayAsleep
        case awaitsLoginItemApproval
        case accessibility(AXError)
        case windowServer(CGError)
        case event(AppleEventFailure)
        case system(Errno)
        case foundation(NSError)
        case cancelled
        case unmatched(Unmatched)
    }

    package let subject: String
    package let reason: Reason

    package init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .event(let failure): [NSUnderlyingErrorKey: failure]
            case .system(let error): [NSUnderlyingErrorKey: error]
            case .foundation(let error), .quarantined(_, let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .noGrant, .parentLacksGrant, .alreadyRunning, .outsideClaim, .unanswered, .unansweredMayHaveRun, .notOpen, .disabled, .secureField, .readBack, .noChangeSeen, .refused, .undelivered,
                .systemShortcut, .frontElsewhere,
                .keyFocusElsewhere, .covered, .closedToCapture, .notText, .pastReplyDeadline, .frontListHeld, .noIdentifier, .noPath, .noExecutable, .noWindow, .outsideWindow, .noDisplay, .absent,
                .noBackingStore, .spansDisplays, .noImage,
                .noEncoder, .notOnConsole, .screenLocked, .displayAsleep, .awaitsLoginItemApproval, .accessibility, .windowServer, .cancelled:
                [:]
        }
    }
}

extension ApplicationFailure {
    init(subject: String, cause: any Error) {
        self.init(subject: subject, reason: (cause as? Errno).map(Reason.system) ?? .foundation(cause as NSError))
    }

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
                case (.cannotComplete, false): Self(subject: subject, reason: action.map(Reason.unansweredMayHaveRun) ?? .unanswered)
                case (.invalidUIElement, false): Self(subject: subject, reason: .notOpen(nearest: []))
                case (_, _): refused()
            }
    }
}

extension AXError: StatusCode {
    package var name: String {
        switch self {
            case .success: "kAXErrorSuccess"
            case .failure: "kAXErrorFailure"
            case .illegalArgument: "kAXErrorIllegalArgument"
            case .invalidUIElement: "kAXErrorInvalidUIElement"
            case .invalidUIElementObserver: "kAXErrorInvalidUIElementObserver"
            case .cannotComplete: "kAXErrorCannotComplete"
            case .attributeUnsupported: "kAXErrorAttributeUnsupported"
            case .actionUnsupported: "kAXErrorActionUnsupported"
            case .notificationUnsupported: "kAXErrorNotificationUnsupported"
            case .notImplemented: "kAXErrorNotImplemented"
            case .notificationAlreadyRegistered: "kAXErrorNotificationAlreadyRegistered"
            case .notificationNotRegistered: "kAXErrorNotificationNotRegistered"
            case .apiDisabled: "kAXErrorAPIDisabled"
            case .noValue: "kAXErrorNoValue"
            case .parameterizedAttributeUnsupported: "kAXErrorParameterizedAttributeUnsupported"
            case .notEnoughPrecision: "kAXErrorNotEnoughPrecision"
            @unknown default: String(rawValue)
        }
    }
}

extension CGError: StatusCode {
    package var name: String { AppleEventFailure.Status(rawValue).name }
}
