import AppKit
import CustomDump
import PropertyBased
import Testing

@testable import Applications

// --- [TYPES] ---------------------------------------------------------------------------

enum Shape: CaseIterable, Sendable {
    case clicks
    case stroke
    case drag
    case hover
    case wheel
    case key
}

// --- [CONSTANTS] -----------------------------------------------------------------------

private let downs: Set<CGEventType> = [.leftMouseDown, .rightMouseDown, .otherMouseDown]
private let ups: Set<CGEventType> = [.leftMouseUp, .rightMouseUp, .otherMouseUp]
private let moves: Set<CGEventType> = [.leftMouseDragged, .rightMouseDragged, .otherMouseDragged, .mouseMoved]
private let modifierKeys: [Modifier: (code: Int64, flag: CGEventFlags)] = [
    .command: (55, .maskCommand), .shift: (56, .maskShift), .option: (58, .maskAlternate), .control: (59, .maskControl), .function: (63, .maskSecondaryFn),
]
private let keys: [(key: Key, strokes: Int)] = [(.character("a"), 1), (.character("A"), 1), (.character("é"), 2), (.code(48), 1)]
private let layout = KeyboardLayout(name: "Test", strokes: ["a": [(0, [], "a")], "A": [(0, .maskShift, "A")], "é": [(14, .maskAlternate, ""), (14, [], "é")]])
private let buttons: [CGMouseButton] = [.left, .right, .center]
private let pressureStep: Double = 1.0 / 255
private let tiltStep: Double = 1.0 / 32767

// --- [MODELS] --------------------------------------------------------------------------

struct Draw: Sendable {
    let variant: Int
    let hold: Int
    let count: Int
    let extra: Int?
    let jitter: Int
    let points: [SIMD2<Double>]
    let modifiers: Set<Modifier>
}

struct Host: Sendable {
    let interval: Duration
    let delay: Duration
    let repeatInterval: Duration
    let rate: Int
    let first: Int64
    let seed: UInt64
}

private let draws = zip(
    Gen.int(in: 0...11),
    Gen.int(in: 0...800),
    Gen.int(in: 1...4),
    Gen.int(in: 0...600).optional,
    Gen.int(in: 0...120),
    Gen.double(in: -400...400).simd2.array(of: 2...10),
    Gen<Modifier>.case.set(ofAtMost: 0...3),
)
.map { (draw: (Int, Int, Int, Int?, Int, [SIMD2<Double>], Set<Modifier>)) -> Draw in
    Draw(variant: draw.0, hold: draw.1, count: draw.2, extra: draw.3, jitter: draw.4, points: draw.5, modifiers: draw.6)
}

private let hosts = zip(Gen.int(in: 100...900), Gen.int(in: 100...600), Gen.int(in: 15...100), Gen.int(in: 0...160), Gen.int64(in: 0...10_000), Gen.uint64())
    .map { (host: (Int, Int, Int, Int, Int64, UInt64)) -> Host in
        Host(interval: .milliseconds(host.0), delay: .milliseconds(host.1), repeatInterval: .milliseconds(host.2), rate: host.3, first: host.4, seed: host.5)
    }

// --- [OPERATIONS] ----------------------------------------------------------------------

private func base(
    _ contact: Input.Contact = .button(.left),
    points: [SIMD2<Double>] = [SIMD2(10, 20)],
    pressures: [Double?] = [nil],
    hold: Duration = .milliseconds(30),
    count: Int = 1,
    every: Duration? = nil,
    jitter: Duration = .zero,
) -> (contact: Input.Contact, path: [Input.Sample], hold: Duration, schedule: Input.Schedule) {
    let path: [Input.Sample] = zip(points, pressures).map { (at: SIMD2<Double>, pressure: Double?) -> Input.Sample in Input.Sample(at: at, pressure: pressure, tilt: nil) }
    return (contact, path, hold, Input.Schedule(count: count, every: every, jitter: jitter))
}

