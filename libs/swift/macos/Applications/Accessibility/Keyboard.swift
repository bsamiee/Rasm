import Carbon.HIToolbox
public import CoreGraphics
import Foundation

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
