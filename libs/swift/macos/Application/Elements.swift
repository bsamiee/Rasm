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

public struct Element: Codable, Hashable, Sendable {
    public let id: CGWindowID
    public let kind: ElementKind
    public let role: String
    public let title: String?
    public let text: [String]
    public let buttons: [String]
}

public struct ElementNotification: Sendable {
    public let name: String
    public let element: Element?
}

public struct Elements: Sendable {
    public let front: CGWindowID?
    public let open: [Element]
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ElementFailure: LocalizedError {
    case untrusted
    case unresponsive
    case unreadable(AXError)
    case absent(CGWindowID)
    case noButton(String, [String])
    case refused(String, AXError)

    public var errorDescription: String? {
        switch self {
            case .untrusted: "Accessibility access is off for this process: allow it under System Settings, Privacy & Security, Accessibility"
            case .unresponsive: "The application did not answer an Accessibility request before its timeout"
            case .unreadable(let error): "Accessibility read failed with AXError \(error.rawValue)"
            case .absent(let id): "No open window, dialog, sheet, or menu with id \(id)"
            case .noButton(let label, let buttons): "No button \(label) on the element, buttons: \(buttons.joined(separator: ", "))"
            case .refused(let label, let error): "Pressing \(label) returned AXError \(error.rawValue)"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum Accessibility {
    public static func trusted(prompting: Bool) -> Bool {
        AXIsProcessTrustedWithOptions(["AXTrustedCheckOptionPrompt": prompting] as CFDictionary)
    }
}

extension AXUIElement {
    // --- [READS]
    subscript<Value>(attribute: String) -> Value? {
        var value: CFTypeRef?
        return unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) == .success ? value as? Value : nil
    }

    var window: CGWindowID? {
        var id: CGWindowID = 0
        return unsafe windowIdentifier(self, &id) == .success ? id : nil
    }

    var role: String? { self[kAXRoleAttribute] }
    var children: [AXUIElement] { self[kAXChildrenAttribute] ?? [] }
    var label: String? { [kAXTitleAttribute, kAXDescriptionAttribute].lazy.compactMap { attribute in self[attribute] as String? }.first { label in !label.isEmpty } }
    var candidates: [AXUIElement] {
        let menuBar: AXUIElement? = self[kAXMenuBarAttribute]
        let top: [AXUIElement] = children.filter { child in child.role != kAXMenuBarRole }
        return top + top.flatMap { child in child.children.filter { inner in inner.role == kAXSheetRole } } + (menuBar?.children ?? []).flatMap(\.children)
    }

    var descendants: [AXUIElement] {
        let collections: Set<String?> = [kAXScrollAreaRole, kAXOutlineRole, kAXTableRole, kAXListRole, kAXBrowserRole]
        return [self] + children.filter { child in !collections.contains(child.role) }.flatMap(\.descendants)
    }
    func described() -> (element: Element, buttons: [(AXUIElement, String)])? {
        let kinds: [String: ElementKind] = [kAXMenuRole: .menu, kAXSheetRole: .sheet, kAXLayoutAreaRole: .dialog, kAXDialogSubrole: .dialog, kAXSystemDialogSubrole: .dialog]
        guard let window, let role else { return nil }
        let subrole: String? = self[kAXSubroleAttribute]
        let kind: ElementKind = subrole.flatMap { subrole in kinds[subrole] } ?? kinds[role] ?? .window
        let nodes: [AXUIElement] = kind == .dialog || kind == .sheet ? descendants : []
        let text: [String] = nodes.compactMap { node in node.role == kAXStaticTextRole ? node[kAXValueAttribute] as String? : nil }.filter { line in !line.isEmpty }
        let close: [(AXUIElement, String)] =
            kind == .menu ? [(self, kAXCancelAction)] : (self[kAXCloseButtonAttribute] as AXUIElement?).map { button in [(button, kAXCloseButtonAttribute)] } ?? []
        let buttons: [(AXUIElement, String)] = nodes.compactMap { node in node.role == kAXButtonRole ? node.label.map { label in (node, label) } : nil } + close
        let title: String? = kind == .menu ? (self[kAXParentAttribute] as AXUIElement?)?.label : label
        return (Element(id: window, kind: kind, role: subrole ?? role, title: title, text: text, buttons: buttons.map(\.1)), buttons)
    }
}

extension NSRunningApplication {
    // --- [ELEMENTS]
    private var element: Result<AXUIElement, ElementFailure> {
        guard Accessibility.trusted(prompting: false) else { return .failure(.untrusted) }
        let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
        AXUIElementSetMessagingTimeout(application, 1)
        return .success(application)
    }
    public func elements() -> Result<Elements, ElementFailure> {
        element.flatMap { application in
            var children: CFTypeRef?
            return switch unsafe AXUIElementCopyAttributeValue(application, kAXChildrenAttribute as CFString, &children) {
                case .cannotComplete: .failure(.unresponsive)
                case .success, .noValue, .attributeUnsupported:
                    .success(
                        Elements(front: (application[kAXFocusedWindowAttribute] as AXUIElement?)?.window, open: application.candidates.compactMap { candidate in candidate.described()?.element })
                    )
                case let error: .failure(.unreadable(error))
            }
        }
    }

    @MainActor
    public func elementNotifications<Value>(_ names: [String], _ body: (AsyncStream<ElementNotification>) async -> Value) async -> Result<Value, ElementFailure> {
        guard Accessibility.trusted(prompting: false) else { return .failure(.untrusted) }
        var created: AXObserver?
        let status: AXError = unsafe AXObserverCreate(processIdentifier, observed, &created)
        guard status == .success, let observer: AXObserver = created else { return .failure(.unreadable(status)) }
        let (notifications, continuation): (AsyncStream<ElementNotification>, AsyncStream<ElementNotification>.Continuation) = AsyncStream.makeStream()
        let observation: Observation = Observation(continuation: continuation)
        let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
        for name: String in names { unsafe AXObserverAddNotification(observer, application, name as CFString, Unmanaged.passUnretained(observation).toOpaque()) }
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        let value: Value = await body(notifications)
        CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        continuation.finish()
        return .success(value)
    }

    // --- [ACTIONS]
    public func press(_ button: String, on id: CGWindowID) -> Result<Void, ElementFailure> {
        element.flatMap { application in
            guard let found: (element: Element, buttons: [(AXUIElement, String)]) = application.candidates.lazy.compactMap({ candidate in candidate.window == id ? candidate.described() : nil }).first
            else {
                return .failure(.absent(id))
            }
            guard let target: AXUIElement = found.buttons.first(where: { _, label in label == button })?.0 else { return .failure(.noButton(button, found.element.buttons)) }
            let action: String = found.element.kind == .menu ? kAXCancelAction : kAXPressAction
            let pressed: AXError = AXUIElementPerformAction(target, action as CFString)
            return pressed == .success ? .success(()) : .failure(.refused(button, pressed))
        }
    }
}

private final class Observation: Sendable {
    let continuation: AsyncStream<ElementNotification>.Continuation

    init(continuation: AsyncStream<ElementNotification>.Continuation) {
        self.continuation = continuation
    }
}

private let observed: AXObserverCallback = { _, element, notification, info in
    guard let info: UnsafeMutableRawPointer = unsafe info else { return }
    let observation: Observation = unsafe Unmanaged<Observation>.fromOpaque(info).takeUnretainedValue()
    observation.continuation.yield(ElementNotification(name: notification as String, element: element.described()?.element))
}

@_silgen_name("_AXUIElementGetWindow")
private func windowIdentifier(_ element: AXUIElement, _ identifier: UnsafeMutablePointer<CGWindowID>) -> AXError
