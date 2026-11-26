import Carbon.HIToolbox
public import CoreGraphics
import Foundation
import Results

// --- [MODELS] --------------------------------------------------------------------------

public enum Modifier: String, CaseIterable, Codable, Sendable {
    case command
    case shift
    case option
    case control
    case function

    var key: (code: CGKeyCode, flag: CGEventFlags) {
        let (code, flag): (Int, CGEventFlags) =
            switch self {
                case .command: (kVK_Command, .maskCommand)
                case .shift: (kVK_Shift, .maskShift)
                case .option: (kVK_Option, .maskAlternate)
                case .control: (kVK_Control, .maskControl)
                case .function: (kVK_Function, .maskSecondaryFn)
            }
        return (CGKeyCode(code), flag)
    }
}

public enum Key: Codable, Hashable, Sendable {
    public enum Special: String, CaseIterable, Codable, Sendable {
        case `return`, enter, tab, space, delete, forwardDelete, escape, home, end, pageUp, pageDown, left, right, down, up, help
        case f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12

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
                    case .home: (kVK_Home, kMenuNorthwestArrowGlyph)
                    case .end: (kVK_End, kMenuSoutheastArrowGlyph)
                    case .pageUp: (kVK_PageUp, kMenuPageUpGlyph)
                    case .pageDown: (kVK_PageDown, kMenuPageDownGlyph)
                    case .left: (kVK_LeftArrow, kMenuLeftArrowGlyph)
                    case .right: (kVK_RightArrow, kMenuRightArrowGlyph)
                    case .down: (kVK_DownArrow, kMenuDownArrowGlyph)
                    case .up: (kVK_UpArrow, kMenuUpArrowGlyph)
                    case .help: (kVK_Help, kMenuHelpGlyph)
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

    func strokes(holding modifiers: Set<Modifier>, on layout: KeyboardLayout) throws(ApplicationFailure) -> [(code: CGKeyCode, flags: CGEventFlags, text: String?)] {
        let typed: [(code: CGKeyCode, flags: CGEventFlags, text: String?)]
        switch self {
            case .code(let code): typed = [(code, [], nil)]
            case .special(let special): typed = [(special.codes.key, [], nil)]
            case .character(let character):
                guard let strokes = layout.strokes[character] else {
                    let keyboard: String = layout.name.map { name in "key of keyboard layout \(name)" } ?? "key of the current keyboard layout"
                    throw ApplicationFailure(subject: "key \(character)", reason: .unmatched(Unmatched(subject: keyboard, query: String(character), candidates: [])))
                }
                typed = strokes
        }
        let held = CGEventFlags(modifiers.map(\.key.flag))
        return typed.map { stroke in (stroke.code, stroke.flags.union(held), stroke.text) }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct KeyboardLayout: Sendable {
    let name: String?
    let strokes: [Character: [(code: CGKeyCode, flags: CGEventFlags, text: String?)]]

    @MainActor
    static func current(typing keyboardType: CGEventSourceKeyboardType) -> Self {
        let source = unsafe TISCopyCurrentKeyboardLayoutInputSource().takeRetainedValue()
        let name: String? = unsafe TISGetInputSourceProperty(source, kTISPropertyLocalizedName).map { name in unsafe Unmanaged<CFString>.fromOpaque(name).takeUnretainedValue() as String }
        let data: CFData? = unsafe TISGetInputSourceProperty(source, kTISPropertyUnicodeKeyLayoutData).map { data in unsafe Unmanaged<CFData>.fromOpaque(data).takeUnretainedValue() }
        guard let data, let bytes = unsafe CFDataGetBytePtr(data) else { return Self(name: name, strokes: [:]) }
        let layout = unsafe UnsafeRawPointer(bytes).assumingMemoryBound(to: UCKeyboardLayout.self)
        let states: [(state: Int, flags: CGEventFlags)] = [(0, []), (shiftKey, .maskShift), (optionKey, .maskAlternate), (shiftKey | optionKey, [.maskShift, .maskAlternate])]
        let keys: [(code: CGKeyCode, state: UInt32, flags: CGEventFlags)] = (CGKeyCode(0)...CGKeyCode(keyCodeMask >> 8)).flatMap { code in
            states.map { held in (code, UInt32(held.state >> 8), held.flags) }
        }
        let translate = { (key: (code: CGKeyCode, state: UInt32, flags: CGEventFlags), dead: UInt32) -> (typed: Character?, dead: UInt32)? in
            var (state, length, characters): (UInt32, Int, [UniChar]) = (dead, 0, [0, 0, 0, 0])
            let status = unsafe UCKeyTranslate(layout, key.code, UInt16(kUCKeyActionDown), key.state, keyboardType, 0, &state, characters.count, &length, &characters)
            let text = String(decoding: characters.prefix(length), as: UTF16.self)
            return status == noErr ? (text.count == 1 ? text.first : nil, text.isEmpty ? state : 0) : nil
        }
        let pressed: [(key: (code: CGKeyCode, state: UInt32, flags: CGEventFlags), typed: Character?, dead: UInt32)] = keys.compactMap { key in
            translate(key, 0).map { output in (key, output.typed, output.dead) }
        }
        let single: [(Character, [(code: CGKeyCode, flags: CGEventFlags, text: String?)])] = pressed.compactMap { press in
            press.typed.map { character in (character, [(press.key.code, press.key.flags, String(character))]) }
        }
        let composed: [(Character, [(code: CGKeyCode, flags: CGEventFlags, text: String?)])] = pressed.filter { press in press.dead != 0 }.flatMap { press in
            keys.compactMap { key in translate(key, press.dead)?.typed.map { character in (character, [(press.key.code, press.key.flags, ""), (key.code, key.flags, String(character))]) } }
        }
        return Self(name: name, strokes: Dictionary(single + composed) { first, _ in first })
    }
}
