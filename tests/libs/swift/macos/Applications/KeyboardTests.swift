import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import CasePathsCore
import CustomDump
import Encoding
import Fixture
import ScreenCapture
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct KeyboardTests {
    @Test(
        .enabled(if: AXIsProcessTrusted(), "Typing into the fixture from outside needs this test process to hold the Accessibility grant"),
        arguments: [(Inventory.text, "Façade Noël, À É"), (Inventory.measure, "Ça Ø Å")],
    )
    func typesEachCharacterIntoAFocusedFieldThroughTheKeysOfTheCurrentLayout(into entry: Inventory, typing text: String) async throws {
        let (before, after): (JSONValue?, JSONValue?) = try await FixtureSession.launched { fixture in
            let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
            let instance = try Instance(application)
            let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(instance)
            try await fixture.send(.report)
            let shown: JSONValue? = try await fixture.next(\.report).value.controls.first { control in control.entry == entry && control.member == 0 }?.value
            let model: Model = try await watch.model(.application)
            let field: Control = try #require(model.containers.flatMap(\.controls).first { control in shown != nil && control.value?.text.map(JSONValue.string) == shown })
            let keys: [ElementAction] =
                [.key(ref: field.ref, key: .character("a"), modifiers: [.command])] + text.map { character in .key(ref: field.ref, key: .character(character), modifiers: []) }
                + [.key(ref: field.ref, key: .special(.return), modifiers: [])]
            let outcomes: [Result<ElementAction.Delivery, ApplicationFailure>] = try await instance.act(keys, in: watch).outcomes
            let posted: Bool = outcomes.allSatisfy { outcome in outcome.is(\.success) }
            let failures: Comment = "Posting the keys answered \(outcomes)"
            try #require(posted, failures)
            try await fixture.send(.report)
            let typed: JSONValue? = try await fixture.next(\.report).value.controls.first { control in control.entry == entry && control.member == 0 }?.value
            return (shown, typed)
        }
        let changed: Bool = before != JSONValue.string(text)
        #expect(changed)
        expectNoDifference(after, .string(text))
    }
}
