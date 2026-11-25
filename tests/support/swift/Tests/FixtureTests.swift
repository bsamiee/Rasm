import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
import CustomDump
import Fixture
import Testing

// --- [MODELS] --------------------------------------------------------------------------

struct Read: Hashable {
    let role: String?
    let subrole: String?
    let frame: CGRect
    let parent: String?
    let help: String?
    let caption: String?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct FixtureTests {
    @Test(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
    func reportsEveryMemberOnScreenAsAnOutsideAccessibilityReadOfTheProcessFindsIt() async throws {
        let roles: [NSAccessibility.Role] = [
            .button, .checkBox, .radioButton, .popUpButton, .comboBox, .tabGroup, .radioGroup, .table, .outline, .slider, .incrementor, .textField, .colorWell, .dateTimeAreaRole,
        ]
        let documented: Set<String> = Set(roles.map(\.rawValue))
        let (report, outside): (Report, [Read]) = try await FixtureSession.launched { fixture in
            try await fixture.send(.show(surface: .panel))
            try await fixture.send(.report)
            let report = try await fixture.next(\.report).value
            func read(_ element: AXUIElement, _ attribute: String) -> (answer: AXError, value: CFTypeRef?) {
                var value: CFTypeRef?
                return (unsafe AXUIElementCopyAttributeValue(element, attribute as CFString, &value), value)
            }
            func elements(_ slot: CFTypeRef?) -> [AXUIElement] {
                let held: [CFTypeRef] = (slot as? [CFTypeRef]) ?? slot.map { single in [single] } ?? []
                return held.compactMap { one in CFGetTypeID(one) == AXUIElementGetTypeID() ? unsafe unsafeDowncast(one, to: AXUIElement.self) : nil }
            }
            let attributes = [kAXRoleAttribute, kAXSubroleAttribute, kAXPositionAttribute, kAXSizeAttribute, kAXParentAttribute, kAXHelpAttribute, kAXTitleUIElementAttribute, kAXChildrenAttribute]
            let height = CGDisplayBounds(CGMainDisplayID()).height
            let windows = read(AXUIElementCreateApplication(fixture.process), kAXWindowsAttribute)
            let readable: Bool = windows.answer == AXError.success
            let refusal: Comment = "Reading the fixture's windows answered \(windows.answer.rawValue)"
            try #require(readable, refusal)
            var pending = elements(windows.value)
            var outside: [Read] = []
            while let element = pending.popLast() {
                var copied: CFArray?
                _ = unsafe AXUIElementCopyMultipleAttributeValues(element, attributes as CFArray, [], &copied)
                let slots: [String: CFTypeRef] = Dictionary(uniqueKeysWithValues: zip(attributes, copied as? [CFTypeRef] ?? []))
                pending += elements(slots[kAXChildrenAttribute])
                var origin = CGPoint.zero
                var size = CGSize.zero
                guard let position = slots[kAXPositionAttribute], let extent = slots[kAXSizeAttribute], CFGetTypeID(position) == AXValueGetTypeID(), CFGetTypeID(extent) == AXValueGetTypeID(),
                    unsafe AXValueGetValue(unsafeDowncast(position, to: AXValue.self), .cgPoint, &origin), unsafe AXValueGetValue(unsafeDowncast(extent, to: AXValue.self), .cgSize, &size)
                else { continue }
                outside.append(
                    Read(
                        role: slots[kAXRoleAttribute] as? String,
                        subrole: slots[kAXSubroleAttribute] as? String,
                        frame: CGRect(x: origin.x, y: height - origin.y - size.height, width: size.width, height: size.height),
                        parent: elements(slots[kAXParentAttribute]).first.flatMap { parent in read(parent, kAXRoleAttribute).value as? String },
                        help: slots[kAXHelpAttribute] as? String,
                        caption: elements(slots[kAXTitleUIElementAttribute]).first.flatMap { title in read(title, kAXValueAttribute).value as? String },
                    )
                )
            }
            return (report, outside)
        }
        let visible: Set<Int> = Set(report.windows.filter(\.visible).map(\.number))
        let reported: [Read] = report.controls.filter { control in control.window.map { number throws(Never) in visible.contains(number) } ?? false }.compactMap { control in
            control.frame.map { frame in Read(role: control.role, subrole: control.subrole, frame: frame, parent: control.parent, help: control.help, caption: control.caption) }
        }
        let frames: Set<CGRect> = Set(reported.map(\.frame))
        let seen: Set<Read> = Set(outside.filter { read in frames.contains(read.frame) && read.role.map { role throws(Never) in documented.contains(role) } ?? false })
        let stated: Set<Read> = Set(reported.filter { read in read.role.map { role throws(Never) in documented.contains(role) } ?? false })
        expectNoDifference(seen, stated)
        let shown: Set<String> = Set(reported.compactMap(\.role))
        let covered: Bool = shown.isSuperset(of: documented)
        #expect(covered)
    }
}
