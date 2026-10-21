public import AppKit
import Results

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Codable, Hashable, Sendable {
    case press(label: String, under: [String])
    case set(field: String, value: String)

    public var label: String {
        switch self {
            case .press(let label, _), .set(let label, _): label
        }
    }
}

public enum ElementTarget: Hashable, Sendable, CustomStringConvertible {
    case element(CGWindowID)
    case menuBar

    public var description: String {
        switch self {
            case .element(let id): "element \(id)"
            case .menuBar: "the menu bar"
        }
    }

    func entries(of application: AXUIElement) throws(ElementFailure) -> (offered: [(name: String, node: AXUIElement, action: String?)], fields: [(label: String, node: AXUIElement)]) {
        switch self {
            case .element(let id):
                let described: Described = try application.described(window: id)
                return (described.targets.map { entry in (entry.name, entry.node, entry.action) }, described.fields)
            case .menuBar: return (try (application.value(kAXMenuBarAttribute) as AXUIElement?)?.items() ?? [], [])
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    @concurrent
    public func perform(_ action: ElementAction, on target: ElementTarget) async -> Result<Void, ElementFailure> {
        await withTaskExecutorPreference(accessibility) {
            let application: AXUIElement = AXUIElementCreateApplication(processIdentifier)
            return Result { () throws(ElementFailure) in
                func pick<Entry>(_ kind: String, _ query: String, among entries: [Entry], named name: (Entry) -> String) throws(ElementFailure) -> Entry {
                    try Unmatched.single(entries.filter { entry in name(entry) == query }, subject: "\(kind) of \(target)", query: query, candidates: entries.map(name), named: name)
                        .mapError(ElementFailure.unmatched)
                        .get()
                }
                let (offered, fields): ([(name: String, node: AXUIElement, action: String?)], [(label: String, node: AXUIElement)]) = try target.entries(of: application)
                let outcome: AXError
                switch action {
                    case .set(let field, let value):
                        outcome = try AXUIElementSetAttributeValue(pick("control", field, among: fields, named: \.label).node, kAXValueAttribute as CFString, value as CFString)
                    case .press(let label, let under):
                        let level: [(name: String, node: AXUIElement, action: String?)] = try under.reduce(
                            Result<[(name: String, node: AXUIElement, action: String?)], ElementFailure>.success(offered)
                        ) {
                            reached,
                            step in
                            reached.flatMap { level in Result { () throws(ElementFailure) in try pick("item", step, among: level, named: \.name).node.items() } }
                        }
                        .get()
                        let entry: (name: String, node: AXUIElement, action: String) = try pick(
                            "action",
                            label,
                            among: level.compactMap { entry in entry.action.map { action in (entry.name, entry.node, action) } },
                            named: \.name,
                        )
                        outcome = AXUIElementPerformAction(entry.node, entry.action as CFString)
                }
                switch outcome {
                    case .success, .cannotComplete: break
                    case let error: throw .refused(action.label, error)
                }
            }
        }
    }
}
