public import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
public import CoreGraphics
public import Foundation
import OrderedCollections
import RegexBuilder

// --- [CONSTANTS] -----------------------------------------------------------------------

private let recognizedAttribute: String = "recognized"
private let keyEquivalentAttribute: String = "keyEquivalent"
private let sectionFields: (object: String, description: String) = ("SectionObject", "SectionDescription")
private let pressActions: Set<String> = [kAXPressAction, kAXPickAction, kAXConfirmAction]

// --- [MODELS] --------------------------------------------------------------------------

public struct Hierarchy: Sendable {
    public let elements: [Element]
    public let roots: [Int]
    public let windows: [InstanceState.Window]
    let documents: [URL]
    public let regular: Bool
    public let focused: Int?
    public let truncated: [Int]
    public let extras: Int?
    var recognized: [CGWindowID: [RecognizedLine]] = [:]
    var runs: [Int: [Control.Run]] = [:]

    var undescribed: [InstanceState.Window] {
        let described: Set<CGWindowID> = Set(elements.compactMap(\.window))
        return windows.filter { row in
            row.onScreen && row.container == nil && !described.contains(row.number) && (row.layer < InstanceState.Window.Band.mainMenu.level || row.band == .popUpMenu)
        }
    }

    var flyouts: [InstanceState.Window] { undescribed.filter { row in row.band == .modalPanel || row.band == .popUpMenu } }
}

public struct Control: Encodable, Equatable, Sendable {
    public enum Kind: String, Codable, Sendable {
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

        public var valued: Bool {
            switch self {
                case .toggle, .choice, .number, .measure, .color, .date: true
                case .action, .multi, .row, .text, .item, .region: false
            }
        }

        public var holdsValueAttribute: Bool {
            switch self {
                case .toggle, .number, .measure, .text, .color, .date: true
                case .action, .choice, .multi, .row, .item, .region: false
            }
        }

        init?(_ element: Element, measured: Bool) {
            let value: Fact? = element.facts[kAXValueAttribute]
            let bare: Bool = element.children.isEmpty && value == nil && element.label == nil && element.frame != nil && element.actions.allSatisfy { action in action == kAXShowMenuAction }
            switch (element.role ?? kAXUnknownRole, element.text(kAXSubroleAttribute)) {
                case (kAXMenuItemRole, _):
                    guard element.label != nil else { return nil }
                    self = .item
                case (kAXMenuBarItemRole, _): self = .item
                case (kAXTextFieldRole, _) where measured, (kAXButtonRole, _) where measured: self = .measure
                case (_, kAXSwitchSubrole), (_, kAXToggleSubrole), (kAXCheckBoxRole, _), (kAXRadioButtonRole, _), (kAXDisclosureTriangleRole, _): self = .toggle
                case (_, kAXSearchFieldSubrole), (_, kAXSecureTextFieldSubrole), (kAXTextFieldRole, _), (kAXTextAreaRole, _): self = .text
                case (kAXPopUpButtonRole, _), (kAXComboBoxRole, _), (kAXRadioGroupRole, _), (kAXTabGroupRole, _): self = .choice
                case (kAXTableRole, _), (kAXOutlineRole, _), (kAXListRole, _), (kAXBrowserRole, _), (kAXGridRole, _): self = .multi
                case (kAXRowRole, _): self = .row
                case (kAXSliderRole, _), (kAXIncrementorRole, _), (kAXValueIndicatorRole, _), (kAXLevelIndicatorRole, _), (kAXProgressIndicatorRole, _), (kAXBusyIndicatorRole, _),
                    (kAXRelevanceIndicatorRole, _), (kAXSplitterRole, _):
                    self = .number
                case (kAXGroupRole, _) where value?.number != nil, (kAXUnknownRole, _) where value?.number != nil: self = .number
                case (kAXColorWellRole, _): self = .color
                case (kAXDateFieldRole, _), (kAXTimeFieldRole, _), (NSAccessibility.Role.dateTimeAreaRole.rawValue, _): self = .date
                case (_, _) where value?.date != nil: self = .date
                case (_, _) where element.actions.contains(kAXIncrementAction): self = .number
                case (kAXMenuButtonRole, _),
                    (_, _) where element.actions.contains(where: pressActions.contains):
                    self = .action
                case (kAXUnknownRole, _) where bare: self = .region
                case (_, _): return nil
            }
        }
    }

