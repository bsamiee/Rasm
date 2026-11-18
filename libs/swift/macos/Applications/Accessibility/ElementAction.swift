import AppKit
import ApplicationServices
import AsyncAlgorithms
import Carbon.HIToolbox
import CasePathsCore
public import CoreGraphics
public import Encoding
import Processes
import Results

// --- [CONSTANTS] -----------------------------------------------------------------------

private let choosingRoles: Set<String> = [kAXMenuRole, kAXListRole]

// --- [MODELS] --------------------------------------------------------------------------

public enum Modifier: String, CaseIterable, Codable, Sendable {
    case command
    case shift
    case option
    case control
    case function

    var flag: CGEventFlags {
        switch self {
            case .command: .maskCommand
            case .shift: .maskShift
            case .option: .maskAlternate
            case .control: .maskControl
            case .function: .maskSecondaryFn
        }
    }
}

public enum Key: Codable, Hashable, Sendable {
    public enum Named: String, CaseIterable, Codable, Sendable {
        case `return`, enter, tab, space, delete, forwardDelete, escape, home, end, pageUp, pageDown, left, right, down, up, help
        case f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12

        var code: CGKeyCode {
            let code =
                switch self {
                    case .return: kVK_Return
                    case .enter: kVK_ANSI_KeypadEnter
                    case .tab: kVK_Tab
                    case .space: kVK_Space
                    case .delete: kVK_Delete
                    case .forwardDelete: kVK_ForwardDelete
                    case .escape: kVK_Escape
                    case .home: kVK_Home
                    case .end: kVK_End
                    case .pageUp: kVK_PageUp
                    case .pageDown: kVK_PageDown
                    case .left: kVK_LeftArrow
                    case .right: kVK_RightArrow
                    case .down: kVK_DownArrow
                    case .up: kVK_UpArrow
                    case .help: kVK_Help
                    case .f1: kVK_F1
                    case .f2: kVK_F2
                    case .f3: kVK_F3
                    case .f4: kVK_F4
                    case .f5: kVK_F5
                    case .f6: kVK_F6
                    case .f7: kVK_F7
                    case .f8: kVK_F8
                    case .f9: kVK_F9
                    case .f10: kVK_F10
                    case .f11: kVK_F11
                    case .f12: kVK_F12
                }
            return CGKeyCode(code)
        }
    }

    case named(Named)
    case character(String)
    case code(CGKeyCode)

    public init(from decoder: any Decoder) throws {
        let value = try decoder.singleValueContainer()
        do { self = .code(try value.decode(CGKeyCode.self)) } catch DecodingError.typeMismatch {
            let text = try value.decode(String.self)
            switch (Named(rawValue: text), text.count) {
                case (.some(let named), _): self = .named(named)
                case (.none, 1): self = .character(text)
                case (.none, _):
                    throw DecodingError.dataCorruptedError(in: value, debugDescription: "\(text) is neither one character nor a key of \(Named.allCases.map(\.rawValue).joined(separator: ", "))")
            }
        }
    }

    public func encode(to encoder: any Encoder) throws {
        var value = encoder.singleValueContainer()
        switch self {
            case .named(let named): try value.encode(named)
            case .character(let text): try value.encode(text)
            case .code(let code): try value.encode(code)
        }
    }

    @MainActor
    var stroke: (code: CGKeyCode, shift: Bool, text: String?) {
        switch self {
            case .code(let code): (code, false, nil)
            case .named(let named): (named.code, false, nil)
            case .character(let text): Self.located(text).map { located in (located.code, located.shift, text) } ?? (0, false, text)
        }
    }

    @MainActor
    private static func located(_ text: String) -> (code: CGKeyCode, shift: Bool)? {
        guard let property = unsafe TISGetInputSourceProperty(TISCopyCurrentKeyboardLayoutInputSource().takeRetainedValue(), kTISPropertyUnicodeKeyLayoutData) else { return nil }
        let layout = unsafe Unmanaged<CFData>.fromOpaque(property).takeUnretainedValue() as Data
        let shift = UInt32(shiftKey >> 8)
        return unsafe layout.withUnsafeBytes { bytes -> (code: CGKeyCode, shift: Bool)? in
            guard let keyboard = unsafe bytes.assumingMemoryBound(to: UCKeyboardLayout.self).baseAddress else { return nil }
            let strokes: [(code: CGKeyCode, state: UInt32)] = (CGKeyCode(0)..<128).flatMap { (code: CGKeyCode) -> [(code: CGKeyCode, state: UInt32)] in [(code, 0), (code, shift)] }
            let typed = strokes.first { (stroke: (code: CGKeyCode, state: UInt32)) -> Bool in
                var (dead, length, characters): (UInt32, Int, [UniChar]) = (0, 0, [0, 0, 0, 0])
                let status = unsafe UCKeyTranslate(
                    keyboard,
                    stroke.code,
                    UInt16(kUCKeyActionDisplay),
                    stroke.state,
                    UInt32(LMGetKbdType()),
                    OptionBits(kUCKeyTranslateNoDeadKeysMask),
                    &dead,
                    characters.count,
                    &length,
                    &characters,
                )
                let translated = String(decoding: characters.prefix(length), as: UTF16.self)
                return status == noErr && translated == text
            }
            return typed.map { stroke in (stroke.code, stroke.state == shift) }
        }
    }
}

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
        if case .press(_, kAXRaiseAction) = self {
            throw ApplicationFailure(subject: "\(self), raising a window takes focus from the frontmost application", reason: .refused(nil))
        }
        guard let id = element else {
            let focused: CGWindowID?
            do throws(ApplicationFailure) {
                focused = try (AXUIElement.application(process).value(kAXFocusedWindowAttribute) as CFTypeRef?).flatMap(AXUIElement.checked)?.window
            } catch  where error.reason.is(\.unanswered) {
                focused = nil
            }
            for stroke in strokes { try Self.post(stroke.code, stroke.flags, stroke.text, to: process) }
            return focused
        }
        guard let node = try AXUIElement.element(id.token), let owner = node.process else { throw ApplicationFailure(subject: id.description, reason: .unresolved) }
        if owner != process, NSRunningApplication(processIdentifier: owner)?.isDialogHelper != true { throw ApplicationFailure(subject: id.description, reason: .foreign(process)) }
        let read = try node.values([kAXEnabledAttribute, kAXWindowAttribute, kAXRoleAttribute])
        let outcome: AXError =
            switch (self, read.values[kAXEnabledAttribute] as? Bool) {
                case (.press, false), (.choose, false): throw ApplicationFailure(subject: description, reason: .disabled)
                case (.press(_, let action), _): AXUIElementPerformAction(node, action as CFString)
                case (.set(_, let value), _): try assign(value, to: node, role: read.values[kAXRoleAttribute] as? String, in: owner, typing: strokes)
                case (.choose(_, let item), _): try assign(.string(item), to: node, role: read.values[kAXRoleAttribute] as? String, in: owner, typing: strokes)
                case (.key, _): .success
            }
        guard outcome != .success else { return read.values[kAXWindowAttribute].flatMap(AXUIElement.checked)?.window }
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
        to node: AXUIElement,
        role: String?,
        in process: pid_t,
        typing strokes: [(code: CGKeyCode, flags: CGEventFlags, text: String?)],
    ) throws(ApplicationFailure) -> AXError {
        let attribute = valueAttribute(of: role)
        let current = { () throws(ApplicationFailure) -> JSONValue? in (try node.value(attribute) as CFTypeRef?).flatMap(JSONValue.init(accessibility:)) }
        let (actions, menus) = (try node.actionNames(), try node.menus(choosingRoles))
        let opening: String? =
            switch (actions.contains(kAXShowMenuAction), role) {
                case (true, _): kAXShowMenuAction
                case (false, kAXPopUpButtonRole), (false, kAXComboBoxRole), (false, kAXMenuButtonRole): kAXPressAction
                case (false, _): nil
            }
        let stepping = actions.contains(kAXIncrementAction) && actions.contains(kAXDecrementAction) ? target.number : nil
        switch (try node.settable(attribute), target.text, !menus.isEmpty || opening != nil, stepping) {
            case (true, _, _, _):
                let typed: JSONValue =
                    switch (try current(), target.number, target.text.flatMap(Bool.init)) {
                        case (.bool, _, .some(let flag)): .bool(flag)
                        case (.number, .some(let number), _): .number(number)
                        case (_, _, _): target
                    }
                let written = AXUIElementSetAttributeValue(node, attribute as CFString, try typed.accessibility)
                let read = try written == .success ? current() : nil
                guard written != .success || read == typed || typed.number.map({ number in read?.number == number }) == true else {
                    throw ApplicationFailure(subject: description, reason: .unchanged(read))
                }
                return written
            case (false, .some(let item), true, _): return try choose(item, on: node, listing: menus, opening: opening, in: process)
            case (false, _, _, .some(let goal)):
                var (value, answer) = (try current()?.number, AXError.success)
                while let reached = value, reached != goal, answer == .success {
                    answer = AXUIElementPerformAction(node, (reached < goal ? kAXIncrementAction : kAXDecrementAction) as CFString)
                    let next = try answer == .success ? current()?.number ?? reached : reached
                    value = answer != .success || abs(goal - next) < abs(goal - reached) ? next : nil
                }
                guard answer != .success || value == goal else { throw ApplicationFailure(subject: description, reason: .unchanged(try current())) }
                return answer
            case (false, .some, false, .none):
                let focused = AXUIElementSetAttributeValue(node, kAXFocusedAttribute as CFString, kCFBooleanTrue)
                guard focused == .success else { return focused }
                for stroke in strokes { try Self.post(stroke.code, stroke.flags, stroke.text, to: process) }
                return .success
            case (false, .none, _, .none): throw ApplicationFailure(subject: description, reason: .unchanged(try current()))
        }
    }

    private func choose(_ item: String, on node: AXUIElement, listing menus: [AXUIElement], opening action: String?, in process: pid_t) throws(ApplicationFailure) -> AXError {
        let labeled = { (entries: [AXUIElement]) throws(ApplicationFailure) -> [(element: AXUIElement, label: String?)] in
            try entries.map { entry throws(ApplicationFailure) in
                let read = try entry.values([kAXTitleAttribute, kAXDescriptionAttribute, kAXTitleUIElementAttribute, kAXValueAttribute])
                return (entry, try entry.label(read) ?? read.values[kAXValueAttribute] as? String)
            }
        }
        let items = { (menus: [AXUIElement]) throws(ApplicationFailure) in try labeled(menus.map { menu throws(ApplicationFailure) in try menu.elements(kAXChildrenAttribute) }.flatMap(\.self)) }
        let listed = try items(menus)
        if listed.isEmpty, let action {
            AXUIElementSetMessagingTimeout(node, Float(messagingTimeout / 2 / .seconds(1)))
            let opened = AXUIElementPerformAction(node, action as CFString)
            AXUIElementSetMessagingTimeout(node, Float(messagingTimeout / .seconds(1)))
            guard [.success, .cannotComplete].contains(opened) else { return opened }
        }
        let opened = try listed.isEmpty ? node.menus(choosingRoles) : menus
        let shown = try listed.isEmpty ? items(opened) : listed
        guard shown.isEmpty else {
            guard let match = shown.first(where: { entry in entry.label == item }) else {
                for menu in opened { _ = AXUIElementPerformAction(menu, kAXCancelAction as CFString) }
                throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "item of \(description)", query: item, candidates: shown.compactMap(\.label))))
            }
            return AXUIElementPerformAction(match.element, kAXPressAction as CFString)
        }
        let owned = try labeled(node.elements(kAXChildrenAttribute)).filter { entry in entry.label != nil }
        let (selected, value): ([AXUIElement], String?) = (try node.elements(kAXSelectedChildrenAttribute), try node.value(kAXValueAttribute))
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
        try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) in
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
        var (outcomes, acted, blind): ([Result<CGWindowID?, ApplicationFailure>], Set<CGWindowID>, Bool) = ([], [], false)
        for action in actions where outcomes.last?.is(\.failure) != true {
            let strokes = await MainActor.run { action.strokes }
            let outcome = await Result { () async throws(ApplicationFailure) in
                try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) in try action.run(in: process.pid, strokes: strokes) }
            }
            let settled: Result<CGWindowID?, ApplicationFailure> =
                switch outcome {
                    case .failure(let failure) where failure.reason.is(\.noProcess) || failure.reason == .unanswered(action): .success(nil)
                    case let outcome: outcome
                }
            outcomes.append(settled)
            acted.formUnion([settled[case: \.success].flatMap(\.self)].compactMap(\.self))
            blind = blind || (action.element == nil && settled == .success(nil))
        }
        var last: InstanceState?
        for await next in merge(states.map(Optional.some), AsyncTimerSequence(interval: messagingTimeout * 2, clock: clock).map { _ in InstanceState?.none }) {
            if let next, !Self.settled(next, acting: acted, blind: blind) {
                last = next
                continue
            }
            if let state = next ?? last { return (outcomes, state) }
        }
        throw ApplicationFailure(subject: actions.map(\.description).joined(separator: ", "), reason: .cancelled)
    }

    private static func settled(_ state: InstanceState, acting windows: Set<CGWindowID>, blind: Bool) -> Bool {
        switch state {
            case .exited, .failed: true
            case .running(let running) where running.status.is(\.unresponsive): true
            case .running(let running): !running.asking.contains { row in windows.contains(row.number) } && !(blind && !running.asking.isEmpty)
        }
    }
}
