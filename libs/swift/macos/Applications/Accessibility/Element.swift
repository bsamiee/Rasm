public import ApplicationServices
import CasePaths
public import Encoding
public import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let buttonAttributes: [String] = [kAXCloseButtonAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute]
private let childAttributes: [String] = [kAXChildrenAttribute, kAXVisibleRowsAttribute, kAXVisibleChildrenAttribute]
private let nodeAttributes: [String] =
    [kAXRoleAttribute, kAXSubroleAttribute, kAXEnabledAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXTitleUIElementAttribute] + childAttributes
private let menuRoles: Set<String> = [kAXMenuRole, kAXMenuBarRole]
private let boundedRoles: Set<String> = [kAXSheetRole, kAXPopoverRole]

// --- [MODELS] --------------------------------------------------------------------------

public struct Control: Encodable, Hashable, Sendable {
    public let id: Element.ID
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
    private enum CodingKeys: CodingKey {
        case reads, role, label, error
    }

    public let reads: [String]
    public let role: String?
    public let label: String?
    public let error: AXError

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(reads, forKey: .reads)
        try fields.encodeIfPresent(role, forKey: .role)
        try fields.encodeIfPresent(label, forKey: .label)
        try fields.encode(error.rawValue, forKey: .error)
    }
}

public struct Element: Encodable, Hashable, Sendable {
    public struct ID: Codable, Hashable, Sendable, CustomStringConvertible {
        let token: Data

        init(token: Data) { self.token = token }

        public init?(base64Encoded text: String) {
            guard let token = Data(base64Encoded: text) else { return nil }
            self.token = token
        }

        public init(from decoder: any Decoder) throws { token = try decoder.singleValueContainer().decode(Data.self) }

        public func encode(to encoder: any Encoder) throws {
            var container = encoder.singleValueContainer()
            try container.encode(token)
        }

        public var description: String { token.base64EncodedString() }
    }

    public enum Kind: String, Encodable, Sendable {
        case filePanel
        case sheet
        case alert
        case modal
        case system
        case menu
        case window
    }

    public struct Detail: Encodable, Hashable, Sendable {
        public let main: Bool?
        public let focused: Bool?
        public let text: [String]
        public let controls: [Control]
        public let unreadable: [FailedRead]
    }

    public struct Answers: Encodable, Hashable, Sendable {
        public let accept: Control?
        public let discard: Control?
        public let cancel: Control?
        public let buttons: [Control]
    }

    private enum CodingKeys: CodingKey {
        case id, role, subrole, modal, identifier, description, title, document, edited, minimized, actions, detail, kind, asking, answers
    }

    public let id: ID
    public let role: String
    public let subrole: String?
    public let modal: Bool?
    public let identifier: String?
    public let description: String?
    public let title: String?
    public let document: URL?
    public let edited: Bool?
    public let minimized: Bool?
    public let actions: [String]
    public let detail: Detail?
    let helper: Bool

    public var kind: Kind {
        switch (helper, identifier.map(["save-panel", "open-panel"].contains) == true, role, modal == true, description == "alert", subrole) {
            case (true, _, _, _, _, _): .system
            case (_, true, _, _, _, _): .filePanel
            case (_, _, kAXSheetRole, _, _, _): .sheet
            case (_, _, _, true, true, _): .alert
            case (_, _, _, true, false, _): .modal
            case (_, _, _, false, true, kAXSystemDialogSubrole): .system
            case (_, _, let role, _, _, _) where menuRoles.contains(role): .menu
            case (_, _, _, _, _, _): .window
        }
    }

    public var asking: Bool {
        switch kind {
            case .filePanel: role == kAXSheetRole || modal == true
            case .sheet, .alert, .modal, .system: true
            case .menu, .window: false
        }
    }

    public var answers: Answers? {
        guard let controls = detail?.controls else { return nil }
        let attributed: (String) -> Control? = { attribute in controls.first { control in control.attribute == attribute } }
        let identified: (String) -> Control? = { identifier in controls.first { control in control.identifier == identifier } }
        let answers = Answers(
            accept: attributed(kAXDefaultButtonAttribute) ?? identified("OKButton"),
            discard: identified("DontSaveButton"),
            cancel: attributed(kAXCancelButtonAttribute) ?? identified("CancelButton") ?? attributed(kAXCloseButtonAttribute),
            buttons: controls.filter { control in control.role == kAXButtonRole && control.label != nil },
        )
        return answers.accept == nil && answers.discard == nil && answers.cancel == nil && answers.buttons.isEmpty ? nil : answers
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(id, forKey: .id)
        try fields.encode(role, forKey: .role)
        try fields.encodeIfPresent(subrole, forKey: .subrole)
        try fields.encodeIfPresent(modal, forKey: .modal)
        try fields.encodeIfPresent(identifier, forKey: .identifier)
        try fields.encodeIfPresent(description, forKey: .description)
        try fields.encodeIfPresent(title, forKey: .title)
        try fields.encodeIfPresent(document, forKey: .document)
        try fields.encodeIfPresent(edited, forKey: .edited)
        try fields.encodeIfPresent(minimized, forKey: .minimized)
        try fields.encode(actions, forKey: .actions)
        try fields.encodeIfPresent(detail, forKey: .detail)
        try fields.encode(kind, forKey: .kind)
        try fields.encode(asking, forKey: .asking)
        try fields.encodeIfPresent(answers, forKey: .answers)
    }
}

