public import AppKit
public import ApplicationServices

// --- [TYPES] ---------------------------------------------------------------------------

public enum ElementKind: String, Codable, Sendable {
    case window
    case dialog
    case sheet
    case menu
}

// --- [MODELS] --------------------------------------------------------------------------

public enum Control: Codable, Hashable, Sendable {
    public enum Kind: String, Codable, Sendable {
        case field
        case popUp
    }

    case text(kind: Kind, label: String?, value: String?)
    case checkBox(label: String?, on: Bool)
}

public enum ElementAction: Hashable, Sendable {
    case press(String)
    case set(field: String, value: String)
    case close
    case cancel
}

extension ElementAction: Codable {
    private enum Key: CodingKey { case field, value }

    public init(label: String) {
        self =
            switch label {
                case kAXCloseButtonAttribute: .close
                case kAXCancelAction: .cancel
                default: .press(label)
            }
    }

    public var label: String {
        switch self {
            case .press(let label), .set(let label, _): label
            case .close: kAXCloseButtonAttribute
            case .cancel: kAXCancelAction
        }
    }

    public init(from decoder: any Decoder) throws {
        self = try Result { try decoder.singleValueContainer().decode(String.self) }
            .map(Self.init(label:))
            .flatMapError { _ in
                Result {
                    let fields: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
                    return try .set(field: fields.decode(String.self, forKey: .field), value: fields.decode(String.self, forKey: .value))
                }
            }
            .get()
    }

    public func encode(to encoder: any Encoder) throws {
        switch self {
            case .press, .close, .cancel:
                var single: any SingleValueEncodingContainer = encoder.singleValueContainer()
                try single.encode(label)
            case .set(let field, let value):
                var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
                try fields.encode(field, forKey: .field)
                try fields.encode(value, forKey: .value)
        }
    }
}

public struct Element: Codable, Hashable, Sendable {
    public let id: CGWindowID
    public let kind: ElementKind
    public let role: String
    public let title: String?
    public let text: [String]
    public let controls: [Control]
    public let actions: [ElementAction]
}

public struct ApplicationStatus: Codable, Equatable, Sendable {
    public let responding: Bool
    public let frontWindow: CGWindowID?
    public let elements: [Element]
}

struct Described {
    let element: Element
    let targets: [ElementAction: AXUIElement]
    let fields: [String: AXUIElement]
}

private struct Part {
    let node: AXUIElement
    let text: String?
    let control: Control?
    let press: String?

    var field: String? {
        if case .text(.field, let label, _) = control { label } else { nil }
    }

