import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import AsyncAlgorithms
import CasePathsCore
import CustomDump
import Encoding
import Fixture
import OrderedCollections
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let expected: [Inventory: (kind: Control.Kind, label: any KeyPath<Report.Control, String?> & Sendable)] = [
    .action: (.action, \.title), .toggle: (.toggle, \.title), .popup: (.choice, \.caption), .deferred: (.choice, \.caption), .combo: (.choice, \.caption),
    .radio: (.choice, \.caption), .tabs: (.choice, \.caption), .segments: (.choice, \.caption), .tools: (.choice, \.description), .table: (.multi, \.description),
    .outline: (.multi, \.description), .slider: (.number, \.caption), .stepper: (.number, \.caption), .measure: (.measure, \.caption), .text: (.text, \.caption),
    .search: (.text, \.caption), .secure: (.text, \.caption), .color: (.color, \.caption), .date: (.date, \.caption), .region: (.region, \.title), .siblings: (.action, \.help),
]

// --- [MODELS] --------------------------------------------------------------------------

struct Member: Hashable {
    let entry: Inventory
    let index: Int
}

struct Shown: Equatable {
    let kind: Control.Kind?
    let label: String?
}

struct Chord: Hashable {
    let key: Key?
    let modifiers: Set<Modifier>
}

