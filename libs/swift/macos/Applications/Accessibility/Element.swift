public import ApplicationServices
import CasePaths
public import Encoding
public import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let buttonAttributes: [String] = [kAXCloseButtonAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute]
private let childAttributes: [String] = [kAXChildrenAttribute, kAXVisibleRowsAttribute, kAXVisibleChildrenAttribute]
private let outlineAttributes: [String] =
    [
        kAXRoleAttribute, kAXSubroleAttribute, kAXModalAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXDocumentAttribute, kAXMainAttribute,
        kAXFocusedAttribute, kAXMinimizedAttribute, kAXPositionAttribute, kAXSizeAttribute, kAXParentAttribute,
    ] + buttonAttributes + childAttributes
private let nodeAttributes: [String] =
    [kAXRoleAttribute, kAXSubroleAttribute, kAXEnabledAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXTitleUIElementAttribute] + childAttributes
private let valuedRoles: Set<String> = [kAXStaticTextRole, kAXTextFieldRole, kAXComboBoxRole, kAXPopUpButtonRole, kAXCheckBoxRole, kAXRadioButtonRole]
private let menuRoles: Set<String> = [kAXMenuRole, kAXMenuBarRole]

// --- [MODELS] --------------------------------------------------------------------------

public struct Control: Encodable, Hashable, Sendable {
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

public struct FailedRead: Encodable, Hashable, Sendable {
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

@CasePathable
@dynamicMemberLookup
private enum Part {
    case text(String)
    case control(Control)
    case unreadable(FailedRead)