    public enum Evidence: String, Codable, Sendable {
        case attribute
        case marked
    }

    public struct Label: Encodable, Equatable, Sendable {
        public let text: String
        public let attribute: String
    }

    public struct Option: Encodable, Equatable, Sendable {
        public let label: String
        public let ref: Int
        public let help: String?
    }

    public struct Range: Encodable, Equatable, Sendable {
        public let minimum: Double?
        public let maximum: Double?
        public let step: Double?
        public let wraps: Bool?
    }

    public struct Shortcut: Encodable, Equatable, Sendable {
        public let key: Key
        public let modifiers: [Modifier]
    }

    public enum Invalid: String, Encodable, Sendable {
        case `true`
        case grammar
        case spelling
    }

    public enum Current: String, Encodable, Sendable {
        case `true`
        case page
        case step
        case location
        case date
        case time
    }

    public enum Sort: String, Encodable, Sendable {
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

    public struct Cell: Encodable, Equatable, Sendable {
        public let column: String?
        public let text: String
    }

    public struct TextRange: Encodable, Equatable, Sendable {
        public let location: Int
        public let length: Int
    }

    public struct Run: Encodable, Equatable, Sendable {
        public let range: TextRange
        public let text: String
        public let attributes: [String: Fact]
        public let frame: CGRect?
    }

    public struct Scroll: Encodable, Equatable, Sendable {
        public let contentSize: CGSize?
        public let horizontalScrollBar: Double?
        public let verticalScrollBar: Double?
    }

    public let ref: Int
    public let locator: Locator
    public let kind: Kind
    public let role: String
    public let subrole: String?
    public let label: Label?
    public let labels: [String]?
    public let section: String?
    public var value: Fact?
    public let enabled: Bool?
    public let focused: Bool?
    public let selected: Evidence?
    public let help: String?
    public let frame: CGRect?
    public let positional: Bool
    public let options: [Option]?
    public let range: Range?
    public let mark: String?
    public let shortcut: Shortcut?
    public let keyEquivalent: String?
    public let opens: Int?
    public let separated: Bool?
    public let alternate: Int?
    public let placeholder: String?
    public let level: Int?
    public let index: Int?
    public let row: Int?
    public let expanded: Bool?
    public let actions: [String]?
    public let alternateUIVisible: Bool?
    public let disclosedBy: Int?
    public let total: Int?
    public let sort: Sort?
    public let cells: [Cell]?
    public let column: String?
    public let scroll: Scroll?
    public let selectedTextRanges: [TextRange]?
    public let numberOfCharacters: Int?
    public let insertionPointLineNumber: Int?
    public let runs: [Run]?
    public let invalid: Invalid?
    public let required: Bool?
    public let current: Current?
}

public struct Container: Encodable, Equatable, Sendable {
    public enum Kind: String, Codable, Sendable {
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

        init?(_ element: Element, root: Bool, in hierarchy: Hierarchy) {
            switch (root, element.role ?? kAXUnknownRole, element.text(kAXSubroleAttribute), element.flag(kAXModalAttribute) == true) {
                case (true, kAXMenuBarRole, _, _): self = hierarchy.extras == element.id ? .statusItem : .menuBar
                case (true, kAXMenuRole, _, _): self = .menu
                case (_, kAXSheetRole, _, _): self = .sheet
                case (_, kAXPopoverRole, _, _): self = .popover
                case (_, kAXDrawerRole, _, _): self = .drawer
                case (_, kAXToolbarRole, _, _): self = .toolbar
                case (_, NSAccessibility.Role.webAreaRole.rawValue, _, _): self = .web
                case (false, _, kAXApplicationDialogSubrole, _), (false, _, kAXApplicationAlertDialogSubrole, _): self = .dialog
                case (true, _, kAXDialogSubrole, _), (true, _, kAXSystemDialogSubrole, _), (true, _, _, true): self = hierarchy.regular ? .dialog : .prompt
                case (_, kAXLayoutAreaRole, _, _) where root || !element.children.isEmpty, (true, _, kAXFloatingWindowSubrole, _), (true, _, kAXSystemFloatingWindowSubrole, _): self = .panel
                case (true, _, _, _): self = .window
                case (false, _, _, _): return nil
            }
        }
    }

