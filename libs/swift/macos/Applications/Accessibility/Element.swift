import AppKit
public import ApplicationServices
import CasePaths
public import Encoding
public import Foundation
import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let buttonAttributes: [String] = [kAXCloseButtonAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute]
private let childAttributes: [String] = [kAXChildrenAttribute, kAXVisibleRowsAttribute, kAXVisibleChildrenAttribute]
private let elementAttributes: [String] =
    [
        kAXRoleAttribute, kAXSubroleAttribute, kAXEnabledAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXTitleUIElementAttribute, kAXValueAttribute,
        kAXSelectedAttribute,
    ]
    + frameAttributes + childAttributes
private let menuRoles: Set<String> = [kAXMenuRole, kAXMenuBarRole]
private let boundedRoles: Set<String> = [kAXSheetRole, kAXPopoverRole]
@usableFromInline let detailDepth: Int = 16

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

public struct TopLevelElement: Encodable, Hashable, Sendable {
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

    public let id: Element.ID
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
        let accept: Control? = attributed(kAXDefaultButtonAttribute) ?? identified("OKButton")
        let cancel: Control? = attributed(kAXCancelButtonAttribute) ?? identified("CancelButton") ?? attributed(kAXCloseButtonAttribute)
        let answers = Answers(
            accept: accept,
            discard: identified("DontSaveButton"),
            cancel: cancel,
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

public struct Element: Encodable, Sendable {
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

    public let id: ID
    public let role: String?
    public let subrole: String?
    public let label: String?
    public let identifier: String?
    public let value: JSONValue?
    public let settable: Bool
    public let enabled: Bool?
    public let actions: [String]
    public let frame: CGRect?
    public let count: Int
    public let children: [Self]?
    public let unreadable: [FailedRead]
}

@CasePathable
@dynamicMemberLookup
private enum Part {
    case text(String)
    case control(Control)
    case unreadable(FailedRead)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Element {
    fileprivate func parts(marking marked: [Data: String]) -> [Part] {
        guard let role else { return unreadable.map(Part.unreadable) }
        guard !boundedRoles.contains(role) else { return [] }
        let nested = children ?? []
        let menus = nested.filter { child in child.role == kAXMenuRole }.flatMap { child in child.parts(marking: marked) }
        let rest = nested.filter { child in child.role != kAXMenuRole }.flatMap { child in child.parts(marking: marked) }
        let controlled = role != kAXStaticTextRole && (value != nil || actions.contains(kAXPressAction))
        let items = menus.compactMap(\.control)
        let captions = rest.lazy.compactMap { (part: Part) -> String? in
            switch part {
                case .text(let text): text
                case .control(let control): control.label
                case .unreadable: nil
            }
        }
        let named = label ?? (role == kAXRowRole ? captions.first : nil)
        let own: Part? =
            switch (role, value) {
                case (kAXStaticTextRole, .string(let text)) where !text.unicodeScalars.allSatisfy({ scalar in scalar.properties.isWhitespace || scalar.properties.generalCategory == .privateUse }):
                    .text(text)
                case (kAXStaticTextRole, _), (kAXMenuRole, _): nil
                case _ where controlled && (role != kAXMenuItemRole || label != nil):
                    .control(
                        Control(
                            id: id,
                            role: role,
                            subrole: subrole,
                            label: named,
                            identifier: identifier,
                            attribute: marked[id.token],
                            value: value,
                            enabled: enabled ?? true,
                            actions: actions,
                            items: items.isEmpty ? nil : items,
                        )
                    )
                case _: nil
            }
        return [own].compactMap(\.self) + unreadable.map(Part.unreadable) + menus.filter { part in part.is(\.unreadable) } + rest
    }
}

extension AXUIElement {
    static func candidates(_ read: Attributes) throws(ApplicationFailure) -> [AXUIElement] {
        try (read.values[kAXWindowsAttribute] as? [CFTypeRef] ?? []).compactMap(cast).map { window throws(ApplicationFailure) in
            try [window]
                + window.elements(kAXChildrenAttribute).map { child throws(ApplicationFailure) -> AXUIElement? in
                    let role: String? = try child.value(kAXRoleAttribute)
                    return role.map(boundedRoles.contains) == true ? child : nil
                }
                .compactMap(\.self)
        }
        .flatMap(\.self)
    }

    func menus(_ roles: Set<String>) throws(ApplicationFailure) -> [AXUIElement] {
        try elements(kAXChildrenAttribute).map { child throws(ApplicationFailure) -> [AXUIElement] in
            switch try child.value(kAXRoleAttribute) as String? {
                case .some(let role) where roles.contains(role): [child]
                case .some(kAXScrollAreaRole): try child.menus(roles)
                case .some, .none: []
            }
        }
        .flatMap(\.self)
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
        return Array((nested as? [CFTypeRef] ?? []).compactMap(cast).enumerated().dropFirst(role == kAXMenuBarRole ? 1 : 0))
    }

    private static func attempt<Value>(_ body: () throws(ApplicationFailure) -> Value) throws(ApplicationFailure) -> (value: Value?, unread: FailedRead?) {
        do throws(ApplicationFailure) { return (try body(), nil) } catch {
            guard let read = error.reason[case: \.unreadable] else { throw error }
            return (nil, read)
        }
    }

    func readElement(menu: (process: pid_t, path: [Int])?, depth: Int) throws(ApplicationFailure) -> Element {
        let attempted = try Self.attempt { () throws(ApplicationFailure) in try values(elementAttributes) }
        guard let read = attempted.value else {
            let id = Element.ID(token: menu.map { menu in Self.menuItem(of: menu.process, at: menu.path) } ?? token)
            return Element(
                id: id,
                role: nil,
                subrole: nil,
                label: nil,
                identifier: nil,
                value: nil,
                settable: false,
                enabled: nil,
                actions: [],
                frame: nil,
                count: 0,
                children: nil,
                unreadable: [attempted.unread].compactMap(\.self),
            )
        }
        let role = read.values[kAXRoleAttribute] as? String
        let bars = try role == kAXMenuBarRole && menu == nil ? process.map { process throws(ApplicationFailure) in (process, try Self.application(process).values(menuBars)) } : nil
        let index: Int? = bars.flatMap { (_: pid_t, read: Attributes) -> Int? in
            menuBars.firstIndex { (bar: String) -> Bool in read.values[bar].map { (value: CFTypeRef) -> Bool in CFEqual(value, self) } == true }
        }
        let context: (process: pid_t, path: [Int])? = menu ?? bars.flatMap { (process: pid_t, _: Attributes) in index.map { index in (process: process, path: [index]) } }
        let acted = try Self.attempt { () throws(ApplicationFailure) in try actionNames() }
        let valued = try Self.attempt { () throws(ApplicationFailure) -> CFTypeRef? in
            try role == kAXTextAreaRole ? visibleText().map { text in text as CFString } : read.values[valueAttribute(of: role)]
        }
        let labeled = try Self.attempt { () throws(ApplicationFailure) in try label(read) }
        let writable = try Self.attempt { () throws(ApplicationFailure) in try settable(valueAttribute(of: role)) }
        let nested = Self.children(read)
        let children =
            try depth > 0
            ? nested.map { child throws(ApplicationFailure) in
                try child.element.readElement(menu: context.map { context in (context.process, context.path + [child.offset]) }, depth: depth - 1)
            } : nil
        let (origin, size) = (AXValue.unpack(read.values[kAXPositionAttribute], .cgPoint, CGPoint.zero), AXValue.unpack(read.values[kAXSizeAttribute], .cgSize, CGSize.zero))
        return Element(
            id: Element.ID(token: context.map { context in Self.menuItem(of: context.process, at: context.path) } ?? token),
            role: role,
            subrole: read.values[kAXSubroleAttribute] as? String,
            label: labeled.value.flatMap(\.self),
            identifier: read.values[kAXIdentifierAttribute] as? String,
            value: valued.value.flatMap(\.self).flatMap(JSONValue.init(accessibility:)),
            settable: writable.value ?? false,
            enabled: read.values[kAXEnabledAttribute] as? Bool,
            actions: acted.value ?? [],
            frame: origin.flatMap { origin in size.map { size in CGRect(origin: origin, size: size) } },
            count: nested.count,
            children: children,
            unreadable: read.failed + [acted.unread, valued.unread, labeled.unread, writable.unread].compactMap(\.self),
        )
    }

    func describe(
        walking walked: (TopLevelElement) -> Bool,
        helper: Bool,
        among listed: Set<CGWindowID>,
        bar: Int? = nil,
    ) throws(ApplicationFailure) -> (window: CGWindowID?, element: TopLevelElement)? {
        let read = try values(
            [
                kAXRoleAttribute, kAXSubroleAttribute, kAXModalAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXDocumentAttribute, kAXMainAttribute,
                kAXFocusedAttribute, kAXMinimizedAttribute, kAXParentAttribute,
            ] + buttonAttributes + childAttributes
        )
        guard let role = read.values[kAXRoleAttribute] as? String else { return nil }
        let window = self.window.flatMap { number in listed.contains(number) ? number : nil }
        guard window != nil || menuRoles.contains(role) else { return nil }
        let marked = buttonAttributes.compactMap { attribute in
            read.values[attribute].flatMap(Self.cast).map { button in (attribute: attribute, button: button) }
        }
        let (subrole, modal) = (read.values[kAXSubroleAttribute] as? String, read.values[kAXModalAttribute] as? Bool)
        let (id, actions, edited): (Data, [String], Bool?) = try (token, actionNames(), marked.first { mark in mark.attribute == kAXCloseButtonAttribute }?.button.value(kAXEditedAttribute))
        let title: String? =
            try role == kAXMenuRole
            ? read.values[kAXParentAttribute].flatMap(Self.cast).flatMap { parent throws(ApplicationFailure) in try parent.label(parent.values(elementAttributes)) }
            : (read.values[kAXTitleAttribute] as? String).flatMap { title in title.isEmpty ? nil : title }
        let element: (TopLevelElement.Detail?) -> TopLevelElement = { detail in
            TopLevelElement(
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
        let tokens = Dictionary(marked.map { mark in (mark.button.token, mark.attribute) }) { first, _ in first }
        let walk = try Self.children(read).map { child throws(ApplicationFailure) in
            try child.element.readElement(menu: bar.flatMap { bar in process.map { process in (process, [bar, child.offset]) } }, depth: bar == nil ? detailDepth : 0).parts(marking: tokens)
        }
        .flatMap(\.self)
        let parts =
            try walk
            + marked.filter { mark in !walk.contains { part in part.control?.attribute == mark.attribute } }
            .map { mark throws(ApplicationFailure) in try mark.button.readElement(menu: nil, depth: 0).parts(marking: tokens) }.flatMap(\.self)
        return (
            window,
            element(
                TopLevelElement.Detail(
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

extension Element.ID {
    @concurrent
    public func frame() async throws(ApplicationFailure) -> (window: CGWindowID, frame: CGRect) {
        try await withTaskExecutorPreference(accessibilityQueue) { () throws(ApplicationFailure) in
            guard let element = try AXUIElement.element(token) else { throw ApplicationFailure(subject: description, reason: .unresolved) }
            let read = try element.values(frameAttributes + [kAXWindowAttribute])
            guard let origin = AXValue.unpack(read.values[kAXPositionAttribute], .cgPoint, CGPoint.zero), let size = AXValue.unpack(read.values[kAXSizeAttribute], .cgSize, CGSize.zero) else {
                throw ApplicationFailure(subject: description, reason: .unreadable(FailedRead(reads: frameAttributes, role: nil, label: read.label, error: .noValue)))
            }
            guard let window = (read.values[kAXWindowAttribute].flatMap(AXUIElement.cast) ?? element).window else { throw ApplicationFailure(subject: description, reason: .noWindow) }
            return (window, CGRect(origin: origin, size: size))
        }
    }
}

extension Instance {
    @concurrent
    public func element(from root: Element.ID?, depth: Int = detailDepth) async throws(ApplicationFailure) -> Element {
        try await withTaskExecutorPreference(accessibilityQueue) { () throws(ApplicationFailure) in
            guard let root else { return try AXUIElement.application(process.pid).readElement(menu: nil, depth: depth) }
            guard let element = try AXUIElement.element(root.token), let owner = element.process else { throw ApplicationFailure(subject: root.description, reason: .unresolved) }
            guard owner == process.pid || NSRunningApplication(processIdentifier: owner)?.isDialogHelper == true else {
                throw ApplicationFailure(subject: root.description, reason: .foreign(process.pid))
            }
            return try element.readElement(menu: AXUIElement.menuPath(root.token), depth: depth)
        }
    }

    @concurrent
    public func documents() async throws(ApplicationFailure) -> [CGWindowID: URL] {
        try await withTaskExecutorPreference(accessibilityQueue) { () throws(ApplicationFailure) in
            let rows = try AXUIElement.application(process.pid).elements(kAXWindowsAttribute).map { window throws(ApplicationFailure) in
                (number: window.window, document: (try window.value(kAXDocumentAttribute) as String?).flatMap(URL.init(string:)))
            }
            return Dictionary(rows.compactMap { row in row.number.flatMap { number in row.document.map { document in (number, document) } } }) { first, _ in first }
        }
    }
}
