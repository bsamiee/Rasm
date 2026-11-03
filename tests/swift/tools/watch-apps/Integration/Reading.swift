import AppKit
import CasePaths
import CustomDump
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [MODELS] --------------------------------------------------------------------------

private struct Shown: Equatable {
    let identifier: String?
    let role: String?
    let label: String?
    let value: WatchAppsLine.Scalar?
    let items: [String]?

    init(identifier: String?, role: String?, label: String?, value: WatchAppsLine.Scalar?, items: [String]?) {
        self.identifier = identifier
        self.role = role
        self.label = label
        self.value = value
        self.items = items
    }

    init(_ control: WatchAppsLine.Control) {
        identifier = control.identifier
        role = control.role
        label = control.label
        value = control.value
        items = control.items?.compactMap(\.label)
    }
}

private struct Placed: Equatable {
    let title: String?
    let left: CGFloat?
    let size: CGSize?
    let minimized: Bool?
    let document: URL?
    let edited: Bool?
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Reading {
        @MainActor
        @Test(.application(FixtureApplication.self, launches: false))
        func readGivesWindowDetailUsageAndMenus() async throws {
            let fixture: Fixture = try Fixture()
            let document: URL = URL.temporaryDirectory.appending(component: "\(fixture.identifier).txt")
            let pid: Int32 = try await launch(fixture, Settings(windows: 2, document: document, edited: true)).pid
            let read: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["read", fixture.identifier],
                as: WatchAppsLine.self,
            )
            let instances: [Int32: WatchAppsLine.Instance] = try #require(read.lines.first?.entry.instances)
            _ = try #require(instances[pid]?.running?.usage, "read lines hold usage")
            let elements: [WatchAppsLine.Element] = try #require(instances[pid]?.running?.status.responding?.elements)
            let style: NSWindow.StyleMask = [.titled, .closable, .miniaturizable, .resizable]
            let size: CGSize = NSWindow.frameRect(forContentRect: NSRect(origin: .zero, size: windowSize), styleMask: style).size
            let windows: [WatchAppsLine.Element] = elements.filter { element in element.role == kAXWindowRole }
            let expected: [String: Placed] = Dictionary(
                uniqueKeysWithValues: (0..<2).map { (index: Int) -> (String, Placed) in
                    let title: String = "Fixture \(index)"
                    let left: CGFloat = 200 + CGFloat(index) * 40
                    return (title, Placed(title: title, left: left, size: size, minimized: false, document: document, edited: true))
                }
            )
            let placed: [String: Placed] = Dictionary(
                windows.map { window in
                    (
                        window.title ?? window.id,
                        Placed(title: window.title, left: window.detail?.frame?.minX, size: window.detail?.frame?.size, minimized: window.minimized, document: window.document, edited: window.edited),
                    )
                },
                uniquingKeysWith: { first, _ in first },
            )
            let unnumbered: [String?] = windows.filter { window in window.window == nil }.map(\.title)
            let text: [String] = windows.compactMap(\.detail?.text).joined().map(\.self)
            let first: WatchAppsLine.Element = try #require(windows.titled("Fixture 0"))
            let shown: [Shown] = (first.detail?.controls ?? []).filter { control in control.subrole == nil }.map(Shown.init)
            let close: String? = first.detail?.controls?.first { control in control.subrole == kAXCloseButtonSubrole }?.attribute
            let ids: [String] = elements.compactMap(\.detail?.controls).joined().map(\.id)
            let bar: WatchAppsLine.Element = try #require(elements.menuBar)
            let menus: [WatchAppsLine.Control] = try #require(bar.detail?.controls)
            let apple: [String?] = menus.filter { menu in menu.label == "Apple" || menu.items?.contains { item in item.label == "About This Mac" } == true }.map(\.label)
            let items: [WatchAppsLine.Control] = bar.control(labeled: String(describing: Scenario.self))?.items ?? []
            let menuScenarios: [String?] = Scenario.allCases.filter { scenario in !scenario.atLaunch }.map(\.rawValue)
            let scenarios: [String?] = items.map(\.label).filter { label in menuScenarios.contains(label) }
            let enablement: [Bool?] = [items.first { item in item.label == "disabled" }?.enabled, items.first { item in item.label == Scenario.window.rawValue }?.enabled]
            let quits: Bool = menus.contains { menu in menu.items?.contains { item in item.label == "Quit" } == true }
            expectNoDifference(read.status, .exited(0))
            expectNoDifference(read.lines.map(\.key), [fixture.identifier])
            expectNoDifference(pids(in: read.lines), [[pid]])
            expectNoDifference(placed, expected)
            expectNoDifference(unnumbered, [])
            expectNoDifference(text.contains("idle"), true, "\(text)")
            expectNoDifference(
                shown,
                [
                    Shown(identifier: "press", role: kAXButtonRole, label: "Press", value: nil, items: nil),
                    Shown(identifier: "field", role: kAXTextFieldRole, label: nil, value: .text(""), items: nil),
                    Shown(identifier: nil, role: kAXCheckBoxRole, label: "Check", value: .number(0), items: nil),
                    Shown(identifier: nil, role: kAXPopUpButtonRole, label: nil, value: .text("One"), items: nil),
                ],
            )
            expectNoDifference(close, kAXCloseButtonAttribute)
            expectNoDifference(Set(ids).count, ids.count)
            expectNoDifference(bar.window, nil)
            expectNoDifference(apple, [])
            expectNoDifference(scenarios, menuScenarios)
            expectNoDifference(enablement, [false, true])
            #expect(quits)
            let popUp: WatchAppsLine.Control = try #require(first.detail?.controls?.first { control in control.role == kAXPopUpButtonRole })
            try await perform(popUp, kAXShowMenuAction)
            let opened: [WatchAppsLine.Element] = try await watch_appsIntegrationTests.read(fixture, pid: pid)
            let open: WatchAppsLine.Control? = opened.titled("Fixture 0")?.detail?.controls?.first { control in control.id == popUp.id }
            expectNoDifference(open?.items?.compactMap(\.label), ["One", "Two", "Three"])
            try await perform(try #require(open?.items?.first), kAXCancelAction)
        }

        @MainActor
        @Test(.application(FixtureApplication.self, launches: false))
        func windowAccessibilityOmitsPrintsUnderWindowsOnHeartbeatAndRead() async throws {
            let fixture: Fixture = try Fixture()
            try #require(CGPreflightScreenCaptureAccess(), "window titles need Screen Recording grant")
            let pid: Int32 = try await launch(fixture).pid
            let described: Int = try #require(try await read(fixture, pid: pid).titled("Fixture 0")?.window)
            let shown: WatchAppsLine = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.undescribed, in: fixture, pid: pid)
                return try await lines.next { line in line.windows(of: pid)?.contains { window in window.title == "Undescribed" } == true }
            }
            .outcome
            let status: WatchAppsLine.Status = try await status(of: fixture, pid: pid)
            let listed: [WatchAppsLine.Window] = status.windows
            let undescribed: WatchAppsLine.Window = try #require(listed.first { window in window.title == "Undescribed" })
            let size: CGSize = NSWindow.frameRect(forContentRect: NSRect(origin: .zero, size: undescribedSize), styleMask: [.titled]).size
            expectNoDifference(shown.windows(of: pid)?.contains { window in window.number == described }, false)
            expectNoDifference(shown.elements(of: pid)?.contains { element in element.title == "Undescribed" }, false)
            expectNoDifference(status.responding?.elements.contains { element in element.title == "Undescribed" }, false)
            expectNoDifference(listed.contains { window in window.number == described }, false)
            expectNoDifference([undescribed.frame.width, undescribed.frame.height], [size.width, size.height])
            expectNoDifference([undescribed.layer, undescribed.onScreen ? 1 : 0], [0, 1])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func pidSelectsOneInstanceAndRefusesOthers() async throws {
            let fixture: Fixture = try Fixture()
            let first: Int32 = try await launch(fixture).pid
            let second: Int32 = try await launch(fixture, flags: ["--new"], running: [first]).pid
            let all: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["read", fixture.identifier],
                as: WatchAppsLine.self,
            )
            expectNoDifference(pids(in: all.lines), [[first, second].sorted()])
            for pid: Int32 in [first, second] {
                let one: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                    of: .product(named: "watch-apps"),
                    arguments: ["read", "--pid", String(pid)],
                    as: WatchAppsLine.self,
                )
                expectNoDifference(pids(in: one.lines), [[pid]])
                expectNoDifference(one.status, .exited(0))
            }
            try await expectRefusal(["read", "--pid", "1"], keyedBy: "1")
        }
    }
}
