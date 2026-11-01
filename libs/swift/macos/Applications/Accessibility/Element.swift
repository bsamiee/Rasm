public import ApplicationServices
import CasePaths
public import Encoding
public import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let buttonAttributes: [String] = [kAXCloseButtonAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute]
private let outlineAttributes: [String] =
    [
        kAXRoleAttribute, kAXSubroleAttribute, kAXModalAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXDocumentAttribute, kAXMainAttribute,
        kAXFocusedAttribute, kAXMinimizedAttribute, kAXPositionAttribute, kAXSizeAttribute,
    ] + buttonAttributes
private let nodeAttributes: [String] = [
    kAXRoleAttribute, kAXSubroleAttribute, kAXEnabledAttribute, kAXIdentifierAttribute, kAXChildrenAttribute, kAXVisibleRowsAttribute, kAXVisibleChildrenAttribute,
]
private let valuedRoles: Set<String> = [kAXStaticTextRole, kAXTextFieldRole, kAXComboBoxRole, kAXPopUpButtonRole, kAXCheckBoxRole, kAXRadioButtonRole]
private let menuRoles: Set<String> = [kAXMenuRole, kAXMenuBarRole]

// --- [MODELS] --------------------------------------------------------------------------

public struct Control: Codable, Hashable, Sendable {
    public let id: Data
    public let role: String
    public let subrole: String?
    public let label: String?
    public let identifier: String?
    public let attribute: String?
    public let value: JSONValue?
    public let enabled: Bool
    public let actions: [String]
    public let items: [Self]?
}

public struct FailedRead: Codable, Hashable, Sendable {
    public let reads: [String]
    public let role: String?
    public let label: String?
    public let error: AXError.RawValue
}

public struct Element: Encodable, Hashable, Sendable {
    public struct Detail: Encodable, Hashable, Sendable {
        public let frame: CGRect?
        public let main: Bool?
        public let focused: Bool?
        public let minimized: Bool?
        public let text: [String]
        public let controls: [Control]
        public let unreadable: [FailedRead]
    }

    public let id: Data
    public let window: CGWindowID?
    public let role: String
    public let subrole: String?
    public let modal: Bool?
    public let identifier: String?
    public let description: String?
    public let title: String?
    public let document: URL?
    public let edited: Bool?
    public let actions: [String]
    public let requiresAnswer: Bool
    public let detail: Detail?
}

private struct Part {
    let text: String?
    let control: Control?