struct Row {
    let member: Member
    let role: String?
    let window: Int
    let wanted: Shown
    let found: Control?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    struct ModelTests {
        static func found(_ reported: Report.Control, in model: Model) -> Control? {
            let height: CGFloat = CGDisplayBounds(CGMainDisplayID()).height
            let placed: [(frame: CGRect, control: Control)] = model.containers.flatMap(\.controls).compactMap { (control: Control) -> (frame: CGRect, control: Control)? in
                control.frame.map { seen in (CGRect(x: seen.minX, y: height - seen.maxY, width: seen.width, height: seen.height), control) }
            }
            .sorted { first, second in first.frame.width * first.frame.height < second.frame.width * second.frame.height }
            return reported.frame.flatMap { frame in placed.first { held in held.frame.contains(frame) }?.control }
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func modelsEachReportedControlWithItsKindAndLabelAndKeepsItsRefWhenItsWindowShowsAgainOrItsLinesReorder() async throws {
            let passes: [(report: Report, model: Model)] = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                var passes: [(report: Report, model: Model)] = []
                for command: Command in [.show(surface: .panel), .dismiss(surface: .controls), .show(surface: .controls), .reorder(from: 0, to: 2)] {
                    try await fixture.send(command)
                    try await fixture.send(.report)
                    let report: Report = try await fixture.next(\.report).value
                    let model: Model = try await watch.model(.application)
                    passes.append((report, model))
                }
                await Captures.current?.take(in: watch)
                return passes
            }
            let rows: ((report: Report, model: Model)) -> [Row] = { pass in
                let visible: Set<Int> = Set(pass.report.windows.filter(\.visible).map(\.number))
                return pass.report.controls.compactMap { (reported: Report.Control) -> Row? in
                    guard let row = expected[reported.entry], let window = reported.window, visible.contains(window), reported.frame != nil else { return nil }
                    return Row(
                        member: Member(entry: reported.entry, index: reported.member),
                        role: reported.role,
                        window: window,
                        wanted: Shown(kind: row.kind, label: reported[keyPath: row.label]),
                        found: Self.found(reported, in: pass.model),
                    )
                }
            }
            let first: [Row] = rows(passes[0])
            expectNoDifference(
                Dictionary(uniqueKeysWithValues: first.map { row in (row.member, Shown(kind: row.found?.kind, label: row.found?.label?.text)) }),
                Dictionary(uniqueKeysWithValues: first.map { row in (row.member, row.wanted) }),
            )
            let number: Int? = passes[0].report.windows.first { window in window.surface == .controls }?.number
            let lines: [[String?]: [Row]] = Dictionary(grouping: first.filter { row in row.window == number }) { row in [row.role, row.wanted.label] }
            let positional: Set<Member> = Set(lines.values.filter { group in group.count > 1 }.joined().map(\.member))
            let refs: ((report: Report, model: Model)) -> [Member: Int] = { pass in
                Dictionary(uniqueKeysWithValues: rows(pass).compactMap { (row: Row) -> (Member, Int)? in row.found.map { control in (row.member, control.ref) } })
            }
            let before: [Member: Int] = refs(passes[0])
            expectNoDifference(refs(passes[2]), before)
            expectNoDifference(refs(passes[3]).filter { member, _ in !positional.contains(member) }, before.filter { member, _ in !positional.contains(member) })
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func readsEachMenuItemShortcutAsTheKeyStepThatPressesIt() async throws {
            let (report, levels): (Report, [Model]) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let bar: Container = try #require(try await watch.model(.application).containers.first { container in container.kind == .menuBar })
                var levels: [Model] = []
                for opener in bar.controls { levels.append(try await watch.model(.menu(ref: opener.ref))) }
                await Captures.current?.take(in: watch)
                return (report, levels)
            }
            let specials: [NSEvent.SpecialKey: Key.Special] = [.f5: .f5]
            let held: [(flag: NSEvent.ModifierFlags, modifier: Modifier)] = [(.command, .command), (.shift, .shift), (.option, .option), (.control, .control), (.function, .function)]
            let pressed: [String: Chord] = Dictionary(
                uniqueKeysWithValues: report.controls.compactMap { (control: Report.Control) -> (String, Chord)? in
                    guard let title = control.title, let shortcut = control.shortcut, case .known(let mask) = shortcut.modifiers, let scalar = shortcut.key.unicodeScalars.first else { return nil }
                    let key: Key = specials[NSEvent.SpecialKey(rawValue: Int(scalar.value))].map(Key.special) ?? .character(Character(shortcut.key))
                    return (title, Chord(key: key, modifiers: Set(held.filter { flag, _ in mask.contains(flag) }.map(\.modifier))))
                }
            )
            let read: [String: Chord] = Dictionary(
                levels.flatMap(\.containers).flatMap(\.controls).compactMap { (control: Control) -> (String, Chord)? in
                    guard let title = control.label?.text, pressed[title] != nil else { return nil }
                    return (title, Chord(key: control.shortcut?.key, modifiers: Set(control.shortcut?.modifiers ?? [])))
                }
            ) { first, _ in first }
            #expect(!pressed.isEmpty, "The fixture reports no menu item with a shortcut")
            expectNoDifference(read, pressed)
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func readsUnsavedChangesOfEachDocumentWindowAsTheFixtureReportsThem() async throws {
            let (report, model): (Report, Model) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.show(surface: .save))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let model: Model = try await watch.model(.application)
                await Captures.current?.take(in: watch)
                return (report, model)
            }
            let documents: Set<Surface> = [.controls, .lists, .covered]
            let windows: [Report.Window] = report.windows.filter { (window: Report.Window) -> Bool in window.visible && window.surface.map(documents.contains) == true }
            let stated: [Int: Bool?] = Dictionary(uniqueKeysWithValues: windows.map { (window: Report.Window) -> (Int, Bool?) in (window.number, window.edited) })
            let read: [Int: Bool?] = Dictionary(
                model.containers.filter { container in container.parent == nil }.compactMap { container in container.window.map { number in (Int(number), container.edited) } }
            ) { first, _ in first }
            expectNoDifference(Set(stated.values), [true, false], "The fixture reports an edited document window beside an unedited one")
            expectNoDifference(read.filter { number, _ in stated[number] != nil }, stated)
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func carriesEachInputLabelBeyondTheTitleAsAFurtherLabel() async throws {
            let (report, model): (Report, Model) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let model: Model = try await watch.model(.application)
                await Captures.current?.take(in: watch)
                return (report, model)
            }
            let reported: Report.Control = try #require(report.controls.first { control in control.entry == .action })
            let control: Control = try #require(Self.found(reported, in: model))
            expectNoDifference(control.label?.text, reported.title)
            expectNoDifference(control.labels, reported.inputs?.filter { input in input != reported.title })
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func readsEachToolbarToolWithItsTooltipAndStatesTheSelectedToolAsThePointerTool() async throws {
            let passes: [Model] = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                var passes: [Model] = []
                for segment in Inventory.toolSegments.indices.reversed() {
                    try await fixture.send(.select(entry: .tools, member: 0, segment: segment))
                    try await fixture.send(.report)
                    _ = try await fixture.next(\.report)
                    passes.append(try await watch.model(.application))
                }
                await Captures.current?.take(in: watch)
                return passes
            }
            let toolbar: Container? = passes[0].containers.first { container in container.kind == .toolbar }
            let choice: Control? = toolbar?.controls.first { control in control.kind == .choice }
            let options: [Control.Option] = try #require(choice?.options)
            expectNoDifference(options.map { option in [option.label, option.help] }, Inventory.toolSegments.map { tool in [tool.label, tool.help] })
            expectNoDifference(passes.map { model in [model.pointer?.tool, model.pointer?.help] }, Inventory.toolSegments.reversed().map { tool in [tool.label, tool.help] })
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func readsEachTableRowCellsUnderTheirColumnHeaderAndCountsTheRowsTheTableHolds() async throws {
            let (report, model): (Report, Model) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let model: Model = try await watch.model(.application)
                await Captures.current?.take(in: watch)
                return (report, model)
            }
            let reported: Report.Control = try #require(report.controls.first { control in control.entry == .table })
            let table: Control = try #require(Self.found(reported, in: model))
            let rows: [Control] = model.containers.flatMap(\.controls).filter { control in
                control.kind == .row && control.frame.map { frame in table.frame?.contains(frame) == true } == true
            }
            expectNoDifference(table.total, reported.rows)
            expectNoDifference(rows.map { row in row.cells?.compactMap(\.column) }, Array(repeating: reported.columns, count: reported.rows ?? 0))
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func listsEachCustomActionOfAControlByTheNameItsViewGivesIt() async throws {
            let (report, model): (Report, Model) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let model: Model = try await watch.model(.application)
                await Captures.current?.take(in: watch)
                return (report, model)
            }
            let reported: Report.Control = try #require(report.controls.first { control in control.entry == .color })
            let named: [String] = try #require(reported.customActions)
            let listed: [String] = try #require(Self.found(reported, in: model)?.actions)
            #expect(!named.isEmpty && Set(named).isSubset(of: Set(listed)), "The view names \(named), the model lists \(listed)")
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Acting on the fixture from outside needs this test process to hold the Accessibility grant"))
        func replacesTheRangeOfAFieldItsTextNamesAndLeavesTheRestAsTheFixtureReportsIt() async throws {
            let (before, after, outcomes): (Report, Report, [Result<Step.Delivery, ApplicationFailure>]) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let before: Report = try await fixture.next(\.report).value
                let reported: Report.Control = try #require(before.controls.first { control in control.entry == .text })
                let field: Control = try #require(Self.found(reported, in: try await watch.model(.application)))
                let steps: [Step] = try JSONDecoder().decode([Step].self, from: Data(#"[{"at":\#(field.ref),"range":"titled","to":"TITLED"}]"#.utf8))
                let acted = try await watch.act(steps)
                try await fixture.send(.report)
                let after: Report = try await fixture.next(\.report).value
                await Captures.current?.take(in: watch)
                return (before, after, acted.outcomes)
            }
            let value: (Report) -> JSONValue? = { report in report.controls.first { control in control.entry == .text }?.value }
            expectNoDifference(value(before), .string("Untitled"))
            expectNoDifference(value(after), .string("UnTITLED"))
            #expect(outcomes.allSatisfy { outcome in outcome.is(\.success) }, "\(outcomes)")
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func readsAProgressBarAsANumberControl() async throws {
            let (report, model): (Report, Model) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.show(surface: .progress))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let model: Model = try await watch.model(.application)
                await Captures.current?.take(in: watch)
                return (report, model)
            }
            let reported: Report.Control = try #require(report.controls.first { control in control.entry == .progress && control.member == 0 })
            expectNoDifference(reported.role, NSAccessibility.Role.progressIndicator.rawValue)
            expectNoDifference(Self.found(reported, in: model)?.kind, .number)
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func searchesTheApplicationForAControlByItsTitleAndFindsNothingForTextNoElementHolds() async throws {
            let (title, found, missing): (String, Model, Model) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let title: String = try #require(report.controls.first { control in control.entry == .toggle }?.title)
                let (found, missing): (Model, Model) = (
                    try await watch.model(.search(text: title, key: .anyTypeSearchKey, limit: 8)), try await watch.model(.search(text: UUID().uuidString, key: .anyTypeSearchKey, limit: 8)),
                )
                await Captures.current?.take(in: watch)
                return (title, found, missing)
            }
            let labels: Set<String> = Set(found.containers.flatMap(\.controls).compactMap(\.label?.text))
            #expect(labels.contains(title), "Searching \(String(reflecting: title)) found \(labels)")
            expectNoDifference(missing.containers.map(\.ref), [])
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
        func readsAStatusItemShowingOnlyAnImageAsAnItemWhoseClosedMenuReads() async throws {
            let (items, menu): ([Control], Model) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.show(surface: .status))
                try await fixture.send(.report)
                _ = try await fixture.next(\.report)
                let items: [Control] = try await watch.model(.application).containers.filter { container in container.kind == .statusItem }.flatMap(\.controls)
                let item: Control = try #require(items.first)
                let menu: Model = try await watch.model(.menu(ref: item.ref))
                await Captures.current?.take(in: watch)
                return (items, menu)
            }
            expectNoDifference(items.map(\.kind), [.item])
            #expect(items.allSatisfy { item in item.opens != nil }, "The status item names no menu it opens: \(items)")
            expectNoDifference(menu.containers.map { container in container.controls.compactMap(\.label?.text) }, [["Pause", "Resume"]])
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Reading the menu bar agent from outside needs this test process to hold the Accessibility grant"))
        func countsTheStatusItemOfTheFixtureInItsListedStateWhileItShows() async throws {
            let seen: [Result<Int, ApplicationFailure>] = try await FixtureSession.launched { fixture in
                let process: pid_t = fixture.process
                let counts = Watches().states { application in application.processIdentifier == process }.compactMap { listed in
                    listed[case: \.success]?.values.first?[case: \.running]?.statusItems
                }
                var seen: [Result<Int, ApplicationFailure>] = []
                reading: for await count in counts.removeDuplicates() {
                    seen.append(count)
                    switch (seen.count, count) {
                        case (1, _): try await fixture.send(.show(surface: .status))
                        case (_, .success(1)): try await fixture.send(.dismiss(surface: .status))
                        case (_, .success(0)): break reading
                        case (_, _): continue
                    }
                }
                return seen
            }
            expectNoDifference(seen, [.success(0), .success(1), .success(0)])
        }
    }
}
