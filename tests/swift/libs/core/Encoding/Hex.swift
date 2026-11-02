import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let nonHexCharacters: [Character] = ["\u{FF10}", "\u{FF11}", "\u{FF21}", "\u{FF41}", "\u{0660}", "\u{06F3}", "\u{0966}", "\u{00B9}", "\u{2460}", "g", "G", "x", " ", "-", "\u{0301}"]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Hex {
    @Test
    func encodesTwoLowercaseDigitsPerByteHighNibbleFirst() async {
        let lowercaseDigits: [Character] = Array("0123456789abcdef")
        await propertyCheck(input: Gen.uint8().array(of: 0...512)) { bytes in
            let expected: String = String(bytes.flatMap { byte in [lowercaseDigits[Int(byte >> 4)], lowercaseDigits[Int(byte & 0x0F)]] })
            expectNoDifference(bytes.hexEncoded(), expected)
            expectNoDifference(bytes.hexEncoded(uppercase: true), expected.uppercased())
            expectNoDifference(Data(bytes).hexEncoded(), expected)
        }
    }

    @Test
    func roundTrip() async {
        await propertyCheck(input: Gen.uint8().array(of: 0...512)) { bytes in
            expectNoDifference([UInt8](hexEncoded: bytes.hexEncoded()), bytes)
            expectNoDifference([UInt8](hexEncoded: bytes.hexEncoded(uppercase: true)), bytes)
            expectNoDifference(Data(hexEncoded: bytes.hexEncoded()), Data(bytes))
        }
    }

    @Test
    func decodesMixedCaseAndSubstrings() {
        expectNoDifference([UInt8](hexEncoded: "aBcD0f"), [0xAB, 0xCD, 0x0F])
        expectNoDifference([UInt8](hexEncoded: "--ff00--".dropFirst(2).dropLast(2)), [0xFF, 0x00])
        expectNoDifference([UInt8](hexEncoded: ""), [])
    }

    @Test
    func rejectsOddLengthPrefixesAndWhitespace() {
        for text: String in ["a", "abc", "0x00", " 00", "00 ", "0 0", "00\n"] {
            expectNoDifference([UInt8](hexEncoded: text), nil, "\(text.debugDescription)")
        }
    }

    @Test
    func rejectsOneNonHexCharacterAnywhere() async {
        await propertyCheck(input: Gen.uint8().array(of: 1...64), Gen.element(of: nonHexCharacters).compactMap(\.self), Gen.int(in: 0...127)) { bytes, character, position in
            let digits: [Character] = Array(bytes.hexEncoded())
            let index: Int = position % digits.count
            let text: String = String(digits[..<index]) + String(character) + String(digits[(index + 1)...])
            expectNoDifference([UInt8](hexEncoded: text), nil, "\(text.debugDescription)")
        }
    }
}
