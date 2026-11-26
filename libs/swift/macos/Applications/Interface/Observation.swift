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
        let labels: [String]?
        let section: String?
        let help: String?
        let range: Control.Range?
        let options: [String]?
        let shortcut: Control.Shortcut?
        let mark: String?
        let opens: Locator?
        let separated: Bool?
        let alternate: Locator?
        let parent: Locator?
        let positional: Bool

        init(_ control: Control, locator: (Int) -> Locator) {
            kind = control.kind
            role = control.role
            subrole = control.subrole
            label = control.label
            labels = control.labels
            section = control.section
            help = control.help
            range = control.range
            options = control.options?.map(\.label)
            shortcut = control.shortcut
            mark = control.mark
            opens = control.opens.map(locator)
            separated = control.separated
            alternate = control.alternate.map(locator)
            parent = control.row.map(locator)
            positional = control.positional
        }
    }

    struct Opener: Encodable, Sendable {
        let container: Locator
        let control: Locator
    }

    enum Condition: Encodable, Sendable {
        case value(at: Locator, value: Fact.Scalar)
        case documents(held: Bool)
        case modal(at: Locator?)

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
    let container: Locator
    let kind: Container.Kind
    let title: String?
    let modal: Bool
    let layer: Int?
    let size: CGSize?
    let `default`: Locator?
    let cancel: Locator?
    let close: Locator?
    let opened: Date
    let at: Date
    let truncated: Bool
    let opener: Opener?
    let documents: [String]
    let conditions: [Condition]
    let controls: [String: Shape]
    let layout: [String: CGRect]
    let values: [String: Fact.Scalar]

    init(
        _ container: Container,
        in open: OrderedDictionary<Int, Container>,
        build: String,
        opener: (control: Control, container: Int)?,
        opened: Date,
        at: Date,
        locator: (Int) -> Locator,
    ) throws(ApplicationFailure) {
        let keyed: [(key: String, control: Control)] = try container.controls.map { control throws(ApplicationFailure) in (key: try control.locator.text(), control: control) }
        let held: [URL] = open.values.compactMap(\.document)
        let atoms: [Condition] = sequence(first: container) { scope in scope.parent.flatMap { parent in open[parent] } }
            .flatMap(\.controls)
            .compactMap { (control: Control) -> Condition? in
                [.choice, .toggle].contains(control.kind) ? control.value?.scalar.map { value in Condition.value(at: control.locator, value: value) } : nil
            }
        self.build = build
        self.container = container.locator
        kind = container.kind
        title = container.document == nil ? container.title : nil
        modal = container.modal
        layer = container.layer
        size = container.frame?.size
        self.default = container.default.map(locator)
        cancel = container.cancel.map(locator)
        close = container.close.map(locator)
        self.opened = opened
        self.at = at
        truncated = container.truncated
        self.opener = opener.map { opener in Opener(container: locator(opener.container), control: opener.control.locator) }
        documents = held.map { url in url.isFileURL ? url.path(percentEncoded: false) : url.absoluteString }
        conditions = atoms + [.documents(held: !held.isEmpty), .modal(at: open.values.last { shown in shown.modal && shown.asking }?.locator)]
        controls = Dictionary(uniqueKeysWithValues: keyed.map { key, control in (key, Shape(control, locator: locator)) })
        layout = Dictionary(
            uniqueKeysWithValues: keyed.compactMap { key, control in
                control.frame.flatMap { frame in container.frame.map { bounds in (key, frame.offsetBy(dx: -bounds.origin.x, dy: -bounds.origin.y)) } }
            }
        )
        values = Dictionary(uniqueKeysWithValues: keyed.compactMap { key, control in control.kind.valued ? control.value?.scalar.map { value in (key, value) } : nil })
    }
}
