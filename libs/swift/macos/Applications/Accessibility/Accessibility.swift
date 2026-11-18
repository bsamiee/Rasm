@unsafe @preconcurrency import ApplicationServices
import Encoding
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let labelAttributes: [String] = [kAXTitleAttribute, kAXDescriptionAttribute]
private let menuMark: Data = .init("menu".utf8)
let menuBars: [String] = [kAXMenuBarAttribute, kAXExtrasMenuBarAttribute]
let frameAttributes: [String] = [kAXPositionAttribute, kAXSizeAttribute]
let messagingTimeout: Duration = .seconds(1)

// --- [MODELS] --------------------------------------------------------------------------

struct Attributes {
    let values: [String: CFTypeRef]
    let refused: [(attribute: String, error: AXError)]

    init(_ slots: [(attribute: String, slot: CFTypeRef)]) {
        let read = slots.map { attribute, slot in (attribute: attribute, slot: slot, error: AXValue.unpacked(slot, .axError, AXError.success)) }
        values = Dictionary(uniqueKeysWithValues: read.filter { slot in slot.error == nil && !(slot.slot is NSNull) }.map { slot in (slot.attribute, slot.slot) })
        refused = read.compactMap { slot in
            slot.error.flatMap { error in [.noValue, .attributeUnsupported, .invalidUIElement].contains(error) ? nil : (attribute: slot.attribute, error: error) }
        }
    }

    var label: String? { labelAttributes.lazy.compactMap { attribute in values[attribute] as? String }.first { label in !label.isEmpty } }

