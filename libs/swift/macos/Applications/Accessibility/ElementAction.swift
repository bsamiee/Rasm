import ApplicationServices
public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Codable, Hashable, Sendable, CustomStringConvertible {
    case perform(element: Data, action: String)
    case set(element: Data, value: String)

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
    @discardableResult
    public func perform(in process: pid_t? = nil) async throws(ApplicationFailure) -> pid_t {
        try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) -> pid_t in
            let subject: String = "element \(element.base64EncodedString())"
            guard let node: AXUIElement = AXUIElement.element(element), let owner: pid_t = node.process else { throw ApplicationFailure(subject: subject, reason: .unresolved) }
            if let process, owner != process { throw ApplicationFailure(subject: subject, reason: .foreign(process)) }
            let read: [String: CFTypeRef] = try node.values([kAXRoleAttribute, kAXWindowAttribute, kAXEnabledAttribute])
            let window: AXUIElement? = read[kAXRoleAttribute] as? String == kAXWindowRole ? node : read[kAXWindowAttribute].flatMap(AXUIElement.checked)
            let open: Bool? = try window.map { window throws(ApplicationFailure) in
                try AXUIElementCreateApplication(owner).elements(kAXWindowsAttribute).contains(window)
            }
            guard open != false else { throw ApplicationFailure(subject: subject, reason: .closed) }
            let outcome: AXError =
                switch (self, read[kAXEnabledAttribute] as? Bool) {
                    case (.perform(_, kAXRaiseAction), _): throw ApplicationFailure(subject: description, reason: .activating)
                    case (.perform, false): throw ApplicationFailure(subject: description, reason: .disabled)
                    case (.perform(_, let action), _): AXUIElementPerformAction(node, action as CFString)
                    case (.set(_, let value), _): try node.write(value as CFString, to: kAXValueAttribute)
                }
            guard outcome != .success else { return owner }
            throw ApplicationFailure(outcome, subject: description, process: owner, answering: self) {
                outcome == .invalidUIElement ? ApplicationFailure(subject: subject, reason: .unresolved) : ApplicationFailure(subject: description, reason: .refused(outcome))
            }
        }
    }
}