@CasePathable
@dynamicMemberLookup
private enum Part {
    case text(String)
    case control(Control)
    case unreadable(FailedRead)

    init(_ failure: ApplicationFailure) throws(ApplicationFailure) {
        guard let read = failure.reason[case: \.unreadable] else { throw failure }
        self = .unreadable(read)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension AXUIElement {
    static func candidates(_ read: Attributes) throws(ApplicationFailure) -> [AXUIElement] {
        try (read.values[kAXWindowsAttribute] as? [CFTypeRef] ?? []).compactMap(checked).map { window throws(ApplicationFailure) in
            try [window]
                + window.elements(kAXChildrenAttribute).map { child throws(ApplicationFailure) -> AXUIElement? in
                    let role: String? = try child.value(kAXRoleAttribute)
                    return role.map(boundedRoles.contains) == true ? child : nil
                }
                .compactMap(\.self)
        }
        .flatMap(\.self)
    }

    func applicationMenus() throws(ApplicationFailure) -> [AXUIElement] {
        try elements(kAXChildrenAttribute).map { child throws(ApplicationFailure) -> AXUIElement? in
            let role: String? = try child.value(kAXRoleAttribute)
            return role == kAXMenuRole ? child : nil
        }
        .compactMap(\.self)
    }

    private static func children(_ read: Attributes) -> [(offset: Int, element: AXUIElement)] {
        let role = read.values[kAXRoleAttribute] as? String
        let nested: CFTypeRef? =
            switch role {
                case kAXBrowserRole: nil
                case kAXTableRole, kAXOutlineRole: read.values[kAXVisibleRowsAttribute]
                case kAXListRole: read.values[kAXVisibleChildrenAttribute] ?? read.values[kAXChildrenAttribute]
                case _: read.values[kAXChildrenAttribute]
            }
        return Array((nested as? [CFTypeRef] ?? []).compactMap(checked).enumerated().dropFirst(role == kAXMenuBarRole ? 1 : 0))
    }

    private func walk(marking marked: [(attribute: String, node: AXUIElement)], menu: (process: pid_t, path: [Int])?) throws(ApplicationFailure) -> (role: String?, parts: [Part]) {
        do throws(ApplicationFailure) {
            let read = try values(nodeAttributes)
            let role = read.values[kAXRoleAttribute] as? String
            guard role.map(boundedRoles.contains) != true else { return (role, []) }
            let children = try Self.children(read).map { child throws(ApplicationFailure) in
                try child.element.walk(marking: marked, menu: menu.map { menu in (menu.process, menu.path + [child.offset]) })
            }
            let menus = children.filter { child in child.role == kAXMenuRole }.flatMap(\.parts)
            let own = try parts(
                read,
                id: menu.map { menu in Self.menuItem(of: menu.process, at: menu.path) } ?? token,
                attribute: marked.first { mark in mark.node == self }?.attribute,
                items: menus.compactMap(\.control),
            )
            return (role, own + menus.filter { part in part.is(\.unreadable) } + children.filter { child in child.role != kAXMenuRole }.flatMap(\.parts))
        } catch {
            return (nil, [try Part(error)])
        }
    }

    private func parts(_ read: Attributes, id: Data, attribute: String?, items: [Control]) throws(ApplicationFailure) -> [Part] {
        guard let role = read.values[kAXRoleAttribute] as? String, role != kAXMenuRole else { return read.failed.map(Part.unreadable) }
        let valued = Result { () throws(ApplicationFailure) -> CFTypeRef? in
            switch role {
                case kAXTextAreaRole: try visibleText().map { text in text as CFString }
                case kAXStaticTextRole, kAXTextFieldRole, kAXComboBoxRole, kAXPopUpButtonRole, kAXCheckBoxRole, kAXRadioButtonRole: try value(kAXValueAttribute)
                case _: nil
            }
        }
        let unvalued = try valued[case: \.failure].map { failure throws(ApplicationFailure) in try Part(failure) }
        let acted = Result { () throws(ApplicationFailure) in try role == kAXStaticTextRole ? [] : actionNames() }
        let unacted = try acted[case: \.failure].map { failure throws(ApplicationFailure) in try Part(failure) }
        let (held, names): (CFTypeRef?, [String]) = (valued[case: \.success], acted[case: \.success] ?? [])
        let controlled = role != kAXStaticTextRole && (held != nil || names.contains(kAXPressAction) || valued.is(\.failure))
        let labeled = Result { () throws(ApplicationFailure) in try controlled ? label(read) : nil }
        let unlabeled = try labeled[case: \.failure].map { failure throws(ApplicationFailure) in try Part(failure) }
        let name: String? = labeled[case: \.success]
        let unread = read.failed.map(Part.unreadable) + [unvalued, unacted, unlabeled].compactMap(\.self)
        let own: Part? =
            switch role {
                case kAXStaticTextRole:
                    (held as? String).flatMap { text in
                        text.unicodeScalars.allSatisfy { scalar in scalar.properties.isWhitespace || scalar.properties.generalCategory == .privateUse } ? nil : .text(text)
                    }
                case _ where controlled && (role != kAXMenuItemRole || name != nil):
                    .control(
                        Control(
                            id: Element.ID(token: id),
                            role: role,
                            subrole: read.values[kAXSubroleAttribute] as? String,
                            label: name,
                            identifier: read.values[kAXIdentifierAttribute] as? String,
                            attribute: attribute,
                            value: held.flatMap(JSONValue.init(accessibility:)),
                            enabled: read.values[kAXEnabledAttribute] as? Bool ?? true,
                            actions: names,
                            items: items.isEmpty ? nil : items,
                        )
                    )
                case _: nil
            }
        return [own].compactMap(\.self) + unread
    }

    func described(walking walked: (Element) -> Bool, helper: Bool, among listed: [InstanceState.Window], bar: Int? = nil) throws(ApplicationFailure) -> (window: CGWindowID?, element: Element)? {
        let read = try values(
            [
                kAXRoleAttribute, kAXSubroleAttribute, kAXModalAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXDocumentAttribute, kAXMainAttribute,
                kAXFocusedAttribute, kAXMinimizedAttribute, kAXParentAttribute,
            ] + buttonAttributes + childAttributes + frameAttributes
        )
        guard let role = read.values[kAXRoleAttribute] as? String else { return nil }
        let window = Self.window(framed: read, among: listed)
        guard window != nil || menuRoles.contains(role) else { return nil }
        let marked = buttonAttributes.compactMap { attribute in
            read.values[attribute].flatMap(Self.checked).map { node in (attribute: attribute, node: node) }
        }
        let (subrole, modal) = (read.values[kAXSubroleAttribute] as? String, read.values[kAXModalAttribute] as? Bool)
        let (id, actions, edited): (Data, [String], Bool?) = try (token, actionNames(), marked.first { mark in mark.attribute == kAXCloseButtonAttribute }?.node.value(kAXEditedAttribute))
        let title: String? =
            try role == kAXMenuRole
            ? read.values[kAXParentAttribute].flatMap(Self.checked).flatMap { parent throws(ApplicationFailure) in try parent.label(parent.values(nodeAttributes)) }
            : (read.values[kAXTitleAttribute] as? String).flatMap { title in title.isEmpty ? nil : title }
        let element: (Element.Detail?) -> Element = { detail in
            Element(
                id: Element.ID(token: id),
                role: role,
                subrole: subrole,
                modal: modal,
                identifier: read.values[kAXIdentifierAttribute] as? String,
                description: (read.values[kAXDescriptionAttribute] as? String).flatMap { description in description.isEmpty ? nil : description },
                title: title,
                document: (read.values[kAXDocumentAttribute] as? String).flatMap { document in URL(string: document) },
                edited: edited,
                minimized: read.values[kAXMinimizedAttribute] as? Bool,
                actions: actions,
                detail: detail,
                helper: helper,
            )
        }
        guard walked(element(nil)) else { return (window, element(nil)) }
        let walk = try Self.children(read).map { child throws(ApplicationFailure) in
            try child.element.walk(marking: marked, menu: bar.flatMap { bar in process.map { process in (process, [bar, child.offset]) } }).parts
        }
        .flatMap(\.self)
        let parts =
            try walk
            + marked.filter { mark in !walk.contains { part in part.control?.attribute == mark.attribute } }
            .map { mark throws(ApplicationFailure) in try mark.node.walk(marking: marked, menu: nil).parts }.flatMap(\.self)
        return (
            window,
            element(
                Element.Detail(
                    main: read.values[kAXMainAttribute] as? Bool,
                    focused: read.values[kAXFocusedAttribute] as? Bool,
                    text: parts.compactMap(\.text),
                    controls: parts.compactMap(\.control),
                    unreadable: read.failed + parts.compactMap(\.unreadable),
                )
            ),
        )
    }
}
