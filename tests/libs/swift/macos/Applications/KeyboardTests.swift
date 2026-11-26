import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import CasePathsCore
import CustomDump
import Encoding
import Fixture
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    struct KeyboardTests {
        @Test(
            .enabled(if: AXIsProcessTrusted(), "Typing into the fixture from outside needs this test process to hold the Accessibility grant"),
            arguments: [(Inventory.text, "Façade Noël, À É"), (Inventory.measure, "Ça Ø Å")],
        )
        func typesEachCharacterIntoAFocusedFieldThroughTheKeysOfTheCurrentLayout(into entry: Inventory, typing text: String) async throws {
            let (before, after): (JSONValue?, JSONValue?) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let instance = try Instance(application)
                let watch: Watch = await Watches().watch(instance)
                try await fixture.send(.report)
                let shown: JSONValue? = try await fixture.next(\.report).value.controls.first { control in control.entry == entry && control.member == 0 }?.value
                let model: Model = try await watch.model(.application)
                let field: Control = try #require(model.containers.flatMap(\.controls).first { control in shown != nil && control.value?.text.map(JSONValue.string) == shown })
                let typed: [JSONValue] = text.map { character in .object(["at": .number(Double(field.ref)), "key": .string(String(character))]) }
                let keys: [JSONValue] =
                    [.object(["at": .number(Double(field.ref)), "key": .string("a"), "modifiers": .array([.string("command")])])] + typed + [
                        .object(["at": .number(Double(field.ref)), "key": .string("return")])
                    ]
                let steps: [Step] = try JSONDecoder().decode([Step].self, from: JSONEncoder().encode(keys))
                let outcomes: [Result<Step.Delivery, ApplicationFailure>] = try await watch.act(steps).outcomes
                let posted: Bool = outcomes.allSatisfy { outcome in outcome.is(\.success) }
                let failures: Comment = "Posting the keys answered \(outcomes)"
                try #require(posted, failures)
                try await fixture.send(.report)
                let typed: JSONValue? = try await fixture.next(\.report).value.controls.first { control in control.entry == entry && control.member == 0 }?.value
                await Captures.current?.take(in: watch)
                return (shown, typed)
            }
            #expect(before != JSONValue.string(text), "The field held the typed text before typing")
            expectNoDifference(after, .string(text))
        }
    }
}
