import AppKit
import ApplicationServices
import AsyncAlgorithms
import Carbon.HIToolbox
import CasePathsCore
public import CoreGraphics
public import Encoding
import Processes
import Results

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
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ElementAction {
    fileprivate func run(in process: pid_t, stroke: (code: CGKeyCode, shift: Bool, text: String?)?) throws(ApplicationFailure) -> CGWindowID? {
        if case .press(_, kAXRaiseAction) = self {
            throw ApplicationFailure(subject: "\(self), raising a window takes focus from the frontmost application", reason: .refused(nil))
        }
        guard let id = element else {
            guard case .key(_, let modifiers) = self, let stroke else { return nil }
            let focused: CGWindowID?
            do throws(ApplicationFailure) {
                focused = try (AXUIElement.application(process).value(kAXFocusedWindowAttribute) as CFTypeRef?).flatMap(AXUIElement.checked)?.window
            } catch  where error.reason.is(\.unanswered) {
                focused = nil
            }
            let (source, flags) = (CGEventSource(stateID: .privateState), CGEventFlags(modifiers.map(\.flag)).union(stroke.shift ? .maskShift : []))
            for down in [true, false] {
                guard let event = CGEvent(keyboardEventSource: source, virtualKey: stroke.code, keyDown: down) else { throw ApplicationFailure(subject: description, reason: .refused(nil)) }
                event.flags = flags
                let units = Array(stroke.text?.utf16 ?? "".utf16)
                if !units.isEmpty { units.withUnsafeBufferPointer { units in unsafe event.keyboardSetUnicodeString(stringLength: units.count, unicodeString: units.baseAddress) } }
                event.postToPid(process)
            }
            return focused
        }
        guard let node = try AXUIElement.element(id.token), let owner = node.process else { throw ApplicationFailure(subject: id.description, reason: .unresolved) }
        if owner != process, NSRunningApplication(processIdentifier: owner)?.isDialogHelper != true { throw ApplicationFailure(subject: id.description, reason: .foreign(process)) }
        let read = try node.values([kAXEnabledAttribute, kAXWindowAttribute, kAXRoleAttribute])
        let outcome: AXError =
            switch (self, read.values[kAXEnabledAttribute] as? Bool, read.values[kAXRoleAttribute] as? String) {
                case (.press, false, _), (.choose, false, _): throw ApplicationFailure(subject: description, reason: .disabled)
                case (.press(_, let action), _, _): AXUIElementPerformAction(node, action as CFString)
                case (.set(_, let value), _, _): try node.write(value.accessibility, to: kAXValueAttribute)
                case (.choose(_, let item), _, kAXComboBoxRole): try node.write(item as CFString, to: kAXValueAttribute)
                case (.choose(_, let item), _, _): try choose(item, on: node)
                case (.key, _, _): .success
            }
        guard outcome != .success else { return read.values[kAXWindowAttribute].flatMap(AXUIElement.checked)?.window }
        throw ApplicationFailure(outcome, subject: description, process: owner, answering: self) {
            ApplicationFailure(subject: description, reason: outcome == .invalidUIElement ? .unresolved : .refused(outcome))
        }
    }

    private func choose(_ item: String, on node: AXUIElement) throws(ApplicationFailure) -> AXError {
        let items = { () throws(ApplicationFailure) -> [(element: AXUIElement, label: String?)] in
            try node.menus().map { menu throws(ApplicationFailure) in
                try menu.elements(kAXChildrenAttribute).map { entry throws(ApplicationFailure) in
                    (entry, try entry.label(entry.values([kAXTitleAttribute, kAXDescriptionAttribute, kAXTitleUIElementAttribute])))
                }
            }
            .flatMap(\.self)
        }
        if let listed = try items().first(where: { entry in entry.label == item }) { return AXUIElementPerformAction(listed.element, kAXPressAction as CFString) }
        let opened = AXUIElementPerformAction(node, (try node.actionNames().contains(kAXShowMenuAction) ? kAXShowMenuAction : kAXPressAction) as CFString)
        guard opened == .success else { return opened }
        let shown = try items()
        guard let match = shown.first(where: { entry in entry.label == item }) else {
            for menu in try node.menus() { _ = AXUIElementPerformAction(menu, kAXCancelAction as CFString) }
            throw ApplicationFailure(subject: description, reason: .unmatched(Unmatched(subject: "menu item of \(description)", query: item, candidates: shown.compactMap(\.label))))
        }
        return AXUIElementPerformAction(match.element, kAXPressAction as CFString)
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
            let stroke: (code: CGKeyCode, shift: Bool, text: String?)? = if case .key(let key, _) = action { await MainActor.run { key.stroke } } else { nil }
            let outcome = await Result { () async throws(ApplicationFailure) in
                try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) in try action.run(in: process.pid, stroke: stroke) }
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
