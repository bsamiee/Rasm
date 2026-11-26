import CoreServices
public import Foundation

// --- [SERVICES] ------------------------------------------------------------------------

let appleEventQueue: DispatchQueue = .init(label: "AppleEvents", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    public static func send<Value: Sendable>(
        _ event: sending NSAppleEventDescriptor,
        within deadline: Duration?,
        reading: @escaping @Sendable (NSAppleEventDescriptor) -> Value,
    ) async throws(AppleEventFailure) -> Value {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAETargetAddressNotPermitted)]
        let unconsented: Set<OSStatus> = [OSStatus(errAEEventNotPermitted), OSStatus(errAEEventWouldRequireUserConsent)]
        let subject = (UInt64(event.eventClass) << 32 | UInt64(event.eventID)).sdef
        let (replies, answer) = AsyncStream<Result<Value, AppleEventFailure>>.makeStream()
        Task(executorPreference: appleEventQueue) { [event] in
            let options: SendOptions = [.waitForReply, .neverInteract, SendOptions(rawValue: UInt(kAEDoNotPromptForUserConsent))]
            let (reply, thrown): (NSAppleEventDescriptor?, OSStatus?)
            do { (reply, thrown) = (try event.sendEvent(options: options, timeout: deadline.map { deadline in (deadline / .seconds(1)).magnitude } ?? NSAppleEventTimeOutDefault), nil) } catch {
                (reply, thrown) = (nil, OSStatus(truncatingIfNeeded: (error as NSError).code))
            }
            let status = thrown ?? reply?.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value ?? noErr
            let undecided =
                if unconsented.contains(status) {
                    await event.attributeDescriptor(forKeyword: AEKeyword(keyAddressAttr))?.consent(to: event.eventClass, event.eventID, asking: false) == OSStatus(errAEEventWouldRequireUserConsent)
                } else {
                    false
                }
            answer.yield(
                Result { () throws(AppleEventFailure) in
                    switch (thrown, status) {
                        case (_, let code) where unconsented.contains(code):
                            throw AppleEventFailure(subject: subject, reason: .unconsented(undecided ? OSStatus(errAEEventWouldRequireUserConsent) : code))
                        case (_, let code) where undelivered.contains(code): throw AppleEventFailure(subject: subject, reason: .undelivered(code))
                        case (.some(let code), _): throw AppleEventFailure(subject: subject, reason: .unanswered(code))
                        case (.none, let code) where code != noErr:
                            throw AppleEventFailure(subject: subject, reason: .replied(code, reply?.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
                        case (.none, _): return reading(reply?.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
                    }
                }
            )
            answer.finish()
        }
        return try (await replies.first { _ in true } ?? .failure(AppleEventFailure(subject: subject, reason: .cancelled))).get()
    }

    @concurrent
    public func consent(to eventClass: AEEventClass, _ eventID: AEEventID, asking: Bool) async -> OSStatus {
        await withTaskExecutorPreference(appleEventQueue) { unsafe AEDeterminePermissionToAutomateTarget(aeDesc, eventClass, eventID, asking) }
    }
}
