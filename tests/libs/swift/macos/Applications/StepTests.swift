import AppKit
import Carbon.HIToolbox
import CasePathsCore
import CustomDump
import Fixture
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

private func steps(_ text: String) throws -> [Step] { try JSONDecoder().decode([Step].self, from: Data(text.utf8)) }

private func enabledChords() -> [(code: CGKeyCode, modifiers: Set<Modifier>)] {
    var listed: Unmanaged<CFArray>?
    let rows: [[String: Any]] = unsafe CopySymbolicHotKeys(&listed) == noErr ? unsafe listed?.takeRetainedValue() as? [[String: Any]] ?? [] : []
    return rows.compactMap { (row: [String: Any]) -> (code: CGKeyCode, modifiers: Set<Modifier>)? in
        guard row[kHISymbolicHotKeyEnabled as String] as? Bool == true, let code = row[kHISymbolicHotKeyCode as String] as? Int, let state = row[kHISymbolicHotKeyModifiers as String] as? Int else {
            return nil
        }
        let function: Set<Modifier> = state & kEventKeyModifierFnMask != 0 ? [.function] : []
        return (CGKeyCode(code), function.union(Modifier.allCases.filter { modifier in state & Int(modifier.key.state) << 8 != 0 }))
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite
struct StepTests {
    @Test
    func readsEachStepShapeTheTypeScriptSideWritesIntoTheActionItsFieldsDecide() throws {
        let read: [Step.Action] = try steps(
            #"""
            [{"at":3},{"at":3,"action":"AXShowMenu"},{"at":4,"to":"Tabloid","from":"process"},{"key":"a","modifiers":["command"],"at":5,"hold":250,"edge":"down"},
             {"pointer":7,"path":[[10,20,0.5],[30,40,0.5,0.1,-0.1]],"count":2,"every":300},{"pointer":[1,2],"by":[0,-3],"unit":"line"},{"pointer":[1,2],"button":"none","windows":[9]},{"pointer":[1,2],"button":"middle"}]
            """#
        ).map(\.action)
        let both: Set<Input.Edge> = [.down, .up]
        let single = Input.Schedule(count: 1, every: nil, jitter: .zero)
        let keyed: [Step.Action] = [
            Step.Action.press(ref: 3, action: kAXPressAction),
            Step.Action.press(ref: 3, action: kAXShowMenuAction),
            Step.Action.set(ref: 4, value: .text("Tabloid")),
            Step.Action.input(contact: .key(.character("a")), path: [], focus: 5, hold: .milliseconds(250), modifiers: [.command], schedule: single, edges: [.down], windows: []),
        ]
        let pointed: [Step.Action] = [
            Step.Action.input(
                contact: .button(.left),
                path: [
                    .element(ref: 7), .point(Input.Sample(at: SIMD2<Double>(10, 20), pressure: 0.5, tilt: nil)),
                    .point(Input.Sample(at: SIMD2<Double>(30, 40), pressure: 0.5, tilt: SIMD2<Double>(0.1, -0.1))),
                ],
                focus: nil,
                hold: .zero,
                modifiers: [],
                schedule: Input.Schedule(count: 2, every: .milliseconds(300), jitter: .zero),
                edges: both,
                windows: [],
            ),
            Step.Action.input(
                contact: .wheel(by: SIMD2<Double>(0, -3), unit: .line),
                path: [.point(Input.Sample(at: SIMD2<Double>(1, 2), pressure: nil, tilt: nil))],
                focus: nil,
                hold: .zero,
                modifiers: [],
                schedule: single,
                edges: both,
                windows: [],
            ),
            Step.Action.input(
                contact: .hover,
                path: [.point(Input.Sample(at: SIMD2<Double>(1, 2), pressure: nil, tilt: nil))],
                focus: nil,
                hold: .zero,
                modifiers: [],
                schedule: single,
                edges: both,
                windows: [9],
            ),
            Step.Action.input(
                contact: .button(.center),
                path: [.point(Input.Sample(at: SIMD2<Double>(1, 2), pressure: nil, tilt: nil))],
                focus: nil,
                hold: .zero,
                modifiers: [],
                schedule: single,
                edges: both,
                windows: [],
            ),
        ]
        let expected: [Step.Action] = keyed + pointed
        expectNoDifference(read, expected)
        expectNoDifference(try steps(#"[{"at":4,"to":"Tabloid","from":"process"}]"#).map(\.from), [.process])
    }

    @Test(arguments: [#"[{"action":"AXPress"}]"#, #"[{"pointer":[1,2,3,4]}]"#])
    func refusesAStepNamingNoTargetOrAPointOfNoKnownLength(text: String) {
        #expect(throws: DecodingError.self) { try steps(text) }
    }

    @Test
    func refusesEveryMalformedStepOfABatchTogetherBeforeAnyRuns() async throws {
        let batch: [Step] = try steps(#"[{"key":"a"},{"pointer":[1,2,0.5],"button":"right"},{"pointer":[1,2],"count":2,"every":10,"hold":50}]"#)
        let watch: Watch = await Watches().watch(try Instance(NSRunningApplication.current), driven: false)
        let refusal = await #expect(throws: ApplicationFailure.self) { try await watch.act(batch) }
        expectNoDifference(refusal?.reason, .refused)
        let subject: String = refusal?.subject ?? ""
        let named: Bool = subject.contains("step 2") && subject.contains("step 3") && !subject.contains("step 1")
        #expect(named, "\(String(describing: refusal))")
    }

    @Test(.enabled(if: CGPreflightPostEventAccess(), "The system rung checks the PostEvent grant before the shortcut table"))
    func refusesAtTheSystemRungEveryChordTheSystemTakesAsAnEnabledShortcut() async throws {
        let layout = KeyboardLayout(strokes: [:])
        let source: CGEventSource = try #require(CGEventSource(stateID: .privateState))
        var generator = SystemRandomNumberGenerator()
        for chord in enabledChords() {
            let input = try Input(
                contact: .key(.code(chord.code)),
                path: [],
                hold: .zero,
                modifiers: chord.modifiers,
                schedule: Input.Schedule(count: 1, every: nil, jitter: .zero),
                edges: [.down, .up],
            )
            let timeline = try input.timeline(
                doubleClick: .milliseconds(500),
                keyRepeat: (.milliseconds(250), .milliseconds(33)),
                frame: .seconds(1) / 60,
                layout: layout,
                numbering: (1, 1),
                using: &generator,
            )
            let refusal = await #expect(throws: ApplicationFailure.self) { try await Route.system.post(timeline, to: getpid(), from: source, frame: .milliseconds(16), clock: ContinuousClock()) }
            let shortcut: Bool = refusal?.subject.contains("as its own shortcut") == true
            #expect(shortcut, "key code \(chord.code) with \(chord.modifiers): \(String(describing: refusal))")
        }
    }
}

extension WindowServerTests {
    struct ActTests {
        private static func control(_ entry: Inventory, of fixture: FixtureSession, in watch: Watch) async throws -> Applications.Control {
            try await fixture.send(.report)
            let report: Report = try await fixture.next(\.report).value
            let model: Model = try await watch.model(.application)
            let reported: Report.Control = try #require(report.controls.first { control in control.entry == entry })
            let found: Applications.Control? = ModelTests.found(reported, in: model)
            return try #require(found)
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Acting on the fixture from outside needs this test process to hold the Accessibility grant"))
        func endsTheRunAtTheStepWhoseInstanceExitedWithNoLaterStepDelivered() async throws {
            let outcomes: [Result<Step.Delivery, ApplicationFailure>] = try await FixtureSession.launched { fixture in
                let watch: Watch = await Watches().watch(try Instance(try #require(NSRunningApplication(processIdentifier: fixture.process))))
                let apply: Applications.Control = try await Self.control(.action, of: fixture, in: watch)
                let states = await watch.states()
                kill(fixture.process, SIGKILL)
                _ = await states.first { state in state.is(\.exited) }
                let steps: [Step] = try JSONDecoder().decode([Step].self, from: Data(#"[{"at":\#(apply.ref)},{"at":\#(apply.ref)},{"at":\#(apply.ref)}]"#.utf8))
                return try await watch.act(steps).outcomes
            }
            expectNoDifference(outcomes.map { outcome in outcome[case: \.success]?.proof }, [.exit])
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Acting on the fixture from outside needs this test process to hold the Accessibility grant"))
        func closesThePopUpLevelWindowAStepOpenedBehindTheFrontmostApplicationBeforeActReturns() async throws {
            let (outcomes, left): ([Result<Step.Delivery, ApplicationFailure>], [CGWindowID]) = try await FixtureSession.launched { fixture in
                let watch: Watch = await Watches().watch(try Instance(try #require(NSRunningApplication(processIdentifier: fixture.process))))
                let opener: Applications.Control = try await Self.control(.drawer, of: fixture, in: watch)
                let steps: [Step] = try JSONDecoder().decode([Step].self, from: Data(#"[{"at":\#(opener.ref)}]"#.utf8))
                let acted = try await watch.act(steps).outcomes
                let shown: [CGWindowID] = InstanceState.Window.all(.optionOnScreenOnly, of: fixture.process)[fixture.process, default: []]
                    .filter { row in row.layer >= InstanceState.Window.Band.popUpMenu.level }.map(\.number)
                await Captures.current?.take(in: watch)
                return (acted, shown)
            }
            expectNoDifference(left, [])
            expectNoDifference(outcomes.map { outcome in outcome[case: \.success]?.popUps.map(\.closed) }, [[true]])
        }
    }
}
