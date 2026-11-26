import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
import CustomDump
import Encoding
import Fixture
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let pressureStep: Double = 1.0 / 255
private let tiltStep: Double = 1.0 / 32767
private let rate: Int = 60

// --- [OPERATIONS] ----------------------------------------------------------------------

private func post(_ input: (SIMD2<Double>) throws(ApplicationFailure) -> Input, through route: (CGWindowID) -> Route, to fixture: FixtureSession) async throws -> [Duration] {
    try await fixture.send(.report)
    let region: Report.Control = try #require(try await fixture.next(\.report).value.controls.first { control in control.entry == .region })
    let (window, frame): (Int, CGRect) = (try #require(region.window), try #require(region.frame))
    let center: SIMD2<Double> = SIMD2(frame.midX, CGDisplayBounds(CGMainDisplayID()).height - frame.midY)
    let source: CGEventSource = try #require(CGEventSource(stateID: .privateState))
    var generator = SystemRandomNumberGenerator()
    let keyRepeat: (delay: Duration, interval: Duration) = (.seconds(NSEvent.keyRepeatDelay), .seconds(NSEvent.keyRepeatInterval))
    let timeline = try input(center).timeline(
        doubleClick: .seconds(NSEvent.doubleClickInterval),
        keyRepeat: keyRepeat,
        frames: rate,
        layout: KeyboardLayout(name: nil, strokes: [:]),
        firstPress: (1, 1),
        using: &generator,
    )
    try await route(CGWindowID(window)).post(timeline, to: fixture.process, from: source, frame: .seconds(1) / rate, clock: ContinuousClock())
    let instance = try Instance(try #require(NSRunningApplication(processIdentifier: fixture.process)))
    await Captures.current?.take(in: await Watches { _, _ throws(ApplicationFailure) in [] }.watch(instance))
    return timeline.filter { (_: Duration, event: CGEvent) -> Bool in event.type == .leftMouseDown }.map(\.offset)
}

extension WindowServerTests {
    struct RouteTests {
        @Test(
            .enabled(if: AXIsProcessTrusted(), "Posting into the fixture from outside needs this test process to hold the Accessibility grant"),
            arguments: [(nil, [1, 2, 3]), (.milliseconds(600), [1, 1, 1])] as [(Duration?, [Int])],
        )
        func logsTheClickStateOfEachPressOfARunAtOnePointBehindTheOwnersWindows(every: Duration?, states: [Int]) async throws {
            let (pressed, logged): ([Duration], [(down: Bool, clicks: Int, timestamp: TimeInterval)]) = try await FixtureSession.launched { fixture in
                let pressed: [Duration] = try await post(
                    { (point: SIMD2<Double>) throws(ApplicationFailure) -> Input in
                        try Input(
                            contact: .button(.left),
                            path: [Input.Sample(at: point, pressure: nil, tilt: nil)],
                            hold: .milliseconds(30),
                            modifiers: [],
                            schedule: Input.Schedule(count: 3, every: every, jitter: .zero),
                            edges: [.down, .up],
                        )
                    },
                    through: Route.window(number:),
                    to: fixture,
                )
                var logged: [(down: Bool, clicks: Int, timestamp: TimeInterval)] = []
                while logged.count < 2 * states.count {
                    let line = try await fixture.next(\.pointer).value
                    logged.append((line.type == .known(.leftMouseDown), line.clickCount, line.timestamp))
                }
                return (pressed, logged)
            }
            let downs: [(down: Bool, clicks: Int, timestamp: TimeInterval)] = logged.filter(\.down)
            let posted: [Duration] = zip(pressed.dropFirst(), pressed).map { (later: Duration, earlier: Duration) -> Duration in later - earlier }
            let arrived: [Duration] = zip(downs.dropFirst(), downs).map { later, earlier in .seconds(later.timestamp - earlier.timestamp) }
            expectNoDifference(downs.map(\.clicks), states)
            #expect(zip(arrived, posted).allSatisfy { (arrival: Duration, gap: Duration) -> Bool in arrival >= gap }, "arrived \(arrived) against posted \(posted)")
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Posting into the fixture from outside needs this test process to hold the Accessibility grant"))
        func logsAPenStrokeInProximityWithItsPressureAndTiltAndNoActivation() async throws {
            let (entering, leaving, lines): (Bool, Bool, [Line]) = try await FixtureSession.launched { fixture in
                _ = try await post(
                    { (point: SIMD2<Double>) throws(ApplicationFailure) -> Input in
                        let path: [Input.Sample] = [
                            Input.Sample(at: point - SIMD2(40, 0), pressure: 0.2, tilt: SIMD2(0.3, -0.2)), Input.Sample(at: point + SIMD2(40, 0), pressure: 0.8, tilt: SIMD2(0.3, -0.2)),
                        ]
                        return try Input(
                            contact: .button(.left),
                            path: path,
                            hold: .milliseconds(200),
                            modifiers: [],
                            schedule: Input.Schedule(count: 1, every: nil, jitter: .zero),
                            edges: [.down, .up],
                        )
                    },
                    through: Route.window(number:),
                    to: fixture,
                )
                let entered = try await fixture.next(\.proximity)
                let left = try await fixture.next(\.proximity)
                return (entered.value.isEnteringProximity, left.value.isEnteringProximity, entered.before + left.before)
            }
            let strokes = lines.compactMap { (line: Line) in line[case: \.pointer] }
            let drags: [Double] = (1...12).map { (frame: Int) -> Double in 0.2 + 0.6 * Double(frame) / 12 }
            let expected: [Double] = [0.2] + drags + [0]
            let pressures: [Double] = strokes.map { stroke in Double(stroke.pressure) }
            let tilts: [CGPoint] = strokes.dropLast().compactMap(\.tilt)
            expectNoDifference([entering, leaving], [true, false])
            expectNoDifference(strokes.map(\.type), [.known(.leftMouseDown)] + Array(repeating: .known(.leftMouseDragged), count: 12) + [.known(.leftMouseUp)])
            expectNoDifference(Set(strokes.map(\.subtype)), [.known(.tabletPoint)])
            #expect(
                pressures.count == expected.count && zip(pressures, expected).allSatisfy { (read: Double, posted: Double) -> Bool in abs(read - posted) <= pressureStep },
                "pressures \(pressures)",
            )
            #expect(tilts.count == strokes.count - 1 && tilts.allSatisfy { tilt in abs(tilt.x - 0.3) <= tiltStep && abs(tilt.y + 0.2) <= tiltStep }, "tilts \(tilts)")
            expectNoDifference(lines.compactMap { (line: Line) in line[case: \.active] }, [])
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Posting into the fixture from outside needs this test process to hold the Accessibility grant"))
        func bracketsTheFocusedRouteWithTheActivationAndDeactivationTheApplicationLogs() async throws {
            let (opened, closed, between): (Bool, Bool, [Line]) = try await FixtureSession.launched { fixture in
                _ = try await post(
                    { (point: SIMD2<Double>) throws(ApplicationFailure) -> Input in
                        try Input(
                            contact: .button(.left),
                            path: [Input.Sample(at: point, pressure: nil, tilt: nil)],
                            hold: .milliseconds(30),
                            modifiers: [],
                            schedule: Input.Schedule(count: 1, every: nil, jitter: .zero),
                            edges: [.down, .up],
                        )
                    },
                    through: Route.focused(number:),
                    to: fixture,
                )
                let first = try await fixture.next(\.active)
                let second = try await fixture.next(\.active)
                return (first.value, second.value, second.before)
            }
            expectNoDifference([opened, closed], [true, false])
            expectNoDifference(between.compactMap { (line: Line) in line[case: \.pointer]?.type }, [.known(.leftMouseDown), .known(.leftMouseUp)])
        }
    }
}
