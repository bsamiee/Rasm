import AppKit
import CasePaths
import CustomDump
import Encoding
import Foundation
import TestSupport
import Testing

@testable import AppKitFixture
@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let alertControls: Set<String> = ["action-button-1", "action-button-2"]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func actions(of controls: [Control]) -> [String] {
    controls.flatMap { control in control.actions + actions(of: control.items ?? []) }
}

private func titled(_ title: String, in elements: [Element]) throws -> Element {
    try #require(elements.first { element in element.title == title }, "window \(title) in \(elements.compactMap(\.title))")
}

private func asked(_ instance: NSRunningApplication, by scenario: Scenario, showing controls: Set<String>) async throws -> Element {
    let seen: [InstanceState] = try await observe(
        instance,
        until: { seen in controls.isSubset(of: seen.last?.asking?.detail?.controls.compactMap(\.identifier) ?? []) },
        acting: { try await run(scenario, in: instance) },
    )
    return try #require(seen.last?.asking)
}

private func dismissed(_ instance: NSRunningApplication, pressing control: Control) async throws {
    let next: InstanceState = try await ElementAction.perform(element: control.id, action: kAXPressAction).perform(in: instance.processIdentifier)
    expectNoDifference(next.elements.filter(\.asking).map(\.role), [])
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Elements {
        @Test(.application(FixtureApplication.self, launches: false))
        func windowHoldsItsDocumentEditedStateAndControls() async throws {
            let document: URL = URL.libraryDirectory.appending(component: "Rasm Applications Document.txt")
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings(document: document))
            let running: InstanceState.Running = try await read(instance)
            let window: Element = try titled("Fixture 0", in: running.elements)
            let outline: String = #"{"role":"AXWindow","title":"Fixture 0","document":"\#(document.absoluteString)","edited":false,"minimized":false,"kind":"window","asking":false}"#
            expectNoDifference(try shape(window, dropping: ["id", "detail", "modal", "subrole", "actions", "answers"]), try json(outline))
            let detail: Element.Detail = try #require(window.detail)
            expectNoDifference(Set(detail.text), ["idle", "Fixture 0"])
            let declared: [JSONValue] = try detail.controls.filter { control in control.identifier != nil || control.attribute == kAXCloseButtonAttribute || control.label == "Check" }
                .map { control in try shape(control, dropping: ["id", "items", "actions", "subrole"]) }
            expectNoDifference(
                declared,
                try [
                    json(#"{"role":"AXButton","label":"Press","identifier":"press","enabled":true}"#), json(#"{"role":"AXTextField","identifier":"field","value":"","enabled":true}"#),
                    json(#"{"role":"AXCheckBox","label":"Check","value":0,"enabled":true}"#), json(#"{"role":"AXButton","attribute":"AXCloseButton","enabled":true}"#),
                ],
            )
            let popUp: Control = try #require(detail.controls.first { control in control.role == kAXPopUpButtonRole })
            expectNoDifference(try shape(popUp, dropping: ["id", "items", "actions", "label"]), try json(#"{"role":"AXPopUpButton","value":"One","enabled":true}"#))
            let listed: [String] = running.elements.flatMap(\.actions) + actions(of: running.elements.compactMap(\.detail).flatMap(\.controls))
            let raises: Bool = listed.contains(kAXRaiseAction)
            #expect(raises, "library lists the host action")
            try await run(.edited, in: instance)
            let edited: Element = try titled("Fixture 0", in: try await read(instance).elements)
            expectNoDifference(
                try shape(edited, dropping: ["id", "detail", "modal", "subrole", "actions", "answers"]),
                try json(outline.replacing(#""edited":false"#, with: #""edited":true"#)),
            )
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func popUpButtonListsItemsOfMenuItOpens() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let window: Element = try titled("Fixture 0", in: try await read(instance).elements)
            let popUp: Control = try #require(window.detail?.controls.first { control in control.role == kAXPopUpButtonRole })
            expectNoDifference(popUp.items, nil)
            _ = try await ElementAction.perform(element: popUp.id, action: kAXShowMenuAction).perform()
            let open: Control? = try titled("Fixture 0", in: try await read(instance).elements).detail?.controls.first { control in control.id == popUp.id }
            expectNoDifference(open?.items?.compactMap(\.label), ["One", "Two", "Three"])
            _ = try await ElementAction.perform(element: try #require(open?.items?.first).id, action: kAXCancelAction).perform()
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func menuBarListsLabeledItemsWithoutAppleMenu() async throws {
            let (installation, instance): (Installation, NSRunningApplication) = try await launched()
            let running: InstanceState.Running = try await read(instance)
            let bar: Element = try #require(running.menus.first { element in element.role == kAXMenuBarRole })
            expectNoDifference(bar.kind, .menu)
            expectNoDifference(bar.asking, false)
            let scenarios: String = String(describing: Scenario.self)
            expectNoDifference(bar.detail?.controls.compactMap(\.label), [installation.name, scenarios])
            let items: [Control] = try menu(scenarios, in: running)
            let runtime: [String] = Scenario.allCases.filter { scenario in !scenario.atLaunch }.map(\.rawValue)
            expectNoDifference(items.map(\.label), (runtime + ["disabled"]).map(Optional.some))
            expectNoDifference(items.filter { item in !item.enabled }.compactMap(\.label), ["disabled"])
            let quits: Bool = try menu(installation.name, in: running).contains { item in item.label == "Quit" }
            #expect(quits, "application menu offers Quit")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func sheetAlertAsksAndItsIdsGoStaleOnceClosed() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let sheet: Element = try await asked(instance, by: .sheet, showing: alertControls)
            expectNoDifference(sheet.kind, .sheet)
            expectNoDifference(sheet.detail?.text, ["Sheet alert", Scenario.sheet.rawValue])
            expectNoDifference(try sheet.detail.map { detail in Set(try detail.controls.map { control in try shape(control) }) }, try alertButtons())
            let cancel: Control = try #require(sheet.answers?.cancel)
            try await dismissed(instance, pressing: cancel)
            let stale: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await ElementAction.perform(element: cancel.id, action: kAXPressAction).perform() }
            let unresolved: Bool = stale.reason.is(\.unresolved)
            #expect(unresolved, "stale id failed \(stale.reason): \(stale.localizedDescription)")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func appModalAlertAsksUntilPressed() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let modal: Element = try await asked(instance, by: .modal, showing: alertControls)
            expectNoDifference(modal.kind, .alert)
            expectNoDifference(modal.detail?.text, ["Modal alert", Scenario.modal.rawValue])
            try await dismissed(instance, pressing: try #require(modal.answers?.accept))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func savePanelSheetReadsFilePanelAndAnswersByIdentifier() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let sheet: Element = try await asked(instance, by: .saveSheet, showing: ["OKButton", "CancelButton"])
            expectNoDifference(sheet.kind, .filePanel)
            expectNoDifference(sheet.answers?.accept?.identifier, "OKButton")
            try await dismissed(instance, pressing: try #require(sheet.answers?.cancel))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func nonActivatingSystemDialogSubroleAsksNothing() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.elements.contains { element in element.title == "System dialog" } == true },
                acting: { try await run(.systemDialog, in: instance) },
            )
            let dialog: Element = try titled("System dialog", in: try #require(seen.last).elements)
            expectNoDifference(dialog.kind, .window)
            expectNoDifference(dialog.asking, false)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func floatingPanelAsksNothing() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.elements.contains { element in element.title == "Floating panel" } == true },
                acting: { try await run(.floatingPanel, in: instance) },
            )
            expectNoDifference(try titled("Floating panel", in: try #require(seen.last).elements).asking, false)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func tableWalkReadsItsVisibleRowsAlone() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try await run(.table, in: instance)
            let table: Element = try titled("Table", in: try await read(instance).elements)
            let rows: [String] = table.detail?.controls.compactMap { control in control.value.flatMap { value in if case .string(let text) = value { text } else { nil } } } ?? []
            expectNoDifference(rows, (0..<Int(windowSize.height / rowHeight)).map { row in "Row \(row)" })
        }
    }
}
