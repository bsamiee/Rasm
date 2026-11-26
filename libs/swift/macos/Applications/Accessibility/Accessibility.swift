import AppKit
@unsafe @preconcurrency public import ApplicationServices
public import CasePaths
import CasePathsCore
public import Encoding
public import Foundation
import SPI

// --- [CONSTANTS] -----------------------------------------------------------------------

let messagingTimeout: Duration = .seconds(1)
let frameAttribute: String = "AXFrame"
let userInputLabelsAttribute: String = "AXUserInputLabels"
let fullScreenAttribute: String = "AXFullScreen"
let sectionsAttribute: String = "AXSections"
let contentSizeAttribute: String = "AXContentSize"
let manualAccessibilityAttribute: String = "AXManualAccessibility"
let preferredLanguageAttribute: String = "AXPreferredLanguage"
let frameAttributes: [String] = [frameAttribute, NSAccessibility.Attribute.activationPoint.rawValue]
let elementAttributes: [String] = [
    kAXRoleAttribute, kAXSubroleAttribute, kAXRoleDescriptionAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXHelpAttribute, kAXIdentifierAttribute, kAXDOMIdentifierAttribute,
    userInputLabelsAttribute, kAXValueAttribute, kAXEnabledAttribute, kAXFocusedAttribute, kAXSelectedAttribute, kAXParentAttribute, kAXChildrenAttribute, kAXTitleUIElementAttribute,
    kAXPlaceholderValueAttribute, kAXExpandedAttribute, kAXVisibleCharacterRangeAttribute, frameAttribute,
]
let windowAttributes: [String] =
    elementAttributes + [
        kAXModalAttribute, kAXMainAttribute, kAXDocumentAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute, kAXCloseButtonAttribute, fullScreenAttribute, sectionsAttribute,
    ]
let webAttributes: [String] = [
    kAXInvalidAttribute, NSAccessibility.Attribute.required.rawValue, kAXARIACurrentAttribute, kAXARIARowCountAttribute, kAXLoadedAttribute, kAXLoadingProgressAttribute,
]
let menuAttributes: [String] = [
    kAXRoleAttribute, kAXSubroleAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXHelpAttribute, kAXIdentifierAttribute, kAXEnabledAttribute, kAXSelectedAttribute,
    kAXMenuItemCmdCharAttribute, kAXMenuItemCmdVirtualKeyAttribute, kAXMenuItemCmdGlyphAttribute, kAXMenuItemCmdModifiersAttribute, kAXMenuItemMarkCharAttribute, kAXURLAttribute,
    kAXChildrenAttribute, kAXParentAttribute, frameAttribute,
]
private let declaredActions: Set<String> = [
    kAXPressAction, kAXIncrementAction, kAXDecrementAction, kAXConfirmAction, kAXPickAction, kAXCancelAction, kAXRaiseAction, kAXShowMenuAction, kAXShowAlternateUIAction,
    kAXShowDefaultUIAction, NSAccessibility.Action.delete.rawValue, NSAccessibility.Action.scrollToVisibleAction.rawValue,
]
private let windowRoles: Set<String> = [kAXWindowRole, kAXSheetRole, kAXDrawerRole, kAXPopoverRole]

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
@dynamicMemberLookup
public enum Fact: CaseTagged, Hashable, Sendable {
    public enum Scalar: Codable, Hashable, Sendable {
        case bool(Bool)
        case number(Double)
        case text(String)
        case date(Date)
        case rect(CGRect)

        public init(from decoder: any Decoder) throws {
            let value = try decoder.singleValueContainer()
            self =
                try value.decodeIfMatching(Bool.self).map(Self.bool) ?? value.decodeIfMatching(Double.self).map(Self.number) ?? value.decodeIfMatching(CGRect.self).map(Self.rect)
                ?? .text(value.decode(String.self))
        }

        public func encode(to encoder: any Encoder) throws {
            var value = encoder.singleValueContainer()
            switch self {
                case .bool(let flag): try value.encode(flag)
                case .number(let number): try value.encode(number)
                case .text(let text): try value.encode(text)
                case .date(let date): try value.encode(date)
                case .rect(let frame): try value.encode(frame)
            }
        }
    }

    case bool(value: Bool)
    case integer(value: Int)
    case real(value: Double)
    case text(value: String)
    case date(value: Date)
    case url(value: URL)
    case point(value: CGPoint)
    case size(value: CGSize)
    case rect(value: CGRect)
    case color(red: Double, green: Double, blue: Double, alpha: Double)
    case range(location: Int, length: Int)
    case element(id: Int)
    case list(values: [Self])
    case record(fields: [String: Self])
    case refused(error: Lenient<AXError>)

    public var number: Double? { self.integer.map(Double.init) ?? self.real }

