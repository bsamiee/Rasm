import AppKit
import CasePathsCore
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
private let layout = KeyboardLayout(
    strokes: [
        []: [
            "a": [Stroke(code: 0, flags: [], text: "a")], "A": [Stroke(code: 0, flags: .maskShift, text: "A")],
            "é": [Stroke(code: 14, flags: .maskAlternate, text: ""), Stroke(code: 14, flags: [], text: "é")],
        ]
    ]
)
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
    let frame: Duration
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

private let hosts = zip(Gen.int(in: 100...900), Gen.int(in: 100...600), Gen.int(in: 15...100), Gen.int(in: 4...34), Gen.int64(in: 0...10_000), Gen.uint64())
    .map { (host: (Int, Int, Int, Int, Int64, UInt64)) -> Host in
        Host(interval: .milliseconds(host.0), delay: .milliseconds(host.1), repeatInterval: .milliseconds(host.2), frame: .milliseconds(host.3), first: host.4, seed: host.5)
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
    edges: Set<Input.Edge> = [.down, .up],
) -> (contact: Input.Contact, path: [Input.Sample], hold: Duration, schedule: Input.Schedule, edges: Set<Input.Edge>) {
    let path: [Input.Sample] = zip(points, pressures).map { (at: SIMD2<Double>, pressure: Double?) -> Input.Sample in Input.Sample(at: at, pressure: pressure, tilt: nil) }
    return (contact, path, hold, Input.Schedule(count: count, every: every, jitter: jitter), edges)
}

private func input(_ shape: Shape, _ draw: Draw, edges: Set<Input.Edge> = [.down, .up]) throws(ApplicationFailure) -> Input {
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
    let by: SIMD2<Double> = draw.points[1] + SIMD2(0, draw.points[1].y < 0 ? -1 : 1)
    return switch shape {
        case .clicks: try Input(contact: .button(button), path: point, hold: hold, modifiers: draw.modifiers, schedule: run, edges: edges)
        case .stroke: try Input(contact: .button(.left), path: pen, hold: hold, modifiers: draw.modifiers, schedule: single, edges: edges)
        case .drag: try Input(contact: .button(button), path: line, hold: hold, modifiers: draw.modifiers, schedule: single, edges: edges)
        case .hover: try Input(contact: .hover, path: line, hold: hold, modifiers: draw.modifiers, schedule: single, edges: edges)
        case .wheel: try Input(contact: .wheel(by: by, unit: unit), path: point, hold: hold, modifiers: draw.modifiers, schedule: run, edges: edges)
        case .key: try Input(contact: .key(keys[draw.variant % keys.count].key), path: [], hold: hold, modifiers: draw.modifiers, schedule: run, edges: edges)
    }
}

private func timeline(of input: Input, _ host: Host) throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] {
    var generator = Xoshiro(seed: (host.seed, 1, 2, 3))
    return try input.timeline(doubleClick: host.interval, keyRepeat: (host.delay, host.repeatInterval), frame: host.frame, layout: layout, numbering: (host.first, 1), using: &generator)
}

private func at(_ event: CGEvent) -> SIMD2<Double> { SIMD2(event.location.x, event.location.y) }

private func field(_ field: CGEventField, of events: [CGEvent]) -> [Int64] { events.map { (event: CGEvent) -> Int64 in event.getIntegerValueField(field) } }

private func events(_ timed: [(offset: Duration, event: CGEvent)], among types: Set<CGEventType>) -> [(offset: Duration, event: CGEvent)] {
    timed.filter { (_: Duration, event: CGEvent) -> Bool in types.contains(event.type) }
}

private let refusals: [(contact: Input.Contact, path: [Input.Sample], hold: Duration, schedule: Input.Schedule, edges: Set<Input.Edge>)] = [
    base(count: 0), base(hold: .milliseconds(-1)), base(count: 2, every: .milliseconds(20)), base(count: 2, jitter: .milliseconds(10)), base(points: [], pressures: []),
    base(points: [SIMD2(10, 20), SIMD2(30, 5)], pressures: [nil, nil], count: 2, every: .milliseconds(200)),
    base(.wheel(by: SIMD2(0, 40), unit: .pixel), points: [SIMD2(10, 20), SIMD2(30, 5)], pressures: [nil, nil]), base(.wheel(by: SIMD2(0, 1e10), unit: .pixel)),
    base(.button(.right), pressures: [0.5]), base(pressures: [1.5]), base(points: [SIMD2(10, 20), SIMD2(30, 5)], pressures: [0.5, nil]), base(points: [SIMD2(.nan, 20)]),
    base(.key(.code(48)), points: [], pressures: [], edges: []), base(.wheel(by: SIMD2(0.3, -0.4), unit: .line)), base(points: [SIMD2(1e10, 20)]),
]

private let edgeSets: [Set<Input.Edge>] = [[], [.down], [.up], [.down, .up]]

@Suite
struct InputTests {
    @Test
    func refusesNoneOfTheBaseInput() throws {
        let row = base()
        let made = try Input(contact: row.contact, path: row.path, hold: row.hold, modifiers: [], schedule: row.schedule, edges: row.edges)
        expectNoDifference(made.path.count, 1)
    }

