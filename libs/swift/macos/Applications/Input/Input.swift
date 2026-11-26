import AppKit
import CasePaths
import CasePathsCore
import CoreGraphics

// --- [MODELS] --------------------------------------------------------------------------

struct Input: Hashable, Sendable {
    @CasePathable
    enum Contact: Hashable, Sendable {
        case button(CGMouseButton)
        case hover
        case wheel(by: SIMD2<Double>, unit: CGScrollEventUnit)
        case key(Key)
    }

    enum Edge: String, CaseIterable, Decodable, Hashable, Sendable {
        case down
        case up
    }

    struct Sample: Hashable, Sendable {
        let at: SIMD2<Double>
        let pressure: Double?
        let tilt: SIMD2<Double>?
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

    init(contact: Contact, path: [Sample], hold: Duration, modifiers: Set<Modifier>, schedule: Schedule, edges: Set<Edge>) throws(ApplicationFailure) {
        let delta: SIMD2<Double> = contact[case: \.wheel]?.by ?? .zero
        let pen: Bool = path.contains { sample in sample.pressure != nil || sample.tilt != nil }
        let spacing: Duration = hold + schedule.jitter
        let crowded: Bool = schedule.every.map { (every: Duration) -> Bool in every < spacing } ?? false
        let finite: Bool = path.allSatisfy { sample in sample.at.x.isFinite && sample.at.y.isFinite }
        let mixed: Bool = Set(path.map { sample in [sample.pressure == nil, sample.tilt == nil] }).count > 1
        let ranged: Bool = path.allSatisfy { sample in (0...1).contains(sample.pressure ?? 0) && (-1...1).contains(sample.tilt?.x ?? 0) && (-1...1).contains(sample.tilt?.y ?? 0) }
        let wheeled: Bool = delta.x.magnitude < Double(Int32.max) && delta.y.magnitude < Double(Int32.max)
        let pointed: Bool = !path.isEmpty || contact.is(\.key)
        let traced: Bool = path.count < 2 || !(contact.is(\.key) || contact.is(\.wheel) || schedule.count > 1)
        let rules: [(kept: Bool, rule: String)] = [
            (schedule.count >= 1, "count \(schedule.count) below 1"),
            (min(hold, schedule.jitter) >= Duration.zero, "a negative hold \(hold) or jitter \(schedule.jitter)"),
            (!crowded, "presses every \(schedule.every ?? spacing) under the hold plus jitter \(spacing)"),
            (schedule.every != nil || schedule.jitter == Duration.zero, "jitter \(schedule.jitter) on a click run, which has no interval to vary"),
            (pointed, "a pointer input with no point"),
            (traced, "a path of \(path.count) points on a key, a wheel, or more than one press"),
            (finite, "a point off the plane"),
            (wheeled, "a wheel delta \(delta) beyond \(Int32.max)"),
            (!pen || contact == .button(.left), "pressure or tilt off the left button"),
            (!mixed, "pressure or tilt on some points alone"),
            (ranged, "pressure outside 0 to 1 or tilt outside -1 to 1"),
            (!edges.isEmpty || !contact.is(\.key), "a key with neither its down nor its up"),
        ]
        let refused: [String] = rules.filter { rule in !rule.kept }.map(\.rule)
        guard refused.isEmpty else { throw ApplicationFailure(subject: "input with \(refused.formatted())", reason: .refused) }
        self.contact = contact
        self.path = path
        self.hold = hold
        self.modifiers = modifiers
        self.schedule = schedule
        self.edges = edges
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Input {
    func timeline(
        doubleClick interval: Duration,
        keyRepeat: (delay: Duration, interval: Duration),
        frames rate: Int,
        layout: KeyboardLayout,
        firstPress first: (number: Int64, clickState: Int64),
        using generator: inout some RandomNumberGenerator,
    ) throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] {
        let every = schedule.every ?? max(hold, (hold + interval) / 2)
        let presses = (0..<schedule.count).reduce(into: [(start: Duration, state: Int64, edges: Set<Edge>)]()) { presses, index in
            let start = presses.last.map { last in last.start + every + schedule.jitter * Double.random(in: -1...1, using: &generator) } ?? .zero
            let state = presses.last.map { last in start - last.start < interval ? last.state + 1 : 1 } ?? first.clickState
            let inner: [Edge] = (index > 0 ? [.down] : []) + (index < schedule.count - 1 ? [.up] : [])
            presses.append((start, state, edges.union(inner)))
        }
        let frames = max(1, Int((hold / .seconds(1) * Double(rate)).rounded(.down)))
        let lengths = zip(path, path.dropFirst()).reduce(into: [0.0]) { (lengths: inout [Double], pair: (from: Sample, to: Sample)) in
            let step: SIMD2<Double> = pair.to.at - pair.from.at
            lengths.append(lengths[lengths.count - 1] + (step * step).sum().squareRoot())
        }
        let walked = (0..<(path.count < 2 ? 0 : frames)).reduce(into: (segment: 0, samples: [Sample]())) { walk, frame in
            let target = lengths[lengths.count - 1] * Double(frame + 1) / Double(frames)
            walk.segment = (walk.segment..<(path.count - 2)).first { segment in lengths[segment + 1] > target } ?? path.count - 2
            let (from, to) = (path[walk.segment], path[walk.segment + 1])
            let span = lengths[walk.segment + 1] - lengths[walk.segment]
            let share = span > 0 ? (target - lengths[walk.segment]) / span : 1
            let pressure: Double? = from.pressure.flatMap { start in to.pressure.map { end in start + (end - start) * share } }
            let tilt: SIMD2<Double>? = from.tilt.flatMap { start in to.tilt.map { end in start + (end - start) * share } }
            walk.samples.append(Sample(at: from.at + (to.at - from.at) * share, pressure: pressure, tilt: tilt))
        }
        let held = CGEventFlags(modifiers.map(\.key.flag))
        let pen = path.contains { sample in sample.pressure != nil || sample.tilt != nil }
        let button = contact[case: \.button]
        let types: (down: CGEventType, drag: CGEventType, up: CGEventType?) =
            switch button {
                case .none: (.mouseMoved, .mouseMoved, nil)
                case .some(.left): (.leftMouseDown, .leftMouseDragged, .leftMouseUp)
                case .some(.right): (.rightMouseDown, .rightMouseDragged, .rightMouseUp)
                case .some: (.otherMouseDown, .otherMouseDragged, .otherMouseUp)
            }
        let pressed: [[(offset: Duration, event: CGEvent)]]
        switch contact {
            case .button, .hover:
                let last = path[path.count - 1]
                let lifted = Sample(at: last.at, pressure: last.pressure.map { _ in 0 }, tilt: last.tilt)
                pressed = try presses.enumerated().map { index, press throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] in
                    let numbered: [(CGEventField, Int64)] = [(.mouseEventClickState, press.state), (.mouseEventNumber, first.number + Int64(index))]
                    let fields: [(CGEventField, Int64)] = button == nil ? [] : numbered + (pen ? [(.mouseEventSubtype, Int64(CGEventMouseSubtype.tabletPoint.rawValue))] : [])
                    let dragged: [(offset: Duration, type: CGEventType, sample: Sample)] = walked.samples.enumerated().map { frame, sample in
                        (press.start + hold * (frame + 1) / frames, types.drag, sample)
                    }
                    let released: [(offset: Duration, type: CGEventType, sample: Sample)] = (press.edges.contains(.up) ? types.up : nil).map { up in [(press.start + hold, up, lifted)] } ?? []
                    let course: [(offset: Duration, type: CGEventType, sample: Sample)] = [(press.start, press.edges.contains(.down) ? types.down : types.drag, path[0])] + dragged + released
                    return try course.map { offset, type, sample throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
                        let made = CGEvent(mouseEventSource: nil, mouseType: type, mouseCursorPosition: .zero, mouseButton: button ?? .left)
                        return (offset, try event(type, from: made, at: sample, flags: held, fields: fields, text: nil))
                    }
                }
            case .wheel(let by, let unit):
                let steps = (0...frames).map { frame in (by * Double(frame) / Double(frames)).rounded(.toNearestOrAwayFromZero) }
                let shares = zip(steps.dropFirst(), steps).map { reached, before in reached - before } + (unit == .pixel ? [.zero] : [])
                pressed = try presses.map { press throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] in
                    try shares.enumerated().map { frame, share throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
                        let phase: CGScrollPhase =
                            switch (frame, press.edges.contains(.down), press.edges.contains(.up)) {
                                case (0, true, _): .began
                                case (frames, _, true): .ended
                                default: .changed
                            }
                        let made = CGEvent(scrollWheelEvent2Source: nil, units: unit, wheelCount: 2, wheel1: Int32(share.y), wheel2: Int32(share.x), wheel3: 0)
                        let fields: [(CGEventField, Int64)] = unit == .pixel ? [(.scrollWheelEventScrollPhase, Int64(phase.rawValue))] : []
                        return (press.start + hold * frame / frames, try event(.scrollWheel, from: made, at: path[0], flags: held, fields: fields, text: nil))
                    }
                }
            case .key(let key):
                let strokes = try key.strokes(holding: modifiers, on: layout)
                let typing = presses.flatMap { press in strokes.indices.map { order in (press: press, order: order) } }
                pressed = try typing.map { press, order throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] in
                    let (stroke, last) = (strokes[order], order == strokes.count - 1)
                    let posting: Set<Edge> = last ? press.edges : press.edges.contains(.down) ? Set(Edge.allCases) : []
                    let repeats = last ? Array(sequence(first: keyRepeat.delay) { offset in offset + keyRepeat.interval }.prefix { offset in offset <= hold }) : []
                    let course: [(offset: Duration, edge: Edge)] = [(press.start, .down)] + repeats.map { offset in (press.start + offset, .down) } + [(press.start + (last ? hold : .zero), .up)]
                    let typed = try course.filter { _, edge in posting.contains(edge) }.map { offset, edge throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
                        let made = CGEvent(keyboardEventSource: nil, virtualKey: stroke.code, keyDown: edge == .down)
                        return (offset, try event(edge == .down ? .keyDown : .keyUp, from: made, at: path.first, flags: stroke.flags, fields: [], text: stroke.text))
                    }
                    let added: [Modifier] = Modifier.allCases.filter { modifier in stroke.flags.contains(modifier.key.flag) && !modifiers.contains(modifier) }
                    return try bracket(added, holding: modifiers, around: typed, edges: posting)
                }
        }
        let course = pressed.flatMap(\.self)
        let proximity = try Edge.allCases.map { edge throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] in
            guard pen, edges.contains(edge) else { return [] }
            let fields: [(CGEventField, Int64)] = [
                (.tabletProximityEventEnterProximity, edge == .down ? 1 : 0), (.tabletProximityEventPointerType, Int64(NSEvent.PointingDeviceType.pen.rawValue)),
                (
                    .tabletProximityEventCapabilityMask,
                    Int64(NX_TABLET_CAPABILITY_ABSXMASK | NX_TABLET_CAPABILITY_ABSYMASK | NX_TABLET_CAPABILITY_PRESSUREMASK | NX_TABLET_CAPABILITY_TILTXMASK | NX_TABLET_CAPABILITY_TILTYMASK),
                ),
            ]
            let offset: Duration = edge == .down ? .zero : course.last?.offset ?? .zero
            return [(offset, try event(.tabletProximity, from: CGEvent(source: nil), at: Sample(at: path[0].at, pressure: nil, tilt: nil), flags: held, fields: fields, text: nil))]
        }
        return try bracket(Modifier.allCases.filter(modifiers.contains), holding: [], around: proximity[0] + course + proximity[1], edges: edges)
    }

    private func bracket(
        _ added: [Modifier],
        holding base: Set<Modifier>,
        around inner: [(offset: Duration, event: CGEvent)],
        edges: Set<Edge>,
    ) throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] {
        let (start, end): (Duration, Duration) = (inner.first?.offset ?? .zero, inner.last?.offset ?? .zero)
        let opening: [(offset: Duration, modifier: Modifier, held: Set<Modifier>)] = edges.contains(.down) ? added.indices.map { index in (start, added[index], base.union(added[...index])) } : []
        let closing: [(offset: Duration, modifier: Modifier, held: Set<Modifier>)] =
            edges.contains(.up) ? added.indices.reversed().map { index in (end, added[index], base.union(added[..<index])) } : []
        let changed = try (opening + closing).map { offset, modifier, held throws(ApplicationFailure) -> (offset: Duration, event: CGEvent) in
            let made = CGEvent(keyboardEventSource: nil, virtualKey: modifier.key.code, keyDown: held.contains(modifier))
            return (offset, try event(.flagsChanged, from: made, at: path.first, flags: CGEventFlags(held.map(\.key.flag)), fields: [], text: nil))
        }
        return Array(changed.prefix(opening.count)) + inner + changed.suffix(closing.count)
    }

    private func event(_ type: CGEventType, from made: CGEvent?, at sample: Sample?, flags: CGEventFlags, fields: [(CGEventField, Int64)], text: String?) throws(ApplicationFailure) -> CGEvent {
        guard let made else { throw ApplicationFailure(subject: "event of type \(type.rawValue) for input \(contact)", reason: .refused) }
        let pressure: [(CGEventField, Double)] = sample?.pressure.map { pressure in [(.mouseEventPressure, pressure), (.tabletEventPointPressure, pressure)] } ?? []
        let tilt: [(CGEventField, Double)] = sample?.tilt.map { tilt in [(.tabletEventTiltX, tilt.x), (.tabletEventTiltY, tilt.y)] } ?? []
        made.type = type
        made.flags = flags.union(.maskNonCoalesced)
        if let sample { made.location = CGPoint(x: sample.at.x, y: sample.at.y) }
        for (field, value) in fields { made.setIntegerValueField(field, value: value) }
        for (field, value) in pressure + tilt { made.setDoubleValueField(field, value: value) }
        if let units = text.map({ text in Array(text.utf16) }) {
            units.withUnsafeBufferPointer { units in unsafe made.keyboardSetUnicodeString(stringLength: units.count, unicodeString: units.baseAddress) }
        }
        return made
    }
}
