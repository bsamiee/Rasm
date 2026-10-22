public import ApplicationServices
public import Encoding
public import Foundation
public import Results
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let labelAttributes: [String] = [kAXTitleAttribute, kAXDescriptionAttribute]
private let buttonAttributes: [String] = [kAXCloseButtonAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute]
let messagingTimeout: Duration = .seconds(1)

// --- [MODELS] --------------------------------------------------------------------------

public struct Control: Codable, Hashable, Sendable {
    public let id: Data
    public let role: String
    public let label: String?
    public let value: JSONValue?
    public let actions: [String]
}

public struct Button: Codable, Hashable, Sendable {
    public let id: Data
    public let role: String
    public let subrole: String?
    public let label: String?
    public let identifier: String?
    public let attribute: String?
    public let actions: [String]
    public let items: [Self]
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
    public let buttons: [Button]
    public let actions: [String]
    public let unreadable: [FailedRead]

    public var requiresAnswer: Bool { role == kAXSheetRole || modal == true || subrole == kAXSystemDialogSubrole }
}

extension Element: Encodable {
    private enum Key: CodingKey {
        case id, window, role, subrole, modal, identifier, description, title, text, controls, buttons, actions, unreadable, requiresAnswer
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
        try fields.encode(buttons, forKey: .buttons)
        try fields.encode(actions, forKey: .actions)
        try fields.encode(unreadable, forKey: .unreadable)
        try fields.encode(requiresAnswer, forKey: .requiresAnswer)
    }
}

private struct Part {
    let button: Button?
    let text: String?
    let control: Control?

    init(_ node: AXUIElement, attribute: String?) throws(ElementFailure) {
        let attributes: [String: CFTypeRef] = try node.values(
            [kAXRoleAttribute, kAXSubroleAttribute, kAXEnabledAttribute, kAXValueAttribute, kAXIdentifierAttribute, kAXChildrenAttribute] + labelAttributes
        )
        let role: String? = attributes[kAXRoleAttribute] as? String
        let label: String? = try node.label(attributes)
        let actions: [String] = try node.actionNames()
        button = try role.flatMap { role throws(ElementFailure) in
            try attributes[kAXEnabledAttribute] as? Bool != false && actions.contains(kAXPressAction)
                ? Button(
                    id: node.token,
                    role: role,
                    subrole: attributes[kAXSubroleAttribute] as? String,
                    label: label,
                    identifier: attributes[kAXIdentifierAttribute] as? String,
                    attribute: attribute,
                    actions: actions,
                    items: (attributes[kAXChildrenAttribute] as? [AXUIElement] ?? [])
                        .map { child throws(ElementFailure) in try child.value(kAXRoleAttribute) == kAXMenuRole ? child.children() : [] }.flatMap(\.self)
                        .map { item throws(ElementFailure) in try Self(item, attribute: nil).button }.compactMap(\.self),
                )
                : nil
        }
        text = role == kAXStaticTextRole ? (attributes[kAXValueAttribute] as? String).flatMap { line in line.isEmpty ? nil : line } : nil
        control = role.flatMap { role in
            switch role {
                case kAXTextFieldRole, kAXTextAreaRole, kAXComboBoxRole, kAXPopUpButtonRole, kAXCheckBoxRole, kAXRadioButtonRole:
                    Control(id: node.token, role: role, label: label, value: attributes[kAXValueAttribute].flatMap(JSONValue.init(accessibility:)), actions: actions)
                default: nil
            }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ElementFailure: CustomNSError, LocalizedError {
    case untrusted
    case unanswered(ElementAction?)
    case noProcess
    case unreadable(FailedRead)
    case unobservable(AggregateError<(notification: String, error: AXError)>)
    case absent(Data)
    case activating(ElementAction)
    case refused(ElementAction, AXError)

    static func reading(_ error: AXError, _ reads: [String], on element: AXUIElement) -> Self {
        switch error {
            case .apiDisabled: .untrusted
            case .cannotComplete: .unanswered(nil)
            default: .unreadable(FailedRead(error, reading: reads, on: element))
        }
    }

    public var errorDescription: String? {
        switch self {
            case .untrusted: "Process lacks Accessibility access, allow it in System Settings > Privacy & Security > Accessibility"
            case .unanswered(.none): "Application did not answer within Accessibility messaging timeout"
            case .unanswered(.some(let action)): "\(action) got no answer within messaging timeout and can have run"
            case .noProcess: Errno.noSuchProcess.description
            case .unreadable(let read):
                "\(read.reads.joined(separator: ", ")) on \(read.role ?? "element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error)"
            case .unobservable(let refusals):
                "Application refused Accessibility notifications: \(refusals.errors.map { refusal in "\(refusal.notification) (AXError \(refusal.error.rawValue))" }.joined(separator: ", "))"
            case .absent(let id): "Application has no open element with id \(id.base64EncodedString())"
            case .activating(let action): "\(action) not sent, AXRaise brings window forward"
            case .refused(let action, let error): "Application refused \(action) with AXError \(error.rawValue)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .noProcess: [NSUnderlyingErrorKey: Errno.noSuchProcess]
            case .untrusted, .unanswered, .unreadable, .unobservable, .absent, .activating, .refused: [:]
        }
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
    // --- [IDENTITY]
    var token: Data { unsafe remoteToken(self).takeRetainedValue() as Data }

    static func element(_ token: Data) -> AXUIElement? { unsafe remoteElement(token as CFData)?.takeRetainedValue() }

    var process: pid_t? {
        var pid: pid_t = 0
        return unsafe AXUIElementGetPid(self, &pid) == .success ? pid : nil
    }

    // --- [READS]
    func value<Value>(_ attribute: String) throws(ElementFailure) -> Value? {
        var value: CFTypeRef?
        return switch unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) {
            case .success: value as? Value
            case .noValue, .attributeUnsupported, .invalidUIElement: nil
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
                            case (.some(.noValue), _), (.some(.attributeUnsupported), _), (.some(.invalidUIElement), _), (.none, true): nil
                            case (.some(let error), _): throw .reading(error, [attribute], on: self)
                            case (.none, false): (attribute, slot)
                        }
                    }
                    .compactMap(\.self)
                )
            case .invalidUIElement: [:]
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
            case .success: (names as? [String] ?? []).filter { action in action != kAXRaiseAction }
            case .failure, .invalidUIElement: []
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
        let bar: AXUIElement? = try value(kAXMenuBarAttribute)
        let menus: [AXUIElement] = try (bar?.children() ?? []).map { item throws(ElementFailure) in try item.children() }.flatMap(\.self)
        let windows: [AXUIElement] = try value(kAXWindowsAttribute) ?? []
        return try menus + windows.map { window throws(ElementFailure) in try [window] + window.children { role in role == kAXSheetRole } }.flatMap(\.self) + [bar].compactMap(\.self)
    }

