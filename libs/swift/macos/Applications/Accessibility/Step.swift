import AppKit
import AppleEvents
@unsafe @preconcurrency import ApplicationServices
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
            case answer, change, value, limit, posting, unanswered, exit, standing
        }

        struct Attempt: Encodable, Equatable, Sendable {
            let route: Route.Kind
            let failure: ApplicationFailure
        }

        struct PopUp: Encodable, Equatable, Sendable {
            let window: CGWindowID
            let closed: Bool
        }

        struct Resolved: Encodable, Equatable, Sendable {
            enum Effect: String, Encodable, Sendable {
                case pressed, selected, focused
            }

            let ref: Int
            let effect: Effect
        }

        struct Scrolled: Encodable, Equatable, Sendable {
            struct Row: Encodable, Equatable, Sendable {
                let index: Int
                let label: String?
            }

            let horizontal: Double?
            let vertical: Double?
            let rows: Int?
            let showing: [Row]
        }

        private enum CodingKeys: String, CodingKey {
            case proof, route, tried, presentation, popUps, scrolled, resolved, followed, surface, restless
        }

        let proof: Proof
        var window: CGWindowID?
        var route: Route?
        var tried: [Attempt] = []
        var presentation: Watch.Presentation?
        var popUps: [PopUp] = []
        var scrolled: Scrolled?
        var resolved: Resolved?
        var followed: Int?
        var surface: Int?
        var restless: Bool = false
    }

    fileprivate enum Scrolling {
        case writing
        case reading
        case reporting
    }

    fileprivate enum ReadBack {
        case posting
        case change
        case value(attribute: String, before: Fact.Scalar?, after: Fact.Scalar)
        case position(before: [Double])

        init(contact: Input.Contact, edges: Set<Input.Edge>) {
            self =
                switch (contact, edges == everyEdge) {
                    case (.button, true), (.key, true), (.wheel, true): .change
                    case (.button, false), (.key, false), (.wheel, false), (.hover, _), (.modifiers, _): .posting
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
        case drawn
        case nothing
    }

    private enum Tag: String, Decodable {
        case key, pointer, press, hold
    }

    private enum CodingKeys: String, CodingKey {
        case tag = "_tag"
        case at, action, to, range, key, modifiers, pointer, path, button, by, unit, hold, count, every, jitter, edge, from, windows
    }

    private struct Milliseconds: RawRepresentable, Decodable {
        let rawValue: Double

        init?(rawValue: Double) {
            let longest: Double = replyDeadline / .milliseconds(1)
            guard (0...longest).contains(rawValue) else { return nil }
            self.rawValue = rawValue
        }

        var duration: Duration { .milliseconds(rawValue) }
    }

    private typealias Rule = (broken: Bool, key: CodingKeys, rule: String)

    let action: Action
    let from: Route.Kind?

    init(_ action: Action, from: Route.Kind? = nil) {
        self.action = action
        self.from = from
    }

    package init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        let deadline: Double = replyDeadline / .milliseconds(1)
        let longest = Int(deadline)
        let pacing = { () throws -> (hold: Duration, schedule: Input.Schedule, edges: Set<Input.Edge>, rules: [Rule]) in
            let hold: Duration = try fields.decodeIfPresent(Milliseconds.self, forKey: .hold)?.duration ?? .zero
            let schedule = try Input.Schedule(
                count: fields.decodeIfPresent(Int.self, forKey: .count) ?? 1,
                every: fields.decodeIfPresent(Milliseconds.self, forKey: .every)?.duration,
                jitter: fields.decodeIfPresent(Milliseconds.self, forKey: .jitter)?.duration ?? .zero,
            )
            let edges: Set<Input.Edge> = try fields.decodeIfPresent(Input.Edge.self, forKey: .edge).map { (edge: Input.Edge) -> Set<Input.Edge> in [edge] } ?? everyEdge
            let spacing: Duration = hold + schedule.jitter
            let run: Duration = (schedule.every ?? .zero) * min(max(schedule.count - 1, 0), longest) + hold
            let rows: [Rule] = [
                (!(1...longest).contains(schedule.count), .count, "count \(schedule.count) outside 1 to one press per millisecond of the reply deadline"),
                (run > replyDeadline, .every, "presses running \(run) past the reply deadline"),
                ((schedule.every ?? spacing) < spacing, .every, "presses every \(schedule.every ?? spacing) under the hold plus jitter \(spacing)"),
                (schedule.every == nil && schedule.jitter > .zero, .jitter, "jitter \(schedule.jitter) on a click run, which has no interval to vary"),
            ]
            return (hold, schedule, edges, rows)
        }
        let rules: [Rule]
        switch try fields.decode(Tag.self, forKey: .tag) {
            case .key:
                let paced = try pacing()
                action = try .input(
                    contact: .key(fields.decode(Key.self, forKey: .key)),
                    path: [],
                    focus: fields.decodeIfPresent(Int.self, forKey: .at),
                    hold: paced.hold,
                    modifiers: fields.decodeIfPresent(Set<Modifier>.self, forKey: .modifiers) ?? [],
                    schedule: paced.schedule,
                    edges: paced.edges,
                    windows: [],
                )
                from = try fields.decodeIfPresent(Route.Kind.self, forKey: .from)
                rules = paced.rules
            case .pointer:
                let paced = try pacing()
                let path: [Aim] = try [fields.decode(Aim.self, forKey: .pointer)] + (fields.decodeIfPresent([Aim].self, forKey: .path) ?? [])
                let (by, button, unit): (SIMD2<Double>?, Button?, ScrollUnit?) = try (
                    fields.decodeIfPresent(SIMD2<Double>.self, forKey: .by), fields.decodeIfPresent(Button.self, forKey: .button), fields.decodeIfPresent(ScrollUnit.self, forKey: .unit),
                )
                let contact: Input.Contact = by.map { (by: SIMD2<Double>) -> Input.Contact in .wheel(by: by, unit: unit?.unit ?? .pixel) } ?? button?.contact ?? .button(.left)
                let plane = Double(Int32.max)
                let samples: [Input.Sample] = path.compactMap { (aim: Aim) -> Input.Sample? in aim[case: \.point] }
                let pen: [Bool] = samples.map { (sample: Input.Sample) -> Bool in sample.pressure != nil || sample.tilt != nil }
                let reach: Double? = by.map { (by: SIMD2<Double>) -> Double in max(by.x.magnitude, by.y.magnitude).rounded() }
                let offPlane: Bool = samples.contains { (sample: Input.Sample) -> Bool in sample.at.x.magnitude > plane || sample.at.y.magnitude > plane }
                let unranged: Bool = samples.contains { (sample: Input.Sample) -> Bool in
                    !(0...1).contains(sample.pressure ?? 0) || !(-1...1).contains(sample.tilt?.x ?? 0) || !(-1...1).contains(sample.tilt?.y ?? 0)
                }
                action = try .input(
                    contact: contact,
                    path: path,
                    focus: nil,
                    hold: paced.hold,
                    modifiers: fields.decodeIfPresent(Set<Modifier>.self, forKey: .modifiers) ?? [],
                    schedule: paced.schedule,
                    edges: paced.edges,
                    windows: fields.decodeIfPresent([CGWindowID].self, forKey: .windows) ?? [],
                )
                from = try fields.decodeIfPresent(Route.Kind.self, forKey: .from)
                let placed: [Rule] = [
                    (path.count > 1 && (by != nil || paced.schedule.count > 1), .path, "a path of \(path.count) points on a wheel or more than one press"),
                    (offPlane, .pointer, "a point off the plane of ±\(Int32.max)"),
                    (reach.map { (reach: Double) -> Bool in !(1..<plane).contains(reach) } == true, .by, "a wheel delta under one whole unit or beyond ±\(Int32.max)"),
                    (pen.contains(true) && contact != .button(.left), .button, "pressure or tilt off the left button"),
                    (pen.contains(true) && (pen.contains(false) || samples.count < path.count), .path, "pressure or tilt on some points alone"),
                    (unranged, .pointer, "pressure outside 0 to 1 or tilt outside -1 to 1"),
                ]
                rules = paced.rules + placed
            case .press:
                let ref: Int = try fields.decode(Int.self, forKey: .at)
                let named: String? = try fields.decodeIfPresent(String.self, forKey: .action)
                let (value, range): (Fact.Scalar?, TextRange?) = try (fields.decodeIfPresent(Fact.Scalar.self, forKey: .to), fields.decodeIfPresent(TextRange.self, forKey: .range))
                action =
                    switch (value, range) {
                        case (.some(.text(let text)), .some(let range)): .replace(ref: ref, range: range, text: text)
                        case (.some, .some): throw DecodingError.dataCorruptedError(forKey: .to, in: fields, debugDescription: "A range is replaced by text alone")
                        case (.none, .some(let range)): .select(ref: ref, range: range)
                        case (.some(let value), .none): .set(ref: ref, value: value)
                        case (.none, .none): .press(ref: ref, action: named ?? kAXPressAction)
                    }
                from = try fields.decodeIfPresent(Route.Kind.self, forKey: .from)
                rules = [(named == kAXRaiseAction, .action, "\(kAXRaiseAction) takes focus from the frontmost application")]
            case .hold:
                let modifiers: Set<Modifier> = try fields.decode(Set<Modifier>.self, forKey: .modifiers)
                action = try .input(
                    contact: .modifiers,
                    path: [],
                    focus: nil,
                    hold: fields.decode(Milliseconds.self, forKey: .hold).duration,
                    modifiers: modifiers,
                    schedule: once,
                    edges: everyEdge,
                    windows: [],
                )
                from = nil
                rules = [(modifiers.isEmpty, .modifiers, "a hold of no modifier")]
        }
        let broken: [Rule] = rules.filter(\.broken)
        guard broken.isEmpty else { throw DecodingError.dataCorruptedError(forKey: broken[0].key, in: fields, debugDescription: broken.map(\.rule).formatted()) }
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
                        case (.hover, .none, _, _, _), (.wheel, .none, _, _, _), (.key, .none, _, _, _), (.modifiers, .none, _, _, _): ("\(contact)", schedule.count)
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
    fileprivate typealias Scroll = (window: CGWindowID?, host: CGWindowID?, position: [Double], proof: Delivery.Proof?, tried: [Delivery.Attempt], scrolled: Delivery.Scrolled?)
    fileprivate typealias Reached = (number: CGWindowID, owner: pid_t, element: AXUIElement?, host: CGWindowID?, frame: CGRect?)

    fileprivate static func window(holding sample: Input.Sample, of process: pid_t, among numbers: [CGWindowID] = []) throws(ApplicationFailure) -> Reached? {
        let point = CGPoint(x: sample.at.x, y: sample.at.y)
        let serving = { (element: AXUIElement) -> pid_t? in
            var served: pid_t = 0
            return unsafe actualProcess(element, &served) == .success ? served : nil
        }
        let hit = { (root: AXUIElement) -> AXUIElement? in
            var found: AXUIElement?
            return unsafe AXUIElementCopyElementAtPosition(root, Float(point.x), Float(point.y), &found) == .success ? found : nil
        }
        let first: AXUIElement? = hit(AXUIElementCreateApplication(process))
        var (under, host): (AXUIElement?, AXUIElement?) = (first, first)
        while let held = host {
            let served: [AXUIElement] = try held.elements(kAXChildrenAttribute).filter { (child: AXUIElement) -> Bool in serving(child) != serving(held) }
            let holding: [AXUIElement?] = try served.map { (child: AXUIElement) throws(ApplicationFailure) -> AXUIElement? in
                try child.read([frameAttribute]).element.frame?.contains(point) == true ? child : nil
            }
            let deeper: AXUIElement? = holding.compactMap(\.self).first.flatMap(hit)
            (under, host) = (deeper ?? under, deeper.flatMap { (deeper: AXUIElement) -> AXUIElement? in serving(deeper) == serving(held) ? nil : deeper })
        }
        let number = { (element: AXUIElement) -> CGWindowID? in
            var number: CGWindowID = 0
            return unsafe windowNumber(element, &number) == .success ? number : nil
        }
        let (drawing, presenting): (CGWindowID?, CGWindowID?) = (under.flatMap(number), first.flatMap(number))
        let described: (bounds: CGRect, owner: pid_t)? = drawing.flatMap(windowDescription(of:))
        let named: Bool = numbers.isEmpty || [drawing, presenting].contains { (window: CGWindowID?) -> Bool in window.map(numbers.contains) == true }
        let frame: CGRect? = try under.flatMap { (under: AXUIElement) throws(ApplicationFailure) -> CGRect? in try under.read([frameAttribute]).element.frame }
        if let drawing, let described, named { return (drawing, described.owner, under, presenting, frame) }
        let own: CGWindowID? = InstanceState.Window.all(.optionOnScreenOnly, of: process)[process, default: []].first { row in
            (numbers.isEmpty || numbers.contains(row.number)) && row.frame.contains(point)
        }?.number
        if let own { return (own, process, under, own, frame) }
        let listed: [pid_t: [InstanceState.Window]] = InstanceState.Window.all(.optionAll)
        return InstanceState.Activity.statusBars(in: listed).first { bar in
            var (hit, answering): (AXUIElement?, pid_t) = (nil, 0)
            guard bar.host != process, listed[bar.host, default: []].contains(where: { row in row.number == bar.number && row.frame.contains(point) }) else { return false }
            let found: AXError = unsafe AXUIElementCopyElementAtPosition(AXUIElementCreateApplication(bar.host), Float(point.x), Float(point.y), &hit)
            return found == .success && hit.map { hit in unsafe actualProcess(hit, &answering) == .success } == true && answering == process
        }
        .map { bar in (bar.number, bar.host, nil, nil, nil) }
    }

    fileprivate func plan(on element: AXUIElement?, presented: Bool) throws(ApplicationFailure) -> Plan {
        guard let element else { throw ApplicationFailure(subject: "the reference", reason: .notOpen(nearest: [])) }
        let accessible: Bool = from.map { kind in kind == .accessibility } ?? true
        let shown: [String] =
            switch action {
                case .set: [kAXValueAttribute, kAXSelectedAttribute, kAXValueIncrementAttribute]
                case .select, .replace: [kAXValueAttribute, kAXSelectedAttribute]
                case .press, .input: []
            }
        let stated: [String] = [kAXEnabledAttribute, kAXRoleAttribute, kAXSubroleAttribute, kAXFocusedAttribute, kAXWindowAttribute, kAXParentAttribute]
        var read = try element.element(reading: stated + frameAttributes + shown)
        let secure: Bool =
            switch action {
                case .press: false
                case .set, .select, .replace, .input: try read.element.text(kAXSubroleAttribute) == kAXSecureTextFieldSubrole ? !element.settable(kAXValueAttribute) : false
            }
        guard !secure else { throw ApplicationFailure(subject: "the element", reason: .secureField) }
        let point: CGPoint? = read.element.activation ?? read.element.frame.map { (frame: CGRect) -> CGPoint in CGPoint(x: frame.midX, y: frame.midY) }
        let sample: Input.Sample? = point.map { (point: CGPoint) -> Input.Sample in Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil) }
        let answering: pid_t? = element.process
        let drawn: Reached? =
            try read.element.window == nil
            ? answering.flatMap { (process: pid_t) throws(ApplicationFailure) -> Reached? in
                try sample.flatMap { (sample: Input.Sample) throws(ApplicationFailure) -> Reached? in try Self.window(holding: sample, of: process) }
            } : nil
        let hosted: Bool = drawn.map { (drawn: Reached) -> Bool in drawn.owner != answering } == true
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
                try Watch.press(pressable[0], action: opening)
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
                        return (read.element.window, click, [Delivery.Attempt(route: .accessibility, failure: refusal)])
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
                    case (.none, false): throw ApplicationFailure(subject: "the window", reason: .readBack(after.map(Fact.Scalar.rect)))
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
        let refusal: ApplicationFailure.Reason =
            switch (owner, written) {
                case (.none, _): .unmatched(Unmatched(subject: "list", query: nil, candidates: selections.keys.sorted()))
                case (.some, .none): .refused
                case (.some, .some(.success)): .readBack(nil)
                case (.some, .some(let answer)): .accessibility(answer)
            }
        return try (read.element.window, click.get(), [Delivery.Attempt(route: .accessibility, failure: ApplicationFailure(subject: "selecting the row", reason: refusal))])
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
            throw ApplicationFailure(subject: "the selection", reason: selected == .success ? .readBack(nil) : .accessibility(selected))
        }
        guard let text else { return (read.element.window, .proven(.value), []) }
        let expected: String = (value as NSString).replacingCharacters(in: span, with: text)
        let keys: [(key: Key, modifiers: Set<Modifier>)] =
            text.isEmpty ? [(Key.special(.delete), Set<Modifier>())] : text.map { (character: Character) -> (key: Key, modifiers: Set<Modifier>) in (Key.character(character), []) }
        let keyed: Course = .posting(route: .process, inputs: Self.typing(keys), readBack: .value(attribute: kAXValueAttribute, before: .text(value), after: .text(expected)))
        guard try window?.read([kAXDocumentAttribute]).element.text(kAXDocumentAttribute) == nil else {
            return (read.element.window, keyed, [Delivery.Attempt(route: .accessibility, failure: ApplicationFailure(subject: "replacing the text outside the document's undo", reason: .refused))])
        }
        let written: AXError = AXUIElementSetAttributeValue(element, kAXSelectedTextAttribute as CFString, text as CFString)
        let after: String = try element.read([kAXValueAttribute]).element.text(kAXValueAttribute) ?? ""
        guard written != .success || after != expected else { return (read.element.window, .proven(.value), []) }
        return (
            read.element.window, keyed,
            [
                Delivery.Attempt(
                    route: .accessibility,
                    failure: written == .success
                        ? ApplicationFailure(subject: "the text", reason: .readBack(.text(after))) : ApplicationFailure(subject: "replacing the text", reason: .accessibility(written)),
                )
            ],
        )
    }

    private static func scroll(_ aimed: AXUIElement?, at sample: Input.Sample?, of process: pid_t, by distance: SIMD2<Double>, scrolling: Scrolling) throws(ApplicationFailure) -> Scroll {
        let contentSize: String = NSAccessibility.Attribute.contentSize.rawValue
        let bars: [String] = [kAXHorizontalScrollBarAttribute, kAXVerticalScrollBarAttribute]
        let reached: Reached? = try sample.flatMap { (sample: Input.Sample) throws(ApplicationFailure) -> Reached? in try Self.window(holding: sample, of: process) }
        let deep: AXUIElement? = reached.flatMap { (reached: Reached) -> AXUIElement? in reached.owner != process ? reached.element : nil }
        let under: AXUIElement? = deep ?? aimed ?? reached?.element
        var number: CGWindowID = 0
        let window: CGWindowID? = under.flatMap { (under: AXUIElement) -> CGWindowID? in unsafe windowNumber(under, &number) == .success ? number : nil }
        let asked: [Int] = bars.indices.filter { (axis: Int) -> Bool in distance[axis] != 0 }
        var (next, area): (AXUIElement?, (read: Read, span: SIMD2<Double>?)?) = (under, nil)
        while let current = next, area == nil {
            let up: Read = try current.read([kAXRoleAttribute, kAXParentAttribute, frameAttribute, contentSize] + bars)
            let span: SIMD2<Double>? = up.element.facts[contentSize]?.size.flatMap { (content: CGSize) -> SIMD2<Double>? in
                up.element.frame.map { (frame: CGRect) -> SIMD2<Double> in SIMD2(content.width - frame.width, content.height - frame.height) }
            }
            let exceeds: Bool = span.map { (span: SIMD2<Double>) -> Bool in asked.contains { (axis: Int) -> Bool in span[axis] > 0 } } ?? true
            let parent: AXUIElement? = up.element.facts[kAXParentAttribute]?.element.map { (id: Int) -> AXUIElement in up.handles[id] }
            (next, area) = (parent, up.element.role == kAXScrollAreaRole && exceeds ? (read: up, span: span) : nil)
        }
        guard let area else { return (window, reached?.host, [], nil, [], nil) }
        let host: CGWindowID? =
            reached?.host
            ?? area.read.element.frame.flatMap { (frame: CGRect) -> CGWindowID? in
                InstanceState.Window.all(.optionOnScreenOnly, of: process)[process, default: []].first { (row: InstanceState.Window) -> Bool in
                    row.frame.contains(CGPoint(x: frame.midX, y: frame.midY))
                }?.number
            }
        let showing = { () throws(ApplicationFailure) -> Delivery.Scrolled? in
            guard scrolling != .reading else { return nil }
            let values: [Double?] = try bars.map { (attribute: String) throws(ApplicationFailure) -> Double? in
                let bar: AXUIElement? = area.read.element.facts[attribute]?.element.map { (id: Int) -> AXUIElement in area.read.handles[id] }
                return try bar?.read([kAXValueAttribute]).element.facts[kAXValueAttribute]?.number
            }
            let holders: Set<String> = [kAXGroupRole, kAXScrollAreaRole, kAXSplitGroupRole, kAXLayoutAreaRole]
            var (queue, cursor, list): ([AXUIElement], Int, (list: AXUIElement, rows: [AXUIElement])?) = (try area.read.handles[0].elements(kAXChildrenAttribute), 0, nil)
            while list == nil, cursor < queue.count {
                let held: Read = try queue[cursor].read([kAXRoleAttribute, kAXVisibleRowsAttribute, kAXChildrenAttribute])
                let rows: [AXUIElement] = held.element.facts[kAXVisibleRowsAttribute]?.elements.map { (id: Int) -> AXUIElement in held.handles[id] } ?? []
                let children: [AXUIElement] = held.element.facts[kAXChildrenAttribute]?.elements.map { (id: Int) -> AXUIElement in held.handles[id] } ?? []
                (queue, cursor, list) = (
                    queue + (rows.isEmpty && held.element.role.map(holders.contains) == true ? children : []), cursor + 1, rows.isEmpty ? nil : (list: queue[cursor], rows: rows),
                )
            }
            var total: CFIndex = 0
            let counted: AXError =
                list.map { (list: (list: AXUIElement, rows: [AXUIElement])) -> AXError in unsafe AXUIElementGetAttributeValueCount(list.list, kAXRowsAttribute as CFString, &total) } ?? .noValue
            let rows: [Delivery.Scrolled.Row] = try (list?.rows ?? []).map { (row: AXUIElement) throws(ApplicationFailure) -> Delivery.Scrolled.Row? in
                let cells: [AXUIElement] = try row.elements(kAXChildrenAttribute)
                let inner: [AXUIElement] = try cells.map { (cell: AXUIElement) throws(ApplicationFailure) -> [AXUIElement] in try cell.elements(kAXChildrenAttribute) }.flatMap(\.self)
                let texts: [String?] = try ([row] + cells + inner).map { (element: AXUIElement) throws(ApplicationFailure) -> String? in
                    let fields: Element = try element.read([kAXRoleAttribute, kAXTitleAttribute, kAXValueAttribute]).element
                    return fields.role == kAXStaticTextRole ? fields.text(kAXValueAttribute) : fields.text(kAXTitleAttribute)
                }
                let index: Int? = try row.read([kAXIndexAttribute]).element.facts[kAXIndexAttribute]?.number.map(Int.init)
                return index.map { (index: Int) -> Delivery.Scrolled.Row in Delivery.Scrolled.Row(index: index, label: texts.compactMap(\.self).first) }
            }
            .compactMap(\.self)
            return Delivery.Scrolled(horizontal: values[0], vertical: values[1], rows: counted == .success ? total : nil, showing: rows)
        }
        let moving: [Int] = asked.filter { (axis: Int) -> Bool in area.span.map { (span: SIMD2<Double>) -> Bool in span[axis] > 0 } ?? true }
        let scrollers: [(axis: Int, bar: AXUIElement, before: Double)] = try moving.map { (axis: Int) throws(ApplicationFailure) -> (axis: Int, bar: AXUIElement, before: Double)? in
            guard let bar = area.read.element.facts[bars[axis]]?.element.map({ (id: Int) -> AXUIElement in area.read.handles[id] }) else { return nil }
            return try bar.read([kAXValueAttribute]).element.facts[kAXValueAttribute]?.number.map { (before: Double) -> (axis: Int, bar: AXUIElement, before: Double) in (axis, bar, before) }
        }
        .compactMap(\.self)
        let position: [Double] = scrollers.map(\.before)
        guard let span = area.span, scrollers.count == moving.count else { return (window, host, position, nil, [], try scrolling == .reporting ? showing() : nil) }
        let goals: [(bar: AXUIElement, before: Double, goal: Double)] = scrollers.map { (axis: Int, bar: AXUIElement, before: Double) -> (bar: AXUIElement, before: Double, goal: Double) in
            (bar, before, min(max(before - distance[axis] / span[axis], 0), 1))
        }
        let pending: [(bar: AXUIElement, before: Double, goal: Double)] = goals.filter { axis in axis.goal != axis.before }
        guard !pending.isEmpty else { return (window, host, position, .limit, [], try showing()) }
        let settable: [Bool] = try (scrolling == .writing ? pending : []).map { axis throws(ApplicationFailure) -> Bool in try axis.bar.settable(kAXValueAttribute) }
        guard scrolling == .writing, settable.allSatisfy(\.self) else { return (window, host, position, nil, [], try scrolling == .reporting ? showing() : nil) }
        let moved: [Bool] = try pending.map { axis throws(ApplicationFailure) -> Bool in
            let answer: AXError = AXUIElementSetAttributeValue(axis.bar, kAXValueAttribute as CFString, axis.goal as NSNumber)
            let after: Double? = try answer == .success ? axis.bar.read([kAXValueAttribute]).element.facts[kAXValueAttribute]?.number : nil
            return after.map { (after: Double) -> Bool in after != axis.before } ?? false
        }
        guard moved.contains(true) else { throw ApplicationFailure(subject: "the scroll bar", reason: .readBack(nil)) }
        return (window, host, position, .value, [], try showing())
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
        let value: Fact.Scalar =
            switch (shown, target.number, target.text.flatMap(Bool.init), date) {
                case (.bool, _, .some(let flag), _): .bool(flag)
                case (.number, .some(let number), _, _): .number(number)
                case (.date, _, _, .some(let date)): .date(date)
                case (_, _, _, _): target
            }
        let step: Double? = read.element.facts[kAXValueIncrementAttribute]?.number.flatMap { (step: Double) -> Double? in step > 0 ? step : nil }
        let toggled = { (tried: [Delivery.Attempt]) throws(ApplicationFailure) -> Plan in
            let pressed: Plan = try Step(.press(ref: 0, action: kAXPressAction), from: from).plan(on: element, presented: presented)
            let readBack: ReadBack = .value(attribute: attribute, before: shown, after: value)
            let course: Course =
                switch pressed.course {
                    case .posting(let route, let inputs, _): .posting(route: route, inputs: inputs, readBack: readBack)
                    case .proven, .answered: .answered(readBack)
                    case .opening: throw ApplicationFailure(subject: "the value", reason: .readBack(shown))
                }
            return (pressed.window, course, tried + pressed.tried)
        }
        if case .bool = value, shown?.states(value) == true { return (read.element.window, .proven(.standing), []) }
        switch (settable, target.text, accessible && (!menus.isEmpty || opening != nil), stepping) {
            case (true, _, _, _):
                guard let boxed = value.accessibility else { throw ApplicationFailure(subject: "making the value \(value)", reason: .refused) }
                let answer = AXUIElementSetAttributeValue(element, attribute as CFString, boxed)
                let after: Fact.Scalar? = try answer == .success ? current() : nil
                let attempt = Delivery.Attempt(
                    route: .accessibility,
                    failure: answer == .success
                        ? ApplicationFailure(subject: "the value", reason: .readBack(after)) : ApplicationFailure(subject: "writing the value", reason: .accessibility(answer)),
                )
                switch (answer, after?.states(value, within: step) == true, target.text, value) {
                    case (.success, true, _, _): return (read.element.window, .proven(.value), [])
                    case (_, false, .some, _): return try typed([attempt])
                    case (_, false, .none, .bool): return try toggled([attempt])
                    case (.success, false, .none, _): throw ApplicationFailure(subject: "the value", reason: .readBack(after))
                    case (_, _, _, _):
                        throw ApplicationFailure(answer, subject: "writing the value", process: element.process, answering: self)
                }
            case (false, .some(let item), true, _): return try choose(item, on: element, listing: menus, opening: opening, read: read, click: click, presented: presented)
            case (false, _, _, .some(let goal)):
                var (held, answer) = (shown?.number, AXError.success)
                while let reached = held, reached != goal, answer == .success {
                    answer = AXUIElementPerformAction(element, (reached < goal ? kAXIncrementAction : kAXDecrementAction) as CFString)
                    let next = try answer == .success ? current()?.number ?? reached : reached
                    held = answer != .success || abs(goal - next) < abs(goal - reached) ? next : nil
                }
                guard answer == .success else { throw ApplicationFailure(answer, subject: "stepping the value", process: element.process, answering: self) }
                let after: Fact.Scalar? = try current()
                guard after?.states(.number(goal), within: step) == true else { throw ApplicationFailure(subject: "the value", reason: .readBack(after)) }
                return (read.element.window, .proven(.value), [])
            case (false, .some, _, .none): return try typed([])
            case (false, .none, _, .none):
                guard case .bool = value else { throw ApplicationFailure(subject: "the value", reason: .readBack(shown)) }
                return try toggled([])
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
        if let opening { try Watch.press(element, action: opening) }
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
            throw ApplicationFailure(subject: "focusing the element", reason: written == .success ? .readBack(nil) : .accessibility(written))
        }
        return (read.element.window, .posting(route: route, inputs: clicked + typed, readBack: readBack), tried)
    }

    fileprivate func run(
        on watch: Watch,
        clipboard: Int,
        host: Host,
        held: inout [Input.Contact: (number: Int64, clickState: Int64, route: Route, path: [Input.Sample])],
        streams: inout [CGWindowID: (output: Result<StreamOutput, ApplicationFailure>, call: AsyncStream<Void>.Continuation)],
        source: CGEventSource,
        using generator: inout some RandomNumberGenerator,
        clock: some Clock<Duration> & Sendable,
    ) async throws(ApplicationFailure) -> Delivery {
        let process: pid_t = watch.instance.process.pid
        for stream in streams.values { stream.output[case: \.success]?.disturb() }
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
        let scrolling = { @Sendable (mode: Scrolling) async throws(ApplicationFailure) -> Scroll? in
            guard case .input(.wheel(let by, let unit), let path, .none, _, _, _, _, _) = action, let aim = path.first else { return nil }
            let allowed: Scrolling = unit == .pixel || mode != .writing ? mode : .reading
            return try await watch.perform(on: aim[case: \.element]) { (element: AXUIElement?) throws(ApplicationFailure) -> Scroll in
                try Self.scroll(element, at: aim[case: \.point], of: process, by: by, scrolling: allowed)
            }
        }
        let planned: Plan
        var (presentation, surface, landing, resolution, revealed): (Watch.Presentation?, Int?, Reached?, Delivery.Resolved?, Delivery.Scrolled?) = (nil, nil, nil, nil, nil)
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
                        case .failure(let failure) where failure.reason.is(\.notOpen): (ref, nil)
                        case .failure(let failure): throw failure
                    }
                let presented: (value: (plan: Plan, shown: Int?), presentation: Watch.Presentation?)
                do throws(ApplicationFailure) {
                    presented = try await watch.presenting(through, by: opening, clock: clock) { (surfaces: [Container]) async throws(ApplicationFailure) -> (plan: Plan, shown: Int?) in
                        switch (pending, opening) {
                            case (.success(let read), .none): (read, nil)
                            case (_, _): (try await watch.perform(on: ref) { element throws(ApplicationFailure) in try plan(on: element, presented: opening != nil) }, surfaces.last?.ref)
                        }
                    }
                } catch  where error.reason == .noChangeSeen && opening != nil {
                    let clicked: Plan = try await watch.perform(on: ref) { element throws(ApplicationFailure) in try Step(action, from: .window).plan(on: element, presented: false) }
                    presented = ((clicked, nil), nil)
                }
                surface =
                    switch (action, opening, presented.presentation) {
                        case (.press, .some, .none): presented.value.shown
                        case (_, _, _): nil
                    }
                (planned, presentation) = (surface.map { (_: Int) -> Plan in (presented.value.plan.window, .proven(.standing), []) } ?? presented.value.plan, presented.presentation)
            case .input(let contact, let path, .none, let hold, let modifiers, let schedule, let edges, let windows):
                let scrolled: Scroll? = try await scrolling(from == nil || from == .accessibility ? .writing : .reading)
                if let proof = scrolled?.proof {
                    if proof == .value, let window = scrolled?.host ?? scrolled?.window { await watch.invalidate(window: window) }
                    return Delivery(proof: proof, scrolled: scrolled?.scrolled)
                }
                var (samples, numbers, tried): ([Input.Sample], [CGWindowID], [Delivery.Attempt]) = ([], windows, scrolled?.tried ?? [])
                for case .element(let ref) in path {
                    do throws(ApplicationFailure) {
                        try await watch.perform(on: ref) { (_: AXUIElement?) throws(ApplicationFailure) -> Void in }
                    } catch  where error.reason.is(\.notOpen) {
                        presentation = try await watch.presenting(ref, clock: clock) { (_: [Container]) async throws(ApplicationFailure) -> Void in }.presentation ?? presentation
                    }
                }
                for aim in path {
                    let reached: (sample: Input.Sample, window: CGWindowID?, tried: [Delivery.Attempt], scrolled: Delivery.Scrolled?) =
                        switch aim {
                            case .point(let sample): (sample, nil, [], nil)
                            case .element(let ref): try await watch.perform(on: ref) { element throws(ApplicationFailure) in try Self.reach(element, revealing: contact == .hover) }
                        }
                    (samples, numbers, tried, revealed) = (
                        samples + [reached.sample], numbers + (reached.window.map { number in [number] } ?? []), tried + reached.tried, reached.scrolled ?? revealed,
                    )
                }
                landing = try samples.first.flatMap { (start: Input.Sample) throws(ApplicationFailure) -> Reached? in try Self.window(holding: start, of: process, among: numbers) }
                let shown: CGWindowID? = landing?.number
                let keyed: Bool = contact.is(\.key) || contact == .modifiers
                let focused: CGWindowID? =
                    keyed
                    ? try await watch.perform(on: nil) { (_: AXUIElement?) throws(ApplicationFailure) -> CGWindowID? in
                        do throws(ApplicationFailure) {
                            return try AXUIElementCreateApplication(process).elements(kAXFocusedWindowAttribute).first?.window
                        } catch let failure where failure.reason.is(\.unanswered) || failure.reason.is(\.unansweredMayHaveRun) {
                            return nil
                        }
                    } : nil
                let window: CGWindowID? = scrolled?.window ?? shown ?? numbers.first ?? focused
                let input = Input(contact: contact, path: samples, hold: hold, modifiers: modifiers, schedule: schedule, edges: edges)
                guard let route = keyed ? Route.process : window.map(Route.window(number:)) else { throw ApplicationFailure(subject: "the point", reason: .noWindow) }
                let position: [Double] = scrolled?.position ?? []
                let readBack: ReadBack = position.isEmpty ? ReadBack(contact: contact, edges: edges) : .position(before: position)
                let plain: Bool =
                    (from == nil || from == .accessibility) && contact == .button(.left) && samples.count == 1 && edges == everyEdge && schedule.count == 1 && modifiers.isEmpty
                    && hold == .zero
                var meaning: AXUIElement? = plain ? landing?.element : nil
                var (meant, effect): (AXUIElement?, Delivery.Resolved.Effect) = (nil, .pressed)
                while let current = meaning, meant == nil {
                    let read: Read = try current.element(reading: [kAXRoleAttribute, kAXParentAttribute])
                    let takes: Bool = read.element.actions.contains(kAXPressAction) || read.element.role == kAXRowRole
                    effect = read.element.actions.contains(kAXPressAction) ? .pressed : .selected
                    let parent: AXUIElement? = read.element.facts[kAXParentAttribute]?.element.map { (id: Int) -> AXUIElement in read.handles[id] }
                    (meaning, meant) = (takes || read.element.role.map([kAXWindowRole, kAXApplicationRole].contains) == true ? nil : parent, takes ? current : nil)
                }
                let pressed: Plan? = try meant.map { (meant: AXUIElement) throws(ApplicationFailure) -> Plan in
                    try Step(.press(ref: 0, action: kAXPressAction), from: from).plan(on: meant, presented: false)
                }
                let answered: Bool = pressed.map { (pressed: Plan) -> Bool in pressed.course.is(\.proven) || pressed.course.is(\.answered) } == true
                let named: Int? = if let meant, answered { await watch.reference(of: meant) } else { nil }
                resolution = named.map { (ref: Int) -> Delivery.Resolved in Delivery.Resolved(ref: ref, effect: effect) }
                planned =
                    if let pressed, !pressed.course.is(\.opening) {
                        (pressed.window ?? window, pressed.course, tried + pressed.tried)
                    } else {
                        (window, .posting(route: route, inputs: [input], readBack: readBack), tried)
                    }
        }
        let reading = { (attribute: String) async throws(ApplicationFailure) -> Fact.Scalar? in
            try await watch.perform(on: action.ref) { element throws(ApplicationFailure) in try element?.read([attribute]).element.facts[attribute]?.scalar }
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
                seen: (events: AsyncStream<Watch.Message>, frames: AsyncStream<StreamOutput.Frame>?),
                lingering: (events: AsyncStream<Watch.Message>, frames: AsyncStream<StreamOutput.Frame>?)?,
                kind: Route.Kind,
                baseline: (output: StreamOutput?, posted: UInt64, changing: [CGRect], windows: Set<CGWindowID>, caret: CGRect?, values: AsyncStream<Void>?)
            ) async throws(ApplicationFailure) -> (proof: Delivery.Proof, restless: Bool, followed: Int?) in
            let undrawn: Delivery.Proof = kind == .accessibility ? .answer : .posting
            let tied: @Sendable (InstanceEvent.Change) -> Void? = Watch.evidence(window: planned.window, control: action.ref, listed: baseline.windows)
            let started = clock.now
            let answer: Duration = await watch.answerTime
            switch readBack {
                case .posting: return (.posting, false, nil)
                case .change:
                    let (insertion, changing, posted): (CGRect?, [CGRect], UInt64) = (baseline.caret, baseline.changing, baseline.posted)
                    let proving = { @Sendable (frame: StreamOutput.Frame) -> Bool in
                        let regions: [CGRect] = frame.dirty.filter { (region: CGRect) -> Bool in insertion.map { (insertion: CGRect) -> Bool in !insertion.contains(region) } ?? true }
                        let fresh: Bool = regions.contains { (region: CGRect) -> Bool in !changing.contains { (moving: CGRect) -> Bool in moving.intersects(region) } }
                        return frame.drew && frame.displayed.map { (displayed: UInt64) -> Bool in displayed > posted } == true && fresh
                    }
                    let heard = { @Sendable (events: AsyncStream<Watch.Message>, limit: Duration) async throws(ApplicationFailure) -> Evidence in
                        try await watch.next(tied, in: events, clock: clock, within: limit - started.duration(to: clock.now)) == nil ? .nothing : .event
                    }
                    let painted = { @Sendable (frames: AsyncStream<StreamOutput.Frame>?, limit: Duration) async throws(ApplicationFailure) -> Evidence in
                        if await frames?.contains(where: proving) == true { return .drawn }
                        do { try await clock.sleep(until: started.advanced(by: limit), tolerance: nil) } catch { throw ApplicationFailure(subject: "the \(kind)", reason: .cancelled) }
                        return .nothing
                    }
                    let sight = { @Sendable (shown: (events: AsyncStream<Watch.Message>, frames: AsyncStream<StreamOutput.Frame>?), limit: Duration) async throws(ApplicationFailure) -> Evidence in
                        try await race(
                            { () async throws(ApplicationFailure) -> Evidence in try await heard(shown.events, limit) },
                            against: { () async throws(ApplicationFailure) -> Evidence in try await painted(shown.frames, limit) },
                        )
                    }
                    let late = { @Sendable () async -> (drawn: Bool, after: Duration)? in
                        guard let lingering else { return nil }
                        let found: Result<Evidence, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Evidence in try await sight(lingering, messagingTimeout) }
                        return switch found {
                            case .success(.event): (false, started.duration(to: clock.now))
                            case .success(.drawn): (true, started.duration(to: clock.now))
                            case .success(.nothing), .failure: nil
                        }
                    }
                    guard try await sight(seen, answer) == .nothing else {
                        await watch.prove(after: started.duration(to: clock.now))
                        return (.change, false, nil)
                    }
                    let followed: Int? = if lingering != nil, answer < messagingTimeout { await watch.follow(late) } else { nil }
                    return (undrawn, baseline.output?.drew(after: posted) == true, followed)
                case .value(let attribute, let before, let after):
                    let deadline = started.advanced(by: answer)
                    var read: Fact.Scalar? = try await reading(attribute)
                    while read == before, read?.states(after) != true, clock.now < deadline, try await watch.next(tied, in: seen.events, clock: clock) != nil {
                        read = try await reading(attribute)
                    }
                    switch (read?.states(after) == true, read == before) {
                        case (true, _):
                            await watch.prove(after: started.duration(to: clock.now))
                            return (.value, false, nil)
                        case (false, true): return (undrawn, false, nil)
                        case (false, false): throw ApplicationFailure(subject: "the value", reason: .readBack(read))
                    }
                case .position(let before):
                    let after: [Double] = try await race(
                        { () async throws(ApplicationFailure) -> [Double] in
                            var notices: AsyncStream<Void>.Iterator? = baseline.values?.makeAsyncIterator()
                            var read: [Double] = try await scrolling(.reading)?.position ?? before
                            while read == before, await notices?.next() != nil { read = try await scrolling(.reading)?.position ?? before }
                            return read
                        },
                        against: { () async throws(ApplicationFailure) -> [Double] in
                            do { try await clock.sleep(for: answer) } catch { throw ApplicationFailure(subject: "the \(kind)", reason: .cancelled) }
                            return before
                        },
                    )
                    let moved: Bool = after != before && after.count == before.count
                    if moved { await watch.prove(after: started.duration(to: clock.now)) }
                    return (moved ? .value : undrawn, false, nil)
            }
        }
        guard case .posting(let entry, let inputs, let readBack) = planned.course else {
            let tied: @Sendable (InstanceEvent.Change) -> Void? = Watch.evidence(window: planned.window, control: action.ref, listed: shownBefore)
            let asked: Watch.Presentation?? =
                switch planned.course {
                    case .proven(.answer):
                        try await watch.next(
                            { (change: InstanceEvent.Change) -> Watch.Presentation?? in tied(change).map { _ in change[case: \.asking].flatMap(\.presentation) } },
                            in: changes,
                            clock: clock,
                            within: watch.answerTime,
                        )
                    case .proven, .answered, .opening, .posting: nil
                }
            let proof: Delivery.Proof =
                switch planned.course {
                    case .proven(let proof): asked == nil ? proof : .change
                    case .answered(let readBack): try await confirming(readBack, (changes, nil), nil, .accessibility, (nil, 0, [], shownBefore, nil, nil)).proof
                    case .opening, .posting: .change
                }
            return Delivery(
                proof: proof,
                window: planned.window,
                tried: planned.tried,
                presentation: presentation ?? asked.flatMap(\.self),
                scrolled: revealed,
                resolved: resolution,
                surface: surface,
            )
        }
        let lifted: (number: Int64, clickState: Int64, route: Route, path: [Input.Sample])? = inputs.count == 1 && inputs[0].edges == [.up] ? held[inputs[0].contact] : nil
        let below: Set<Route.Kind> = Set(Route.Kind.allCases.prefix { kind in kind != from && from != nil })
        let owner: pid_t? = planned.window.flatMap { (number: CGWindowID) -> pid_t? in windowDescription(of: number)?.owner }
        let foreign: Bool = owner.map { (owner: pid_t) -> Bool in owner != process } == true
        let barred: Bool =
            foreign
            && planned.window.map { (number: CGWindowID) -> Bool in
                InstanceState.Activity.statusBars(in: InstanceState.Window.all(.optionAll)).contains { (bar: (host: pid_t, number: CGWindowID)) -> Bool in bar.number == number }
            } == true
        let routes: [Route] =
            lifted.map { lifted in [lifted.route] }
            ?? entry.fallbacks(window: planned.window).filter { (route: Route) -> Bool in !below.contains(route.kind) && !(foreign && route.kind == .focused) }
        let partial: Bool = inputs.contains { input in input.edges != everyEdge || input.contact == .hover }
        let target: NSRunningApplication = watch.instance.application
        let named: String = target.localizedName ?? "the application"
        let activating = { (application: NSRunningApplication, from: NSRunningApplication?, active: Bool) async throws(ApplicationFailure) -> Void in
            let name: String = application.localizedName ?? "the application"
            let subject: String = active ? "bringing \(name) forward" : "putting \(name) back in front"
            let activations: AsyncStream<Watch.Message> = await watch.messages()
            guard let from, application.activate(from: from, options: []) else { throw ApplicationFailure(subject: subject, reason: .refused) }
            let seen: Void? = try await watch.next({ (change: InstanceEvent.Change) -> Void? in if case .frontmost(active) = change { () } else { nil } }, in: activations, clock: clock)
            guard seen != nil else { throw ApplicationFailure(subject: subject, reason: .noChangeSeen) }
        }
        let (pictured, summoning): (CGWindowID?, Bool) =
            switch readBack {
                case .change: (barred ? nil : foreign ? landing?.host : planned.window, barred)
                case .posting, .value, .position: (nil, false)
            }
        if let pictured, streams[pictured] == nil {
            let (calling, call) = AsyncStream<Void>.makeStream()
            streams[pictured] = (await watch.frames(of: pictured, during: calling), call)
        }
        let drawing: Result<StreamOutput, ApplicationFailure>? = if let pictured { streams[pictured]?.output } else { nil }
        let values: AsyncStream<Void>? = if case .position = readBack { watch.notices(of: kAXValueChangedNotification) } else { nil }
        var (tried, thrown, previous, delivered): ([Delivery.Attempt], ApplicationFailure?, NSRunningApplication?, Delivery?) = (planned.tried, nil, nil, nil)
        var undelivered: [Route.Kind: ApplicationFailure.Reason] = [:]
        for route in partial ? Array(routes.prefix(1)) : routes where thrown == nil && delivered == nil {
            let attempt = await Result {
                () async throws(ApplicationFailure) -> (
                    proof: Delivery.Proof, held: (number: Int64, clickState: Int64), restored: ApplicationFailure?, note: Delivery.Attempt?, restless: Bool, followed: Int?
                ) in
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
                let output: StreamOutput? = drawing?[case: \.success]
                let changing: [CGRect] = pictured.map { (number: CGWindowID) -> [CGRect] in watch.restless(of: number) } ?? []
                let insertion: CGRect? = if output != nil { await caret() } else { nil }
                let listed: Set<CGWindowID> = hosted()
                let ahead: Bool = summoning && focusedProcesses().front == process
                if summoning { try PermittedFrontList.shared.admit(process).get() }
                let fronts: AsyncStream<Watch.Message>? = if ahead { await watch.messages() } else { nil }
                let aimed: Input.Sample? = inputs.contains { (input: Input) -> Bool in input.contact.is(\.button) } ? inputs.first?.path.first : nil
                if let landing, let aimed {
                    let under: Reached? = try Self.window(holding: aimed, of: process)
                    guard under?.number == landing.number, under?.frame == landing.frame else { throw ApplicationFailure(subject: "what lay under the point", reason: .notOpen(nearest: [])) }
                }
                let seen: (events: AsyncStream<Watch.Message>, frames: AsyncStream<StreamOutput.Frame>?) = await (watch.messages(), output?.frames())
                let lingering: (events: AsyncStream<Watch.Message>, frames: AsyncStream<StreamOutput.Frame>?) = await (watch.messages(), output?.frames())
                let instant: UInt64 = mach_absolute_time()
                let restored: ApplicationFailure? = try await route.post(timeline, to: process, from: source, frame: host.frame, clock: clock)
                let confirmed: (proof: Delivery.Proof, restless: Bool, followed: Int?) = try await confirming(
                    readBack,
                    seen,
                    lingering,
                    route.kind,
                    (output, instant, changing, listed, insertion, values),
                )
                let note: Delivery.Attempt? = drawing?[case: \.failure].map { (failure: ApplicationFailure) -> Delivery.Attempt in Delivery.Attempt(route: route.kind, failure: failure) }
                guard summoning else { return (confirmed.proof, numbering, restored, note, confirmed.restless, confirmed.followed) }
                let shown: Set<CGWindowID> = hosted()
                if let fronts, shown.isStrictSubset(of: listed) {
                    _ = try await watch.next({ (change: InstanceEvent.Change) -> Void? in change[case: \.frontmost] == false ? () : nil }, in: fronts, clock: clock)
                }
                presentation =
                    switch (shown == listed, focusedProcesses().front == process, ahead) {
                        case (false, true, _): .forward
                        case (false, false, true): .restored(back: true)
                        case (_, _, _): presentation
                    }
                return (shown == listed ? .posting : .change, numbering, restored, note, false, nil)
            }
            switch attempt {
                case .success(let reached) where reached.restored == nil:
                    let down: Input? = inputs.last { input in input.edges == [.down] }
                    if partial { held[inputs[0].contact] = down.map { (down: Input) in (reached.held.number, reached.held.clickState, route, Array(down.path.suffix(1))) } }
                    delivered = Delivery(
                        proof: reached.proof,
                        window: planned.window,
                        route: route,
                        tried: tried + [reached.note].compactMap(\.self),
                        presentation: presentation,
                        followed: reached.followed,
                        restless: reached.restless,
                    )
                case .success(let reached): thrown = reached.restored
                case .failure(let failure):
                    switch failure.reason {
                        case .refused, .noWindow, .absent, .noChangeSeen, .systemShortcut, .frontElsewhere, .keyFocusElsewhere, .covered, .windowServer:
                            tried.append(Delivery.Attempt(route: route.kind, failure: failure))
                            undelivered[route.kind] = failure.reason
                        default: thrown = failure
                    }
            }
        }
        let back: NSRunningApplication? = previous.flatMap { (previous: NSRunningApplication) -> NSRunningApplication? in focusedProcesses().front == process ? previous : nil }
        let restored: Result<Void, ApplicationFailure>? = if let back { await Result { () async throws(ApplicationFailure) in try await activating(back, target, false) } } else { nil }
        if var delivered {
            let wheeled: Bool = if case .input(.wheel, _, .none, _, _, _, _, _) = action { true } else { false }
            let reported: Scroll? = try await wheeled && delivered.proof == .value ? scrolling(.reporting) : nil
            (delivered.scrolled, delivered.resolved) = (reported?.scrolled ?? revealed, resolution)
            if let window = reported?.host ?? reported?.window { await watch.invalidate(window: window) }
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

    private static func reach(
        _ element: AXUIElement?,
        revealing: Bool,
    ) throws(ApplicationFailure) -> (sample: Input.Sample, window: CGWindowID?, tried: [Delivery.Attempt], scrolled: Delivery.Scrolled?) {
        guard let element else { throw ApplicationFailure(subject: "the reference", reason: .notOpen(nearest: [])) }
        let contentSize: String = NSAccessibility.Attribute.contentSize.rawValue
        let bars: [String] = [kAXHorizontalScrollBarAttribute, kAXVerticalScrollBarAttribute]
        let aimed: CGRect? = try element.read(frameAttributes).element.frame
        var (next, scrolled): (AXUIElement?, Delivery.Scrolled?) = (try element.elements(kAXParentAttribute).first, nil)
        while let current = next, let aimed, scrolled == nil {
            let area: Read = try current.read([kAXRoleAttribute, kAXParentAttribute, frameAttribute, contentSize] + bars)
            let frame: CGRect? = area.element.role == kAXScrollAreaRole ? area.element.frame : nil
            let content: CGSize? = area.element.facts[contentSize]?.size
            let shown: Bool = frame.map { (frame: CGRect) -> Bool in frame.contains(CGPoint(x: aimed.midX, y: aimed.midY)) } ?? true
            let moved: [Double?] = try bars.indices.map { (axis: Int) throws(ApplicationFailure) -> Double? in
                guard !shown, let frame, let content, let bar = area.element.facts[bars[axis]]?.element.map({ (id: Int) -> AXUIElement in area.handles[id] }) else { return nil }
                let (start, size, extent, center): (Double, Double, Double, Double) =
                    axis == 0 ? (frame.minX, frame.width, content.width, aimed.midX) : (frame.minY, frame.height, content.height, aimed.midY)
                guard extent > size, let value = try bar.read([kAXValueAttribute]).element.facts[kAXValueAttribute]?.number else { return nil }
                let goal: Double = min(max((center - start + value * (extent - size) - size / 2) / (extent - size), 0), 1)
                let answer: AXError = AXUIElementSetAttributeValue(bar, kAXValueAttribute as CFString, goal as NSNumber)
                return answer == .success ? try bar.read([kAXValueAttribute]).element.facts[kAXValueAttribute]?.number : nil
            }
            let parent: AXUIElement? = area.element.facts[kAXParentAttribute]?.element.map { (id: Int) -> AXUIElement in area.handles[id] }
            (next, scrolled) = (
                moved.contains { (value: Double?) -> Bool in value != nil } ? nil : parent,
                moved.contains { (value: Double?) -> Bool in value != nil } ? Delivery.Scrolled(horizontal: moved[0], vertical: moved[1], rows: nil, showing: []) : nil,
            )
        }
        let read = try element.element(reading: frameAttributes)
        let revealed: AXError = revealing && read.element.actions.contains(kAXShowAlternateUIAction) ? AXUIElementPerformAction(element, kAXShowAlternateUIAction as CFString) : .success
        guard let point = read.element.activation ?? read.element.frame.map({ frame in CGPoint(x: frame.midX, y: frame.midY) }) else {
            throw ApplicationFailure(subject: "reading the position of \(element.name)", reason: .accessibility(.noValue))
        }
        let tried: [Delivery.Attempt] =
            revealed == .success
            ? [] : [Delivery.Attempt(route: .accessibility, failure: ApplicationFailure(subject: "revealing \(element.name)", reason: .accessibility(revealed)))]
        return (Input.Sample(at: SIMD2(point.x, point.y), pressure: nil, tilt: nil), read.element.window, tried, scrolled)
    }
}

