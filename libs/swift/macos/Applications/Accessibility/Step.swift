import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePaths
import CasePathsCore
public import CoreGraphics
import Encoding
public import Processes
import Results
public import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let once = Input.Schedule(count: 1, every: nil, jitter: .zero)
private let selectTextWithCriteria: String = "AXSelectTextWithCriteria"
private let selections: [String: String] = [
    kAXOutlineRole: kAXSelectedRowsAttribute, kAXTableRole: kAXSelectedRowsAttribute, kAXListRole: kAXSelectedChildrenAttribute, kAXBrowserRole: kAXSelectedChildrenAttribute,
]

// --- [MODELS] --------------------------------------------------------------------------

public struct Step: Decodable, Hashable, Sendable, CustomStringConvertible {
    @CasePathable
    enum Aim: Decodable, Hashable, Sendable {
        case element(ref: Int)
        case point(Input.Sample)

        init(from decoder: any Decoder) throws {
            let value = try decoder.singleValueContainer()
            if let ref = try value.decodeIfMatching(Int.self) {
                self = .element(ref: ref)
                return
            }
            let numbers: [Double] = try value.decode([Double].self)
            let sample: Input.Sample? =
                switch numbers.count {
                    case 2: Input.Sample(at: SIMD2(numbers[0], numbers[1]), pressure: nil, tilt: nil)
                    case 3: Input.Sample(at: SIMD2(numbers[0], numbers[1]), pressure: numbers[2], tilt: nil)
                    case 5: Input.Sample(at: SIMD2(numbers[0], numbers[1]), pressure: numbers[2], tilt: SIMD2(numbers[3], numbers[4]))
                    default: nil
                }
            guard let sample else {
                throw DecodingError.dataCorruptedError(in: value, debugDescription: "\(numbers) is neither a reference nor [x, y], [x, y, pressure], or [x, y, pressure, tilt x, tilt y]")
            }
            self = .point(sample)
        }
    }

    enum TextRange: Decodable, Hashable, Sendable {
        case characters(location: Int, length: Int)
        case text(String)

        init(from decoder: any Decoder) throws {
            let value = try decoder.singleValueContainer()
            if let text = try value.decodeIfMatching(String.self) {
                self = .text(text)
                return
            }
            let bounds: [Int] = try value.decode([Int].self)
            guard bounds.count == 2, bounds.allSatisfy({ bound in bound >= 0 }) else {
                throw DecodingError.dataCorruptedError(in: value, debugDescription: "\(bounds) is neither text nor [location, length]")
            }
            self = .characters(location: bounds[0], length: bounds[1])
        }
    }

    enum Action: Hashable, Sendable {
        case press(ref: Int, action: String)
        case set(ref: Int, value: Fact.Scalar)
        case select(ref: Int, range: TextRange)
        case replace(ref: Int, range: TextRange, text: String)
        case input(contact: Input.Contact, path: [Aim], focus: Int?, hold: Duration, modifiers: Set<Modifier>, schedule: Input.Schedule, edges: Set<Input.Edge>, windows: [CGWindowID])
    }

    public struct Delivery: Encodable, Equatable, Sendable {
        public struct Attempt: Encodable, Equatable, Sendable {
            public let rung: Rung
            public let failure: String
        }

        public let window: CGWindowID?
        public let route: Route?
        public let tried: [Attempt]
        public let presentation: Watch.Presentation?
        public let pointer: SIMD2<Double>?
    }

    fileprivate enum Proof {
        case posting
        case change
        case value(attribute: String, before: Fact.Scalar?, after: Fact.Scalar)
    }

    private enum CodingKeys: String, CodingKey {
        case at, action, to, range, key, modifiers, pointer, path, button, by, unit, hold, count, every, jitter, edge, from, windows
    }

    let action: Action
    let from: Rung?

    init(_ action: Action, from: Rung? = nil) {
        self.action = action
        self.from = from
    }