    public let ref: Int
    public let locator: Locator
    public let kind: Kind
    public let title: String?
    public let help: String?
    public let document: URL?
    public let edited: Bool?
    public let fullScreen: Bool?
    public let loaded: Bool?
    public let loadingProgress: Double?
    public let window: CGWindowID?
    public let frame: CGRect?
    public let layer: Int?
    public let modal: Bool
    public let asking: Bool
    public let focused: Bool?
    public let parent: Int?
    public let `default`: Int?
    public let cancel: Int?
    public let close: Int?
    public let text: [String]
    public let truncated: Bool
    public var controls: [Control]
}

public struct Model: Encodable, Sendable {
    public enum Scope: Decodable, Sendable {
        case application
        case container(ref: Int)
        case menu(ref: Int)
        case rows(ref: Int, from: Int, count: Int)
        case text(ref: Int)
        case search(text: String, key: NSAccessibilitySearchKey, limit: Int)
        case menus

        private enum CodingKeys: CodingKey {
            case application, container, menu, rows, text, search, menus
        }

        private enum Field: String, CodingKey {
            case tag = "_tag"
            case ref, from, count, text, key, limit
        }

        public init(from decoder: any Decoder) throws {
            let fields = try decoder.container(keyedBy: Field.self)
            let tag: String = try fields.decode(String.self, forKey: .tag)
            self =
                switch CodingKeys(stringValue: tag) {
                    case .application: .application
                    case .container: try .container(ref: fields.decode(Int.self, forKey: .ref))
                    case .menu: try .menu(ref: fields.decode(Int.self, forKey: .ref))
                    case .rows: try .rows(ref: fields.decode(Int.self, forKey: .ref), from: fields.decode(Int.self, forKey: .from), count: fields.decode(Int.self, forKey: .count))
                    case .text: try .text(ref: fields.decode(Int.self, forKey: .ref))
                    case .search:
                        try .search(
                            text: fields.decode(String.self, forKey: .text),
                            key: fields.decodeIfPresent(String.self, forKey: .key).map(NSAccessibilitySearchKey.init(rawValue:)) ?? .anyTypeSearchKey,
                            limit: fields.decode(Int.self, forKey: .limit),
                        )
                    case .menus: .menus
                    case nil: throw DecodingError.dataCorruptedError(forKey: .tag, in: fields, debugDescription: "\(tag) names no \(Self.self)")
                }
        }
    }

    public struct Known: Decodable, Sendable {
        public let build: String?
        public let containers: Set<String>
        public let labels: Set<String>
    }

    private enum CodingKeys: CodingKey {
        case sequence, containers, pointer
    }

