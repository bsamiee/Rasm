public import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
public import CoreGraphics
public import Foundation
import OrderedCollections
import RegexBuilder

// --- [CONSTANTS] -----------------------------------------------------------------------

private let recognizedAttribute: String = "recognized"
private let pressActions: Set<String> = [kAXPressAction, kAXPickAction, kAXConfirmAction]

// --- [MODELS] --------------------------------------------------------------------------

public struct Hierarchy: Sendable {
    public let elements: [Element]
    public let roots: [Int]
    public let windows: [InstanceState.Window]
    public let regular: Bool
    public let focused: Int?
    public let truncated: [Int]
    public let extras: Int?
    var recognized: [CGWindowID: [(text: String, frame: CGRect)]] = [:]
    var runs: [Int: [Control.Run]] = [:]

    var undescribed: [InstanceState.Window] {
        let described: Set<CGWindowID> = Set(elements.compactMap(\.window))
        return windows.filter { row in row.onScreen && row.layer < Int(CGWindowLevelForKey(.mainMenuWindow)) && !described.contains(row.number) }
    }

    var flyouts: [InstanceState.Window] { undescribed.filter { row in row.layer == modalLayers.lowerBound } }
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
                case (kAXTableRole, _), (kAXOutlineRole, _), (kAXListRole, _), (kAXBrowserRole, _): self = .multi
                case (kAXRowRole, _): self = .row
                case (kAXSliderRole, _), (kAXIncrementorRole, _), (kAXValueIndicatorRole, _), (kAXLevelIndicatorRole, _), (kAXProgressIndicatorRole, _), (kAXBusyIndicatorRole, _),
                    (kAXRelevanceIndicatorRole, _):
                    self = .number
                case (kAXGroupRole, _) where value?.number != nil, (kAXUnknownRole, _) where value?.number != nil: self = .number
                case (kAXColorWellRole, _): self = .color
                case (kAXDateFieldRole, _), (kAXTimeFieldRole, _), (NSAccessibility.Role.dateTimeAreaRole.rawValue, _): self = .date
                case (_, _) where value?.date != nil: self = .date
                case (_, _) where element.actions.contains(where: pressActions.contains): self = .action
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
    public let value: Fact?
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
    public let controls: [Control]
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

        private enum Key: String, CodingKey {
            case tag = "_tag"
            case ref, from, count, text, key, limit
        }

        public init(from decoder: any Decoder) throws {
            let fields = try decoder.container(keyedBy: Key.self)
            self =
                switch try fields.decode(String.self, forKey: .tag) {
                    case "application": .application
                    case "container": try .container(ref: fields.decode(Int.self, forKey: .ref))
                    case "menu": try .menu(ref: fields.decode(Int.self, forKey: .ref))
                    case "rows": try .rows(ref: fields.decode(Int.self, forKey: .ref), from: fields.decode(Int.self, forKey: .from), count: fields.decode(Int.self, forKey: .count))
                    case "text": try .text(ref: fields.decode(Int.self, forKey: .ref))
                    case "search":
                        try .search(
                            text: fields.decode(String.self, forKey: .text),
                            key: NSAccessibilitySearchKey(rawValue: fields.decodeIfPresent(String.self, forKey: .key) ?? NSAccessibilitySearchKey.anyTypeSearchKey.rawValue),
                            limit: fields.decode(Int.self, forKey: .limit),
                        )
                    case "menus": .menus
                    case let tag: throw DecodingError.dataCorruptedError(forKey: .tag, in: fields, debugDescription: "\(tag) names no scope")
                }
        }
    }

    public struct Known: Decodable, Sendable {
        public let build: String
        public let containers: Set<String>
        public let customWords: [String]
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

    fileprivate func keyed(by label: Control.Label?) -> (role: String, keys: [Locator.Step.Key]) {
        let keys: [Locator.Step.Key?] = [
            (text(kAXIdentifierAttribute) ?? text(kAXDOMIdentifierAttribute)).map(Locator.Step.Key.identifier),
            text(kAXDocumentAttribute).flatMap(URL.init(string:)).map(Locator.Step.Key.document),
            label.flatMap { label in .label(folding: label.text) },
        ]
        return (role: role ?? kAXUnknownRole, keys: keys.compactMap(\.self) + [.sole])
    }
}

