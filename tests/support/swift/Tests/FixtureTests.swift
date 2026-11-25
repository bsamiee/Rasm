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
    @Test
    func reportsEveryMemberOnScreenAsAnOutsideAccessibilityReadOfTheProcessFindsIt() async throws {
        let documented = Set(
            [NSAccessibility.Role.button, .checkBox, .radioButton, .popUpButton, .comboBox, .tabGroup, .radioGroup, .table, .outline, .slider, .incrementor, .textField, .colorWell, .dateTimeArea]
                .map(\.rawValue)
        )
        let (report, outside) = try await FixtureSession.launched { fixture in
            try await fixture.send(.show(surface: .panel))
            try await fixture.send(.report)
            let report = try await fixture.next(\.report).value
            func read(_ element: AXUIElement, _ attribute: String) -> CFTypeRef? {
                var value: CFTypeRef?
                return unsafe AXUIElementCopyAttributeValue(element, attribute as CFString, &value) == .success ? value : nil
            }
            func elements(_ slot: CFTypeRef?) -> [AXUIElement] {
                ((slot as? [CFTypeRef]) ?? slot.map { single in [single] } ?? []).compactMap { held in
                    CFGetTypeID(held) == AXUIElementGetTypeID() ? unsafe unsafeDowncast(held, to: AXUIElement.self) : nil
                }
            }
            let attributes = [kAXRoleAttribute, kAXSubroleAttribute, kAXPositionAttribute, kAXSizeAttribute, kAXParentAttribute, kAXHelpAttribute, kAXTitleUIElementAttribute, kAXChildrenAttribute]
            let height = CGDisplayBounds(CGMainDisplayID()).height
            var pending = elements(read(AXUIElementCreateApplication(fixture.process), kAXWindowsAttribute))
            var outside: [Read] = []
            while let element = pending.popLast() {
                var copied: CFArray?
                _ = unsafe AXUIElementCopyMultipleAttributeValues(element, attributes as CFArray, [], &copied)
                let slots = Dictionary(uniqueKeysWithValues: zip(attributes, copied as? [CFTypeRef] ?? []))
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
                        parent: elements(slots[kAXParentAttribute]).first.flatMap { parent in read(parent, kAXRoleAttribute) as? String },
                        help: slots[kAXHelpAttribute] as? String,
                        caption: elements(slots[kAXTitleUIElementAttribute]).first.flatMap { title in read(title, kAXValueAttribute) as? String },
                    )
                )
            }
            return (report, outside)
        }
        let visible = Set(report.windows.filter(\.visible).map(\.number))
        let reported = report.controls.filter { control in control.window.map(visible.contains) ?? false }.compactMap { control in
            control.frame.map { frame in Read(role: control.role, subrole: control.subrole, frame: frame, parent: control.parent, help: control.help, caption: control.caption) }
        }
        let frames = Set(reported.map(\.frame))
        expectNoDifference(
            Set(outside.filter { read in frames.contains(read.frame) && read.role.map(documented.contains) ?? false }),
            Set(reported.filter { read in read.role.map(documented.contains) ?? false }),
        )
        #expect(Set(reported.compactMap(\.role)).isSuperset(of: documented))
    }
}