    @Test(arguments: refusals)
    func refusesAnInputOneRuleFromTheBase(contact: Input.Contact, path: [Input.Sample], hold: Duration, schedule: Input.Schedule, edges: Set<Input.Edge>) {
        let refusal = #expect(throws: ApplicationFailure.self) { try Input(contact: contact, path: path, hold: hold, modifiers: [], schedule: schedule, edges: edges) }
        expectNoDifference(refusal?.reason, .refused)
    }

    @Test(arguments: [(nil, [1, 2, 3]), (.milliseconds(600), [1, 1, 1])] as [(Duration?, [Int64])])
    func runsClicksAtOnePointUpToTheDoubleClickInterval(every: Duration?, states: [Int64]) throws {
        let row = base(points: [SIMD2(40, 25)], count: 3, every: every)
        let clicks = try Input(contact: row.contact, path: row.path, hold: row.hold, modifiers: [], schedule: row.schedule, edges: row.edges)
        var generator = Xoshiro(seed: (1, 2, 3, 4))
        let timed = try clicks.timeline(
            doubleClick: .milliseconds(500),
            keyRepeat: (.milliseconds(250), .milliseconds(33)),
            frame: .seconds(1) / 120,
            layout: layout,
            numbering: (7, 1),
            using: &generator,
        )
        expectNoDifference(field(.mouseEventClickState, of: events(timed, among: downs).map(\.event)), states)
    }

    @Test
    func releasesTheHeldPressWithItsNumberAndClickState() throws {
        let row = base(points: [SIMD2(40, 25)], edges: [.up])
        let release = try Input(contact: row.contact, path: row.path, hold: row.hold, modifiers: [], schedule: row.schedule, edges: row.edges)
        var generator = Xoshiro(seed: (1, 2, 3, 4))
        let timed = try release.timeline(
            doubleClick: .milliseconds(500),
            keyRepeat: (.milliseconds(250), .milliseconds(33)),
            frame: .seconds(1) / 120,
            layout: layout,
            numbering: (9, 2),
            using: &generator,
        )
        let released: [CGEvent] = events(timed, among: ups).map(\.event)
        expectNoDifference(events(timed, among: downs).count, 0)
        expectNoDifference(field(.mouseEventClickState, of: released), [2])
        expectNoDifference(field(.mouseEventNumber, of: released), [9])
    }

