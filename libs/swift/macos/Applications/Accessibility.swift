public import AppKit
public import ApplicationServices
public import JSON
public import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

private let labelAttributes: [String] = [kAXTitleAttribute, kAXDescriptionAttribute]

// --- [MODELS] --------------------------------------------------------------------------

public struct Control: Codable, Hashable, Sendable {
    public let role: String
    public let label: String?
    public let value: JSONValue?
}

public enum ElementAction: Hashable, Sendable {
    case press(String)
    case set(field: String, value: String)

    public var label: String {
        switch self {
            case .press(let label), .set(let label, _): label
        }
    }
}

extension ElementAction: Codable {
    private enum Key: CodingKey { case field, value }

    public init(from decoder: any Decoder) throws {
        self = try Result { try .press(decoder.singleValueContainer().decode(String.self)) }
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
            case .press(let label):
                var single: any SingleValueEncodingContainer = encoder.singleValueContainer()
                try single.encode(label)
            case .set(let field, let value):
                var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
                try fields.encode(field, forKey: .field)
                try fields.encode(value, forKey: .value)
        }
    }
}

public struct FailedRead: Hashable, Sendable {
    public let reads: [String]
    public let role: String?
    public let label: String?
    public let error: AXError
}

extension FailedRead: Codable {
    private enum Key: CodingKey { case reads, role, label, error }

    public init(_ error: AXError, reading reads: [String], on element: AXUIElement) {
        let named: [String] = [kAXRoleAttribute] + labelAttributes
        var values: CFArray?
        _ = unsafe AXUIElementCopyMultipleAttributeValues(element, named as CFArray, [], &values)
        let names: [String: String] = Dictionary(
            uniqueKeysWithValues: zip(named, values as? [CFTypeRef] ?? []).compactMap { attribute, value in (value as? String).map { name in (attribute, name) } }
        )
        self.init(reads: reads, role: names[kAXRoleAttribute], label: labelAttributes.lazy.compactMap { attribute in names[attribute] }.first { label in !label.isEmpty }, error: error)
    }

    public init(from decoder: any Decoder) throws {
        let fields: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let code: Int32 = try fields.decode(Int32.self, forKey: .error)
        guard let error: AXError = AXError(rawValue: code) else { throw DecodingError.dataCorruptedError(forKey: .error, in: fields, debugDescription: "AXError \(code) names no case") }
        try self.init(
            reads: fields.decode([String].self, forKey: .reads),
            role: fields.decodeIfPresent(String.self, forKey: .role),
            label: fields.decodeIfPresent(String.self, forKey: .label),
            error: error,
        )
    }

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encode(reads, forKey: .reads)
        try fields.encodeIfPresent(role, forKey: .role)
        try fields.encodeIfPresent(label, forKey: .label)
        try fields.encode(error.rawValue, forKey: .error)
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

public enum ApplicationStatus: Equatable, Sendable {
    case responding([Element])
    case unresponsive
}

extension ApplicationStatus: Encodable {
    private enum Key: String, CodingKey {
        case elements
        case tag = "_tag"
    }

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch self {
            case .responding(let elements):
                try fields.encode("responding", forKey: .tag)
                try fields.encode(elements, forKey: .elements)
            case .unresponsive: try fields.encode("unresponsive", forKey: .tag)
        }
    }
}

struct Described {
    let element: Element
    let targets: [(name: String, node: AXUIElement, action: String)]
    let fields: [(label: String, node: AXUIElement)]
}

private struct Part {
    let node: AXUIElement
    let text: String?
    let control: Control?
    let press: String?