    public var elements: [Int] { self.list?.compactMap(\.element) ?? self.element.map { id in [id] } ?? [] }

    public var scalar: Scalar? {
        switch self {
            case .bool(let flag): .bool(flag)
            case .integer(let number): .number(Double(number))
            case .real(let number): .number(number)
            case .text(let text): .text(text)
            case .url(let url): .text(url.absoluteString)
            case .date(let date): .date(date)
            case .point, .size, .rect, .color, .range, .element, .list, .record, .refused: nil
        }
    }
}

public struct Element: Hashable, Sendable {
    public let id: Int
    public let facts: [String: Fact]
    public let actions: [String]
    public var window: CGWindowID?
    public var descriptions: [String: String] = [:]
    public var counts: [String: Int] = [:]

    public func text(_ attribute: String) -> String? { facts[attribute]?.text.flatMap { text in text.isEmpty ? nil : text } }

    public var role: String? { text(kAXRoleAttribute) }

    public var frame: CGRect? { facts[frameAttribute]?.rect }

    public var activation: CGPoint? { facts[NSAccessibility.Attribute.activationPoint.rawValue]?.point }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Fact {
    init?(_ value: CFTypeRef, interning intern: (AXUIElement) -> Int) {
        let fact: Self? =
            switch CFGetTypeID(value) {
                case CFBooleanGetTypeID(): (value as? Bool).map(Self.bool(value:))
                case CFNumberGetTypeID():
                    (value as? NSNumber).map { (number: NSNumber) -> Self in CFNumberIsFloatType(number as CFNumber) ? .real(value: number.doubleValue) : .integer(value: number.intValue) }
                case CFStringGetTypeID(): (value as? String).map(Self.text(value:))
                case CFAttributedStringGetTypeID(): (value as? NSAttributedString).map { (text: NSAttributedString) -> Self in .text(value: text.string) }
                case CFDateGetTypeID(): (value as? Date).map(Self.date(value:))
                case CFURLGetTypeID(): (value as? URL).map { (url: URL) -> Self in .url(value: (unsafe CFURLCreateFilePathURL(nil, url as CFURL, nil)?.takeRetainedValue() ?? url as CFURL) as URL) }
                case CFArrayGetTypeID(): (value as? [AnyObject]).map { (items: [AnyObject]) -> Self in .list(values: items.compactMap { item in Self(item, interning: intern) }) }
                case CFDictionaryGetTypeID():
                    (value as? [String: AnyObject]).map { (fields: [String: AnyObject]) -> Self in .record(fields: fields.compactMapValues { field in Self(field, interning: intern) }) }
                case CGColor.typeID:
                    CGColorSpace(name: CGColorSpace.sRGB).flatMap { space in unsafe unsafeDowncast(value, to: CGColor.self).converted(to: space, intent: .defaultIntent, options: nil)?.components }
                        .flatMap { (parts: [CGFloat]) -> Self? in parts.count == 4 ? .color(red: parts[0], green: parts[1], blue: parts[2], alpha: parts[3]) : nil }
                case AXUIElementGetTypeID(): .element(id: intern(unsafe unsafeDowncast(value, to: AXUIElement.self)))
                case AXValueGetTypeID():
                    switch AXValueGetType(unsafe unsafeDowncast(value, to: AXValue.self)) {
                        case .cgPoint: AXValue.unpack(value, into: CGPoint.zero).map(Self.point(value:))
                        case .cgSize: AXValue.unpack(value, into: CGSize.zero).map(Self.size(value:))
                        case .cgRect: AXValue.unpack(value, into: CGRect.zero).map(Self.rect(value:))
                        case .cfRange: AXValue.unpack(value, into: CFRange()).map { (range: CFRange) -> Self in .range(location: range.location, length: range.length) }
                        case .axError:
                            AXValue.unpack(value, into: AXError.success).flatMap { (error: AXError) -> Self? in
                                [.noValue, .attributeUnsupported].contains(error) ? nil : .refused(error: Lenient(rawValue: error.rawValue))
                            }
                        case .illegal: nil
                        @unknown default: nil
                    }
                default: nil
            }
        guard let fact else { return nil }
        self = fact
    }
}

extension Fact.Scalar {
    var number: Double? {
        switch self {
            case .number(let number): number
            case .text(let text): Double(text.trimmingCharacters(in: .whitespaces))
            case .bool, .date, .rect: nil
        }
    }

    var text: String? {
        switch self {
            case .text(let text): text
            case .number(let number): number.formatted(FloatingPointFormatStyle<Double>(locale: Locale(identifier: "en_US_POSIX")).grouping(.never).precision(.significantDigits(1...17)))
            case .bool, .date, .rect: nil
        }
    }

