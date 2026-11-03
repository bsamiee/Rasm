import ApplicationServices
public import CoreGraphics
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
    public var process: pid_t {
        get throws(ApplicationFailure) {
            guard let owner: pid_t = try AXUIElement.element(element)?.process else { throw ApplicationFailure(subject: element.base64EncodedString(), reason: .unresolved) }
            return owner
        }
    }

    @concurrent
    @discardableResult
    public func perform(in process: pid_t? = nil) async throws(ApplicationFailure) -> CGWindowID? {
        try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) -> CGWindowID? in
            let subject: String = element.base64EncodedString()
            guard let node: AXUIElement = try AXUIElement.element(element), let owner: pid_t = node.process else { throw ApplicationFailure(subject: subject, reason: .unresolved) }
            if let process, owner != process { throw ApplicationFailure(subject: subject, reason: .foreign(process)) }
            let read: [String: CFTypeRef] = try node.values([kAXEnabledAttribute, kAXWindowAttribute])
            let window: CGWindowID? = try read[kAXWindowAttribute].flatMap(AXUIElement.checked)?.window()
            let outcome: AXError =
                switch (self, read[kAXEnabledAttribute] as? Bool) {
                    case (.perform(_, kAXRaiseAction), _): throw ApplicationFailure(subject: subject, reason: .activating)
                    case (.perform, false): throw ApplicationFailure(subject: subject, reason: .disabled)
                    case (.perform(_, let action), _): AXUIElementPerformAction(node, action as CFString)
                    case (.set(_, let value), _): try node.write(value as CFString, to: kAXValueAttribute)
                }
            guard outcome != .success else { return window }
            throw ApplicationFailure(outcome, subject: subject, process: owner, answering: self) {
                ApplicationFailure(subject: subject, reason: outcome == .invalidUIElement ? .unresolved : .refused(outcome))
            }
        }
    }
}