    init(_ node: AXUIElement, read: [String: CFTypeRef], attribute: String?) throws(ApplicationFailure) {
        let role: String? = read[kAXRoleAttribute] as? String
        let value: CFTypeRef? =
            switch role {
                case kAXTextAreaRole: try node.visibleText().map { text in text as CFString }
                case let role? where valuedRoles.contains(role): try node.values([kAXValueAttribute])[kAXValueAttribute]
                case _: nil
            }
        let actions: [String] = try node.actionNames()
        let enabled: Bool = read[kAXEnabledAttribute] as? Bool ?? true
        text = role == kAXStaticTextRole ? (value as? String).flatMap { line in line.isEmpty ? nil : line } : nil
        control = try role.flatMap { role throws(ApplicationFailure) -> Control? in
            let label: String? = try node.label(read)
            guard role != kAXStaticTextRole, role != kAXMenuItemRole || label != nil, value != nil || actions.contains(kAXPressAction) else { return nil }
            let menus: [AXUIElement] = try (read[kAXChildrenAttribute] as? [CFTypeRef] ?? []).compactMap(AXUIElement.checked)
                .map { child throws(ApplicationFailure) in try child.value(kAXRoleAttribute) == kAXMenuRole ? child.elements(kAXChildrenAttribute) : [] }.flatMap(\.self)
            return try Control(
                id: node.token,
                role: role,
                subrole: read[kAXSubroleAttribute] as? String,
                label: label,
                identifier: read[kAXIdentifierAttribute] as? String,
                attribute: attribute,
                value: value.flatMap(JSONValue.init(accessibility:)),
                enabled: enabled,
                actions: actions,
                items: menus.isEmpty ? nil : menus.map { item throws(ApplicationFailure) in try Self(item, read: item.values(nodeAttributes), attribute: nil).control }.compactMap(\.self),
            )
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Element {
    init(_ node: AXUIElement, window: CGWindowID?, role: String, read: [String: CFTypeRef], title: String?, edited: Bool?) throws(ApplicationFailure) {
        let (subrole, modal): (String?, Bool?) = (read[kAXSubroleAttribute] as? String, read[kAXModalAttribute] as? Bool)
        try self.init(
            id: node.token,
            window: window,
            role: role,
            subrole: subrole,
            modal: modal,
            identifier: read[kAXIdentifierAttribute] as? String,
            description: read[kAXDescriptionAttribute] as? String,
            title: title,
            document: (read[kAXDocumentAttribute] as? String).flatMap { document in URL(string: document) },
            edited: edited,
            actions: node.actionNames(),
            requiresAnswer: role == kAXSheetRole || modal == true || subrole == kAXSystemDialogSubrole,
            detail: nil,
        )
    }

    func detailed(_ detail: Detail) -> Self {
        Self(
            id: id,
            window: window,
            role: role,
            subrole: subrole,
            modal: modal,
            identifier: identifier,
            description: description,
            title: title,
            document: document,
            edited: edited,
            actions: actions,
            requiresAnswer: requiresAnswer,
            detail: detail,
        )
    }
}

extension AXUIElement {
    func candidates() throws(ApplicationFailure) -> [AXUIElement] {
        let bar: AXUIElement? = try element(kAXMenuBarAttribute)
        let menus: [AXUIElement] = try (bar?.elements(kAXChildrenAttribute).dropFirst() ?? []).map { item throws(ApplicationFailure) in try item.elements(kAXChildrenAttribute) }.flatMap(\.self)
        let windows: [AXUIElement] = try elements(kAXWindowsAttribute).map { window throws(ApplicationFailure) in
            try [window] + window.elements(kAXChildrenAttribute).map { child throws(ApplicationFailure) in try child.value(kAXRoleAttribute) == kAXSheetRole ? child : nil }.compactMap(\.self)
        }
        .flatMap(\.self)
        return menus + windows + [bar].compactMap(\.self)
    }

    func descendants(skipping skipped: Set<String> = []) throws(ApplicationFailure) -> [(node: AXUIElement, read: [String: CFTypeRef])] {
        let read: [String: CFTypeRef] = try values(nodeAttributes)
        let role: String? = read[kAXRoleAttribute] as? String
        guard role.map(skipped.contains) != true else { return [] }
        let nested: CFTypeRef? =
            switch role {
                case kAXBrowserRole: nil
                case kAXTableRole, kAXOutlineRole: read[kAXVisibleRowsAttribute]
                case kAXListRole: read[kAXVisibleChildrenAttribute] ?? read[kAXChildrenAttribute]
                case _: read[kAXChildrenAttribute]
            }
        let children: [AXUIElement] = (nested as? [CFTypeRef] ?? []).compactMap(Self.checked)
        return try [(node: self, read: read)] + children.map { child throws(ApplicationFailure) in try child.descendants(skipping: [kAXSheetRole]) }.flatMap(\.self)
    }

    func described(walking walked: (Element) -> Bool) throws(ApplicationFailure) -> Element? {
        let read: [String: CFTypeRef] = try values(outlineAttributes)
        guard let role: String = read[kAXRoleAttribute] as? String else { return nil }
        let window: CGWindowID? = try window()
        guard window != nil || role == kAXMenuBarRole else { return nil }
        let marked: [(attribute: String, node: AXUIElement)] = buttonAttributes.compactMap { attribute in
            read[attribute].flatMap(Self.checked).map { node in (attribute: attribute, node: node) }
        }
        let title: String? =
            try role == kAXMenuRole
            ? element(kAXParentAttribute).flatMap { parent throws(ApplicationFailure) in try parent.label(parent.values([])) }
            : (read[kAXTitleAttribute] as? String).flatMap { title in title.isEmpty ? nil : title }
        let edited: Bool? = try marked.first { mark in mark.attribute == kAXCloseButtonAttribute }?.node.value(kAXEditedAttribute)
        let outline: Element = try Element(self, window: window, role: role, read: read, title: title, edited: edited)
        guard walked(outline) else { return menuRoles.contains(role) ? nil : outline }
        let walk: [(node: AXUIElement, read: [String: CFTypeRef])] =
            try menuRoles.contains(role)
            ? elements(kAXChildrenAttribute).dropFirst(role == kAXMenuBarRole ? 1 : 0).map { child throws(ApplicationFailure) in (node: child, read: try child.values(nodeAttributes)) }
            : descendants()
        let unwalked: [(node: AXUIElement, read: [String: CFTypeRef])] = try marked.map(\.node).filter { node in !walk.contains { step in step.node == node } }
            .map { node throws(ApplicationFailure) in (node: node, read: try node.values(nodeAttributes)) }
        let reads: [Result<Part, ApplicationFailure>] = (walk + unwalked).map { step in
            Result { () throws(ApplicationFailure) in try Part(step.node, read: step.read, attribute: marked.first { mark in mark.node == step.node }?.attribute) }
        }
        let parts: [Part] = try reads.map { read throws(ApplicationFailure) -> Part? in
            switch read {
                case .success(let part): part
                case .failure(let failure) where failure.reason.is(\.unreadable): nil
                case .failure(let failure): throw failure
            }
        }
        .compactMap(\.self)
        let origin: CGPoint? = AXValue.unpacked(read[kAXPositionAttribute], .cgPoint, CGPoint.zero)
        let size: CGSize? = AXValue.unpacked(read[kAXSizeAttribute], .cgSize, CGSize.zero)
        return outline.detailed(
            Element.Detail(
                frame: origin.flatMap { origin in size.map { size in CGRect(origin: origin, size: size) } },
                main: read[kAXMainAttribute] as? Bool,
                focused: read[kAXFocusedAttribute] as? Bool,
                minimized: read[kAXMinimizedAttribute] as? Bool,
                text: parts.compactMap(\.text),
                controls: parts.compactMap(\.control),
                unreadable: reads.compactMap { read in read[case: \.failure]?.reason[case: \.unreadable] },
            )
        )
    }
}