    public init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        let timed = { (key: CodingKeys) throws -> Duration? in try fields.decodeIfPresent(Double.self, forKey: key).map { milliseconds in Duration.milliseconds(milliseconds) } }
        let (key, pointer, at): (Key?, Aim?, Int?) = try (
            fields.decodeIfPresent(Key.self, forKey: .key), fields.decodeIfPresent(Aim.self, forKey: .pointer), fields.decodeIfPresent(Int.self, forKey: .at),
        )
        let (by, button, unit): (SIMD2<Double>?, String?, String?) = try (
            fields.decodeIfPresent(SIMD2<Double>.self, forKey: .by), fields.decodeIfPresent(String.self, forKey: .button), fields.decodeIfPresent(String.self, forKey: .unit),
        )
        let contact: Input.Contact? =
            switch (key, pointer, by, button) {
                case (.some(let key), _, _, _): .key(key)
                case (.none, .none, _, _): nil
                case (.none, .some, .some(let by), _): .wheel(by: by, unit: unit == "line" ? .line : .pixel)
                case (.none, .some, .none, "none"): .hover
                case (.none, .some, .none, "right"): .button(.right)
                case (.none, .some, .none, "middle"): .button(.center)
                case (.none, .some, .none, _): .button(.left)
            }
        let value: Fact.Scalar? = try fields.decodeIfPresent(Fact.Scalar.self, forKey: .to)
        from = try fields.decodeIfPresent(Rung.self, forKey: .from)
        let path: [Aim] = try pointer.map { (first: Aim) throws -> [Aim] in [first] + (try fields.decodeIfPresent([Aim].self, forKey: .path) ?? []) } ?? []
        let schedule = Input.Schedule(count: try fields.decodeIfPresent(Int.self, forKey: .count) ?? 1, every: try timed(.every), jitter: try timed(.jitter) ?? .zero)
        let edge: Input.Edge? = try fields.decodeIfPresent(Input.Edge.self, forKey: .edge)
        let (hold, modifiers, windows): (Duration?, Set<Modifier>?, [CGWindowID]?) = try (
            timed(.hold), fields.decodeIfPresent(Set<Modifier>.self, forKey: .modifiers), fields.decodeIfPresent([CGWindowID].self, forKey: .windows),
        )
        let range: TextRange? = try fields.decodeIfPresent(TextRange.self, forKey: .range)
        switch (contact, at, value, range) {
            case (.some(let contact), _, _, _):
                let edges: Set<Input.Edge> = edge.map { edge in [edge] } ?? Set(Input.Edge.allCases)
                action = .input(contact: contact, path: path, focus: at, hold: hold ?? .zero, modifiers: modifiers ?? [], schedule: schedule, edges: edges, windows: windows ?? [])
            case (.none, .some(let ref), .some(.text(let text)), .some(let range)): action = .replace(ref: ref, range: range, text: text)
            case (.none, .some, .some, .some): throw DecodingError.dataCorruptedError(forKey: .to, in: fields, debugDescription: "A range is replaced by text alone")
            case (.none, .some(let ref), .none, .some(let range)): action = .select(ref: ref, range: range)
            case (.none, .some(let ref), .some(let value), .none): action = .set(ref: ref, value: value)
            case (.none, .some(let ref), .none, .none): action = .press(ref: ref, action: try fields.decodeIfPresent(String.self, forKey: .action) ?? kAXPressAction)
            case (.none, .none, _, _): throw DecodingError.keyNotFound(CodingKeys.at, DecodingError.Context(codingPath: fields.codingPath, debugDescription: "A step names at, key, or pointer"))
        }
    }

    public var description: String {
        switch action {
            case .press(let ref, let action): "\(action) on \(ref)"
            case .set(let ref, let value): "set \(kAXValueAttribute) \(value) on \(ref)"
            case .select(let ref, let range): "select \(range) on \(ref)"
            case .replace(let ref, let range, let text): "replace \(range) with \(String(reflecting: text)) on \(ref)"
            case .input(let contact, let path, let focus, _, let modifiers, let schedule, let edges, _):
                "\(contact) ×\(schedule.count) \(edges.map(\.rawValue).sorted()) with \(modifiers.map(\.rawValue).sorted()) at \(focus.map { ref in [Aim.element(ref: ref)] } ?? path)"
        }
    }

    var refusal: ApplicationFailure? {
        guard case .input(let contact, let path, _, let hold, let modifiers, let schedule, let edges, _) = action else { return nil }
        let samples: [Input.Sample] = path.map { aim in aim[case: \.point] ?? Input.Sample(at: .zero, pressure: nil, tilt: nil) }
        return Result { () throws(ApplicationFailure) in try Input(contact: contact, path: samples, hold: hold, modifiers: modifiers, schedule: schedule, edges: edges) }[case: \.failure]
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Step {
    fileprivate typealias Plan = (window: CGWindowID?, posting: (route: Route, inputs: [Input], proof: Proof)?, opening: String?, tried: [Delivery.Attempt])

    fileprivate func plan(on element: AXUIElement?, presented: Bool) throws(ApplicationFailure) -> Plan {
        guard let element else { throw ApplicationFailure(subject: description, reason: .stale(reached: 0, candidates: [])) }
        let accessible: Bool = from.map { rung in rung == .accessibility } ?? true
        let read = try element.read([kAXEnabledAttribute, kAXRoleAttribute, kAXFocusedAttribute, kAXWindowAttribute] + frameAttributes)
        let clicking: (route: Route, inputs: [Input], proof: Proof)? = try read.element.window.flatMap { number throws(ApplicationFailure) in
            let point: CGPoint? = read.element.activation ?? read.element.frame.map { frame in CGPoint(x: frame.midX, y: frame.midY) }
            return try point.map { point throws(ApplicationFailure) in
                let center: [Input.Sample] = [Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil)]
                return (.window(number: number), [try Input(contact: .button(.left), path: center, hold: .zero, modifiers: [], schedule: once, edges: Set(Input.Edge.allCases))], .change)
            }
        }
        switch (action, read.element.facts[kAXEnabledAttribute]?.bool) {
            case (.press(_, kAXRaiseAction), _): throw ApplicationFailure(subject: "\(description), raising a window takes focus from the frontmost application", reason: .refused)
            case (.press, false), (.set, false), (.select, false), (.replace, false): throw ApplicationFailure(subject: description, reason: .disabled)
            case (.press, _) where presented: return (read.element.window, nil, nil, [])
            case (.press(_, let action), _) where action == kAXShowMenuAction || action == kAXPressAction && read.element.role.map(pressOpened.contains) == true:
                return (read.element.window, nil, action, [])
            case (.press(_, kAXPressAction), _) where read.element.role == kAXRowRole && !read.element.actions.contains(kAXPressAction):
                return try select(element, read: read, clicking: clicking)
            case (.press(_, let action), _) where accessible:
                let named: String = read.element.actions.contains(action) ? action : read.element.descriptions.first { _, text in text == action }?.key ?? action
                let answer: AXError = AXUIElementPerformAction(element, named as CFString)
                let attempt = Delivery.Attempt(rung: .accessibility, failure: "\(named) answered AXError \(answer.rawValue)")
                switch (answer, action, clicking) {
                    case (.success, _, _): return (read.element.window, nil, nil, [])
                    case (.actionUnsupported, kAXPressAction, .some(let clicking)), (.failure, kAXPressAction, .some(let clicking)): return (read.element.window, clicking, nil, [attempt])
                    case (_, _, _):
                        throw ApplicationFailure(answer, subject: description, process: element.process, answering: self) {
                            ApplicationFailure(subject: description, reason: answer == .invalidUIElement ? .stale(reached: 0, candidates: []) : .accessibility(answer))
                        }
                }
            case (.press(_, kAXPressAction), _):
                guard let clicking else { throw ApplicationFailure(subject: "\(frameAttribute) of \(description)", reason: .accessibility(.noValue)) }
                return (read.element.window, clicking, nil, [])
            case (.press, _): throw ApplicationFailure(subject: "\(description) from rung \(from?.rawValue ?? ""), which names an element action no event performs", reason: .refused)
            case (.set(_, .rect(let frame)), _):
                var (origin, size): (CGPoint, CGSize) = (frame.origin, frame.size)
                guard let position = unsafe AXValueCreate(.cgPoint, &origin), let extent = unsafe AXValueCreate(.cgSize, &size) else {
                    throw ApplicationFailure(subject: description, reason: .refused)
                }
                let answer: AXError? = [(kAXSizeAttribute, extent), (kAXPositionAttribute, position), (kAXSizeAttribute, extent)].map { attribute, value in
                    AXUIElementSetAttributeValue(element, attribute as CFString, value)
                }
                .first { answer in answer != .success }
                let placed: Element = try element.read([kAXPositionAttribute, kAXSizeAttribute]).element
                let after: CGRect? = placed.facts[kAXPositionAttribute]?.point.flatMap { origin in placed.facts[kAXSizeAttribute]?.size.map { size in CGRect(origin: origin, size: size) } }
                switch (answer, after == frame) {
                    case (.none, true): return (read.element.window, nil, nil, [])
                    case (.none, false): throw ApplicationFailure(subject: description, reason: .unchanged(after.map(Fact.Scalar.rect)))
                    case (.some(let answer), _):
                        throw ApplicationFailure(answer, subject: description, process: element.process, answering: self) { ApplicationFailure(subject: description, reason: .accessibility(answer)) }
                }
            case (.set(_, let value), _): return try assign(value, to: element, read: read, presented: presented, accessible: accessible)
            case (.select(_, let range), _): return try select(range, in: element, read: read, replacing: nil)
            case (.replace(_, let range, let text), _): return try select(range, in: element, read: read, replacing: text)
            case (.input(let contact, _, _, let hold, let modifiers, let schedule, let edges, _), _):
                let typed = try Input(contact: contact, path: [], hold: hold, modifiers: modifiers, schedule: schedule, edges: edges)
                let aimed = try aim([typed], at: element, read: read, accessible: accessible)
                return (read.element.window, (aimed.route, aimed.inputs, .posting), nil, [])
        }
    }

    private func select(_ row: AXUIElement, read: (element: Element, handles: [AXUIElement]), clicking: (route: Route, inputs: [Input], proof: Proof)?) throws(ApplicationFailure) -> Plan {
        var (ancestor, owner): (AXUIElement?, (element: AXUIElement, attribute: String)?) = (try row.elements(kAXParentAttribute).first, nil)
        while let current = ancestor, owner == nil {
            let attribute: String? = try current.read([kAXRoleAttribute]).element.role.flatMap { role in selections[role] }
            (ancestor, owner) = try attribute == nil ? (current.elements(kAXParentAttribute).first, nil) : (nil, attribute.map { attribute in (current, attribute) })
        }
        let written: AXError? = try owner.flatMap { owner throws(ApplicationFailure) in
            try owner.element.settable(owner.attribute) ? AXUIElementSetAttributeValue(owner.element, owner.attribute as CFString, [row] as CFArray) : nil
        }
        let selected: Bool = try owner.map { owner throws(ApplicationFailure) in try owner.element.elements(owner.attribute).contains { chosen in CFEqual(chosen, row) } } ?? false
        guard !(written == .success && selected) else { return (read.element.window, nil, nil, []) }
        let attempt = Delivery.Attempt(rung: .accessibility, failure: owner.map { owner in "\(owner.attribute) left the row unselected" } ?? "no outline, table, list, or browser holds the row")
        guard let clicking else { throw ApplicationFailure(subject: "\(description), \(attempt.failure)", reason: .refused) }
        return (read.element.window, clicking, nil, [attempt])
    }

    private func select(_ range: TextRange, in element: AXUIElement, read: (element: Element, handles: [AXUIElement]), replacing text: String?) throws(ApplicationFailure) -> Plan {
        var names: CFArray?
        let web: Bool = unsafe AXUIElementCopyParameterizedAttributeNames(element, &names) == .success && (names as? [String])?.contains(selectTextWithCriteria) == true
        let unmatched = ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "text of \(description)", query: "\(range)", candidates: [])))
        switch (range, web) {
            case (.text(let sought), true):
                let criteria: [String: Any] = [
                    "AXSelectTextActivity": text == nil ? "AXSelectTextActivityFindAndSelect" : "AXSelectTextActivityFindAndReplace", "AXSelectTextSearchStrings": [sought],
                    "AXSelectTextAmbiguityResolution": "AXSelectTextAmbiguityResolutionClosestToSelection",
                ]
                let chosen = try element.parameterized(
                    selectTextWithCriteria,
                    criteria.merging(text.map { text in ["AXSelectTextReplacementString": text] } ?? [:]) { first, _ in first } as CFDictionary,
                )
                guard (chosen as? String).map({ chosen in !chosen.isEmpty }) == true else { throw unmatched }
                return (read.element.window, nil, nil, [])
            case (.characters, true): throw ApplicationFailure(subject: "\(description), web content selects text by the text it holds", reason: .refused)
            case (_, false): break
        }
        let value: String = try element.read([kAXValueAttribute]).element.text(kAXValueAttribute) ?? ""
        let span: NSRange? =
            switch range {
                case .characters(let location, let length): location + length <= value.utf16.count ? NSRange(location: location, length: length) : nil
                case .text(let sought): value.range(of: sought).map { found in NSRange(found, in: value) }
            }
        guard let span else { throw unmatched }
        var bounds = CFRange(location: span.location, length: span.length)
        guard let boxed: AXValue = unsafe AXValueCreate(.cfRange, &bounds) else { throw ApplicationFailure(subject: description, reason: .refused) }
        let window: AXUIElement? = read.element.facts[kAXWindowAttribute]?.element.map { id in read.handles[id] }
        let main: AXError =
            try window.map { window throws(ApplicationFailure) -> AXError in
                try window.settable(kAXMainAttribute) ? AXUIElementSetAttributeValue(window, kAXMainAttribute as CFString, kCFBooleanTrue) : .success
            } ?? .success
        let focused: AXError =
            main != .success || read.element.facts[kAXFocusedAttribute]?.bool == true ? main : AXUIElementSetAttributeValue(element, kAXFocusedAttribute as CFString, kCFBooleanTrue)
        let selected: AXError = focused == .success ? AXUIElementSetAttributeValue(element, kAXSelectedTextRangeAttribute as CFString, boxed) : focused
        let shown = try element.read([kAXSelectedTextRangeAttribute]).element.facts[kAXSelectedTextRangeAttribute]?.range
        guard selected == .success, shown?.location == span.location, shown?.length == span.length else {
            throw ApplicationFailure(subject: "\(kAXSelectedTextRangeAttribute) of \(description)", reason: selected == .success ? .unchanged(nil) : .accessibility(selected))
        }
        guard let text else { return (read.element.window, nil, nil, []) }
        let expected: String = (value as NSString).replacingCharacters(in: span, with: text)
        let keys: [Input] = try Self.typing(text.map { character -> (key: Key, modifiers: Set<Modifier>) in (Key.character(character), []) })
        let keyed: (route: Route, inputs: [Input], proof: Proof) = (.process, keys, .value(attribute: kAXValueAttribute, before: .text(value), after: .text(expected)))
        guard try window?.read([kAXDocumentAttribute]).element.text(kAXDocumentAttribute) == nil else {
            return (read.element.window, keyed, nil, [Delivery.Attempt(rung: .accessibility, failure: "\(kAXSelectedTextAttribute) skips the edit model of a document window")])
        }
        let written: AXError = AXUIElementSetAttributeValue(element, kAXSelectedTextAttribute as CFString, text as CFString)
        let after: String = try element.read([kAXValueAttribute]).element.text(kAXValueAttribute) ?? ""
        guard written != .success || after != expected else { return (read.element.window, nil, nil, []) }
        return (
            read.element.window, keyed, nil,
            [Delivery.Attempt(rung: .accessibility, failure: "\(kAXSelectedTextAttribute) answered AXError \(written.rawValue), \(kAXValueAttribute) read back \(String(reflecting: after))")],
        )
    }

    private static func scroll(_ element: AXUIElement?, by distance: SIMD2<Double>, unit: CGScrollEventUnit) throws(ApplicationFailure) -> Bool {
        var area: AXUIElement? = unit == .pixel ? element : nil
        while let current = area, try current.read([kAXRoleAttribute]).element.role != kAXScrollAreaRole { area = try current.elements(kAXParentAttribute).first }
        guard let area else { return false }
        let read = try area.read([frameAttribute, contentSizeAttribute, kAXHorizontalScrollBarAttribute, kAXVerticalScrollBarAttribute])
        guard let frame = read.element.frame, let content = read.element.facts[contentSizeAttribute]?.size else { return false }
        let axes: [(bar: AXUIElement?, moved: Double, span: Double)] = [
            (read.element.facts[kAXHorizontalScrollBarAttribute]?.element.map { id in read.handles[id] }, distance.x, content.width - frame.width),
            (read.element.facts[kAXVerticalScrollBarAttribute]?.element.map { id in read.handles[id] }, distance.y, content.height - frame.height),
        ]
        let needed = axes.filter { axis in axis.moved != 0 }
        let ready: [Bool] = try needed.map { axis throws(ApplicationFailure) -> Bool in
            try axis.bar.map { bar throws(ApplicationFailure) in try bar.settable(kAXValueAttribute) } == true && axis.span > 0
        }
        guard ready.allSatisfy(\.self) else { return false }
        let moved: [Bool] = try needed.compactMap { axis in axis.bar.map { bar in (bar: bar, moved: axis.moved, span: axis.span) } }.map { axis throws(ApplicationFailure) -> Bool in
            let before: Double = try axis.bar.read([kAXValueAttribute]).element.facts[kAXValueAttribute]?.number ?? 0
            let goal: Double = min(max(before - axis.moved / axis.span, 0), 1)
            let answer: AXError = AXUIElementSetAttributeValue(axis.bar, kAXValueAttribute as CFString, goal as NSNumber)
            let after: Double? = try axis.bar.read([kAXValueAttribute]).element.facts[kAXValueAttribute]?.number
            return answer == .success && after.map { after in abs(after - goal) < 0.5 / axis.span || after == goal } == true
        }
        switch (moved.allSatisfy(\.self), moved.contains(true)) {
            case (true, _): return true
            case (false, false): return false
            case (false, true): throw ApplicationFailure(subject: "scroll by \(distance) through \(kAXValueAttribute) of each \(kAXScrollBarRole)", reason: .unchanged(nil))
        }
    }

    private static func entering(_ text: String) -> [(key: Key, modifiers: Set<Modifier>)] {
        let selecting: [(key: Key, modifiers: Set<Modifier>)] = [(Key.character("a"), [Modifier.command])]
        let characters: [(key: Key, modifiers: Set<Modifier>)] = text.map { (character: Character) -> (key: Key, modifiers: Set<Modifier>) in (Key.character(character), []) }
        let returning: [(key: Key, modifiers: Set<Modifier>)] = [(Key.special(.return), [])]
        return selecting + characters + returning
    }

    private static func typing(_ keys: [(key: Key, modifiers: Set<Modifier>)]) throws(ApplicationFailure) -> [Input] {
        try keys.map { key, modifiers throws(ApplicationFailure) -> Input in
            try Input(contact: .key(key), path: [], hold: .zero, modifiers: modifiers, schedule: once, edges: Set(Input.Edge.allCases))
        }
    }

    private func assign(_ target: Fact.Scalar, to element: AXUIElement, read: (element: Element, handles: [AXUIElement]), presented: Bool, accessible: Bool) throws(ApplicationFailure) -> Plan {
        let attribute = read.element.role == kAXRowRole ? kAXSelectedAttribute : kAXValueAttribute
        let current = { () throws(ApplicationFailure) -> Fact.Scalar? in try element.read([attribute]).element.facts[attribute]?.scalar }
        let shown: Fact.Scalar? = try current()
        let settable: Bool = try accessible ? element.settable(attribute) : false
        let menus: [AXUIElement] = try !accessible || settable || target.text == nil ? [] : element.menus()
        let opening: String? =
            switch (read.element.actions.contains(kAXShowMenuAction), read.element.role) {
                case (true, _): kAXShowMenuAction
                case (false, kAXPopUpButtonRole), (false, kAXComboBoxRole), (false, kAXMenuButtonRole): kAXPressAction
                case (false, _): nil
            }
        let stepping: Double? = accessible && read.element.actions.contains(kAXIncrementAction) && read.element.actions.contains(kAXDecrementAction) ? target.number : nil
        let typed = { (tried: [Delivery.Attempt]) throws(ApplicationFailure) -> Plan in
            let aimed = try aim(Self.typing(Self.entering(target.text ?? "")), at: element, read: read, accessible: accessible)
            return (read.element.window, (aimed.route, aimed.inputs, .value(attribute: attribute, before: shown, after: target)), nil, tried)
        }
        var date: Date?
        if case .date? = shown, let text = target.text {
            do { date = try Date(text, strategy: .iso8601) } catch { throw ApplicationFailure(subject: "\(description), \(text) read as an ISO 8601 date", reason: .foundation(error as NSError)) }
        }
        switch (settable, target.text, accessible && (!menus.isEmpty || opening != nil), stepping) {
            case (true, _, _, _):
                let value: Fact.Scalar =
                    switch (shown, target.number, target.text.flatMap(Bool.init), date) {
                        case (.bool, _, .some(let flag), _): .bool(flag)
                        case (.number, .some(let number), _, _): .number(number)
                        case (.date, _, _, .some(let date)): .date(date)
                        case (_, _, _, _): target
                    }
                guard let boxed = value.accessibility else { throw ApplicationFailure(subject: description, reason: .refused) }
                let answer = AXUIElementSetAttributeValue(element, attribute as CFString, boxed)
                let after: Fact.Scalar? = try answer == .success ? current() : nil
                let attempt = Delivery.Attempt(
                    rung: .accessibility,
                    failure: answer == .success ? "\(attribute) read back \(after.map { after in "\(after)" } ?? "unset")" : "\(attribute) answered AXError \(answer.rawValue)",
                )
                switch (answer, after == value || value.number.map({ number in after?.number == number }) == true, target.text) {
                    case (.success, true, _): return (read.element.window, nil, nil, [])
                    case (_, false, .some): return try typed([attempt])
                    case (.success, false, .none): throw ApplicationFailure(subject: description, reason: .unchanged(after))
                    case (_, _, _):
                        throw ApplicationFailure(answer, subject: description, process: element.process, answering: self) { ApplicationFailure(subject: description, reason: .accessibility(answer)) }
                }
            case (false, .some(let item), true, _): return try choose(item, on: element, listing: menus, opening: opening, read: read, presented: presented)
            case (false, _, _, .some(let goal)):
                var (value, answer) = (shown?.number, AXError.success)
                while let reached = value, reached != goal, answer == .success {
                    answer = AXUIElementPerformAction(element, (reached < goal ? kAXIncrementAction : kAXDecrementAction) as CFString)
                    let next = try answer == .success ? current()?.number ?? reached : reached
                    value = answer != .success || abs(goal - next) < abs(goal - reached) ? next : nil
                }
                guard answer != .success || value == goal else { throw ApplicationFailure(subject: description, reason: .unchanged(try current())) }
                return (read.element.window, nil, nil, [])
            case (false, .some, _, .none): return try typed([])
            case (false, .none, _, .none): throw ApplicationFailure(subject: description, reason: .unchanged(shown))
        }
    }

    private func choose(
        _ item: String,
        on element: AXUIElement,
        listing menus: [AXUIElement],
        opening action: String?,
        read: (element: Element, handles: [AXUIElement]),
        presented: Bool,
    ) throws(ApplicationFailure) -> Plan {
        let labeled = { (entries: [AXUIElement]) throws(ApplicationFailure) -> [(element: AXUIElement, label: String?)] in
            try entries.map { entry throws(ApplicationFailure) in
                let fields = try entry.read([kAXTitleAttribute, kAXDescriptionAttribute, kAXTitleUIElementAttribute, kAXValueAttribute])
                let label: String? =
                    switch (fields.element.text(kAXTitleAttribute) ?? fields.element.text(kAXDescriptionAttribute), fields.element.facts[kAXTitleUIElementAttribute]?.element) {
                        case (.some(let label), _): label
                        case (.none, .some(let title)): try fields.handles[title].read([kAXValueAttribute]).element.text(kAXValueAttribute)
                        case (.none, .none): nil
                    }
                return (entry, label ?? fields.element.text(kAXValueAttribute))
            }
        }
        let items = { (menus: [AXUIElement]) throws(ApplicationFailure) in try labeled(menus.map { menu throws(ApplicationFailure) in try menu.elements(kAXChildrenAttribute) }.flatMap(\.self)) }
        let listed = try items(menus)
        let opening: String? = listed.isEmpty && !presented ? action : nil
        if let opening, opening == kAXShowMenuAction || read.element.role.map(pressOpened.contains) == true { return (read.element.window, nil, opening, []) }
        if let opening {
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1)))
            let opened = AXUIElementPerformAction(element, opening as CFString)
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / .seconds(1)))
            guard [.success, .cannotComplete].contains(opened) else { throw ApplicationFailure(subject: "\(opening) on \(description)", reason: .accessibility(opened)) }
        }
        let opened = try opening == nil ? menus : element.menus()
        let shown = try opening == nil ? listed : items(opened)
        guard shown.isEmpty else {
            guard let match = shown.first(where: { entry in entry.label == item }) else {
                for menu in opening == nil ? [] : opened { _ = AXUIElementPerformAction(menu, kAXCancelAction as CFString) }
                throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: shown.compactMap(\.label))))
            }
            let answer = AXUIElementPerformAction(match.element, kAXPressAction as CFString)
            guard answer == .success else { throw ApplicationFailure(subject: "\(kAXPressAction) on \(item) of \(description)", reason: .accessibility(answer)) }
            return (read.element.window, nil, nil, [])
        }
        let state = try element.read([kAXChildrenAttribute, kAXSelectedChildrenAttribute, kAXValueAttribute])
        let owned = try labeled((state.element.facts[kAXChildrenAttribute]?.elements ?? []).map { id in state.handles[id] }).filter { entry in entry.label != nil }
        let selected: [AXUIElement] = (state.element.facts[kAXSelectedChildrenAttribute]?.elements ?? []).map { id in state.handles[id] }
        let from = owned.firstIndex { entry in selected.contains { chosen in CFEqual(chosen, entry.element) } } ?? owned.firstIndex { entry in entry.label == state.element.text(kAXValueAttribute) }
        guard let goal = owned.firstIndex(where: { entry in entry.label == item }), let from else {
            if opening != nil { _ = AXUIElementPerformAction(element, kAXCancelAction as CFString) }
            throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: owned.compactMap(\.label))))
        }
        let arrow: (key: Key, modifiers: Set<Modifier>) = (Key.special(goal > from ? .down : .up), [])
        let returning: [(key: Key, modifiers: Set<Modifier>)] = [(Key.special(.return), [])]
        let moving: [(key: Key, modifiers: Set<Modifier>)] = Array(repeating: arrow, count: abs(goal - from)) + returning
        let aimed = try aim(Self.typing(moving), at: element, read: read, accessible: true)
        return (read.element.window, (aimed.route, aimed.inputs, .value(attribute: kAXValueAttribute, before: state.element.facts[kAXValueAttribute]?.scalar, after: .text(item))), nil, [])
    }

    private func aim(_ typed: [Input], at element: AXUIElement, read: (element: Element, handles: [AXUIElement]), accessible: Bool) throws(ApplicationFailure) -> (route: Route, inputs: [Input]) {
        guard read.element.facts[kAXFocusedAttribute]?.bool != true else { return (.process, typed) }
        let window: AXUIElement? = read.element.facts[kAXWindowAttribute]?.element.map { id in read.handles[id] }
        let main: AXError =
            try window.map { window throws(ApplicationFailure) -> AXError in
                try (accessible ? window.settable(kAXMainAttribute) : false) ? AXUIElementSetAttributeValue(window, kAXMainAttribute as CFString, kCFBooleanTrue) : .success
            } ?? .success
        let written: AXError = accessible && main == .success ? AXUIElementSetAttributeValue(element, kAXFocusedAttribute as CFString, kCFBooleanTrue) : main
        let application: AXUIElement? = written == .success && accessible ? element.process.map(AXUIElementCreateApplication) : nil
        let holder: AXUIElement? = try application?.elements(kAXFocusedUIElementAttribute).first
        let editor: AXUIElement? = try holder?.elements(kAXParentAttribute).first
        let kept: Bool = [holder, editor].compactMap(\.self).contains { (held: AXUIElement) -> Bool in CFEqual(held, element) }
        guard !kept else { return (.process, typed) }
        guard let number = read.element.window, let point = read.element.activation ?? read.element.frame.map({ frame in CGPoint(x: frame.midX, y: frame.midY) }) else {
            throw ApplicationFailure(subject: "\(kAXFocusedAttribute) for \(description)", reason: written == .success ? .refused : .accessibility(written))
        }
        let center: [Input.Sample] = [Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil)]
        return (.window(number: number), [try Input(contact: .button(.left), path: center, hold: .zero, modifiers: [], schedule: once, edges: Set(Input.Edge.allCases))] + typed)
    }

    fileprivate func run(
        on watch: Watch,
        clipboard: Int,
        host: (layout: KeyboardLayout, rate: Int, doubleClick: Duration, keyRepeat: (delay: Duration, interval: Duration)),
        held: inout [Input.Contact: (number: Int64, clickState: Int64, route: Route)],
        source: CGEventSource,
        using generator: inout some RandomNumberGenerator,
        clock: some Clock<Duration> & Sendable,
    ) async throws(ApplicationFailure) -> Delivery {
        let process: pid_t = watch.instance.process.pid
        let planned: Plan
        var presentation: Watch.Presentation?
        var pointer: SIMD2<Double>?
        switch action {
            case .set(clipboard, .text(let text)):
                _ = try await Pasteboard(.general, writing: text)
                return Delivery(window: nil, route: nil, tried: [], presentation: nil, pointer: nil)
            case .set(clipboard, _): throw ApplicationFailure(subject: "\(description), the clipboard takes text alone", reason: .refused)
            case .press(let ref, _), .set(let ref, _), .select(let ref, _), .replace(let ref, _, _), .input(_, _, .some(let ref), _, _, _, _, _):
                let pending: Plan = try await watch.perform(on: ref) { element throws(ApplicationFailure) in try plan(on: element, presented: false) }
                let presented = try await watch.presenting(pending.opening.map { _ in ref }, by: pending.opening, clock: clock) { (_: [Container]) async throws(ApplicationFailure) -> Plan in
                    try await pending.opening == nil ? pending : watch.perform(on: ref) { element throws(ApplicationFailure) in try plan(on: element, presented: true) }
                }
                (planned, presentation) = (presented.value, presented.presentation)
            case .input(let contact, let path, .none, let hold, let modifiers, let schedule, let edges, let windows):
                if case .wheel(let by, let unit) = contact, case .element(let ref) = path.first, from == nil || from == .accessibility,
                    try await watch.perform(on: ref, { element throws(ApplicationFailure) in try Self.scroll(element, by: by, unit: unit) })
                {
                    return Delivery(window: nil, route: nil, tried: [], presentation: nil, pointer: nil)
                }
                var (samples, numbers): ([Input.Sample], [CGWindowID]) = ([], windows)
                for aim in path {
                    let (sample, number): (Input.Sample, CGWindowID?) =
                        switch aim {
                            case .point(let sample): (sample, nil)
                            case .element(let ref): try await watch.perform(on: ref) { element throws(ApplicationFailure) in try Self.reach(element, ref: ref, revealing: contact == .hover) }
                        }
                    (samples, numbers) = (samples + [sample], numbers + (number.map { number in [number] } ?? []))
                }
                let first: CGPoint? = samples.first.map { sample in CGPoint(x: sample.at.x, y: sample.at.y) }
                let shown: CGWindowID? = InstanceState.Window.all(.optionAll)[process, default: []].first { row in
                    (numbers.isEmpty || numbers.contains(row.number)) && row.onScreen && first.map(row.frame.contains) == true
                }?.number
                let focused: CGWindowID? =
                    contact.is(\.key)
                    ? try await watch.perform(on: nil) { _ throws(ApplicationFailure) -> CGWindowID? in
                        do throws(ApplicationFailure) { return try AXUIElementCreateApplication(process).elements(kAXFocusedWindowAttribute).first?.window } catch  where error.reason.is(\.unanswered)
                        { return nil }
                    } : nil
                let window: CGWindowID? = shown ?? numbers.first ?? focused
                let input = try Input(contact: contact, path: samples, hold: hold, modifiers: modifiers, schedule: schedule, edges: edges)
                let route: Route? = contact.is(\.key) ? .process : window.map(Route.window(number:))
                guard let route else { throw ApplicationFailure(subject: "\(description), where no window of process \(process) holds the point", reason: .noWindow) }
                let proof: Proof = contact.is(\.button) && edges == Set(Input.Edge.allCases) ? .change : .posting
                planned = (window, (route, [input], proof), nil, [])
                pointer = samples.last?.at
        }
        guard let posting = planned.posting else { return Delivery(window: planned.window, route: nil, tried: planned.tried, presentation: presentation, pointer: nil) }
        let lifted: (number: Int64, clickState: Int64, route: Route)? = posting.inputs.count == 1 && posting.inputs[0].edges == [.up] ? held[posting.inputs[0].contact] : nil
        let below: Set<Rung> = Set(Rung.allCases.prefix { rung in rung != from && from != nil })
        let ladder: [Route] = lifted.map { lifted in [lifted.route] } ?? posting.route.ladder(window: planned.window).filter { route in !below.contains(route.rung) }
        let partial: Bool = posting.inputs.contains { input in input.edges != Set(Input.Edge.allCases) || input.contact == .hover }
        let rungs: [Route] = partial ? Array(ladder.prefix(1)) : ladder
        var tried: [Delivery.Attempt] = planned.tried
        for route in rungs {
            do throws(ApplicationFailure) {
                if route == .system, !watch.instance.application.isActive {
                    try PermittedFrontList.shared.admit(process).get()
                    let activations = await watch.messages()
                    let raised = AXUIElementSetAttributeValue(AXUIElementCreateApplication(process), kAXFrontmostAttribute as CFString, kCFBooleanTrue)
                    let fronted: Void? = try await watch.next({ (change: InstanceEvent.Change) -> Void? in change[case: \.frontmost] == true ? () : nil }, in: activations, clock: clock)
                    guard raised == .success, fronted != nil else {
                        throw ApplicationFailure(subject: "\(kAXFrontmostAttribute) of process \(process)", reason: raised == .success ? .unanswered(nil) : .accessibility(raised))
                    }
                    presentation = .forward
                }
                let base: Int64 = [CGEventType.leftMouseDown, .rightMouseDown, .otherMouseDown].map { type in Int64(CGEventSource.counterForEventType(source.sourceStateID, eventType: type)) }.reduce(
                    1,
                    +,
                )
                var (timeline, pressed): ([(offset: Duration, event: CGEvent)], Int64) = ([], 0)
                for input in posting.inputs {
                    let first: (number: Int64, clickState: Int64) = lifted.map { lifted in (lifted.number, lifted.clickState) } ?? (base + pressed, 1)
                    let next = try input.timeline(doubleClick: host.doubleClick, keyRepeat: host.keyRepeat, frames: host.rate, layout: host.layout, firstPress: first, using: &generator)
                    let shift: Duration = timeline.last?.offset ?? .zero
                    (timeline, pressed) = (timeline + next.map { offset, event in (shift + offset, event) }, pressed + (input.contact.is(\.button) ? Int64(input.schedule.count) : 0))
                }
                let changes = await watch.messages()
                try await route.post(timeline, to: process, from: source, frame: .seconds(1) / host.rate, clock: clock)
                let shown: Bool =
                    switch posting.proof {
                        case .posting: true
                        case .change: try await watch.next(Self.proving, in: changes, clock: clock) != nil
                        case .value(let attribute, let before, let after):
                            try await watch.perform(on: action.ref) { element throws(ApplicationFailure) in
                                let read: Fact.Scalar? = try element?.read([attribute]).element.facts[attribute]?.scalar
                                return read == after || read != before
                            }
                    }
                guard shown else { throw ApplicationFailure(subject: "\(description) through \(route)", reason: .unseen) }
                let down: Input? = posting.inputs.last { input in input.edges == [.down] }
                if partial { held[posting.inputs[0].contact] = down.map { down in (base + pressed - 1, Int64(down.schedule.count), route) } }
                return Delivery(window: planned.window, route: route, tried: tried, presentation: presentation, pointer: pointer)
            } catch  where [.refused, .noWindow, .unseen].contains(error.reason) || error.reason.is(\.windowServer) {
                tried.append(Delivery.Attempt(rung: route.rung, failure: error.errorDescription ?? error.subject))
            }
        }
        throw ApplicationFailure(subject: "\(description) after \(tried.map { attempt in "\(attempt.rung.rawValue): \(attempt.failure)" }.formatted())", reason: .unseen)
    }

    private static func reach(_ element: AXUIElement?, ref: Int, revealing: Bool) throws(ApplicationFailure) -> (sample: Input.Sample, window: CGWindowID?) {
        guard let element else { throw ApplicationFailure(subject: String(ref), reason: .stale(reached: 0, candidates: [])) }
        let read = try element.read(frameAttributes)
        if revealing, read.element.actions.contains(kAXShowAlternateUIAction) { _ = AXUIElementPerformAction(element, kAXShowAlternateUIAction as CFString) }
        guard let point = read.element.activation ?? read.element.frame.map({ frame in CGPoint(x: frame.midX, y: frame.midY) }) else {
            throw ApplicationFailure(subject: "\(frameAttribute) of \(ref)", reason: .accessibility(.noValue))
        }
        return (Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil), read.element.window)
    }

    private static func proving(_ change: InstanceEvent.Change) -> Void? {
        switch change {
            case .started, .frontmost, .hidden, .status, .exited: nil
            case .opened, .closed, .asking, .focused, .value, .selection, .moved, .window: ()
        }
    }
}

