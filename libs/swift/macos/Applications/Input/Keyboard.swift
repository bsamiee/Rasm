import Carbon.HIToolbox
public import CoreGraphics
import Foundation
import Results

// --- [MODELS] --------------------------------------------------------------------------

struct Stroke: Equatable, Sendable {
    let code: CGKeyCode
    let flags: CGEventFlags
    let text: String?
}

public enum Modifier: String, CaseIterable, Codable, Sendable {
    case command
    case shift
    case option
    case control
    case function

    var key: (code: CGKeyCode, flag: CGEventFlags, state: UInt32) {
        let (code, flag, state): (Int, CGEventFlags, Int) =
            switch self {
                case .command: (kVK_Command, .maskCommand, cmdKey)
                case .shift: (kVK_Shift, .maskShift, shiftKey)
                case .option: (kVK_Option, .maskAlternate, optionKey)
                case .control: (kVK_Control, .maskControl, controlKey)
                case .function: (kVK_Function, .maskSecondaryFn, 0)
            }
        return (CGKeyCode(code), flag, UInt32(state >> 8))
    }
}

public enum Key: Codable, Hashable, Sendable {
    public enum Special: String, CaseIterable, Codable, Sendable {
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

    public init(from decoder: any Decoder) throws {
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

    public func encode(to encoder: any Encoder) throws {
        var value = encoder.singleValueContainer()
        switch self {
            case .special(let special): try value.encode(special)
            case .character(let character): try value.encode(String(character))
            case .code(let code): try value.encode(code)
        }
    }

    func strokes(holding modifiers: Set<Modifier>, on layout: KeyboardLayout) throws(ApplicationFailure) -> [Stroke] {
        let typed: [Stroke]
        switch self {
            case .code(let code): typed = [Stroke(code: code, flags: [], text: nil)]
            case .special(let special): typed = [Stroke(code: special.codes.key, flags: [], text: nil)]
            case .character(let character):
                let tables: [(key: Set<Modifier>, value: [Character: [Stroke]])] = layout.strokes
                    .filter { held, _ in held.isSubset(of: modifiers) }
                    .sorted { first, second in first.key.count > second.key.count }
                guard let strokes = tables.lazy.compactMap({ table in table.value[character] }).first else {
                    let keyboard: String = layout.name.map { name in "key of keyboard layout \(name)" } ?? "key of the current keyboard layout"
                    throw ApplicationFailure(subject: "key \(character)", reason: .unmatched(Unmatched(subject: keyboard, query: String(character), candidates: [])))
                }
                typed = strokes
        }
        let held = CGEventFlags(modifiers.map(\.key.flag))
        return typed.map { stroke in Stroke(code: stroke.code, flags: stroke.flags.union(held), text: stroke.text) }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct KeyboardLayout: Sendable {
    private struct Press {
        let held: Set<Modifier>
        let code: CGKeyCode
        let state: UInt32
        let flags: CGEventFlags
    }

    private struct Typed {
        let held: Set<Modifier>
        let character: Character
        let strokes: [Stroke]
    }

    let name: String?
    let strokes: [Set<Modifier>: [Character: [Stroke]]]

    @MainActor
    static func current(keyboardType: CGEventSourceKeyboardType) -> Self {
        let source = unsafe TISCopyCurrentKeyboardLayoutInputSource().takeRetainedValue()
        let name: String? = unsafe TISGetInputSourceProperty(source, kTISPropertyLocalizedName).map { name in unsafe Unmanaged<CFString>.fromOpaque(name).takeUnretainedValue() as String }
        let data: CFData? = unsafe TISGetInputSourceProperty(source, kTISPropertyUnicodeKeyLayoutData).map { data in unsafe Unmanaged<CFData>.fromOpaque(data).takeUnretainedValue() }
        guard let data, let bytes = unsafe CFDataGetBytePtr(data) else { return Self(name: name, strokes: [:]) }
        let layout = unsafe UnsafeRawPointer(bytes).assumingMemoryBound(to: UCKeyboardLayout.self)
        let info: UInt32 = unsafe layout.pointee.keyLayoutFeatureInfoOffset
        let longest: Int = info == 0 ? Int(UInt8.max) : Int(unsafe UnsafeRawPointer(bytes).load(fromByteOffset: Int(info), as: UCKeyLayoutFeatureInfo.self).maxOutputStringLength)
        let typing: [Set<Modifier>] = [[], [.shift], [.option], [.shift, .option]]
        let states: [(held: Set<Modifier>, typing: Set<Modifier>)] = [[], [.command]].flatMap { (held: Set<Modifier>) in typing.map { set in (held, set) } }
        let presses: [Press] = (CGKeyCode(0)...CGKeyCode(keyCodeMask >> 8)).flatMap { (code: CGKeyCode) -> [Press] in
            states.map { held, set in
                Press(held: held, code: code, state: held.union(set).reduce(UInt32.zero) { state, modifier in state | modifier.key.state }, flags: CGEventFlags(set.map(\.key.flag)))
            }
        }
        var characters = [UniChar](repeating: 0, count: longest)
        let translate: (Press, UInt32) -> (typed: Character?, dead: UInt32)? = { press, dead in
            var (state, length): (UInt32, Int) = (dead, 0)
            let status = unsafe UCKeyTranslate(layout, press.code, UInt16(kUCKeyActionDown), press.state, keyboardType, 0, &state, characters.count, &length, &characters)
            let text = String(decoding: characters.prefix(length), as: UTF16.self)
            return status == noErr ? (text.count == 1 ? text.first : nil, text.isEmpty ? state : 0) : nil
        }
        let translated: [(press: Press, typed: Character?, dead: UInt32)] = presses.compactMap { press in translate(press, 0).map { output in (press, output.typed, output.dead) } }
        let single: [Typed] = translated.compactMap { entry in
            entry.typed.map { character in Typed(held: entry.press.held, character: character, strokes: [Stroke(code: entry.press.code, flags: entry.press.flags, text: String(character))]) }
        }
        let sharing: [Set<Modifier>: [Press]] = Dictionary(grouping: presses) { press in press.held }
        let composed: [Typed] = translated.filter { entry in entry.dead != 0 }.flatMap { (entry: (press: Press, typed: Character?, dead: UInt32)) -> [Typed] in
            (sharing[entry.press.held] ?? []).compactMap { (press: Press) -> Typed? in
                guard let character: Character = translate(press, entry.dead)?.typed else { return nil }
                let lead = Stroke(code: entry.press.code, flags: entry.press.flags, text: "")
                return Typed(held: press.held, character: character, strokes: [lead, Stroke(code: press.code, flags: press.flags, text: String(character))])
            }
        }
        let tables = Dictionary(grouping: single + composed) { entry in entry.held }
        return Self(name: name, strokes: tables.mapValues { entries in Dictionary(entries.map { entry in (entry.character, entry.strokes) }) { first, _ in first } })
    }
}