    init(_ node: AXUIElement) throws(ElementFailure) {
        let attributes: [String: CFTypeRef] = try node.values([kAXRoleAttribute, kAXEnabledAttribute, kAXValueAttribute] + labelAttributes)
        let role: String? = attributes[kAXRoleAttribute] as? String
        let label: String? = try node.label(attributes)
        let actions: [String] = try label == nil || attributes[kAXEnabledAttribute] as? Bool == false ? [] : node.actionNames()
        self.node = node
        text = role == kAXStaticTextRole ? (attributes[kAXValueAttribute] as? String).flatMap { line in line.isEmpty ? nil : line } : nil
        control = role.flatMap { role in
            switch role {
                case kAXTextFieldRole, kAXTextAreaRole, kAXComboBoxRole, kAXPopUpButtonRole, kAXCheckBoxRole, kAXRadioButtonRole:
                    Control(role: role, label: label, value: attributes[kAXValueAttribute].flatMap(JSONValue.init(accessibility:)))
                default: nil
            }
        }
        press = label.flatMap { label in actions.contains(kAXPressAction) && !["...", "…"].contains { ellipsis in label.hasSuffix(ellipsis) } ? label : nil }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ElementFailure: LocalizedError {
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
                "Accessibility read of \(read.reads.joined(separator: ", ")) on \(read.role ?? "an element")\(read.label.map { label in " \"\(label)\"" } ?? "") failed with AXError \(read.error.rawValue)"
            case .unobservable(let refusals):
                "The application refused Accessibility notifications: \(refusals.errors.map { refusal in "\(refusal.notification) (AXError \(refusal.error.rawValue))" }.joined(separator: ", "))"
            case .absent(let id): "No open window, dialog, sheet, or menu with id \(id)"
            case .unmatched(let miss): miss.errorDescription
            case .refused(let action, let error): "Performing \(action) returned AXError \(error.rawValue)"
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

let accessibility: DispatchQueue = DispatchQueue(label: "Applications.accessibility", attributes: .concurrent)

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
                switch (phase, application.isTerminated) {
                    case (.finished, _), (_, true): nil
                    case (.opening(.failure(let error)), false): .failure(error)
                    case (.opening(.success(let observation)), false): await application.status().map { status in (observation, status) }
                    case (.reading(let observation, let last), false): await observation.change(from: last, of: application).map { read in read.map { status in (observation, status) } }
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
    let lifetime: NSKeyValueObservation
    var pending: AsyncStream<Void>.Iterator

    init(_ application: NSRunningApplication) throws(ElementFailure) {
        let names: [String] = [
            kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification, kAXMenuClosedNotification, kAXUIElementDestroyedNotification,
            kAXFocusedWindowChangedNotification, kAXMainWindowChangedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
            kAXWindowDeminiaturizedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification, kAXValueChangedNotification,
        ]
        let element: AXUIElement = AXUIElementCreateApplication(application.processIdentifier)
        let (stream, tick): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
        var created: AXObserver?
        let creation: AXError = unsafe AXObserverCreate(application.processIdentifier, observed, &created)
        guard let observer: AXObserver = created else { throw .reading(creation, ["AXObserverCreate"], on: element) }
        self.observer = observer
        ticks = Ticks(continuation: tick)
        lifetime = application.observe(\.isTerminated, options: [.initial, .new]) { instance, _ in
            if instance.isTerminated { tick.finish() }
        }
        pending = stream.makeAsyncIterator()
        _ = try traverse(names) { name in
            let added: AXError = unsafe AXObserverAddNotification(observer, element, name as CFString, Unmanaged.passUnretained(ticks).toOpaque())
            return added == .success ? .success(()) : .failure(AggregateError(first: (notification: name, error: added)))
        }
        .mapError { refusals -> ElementFailure in refusals.first.error == .apiDisabled ? .untrusted : .unobservable(refusals) }.get()
        CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
    }

    deinit {
        CFRunLoopRemoveSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode)
        lifetime.invalidate()
        ticks.continuation.finish()
    }

