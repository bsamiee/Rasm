public import AppKit
import Results

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Hashable, Sendable {
    case press(String)
    case set(field: String, value: String)

    public var label: String {
        switch self {
            case .press(let label), .set(let label, _): label
        }
    }
}

extension ElementAction: Codable {
    private enum Key: CodingKey { case field, value }

    public init(from decoder: any Decoder) throws {
        self = try Result { try .press(decoder.singleValueContainer().decode(String.self)) }
            .flatMapError { _ in
                Result {
                    let fields: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
                    return try .set(field: fields.decode(String.self, forKey: .field), value: fields.decode(String.self, forKey: .value))
                }
            }
            .get()
    }

    public func encode(to encoder: any Encoder) throws {
        switch self {
            case .press(let label):
                var single: any SingleValueEncodingContainer = encoder.singleValueContainer()
                try single.encode(label)
            case .set(let field, let value):
                var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
                try fields.encode(field, forKey: .field)
                try fields.encode(value, forKey: .value)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    @concurrent
    public func perform(_ action: ElementAction, on id: CGWindowID) async -> Result<Void, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            return Result { () throws(ElementFailure) in
                let found: AXUIElement? = try application.candidates().map { candidate throws(ElementFailure) in try candidate.window() == id ? candidate : nil }.compactMap(\.self).first
                guard let target: Described = try found?.described() else { throw .absent(id) }
                let (kind, offered, performs): (String, [String], [() -> AXError]) =
                    switch action {
                        case .set(let field, let value):
                            (
                                "control", target.element.controls.compactMap(\.label),
                                target.fields.filter { entry in entry.label == field }.map { entry in { AXUIElementSetAttributeValue(entry.node, kAXValueAttribute as CFString, value as CFString) } },
                            )
                        case .press(let name):
                            (
                                "action", target.element.actions,
                                target.targets.filter { entry in entry.name == name }.map { entry in { AXUIElementPerformAction(entry.node, entry.action as CFString) } },
                            )
                    }
                let perform: () -> AXError = try Unmatched.single(performs, subject: "\(kind) of element \(id)", query: action.label, candidates: offered) { _ in action.label }
                    .mapError(ElementFailure.unmatched)
                    .get()
                switch perform() {
                    case .success, .cannotComplete: break
                    case let error: throw .refused(action.label, error)
                }
            }
        }
    }
}
