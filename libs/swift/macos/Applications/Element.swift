public import ApplicationServices
public import Encoding
public import Foundation
public import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

private let labelAttributes: [String] = [kAXTitleAttribute, kAXDescriptionAttribute]
let messagingTimeout: Duration = .seconds(1)

// --- [MODELS] --------------------------------------------------------------------------

public struct Control: Codable, Hashable, Sendable {
    public let role: String
    public let label: String?
    public let value: JSONValue?
}

public struct FailedRead: Codable, Hashable, Sendable {
    public let reads: [String]
    public let role: String?
    public let label: String?
    public let error: AXError.RawValue

    public init(_ error: AXError, reading reads: [String], on element: AXUIElement) {
        let named: [String] = [kAXRoleAttribute] + labelAttributes
        var values: CFArray?
        _ = unsafe AXUIElementCopyMultipleAttributeValues(element, named as CFArray, [], &values)
        let attributes: [String: CFTypeRef] = Dictionary(uniqueKeysWithValues: zip(named, values as? [CFTypeRef] ?? []))
        self.reads = reads
        role = attributes[kAXRoleAttribute] as? String
        label = AXUIElement.ownLabel(attributes)
        self.error = error.rawValue
    }
}

public struct Element: Codable, Hashable, Sendable {
    public let id: CGWindowID
    public let role: String
    public let subrole: String?
    public let title: String?
    public let text: [String]
    public let controls: [Control]
    public let actions: [String]
    public let unreadable: [FailedRead]
}

struct Described {
    let element: Element
    let targets: [(name: String, node: AXUIElement, action: String)]
    let fields: [(label: String, node: AXUIElement)]
}

private struct Part {
    let node: AXUIElement
    let label: String?
    let actions: [String]
    let text: String?
    let control: Control?

    var press: String? { label.flatMap { label in actions.contains(kAXPressAction) && !["...", "…"].contains { ellipsis in label.hasSuffix(ellipsis) } ? label : nil } }

