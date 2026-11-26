import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import CasePathsCore
import CustomDump
import Encoding
import Fixture
import ScreenCapture
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

struct Step: Hashable {
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

@Suite(.timeLimit(.minutes(1)))
struct ModelTests {
    @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
    func modelsEachReportedControlWithItsKindAndLabelAndKeepsItsRefWhenItsWindowShowsAgainOrItsLinesReorder() async throws {
        let passes: [(report: Report, model: Model)] = try await FixtureSession.launched { fixture in
            let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
            let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(try Instance(application))
            var passes: [(report: Report, model: Model)] = []
            for command: Command in [.show(surface: .panel), .dismiss(surface: .controls), .show(surface: .controls), .reorder(from: 0, to: 2)] {
                try await fixture.send(command)
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let model: Model = try await watch.model(.application)
                passes.append((report, model))
            }
            return passes
        }
        let height: CGFloat = CGDisplayBounds(CGMainDisplayID()).height
        let rows: ((report: Report, model: Model)) -> [Row] = { pass in
            let visible: Set<Int> = Set(pass.report.windows.filter(\.visible).map(\.number))
            let placed: [(frame: CGRect, control: Control)] = pass.model.containers.flatMap(\.controls).compactMap { (control: Control) -> (frame: CGRect, control: Control)? in
                control.frame.map { seen in (CGRect(x: seen.minX, y: height - seen.maxY, width: seen.width, height: seen.height), control) }
            }
            .sorted { first, second in first.frame.width * first.frame.height < second.frame.width * second.frame.height }
            return pass.report.controls.compactMap { (reported: Report.Control) -> Row? in
                guard let row = expected[reported.entry], let window = reported.window, visible.contains(window), let frame = reported.frame else { return nil }
                return Row(
                    member: Member(entry: reported.entry, index: reported.member),
                    role: reported.role,
                    window: window,
                    wanted: Shown(kind: row.kind, label: reported[keyPath: row.label]),
                    found: placed.first { held in held.frame.contains(frame) }?.control,
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
            let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(try Instance(application))
            try await fixture.send(.report)
            let report: Report = try await fixture.next(\.report).value
            let bar: Container = try #require(try await watch.model(.application).containers.first { container in container.kind == .menuBar })
            var levels: [Model] = []
            for opener in bar.controls { levels.append(try await watch.model(.menu(ref: opener.ref))) }
            return (report, levels)
        }
        let specials: [NSEvent.SpecialKey: Key.Special] = [.f5: .f5]
        let held: [(flag: NSEvent.ModifierFlags, modifier: Modifier)] = [(.command, .command), (.shift, .shift), (.option, .option), (.control, .control), (.function, .function)]
        let pressed: [String: Step] = Dictionary(
            uniqueKeysWithValues: report.controls.compactMap { (control: Report.Control) -> (String, Step)? in
                guard let title = control.title, let shortcut = control.shortcut, case .known(let mask) = shortcut.modifiers, let scalar = shortcut.key.unicodeScalars.first else { return nil }
                let key: Key = specials[NSEvent.SpecialKey(rawValue: Int(scalar.value))].map(Key.special) ?? .character(Character(shortcut.key))
                return (title, Step(key: key, modifiers: Set(held.filter { flag, _ in mask.contains(flag) }.map(\.modifier))))
            }
        )
        let read: [String: Step] = Dictionary(
            levels.flatMap(\.containers).flatMap(\.controls).compactMap { (control: Control) -> (String, Step)? in
                guard let title = control.label?.text, pressed[title] != nil else { return nil }
                return (title, Step(key: control.shortcut?.key, modifiers: Set(control.shortcut?.modifiers ?? [])))
            }
        ) { first, _ in first }
        let covered: Bool = !pressed.isEmpty
        #expect(covered, "The fixture reports no menu item with a shortcut")
        expectNoDifference(read, pressed)
    }
}
