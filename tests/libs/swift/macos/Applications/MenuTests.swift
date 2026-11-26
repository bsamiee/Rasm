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
struct MenuTests {
    @Test(
        .enabled(
            if: AXIsProcessTrusted() && CGPreflightScreenCaptureAccess(),
            "Opening and imaging the fixture's menus from outside needs this test process to hold the Accessibility and Screen Recording grants",
        )
    )
    func presentsEveryMenuLevelBehindTheFrontmostApplicationWithItsItemsAndClosesIt() async throws {
        let (presented, images, lines, focus, chosen):
            ([String: (levels: [Container], presentation: Watch.Presentation?)], [(size: CGSize, frame: CGRect)], [Line], (fixture: pid_t, before: pid_t?, open: pid_t?), JSONValue?) =
                try await FixtureSession.launched { fixture in
                    let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                    let instance = try Instance(application)
                    let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(instance)
                    let model: Model = try await watch.model(.application)
                    let region: Control = try #require(model.containers.first { container in container.title == "Covered" }?.controls.first { control in control.kind == .region })
                    let pageSize: Control = try #require(model.containers.flatMap(\.controls).first { control in control.role == kAXPopUpButtonRole && control.label?.text == "Page Size" })
                    let opening = await watch.messages()
                    let shown: Int32? = try await watch.perform(on: region.ref) { element in element.map { element in AXUIElementPerformAction(element, kAXShowMenuAction as CFString).rawValue } }
                    let refusal: Comment = "\(kAXShowMenuAction) on the region answered AXError \(String(describing: shown))"
                    try #require(shown == AXError.success.rawValue, refusal)
                    let opened: Int? = await opening.compactMap { (message: Watch.Message) -> Int? in
                        guard case .event(let event) = message, case .opened(let menu, .menu, _) = event.change else { return nil }
                        return menu
                    }.first { _ in true }
                    let context: Int = try #require(opened)
                    let closing = await watch.messages()
                    _ = try await watch.perform(on: context) { element in element.map { element in AXUIElementPerformAction(element, kAXCancelAction as CFString).rawValue } }
                    _ = await closing.first { (message: Watch.Message) -> Bool in
                        guard case .event(let event) = message, case .closed(context, .menu, _) = event.change else { return false }
                        return true
                    }
                    let first = try await watch.presenting(context) { (menus: [Container]) in menus }
                    let arrange: Int = try #require(first.value.last?.controls.first { control in control.label?.text == "Arrange" }?.opens)
                    let second = try await watch.presenting(arrange) { (menus: [Container]) in menus }
                    let align: Int = try #require(second.value.last?.controls.first { control in control.label?.text == "Align" }?.opens)
                    let third = try await watch.presenting(align) { (menus: [Container]) in menus }
                    let before: pid_t? = keyFocusProcess()
                    let popup = try await watch.presenting(pageSize.ref) { (menus: [Container]) async throws(ApplicationFailure) -> (levels: [Container], open: pid_t?, posted: Bool) in
                        let open: pid_t? = keyFocusProcess()
                        let keys: [ElementAction] = [.key(ref: nil, key: .special(.down), modifiers: []), .key(ref: nil, key: .special(.return), modifiers: [])]
                        let outcomes: [Result<ElementAction.Delivery, ApplicationFailure>] = try await instance.act(keys, in: watch).outcomes
                        return (menus, open, outcomes.allSatisfy { outcome in outcome.is(\.success) })
                    }
                    try #require(popup.value.posted, "Posting Down and Return to the open Page Size menu failed")
                    let shots: [Screenshot] = try await instance.capture(.element(ref: align), in: watch).map { shot in try shot.get() }
                    try await fixture.send(.report)
                    let reported: (value: Report, before: [Line]) = try await fixture.next(\.report)
                    return (
                        [
                            "Context": (first.value, first.presentation), "Arrange": (second.value, second.presentation), "Align": (third.value, third.presentation),
                            "Page Size": (popup.value.levels, popup.presentation),
                        ],
                        shots.map { shot in (CGSize(width: shot.image.width, height: shot.image.height), shot.frame) },
                        reported.before,
                        (fixture.process, before, popup.value.open),
                        reported.value.controls.first { control in control.entry == .popup && control.member == 0 }?.value,
                    )
                }
        let titles: [String: [[String]]] = presented.mapValues { shown in shown.levels.map { level in level.controls.compactMap(\.label?.text) } }
        expectNoDifference(
            titles,
            [
                "Context": [["Cut", "Copy", "Paste", "Arrange"]],
                "Arrange": [["Cut", "Copy", "Paste", "Arrange"], ["Align"]],
                "Align": [["Cut", "Copy", "Paste", "Arrange"], ["Align"], ["Left", "Right"]],
                "Page Size": [["Letter", "A4", "Tabloid"]],
            ],
        )
        let levels: [Container] = presented.values.flatMap(\.levels)
        let windowed: Bool = levels.allSatisfy { level in level.kind == .menu && level.window != nil && level.layer == Int(CGWindowLevelForKey(.popUpMenuWindow)) }
        #expect(windowed, "Each open level is a menu in its own window at the pop-up menu level: \(levels.map { level in (level.kind, level.window, level.layer) })")
        let behind: [String: Bool] = presented.mapValues { shown in if case .behind? = shown.presentation { true } else { false } }
        expectNoDifference(behind, ["Context": true, "Arrange": true, "Align": true, "Page Size": true])
        let scale: Double = try #require(CGDisplayCopyDisplayMode(CGMainDisplayID()).map { mode in Double(mode.pixelWidth) / Double(mode.width) })
        expectNoDifference(images.map(\.size), images.map { image in CGSize(width: (image.frame.width * scale).rounded(), height: (image.frame.height * scale).rounded()) })
        #expect(images.count == 3, "Imaging the deepest level opens three levels, one image each, and took \(images.count)")
        let tracked: [String: [Bool]] = Dictionary(grouping: lines.compactMap { line in line[case: \.tracking] }) { track in track.title }.mapValues { tracks in tracks.map(\.began) }
        expectNoDifference(tracked, ["Context": [true, false, true, false, true, false, true, false, true, false], "Page Size": [true, false]])
        let balance: [String: Int] = Dictionary(lines.compactMap { line in line[case: \.menu].map { menu in (menu.title, menu.opened ? 1 : -1) } }, uniquingKeysWith: +)
        expectNoDifference(balance.filter { _, open in open != 0 }, [:])
        #expect(Set(balance.keys).isSuperset(of: ["Context", "Arrange", "Align", "Page Size"]), "The fixture logs \(balance.keys.sorted()) as opened")
        #expect(!lines.contains { line in line[case: \.active] == true }, "The fixture became active")
        #expect(focus.before != focus.fixture, "Key focus sat with the fixture, process \(focus.fixture), before its Page Size menu opened")
        #expect(focus.open == focus.before, "Key focus moved from process \(String(describing: focus.before)) to \(String(describing: focus.open)) while the Page Size menu stayed open")
        expectNoDifference(chosen, .string("Tabloid"))
        let sources: [pid_t?] = lines.compactMap { line in line[case: \.key]?.sourceProcess }
        #expect(sources.allSatisfy { source in source == getpid() }, "The fixture took keys from processes \(sources) other than this test process \(getpid())")
    }
}
