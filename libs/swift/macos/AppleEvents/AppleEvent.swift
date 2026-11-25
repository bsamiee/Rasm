import CoreServices
public import Foundation

// --- [SERVICES] ------------------------------------------------------------------------

let appleEventQueue: DispatchQueue = .init(label: "AppleEvents", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    public static func send<Value: Sendable>(
        _ event: sending NSAppleEventDescriptor,
        timeout: TimeInterval = NSAppleEventTimeOutNone,
        reading: @escaping @Sendable (NSAppleEventDescriptor) -> Value,
    ) async throws(AppleEventFailure) -> Value {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAETargetAddressNotPermitted), OSStatus(errAEEventNotPermitted), OSStatus(errAEEventWouldRequireUserConsent)]
        let subject = (UInt64(event.eventClass) << 32 | UInt64(event.eventID)).sdef
        let (replies, answer) = AsyncStream<Result<Value, AppleEventFailure>>.makeStream()
        Task(executorPreference: appleEventQueue) { [event] in
            answer.yield(
                Result { () throws(AppleEventFailure) in
                    let reply: NSAppleEventDescriptor
                    do { reply = try event.sendEvent(options: [.waitForReply, .neverInteract], timeout: timeout) } catch {
                        let status = OSStatus(truncatingIfNeeded: (error as NSError).code)
                        throw AppleEventFailure(subject: subject, reason: undelivered.contains(status) ? .undelivered(status) : .unanswered(status))
                    }
                    return switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
                        case .some(let code) where undelivered.contains(code): throw AppleEventFailure(subject: subject, reason: .undelivered(code))
                        case .some(let code) where code != noErr:
                            throw AppleEventFailure(subject: subject, reason: .replied(code, reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
                        case .some, .none: reading(reply.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
                    }
                }
            )
            answer.finish()
        }
        return try (await replies.first { _ in true } ?? .failure(AppleEventFailure(subject: subject, reason: .cancelled))).get()
    }
}
