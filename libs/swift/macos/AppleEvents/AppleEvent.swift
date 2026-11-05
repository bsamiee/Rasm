import CoreServices
public import Foundation

// --- [SERVICES] ------------------------------------------------------------------------

package let appleEvents: DispatchQueue = DispatchQueue(label: "AppleEvents", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    public func send(timeout: TimeInterval = NSAppleEventTimeOutNone) throws(AppleEventFailure) -> NSAppleEventDescriptor {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAETargetAddressNotPermitted), OSStatus(errAEEventNotPermitted)]
        let subject: String = (UInt64(eventClass) << 32 | UInt64(eventID)).sdef
        let reply: NSAppleEventDescriptor
        do {
            reply = try sendEvent(options: [.waitForReply, .neverInteract], timeout: timeout)
        } catch {
            let status: OSStatus = OSStatus(truncatingIfNeeded: (error as NSError).code)
            throw AppleEventFailure(subject: subject, reason: undelivered.contains(status) ? .undelivered(status) : .unanswered(status))
        }
        return switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
            case .some(let code) where undelivered.contains(code): throw AppleEventFailure(subject: subject, reason: .undelivered(code))
            case .some(let code) where code != noErr:
                throw AppleEventFailure(subject: subject, reason: .replied(code, reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
            case .some, .none: reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null()
        }
    }

    @concurrent
    public func sent(timeout: TimeInterval = NSAppleEventTimeOutNone) async throws(AppleEventFailure) -> NSAppleEventDescriptor {
        try await withTaskExecutorPreference(appleEvents) { () throws(AppleEventFailure) in try send(timeout: timeout) }
    }
}
