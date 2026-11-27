import Carbon.HIToolbox
import CoreGraphics
import Foundation
import Synchronization

// --- [TYPES] ---------------------------------------------------------------------------

enum Modifier: String, CaseIterable, Codable, Sendable {
    case command
    case shift
    case option
    case control
    case function

    var key: (code: CGKeyCode, flag: CGEventFlags, state: UInt32) {
        let (code, flag, device, state): (Int, CGEventFlags, Int32, Int) =
            switch self {
                case .command: (kVK_Command, .maskCommand, NX_DEVICELCMDKEYMASK, cmdKey)
                case .shift: (kVK_Shift, .maskShift, NX_DEVICELSHIFTKEYMASK, shiftKey)
                case .option: (kVK_Option, .maskAlternate, NX_DEVICELALTKEYMASK, optionKey)
                case .control: (kVK_Control, .maskControl, NX_DEVICELCTLKEYMASK, controlKey)
                case .function: (kVK_Function, .maskSecondaryFn, 0, 0)
            }
        return (CGKeyCode(code), flag.union(CGEventFlags(rawValue: UInt64(device))), UInt32(state >> 8))
    }
}

// --- [MODELS] --------------------------------------------------------------------------

struct Stroke: Equatable, Sendable {
    let code: CGKeyCode
    let flags: CGEventFlags
    let text: String?
}

enum Key: Codable, Hashable, Sendable {
    enum Special: String, CaseIterable, Codable, Sendable {
        case `return`, enter, tab, space, delete, forwardDelete, escape, clear, home, end, pageUp, pageDown, left, right, down, up, help, contextualMenu, eisu, kana
        case f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15, f16, f17, f18, f19

        private static let glyphs: [Int: Self] = Dictionary(uniqueKeysWithValues: allCases.map { special in (special.codes.glyph, special) })

        init?(glyph: Int) {
            guard let special = Self.glyphs[glyph] else { return nil }
            self = special
        }

        var codes: (key: CGKeyCode, glyph: Int) {
            let (key, glyph): (Int, Int) =
                switch self {
                    case .return: (kVK_Return, kMenuReturnGlyph)
                    case .enter: (kVK_ANSI_KeypadEnter, kMenuEnterGlyph)
                    case .tab: (kVK_Tab, kMenuTabRightGlyph)
                    case .space: (kVK_Space, kMenuSpaceGlyph)
                    case .delete: (kVK_Delete, kMenuDeleteLeftGlyph)
                    case .forwardDelete: (kVK_ForwardDelete, kMenuDeleteRightGlyph)
                    case .escape: (kVK_Escape, kMenuEscapeGlyph)
                    case .clear: (kVK_ANSI_KeypadClear, kMenuClearGlyph)
                    case .home: (kVK_Home, kMenuNorthwestArrowGlyph)
                    case .end: (kVK_End, kMenuSoutheastArrowGlyph)
                    case .pageUp: (kVK_PageUp, kMenuPageUpGlyph)
                    case .pageDown: (kVK_PageDown, kMenuPageDownGlyph)
                    case .left: (kVK_LeftArrow, kMenuLeftArrowGlyph)
                    case .right: (kVK_RightArrow, kMenuRightArrowGlyph)
                    case .down: (kVK_DownArrow, kMenuDownArrowGlyph)
                    case .up: (kVK_UpArrow, kMenuUpArrowGlyph)
                    case .help: (kVK_Help, kMenuHelpGlyph)
                    case .contextualMenu: (kVK_ContextualMenu, kMenuContextualMenuGlyph)
                    case .eisu: (kVK_JIS_Eisu, kMenuEisuGlyph)
                    case .kana: (kVK_JIS_Kana, kMenuKanaGlyph)
                    case .f1: (kVK_F1, kMenuF1Glyph)
                    case .f2: (kVK_F2, kMenuF2Glyph)
                    case .f3: (kVK_F3, kMenuF3Glyph)
                    case .f4: (kVK_F4, kMenuF4Glyph)
                    case .f5: (kVK_F5, kMenuF5Glyph)
                    case .f6: (kVK_F6, kMenuF6Glyph)
                    case .f7: (kVK_F7, kMenuF7Glyph)
                    case .f8: (kVK_F8, kMenuF8Glyph)
                    case .f9: (kVK_F9, kMenuF9Glyph)
                    case .f10: (kVK_F10, kMenuF10Glyph)
                    case .f11: (kVK_F11, kMenuF11Glyph)
                    case .f12: (kVK_F12, kMenuF12Glyph)
                    case .f13: (kVK_F13, kMenuF13Glyph)
                    case .f14: (kVK_F14, kMenuF14Glyph)
                    case .f15: (kVK_F15, kMenuF15Glyph)
                    case .f16: (kVK_F16, kMenuF16Glyph)
                    case .f17: (kVK_F17, kMenuF17Glyph)
                    case .f18: (kVK_F18, kMenuF18Glyph)
                    case .f19: (kVK_F19, kMenuF19Glyph)
                }
            return (CGKeyCode(key), glyph)
        }
    }

    case special(Special)
    case character(Character)
    case code(CGKeyCode)

    init(from decoder: any Decoder) throws {
        let value = try decoder.singleValueContainer()
        do { self = .code(try value.decode(CGKeyCode.self)) } catch DecodingError.typeMismatch {
            let text = try value.decode(String.self)
            switch (Special(rawValue: text), text.count) {
                case (.some(let special), _): self = .special(special)
                case (.none, 1): self = .character(Character(text))
                case (.none, _):
                    throw DecodingError.dataCorruptedError(in: value, debugDescription: "\(text) is neither one character nor a key of \(Special.allCases.map(\.rawValue).joined(separator: ", "))")
            }
        }
    }

    func encode(to encoder: any Encoder) throws {
        var value = encoder.singleValueContainer()
        switch self {
            case .special(let special): try value.encode(special)
            case .character(let character): try value.encode(String(character))
            case .code(let code): try value.encode(code)
        }
    }

    func strokes(holding modifiers: Set<Modifier>, on layout: KeyboardLayout) -> [Stroke] {
        let typed: [Stroke]
        switch self {
            case .code(let code): typed = [Stroke(code: code, flags: [], text: nil)]
            case .special(let special): typed = [Stroke(code: special.codes.key, flags: [], text: nil)]
            case .character(let character):
                let shown: [Character] = (modifiers.isDisjoint(with: [.command, .control]) ? [] : Array(character.lowercased())) + [character]
                let tables: [[Character: [Stroke]]] = layout.strokes.filter { held, _ in held.isSubset(of: modifiers) }.sorted { first, second in first.key.count > second.key.count }.map(\.value)
                let found: [Stroke]? = shown.lazy.compactMap { (name: Character) -> [Stroke]? in tables.lazy.compactMap { (table: [Character: [Stroke]]) -> [Stroke]? in table[name] }.first }.first
                typed = found ?? [Stroke(code: 0, flags: [], text: String(character))]
        }
        let held = CGEventFlags(modifiers.map(\.key.flag))
        return typed.map { stroke in Stroke(code: stroke.code, flags: stroke.flags.union(held), text: stroke.text) }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct KeyboardLayout: Sendable {
    static let layouts: Mutex<[CGEventSourceKeyboardType: KeyboardLayout]> = {
        unsafe CFNotificationCenterAddObserver(
            CFNotificationCenterGetDistributedCenter(),
            nil,
            { _, _, _, _, _ in Self.layouts.withLock { layouts in layouts.removeAll() } },
            kTISNotifySelectedKeyboardInputSourceChanged,
            nil,
            .deliverImmediately,
        )
        return Mutex([:])
    }()

    let strokes: [Set<Modifier>: [Character: [Stroke]]]

    @MainActor
    static func current(keyboardType: CGEventSourceKeyboardType) -> Self {
        if let kept = layouts.withLock({ layouts in layouts[keyboardType] }) { return kept }
        let source = unsafe TISCopyCurrentKeyboardLayoutInputSource().takeRetainedValue()
        let data: CFData? = unsafe TISGetInputSourceProperty(source, kTISPropertyUnicodeKeyLayoutData).map { data in unsafe Unmanaged<CFData>.fromOpaque(data).takeUnretainedValue() }
        defer { extendLifetime(data) }
        let uchr: UnsafePointer<UCKeyboardLayout>? = unsafe data.flatMap(CFDataGetBytePtr).map { bytes in unsafe UnsafeRawPointer(bytes).assumingMemoryBound(to: UCKeyboardLayout.self) }
        typealias Press = (held: Set<Modifier>, code: CGKeyCode, flags: CGEventFlags, state: UInt32)
        typealias Output = (text: String, dead: UInt32)
        typealias Translation = (press: Press, output: Output)
        typealias Row = (held: Set<Modifier>, character: Character, strokes: [Stroke])
        var characters = [UniChar](repeating: 0, count: Int(UInt8.max))
        let translate = { (code: CGKeyCode, state: UInt32, dead: UInt32) -> Output? in
            var (next, length): (UInt32, Int) = (dead, 0)
            let status: OSStatus = unsafe UCKeyTranslate(uchr, code, UInt16(kUCKeyActionDown), state, keyboardType, 0, &next, characters.count, &length, &characters)
            return status == noErr ? (String(decoding: characters.prefix(length), as: UTF16.self), next) : nil
        }
        let typing: [Set<Modifier>] = [[], [.shift], [.option], [.shift, .option]]
        let states: [(held: Set<Modifier>, flags: CGEventFlags, state: UInt32)] = [[], [.command]].flatMap { (held: Set<Modifier>) in
            typing.map { set in (held, CGEventFlags(set.map(\.key.flag)), held.union(set).reduce(UInt32.zero) { state, modifier in state | modifier.key.state }) }
        }
        let presses: [Press] = states.flatMap { held, flags, state in
            (CGKeyCode(0)...CGKeyCode(keyCodeMask >> 8)).map { code in (held, code, flags, state) }
        }
        let translated: [Translation] = presses.compactMap { (press: Press) -> Translation? in
            translate(press.code, press.state, 0).map { (output: Output) in (press: press, output: output) }
        }
        let sharing: [Set<Modifier>: [Press]] = Dictionary(grouping: presses) { (press: Press) -> Set<Modifier> in press.held }
        let deadEnds: [Translation] = translated.filter { (entry: Translation) -> Bool in entry.output.text.isEmpty && entry.output.dead != 0 }
        let leads: [(lead: Stroke, dead: UInt32, press: Press)] = deadEnds.flatMap { (entry: Translation) -> [(lead: Stroke, dead: UInt32, press: Press)] in
            let lead = Stroke(code: entry.press.code, flags: entry.press.flags, text: "")
            return (sharing[entry.press.held] ?? []).map { (next: Press) in (lead: lead, dead: entry.output.dead, press: next) }
        }
        let singles: [Translation] = translated.filter { (entry: Translation) -> Bool in entry.output.text.count == 1 }
        let single: [Row] = singles.map { (entry: Translation) -> Row in
            (entry.press.held, Character(entry.output.text), [Stroke(code: entry.press.code, flags: entry.press.flags, text: entry.output.text)])
        }
        let composed: [Row] = leads.compactMap { lead, dead, press in
            guard let output = translate(press.code, press.state, dead), output.text.count == 1 else { return nil }
            return (press.held, Character(output.text), [lead, Stroke(code: press.code, flags: press.flags, text: output.text)])
        }
        let rows: [Row] = single + composed
        let grouped: [Set<Modifier>: [Row]] = Dictionary(grouping: rows) { (row: Row) -> Set<Modifier> in row.held }
        let tables: [Set<Modifier>: [Character: [Stroke]]] = grouped.mapValues { (held: [Row]) -> [Character: [Stroke]] in
            let pairs: [(Character, [Stroke])] = held.map { (row: Row) -> (Character, [Stroke]) in (row.character, row.strokes) }
            return Dictionary(pairs) { (kept: [Stroke], next: [Stroke]) -> [Stroke] in next.count < kept.count ? next : kept }
        }
        let layout = Self(strokes: tables)
        layouts.withLock { layouts in layouts[keyboardType] = layout }
        return layout
    }
}