    @Test(arguments: edgeSets)
    func postsOnlyTheEdgesAStrokeHolds(edges: Set<Input.Edge>) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let posted: [CGEvent] = try timeline(of: input(.stroke, draw, edges: edges), host).map(\.event)
            let (down, up): (Int, Int) = (edges.contains(.down) ? 1 : 0, edges.contains(.up) ? 1 : 0)
            let proximity: [Int64] = field(.tabletProximityEventEnterProximity, of: posted.filter { (event: CGEvent) -> Bool in event.type == .tabletProximity })
            expectNoDifference(posted.count { (event: CGEvent) -> Bool in event.type == .leftMouseDown }, down)
            expectNoDifference(posted.count { (event: CGEvent) -> Bool in event.type == .leftMouseUp }, up)
            expectNoDifference(proximity, Array(repeating: 1, count: down) + Array(repeating: 0, count: up))
            expectNoDifference(posted.count { (event: CGEvent) -> Bool in event.type == .flagsChanged }, draw.modifiers.count * (down + up))
        }
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
            expectNoDifference([at(enter), at(leave)], [first.at, try #require(stroke.path.last).at])
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
    func movesThroughEveryPointOfThePathNoTwoMovesInsideOneFrame(shape: Shape) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let moving = try input(shape, draw)
            let pointed: [(offset: Duration, event: CGEvent)] = events(try timeline(of: moving, host), among: downs.union(moves))
            let reached: [SIMD2<Double>] = pointed.map { (_: Duration, event: CGEvent) -> SIMD2<Double> in at(event) }
            let visits: [Int?] = moving.path.map { (sample: Input.Sample) -> Int? in
                reached.firstIndex { (point: SIMD2<Double>) -> Bool in ((point - sample.at) * (point - sample.at)).sum() < 1e-12 }
            }
            let offsets: [Duration] = pointed.map(\.offset)
            let gaps: [Duration] = zip(offsets.dropFirst(), offsets).map { (later: Duration, earlier: Duration) -> Duration in later - earlier }
            #expect(visits.allSatisfy { (visit: Int?) -> Bool in visit != nil }, "path \(moving.path.map(\.at)) against moves \(reached)")
            expectNoDifference(visits.compactMap(\.self), visits.compactMap(\.self).sorted())
            #expect(gaps.allSatisfy { (gap: Duration) -> Bool in gap >= host.frame - .nanoseconds(1) }, "gaps \(gaps) under the frame \(host.frame)")
        }
    }

    @Test(arguments: [Shape.stroke, .drag, .hover])
    func carriesThePointerDeltaOfEachMoveSummingToThePath(shape: Shape) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let moving = try input(shape, draw)
            let pointed: [CGEvent] = events(try timeline(of: moving, host), among: downs.union(moves).union(ups)).map(\.event)
            let ends: (first: SIMD2<Double>, last: SIMD2<Double>) = (try #require(moving.path.first).at, try #require(moving.path.last).at)
            let travelled: SIMD2<Double> = ends.last.rounded(.toNearestOrAwayFromZero) - ends.first.rounded(.toNearestOrAwayFromZero)
            let summed: [Int64] = [field(.mouseEventDeltaX, of: pointed).reduce(0, +), field(.mouseEventDeltaY, of: pointed).reduce(0, +)]
            expectNoDifference(summed, [Int64(travelled.x), Int64(travelled.y)])
        }
    }

    @Test(arguments: [Shape.clicks, .drag])
    func pressesAMouseButtonAtFullPressureAndReleasesItAtNone(shape: Shape) async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let posted: [CGEvent] = try timeline(of: input(shape, draw), host).map(\.event)
            let pressing: [Double] = posted.filter { (event: CGEvent) -> Bool in downs.union(moves).contains(event.type) }.map { (event: CGEvent) -> Double in
                event.getDoubleValueField(.mouseEventPressure)
            }
            let released: [Double] = posted.filter { (event: CGEvent) -> Bool in ups.contains(event.type) }.map { (event: CGEvent) -> Double in event.getDoubleValueField(.mouseEventPressure) }
            expectNoDifference(Set(pressing), [1])
            expectNoDifference(Set(released), [0])
        }
    }

    @Test
    func pacesPressesOfAKeyAtTheKeyRepeatIntervalWhereNoEveryIsGiven() throws {
        let tab = try Input(contact: .key(.special(.tab)), path: [], hold: .zero, modifiers: [], schedule: Input.Schedule(count: 3, every: nil, jitter: .zero), edges: [.down, .up])
        var generator = Xoshiro(seed: (1, 2, 3, 4))
        let timed = try tab.timeline(
            doubleClick: .milliseconds(500),
            keyRepeat: (.milliseconds(250), .milliseconds(33)),
            frame: .seconds(1) / 120,
            layout: layout,
            numbering: (1, 1),
            using: &generator,
        )
        let pressed: [Duration] = events(timed, among: [.keyDown]).map(\.offset)
        expectNoDifference(zip(pressed.dropFirst(), pressed).map { (later: Duration, earlier: Duration) -> Duration in later - earlier }, [.milliseconds(33), .milliseconds(33)])
    }

    @Test
    func spreadsTheWheelDeltaAcrossEachPressInWholeUnitsAndPhases() async {
        await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
            let wheel = try input(.wheel, draw)
            let by: SIMD2<Double> = try #require(wheel.contact[case: \.wheel]).by
            let scrolled: [CGEvent] = try timeline(of: wheel, host).map(\.event).filter { (event: CGEvent) -> Bool in event.type == .scrollWheel }
            let pixel: Bool = draw.variant.isMultiple(of: 2)
            let each: Int = scrolled.count / wheel.schedule.count
            let axes: (vertical: CGEventField, horizontal: CGEventField) =
                pixel ? (.scrollWheelEventPointDeltaAxis1, .scrollWheelEventPointDeltaAxis2) : (.scrollWheelEventDeltaAxis1, .scrollWheelEventDeltaAxis2)
            let presses: [[CGEvent]] = stride(from: 0, to: scrolled.count, by: max(each, 1)).map { (start: Int) -> [CGEvent] in Array(scrolled[start..<min(start + each, scrolled.count)]) }
            let moving: [[CGEvent]] = presses.map { (press: [CGEvent]) -> [CGEvent] in Array(press.dropLast(pixel ? 1 : 0)) }
            let still: [CGEvent] = moving.flatMap(\.self).filter { (event: CGEvent) -> Bool in event.getIntegerValueField(axes.vertical) == 0 && event.getIntegerValueField(axes.horizontal) == 0 }
            let phases: [[Int64]] = presses.map { (press: [CGEvent]) -> [Int64] in field(.scrollWheelEventScrollPhase, of: press) }
            let expected: [Int64] = pixel ? [1] + Array(repeating: 2, count: max(each - 2, 0)) + [4] : Array(repeating: 0, count: each)
            expectNoDifference(scrolled.count, wheel.schedule.count * each)
            #expect(each <= Int(max(by.x.magnitude, by.y.magnitude).rounded()) + (pixel ? 1 : 0), "\(each) events for a delta of \(by)")
            expectNoDifference(still.count, 0)
            expectNoDifference(presses.map { (press: [CGEvent]) -> Int64 in field(axes.vertical, of: press).reduce(0, +) }, Array(repeating: Int64(by.y.rounded()), count: presses.count))
            expectNoDifference(presses.map { (press: [CGEvent]) -> Int64 in field(axes.horizontal, of: press).reduce(0, +) }, Array(repeating: Int64(by.x.rounded()), count: presses.count))
            expectNoDifference(phases, Array(repeating: expected, count: presses.count))
        }
    }
}