extension Model {
    public init(_ hierarchy: Hierarchy, sequence: Int, under: [Int: Locator] = [:], locale: Locale = .autoupdatingCurrent, ref intern: (Locator) -> Int) {
        let flyouts: Set<CGWindowID> = Set(hierarchy.flyouts.map(\.number))
        let regions: [[Element]] = hierarchy.undescribed.map { row in
            let id: Int = ~(Int(row.number) << 32)
            let found: [(text: String, frame: CGRect)]? = flyouts.contains(row.number) ? hierarchy.recognized[row.number] ?? [] : nil
            let members: [(facts: [String: Fact], frame: CGRect)] =
                found.map { found in found.map { item in ([kAXRoleAttribute: .text(value: kAXMenuItemRole), recognizedAttribute: .text(value: item.text)], item.frame) } } ?? [([:], row.frame)]
            let window: [String: Fact?] = [
                kAXRoleAttribute: .text(value: found == nil ? kAXWindowRole : kAXMenuRole), kAXTitleAttribute: row.title.map { title in .text(value: title) },
                kAXChildrenAttribute: .list(values: members.indices.map { index in .element(id: id - 1 - index) }),
            ]
            return ([(facts: window.compactMapValues(\.self), frame: row.frame)] + members).enumerated().map { offset, entry in
                Element(id: id - offset, facts: entry.facts.merging([frameAttribute: .rect(value: entry.frame)]) { first, _ in first }, actions: [], window: row.number)
            }
        }
        let held: [Int: Element] = Dictionary((hierarchy.elements + regions.joined()).map { element in (element.id, element) }) { first, _ in first }
        let roots: OrderedSet<Int> = OrderedSet(hierarchy.roots.filter { root in held[root] != nil } + regions.compactMap(\.first?.id))
        var (order, walked, pending): (OrderedSet<Int>, [Int: Int], [Int]) = ([], [:], Array(roots.reversed()))
        while let id = pending.popLast() {
            guard let element = held[id], order.append(id).inserted else { continue }
            let children: [Int] = element.children.filter { child in held[child] != nil && walked[child] == nil && !roots.contains(child) }
            walked.merge(children.map { child in (child, id) }) { first, _ in first }
            pending += children.reversed()
        }
        let parents: [Int: Int] = Dictionary(
            uniqueKeysWithValues: held.compactMap { id, element in (element.facts[kAXParentAttribute]?.element.flatMap { parent in held[parent]?.id } ?? walked[id]).map { parent in (id, parent) } }
        )
        let kinds: [Int: Container.Kind] = Dictionary(
            uniqueKeysWithValues: held.compactMap { id, element in Container.Kind(element, root: roots.contains(id), in: hierarchy).map { kind in (id, kind) } }
        )
        let owners: [Int: Int] = Dictionary(
            uniqueKeysWithValues: held.keys.compactMap { (id: Int) -> (Int, Int)? in
                let chain: some Sequence<Int> = Swift.sequence(first: id) { (child: Int) -> Int? in parents[child] }.dropFirst().prefix(held.count)
                return chain.first { (scope: Int) -> Bool in kinds[scope] != nil }.map { (owner: Int) -> (Int, Int) in (id, owner) }
            }
        )
        let grouped: [Element] = order.compactMap { (id: Int) -> Element? in kinds[id] == nil ? held[id] : nil }
        let members: [Int: [Element]] = Dictionary(grouping: grouped) { (element: Element) -> Int in owners[element.id] ?? element.id }
        let containers: [(element: Element, kind: Container.Kind)] = kinds.compactMap { id, kind -> (place: Int, element: Element, kind: Container.Kind)? in
            held[id].map { element in (place: order.firstIndex(of: id) ?? members[id]?.first.flatMap { member in order.firstIndex(of: member.id) } ?? order.count, element: element, kind: kind) }
        }
        .sorted { first, second in (first.place, first.element.id) < (second.place, second.element.id) }.map { entry in (element: entry.element, kind: entry.kind) }
        let siblings: [[Element]] = Array(Dictionary(grouping: containers.map(\.element)) { (element: Element) -> Int? in owners[element.id] }.values)
        let steps: [Int: Locator.Step] = Dictionary(
            uniqueKeysWithValues: siblings.flatMap { (group: [Element]) -> [(Int, Locator.Step)] in
                Array(zip(group.map(\.id), Locator.Step.distinguish(group.map { (sibling: Element) -> (role: String, keys: [Locator.Step.Key]) in sibling.keyed(by: sibling.label) })))
            }
        )
        let scopes: [(element: Element, kind: Container.Kind, locator: Locator)] = containers.map { element, kind in
            let chain: [Int] = Swift.sequence(first: element.id) { scope in owners[scope] }.prefix(held.count).reversed()
            let base: Locator = chain.first.flatMap { root in under[root] } ?? Locator(steps: [])
            return (element, kind, chain.compactMap { scope in steps[scope] }.reduce(base) { locator, step in locator.appending(step) })
        }
        let refs: [Int: Int] = Dictionary(uniqueKeysWithValues: scopes.map { scope in (scope.element.id, intern(scope.locator)) })
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
                guard let object = fields["SectionObject"]?.element, let name = fields["SectionDescription"]?.text else { return nil }
                return (object, name)
            }
        ) { first, _ in first }
        let rows: [CGWindowID: InstanceState.Window] = Dictionary(hierarchy.windows.map { row in (row.number, row) }) { first, _ in first }
        let built: [(container: Container, located: [(ref: Int, element: Int)])] = scopes.map { element, kind, locator in
            let inner: [Element] = members[element.id] ?? []
            let (controls, consumed): ([(control: Control, located: [(ref: Int, element: Int)])], Set<Int>) = Self.controls(
                of: inner.filter { member in !titles.contains(member.id) },
                in: locator,
                held: held,
                parents: parents,
                measured: measured,
                sections: sections,
                focused: hierarchy.focused,
                runs: hierarchy.runs,
                intern: intern,
            )
            let located: [(ref: Int, element: Int)] = [(ref: intern(locator), element: element.id)] + controls.flatMap(\.located)
            let answers: [Int: Int] = Dictionary(located.map { entry in (entry.element, entry.ref) }) { first, _ in first }
            let window: CGWindowID? = Swift.sequence(first: element.id) { scope in owners[scope] }.prefix(held.count).lazy.compactMap { scope in held[scope]?.window }.first
            let layer: Int? = window.flatMap { number in rows[number]?.layer }
            let title: String? = element.label?.text
            let modal: Bool = element.flag(kAXModalAttribute) == true || kind != .menu && layer.map(modalLayers.contains) == true
            let container = Container(
                ref: intern(locator),
                locator: locator,
                kind: kind,
                title: title,
                help: element.text(kAXHelpAttribute),
                document: element.text(kAXDocumentAttribute).flatMap(URL.init(string:)),
                edited: element.facts[kAXCloseButtonAttribute]?.element.flatMap { button in held[button]?.flag(kAXEditedAttribute) },
                fullScreen: element.flag(fullScreenAttribute),
                loaded: element.flag(kAXLoadedAttribute),
                loadingProgress: element.facts[kAXLoadingProgressAttribute]?.number,
                window: window,
                frame: element.frame,
                layer: layer,
                modal: modal,
                asking: kind.asking(modal: modal) || element.flag(kAXModalAttribute) == true,
                focused: element.flag(kAXFocusedAttribute) ?? element.flag(kAXMainAttribute),
                parent: owners[element.id].flatMap { owner in refs[owner] },
                default: element.facts[kAXDefaultButtonAttribute]?.element.flatMap { button in answers[button] },
                cancel: element.facts[kAXCancelButtonAttribute]?.element.flatMap { button in answers[button] },
                close: element.facts[kAXCloseButtonAttribute]?.element.flatMap { button in answers[button] },
                text: inner.filter { member in member.role == kAXStaticTextRole && !titles.contains(member.id) && !consumed.contains(member.id) }.compactMap { member in
                    member.text(kAXValueAttribute).flatMap { line in
                        line == title || line.unicodeScalars.allSatisfy { scalar in scalar.properties.isWhitespace || scalar.properties.generalCategory == .privateUse } ? nil : line
                    }
                },
                truncated: truncated.contains(element.id) || inner.contains { member in truncated.contains(member.id) },
                controls: controls.map(\.control),
            )
            return (container, located)
        }
        let shown: [Int: Element] = Dictionary(uniqueKeysWithValues: regions.joined().map { element in (element.id, element) })
        let entries: [(ref: Int, element: Int)] = built.flatMap(\.located)
        self.init(
            sequence: sequence,
            containers: built.map(\.container),
            located: Dictionary(entries.filter { entry in shown[entry.element] == nil }.map { entry in (entry.ref, entry.element) }) { first, _ in first },
            drawn: Dictionary(
                entries.compactMap { entry -> (Int, (window: CGWindowID, frame: CGRect))? in
                    guard let element = shown[entry.element], let window = element.window, let frame = element.frame else { return nil }
                    return (entry.ref, (window: window, frame: frame))
                }
            ) { first, _ in first },
        )
    }

    private static func controls(
        of members: [Element],
        in container: Locator,
        held: [Int: Element],
        parents: [Int: Int],
        measured: Set<Int>,
        sections: [Int: String],
        focused: Int?,
        runs: [Int: [Control.Run]],
        intern: (Locator) -> Int,
    ) -> (controls: [(control: Control, located: [(ref: Int, element: Int)])], consumed: Set<Int>) {
        let own: [Int: Control.Kind] = Dictionary(
            uniqueKeysWithValues: members.compactMap { member in Control.Kind(member, measured: measured.contains(member.id)).map { kind in (member.id, kind) } }
        )
        let items: [Int] = members.filter { member in member.role == kAXListRole && own[member.id] == .multi }.flatMap(\.children)
        let kinds: [Int: Control.Kind] = own.merging(items.map { item in (item, Control.Kind.row) }) { kind, _ in kind }
        let marked: Set<Int> = Set(members.filter { member in member.text(kAXValueAttribute) != nil && !measured.contains(member.id) }.map(\.id))
        let tabbed: [(tab: Int, group: Int)] = members.flatMap { (member: Element) -> [(tab: Int, group: Int)] in
            member.role == kAXTabGroupRole ? (member.facts[kAXTabsAttribute]?.elements ?? []).map { (tab: Int) -> (tab: Int, group: Int) in (tab: tab, group: member.id) } : []
        }
        let tabs: [Int: Int] = Dictionary(tabbed) { first, _ in first }
        let headers: [Int: [Int: Element]] = Dictionary(grouping: members.filter { member in member.role == kAXColumnRole }) { column in parents[column.id] ?? column.id }.mapValues { columns in
            Dictionary(
                columns.compactMap { (column: Element) -> (Int, Element)? in
                    guard let index = column.facts[kAXIndexAttribute]?.integer, let header = column.facts[kAXHeaderAttribute]?.element.flatMap({ header in held[header] }) else { return nil }
                    return (index, header)
                }
            ) { first, _ in first }
        }
        let headed: [Int: String] = Dictionary(
            uniqueKeysWithValues: members.compactMap { (cell: Element) -> (Int, String)? in
                guard cell.role == kAXCellRole, let index = cell.facts[kAXColumnIndexRangeAttribute]?.range?.location, let table = parents[cell.id].flatMap({ row in parents[row] }) else { return nil }
                return headers[table]?[index]?.label.map { label in (cell.id, label.text) }
            }
        )
        let lineage: [(element: Element, row: Int?, consumer: Int?, column: String?)] = members.map { member in
            let ancestors: [Int] = Array(Swift.sequence(first: member.id) { child in parents[child] }.dropFirst().prefix(held.count))
            let row: Int? = ancestors.first { scope in kinds[scope] == .row }
            let consumer: Int? =
                ancestors.first { (scope: Int) -> Bool in
                    let role: String? = held[scope]?.role
                    return kinds[scope] == .choice && role != kAXTabGroupRole || role == kAXScrollBarRole || scope == row && kinds[member.id] == nil
                } ?? tabs[member.id]
            return (element: member, row: row, consumer: consumer, column: ancestors.lazy.compactMap { scope in headed[scope] }.first)
        }
        let columned: [Int: String] = Dictionary(uniqueKeysWithValues: lineage.compactMap { entry in entry.column.map { column in (entry.element.id, column) } })
        let enclosed: [(element: Element, consumer: Int)] = lineage.compactMap { entry in entry.consumer.map { consumer in (element: entry.element, consumer: consumer) } }
        let inside: [Int: [Element]] = Dictionary(grouping: enclosed, by: \.consumer).mapValues { pairs in pairs.map(\.element) }
        let rowed: [(element: Element, row: Int)] = lineage.compactMap { entry in entry.row.map { row in (element: entry.element, row: row) } }
        let cells: [Int: [Element]] = Dictionary(grouping: rowed, by: \.row).mapValues { pairs in pairs.map(\.element) }
        let rows: [Int: Int] = Dictionary(uniqueKeysWithValues: rowed.map { entry in (entry.element.id, entry.row) })
        let consumed: Set<Int> = Set(enclosed.map(\.element.id))
        let classified: [(element: Element, kind: Control.Kind)] = members.compactMap { member in consumed.contains(member.id) ? nil : kinds[member.id].map { kind in (element: member, kind: kind) } }
        let stated: (Element) -> Bool = { member in
            member.role == kAXRadioButtonRole || member.text(kAXSubroleAttribute).map(["AXSegment", NSAccessibility.Subrole.tabButtonSubrole.rawValue].contains) == true
        }
        let candidates: [Element] = classified.filter { entry in stated(entry.element) || entry.kind == .action && entry.element.role == kAXButtonRole }.map(\.element)
        let groups: [[Element]] = Dictionary(grouping: candidates) { (element: Element) -> Int in parents[element.id] ?? element.id }.values.flatMap { (siblings: [Element]) -> [[Element]] in
            Array(Dictionary(grouping: siblings) { (sibling: Element) -> String? in sibling.role }.values)
        }
        let sets: [[Element]] = groups.filter { (set: [Element]) -> Bool in
            set.count > 1 && (set.first.map(stated) == true || set.count(where: { (member: Element) -> Bool in marked.contains(member.id) }) == 1)
        }
        let leaders: [Int: [Element]] = Dictionary(uniqueKeysWithValues: sets.compactMap { set in set.first.map { leader in (leader.id, set) } })
        let followers: Set<Int> = Set(sets.flatMap { set in set.dropFirst().map(\.id) })
        let entries: [(element: Element, kind: Control.Kind, set: [Element]?, label: Control.Label?)] = classified.compactMap { element, kind in
            let caption: Control.Label? = element.caption(in: held)
            let shown: [Element] = [element] + (cells[element.id] ?? [])
            let cell: Control.Label? = shown.lazy.compactMap { (cell: Element) -> Control.Label? in cell.label ?? cell.caption(in: held) ?? cell.labelled(kAXValueAttribute) }.first
            let described: Control.Label? =
                switch kind {
                    case .row, .multi, .region: nil
                    case .action, .toggle, .choice, .number, .measure, .text, .color, .date, .item: element.labelled(kAXRoleDescriptionAttribute)
                }
            let label: Control.Label? = kind == .row ? cell : element.label ?? caption ?? described ?? element.labelled(kAXHelpAttribute)
            return switch (followers.contains(element.id), leaders[element.id]) {
                case (true, _): nil
                case (false, .some(let set)): (element: element, kind: .choice, set: set, label: caption)
                case (false, .none): (element: element, kind: kind, set: nil, label: label)
            }
        }
        let classes: [Locator.Step: Int] = Dictionary(
            entries.compactMap { entry -> (Locator.Step, Int)? in
                entry.label.flatMap { (label: Control.Label) -> Locator.Step.Key? in .label(folding: label.text) }.map { (key: Locator.Step.Key) -> (Locator.Step, Int) in
                    (Locator.Step(role: entry.element.role ?? kAXUnknownRole, key: key), 1)
                }
            },
            uniquingKeysWith: +,
        )
        let named: [(element: Element, kind: Control.Kind, set: [Element]?, label: Control.Label?)] = entries.map { element, kind, set, label in
            let key: Locator.Step.Key? = label.flatMap { label in .label(folding: label.text) }
            let shared: Bool = key.map { key in classes[Locator.Step(role: element.role ?? kAXUnknownRole, key: key), default: 0] > 1 } == true
            return (element: element, kind: kind, set: set, label: shared ? element.labelled(kAXHelpAttribute) ?? label : label)
        }
        let steps: [Locator.Step] = Locator.Step.distinguish(
            named.map { entry -> (role: String, keys: [Locator.Step.Key]) in
                let members: [Locator.Step.Key] = [entry.label.flatMap { label in Locator.Step.Key.label(folding: label.text) }].compactMap(\.self) + [.members]
                return entry.set == nil ? entry.element.keyed(by: entry.label) : (role: entry.element.role ?? kAXUnknownRole, keys: members)
            }
        )
        let refs: [Int: Int] = Dictionary(zip(named, steps).map { entry, step in (entry.element.id, intern(container.appending(step))) }) { first, _ in first }
        let separators: Set<Int> = Set(
            members.filter { (member: Element) -> Bool in
                member.role == kAXMenuItemRole && member.text(kAXTitleAttribute) == nil && member.children.isEmpty && member.flag(kAXEnabledAttribute) == false
            }.map(\.id)
        )
        let separated: Set<Int> = Set(
            Set(separators.compactMap { separator in parents[separator] }).compactMap { parent in held[parent]?.children }.flatMap { (siblings: [Int]) -> [Int] in
                zip(siblings, siblings.dropFirst()).compactMap { (before: Int, after: Int) -> Int? in separators.contains(before) ? after : nil }
            }
        )
        let alternates: [Int: Int] = Dictionary(
            zip(named, named.dropFirst()).compactMap { previous, entry -> (Int, Int)? in
                guard entry.kind == .item, previous.kind == .item, let frame = entry.element.frame, !frame.isEmpty, frame == previous.element.frame else { return nil }
                return refs[previous.element.id].map { ref in (entry.element.id, ref) }
            }
        ) { first, _ in first }
        let controls: [(control: Control, located: [(ref: Int, element: Int)])] = zip(named, steps).map { entry, step in
            let (element, kind, set, label): (Element, Control.Kind, [Element]?, Control.Label?) = entry
            let locator: Locator = container.appending(step)
            let menus: [Element] = element.children.compactMap { child in held[child] }
            let opens: Int? = zip(menus, Locator.Step.distinguish(menus.map { (menu: Element) -> (role: String, keys: [Locator.Step.Key]) in menu.keyed(by: menu.label) }))
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
            let optionSteps: [Locator.Step] = Locator.Step.distinguish(
                listed.map { option in (role: option.role ?? kAXUnknownRole, keys: [option.optionText.flatMap(Locator.Step.Key.label(folding:))].compactMap(\.self)) }
            )
            let optionRefs: [Int] = optionSteps.map { step in intern(locator.appending(step)) }
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
                        listed.first { option in stated(option) ? option.facts[kAXValueAttribute]?.number == 1 || option.flag(kAXSelectedAttribute) == true : marked.contains(option.id) }
                    case (.none, _): nil
                }
            let value: Fact? =
                switch (kind, chosen) {
                    case (.choice, .some(let option)): option.optionText.map { text in .text(value: text) }
                    case (.text, _) where element.text(kAXSubroleAttribute) == kAXSecureTextFieldSubrole: nil
                    case (.toggle, _), (.choice, _), (.number, _), (.measure, _), (.color, _), (.date, _), (.text, _): element.facts[kAXValueAttribute]
                    case (.row, _), (.item, _): element.facts[kAXURLAttribute]
                    case (.action, _), (.multi, _), (.region, _): nil
                }
            let selected: Control.Evidence? =
                switch (kind, set?.first.map(stated)) {
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
                ?? element.text(kAXMenuItemCmdCharAttribute).flatMap { character in character.lowercased().first }.map(Key.character)
            let masks: [(mask: AXMenuItemModifiers, modifier: Modifier)] = [(.shift, .shift), (.option, .option), (.control, .control), (AXMenuItemModifiers(rawValue: 1 << 4), .function)]
            let modifiers: [Modifier] = (mask.contains(.noCommand) ? [] : [.command]) + masks.filter { pair in mask.contains(pair.mask) }.map(\.modifier)
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
                focused: element.flag(kAXFocusedAttribute) == true || focused == element.id ? true : nil,
                selected: selected,
                help: set == nil ? element.text(kAXHelpAttribute) : nil,
                frame: set.map { members in members.compactMap(\.frame).reduce(CGRect.null) { union, frame in union.union(frame) } } ?? element.frame,
                positional: locator.positional,
                options: options.isEmpty ? nil : options,
                range: range.minimum == nil && range.maximum == nil && range.step == nil && range.wraps == nil ? nil : range,
                mark: kind == .item ? element.text(kAXMenuItemMarkCharAttribute) : nil,
                shortcut: kind == .item ? key.map { key in Control.Shortcut(key: key, modifiers: modifiers) } : nil,
                opens: opens,
                separated: separated.contains(element.id) ? true : nil,
                alternate: alternates[element.id],
                placeholder: element.text(kAXPlaceholderValueAttribute),
                level: kind == .row ? element.facts[kAXDisclosureLevelAttribute]?.integer : nil,
                index: kind == .row ? element.facts[kAXIndexAttribute]?.integer : nil,
                row: rows[element.id].flatMap { row in refs[row] },
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
                insertionPointLineNumber: element.facts[kAXInsertionPointLineNumberAttribute]?.integer,
                runs: runs[element.id],
                invalid: element.text(kAXInvalidAttribute).flatMap(Control.Invalid.init(rawValue:)),
                required: element.flag(NSAccessibility.Attribute.required.rawValue),
                current: element.text(kAXARIACurrentAttribute).flatMap(Control.Current.init(rawValue:)),
            )
            return (control, [(ref: control.ref, element: element.id)] + zip(optionRefs, listed.map(\.id)).map { ref, member in (ref: ref, element: member) })
        }
        return (controls, consumed.union(classified.map(\.element.id)))
    }
}
