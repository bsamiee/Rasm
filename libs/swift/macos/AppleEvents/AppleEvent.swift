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
                "Automation permission denied, nothing ran. Allow this process in System Settings > Privacy & Security > Automation"
            case .undelivered(let status): "Apple Event undelivered (\(status)), nothing ran"
            case .unanswered(let status): "Application sent no reply (\(status)), event outcome unknown"
            case .replied(let status, let message): "\(message ?? "Application replied with an error") (\(status))"
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

let appleEvents: DispatchQueue = DispatchQueue(label: "AppleEvents", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    @concurrent
    public func sent(within timeout: Duration) async -> Result<NSAppleEventDescriptor, AppleEventFailure> {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAEEventNotPermitted)]
        return await withTaskExecutorPreference(appleEvents) {
            Result { try sendEvent(options: [.waitForReply, .neverInteract], timeout: timeout / .seconds(1)) }
                .mapError { error -> AppleEventFailure in
                    let status: OSStatus = OSStatus((error as NSError).code)
                    return undelivered.contains(status) ? .undelivered(status) : .unanswered(status)
                }
                .flatMap { answer in
                    switch answer.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                        case .some(let code) where undelivered.contains(code): .failure(.undelivered(code))
                        case .some(let code) where code != noErr: .failure(.replied(code, answer.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
                        case .some, .none: .success(answer.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
                    }
                }
        }
    }
}