private func input(_ shape: Shape, _ draw: Draw) throws(ApplicationFailure) -> Input {
    let hold: Duration = .milliseconds(draw.hold)
    let every: Duration? = draw.extra.map { (extra: Int) -> Duration in .milliseconds(draw.hold + draw.jitter + extra) }
    let run = Input.Schedule(count: draw.count, every: every, jitter: every == nil ? .zero : .milliseconds(draw.jitter))
    let single = Input.Schedule(count: 1, every: nil, jitter: .zero)
    let point: [Input.Sample] = [Input.Sample(at: draw.points[0], pressure: nil, tilt: nil)]
    let line: [Input.Sample] = draw.points.map { (at: SIMD2<Double>) -> Input.Sample in Input.Sample(at: at, pressure: nil, tilt: nil) }
    let pen: [Input.Sample] = draw.points.enumerated().map { (index: Int, at: SIMD2<Double>) -> Input.Sample in
        let tilt: SIMD2<Double> = SIMD2(0.3, -0.2) * Double(index % 3)
        return Input.Sample(at: at, pressure: Double(index + 1) / Double(draw.points.count), tilt: tilt)
    }
    let button: CGMouseButton = buttons[draw.variant % buttons.count]
    let unit: CGScrollEventUnit = draw.variant.isMultiple(of: 2) ? .pixel : .line
    return switch shape {
        case .clicks: try Input(contact: .button(button), path: point, hold: hold, modifiers: draw.modifiers, schedule: run)
        case .stroke: try Input(contact: .button(.left), path: pen, hold: hold, modifiers: draw.modifiers, schedule: single)
        case .drag: try Input(contact: .button(button), path: line, hold: hold, modifiers: draw.modifiers, schedule: single)
        case .hover: try Input(contact: .hover, path: line, hold: hold, modifiers: draw.modifiers, schedule: single)
        case .wheel: try Input(contact: .wheel(by: draw.points[1], unit: unit), path: point, hold: hold, modifiers: draw.modifiers, schedule: run)
        case .key: try Input(contact: .key(keys[draw.variant % keys.count].key), path: [], hold: hold, modifiers: draw.modifiers, schedule: run)
    }
}

private func timeline(of input: Input, _ host: Host) throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] {
    var generator = Xoshiro(seed: (host.seed, 1, 2, 3))
    return try input.timeline(doubleClick: host.interval, keyRepeat: (host.delay, host.repeatInterval), frames: host.rate, layout: layout, numbering: host.first, using: &generator)
}

private func frames(of input: Input, rate: Int) -> Int {
    let seconds: Double = input.hold / .seconds(1)
    return max(1, Int((seconds * Double(rate)).rounded(.down)))
}

private func along(_ points: [SIMD2<Double>], fraction: Double) -> SIMD2<Double> {
    let lengths: [Double] = zip(points, points.dropFirst()).map { (from: SIMD2<Double>, to: SIMD2<Double>) -> Double in
        let step: SIMD2<Double> = to - from
        return (step * step).sum().squareRoot()
    }
    let ends: [Double] = lengths.indices.map { (index: Int) -> Double in lengths[...index].reduce(0, +) }
    let target: Double = (ends.last ?? 0) * fraction
    let index: Int = ends.firstIndex { (end: Double) -> Bool in end >= target } ?? lengths.count - 1
    let start: Double = index == 0 ? 0 : ends[index - 1]
    let share: Double = lengths[index] > 0 ? (target - start) / lengths[index] : 1
    let step: SIMD2<Double> = points[index + 1] - points[index]
    return points[index] + step * share
}

private func field(_ field: CGEventField, of events: [CGEvent]) -> [Int64] { events.map { (event: CGEvent) -> Int64 in event.getIntegerValueField(field) } }

private func events(_ timed: [(offset: Duration, event: CGEvent)], among types: Set<CGEventType>) -> [(offset: Duration, event: CGEvent)] {
    timed.filter { (_: Duration, event: CGEvent) -> Bool in types.contains(event.type) }
}

