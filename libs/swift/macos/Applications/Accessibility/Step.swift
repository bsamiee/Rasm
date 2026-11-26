import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
import CasePaths
import CasePathsCore
public import CoreGraphics
import Encoding
public import Processes
import Results
public import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let once = Input.Schedule(count: 1, every: nil, jitter: .zero)
private let everyEdge: Set<Input.Edge> = Set(Input.Edge.allCases)
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

    private enum Button: String, Decodable {
        case left
        case right
        case center = "middle"
        case hover = "none"

        var contact: Input.Contact {
            switch self {
                case .left: .button(.left)
                case .right: .button(.right)
                case .center: .button(.center)
                case .hover: .hover
            }
        }
    }

    private enum ScrollUnit: String, Decodable {
        case pixel, line

        var unit: CGScrollEventUnit {
            switch self {
                case .pixel: .pixel
                case .line: .line
            }
        }
    }

    enum Action: Hashable, Sendable {
        case press(ref: Int, action: String)
        case set(ref: Int, value: Fact.Scalar)
        case select(ref: Int, range: TextRange)
        case replace(ref: Int, range: TextRange, text: String)
        case input(contact: Input.Contact, path: [Aim], focus: Int?, hold: Duration, modifiers: Set<Modifier>, schedule: Input.Schedule, edges: Set<Input.Edge>, windows: [CGWindowID])

        fileprivate var ref: Int? {
            switch self {
                case .press(let ref, _), .set(let ref, _), .select(let ref, _), .replace(let ref, _, _): ref
                case .input(_, _, let focus, _, _, _, _, _): focus
            }
        }
    }

    public struct Delivery: Encodable, Equatable, Sendable {
        public enum Proof: String, Encodable, Sendable {
            case answer, change, value, posting, unanswered, exit
        }

        public struct Attempt: Encodable, Equatable, Sendable {
            public let rung: Rung
            public let failure: String
        }

        public struct PopUp: Encodable, Equatable, Sendable {
            public let window: CGWindowID
            public let closed: Bool
        }

        public let proof: Proof
        public internal(set) var window: CGWindowID?
        public internal(set) var route: Route?
        public internal(set) var tried: [Attempt] = []
        public internal(set) var presentation: Watch.Presentation?
        public internal(set) var pointer: SIMD2<Double>?
        public internal(set) var popUps: [PopUp] = []
    }

    fileprivate enum ReadBack {
        case posting
        case change
        case value(attribute: String, before: Fact.Scalar?, after: Fact.Scalar)
    }

    @CasePathable
    fileprivate enum Course {
        case proven(Delivery.Proof)
        case posting(route: Route, inputs: [Input], readBack: ReadBack)
        case opening(action: String)
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
        let (by, button, unit): (SIMD2<Double>?, Button?, ScrollUnit?) = try (
            fields.decodeIfPresent(SIMD2<Double>.self, forKey: .by), fields.decodeIfPresent(Button.self, forKey: .button), fields.decodeIfPresent(ScrollUnit.self, forKey: .unit),
        )
        let contact: Input.Contact? =
            switch (key, pointer, by) {
                case (.some(let key), _, _): .key(key)
                case (.none, .none, _): nil
                case (.none, .some, .some(let by)): .wheel(by: by, unit: unit?.unit ?? .pixel)
                case (.none, .some, .none): button?.contact ?? .button(.left)
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
                action = .input(
                    contact: contact,
                    path: path,
                    focus: at,
                    hold: hold ?? .zero,
                    modifiers: modifiers ?? [],
                    schedule: schedule,
                    edges: edge.map { edge in [edge] } ?? everyEdge,
                    windows: windows ?? [],
                )
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
    fileprivate typealias Plan = (window: CGWindowID?, course: Course, tried: [Delivery.Attempt])
    fileprivate typealias Read = (element: Element, handles: [AXUIElement])
    fileprivate typealias Host = (layout: KeyboardLayout, rate: Int, doubleClick: Duration, keyRepeat: (delay: Duration, interval: Duration))

    fileprivate func plan(on element: AXUIElement?, presented: Bool) throws(ApplicationFailure) -> Plan {
        guard let element else { throw ApplicationFailure(subject: description, reason: .stale(reached: 0, candidates: [])) }
        let accessible: Bool = from.map { rung in rung == .accessibility } ?? true
        let shown: [String] =
            switch action {
                case .set, .select, .replace: [kAXValueAttribute, kAXSelectedAttribute]
                case .press, .input: []
            }
        let read = try element.element(reading: [kAXEnabledAttribute, kAXRoleAttribute, kAXFocusedAttribute, kAXWindowAttribute, kAXParentAttribute] + frameAttributes + shown)
        let point: CGPoint? = read.element.activation ?? read.element.frame.map { frame in CGPoint(x: frame.midX, y: frame.midY) }
        let center: [Input.Sample] = point.map { point in [Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil)] } ?? []
        let pressing: Input? = center.isEmpty ? nil : try Input(contact: .button(.left), path: center, hold: .zero, modifiers: [], schedule: once, edges: everyEdge)
        let click: Course? = read.element.window.flatMap { number in pressing.map { pressing in Course.posting(route: .window(number: number), inputs: [pressing], readBack: .change) } }
        switch (action, read.element.facts[kAXEnabledAttribute]?.bool) {
            case (.press(_, kAXRaiseAction), _): throw ApplicationFailure(subject: "\(description), raising a window takes focus from the frontmost application", reason: .refused)
            case (.press, false), (.set, false), (.select, false), (.replace, false): throw ApplicationFailure(subject: description, reason: .disabled)
            case (.press, _) where presented: return (read.element.window, .proven(.change), [])
            case (.press(_, let action), _) where action == kAXShowMenuAction || action == kAXPressAction && read.element.role.map(pressOpened.contains) == true:
                return (read.element.window, .opening(action: action), [])
            case (.press(_, kAXPressAction), _) where read.element.role == kAXRowRole && !read.element.actions.contains(kAXPressAction):
                return try select(element, read: read, click: click)
            case (.press(_, let action), _) where accessible:
                let named: String = read.element.actions.contains(action) ? action : read.element.descriptions.first { _, text in text == action }?.key ?? action
                let answer: AXError = AXUIElementPerformAction(element, named as CFString)
                switch (answer, action, click) {
                    case (.success, _, _): return (read.element.window, .proven(.answer), [])
                    case (.actionUnsupported, kAXPressAction, .some(let click)), (.failure, kAXPressAction, .some(let click)):
                        return (read.element.window, click, [Delivery.Attempt(rung: .accessibility, failure: "\(named) answered AXError \(answer.rawValue)")])
                    case (_, _, _):
                        throw ApplicationFailure(answer, subject: description, process: element.process, answering: self) { ApplicationFailure(subject: description, reason: .accessibility(answer)) }
                }
            case (.press(_, kAXPressAction), _):
                guard let click else { throw ApplicationFailure(subject: "\(frameAttribute) of \(description)", reason: .accessibility(.noValue)) }
                return (read.element.window, click, [])
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
                    case (.none, true): return (read.element.window, .proven(.value), [])
                    case (.none, false): throw ApplicationFailure(subject: description, reason: .unchanged(after.map(Fact.Scalar.rect)))
                    case (.some(let answer), _):
                        throw ApplicationFailure(answer, subject: description, process: element.process, answering: self) { ApplicationFailure(subject: description, reason: .accessibility(answer)) }
                }
            case (.set(_, let value), _): return try assign(value, to: element, read: read, click: click, presented: presented, accessible: accessible)
            case (.select(_, let range), _): return try select(range, in: element, read: read, replacing: nil)
            case (.replace(_, let range, let text), _): return try select(range, in: element, read: read, replacing: text)
            case (.input(let contact, _, _, let hold, let modifiers, let schedule, let edges, _), _):
                let typed = try Input(contact: contact, path: [], hold: hold, modifiers: modifiers, schedule: schedule, edges: edges)
                return try aim([typed], at: element, read: read, click: click, accessible: accessible, readBack: .posting)
        }
    }

    private func select(_ row: AXUIElement, read: Read, click: Course?) throws(ApplicationFailure) -> Plan {
        var (ancestor, owner): (AXUIElement?, (element: AXUIElement, attribute: String)?) = (read.element.facts[kAXParentAttribute]?.element.map { id in read.handles[id] }, nil)
        while let current = ancestor, owner == nil {
            let up = try current.read([kAXRoleAttribute, kAXParentAttribute])
            (ancestor, owner) = (
                up.element.facts[kAXParentAttribute]?.element.map { id in up.handles[id] }, up.element.role.flatMap { role in selections[role] }.map { attribute in (current, attribute) },
            )
        }
        let written: AXError? = try owner.flatMap { owner throws(ApplicationFailure) in
            try owner.element.settable(owner.attribute) ? AXUIElementSetAttributeValue(owner.element, owner.attribute as CFString, [row] as CFArray) : nil
        }
        let selected: Bool = try owner.map { owner throws(ApplicationFailure) in try owner.element.elements(owner.attribute).contains { chosen in CFEqual(chosen, row) } } ?? false
        guard !(written == .success && selected) else { return (read.element.window, .proven(.value), []) }
        let attempt = Delivery.Attempt(rung: .accessibility, failure: owner.map { owner in "\(owner.attribute) left the row unselected" } ?? "no outline, table, list, or browser holds the row")
        guard let click else { throw ApplicationFailure(subject: "\(description), \(attempt.failure)", reason: .refused) }
        return (read.element.window, click, [attempt])
    }

    private func select(_ range: TextRange, in element: AXUIElement, read: Read, replacing text: String?) throws(ApplicationFailure) -> Plan {
        var names: CFArray?
        let listed: AXError = unsafe AXUIElementCopyParameterizedAttributeNames(element, &names)
        guard [.success, .noValue, .attributeUnsupported, .notImplemented].contains(listed) else { throw element.failure(listed, reading: selectTextWithCriteria) }
        let web: Bool = (names as? [String])?.contains(selectTextWithCriteria) == true
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
                return (read.element.window, .proven(.value), [])
            case (.characters, true): throw ApplicationFailure(subject: "\(description), web content selects text by the text it holds", reason: .refused)
            case (_, false): break
        }
        let value: String = read.element.text(kAXValueAttribute) ?? ""
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
        guard let text else { return (read.element.window, .proven(.value), []) }
        let expected: String = (value as NSString).replacingCharacters(in: span, with: text)
        let keys: [(key: Key, modifiers: Set<Modifier>)] =
            text.isEmpty ? [(Key.special(.delete), Set<Modifier>())] : text.map { (character: Character) -> (key: Key, modifiers: Set<Modifier>) in (Key.character(character), []) }
        let keyed: Course = .posting(route: .process, inputs: try Self.typing(keys), readBack: .value(attribute: kAXValueAttribute, before: .text(value), after: .text(expected)))
        guard try window?.read([kAXDocumentAttribute]).element.text(kAXDocumentAttribute) == nil else {
            return (read.element.window, keyed, [Delivery.Attempt(rung: .accessibility, failure: "\(kAXSelectedTextAttribute) skips the edit model of a document window")])
        }
        let written: AXError = AXUIElementSetAttributeValue(element, kAXSelectedTextAttribute as CFString, text as CFString)
        let after: String = try element.read([kAXValueAttribute]).element.text(kAXValueAttribute) ?? ""
        guard written != .success || after != expected else { return (read.element.window, .proven(.value), []) }
        return (
            read.element.window, keyed,
            [Delivery.Attempt(rung: .accessibility, failure: "\(kAXSelectedTextAttribute) answered AXError \(written.rawValue), \(kAXValueAttribute) read back \(String(reflecting: after))")],
        )
    }

    private static func scroll(_ element: AXUIElement?, by distance: SIMD2<Double>, unit: CGScrollEventUnit) throws(ApplicationFailure) -> Bool {
        var (next, area): (AXUIElement?, Read?) = (unit == .pixel ? element : nil, nil)
        while let current = next, area == nil {
            let up = try current.read([kAXRoleAttribute, kAXParentAttribute, frameAttribute, contentSizeAttribute, kAXHorizontalScrollBarAttribute, kAXVerticalScrollBarAttribute])
            (next, area) = up.element.role == kAXScrollAreaRole ? (nil, up) : (up.element.facts[kAXParentAttribute]?.element.map { id in up.handles[id] }, nil)
        }
        guard let read = area, let frame = read.element.frame, let content = read.element.facts[contentSizeAttribute]?.size else { return false }
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

    private static func typing(_ keys: [(key: Key, modifiers: Set<Modifier>)]) throws(ApplicationFailure) -> [Input] {
        try keys.map { key, modifiers throws(ApplicationFailure) -> Input in try Input(contact: .key(key), path: [], hold: .zero, modifiers: modifiers, schedule: once, edges: everyEdge) }
    }

    private func assign(_ target: Fact.Scalar, to element: AXUIElement, read: Read, click: Course?, presented: Bool, accessible: Bool) throws(ApplicationFailure) -> Plan {
        let attribute = read.element.role == kAXRowRole ? kAXSelectedAttribute : kAXValueAttribute
        let current = { () throws(ApplicationFailure) -> Fact.Scalar? in try element.read([attribute]).element.facts[attribute]?.scalar }
        let shown: Fact.Scalar? = read.element.facts[attribute]?.scalar
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
            let characters: [(key: Key, modifiers: Set<Modifier>)] = (target.text ?? "").map { (character: Character) -> (key: Key, modifiers: Set<Modifier>) in (Key.character(character), []) }
            let entry: [(key: Key, modifiers: Set<Modifier>)] = [(Key.character("a"), Set([Modifier.command]))] + characters + [(Key.special(.return), Set<Modifier>())]
            return try aim(Self.typing(entry), at: element, read: read, click: click, accessible: accessible, readBack: .value(attribute: attribute, before: shown, after: target), tried: tried)
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
                    case (.success, true, _): return (read.element.window, .proven(.value), [])
                    case (_, false, .some): return try typed([attempt])
                    case (.success, false, .none): throw ApplicationFailure(subject: description, reason: .unchanged(after))
                    case (_, _, _):
                        throw ApplicationFailure(answer, subject: description, process: element.process, answering: self) { ApplicationFailure(subject: description, reason: .accessibility(answer)) }
                }
            case (false, .some(let item), true, _): return try choose(item, on: element, listing: menus, opening: opening, read: read, click: click, presented: presented)
            case (false, _, _, .some(let goal)):
                var (value, answer) = (shown?.number, AXError.success)
                while let reached = value, reached != goal, answer == .success {
                    answer = AXUIElementPerformAction(element, (reached < goal ? kAXIncrementAction : kAXDecrementAction) as CFString)
                    let next = try answer == .success ? current()?.number ?? reached : reached
                    value = answer != .success || abs(goal - next) < abs(goal - reached) ? next : nil
                }
                switch (answer, value == goal) {
                    case (.success, true): return (read.element.window, .proven(.value), [])
                    case (.success, false): throw ApplicationFailure(subject: description, reason: .unchanged(try current()))
                    case (_, _):
                        throw ApplicationFailure(answer, subject: description, process: element.process, answering: self) { ApplicationFailure(subject: description, reason: .accessibility(answer)) }
                }
            case (false, .some, _, .none): return try typed([])
            case (false, .none, _, .none): throw ApplicationFailure(subject: description, reason: .unchanged(shown))
        }
    }

    private func choose(
        _ item: String,
        on element: AXUIElement,
        listing menus: [AXUIElement],
        opening action: String?,
        read: Read,
        click: Course?,
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
        let unmatched = { (cancelling: [AXUIElement], candidates: [String]) -> ApplicationFailure in
            let cancelled: AXError? = cancelling.map { menu in AXUIElementPerformAction(menu, kAXCancelAction as CFString) }.first { answer in answer != .success }
            return cancelled.map { answer in
                ApplicationFailure(subject: "\(kAXCancelAction) on the list of \(description) after \(item) matched none of \(candidates.formatted())", reason: .accessibility(answer))
            } ?? ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: candidates)))
        }
        let listed = try items(menus)
        let opening: String? = listed.isEmpty && !presented ? action : nil
        if let opening, opening == kAXShowMenuAction || read.element.role.map(pressOpened.contains) == true { return (read.element.window, .opening(action: opening), []) }
        if let opening {
            let answers: [AXError] = [
                AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1))), AXUIElementPerformAction(element, opening as CFString), AXUIElementSetMessagingTimeout(element, 0),
            ]
            if let refused = answers.first(where: { answer in ![.success, .cannotComplete].contains(answer) }) {
                throw ApplicationFailure(subject: "\(opening) on \(description)", reason: .accessibility(refused))
            }
        }
        let opened = try opening == nil ? menus : element.menus()
        let shown = try opening == nil ? listed : items(opened)
        guard shown.isEmpty else {
            guard let match = shown.first(where: { entry in entry.label == item }) else { throw unmatched(opening == nil ? [] : opened, shown.compactMap(\.label)) }
            let answer = AXUIElementPerformAction(match.element, kAXPressAction as CFString)
            guard answer == .success else { throw ApplicationFailure(subject: "\(kAXPressAction) on \(item) of \(description)", reason: .accessibility(answer)) }
            return (read.element.window, .proven(.answer), [])
        }
        let state = try element.read([kAXChildrenAttribute, kAXSelectedChildrenAttribute, kAXValueAttribute])
        let owned = try labeled((state.element.facts[kAXChildrenAttribute]?.elements ?? []).map { id in state.handles[id] }).filter { entry in entry.label != nil }
        let selected: [AXUIElement] = (state.element.facts[kAXSelectedChildrenAttribute]?.elements ?? []).map { id in state.handles[id] }
        let from = owned.firstIndex { entry in selected.contains { chosen in CFEqual(chosen, entry.element) } } ?? owned.firstIndex { entry in entry.label == state.element.text(kAXValueAttribute) }
        guard let goal = owned.firstIndex(where: { entry in entry.label == item }), let from else { throw unmatched(opening == nil ? [] : [element], owned.compactMap(\.label)) }
        let arrow: (key: Key, modifiers: Set<Modifier>) = (Key.special(goal > from ? .down : .up), Set<Modifier>())
        let moving: [(key: Key, modifiers: Set<Modifier>)] = Array(repeating: arrow, count: abs(goal - from)) + [(Key.special(.return), Set<Modifier>())]
        let readBack: ReadBack = .value(attribute: kAXValueAttribute, before: state.element.facts[kAXValueAttribute]?.scalar, after: .text(item))
        return try aim(Self.typing(moving), at: element, read: read, click: click, accessible: true, readBack: readBack)
    }

    private func aim(
        _ typed: [Input],
        at element: AXUIElement,
        read: Read,
        click: Course?,
        accessible: Bool,
        readBack: ReadBack,
        tried: [Delivery.Attempt] = [],
    ) throws(ApplicationFailure) -> Plan {
        guard read.element.facts[kAXFocusedAttribute]?.bool != true else { return (read.element.window, .posting(route: .process, inputs: typed, readBack: readBack), tried) }
        let window: AXUIElement? = read.element.facts[kAXWindowAttribute]?.element.map { id in read.handles[id] }
        let main: AXError =
            try window.map { window throws(ApplicationFailure) -> AXError in
                try (accessible ? window.settable(kAXMainAttribute) : false) ? AXUIElementSetAttributeValue(window, kAXMainAttribute as CFString, kCFBooleanTrue) : .success
            } ?? .success
        let written: AXError = accessible && main == .success ? AXUIElementSetAttributeValue(element, kAXFocusedAttribute as CFString, kCFBooleanTrue) : main
        let application: AXUIElement? = written == .success && accessible ? element.process.map(AXUIElementCreateApplication) : nil
        let holder: AXUIElement? = try application?.elements(kAXFocusedUIElementAttribute).first
        let editor: AXUIElement? = try holder?.elements(kAXParentAttribute).first
        guard ![holder, editor].compactMap(\.self).contains(where: { held in CFEqual(held, element) }) else {
            return (read.element.window, .posting(route: .process, inputs: typed, readBack: readBack), tried)
        }
        guard case .posting(let route, let clicked, _)? = click else {
            throw ApplicationFailure(subject: "\(kAXFocusedAttribute) for \(description)", reason: written == .success ? .refused : .accessibility(written))
        }
        return (read.element.window, .posting(route: route, inputs: clicked + typed, readBack: readBack), tried)
    }

    fileprivate func run(
        on watch: Watch,
        clipboard: Int,
        host: Host,
        held: inout [Input.Contact: (number: Int64, clickState: Int64, route: Route)],
        source: CGEventSource,
        using generator: inout some RandomNumberGenerator,
        clock: some Clock<Duration> & Sendable,
    ) async throws(ApplicationFailure) -> Delivery {
        let process: pid_t = watch.instance.process.pid
        let planned: Plan
        var (presentation, pointer): (Watch.Presentation?, SIMD2<Double>?) = (nil, nil)
        switch action {
            case .set(clipboard, .text(let text)):
                try Pasteboard.write(text, to: .general)
                return Delivery(proof: .answer)
            case .set(clipboard, _): throw ApplicationFailure(subject: "\(description), the clipboard takes text alone", reason: .refused)
            case .press(let ref, _), .set(let ref, _), .select(let ref, _), .replace(let ref, _, _), .input(_, _, .some(let ref), _, _, _, _, _):
                let pending: Plan = try await watch.perform(on: ref) { element throws(ApplicationFailure) in try plan(on: element, presented: false) }
                let opening: String? = pending.course[case: \.opening]
                let presented = try await watch.presenting(opening.map { _ in ref }, by: opening, clock: clock) { (_: [Container]) async throws(ApplicationFailure) -> Plan in
                    try await opening == nil ? pending : watch.perform(on: ref) { element throws(ApplicationFailure) in try plan(on: element, presented: true) }
                }
                (planned, presentation) = (presented.value, presented.presentation)
            case .input(let contact, let path, .none, let hold, let modifiers, let schedule, let edges, let windows):
                if case .wheel(let by, let unit) = contact, case .element(let ref) = path.first, from == nil || from == .accessibility,
                    try await watch.perform(on: ref, { element throws(ApplicationFailure) in try Self.scroll(element, by: by, unit: unit) })
                {
                    return Delivery(proof: .value)
                }
                var (samples, numbers, tried): ([Input.Sample], [CGWindowID], [Delivery.Attempt]) = ([], windows, [])
                for aim in path {
                    let reached: (sample: Input.Sample, window: CGWindowID?, tried: [Delivery.Attempt]) =
                        switch aim {
                            case .point(let sample): (sample, nil, [])
                            case .element(let ref): try await watch.perform(on: ref) { element throws(ApplicationFailure) in try Self.reach(element, ref: ref, revealing: contact == .hover) }
                        }
                    (samples, numbers, tried) = (samples + [reached.sample], numbers + (reached.window.map { number in [number] } ?? []), tried + reached.tried)
                }
                let shown: CGWindowID? = samples.first.flatMap { start in
                    InstanceState.Window.all(.optionOnScreenOnly, of: process)[process, default: []].first { row in
                        (numbers.isEmpty || numbers.contains(row.number)) && row.frame.contains(CGPoint(x: start.at.x, y: start.at.y))
                    }?.number
                }
                let focused: CGWindowID? =
                    contact.is(\.key)
                    ? try await watch.perform(on: nil) { _ throws(ApplicationFailure) -> CGWindowID? in
                        do throws(ApplicationFailure) { return try AXUIElementCreateApplication(process).elements(kAXFocusedWindowAttribute).first?.window } catch  where error.reason.is(\.unanswered)
                        { return nil }
                    } : nil
                let window: CGWindowID? = shown ?? numbers.first ?? focused
                let input = try Input(contact: contact, path: samples, hold: hold, modifiers: modifiers, schedule: schedule, edges: edges)
                guard let route = contact.is(\.key) ? Route.process : window.map(Route.window(number:)) else {
                    throw ApplicationFailure(subject: "\(description), where no window of process \(process) holds the point", reason: .noWindow)
                }
                planned = (window, .posting(route: route, inputs: [input], readBack: contact.is(\.button) && edges == everyEdge ? .change : .posting), tried)
                pointer = samples.last?.at
        }
        guard case .posting(let entry, let inputs, let readBack) = planned.course else {
            return Delivery(proof: planned.course[case: \.proven] ?? .change, window: planned.window, tried: planned.tried, presentation: presentation)
        }
        let lifted: (number: Int64, clickState: Int64, route: Route)? = inputs.count == 1 && inputs[0].edges == [.up] ? held[inputs[0].contact] : nil
        let below: Set<Rung> = Set(Rung.allCases.prefix { rung in rung != from && from != nil })
        let ladder: [Route] = lifted.map { lifted in [lifted.route] } ?? entry.ladder(window: planned.window).filter { route in !below.contains(route.rung) }
        let partial: Bool = inputs.contains { input in input.edges != everyEdge || input.contact == .hover }
        var tried: [Delivery.Attempt] = planned.tried
        for route in partial ? Array(ladder.prefix(1)) : ladder {
            do throws(ApplicationFailure) {
                if route == .system, !watch.instance.application.isActive {
                    try PermittedFrontList.shared.admit(process).get()
                    let activations = await watch.messages()
                    let raised = AXUIElementSetAttributeValue(AXUIElementCreateApplication(process), kAXFrontmostAttribute as CFString, kCFBooleanTrue)
                    let fronted: Void? = try await watch.next({ (change: InstanceEvent.Change) -> Void? in change[case: \.frontmost] == true ? () : nil }, in: activations, clock: clock)
                    guard raised == .success, fronted != nil else {
                        let subject = "\(kAXFrontmostAttribute) of process \(process)"
                        throw raised == .success
                            ? ApplicationFailure(subject: subject, reason: .unseen)
                            : ApplicationFailure(raised, subject: subject, process: process) { ApplicationFailure(subject: subject, reason: .accessibility(raised)) }
                    }
                    presentation = .forward
                }
                let base: Int64 = [CGEventType.leftMouseDown, .rightMouseDown, .otherMouseDown].map { type in Int64(CGEventSource.counterForEventType(source.sourceStateID, eventType: type)) }.reduce(
                    1,
                    +,
                )
                var (timeline, pressed): ([(offset: Duration, event: CGEvent)], Int64) = ([], 0)
                for input in inputs {
                    let first: (number: Int64, clickState: Int64) = lifted.map { lifted in (lifted.number, lifted.clickState) } ?? (base + pressed, 1)
                    let next = try input.timeline(doubleClick: host.doubleClick, keyRepeat: host.keyRepeat, frame: .seconds(1) / host.rate, layout: host.layout, numbering: first, using: &generator)
                    let shift: Duration = timeline.last?.offset ?? .zero
                    (timeline, pressed) = (timeline + next.map { offset, event in (shift + offset, event) }, pressed + (input.contact.is(\.button) ? Int64(input.schedule.count) : 0))
                }
                let changes = await watch.messages()
                try await route.post(timeline, to: process, from: source, frame: .seconds(1) / host.rate, clock: clock)
                let (proven, proof): (Bool, Delivery.Proof)
                switch readBack {
                    case .posting: (proven, proof) = (true, .posting)
                    case .change: (proven, proof) = (try await watch.next(Self.proving, in: changes, clock: clock) != nil, .change)
                    case .value(let attribute, let before, let after):
                        let reading = { () async throws(ApplicationFailure) -> Fact.Scalar? in
                            try await watch.perform(on: action.ref) { element throws(ApplicationFailure) in try element?.read([attribute]).element.facts[attribute]?.scalar }
                        }
                        let first: Fact.Scalar? = try await reading()
                        let read: Fact.Scalar? = first == after || first != before ? first : try await watch.next(Self.proving, in: changes, clock: clock) == nil ? first : reading()
                        (proven, proof) = (read == after || read != before, .value)
                }
                guard proven else { throw ApplicationFailure(subject: "\(description) through \(route)", reason: .unseen) }
                let down: Input? = inputs.last { input in input.edges == [.down] }
                if partial { held[inputs[0].contact] = down.map { down in (base + pressed - 1, Int64(down.schedule.count), route) } }
                return Delivery(proof: proof, window: planned.window, route: route, tried: tried, presentation: presentation, pointer: pointer)
            } catch  where [.refused, .noWindow, .unseen].contains(error.reason) || error.reason.is(\.windowServer) {
                tried.append(Delivery.Attempt(rung: route.rung, failure: error.errorDescription ?? error.subject))
            }
        }
        throw ApplicationFailure(subject: "\(description) after \(tried.map { attempt in "\(attempt.rung.rawValue): \(attempt.failure)" }.formatted())", reason: .unseen)
    }

    private static func reach(_ element: AXUIElement?, ref: Int, revealing: Bool) throws(ApplicationFailure) -> (sample: Input.Sample, window: CGWindowID?, tried: [Delivery.Attempt]) {
        guard let element else { throw ApplicationFailure(subject: String(ref), reason: .stale(reached: 0, candidates: [])) }
        let read = try element.element(reading: frameAttributes)
        let revealed: AXError = revealing && read.element.actions.contains(kAXShowAlternateUIAction) ? AXUIElementPerformAction(element, kAXShowAlternateUIAction as CFString) : .success
        guard let point = read.element.activation ?? read.element.frame.map({ frame in CGPoint(x: frame.midX, y: frame.midY) }) else {
            throw ApplicationFailure(subject: "\(frameAttribute) of \(ref)", reason: .accessibility(.noValue))
        }
        let tried: [Delivery.Attempt] = revealed == .success ? [] : [Delivery.Attempt(rung: .accessibility, failure: "\(kAXShowAlternateUIAction) on \(ref) answered AXError \(revealed.rawValue)")]
        return (Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil), read.element.window, tried)
    }

    fileprivate static func proving(_ change: InstanceEvent.Change) -> Void? {
        switch change {
            case .started, .frontmost, .hidden, .status, .exited: nil
            case .opened, .closed, .asking, .focused, .value, .selection, .moved, .window: ()
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
        stepping: @Sendable (_ index: Int) -> Void = { _ in },
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
        let typing: Bool = steps.contains { step in
            switch step.action {
                case .set, .replace, .input(.key, _, _, _, _, _, _, _): true
                case .press, .select, .input: false
            }
        }
        let host = await MainActor.run {
            (
                layout: typing ? KeyboardLayout.current(keyboardType: keyboardType) : KeyboardLayout(strokes: [:]),
                rate: max(NSScreen.main?.maximumFramesPerSecond ?? 1, 1),
                doubleClick: Duration.seconds(NSEvent.doubleClickInterval),
                keyRepeat: (delay: Duration.seconds(NSEvent.keyRepeatDelay), interval: Duration.seconds(NSEvent.keyRepeatInterval)),
            )
        }
        let clipboard: Int = await intern([Pasteboard.locator(of: .general)])[0]
        var (outcomes, held, generator): ([Result<Step.Delivery, ApplicationFailure>], [Input.Contact: (number: Int64, clickState: Int64, route: Route)], SystemRandomNumberGenerator) = (
            [], [:], SystemRandomNumberGenerator(),
        )
        let popUps = { () -> [CGWindowID] in
            InstanceState.Window.all(.optionOnScreenOnly, of: process.pid)[process.pid, default: []].filter { row in row.layer >= InstanceState.Window.Band.popUpMenu.level }.map(\.number)
        }
        var (previous, shown, opened): (AsyncStream<Message>?, [CGWindowID], [[CGWindowID]]) = (nil, popUps(), [])
        for (index, step) in steps.enumerated() where outcomes.last.map({ outcome in outcome[case: \.success].map { delivery in delivery.proof != .exit } == true }) ?? true {
            stepping(index)
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
                    switch (failure.reason == .unanswered(step), failure.reason == .system(.noSuchProcess)) {
                        case (true, _): .success(Step.Delivery(proof: .unanswered))
                        case (_, true): .success(Step.Delivery(proof: .exit))
                        case (false, false): .failure(failure)
                    }
                }
            )
            let after: [CGWindowID] = popUps()
            (previous, shown, opened) = (events, after, opened + [after.filter { number in !shown.contains(number) }])
        }
        let inside: CGWindowID? = outcomes.last?[case: \.success]?.window
        let open: Set<CGWindowID> = instance.application.isActive ? [] : Set(shown).subtracting(inside.map { number in [number] } ?? [])
        for (index, numbers) in opened.enumerated() where !open.isDisjoint(with: numbers) {
            var closed: [Step.Delivery.PopUp] = []
            for number in numbers where open.contains(number) {
                closed.append(Step.Delivery.PopUp(window: number, closed: await dismiss(number, host: host, source: source, using: &generator, clock: clock)))
            }
            outcomes[index] = outcomes[index].map { delivery in
                var stated = delivery
                stated.popUps = closed
                return stated
            }
        }
        var last: InstanceState = await current
        let quieted = await messages().debounce(for: quiet, clock: clock).prefix(1).map { _ in await self.beat() }
        for await next in merge(await states().dropFirst(), quieted) {
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

    private nonisolated func dismiss(
        _ number: CGWindowID,
        host: Step.Host,
        source: CGEventSource,
        using generator: inout some RandomNumberGenerator,
        clock: some Clock<Duration> & Sendable,
    ) async -> Bool {
        let process: pid_t = instance.process.pid
        let application: AXUIElement = AXUIElementCreateApplication(process)
        let changes = await messages()
        _ = await Result { () async throws(ApplicationFailure) in
            let windows: [AXUIElement] = try application.elements(kAXWindowsAttribute)
            let window: AXUIElement? = windows.first { window in window.window == number }
            let cancelled: AXError? = try window.flatMap { window throws(ApplicationFailure) in
                try window.element(reading: []).element.actions.contains(kAXCancelAction) ? AXUIElementPerformAction(window, kAXCancelAction as CFString) : nil
            }
            let raised: AXError =
                try window.flatMap { window throws(ApplicationFailure) -> AXError? in
                    switch try (cancelled, application.settable(kAXFocusedWindowAttribute), window.settable(kAXMainAttribute)) {
                        case (.none, true, _): AXUIElementSetAttributeValue(application, kAXFocusedWindowAttribute as CFString, window)
                        case (.none, false, true): AXUIElementSetAttributeValue(window, kAXMainAttribute as CFString, kCFBooleanTrue)
                        case (.none, false, false), (.some, _, _): nil
                    }
                } ?? .success
            let focused: CGWindowID? = try application.elements(kAXFocusedWindowAttribute).first?.window
            let keyed: Bool = cancelled == nil && raised == .success && focused == number
            if keyed {
                let escape = try Input(contact: .key(.special(.escape)), path: [], hold: .zero, modifiers: [], schedule: once, edges: everyEdge)
                let timeline = try escape.timeline(
                    doubleClick: host.doubleClick,
                    keyRepeat: host.keyRepeat,
                    frame: .seconds(1) / host.rate,
                    layout: host.layout,
                    numbering: (1, 1),
                    using: &generator,
                )
                try await Route.process.post(timeline, to: process, from: source, frame: .seconds(1) / host.rate, clock: clock)
            }
            if keyed || cancelled == .success { _ = try await next(Step.proving, in: changes, clock: clock) }
        }
        return !InstanceState.Window.all(.optionOnScreenOnly, of: process)[process, default: []].contains { row in row.number == number }
    }
}
