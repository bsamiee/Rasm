package import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
import CoreGraphics
import Foundation
import OrderedCollections
import RegexBuilder
import SPI

// --- [CONSTANTS] -----------------------------------------------------------------------

private let recognizedAttribute: String = "recognized"
private let keyEquivalentAttribute: String = "keyEquivalent"

// --- [MODELS] --------------------------------------------------------------------------

struct Hierarchy: Sendable {
    var elements: [Element]
    var roots: [Int]
    let windows: [InstanceState.Window]
    let documents: [URL]
    let regular: Bool
    let focused: Int?
    var truncated: [Int]
    let extras: Int?
    var full: Set<Int> = []
    var recognized: [CGWindowID: [RecognizedLine]] = [:]
    var runs: [Int: [Control.Run]] = [:]

    var described: Set<CGWindowID> {
        let read: Set<Int> = Set(elements.map(\.id))
        let cut: Set<Int> = Set(truncated)
        return Set(
            elements.compactMap { (element: Element) -> CGWindowID? in
                cut.contains(element.id) || element.children.contains { (child: Int) -> Bool in read.contains(child) } ? element.window : nil
            }
        )
    }

    var unreadable: [CGWindowID: Int] {
        let described: Set<CGWindowID> = self.described
        let hollow: [(CGWindowID, Int)] = elements.compactMap { (element: Element) -> (CGWindowID, Int)? in
            element.window.flatMap { (window: CGWindowID) -> (CGWindowID, Int)? in described.contains(window) ? nil : (window, element.id) }
        }
        return Dictionary(hollow) { (first: Int, _: Int) -> Int in first }
    }

    var undescribed: [InstanceState.Window] {
        let (described, unreadable): (Set<CGWindowID>, [CGWindowID: Int]) = (self.described, self.unreadable)
        return windows.filter { row in
            row.onScreen && !described.contains(row.number) && (row.container == nil || unreadable[row.number] != nil)
                && (row.layer < InstanceState.Window.Band.mainMenu.level || row.band == .popUpMenu)
        }
    }

    var flyouts: [InstanceState.Window] { undescribed.filter { row in row.band == .modalPanel || row.band == .popUpMenu } }
}

struct Control: Encodable, Equatable, Sendable {
    enum Kind: String, Codable, Sendable {
        case action
        case toggle
        case choice
        case multi
        case row
        case number
        case measure
        case text
        case color
        case date
        case item
        case region
        case label

        var valued: Bool {
            switch self {
                case .toggle, .choice, .number, .measure, .color, .date: true
                case .action, .multi, .row, .text, .item, .region, .label: false
            }
        }

        var holdsValueAttribute: Bool {
            switch self {
                case .toggle, .number, .measure, .text, .color, .date, .label: true
                case .action, .choice, .multi, .row, .item, .region: false
            }
        }

        var defaultActions: Set<String> {
            switch self {
                case .action, .multi, .row, .region: [kAXPressAction, kAXPickAction, kAXConfirmAction]
                case .text, .label: [kAXPressAction, kAXPickAction]
                case .item: [kAXPressAction, kAXPickAction, kAXConfirmAction, kAXCancelAction]
                case .toggle, .choice, .number, .measure, .color, .date: [kAXPressAction, kAXPickAction, kAXConfirmAction, kAXIncrementAction, kAXDecrementAction]
            }
        }

        private static let roles: [String: Self] = [
            kAXMenuBarItemRole: .item, kAXMenuItemRole: .item, kAXCheckBoxRole: .toggle, kAXRadioButtonRole: .toggle, kAXDisclosureTriangleRole: .toggle, kAXTextFieldRole: .text,
            kAXTextAreaRole: .text, kAXPopUpButtonRole: .choice, kAXComboBoxRole: .choice, kAXRadioGroupRole: .choice, kAXTabGroupRole: .choice, kAXTableRole: .multi,
            kAXOutlineRole: .multi, kAXListRole: .multi, kAXBrowserRole: .multi, kAXGridRole: .multi, kAXRowRole: .row, kAXSliderRole: .number, kAXIncrementorRole: .number,
            kAXValueIndicatorRole: .number, kAXLevelIndicatorRole: .number, kAXProgressIndicatorRole: .number, kAXBusyIndicatorRole: .number, kAXRelevanceIndicatorRole: .number,
            kAXSplitterRole: .number, kAXColorWellRole: .color, kAXDateFieldRole: .date, kAXTimeFieldRole: .date, NSAccessibility.Role.dateTimeAreaRole.rawValue: .date,
            kAXMenuButtonRole: .action,
        ]

        private static let subroles: [String: Self] = [kAXSwitchSubrole: .toggle, kAXToggleSubrole: .toggle, kAXSearchFieldSubrole: .text, kAXSecureTextFieldSubrole: .text]

        init?(_ element: Element, measured: Bool) {
            let role: String = element.role ?? kAXUnknownRole
            let value: Fact? = element.facts[kAXValueAttribute]
            let bare: Bool = element.children.isEmpty && value == nil && element.label == nil && element.frame != nil && element.actions.allSatisfy { action in action == kAXShowMenuAction }
            let subrole: Self? = element.text(kAXSubroleAttribute).flatMap { (name: String) -> Self? in Self.subroles[name] }
            let worded: Bool = element.text(kAXValueAttribute)?.unicodeScalars.contains { scalar in !scalar.properties.isWhitespace && scalar.properties.generalCategory != .privateUse } == true
            let kind: Self? =
                switch (Self.roles[role], subrole) {
                    case (.item, _) where role == kAXMenuItemRole && element.label == nil: nil
                    case (.item, _): .item
                    case (_, _) where measured && (role == kAXTextFieldRole || role == kAXButtonRole): .measure
                    case (_, .some(let kind)), (.some(let kind), .none): kind
                    case (.none, .none) where value?.number != nil && (role == kAXGroupRole || role == kAXUnknownRole): .number
                    case (.none, .none) where value?.date != nil: .date
                    case (.none, .none) where element.actions.contains(kAXIncrementAction): .number
                    case (.none, .none) where element.actions.contains(where: Self.action.defaultActions.contains): .action
                    case (.none, .none) where role == kAXStaticTextRole && worded && element.frame?.isEmpty == false: .label
                    case (.none, .none) where role == kAXUnknownRole && bare: .region
                    case (.none, .none): nil
                }
            guard let kind else { return nil }
            self = kind
        }
    }

    enum Evidence: String, Codable, Sendable {
        case attribute
        case marked
    }

    struct Label: Encodable, Equatable, Sendable {
        let text: String
        let attribute: String
    }

    struct Option: Encodable, Equatable, Sendable {
        let label: String
        let ref: Int
        let help: String?
    }

    struct Range: Encodable, Equatable, Sendable {
        let minimum: Double?
        let maximum: Double?
        let step: Double?
        let wraps: Bool?
    }

    struct Shortcut: Encodable, Equatable, Sendable {
        let key: Key
        let modifiers: [Modifier]
    }

    enum Invalid: String, Encodable, Sendable {
        case `true`
        case grammar
        case spelling
    }

    enum Current: String, Encodable, Sendable {
        case `true`
        case page
        case step
        case location
        case date
        case time
    }

    enum Sort: String, Encodable, Sendable {
        case ascending
        case descending

        init?(_ direction: String?) {
            switch direction {
                case .some(kAXAscendingSortDirectionValue): self = .ascending
                case .some(kAXDescendingSortDirectionValue): self = .descending
                case .some, .none: return nil
            }
        }
    }

