import ApplicationServices
public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Codable, Hashable, Sendable, CustomStringConvertible {
    case perform(element: Data, action: String)
    case set(element: Data, value: String)

    static let activating: Set<String> = [kAXRaiseAction]

    public var element: Data {
        switch self {
            case .perform(let element, _), .set(let element, _): element
        }
    }

    public var description: String {
        switch self {
            case .perform(let element, let action): "\(action) on \(element.base64EncodedString())"
            case .set(let element, let value): "set \(kAXValueAttribute) \"\(value)\" on \(element.base64EncodedString())"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ElementAction {
    @concurrent
    public func perform(in process: pid_t? = nil) async throws(ApplicationFailure) {
        try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) in
            let absent: ApplicationFailure = ApplicationFailure(subject: "Application", reason: .absent("open element \(element.base64EncodedString())"))
            guard let node: AXUIElement = AXUIElement.element(element), process == nil || node.process == process else { throw absent }
            let read: [String: CFTypeRef] = try node.values([kAXRoleAttribute, kAXWindowAttribute, kAXEnabledAttribute])
            let window: AXUIElement? = read[kAXRoleAttribute] as? String == kAXWindowRole ? node : read[kAXWindowAttribute].flatMap(AXUIElement.checked)
            let open: Bool? = try window.map { window throws(ApplicationFailure) in
                try node.process.map(AXUIElementCreateApplication)?.elements(kAXWindowsAttribute).contains(window) ?? false
            }
            guard open != false else { throw absent }
            let outcome: AXError =
                switch (self, read[kAXEnabledAttribute] as? Bool) {
                    case (.perform(_, let action), _) where Self.activating.contains(action): throw ApplicationFailure(subject: description, reason: .activating)
                    case (.perform, false): throw ApplicationFailure(subject: description, reason: .disabled)
                    case (.perform(_, let action), _): AXUIElementPerformAction(node, action as CFString)
                    case (.set(_, let value), _): try node.write(value as CFString, to: kAXValueAttribute)
                }
            guard outcome != .success else { return }
            throw ApplicationFailure(outcome, subject: description, process: node.process, answering: self) {
                outcome == .invalidUIElement ? absent : ApplicationFailure(subject: description, reason: .refused(outcome))
            }
        }
    }
}