    init(_ failure: ApplicationFailure) throws(ApplicationFailure) {
        guard let read: FailedRead = failure.reason[case: \.unreadable] else { throw failure }
        self = .unreadable(read)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension AXUIElement {
    func candidates() throws(ApplicationFailure) -> [AXUIElement] {
        let bar: AXUIElement? = try element(kAXMenuBarAttribute)
        let windows: [AXUIElement] = try elements(kAXWindowsAttribute).map { window throws(ApplicationFailure) in
            try [window] + window.elements(kAXChildrenAttribute).map { child throws(ApplicationFailure) in try child.value(kAXRoleAttribute) == kAXSheetRole ? child : nil }.compactMap(\.self)
        }
        .flatMap(\.self)
        return windows + [bar].compactMap(\.self)
    }

    private static func children(_ read: [String: CFTypeRef]) -> [AXUIElement] {
        let role: String? = read[kAXRoleAttribute] as? String
        let nested: CFTypeRef? =
            switch role {
                case kAXBrowserRole: nil
                case kAXTableRole, kAXOutlineRole: read[kAXVisibleRowsAttribute]
                case kAXListRole: read[kAXVisibleChildrenAttribute] ?? read[kAXChildrenAttribute]
                case _: read[kAXChildrenAttribute]
            }
        return (nested as? [CFTypeRef] ?? []).compactMap(checked).dropFirst(role == kAXMenuBarRole ? 1 : 0).map(\.self)
    }

    private func walk(marking marked: [(attribute: String, node: AXUIElement)]) throws(ApplicationFailure) -> (role: String?, parts: [Part]) {
        do throws(ApplicationFailure) {
            let read: [String: CFTypeRef] = try values(nodeAttributes)
            let role: String? = read[kAXRoleAttribute] as? String
            guard role != kAXSheetRole else { return (role, []) }
            let children: [(role: String?, parts: [Part])] = try Self.children(read).map { child throws(ApplicationFailure) in try child.walk(marking: marked) }
            let menus: [Part] = children.filter { child in child.role == kAXMenuRole }.flatMap(\.parts)
            let own: Part? = try part(read, attribute: marked.first { mark in mark.node == self }?.attribute, items: menus.compactMap(\.control))
            return (role, [own].compactMap(\.self) + menus.filter { part in part.is(\.unreadable) } + children.filter { child in child.role != kAXMenuRole }.flatMap(\.parts))
        } catch {
            return (nil, [try Part(error)])
        }
    }

    private func part(_ read: [String: CFTypeRef], attribute: String?, items: [Control]) throws(ApplicationFailure) -> Part? {
        do throws(ApplicationFailure) {
            guard let role: String = read[kAXRoleAttribute] as? String, role != kAXMenuRole else { return nil }
            let value: CFTypeRef? =
                switch role {
                    case kAXTextAreaRole: try visibleText().map { text in text as CFString }
                    case _ where valuedRoles.contains(role): try values([kAXValueAttribute])[kAXValueAttribute]
                    case _: nil
                }
            guard role != kAXStaticTextRole else {
                return (value as? String).flatMap { text in
                    text.unicodeScalars.allSatisfy { scalar in scalar.properties.isWhitespace || scalar.properties.generalCategory == .privateUse } ? nil : .text(text)
                }
            }
            let actions: [String] = try actionNames()
            guard value != nil || actions.contains(kAXPressAction) else { return nil }
            let label: String? = try label(read)
            return role == kAXMenuItemRole && label == nil
                ? nil
                : .control(
                    Control(
                        id: token,
                        role: role,
                        subrole: read[kAXSubroleAttribute] as? String,
                        label: label,
                        identifier: read[kAXIdentifierAttribute] as? String,
                        attribute: attribute,
                        value: value.flatMap(JSONValue.init(accessibility:)),
                        enabled: read[kAXEnabledAttribute] as? Bool ?? true,
                        actions: actions,
                        items: items.isEmpty ? nil : items,
                    )
                )
        } catch {
            return try Part(error)
        }
    }

    func described(walking walked: (Element) -> Bool) throws(ApplicationFailure) -> Element? {
        let read: [String: CFTypeRef] = try values(outlineAttributes)
        guard let role: String = read[kAXRoleAttribute] as? String else { return nil }
        let window: CGWindowID? = try window()
        guard window != nil || role == kAXMenuBarRole else { return nil }
        let marked: [(attribute: String, node: AXUIElement)] = buttonAttributes.compactMap { attribute in
            read[attribute].flatMap(Self.checked).map { node in (attribute: attribute, node: node) }
        }
        let (subrole, modal): (String?, Bool?) = (read[kAXSubroleAttribute] as? String, read[kAXModalAttribute] as? Bool)
        let (id, actions, edited): (Data, [String], Bool?) = try (token, actionNames(), marked.first { mark in mark.attribute == kAXCloseButtonAttribute }?.node.value(kAXEditedAttribute))
        let title: String? =
            try role == kAXMenuRole
            ? read[kAXParentAttribute].flatMap(Self.checked).flatMap { parent throws(ApplicationFailure) in try parent.label(parent.values(nodeAttributes)) }
            : (read[kAXTitleAttribute] as? String).flatMap { title in title.isEmpty ? nil : title }
        let element: (Element.Detail?) -> Element = { detail in
            Element(
                id: id,
                window: window,
                role: role,
                subrole: subrole,
                modal: modal,
                identifier: read[kAXIdentifierAttribute] as? String,
                description: read[kAXDescriptionAttribute] as? String,
                title: title,
                document: (read[kAXDocumentAttribute] as? String).flatMap { document in URL(string: document) },
                edited: edited,
                actions: actions,
                requiresAnswer: role == kAXSheetRole || modal == true || subrole == kAXSystemDialogSubrole,
                detail: detail,
            )
        }
        guard walked(element(nil)) else { return menuRoles.contains(role) ? nil : element(nil) }
        let walk: [Part] = try Self.children(read).map { child throws(ApplicationFailure) in try child.walk(marking: marked).parts }.flatMap(\.self)
        let parts: [Part] =
            try walk
            + marked.filter { mark in !walk.contains { part in part.control?.attribute == mark.attribute } }
            .map { mark throws(ApplicationFailure) in try mark.node.walk(marking: marked).parts }.flatMap(\.self)
        let origin: CGPoint? = AXValue.unpacked(read[kAXPositionAttribute], .cgPoint, CGPoint.zero)
        let size: CGSize? = AXValue.unpacked(read[kAXSizeAttribute], .cgSize, CGSize.zero)
        return element(
            Element.Detail(
                frame: origin.flatMap { origin in size.map { size in CGRect(origin: origin, size: size) } },
                main: read[kAXMainAttribute] as? Bool,
                focused: read[kAXFocusedAttribute] as? Bool,
                minimized: read[kAXMinimizedAttribute] as? Bool,
                text: parts.compactMap(\.text),
                controls: parts.compactMap(\.control),
                unreadable: parts.compactMap(\.unreadable),
            )
        )
    }
}
