import AppKit
import Applications
import CasePaths
import CustomDump
import Encoding
import Foundation
import TestSupport
import Testing

@testable import AppKitFixture

// --- [OPERATIONS] ----------------------------------------------------------------------

private func actions(of controls: [Control]) -> [String] {
    controls.flatMap { control in control.actions + actions(of: control.items ?? []) }
}

private func titled(_ title: String, in elements: [Element]) throws -> Element {
    try #require(elements.first { element in element.title == title }, "window \(title) in \(elements.compactMap(\.title))")
}

private func answered(_ instance: NSRunningApplication, by scenario: Scenario) async throws -> Element {
    let seen: [Observed] = try await observe(
        instance,
        within: .seconds(15),
        until: { seen in seen.last?.state.elements.contains(where: \.requiresAnswer) == true },
        acting: { try await run(scenario, in: instance) },
    )
    return try #require(seen.last?.state.asking)
}

private func dismissed(_ instance: NSRunningApplication, pressing control: Control) async throws {
    let seen: [Observed] = try await observe(
        instance,
        within: .seconds(15),
        until: { seen in seen.count > 1 && seen.last?.state.elements.contains(where: \.requiresAnswer) == false },
        acting: { try await ElementAction.perform(element: control.id, action: kAXPressAction).perform() },
    )
    expectNoDifference(seen.last?.state.elements.filter(\.requiresAnswer).map(\.role), [])
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Elements {
        @Test(.application(FixtureApplication.self, launches: false, answering: answerCrashAlert))
        func windowCarriesItsDocumentEditedStateAndControls() async throws {
            let document: URL = URL.libraryDirectory.appending(component: "Rasm Applications Document.txt")
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings(document: document, hidden: true))
            let elements: [Element] = try await read(instance)
            let window: Element = try titled("Fixture 0", in: elements)
            let outline: String = #"{"role":"AXWindow","title":"Fixture 0","document":"\#(document.absoluteString)","edited":false,"minimized":false,"actions":[],"requiresAnswer":false}"#
            expectNoDifference(try shape(window, dropping: ["id", "window", "detail", "modal", "subrole"]), try json(outline))
            let hiddenSubrole: Bool = [kAXStandardWindowSubrole, kAXDialogSubrole].contains(window.subrole)
            #expect(hiddenSubrole, "hidden window subrole \(window.subrole ?? "absent")")
            let numbered: Bool = window.window != nil
            #expect(numbered, "a window carries its CoreGraphics window number")
            let detail: Element.Detail = try #require(window.detail)
            expectNoDifference(detail.frame?.size.width, 400)
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
            expectNoDifference((elements.flatMap(\.actions) + actions(of: elements.compactMap(\.detail).flatMap(\.controls))).filter { action in action == kAXRaiseAction }, [])
            try await run(.edited, in: instance)
            let edited: Element = try titled("Fixture 0", in: try await read(instance))
            expectNoDifference(try shape(edited, dropping: ["id", "window", "detail", "modal", "subrole"]), try json(outline.replacing(#""edited":false"#, with: #""edited":true"#)))
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func popUpButtonListsTheItemsOfTheMenuItOpens() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let window: Element = try titled("Fixture 0", in: try await read(instance))
            let popUp: Control = try #require(window.detail?.controls.first { control in control.role == kAXPopUpButtonRole })
            expectNoDifference(popUp.items, nil)
            try await ElementAction.perform(element: popUp.id, action: kAXShowMenuAction).perform()
            let open: Control? = try titled("Fixture 0", in: try await read(instance)).detail?.controls.first { control in control.id == popUp.id }
            expectNoDifference(open?.items?.compactMap(\.label), ["One", "Two", "Three"])
            try await ElementAction.perform(element: try #require(open?.items?.first).id, action: kAXCancelAction).perform()
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func menuBarListsLabeledItemsWithoutTheAppleMenu() async throws {
            let (installation, instance): (Installation, NSRunningApplication) = try await launched()
            let elements: [Element] = try await read(instance)
            let bar: Element = try #require(elements.first { element in element.role == kAXMenuBarRole })
            expectNoDifference(bar.window, nil)
            expectNoDifference(bar.requiresAnswer, false)
            expectNoDifference(bar.detail?.controls.compactMap(\.label), [installation.name, "Scenario"])
            let items: [Control] = try menu("Scenario", in: elements)
            let runtime: [String] = Scenario.allCases.filter { scenario in !scenario.atLaunch }.map(\.rawValue)
            expectNoDifference(items.map(\.label), (runtime + ["disabled"]).map(Optional.some))
            expectNoDifference(items.filter { item in !item.enabled }.compactMap(\.label), ["disabled"])
            expectNoDifference(items.filter { item in !item.actions.contains(kAXPressAction) }.compactMap(\.label), [])
            let quits: Bool = try menu(installation.name, in: elements).contains { item in item.label == "Quit" }
            #expect(quits, "application menu offers Quit")
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func sheetAlertRequiresAnAnswerAndItsIdsGoStaleOnceClosed() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let sheet: Element = try await answered(instance, by: .sheet)
            expectNoDifference(try shape(sheet), try json(#"{"role":"AXSheet","identifier":"_NS:87","description":"alert","actions":[],"requiresAnswer":true}"#))
            expectNoDifference(sheet.detail?.text, ["Sheet alert", "sheet"])
            expectNoDifference(try sheet.detail.map { detail in Set(try detail.controls.map { control in try shape(control) }) }, try alertButtons())
            let cancel: Control = try #require(sheet.detail?.controls.first { control in control.identifier == "action-button-2" })
            try await dismissed(instance, pressing: cancel)
            let stale: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await ElementAction.perform(element: cancel.id, action: kAXPressAction).perform() }
            let absent: Bool = stale.reason.is(\.absent)
            #expect(absent, "stale id failed \(stale.reason): \(stale.localizedDescription)")
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func appModalAlertRequiresAnAnswerUntilPressed() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let modal: Element = try await answered(instance, by: .modal)
            expectNoDifference(
                try shape(modal),
                try json(#"{"role":"AXWindow","subrole":"AXDialog","modal":true,"identifier":"_NS:87","description":"alert","minimized":false,"actions":[],"requiresAnswer":true}"#),
            )
            expectNoDifference(modal.detail?.text, ["Modal alert", "modal"])
            expectNoDifference(try modal.detail.map { detail in Set(try detail.controls.map { control in try shape(control) }) }, try alertButtons())
            try await dismissed(instance, pressing: try #require(modal.detail?.controls.first { control in control.identifier == "action-button-1" }))
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func savePanelSheetNamesItsButtonsWithoutDefaultOrCancelAttributes() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            let sheet: Element = try await answered(instance, by: .saveSheet)
            expectNoDifference(try shape(sheet, dropping: ["id", "window", "detail", "description"]), try json(#"{"role":"AXSheet","identifier":"save-panel","actions":[],"requiresAnswer":true}"#))
            let buttons: [Control] = try #require(sheet.detail?.controls.filter { control in ["OKButton", "CancelButton"].contains(control.identifier) })
            expectNoDifference(Set(buttons.compactMap(\.identifier)), ["OKButton", "CancelButton"])
            expectNoDifference(buttons.compactMap(\.attribute), [])
            try await dismissed(instance, pressing: try #require(buttons.first { control in control.identifier == "CancelButton" }))
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func systemDialogSubroleRequiresAnAnswer() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try await run(.systemDialog, in: instance)
            let dialog: Element = try titled("System dialog", in: try await read(instance))
            expectNoDifference(
                try shape(dialog, dropping: ["id", "window", "detail", "modal"]),
                try json(#"{"role":"AXWindow","subrole":"AXSystemDialog","title":"System dialog","minimized":false,"actions":[],"requiresAnswer":true}"#),
            )
            let dismiss: Control = try #require(dialog.detail?.controls.first { control in control.label == "Dismiss" })
            try await ElementAction.perform(element: dismiss.id, action: kAXPressAction).perform()
            let remaining: [Element] = try await read(instance)
            expectNoDifference(remaining.filter(\.requiresAnswer).map(\.role), [])
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func floatingPanelRequiresNoAnswer() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try await run(.floatingPanel, in: instance)
            let panel: Element = try titled("Floating panel", in: try await read(instance))
            let line: String = #"{"role":"AXWindow","subrole":"AXFloatingWindow","title":"Floating panel","minimized":false,"actions":[],"requiresAnswer":false}"#
            expectNoDifference(try shape(panel, dropping: ["id", "window", "detail", "modal"]), try json(line))
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: answerCrashAlert))
        func tableWalkReadsItsVisibleRowsAlone() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try await run(.table, in: instance)
            let table: Element = try titled("Table", in: try await read(instance))
            let rows: [String] = table.detail?.controls.compactMap { control in control.value.flatMap { value in if case .string(let text) = value { text } else { nil } } } ?? []
            expectNoDifference(rows, (0..<Int(windowSize.height / rowHeight)).map { row in "Row \(row)" })
        }
    }
}