    public let sequence: Int
    public let containers: [Container]
    let located: [Int: Int]
    var drawn: [Int: (window: CGWindowID, frame: CGRect)] = [:]
    public var pointer: Pointer?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Element {
    fileprivate var label: Control.Label? {
        [kAXTitleAttribute, kAXDescriptionAttribute, recognizedAttribute, userInputLabelsAttribute, kAXFilenameAttribute].lazy.compactMap { attribute in labelled(attribute) }.first
    }

    fileprivate var inputLabels: [String] { facts[userInputLabelsAttribute]?.list?.compactMap(\.text).filter { text in !text.isEmpty } ?? [] }

    var supplementary: (read: [String], counted: [String]) {
        switch (Control.Kind(self, measured: false), role, text(kAXSubroleAttribute)) {
            case (.number, _, _): ([kAXMinValueAttribute, kAXMaxValueAttribute, kAXValueIncrementAttribute, kAXValueWrapsAttribute], [])
            case (.multi, _, _): ([kAXVisibleRowsAttribute, kAXVisibleChildrenAttribute, kAXHeaderAttribute, kAXVisibleColumnsAttribute], [kAXRowsAttribute])
            case (.row, _, _):
                ([kAXDisclosingAttribute, kAXDisclosureLevelAttribute, kAXIndexAttribute, kAXURLAttribute, kAXFilenameAttribute, kAXDisclosedByRowAttribute, kAXAlternateUIVisibleAttribute], [])
            case (.text, _, _): ([kAXSelectedTextRangeAttribute, kAXSelectedTextRangesAttribute, kAXNumberOfCharactersAttribute, kAXInsertionPointLineNumberAttribute], [])
            case (.choice, kAXTabGroupRole, _): ([kAXTabsAttribute], [])
            case (_, kAXColumnRole, _): ([kAXHeaderAttribute, kAXIndexAttribute], [])
            case (_, kAXCellRole, _): ([kAXColumnIndexRangeAttribute], [])
            case (_, kAXScrollAreaRole, _): ([kAXVerticalScrollBarAttribute, kAXHorizontalScrollBarAttribute, contentSizeAttribute], [])
            case (_, NSAccessibility.Role.webAreaRole.rawValue, _): ([kAXLoadedAttribute, kAXLoadingProgressAttribute], [])
            case (_, _, kAXSortButtonSubrole): ([kAXSortDirectionAttribute], [])
            case (_, _, kAXCloseButtonSubrole): ([kAXEditedAttribute], [])
            case (_, _, _): ([], [])
        }
    }

    fileprivate var optionText: String? { label?.text ?? text(kAXValueAttribute) }

    fileprivate var statesSelection: Bool { role == kAXRadioButtonRole || text(kAXSubroleAttribute).map(["AXSegment", NSAccessibility.Subrole.tabButtonSubrole.rawValue].contains) == true }

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
    public init(_ hierarchy: Hierarchy, sequence: Int, under: [Int: Locator] = [:], locale: Locale = .autoupdatingCurrent, ref intern: (Locator) -> Int) {
        let flyouts: Set<CGWindowID> = Set(hierarchy.flyouts.map(\.number))
        let regions: [[Element]] = hierarchy.undescribed.map { (row: InstanceState.Window) -> [Element] in
            let id: Int = ~(Int(row.number) << 32)
            let read: [RecognizedLine] = (hierarchy.recognized[row.number] ?? []).sorted { first, second in (first.frame.minY, first.frame.minX) < (second.frame.minY, second.frame.minX) }
            let column: CGFloat? = read.map { (line: RecognizedLine) -> (shared: Int, edge: CGFloat) in
                (shared: read.count(where: { (other: RecognizedLine) -> Bool in abs(other.frame.minX - line.frame.minX) < line.frame.height }), edge: line.frame.minX)
            }.max { first, second in (first.shared, second.edge) < (second.shared, first.edge) }?.edge
            let lines: [(facts: [String: Fact], frame: CGRect, section: String?)] = read.reduce(
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
                member.section.map { section in Fact.record(fields: [sectionFields.object: .element(id: id - 1 - offset), sectionFields.description: .text(value: section)]) }
            }
            let window: [String: Fact?] = [
                kAXRoleAttribute: .text(value: menu ? kAXMenuRole : kAXWindowRole), kAXTitleAttribute: row.title.map(Fact.text(value:)),
                kAXChildrenAttribute: .list(values: members.indices.map { index in .element(id: id - 1 - index) }), sectionsAttribute: .list(values: sections),
            ]
            return ([(facts: window.compactMapValues(\.self), frame: row.frame)] + members.map { member in (facts: member.facts, frame: member.frame) }).enumerated().map { offset, entry in
                Element(id: id - offset, facts: entry.facts.merging([frameAttribute: .rect(value: entry.frame)]) { first, _ in first }, actions: [], window: row.number)
            }
        }
        let held: [Int: Element] = Dictionary((hierarchy.elements + regions.joined()).map { element in (element.id, element) }) { first, _ in first }
        let home: URL = .homeDirectory
        let documents: OrderedSet<URL> = OrderedSet(hierarchy.documents + held.values.compactMap { (element: Element) -> URL? in element.text(kAXDocumentAttribute).flatMap(URL.init(string:)) })
        let places: [(text: String, url: URL)] =
            documents.flatMap { (url: URL) -> [(text: String, url: URL)] in [(text: url.lastPathComponent, url: url), (text: url.deletingPathExtension().lastPathComponent, url: url)] }
            + [(text: home.lastPathComponent, url: home), (text: NSUserName(), url: home)]
        let withheld: [(name: String, url: URL)] = places.compactMap { (entry: (text: String, url: URL)) -> (name: String, url: URL)? in
            Locator.Step.Key.label(folding: entry.text)?[case: \.label].map { name in (name: name, url: entry.url) }
        }.sorted { first, second in first.name.count > second.name.count }
        let roots: OrderedSet<Int> = OrderedSet(hierarchy.roots.filter { root in held[root] != nil } + regions.compactMap(\.first?.id))
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
            let base: Locator = chain.first.flatMap { root in under[root] } ?? Locator(steps: [])
            return (element, kind, chain.compactMap { scope in placed[scope] }.reduce(base) { locator, step in locator.appending(step) })
        }
        let locators: [Int: Locator] = Dictionary(uniqueKeysWithValues: scopes.map { scope in (scope.element.id, scope.locator) })
        let unit = Regex {
            ZeroOrMore(.whitespace)
            FloatingPointFormatStyle<Double>(locale: locale)
            ZeroOrMore(.whitespace)
            CharacterClass(.digit, .whitespace, .anyOf(".,+-")).inverted
        }
        let measured: Set<Int> = Set(hierarchy.elements.compactMap { element in element.text(kAXValueAttribute)?.prefixMatch(of: unit).map { _ in element.id } })
        let titles: Set<Int> = Set(hierarchy.elements.compactMap { element in element.facts[kAXTitleUIElementAttribute]?.element })
        let truncated: Set<Int> = Set(hierarchy.truncated)
        let sections: [Int: String] = Dictionary(
            held.values.flatMap { (element: Element) -> [Fact] in element.facts[sectionsAttribute]?.list ?? [] }.compactMap { (section: Fact) -> (Int, String)? in
                let fields: [String: Fact] = section.record ?? [:]
                return fields[sectionFields.object]?.element.flatMap { object in fields[sectionFields.description]?.text.map { name in (object, name) } }
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
            operable.flatMap { (member: Element) -> [(Int, Int)] in member.role == kAXTabGroupRole ? (member.facts[kAXTabsAttribute]?.elements ?? []).map { tab in (tab, member.id) } : [] }
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
        let placements: [(element: Element, row: Int?, consumer: Int?, column: String?)] = operable.map { member in
            let inner: [Int] = lineage[member.id]?.inner ?? []
            let row: Int? = inner.first { scope in classes[scope] == .row }
            let consumer: Int? =
                inner.first { (scope: Int) -> Bool in
                    let role: String? = held[scope]?.role
                    return classes[scope] == .choice && role != kAXTabGroupRole || role == kAXScrollBarRole || scope == row && classes[member.id] == nil
                } ?? tabs[member.id]
            return (element: member, row: row, consumer: consumer, column: inner.lazy.compactMap { scope in headed[scope] }.first)
        }
        let columned: [Int: String] = Dictionary(uniqueKeysWithValues: placements.compactMap { entry in entry.column.map { column in (entry.element.id, column) } })
        let enclosed: [(element: Element, consumer: Int)] = placements.compactMap { entry in entry.consumer.map { consumer in (element: entry.element, consumer: consumer) } }
        let inside: [Int: [Element]] = Dictionary(grouping: enclosed, by: \.consumer).mapValues { pairs in pairs.map(\.element) }
        let rowed: [(element: Element, row: Int)] = placements.compactMap { entry in entry.row.map { row in (element: entry.element, row: row) } }
        let cells: [Int: [Element]] = Dictionary(grouping: rowed, by: \.row).mapValues { pairs in pairs.map(\.element) }
        let rowOf: [Int: Int] = Dictionary(uniqueKeysWithValues: rowed.map { entry in (entry.element.id, entry.row) })
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
                    case (.row, _): []
                    case (.multi, _), (.region, _): [kAXHelpAttribute]
                    case (.action, kAXStaticTextRole): [kAXValueAttribute, kAXHelpAttribute]
                    case (.action, _), (.toggle, _), (.choice, _), (.number, _), (.measure, _), (.text, _), (.color, _), (.date, _), (.item, _): [kAXHelpAttribute, kAXRoleDescriptionAttribute]
                }
            let cell: Control.Label? = ([element] + (cells[element.id] ?? [])).lazy.compactMap { (shown: Element) -> Control.Label? in
                shown.label ?? shown.caption(in: held) ?? shown.labelled(kAXValueAttribute)
            }.first
            let label: Control.Label? = kind == .row ? cell : element.label ?? caption ?? fallback.lazy.compactMap(element.labelled).first
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
        let alternates: [Int: Int] = Dictionary(
            zip(addressed, addressed.dropFirst()).compactMap { previous, entry -> (Int, Int)? in
                guard entry.kind == .item, previous.kind == .item, previous.owner == entry.owner, let frame = entry.element.frame, !frame.isEmpty, frame == previous.element.frame else { return nil }
                return refs[previous.element.id].map { ref in (entry.element.id, ref) }
            }
        ) { first, _ in first }
        let masks: [(mask: AXMenuItemModifiers, modifier: Modifier)] = [(.shift, .shift), (.option, .option), (.control, .control), (AXMenuItemModifiers(rawValue: 1 << 4), .function)]
        let built: [(owner: Int, control: Control, located: [(ref: Int, element: Int)])] = addressed.map { owner, element, kind, set, label, locator in
            let menus: [Element] = element.children.compactMap { child in held[child] }
            let opens: Int? = zip(menus, Locator.Step.distinguish(menus.map { (menu: Element) -> (role: String, keys: [Locator.Step.Key]) in menu.identify(by: menu.label, redacting: withheld) }))
                .first { (menu: Element, _: Locator.Step) -> Bool in menu.role == kAXMenuRole }.map { (_: Element, keyed: Locator.Step) -> Int in intern(locator.appending(keyed)) }
            let disclosable: Bool = cells[element.id]?.contains { cell in cell.role == kAXDisclosureTriangleRole } == true
            let listed: [Element] =
                switch (set, element.role) {
                    case (.some(let members), _): members
                    case (.none, kAXTabGroupRole): (element.facts[kAXTabsAttribute]?.elements ?? []).compactMap { tab in held[tab] }
                    case (.none, kAXRadioGroupRole): element.children.compactMap { child in held[child] }
                    case (.none, kAXPopUpButtonRole), (.none, kAXComboBoxRole):
                        (inside[element.id] ?? []).filter { option in option.children.isEmpty && option.role != kAXButtonRole && option.optionText != nil }
                    case (.none, _): []
                }
            let optionRefs: [Int] = Locator.Step.distinguish(
                listed.map { option in (role: option.role ?? kAXUnknownRole, keys: [option.optionText.flatMap(Locator.Step.Key.label(folding:))?.redacting(withheld)].compactMap(\.self)) }
            ).map { step in intern(locator.appending(step)) }
            let shownCells: [Control.Cell] = (cells[element.id] ?? []).filter { cell in cell.role == kAXStaticTextRole }.compactMap { cell in
                cell.text(kAXValueAttribute).map { text in Control.Cell(column: columned[cell.id], text: text) }
            }
            let area: Element? = parents[element.id].flatMap { parent in held[parent] }.flatMap { parent in parent.role == kAXScrollAreaRole ? parent : nil }
            let bars: [Double?] = [kAXHorizontalScrollBarAttribute, kAXVerticalScrollBarAttribute].map { attribute in
                area?.facts[attribute]?.element.flatMap { bar in held[bar]?.facts[kAXValueAttribute]?.number }
            }
            let performed: Set<String> = pressActions.union([kAXRaiseAction, NSAccessibility.Action.scrollToVisibleAction.rawValue] + (kind.valued ? [kAXIncrementAction, kAXDecrementAction] : []))
            let actions: [String] = element.actions.filter { action in !performed.contains(action) }.map { action in element.descriptions[action] ?? action }
            let ranges: [Fact]? = element.facts[kAXSelectedTextRangesAttribute]?.list ?? element.facts[kAXSelectedTextRangeAttribute].map { range in [range] }
            let options: [Control.Option] = zip(listed, optionRefs).compactMap { option, ref in
                option.optionText.map { text in Control.Option(label: text, ref: ref, help: option.text(kAXHelpAttribute)) }
            }
            let chosen: Element? =
                switch (set, element.role) {
                    case (.none, kAXTabGroupRole): element.facts[kAXValueAttribute]?.element.flatMap { tab in held[tab] }
                    case (.some, _), (.none, kAXRadioGroupRole):
                        listed.first { option in option.statesSelection ? option.facts[kAXValueAttribute]?.number == 1 || option.flag(kAXSelectedAttribute) == true : marked.contains(option.id) }
                    case (.none, _): nil
                }
            let read: Fact? = element.facts[kAXValueAttribute].flatMap { fact in fact.elements.isEmpty ? fact : nil }
            let value: Fact? =
                switch (kind, chosen) {
                    case (.choice, .some(let option)): option.optionText.map(Fact.text(value:))
                    case (.text, _) where element.text(kAXSubroleAttribute) == kAXSecureTextFieldSubrole: nil
                    case (.toggle, _), (.choice, _), (.number, _), (.measure, _), (.color, _), (.date, _), (.text, _): read
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
            let frames: [CGRect] = (set ?? [element]).compactMap(\.frame)
            let inputs: [String] = element.inputLabels.filter { text in text != label?.text }
            let control = Control(
                ref: intern(locator),
                locator: locator,
                kind: kind,
                role: element.role ?? kAXUnknownRole,
                subrole: element.text(kAXSubroleAttribute),
                label: label,
                labels: inputs.isEmpty ? nil : inputs,
                section: Swift.sequence(first: element.id) { child in parents[child] }.prefix(held.count).lazy.compactMap { scope in sections[scope] }.first,
                value: value,
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
                alternate: alternates[element.id],
                placeholder: element.text(kAXPlaceholderValueAttribute),
                level: kind == .row ? element.facts[kAXDisclosureLevelAttribute]?.integer : nil,
                index: kind == .row ? element.facts[kAXIndexAttribute]?.integer : nil,
                row: rowOf[element.id].flatMap { row in refs[row] },
                expanded: element.flag(kAXExpandedAttribute) ?? (kind == .row ? element.flag(kAXDisclosingAttribute).flatMap { open in open || disclosable ? open : nil } : nil),
                actions: actions.isEmpty ? nil : actions,
                alternateUIVisible: element.actions.contains(kAXShowAlternateUIAction) ? element.flag(kAXAlternateUIVisibleAttribute) : nil,
                disclosedBy: element.facts[kAXDisclosedByRowAttribute]?.element.flatMap { row in refs[row] },
                total: kind == .multi ? element.facts[kAXARIARowCountAttribute]?.integer ?? element.counts[kAXRowsAttribute] : nil,
                sort: Control.Sort(element.text(kAXSortDirectionAttribute)),
                cells: shownCells.contains { cell in cell.column != nil } ? shownCells : nil,
                column: columned[element.id],
                scroll: area.map { area in Control.Scroll(contentSize: area.facts[contentSizeAttribute]?.size, horizontalScrollBar: bars[0], verticalScrollBar: bars[1]) },
                selectedTextRanges: ranges?.compactMap { range in range.range.map { range in Control.TextRange(location: range.location, length: range.length) } },
                numberOfCharacters: element.facts[kAXNumberOfCharactersAttribute]?.integer,
                insertionPointLineNumber: element.facts[kAXInsertionPointLineNumberAttribute]?.integer.flatMap { line in line == NSNotFound ? nil : line },
                runs: hierarchy.runs[element.id],
                invalid: element.text(kAXInvalidAttribute).flatMap(Control.Invalid.init(rawValue:)),
                required: element.flag(NSAccessibility.Attribute.required.rawValue),
                current: element.text(kAXARIACurrentAttribute).flatMap(Control.Current.init(rawValue:)),
            )
            return (owner: owner, control: control, located: [(ref: control.ref, element: element.id)] + zip(optionRefs, listed.map(\.id)).map { ref, member in (ref: ref, element: member) })
        }
        let controls: [Int: [Control]] = Dictionary(grouping: built, by: \.owner).mapValues { entries in entries.map(\.control) }
        let assembled: [Container] = scopes.map { element, kind, locator in
            let inner: [Element] = members[element.id] ?? []
            let window: CGWindowID? = Swift.sequence(first: element.id) { scope in owners[scope] }.prefix(held.count).lazy.compactMap { scope in held[scope]?.window }.first
            let row: InstanceState.Window? = window.flatMap { number in windowRows[number] }
            let title: String? = element.label?.text
            let declared: Bool = element.flag(kAXModalAttribute) == true
            let modal: Bool = declared || kind != .menu && row?.band == .modalPanel
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
                fullScreen: element.flag(fullScreenAttribute),
                loaded: element.flag(kAXLoadedAttribute),
                loadingProgress: element.facts[kAXLoadingProgressAttribute]?.number,
                window: window,
                frame: element.frame,
                layer: row?.layer,
                modal: modal,
                asking: declared || kind.asking(modal: modal) || row?.remote != nil,
                focused: element.flag(kAXFocusedAttribute) ?? element.flag(kAXMainAttribute),
                parent: owners[element.id].flatMap { owner in refs[owner] },
                default: element.facts[kAXDefaultButtonAttribute]?.element.flatMap { button in refs[button] },
                cancel: element.facts[kAXCancelButtonAttribute]?.element.flatMap { button in refs[button] },
                close: element.facts[kAXCloseButtonAttribute]?.element.flatMap { button in refs[button] },
                text: shownText,
                truncated: cut,
                controls: controls[element.id] ?? [],
            )
        }
        let shown: Set<Int> = Set(regions.joined().map(\.id))
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
