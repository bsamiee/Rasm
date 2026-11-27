import CoreGraphics
import Foundation
import OrderedCollections

// --- [MODELS] --------------------------------------------------------------------------

struct Observation: Encodable, Sendable {
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
        let keyEquivalent: String?
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
            keyEquivalent = control.keyEquivalent
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

    let build: String?
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
    let documents: OrderedSet<String>
    let conditions: [Condition]
    let controls: [String: Shape]
    let layout: [String: CGRect]
    let values: [String: Fact.Scalar]

    init?(
        _ container: Container,
        in open: OrderedDictionary<Int, Container>,
        build: String?,
        opener: (control: Control, container: Int)?,
        after last: Self?,
        at: Date,
        locator: (Int) -> Locator,
    ) throws(ApplicationFailure) {
        let keyed: [String: Control] = try Dictionary(
            container.controls.filter { control in control.kind != .row }.map { control throws(ApplicationFailure) -> (String, Control) in (try control.locator.text(), control) }
        ) { first, _ in first }
        self.build = build
        self.container = container.locator
        kind = container.kind
        title = container.document == nil ? container.title : nil
        modal = container.modal
        layer = container.layer
        self.default = container.default.map(locator)
        cancel = container.cancel.map(locator)
        close = container.close.map(locator)
        controls = keyed.mapValues { control in Shape(control, locator: locator) }
        if let last, (last.kind, last.title, last.modal, last.layer) == (kind, title, modal, layer),
            (last.default, last.cancel, last.close, last.controls) == (self.default, cancel, close, controls)
        {
            return nil
        }
        let held: [URL] = open.values.compactMap(\.document)
        let chosen: [Condition] = sequence(first: container) { scope in scope.parent.flatMap { parent in open[parent] } }.prefix(open.count + 1).flatMap(\.controls)
            .compactMap { (control: Control) -> Condition? in
                [Control.Kind.choice, .toggle].contains(control.kind) ? control.value?.scalar.map { value in Condition.value(at: control.locator, value: value) } : nil
            }
        size = container.frame?.size
        opened = last?.opened ?? at
        self.at = at
        truncated = container.truncated
        self.opener = opener.map { opener in Opener(container: locator(opener.container), control: opener.control.locator) }
        documents = OrderedSet(held.flatMap { (url: URL) -> [String] in url.isFileURL ? [url.path(percentEncoded: false), url.absoluteString] : [url.absoluteString] })
        conditions = chosen + [.documents(held: !held.isEmpty), .modal(at: open.values.last { shown in shown.modal && shown.asking }?.locator)]
        layout = (container.frame?.origin).map { origin in keyed.compactMapValues { control in control.frame?.offsetBy(dx: -origin.x, dy: -origin.y) } } ?? [:]
        values = keyed.compactMapValues { control in control.kind.valued ? control.value?.scalar : nil }
    }
}
