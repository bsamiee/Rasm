import AppKit
import Carbon.OpenScripting
import CoreServices
import Encoding
package import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

package let replyDeadline: Duration = .seconds(120)

// --- [SERVICES] ------------------------------------------------------------------------

let appleEventQueue: DispatchQueue = .init(label: "AppleEvents", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSAppleEventDescriptor {
    package static func send<Value: Sendable>(
        _ event: sending NSAppleEventDescriptor,
        within deadline: Duration?,
        reading: @escaping @Sendable (NSAppleEventDescriptor) -> Value,
    ) async throws(AppleEventFailure) -> Value {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAETargetAddressNotPermitted)]
        let unconsented: Set<OSStatus> = [OSStatus(errAEEventNotPermitted), OSStatus(errAEEventWouldRequireUserConsent)]
        let unanswered: Set<OSStatus> = [OSStatus(errAEWaitCanceled), OSStatus(errAETimeout), OSStatus(connectionInvalid)]
        let address = event.attributeDescriptor(forKeyword: AEKeyword(keyAddressAttr))
        let process: pid_t? = address.flatMap { address in
            address.descriptorType == DescType(typeKernelProcessID) && address.data.count == MemoryLayout<pid_t>.size ? address.data.bytes.load(fromByteOffset: 0, as: pid_t.self) : nil
        }
        let application = process.flatMap { process in NSRunningApplication(processIdentifier: process)?.localizedName } ?? "its target application"
        let subject = "Apple event \((UInt64(event.eventClass) << 32 | UInt64(event.eventID)).sdef) to \(application)"
        let (replies, answer) = AsyncStream<Result<Value, AppleEventFailure>>.makeStream()
        Task(executorPreference: appleEventQueue) { [event] in
            let options: SendOptions = [.waitForReply, .neverInteract, SendOptions(rawValue: UInt(kAEDoNotPromptForUserConsent))]
            let (reply, thrown): (NSAppleEventDescriptor?, OSStatus?)
            do { (reply, thrown) = (try event.sendEvent(options: options, timeout: deadline.map { deadline in (deadline / .seconds(1)).magnitude } ?? NSAppleEventTimeOutDefault), nil) } catch {
                (reply, thrown) = (nil, OSStatus(truncatingIfNeeded: (error as NSError).code))
            }
            let status = thrown ?? reply?.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value ?? noErr
            let decided: OSStatus =
                if unconsented.contains(status) {
                    await event.attributeDescriptor(forKeyword: AEKeyword(keyAddressAttr))?.consent(to: event.eventClass, event.eventID, asking: false) ?? status
                } else {
                    status
                }
            let message = (reply?.paramDescriptor(forKeyword: AEKeyword(keyErrorString)) ?? reply?.paramDescriptor(forKeyword: AEKeyword(kOSAErrorBriefMessage)))?.stringValue
            let offending: String? = reply?.paramDescriptor(forKeyword: AEKeyword(kOSAErrorOffendingObject)).flatMap { object in reading(object) as? AppleEventValue }.map { value in
                switch Result(catching: { try JSONEncoder.jsonLines.encode(value) }) {
                    case .success(let data): String(decoding: data, as: UTF8.self)
                    case .failure(let error): "unreadable, \(error.localizedDescription)"
                }
            }
            answer.yield(
                Result { () throws(AppleEventFailure) in
                    switch (thrown, status) {
                        case (_, let code) where unconsented.contains(code):
                            let consent = AppleEventFailure.Status(decided == OSStatus(errAEEventWouldRequireUserConsent) ? decided : code)
                            throw AppleEventFailure(subject: subject, reason: .unconsented(consent, settings: "\(privacySettings)?Privacy_Automation"))
                        case (_, let code) where undelivered.contains(code): throw AppleEventFailure(subject: subject, reason: .undelivered(AppleEventFailure.Status(code)))
                        case (.some, let code) where unanswered.contains(code): throw AppleEventFailure(subject: subject, reason: .unanswered(AppleEventFailure.Status(code)))
                        case (_, let code) where code != noErr: throw AppleEventFailure(subject: subject, reason: .replied(AppleEventFailure.Status(code), message: message, offending: offending))
                        default: return reading(reply?.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
                    }
                }
            )
            answer.finish()
        }
        return try (await replies.first { _ in true } ?? .failure(AppleEventFailure(subject: subject, reason: .cancelled))).get()
    }

    @concurrent
    package func consent(to eventClass: AEEventClass, _ eventID: AEEventID, asking: Bool) async -> OSStatus {
        await withTaskExecutorPreference(appleEventQueue) { unsafe AEDeterminePermissionToAutomateTarget(aeDesc, eventClass, eventID, asking) }
    }
}
