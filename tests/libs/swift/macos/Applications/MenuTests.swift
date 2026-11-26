import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import AsyncAlgorithms
import CasePathsCore
import CustomDump
import Encoding
import Fixture
import ScreenCapture
import System
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    struct MenuTests {
        @Test(
            .enabled(
                if: AXIsProcessTrusted() && CGPreflightScreenCaptureAccess(),
                "Opening and imaging the fixture's menus from outside needs this test process to hold the Accessibility and Screen Recording grants",
            )
        )
        func presentsEveryMenuLevelBehindTheFrontmostApplicationWithItsItemsAndClosesIt() async throws {
            let (presented, images, lines, focus, delivered, chosen):
                (
                    [String: (levels: [Container], presentation: Watch.Presentation?)], [(size: CGSize, frame: CGRect)], [Line], (fixture: pid_t, before: pid_t?, during: Set<pid_t?>),
                    [Result<ElementAction.Delivery, ApplicationFailure>], JSONValue?
                ) =
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
                        let answered: Bool = shown == AXError.success.rawValue
                        try #require(answered, refusal)
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
                        let arranging: Int? = first.value.last?.controls.first { (control: Control) -> Bool in control.label?.text == "Arrange" }?.opens
                        let arrange: Int = try #require(arranging)
                        let second = try await watch.presenting(arrange) { (menus: [Container]) in menus }
                        let aligning: Int? = second.value.last?.controls.first { (control: Control) -> Bool in control.label?.text == "Align" }?.opens
                        let align: Int = try #require(aligning)
                        let third = try await watch.presenting(align) { (menus: [Container]) in menus }
                        let before: pid_t? = focusedProcesses().keyFocus
                        let sampling = Task {
                            await AsyncTimerSequence(interval: .milliseconds(1), clock: ContinuousClock()).map { _ in focusedProcesses().keyFocus }.reduce(into: Set<pid_t?>()) { held, holder in
                                held.insert(holder)
                            }
                        }
                        let outcomes: [Result<ElementAction.Delivery, ApplicationFailure>] = try await instance.act([.choose(ref: pageSize.ref, item: "Tabloid")], in: watch).outcomes
                        sampling.cancel()
                        let during: Set<pid_t?> = await sampling.value
                        let shots: [Screenshot] = try await instance.capture(.element(ref: align), in: watch).map { shot in try shot.get() }
                        try await fixture.send(.report)
                        let reported: (value: Report, before: [Line]) = try await fixture.next(\.report)
                        await Captures.current?.take(in: watch)
                        return (
                            [
                                "Context": (first.value, first.presentation), "Arrange": (second.value, second.presentation), "Align": (third.value, third.presentation),
                            ],
                            shots.map { shot in (CGSize(width: shot.image.width, height: shot.image.height), shot.frame) },
                            reported.before,
                            (fixture.process, before, during),
                            outcomes,
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
                ],
            )
            let levels: [Container] = presented.values.flatMap(\.levels)
            let windowed: Bool = levels.allSatisfy { level in level.kind == .menu && level.window != nil && level.layer == Int(CGWindowLevelForKey(.popUpMenuWindow)) }
            #expect(windowed, "Each open level is a menu in its own window at the pop-up menu level: \(levels.map { level in (level.kind, level.window, level.layer) })")
            let behind: [String: Bool] = presented.mapValues { shown in if case .behind? = shown.presentation { true } else { false } }
            expectNoDifference(behind, ["Context": true, "Arrange": true, "Align": true])
            let scale: Double = try #require(CGDisplayCopyDisplayMode(CGMainDisplayID()).map { mode in Double(mode.pixelWidth) / Double(mode.width) })
            expectNoDifference(images.map(\.size), images.map { image in CGSize(width: (image.frame.width * scale).rounded(), height: (image.frame.height * scale).rounded()) })
            expectNoDifference(images.count, 3, "Imaging the deepest level opens three levels, one image each")
            let tracked: [String: [Bool]] = Dictionary(grouping: lines.compactMap { line in line[case: \.tracking] }) { track in track.title }.mapValues { tracks in tracks.map(\.began) }
            expectNoDifference(tracked, ["Context": [true, false, true, false, true, false, true, false, true, false], "Page Size": [true, false]])
            let opens: [(String, Int)] = lines.compactMap { (line: Line) -> (String, Int)? in line[case: \.menu].map { (title: String, opened: Bool) -> (String, Int) in (title, opened ? 1 : -1) } }
            let balance: [String: Int] = Dictionary(opens, uniquingKeysWith: +)
            expectNoDifference(balance.filter { _, open in open != 0 }, [:])
            expectNoDifference(Set(["Context", "Arrange", "Align", "Page Size"]).subtracting(balance.keys), [])
            #expect(!lines.contains { line in line[case: \.active] == true }, "The fixture became active")
            let held: Comment = "Key focus sat with the fixture, process \(focus.fixture), before its Page Size menu opened"
            #expect(focus.before != focus.fixture, held)
            expectNoDifference(focus.during, [focus.before], "Key focus holders while choosing Tabloid from Page Size against the holder before the press")
            let presentations: [Watch.Presentation?] = delivered.map { outcome in outcome[case: \.success]?.presentation }
            expectNoDifference(presentations, [.behind])
            expectNoDifference(chosen, .string("Tabloid"))
            let sources: [pid_t?] = lines.compactMap { line in line[case: \.key]?.sourceProcess }
            let foreign: Comment = "The fixture took keys from processes \(sources) other than this test process \(getpid())"
            #expect(sources.allSatisfy { (source: pid_t?) -> Bool in source == getpid() }, foreign)
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Presenting the fixture's Page Size menu from outside needs this test process to hold the Accessibility grant"))
        func holdsThePermittedFrontListOverEveryFrontableApplicationButTheFixtureAcrossThePresentation() async throws {
            let (changes, frontable): ([Int], Int) = try await FixtureSession.launched { fixture in
                let instance = try Instance(try #require(NSRunningApplication(processIdentifier: fixture.process)))
                let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(instance)
                let model: Model = try await watch.model(.application)
                let pageSize: Control = try #require(model.containers.flatMap(\.controls).first { control in control.role == kAXPopUpButtonRole && control.label?.text == "Page Size" })
                let sampling = Task {
                    await AsyncTimerSequence(interval: .milliseconds(1), clock: ContinuousClock()).map { _ in PermittedFrontList.entries }
                        .reduce(into: [Int]()) { held, entries in if held.last != entries { held.append(entries) } }
                }
                let frontable: Int = NSWorkspace.shared.runningApplications.count { application in application.activationPolicy == .regular && application.processIdentifier != fixture.process }
                _ = try await watch.presenting(pageSize.ref) { (menus: [Container]) in menus }
                let settled: Int = PermittedFrontList.entries
                sampling.cancel()
                await Captures.current?.take(in: watch)
                return (await sampling.value + [settled], frontable)
            }
            let peak: Int = changes.max() ?? 0
            #expect(peak >= frontable, "The permitted front list peaked at \(peak) entries while \(frontable) regular applications besides the fixture ran: \(changes)")
            expectNoDifference(changes.last, 0, "Permitted front entries left after the presentation")
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Presenting the fixture's Page Size menu from outside needs this test process to hold the Accessibility grant"))
        func refusesAPresentationWithTheLockReasonWhileAnotherHolderKeepsThePermittedFrontListLockPastItsDeadline() async throws {
            let journal = FilePath(URL.temporaryDirectory.appending(component: String(reflecting: PermittedFrontList.self)).path(percentEncoded: false))
            let (refusal, lines): (ApplicationFailure.Reason?, [Line]) = try await FixtureSession.launched { fixture in
                let instance = try Instance(try #require(NSRunningApplication(processIdentifier: fixture.process)))
                let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(instance)
                let model: Model = try await watch.model(.application)
                let pageSize: Control = try #require(model.containers.flatMap(\.controls).first { control in control.role == kAXPopUpButtonRole && control.label?.text == "Page Size" })
                let holder: FileDescriptor = try FileDescriptor.open(journal, .readWrite, options: [.create, .exclusiveLock, .nonBlocking, .closeOnExec], permissions: .ownerReadWrite)
                let outcome: Result<[Container], ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> [Container] in
                    try await watch.presenting(pageSize.ref) { (menus: [Container]) in menus }.value
                }
                try holder.close()
                await Captures.current?.take(in: watch)
                try await fixture.send(.report)
                return (outcome[case: \.failure]?.reason, try await fixture.next(\.report).before)
            }
            expectNoDifference(refusal, .system(.wouldBlock))
            let tracked: [String] = lines.compactMap { line in line[case: \.tracking]?.title }
            expectNoDifference(tracked.filter { title in title == "Page Size" }, [], "The fixture tracked menus \(tracked) while the permitted front list journal lock stayed held")
        }
    }
}
