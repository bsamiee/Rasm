import AppKit
import Applications
import CasePaths
import CustomDump
import Encoding
import TestSupport
import Testing

@testable import AppKitFixture

// --- [OPERATIONS] ----------------------------------------------------------------------

private func controls(of instance: NSRunningApplication) async throws -> (window: Element, controls: [Control]) {
    let window: Element = try #require(try await read(instance).first { element in element.title == "Fixture 0" })
    return (window, window.detail?.controls ?? [])
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct ElementActions {
        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func pressRunsControlActionBeforeAnswering() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let press: Control = try #require(try await controls(of: instance).controls.first { control in control.identifier == "press" })
            try await ElementAction.perform(element: press.id, action: kAXPressAction).perform(in: instance.processIdentifier)
            let pressed: Element = try await controls(of: instance).window
            expectNoDifference(pressed.detail.map { detail in Set(detail.text) }, ["pressed Press", "Fixture 0"])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments), arguments: ["Größe 42 ✓", ""])
        func setWritesFieldValueWithoutChangeTracking(text: String) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let field: Control = try #require(try await controls(of: instance).controls.first { control in control.identifier == "field" })
            try await ElementAction.set(element: field.id, value: "draft").perform()
            try await ElementAction.set(element: field.id, value: text).perform()
            let (window, listed): (Element, [Control]) = try await controls(of: instance)
            expectNoDifference(listed.first { control in control.identifier == "field" }?.value, .string(text))
            expectNoDifference(window.detail.map { detail in Set(detail.text) }, ["idle", "Fixture 0"])
            expectNoDifference(window.edited, false)
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func setOnElementWithoutSettableValueFails() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let (window, listed): (Element, [Control]) = try await controls(of: instance)
            let close: Control = try #require(listed.first { control in control.attribute == kAXCloseButtonAttribute })
            for target: Data in [close.id, window.id] {
                let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await ElementAction.set(element: target, value: "text").perform() }
                let answered: Bool = !failure.reason.is(\.unanswered)
                #expect(answered, "set failed as unanswered: \(failure.localizedDescription)")
            }
            let unchanged: Element = try await controls(of: instance).window
            expectNoDifference(unchanged.title, "Fixture 0")
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func raiseIsRefusedUnsentAndLeavesFixtureInBackground() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let window: Element = try await controls(of: instance).window
            let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await ElementAction.perform(element: window.id, action: kAXRaiseAction).perform() }
            let activating: Bool = failure.reason.is(\.activating)
            #expect(activating, "raise failed \(failure.reason)")
            #expect(!instance.isActive, "fixture stays in background")
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func menuItemOfBackgroundAppOpensWindowThatWatchReads() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [Observed] = try await observe(
                instance,
                within: .seconds(15),
                until: { seen in seen.last?.state.elements.contains { element in element.title == "Fixture 1" } == true },
                acting: { try await run(.window, in: instance) },
            )
            expectNoDifference(Set(try #require(seen.last).state.elements.filter { element in element.role == kAXWindowRole }.compactMap(\.title)), ["Fixture 0", "Fixture 1"])
            #expect(!instance.isActive, "fixture stays in background")
        }
    }
}