    init(_ node: AXUIElement) throws(ElementFailure) {
        let attributes: [String: CFTypeRef] = try node.values([kAXRoleAttribute, kAXEnabledAttribute, kAXValueAttribute] + labelAttributes)
        let role: String? = attributes[kAXRoleAttribute] as? String
        let label: String? = try node.label(attributes)
        self.node = node
        self.label = label
        actions = try label == nil || attributes[kAXEnabledAttribute] as? Bool == false ? [] : node.actionNames()
        text = role == kAXStaticTextRole ? (attributes[kAXValueAttribute] as? String).flatMap { line in line.isEmpty ? nil : line } : nil
        control = role.flatMap { role in
            switch role {
                case kAXTextFieldRole, kAXTextAreaRole, kAXComboBoxRole, kAXPopUpButtonRole, kAXCheckBoxRole, kAXRadioButtonRole:
                    Control(role: role, label: label, value: attributes[kAXValueAttribute].flatMap(JSONValue.init(accessibility:)))
                default: nil
            }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ElementFailure: CustomNSError, LocalizedError {
    case untrusted
    case unanswered
    case unreadable(FailedRead)
    case unobservable(AggregateError<(notification: String, error: AXError)>)
    case absent(CGWindowID)
    case unmatched(Unmatched)
    case refused(String, AXError)

    static func reading(_ error: AXError, _ reads: [String], on element: AXUIElement) -> Self {
        switch error {
            case .apiDisabled: .untrusted
            case .cannotComplete: .unanswered
            default: .unreadable(FailedRead(error, reading: reads, on: element))
        }
    }

    public var errorDescription: String? {
        switch self {
            case .untrusted: "Accessibility access is off for this process: allow it under System Settings, Privacy & Security, Accessibility"
            case .unanswered: "The application did not answer an Accessibility request within the messaging timeout"
            case .unreadable(let read):
                "Accessibility read of \(read.reads.joined(separator: ", ")) on \(read.role ?? "an element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error)"
            case .unobservable(let refusals):
                "The application refused Accessibility notifications: \(refusals.errors.map { refusal in "\(refusal.notification) (AXError \(refusal.error.rawValue))" }.joined(separator: ", "))"
            case .absent(let id): "No open window, dialog, sheet, or menu with id \(id)"
            case .unmatched(let miss): miss.errorDescription
            case .refused(let action, let error): "Performing \(action) returned AXError \(error.rawValue)"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .unmatched(let miss) = self { [NSUnderlyingErrorKey: miss] } else { [:] }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

let accessibility: DispatchQueue = {
    AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
    return DispatchQueue(label: "Applications.accessibility", attributes: .concurrent)
}()

// --- [OPERATIONS] ----------------------------------------------------------------------

extension JSONValue {
    init?(accessibility value: CFTypeRef) {
        switch value {
            case let flag as CFBoolean where CFGetTypeID(value) == CFBooleanGetTypeID(): self = .bool(CFBooleanGetValue(flag))
            case let number as NSNumber: self = .number(number.decimalValue)
            case let text as String: self = .string(text)
            default: return nil
        }
    }
}

extension AXError {
    init?(_ slot: CFTypeRef) {
        switch slot {
            case let value as AXValue where CFGetTypeID(value) == AXValueGetTypeID() && AXValueGetType(value) == .axError:
                var error: AXError = .success
                _ = unsafe AXValueGetValue(value, .axError, &error)
                self = error
            default: return nil
        }
    }
}

extension AXUIElement {
    // --- [READS]
    func value<Value>(_ attribute: String) throws(ElementFailure) -> Value? {
        var value: CFTypeRef?
        return switch unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) {
            case .success: value as? Value
            case .noValue, .attributeUnsupported: nil
            case let error: throw .reading(error, [attribute], on: self)
        }
    }

    func values(_ attributes: [String]) throws(ElementFailure) -> [String: CFTypeRef] {
        var values: CFArray?
        return switch unsafe AXUIElementCopyMultipleAttributeValues(self, attributes as CFArray, [], &values) {
            case .success:
                try Dictionary(
                    uniqueKeysWithValues: zip(attributes, values as? [CFTypeRef] ?? []).map { attribute, slot throws(ElementFailure) -> (String, CFTypeRef)? in
                        switch (AXError(slot), slot is NSNull) {
                            case (.some(.noValue), _), (.some(.attributeUnsupported), _), (.none, true): nil
                            case (.some(let error), _): throw .reading(error, [attribute], on: self)
                            case (.none, false): (attribute, slot)
                        }
                    }
                    .compactMap(\.self)
                )
            case let error: throw .reading(error, attributes, on: self)
        }
    }

    func window() throws(ElementFailure) -> CGWindowID? {
        var id: CGWindowID = 0
        return switch unsafe windowIdentifier(self, &id) {
            case .success: id
            case let error where error == .cannotComplete || error == .apiDisabled: throw .reading(error, ["_AXUIElementGetWindow"], on: self)
            default: nil
        }
    }

    func actionNames() throws(ElementFailure) -> [String] {
        var names: CFArray?
        return switch unsafe AXUIElementCopyActionNames(self, &names) {
            case .success: names as? [String] ?? []
            case .failure: []
            case let error: throw .reading(error, ["AXUIElementCopyActionNames"], on: self)
        }
    }

    func children() throws(ElementFailure) -> [AXUIElement] { try value(kAXChildrenAttribute) ?? [] }

    func children(where kept: (String?) -> Bool) throws(ElementFailure) -> [AXUIElement] {
        try children().map { child throws(ElementFailure) in try kept(child.value(kAXRoleAttribute)) ? child : nil }.compactMap(\.self)
    }

    static func ownLabel(_ attributes: [String: CFTypeRef]) -> String? {
        labelAttributes.lazy.compactMap { attribute in attributes[attribute] as? String }.first { label in !label.isEmpty }
    }

    func label(_ attributes: [String: CFTypeRef]) throws(ElementFailure) -> String? {
        switch Self.ownLabel(attributes) {
            case .some(let label): label
            case .none: try (value(kAXTitleUIElementAttribute) as AXUIElement?)?.value(kAXValueAttribute)
        }
    }

    // --- [WALK]
    func candidates() throws(ElementFailure) -> [AXUIElement] {
        let menus: [AXUIElement] = try ((value(kAXMenuBarAttribute) as AXUIElement?)?.children() ?? []).map { item throws(ElementFailure) in try item.children() }.flatMap(\.self)
        return try menus + children { role in role != kAXMenuBarRole }.map { child throws(ElementFailure) in try child.children { role in role == kAXSheetRole } + [child] }.flatMap(\.self)
    }

    func items() throws(ElementFailure) -> [(name: String, node: AXUIElement, action: String?)] {
        try children().map { child throws(ElementFailure) in try child.value(kAXRoleAttribute) == kAXMenuRole ? child.children() : [child] }.flatMap(\.self)
            .map { node throws(ElementFailure) in try Part(node) }
            .compactMap { part in part.label.map { label in (label, part.node, part.press.map { _ in kAXPressAction }) } }
    }

    func descendants() throws(ElementFailure) -> [AXUIElement] {
        let collections: Set<String?> = [kAXScrollAreaRole, kAXOutlineRole, kAXTableRole, kAXListRole, kAXBrowserRole]
        return try [self] + children { role in !collections.contains(role) }.map { child throws(ElementFailure) in try child.descendants() }.flatMap(\.self)
    }

    func described(window id: CGWindowID) throws(ElementFailure) -> Described {
        let reads: some Sequence<Result<AXUIElement?, ElementFailure>> = try candidates().lazy.map { candidate in Result { () throws(ElementFailure) in try candidate.window() == id ? candidate : nil }
        }
        return switch try reads.first(where: { read in if case .success(.none) = read { false } else { true } })?.get()?.described() {
            case .some(let described): described
            case .none: throw .absent(id)
        }
    }

    func described() throws(ElementFailure) -> Described? {
        func separated<Value>(_ reads: [Result<Value, ElementFailure>]) throws(ElementFailure) -> ([Value], [FailedRead]) {
            let kept: [(value: Value?, unreadable: FailedRead?)] = try reads.map { read throws(ElementFailure) in
                switch read {
                    case .success(let value): (value, nil)
                    case .failure(.unreadable(let failed)): (nil, failed)
                    case .failure(let error): throw error
                }
            }
            return (kept.compactMap(\.value), kept.compactMap(\.unreadable))
        }
        guard let window: CGWindowID = try window() else { return nil }
        let attributes: [String: CFTypeRef] = try values([kAXRoleAttribute, kAXSubroleAttribute, kAXModalAttribute] + labelAttributes)
        guard let role: String = attributes[kAXRoleAttribute] as? String else { return nil }
        let subrole: String? = attributes[kAXSubroleAttribute] as? String
        let nodes: [AXUIElement] =
            switch (subrole, role, attributes[kAXModalAttribute] as? Bool) {
                case (.some(kAXDialogSubrole), _, _), (.some(kAXSystemDialogSubrole), _, _), (_, kAXSheetRole, _), (_, kAXLayoutAreaRole, _), (_, _, .some(true)): try descendants()
                case (_, kAXMenuRole, _): try children()
                default: []
            }
        let (parts, unreadableParts): ([Part], [FailedRead]) = try separated(nodes.map { node in Result { () throws(ElementFailure) in try Part(node) } })
        let (buttons, unreadableButtons): ([(name: String, node: AXUIElement, action: String)?], [FailedRead]) = try separated(
            [kAXCloseButtonAttribute, kAXCancelButtonAttribute, kAXDefaultButtonAttribute].map { attribute in
                Result { () throws(ElementFailure) in try (value(attribute) as AXUIElement?).map { button in (name: attribute, node: button, action: kAXPressAction) } }
            }
        )
        let cancels: [(name: String, node: AXUIElement, action: String)] = try actionNames().contains(kAXCancelAction) ? [(kAXCancelAction, self, kAXCancelAction)] : []
        let targets: [(name: String, node: AXUIElement, action: String)] =
            parts.compactMap { part in part.press.map { label in (label, part.node, kAXPressAction) } } + buttons.compactMap(\.self) + cancels
        let title: String? =
            try role == kAXMenuRole
            ? (value(kAXParentAttribute) as AXUIElement?).flatMap { parent throws(ElementFailure) in try parent.label(parent.values(labelAttributes)) } : label(attributes)
        return Described(
            element: Element(
                id: window,
                role: role,
                subrole: subrole,
                title: title,
                text: parts.compactMap(\.text),
                controls: parts.compactMap(\.control),
                actions: targets.map(\.name),
                unreadable: unreadableParts + unreadableButtons,
            ),
            targets: targets,
            fields: parts.compactMap { part in part.control?.label.map { label in (label, part.node) } },
        )
    }
}

@_silgen_name("_AXUIElementGetWindow")
private func windowIdentifier(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError
