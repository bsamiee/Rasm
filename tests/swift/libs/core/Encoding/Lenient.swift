import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

// --- [TYPES] ---------------------------------------------------------------------------

private enum Tier: String, Sendable {
    case pro
    case max = "max-5x"
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func spelling(_ tier: Lenient<Tier>) -> String {
    switch tier {
        case .known(let known): "known \(known.rawValue)"
        case .unknown(let text): "unknown \(text)"
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct LenientValues {
    @Test
    func readsKnownSpellingsAsKnownCases() {
        expectNoDifference(spelling(Lenient<Tier>(rawValue: "pro")), "known pro")
        expectNoDifference(spelling(Lenient<Tier>(rawValue: "max-5x")), "known max-5x")
    }

    @Test
    func keepsEveryOtherSpellingVerbatim() {
        expectNoDifference(spelling(Lenient<Tier>(rawValue: "PRO")), "unknown PRO")
        expectNoDifference(spelling(Lenient<Tier>(rawValue: "max")), "unknown max")
        expectNoDifference(spelling(Lenient<Tier>(rawValue: "")), "unknown ")
    }

    @Test
    func wireRoundTripKeepsTheSpelling() async {
        let spellings: [String] = ["pro", "max-5x", "team", "", "\u{2028}", "Pro "]
        await propertyCheck(input: Gen.oneOf(Gen.element(of: spellings).compactMap(\.self), Gen.letterOrNumber.string(of: 0...8))) { text in
            let lenient: Lenient<Tier> = Lenient(rawValue: text)
            let encoded: Data = try JSONEncoder().encode(lenient)
            expectNoDifference(try JSONDecoder().decode(String.self, from: encoded), text)
            let decoded: Lenient<Tier> = try JSONDecoder().decode(Lenient<Tier>.self, from: encoded)
            expectNoDifference(spelling(decoded), spelling(lenient))
            expectNoDifference(decoded.rawValue, text)
        }
    }

    @Test
    func rejectsNonStringWire() {
        for text: String in ["1", "null", "true", "[\"pro\"]"] {
            #expect(throws: DecodingError.self, "\(text)") { try JSONDecoder().decode(Lenient<Tier>.self, from: Data(text.utf8)) }
        }
    }
}
