public import AppKit

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
            case .set(let element, let value): "setting \(kAXValueAttribute) of \(element.base64EncodedString()) to \"\(value)\""
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    public static func owning(_ element: Data) -> Self? {
        AXUIElement.element(element)?.process.flatMap(Self.init(processIdentifier:))
    }

    @concurrent
    public func perform(_ action: ElementAction) async -> Result<Void, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            Result { () throws(ElementFailure) in
                guard let node: AXUIElement = AXUIElement.element(action.element), node.process == processIdentifier else { throw .absent(action.element) }
                let outcome: AXError =
                    switch action {
                        case .perform(_, kAXRaiseAction): throw .activating(action)
                        case .perform(_, let name): AXUIElementPerformAction(node, name as CFString)
                        case .set(_, let value): AXUIElementSetAttributeValue(node, kAXValueAttribute as CFString, value as CFString)
                    }
                switch outcome {
                    case .success: return
                    case .invalidUIElement: throw .absent(action.element)
                    case .cannotComplete: throw vanished.map(ElementFailure.noProcess) ?? .unanswered(action)
                    case let error: throw .refused(action, error)
                }
            }
        }
    }
}
