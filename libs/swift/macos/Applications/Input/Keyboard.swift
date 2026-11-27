import Carbon.HIToolbox
import CoreGraphics
import Foundation

// --- [TYPES] ---------------------------------------------------------------------------

enum Modifier: String, CaseIterable, Codable, Sendable {
    case command
    case shift
    case option
    case control
    case function

    var key: (code: CGKeyCode, flag: CGEventFlags, state: UInt32) {
        let (code, flag, device, carbon): (Int, CGEventFlags, Int32, Int) =
            switch self {
                case .command: (kVK_Command, .maskCommand, NX_DEVICELCMDKEYMASK, cmdKey)
                case .shift: (kVK_Shift, .maskShift, NX_DEVICELSHIFTKEYMASK, shiftKey)
                case .option: (kVK_Option, .maskAlternate, NX_DEVICELALTKEYMASK, optionKey)
                case .control: (kVK_Control, .maskControl, NX_DEVICELCTLKEYMASK, controlKey)
                case .function: (kVK_Function, .maskSecondaryFn, 0, kEventKeyModifierFnMask)
            }
        return (CGKeyCode(code), flag.union(CGEventFlags(rawValue: UInt64(device))), UInt32(carbon >> 8))
    }
}

// --- [MODELS] --------------------------------------------------------------------------

struct Stroke: Equatable, Sendable {
    let code: CGKeyCode
    let flags: CGEventFlags
    let text: String?
}

enum Key: Codable, Hashable, Sendable {
    // --- [KEYS]

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

    // --- [CODING]

    init(from decoder: any Decoder) throws {
        let value = try decoder.singleValueContainer()
        do { self = .code(try value.decode(CGKeyCode.self)) } catch DecodingError.typeMismatch {
            let text = try value.decode(String.self)
            switch (Special(rawValue: text), text.count) {
                case (.some(let special), _): self = .special(special)
                case (.none, 1): self = .character(Character(text))
                case (.none, _):
                    throw DecodingError.dataCorruptedError(
                        in: value,
                        debugDescription: "\(text.debugDescription) is not one character, a key code number, or one of \(Special.allCases.map(\.rawValue).formatted(.list(type: .or)))",
                    )
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

    // --- [STROKES]

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
    // --- [LAYOUT]

    @MainActor
    private static var kept: (source: String?, keyboardType: CGEventSourceKeyboardType, layout: Self)?

    let strokes: [Set<Modifier>: [Character: [Stroke]]]

    @MainActor
    static func current(keyboardType: CGEventSourceKeyboardType) -> Self {
        let source = unsafe TISCopyCurrentKeyboardLayoutInputSource().takeRetainedValue()
        let identifier: String? = unsafe TISGetInputSourceProperty(source, kTISPropertyInputSourceID).map { name in unsafe Unmanaged<CFString>.fromOpaque(name).takeUnretainedValue() as String }
        if let kept, kept.source == identifier, kept.keyboardType == keyboardType { return kept.layout }
        let data: CFData? = unsafe TISGetInputSourceProperty(source, kTISPropertyUnicodeKeyLayoutData).map { data in unsafe Unmanaged<CFData>.fromOpaque(data).takeUnretainedValue() }
        defer { extendLifetime(data) }
        let uchr: UnsafePointer<UCKeyboardLayout>? = unsafe data.flatMap(CFDataGetBytePtr).map { bytes in unsafe UnsafeRawPointer(bytes).assumingMemoryBound(to: UCKeyboardLayout.self) }
        let size: CGKeyCode =
            unsafe uchr.flatMap { (layout: UnsafePointer<UCKeyboardLayout>) -> CGKeyCode? in
                let types = unsafe UnsafeBufferPointer(start: layout.pointer(to: \.keyboardTypeList), count: Int(layout.pointee.keyboardTypeCount))
                let found: UCKeyboardTypeHeader? = unsafe types.first { (header: UCKeyboardTypeHeader) -> Bool in header.keyboardTypeFirst <= keyboardType && keyboardType <= header.keyboardTypeLast }
                let matched: UCKeyboardTypeHeader? = unsafe found ?? types.first
                return matched.map { (header: UCKeyboardTypeHeader) -> CGKeyCode in
                    unsafe UnsafeRawPointer(layout).loadUnaligned(fromByteOffset: Int(header.keyToCharTableIndexOffset), as: UCKeyToCharTableIndex.self).keyToCharTableSize
                }
            } ?? 0
        typealias Press = (held: Set<Modifier>, code: CGKeyCode, flags: CGEventFlags, state: UInt32)
        typealias Output = (text: String, dead: UInt32)
        typealias Row = (held: Set<Modifier>, character: Character, strokes: [Stroke])
        var characters = [UniChar](repeating: 0, count: Int(UInt8.max))
        let translate = { (code: CGKeyCode, state: UInt32, dead: UInt32) -> Output? in
            var (next, length): (UInt32, Int) = (dead, 0)
            let status: OSStatus = unsafe UCKeyTranslate(uchr, code, UInt16(kUCKeyActionDown), state, keyboardType, 0, &next, characters.count, &length, &characters)
            return status == noErr ? (String(decoding: characters.prefix(length), as: UTF16.self), next) : nil
        }
        let typing: [Set<Modifier>] = [[], [.shift], [.option], [.shift, .option]]
        let presses: [Press] = [[], [.command]].flatMap { (held: Set<Modifier>) -> [Press] in
            typing.flatMap { (set: Set<Modifier>) -> [Press] in
                let (flags, state): (CGEventFlags, UInt32) = (CGEventFlags(set.map(\.key.flag)), held.union(set).reduce(UInt32.zero) { state, modifier in state | modifier.key.state })
                return (0..<size).map { (code: CGKeyCode) -> Press in (held, code, flags, state) }
            }
        }
        let translated: [(press: Press, output: Output)] = presses.compactMap { (press: Press) -> (press: Press, output: Output)? in
            translate(press.code, press.state, 0).map { (output: Output) -> (press: Press, output: Output) in (press, output) }
        }
        let sharing: [Set<Modifier>: [Press]] = Dictionary(grouping: presses) { (press: Press) -> Set<Modifier> in press.held }
        let rows: [Row] = translated.flatMap { (lead: Press, output: Output) -> [Row] in
            let follows: [(strokes: [Stroke], press: Press, output: Output?)] =
                if output.text.isEmpty, output.dead != 0 {
                    sharing[lead.held, default: []].map { (press: Press) -> (strokes: [Stroke], press: Press, output: Output?) in
                        ([Stroke(code: lead.code, flags: lead.flags, text: "")], press, translate(press.code, press.state, output.dead))
                    }
                } else {
                    [([], lead, output)]
                }
            return follows.compactMap { (strokes: [Stroke], press: Press, output: Output?) -> Row? in
                guard let output, output.text.count == 1 else { return nil }
                return (press.held, Character(output.text), strokes + [Stroke(code: press.code, flags: press.flags, text: output.text)])
            }
        }
        let tables: [Set<Modifier>: [Character: [Stroke]]] = rows.reduce(into: [:]) { (built: inout [Set<Modifier>: [Character: [Stroke]]], row: Row) in
            let shortest: [Stroke] = built[row.held]?[row.character] ?? row.strokes
            built[row.held, default: [:]][row.character] = row.strokes.count < shortest.count ? row.strokes : shortest
        }
        let layout = Self(strokes: tables)
        kept = (identifier, keyboardType, layout)
        return layout
    }

    // --- [HOT_KEYS]

    @MainActor
    static var symbolicHotKeys: [CGKeyCode: Set<Set<Modifier>>] {
        get throws(ApplicationFailure) {
            var listed: Unmanaged<CFArray>?
            let status: OSStatus = unsafe CopySymbolicHotKeys(&listed)
            guard status == noErr else { throw ApplicationFailure(subject: "Symbolic hot keys", reason: .foundation(NSError(domain: NSOSStatusErrorDomain, code: Int(status)))) }
            let rows: [[String: Any]] = unsafe listed?.takeRetainedValue() as? [[String: Any]] ?? []
            return rows.reduce(into: [:]) { (hotKeys: inout [CGKeyCode: Set<Set<Modifier>>], row: [String: Any]) in
                guard row[kHISymbolicHotKeyEnabled as String] as? Bool == true, let code = (row[kHISymbolicHotKeyCode as String] as? Int).flatMap({ code in CGKeyCode(exactly: code) }),
                    let state = row[kHISymbolicHotKeyModifiers as String] as? Int
                else { return }
                hotKeys[code, default: []].insert(Set(Modifier.allCases.filter { (modifier: Modifier) -> Bool in state & Int(modifier.key.state) << 8 != 0 }))
            }
        }
    }
}