extension Step.Action {
    fileprivate var ref: Int? {
        switch self {
            case .press(let ref, _), .set(let ref, _), .select(let ref, _), .replace(let ref, _, _): ref
            case .input(_, _, let focus, _, _, _, _, _): focus
        }
    }
}

extension AXUIElement {
    fileprivate func menus() throws(ApplicationFailure) -> [AXUIElement] {
        try elements(kAXChildrenAttribute).map { child throws(ApplicationFailure) -> [AXUIElement] in
            switch try child.read([kAXRoleAttribute]).element.role {
                case .some(kAXMenuRole), .some(kAXListRole): [child]
                case .some(kAXScrollAreaRole): try child.menus()
                case .some, .none: []
            }
        }
        .flatMap(\.self)
    }
}

extension Watch {
    @concurrent
    public nonisolated func act(
        _ steps: [Step],
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> (outcomes: [Result<Step.Delivery, ApplicationFailure>], state: InstanceState, usage: Result<ResourceUsage.Interval, Errno>) {
        let process: ProcessInstance = instance.process
        let refused: [String] = steps.enumerated().compactMap { index, step in step.refusal.map { failure in "step \(index + 1) \(failure.subject)" } }
        guard refused.isEmpty else { throw ApplicationFailure(subject: refused.formatted(), reason: .refused) }
        let began = Result { () throws(Errno) in try ResourceUsage(of: process) }
        guard let source = CGEventSource(stateID: .privateState) else { throw ApplicationFailure(subject: "private event source for process \(process.pid)", reason: .refused) }
        let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: "Acting on process \(process.pid)")
        defer { ProcessInfo.processInfo.endActivity(activity) }
        let keyboardType: CGEventSourceKeyboardType = source.keyboardType
        let host = await MainActor.run {
            (
                layout: KeyboardLayout.current(keyboardType: keyboardType),
                rate: max(NSScreen.main?.maximumFramesPerSecond ?? 1, 1),
                doubleClick: Duration.seconds(NSEvent.doubleClickInterval),
                keyRepeat: (delay: Duration.seconds(NSEvent.keyRepeatDelay), interval: Duration.seconds(NSEvent.keyRepeatInterval)),
            )
        }
        let clipboard: Int = await intern([Pasteboard.locator(of: .general)])[0]
        var (outcomes, held, generator): ([Result<Step.Delivery, ApplicationFailure>], [Input.Contact: (number: Int64, clickState: Int64, route: Route)], SystemRandomNumberGenerator) = (
            [], [:], SystemRandomNumberGenerator(),
        )
        var previous: AsyncStream<Message>?
        for step in steps where outcomes.last?.is(\.failure) != true {
            let events = await messages()
            var outcome = await Result { () async throws(ApplicationFailure) in
                try await step.run(on: self, clipboard: clipboard, host: host, held: &held, source: source, using: &generator, clock: clock)
            }
            if let previous, outcome[case: \.failure]?.reason.is(\.stale) == true,
                try await next({ (change: InstanceEvent.Change) -> Void? in change[case: \.opened].map { _ in () } }, in: previous, clock: clock) != nil
            {
                outcome = await Result { () async throws(ApplicationFailure) in
                    try await step.run(on: self, clipboard: clipboard, host: host, held: &held, source: source, using: &generator, clock: clock)
                }
            }
            outcomes.append(
                outcome.flatMapError { failure in
                    [.system(.noSuchProcess), .unanswered(step)].contains(failure.reason)
                        ? .success(Step.Delivery(window: nil, route: nil, tried: [], presentation: nil, pointer: nil)) : .failure(failure)
                }
            )
            previous = events
        }
        var last: InstanceState = await current
        for await next in await states().dropFirst() {
            let answered: Bool =
                switch next {
                    case .exited, .failed: true
                    case .running(let running): running.status.isIdle || running.status.is(\.unresponsive) || next == last
                }
            last = next
            if answered { break }
        }
        return (outcomes, last, Result { () throws(Errno) in try ResourceUsage(of: process) }.flatMap { after in began.map(after.since) })
    }
}
