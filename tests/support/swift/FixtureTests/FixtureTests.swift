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
    let frame: CGRect?
    let parent: String?
    let help: String?
    let caption: String?
    let refused: [String: AXError]
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
            func read(_ element: AXUIElement, _ attribute: String) throws -> CFTypeRef? {
                var value: CFTypeRef?
                let answer: AXError = unsafe AXUIElementCopyAttributeValue(element, attribute as CFString, &value)
                let answered: Bool = answer == .success
                let refusal: Comment = "Reading \(attribute) of the fixture answered AXError \(answer.rawValue)"
                try #require(answered, refusal)
                return value
            }
            func decoded<Value: BitwiseCopyable>(_ slot: CFTypeRef, as type: AXValueType, from start: Value) throws -> Value {
                var value: Value = start
                let typed: Bool = unsafe CFGetTypeID(slot) == AXValueGetTypeID() && AXValueGetValue(unsafeDowncast(slot, to: AXValue.self), type, &value)
                let mistyped: Comment = "The fixture answered \(slot) where an AXValue of type \(type.rawValue) belongs"
                try #require(typed, mistyped)
                return value
            }
            func elements(_ slot: CFTypeRef?) -> [AXUIElement] {
                let held: [CFTypeRef] = (slot as? [CFTypeRef]) ?? slot.map { single in [single] } ?? []
                return held.compactMap { one in CFGetTypeID(one) == AXUIElementGetTypeID() ? unsafe unsafeDowncast(one, to: AXUIElement.self) : nil }
            }
            let attributes = [kAXRoleAttribute, kAXSubroleAttribute, kAXPositionAttribute, kAXSizeAttribute, kAXParentAttribute, kAXHelpAttribute, kAXTitleUIElementAttribute, kAXChildrenAttribute]
            let height = CGDisplayBounds(CGMainDisplayID()).height
            var pending = elements(try read(AXUIElementCreateApplication(fixture.process), kAXWindowsAttribute))
            var outside: [Read] = []
            while let element = pending.popLast() {
                var copied: CFArray?
                let answer: AXError = unsafe AXUIElementCopyMultipleAttributeValues(element, attributes as CFArray, [], &copied)
                let answered: Bool = answer == .success
                let refusal: Comment = "Reading \(attributes) of a fixture element answered AXError \(answer.rawValue)"
                try #require(answered, refusal)
                let slots: [String: CFTypeRef] = Dictionary(uniqueKeysWithValues: zip(attributes, try #require(copied as? [CFTypeRef])))
                let errors: [String: AXError] = try slots.filter { _, slot in CFGetTypeID(slot) == AXValueGetTypeID() && AXValueGetType(unsafe unsafeDowncast(slot, to: AXValue.self)) == .axError }
                    .mapValues { slot in try decoded(slot, as: .axError, from: AXError.success) }
                let values: [String: CFTypeRef] = slots.filter { attribute, slot in errors[attribute] == nil && CFGetTypeID(slot) != CFNullGetTypeID() }
                pending += elements(values[kAXChildrenAttribute])
                let origin: CGPoint? = try values[kAXPositionAttribute].map { slot in try decoded(slot, as: .cgPoint, from: CGPoint.zero) }
                let size: CGSize? = try values[kAXSizeAttribute].map { slot in try decoded(slot, as: .cgSize, from: CGSize.zero) }
                outside.append(
                    Read(
                        role: values[kAXRoleAttribute] as? String,
                        subrole: values[kAXSubroleAttribute] as? String,
                        frame: origin.flatMap { origin in size.map { size in CGRect(x: origin.x, y: height - origin.y - size.height, width: size.width, height: size.height) } },
                        parent: try elements(values[kAXParentAttribute]).first.flatMap { parent in try read(parent, kAXRoleAttribute) as? String },
                        help: values[kAXHelpAttribute] as? String,
                        caption: try elements(values[kAXTitleUIElementAttribute]).first.flatMap { title in try read(title, kAXValueAttribute) as? String },
                        refused: errors.filter { _, error in ![AXError.noValue, .attributeUnsupported].contains(error) },
                    )
                )
            }
            return (report, outside)
        }
        let visible: Set<Int> = Set(report.windows.filter(\.visible).map(\.number))
        let reported: [Read] = report.controls.filter { control in control.window.map { number throws(Never) in visible.contains(number) } ?? false }.compactMap { control in
            control.frame.map { frame in Read(role: control.role, subrole: control.subrole, frame: frame, parent: control.parent, help: control.help, caption: control.caption, refused: [:]) }
        }
        let frames: Set<CGRect> = Set(reported.compactMap(\.frame))
        let seen: Set<Read> = Set(
            outside.filter { read in
                !read.refused.isEmpty || (read.frame.map { frame throws(Never) in frames.contains(frame) } ?? false && read.role.map { role throws(Never) in documented.contains(role) } ?? false)
            }
        )
        let stated: Set<Read> = Set(reported.filter { read in read.role.map { role throws(Never) in documented.contains(role) } ?? false })
        expectNoDifference(seen, stated)
        expectNoDifference(documented.subtracting(reported.compactMap(\.role)), [])
    }
}
