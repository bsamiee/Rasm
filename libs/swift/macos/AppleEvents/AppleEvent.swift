import Carbon.OpenScripting
public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public enum AppleEventFailure: LocalizedError {
    case undelivered(OSStatus)
    case unanswered(OSStatus)
    case replied(OSStatus, String?)

    public var errorDescription: String? {
        switch self {
            case .undelivered(OSStatus(errAEEventNotPermitted)):
                "Automation permission denied. Grant in System Settings > Privacy & Security > Automation"
            case .undelivered(let status): "Apple Event undelivered (\(status))"
            case .unanswered(let status): "Application sent no reply (\(status)). Event may have run"
            case .replied(let status, let message): "\(message ?? "Application replied with error") (\(status))"
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

let appleEvents: DispatchQueue = DispatchQueue(label: "AppleEvents", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    @concurrent
    public func sent() async -> Result<NSAppleEventDescriptor, AppleEventFailure> {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAETargetAddressNotPermitted), OSStatus(errAEEventNotPermitted)]
        return await withTaskExecutorPreference(appleEvents) {
            var reply: AppleEvent = unsafe AppleEvent()
            let status: OSStatus =
                unsafe aeDesc.map { event in unsafe AESendMessage(event, &reply, AESendMode(kAEWaitReply) | AESendMode(kAENeverInteract), kNoTimeOut) } ?? OSStatus(errAENotAEDesc)
            let answer: NSAppleEventDescriptor = unsafe NSAppleEventDescriptor(aeDescNoCopy: &reply)
            let delivered: Result<NSAppleEventDescriptor, AppleEventFailure> =
                status == noErr ? .success(answer) : .failure(undelivered.contains(status) ? .undelivered(status) : .unanswered(status))
            return delivered.flatMap { answer in
                switch answer.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                    case .some(let code) where undelivered.contains(code): .failure(.undelivered(code))
                    case .some(let code) where code != noErr: .failure(.replied(code, answer.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
                    case .some, .none: .success(answer.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
                }
            }
        }
    }
}
