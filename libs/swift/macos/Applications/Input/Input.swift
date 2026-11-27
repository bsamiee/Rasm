import AppleEvents
import CasePaths
import CasePathsCore
import CoreGraphics
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

struct Input: Hashable, Sendable {
    @CasePathable
    enum Contact: Hashable, Sendable, CustomStringConvertible {
        case button(CGMouseButton)
        case hover
        case wheel(by: SIMD2<Double>, unit: CGScrollEventUnit)
        case key(Key)
        case modifiers

        var description: String {
            switch self {
                case .button(.left): "left button"
                case .button(.right): "right button"
                case .button(.center): "middle button"
                case .button(let button): "button \(button.rawValue + 1)"
                case .hover: "hover"
                case .wheel(let by, let unit): "scroll by \(by.coordinates) \(unit == .line ? "lines" : "pixels")"
                case .key(.special(let special)): "key \(special.rawValue)"
                case .key(.character(let character)): "key \(String(character).debugDescription)"
                case .key(.code(let code)): "key code \(code)"
                case .modifiers: "modifier hold"
            }
        }
    }

    enum Edge: String, CaseIterable, Decodable, Hashable, Sendable {
        case down
        case up
    }

    struct Sample: Hashable, Sendable, CustomStringConvertible {
        let at: SIMD2<Double>
        let pressure: Double?
        let tilt: SIMD2<Double>?

        var description: String { at.coordinates }
    }

    struct Schedule: Hashable, Sendable {
        let count: Int
        let every: Duration?
        let jitter: Duration
    }

