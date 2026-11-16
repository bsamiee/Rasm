import CoreServices
public import Foundation

// --- [SERVICES] ------------------------------------------------------------------------

let appleEvents: DispatchQueue = .init(label: "AppleEvents", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    @concurrent
    public func send(timeout: TimeInterval = NSAppleEventTimeOutNone) async throws(AppleEventFailure) -> NSAppleEventDescriptor {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAETargetAddressNotPermitted), OSStatus(errAEEventNotPermitted), OSStatus(errAEEventWouldRequireUserConsent)]
        let subject = (UInt64(eventClass) << 32 | UInt64(eventID)).sdef
        let sent = await withTaskExecutorPreference(appleEvents) { Result { try sendEvent(options: [.waitForReply, .neverInteract], timeout: timeout) } }
        let reply = try sent.mapError { error in
            let status = OSStatus(truncatingIfNeeded: (error as NSError).code)
            return AppleEventFailure(subject: subject, reason: undelivered.contains(status) ? .undelivered(status) : .unanswered(status))
        }
        .get()
        return switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
            case .some(let code) where undelivered.contains(code): throw AppleEventFailure(subject: subject, reason: .undelivered(code))
            case .some(let code) where code != noErr:
                throw AppleEventFailure(subject: subject, reason: .replied(code, reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
            case .some, .none: reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null()
        }
    }
}
