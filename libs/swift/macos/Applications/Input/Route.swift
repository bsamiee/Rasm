import AppKit
import AsyncAlgorithms
import Carbon.HIToolbox
public import CoreGraphics
public import Encoding
import System

// --- [TYPES] ---------------------------------------------------------------------------

public enum Route: CaseTagged, Hashable, Sendable {
    case process
    case window(number: CGWindowID)
    case focused(number: CGWindowID)
    case system

    func ladder(window: CGWindowID?) -> [Self] {
        switch (self, window) {
            case (.process, .some(let number)): [.process, .focused(number: number), .system]
            case (.process, .none): [.process, .system]
            case (.window(let number), _): [.window(number: number), .focused(number: number), .system]
            case (.focused(let number), _): [.focused(number: number), .system]
            case (.system, _): [.system]
        }
    }

    var rung: Rung {
        switch self {
            case .process: .process
            case .window: .window
            case .focused: .focused
            case .system: .system
        }
    }
}

public enum Rung: String, CaseIterable, Codable, Sendable {
    case accessibility
    case process
    case window
    case focused
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
    func post(
        _ timeline: [(offset: Duration, event: CGEvent)],
        to process: pid_t,
        from source: CGEventSource,
        frame: Duration,
        clock: some Clock<Duration> & Sendable,
    ) async throws(ApplicationFailure) {
        try Grant.postEvent.check()
        let subject = "input to process \(process) through \(self)"
        let (number, focusing, tapped): (CGWindowID?, Bool, Bool) =
            switch self {
                case .process: (nil, false, false)
                case .window(let number): (number, false, false)
                case .focused(let number): (number, true, false)
                case .system: (nil, false, true)
            }
        let keys: Set<CGEventType> = [.keyDown, .keyUp, .flagsChanged]
        let releases: Set<CGEventType> = [.leftMouseUp, .rightMouseUp, .otherMouseUp]
        let (pointing, keyed): (Bool, Bool) = (timeline.contains { _, event in !keys.contains(event.type) }, timeline.contains { _, event in keys.contains(event.type) })
        var listed: Unmanaged<CFArray>?
        let read: OSStatus = tapped && timeline.contains(where: { _, event in event.type == .keyDown }) ? unsafe CopySymbolicHotKeys(&listed) : OSStatus(noErr)
        guard read == noErr else { throw ApplicationFailure(subject: "symbolic hot keys for \(subject)", reason: .foundation(NSError(domain: NSOSStatusErrorDomain, code: Int(read)))) }
        let hotKeys: [[String: Any]] = unsafe listed?.takeRetainedValue() as? [[String: Any]] ?? []
        let carbon: Int = Modifier.allCases.reduce(kEventKeyModifierFnMask) { mask, modifier in mask | Int(modifier.key.state) << 8 }
        let chords: Set<SIMD2<Int>> = Set(
            hotKeys.compactMap { (row: [String: Any]) -> SIMD2<Int>? in
                guard row[kHISymbolicHotKeyEnabled as String] as? Bool == true, let code = row[kHISymbolicHotKeyCode as String] as? Int,
                    let state = row[kHISymbolicHotKeyModifiers as String] as? Int
                else { return nil }
                return SIMD2(code, state & carbon)
            }
        )
        let claimed: [String] = timeline.compactMap { (_: Duration, event: CGEvent) -> String? in
            guard event.type == .keyDown, !chords.isEmpty else { return nil }
            let held: [Modifier] = Modifier.allCases.filter { modifier in event.flags.contains(modifier.key.flag) }
            let function: Int = event.flags.contains(.maskSecondaryFn) ? kEventKeyModifierFnMask : 0
            let chord = SIMD2(Int(event.getIntegerValueField(.keyboardEventKeycode)), held.reduce(function) { state, modifier in state | Int(modifier.key.state) << 8 })
            return chords.contains(chord) ? "key code \(chord.x) with \(held.map(\.rawValue).formatted())" : nil
        }
        guard claimed.isEmpty else { throw ApplicationFailure(subject: "\(subject), where the system takes \(claimed.formatted()) as its own shortcut", reason: .refused) }
        let focus: (front: pid_t?, keyFocus: pid_t?)? = tapped ? focusedProcesses() : nil
        guard focus.map({ focus in !pointing || focus.front == process }) != false else {
            throw ApplicationFailure(subject: "\(subject), which posts pointer events to the front process alone", reason: .refused)
        }
        guard focus.map({ focus in !keyed || focus.keyFocus == process }) != false else {
            throw ApplicationFailure(subject: "\(subject), which posts keys to the key-focus process alone", reason: .refused)
        }
        let presses: Set<CGEventType> = tapped ? [.leftMouseDown, .rightMouseDown, .otherMouseDown, .scrollWheel] : []
        for (_, event) in timeline where presses.contains(event.type) {
            var (screen, local, window, connection, owner): (CGPoint, CGPoint, CGWindowID, Int32, pid_t) = (event.location, .zero, kCGNullWindowID, 0, 0)
            let found: CGError = unsafe findWindowAndOwner(mainConnection(), 0, 1, 0, &screen, &local, &window, &connection)
            let owned: CGError = found == .success ? unsafe processOfConnection(connection, &owner) : found
            guard owned == .success else { throw ApplicationFailure(subject: "window under \(event.location) for \(subject)", reason: .windowServer(owned)) }
            guard owner == process else { throw ApplicationFailure(subject: "\(subject), where window \(window) of process \(owner) lies over \(event.location)", reason: .refused) }
        }
        let origin: CGPoint? = try number.map { (number: CGWindowID) throws(ApplicationFailure) -> CGPoint in
            let row: [String: Any]? = (CGWindowListCopyWindowInfo(.optionIncludingWindow, number) as? [[String: Any]])?.first
            let bounds: CGRect? = (row?[kCGWindowBounds as String] as? [String: Any]).flatMap { (bounds: [String: Any]) -> CGRect? in CGRect(dictionaryRepresentation: bounds as CFDictionary) }
            guard let bounds, !bounds.isEmpty else { throw ApplicationFailure(subject: "window \(number) of \(subject)", reason: .noWindow) }
            return bounds.origin
        }
        let stamps: [(field: CGEventField?, value: (CGEvent) -> Int64)] =
            number.map { (number: CGWindowID) -> [(field: CGEventField?, value: (CGEvent) -> Int64)] in
                [
                    (.eventTargetUnixProcessID, { (_: CGEvent) -> Int64 in Int64(process) }), (routing.window, { (_: CGEvent) -> Int64 in Int64(number) }),
                    (.mouseEventWindowUnderMousePointer, { (_: CGEvent) -> Int64 in Int64(number) }),
                    (.mouseEventWindowUnderMousePointerThatCanHandleThisEvent, { (_: CGEvent) -> Int64 in Int64(number) }),
                    (routing.gesture, { (event: CGEvent) -> Int64 in event.getIntegerValueField(.mouseEventNumber) }),
                ]
            } ?? []
        let signed: Bool = !tapped && !pointing && timeline.allSatisfy { _, event in !event.flags.contains(.maskCommand) }
        let authentication: AnyClass? = signed ? NSClassFromString("SLSEventAuthenticationMessage") : nil
        let resting: CGPoint? = tapped ? timeline.last(where: { _, event in releases.contains(event.type) })?.event.location : nil
        let pointer: CGPoint? = resting.flatMap { _ in CGEvent(source: nil)?.location }
        let buttons: Set<CGMouseButton> = Set(
            timeline.compactMap { (_: Duration, event: CGEvent) -> CGMouseButton? in
                releases.contains(event.type) ? CGMouseButton(rawValue: UInt32(event.getIntegerValueField(.mouseEventButtonNumber))) : nil
            }
        )
        for state in [CGEventSuppressionState.eventSuppressionStateSuppressionInterval, .eventSuppressionStateRemoteMouseDrag] {
            source.setLocalEventsFilterDuringSuppressionState([.permitLocalMouseEvents, .permitLocalKeyboardEvents, .permitSystemDefinedEvents], state: state)
        }
        var serial = ProcessSerialNumber()
        guard unsafe !focusing || findProcess(process, &serial) else { throw ApplicationFailure(subject: String(process), reason: .system(.noSuchProcess)) }
        var activation = [UInt8](repeating: 0, count: record.size)
        activation[record.length] = UInt8(record.size)
        activation[record.kind] = UInt8(NSEvent.EventType.appKitDefined.rawValue)
        activation[record.subtype] = UInt8(NSEvent.EventSubtype.applicationActivated.rawValue)
        activation.withUnsafeMutableBytes { bytes in unsafe bytes.storeBytes(of: (number ?? kCGNullWindowID).littleEndian, toByteOffset: record.window, as: CGWindowID.self) }
        let opened: CGError = focusing ? unsafe postEventRecord(&serial, activation) : .success
        guard opened == .success else { throw ApplicationFailure(subject: "focus record of \(subject)", reason: .windowServer(opened)) }
        let lifts: Set<CGEventType> = releases.union([.keyUp, .flagsChanged, .tabletProximity])
        let start = clock.now
        var (walked, posted): (ApplicationFailure?, Bool) = (nil, false)
        for (offset, event) in timeline {
            if walked == nil {
                do { try await clock.sleep(until: start.advanced(by: offset), tolerance: nil) } catch { walked = ApplicationFailure(subject: subject, reason: .cancelled) }
            }
            guard walked == nil || posted && lifts.contains(event.type) else { continue }
            event.setSource(source)
            let message: AnyObject? = unsafe (signed ? recordPointer(event) : nil).flatMap { (record: UnsafeRawPointer) -> AnyObject? in
                unsafe (authentication?.message?(eventRecord: record, pid: process, version: 0)).flatMap(\.self)
            }
            guard !signed || message != nil else {
                walked = walked ?? ApplicationFailure(subject: "signing key event \(event.type.rawValue) of \(subject)", reason: .refused)
                continue
            }
            if let message { setAuthenticationMessage(event, message) }
            if let origin, !keys.contains(event.type) {
                for case (let field?, let value) in stamps { event.setIntegerValueField(field, value: value(event)) }
                setWindowLocation(event, CGPoint(x: event.location.x - origin.x, y: event.location.y - origin.y))
            }
            if tapped { event.post(tap: .cghidEventTap) } else { event.postToPid(process) }
            posted = true
        }
        activation[record.subtype] = UInt8(NSEvent.EventSubtype.applicationDeactivated.rawValue)
        let closed: CGError = focusing ? unsafe postEventRecord(&serial, activation) : .success
        let warped: CGError = await withTaskCancellationShield { () async -> CGError in
            guard let pointer, let resting else { return .success }
            let settled: Bool = await AsyncTimerSequence(interval: frame, clock: clock).prefix(Int(messagingTimeout / frame)).contains { _ in
                let at: CGPoint = CGEvent(source: nil)?.location ?? pointer
                return hypot(at.x - resting.x, at.y - resting.y) < 1 && !buttons.contains { button in CGEventSource.buttonState(.combinedSessionState, button: button) }
            }
            return settled ? CGWarpMouseCursorPosition(pointer) : .success
        }
        if let walked { throw walked }
        guard closed == .success else { throw ApplicationFailure(subject: "defocus record of \(subject)", reason: .windowServer(closed)) }
        guard warped == .success else { throw ApplicationFailure(subject: "pointer restore of \(subject)", reason: .windowServer(warped)) }
    }
}

@_silgen_name("SLPSPostEventRecordTo")
private func postEventRecord(_: UnsafePointer<ProcessSerialNumber>, _: UnsafePointer<UInt8>) -> CGError

@_silgen_name("SLEventRecordPointer")
private func recordPointer(_: CGEvent) -> UnsafeRawPointer?

@_silgen_name("SLEventSetAuthenticationMessage")
private func setAuthenticationMessage(_: CGEvent, _: AnyObject)

@_silgen_name("CGEventSetWindowLocation")
private func setWindowLocation(_: CGEvent, _: CGPoint)