    func change(from last: ApplicationStatus, of application: NSRunningApplication) async -> Result<ApplicationStatus, ElementFailure>? {
        while await pending.next(isolation: #isolation) != nil, !application.isTerminated {
            switch await application.status() {
                case .success(last): continue
                case let read: return read
            }
        }
        return nil
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension JSONValue {
    init?(accessibility value: CFTypeRef) {
        switch value {
            case let flag as CFBoolean where CFGetTypeID(value) == CFBooleanGetTypeID(): self = .bool(CFBooleanGetValue(flag))
            case let number as Int: self = .number(Decimal(number))
            case let number as Double: self = .number(Decimal(number))
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

    func label(_ attributes: [String: CFTypeRef]) throws(ElementFailure) -> String? {
        switch labelAttributes.lazy.compactMap({ attribute in attributes[attribute] as? String }).first(where: { label in !label.isEmpty }) {
            case .some(let label): label
            case .none: try (value(kAXTitleUIElementAttribute) as AXUIElement?)?.value(kAXValueAttribute)
        }
    }

    // --- [WALK]
    func candidates() throws(ElementFailure) -> [AXUIElement] {
        let menus: [AXUIElement] = try ((value(kAXMenuBarAttribute) as AXUIElement?)?.children() ?? []).map { item throws(ElementFailure) in try item.children() }.flatMap(\.self)
        return try menus + children { role in role != kAXMenuBarRole }.map { child throws(ElementFailure) in try child.children { role in role == kAXSheetRole } + [child] }.flatMap(\.self)
    }

    func descendants() throws(ElementFailure) -> [AXUIElement] {
        let collections: Set<String?> = [kAXScrollAreaRole, kAXOutlineRole, kAXTableRole, kAXListRole, kAXBrowserRole]
        return try [self] + children { role in !collections.contains(role) }.map { child throws(ElementFailure) in try child.descendants() }.flatMap(\.self)
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
        let attributes: [String: CFTypeRef] = try values([kAXRoleAttribute, kAXSubroleAttribute] + labelAttributes)
        guard let role: String = attributes[kAXRoleAttribute] as? String else { return nil }
        let subrole: String? = attributes[kAXSubroleAttribute] as? String
        let nodes: [AXUIElement] =
            switch (subrole, role) {
                case (.some(kAXDialogSubrole), _), (.some(kAXSystemDialogSubrole), _), (_, kAXSheetRole), (_, kAXLayoutAreaRole): try descendants()
                case (_, kAXMenuRole): try children()
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

extension NSRunningApplication {
    // --- [STATUS]
    @concurrent
    public func status() async -> Result<ApplicationStatus, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            let read: Result<ApplicationStatus, ElementFailure> = Result { () throws(ElementFailure) in
                try .responding(application.candidates().map { candidate throws(ElementFailure) in try candidate.described()?.element }.compactMap(\.self))
            }
            return switch read {
                case .failure(.unanswered): .success(.unresponsive)
                case .success, .failure: read
            }
        }
    }

    // --- [ACTIONS]
    @concurrent
    public func perform(_ action: ElementAction, on id: CGWindowID) async -> Result<Void, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            return Result { () throws(ElementFailure) in
                let found: AXUIElement? = try application.candidates().map { candidate throws(ElementFailure) in try candidate.window() == id ? candidate : nil }.compactMap(\.self).first
                guard let target: Described = try found?.described() else { throw .absent(id) }
                let (kind, offered, performs): (String, [String], [() -> AXError]) =
                    switch action {
                        case .set(let field, let value):
                            (
                                "control", target.element.controls.compactMap(\.label),
                                target.fields.filter { entry in entry.label == field }.map { entry in { AXUIElementSetAttributeValue(entry.node, kAXValueAttribute as CFString, value as CFString) } },
                            )
                        case .press(let name):
                            (
                                "action", target.element.actions,
                                target.targets.filter { entry in entry.name == name }.map { entry in { AXUIElementPerformAction(entry.node, entry.action as CFString) } },
                            )
                    }
                let perform: () -> AXError =
                    switch (performs.first, performs.count) {
                        case (.some(let perform), 1): perform
                        case (.none, _): throw .unmatched(Unmatched(subject: "\(kind) of element \(id)", query: action.label, candidates: offered))
                        case (.some, _): throw .unmatched(Unmatched(subject: "single \(kind) of element \(id)", query: action.label, candidates: offered))
                    }
                switch perform() {
                    case .success, .cannotComplete: break
                    case let error: throw .refused(action.label, error)
                }
            }
        }
    }
}

private let observed: AXObserverCallback = { _, _, _, refcon in
    guard let refcon: UnsafeMutableRawPointer = unsafe refcon else { return }
    unsafe Unmanaged<Ticks>.fromOpaque(refcon).takeUnretainedValue().continuation.yield(())
}

@_silgen_name("_AXUIElementGetWindow")
private func windowIdentifier(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError
