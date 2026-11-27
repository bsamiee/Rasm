import AppKit
import AsyncAlgorithms
import Carbon.HIToolbox
import CoreGraphics
import Encoding
import SPI
import System

// --- [TYPES] ---------------------------------------------------------------------------

enum Route: CaseTagged, Hashable, Sendable {
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

package enum Rung: String, CaseIterable, Codable, Sendable, CustomStringConvertible {
    case accessibility
    case process
    case window
    case focused
    case system

    package var description: String {
        switch self {
            case .accessibility: "Accessibility call"
            case .process: "process route"
            case .window: "window route"
            case .focused: "focused-window route"
            case .system: "system route"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Route {
    func post(
        _ timeline: [(offset: Duration, event: CGEvent)],
        to process: pid_t,
        from source: CGEventSource,
        frame: Duration,
        clock: some Clock<Duration> & Sendable,
    ) async throws(ApplicationFailure) -> ApplicationFailure? {
        try Grant.postEvent.check()
        let subject = "\(rung)"
        let (number, focused, tapped): (CGWindowID?, CGWindowID?, Bool) =
            switch self {
                case .process: (nil, nil, false)
                case .window(let number): (number, nil, false)
                case .focused(let number): (number, number, false)
                case .system: (nil, nil, true)
            }
        let keys: Set<CGEventType> = [.keyDown, .keyUp, .flagsChanged]
        let releases: Set<CGEventType> = [.leftMouseUp, .rightMouseUp, .otherMouseUp]
        let presses: Set<CGEventType> = [.leftMouseDown, .rightMouseDown, .otherMouseDown, .scrollWheel]
        let types: Set<CGEventType> = Set(timeline.map { (_: Duration, event: CGEvent) -> CGEventType in event.type })
        let (pointing, keyed): (Bool, Bool) = (!types.isSubset(of: keys), !types.isDisjoint(with: keys))
        let authenticated: Bool = keyed && !tapped
        let hotKeys: Result<Set<SIMD2<Int>>, ApplicationFailure> =
            if tapped, types.contains(.keyDown) {
                await MainActor.run { () -> Result<Set<SIMD2<Int>>, ApplicationFailure> in
                    var listed: Unmanaged<CFArray>?
                    let read: OSStatus = unsafe CopySymbolicHotKeys(&listed)
                    let rows: [[String: Any]] = unsafe listed?.takeRetainedValue() as? [[String: Any]] ?? []
                    let carbon: Int = Modifier.allCases.reduce(kEventKeyModifierFnMask) { (mask: Int, modifier: Modifier) -> Int in mask | Int(modifier.key.state) << 8 }
                    let enabled: [SIMD2<Int>] = rows.compactMap { (row: [String: Any]) -> SIMD2<Int>? in
                        switch (row[kHISymbolicHotKeyEnabled as String] as? Bool, row[kHISymbolicHotKeyCode as String] as? Int, row[kHISymbolicHotKeyModifiers as String] as? Int) {
                            case (true?, let code?, let state?): SIMD2(code, state & carbon)
                            case (_, _, _): nil
                        }
                    }
                    guard read == noErr else {
                        return .failure(ApplicationFailure(subject: "symbolic hot keys for the \(subject)", reason: .foundation(NSError(domain: NSOSStatusErrorDomain, code: Int(read)))))
                    }
                    return .success(Set(enabled))
                }
            } else {
                .success([])
            }
        let chords: Set<SIMD2<Int>> = try hotKeys.get()
        let claimed: [String] = timeline.compactMap { (_: Duration, event: CGEvent) -> String? in
            guard event.type == .keyDown else { return nil }
            let held: [Modifier] = Modifier.allCases.filter { (modifier: Modifier) -> Bool in event.flags.contains(modifier.key.flag) }
            let function: Int = event.flags.contains(.maskSecondaryFn) ? kEventKeyModifierFnMask : 0
            let chord = SIMD2(Int(event.getIntegerValueField(.keyboardEventKeycode)), held.reduce(function) { (state: Int, modifier: Modifier) -> Int in state | Int(modifier.key.state) << 8 })
            return chords.contains(chord) ? "key code \(chord.x) with \(held.map(\.rawValue).formatted())" : nil
        }
        guard claimed.isEmpty else { throw ApplicationFailure(subject: "\(claimed.formatted()) on the \(subject)", reason: .symbolicHotKey) }
        let focus: (front: pid_t?, keyFocus: pid_t?)? = tapped ? focusedProcesses() : nil
        if let focus, pointing, focus.front != process { throw ApplicationFailure(subject: subject, reason: .notFrontmost(focus.front)) }
        if let focus, keyed, focus.keyFocus != process { throw ApplicationFailure(subject: subject, reason: .notKeyFocus(focus.keyFocus)) }
        for (_, event) in timeline where tapped && presses.contains(event.type) {
            let at = "the \(subject) at \(Int(event.location.x)),\(Int(event.location.y))"
            var (screen, local, window, connection, owner): (CGPoint, CGPoint, CGWindowID, Int32, pid_t) = (event.location, .zero, kCGNullWindowID, 0, 0)
            let found: CGError = unsafe findWindowAndOwner(mainConnection(), 0, 1, 0, &screen, &local, &window, &connection)
            let owned: CGError = found == .success ? unsafe processOfConnection(connection, &owner) : found
            guard owned == .success else { throw ApplicationFailure(subject: "hit test of \(at)", reason: .windowServer(owned)) }
            guard owner == process else { throw ApplicationFailure(subject: at, reason: .covered(by: owner)) }
        }
        let target: (origin: CGPoint, owner: pid_t)? = try number.map { (number: CGWindowID) throws(ApplicationFailure) -> (origin: CGPoint, owner: pid_t) in
            guard let described = windowDescription(of: number), !described.bounds.isEmpty else { throw ApplicationFailure(subject: "target window of the \(subject)", reason: .absent) }
            return (described.bounds.origin, described.owner)
        }
        let (origin, receiver): (CGPoint?, pid_t) = (target?.origin, target?.owner ?? process)
        let stamps: [(field: CGEventField, value: Int64)] =
            number.map { (number: CGWindowID) -> [(field: CGEventField, value: Int64)] in
                [
                    (.eventTargetUnixProcessID, Int64(receiver)), (windowNumberField, Int64(number)), (.mouseEventWindowUnderMousePointer, Int64(number)),
                    (.mouseEventWindowUnderMousePointerThatCanHandleThisEvent, Int64(number)),
                ]
            } ?? []
        let resting: CGPoint? = tapped ? timeline.last(where: { _, event in releases.contains(event.type) })?.event.location : nil
        let pointer: CGPoint? = resting.flatMap { _ in CGEvent(source: nil)?.location }
        let buttons: Set<CGMouseButton> = Set(
            timeline.compactMap { (_: Duration, event: CGEvent) -> CGMouseButton? in
                releases.contains(event.type) ? CGMouseButton(rawValue: UInt32(event.getIntegerValueField(.mouseEventButtonNumber))) : nil
            }
        )
        var serial = ProcessSerialNumber()
        guard unsafe (focused == nil && !authenticated) || findProcess(receiver, &serial) else {
            throw ApplicationFailure(subject: "application receiving the \(subject)", reason: .system(.noSuchProcess))
        }
        var (receiving, reported, version): (Int32, pid_t, UInt32) = (0, 0, 0)
        let joined: CGError = authenticated ? unsafe connectionOfSerial(mainConnection(), &serial, &receiving) : .success
        let versioned: CGError = authenticated && joined == .success ? unsafe processAndVersionOfConnection(receiving, &reported, &version) : joined
        guard versioned == .success else { throw ApplicationFailure(subject: "window-server version of the application receiving the \(subject)", reason: .windowServer(versioned)) }
        let activation: CGEvent? = try focused.map { (window: CGWindowID) throws(ApplicationFailure) -> CGEvent in
            guard let record = CGEvent(source: nil) else { throw ApplicationFailure(subject: "focus record of the \(subject)", reason: .refused) }
            record.type = appKitDefinedEventType
            record.setIntegerValueField(windowNumberField, value: Int64(window))
            record.setIntegerValueField(compoundSubtypeField, value: Int64(NSEvent.EventSubtype.applicationActivated.rawValue))
            return record
        }
        let opened: CGError = activation.map { (record: CGEvent) -> CGError in unsafe postEventRecord(&serial, recordPointer(record)) } ?? .success
        guard opened == .success else { throw ApplicationFailure(subject: "focus record of the \(subject)", reason: .windowServer(opened)) }
        let flags: [(CGKeyCode, CGEventFlags)] = Modifier.allCases.map { (modifier: Modifier) -> (CGKeyCode, CGEventFlags) in (modifier.key.code, modifier.key.flag) }
        let modifiers: [CGKeyCode: CGEventFlags] = Dictionary(uniqueKeysWithValues: flags)
        let edges: [(contact: SIMD2<Int64>, down: Bool)?] = timeline.map { (_: Duration, event: CGEvent) -> (contact: SIMD2<Int64>, down: Bool)? in
            let (code, button): (Int64, Int64) = (event.getIntegerValueField(.keyboardEventKeycode), event.getIntegerValueField(.mouseEventButtonNumber))
            return switch event.type {
                case .keyDown, .keyUp: (SIMD2(0, code), event.type == .keyDown)
                case .flagsChanged: modifiers[CGKeyCode(truncatingIfNeeded: code)].map { (flag: CGEventFlags) -> (contact: SIMD2<Int64>, down: Bool) in (SIMD2(0, code), event.flags.contains(flag)) }
                case .leftMouseDown, .rightMouseDown, .otherMouseDown: (SIMD2(1, button), true)
                case .leftMouseUp, .rightMouseUp, .otherMouseUp: (SIMD2(1, button), false)
                case .tabletProximity: (SIMD2(2, 0), event.getIntegerValueField(.tabletProximityEventEnterProximity) != 0)
                default: nil
            }
        }
        let start = clock.now
        var (walked, posted, held): (ApplicationFailure?, Bool, Set<SIMD2<Int64>>) = (nil, false, [])
        for ((offset, event), edge) in zip(timeline, edges) {
            if walked == nil {
                do { try await clock.sleep(until: start.advanced(by: offset), tolerance: nil) } catch { walked = ApplicationFailure(subject: "input on the \(subject)", reason: .cancelled) }
            }
            guard walked == nil || (edge.map { (edge: (contact: SIMD2<Int64>, down: Bool)) -> Bool in !edge.down && held.contains(edge.contact) } ?? false) else { continue }
            event.setSource(source)
            let message: EventAuthenticationMessage? =
                authenticated && keys.contains(event.type) ? unsafe EventAuthenticationMessage(eventRecord: recordPointer(event), pid: receiver, version: version) : nil
            if let message { setAuthenticationMessage(event, message) }
            if let origin, !keys.contains(event.type) {
                for (field, value) in stamps { event.setIntegerValueField(field, value: value) }
                setWindowLocation(event, CGPoint(x: event.location.x - origin.x, y: event.location.y - origin.y))
            }
            if tapped { event.post(tap: .cghidEventTap) } else { event.postToPid(receiver) }
            posted = true
            switch edge {
                case .some(let edge) where edge.down: held.insert(edge.contact)
                case .some(let edge): held.remove(edge.contact)
                case .none: break
            }
        }
        activation?.setIntegerValueField(compoundSubtypeField, value: Int64(NSEvent.EventSubtype.applicationDeactivated.rawValue))
        let closed: CGError = activation.map { (record: CGEvent) -> CGError in unsafe postEventRecord(&serial, recordPointer(record)) } ?? .success
        let restored: Bool = await withTaskCancellationShield { () async -> Bool in
            guard let pointer, let resting,
                await AsyncTimerSequence(interval: frame, clock: clock).prefix(Int(messagingTimeout / frame)).contains(where: { _ in
                    let at: CGPoint = CGEvent(source: nil)?.location ?? pointer
                    return hypot(at.x - resting.x, at.y - resting.y) < 1 && !buttons.contains { button in CGEventSource.buttonState(.combinedSessionState, button: button) }
                })
            else { return true }
            source.localEventsSuppressionInterval = 0
            let move: CGEvent? = CGEvent(mouseEventSource: source, mouseType: .mouseMoved, mouseCursorPosition: pointer, mouseButton: .left)
            move?.post(tap: .cghidEventTap)
            return move != nil
        }
        let unclosed: ApplicationFailure? = closed == .success ? nil : ApplicationFailure(subject: "defocus record of the \(subject)", reason: .windowServer(closed))
        let unrestored: ApplicationFailure? = restored ? nil : ApplicationFailure(subject: "pointer restore after the \(subject)", reason: .refused)
        let ended: ApplicationFailure? = walked ?? unclosed ?? unrestored
        switch (ended, posted) {
            case (.none, _): return nil
            case (.some(let failure), true) where failure.reason != .cancelled: return failure
            case (.some(let failure), _): throw failure
        }
    }
}
