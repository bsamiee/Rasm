package import CasePaths
import CoreServices
package import Encoding
package import Foundation
package import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

package let privacySettings: String = "x-apple.systempreferences:com.apple.settings.PrivacySecurity.extension"

private let replyErrors: [OSStatus: String] = [
    OSStatus(errAECoercionFail): "a value does not coerce to the type its parameter takes",
    OSStatus(errAEDescNotFound): "a parameter the command requires is missing",
    OSStatus(errAEWrongDataType): "a value has the wrong type",
    OSStatus(errAEEventNotHandled): "the application handles no such event, its scripting dictionary lists the commands it takes",
    OSStatus(errAENoUserInteraction): "the command needs user interaction, which events sent behind the user's windows refuse",
    OSStatus(errAEIllegalIndex): "the index lies outside its container",
    OSStatus(errAEImpossibleRange): "the range runs backward or past its container",
    OSStatus(errAEAccessorNotFound): "the container holds no element of that class",
    OSStatus(errAENoSuchObject): "no object matches the specifier",
    OSStatus(errAEEventFailed): "the command failed",
    OSStatus(errAETypeError): "a value has the wrong type",
    OSStatus(errAEBadKeyForm): "the container takes no specifier of that key form",
    OSStatus(errAENotModifiable): "the property is read only",
    OSStatus(errAEPrivilegeError): "the application refuses the access",
    OSStatus(errAEReadDenied): "the property cannot be read",
    OSStatus(errAEWriteDenied): "the property cannot be set",
    OSStatus(errAEIndexTooLarge): "the index is past the last element",
    OSStatus(errAENotAnElement): "the object is no element of its container",
    OSStatus(errAECantSupplyType): "the application cannot return the type asked for",
    OSStatus(errAECantHandleClass): "the command takes no object of that class",
    OSStatus(errAENotAnEnumMember): "the property takes no such enumerator",
    OSStatus(errAECantPutThatThere): "the container cannot hold an element of that class",
]

// --- [ERRORS] --------------------------------------------------------------------------

package struct AppleEventFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    package enum Reason: CaseLabeled, Equatable, Sendable {
        case undeclared
        case osa(OSStatus)
        case foundation(NSError)
        case unmatched(Unmatched)
        case unencodable
        case activating
        case unconsented(OSStatus)
        case undelivered(OSStatus)
        case unanswered(OSStatus)
        case replied(OSStatus, message: String?, offending: String?)
        case cancelled
    }

    package let subject: String
    package let reason: Reason

    package init(subject: String, reason: Reason) {
        self.subject = subject
        self.reason = reason
    }

    package var errorDescription: String? {
        switch reason {
            case .undeclared:
                "\(subject) has no scripting dictionary: it holds no OSAScriptingDefinition file, sdef or aete resource named for its executable, or script suite, no instance of it runs to answer for one, and none is cached, open it to read the dictionary its running instance answers"
            case .osa(let status): "OpenScripting read no scripting definition from \(subject), error \(status)"
            case .foundation(let error): "\(subject): \(error.localizedDescription)"
            case .unmatched(let miss): miss.errorDescription
            case .unencodable: "No Apple event descriptor holds \(subject)"
            case .activating: "\(subject) brings its application to the front and is refused, as every event goes behind the user's windows"
            case .unconsented(OSStatus(errAEEventWouldRequireUserConsent)):
                "Automation consent for the \(subject) is undecided and was not asked, asking once adds its switch to Privacy & Security > Automation at \(privacySettings)?Privacy_Automation"
            case .unconsented: "Automation consent for the \(subject) is denied, turn on its switch in Privacy & Security > Automation at \(privacySettings)?Privacy_Automation"
            case .undelivered(OSStatus(procNotFound)): "The \(subject) found the application not running, open it first"
            case .undelivered(let status): "The \(subject) is not permitted to reach the application, error \(status)"
            case .unanswered(OSStatus(errAETimeout)): "The \(subject) got no reply within its deadline, the event can still be running"
            case .unanswered(OSStatus(connectionInvalid)): "The \(subject) got no reply, as the application quit while handling it"
            case .unanswered(let status): "The \(subject) got no reply, error \(status), the event can have run"
            case .replied(let status, let message, let offending):
                switch (message ?? replyErrors[status], offending) {
                    case (.some(let message), .some(let object)): "The \(subject) failed with error \(status) on offending object \(object): \(message)"
                    case (.some(let message), .none): "The \(subject) failed with error \(status): \(message)"
                    case (.none, .some(let object)): "The \(subject) failed with error \(status) on offending object \(object)"
                    case (.none, .none): "The \(subject) failed with error \(status)"
                }
            case .cancelled: "Stopped waiting for the reply to the \(subject), the event can still be running"
        }
    }

    package var status: OSStatus? {
        switch reason {
            case .osa(let status), .unconsented(let status), .undelivered(let status), .unanswered(let status), .replied(let status, _, _): status
            case .undeclared, .foundation, .unmatched, .unencodable, .activating, .cancelled: nil
        }
    }

    package var errorUserInfo: [String: Any] {
        switch reason {
            case .foundation(let error): [NSUnderlyingErrorKey: error]
            case .unmatched(let miss): [NSUnderlyingErrorKey: miss]
            case .undeclared, .osa, .unencodable, .activating, .unconsented, .undelivered, .unanswered, .replied, .cancelled:
                status.map { status in [NSUnderlyingErrorKey: NSError(domain: NSOSStatusErrorDomain, code: Int(status))] } ?? [:]
        }
    }
}

extension AppleEventFailure: Encodable {
    private enum Key: CodingKey {
        case reason, subject, message, status
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(reason, forKey: .reason)
        try fields.encode(subject, forKey: .subject)
        try fields.encodeIfPresent(errorDescription, forKey: .message)
        try fields.encodeIfPresent(status, forKey: .status)
    }
}