extension Fact.Scalar {
    fileprivate func states(_ asked: Self, within step: Double? = nil) -> Bool {
        switch (self, asked, number, asked.number, step) {
            case (.number(let shown), .bool(let flag), _, _, _): (shown != 0) == flag
            case (_, _, .some(let shown), .some(let goal), .some(let step)): abs(shown - goal) < step
            case (_, _, .some(let shown), .some(let goal), .none): Float(shown) == Float(goal)
            case (_, _, _, _, _): self == asked
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
    package nonisolated func act(
        _ steps: [Step],
        stepping: @escaping @Sendable (_ index: Int) -> Void = { _ in },
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> (
        outcomes: [Result<Step.Delivery, ApplicationFailure>],
        state: InstanceState,
        diagnosis: (usage: Result<ResourceUsage.Interval, Errno>, load: Result<SystemLoad.Interval, ProcessFailure>, faults: [LogEntry])?
    ) {
        try await step {
            [self] () async throws(ApplicationFailure) -> (
                outcomes: [Result<Step.Delivery, ApplicationFailure>],
                state: InstanceState,
                diagnosis: (usage: Result<ResourceUsage.Interval, Errno>, load: Result<SystemLoad.Interval, ProcessFailure>, faults: [LogEntry])?
            ) in
            let (process, started): (ProcessInstance, Date) = (instance.process, .now)
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
            var streams: [CGWindowID: (output: Result<StreamOutput, ApplicationFailure>, call: AsyncStream<Void>.Continuation)] = [:]
            defer { for stream in streams.values { stream.call.finish() } }
            var generator = SystemRandomNumberGenerator()
            let popUps = { () -> [CGWindowID] in
                InstanceState.Window.all(.optionOnScreenOnly, of: process.pid)[process.pid, default: []].filter { row in row.layer >= InstanceState.Window.Band.popUpMenu.level }.map(\.number)
            }
            var (previous, shown, opened): (AsyncStream<Message>?, [CGWindowID], [[CGWindowID]]) = (nil, popUps(), [])
            for (index, step) in steps.enumerated() where outcomes.last.map({ outcome in outcome[case: \.success].map { delivery in delivery.proof != .exit } == true }) ?? true {
                stepping(index)
                let events = await messages()
                var outcome = await Result { () async throws(ApplicationFailure) in
                    try await step.run(on: self, clipboard: clipboard, host: host, held: &held, streams: &streams, source: source, using: &generator, clock: clock)
                }
                if let previous, outcome[case: \.failure]?.reason.is(\.notOpen) == true,
                    try await next({ (change: InstanceEvent.Change) -> Void? in change[case: \.opened].map { _ in () } }, in: previous, clock: clock) != nil
                {
                    outcome = await Result { () async throws(ApplicationFailure) in
                        try await step.run(on: self, clipboard: clipboard, host: host, held: &held, streams: &streams, source: source, using: &generator, clock: clock)
                    }
                }
                outcomes.append(
                    outcome.flatMapError { failure in
                        switch (failure.reason == .unansweredMayHaveRun(step), failure.reason == .system(.noSuchProcess) || failure.reason == .cancelled && process.hasExited) {
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
            for (contact, hold) in held {
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
            let last: InstanceState = await beat()
            let stalled: Bool = last[case: \.running].map { activity in activity.status != .responding } == true
            let missed: Bool = outcomes.contains { outcome in outcome[case: \.success].map { delivery in delivery.proof == .unanswered } ?? true }
            guard stalled || missed else { return (outcomes, last, nil) }
            let usage = Result { () throws(Errno) in try ResourceUsage(of: process) }.flatMap { after in began.usage.map(after.since) }
            let load = began.load.flatMap { start in Result { () throws(ProcessFailure) in try SystemLoad.Interval(since: start) } }
            return (outcomes, last, (usage, load, await faults(since: started)))
        }
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