    init(_ node: AXUIElement) throws(ElementFailure) {
        let role: String? = try node.value(kAXRoleAttribute)
        let label: String? = try node.label()
        let enabled: Bool = try node.value(kAXEnabledAttribute) ?? true
        self.node = node
        text = try role == kAXStaticTextRole ? node.value(kAXValueAttribute).flatMap { (line: String) in line.isEmpty ? nil : line } : nil
        control =
            switch role {
                case .some(kAXTextFieldRole), .some(kAXTextAreaRole), .some(kAXComboBoxRole): .text(kind: .field, label: label, value: try node.value(kAXValueAttribute))
                case .some(kAXCheckBoxRole), .some(kAXRadioButtonRole): .checkBox(label: label, on: try node.value(kAXValueAttribute) == 1)
                case .some(kAXPopUpButtonRole): .text(kind: .popUp, label: label, value: try node.value(kAXValueAttribute))
                default: nil
            }
        let actions: [String] = try label == nil || !enabled ? [] : node.actionNames()
        press = actions.contains(kAXPressAction) ? label : nil
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ElementFailure: LocalizedError {
    case untrusted
    case unreadable(AXError)
    case unobservable(AggregateError<(notification: String, error: AXError)>)
    case absent(CGWindowID)
    case unoffered(ElementAction, [ElementAction])
    case refused(ElementAction, AXError)
    case cancelled

    static func reading(_ error: AXError) -> Self {
        error == .apiDisabled ? .untrusted : .unreadable(error)
    }

    public var errorDescription: String? {
        switch self {
            case .untrusted: "Accessibility access is off for this process: allow it under System Settings, Privacy & Security, Accessibility"
            case .unreadable(let error): "Accessibility read failed with AXError \(error.rawValue)"
            case .unobservable(let refusals):
                "The application refused Accessibility notifications: \(refusals.errors.map { refusal in "\(refusal.notification) (AXError \(refusal.error.rawValue))" }.joined(separator: ", "))"
            case .absent(let id): "No open window, dialog, sheet, or menu with id \(id)"
            case .unoffered(let action, let actions): "The element offers no \(action.label), it offers \(actions.map(\.label).joined(separator: ", "))"
            case .refused(let action, let error): "Performing \(action.label) returned AXError \(error.rawValue)"
            case .cancelled: "Canceled"
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

public struct StatusChanges: AsyncSequence {
    public struct Iterator: AsyncIteratorProtocol {
        private enum Phase {
            case opening(Result<Observation, ElementFailure>)
            case reading(Observation, ApplicationStatus)
            case finished
        }

        let application: NSRunningApplication
        private var phase: Phase

        init(_ application: NSRunningApplication) {
            self.application = application
            phase = .opening(Result { () throws(ElementFailure) in try Observation(application) })
        }

        public mutating func next() async -> Result<ApplicationStatus, ElementFailure>? {
            let read: Result<(Observation, ApplicationStatus), ElementFailure>? =
                switch phase {
                    case .opening(let observing): observing.flatMap { observation in application.status().map { status in (observation, status) } }
                    case .reading(let observation, let last): await observation.change(from: last, of: application).map { read in read.map { status in (observation, status) } }
                    case .finished: nil
                }
            phase =
                switch read {
                    case .success((let observation, let status)): .reading(observation, status)
                    case .failure, .none: .finished
                }
            return read.map { read in read.map(\.1) }
        }
    }

    let application: NSRunningApplication

    public init(_ application: NSRunningApplication) {
        self.application = application
    }

    public func makeAsyncIterator() -> Iterator {
        Iterator(application)
    }
}

private final class Ticks: Sendable {
    let continuation: AsyncStream<Void>.Continuation

    init(continuation: AsyncStream<Void>.Continuation) {
        self.continuation = continuation
    }
}

private final class Observation {
    let observer: AXObserver
    let ticks: Ticks
    var pending: AsyncStream<Void>.Iterator

    init(_ application: NSRunningApplication) throws(ElementFailure) {
        let names: [String] = [
            kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification, kAXUIElementDestroyedNotification,
            kAXFocusedWindowChangedNotification, kAXMainWindowChangedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
            kAXWindowDeminiaturizedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification,
        ]
        let element: AXUIElement = AXUIElementCreateApplication(application.processIdentifier)
        let (stream, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(application.processIdentifier, observed, &created)
        guard let observer: AXObserver = created else { throw .reading(creation) }
        self.observer = observer
        ticks = Ticks(continuation: tick)
        pending = stream.makeAsyncIterator()
        _ = try traverse(names) { name in
            let added: AXError = unsafe AXObserverAddNotification(observer, element, name as CFString, Unmanaged.passUnretained(ticks).toOpaque())
            return added == .success ? .success(()) : .failure(AggregateError(first: (notification: name, error: added)))
        }
        .mapError(ElementFailure.unobservable).get()
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
    }

    deinit {
        CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        ticks.continuation.finish()
    }

    func change(from last: ApplicationStatus, of application: NSRunningApplication) async -> Result<ApplicationStatus, ElementFailure>? {
        while await pending.next(isolation: #isolation) != nil {
            switch application.status() {
                case .success(last): continue
                case let read: return read
            }
        }
        return nil
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum Accessibility {
    @discardableResult
    public static func prompt() -> Bool {
        AXIsProcessTrustedWithOptions(["AXTrustedCheckOptionPrompt": true] as CFDictionary)
    }

    public static func messagingTimeout(_ seconds: Float) -> Result<Void, ElementFailure> {
        let status: AXError = AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), seconds)
        return status == .success ? .success(()) : .failure(.reading(status))
    }
}

extension AXUIElement {
    // --- [READS]
    func value<Value>(_ attribute: String) throws(ElementFailure) -> Value? {
        var value: CFTypeRef?
        return switch unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) {
            case .success: value as? Value
            case .noValue, .attributeUnsupported: nil
            case let error: throw .reading(error)
        }
    }

    func window() throws(ElementFailure) -> CGWindowID? {
        var id: CGWindowID = 0
        return switch unsafe windowIdentifier(self, &id) {
            case .success: id
            case let error where error == .cannotComplete || error == .apiDisabled: throw .reading(error)
            default: nil
        }
    }

    func actionNames() throws(ElementFailure) -> [String] {
        var names: CFArray?
        return switch unsafe AXUIElementCopyActionNames(self, &names) {
            case .success: names as? [String] ?? []
            case .failure: []
            case let error: throw .reading(error)
        }
    }

    func children() throws(ElementFailure) -> [AXUIElement] { try value(kAXChildrenAttribute) ?? [] }

    func children(where kept: (String?) -> Bool) throws(ElementFailure) -> [AXUIElement] {
        try children().map { child throws(ElementFailure) in try kept(child.value(kAXRoleAttribute)) ? child : nil }.compactMap(\.self)
    }

    func label() throws(ElementFailure) -> String? {
        let labels: [String?] = try [kAXTitleAttribute, kAXDescriptionAttribute].map { attribute throws(ElementFailure) in try value(attribute) }
        return labels.lazy.compactMap(\.self).first { label in !label.isEmpty }
    }

    // --- [WALK]
    func candidates() throws(ElementFailure) -> [AXUIElement] {
        let top: [AXUIElement] = try children { role in role != kAXMenuBarRole }
        let sheets: [AXUIElement] = try top.map { child throws(ElementFailure) in try child.children { role in role == kAXSheetRole } }.flatMap(\.self)
        let menus: [AXUIElement] = try ((value(kAXMenuBarAttribute) as AXUIElement?)?.children() ?? []).map { item throws(ElementFailure) in try item.children() }.flatMap(\.self)
        return top + sheets + menus
    }

    func descendants() throws(ElementFailure) -> [AXUIElement] {
        let collections: Set<String?> = [kAXScrollAreaRole, kAXOutlineRole, kAXTableRole, kAXListRole, kAXBrowserRole]
        return try [self] + children { role in !collections.contains(role) }.map { child throws(ElementFailure) in try child.descendants() }.flatMap(\.self)
    }

    func described() throws(ElementFailure) -> Described? {
        guard let window: CGWindowID = try window(), let role: String = try value(kAXRoleAttribute) else { return nil }
        let subrole: String? = try value(kAXSubroleAttribute)
        let kind: ElementKind =
            switch (subrole, role) {
                case (.some(kAXDialogSubrole), _), (.some(kAXSystemDialogSubrole), _): .dialog
                case (_, kAXSheetRole): .sheet
                case (_, kAXMenuRole): .menu
                default: .window
            }
        let nodes: [AXUIElement] =
            switch kind {
                case .dialog, .sheet: try descendants()
                case .menu: try children()
                case .window: []
            }
        let parts: [Part] = try nodes.map { node throws(ElementFailure) in try Part(node) }
        let close: AXUIElement? = try value(kAXCloseButtonAttribute)
        let cancels: Bool = try actionNames().contains(kAXCancelAction)
        let targets: [(ElementAction, AXUIElement)] =
            parts.compactMap { part in part.press.map { label in (.press(label), part.node) } }
            + [close.map { button in (ElementAction.close, button) }, cancels ? (ElementAction.cancel, self) : nil].compactMap(\.self)
        let title: String? = try kind == .menu ? (value(kAXParentAttribute) as AXUIElement?)?.label() : label()
        return Described(
            element: Element(id: window, kind: kind, role: subrole ?? role, title: title, text: parts.compactMap(\.text), controls: parts.compactMap(\.control), actions: targets.map(\.0)),
            targets: Dictionary(targets) { first, _ in first },
            fields: Dictionary(parts.compactMap { part in part.field.map { label in (label, part.node) } }) { first, _ in first },
        )
    }
}

extension NSRunningApplication {
    // --- [STATUS]
    public func status() -> Result<ApplicationStatus, ElementFailure> {
        let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
        let read: Result<ApplicationStatus, ElementFailure> = Result { () throws(ElementFailure) in
            try ApplicationStatus(
                responding: true,
                frontWindow: (application.value(kAXFocusedWindowAttribute) as AXUIElement?)?.window(),
                elements: application.candidates().map { candidate throws(ElementFailure) in try candidate.described()?.element }.compactMap(\.self),
            )
        }
        return switch read {
            case .failure(.unreadable(.cannotComplete)): .success(ApplicationStatus(responding: false, frontWindow: nil, elements: []))
            case .success, .failure: read
        }
    }

    // --- [ACTIONS]
    public func perform(_ action: ElementAction, on id: CGWindowID) -> Result<Void, ElementFailure> {
        let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
        return Result { () throws(ElementFailure) in
            let found: AXUIElement? = try application.candidates().map { candidate throws(ElementFailure) in try candidate.window() == id ? candidate : nil }.compactMap(\.self).first
            guard let target: Described = try found?.described() else { throw .absent(id) }
            let performed: AXError? =
                switch action {
                    case .set(let field, let value): target.fields[field].map { node in AXUIElementSetAttributeValue(node, kAXValueAttribute as CFString, value as CFString) }
                    case .cancel: target.targets[action].map { node in AXUIElementPerformAction(node, kAXCancelAction as CFString) }
                    case .press, .close: target.targets[action].map { node in AXUIElementPerformAction(node, kAXPressAction as CFString) }
                }
            switch performed {
                case .none: throw .unoffered(action, target.element.actions)
                case .some(.success), .some(.cannotComplete): break
                case .some(let error): throw .refused(action, error)
            }
        }
    }

    public func answer(_ text: String, with action: ElementAction) async -> Result<Void, ElementFailure> {
        for await next: Result<ApplicationStatus, ElementFailure> in StatusChanges(self) {
            switch next.map({ snapshot in snapshot.elements.first { element in ([element.title].compactMap(\.self) + element.text).contains { line in line.contains(text) } } }) {
                case .success(.some(let element)): return perform(action, on: element.id)
                case .success(.none): continue
                case .failure(let error): return .failure(error)
            }
        }
        return .failure(.cancelled)
    }
}

private let observed: AXObserverCallback = { _, _, _, refcon in
    guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
    unsafe Unmanaged<Ticks>.fromOpaque(refcon).takeUnretainedValue().continuation.yield(())
}

@_silgen_name("_AXUIElementGetWindow")
private func windowIdentifier(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError
