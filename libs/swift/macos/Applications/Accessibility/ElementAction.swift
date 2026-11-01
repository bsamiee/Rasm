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
            func written(_ value: String) throws(ApplicationFailure) -> AXError {
                var settable: DarwinBoolean = false
                let unsettable: ApplicationFailure = ApplicationFailure(subject: "element \(element.base64EncodedString())", reason: .absent("settable \(kAXValueAttribute)"))
                guard try node.attributeNames().contains(kAXValueAttribute) else { throw unsettable }
                return switch unsafe AXUIElementIsAttributeSettable(node, kAXValueAttribute as CFString, &settable) {
                    case .success where settable.boolValue: AXUIElementSetAttributeValue(node, kAXValueAttribute as CFString, value as CFString)
                    case .success: throw unsettable
                    case let error: error
                }
            }
            let outcome: AXError =
                switch self {
                    case .perform(_, let action) where Self.activating.contains(action): throw ApplicationFailure(subject: description, reason: .activating)
                    case .perform(_, let action): AXUIElementPerformAction(node, action as CFString)
                    case .set(_, let value): try written(value)
                }
            guard outcome != .success else { return }
            throw ApplicationFailure(
                outcome,
                subject: description,
                process: node.process,
                answering: self,
                otherwise: outcome == .invalidUIElement ? absent : ApplicationFailure(subject: description, reason: .refused(outcome)),
            )
        }
    }
}