private let refusals: [(contact: Input.Contact, path: [Input.Sample], hold: Duration, schedule: Input.Schedule)] = [
    base(count: 0), base(hold: .milliseconds(-1)), base(count: 2, every: .milliseconds(20)), base(count: 2, jitter: .milliseconds(10)), base(points: [], pressures: []),
    base(points: [SIMD2(10, 20), SIMD2(30, 5)], pressures: [nil, nil], count: 2, every: .milliseconds(200)),
    base(.wheel(by: SIMD2(0, 40), unit: .pixel), points: [SIMD2(10, 20), SIMD2(30, 5)], pressures: [nil, nil]), base(.wheel(by: SIMD2(0, 1e10), unit: .pixel)),
    base(.button(.right), pressures: [0.5]), base(pressures: [1.5]), base(points: [SIMD2(10, 20), SIMD2(30, 5)], pressures: [0.5, nil]), base(points: [SIMD2(.nan, 20)]),
]

@Suite
struct InputTests {
    @Test
    func refusesNoneOfTheBaseInput() throws {
        let row = base()
        let made = try Input(contact: row.contact, path: row.path, hold: row.hold, modifiers: [], schedule: row.schedule)
        expectNoDifference(made.path.count, 1)
    }

    @Test(arguments: refusals)
    func refusesAnInputOneRuleFromTheBase(contact: Input.Contact, path: [Input.Sample], hold: Duration, schedule: Input.Schedule) {
        let refusal = #expect(throws: ApplicationFailure.self) { try Input(contact: contact, path: path, hold: hold, modifiers: [], schedule: schedule) }
        expectNoDifference(refusal?.reason, .refused(nil))
    }

    @Test(arguments: [(nil, [1, 2, 3]), (.milliseconds(600), [1, 1, 1])] as [(Duration?, [Int64])])
    func runsClicksAtOnePointUpToTheDoubleClickInterval(every: Duration?, states: [Int64]) throws {
        let row = base(points: [SIMD2(40, 25)], count: 3, every: every)
        let clicks = try Input(contact: row.contact, path: row.path, hold: row.hold, modifiers: [], schedule: row.schedule)
        var generator = Xoshiro(seed: (1, 2, 3, 4))
        let timed = try clicks.timeline(doubleClick: .milliseconds(500), keyRepeat: (.milliseconds(250), .milliseconds(33)), frames: 120, layout: layout, numbering: 7, using: &generator)
        expectNoDifference(field(.mouseEventClickState, of: events(timed, among: downs).map(\.event)), states)
    }

