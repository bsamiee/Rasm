@unsafe @preconcurrency import ApplicationServices
import Encoding
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let labelAttributes: [String] = [kAXTitleAttribute, kAXDescriptionAttribute]
private let menuMark: Data = .init("menu".utf8)
let menuBars: [String] = [kAXMenuBarAttribute, kAXExtrasMenuBarAttribute]
let frameAttributes: [String] = [kAXPositionAttribute, kAXSizeAttribute]
let messagingTimeout: Duration = .seconds(1)

// --- [SERVICES] ------------------------------------------------------------------------

let accessibility: DispatchQueue = .init(label: "Applications.accessibility", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

public func checkAccessibilityTrust(prompting: Bool = false) throws(ApplicationFailure) {
    guard unsafe AXIsProcessTrustedWithOptions([kAXTrustedCheckOptionPrompt.takeUnretainedValue(): prompting] as CFDictionary) else {
        throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
    }
}

private func ownLabel(_ read: [String: CFTypeRef]) -> String? {
    labelAttributes.lazy.compactMap { attribute in read[attribute] as? String }.first { label in !label.isEmpty }
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

    static func element(_ id: Data) throws(ApplicationFailure) -> AXUIElement? {
        guard id.starts(with: menuMark), id.count > menuMark.count + 4, (id.count - menuMark.count).isMultiple(of: 4) else {
            return unsafe remoteElement(id as CFData)?.takeRetainedValue().timed
        }
        let words = stride(from: id.startIndex + menuMark.count, to: id.endIndex, by: 4).map { start in id[start..<(start + 4)].reduce(0) { word, byte in word << 8 | Int(byte) } }
        guard menuBars.indices.contains(words[1]) else { return nil }
        let bar = menuBars[words[1]]
        var node = try application(pid_t(truncatingIfNeeded: words[0])).values([bar])[bar].flatMap(checked)
        for index in words.dropFirst(2) {
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
            let read = Dictionary(uniqueKeysWithValues: copied([kAXRoleAttribute] + labelAttributes).slots)
            return ApplicationFailure(subject: subject, reason: .unreadable(FailedRead(reads: reads, role: read[kAXRoleAttribute] as? String, label: ownLabel(read), error: error)))
        }
    }

    func values(_ attributes: [String]) throws(ApplicationFailure) -> [String: CFTypeRef] {
        let (answer, slots) = copied(attributes)
        let refused = slots.lazy.compactMap { attribute, slot in
            AXValue.unpacked(slot, .axError, AXError.success).flatMap { error in
                [.noValue, .attributeUnsupported, .invalidUIElement].contains(error) ? nil : (attribute: attribute, error: error)
            }
        }
        .first
        return switch (answer, refused) {
            case (.success, .some(let refused)): throw failure(refused.error, reading: [refused.attribute])
            case (.success, .none): Dictionary(uniqueKeysWithValues: slots.filter { _, slot in AXValue.unpacked(slot, .axError, AXError.success) == nil && !(slot is NSNull) })
            case (.invalidUIElement, _): [:]
            case (let error, _): throw failure(error, reading: attributes)
        }
    }

    func value<Value>(_ attribute: String) throws(ApplicationFailure) -> Value? {
        try values([attribute])[attribute] as? Value
    }

    func elements(_ attribute: String) throws(ApplicationFailure) -> [AXUIElement] {
        (try values([attribute])[attribute] as? [CFTypeRef] ?? []).compactMap(Self.checked)
    }

    static func window(framed read: [String: CFTypeRef], among listed: [InstanceState.Window]) -> CGWindowID? {
        guard let origin = AXValue.unpacked(read[kAXPositionAttribute], .cgPoint, CGPoint.zero),
            let size = AXValue.unpacked(read[kAXSizeAttribute], .cgSize, CGSize.zero)
        else { return nil }
        let framed = listed.filter { row in row.frame == CGRect(origin: origin, size: size) }
        let shown = framed.filter(\.onScreen)
        return switch (framed.count, shown.count) {
            case (1, _): framed.first?.number
            case (_, 1): shown.first?.number
            case (_, _): nil
        }
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
        guard let range = try values([kAXVisibleCharacterRangeAttribute])[kAXVisibleCharacterRangeAttribute] else { return nil }
        var text: CFTypeRef?
        return switch unsafe AXUIElementCopyParameterizedAttributeValue(self, kAXStringForRangeParameterizedAttribute as CFString, range, &text) {
            case .success: text as? String
            case .noValue, .attributeUnsupported, .parameterizedAttributeUnsupported, .invalidUIElement: nil
            case let error: throw failure(error, reading: [kAXStringForRangeParameterizedAttribute])
        }
    }

    func label(_ read: [String: CFTypeRef]) throws(ApplicationFailure) -> String? {
        switch (ownLabel(read), read[kAXTitleUIElementAttribute].flatMap(Self.checked)) {
            case (.some(let label), _): label
            case (.none, .some(let title)): try title.value(kAXValueAttribute)
            case (.none, .none): nil
        }
    }

    // --- [WRITES]

    func write(_ value: CFTypeRef, to attribute: String) throws(ApplicationFailure) -> AXError {
        var settable: DarwinBoolean = false
        return switch unsafe AXUIElementIsAttributeSettable(self, attribute as CFString, &settable) {
            case .success where settable.boolValue: AXUIElementSetAttributeValue(self, attribute as CFString, value)
            case .success, .noValue, .attributeUnsupported: throw ApplicationFailure(subject: token.base64EncodedString(), reason: .notSettable(attribute))
            case let error: error
        }
    }
}

@_silgen_name("_AXUIElementRemoteTokenCreate")
private func remoteToken(_: AXUIElement) -> Unmanaged<CFData>

@_silgen_name("_AXUIElementCreateWithRemoteToken")
private func remoteElement(_: CFData) -> Unmanaged<AXUIElement>?
