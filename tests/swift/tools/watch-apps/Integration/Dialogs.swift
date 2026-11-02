import CasePaths
import CustomDump
import Foundation
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let alertButtons: Set<Button> = [
    Button(identifier: "action-button-1", label: "OK", attribute: "AXDefaultButton"), Button(identifier: "action-button-2", label: "Cancel", attribute: "AXCancelButton"),
]
private let alertControls: Set<String> = Set(alertButtons.compactMap(\.identifier))

// --- [MODELS] --------------------------------------------------------------------------

private struct Shape: Hashable {
    let role: String?
    let subrole: String?
    let title: String?
    let requiresAnswer: Bool?

    init(_ element: WatchAppsLine.Element) {
        role = element.role
        subrole = element.subrole
        title = element.title
        requiresAnswer = element.requiresAnswer
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func dialog(
    _ scenario: Scenario,
    showing controls: Set<String>,
    answeredBy identifier: String,
    in fixture: Fixture,
) async throws -> (dialog: WatchAppsLine.Element, read: [WatchAppsLine.Element], lines: [WatchAppsLine], pid: Int32) {
    let watched: (outcome: (dialog: WatchAppsLine.Element, read: [WatchAppsLine.Element], pid: Int32), lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) {
        lines in
        let pid: Int32 = try await launch(fixture).pid
        _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
        try await press(scenario, in: fixture, pid: pid)
        let shown: TimedLine<WatchAppsLine> = try await lines.next { line in
            controls.isSubset(of: line.elements(of: pid)?.answers.first?.detail?.controls?.compactMap(\.identifier) ?? [])
        }
        let dialog: WatchAppsLine.Element = try #require(shown.line.elements(of: pid)?.answers.first)
        let read: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
        let answer: WatchAppsLine.Control = try #require(dialog.control(identifier))
        try await perform(answer)
        _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
        try await expectRefusal(["perform", answer.id, "AXPress"], keyedBy: answer.id)
        return (dialog, read, pid)
    }
    let transcript: [WatchAppsLine] = watched.lines.map(\.line)
    let changes: [Phase] = phases(of: watched.outcome.pid, in: transcript).reduce(into: []) { kept, next in
        if kept.last != next { kept.append(next) }
    }
    let answered: [Phase] = Array(changes.drop { phase in phase != .answering }.prefix(2))
    let remaining: [WatchAppsLine.Element]? = transcript.last?.elements(of: watched.outcome.pid)?.answers
    expectNoDifference(answered, [.answering, .responding])
    expectNoDifference(remaining, [])
    return (watched.outcome.dialog, watched.outcome.read, transcript, watched.outcome.pid)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Dialogs {
        @Test(.application(FixtureApplication.self, launches: false))
        func sheetAlertRequiresAnswerUntilPressed() async throws {
            let fixture: Fixture = try Fixture()
            let shown: (dialog: WatchAppsLine.Element, read: [WatchAppsLine.Element], lines: [WatchAppsLine], pid: Int32) = try await dialog(
                .sheet,
                showing: alertControls,
                answeredBy: "action-button-1",
                in: fixture,
            )
            let windowItem: WatchAppsLine.Control? = shown.read.menuBar?.item(.window)
            expectNoDifference(Answer(shown.dialog), Answer(role: "AXSheet", modal: nil, description: "alert", identifier: "_NS:87", requiresAnswer: true, buttons: alertButtons))
            expectNoDifference(shown.dialog.detail?.text, ["Sheet alert", Scenario.sheet.rawValue])
            expectNoDifference(shown.read.answers.map(\.id), [shown.dialog.id])
            expectNoDifference(windowItem?.enabled, true)
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: [Scenario.minimize, .window])
        func windowChangesPrintLinesRequiringNoAnswer(scenario: Scenario) async throws {
            let fixture: Fixture = try Fixture()
            let changed: (WatchAppsLine.Element) -> Bool = { element in
                switch scenario {
                    case .window: element.title == "Fixture 1"
                    default: element.title == "Fixture 0" && element.subrole != "AXStandardWindow"
                }
            }
            let watched: (outcome: (pid: Int32, shown: WatchAppsLine?, read: [WatchAppsLine.Element]), lines: [TimedLine<WatchAppsLine>]) = try await watching(
                [fixture.identifier],
                within: .seconds(15),
            ) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(scenario, in: fixture, pid: pid)
                let shown: TimedLine<WatchAppsLine> = try await lines.next { line in line.elements(of: pid)?.contains(where: changed) == true }
                return (pid, shown.line, try await read(fixture, pid: pid))
            }
            let seen: [Phase] = phases(of: watched.outcome.pid, in: watched.lines.map(\.line)).filter { phase in phase != .launching }
            let windows: [WatchAppsLine.Element] = watched.outcome.read.filter { element in element.role != "AXMenuBar" }
            let titles: [String] = windows.compactMap(\.title).sorted()
            let outlined: Set<Shape>? = watched.outcome.shown?.elements(of: watched.outcome.pid).map { elements in Set(elements.map(Shape.init)) }
            expectNoDifference(Set(seen), [.responding])
            expectNoDifference(outlined, Set(windows.map(Shape.init)), "changed line outlines every window read reports")
            expectNoDifference(watched.outcome.read.answers, [])
            switch scenario {
                case .minimize: expectNoDifference(windows.titled("Fixture 0")?.minimized, true)
                case .window: expectNoDifference(titles, ["Fixture 0", "Fixture 1"])
                default: Issue.record("\(scenario) is no window scenario")
            }
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func setWritesSettableValueAndRefusesOtherElements() async throws {
            let fixture: Fixture = try Fixture()
            let pid: Int32 = try await launch(fixture).pid
            let before: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            let window: WatchAppsLine.Element = try #require(before.titled("Fixture 0"))
            let field: WatchAppsLine.Control = try #require(window.control("field"))
            let press: WatchAppsLine.Control = try #require(window.control("press"))
            let set: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["set", field.id, "typed text"],
                as: WatchAppsLine.self,
            )
            expectNoDifference(set.lines, [])
            expectNoDifference(set.status, .exited(0))
            let after: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            let written: WatchAppsLine.Control? = after.titled("Fixture 0")?.control("field")
            expectNoDifference(written?.value, .text("typed text"))
            expectNoDifference(written?.id, field.id)
            expectNoDifference(after.titled("Fixture 0")?.edited, false)
            try await expectRefusal(["set", press.id, "text"], keyedBy: press.id)
            try await expectRefusal(["set", window.id, "text"], keyedBy: window.id)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func performPressesAndRefusesRaiseAndUnknownActions() async throws {
            let fixture: Fixture = try Fixture()
            let pid: Int32 = try await launch(fixture).pid
            let before: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            let window: WatchAppsLine.Element = try #require(before.titled("Fixture 0"))
            let press: WatchAppsLine.Control = try #require(window.control("press"))
            let actions: [String] = window.actions ?? []
            try await perform(press)
            let after: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            let text: [String] = after.compactMap(\.detail?.text).joined().map(\.self)
            expectNoDifference(text.contains("pressed Press"), true, "\(text)")
            expectNoDifference(actions.contains("AXRaise"), false)
            try await expectRefusal(["perform", window.id, "AXRaise"], keyedBy: window.id)
            try await expectRefusal(["perform", press.id, "AXUnknownAction"], keyedBy: press.id)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func idsOfClosedWindowsAndExitedInstancesFailByID() async throws {
            let fixture: Fixture = try Fixture()
            let pid: Int32 = try await launch(fixture).pid
            let before: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            let window: WatchAppsLine.Element = try #require(before.titled("Fixture 0"))
            let close: WatchAppsLine.Control = try #require(window.detail?.controls?.first { control in control.attribute == "AXCloseButton" })
            let field: WatchAppsLine.Control = try #require(window.control("field"))
            let bar: WatchAppsLine.Element = try #require(before.menuBar)
            let quit: WatchAppsLine.Control = try #require(bar.detail?.controls?.compactMap(\.items).joined().first { item in item.label == "Quit" })
            let menu: WatchAppsLine.Control = try #require(bar.item(.window))
            try await perform(close)
            let closed: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            expectNoDifference(closed.titled("Fixture 0"), nil)
            try await expectRefusal(["set", field.id, "text"], keyedBy: field.id)
            try await expectRefusal(["perform", window.id, "AXPress"], keyedBy: window.id)
            _ = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                try await perform(quit)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .exited }
            }
            try await expectRefusal(["perform", menu.id, "AXPress"], keyedBy: menu.id)
        }
    }
}
