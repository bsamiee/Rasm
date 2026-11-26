import AppKit
import Carbon.HIToolbox
import CustomDump
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
        return (CGKeyCode(code), Set(Modifier.allCases.filter { modifier in state & Int(modifier.key.state) << 8 != 0 }))
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
             {"pointer":7,"path":[[10,20,0.5],[30,40,0.5,0.1,-0.1]],"count":2,"every":300},{"pointer":[1,2],"by":[0,-3],"unit":"line"},{"pointer":[1,2],"button":"none","windows":[9]}]
            """#
        ).map(\.action)
        let both: Set<Input.Edge> = [.down, .up]
        let single = Input.Schedule(count: 1, every: nil, jitter: .zero)
        let expected: [Step.Action] = [
            .press(ref: 3, action: kAXPressAction),
            .press(ref: 3, action: kAXShowMenuAction),
            .set(ref: 4, value: .text("Tabloid")),
            .input(contact: .key(.character("a")), path: [], focus: 5, hold: .milliseconds(250), modifiers: [.command], schedule: single, edges: [.down], windows: []),
            .input(
                contact: .button(.left),
                path: [.element(ref: 7), .point(Input.Sample(at: SIMD2(10, 20), pressure: 0.5, tilt: nil)), .point(Input.Sample(at: SIMD2(30, 40), pressure: 0.5, tilt: SIMD2(0.1, -0.1)))],
                focus: nil,
                hold: .zero,
                modifiers: [],
                schedule: Input.Schedule(count: 2, every: .milliseconds(300), jitter: .zero),
                edges: both,
                windows: [],
            ),
            .input(
                contact: .wheel(by: SIMD2(0, -3), unit: .line),
                path: [.point(Input.Sample(at: SIMD2(1, 2), pressure: nil, tilt: nil))],
                focus: nil,
                hold: .zero,
                modifiers: [],
                schedule: single,
                edges: both,
                windows: [],
            ),
            .input(contact: .hover, path: [.point(Input.Sample(at: SIMD2(1, 2), pressure: nil, tilt: nil))], focus: nil, hold: .zero, modifiers: [], schedule: single, edges: both, windows: [9]),
        ]
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
        #expect(refusal?.subject.contains("step 2") == true && refusal?.subject.contains("step 3") == true && refusal?.subject.contains("step 1") == false, "\(String(describing: refusal))")
    }

    @Test(.enabled(if: CGPreflightPostEventAccess(), "The system rung checks the PostEvent grant before the shortcut table"))
    func refusesAtTheSystemRungEveryChordTheSystemTakesAsAnEnabledShortcut() async throws {
        let layout = KeyboardLayout(name: nil, strokes: [:])
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
            let timeline = try input.timeline(doubleClick: .milliseconds(500), keyRepeat: (.milliseconds(250), .milliseconds(33)), frames: 60, layout: layout, firstPress: (1, 1), using: &generator)
            let refusal = await #expect(throws: ApplicationFailure.self) { try await Route.system.post(timeline, to: getpid(), from: source, frame: .milliseconds(16), clock: ContinuousClock()) }
            #expect(refusal?.subject.contains("as its own shortcut") == true, "key code \(chord.code) with \(chord.modifiers): \(String(describing: refusal))")
        }
    }
}