    @Test(arguments: [Shape.clicks, .stroke, .drag])
    func pairsEveryPressWithOneReleaseOfItsEventNumber(shape: Shape) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let pressing = try input(shape, draw)
            let presses: [CGEvent] = events(try timeline(of: pressing, host), among: downs.union(ups)).map(\.event)
            let order: [Bool] = presses.map { (event: CGEvent) -> Bool in downs.contains(event.type) }
            let numbers: [Int64] = (0..<Int64(pressing.schedule.count)).flatMap { (press: Int64) -> [Int64] in [host.first + press, host.first + press] }
            expectNoDifference(order, Array(repeating: [true, false], count: pressing.schedule.count).flatMap(\.self))
            expectNoDifference(field(.mouseEventNumber, of: presses), numbers)
        }
    }

    @Test
    func raisesClickStateOnlyWithinTheDoubleClickInterval() async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let timed = try timeline(of: input(.clicks, draw), host)
            let pressed: [(offset: Duration, event: CGEvent)] = events(timed, among: downs)
            let released: [CGEvent] = events(timed, among: ups).map(\.event)
            let gaps: [Duration] = zip(pressed.dropFirst(), pressed).map { (later: (offset: Duration, event: CGEvent), earlier: (offset: Duration, event: CGEvent)) -> Duration in
                later.offset - earlier.offset
            }
            let states: [Int64] = gaps.reduce(into: [Int64(1)]) { (states: inout [Int64], gap: Duration) in
                let chained: Bool = gap < host.interval
                states.append(chained ? states[states.count - 1] + 1 : 1)
            }
            expectNoDifference(field(.mouseEventClickState, of: pressed.map(\.event)), states)
            expectNoDifference(field(.mouseEventClickState, of: released), states)
        }
    }

    @Test
    func spacesPressesByTheScheduleAndHoldsEachForTheHold() async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let clicks = try input(.clicks, draw)
            let timed = try timeline(of: clicks, host)
            let pressed: [Duration] = events(timed, among: downs).map(\.offset)
            let released: [Duration] = events(timed, among: ups).map(\.offset)
            let gaps: [Duration] = zip(pressed.dropFirst(), pressed).map { (later: Duration, earlier: Duration) -> Duration in later - earlier }
            let held: [Duration] = zip(released, pressed).map { (up: Duration, down: Duration) -> Duration in up - down }
            let (hold, jitter): (Duration, Duration) = (clicks.hold, clicks.schedule.jitter)
            let scheduled: Bool = gaps.allSatisfy { (gap: Duration) -> Bool in
                guard let every = clicks.schedule.every else { return gap >= hold && (hold >= host.interval || gap < host.interval) }
                return gap >= every - jitter && gap <= every + jitter
            }
            #expect(scheduled)
            expectNoDifference(held, Array(repeating: hold, count: pressed.count))
        }
    }

    @Test(arguments: Shape.allCases)
    func neverStepsBackInTime(shape: Shape) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let offsets: [Duration] = try timeline(of: input(shape, draw), host).map(\.offset)
            expectNoDifference(offsets, offsets.sorted())
            expectNoDifference(offsets.first, .zero)
        }
    }

    @Test
    func wrapsAPressurePathInPenProximity() async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let stroke = try input(.stroke, draw)
            let touched: [CGEvent] = try timeline(of: stroke, host).map(\.event).filter { (event: CGEvent) -> Bool in event.type != .flagsChanged }
            let touching: [CGEvent] = Array(touched.dropFirst().dropLast())
            let (enter, leave): (CGEvent, CGEvent) = (try #require(touched.first), try #require(touched.last))
            let (down, up): (CGEvent, CGEvent) = (try #require(touching.first), try #require(touching.last))
            let first: Input.Sample = try #require(stroke.path.first)
            let pressure: Double = try #require(first.pressure)
            let tilt: SIMD2<Double> = try #require(first.tilt)
            expectNoDifference([enter.type, leave.type], [.tabletProximity, .tabletProximity])
            expectNoDifference(field(.tabletProximityEventEnterProximity, of: [enter, leave]), [1, 0])
            expectNoDifference(enter.getIntegerValueField(.tabletProximityEventPointerType), 1)
            expectNoDifference(Set(field(.mouseEventSubtype, of: touching)), [1])
            let pressed: Double = abs(down.getDoubleValueField(.mouseEventPressure) - pressure)
            let tilted: Double = abs(down.getDoubleValueField(.tabletEventTiltX) - tilt.x)
            #expect(pressed <= pressureStep)
            #expect(tilted <= tiltStep)
            expectNoDifference(up.getDoubleValueField(.mouseEventPressure), 0)
        }
    }

    @Test(arguments: Shape.allCases)
    func bracketsHeldModifiersWithFlagsChanged(shape: Shape) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let posted: [CGEvent] = try timeline(of: input(shape, draw), host).map(\.event)
            let held: [(code: Int64, flag: CGEventFlags)] = draw.modifiers.compactMap { (modifier: Modifier) -> (code: Int64, flag: CGEventFlags)? in modifierKeys[modifier] }
            let flags = CGEventFlags(held.map(\.flag))
            let codes: Set<Int64> = Set(held.map(\.code))
            let (opening, closing): ([CGEvent], [CGEvent]) = (Array(posted.prefix(held.count)), Array(posted.suffix(held.count)))
            let inner: [CGEvent] = Array(posted.dropFirst(held.count).dropLast(held.count))
            expectNoDifference(Set((opening + closing).map(\.type)).subtracting([.flagsChanged]), [])
            expectNoDifference(Set(field(.keyboardEventKeycode, of: opening)), codes)
            expectNoDifference(Set(field(.keyboardEventKeycode, of: closing)), codes)
            #expect(inner.allSatisfy { (event: CGEvent) -> Bool in event.flags.contains(flags) })
            #expect(closing.last.map { (event: CGEvent) -> Bool in event.flags.isDisjoint(with: flags) } ?? true)
        }
    }

    @Test
    func repeatsAHeldKeyAsPlainKeyDowns() async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let held = try input(.key, draw)
            let posted: [CGEvent] = try timeline(of: held, host).map(\.event)
            let strokes: Int = keys[draw.variant % keys.count].strokes
            let ratio: Double = (held.hold - host.delay) / host.repeatInterval
            let repeats: Int = held.hold < host.delay ? 0 : Int(ratio) + 1
            expectNoDifference(posted.count { (event: CGEvent) -> Bool in event.type == .keyDown }, held.schedule.count * (strokes + repeats))
            expectNoDifference(posted.count { (event: CGEvent) -> Bool in event.type == .keyUp }, held.schedule.count * strokes)
            expectNoDifference(Set(field(.keyboardEventAutorepeat, of: posted)), [0])
        }
    }

    @Test(arguments: [Shape.stroke, .drag, .hover])
    func spacesMovesAlongThePathAtTheFrameRate(shape: Shape) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let moving = try input(shape, draw)
            let moved: [(offset: Duration, event: CGEvent)] = Array(events(try timeline(of: moving, host), among: moves).dropFirst(shape == .hover ? 1 : 0))
            let count: Int = frames(of: moving, rate: host.rate)
            let points: [SIMD2<Double>] = (1...count).map { (frame: Int) -> SIMD2<Double> in along(moving.path.map(\.at), fraction: Double(frame) / Double(count)) }
            let reached: [SIMD2<Double>] = moved.map { (_: Duration, event: CGEvent) -> SIMD2<Double> in SIMD2(event.location.x, event.location.y) }
            let misses: [Double] = zip(reached, points).map { (at: SIMD2<Double>, point: SIMD2<Double>) -> Double in
                let miss: SIMD2<Double> = at - point
                return (miss * miss).sum()
            }
            let spacing: Duration = host.rate == 0 ? .zero : .seconds(1) / host.rate - .nanoseconds(1)
            let offsets: [Duration] = moved.map(\.offset)
            let gaps: [Duration] = zip(offsets.dropFirst(), offsets).map { (later: Duration, earlier: Duration) -> Duration in later - earlier }
            expectNoDifference(moved.count, count)
            #expect(misses.allSatisfy { (miss: Double) -> Bool in miss < 1e-12 })
            #expect(gaps.allSatisfy { (gap: Duration) -> Bool in gap >= spacing })
        }
    }

    @Test
    func spreadsTheWheelDeltaAcrossEachPress() async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let wheel = try input(.wheel, draw)
            let scrolled: [CGEvent] = try timeline(of: wheel, host).map(\.event).filter { (event: CGEvent) -> Bool in event.type == .scrollWheel }
            let pixel: Bool = draw.variant.isMultiple(of: 2)
            let count: Int = frames(of: wheel, rate: host.rate)
            let each: Int = count + (pixel ? 1 : 0)
            let axes: (vertical: CGEventField, horizontal: CGEventField) =
                pixel ? (.scrollWheelEventPointDeltaAxis1, .scrollWheelEventPointDeltaAxis2) : (.scrollWheelEventDeltaAxis1, .scrollWheelEventDeltaAxis2)
            let presses: [[CGEvent]] = stride(from: 0, to: scrolled.count, by: each).map { (start: Int) -> [CGEvent] in Array(scrolled[start..<min(start + each, scrolled.count)]) }
            let changed: [Int64] = Array(repeating: 2, count: count - 1)
            let phases: [Int64] = pixel ? [1] + changed + [4] : Array(repeating: 0, count: count)
            let vertical: [Int64] = presses.map { (press: [CGEvent]) -> Int64 in field(axes.vertical, of: press).reduce(0, +) }
            let horizontal: [Int64] = presses.map { (press: [CGEvent]) -> Int64 in field(axes.horizontal, of: press).reduce(0, +) }
            expectNoDifference(scrolled.count, wheel.schedule.count * each)
            expectNoDifference(vertical, Array(repeating: Int64(draw.points[1].y.rounded()), count: presses.count))
            expectNoDifference(horizontal, Array(repeating: Int64(draw.points[1].x.rounded()), count: presses.count))
            expectNoDifference(presses.map { (press: [CGEvent]) -> [Int64] in field(.scrollWheelEventScrollPhase, of: press) }, Array(repeating: phases, count: presses.count))
        }
    }
}
