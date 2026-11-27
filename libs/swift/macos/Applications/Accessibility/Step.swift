import AppKit
import AppleEvents
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
import CasePaths
import CasePathsCore
import CoreGraphics
import Encoding
package import Processes
import Results
import SPI
package import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let once = Input.Schedule(count: 1, every: nil, jitter: .zero)
private let everyEdge: Set<Input.Edge> = Set(Input.Edge.allCases)
private let downs: Set<CGEventType> = [.leftMouseDown, .rightMouseDown, .otherMouseDown]
private let selections: [String: String] = [
    kAXOutlineRole: kAXSelectedRowsAttribute, kAXTableRole: kAXSelectedRowsAttribute, kAXListRole: kAXSelectedChildrenAttribute, kAXBrowserRole: kAXSelectedChildrenAttribute,
]

// --- [MODELS] --------------------------------------------------------------------------

package struct Step: Decodable, Hashable, Sendable, CustomStringConvertible {
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

    enum TextRange: Decodable, Hashable, Sendable, CustomStringConvertible {
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

        var description: String {
            switch self {
                case .characters(let location, let length): "\(length) characters at \(location)"
                case .text(let text): text.debugDescription
            }
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

    package struct Delivery: Encodable, Equatable, Sendable {
        enum Proof: String, Encodable, Sendable {
            case answer, change, value, posting, unanswered, exit
        }

        struct Attempt: Encodable, Equatable, Sendable {
            let rung: Rung
            let failure: String
        }

        struct PopUp: Encodable, Equatable, Sendable {
            let window: CGWindowID
            let closed: Bool
        }

        let proof: Proof
        var window: CGWindowID?
        var route: Route?
        var tried: [Attempt] = []
        var presentation: Watch.Presentation?
        var pointer: SIMD2<Double>?
        var popUps: [PopUp] = []
    }

    fileprivate enum ReadBack {
        case posting
        case change
        case value(attribute: String, before: Fact.Scalar?, after: Fact.Scalar)

        init(contact: Input.Contact, edges: Set<Input.Edge>) {
            self =
                switch (contact, edges == everyEdge) {
                    case (.button, true), (.key, true): .change
                    case (.button, false), (.key, false), (.hover, _), (.wheel, _): .posting
                }
        }
    }

    @CasePathable
    fileprivate enum Course {
        case proven(Delivery.Proof)
        case answered(ReadBack)
        case posting(route: Route, inputs: [Input], readBack: ReadBack)
        case opening(action: String)
    }

    fileprivate enum Evidence: Sendable {
        case event
        case drawn(Screenshot)
        case restless
        case uncompared
        case nothing
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

    package init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        let longest: Double = replyDeadline / .milliseconds(1)
        let timed = { (key: CodingKeys) throws -> Duration? in
            try fields.decodeIfPresent(Double.self, forKey: key).map { (milliseconds: Double) throws -> Duration in
                guard (0...longest).contains(milliseconds) else {
                    let deadline: String = replyDeadline.formatted(.units(allowed: [.milliseconds], width: .abbreviated))
                    throw DecodingError.dataCorruptedError(forKey: key, in: fields, debugDescription: "\(milliseconds.formatted(numberFormat)) ms lies outside 0 to the reply deadline of \(deadline)")
                }
                return .milliseconds(milliseconds)
            }
        }
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
        let (hold, modifiers, windows): (Duration, Set<Modifier>?, [CGWindowID]?) = try (
            timed(.hold) ?? .zero, fields.decodeIfPresent(Set<Modifier>.self, forKey: .modifiers), fields.decodeIfPresent([CGWindowID].self, forKey: .windows),
        )
        let (range, named): (TextRange?, String?) = try (fields.decodeIfPresent(TextRange.self, forKey: .range), fields.decodeIfPresent(String.self, forKey: .action))
        let plane = Double(Int32.max)
        let samples: [Input.Sample] = path.compactMap { (aim: Aim) -> Input.Sample? in aim[case: \.point] }
        let pen: [Bool] = samples.map { (sample: Input.Sample) -> Bool in sample.pressure != nil || sample.tilt != nil }
        let reach: Double? = by.map { (by: SIMD2<Double>) -> Double in max(by.x.magnitude, by.y.magnitude).rounded() }
        let spacing: Duration = hold + schedule.jitter
        let run: Duration = (schedule.every ?? .zero) * min(max(schedule.count - 1, 0), Int(longest)) + hold
        let offPlane: Bool = samples.contains { (sample: Input.Sample) -> Bool in sample.at.x.magnitude > plane || sample.at.y.magnitude > plane }
        let unranged: Bool = samples.contains { (sample: Input.Sample) -> Bool in
            !(0...1).contains(sample.pressure ?? 0) || !(-1...1).contains(sample.tilt?.x ?? 0) || !(-1...1).contains(sample.tilt?.y ?? 0)
        }
        let rules: [(broken: Bool, key: CodingKeys, rule: String)] = [
            (named == kAXRaiseAction, .action, "\(kAXRaiseAction) takes focus from the frontmost application"),
            (!(1...Int(longest)).contains(schedule.count), .count, "count \(schedule.count) outside 1 to one press per millisecond of the reply deadline"),
            (run > replyDeadline, .every, "presses running \(run) past the reply deadline"),
            ((schedule.every ?? spacing) < spacing, .every, "presses every \(schedule.every ?? spacing) under the hold plus jitter \(spacing)"),
            (schedule.every == nil && schedule.jitter > .zero, .jitter, "jitter \(schedule.jitter) on a click run, which has no interval to vary"),
            (path.count > 1 && (by != nil || schedule.count > 1), .path, "a path of \(path.count) points on a wheel or more than one press"),
            (offPlane, .pointer, "a point off the plane of ±\(Int32.max)"),
            (reach.map { (reach: Double) -> Bool in !(1..<plane).contains(reach) } == true, .by, "a wheel delta under one whole unit or beyond ±\(Int32.max)"),
            (pen.contains(true) && contact != .button(.left), .button, "pressure or tilt off the left button"),
            (pen.contains(true) && (pen.contains(false) || samples.count < path.count), .path, "pressure or tilt on some points alone"),
            (unranged, .pointer, "pressure outside 0 to 1 or tilt outside -1 to 1"),
            (pointer != nil && at != nil, .at, "at on a pointer step, which aims by its pointer"),
        ]
        let broken: [(broken: Bool, key: CodingKeys, rule: String)] = rules.filter(\.broken)
        guard broken.isEmpty else { throw DecodingError.dataCorruptedError(forKey: broken[0].key, in: fields, debugDescription: broken.map(\.rule).formatted()) }
        switch (contact, at, value, range) {
            case (.some(let contact), _, _, _):
                action = .input(
                    contact: contact,
                    path: path,
                    focus: at,
                    hold: hold,
                    modifiers: modifiers ?? [],
                    schedule: schedule,
                    edges: edge.map { edge in [edge] } ?? everyEdge,
                    windows: windows ?? [],
                )
            case (.none, .some(let ref), .some(.text(let text)), .some(let range)): action = .replace(ref: ref, range: range, text: text)
            case (.none, .some, .some, .some): throw DecodingError.dataCorruptedError(forKey: .to, in: fields, debugDescription: "A range is replaced by text alone")
            case (.none, .some(let ref), .none, .some(let range)): action = .select(ref: ref, range: range)
            case (.none, .some(let ref), .some(let value), .none): action = .set(ref: ref, value: value)
            case (.none, .some(let ref), .none, .none): action = .press(ref: ref, action: named ?? kAXPressAction)
            case (.none, .none, _, _): throw DecodingError.keyNotFound(CodingKeys.at, DecodingError.Context(codingPath: fields.codingPath, debugDescription: "A step names at, key, or pointer"))
        }
    }

    package var description: String { describe(placing: true) }

    func describe(placing: Bool) -> String {
        switch action {
            case .press(_, let action): return "press of \(action)"
            case .set(_, let value): return "value \(value)"
            case .select(_, let range): return "selection of \(range)"
            case .replace(_, let range, let text): return "replacement of \(range) with \(text.debugDescription)"
            case .input(let contact, let path, _, _, let modifiers, let schedule, let edges, _):
                let edge: Input.Edge? = edges.count == 1 ? edges.first : nil
                let pen: Bool = path.contains { aim in aim[case: \.point].map { sample in sample.pressure != nil || sample.tilt != nil } == true }
                let (gesture, times): (String, Int) =
                    switch (contact, edge, path.count > 1, pen, schedule.count) {
                        case (_, .some(let edge), _, _, _): ("\(contact) \(edge.rawValue)", schedule.count)
                        case (.button, .none, true, true, _): ("pen stroke", 1)
                        case (.button, .none, true, false, _): ("\(contact) drag", 1)
                        case (.button, .none, false, _, 2): ("\(contact) double click", 1)
                        case (.button, .none, false, _, let count): ("\(contact) click", count)
                        case (.hover, .none, _, _, _), (.wheel, .none, _, _, _), (.key, .none, _, _, _): ("\(contact)", schedule.count)
                    }
                let held: String? = modifiers.isEmpty ? nil : "with \(Modifier.allCases.filter(modifiers.contains).map(\.rawValue).formatted())"
                let start: String? = placing ? path.first?[case: \.point].map { sample in "\(path.count > 1 ? "from" : "at") \(sample)" } : nil
                let end: String? = placing ? path.dropFirst().last?[case: \.point].map { sample in "to \(sample)" } : nil
                return [gesture, held, start, end, times > 1 ? "\(times) times" : nil].compactMap(\.self).joined(separator: " ")
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Step {
    fileprivate typealias Plan = (window: CGWindowID?, course: Course, tried: [Delivery.Attempt])
    fileprivate typealias Read = (element: Element, handles: [AXUIElement])
    fileprivate typealias Host = (layout: KeyboardLayout, frame: Duration, doubleClick: Duration, keyRepeat: (delay: Duration, interval: Duration))

    fileprivate static func window(holding sample: Input.Sample, of process: pid_t, among numbers: [CGWindowID] = []) -> (number: CGWindowID, owner: pid_t)? {
        let point = CGPoint(x: sample.at.x, y: sample.at.y)
        let own: CGWindowID? = InstanceState.Window.all(.optionOnScreenOnly, of: process)[process, default: []].first { row in
            (numbers.isEmpty || numbers.contains(row.number)) && row.frame.contains(point)
        }?.number
        if let own { return (own, process) }
        let listed: [pid_t: [InstanceState.Window]] = InstanceState.Window.all(.optionAll)
        return InstanceState.Activity.statusBars(in: listed).first { bar in
            var (hit, answering): (AXUIElement?, pid_t) = (nil, 0)
            guard bar.host != process, listed[bar.host, default: []].contains(where: { row in row.number == bar.number && row.frame.contains(point) }) else { return false }
            let found: AXError = unsafe AXUIElementCopyElementAtPosition(AXUIElementCreateApplication(bar.host), Float(point.x), Float(point.y), &hit)
            return found == .success && hit.map { hit in unsafe actualProcess(hit, &answering) == .success } == true && answering == process
        }
        .map { bar in (bar.number, bar.host) }
    }

    fileprivate func plan(on element: AXUIElement?, presented: Bool) throws(ApplicationFailure) -> Plan {
        guard let element else { throw ApplicationFailure(subject: "the reference", reason: .stale(candidates: [])) }
        let accessible: Bool = from.map { rung in rung == .accessibility } ?? true
        let shown: [String] =
            switch action {
                case .set, .select, .replace: [kAXValueAttribute, kAXSelectedAttribute]
                case .press, .input: []
            }
        var read = try element.element(reading: [kAXEnabledAttribute, kAXRoleAttribute, kAXFocusedAttribute, kAXWindowAttribute, kAXParentAttribute] + frameAttributes + shown)
        let point: CGPoint? = read.element.activation ?? read.element.frame.map { (frame: CGRect) -> CGPoint in CGPoint(x: frame.midX, y: frame.midY) }
        let sample: Input.Sample? = point.map { (point: CGPoint) -> Input.Sample in Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil) }
        let answering: pid_t? = element.process
        let drawn: (number: CGWindowID, owner: pid_t)? = read.element.window == nil ? answering.flatMap { process in sample.flatMap { sample in Self.window(holding: sample, of: process) } } : nil
        let hosted: Bool = drawn.map { (drawn: (number: CGWindowID, owner: pid_t)) -> Bool in drawn.owner != answering } == true
        read.element.window = read.element.window ?? drawn?.number
        let click: Result<Course, ApplicationFailure> =
            switch (sample, read.element.window) {
                case (.some(let sample), .some(let number)):
                    .success(
                        .posting(
                            route: .window(number: number),
                            inputs: [Input(contact: .button(.left), path: [sample], hold: .zero, modifiers: [], schedule: once, edges: everyEdge)],
                            readBack: .change,
                        )
                    )
                case (.some(let sample), .none): .failure(ApplicationFailure(subject: "the point at \(sample)", reason: .noWindow))
                case (.none, _): .failure(ApplicationFailure(subject: "reading the element's position", reason: .accessibility(.noValue)))
            }
        let unpressable: Bool =
            switch action {
                case .press(_, kAXPressAction): !read.element.actions.contains(kAXPressAction) && read.element.role != kAXRowRole
                case .press, .set, .select, .replace, .input: false
            }
        let held: [(item: AXUIElement, pressable: Bool)] = try (unpressable ? element.elements(kAXChildrenAttribute) : []).map {
            (child: AXUIElement) throws(ApplicationFailure) -> (item: AXUIElement, pressable: Bool) in
            (child, try child.element(reading: [kAXRoleAttribute]).element.actions.contains(kAXPressAction))
        }
        let pressable: [AXUIElement] = held.compactMap { (entry: (item: AXUIElement, pressable: Bool)) -> AXUIElement? in entry.pressable ? entry.item : nil }
        switch (action, read.element.facts[kAXEnabledAttribute]?.bool) {
            case (.press, false), (.set, false), (.select, false), (.replace, false): throw ApplicationFailure(subject: "the element", reason: .disabled)
            case (.press, _) where presented: return (read.element.window, .proven(.change), [])
            case (.press, _) where pressable.count == 1:
                let inner: Plan = try plan(on: pressable[0], presented: presented)
                guard case .opening(let opening) = inner.course else { return inner }
                let answers: [AXError] = [
                    AXUIElementSetMessagingTimeout(pressable[0], Float(messagingTimeout / 2 / .seconds(1))), AXUIElementPerformAction(pressable[0], opening as CFString),
                    AXUIElementSetMessagingTimeout(pressable[0], 0),
                ]
                if let refused = answers.first(where: { (answer: AXError) -> Bool in ![.success, .cannotComplete].contains(answer) }) {
                    throw ApplicationFailure(subject: "opening \(pressable[0].name)", reason: .accessibility(refused))
                }
                return (inner.window ?? read.element.window, .answered(.change), [])
            case (.press, _) where pressable.count > 1:
                let candidates: [String] = pressable.map(\.name)
                throw ApplicationFailure(
                    subject: "the container",
                    reason: .unmatched(Unmatched(subject: "press of its own, press one of its items by reference", query: nil, candidates: candidates)),
                )
            case (.press(_, kAXPressAction), _) where hosted: return try (read.element.window, click.get(), [])
            case (.press(_, let action), _) where accessible && (action == kAXShowMenuAction || action == kAXPressAction && read.element.role.map(pressOpened.contains) == true):
                return (read.element.window, .opening(action: action), [])
            case (.press(_, kAXPressAction), _) where read.element.role == kAXRowRole && !read.element.actions.contains(kAXPressAction):
                return try select(element, read: read, click: click)
            case (.press(_, let action), _) where accessible || action != kAXPressAction:
                let named: String? = read.element.actions.contains(action) ? action : read.element.descriptions.first { _, text in text == action }?.key
                let answer: AXError = named.map { (named: String) -> AXError in AXUIElementPerformAction(element, named as CFString) } ?? .actionUnsupported
                let pressing: String = action == kAXPressAction ? "the press" : action
                let refusal = ApplicationFailure(answer, subject: pressing, process: answering, answering: self)
                switch (answer, action, click) {
                    case (.success, _, _): return (read.element.window, .proven(.answer), [])
                    case (.actionUnsupported, _, _):
                        let taken: [String] = read.element.actions.filter { (held: String) -> Bool in held != named }
                        throw ApplicationFailure(subject: "the element", reason: .unmatched(Unmatched(subject: "action", query: action, candidates: taken)))
                    case (.failure, kAXPressAction, .success(let click)):
                        return (read.element.window, click, [Delivery.Attempt(rung: .accessibility, failure: refusal.errorDescription ?? pressing)])
                    case (_, _, _): throw refusal
                }
            case (.press, _): return try (read.element.window, click.get(), [])
            case (.set(_, .rect(let frame)), _):
                var (origin, size): (CGPoint, CGSize) = (frame.origin, frame.size)
                guard let position = unsafe AXValueCreate(.cgPoint, &origin), let extent = unsafe AXValueCreate(.cgSize, &size) else {
                    throw ApplicationFailure(subject: "making the frame value", reason: .refused)
                }
                let answer: AXError? = [(kAXSizeAttribute, extent), (kAXPositionAttribute, position), (kAXSizeAttribute, extent)].map { attribute, value in
                    AXUIElementSetAttributeValue(element, attribute as CFString, value)
                }
                .first { answer in answer != .success }
                let placed: Element = try element.read([kAXPositionAttribute, kAXSizeAttribute]).element
                let after: CGRect? = placed.facts[kAXPositionAttribute]?.point.flatMap { origin in placed.facts[kAXSizeAttribute]?.size.map { size in CGRect(origin: origin, size: size) } }
                switch (answer, after == frame) {
                    case (.none, true): return (read.element.window, .proven(.value), [])
                    case (.none, false): throw ApplicationFailure(subject: "the window", reason: .unchanged(after.map(Fact.Scalar.rect)))
                    case (.some(let answer), _):
                        throw ApplicationFailure(answer, subject: "moving the window", process: element.process, answering: self)
                }
            case (.set(_, let value), _): return try assign(value, to: element, read: read, click: click, presented: presented, accessible: accessible)
            case (.select(_, let range), _): return try select(range, in: element, read: read, replacing: nil)
            case (.replace(_, let range, let text), _): return try select(range, in: element, read: read, replacing: text)
            case (.input(let contact, _, _, let hold, let modifiers, let schedule, let edges, _), _):
                let typed = Input(contact: contact, path: [], hold: hold, modifiers: modifiers, schedule: schedule, edges: edges)
                return try aim([typed], at: element, read: read, click: click, accessible: accessible, readBack: ReadBack(contact: contact, edges: edges))
        }
    }

    private func select(_ row: AXUIElement, read: Read, click: Result<Course, ApplicationFailure>) throws(ApplicationFailure) -> Plan {
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
        let attempt = Delivery.Attempt(rung: .accessibility, failure: owner.map { _ in "the list left the row unselected" } ?? "no outline, table, list, or browser holds the row")
        return try (read.element.window, click.get(), [attempt])
    }

    private func select(_ range: TextRange, in element: AXUIElement, read: Read, replacing text: String?) throws(ApplicationFailure) -> Plan {
        var names: CFArray?
        let listed: AXError = unsafe AXUIElementCopyParameterizedAttributeNames(element, &names)
        guard [.success, .noValue, .attributeUnsupported, .notImplemented].contains(listed) else { throw element.failure(listed, reading: "the text search") }
        let parameterized: [String] = names as? [String] ?? []
        let unmatched = ApplicationFailure(subject: "the text", reason: .unmatched(Unmatched(subject: "text", query: "\(range)", candidates: [])))
        if case .text(let sought) = range, parameterized.contains(kAXSelectTextWithCriteriaParameterizedAttribute) {
            let replacing: [String: Any] = text.map { (text: String) -> [String: Any] in [kAXSelectTextActivity: kAXSelectTextActivityFindAndReplace, kAXSelectTextReplacementString: text] } ?? [:]
            let criteria: [String: Any] = replacing.merging([kAXSelectTextSearchStrings: [sought]]) { (first: Any, _: Any) -> Any in first }
            let chosen: CFTypeRef? = try element.parameterized(kAXSelectTextWithCriteriaParameterizedAttribute, criteria as CFDictionary)
            guard (chosen as? String).map({ (chosen: String) -> Bool in !chosen.isEmpty }) == true else { throw unmatched }
            return (read.element.window, .proven(.value), [])
        }
        let value: String = read.element.text(kAXValueAttribute) ?? ""
        let span: NSRange? =
            switch range {
                case .characters(let location, let length): location + length <= value.utf16.count ? NSRange(location: location, length: length) : nil
                case .text(let sought): value.range(of: sought).map { found in NSRange(found, in: value) }
            }
        guard let span else { throw unmatched }
        var bounds = CFRange(location: span.location, length: span.length)
        guard let boxed: AXValue = unsafe AXValueCreate(.cfRange, &bounds) else {
            throw ApplicationFailure(subject: "making the selection range", reason: .refused)
        }
        let window: AXUIElement? = read.element.facts[kAXWindowAttribute]?.element.map { id in read.handles[id] }
        let main: AXError =
            try window.map { window throws(ApplicationFailure) -> AXError in
                try window.settable(kAXMainAttribute) ? AXUIElementSetAttributeValue(window, kAXMainAttribute as CFString, kCFBooleanTrue) : .success
            } ?? .success
        let focused: AXError =
            main != .success || read.element.facts[kAXFocusedAttribute]?.bool == true ? main : AXUIElementSetAttributeValue(element, kAXFocusedAttribute as CFString, kCFBooleanTrue)
        let marked: Bool = parameterized.contains(kAXTextMarkerRangeForUIElementParameterizedAttribute)
        let whole: CFTypeRef? = try marked ? element.parameterized(kAXTextMarkerRangeForUIElementParameterizedAttribute, element) : nil
        let start: AXTextMarker? = whole.flatMap { (whole: CFTypeRef) -> AXTextMarker? in
            CFGetTypeID(whole) == AXTextMarkerRangeGetTypeID() ? AXTextMarkerRangeCopyStartMarker(unsafe unsafeDowncast(whole, to: AXTextMarkerRange.self)) : nil
        }
        let base: Int? = try start.flatMap { (start: AXTextMarker) throws(ApplicationFailure) -> Int? in
            (try element.parameterized(kAXIndexForTextMarkerParameterizedAttribute, start) as? NSNumber)?.intValue
        }
        let offsets: [Int] = base.map { (base: Int) -> [Int] in [base + span.location, base + span.location + span.length] } ?? []
        let markers: [AXTextMarker] = try offsets.map { (index: Int) throws(ApplicationFailure) -> CFTypeRef? in
            try element.parameterized(kAXTextMarkerForIndexParameterizedAttribute, index as NSNumber)
        }
        .compactMap { (marker: CFTypeRef?) -> AXTextMarker? in
            guard let marker, CFGetTypeID(marker) == AXTextMarkerGetTypeID() else { return nil }
            return unsafe unsafeDowncast(marker, to: AXTextMarker.self)
        }
        let marking: AXTextMarkerRange? = markers.count == 2 ? AXTextMarkerRangeCreate(nil, markers[0], markers[1]) : nil
        let selected: AXError =
            switch (focused, marked, marking) {
                case (.success, false, _): AXUIElementSetAttributeValue(element, kAXSelectedTextRangeAttribute as CFString, boxed)
                case (.success, true, .some(let marking)): AXUIElementSetAttributeValue(element, kAXSelectedTextMarkerRangeAttribute as CFString, marking)
                case (.success, true, .none): .noValue
                case (let refused, _, _): refused
            }
        var current: CFTypeRef?
        let copied: AXError = unsafe marked ? AXUIElementCopyAttributeValue(element, kAXSelectedTextMarkerRangeAttribute as CFString, &current) : .attributeUnsupported
        let selection: AXTextMarkerRange? = current.flatMap { (current: CFTypeRef) -> AXTextMarkerRange? in
            copied == .success && CFGetTypeID(current) == AXTextMarkerRangeGetTypeID() ? unsafe unsafeDowncast(current, to: AXTextMarkerRange.self) : nil
        }
        let ends: [AXTextMarker] = selection.map { (selection: AXTextMarkerRange) -> [AXTextMarker] in [AXTextMarkerRangeCopyStartMarker(selection), AXTextMarkerRangeCopyEndMarker(selection)] } ?? []
        let indexes: [Int] = try ends.map { (marker: AXTextMarker) throws(ApplicationFailure) -> Int? in
            (try element.parameterized(kAXIndexForTextMarkerParameterizedAttribute, marker) as? NSNumber)?.intValue
        }
        .compactMap(\.self)
        let shown: (location: Int, length: Int)? =
            switch (marked, base, indexes.count) {
                case (true, .some(let origin), 2): (indexes[0] - origin, indexes[1] - indexes[0])
                case (true, _, _): nil
                case (false, _, _): try element.read([kAXSelectedTextRangeAttribute]).element.facts[kAXSelectedTextRangeAttribute]?.range
            }
        guard selected == .success, shown?.location == span.location, shown?.length == span.length else {
            throw ApplicationFailure(subject: "the selection", reason: selected == .success ? .unchanged(nil) : .accessibility(selected))
        }
        guard let text else { return (read.element.window, .proven(.value), []) }
        let expected: String = (value as NSString).replacingCharacters(in: span, with: text)
        let keys: [(key: Key, modifiers: Set<Modifier>)] =
            text.isEmpty ? [(Key.special(.delete), Set<Modifier>())] : text.map { (character: Character) -> (key: Key, modifiers: Set<Modifier>) in (Key.character(character), []) }
        let keyed: Course = .posting(route: .process, inputs: Self.typing(keys), readBack: .value(attribute: kAXValueAttribute, before: .text(value), after: .text(expected)))
        guard try window?.read([kAXDocumentAttribute]).element.text(kAXDocumentAttribute) == nil else {
            return (read.element.window, keyed, [Delivery.Attempt(rung: .accessibility, failure: "replacing the text through Accessibility skips the document's undo")])
        }
        let written: AXError = AXUIElementSetAttributeValue(element, kAXSelectedTextAttribute as CFString, text as CFString)
        let after: String = try element.read([kAXValueAttribute]).element.text(kAXValueAttribute) ?? ""
        guard written != .success || after != expected else { return (read.element.window, .proven(.value), []) }
        return (
            read.element.window, keyed,
            [
                Delivery.Attempt(
                    rung: .accessibility,
                    failure: (written == .success
                        ? ApplicationFailure(subject: "the text", reason: .unchanged(.text(after))) : ApplicationFailure(subject: "replacing the text", reason: .accessibility(written)))
                        .errorDescription ?? "the text",
                )
            ],
        )
    }

    private static func scroll(_ element: AXUIElement?, by distance: SIMD2<Double>, unit: CGScrollEventUnit) throws(ApplicationFailure) -> Bool {
        let contentSize: String = NSAccessibility.Attribute.contentSize.rawValue
        var (next, area): (AXUIElement?, Read?) = (unit == .pixel ? element : nil, nil)
        while let current = next, area == nil {
            let up = try current.read([kAXRoleAttribute, kAXParentAttribute, frameAttribute, contentSize, kAXHorizontalScrollBarAttribute, kAXVerticalScrollBarAttribute])
            (next, area) = up.element.role == kAXScrollAreaRole ? (nil, up) : (up.element.facts[kAXParentAttribute]?.element.map { id in up.handles[id] }, nil)
        }
        guard let read = area, let frame = read.element.frame, let content = read.element.facts[contentSize]?.size else { return false }
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
            case (false, true): throw ApplicationFailure(subject: "the scroll bars", reason: .unchanged(nil))
        }
    }

    private static func typing(_ keys: [(key: Key, modifiers: Set<Modifier>)]) -> [Input] {
        keys.map { key, modifiers in Input(contact: .key(key), path: [], hold: .zero, modifiers: modifiers, schedule: once, edges: everyEdge) }
    }

    private func assign(
        _ target: Fact.Scalar,
        to element: AXUIElement,
        read: Read,
        click: Result<Course, ApplicationFailure>,
        presented: Bool,
        accessible: Bool,
    ) throws(ApplicationFailure) -> Plan {
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
            do { date = try Date(text, strategy: .iso8601) } catch { throw ApplicationFailure(subject: "reading the value as an ISO 8601 date", reason: .foundation(error as NSError)) }
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
                guard let boxed = value.accessibility else { throw ApplicationFailure(subject: "making the value \(value)", reason: .refused) }
                let answer = AXUIElementSetAttributeValue(element, attribute as CFString, boxed)
                let after: Fact.Scalar? = try answer == .success ? current() : nil
                let attempt = Delivery.Attempt(
                    rung: .accessibility,
                    failure: (answer == .success
                        ? ApplicationFailure(subject: "the value", reason: .unchanged(after)) : ApplicationFailure(subject: "writing the value", reason: .accessibility(answer)))
                        .errorDescription ?? "the value",
                )
                switch (answer, after == value || value.number.map({ number in after?.number == number }) == true, target.text) {
                    case (.success, true, _): return (read.element.window, .proven(.value), [])
                    case (_, false, .some): return try typed([attempt])
                    case (.success, false, .none): throw ApplicationFailure(subject: "the value", reason: .unchanged(after))
                    case (_, _, _):
                        throw ApplicationFailure(answer, subject: "writing the value", process: element.process, answering: self)
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
                    case (.success, false): throw ApplicationFailure(subject: "the value", reason: .unchanged(try current()))
                    case (_, _):
                        throw ApplicationFailure(answer, subject: "stepping the value", process: element.process, answering: self)
                }
            case (false, .some, _, .none): return try typed([])
            case (false, .none, _, .none): throw ApplicationFailure(subject: "the value", reason: .unchanged(shown))
        }
    }

    private func choose(
        _ item: String,
        on element: AXUIElement,
        listing menus: [AXUIElement],
        opening action: String?,
        read: Read,
        click: Result<Course, ApplicationFailure>,
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
                ApplicationFailure(subject: "closing the list after \(item.debugDescription) matched none of \(candidates.formatted())", reason: .accessibility(answer))
            } ?? ApplicationFailure(subject: "the item", reason: .unmatched(Unmatched(subject: "item", query: item, candidates: candidates)))
        }
        let before: Fact.Scalar? = read.element.facts[kAXValueAttribute]?.scalar
        let listed = try items(menus)
        let opening: String? = listed.isEmpty && !presented ? action : nil
        if let opening, opening == kAXShowMenuAction || read.element.role.map(pressOpened.contains) == true { return (read.element.window, .opening(action: opening), []) }
        if let opening {
            let answers: [AXError] = [
                AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1))), AXUIElementPerformAction(element, opening as CFString), AXUIElementSetMessagingTimeout(element, 0),
            ]
            if let refused = answers.first(where: { answer in ![.success, .cannotComplete].contains(answer) }) {
                throw ApplicationFailure(subject: "opening the list", reason: .accessibility(refused))
            }
        }
        let opened = try opening == nil ? menus : element.menus()
        let shown = try opening == nil ? listed : items(opened)
        guard shown.isEmpty else {
            guard let match = shown.first(where: { entry in entry.label == item }) else { throw unmatched(opening == nil ? [] : opened, shown.compactMap(\.label)) }
            let answer = AXUIElementPerformAction(match.element, kAXPressAction as CFString)
            guard answer == .success else { throw ApplicationFailure(subject: "pressing \(item.debugDescription)", reason: .accessibility(answer)) }
            let chosen: Course = read.element.role == kAXMenuButtonRole ? .proven(.answer) : .answered(.value(attribute: kAXValueAttribute, before: before, after: .text(item)))
            return (read.element.window, chosen, [])
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
        click: Result<Course, ApplicationFailure>,
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
        guard case .success(.posting(let route, let clicked, _)) = click else {
            throw ApplicationFailure(subject: "focusing the element", reason: written == .success ? .unchanged(nil) : .accessibility(written))
        }
        return (read.element.window, .posting(route: route, inputs: clicked + typed, readBack: readBack), tried)
    }

    fileprivate func run(
        on watch: Watch,
        clipboard: Int,
        host: Host,
        held: inout [Input.Contact: (number: Int64, clickState: Int64, route: Route, path: [Input.Sample])],
        source: CGEventSource,
        using generator: inout some RandomNumberGenerator,
        clock: some Clock<Duration> & Sendable,
    ) async throws(ApplicationFailure) -> Delivery {
        let process: pid_t = watch.instance.process.pid
        let changes: AsyncStream<Watch.Message> = await watch.messages()
        let hosted = { () -> Set<CGWindowID> in
            let rows: [[String: Any]] = CGWindowListCopyWindowInfo(.optionOnScreenOnly, kCGNullWindowID) as? [[String: Any]] ?? []
            return Set(
                rows.compactMap { (row: [String: Any]) -> CGWindowID? in
                    guard let owner = row[kCGWindowOwnerPID as String] as? pid_t, owner == process || responsiblePID(owner) == process else { return nil }
                    return row[kCGWindowNumber as String] as? CGWindowID
                }
            )
        }
        let shownBefore: Set<CGWindowID> = hosted()
        let planned: Plan
        var (presentation, pointer): (Watch.Presentation?, SIMD2<Double>?) = (nil, nil)
        switch action {
            case .set(clipboard, let value):
                guard let text = value.text else { throw ApplicationFailure(subject: "the clipboard", reason: .notText(value)) }
                try Pasteboard.write(text, to: .general)
                return Delivery(proof: .answer)
            case .press(let ref, _), .set(let ref, _), .select(let ref, _), .replace(let ref, _, _), .input(_, _, .some(let ref), _, _, _, _, _):
                let pending: Result<Plan, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Plan in
                    try await watch.perform(on: ref) { element throws(ApplicationFailure) in try plan(on: element, presented: false) }
                }
                let (through, opening): (Int?, String?) =
                    switch pending {
                        case .success(let read): (read.course[case: \.opening].map { _ in ref }, read.course[case: \.opening])
                        case .failure(let failure) where failure.reason.is(\.stale): (ref, nil)
                        case .failure(let failure): throw failure
                    }
                let presented: (value: Plan, presentation: Watch.Presentation?)
                do throws(ApplicationFailure) {
                    presented = try await watch.presenting(through, by: opening, clock: clock) { (_: [Container]) async throws(ApplicationFailure) -> Plan in
                        switch (pending, opening) {
                            case (.success(let read), .none): read
                            case (_, _): try await watch.perform(on: ref) { element throws(ApplicationFailure) in try plan(on: element, presented: opening != nil) }
                        }
                    }
                } catch  where error.reason == .unseen && opening != nil {
                    let clicked: Plan = try await watch.perform(on: ref) { element throws(ApplicationFailure) in try Step(action, from: .window).plan(on: element, presented: false) }
                    presented = (clicked, nil)
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
                            case .element(let ref): try await watch.perform(on: ref) { element throws(ApplicationFailure) in try Self.reach(element, revealing: contact == .hover) }
                        }
                    (samples, numbers, tried) = (samples + [reached.sample], numbers + (reached.window.map { number in [number] } ?? []), tried + reached.tried)
                }
                let shown: CGWindowID? = samples.first.flatMap { start in Self.window(holding: start, of: process, among: numbers) }?.number
                let focused: CGWindowID? =
                    contact.is(\.key)
                    ? try await watch.perform(on: nil) { (_: AXUIElement?) throws(ApplicationFailure) -> CGWindowID? in
                        do throws(ApplicationFailure) {
                            return try AXUIElementCreateApplication(process).elements(kAXFocusedWindowAttribute).first?.window
                        } catch let failure where failure.reason.is(\.unanswered) {
                            return nil
                        }
                    } : nil
                let window: CGWindowID? = shown ?? numbers.first ?? focused
                let input = Input(contact: contact, path: samples, hold: hold, modifiers: modifiers, schedule: schedule, edges: edges)
                guard let route = contact.is(\.key) ? Route.process : window.map(Route.window(number:)) else { throw ApplicationFailure(subject: "the point", reason: .noWindow) }
                planned = (window, .posting(route: route, inputs: [input], readBack: ReadBack(contact: contact, edges: edges)), tried)
                pointer = samples.last?.at
        }
        let matches = { (read: Fact.Scalar?, after: Fact.Scalar) -> Bool in read == after || after.number.map { (number: Double) -> Bool in read?.number == number } == true }
        let reading = { (attribute: String) async throws(ApplicationFailure) -> Fact.Scalar? in
            try await watch.perform(on: action.ref) { element throws(ApplicationFailure) in try element?.read([attribute]).element.facts[attribute]?.scalar }
        }
        let picture = { @Sendable (number: CGWindowID) async -> Screenshot? in
            let shots: Result<[Result<Screenshot, ApplicationFailure>], ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> [Result<Screenshot, ApplicationFailure>] in
                try await watch.capture(.windows(numbers: [number]))
            }
            return shots[case: \.success]?.first?[case: \.success]
        }
        let caret = { () async -> CGRect? in
            let read: Result<CGRect?, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> CGRect? in
                try await watch.perform(on: nil) { (_: AXUIElement?) throws(ApplicationFailure) -> CGRect? in
                    let focused: AXUIElement? = try AXUIElementCreateApplication(process).elements(kAXFocusedUIElementAttribute).first
                    guard let focused, let range = try focused.read([kAXSelectedTextRangeAttribute]).element.facts[kAXSelectedTextRangeAttribute]?.range else { return nil }
                    let spans: [CFRange] = [CFRange(location: max(range.location - 1, 0), length: 1), CFRange(location: range.location, length: 1)]
                    let boxes: [CGRect?] = try spans.map { (span: CFRange) throws(ApplicationFailure) -> CGRect? in
                        var bounds: CFRange = span
                        guard let boxed: AXValue = unsafe AXValueCreate(.cfRange, &bounds) else { return nil }
                        return try focused.parameterized(kAXBoundsForRangeParameterizedAttribute, boxed).flatMap { (value: CFTypeRef) -> CGRect? in Fact(value, interning: { _ in 0 })?.rect }
                    }
                    return boxes.compactMap(\.self).reduce(CGRect?.none) { (joint: CGRect?, box: CGRect) -> CGRect? in joint?.union(box) ?? box }
                }
            }
            return switch read {
                case .success(let box): box
                case .failure: nil
            }
        }
        let confirming = {
            (
                readBack: ReadBack,
                events: AsyncStream<Watch.Message>,
                rung: Rung,
                baseline: (shot: Screenshot?, changing: [CGRect], windows: Set<CGWindowID>, caret: CGRect?)
            ) async throws(ApplicationFailure) -> (proof: Delivery.Proof, note: String?) in
            let undrawn: Delivery.Proof = rung == .accessibility ? .answer : .posting
            let tied: @Sendable (InstanceEvent.Change) -> Void? = Watch.evidence(window: planned.window, control: action.ref, listed: baseline.windows)
            switch readBack {
                case .posting: return (.posting, nil)
                case .change:
                    let (insertion, changing): (CGRect?, [CGRect]) = (baseline.caret, baseline.changing)
                    let deadline = clock.now.advanced(by: messagingTimeout)
                    let evidence: Evidence =
                        if let earlier = baseline.shot, let number = earlier.windows.first {
                            try await race(
                                { () async throws(ApplicationFailure) -> Evidence in try await watch.next(tied, in: events, clock: clock) == nil ? .nothing : .event },
                                against: { () async throws(ApplicationFailure) -> Evidence in
                                    var (restless, compared) = (false, false)
                                    while clock.now < deadline, !Task.isCancelled, let shot = await picture(number) {
                                        guard let changed = shot.changed(from: earlier) else { continue }
                                        let regions: [CGRect] = changed.filter { (region: CGRect) -> Bool in insertion.map { (insertion: CGRect) -> Bool in !insertion.contains(region) } ?? true }
                                        let fresh: Bool = regions.contains { (region: CGRect) -> Bool in !changing.contains { (moving: CGRect) -> Bool in moving.intersects(region) } }
                                        if fresh { return .drawn(shot) }
                                        (restless, compared) = (restless || !regions.isEmpty, true)
                                    }
                                    do { try await clock.sleep(until: deadline, tolerance: nil) } catch { throw ApplicationFailure(subject: "the \(rung)", reason: .cancelled) }
                                    return restless ? .restless : compared ? .nothing : .uncompared
                                },
                            )
                        } else {
                            try await watch.next(tied, in: events, clock: clock) == nil ? .nothing : .event
                        }
                    switch evidence {
                        case .event: return (.change, nil)
                        case .nothing: return (undrawn, nil)
                        case .restless: return (undrawn, "the window was already changing there")
                        case .uncompared: return (undrawn, "its picture could not be compared")
                        case .drawn(let shot):
                            var last: Screenshot = shot
                            while clock.now < deadline, !Task.isCancelled, let number = shot.windows.first, let next = await picture(number), next.changed(from: last)?.isEmpty == false { last = next }
                            return (.change, nil)
                    }
                case .value(let attribute, let before, let after):
                    let deadline = clock.now.advanced(by: messagingTimeout)
                    var read: Fact.Scalar? = try await reading(attribute)
                    while read == before, !matches(read, after), clock.now < deadline, try await watch.next(tied, in: events, clock: clock) != nil {
                        read = try await reading(attribute)
                    }
                    switch (matches(read, after), read == before) {
                        case (true, _): return (.value, nil)
                        case (false, true): return (undrawn, nil)
                        case (false, false): throw ApplicationFailure(subject: "the value", reason: .unchanged(read))
                    }
            }
        }
        guard case .posting(let entry, let inputs, let readBack) = planned.course else {
            let proof: Delivery.Proof =
                switch planned.course {
                    case .proven(let proof): proof
                    case .answered(let readBack): try await confirming(readBack, changes, .accessibility, (nil, [], shownBefore, nil)).proof
                    case .opening, .posting: .change
                }
            return Delivery(proof: proof, window: planned.window, tried: planned.tried, presentation: presentation)
        }
        let lifted: (number: Int64, clickState: Int64, route: Route, path: [Input.Sample])? = inputs.count == 1 && inputs[0].edges == [.up] ? held[inputs[0].contact] : nil
        let below: Set<Rung> = Set(Rung.allCases.prefix { rung in rung != from && from != nil })
        let owner: pid_t? = planned.window.flatMap { (number: CGWindowID) -> pid_t? in windowDescription(of: number)?.owner }
        let foreign: Bool = owner.map { (owner: pid_t) -> Bool in owner != process } == true
        let climbed: [Route] = entry.ladder(window: planned.window).filter { (route: Route) -> Bool in !below.contains(route.rung) && !(foreign && route.rung == .focused) }
        let ladder: [Route] = lifted.map { lifted in [lifted.route] } ?? climbed
        let partial: Bool = inputs.contains { input in input.edges != everyEdge || input.contact == .hover }
        let target: NSRunningApplication = watch.instance.application
        let named: String = target.localizedName ?? "the application"
        let activating = { (application: NSRunningApplication, from: NSRunningApplication?, active: Bool) async throws(ApplicationFailure) -> Void in
            let name: String = application.localizedName ?? "the application"
            let subject: String = active ? "bringing \(name) forward" : "putting \(name) back in front"
            let activations: AsyncStream<Watch.Message> = await watch.messages()
            guard let from, application.activate(from: from, options: []) else { throw ApplicationFailure(subject: subject, reason: .refused) }
            let seen: Void? = try await watch.next({ (change: InstanceEvent.Change) -> Void? in if case .frontmost(active) = change { () } else { nil } }, in: activations, clock: clock)
            guard seen != nil else { throw ApplicationFailure(subject: subject, reason: .unseen) }
        }
        let (pictured, summoning): (CGWindowID?, Bool) =
            switch readBack {
                case .change: (foreign ? nil : planned.window, foreign)
                case .posting, .value: (nil, false)
            }
        let resolved: Screenshot? = if let pictured { await picture(pictured) } else { nil }
        let refreshed = clock.now.advanced(by: host.frame)
        var (tried, thrown, previous, delivered): ([Delivery.Attempt], ApplicationFailure?, NSRunningApplication?, Delivery?) = (planned.tried, nil, nil, nil)
        var undelivered: [Rung: ApplicationFailure.Reason] = [:]
        for route in partial ? Array(ladder.prefix(1)) : ladder where thrown == nil && delivered == nil {
            let attempt = await Result {
                () async throws(ApplicationFailure) -> (proof: Delivery.Proof, held: (number: Int64, clickState: Int64), restored: ApplicationFailure?, note: Delivery.Attempt?) in
                let front: pid_t? = route == .system && !foreign ? focusedProcesses().front : nil
                if let front, front != process {
                    try PermittedFrontList.shared.admit(process).get()
                    previous = NSRunningApplication(processIdentifier: front)
                    try await activating(target, previous, true)
                    presentation = .forward
                }
                let base: Int64 = downs.map { type in Int64(CGEventSource.counterForEventType(source.sourceStateID, eventType: type)) }.reduce(1, +)
                var (timeline, pressed): ([(offset: Duration, event: CGEvent)], Int64) = ([], 0)
                for input in inputs {
                    let first: (number: Int64, clickState: Int64) = lifted.map { lifted in (lifted.number, lifted.clickState) } ?? (base + pressed, 1)
                    let next = try input.timeline(doubleClick: host.doubleClick, keyRepeat: host.keyRepeat, frame: host.frame, layout: host.layout, numbering: first, using: &generator)
                    let shift: Duration = timeline.last?.offset ?? .zero
                    (timeline, pressed) = (timeline + next.map { offset, event in (shift + offset, event) }, pressed + (input.contact.is(\.button) ? Int64(input.schedule.count) : 0))
                }
                let pressing: CGEvent? = timeline.last { _, event in downs.contains(event.type) }?.event
                let numbering: (number: Int64, clickState: Int64) = (
                    pressing?.getIntegerValueField(.mouseEventNumber) ?? base + pressed - 1, pressing?.getIntegerValueField(.mouseEventClickState) ?? 1,
                )
                if resolved != nil, clock.now < refreshed {
                    do { try await clock.sleep(until: refreshed, tolerance: nil) } catch { throw ApplicationFailure(subject: "the \(route.rung)", reason: .cancelled) }
                }
                let shot: Screenshot? = if let pictured, resolved != nil { await picture(pictured) } else { nil }
                let paired: [CGRect]? = resolved.flatMap { (first: Screenshot) -> [CGRect]? in shot?.changed(from: first) }
                let learned: [CGRect] = if let pictured { await watch.restless(of: pictured) } else { [] }
                let changing: [CGRect]? = paired.map { (paired: [CGRect]) -> [CGRect] in paired + learned }
                let insertion: CGRect? = if changing != nil { await caret() } else { nil }
                let before: (shot: Screenshot?, changing: [CGRect], windows: Set<CGWindowID>, caret: CGRect?) = (changing == nil ? nil : shot, changing ?? [], hosted(), insertion)
                let ahead: Bool = summoning && focusedProcesses().front == process
                if summoning { try PermittedFrontList.shared.admit(process).get() }
                let fronts: AsyncStream<Watch.Message>? = if ahead { await watch.messages() } else { nil }
                let posted: AsyncStream<Watch.Message> = await watch.messages()
                await watch.posting()
                let restored: ApplicationFailure? = try await route.post(timeline, to: process, from: source, frame: host.frame, clock: clock)
                let confirmed: (proof: Delivery.Proof, note: String?) = try await confirming(readBack, posted, route.rung, before)
                let note: Delivery.Attempt? = confirmed.note.map { (note: String) -> Delivery.Attempt in Delivery.Attempt(rung: route.rung, failure: note) }
                guard summoning else { return (confirmed.proof, numbering, restored, note) }
                let shown: Set<CGWindowID> = hosted()
                if let fronts, shown.isStrictSubset(of: before.windows) {
                    _ = try await watch.next({ (change: InstanceEvent.Change) -> Void? in change[case: \.frontmost] == false ? () : nil }, in: fronts, clock: clock)
                }
                presentation =
                    switch (shown == before.windows, focusedProcesses().front == process, ahead) {
                        case (false, true, _): .forward
                        case (false, false, true): .restored(back: true)
                        case (_, _, _): presentation
                    }
                return (shown == before.windows ? .posting : .change, numbering, restored, note)
            }
            switch attempt {
                case .success(let reached) where reached.restored == nil:
                    let down: Input? = inputs.last { input in input.edges == [.down] }
                    if partial { held[inputs[0].contact] = down.map { (down: Input) in (reached.held.number, reached.held.clickState, route, Array(down.path.suffix(1))) } }
                    delivered = Delivery(proof: reached.proof, window: planned.window, route: route, tried: tried + [reached.note].compactMap(\.self), presentation: presentation, pointer: pointer)
                case .success(let reached): thrown = reached.restored
                case .failure(let failure):
                    switch failure.reason {
                        case .refused, .noWindow, .absent, .unseen, .symbolicHotKey, .notFrontmost, .notKeyFocus, .covered, .windowServer:
                            tried.append(Delivery.Attempt(rung: route.rung, failure: failure.errorDescription ?? failure.subject))
                            undelivered[route.rung] = failure.reason
                        default: thrown = failure
                    }
            }
        }
        let back: NSRunningApplication? = previous.flatMap { (previous: NSRunningApplication) -> NSRunningApplication? in focusedProcesses().front == process ? previous : nil }
        let restored: Result<Void, ApplicationFailure>? = if let back { await Result { () async throws(ApplicationFailure) in try await activating(back, target, false) } } else { nil }
        if var delivered {
            delivered.presentation =
                switch restored {
                    case .some(.success): .restored(back: true)
                    case .some(.failure): .restored(back: false)
                    case .none: delivered.presentation
                }
            return delivered
        }
        let restoring: String? =
            switch restored {
                case .some(.success): "\(named) came in front and was put back"
                case .some(.failure(let failure)): "\(named) came in front and stays there, as \(failure.errorDescription ?? failure.subject)"
                case .none: nil
            }
        throw thrown.map { thrown in ApplicationFailure(subject: restoring.map { restoring in "\(thrown.subject), after \(restoring)," } ?? thrown.subject, reason: thrown.reason) }
            ?? ApplicationFailure(subject: describe(placing: false), reason: .undelivered(routes: undelivered, focus: restoring))
    }

    private static func reach(_ element: AXUIElement?, revealing: Bool) throws(ApplicationFailure) -> (sample: Input.Sample, window: CGWindowID?, tried: [Delivery.Attempt]) {
        guard let element else { throw ApplicationFailure(subject: "the reference", reason: .stale(candidates: [])) }
        let read = try element.element(reading: frameAttributes)
        let revealed: AXError = revealing && read.element.actions.contains(kAXShowAlternateUIAction) ? AXUIElementPerformAction(element, kAXShowAlternateUIAction as CFString) : .success
        guard let point = read.element.activation ?? read.element.frame.map({ frame in CGPoint(x: frame.midX, y: frame.midY) }) else {
            throw ApplicationFailure(subject: "reading the position of \(element.name)", reason: .accessibility(.noValue))
        }
        let tried: [Delivery.Attempt] =
            revealed == .success
            ? [] : [Delivery.Attempt(rung: .accessibility, failure: ApplicationFailure(subject: "revealing \(element.name)", reason: .accessibility(revealed)).errorDescription ?? element.name)]
        return (Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil), read.element.window, tried)
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
    package nonisolated func act(
        _ steps: [Step],
        stepping: @Sendable (_ index: Int) -> Void = { _ in },
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> (
        outcomes: [Result<Step.Delivery, ApplicationFailure>],
        state: InstanceState,
        diagnosis: (usage: Result<ResourceUsage.Interval, Errno>, load: Result<SystemLoad.Interval, ProcessFailure>)?
    ) {
        let process: ProcessInstance = instance.process
        let began: (usage: Result<ResourceUsage, Errno>, load: Result<SystemLoad, ProcessFailure>) = (
            Result { () throws(Errno) in try ResourceUsage(of: process) }, Result { () throws(ProcessFailure) in try SystemLoad() },
        )
        guard let source = CGEventSource(stateID: .privateState) else { throw ApplicationFailure(subject: "private event source", reason: .refused) }
        let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: "Acting on process \(process.pid)")
        defer { ProcessInfo.processInfo.endActivity(activity) }
        let keyboardType: CGEventSourceKeyboardType = source.keyboardType
        let typing: Bool = steps.contains { step in
            switch step.action {
                case .set, .replace, .input(.key, _, _, _, _, _, _, _): true
                case .press, .select, .input: false
            }
        }
        let front: CGRect? = InstanceState.Window.all(.optionOnScreenOnly, of: process.pid)[process.pid, default: []].first { row in row.band == .normal }?.frame
        let host: Step.Host = await MainActor.run { () -> Step.Host in
            var (display, count): (CGDirectDisplayID, UInt32) = (0, 0)
            let found: Bool = front.map { (frame: CGRect) -> Bool in unsafe CGGetDisplaysWithRect(frame, 1, &display, &count) == .success && count > 0 } ?? false
            let screen: NSScreen? = NSScreen.screens.first { (screen: NSScreen) -> Bool in found && screen.cgDirectDisplayID == display } ?? NSScreen.screens.first
            return (
                layout: typing ? KeyboardLayout.current(keyboardType: keyboardType) : KeyboardLayout(strokes: [:]),
                frame: Duration.seconds(screen?.minimumRefreshInterval ?? 1),
                doubleClick: Duration.seconds(NSEvent.doubleClickInterval),
                keyRepeat: (delay: Duration.seconds(NSEvent.keyRepeatDelay), interval: Duration.seconds(NSEvent.keyRepeatInterval)),
            )
        }
        let clipboard: Int = await intern([Pasteboard.locator(of: .general)])[0]
        var outcomes: [Result<Step.Delivery, ApplicationFailure>] = []
        var held: [Input.Contact: (number: Int64, clickState: Int64, route: Route, path: [Input.Sample])] = [:]
        var generator = SystemRandomNumberGenerator()
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
            let added: [CGWindowID] = after.filter { (number: CGWindowID) -> Bool in !shown.contains(number) }
            (previous, shown, opened) = (events, after, opened + [added])
        }
        for (contact, hold) in held where hold.route == .system {
            let release = Input(contact: contact, path: hold.path, hold: .zero, modifiers: [], schedule: once, edges: [.up])
            _ = await Result { () async throws(ApplicationFailure) -> ApplicationFailure? in
                let timeline = try release.timeline(
                    doubleClick: host.doubleClick,
                    keyRepeat: host.keyRepeat,
                    frame: host.frame,
                    layout: host.layout,
                    numbering: (hold.number, hold.clickState),
                    using: &generator,
                )
                return try await hold.route.post(timeline, to: process.pid, from: source, frame: host.frame, clock: clock)
            }
        }
        let inside: CGWindowID? = outcomes.last?[case: \.success]?.window
        let excluded: Set<CGWindowID> = inside.map { (number: CGWindowID) -> Set<CGWindowID> in [number] } ?? []
        let open: Set<CGWindowID> = instance.application.isActive ? [] : Set(shown).subtracting(excluded)
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
        let stalled: Bool = last[case: \.running].map { activity in activity.status != .responding } == true
        let missed: Bool = outcomes.contains { outcome in outcome[case: \.success].map { delivery in delivery.proof == .unanswered } ?? true }
        guard stalled || missed else { return (outcomes, last, nil) }
        let usage = Result { () throws(Errno) in try ResourceUsage(of: process) }.flatMap { after in began.usage.map(after.since) }
        let load = began.load.flatMap { start in Result { () throws(ProcessFailure) in try SystemLoad.Interval(since: start) } }
        return (outcomes, last, (usage, load))
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
        let gone = { @Sendable () -> Bool in
            (CGWindowListCopyWindowInfo(.optionIncludingWindow, number) as? [[String: Any]])?.first?[kCGWindowIsOnscreen as String] as? Bool != true
        }
        let changes = await messages()
        _ = await Result { () async throws(ApplicationFailure) -> Void? in
            let window: AXUIElement? = try application.elements(kAXWindowsAttribute).first { (window: AXUIElement) -> Bool in window.window == number }
            let cancelled: AXError = window.map { (window: AXUIElement) -> AXError in AXUIElementPerformAction(window, kAXCancelAction as CFString) } ?? .actionUnsupported
            if cancelled != .success {
                let focused: CGWindowID? = try application.elements(kAXFocusedWindowAttribute).first?.window
                let escape = Input(contact: .key(.special(.escape)), path: [], hold: .zero, modifiers: [], schedule: once, edges: everyEdge)
                let timeline = try escape.timeline(doubleClick: host.doubleClick, keyRepeat: host.keyRepeat, frame: host.frame, layout: host.layout, numbering: (1, 1), using: &generator)
                let route: Route = focused == number ? .process : .focused(number: number)
                _ = try await route.post(timeline, to: process, from: source, frame: host.frame, clock: clock)
            }
            guard !gone() else { return () }
            return try await next({ (change: InstanceEvent.Change) -> Void? in (change.is(\.window) || change.is(\.closed)) && gone() ? () : nil }, in: changes, clock: clock)
        }
        return gone()
    }
}