    struct TextRange: Encodable, Equatable, Sendable {
        let location: Int
        let length: Int
    }

    struct Run: Encodable, Equatable, Sendable {
        let range: TextRange
        let text: String
        let attributes: [String: Fact]
        let frame: CGRect?
    }

    struct Scroll: Encodable, Equatable, Sendable {
        let contentSize: CGSize?
        let horizontalScrollBar: Double?
        let verticalScrollBar: Double?
    }

    let ref: Int
    let locator: Locator
    let kind: Kind
    let role: String
    let subrole: String?
    let label: Label?
    let labels: [String]?
    let section: String?
    var value: Fact?
    let settable: Bool?
    let enabled: Bool?
    let focused: Bool?
    let selected: Evidence?
    let help: String?
    let frame: CGRect?
    let positional: Bool
    let options: [Option]?
    let range: Range?
    let mark: String?
    let shortcut: Shortcut?
    let keyEquivalent: String?
    let opens: Int?
    let separated: Bool?
    let alternate: Int?
    let placeholder: String?
    let level: Int?
    let index: Int?
    let parent: Int?
    let expanded: Bool?
    let actions: [String]?
    let alternateUIVisible: Bool?
    let disclosedBy: Int?
    let total: Int?
    let sort: Sort?
    let column: String?
    let scroll: Scroll?
    let selectedTextRanges: [TextRange]?
    var numberOfCharacters: Int?
    let insertionPointLineNumber: Int?
    let runs: [Run]?
    let invalid: Invalid?
    let required: Bool?
    let current: Current?

    private enum CodingKeys: CodingKey {
        case ref, locator, kind, role, subrole, label, labels, section, value, settable, enabled, focused, selected, help, frame, positional, range, mark, shortcut, keyEquivalent, opens, separated,
            alternate, placeholder, level, index, expanded, actions, alternateUIVisible, disclosedBy, total, sort, column, scroll, selectedTextRanges, numberOfCharacters,
            insertionPointLineNumber, runs, invalid, required, current
    }
}

struct Container: Encodable, Equatable, Sendable {
    enum Kind: String, Codable, Sendable {
        case window
        case dialog
        case panel
        case sheet
        case popover
        case drawer
        case prompt
        case menuBar
        case statusItem
        case menu
        case toolbar
        case web

        func asking(modal: Bool) -> Bool {
            switch self {
                case .sheet, .prompt: true
                case .dialog: modal
                case .window, .panel, .popover, .drawer, .menuBar, .statusItem, .menu, .toolbar, .web: false
            }
        }

        private static let roles: [String: Self] = [
            kAXMenuBarRole: .menuBar, kAXMenuRole: .menu, kAXSheetRole: .sheet, kAXPopoverRole: .popover, kAXDrawerRole: .drawer, kAXToolbarRole: .toolbar,
            NSAccessibility.Role.webAreaRole.rawValue: .web, kAXLayoutAreaRole: .panel,
        ]

        private static let subroles: [String: Self] = [
            kAXDialogSubrole: .dialog, kAXSystemDialogSubrole: .dialog, kAXApplicationDialogSubrole: .dialog, kAXApplicationAlertDialogSubrole: .dialog, kAXFloatingWindowSubrole: .panel,
            kAXSystemFloatingWindowSubrole: .panel,
        ]

        init?(_ element: Element, root: Bool, in hierarchy: Hierarchy) {
            let byRole: Self? = element.role.flatMap { (role: String) -> Self? in Self.roles[role] }
            let bySubrole: Self? = element.text(kAXSubroleAttribute).flatMap { (subrole: String) -> Self? in Self.subroles[subrole] }
            let stated: Self? = byRole ?? bySubrole
            let kind: Self? =
                switch (stated, root, element.flag(kAXModalAttribute) == true) {
                    case (.menuBar, true, _): hierarchy.extras == element.id ? .statusItem : .menuBar
                    case (.menu, true, _), (.sheet, _, _), (.popover, _, _), (.drawer, _, _), (.toolbar, _, _), (.web, _, _): stated
                    case (_, true, true): hierarchy.regular ? .dialog : .prompt
                    case (.dialog, _, false), (.dialog, false, true): .dialog
                    case (.panel, true, _): .panel
                    case (.panel, false, _) where element.role == kAXLayoutAreaRole && !element.children.isEmpty: .panel
                    case (_, true, _): .window
                    case (_, false, _): nil
                }
            guard let kind else { return nil }
            self = kind
        }
    }

    let ref: Int
    let locator: Locator
    let kind: Kind
    let title: String?
    let help: String?
    let document: URL?
    let edited: Bool?
    let fullScreen: Bool?
    let loaded: Bool?
    let loadingProgress: Double?
    let window: CGWindowID?
    let frame: CGRect?
    let layer: Int?
    let closed: Bool
    let modal: Bool
    let asking: Bool
    let focused: Bool?
    let parent: Int?
    let `default`: Int?
    let cancel: Int?
    let close: Int?
    let text: [String]
    let truncated: Bool
    let unreadable: Bool
    var controls: [Control]

    private enum CodingKeys: CodingKey {
        case ref, locator, kind, title, help, document, edited, fullScreen, loaded, loadingProgress, window, frame, layer, closed, modal, asking, focused, `default`, cancel, close, text, truncated
        case unreadable
    }
}