    var failed: [FailedRead] {
        refused.map { refusal in FailedRead(reads: [refusal.attribute], role: values[kAXRoleAttribute] as? String, label: label, error: refusal.error) }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

let accessibility: DispatchQueue = .init(label: "Applications.accessibility", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

public func checkAccessibilityTrust(prompting: Bool = false) throws(ApplicationFailure) {
    guard unsafe AXIsProcessTrustedWithOptions([kAXTrustedCheckOptionPrompt.takeUnretainedValue(): prompting] as CFDictionary) else {
        throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
    }
}

extension JSONValue {
    init?(accessibility value: CFTypeRef) {
        switch value {
            case let flag as CFBoolean where CFGetTypeID(value) == CFBooleanGetTypeID(): self = .bool(CFBooleanGetValue(flag))
            case let number as NSNumber: self = .number(number.doubleValue)
            case let text as String: self = .string(text)
            default: return nil
        }
    }

    var number: Double? {
        switch self {
            case .number(let number): number
            case .string(let text): Double(text.trimmingCharacters(in: .whitespaces))
            case .null, .bool, .array, .object: nil
        }
    }

    var text: String? {
        switch self {
            case .string(let text): text
            case .number(let number): number.formatted(FloatingPointFormatStyle<Double>(locale: Locale(identifier: "en_US_POSIX")).grouping(.never).precision(.significantDigits(1...17)))
            case .null, .bool, .array, .object: nil
        }
    }

    var accessibility: CFTypeRef {
        get throws(ApplicationFailure) {
            switch self {
                case .bool(let flag): NSNumber(value: flag)
                case .number(let number): number as NSNumber
                case .string(let text): text as CFString
                case .null, .array, .object: throw ApplicationFailure(subject: "\(self)", reason: .malformed("\(self) is not a string, number, or boolean"))
            }
        }
    }
}

extension AXValue {
    static func unpacked<Value: BitwiseCopyable>(_ slot: CFTypeRef?, _ type: AXValueType, _ empty: Value) -> Value? {
        guard let slot: CFTypeRef, CFGetTypeID(slot) == AXValueGetTypeID() else { return nil }
        let value = unsafe unsafeDowncast(slot, to: AXValue.self)
        var unpacked = empty
        return unsafe AXValueGetType(value) == type && AXValueGetValue(value, type, &unpacked) ? unpacked : nil
    }
}

extension AXUIElement {
    // --- [IDENTITY]

    var token: Data { unsafe remoteToken(self).takeRetainedValue() as Data }

    static func menuItem(of process: pid_t, at path: [Int]) -> Data {
        ([UInt32(bitPattern: process)] + path.map { index in UInt32(truncatingIfNeeded: index) }).reduce(into: menuMark) { id, word in
            id.append(contentsOf: stride(from: 24, through: 0, by: -8).map { shift in UInt8(truncatingIfNeeded: word >> shift) })
        }
    }

    static func menuPath(_ id: Data) -> (process: pid_t, path: [Int])? {
        guard id.starts(with: menuMark), id.count > menuMark.count + 4, (id.count - menuMark.count).isMultiple(of: 4) else { return nil }
        let words = stride(from: id.startIndex + menuMark.count, to: id.endIndex, by: 4).map { start in id[start..<(start + 4)].reduce(0) { word, byte in word << 8 | Int(byte) } }
        return (pid_t(truncatingIfNeeded: words[0]), Array(words.dropFirst()))
    }

    static func element(_ id: Data) throws(ApplicationFailure) -> AXUIElement? {
        guard let menu = menuPath(id) else { return unsafe remoteElement(id as CFData)?.takeRetainedValue().timed }
        guard let bar = menu.path.first, menuBars.indices.contains(bar) else { return nil }
        var node = try (application(menu.process).value(menuBars[bar]) as CFTypeRef?).flatMap(checked)
        for index in menu.path.dropFirst() {
            node = try node.flatMap { parent throws(ApplicationFailure) in try parent.elements(kAXChildrenAttribute).dropFirst(index).first }
        }
        return node
    }

    static func application(_ process: pid_t) -> AXUIElement { AXUIElementCreateApplication(process).timed }

    private var timed: AXUIElement {
        AXUIElementSetMessagingTimeout(self, Float(messagingTimeout / .seconds(1)))
        return self
    }

    var process: pid_t? {
        var pid: pid_t = 0
        return unsafe AXUIElementGetPid(self, &pid) == .success ? pid : nil
    }

    static func checked(_ slot: CFTypeRef) -> AXUIElement? {
        CFGetTypeID(slot) == AXUIElementGetTypeID() ? unsafe unsafeDowncast(slot, to: AXUIElement.self).timed : nil
    }

    // --- [READS]

    private func copied(_ attributes: [String]) -> (answer: AXError, slots: [(attribute: String, slot: CFTypeRef)]) {
        var values: CFArray?
        let answer = unsafe AXUIElementCopyMultipleAttributeValues(self, attributes as CFArray, [], &values)
        return (answer, Array(zip(attributes, values as? [CFTypeRef] ?? [])))
    }

    func failure(_ error: AXError, reading reads: [String]) -> ApplicationFailure {
        let subject = token.base64EncodedString()
        return ApplicationFailure(error, subject: subject, process: process) {
            let read = Attributes(copied([kAXRoleAttribute] + labelAttributes).slots)
            return ApplicationFailure(subject: subject, reason: .unreadable(FailedRead(reads: reads, role: read.values[kAXRoleAttribute] as? String, label: read.label, error: error)))
        }
    }

    func values(_ attributes: [String]) throws(ApplicationFailure) -> Attributes {
        let (answer, slots) = copied(attributes)
        let read = Attributes(slots)
        return switch (answer, read.refused.first { refusal in [.cannotComplete, .apiDisabled].contains(refusal.error) }) {
            case (.success, .some(let silent)): throw failure(silent.error, reading: [silent.attribute])
            case (.success, .none): read
            case (.invalidUIElement, _): Attributes([])
            case (let error, _): throw failure(error, reading: attributes)
        }
    }

    func required(_ attributes: [String]) throws(ApplicationFailure) -> Attributes {
        let read = try values(attributes)
        if let refusal = read.refused.first { throw failure(refusal.error, reading: [refusal.attribute]) }
        return read
    }

    func value<Value>(_ attribute: String) throws(ApplicationFailure) -> Value? {
        try required([attribute]).values[attribute] as? Value
    }

    func elements(_ attribute: String) throws(ApplicationFailure) -> [AXUIElement] {
        (try value(attribute) as [CFTypeRef]? ?? []).compactMap(Self.checked)
    }

    var window: CGWindowID? {
        var number: CGWindowID = 0
        return unsafe windowNumber(self, &number) == .success ? number : nil
    }

    func actionNames() throws(ApplicationFailure) -> [String] {
        var names: CFArray?
        return switch unsafe AXUIElementCopyActionNames(self, &names) {
            case .success: names as? [String] ?? []
            case .failure, .invalidUIElement: []
            case let error: throw failure(error, reading: ["AXUIElementCopyActionNames"])
        }
    }

    func visibleText() throws(ApplicationFailure) -> String? {
        guard let range: CFTypeRef = try value(kAXVisibleCharacterRangeAttribute) else { return nil }
        var text: CFTypeRef?
        return switch unsafe AXUIElementCopyParameterizedAttributeValue(self, kAXStringForRangeParameterizedAttribute as CFString, range, &text) {
            case .success: text as? String
            case .noValue, .attributeUnsupported, .parameterizedAttributeUnsupported, .invalidUIElement: nil
            case let error: throw failure(error, reading: [kAXStringForRangeParameterizedAttribute])
        }
    }

    func label(_ read: Attributes) throws(ApplicationFailure) -> String? {
        switch (read.label, read.values[kAXTitleUIElementAttribute].flatMap(Self.checked)) {
            case (.some(let label), _): label
            case (.none, .some(let title)): try title.value(kAXValueAttribute)
            case (.none, .none): nil
        }
    }

    func settable(_ attribute: String) throws(ApplicationFailure) -> Bool {
        var settable: DarwinBoolean = false
        return switch unsafe AXUIElementIsAttributeSettable(self, attribute as CFString, &settable) {
            case .success: settable.boolValue
            case .noValue, .attributeUnsupported, .invalidUIElement: false
            case let error: throw failure(error, reading: ["AXUIElementIsAttributeSettable"])
        }
    }
}

@_silgen_name("_AXUIElementRemoteTokenCreate")
private func remoteToken(_: AXUIElement) -> Unmanaged<CFData>

@_silgen_name("_AXUIElementCreateWithRemoteToken")
private func remoteElement(_: CFData) -> Unmanaged<AXUIElement>?

@_silgen_name("_AXUIElementGetWindow")
private func windowNumber(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError
