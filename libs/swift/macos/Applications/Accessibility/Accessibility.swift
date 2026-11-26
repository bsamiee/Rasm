@unsafe @preconcurrency public import ApplicationServices
public import CasePaths
import CasePathsCore
public import Encoding
public import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

let messagingTimeout: Duration = .seconds(1)
let frameAttributes: [String] = [kAXPositionAttribute, kAXSizeAttribute]
let elementAttributes: [String] =
    [
        kAXRoleAttribute, kAXSubroleAttribute, kAXRoleDescriptionAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXHelpAttribute, kAXIdentifierAttribute,
        kAXDOMIdentifierAttribute, kAXValueAttribute, kAXEnabledAttribute, kAXFocusedAttribute, kAXSelectedAttribute, kAXParentAttribute, kAXChildrenAttribute, kAXVisibleRowsAttribute,
        kAXVisibleChildrenAttribute,
        kAXTitleUIElementAttribute, kAXPlaceholderValueAttribute, kAXMinValueAttribute, kAXMaxValueAttribute, kAXValueIncrementAttribute, kAXAllowedValuesAttribute,
        kAXValueDescriptionAttribute, kAXExpandedAttribute, kAXTabsAttribute, kAXSelectedChildrenAttribute, kAXSelectedRowsAttribute, kAXDisclosingAttribute,
        kAXDisclosureLevelAttribute, kAXIndexAttribute,
    ] + frameAttributes
let windowAttributes: [String] =
    elementAttributes + [
        kAXModalAttribute, kAXMainAttribute, kAXDocumentAttribute, kAXMinimizedAttribute, kAXDefaultButtonAttribute, kAXCancelButtonAttribute, kAXCloseButtonAttribute,
    ]
let menuAttributes: [String] =
    [
        kAXRoleAttribute, kAXSubroleAttribute, kAXTitleAttribute, kAXDescriptionAttribute, kAXHelpAttribute, kAXIdentifierAttribute, kAXEnabledAttribute, kAXSelectedAttribute,
        kAXMenuItemCmdCharAttribute, kAXMenuItemCmdVirtualKeyAttribute, kAXMenuItemCmdGlyphAttribute, kAXMenuItemCmdModifiersAttribute, kAXMenuItemMarkCharAttribute, kAXChildrenAttribute,
        kAXParentAttribute,
    ] + frameAttributes
private let absent: Set<AXError> = [.noValue, .attributeUnsupported]

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
@dynamicMemberLookup
public enum Fact: Codable, Hashable, Sendable {
    public enum Scalar: Codable, Hashable, Sendable {
        case bool(Bool)
        case number(Double)
        case text(String)
        case date(Date)

        public init(from decoder: any Decoder) throws {
            self =
                switch try JSONValue(from: decoder) {
                    case .bool(let flag): .bool(flag)
                    case .number(let number): .number(number)
                    case .string(let text): .text(text)
                    case .null, .array, .object:
                        throw DecodingError.typeMismatch(
                            Self.self,
                            DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "\(kAXValueAttribute) takes a boolean, number, or string"),
                        )
                }
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
            case .point, .size, .rect, .range, .element, .list, .refused: nil
        }
    }
}

public struct Element: Codable, Hashable, Sendable {
    public let id: Int
    public let facts: [String: Fact]
    public let actions: [String]
    public let window: CGWindowID?

    public func text(_ attribute: String) -> String? { facts[attribute]?.text.flatMap { text in text.isEmpty ? nil : text } }

    public var role: String? { text(kAXRoleAttribute) }

    public var frame: CGRect? {
        switch (facts[kAXPositionAttribute], facts[kAXSizeAttribute]) {
            case (.point(let origin), .size(let size)): CGRect(origin: origin, size: size)
            default: nil
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Fact {
    init?(_ value: CFTypeRef, interning intern: (AXUIElement) -> Int) {
        switch CFGetTypeID(value) {
            case CFBooleanGetTypeID(): self = .bool(value: CFBooleanGetValue(unsafe unsafeDowncast(value, to: CFBoolean.self)))
            case CFNumberGetTypeID():
                let number = unsafe unsafeDowncast(value, to: CFNumber.self)
                self = CFNumberIsFloatType(number) ? .real(value: (number as NSNumber).doubleValue) : .integer(value: (number as NSNumber).intValue)
            case CFStringGetTypeID(): self = .text(value: unsafe unsafeDowncast(value, to: CFString.self) as String)
            case CFAttributedStringGetTypeID(): self = .text(value: CFAttributedStringGetString(unsafe unsafeDowncast(value, to: CFAttributedString.self)) as String)
            case CFDateGetTypeID(): self = .date(value: unsafe unsafeDowncast(value, to: CFDate.self) as Date)
            case CFURLGetTypeID(): self = .url(value: unsafe unsafeDowncast(value, to: CFURL.self) as URL)
            case AXUIElementGetTypeID(): self = .element(id: intern(unsafe unsafeDowncast(value, to: AXUIElement.self)))
            case CFArrayGetTypeID():
                guard let items = value as? [AnyObject] else { return nil }
                self = .list(values: items.compactMap { item in Fact(item, interning: intern) })
            case AXValueGetTypeID():
                let boxed = unsafe unsafeDowncast(value, to: AXValue.self)
                let fact: Fact? =
                    switch AXValueGetType(boxed) {
                        case .cgPoint: boxed.unpacked(CGPoint.zero).map { point in .point(value: point) }
                        case .cgSize: boxed.unpacked(CGSize.zero).map { size in .size(value: size) }
                        case .cgRect: boxed.unpacked(CGRect.zero).map { rect in .rect(value: rect) }
                        case .cfRange: boxed.unpacked(CFRange()).map { range in .range(location: range.location, length: range.length) }
                        case .axError: boxed.unpacked(AXError.success).flatMap { error in absent.contains(error) ? nil : .refused(error: Lenient(rawValue: error.rawValue)) }
                        case .illegal: nil
                        @unknown default: nil
                    }
                guard let fact else { return nil }
                self = fact
            default: return nil
        }
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
            case .bool(let flag): NSNumber(value: flag)
            case .number(let number): number as NSNumber
            case .text(let text): text as CFString
            case .date(let date): date as CFDate
        }
    }
}

extension AXValue {
    fileprivate func unpacked<Value: BitwiseCopyable>(_ empty: Value) -> Value? {
        var unpacked = empty
        return unsafe AXValueGetValue(self, AXValueGetType(self), &unpacked) ? unpacked : nil
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

    func element(_ id: Int, reading attributes: [String], interning intern: (AXUIElement) -> Int) -> (answer: AXError, element: Element) {
        var values: CFArray?
        var names: CFArray?
        let answer = unsafe AXUIElementCopyMultipleAttributeValues(self, attributes as CFArray, [], &values)
        let acted = answer == .success ? unsafe AXUIElementCopyActionNames(self, &names) : answer
        let facts = zip(attributes, values as? [AnyObject] ?? []).compactMap { attribute, slot in Fact(slot, interning: intern).map { fact in (attribute, fact) } }
        return (
            [AXError.success, .failure, .attributeUnsupported, .actionUnsupported].contains(acted) ? answer : acted,
            Element(id: id, facts: Dictionary(uniqueKeysWithValues: facts), actions: names as? [String] ?? [], window: window),
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
        let (element, handles) = try read([attribute])
        return (element.facts[attribute]?.elements ?? []).map { id in handles[id] }
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
            let named = element(0, reading: [kAXRoleAttribute, kAXTitleAttribute, kAXDescriptionAttribute]) { _ in 0 }.element
            let role: String = named.role ?? kAXUnknownRole
            let subject: String = (named.text(kAXTitleAttribute) ?? named.text(kAXDescriptionAttribute)).map { label in "\(role) \(String(reflecting: label))" } ?? role
            return ApplicationFailure(subject: subject, reason: .unreadable(attribute: attribute, error: error))
        }
    }
}

@_silgen_name("_AXUIElementGetWindow")
private func windowNumber(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError
