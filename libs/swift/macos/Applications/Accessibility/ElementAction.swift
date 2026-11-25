import AppKit
import ApplicationServices
import AsyncAlgorithms
import CasePathsCore
public import CoreGraphics
public import Encoding
import Processes
import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

private let choosingRoles: Set<String> = [kAXMenuRole, kAXListRole]

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Codable, Hashable, Sendable, CustomStringConvertible {
    case press(element: Element.ID, action: String)
    case set(element: Element.ID, value: JSONValue)
    case choose(element: Element.ID, item: String)
    case key(key: Key, modifiers: Set<Modifier>)

    public var description: String {
        switch self {
            case .press(let element, let action): "\(action) on \(element)"
            case .set(let element, let value): "set \(kAXValueAttribute) \(value) on \(element)"
            case .choose(let element, let item): "choose \(String(reflecting: item)) on \(element)"
            case .key(let key, let modifiers): "key \(key) with \(modifiers.map(\.rawValue).sorted())"
        }
    }

    var element: Element.ID? {
        switch self {
            case .press(let element, _), .set(let element, _), .choose(let element, _): element
            case .key: nil
        }
    }

    @MainActor
    var strokes: [(code: CGKeyCode, flags: CGEventFlags, text: String?)] {
        let typing = { (text: String) -> [(key: Key, modifiers: Set<Modifier>)] in
            let selecting: (key: Key, modifiers: Set<Modifier>) = (Key.character("a"), [.command])
            let entering: (key: Key, modifiers: Set<Modifier>) = (Key.named(.return), [])
            let characters: [(key: Key, modifiers: Set<Modifier>)] = text.map { character in (Key.character(String(character)), []) }
            return [selecting] + characters + [entering]
        }
        let keys: [(key: Key, modifiers: Set<Modifier>)] =
            switch self {
                case .press: []
                case .key(let key, let modifiers): [(key, modifiers)]
                case .set(_, let value): value.text.map { text in typing(text) } ?? []
                case .choose(_, let item): typing(item)
            }
        return keys.map { key, modifiers in
            let stroke = key.stroke
            return (stroke.code, CGEventFlags(modifiers.map(\.flag)).union(stroke.shift ? .maskShift : []), stroke.text)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ElementAction {
    fileprivate func run(in process: pid_t, strokes: [(code: CGKeyCode, flags: CGEventFlags, text: String?)]) throws(ApplicationFailure) -> CGWindowID? {
        if case .press(_, kAXRaiseAction) = self { throw ApplicationFailure(subject: "\(self), raising a window takes focus from the frontmost application", reason: .refused(nil)) }
        guard let id = element else {
            let focused: CGWindowID?
            do throws(ApplicationFailure) {
                focused = try (AXUIElement.application(process).value(kAXFocusedWindowAttribute) as CFTypeRef?).flatMap(AXUIElement.cast)?.window
            } catch  where error.reason.is(\.unanswered) {
                focused = nil
            }
            for stroke in strokes { try Self.post(stroke.code, stroke.flags, stroke.text, to: process) }
            return focused
        }
        guard let element = try AXUIElement.element(id.token), let owner = element.process else { throw ApplicationFailure(subject: id.description, reason: .unresolved) }
        if owner != process, NSRunningApplication(processIdentifier: owner)?.isDialogHelper != true { throw ApplicationFailure(subject: id.description, reason: .foreign(process)) }
        let read = try element.values([kAXEnabledAttribute, kAXWindowAttribute, kAXRoleAttribute])
        let outcome: AXError =
            switch (self, read.values[kAXEnabledAttribute] as? Bool) {
                case (.press, false), (.choose, false): throw ApplicationFailure(subject: description, reason: .disabled)
                case (.press(_, let action), _): AXUIElementPerformAction(element, action as CFString)
                case (.set(_, let value), _): try assign(value, to: element, role: read.values[kAXRoleAttribute] as? String, in: owner, typing: strokes)
                case (.choose(_, let item), _): try assign(.string(item), to: element, role: read.values[kAXRoleAttribute] as? String, in: owner, typing: strokes)
                case (.key, _): .success
            }
        guard outcome != .success else { return read.values[kAXWindowAttribute].flatMap(AXUIElement.cast)?.window }
        throw ApplicationFailure(outcome, subject: description, process: owner, answering: self) {
            ApplicationFailure(subject: description, reason: outcome == .invalidUIElement ? .unresolved : .refused(outcome))
        }
    }

    private static func post(_ code: CGKeyCode, _ flags: CGEventFlags, _ text: String?, to process: pid_t) throws(ApplicationFailure) {
        let (source, units) = (CGEventSource(stateID: .privateState), Array(text?.utf16 ?? "".utf16))
        for down in [true, false] {
            guard let event = CGEvent(keyboardEventSource: source, virtualKey: code, keyDown: down) else { throw ApplicationFailure(subject: "key \(code)", reason: .refused(nil)) }
            event.flags = flags
            if !units.isEmpty { units.withUnsafeBufferPointer { units in unsafe event.keyboardSetUnicodeString(stringLength: units.count, unicodeString: units.baseAddress) } }
            event.postToPid(process)
        }
    }

    private func assign(
        _ target: JSONValue,
        to element: AXUIElement,
        role: String?,
        in process: pid_t,
        typing strokes: [(code: CGKeyCode, flags: CGEventFlags, text: String?)],
    ) throws(ApplicationFailure) -> AXError {
        let attribute = valueAttribute(of: role)
        let current = { () throws(ApplicationFailure) -> JSONValue? in (try element.value(attribute) as CFTypeRef?).flatMap(JSONValue.init(accessibility:)) }
        let (actions, menus) = (try element.actionNames(), try element.menus(choosingRoles))
        let openAction: String? =
            switch (actions.contains(kAXShowMenuAction), role) {
                case (true, _): kAXShowMenuAction
                case (false, kAXPopUpButtonRole), (false, kAXComboBoxRole), (false, kAXMenuButtonRole): kAXPressAction
                case (false, _): nil
            }
        let stepGoal = actions.contains(kAXIncrementAction) && actions.contains(kAXDecrementAction) ? target.number : nil
        switch (try element.settable(attribute), target.text, !menus.isEmpty || openAction != nil, stepGoal) {
            case (true, _, _, _):
                let typed: JSONValue =
                    switch (try current(), target.number, target.text.flatMap(Bool.init)) {
                        case (.bool, _, .some(let flag)): .bool(flag)
                        case (.number, .some(let number), _): .number(number)
                        case (_, _, _): target
                    }
                let written = AXUIElementSetAttributeValue(element, attribute as CFString, try typed.accessibility)
                let read = try written == .success ? current() : nil
                guard written != .success || read == typed || typed.number.map({ number in read?.number == number }) == true else {
                    throw ApplicationFailure(subject: description, reason: .unchanged(read))
                }
                return written
            case (false, .some(let item), true, _): return try choose(item, on: element, listing: menus, opening: openAction, in: process)
            case (false, _, _, .some(let goal)):
                var (value, answer) = (try current()?.number, AXError.success)
                while let reached = value, reached != goal, answer == .success {
                    answer = AXUIElementPerformAction(element, (reached < goal ? kAXIncrementAction : kAXDecrementAction) as CFString)
                    let next = try answer == .success ? current()?.number ?? reached : reached
                    value = answer != .success || abs(goal - next) < abs(goal - reached) ? next : nil
                }
                guard answer != .success || value == goal else { throw ApplicationFailure(subject: description, reason: .unchanged(try current())) }
                return answer
            case (false, .some, false, .none):
                let focused = AXUIElementSetAttributeValue(element, kAXFocusedAttribute as CFString, kCFBooleanTrue)
                guard focused == .success else { return focused }
                for stroke in strokes { try Self.post(stroke.code, stroke.flags, stroke.text, to: process) }
                return .success
            case (false, .none, _, .none): throw ApplicationFailure(subject: description, reason: .unchanged(try current()))
        }
    }

    private func choose(_ item: String, on element: AXUIElement, listing menus: [AXUIElement], opening action: String?, in process: pid_t) throws(ApplicationFailure) -> AXError {
        let labeled = { (entries: [AXUIElement]) throws(ApplicationFailure) -> [(element: AXUIElement, label: String?)] in
            try entries.map { entry throws(ApplicationFailure) in
                let read = try entry.values([kAXTitleAttribute, kAXDescriptionAttribute, kAXTitleUIElementAttribute, kAXValueAttribute])
                return (entry, try entry.label(read) ?? read.values[kAXValueAttribute] as? String)
            }
        }
        let items = { (menus: [AXUIElement]) throws(ApplicationFailure) in try labeled(menus.map { menu throws(ApplicationFailure) in try menu.elements(kAXChildrenAttribute) }.flatMap(\.self)) }
        let listed = try items(menus)
        if listed.isEmpty, let action {
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1)))
            let opened = AXUIElementPerformAction(element, action as CFString)
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / .seconds(1)))
            guard [.success, .cannotComplete].contains(opened) else { return opened }
        }
        let opened = try listed.isEmpty ? element.menus(choosingRoles) : menus
        let shown = try listed.isEmpty ? items(opened) : listed
        guard shown.isEmpty else {
            guard let match = shown.first(where: { entry in entry.label == item }) else {
                for menu in opened { _ = AXUIElementPerformAction(menu, kAXCancelAction as CFString) }
                throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: shown.compactMap(\.label))))
            }
            return AXUIElementPerformAction(match.element, kAXPressAction as CFString)
        }
        let owned = try labeled(element.elements(kAXChildrenAttribute)).filter { entry in entry.label != nil }
        let (selected, value): ([AXUIElement], String?) = (try element.elements(kAXSelectedChildrenAttribute), try element.value(kAXValueAttribute))
        let from = owned.firstIndex { entry in selected.contains { chosen in CFEqual(chosen, entry.element) } } ?? owned.firstIndex { entry in entry.label == value }
        guard let goal = owned.firstIndex(where: { entry in entry.label == item }), let from else {
            try Self.post(Key.Named.escape.code, [], nil, to: process)
            throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: owned.compactMap(\.label))))
        }
        for code in repeatElement(goal > from ? Key.Named.down.code : Key.Named.up.code, count: abs(goal - from)) + [Key.Named.return.code] { try Self.post(code, [], nil, to: process) }
        return .success
    }
}

extension Instance {
    @concurrent
    public static func owning(_ id: Element.ID) async throws(ApplicationFailure) -> Self {
        try await withTaskExecutorPreference(accessibilityQueue) { () throws(ApplicationFailure) in
            guard let owner = try AXUIElement.element(id.token)?.process else { throw ApplicationFailure(subject: id.description, reason: .unresolved) }
            guard let application = NSRunningApplication(processIdentifier: owner) else { throw ApplicationFailure(subject: String(owner), reason: .noProcess) }
            return try Self(application)
        }
    }

    @concurrent
    public func act(
        _ actions: [ElementAction],
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> (outcomes: [Result<CGWindowID?, ApplicationFailure>], state: InstanceState) {
        let states = states(clock: clock)
        var (outcomes, acted, blind, deferred): ([Result<CGWindowID?, ApplicationFailure>], Set<CGWindowID>, Bool, Bool) = ([], [], false, false)
        for action in actions where outcomes.last?.is(\.failure) != true {
            let strokes = await MainActor.run { action.strokes }
            let outcome = await Result { () async throws(ApplicationFailure) in
                try await withTaskExecutorPreference(accessibilityQueue) { () async throws(ApplicationFailure) in try action.run(in: process.pid, strokes: strokes) }
            }
            let result: Result<CGWindowID?, ApplicationFailure> =
                switch outcome {
                    case .failure(let failure) where failure.reason.is(\.noProcess) || failure.reason == .unanswered(action): .success(nil)
                    case let outcome: outcome
                }
            outcomes.append(result)
            acted.formUnion([result[case: \.success].flatMap(\.self)].compactMap(\.self))
            blind = blind || (action.element == nil && result == .success(nil))
            deferred = deferred || (action.element != nil && result == .success(nil))
        }
        var last: InstanceState?
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
