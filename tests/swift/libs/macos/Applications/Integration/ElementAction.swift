import AppKit
import CasePaths
import CustomDump
import Encoding
import TestSupport
import Testing

@testable import AppKitFixture
@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

private func controls(of instance: NSRunningApplication) async throws -> (window: Element, controls: [Control]) {
    let window: Element = try #require(try await read(instance).elements.first { element in element.title == "Fixture 0" })
    return (window, window.detail?.controls ?? [])
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct ElementActions {
        @Test(.application(FixtureApplication.self, launches: false))
        func pressRunsControlActionAndReturnsNextState() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let press: Control = try #require(try await controls(of: instance).controls.first { control in control.identifier == "press" })
            let next: InstanceState = try await ElementAction.perform(element: press.id, action: kAXPressAction).perform(in: instance.processIdentifier)
            expectNoDifference(next.phase, .responding)
            let pressed: Element = try await controls(of: instance).window
            expectNoDifference(pressed.detail.map { detail in Set(detail.text) }, ["pressed Press", "Fixture 0"])
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: ["Größe 42 ✓", ""])
        func setWritesFieldValueWithoutChangeTracking(text: String) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let field: Control = try #require(try await controls(of: instance).controls.first { control in control.identifier == "field" })
            _ = try await ElementAction.set(element: field.id, value: "draft").perform()
            _ = try await ElementAction.set(element: field.id, value: text).perform()
            let (window, listed): (Element, [Control]) = try await controls(of: instance)
            expectNoDifference(listed.first { control in control.identifier == "field" }?.value, .string(text))
            expectNoDifference(window.detail.map { detail in Set(detail.text) }, ["idle", "Fixture 0"])
            expectNoDifference(window.edited, false)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func setOnElementWithoutSettableValueFails() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let (window, listed): (Element, [Control]) = try await controls(of: instance)
            let close: Control = try #require(listed.first { control in control.attribute == kAXCloseButtonAttribute })
            for target: Element.ID in [close.id, window.id] {
                let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await ElementAction.set(element: target, value: "text").perform() }
                let answered: Bool = !failure.reason.is(\.unanswered)
                #expect(answered, "set failed as unanswered: \(failure.localizedDescription)")
            }
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func performNamesRemovedElementAndOtherProcessAsCause() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let (window, listed): (Element, [Control]) = try await controls(of: instance)
            let press: Control = try #require(listed.first { control in control.identifier == "press" })
            let foreign: ApplicationFailure = try await #require(throws: ApplicationFailure.self) {
                try await ElementAction.perform(element: press.id, action: kAXPressAction).perform(in: getpid())
            }
            expectNoDifference(foreign.reason[case: \.foreign], getpid())
            _ = try await ElementAction.perform(element: try #require(listed.first { control in control.attribute == kAXCloseButtonAttribute }).id, action: kAXPressAction).perform()
            let closed: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await ElementAction.perform(element: window.id, action: kAXPressAction).perform() }
            let unresolved: Bool = closed.reason.is(\.unresolved)
            #expect(unresolved, "closed window id failed \(closed.reason): \(closed.localizedDescription)")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func menuItemOfBackgroundAppOpensWindowThatStreamReads() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.elements.contains { element in element.title == "Fixture 1" } == true },
                acting: { try await run(.window, in: instance) },
            )
            expectNoDifference(Set(try #require(seen.last).elements.filter { element in element.role == kAXWindowRole }.compactMap(\.title)), ["Fixture 0", "Fixture 1"])
            #expect(!instance.isActive, "fixture stays in background")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func postedKeyAnswersBackgroundModalAlert() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            _ = try await observe(instance, until: { seen in seen.last?.asking != nil }, acting: { try await run(.modal, in: instance) })
            let next: InstanceState = try await ElementAction.key(process: instance.processIdentifier, code: 36).perform()
            expectNoDifference(next.elements.filter(\.asking).map(\.role), [])
            #expect(!instance.isActive, "fixture stays in background")
        }
    }
}
