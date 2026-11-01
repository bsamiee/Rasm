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
            let named: String = "element \(element.base64EncodedString())"
            guard let node: AXUIElement = AXUIElement.element(element), process == nil || node.process == process else {
                throw ApplicationFailure(subject: named, reason: .absent)
            }
            func written(_ value: String) throws(ApplicationFailure) -> AXError {
                var settable: DarwinBoolean = false
                let checked: AXError = unsafe AXUIElementIsAttributeSettable(node, kAXValueAttribute as CFString, &settable)
                return switch (checked, settable.boolValue) {
                    case (.success, true): AXUIElementSetAttributeValue(node, kAXValueAttribute as CFString, value as CFString)
                    case (.success, false): throw ApplicationFailure(subject: named, reason: .unsettable)
                    case (let error, _): throw ApplicationFailure(subject: description, reason: .refused(error))
                }
            }
            let outcome: AXError =
                switch self {
                    case .perform(_, let action) where Self.activating.contains(action): throw ApplicationFailure(subject: description, reason: .activating)
                    case .perform(_, let action): AXUIElementPerformAction(node, action as CFString)
                    case .set(_, let value): try written(value)
                }
            switch (outcome, node.process) {
                case (.success, _): return
                case (.invalidUIElement, _): throw ApplicationFailure(subject: named, reason: .absent)
                case (.cannotComplete, .some(let pid)) where vanished(pid): throw ApplicationFailure(subject: String(pid), reason: .noProcess)
                case (.cannotComplete, _): throw ApplicationFailure(subject: description, reason: .unanswered(self))
                case (let error, _): throw ApplicationFailure(subject: description, reason: .refused(error))
            }
        }
    }
}
