public import ApplicationServices
import CasePaths
public import Encoding
public import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let buttonAttributes: [String] = [kAXCloseButtonAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute]

// --- [MODELS] --------------------------------------------------------------------------

public struct Control: Codable, Hashable, Sendable {
    public let id: Data
    public let role: String
    public let subrole: String?
    public let label: String?
    public let identifier: String?
    public let attribute: String?
    public let value: JSONValue?
    public let actions: [String]
    public let items: [Self]?
}

public struct FailedRead: Codable, Hashable, Sendable {
    public let reads: [String]
    public let role: String?
    public let label: String?
    public let error: AXError.RawValue
}

public struct Element: Hashable, Sendable {
    public let id: Data
    public let window: CGWindowID?
    public let role: String
    public let subrole: String?
    public let modal: Bool?
    public let identifier: String?
    public let description: String?
    public let title: String?
    public let text: [String]
    public let controls: [Control]
    public let actions: [String]
    public let unreadable: [FailedRead]

    public var requiresAnswer: Bool { role == kAXSheetRole || modal == true || subrole == kAXSystemDialogSubrole }
}

extension Element: Encodable {
    private enum Key: CodingKey {
        case id, window, role, subrole, modal, identifier, description, title, text, controls, actions, unreadable, requiresAnswer
    }

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encode(id, forKey: .id)
        try fields.encodeIfPresent(window, forKey: .window)
        try fields.encode(role, forKey: .role)
        try fields.encodeIfPresent(subrole, forKey: .subrole)
        try fields.encodeIfPresent(modal, forKey: .modal)
        try fields.encodeIfPresent(identifier, forKey: .identifier)
        try fields.encodeIfPresent(description, forKey: .description)
        try fields.encodeIfPresent(title, forKey: .title)
        try fields.encode(text, forKey: .text)
        try fields.encode(controls, forKey: .controls)
        try fields.encode(actions, forKey: .actions)
        try fields.encode(unreadable, forKey: .unreadable)
        try fields.encode(requiresAnswer, forKey: .requiresAnswer)
    }
}

private struct Part {
    let text: String?
    let control: Control?

    init(_ node: AXUIElement, attribute: String?) throws(ApplicationFailure) {
        let read: [String: CFTypeRef] = try node.values([kAXRoleAttribute, kAXSubroleAttribute, kAXEnabledAttribute, kAXValueAttribute, kAXIdentifierAttribute, kAXChildrenAttribute])
        let role: String? = read[kAXRoleAttribute] as? String
        let actions: [String] = try node.actionNames()
        let valued: Bool = role.map([kAXTextFieldRole, kAXTextAreaRole, kAXComboBoxRole, kAXPopUpButtonRole, kAXCheckBoxRole, kAXRadioButtonRole].contains) == true
        text = role == kAXStaticTextRole ? (read[kAXValueAttribute] as? String).flatMap { line in line.isEmpty ? nil : line } : nil
        control = try role.flatMap { role throws(ApplicationFailure) -> Control? in
            guard valued || read[kAXEnabledAttribute] as? Bool != false && actions.contains(kAXPressAction) else { return nil }
            let menus: [AXUIElement] = try (read[kAXChildrenAttribute] as? [CFTypeRef] ?? []).compactMap(AXUIElement.checked)
                .map { child throws(ApplicationFailure) in try child.value(kAXRoleAttribute) == kAXMenuRole ? child.children() : [] }.flatMap(\.self)
            return try Control(
                id: node.token,
                role: role,
                subrole: read[kAXSubroleAttribute] as? String,
                label: node.label(read),
                identifier: read[kAXIdentifierAttribute] as? String,
                attribute: attribute,
                value: read[kAXValueAttribute].flatMap(JSONValue.init(accessibility:)),
                actions: actions,
                items: menus.isEmpty ? nil : menus.map { item throws(ApplicationFailure) in try Self(item, attribute: nil).control }.compactMap(\.self),
            )
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension AXUIElement {
    func candidates() throws(ApplicationFailure) -> [AXUIElement] {
        let bar: AXUIElement? = try element(kAXMenuBarAttribute)
        let menus: [AXUIElement] = try (bar?.children() ?? []).map { item throws(ApplicationFailure) in try item.children() }.flatMap(\.self)
        let windows: [AXUIElement] = try elements(kAXWindowsAttribute)
        return try menus + windows.map { window throws(ApplicationFailure) in try [window] + window.children { role in role == kAXSheetRole } }.flatMap(\.self) + [bar].compactMap(\.self)
    }

    func descendants() throws(ApplicationFailure) -> [AXUIElement] {
        let unwalked: Set<String?> = [kAXSheetRole, kAXScrollAreaRole, kAXOutlineRole, kAXTableRole, kAXListRole, kAXBrowserRole]
        return try [self] + children { role in !unwalked.contains(role) }.map { child throws(ApplicationFailure) in try child.descendants() }.flatMap(\.self)
    }

    func described(walking walked: (Element) -> Bool) throws(ApplicationFailure) -> Element? {
        let read: [String: CFTypeRef] = try values(
            [kAXRoleAttribute, kAXSubroleAttribute, kAXModalAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute] + buttonAttributes
        )
        guard let role: String = read[kAXRoleAttribute] as? String else { return nil }
        let window: CGWindowID? = try window()
        guard window != nil || role == kAXMenuBarRole else { return nil }
        let title: String? =
            try role == kAXMenuRole
            ? element(kAXParentAttribute).flatMap { parent throws(ApplicationFailure) in try parent.label(parent.values([])) }
            : (read[kAXTitleAttribute] as? String).flatMap { title in title.isEmpty ? nil : title }
        let (id, actions): (Data, [String]) = try (token, actionNames())
        let build: ([String], [Control], [FailedRead]) -> Element = { text, controls, unreadable in
            Element(
                id: id,
                window: window,
                role: role,
                subrole: read[kAXSubroleAttribute] as? String,
                modal: read[kAXModalAttribute] as? Bool,
                identifier: read[kAXIdentifierAttribute] as? String,
                description: read[kAXDescriptionAttribute] as? String,
                title: title,
                text: text,
                controls: controls,
                actions: actions,
                unreadable: unreadable,
            )
        }
        let outline: Element = build([], [], [])
        guard walked(outline) else { return [kAXMenuRole, kAXMenuBarRole].contains(role) ? nil : outline }
        let marked: [(attribute: String, node: AXUIElement)] = buttonAttributes.compactMap { attribute in
            read[attribute].flatMap(Self.checked).map { node in (attribute: attribute, node: node) }
        }
        let nodes: [AXUIElement] = try [kAXMenuRole, kAXMenuBarRole].contains(role) ? children() : descendants()
        let reads: [Result<Part, ApplicationFailure>] = (nodes + marked.map(\.node).filter { node in !nodes.contains(node) }).map { node in
            Result { () throws(ApplicationFailure) in try Part(node, attribute: marked.first { mark in mark.node == node }?.attribute) }
        }
        let parts: [Part] = try reads.map { read throws(ApplicationFailure) -> Part? in
            switch read {
                case .success(let part): part
                case .failure(let failure) where failure.reason.is(\.unreadable): nil
                case .failure(let failure): throw failure
            }
        }
        .compactMap(\.self)
        return build(
            parts.compactMap(\.text),
            parts.compactMap(\.control),
            reads.compactMap { read in read[case: \.failure]?.reason[case: \.unreadable] },
        )
    }
}