package struct Model: Encodable, Sendable {
    package enum Scope: Decodable, Sendable {
        case application
        case container(ref: Int)
        case menu(ref: Int)
        case rows(ref: Int, from: Int, count: Int)
        case text(ref: Int)
        case menus

        private enum CodingKeys: CodingKey {
            case application, container, menu, rows, text, menus
        }

        private enum Field: String, CodingKey {
            case tag = "_tag"
            case ref, from, count
        }

        package init(from decoder: any Decoder) throws {
            let fields = try decoder.container(keyedBy: Field.self)
            let tag: String = try fields.decode(String.self, forKey: .tag)
            self =
                switch CodingKeys(stringValue: tag) {
                    case .application: .application
                    case .container: try .container(ref: fields.decode(Int.self, forKey: .ref))
                    case .menu: try .menu(ref: fields.decode(Int.self, forKey: .ref))
                    case .rows: try .rows(ref: fields.decode(Int.self, forKey: .ref), from: fields.decode(Int.self, forKey: .from), count: fields.decode(Int.self, forKey: .count))
                    case .text: try .text(ref: fields.decode(Int.self, forKey: .ref))
                    case .menus: .menus
                    case nil: throw DecodingError.dataCorruptedError(forKey: .tag, in: fields, debugDescription: "\(tag) names no model scope")
                }
        }
    }

    package struct Known: Decodable, Sendable {
        let build: String?
        let containers: Set<String>
        let labels: Set<String>
    }

    indirect enum Node: Encodable, Sendable {
        private enum Key: String, CodingKey {
            case tag = "_tag"
            case children
        }

        case container(Container, children: [Self])
        case control(Control, children: [Self])
        case option(Control.Option)

        func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: Key.self)
            try fields.encodeIfPresent(Mirror(reflecting: self).children.first?.label, forKey: .tag)
            switch self {
                case .container(let held, let children):
                    try held.encode(to: encoder)
                    try fields.encode(children, forKey: .children)
                case .control(let held, let children):
                    try held.encode(to: encoder)
                    try fields.encode(children, forKey: .children)
                case .option(let held):
                    try held.encode(to: encoder)
                    try fields.encode([Self](), forKey: .children)
            }
        }
    }

    private enum CodingKeys: CodingKey {
        case sequence, elements, pointer
    }

    let sequence: Int
    let containers: [Container]
    let located: [Int: Int]
    var drawn: [Int: (window: CGWindowID, frame: CGRect)] = [:]
    var pointer: Pointer?

    var elements: [Node] {
        let refs: Set<Int> = Set(containers.map(\.ref))
        let (inside, roots): ([Container], [Container]) = containers.reduce(into: ([], [])) { split, each in
            each.parent.map { parent in parent != each.ref && refs.contains(parent) } == true ? split.0.append(each) : split.1.append(each)
        }
        let nested: [Int: [Container]] = Dictionary(grouping: inside) { container in container.parent ?? container.ref }
        func control(_ held: Control, cells: [Int: [Control]], rows: [Control] = []) -> Node {
            .control(held, children: (held.options ?? []).map(Node.option) + (cells[held.ref] ?? []).map { cell in control(cell, cells: cells) } + rows.map { row in control(row, cells: cells) })
        }
        func container(_ held: Container) -> Node {
            let listed: Set<Int> = Set(held.controls.map(\.ref))
            let (inner, top): ([Control], [Control]) = held.controls.reduce(into: ([], [])) { split, each in
                each.parent.map { parent in parent != each.ref && listed.contains(parent) } == true ? split.0.append(each) : split.1.append(each)
            }
            let cells: [Int: [Control]] = Dictionary(grouping: inner) { cell in cell.parent ?? cell.ref }
            let runs: [(held: Control, rows: [Control])] = top.reduce(into: []) { runs, each in
                if each.kind == .row, let last = runs.indices.last, runs[last].held.kind == .multi { runs[last].rows.append(each) } else { runs.append((held: each, rows: [])) }
            }
            return .container(held, children: runs.map { run in control(run.held, cells: cells, rows: run.rows) } + (nested[held.ref] ?? []).map(container))
        }
        return roots.map(container)
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(sequence, forKey: .sequence)
        try fields.encode(elements, forKey: .elements)
        try fields.encodeIfPresent(pointer, forKey: .pointer)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Element {
    fileprivate var label: Control.Label? {
        let attributes: [String] = [kAXTitleAttribute, kAXDescriptionAttribute, recognizedAttribute, NSAccessibility.Attribute.userInputLabels.rawValue, kAXFilenameAttribute]
        return attributes.lazy.compactMap { attribute in labelled(attribute) }.first
    }

    fileprivate var inputLabels: [String] { facts[NSAccessibility.Attribute.userInputLabels.rawValue]?.list?.compactMap(\.text).filter { text in !text.isEmpty } ?? [] }

    var supplementary: (read: [String], counted: [String], settable: [String]) {
        switch (Control.Kind(self, measured: false), role, text(kAXSubroleAttribute)) {
            case (.number, _, _): ([kAXMinValueAttribute, kAXMaxValueAttribute, kAXValueIncrementAttribute, kAXValueWrapsAttribute], [], [])
            case (.multi, _, _): ([kAXVisibleRowsAttribute, kAXVisibleChildrenAttribute, kAXHeaderAttribute, kAXVisibleColumnsAttribute], [kAXRowsAttribute], [])
            case (.row, _, _):
                ([kAXDisclosingAttribute, kAXDisclosureLevelAttribute, kAXIndexAttribute, kAXURLAttribute, kAXFilenameAttribute, kAXDisclosedByRowAttribute, kAXAlternateUIVisibleAttribute], [], [])
            case (.text, _, _): ([kAXSelectedTextRangeAttribute, kAXSelectedTextRangesAttribute, kAXNumberOfCharactersAttribute, kAXInsertionPointLineNumberAttribute], [], [kAXValueAttribute])
            case (.choice, kAXTabGroupRole, _): ([kAXTabsAttribute], [], [])
            case (.item, kAXMenuItemRole, _): ([kAXMenuItemPrimaryUIElementAttribute], [], [])
            case (_, kAXColumnRole, _): ([kAXHeaderAttribute, kAXIndexAttribute], [], [])
            case (_, kAXCellRole, _): ([kAXColumnIndexRangeAttribute], [], [])
            case (_, kAXScrollAreaRole, _): ([kAXVerticalScrollBarAttribute, kAXHorizontalScrollBarAttribute, NSAccessibility.Attribute.contentSize.rawValue], [], [])
            case (_, NSAccessibility.Role.webAreaRole.rawValue, _): ([kAXLoadedAttribute, kAXLoadingProgressAttribute], [], [])
            case (_, _, kAXSortButtonSubrole): ([kAXSortDirectionAttribute], [], [])
            case (_, _, kAXCloseButtonSubrole): ([kAXEditedAttribute], [], [])
            case (_, _, _): ([], [], [])
        }
    }

    fileprivate var optionText: String? { label?.text ?? text(kAXValueAttribute) }

    fileprivate var statesSelection: Bool {
        let subroles: [String] = [NSAccessibility.Subrole.segment.rawValue, NSAccessibility.Subrole.tabButtonSubrole.rawValue]
        return role == kAXRadioButtonRole || text(kAXSubroleAttribute).map(subroles.contains) == true
    }

    var children: [Int] {
        switch role {
            case kAXTableRole, kAXOutlineRole: facts[kAXVisibleRowsAttribute].map { rows in rows.elements + columns } ?? facts[kAXChildrenAttribute]?.elements ?? []
            case kAXListRole: (facts[kAXVisibleChildrenAttribute] ?? facts[kAXChildrenAttribute])?.elements ?? []
            case kAXColumnRole: []
            default: facts[kAXChildrenAttribute]?.elements ?? []
        }
    }

    var columns: [Int] { (facts[kAXHeaderAttribute]?.elements ?? []) + (facts[kAXVisibleColumnsAttribute]?.elements ?? []) }

    fileprivate func labelled(_ attribute: String) -> Control.Label? {
        let stated: [Fact] = facts[attribute].map { fact in fact.list ?? [fact] } ?? []
        return stated.lazy.compactMap(\.text).first { text in !text.isEmpty }.map { text in Control.Label(text: text, attribute: attribute) }
    }

    fileprivate func caption(in held: [Int: Element]) -> Control.Label? {
        let titled: Element? = facts[kAXTitleUIElementAttribute]?.element.flatMap { title in held[title] }
        return (titled?.text(kAXValueAttribute) ?? titled?.text(kAXTitleAttribute)).map { text in Control.Label(text: text, attribute: kAXTitleUIElementAttribute) }
    }

    fileprivate func flag(_ attribute: String) -> Bool? { facts[attribute]?.bool ?? facts[attribute]?.integer.map { value in value != 0 } }

    fileprivate func tabs(in held: [Int: Element]) -> [Element] {
        let stated: [Element] = (facts[kAXTabsAttribute]?.elements ?? []).compactMap { (tab: Int) -> Element? in held[tab] }
        let selecting: [Element] = children.compactMap { (child: Int) -> Element? in held[child] }.filter(\.statesSelection)
        return stated.isEmpty ? selecting : stated
    }

    fileprivate func identify(by label: Control.Label?, redacting withheld: [(name: String, url: URL)]) -> (role: String, keys: [Locator.Step.Key]) {
        let document: Locator.Step.Key? = text(kAXDocumentAttribute).flatMap(URL.init(string:)).map(Locator.Step.Key.document(at:))
        let keys: [Locator.Step.Key?] = [
            (text(kAXIdentifierAttribute) ?? text(kAXDOMIdentifierAttribute)).map(Locator.Step.Key.identifier),
            document,
            document == nil ? label.flatMap { label in .label(folding: label.text) } : nil,
        ]
        return (role: role ?? kAXUnknownRole, keys: keys.compactMap { key in key?.redacting(withheld) } + [.sole])
    }
}

extension Model {
    init(_ hierarchy: Hierarchy, sequence: Int, under: [Int: Locator] = [:], locale: Locale = .autoupdatingCurrent, ref intern: (Locator) -> Int) {
        let flyouts: Set<CGWindowID> = Set(hierarchy.flyouts.map(\.number))
        let unreadable: [CGWindowID: Int] = hierarchy.unreadable
        let read: [Int: Element] = Dictionary(hierarchy.elements.map { element in (element.id, element) }) { first, _ in first }
        let regions: [(head: Element, members: [Element], hosted: Bool)] = hierarchy.undescribed.map { (row: InstanceState.Window) -> (head: Element, members: [Element], hosted: Bool) in
            let id: Int = ~(Int(row.number) << 32)
            let recognized: [RecognizedLine] = (hierarchy.recognized[row.number] ?? []).sorted { first, second in (first.frame.minY, first.frame.minX) < (second.frame.minY, second.frame.minX) }
            let column: CGFloat? = recognized.map { (line: RecognizedLine) -> (shared: Int, edge: CGFloat) in
                (shared: recognized.count(where: { (other: RecognizedLine) -> Bool in abs(other.frame.minX - line.frame.minX) < line.frame.height }), edge: line.frame.minX)
            }.max { first, second in (first.shared, second.edge) < (second.shared, first.edge) }?.edge
            let lines: [(facts: [String: Fact], frame: CGRect, section: String?)] = recognized.reduce(
                into: (section: String?.none, members: [(facts: [String: Fact], frame: CGRect, section: String?)]())
            ) { walk, line in
                let heading: Bool = line.isTitle || column.map { edge in line.frame.minX < edge - line.frame.height } == true
                if heading { walk.section = line.text }
                let facts: [String: Fact?] = [
                    kAXRoleAttribute: .text(value: heading ? kAXStaticTextRole : kAXMenuItemRole),
                    (heading ? kAXValueAttribute : recognizedAttribute): .text(value: line.text),
                    kAXEnabledAttribute: heading ? nil : Fact.bool(value: line.enabled),
                    keyEquivalentAttribute: heading ? nil : line.keyEquivalentTranscript.map(Fact.text(value:)),
                ]
                walk.members.append((facts: facts.compactMapValues(\.self), frame: line.frame, section: heading ? nil : walk.section))
            }.members
            let menu: Bool = flyouts.contains(row.number)
            let members: [(facts: [String: Fact], frame: CGRect, section: String?)] = menu ? lines : [(facts: [:], frame: row.frame, section: nil)]
            let sections: [Fact] = members.enumerated().compactMap { offset, member in
                member.section.map { section in Fact.record(fields: [sectionObjectKey: .element(id: id - 1 - offset), sectionDescriptionKey: .text(value: section)]) }
            }
            let listed: [Fact] = members.indices.map { (index: Int) -> Fact in .element(id: id - 1 - index) }
            let content: [String: Fact] = [kAXChildrenAttribute: .list(values: listed), NSAccessibility.Attribute.sections.rawValue: .list(values: sections)]
            let window: [String: Fact?] = [
                kAXRoleAttribute: .text(value: menu ? kAXMenuRole : kAXWindowRole), kAXTitleAttribute: row.title.map(Fact.text(value:)), frameAttribute: .rect(value: row.frame),
            ]
            let host: Element? = unreadable[row.number].flatMap { host in read[host] }
            let framed: [String: Fact] = window.compactMapValues { (fact: Fact?) -> Fact? in fact }.merging(content) { (stated: Fact, _: Fact) -> Fact in stated }
            let synthetic = Element(id: id, facts: framed, actions: [], window: row.number)
            let head: Element =
                host.map { (host: Element) -> Element in
                    let facts: [String: Fact] = host.facts.merging(content) { (_: Fact, drawn: Fact) -> Fact in drawn }
                    return Element(id: host.id, facts: facts, actions: host.actions, window: host.window, descriptions: host.descriptions, counts: host.counts, settable: host.settable)
                } ?? synthetic
            let shown: [Element] = members.enumerated().map { offset, member -> Element in
                let facts: [String: Fact] = member.facts.merging([frameAttribute: .rect(value: member.frame)]) { (stated: Fact, _: Fact) -> Fact in stated }
                return Element(id: id - 1 - offset, facts: facts, actions: [], window: row.number)
            }
            return (head: head, members: shown, hosted: host != nil)
        }
        let drawnElements: [Element] = regions.flatMap { (region: (head: Element, members: [Element], hosted: Bool)) -> [Element] in [region.head] + region.members }
        let drawnPairs: [(Int, Element)] = drawnElements.map { (element: Element) -> (Int, Element) in (element.id, element) }
        let held: [Int: Element] = read.merging(drawnPairs) { (_: Element, drawn: Element) -> Element in drawn }
        let regionHeads: Set<Int> = Set(regions.map(\.head.id))
        let home: URL = .homeDirectory
        let documents: OrderedSet<URL> = OrderedSet(hierarchy.documents + held.values.compactMap { (element: Element) -> URL? in element.text(kAXDocumentAttribute).flatMap(URL.init(string:)) })
        let places: [(text: String, url: URL)] =
            documents.flatMap { (url: URL) -> [(text: String, url: URL)] in [(text: url.lastPathComponent, url: url), (text: url.deletingPathExtension().lastPathComponent, url: url)] }
            + [(text: home.lastPathComponent, url: home), (text: NSUserName(), url: home)]
        let withheld: [(name: String, url: URL)] = places.compactMap { (entry: (text: String, url: URL)) -> (name: String, url: URL)? in
            Locator.Step.Key.label(folding: entry.text)?[case: \.label].map { name in (name: name, url: entry.url) }
        }.sorted { first, second in first.name.count > second.name.count }
        let drawnRoots: [Int] = regions.compactMap { (region: (head: Element, members: [Element], hosted: Bool)) -> Int? in region.hosted ? nil : region.head.id }
        let readRoots: [Int] = hierarchy.roots.filter { (root: Int) -> Bool in held[root] != nil }
        let roots: OrderedSet<Int> = OrderedSet(readRoots + drawnRoots)
        var (order, walked, pending): (OrderedSet<Int>, [Int: Int], [Int]) = ([], [:], Array(roots.reversed()))
        while let id = pending.popLast() {
            guard let element = held[id], order.append(id).inserted else { continue }
            let children: [Int] = element.children.filter { child in held[child] != nil && walked[child] == nil && !roots.contains(child) }
            walked.merge(children.map { child in (child, id) }) { first, _ in first }
            pending += children.reversed()
        }
        let parents: [Int: Int] = held.compactMapValues { (element: Element) -> Int? in element.facts[kAXParentAttribute]?.element.flatMap { parent in held[parent]?.id } }
            .merging(walked) { stated, _ in stated }
        let kinds: [Int: Container.Kind] = held.compactMapValues { (element: Element) -> Container.Kind? in Container.Kind(element, root: roots.contains(element.id), in: hierarchy) }
        let lineage: [Int: (owner: Int?, inner: [Int])] = held.mapValues { (element: Element) -> (owner: Int?, inner: [Int]) in
            let inner: [Int] = Array(Swift.sequence(first: element.id) { child in parents[child] }.dropFirst().prefix(held.count).prefix(while: { (scope: Int) -> Bool in kinds[scope] == nil }))
            return (owner: parents[inner.last ?? element.id].flatMap { owner in kinds[owner].map { _ in owner } }, inner: inner)
        }
        let owners: [Int: Int] = lineage.compactMapValues(\.owner)
        let grouped: [Element] = order.compactMap { id in kinds[id] == nil ? held[id] : nil }
        let members: [Int: [Element]] = Dictionary(grouping: grouped) { (element: Element) -> Int in owners[element.id] ?? element.id }
        let containers: [(element: Element, kind: Container.Kind)] = held.values.compactMap { (element: Element) -> (place: Int, element: Element, kind: Container.Kind)? in
            kinds[element.id].map { kind in
                (place: order.firstIndex(of: element.id) ?? members[element.id]?.first.flatMap { member in order.firstIndex(of: member.id) } ?? order.count, element: element, kind: kind)
            }
        }
        .sorted { first, second in (first.place, first.element.id) < (second.place, second.element.id) }.map { entry in (element: entry.element, kind: entry.kind) }
        let siblings: [[Element]] = Array(Dictionary(grouping: containers.map(\.element)) { (element: Element) -> Int? in owners[element.id] }.values)
        let placed: [Int: Locator.Step] = Dictionary(
            uniqueKeysWithValues: siblings.flatMap { (group: [Element]) -> [(Int, Locator.Step)] in
                let keyed: [(role: String, keys: [Locator.Step.Key])] = group.map { (sibling: Element) -> (role: String, keys: [Locator.Step.Key]) in
                    sibling.identify(by: sibling.label, redacting: withheld)
                }
                return Array(zip(group.map(\.id), Locator.Step.distinguish(keyed)))
            }
        )
        let scopes: [(element: Element, kind: Container.Kind, locator: Locator)] = containers.map { element, kind in
            let chain: [Int] = Swift.sequence(first: element.id) { scope in owners[scope] }.prefix(held.count).reversed()
            let named: Locator? = chain.first.flatMap { root in under[root] }
            return (element, kind, chain.dropFirst(named == nil ? 0 : 1).compactMap { scope in placed[scope] }.reduce(named ?? Locator(steps: [])) { locator, step in locator.appending(step) })
        }
        let locators: [Int: Locator] = Dictionary(uniqueKeysWithValues: scopes.map { scope in (scope.element.id, scope.locator) })
        let unit = Regex {
            ZeroOrMore(.whitespace)
            FloatingPointFormatStyle<Double>(locale: locale)
            ZeroOrMore(.whitespace)
            CharacterClass(.digit, .whitespace, .anyOf(".,+-")).inverted
        }
        let measured: Set<Int> = Set(hierarchy.elements.compactMap { element in element.text(kAXValueAttribute)?.prefixMatch(of: unit).map { _ in element.id } })
        let titles: Set<Int> = Set(hierarchy.elements.compactMap { element in element.facts[kAXTitleUIElementAttribute]?.element.flatMap { title in title == parents[element.id] ? nil : title } })
        let truncated: Set<Int> = Set(hierarchy.truncated)
        let sections: [Int: String] = Dictionary(
            held.values.flatMap { (element: Element) -> [Fact] in element.facts[NSAccessibility.Attribute.sections.rawValue]?.list ?? [] }.compactMap { (section: Fact) -> (Int, String)? in
                let fields: [String: Fact] = section.record ?? [:]
                return fields[sectionObjectKey]?.element.flatMap { object in fields[sectionDescriptionKey]?.text.map { name in (object, name) } }
            }
        ) { first, _ in first }
        let windowRows: [CGWindowID: InstanceState.Window] = Dictionary(hierarchy.windows.map { row in (row.number, row) }) { first, _ in first }
        let operable: [Element] = grouped.filter { member in !titles.contains(member.id) }
        let own: [Int: Control.Kind] = Dictionary(
            uniqueKeysWithValues: operable.compactMap { member in Control.Kind(member, measured: measured.contains(member.id)).map { kind in (member.id, kind) } }
        )
        let items: [(Int, Control.Kind)] = operable.filter { member in member.role == kAXListRole && own[member.id] == .multi }.flatMap(\.children).map { item in (item, Control.Kind.row) }
        let classes: [Int: Control.Kind] = own.merging(items) { kind, _ in kind }
        let marked: Set<Int> = Set(operable.filter { member in member.text(kAXValueAttribute) != nil && !measured.contains(member.id) }.map(\.id))
        let tabs: [Int: Int] = Dictionary(
            operable.flatMap { (member: Element) -> [(Int, Int)] in
                let listed: [Element] = member.role == kAXTabGroupRole ? member.tabs(in: held) : []
                return listed.map { (tab: Element) -> (Int, Int) in (tab.id, member.id) }
            }
        ) { first, _ in first }
        let columns: [Int: [Element]] = Dictionary(grouping: operable.filter { member in member.role == kAXColumnRole }) { column in parents[column.id] ?? column.id }
        let headers: [Int: [Int: String]] = columns.mapValues { (columns: [Element]) -> [Int: String] in
            Dictionary(
                columns.compactMap { (column: Element) -> (Int, String)? in
                    column.facts[kAXIndexAttribute]?.integer.flatMap { index in
                        column.facts[kAXHeaderAttribute]?.element.flatMap { header in held[header]?.label }.map { label in (index, label.text) }
                    }
                }
            ) { first, _ in first }
        }
        let headed: [Int: String] = Dictionary(
            uniqueKeysWithValues: operable.compactMap { (cell: Element) -> (Int, String)? in
                guard cell.role == kAXCellRole, let index = cell.facts[kAXColumnIndexRangeAttribute]?.range?.location, let table = parents[cell.id].flatMap({ row in parents[row] }) else { return nil }
                return headers[table]?[index].map { header in (cell.id, header) }
            }
        )
        let placements: [(element: Element, parent: Int?, consumer: Int?, column: String?)] = operable.map { member in
            let inner: [Int] = lineage[member.id]?.inner ?? []
            let parent: Int? = zip([member.id] + inner, inner).lazy.compactMap { (below: Int, scope: Int) -> Int? in
                classes[scope] == .row ? scope : held[below]?.role == kAXMenuRole ? walked[below].flatMap { (opener: Int) -> Int? in classes[opener] == nil ? nil : opener } : nil
            }.first
            let consumer: Int? =
                inner.first { (scope: Int) -> Bool in
                    let role: String? = held[scope]?.role
                    return classes[scope] == .choice && role != kAXTabGroupRole || role == kAXScrollBarRole || scope == parent && classes[member.id] == nil
                } ?? tabs[member.id]
            return (element: member, parent: parent, consumer: consumer, column: inner.lazy.compactMap { scope in headed[scope] }.first)
        }
        let columned: [Int: String] = Dictionary(uniqueKeysWithValues: placements.compactMap { entry in entry.column.map { column in (entry.element.id, column) } })
        let enclosed: [(element: Element, consumer: Int)] = placements.compactMap { entry in entry.consumer.map { consumer in (element: entry.element, consumer: consumer) } }
        let inside: [Int: [Element]] = Dictionary(grouping: enclosed, by: \.consumer).mapValues { pairs in pairs.map(\.element) }
        let parented: [(element: Element, parent: Int)] = placements.compactMap { entry in entry.parent.map { parent in (element: entry.element, parent: parent) } }
        let beneath: [Int: [Element]] = Dictionary(grouping: parented, by: \.parent).mapValues { pairs in pairs.map(\.element) }
        let parentOf: [Int: Int] = Dictionary(uniqueKeysWithValues: parented.map { entry in (entry.element.id, entry.parent) })
        let consumed: Set<Int> = Set(enclosed.map(\.element.id))
        let classified: [(element: Element, kind: Control.Kind)] = operable.compactMap { member in
            consumed.contains(member.id) ? nil : classes[member.id].map { kind in (element: member, kind: kind) }
        }
        let taken: Set<Int> = consumed.union(classified.map(\.element.id))
        let candidates: [Element] = classified.filter { entry in entry.element.statesSelection || entry.kind == .action && entry.element.role == kAXButtonRole }.map(\.element)
        let sets: [[Element]] = Dictionary(grouping: candidates) { (element: Element) -> Int in parents[element.id] ?? element.id }.values.flatMap { (siblings: [Element]) -> [[Element]] in
            Dictionary(grouping: siblings) { (sibling: Element) -> String? in sibling.role }.values.filter { (set: [Element]) -> Bool in
                set.count > 1 && (set.first?.statesSelection == true || set.count(where: { (member: Element) -> Bool in marked.contains(member.id) }) == 1)
            }
        }
        let leaders: [Int: [Element]] = Dictionary(uniqueKeysWithValues: sets.compactMap { set in set.first.map { leader in (leader.id, set) } })
        let followers: Set<Int> = Set(sets.flatMap { set in set.dropFirst().map(\.id) })
        let entries: [(element: Element, kind: Control.Kind, set: [Element]?, label: Control.Label?)] = classified.compactMap { element, kind in
            guard !followers.contains(element.id) else { return nil }
            let caption: Control.Label? = element.caption(in: held)
            let fallback: [String] =
                switch (kind, element.role) {
                    case (.row, _), (.label, _): []
                    case (.multi, _), (.region, _): [kAXHelpAttribute]
                    case (.action, kAXStaticTextRole): [kAXValueAttribute, kAXHelpAttribute]
                    case (.action, _), (.toggle, _), (.choice, _), (.number, _), (.measure, _), (.text, _), (.color, _), (.date, _), (.item, _): [kAXHelpAttribute, kAXRoleDescriptionAttribute]
                }
            let cell: Control.Label? = ([element] + (beneath[element.id] ?? [])).lazy.compactMap { (shown: Element) -> Control.Label? in
                shown.label ?? shown.caption(in: held) ?? shown.labelled(kAXValueAttribute)
            }.first
            let named: Control.Label? = kind == .label ? nil : element.label ?? caption ?? fallback.lazy.compactMap(element.labelled).first
            let label: Control.Label? = kind == .row ? cell : named
            return leaders[element.id].map { set in (element: element, kind: .choice, set: set, label: caption) } ?? (element: element, kind: kind, set: nil, label: label)
        }
        let addressed: [(owner: Int, element: Element, kind: Control.Kind, set: [Element]?, label: Control.Label?, locator: Locator)] = Dictionary(grouping: entries) { entry in
            owners[entry.element.id] ?? entry.element.id
        }
        .flatMap { owner, group -> [(owner: Int, element: Element, kind: Control.Kind, set: [Element]?, label: Control.Label?, locator: Locator)] in
            let counts: [Locator.Step: Int] = Dictionary(
                group.compactMap { entry -> (Locator.Step, Int)? in
                    entry.label.flatMap { label in Locator.Step.Key.label(folding: label.text) }.map { key in (Locator.Step(role: entry.element.role ?? kAXUnknownRole, key: key), 1) }
                },
                uniquingKeysWith: +,
            )
            let named: [(element: Element, kind: Control.Kind, set: [Element]?, label: Control.Label?)] = group.map { element, kind, set, label in
                let key: Locator.Step.Key? = label.flatMap { label in .label(folding: label.text) }
                let shared: Bool = key.map { key in counts[Locator.Step(role: element.role ?? kAXUnknownRole, key: key), default: 0] > 1 } == true
                return (element: element, kind: kind, set: set, label: shared ? element.labelled(kAXHelpAttribute) ?? label : label)
            }
            let steps: [Locator.Step] = Locator.Step.distinguish(
                named.map { entry -> (role: String, keys: [Locator.Step.Key]) in
                    let keys: [Locator.Step.Key] = [entry.label.flatMap { label in Locator.Step.Key.label(folding: label.text)?.redacting(withheld) }].compactMap(\.self) + [.members]
                    return entry.set == nil ? entry.element.identify(by: entry.label, redacting: withheld) : (role: entry.element.role ?? kAXUnknownRole, keys: keys)
                }
            )
            return zip(named, steps).compactMap { entry, step in
                locators[owner].map { container in (owner: owner, element: entry.element, kind: entry.kind, set: entry.set, label: entry.label, locator: container.appending(step)) }
            }
        }
        let refs: [Int: Int] = Dictionary(
            scopes.map { scope in (scope.element.id, intern(scope.locator)) } + addressed.map { entry in (entry.element.id, intern(entry.locator)) }
        ) { first, _ in first }
        let separators: Set<Int> = Set(
            operable.filter { (member: Element) -> Bool in member.role == kAXMenuItemRole && member.label == nil && member.children.isEmpty && member.flag(kAXEnabledAttribute) == false }.map(\.id)
        )
        let separated: Set<Int> = Set(
            Set(separators.compactMap { separator in parents[separator] }).compactMap { parent in held[parent]?.children }.flatMap { (siblings: [Int]) -> [Int] in
                zip(siblings, siblings.dropFirst()).compactMap { (before: Int, after: Int) -> Int? in separators.contains(before) ? after : nil }
            }
        )
        let masks: [(mask: AXMenuItemModifiers, modifier: Modifier)] = [(.shift, .shift), (.option, .option), (.control, .control), (functionModifier, .function)]
        let built: [(owner: Int, control: Control, located: [(ref: Int, element: Int)])] = addressed.map { owner, element, kind, set, label, locator in
            let menus: [Element] = element.children.compactMap { child in held[child] }
            let opened: (ref: Int, element: Int)? = zip(
                menus,
                Locator.Step.distinguish(menus.map { (menu: Element) -> (role: String, keys: [Locator.Step.Key]) in menu.identify(by: menu.label, redacting: withheld) }),
            )
            .first { (menu: Element, _: Locator.Step) -> Bool in menu.role == kAXMenuRole }.map { (menu: Element, keyed: Locator.Step) -> (ref: Int, element: Int) in
                (ref: intern(locator.appending(keyed)), element: menu.id)
            }
            let opens: Int? = opened?.ref
            let disclosable: Bool = beneath[element.id]?.contains { cell in cell.role == kAXDisclosureTriangleRole } == true
            let role: String = element.role ?? kAXUnknownRole
            let opensMenu: Bool = opens != nil || role == kAXPopUpButtonRole || role == kAXComboBoxRole || role == kAXMenuButtonRole
            let controlsOther: Bool = element.facts[kAXLinkedUIElementsAttribute]?.elements.isEmpty == false
            let expandable: Bool = opensMenu || controlsOther || kind == .row || role == kAXDisclosureTriangleRole
            let disclosing: Bool? = kind == .row ? element.flag(kAXDisclosingAttribute).flatMap { (open: Bool) -> Bool? in open || disclosable ? open : nil } : nil
            let listed: [Element] =
                switch (set, element.role) {
                    case (.some(let members), _): members
                    case (.none, kAXTabGroupRole): element.tabs(in: held)
                    case (.none, kAXRadioGroupRole): element.children.compactMap { child in held[child] }
                    case (.none, kAXPopUpButtonRole), (.none, kAXComboBoxRole):
                        (inside[element.id] ?? []).filter { option in option.children.isEmpty && option.role != kAXButtonRole && option.optionText != nil }
                    case (.none, _): []
                }
            let optionRefs: [Int] = Locator.Step.distinguish(
                listed.map { option in (role: option.role ?? kAXUnknownRole, keys: [option.optionText.flatMap(Locator.Step.Key.label(folding:))?.redacting(withheld)].compactMap(\.self)) }
            ).map { step in intern(locator.appending(step)) }
            let area: Element? = parents[element.id].flatMap { parent in held[parent] }.flatMap { parent in parent.role == kAXScrollAreaRole ? parent : nil }
            let room: (content: CGSize, shown: CGSize)? = area.flatMap { (area: Element) -> (content: CGSize, shown: CGSize)? in
                guard let content = area.facts[NSAccessibility.Attribute.contentSize.rawValue]?.size, let shown = area.frame?.size else { return nil }
                return (content, shown)
            }
            let bars: [Double?] = [(kAXHorizontalScrollBarAttribute, \CGSize.width), (kAXVerticalScrollBarAttribute, \CGSize.height)].map {
                (attribute: String, axis: KeyPath<CGSize, CGFloat>) -> Double? in
                guard room.map({ (room: (content: CGSize, shown: CGSize)) -> Bool in room.content[keyPath: axis] > room.shown[keyPath: axis] }) != false else { return nil }
                return area?.facts[attribute]?.element.flatMap { (bar: Int) -> Double? in held[bar]?.facts[kAXValueAttribute]?.number }
            }
            let universal: [String] = [kAXRaiseAction, NSAccessibility.Action.scrollToVisibleAction.rawValue]
            let unstated: Set<String> = kind.defaultActions.union(opensMenu ? universal : universal + [kAXShowMenuAction])
            let stated: [String] = element.actions.filter { (action: String) -> Bool in !unstated.contains(action) }
            let actions: [String] = stated.map { (action: String) -> String in element.descriptions[action] ?? action }
            let ranges: [Fact] = element.facts[kAXSelectedTextRangesAttribute]?.list ?? element.facts[kAXSelectedTextRangeAttribute].map { (range: Fact) -> [Fact] in [range] } ?? []
            let characters: Int = element.facts[kAXNumberOfCharactersAttribute]?.integer ?? Int.max
            let textRanges: [Control.TextRange] = ranges.compactMap { (fact: Fact) -> Control.TextRange? in
                fact.range.flatMap { (range: (location: Int, length: Int)) -> Control.TextRange? in
                    let stated: Bool = range.location >= 0 && range.location != NSNotFound && range.length >= 0 && range.length <= characters - range.location
                    return stated ? Control.TextRange(location: range.location, length: range.length) : nil
                }
            }
            let rowCount: Int? = element.facts[kAXARIARowCountAttribute]?.integer.flatMap { (count: Int) -> Int? in count >= 0 ? count : nil }
            let options: [Control.Option] = zip(listed, optionRefs).compactMap { option, ref in
                option.optionText.map { text in Control.Option(label: text, ref: ref, help: option.text(kAXHelpAttribute)) }
            }
            let selection: Element? = listed.first { (option: Element) -> Bool in
                option.statesSelection ? option.facts[kAXValueAttribute]?.number == 1 || option.flag(kAXSelectedAttribute) == true : marked.contains(option.id)
            }
            let chosen: Element? =
                switch (set, element.role) {
                    case (.none, kAXTabGroupRole): element.facts[kAXValueAttribute]?.element.flatMap { tab in held[tab] } ?? selection
                    case (.some, _), (.none, kAXRadioGroupRole): selection
                    case (.none, _): nil
                }
            let read: Fact? = element.facts[kAXValueAttribute].flatMap { fact in fact.elements.isEmpty ? fact : nil }
            let value: Fact? =
                switch (kind, chosen) {
                    case (.choice, .some(let option)): option.optionText.map(Fact.text(value:))
                    case (.text, _) where element.text(kAXSubroleAttribute) == kAXSecureTextFieldSubrole: nil
                    case (.toggle, _), (.choice, _), (.number, _), (.measure, _), (.color, _), (.date, _), (.text, _), (.label, _): read
                    case (.row, _), (.item, _): element.facts[kAXURLAttribute]
                    case (.action, _), (.multi, _), (.region, _): nil
                }
            let selected: Control.Evidence? =
                switch (kind, set?.first?.statesSelection) {
                    case (.choice, false): .marked
                    case (.choice, _) where value != nil: .attribute
                    case (.row, _) where element.flag(kAXSelectedAttribute) == true: .attribute
                    case (_, _): nil
                }
            let range = Control.Range(
                minimum: element.facts[kAXMinValueAttribute]?.number,
                maximum: element.facts[kAXMaxValueAttribute]?.number,
                step: element.facts[kAXValueIncrementAttribute]?.number,
                wraps: element.flag(kAXValueWrapsAttribute),
            )
            let mask = AXMenuItemModifiers(rawValue: UInt32(clamping: element.facts[kAXMenuItemCmdModifiersAttribute]?.integer ?? 0))
            let key: Key? =
                element.facts[kAXMenuItemCmdGlyphAttribute]?.integer.flatMap(Key.Special.init(glyph:)).map(Key.special)
                ?? element.facts[kAXMenuItemCmdVirtualKeyAttribute]?.integer.flatMap { code in CGKeyCode(exactly: code) }.map(Key.code)
                ?? element.text(kAXMenuItemCmdCharAttribute).flatMap(\.first).map(Key.character)
            let modifiers: [Modifier] = (mask.contains(.noCommand) ? [] : [.command]) + masks.filter { pair in mask.contains(pair.mask) }.map(\.modifier)
            let frames: [CGRect] = (set ?? [element]).compactMap { (shown: Element) -> CGRect? in
                let ancestors: [Int] = Array(Swift.sequence(first: shown.id) { (id: Int) -> Int? in parents[id] }.dropFirst().prefix(held.count))
                let areas: [CGRect] = ancestors.compactMap { (id: Int) -> CGRect? in held[id].flatMap { (scope: Element) -> CGRect? in scope.role == kAXScrollAreaRole ? scope.frame : nil } }
                let visible: CGRect? = shown.frame.map { (frame: CGRect) -> CGRect in areas.reduce(frame) { (cut: CGRect, area: CGRect) -> CGRect in cut.intersection(area) } }
                return visible.flatMap { (frame: CGRect) -> CGRect? in frame.isEmpty ? nil : frame }
            }
            let inputs: [String] = element.inputLabels.filter { text in text != label?.text }
            let primary: Int? = element.facts[kAXMenuItemPrimaryUIElementAttribute]?.element.flatMap { (item: Int) -> Int? in item == element.id ? nil : refs[item] }
            let control = Control(
                ref: intern(locator),
                locator: locator,
                kind: kind,
                role: element.role ?? kAXUnknownRole,
                subrole: element.text(kAXSubroleAttribute),
                label: label,
                labels: inputs.isEmpty ? nil : inputs,
                section: Swift.sequence(first: element.id) { child in parents[child] }.prefix(held.count).prefix { scope in scope != parentOf[element.id] }.lazy.compactMap { scope in sections[scope] }
                    .first,
                value: value,
                settable: element.settable[kAXValueAttribute],
                enabled: element.flag(kAXEnabledAttribute),
                focused: element.flag(kAXFocusedAttribute) == true || hierarchy.focused == element.id ? true : nil,
                selected: selected,
                help: set == nil ? element.text(kAXHelpAttribute) : nil,
                frame: frames.first.map { first in frames.dropFirst().reduce(first) { union, frame in union.union(frame) } },
                positional: locator.positional,
                options: options.isEmpty ? nil : options,
                range: range.minimum == nil && range.maximum == nil && range.step == nil && range.wraps == nil ? nil : range,
                mark: kind == .item ? element.text(kAXMenuItemMarkCharAttribute) : nil,
                shortcut: kind == .item ? key.map { key in Control.Shortcut(key: key, modifiers: modifiers) } : nil,
                keyEquivalent: element.text(keyEquivalentAttribute),
                opens: opens,
                separated: separated.contains(element.id) ? true : nil,
                alternate: primary,
                placeholder: element.text(kAXPlaceholderValueAttribute),
                level: kind == .row ? element.facts[kAXDisclosureLevelAttribute]?.integer : nil,
                index: kind == .row ? element.facts[kAXIndexAttribute]?.integer : nil,
                parent: parentOf[element.id].flatMap { parent in refs[parent] },
                expanded: expandable ? element.flag(kAXExpandedAttribute) ?? disclosing : nil,
                actions: actions.isEmpty ? nil : actions,
                alternateUIVisible: element.actions.contains(kAXShowAlternateUIAction) ? element.flag(kAXAlternateUIVisibleAttribute) : nil,
                disclosedBy: element.facts[kAXDisclosedByRowAttribute]?.element.flatMap { row in refs[row] },
                total: kind == .multi ? rowCount ?? element.counts[kAXRowsAttribute] : nil,
                sort: Control.Sort(element.text(kAXSortDirectionAttribute)),
                column: columned[element.id],
                scroll: area.map { area in Control.Scroll(contentSize: area.facts[NSAccessibility.Attribute.contentSize.rawValue]?.size, horizontalScrollBar: bars[0], verticalScrollBar: bars[1]) },
                selectedTextRanges: textRanges.isEmpty ? nil : textRanges,
                numberOfCharacters: element.facts[kAXNumberOfCharactersAttribute]?.integer,
                insertionPointLineNumber: element.facts[kAXInsertionPointLineNumberAttribute]?.integer.flatMap { line in line == NSNotFound ? nil : line },
                runs: hierarchy.runs[element.id],
                invalid: element.text(kAXInvalidAttribute).flatMap(Control.Invalid.init(rawValue:)),
                required: element.flag(NSAccessibility.Attribute.required.rawValue),
                current: element.text(kAXARIACurrentAttribute).flatMap(Control.Current.init(rawValue:)),
            )
            return (
                owner: owner, control: control,
                located: [(ref: control.ref, element: element.id)] + zip(optionRefs, listed.map(\.id)).map { ref, member in (ref: ref, element: member) } + [opened].compactMap(\.self),
            )
        }
        let controls: [Int: [Control]] = Dictionary(grouping: built, by: \.owner).mapValues { entries in entries.map(\.control) }
        let assembled: [Container] = scopes.map { element, kind, locator in
            let inner: [Element] = members[element.id] ?? []
            let frame: CGRect? = element.frame.flatMap { frame in frame.isEmpty ? nil : frame }
            let closed: Bool = kind == .menu && frame == nil
            let chain: [Int] = Array(Swift.sequence(first: element.id) { (scope: Int) -> Int? in owners[scope] }.prefix(held.count))
            let owned: CGWindowID? = chain.lazy.compactMap { (scope: Int) -> CGWindowID? in held[scope]?.window }.first
            let menuRows: [InstanceState.Window] =
                frame.map { (frame: CGRect) -> [InstanceState.Window] in
                    hierarchy.windows.filter { (row: InstanceState.Window) -> Bool in row.onScreen && row.band == .popUpMenu && row.frame.contains(frame) }
                } ?? []
            let enclosing: InstanceState.Window? = menuRows.min { (first: InstanceState.Window, second: InstanceState.Window) -> Bool in
                first.frame.width * first.frame.height < second.frame.width * second.frame.height
            }
            let window: CGWindowID? = owned ?? enclosing?.number
            let row: InstanceState.Window? = window.flatMap { number in windowRows[number] }
            let title: String? = element.label?.text
            let modal: Bool = element.flag(kAXModalAttribute) == true
            let shownText: [String] = inner.filter { (member: Element) -> Bool in member.role == kAXStaticTextRole && !titles.contains(member.id) && !taken.contains(member.id) }
                .compactMap { (member: Element) -> String? in
                    member.text(kAXValueAttribute).flatMap { (line: String) -> String? in
                        line == title || line.unicodeScalars.allSatisfy { scalar in scalar.properties.isWhitespace || scalar.properties.generalCategory == .privateUse } ? nil : line
                    }
                }
            let cut: Bool = truncated.contains(element.id) || inner.contains { (member: Element) -> Bool in truncated.contains(member.id) }
            let edited: Bool? = element.facts[kAXCloseButtonAttribute]?.element.flatMap { (button: Int) -> Bool? in held[button]?.flag(kAXEditedAttribute) }
            return Container(
                ref: intern(locator),
                locator: locator,
                kind: kind,
                title: title,
                help: element.text(kAXHelpAttribute),
                document: element.text(kAXDocumentAttribute).flatMap(URL.init(string:)),
                edited: edited,
                fullScreen: element.flag(NSAccessibility.Attribute.fullScreen.rawValue),
                loaded: element.flag(kAXLoadedAttribute),
                loadingProgress: element.facts[kAXLoadingProgressAttribute]?.number,
                window: window,
                frame: frame,
                layer: row?.layer,
                closed: closed,
                modal: modal,
                asking: modal || kind.asking(modal: modal) || row?.remote != nil,
                focused: element.flag(kAXFocusedAttribute) ?? element.flag(kAXMainAttribute),
                parent: owners[element.id].flatMap { owner in refs[owner] },
                default: element.facts[kAXDefaultButtonAttribute]?.element.flatMap { button in refs[button] },
                cancel: element.facts[kAXCancelButtonAttribute]?.element.flatMap { button in refs[button] },
                close: element.facts[kAXCloseButtonAttribute]?.element.flatMap { button in refs[button] },
                text: shownText,
                truncated: cut,
                unreadable: regionHeads.contains(element.id),
                controls: controls[element.id] ?? [],
            )
        }
        let shownElements: [Element] = regions.flatMap { (region: (head: Element, members: [Element], hosted: Bool)) -> [Element] in
            region.hosted ? region.members : [region.head] + region.members
        }
        let shown: Set<Int> = Set(shownElements.map(\.id))
        let located: [(ref: Int, element: Int)] = scopes.map { scope in (ref: intern(scope.locator), element: scope.element.id) } + built.flatMap(\.located)
        self.init(
            sequence: sequence,
            containers: assembled,
            located: Dictionary(located.filter { entry in !shown.contains(entry.element) }.map { entry in (entry.ref, entry.element) }) { first, _ in first },
            drawn: Dictionary(
                located.compactMap { entry -> (Int, (window: CGWindowID, frame: CGRect))? in
                    guard shown.contains(entry.element), let element = held[entry.element], let window = element.window, let frame = element.frame else { return nil }
                    return (entry.ref, (window: window, frame: frame))
                }
            ) { first, _ in first },
        )
    }
}
