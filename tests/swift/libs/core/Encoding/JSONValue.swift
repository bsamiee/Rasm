import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let boundaryNumbers: [Double] = [
    0, -0.0, 1, -1, 0.1, 1.0 / 3.0, 9_007_199_254_740_992, -9_007_199_254_740_992, 9_007_199_254_740_994, 18_446_744_073_709_551_616, 1e300, -1e300,
    5e-324, 2.2250738585072014e-308, Double.greatestFiniteMagnitude, -Double.greatestFiniteMagnitude, 123_456.789,
]
private let nesting: String = String(repeating: "{\"c\":[", count: 100) + String(repeating: "]}", count: 100)
private let mismatches: [(text: String, kind: String)] = [
    (text: #"{"label":1,"weights":[]}"#, kind: "typeMismatch"),
    (text: #"{"weights":[]}"#, kind: "keyNotFound label"),
    (text: #"{"label":null,"weights":[]}"#, kind: "valueNotFound"),
    (text: #"{"label":"a","weights":[1,"x"]}"#, kind: "typeMismatch"),
    (text: #"{"label":"a","weights":{}}"#, kind: "typeMismatch"),
    (text: #"[]"#, kind: "typeMismatch"),
]
private let textScalars: [Character] = [
    "a", "Z", "0", " ", "\"", "\\", "/", "\n", "\r", "\t", "\u{0}", "\u{1}", "\u{1F}", "\u{7F}", "\u{2028}", "\u{2029}", "\u{FEFF}", "é", "e\u{301}", "ß", "中",
    "🙂", "👩‍👩‍👧",
]

// --- [MODELS] --------------------------------------------------------------------------

private struct Inner: Codable, Equatable {
    let label: String
    let weights: [Double]
}

private struct Node: Codable {
    let c: [Self]
}

private struct Record: Codable, Equatable {
    let count: Int
    let small: Int8
    let unsigned: UInt32
    let ratio: Double
    let single: Float
    let flag: Bool
    let text: String
    let tags: [String]
    let nested: Inner?
    let absent: String?
    let table: [String: Int]
    let link: URL
    let bytes: Data
    let instant: Date
    let extra: JSONValue
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func number(using generator: inout some RandomNumberGenerator) -> Double {
    switch Int.random(in: 0..<4, using: &generator) {
        case 0: boundaryNumbers.randomElement(using: &generator) ?? 0
        case 1: Double(Int.random(in: -9_007_199_254_740_992...9_007_199_254_740_992, using: &generator))
        case 2: Double.random(in: -1e6...1e6, using: &generator)
        default: Double(sign: Bool.random(using: &generator) ? .minus : .plus, exponent: Int.random(in: -1000...1000, using: &generator), significand: Double.random(in: 1..<2, using: &generator))
    }
}

private func text(using generator: inout some RandomNumberGenerator) -> String {
    String((0..<Int.random(in: 0...12, using: &generator)).map { _ in textScalars.randomElement(using: &generator) ?? "a" })
}

private func value(depth: Int, using generator: inout some RandomNumberGenerator) -> JSONValue {
    switch Int.random(in: 0..<(depth > 0 ? 7 : 4), using: &generator) {
        case 0: .null
        case 1: .bool(Bool.random(using: &generator))
        case 2: .number(number(using: &generator))
        case 3: .string(text(using: &generator))
        case 4, 5:
            .array((0..<Int.random(in: 0...5, using: &generator)).map { _ in value(depth: depth - 1, using: &generator) })
        default:
            .object(
                Dictionary(
                    (0..<Int.random(in: 0...5, using: &generator)).map { _ in (text(using: &generator), value(depth: depth - 1, using: &generator)) },
                    uniquingKeysWith: { first, _ in first },
                )
            )
    }
}

func jsonValues() -> Generator<JSONValue, Shrink.None<JSONValue>> {
    Generator { generator in value(depth: Int.random(in: 0...8, using: &generator), using: &generator) }
}

func sortedEncoder() -> JSONEncoder {
    let encoder: JSONEncoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    return encoder
}

private func parsed(_ text: String) throws -> JSONValue {
    try JSONDecoder().decode(JSONValue.self, from: Data(text.utf8))
}

private func codingPath(_ error: DecodingError) -> [String] {
    let context: DecodingError.Context =
        switch error {
            case .typeMismatch(_, let context), .valueNotFound(_, let context), .keyNotFound(_, let context), .dataCorrupted(let context): context
            @unknown default: DecodingError.Context(codingPath: [], debugDescription: "")
        }
    return context.codingPath.map { key in key.intValue.map(String.init) ?? key.stringValue }
}

private func kind(_ error: DecodingError) -> String {
    switch error {
        case .typeMismatch: "typeMismatch"
        case .valueNotFound: "valueNotFound"
        case .keyNotFound(let key, _): "keyNotFound \(key.stringValue)"
        case .dataCorrupted: "dataCorrupted"
        @unknown default: "unknown"
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct JSONValues {
    @Test
    func parsesEachLiteralIntoItsOwnCase() throws {
        expectNoDifference(try parsed("null"), .null)
        expectNoDifference(try parsed("true"), .bool(true))
        expectNoDifference(try parsed("false"), .bool(false))
        expectNoDifference(try parsed("0"), .number(0))
        expectNoDifference(try parsed("1"), .number(1))
        expectNoDifference(try parsed("-2.5e3"), .number(-2500))
        expectNoDifference(try parsed("\"1\""), .string("1"))
        expectNoDifference(try parsed("\"true\""), .string("true"))
        expectNoDifference(try parsed("[]"), .array([]))
        expectNoDifference(try parsed("{}"), .object([:]))
    }

    @Test
    func parsesNestedStructure() throws {
        expectNoDifference(
            try parsed(#"{"a":[1,"x",null,{"b":false}],"c":{"d":[[]]}," ":"🙂"}"#),
            .object([
                "a": .array([.number(1), .string("x"), .null, .object(["b": .bool(false)])]),
                "c": .object(["d": .array([.array([])])]),
                "\u{2028}": .string("🙂"),
            ]),
        )
    }

    @Test
    func holdsIntegersAboveTwoToThe53rdAsNearestDouble() throws {
        expectNoDifference(try parsed("9007199254740993"), .number(9_007_199_254_740_992))
        expectNoDifference(try parsed("-9007199254740993"), .number(-9_007_199_254_740_992))
        expectNoDifference(try parsed("18446744073709551617"), .number(18_446_744_073_709_551_616))
        expectNoDifference(String(bytes: try sortedEncoder().encode(try parsed("9007199254740993")), encoding: .utf8), "9007199254740992")
    }

    @Test
    func encodesAsPlainJSON() throws {
        let value: JSONValue = .object([
            "b": .array([.null, .bool(true), .number(1), .number(-0.5), .string("x\n\u{2028}")]),
            "a": .object([:]),
            "c": .array([]),
        ])
        expectNoDifference(String(bytes: try sortedEncoder().encode(value), encoding: .utf8), #"{"a":{},"b":[null,true,1,-0.5,"x\n"# + "\u{2028}" + #""],"c":[]}"#)
    }

    @Test
    func roundTripIsLosslessAndReencodesByteIdentical() async throws {
        let encoder: JSONEncoder = sortedEncoder()
        await propertyCheck(input: jsonValues()) { value in
            let encoded: Data = try encoder.encode(value)
            let decoded: JSONValue = try JSONDecoder().decode(JSONValue.self, from: encoded)
            expectNoDifference(decoded, value)
            expectNoDifference(String(bytes: try encoder.encode(decoded), encoding: .utf8), String(bytes: encoded, encoding: .utf8))
        }
    }

    @Test
    func keepsSignOfNegativeZero() throws {
        let decoded: JSONValue = try parsed("-0")
        let sign: FloatingPointSign? =
            switch decoded {
                case .number(let number): number.sign
                case .null, .bool, .string, .array, .object: nil
            }
        expectNoDifference(sign, .minus)
        expectNoDifference(String(bytes: try sortedEncoder().encode(decoded), encoding: .utf8), "-0")
    }

    @Test
    func decodesNestingThatJSONDecoderDecodesIntoCodableTypes() async throws {
        let reference: Node = try JSONDecoder().decode(Node.self, from: Data(nesting.utf8))
        expectNoDifference(Array(sequence(first: reference) { node in node.c.first }).count, 100)
        await #expect(processExitsWith: .success) {
            _ = try JSONDecoder().decode(JSONValue.self, from: Data(nesting.utf8))
        }
    }

    @Test
    func roundTripsNestingThatJSONEncoderEncodesFromCodableTypes() async throws {
        let reference: Data = try JSONEncoder().encode((0..<99).reduce(Node(c: [])) { inner, _ in Node(c: [inner]) })
        expectNoDifference(String(bytes: reference, encoding: .utf8), nesting)
        await #expect(processExitsWith: .success) {
            let deep: JSONValue = (0..<100).reduce(JSONValue.array([])) { inner, _ in .object(["c": .array([inner])]) }
            let encoded: Data = try JSONEncoder().encode(deep)
            guard try JSONDecoder().decode(JSONValue.self, from: encoded) == deep else { exit(EXIT_FAILURE) }
        }
    }

    @Test
    func rejectsMalformedText() {
        for text: String in ["", "tru", "[1 2]", "{\"a\"}", "NaN", "Infinity", "'a'", "{a:1}", "\"\\x\"", "[1]]"] {
            let failure: DecodingError? = #expect(throws: DecodingError.self, "\(text.debugDescription)") { try parsed(text) }
            expectNoDifference(failure.map(kind), "dataCorrupted")
        }
    }
}

struct JSONValueDecoding {
    @Test
    func decodesWhatJSONDecoderDecodesFromSameText() async throws {
        let encoder: JSONEncoder = sortedEncoder()
        let records: Generator<Record, some Sequence & SendableMetatype> = zip(
            zip(Gen.int(in: -9_007_199_254_740_992...9_007_199_254_740_992), Gen.int8(), Gen.uint32(), Gen.double(in: -1e12...1e12)),
            zip(Gen.bool, Gen.unicodeScalar(in: " "..."\u{10FF}").map(Character.init).string(of: 0...16), Gen.letterOrNumber.string(of: 0...6).array(of: 0...4)),
            zip(Gen.double(in: -1...1).array(of: 0...3).optional, Gen.int(in: -99...99).array(of: 0...3), Gen.uint8().array(of: 0...24), Gen.double(in: -1e9...1e9)),
            jsonValues(),
        )
        .map { parts in
            Record(
                count: parts.0.0,
                small: parts.0.1,
                unsigned: parts.0.2,
                ratio: parts.0.3,
                single: Float(parts.0.3),
                flag: parts.1.0,
                text: parts.1.1,
                tags: parts.1.2,
                nested: parts.2.0.map { weights in Inner(label: parts.1.1, weights: weights) },
                absent: nil,
                table: Dictionary(parts.2.1.enumerated().map { index, count in ("k\(index)", count) }, uniquingKeysWith: { first, _ in first }),
                link: URL(string: "https://example.com/\(parts.2.1.count)?q=\(parts.1.0)") ?? URL(filePath: "/"),
                bytes: Data(parts.2.2),
                instant: Date(timeIntervalSinceReferenceDate: parts.2.3),
                extra: parts.3,
            )
        }
        await propertyCheck(input: records) { record in
            let encoded: Data = try encoder.encode(record)
            let reference: Record = try JSONDecoder().decode(Record.self, from: encoded)
            let value: JSONValue = try JSONDecoder().decode(JSONValue.self, from: encoded)
            expectNoDifference(try value.decode(Record.self), reference)
        }
    }

    @Test(arguments: mismatches)
    func failsWithJSONDecoderErrorKindAndPath(text: String, expected: String) throws {
        let data: Data = Data(text.utf8)
        let reference: DecodingError = try #require(throws: DecodingError.self) { try JSONDecoder().decode(Inner.self, from: data) }
        let value: JSONValue = try JSONDecoder().decode(JSONValue.self, from: data)
        let failure: DecodingError = try #require(throws: DecodingError.self) { try value.decode(Inner.self) }
        expectNoDifference(kind(reference), expected)
        expectNoDifference(kind(failure), kind(reference))
        expectNoDifference(codingPath(failure), codingPath(reference))
    }

    @Test
    func absentAndNullOptionalsDecodeAsNil() throws {
        let absent: Inner? = try JSONValue.null.decode(Inner?.self)
        expectNoDifference(absent, nil)
        let withNull: [String?] = try JSONValue.array([.string("a"), .null]).decode([String?].self)
        expectNoDifference(withNull, ["a", nil])
    }

    @Test
    func decodesItselfUnchanged() throws {
        let value: JSONValue = .object(["a": .array([.number(1), .null])])
        expectNoDifference(try value.decode(JSONValue.self), value)
    }

    @Test
    func decodesIntegersAboveTwoToThe53rdFromTheirDouble() throws {
        expectNoDifference(try JSONValue.number(9_007_199_254_740_992).decode(Int.self), 9_007_199_254_740_992)
        expectNoDifference(try JSONValue.number(4_294_967_295).decode(UInt32.self), 4_294_967_295)
    }

    @Test
    func rejectsValuesTargetTypeCannotHoldExactly() {
        #expect(throws: DecodingError.self) { try JSONValue.number(3.5).decode(Int.self) }
        #expect(throws: DecodingError.self) { try JSONValue.number(300).decode(Int8.self) }
        #expect(throws: DecodingError.self) { try JSONValue.number(-1).decode(UInt.self) }
        #expect(throws: DecodingError.self) { try JSONValue.number(1e300).decode(Int64.self) }
        #expect(throws: DecodingError.self) { try JSONValue.bool(true).decode(Int.self) }
        #expect(throws: DecodingError.self) { try JSONValue.number(1).decode(Bool.self) }
        #expect(throws: DecodingError.self) { try JSONValue.number(1).decode(String.self) }
        #expect(throws: DecodingError.self) { try JSONValue.string("not base64!").decode(Data.self) }
    }
}
