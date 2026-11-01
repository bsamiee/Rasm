@unsafe @preconcurrency import ApplicationServices
import Encoding
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private let labelAttributes: [String] = [kAXTitleAttribute, kAXDescriptionAttribute]

// --- [SERVICES] ------------------------------------------------------------------------

let accessibility: DispatchQueue = DispatchQueue(label: "Applications.accessibility", attributes: .concurrent)

// --- [OPERATIONS] ----------------------------------------------------------------------

public func checkAccessibilityTrust(prompting: Bool = false) throws(ApplicationFailure) {
    guard unsafe AXIsProcessTrustedWithOptions([kAXTrustedCheckOptionPrompt.takeUnretainedValue(): prompting] as CFDictionary) else {
        throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
    }
}

func vanished(_ process: pid_t) -> Bool {
    var info: proc_bsdinfo = proc_bsdinfo()
    return unsafe proc_pidinfo(process, PROC_PIDTBSDINFO, 0, &info, Int32(MemoryLayout<proc_bsdinfo>.size)) <= 0 && errno == ESRCH
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

    static func checked(_ slot: CFTypeRef) -> AXUIElement? {
        CFGetTypeID(slot) == AXUIElementGetTypeID() ? unsafe unsafeDowncast(slot, to: AXUIElement.self) : nil
    }

    // --- [READS]
    func failure(_ error: AXError, reading reads: [String], among read: [String: CFTypeRef] = [:]) -> ApplicationFailure {
        switch error {
            case .apiDisabled: ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .untrusted)
            case .cannotComplete: ApplicationFailure(subject: "element \(token.base64EncodedString())", reason: .unanswered(nil))
            default:
                ApplicationFailure(
                    subject: "element \(token.base64EncodedString())",
                    reason: .unreadable(FailedRead(reads: reads, role: read[kAXRoleAttribute] as? String, label: ownLabel(read), error: error.rawValue)),
                )
        }
    }

    func values(_ attributes: [String]) throws(ApplicationFailure) -> [String: CFTypeRef] {
        let named: [String] = attributes + ([kAXRoleAttribute] + labelAttributes).filter { attribute in !attributes.contains(attribute) }
        var values: CFArray?
        let answer: AXError = unsafe AXUIElementCopyMultipleAttributeValues(self, named as CFArray, [], &values)
        let slots: Zip2Sequence<[String], [CFTypeRef]> = zip(named, values as? [CFTypeRef] ?? [])
        let read: [String: CFTypeRef] = Dictionary(uniqueKeysWithValues: slots.filter { _, slot in AXError(slot) == nil && !(slot is NSNull) })
        let refused: (attribute: String, error: AXError)? = slots.lazy.compactMap { attribute, slot in
            AXError(slot).flatMap { error in
                attributes.contains(attribute) && ![.noValue, .attributeUnsupported, .invalidUIElement].contains(error) ? (attribute: attribute, error: error) : nil
            }
        }
        .first
        return switch (answer, refused) {
            case (.success, .some(let refused)): throw failure(refused.error, reading: [refused.attribute], among: read)
            case (.success, .none): read
            case (.invalidUIElement, _): [:]
            case (let error, _): throw failure(error, reading: attributes)
        }
    }

    func value<Value>(_ attribute: String) throws(ApplicationFailure) -> Value? {
        try values([attribute])[attribute] as? Value
    }

    func element(_ attribute: String) throws(ApplicationFailure) -> AXUIElement? {
        try values([attribute])[attribute].flatMap(Self.checked)
    }

    func elements(_ attribute: String) throws(ApplicationFailure) -> [AXUIElement] {
        (try values([attribute])[attribute] as? [CFTypeRef] ?? []).compactMap(Self.checked)
    }

    func window() throws(ApplicationFailure) -> CGWindowID? {
        var id: CGWindowID = 0
        return switch unsafe windowIdentifier(self, &id) {
            case .success: id
            case let error where error == .cannotComplete || error == .apiDisabled: throw failure(error, reading: ["_AXUIElementGetWindow"])
            default: nil
        }
    }

    func actionNames() throws(ApplicationFailure) -> [String] {
        var names: CFArray?
        return switch unsafe AXUIElementCopyActionNames(self, &names) {
            case .success: (names as? [String] ?? []).filter { action in !ElementAction.activating.contains(action) }
            case .failure, .invalidUIElement: []
            case let error: throw failure(error, reading: ["AXUIElementCopyActionNames"])
        }
    }

    func children(where kept: ((String?) -> Bool)? = nil) throws(ApplicationFailure) -> [AXUIElement] {
        let children: [AXUIElement] = try elements(kAXChildrenAttribute)
        return switch kept {
            case .some(let kept): try children.map { child throws(ApplicationFailure) in try kept(child.value(kAXRoleAttribute)) ? child : nil }.compactMap(\.self)
            case .none: children
        }
    }

    func label(_ read: [String: CFTypeRef]) throws(ApplicationFailure) -> String? {
        switch ownLabel(read) {
            case .some(let label): label
            case .none: try element(kAXTitleUIElementAttribute)?.value(kAXValueAttribute)
        }
    }
}

@_silgen_name("_AXUIElementGetWindow")
private func windowIdentifier(_: AXUIElement, _: UnsafeMutablePointer<CGWindowID>) -> AXError

@_silgen_name("_AXUIElementRemoteTokenCreate")
private func remoteToken(_: AXUIElement) -> Unmanaged<CFData>

@_silgen_name("_AXUIElementCreateWithRemoteToken")
private func remoteElement(_: CFData) -> Unmanaged<AXUIElement>?