    var accessibility: CFTypeRef? {
        switch self {
            case .bool(let flag): flag as NSNumber
            case .number(let number): number as NSNumber
            case .text(let text): text as CFString
            case .date(let date): date as CFDate
            case .rect(var frame): unsafe AXValueCreate(.cgRect, &frame)
        }
    }
}

extension AXValue {
    fileprivate static func unpack<Value: BitwiseCopyable>(_ slot: CFTypeRef, into empty: Value) -> Value? {
        let boxed: AXValue = unsafe unsafeDowncast(slot, to: AXValue.self)
        var value = empty
        return unsafe AXValueGetValue(boxed, AXValueGetType(boxed), &value) ? value : nil
    }
}

extension AXUIElement {
    // --- [IDENTITY]

    var process: pid_t? {
        var pid: pid_t = 0
        return unsafe AXUIElementGetPid(self, &pid) == .success ? pid : nil
    }

    var window: CGWindowID? {
        var (number, connection, owner): (CGWindowID, Int32, pid_t) = (0, 0, 0)
        guard unsafe windowNumber(self, &number) == .success else { return nil }
        guard let process, unsafe windowOwner(mainConnection(), number, &connection) == .success, unsafe processOfConnection(connection, &owner) == .success, owner != process else {
            return number
        }
        return InstanceState.Window.all(.optionAll, of: process)[process]?.first { row in row.remote == number }?.number ?? number
    }

    static func cast(_ slot: CFTypeRef) -> AXUIElement? { CFGetTypeID(slot) == AXUIElementGetTypeID() ? unsafe unsafeDowncast(slot, to: AXUIElement.self) : nil }

    // --- [READS]

    private func copy(_ attributes: [String], interning intern: (AXUIElement) -> Int) -> (answer: AXError, facts: [String: Fact]) {
        var values: CFArray?
        let answer = unsafe AXUIElementCopyMultipleAttributeValues(self, attributes as CFArray, [], &values)
        let facts = zip(attributes, values as? [AnyObject] ?? []).compactMap { attribute, slot in Fact(slot, interning: intern).map { fact in (attribute, fact) } }
        return (answer, Dictionary(uniqueKeysWithValues: facts))
    }

    func element(_ id: Int, reading attributes: [String], interning intern: (AXUIElement) -> Int) -> (answer: AXError, element: Element) {
        let answered: [AXError] = [.success, .failure, .attributeUnsupported, .actionUnsupported]
        var names: CFArray?
        let (answer, facts) = copy(attributes, interning: intern)
        let acted: AXError = answer == .success ? unsafe AXUIElementCopyActionNames(self, &names) : answer
        let read = Element(id: id, facts: facts, actions: names as? [String] ?? [], window: nil)
        let (wanted, counted): (read: [String], counted: [String]) = answered.contains(acted) ? read.supplementary : ([], [])
        let missing: [String] = wanted.filter { attribute in !attributes.contains(attribute) }
        let (extended, added) = missing.isEmpty ? (AXError.success, [:]) : copy(missing, interning: intern)
        let descriptions: [(String, String)] = read.actions.filter { action in !declaredActions.contains(action) }.compactMap { action in
            var text: CFString?
            return unsafe AXUIElementCopyActionDescription(self, action as CFString, &text) == .success ? (text as String?).map { text in (action, text) } : nil
        }
        let counts: [(String, Int)] = counted.compactMap { attribute in
            var total: CFIndex = 0
            return unsafe AXUIElementGetAttributeValueCount(self, attribute as CFString, &total) == .success ? (attribute, total) : nil
        }
        return (
            [acted, extended].first { error in !answered.contains(error) } ?? answer,
            Element(
                id: id,
                facts: facts.merging(added) { first, _ in first },
                actions: read.actions,
                window: answer == .success && read.role.map(windowRoles.contains) == true ? window : nil,
                descriptions: Dictionary(descriptions) { first, _ in first },
                counts: Dictionary(counts) { first, _ in first },
            ),
        )
    }

    func element(reading attributes: [String]) throws(ApplicationFailure) -> (element: Element, handles: [AXUIElement]) {
        var handles: [AXUIElement] = [self]
        var (answer, read) = element(0, reading: attributes) { handle in
            handles.append(handle)
            return handles.count - 1
        }
        guard [.success, .invalidUIElement].contains(answer) else { throw failure(answer, reading: attributes.joined(separator: ", ")) }
        read.window = answer == .success ? read.window ?? window : nil
        return (read, handles)
    }