    func descendants() throws(ElementFailure) -> [AXUIElement] {
        let unwalked: Set<String?> = [kAXSheetRole, kAXScrollAreaRole, kAXOutlineRole, kAXTableRole, kAXListRole, kAXBrowserRole]
        return try [self] + children { role in !unwalked.contains(role) }.map { child throws(ElementFailure) in try child.descendants() }.flatMap(\.self)
    }

    func described(walking walked: (Element) -> Bool) throws(ElementFailure) -> Element? {
        let attributes: [String: CFTypeRef] = try values(
            [kAXRoleAttribute, kAXSubroleAttribute, kAXModalAttribute, kAXIdentifierAttribute, kAXTitleAttribute, kAXDescriptionAttribute] + buttonAttributes
        )
        guard let role: String = attributes[kAXRoleAttribute] as? String else { return nil }
        let window: CGWindowID? = try window()
        guard window != nil || role == kAXMenuBarRole else { return nil }
        let title: String? =
            try role == kAXMenuRole
            ? (value(kAXParentAttribute) as AXUIElement?).flatMap { parent throws(ElementFailure) in try parent.label(parent.values(labelAttributes)) }
            : (attributes[kAXTitleAttribute] as? String).flatMap { title in title.isEmpty ? nil : title }
        let (id, actions): (Data, [String]) = try (token, actionNames())
        let element: ([String], [Control], [Button], [FailedRead]) -> Element = { text, controls, buttons, unreadable in
            Element(
                id: id,
                window: window,
                role: role,
                subrole: attributes[kAXSubroleAttribute] as? String,
                modal: attributes[kAXModalAttribute] as? Bool,
                identifier: attributes[kAXIdentifierAttribute] as? String,
                description: attributes[kAXDescriptionAttribute] as? String,
                title: title,
                text: text,
                controls: controls,
                buttons: buttons,
                actions: actions,
                unreadable: unreadable,
            )
        }
        let outline: Element = element([], [], [], [])
        guard walked(outline) else { return [kAXMenuRole, kAXMenuBarRole].contains(role) ? nil : outline }
        let marked: [(attribute: String, node: AXUIElement)] = buttonAttributes.compactMap { attribute in
            attributes[attribute].flatMap { slot in CFGetTypeID(slot) == AXUIElementGetTypeID() ? (attribute, unsafe unsafeDowncast(slot, to: AXUIElement.self)) : nil }
        }
        let nodes: [AXUIElement] = try [kAXMenuRole, kAXMenuBarRole].contains(role) ? children() : descendants()
        let reads: [Result<Part, ElementFailure>] = (nodes + marked.map(\.node).filter { node in !nodes.contains(node) }).map { node in
            Result { () throws(ElementFailure) in try Part(node, attribute: marked.first { mark in mark.node == node }?.attribute) }
        }
        let parts: [Part] = try reads.map { read throws(ElementFailure) -> Part? in
            switch read {
                case .success(let part): part
                case .failure(.unreadable): nil
                case .failure(let error): throw error
            }
        }
        .compactMap(\.self)
        return element(
            parts.compactMap(\.text),
            parts.compactMap(\.control),
            parts.compactMap(\.button),
            reads.compactMap { read in if case .failure(.unreadable(let failed)) = read { failed } else { nil } },
        )
    }
}

@_silgen_name("_AXUIElementGetWindow")
private func windowIdentifier(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError

@_silgen_name("_AXUIElementRemoteTokenCreate")
private func remoteToken(_: AXUIElement) -> Unmanaged<CFData>

@_silgen_name("_AXUIElementCreateWithRemoteToken")
private func remoteElement(_: CFData) -> Unmanaged<AXUIElement>?
