import AppKit
@unsafe @preconcurrency public import ApplicationServices
public import CasePaths
import CasePathsCore
public import Encoding
public import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

let messagingTimeout: Duration = .seconds(1)
let modalLayers: Range<Int> = Int(CGWindowLevelForKey(.modalPanelWindow))..<Int(CGWindowLevelForKey(.utilityWindow))
let frameAttribute: String = "AXFrame"
let userInputLabelsAttribute: String = "AXUserInputLabels"
let fullScreenAttribute: String = "AXFullScreen"
let sectionsAttribute: String = "AXSections"
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
let menuAttributes: [String] = [
    kAXRoleAttribute, kAXSubroleAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXHelpAttribute, kAXIdentifierAttribute, kAXEnabledAttribute, kAXSelectedAttribute,
    kAXMenuItemCmdCharAttribute, kAXMenuItemCmdVirtualKeyAttribute, kAXMenuItemCmdGlyphAttribute, kAXMenuItemCmdModifiersAttribute, kAXMenuItemMarkCharAttribute, kAXURLAttribute,
    kAXChildrenAttribute, kAXParentAttribute, frameAttribute,
]

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
@dynamicMemberLookup
public enum Fact: CaseTagged, Hashable, Sendable {
    public enum Scalar: Codable, Hashable, Sendable {
        case bool(Bool)
        case number(Double)
        case text(String)
        case date(Date)

        public init(from decoder: any Decoder) throws {
            let value = try decoder.singleValueContainer()
            self = try value.decodeIfMatching(Bool.self).map(Self.bool) ?? value.decodeIfMatching(Double.self).map(Self.number) ?? .text(value.decode(String.self))
        }

        public func encode(to encoder: any Encoder) throws {
            var value = encoder.singleValueContainer()
            switch self {
                case .bool(let flag): try value.encode(flag)
                case .number(let number): try value.encode(number)
                case .text(let text): try value.encode(text)
                case .date(let date): try value.encode(date)
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
            case .point, .size, .rect, .range, .element, .list, .record, .refused: nil
        }
    }
}

public struct Element: Hashable, Sendable {
    public let id: Int
    public let facts: [String: Fact]
    public let actions: [String]
    public let window: CGWindowID?

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
            case .bool, .date: nil
        }
    }

    var text: String? {
        switch self {
            case .text(let text): text
            case .number(let number): number.formatted(FloatingPointFormatStyle<Double>(locale: Locale(identifier: "en_US_POSIX")).grouping(.never).precision(.significantDigits(1...17)))
            case .bool, .date: nil
        }
    }

    var accessibility: CFTypeRef {
        switch self {
            case .bool(let flag): flag as NSNumber
            case .number(let number): number as NSNumber
            case .text(let text): text as CFString
            case .date(let date): date as CFDate
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
        var number: CGWindowID = 0
        return unsafe windowNumber(self, &number) == .success ? number : nil
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
        let read = Element(id: id, facts: facts, actions: names as? [String] ?? [], window: window)
        let wanted: [String] = answered.contains(acted) ? read.supplementary.filter { attribute in !attributes.contains(attribute) } : []
        let (extended, added) = wanted.isEmpty ? (AXError.success, [:]) : copy(wanted, interning: intern)
        return (
            [acted, extended].first { error in !answered.contains(error) } ?? answer,
            Element(id: id, facts: facts.merging(added) { first, _ in first }, actions: read.actions, window: read.window),
        )
    }

    func read(_ attributes: [String]) throws(ApplicationFailure) -> (element: Element, handles: [AXUIElement]) {
        var handles: [AXUIElement] = [self]
        let (answer, read) = element(0, reading: attributes) { handle in
            handles.append(handle)
            return handles.count - 1
        }
        guard [.success, .invalidUIElement].contains(answer) else { throw failure(answer, reading: attributes.joined(separator: ", ")) }
        return (read, handles)
    }

    func elements(_ attribute: String) throws(ApplicationFailure) -> [AXUIElement] {
        var value: CFTypeRef?
        return switch unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) {
            case .success, .noValue, .attributeUnsupported, .invalidUIElement: ((value as? [AnyObject]) ?? value.map { slot in [slot] } ?? []).compactMap(Self.cast)
            case let error: throw failure(error, reading: attribute)
        }
    }

    func search(_ text: String, limit: Int) throws(ApplicationFailure) -> [AXUIElement] {
        let attribute: String = NSAccessibility.ParameterizedAttribute.uiElementsForSearchPredicateParameterizedAttribute.rawValue
        let predicate: [String: Any] = [
            NSAccessibilitySearchIdentifiersKey: NSAccessibilitySearchKey.anyTypeSearchKey.rawValue, NSAccessibilitySearchTextKey: text, NSAccessibilitySearchResultsLimitKey: limit,
            NSAccessibilitySearchDirectionKey: NSAccessibilitySearchDirectionNext,
        ]
        var found: CFTypeRef?
        let answer = unsafe AXUIElementCopyParameterizedAttributeValue(self, attribute as CFString, predicate as CFDictionary, &found)
        guard answer == .success else { throw failure(answer, reading: attribute) }
        return (found as? [AnyObject] ?? []).compactMap(Self.cast)
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

@_silgen_name("_AXUIElementGetWindow")
private func windowNumber(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError
