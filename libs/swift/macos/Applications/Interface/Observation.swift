import CoreGraphics
import Foundation
import OrderedCollections

// --- [MODELS] --------------------------------------------------------------------------

public struct Observation: Encodable, Sendable {
    struct Shape: Encodable, Equatable, Sendable {
        let kind: Control.Kind
        let role: String
        let subrole: String?
        let label: Control.Label?
        let help: String?
        let range: Control.Range?
        let options: [String]?
        let shortcut: Control.Shortcut?
        let mark: String?
        let opens: String?
        let separated: Bool?
        let alternate: String?
        let parent: String?
        let positional: Bool
    }

    struct Opener: Encodable, Sendable {
        let container: String
        let control: String
    }

    enum Condition: Encodable, Sendable {
        case value(at: String, value: Fact.Scalar)
        case documents(held: Bool)
        case modal(at: String?)

        private enum CodingKeys: String, CodingKey {
            case tag = "_tag"
            case at, value, held
        }

        func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: CodingKeys.self)
            switch self {
                case .value(let at, let value):
                    try fields.encode("Value", forKey: .tag)
                    try fields.encode(at, forKey: .at)
                    try fields.encode(value, forKey: .value)
                case .documents(let held):
                    try fields.encode("Documents", forKey: .tag)
                    try fields.encode(held, forKey: .held)
                case .modal(let at):
                    try fields.encode("Modal", forKey: .tag)
                    try fields.encode(at, forKey: .at)
            }
        }
    }

    let build: String
    let container: String
    let kind: Container.Kind
    let title: String?
    let document: Bool
    let modal: Bool
    let layer: Int
    let size: CGSize
    let `default`: String?
    let cancel: String?
    let close: String?
    let opened: Date
    let at: Date
    let complete: Bool
    let cause: Opener?
    let documents: [String]
    let scope: [Condition]
    let controls: [String: Shape]
    let layout: [String: CGRect]
    let values: [String: Fact.Scalar]

    init(_ container: Container, in open: OrderedDictionary<Int, Container>, build: String, cause: Locator?, opened: Date, at: Date, locator: (Int) -> Locator) throws(ApplicationFailure) {
        let origin: CGPoint = container.frame?.origin ?? .zero
        let read: [(key: String, control: Control, shape: Shape)] = try container.controls.map { control throws(ApplicationFailure) in
            let shape = Shape(
                kind: control.kind,
                role: control.role,
                subrole: control.subrole,
                label: control.label,
                help: control.help,
                range: control.range,
                options: control.options?.map(\.label),
                shortcut: control.shortcut,
                mark: control.mark,
                opens: try control.opens.map(locator)?.text(),
                separated: control.separated,
                alternate: try control.alternate.map(locator)?.text(),
                parent: try control.row.map(locator)?.text(),
                positional: control.positional,
            )
            return (key: try control.locator.text(), control: control, shape: shape)
        }
        let atoms: [Condition] = try sequence(first: container) { scope in scope.parent.flatMap { parent in open[parent] } }
            .flatMap(\.controls)
            .compactMap { (control: Control) -> (locator: Locator, value: Fact.Scalar)? in
                control.kind == .choice || control.kind == .toggle ? control.value?.scalar.map { value in (locator: control.locator, value: value) } : nil
            }
            .map { atom throws(ApplicationFailure) in Condition.value(at: try atom.locator.text(), value: atom.value) }
        let asking: Locator? = open.values.last { shown in shown.modal && shown.asking }?.locator
        let held: [URL] = open.values.compactMap(\.document)
        let level: CGWindowLevelKey =
            switch container.kind {
                case .menuBar: .mainMenuWindow
                case .statusItem: .statusWindow
                case .menu: .popUpMenuWindow
                case .window, .dialog, .panel, .sheet, .popover, .drawer, .prompt, .toolbar: .normalWindow
            }
        self.build = build
        self.container = try container.locator.text()
        kind = container.kind
        title = container.title
        document = container.document != nil
        modal = container.modal
        layer = container.layer ?? Int(CGWindowLevelForKey(level))
        size = container.frame?.size ?? .zero
        self.default = try container.default.map(locator)?.text()
        cancel = try container.cancel.map(locator)?.text()
        close = try container.close.map(locator)?.text()
        self.opened = opened
        self.at = at
        complete = !container.truncated
        self.cause = try cause.map { control throws(ApplicationFailure) in Opener(container: try Locator(steps: Array(control.steps.dropLast())).text(), control: try control.text()) }
        documents = held.map { url in url.isFileURL ? url.path(percentEncoded: false) : url.absoluteString }
        scope = atoms + [.documents(held: !held.isEmpty), .modal(at: try asking?.text())]
        controls = Dictionary(uniqueKeysWithValues: read.map { entry in (entry.key, entry.shape) })
        layout = Dictionary(uniqueKeysWithValues: read.compactMap { entry in entry.control.frame.map { frame in (entry.key, frame.offsetBy(dx: -origin.x, dy: -origin.y)) } })
        values = Dictionary(uniqueKeysWithValues: read.compactMap { entry in entry.control.kind.valued ? entry.control.value?.scalar.map { value in (entry.key, value) } : nil })
    }
}