    let contact: Contact
    let path: [Sample]
    let hold: Duration
    let modifiers: Set<Modifier>
    let schedule: Schedule
    let edges: Set<Edge>
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension SIMD2 where Scalar == Double {
    var coordinates: String { "\(x.formatted(numberFormat)),\(y.formatted(numberFormat))" }
}

extension Input {
    func timeline(
        doubleClick interval: Duration,
        keyRepeat: (delay: Duration, interval: Duration),
        frame: Duration,
        layout: KeyboardLayout,
        numbering first: (number: Int64, clickState: Int64),
        using generator: inout some RandomNumberGenerator,
    ) throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] {
        let walked: [Double] = zip(path, path.dropFirst()).reduce(into: []) { (walked: inout [Double], pair: (from: Sample, to: Sample)) in
            let step: SIMD2<Double> = pair.to.at - pair.from.at
            walked.append((walked.last ?? 0) + (step * step).sum().squareRoot())
        }
        let travel: Double = contact[case: \.wheel].map { wheel in max(wheel.by.x.magnitude, wheel.by.y.magnitude) } ?? walked.last ?? 0
        let paced: Int = max(contact.is(\.wheel) ? 1 : walked.count, Int(min(hold / frame, travel.rounded())))
        let span: Duration = max(hold, frame * paced)
        let step: Duration = span / max(paced, 1)
        let every: Duration = schedule.every ?? (contact.is(\.key) ? max(span, keyRepeat.interval) : max(span, (span + interval) / 2))
        let least: Duration = max(every - schedule.jitter, span) * (schedule.count - 1) + span
        guard least <= replyDeadline else {
            throw ApplicationFailure(
                subject: "A run of \(schedule.count) presses of \(contact) every \(every.formatted(.units(allowed: [.seconds, .milliseconds], width: .abbreviated)))",
                reason: .pastReplyDeadline,
            )
        }
        let initial: (start: Duration, clickState: Int64, number: Int64, edges: Set<Edge>) = (.zero, first.clickState, first.number, edges.union(schedule.count > 1 ? [Edge.up] : []))
        let presses: [(start: Duration, clickState: Int64, number: Int64, edges: Set<Edge>)] = (1..<schedule.count).reduce(into: [initial]) { presses, index in
            let last = presses[presses.count - 1]
            let start: Duration = max(last.start + every + schedule.jitter * Double.random(in: -1...1, using: &generator), last.start + span)
            presses.append((start, start - last.start < interval ? last.clickState + 1 : 1, last.number + 1, edges.union(index < schedule.count - 1 ? [Edge.down, .up] : [Edge.down])))
        }
        let ends: [Int] = walked.enumerated().map { (index: Int, distance: Double) -> Int in
            index + 1 + Int((Double(paced - walked.count) * (travel > 0 ? distance / travel : Double(index + 1) / Double(walked.count))).rounded())
        }
        let points: [Sample] =
            Array(path.prefix(1))
            + zip(zip(path, path.dropFirst()), zip([0] + ends, ends)).flatMap { (pair: (from: Sample, to: Sample), bounds: (start: Int, end: Int)) -> [Sample] in
                (1...(bounds.end - bounds.start)).map { (move: Int) -> Sample in
                    let share = Double(move) / Double(bounds.end - bounds.start)
                    return Sample(
                        at: pair.from.at + (pair.to.at - pair.from.at) * share,
                        pressure: pair.from.pressure.map { start in start + ((pair.to.pressure ?? start) - start) * share },
                        tilt: pair.from.tilt.map { start in start + ((pair.to.tilt ?? start) - start) * share },
                    )
                }
            }
        let pen: Bool = path.first.map { sample in sample.pressure != nil || sample.tilt != nil } ?? false
        let held = CGEventFlags(modifiers.map(\.key.flag))
        let course: [(offset: Duration, event: CGEvent)]
        switch contact {
            case .button, .hover:
                let button: CGMouseButton? = contact[case: \.button]
                let types: (down: CGEventType, drag: CGEventType, up: CGEventType?) =
                    switch button {
                        case .none: (.mouseMoved, .mouseMoved, nil)
                        case .some(.left): (.leftMouseDown, .leftMouseDragged, .leftMouseUp)
                        case .some(.right): (.rightMouseDown, .rightMouseDragged, .rightMouseUp)
                        case .some: (.otherMouseDown, .otherMouseDragged, .otherMouseUp)
                    }
                let moments: [(offset: Duration, type: CGEventType, point: Int, press: Int)] = presses.indices.flatMap {
                    (index: Int) -> [(offset: Duration, type: CGEventType, point: Int, press: Int)] in
                    let press = presses[index]
                    let lifted: [(offset: Duration, type: CGEventType, point: Int, press: Int)] =
                        (press.edges.contains(.up) ? types.up : nil).map { up in [(press.start + span, up, points.count - 1, index)] } ?? []
                    let moved: [(offset: Duration, type: CGEventType, point: Int, press: Int)] = points.indices.dropFirst().map { point in (press.start + step * point, types.drag, point, index) }
                    return [(press.start, press.edges.contains(.down) ? types.down : types.drag, 0, index)] + moved + lifted
                }
                course = try moments.map { offset, type, point, press throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
                    let (sample, up): (Sample, Bool) = (points[point], type == types.up)
                    let delta: SIMD2<Double> = up || point == 0 ? .zero : sample.at.rounded(.toNearestOrAwayFromZero) - points[point - 1].at.rounded(.toNearestOrAwayFromZero)
                    let pressure: Double = up ? 0 : sample.pressure ?? 1
                    let made = CGEvent(mouseEventSource: nil, mouseType: type, mouseCursorPosition: CGPoint(x: sample.at.x, y: sample.at.y), mouseButton: button ?? .left)
                    let event = try self.event(from: made, at: nil, flags: held.union(.maskNonCoalesced))
                    if pen { event.setIntegerValueField(.mouseEventSubtype, value: Int64(CGEventMouseSubtype.tabletPoint.rawValue)) }
                    event.setIntegerValueField(.mouseEventDeltaX, value: Int64(delta.x))
                    event.setIntegerValueField(.mouseEventDeltaY, value: Int64(delta.y))
                    guard button != nil else { return (offset, event) }
                    event.setIntegerValueField(.mouseEventClickState, value: presses[press].clickState)
                    event.setIntegerValueField(.mouseEventNumber, value: presses[press].number)
                    event.setDoubleValueField(.mouseEventPressure, value: pressure)
                    guard pen else { return (offset, event) }
                    event.setDoubleValueField(.tabletEventPointPressure, value: pressure)
                    event.setDoubleValueField(.tabletEventTiltX, value: sample.tilt?.x ?? 0)
                    event.setDoubleValueField(.tabletEventTiltY, value: sample.tilt?.y ?? 0)
                    return (offset, event)
                }
            case .wheel(let by, let unit):
                let reached: [SIMD2<Double>] = (0...paced).map { index in (by * Double(index) / Double(paced)).rounded(.toNearestOrAwayFromZero) }
                let moments: [(offset: Duration, share: SIMD2<Double>, phase: CGScrollPhase)] = presses.flatMap { press -> [(offset: Duration, share: SIMD2<Double>, phase: CGScrollPhase)] in
                    let ended: [(offset: Duration, share: SIMD2<Double>, phase: CGScrollPhase)] = unit == .pixel && press.edges.contains(.up) ? [(press.start + span, .zero, .ended)] : []
                    return zip(reached.dropFirst(), reached).enumerated().map { (index: Int, pair: (SIMD2<Double>, SIMD2<Double>)) -> (offset: Duration, share: SIMD2<Double>, phase: CGScrollPhase) in
                        (press.start + step * index, pair.0 - pair.1, index == 0 && press.edges.contains(.down) ? .began : .changed)
                    } + ended
                }
                course = try moments.map { offset, share, phase throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
                    let made = CGEvent(scrollWheelEvent2Source: nil, units: unit, wheelCount: 2, wheel1: Int32(share.y), wheel2: Int32(share.x), wheel3: 0)
                    let event = try self.event(from: made, at: path[0].at, flags: held)
                    guard unit == .pixel else { return (offset, event) }
                    event.setIntegerValueField(.scrollWheelEventScrollPhase, value: Int64(phase.rawValue))
                    return (offset, event)
                }
            case .key(let key):
                let strokes: [Stroke] = key.strokes(holding: modifiers, on: layout)
                let repeats: [Duration] = Array(sequence(first: keyRepeat.delay) { offset in offset + keyRepeat.interval }.prefix { offset in offset <= hold })
                let typings: [(press: Int, order: Int)] = presses.indices.flatMap { (press: Int) -> [(press: Int, order: Int)] in
                    strokes.indices.map { (order: Int) -> (press: Int, order: Int) in (press, order) }
                }
                course = try typings.map { press, order throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] in
                    let (stroke, start, last): (Stroke, Duration, Bool) = (strokes[order], presses[press].start, order == strokes.count - 1)
                    let posted: Set<Edge> =
                        switch (last, presses[press].edges.contains(.down)) {
                            case (true, _): presses[press].edges
                            case (false, true): Set(Edge.allCases)
                            case (false, false): []
                        }
                    let downs: [Duration] = posted.contains(.down) ? [start] + (last ? repeats.map { offset in start + offset } : []) : []
                    let moments: [(offset: Duration, down: Bool)] = downs.map { offset in (offset, true) } + (posted.contains(.up) ? [(start + (last ? hold : .zero), false)] : [])
                    let typed: [(offset: Duration, event: CGEvent)] = try moments.map { offset, down throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
                        (offset, try event(from: CGEvent(keyboardEventSource: nil, virtualKey: stroke.code, keyDown: down), at: path.first?.at, flags: stroke.flags, text: stroke.text))
                    }
                    let added: [Modifier] = Modifier.allCases.filter { modifier in stroke.flags.contains(modifier.key.flag) && !modifiers.contains(modifier) }
                    return try bracket(added, holding: held, around: typed, edges: posted)
                }
                .flatMap(\.self)
            case .modifiers: course = []
        }
        let proximity: [[(offset: Duration, event: CGEvent)]] = try Edge.allCases.map { edge throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] in
            guard pen, edges.contains(edge) else { return [] }
            let entering: Bool = edge == .down
            let event = try self.event(from: CGEvent(source: nil), at: (entering ? path.first : path.last)?.at, flags: held)
            let capabilities = NX_TABLET_CAPABILITY_ABSXMASK | NX_TABLET_CAPABILITY_ABSYMASK | NX_TABLET_CAPABILITY_PRESSUREMASK | NX_TABLET_CAPABILITY_TILTXMASK | NX_TABLET_CAPABILITY_TILTYMASK
            event.type = .tabletProximity
            event.setIntegerValueField(.tabletProximityEventEnterProximity, value: entering ? 1 : 0)
            event.setIntegerValueField(.tabletProximityEventPointerType, value: Int64(NX_TABLET_POINTER_PEN))
            event.setIntegerValueField(.tabletProximityEventCapabilityMask, value: Int64(capabilities))
            return [(entering ? .zero : course.last?.offset ?? .zero, event)]
        }
        return try bracket(Modifier.allCases.filter(modifiers.contains), holding: [], around: proximity[0] + course + proximity[1], edges: edges)
    }

    private func bracket(
        _ added: [Modifier],
        holding base: CGEventFlags,
        around inner: [(offset: Duration, event: CGEvent)],
        edges: Set<Edge>,
    ) throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] {
        let all: CGEventFlags = base.union(CGEventFlags(added.map(\.key.flag)))
        let opening: [(offset: Duration, modifier: Modifier, flags: CGEventFlags)] = (edges.contains(.down) ? added : []).reduce(into: []) { built, modifier in
            built.append((inner.first?.offset ?? .zero, modifier, (built.last?.flags ?? base).union(modifier.key.flag)))
        }
        let closing: [(offset: Duration, modifier: Modifier, flags: CGEventFlags)] = (edges.contains(.up) ? added : []).reversed().reduce(into: []) { built, modifier in
            built.append((inner.last?.offset ?? hold, modifier, (built.last?.flags ?? all).subtracting(modifier.key.flag)))
        }
        let changed: [(offset: Duration, event: CGEvent)] = try (opening + closing).map { offset, modifier, flags throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
            (offset, try event(from: CGEvent(keyboardEventSource: nil, virtualKey: modifier.key.code, keyDown: flags.contains(modifier.key.flag)), at: path.first?.at, flags: flags))
        }
        return Array(changed.prefix(opening.count)) + inner + changed.suffix(closing.count)
    }

    private func event(from made: CGEvent?, at point: SIMD2<Double>?, flags: CGEventFlags, text: String? = nil) throws(ApplicationFailure) -> CGEvent {
        guard let made else { throw ApplicationFailure(subject: "event for \(contact)", reason: .refused) }
        let own: CGEventFlags = [.keyDown, .keyUp].contains(made.type) ? made.flags.intersection([.maskHelp, .maskSecondaryFn, .maskNumericPad]) : []
        made.flags = own.union(flags)
        if let point { made.location = CGPoint(x: point.x, y: point.y) }
        if let text {
            let units: [UniChar] = Array(text.utf16)
            unsafe made.keyboardSetUnicodeString(stringLength: units.count, unicodeString: units)
        }
        return made
    }
}