    func read(_ attributes: [String]) throws(ApplicationFailure) -> (element: Element, handles: [AXUIElement]) {
        var handles: [AXUIElement] = [self]
        let (answer, facts) = copy(attributes) { handle in
            handles.append(handle)
            return handles.count - 1
        }
        guard [.success, .invalidUIElement].contains(answer) else { throw failure(answer, reading: attributes.joined(separator: ", ")) }
        return (Element(id: 0, facts: facts, actions: [], window: nil), handles)
    }

    func elements(_ attribute: String) throws(ApplicationFailure) -> [AXUIElement] {
        var value: CFTypeRef?
        return switch unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) {
            case .success, .noValue, .attributeUnsupported, .invalidUIElement: ((value as? [AnyObject]) ?? value.map { slot in [slot] } ?? []).compactMap(Self.cast)
            case let error: throw failure(error, reading: attribute)
        }
    }

    func parameterized(_ attribute: String, _ parameter: CFTypeRef) throws(ApplicationFailure) -> CFTypeRef? {
        var value: CFTypeRef?
        return switch unsafe AXUIElementCopyParameterizedAttributeValue(self, attribute as CFString, parameter, &value) {
            case .success, .noValue: value
            case let error: throw failure(error, reading: attribute)
        }
    }

    func search(_ text: String, key: NSAccessibilitySearchKey, limit: Int) throws(ApplicationFailure) -> [AXUIElement] {
        let predicate: [String: Any] = [
            NSAccessibilitySearchIdentifiersKey: key.rawValue, NSAccessibilitySearchResultsLimitKey: limit, NSAccessibilitySearchDirectionKey: NSAccessibilitySearchDirectionNext,
        ]
        let found = try parameterized(
            NSAccessibility.ParameterizedAttribute.uiElementsForSearchPredicateParameterizedAttribute.rawValue,
            predicate.merging(text.isEmpty ? [:] : [NSAccessibilitySearchTextKey: text]) { first, _ in first } as CFDictionary,
        )
        return (found as? [AnyObject] ?? []).compactMap(Self.cast)
    }

    func runs(interning intern: (AXUIElement) -> Int) throws(ApplicationFailure) -> [Control.Run] {
        guard case .range(let start, let length) = copy([kAXVisibleCharacterRangeAttribute], interning: intern).facts[kAXVisibleCharacterRangeAttribute] else { return [] }
        var visible = CFRange(location: start, length: length)
        let span: AXValue? = unsafe AXValueCreate(.cfRange, &visible)
        guard let span, let styled = try parameterized(kAXAttributedStringForRangeParameterizedAttribute, span) as? NSAttributedString else { return [] }
        var spans: [(range: NSRange, attributes: [NSAttributedString.Key: Any])] = []
        unsafe styled.enumerateAttributes(in: NSRange(location: 0, length: styled.length)) { attributes, range, _ in spans.append((range, attributes)) }
        return try spans.map { (range: NSRange, attributes: [NSAttributedString.Key: Any]) throws(ApplicationFailure) -> Control.Run in
            var bounds = CFRange(location: start + range.location, length: range.length)
            let boxed: AXValue? = unsafe AXValueCreate(.cfRange, &bounds)
            let frame: CFTypeRef? = try boxed.flatMap { boxed throws(ApplicationFailure) in try parameterized(kAXBoundsForRangeParameterizedAttribute, boxed) }
            return Control.Run(
                range: Control.TextRange(location: bounds.location, length: bounds.length),
                text: styled.attributedSubstring(from: range).string,
                attributes: Dictionary(uniqueKeysWithValues: attributes.compactMap { key, value in Fact(value as AnyObject, interning: intern).map { fact in (key.rawValue, fact) } }),
                frame: frame.flatMap { value in Fact(value, interning: intern)?.rect },
            )
        }
    }

    func settable(_ attribute: String) throws(ApplicationFailure) -> Bool {
        var settable: DarwinBoolean = false
        return switch unsafe AXUIElementIsAttributeSettable(self, attribute as CFString, &settable) {
            case .success: settable.boolValue
            case .noValue, .attributeUnsupported, .invalidUIElement: false
            case let error: throw failure(error, reading: attribute)
        }
    }

    func failure(_ error: AXError, reading attribute: String) -> ApplicationFailure {
        ApplicationFailure(error, subject: process.map { pid in "\(attribute) of process \(pid)" } ?? attribute, process: process) {
            let named = Element(id: 0, facts: copy([kAXRoleAttribute, kAXTitleAttribute, kAXDescriptionAttribute]) { _ in 0 }.facts, actions: [], window: nil)
            let role: String = named.role ?? kAXUnknownRole
            let subject: String = (named.text(kAXTitleAttribute) ?? named.text(kAXDescriptionAttribute)).map { label in "\(role) \(String(reflecting: label))" } ?? role
            return ApplicationFailure(subject: "\(attribute) of \(subject)", reason: .accessibility(error))
        }
    }
}
