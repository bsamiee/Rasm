import AppKit
public import CoreGraphics
public import Encoding
import System

// --- [TYPES] ---------------------------------------------------------------------------

public enum Route: CaseTagged, Hashable, Sendable {
    case process
    case window(number: CGWindowID)
    case focused(number: CGWindowID)
    case system
}

@objc private protocol EventAuthenticationMessage {
    @objc(messageWithEventRecord:pid:version:)
    static func message(eventRecord: UnsafeRawPointer, pid: pid_t, version: UInt32) -> AnyObject?
}

// --- [CONSTANTS] -----------------------------------------------------------------------

private let record: (size: Int, length: Int, kind: Int, window: Int, subtype: Int) = (0xF8, 0x04, 0x08, 0x3C, 0x8A)
private let routing: (window: CGEventField?, gesture: CGEventField?) = (CGEventField(rawValue: 51), CGEventField(rawValue: 58))

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Route {
    func post(_ timeline: [(offset: Duration, event: CGEvent)], to process: pid_t, from source: CGEventSource, clock: some Clock<Duration>) async throws(ApplicationFailure) {
        let subject = "input to process \(process) through \(self)"
        let (number, subtypes): (CGWindowID?, [NSEvent.EventSubtype]) =
            switch self {
                case .window(let number): (number, [])
                case .focused(let number): (number, [.applicationActivated, .applicationDeactivated])
                case .process, .system: (nil, [])
            }
        let target: (number: CGWindowID, origin: CGPoint)? = try number.map { number throws(ApplicationFailure) -> (number: CGWindowID, origin: CGPoint) in
            let rows: [[String: Any]] = CGWindowListCopyWindowInfo(.optionIncludingWindow, number) as? [[String: Any]] ?? []
            guard let shown = rows.first.flatMap(InstanceState.Window.init) else { throw ApplicationFailure(subject: "window \(number) of \(subject)", reason: .noWindow) }
            return (number, shown.frame.origin)
        }
        let identity: [UInt8] = number.map { number in unsafe withUnsafeBytes(of: number.littleEndian, Array.init) } ?? []
        let marks: [[UInt8]] = subtypes.map { subtype in
            (0..<record.size).map { offset in
                switch offset {
                    case record.length: UInt8(record.size)
                    case record.kind: UInt8(NSEvent.EventType.appKitDefined.rawValue)
                    case record.window..<record.window + identity.count: identity[offset - record.window]
                    case record.subtype: UInt8(subtype.rawValue)
                    default: 0
                }
            }
        }
        var serial = ProcessSerialNumber()
        guard unsafe marks.isEmpty || findProcess(process, &serial) else { throw ApplicationFailure(subject: String(process), reason: .system(.noSuchProcess)) }
        guard self != .system || NSWorkspace.shared.frontmostApplication?.processIdentifier == process else {
            throw ApplicationFailure(subject: "\(subject), which posts to the frontmost process alone", reason: .refused)
        }
        for state in [CGEventSuppressionState.eventSuppressionStateSuppressionInterval, .eventSuppressionStateRemoteMouseDrag] {
            source.setLocalEventsFilterDuringSuppressionState([.permitLocalMouseEvents, .permitLocalKeyboardEvents, .permitSystemDefinedEvents], state: state)
        }
        let keys: Set<CGEventType> = [.keyDown, .keyUp, .flagsChanged]
        let signed: Bool = self != .system && timeline.allSatisfy { _, event in keys.contains(event.type) && !event.flags.contains(.maskCommand) }
        let authentication: AnyClass? = NSClassFromString("SLSEventAuthenticationMessage")
        let opened: CGError? = marks.first.map { mark in unsafe postEventRecord(&serial, mark) }
        if let opened, opened != .success { throw ApplicationFailure(subject: "focus record of \(subject)", reason: .windowServer(opened)) }
        let start = clock.now
        var copied = [UInt8](repeating: 0, count: record.size)
        let walked: ApplicationFailure?
        do throws(ApplicationFailure) {
            for (offset, event) in timeline {
                do { try await clock.sleep(until: start.advanced(by: offset), tolerance: nil) } catch { throw ApplicationFailure(subject: subject, reason: .cancelled) }
                event.setSource(source)
                let read: Int32? = unsafe signed ? getEventRecord(event, &copied, copied.count) : nil
                let message: AnyObject? = unsafe read == 0 ? (authentication?.message?(eventRecord: copied, pid: process, version: 0)).flatMap(\.self) : nil
                guard read == nil || message != nil else { throw ApplicationFailure(subject: "signing key event \(event.type.rawValue) of \(subject)", reason: .refused) }
                let aimed: (number: CGWindowID, origin: CGPoint)? = keys.contains(event.type) ? nil : target
                let stamps: [(CGEventField?, Int64)] =
                    aimed.map { number, _ in
                        [
                            (.eventTargetUnixProcessID, Int64(process)), (routing.window, Int64(number)), (.mouseEventWindowUnderMousePointer, Int64(number)),
                            (.mouseEventWindowUnderMousePointerThatCanHandleThisEvent, Int64(number)), (routing.gesture, event.getIntegerValueField(.mouseEventNumber)),
                        ]
                    } ?? []
                if let message { setAuthenticationMessage(event, message) }
                for case (let field?, let value) in stamps { event.setIntegerValueField(field, value: value) }
                if let aimed { setWindowLocation(event, CGPoint(x: event.location.x - aimed.origin.x, y: event.location.y - aimed.origin.y)) }
                if self == .system { event.post(tap: .cghidEventTap) } else { event.postToPid(process) }
            }
            walked = nil
        } catch {
            walked = error
        }
        let closed: CGError? = marks.last.map { mark in unsafe postEventRecord(&serial, mark) }
        if let walked { throw walked }
        if let closed, closed != .success { throw ApplicationFailure(subject: "defocus record of \(subject)", reason: .windowServer(closed)) }
    }
}

@_silgen_name("SLPSPostEventRecordTo")
private func postEventRecord(_: UnsafePointer<ProcessSerialNumber>, _: UnsafePointer<UInt8>) -> CGError

@_silgen_name("SLEventGetEventRecord")
private func getEventRecord(_: CGEvent, _: UnsafeMutableRawPointer, _: Int) -> Int32

@_silgen_name("SLEventSetAuthenticationMessage")
private func setAuthenticationMessage(_: CGEvent, _: AnyObject)

@_silgen_name("CGEventSetWindowLocation")
private func setWindowLocation(_: CGEvent, _: CGPoint)
