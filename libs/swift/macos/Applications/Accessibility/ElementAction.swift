import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
import CasePathsCore
public import CoreGraphics
import Processes
import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

private let once = Input.Schedule(count: 1, every: nil, jitter: .zero)

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Codable, Hashable, Sendable, CustomStringConvertible {
    public struct Delivery: Encodable, Equatable, Sendable {
        public let window: CGWindowID?
        public let route: Route?
    }

    case press(ref: Int, action: String)
    case set(ref: Int, value: Fact.Scalar)
    case choose(ref: Int, item: String)
    case key(ref: Int?, key: Key, modifiers: Set<Modifier>)

    public var description: String {
        switch self {
            case .press(let ref, let action): "\(action) on \(ref)"
            case .set(let ref, let value): "set \(kAXValueAttribute) \(value) on \(ref)"
            case .choose(let ref, let item): "choose \(String(reflecting: item)) on \(ref)"
            case .key(.none, let key, let modifiers): "key \(key) with \(modifiers.map(\.rawValue).sorted())"
            case .key(.some(let ref), let key, let modifiers): "key \(key) with \(modifiers.map(\.rawValue).sorted()) on \(ref)"
        }
    }

    fileprivate var ref: Int? {
        switch self {
            case .press(let ref, _), .set(let ref, _), .choose(let ref, _): ref
            case .key(let ref, _, _): ref
        }
    }

    private var keys: [(key: Key, modifiers: Set<Modifier>)] {
        let entering = { (text: String) -> [(key: Key, modifiers: Set<Modifier>)] in
            let characters: [(key: Key, modifiers: Set<Modifier>)] = text.map { (character: Character) -> (key: Key, modifiers: Set<Modifier>) in (Key.character(character), []) }
            let selecting: [(key: Key, modifiers: Set<Modifier>)] = [(Key.character("a"), [Modifier.command])]
            let returning: [(key: Key, modifiers: Set<Modifier>)] = [(Key.special(.return), [])]
            return selecting + characters + returning
        }
        return switch self {
            case .press: []
            case .key(_, let key, let modifiers): [(key, modifiers)]
            case .set(_, let value): value.text.map { text in entering(text) } ?? []
            case .choose(_, let item): entering(item)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ElementAction {
    fileprivate func run(in process: pid_t, on element: AXUIElement?) throws(ApplicationFailure) -> (window: CGWindowID?, posting: (route: Route, inputs: [Input])?) {
        if case .press(_, kAXRaiseAction) = self { throw ApplicationFailure(subject: "\(description), raising a window takes focus from the frontmost application", reason: .refused(nil)) }
        guard let element else {
            let focused: CGWindowID?
            do throws(ApplicationFailure) {
                focused = try AXUIElementCreateApplication(process).elements(kAXFocusedWindowAttribute).first?.window
            } catch  where error.reason.is(\.unanswered) {
                focused = nil
            }
            return (focused, (.process, try Self.typing(keys)))
        }
        let read = try element.read([kAXEnabledAttribute, kAXRoleAttribute, kAXFocusedAttribute, kAXWindowAttribute] + frameAttributes)
        let outcome: (answer: AXError, posting: (route: Route, inputs: [Input])?) =
            switch (self, read.element.facts[kAXEnabledAttribute]?.bool) {
                case (.press, false), (.choose, false): throw ApplicationFailure(subject: description, reason: .disabled)
                case (.press(_, let action), _): (AXUIElementPerformAction(element, action as CFString), nil)
                case (.set(_, let value), _): try assign(value, to: element, read: read)
                case (.choose(_, let item), _): try assign(.text(item), to: element, read: read)
                case (.key, _): (.success, try aim(Self.typing(keys), at: element, read: read))
            }
        guard outcome.answer != .success else { return (read.element.window, outcome.posting) }
        throw ApplicationFailure(outcome.answer, subject: description, process: element.process ?? process, answering: self) {
            ApplicationFailure(subject: description, reason: outcome.answer == .invalidUIElement ? .stale(reached: 0, candidates: []) : .refused(outcome.answer))
        }
    }

    private static func typing(_ keys: [(key: Key, modifiers: Set<Modifier>)]) throws(ApplicationFailure) -> [Input] {
        try keys.map { key, modifiers throws(ApplicationFailure) -> Input in try Input(contact: .key(key), path: [], hold: .zero, modifiers: modifiers, schedule: once) }
    }

    private func assign(
        _ target: Fact.Scalar,
        to element: AXUIElement,
        read: (element: Element, handles: [AXUIElement]),
    ) throws(ApplicationFailure) -> (answer: AXError, posting: (route: Route, inputs: [Input])?) {
        let attribute = read.element.role == kAXRowRole ? kAXSelectedAttribute : kAXValueAttribute
        let current = { () throws(ApplicationFailure) -> Fact.Scalar? in try element.read([attribute]).element.facts[attribute]?.scalar }
        let (shown, settable): (Fact.Scalar?, Bool) = (try current(), try element.settable(attribute))
        let menus: [AXUIElement] = try settable || target.text == nil ? [] : element.menus()
        let opening: String? =
            switch (read.element.actions.contains(kAXShowMenuAction), read.element.role) {
                case (true, _): kAXShowMenuAction
                case (false, kAXPopUpButtonRole), (false, kAXComboBoxRole), (false, kAXMenuButtonRole): kAXPressAction
                case (false, _): nil
            }
        let stepping = read.element.actions.contains(kAXIncrementAction) && read.element.actions.contains(kAXDecrementAction) ? target.number : nil
        switch (settable, target.text, !menus.isEmpty || opening != nil, stepping) {
            case (true, _, _, _):
                let value: Fact.Scalar =
                    switch (shown, target.number, target.text.flatMap(Bool.init)) {
                        case (.bool, _, .some(let flag)): .bool(flag)
                        case (.number, .some(let number), _): .number(number)
                        case (_, _, _): target
                    }
                let answer = AXUIElementSetAttributeValue(element, attribute as CFString, value.accessibility)
                let after = try answer == .success ? current() : nil
                guard answer != .success || after == value || value.number.map({ number in after?.number == number }) == true else {
                    throw ApplicationFailure(subject: description, reason: .unchanged(after))
                }
                return (answer, nil)
            case (false, .some(let item), true, _): return try choose(item, on: element, listing: menus, opening: opening, read: read)
            case (false, _, _, .some(let goal)):
                var (value, answer) = (shown?.number, AXError.success)
                while let reached = value, reached != goal, answer == .success {
                    answer = AXUIElementPerformAction(element, (reached < goal ? kAXIncrementAction : kAXDecrementAction) as CFString)
                    let next = try answer == .success ? current()?.number ?? reached : reached
                    value = answer != .success || abs(goal - next) < abs(goal - reached) ? next : nil
                }
                guard answer != .success || value == goal else { throw ApplicationFailure(subject: description, reason: .unchanged(try current())) }
                return (answer, nil)
            case (false, .some, false, .none): return (.success, try aim(Self.typing(keys), at: element, read: read))
            case (false, .none, _, .none): throw ApplicationFailure(subject: description, reason: .unchanged(shown))
        }
    }

    private func choose(
        _ item: String,
        on element: AXUIElement,
        listing menus: [AXUIElement],
        opening action: String?,
        read: (element: Element, handles: [AXUIElement]),
    ) throws(ApplicationFailure) -> (answer: AXError, posting: (route: Route, inputs: [Input])?) {
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
        let opening: String? = listed.isEmpty ? action : nil
        if let opening {
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1)))
            let opened = AXUIElementPerformAction(element, opening as CFString)
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / .seconds(1)))
            guard [.success, .cannotComplete].contains(opened) else { return (opened, nil) }
        }
        let opened = try opening == nil ? menus : element.menus()
        let shown = try opening == nil ? listed : items(opened)
        guard shown.isEmpty else {
            guard let match = shown.first(where: { entry in entry.label == item }) else {
                for menu in opened { _ = AXUIElementPerformAction(menu, kAXCancelAction as CFString) }
                throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: shown.compactMap(\.label))))
            }
            return (AXUIElementPerformAction(match.element, kAXPressAction as CFString), nil)
        }
        let state = try element.read([kAXChildrenAttribute, kAXSelectedChildrenAttribute, kAXValueAttribute])
        let owned = try labeled((state.element.facts[kAXChildrenAttribute]?.elements ?? []).map { id in state.handles[id] }).filter { entry in entry.label != nil }
        let selected: [AXUIElement] = (state.element.facts[kAXSelectedChildrenAttribute]?.elements ?? []).map { id in state.handles[id] }
        let from = owned.firstIndex { entry in selected.contains { chosen in CFEqual(chosen, entry.element) } } ?? owned.firstIndex { entry in entry.label == state.element.text(kAXValueAttribute) }
        guard let goal = owned.firstIndex(where: { entry in entry.label == item }), let from else {
            if opening != nil { _ = AXUIElementPerformAction(element, kAXCancelAction as CFString) }
            throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: owned.compactMap(\.label))))
        }
        let step: (key: Key, modifiers: Set<Modifier>) = (Key.special(goal > from ? .down : .up), [])
        let moving: [(key: Key, modifiers: Set<Modifier>)] = Array(repeating: step, count: abs(goal - from))
        let returning: [(key: Key, modifiers: Set<Modifier>)] = [(Key.special(.return), [])]
        return (.success, try aim(Self.typing(moving + returning), at: element, read: read))
    }

    private func aim(_ typed: [Input], at element: AXUIElement, read: (element: Element, handles: [AXUIElement])) throws(ApplicationFailure) -> (route: Route, inputs: [Input]) {
        guard read.element.facts[kAXFocusedAttribute]?.bool != true else { return (.process, typed) }
        let window: AXUIElement? = read.element.facts[kAXWindowAttribute]?.element.map { id in read.handles[id] }
        let main: AXError =
            try window.map { window throws(ApplicationFailure) -> AXError in
                try window.settable(kAXMainAttribute) ? AXUIElementSetAttributeValue(window, kAXMainAttribute as CFString, kCFBooleanTrue) : .success
            } ?? .success
        let written: AXError = main == .success ? AXUIElementSetAttributeValue(element, kAXFocusedAttribute as CFString, kCFBooleanTrue) : main
        let kept: Bool? = written == .success ? try element.read([kAXFocusedAttribute]).element.facts[kAXFocusedAttribute]?.bool : nil
        guard kept != true else { return (.process, typed) }
        guard let number = read.element.window, let frame = read.element.frame else { throw ApplicationFailure(subject: "\(kAXFocusedAttribute) for \(description)", reason: .refused(written)) }
        let center: [Input.Sample] = [Input.Sample(at: SIMD2(frame.midX, frame.midY), pressure: nil, tilt: nil)]
        return (.window(number: number), [try Input(contact: .button(.left), path: center, hold: .zero, modifiers: [], schedule: once)] + typed)
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

extension Instance {
    @concurrent
    public func act(
        _ actions: [ElementAction],
        in watch: Watch,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> (outcomes: [Result<ElementAction.Delivery, ApplicationFailure>], state: InstanceState) {
        var (outcomes, acted, blind, deferred): ([Result<ElementAction.Delivery, ApplicationFailure>], Set<CGWindowID>, Bool, Bool) = ([], [], false, false)
        guard let source = CGEventSource(stateID: .privateState) else { throw ApplicationFailure(subject: "private event source for process \(process.pid)", reason: .refused(nil)) }
        let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: "Acting on process \(process.pid)")
        defer { ProcessInfo.processInfo.endActivity(activity) }
        let keyboardType: CGEventSourceKeyboardType = source.keyboardType
        let host = await MainActor.run {
            (
                layout: KeyboardLayout.current(typing: keyboardType),
                rate: NSScreen.main?.maximumFramesPerSecond ?? 0,
                doubleClick: Duration.seconds(NSEvent.doubleClickInterval),
                keyRepeat: (delay: Duration.seconds(NSEvent.keyRepeatDelay), interval: Duration.seconds(NSEvent.keyRepeatInterval)),
            )
        }
        var generator = SystemRandomNumberGenerator()
        for action in actions where outcomes.last?.is(\.failure) != true {
            let outcome: Result<ElementAction.Delivery, ApplicationFailure>
            do throws(ApplicationFailure) {
                let step = try await watch.perform(on: action.ref) { element throws(ApplicationFailure) in try action.run(in: process.pid, on: element) }
                let pressed = [CGEventType.leftMouseDown, .rightMouseDown, .otherMouseDown].map { type in CGEventSource.counterForEventType(source.sourceStateID, eventType: type) }.reduce(0, +)
                let timelines = try (step.posting?.inputs ?? []).map { input throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] in
                    try input.timeline(doubleClick: host.doubleClick, keyRepeat: host.keyRepeat, frames: host.rate, layout: host.layout, numbering: Int64(pressed) + 1, using: &generator)
                }
                let timeline = timelines.reduce(into: [(offset: Duration, event: CGEvent)]()) { walk, next in
                    let shift: Duration = walk.last?.offset ?? .zero
                    walk += next.map { offset, event in (shift + offset, event) }
                }
                try await (step.posting?.route ?? .process).post(timeline, to: process.pid, from: source, clock: clock)
                outcome = .success(ElementAction.Delivery(window: step.window, route: step.posting?.route))
            } catch  where error.reason.is(\.noProcess) || error.reason == .unanswered(action) {
                outcome = .success(ElementAction.Delivery(window: nil, route: nil))
            } catch {
                outcome = .failure(error)
            }
            let reached: CGWindowID?? = outcome[case: \.success].map(\.window)
            outcomes.append(outcome)
            if case .some(.some(let number)) = reached { acted.insert(number) }
            blind = blind || (action.ref == nil && reached == .some(nil))
            deferred = deferred || (action.ref != nil && reached == .some(nil))
        }
        var last: InstanceState?
        let states = await watch.states().dropFirst()
        for await next in merge(states.map(Optional.some), AsyncTimerSequence(interval: messagingTimeout * 2, clock: clock).map { _ in InstanceState?.none }) {
            if let next, !Self.isIdle(next, acting: acted, blind: blind, deferred: deferred && last == nil) {
                last = next
                continue
            }
            if let state = next ?? last { return (outcomes, state) }
        }
        throw ApplicationFailure(subject: actions.map(\.description).joined(separator: ", "), reason: .cancelled)
    }

    private static func isIdle(_ state: InstanceState, acting windows: Set<CGWindowID>, blind: Bool, deferred: Bool) -> Bool {
        switch state {
            case .exited, .failed: true
            case .running(let running) where !running.status.isIdle: false
            case .running(let running) where running.status.is(\.unresponsive): true
            case .running(let running):
                !running.asking.contains { row in windows.contains(row.number) } && !(blind && !running.asking.isEmpty) && !(deferred && running.asking.isEmpty)
        }
    }
}
