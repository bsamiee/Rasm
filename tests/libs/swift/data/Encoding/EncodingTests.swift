import AsyncAlgorithms
import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private struct Window: Codable, Equatable {
    let number: Int
    let title: String
    let frame: [Double]
    let link: URL
    let icon: Data
    let minimized: Bool?
}

private struct Numbered: Codable, Equatable, Sendable {
    let number: Int
}

private struct Stamp: Encodable {
    let path: String
    let at: Date
}

private enum Role: String, Sendable {
    case button = "AXButton"
}

private enum Event: CaseKeyed {
    case opened(Int)
    case closed
}

private enum Phase: CaseLabeled {
    case idle
    case waiting(seconds: Int)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func nested(_ depth: Int) -> Data { Data((String(repeating: "[", count: depth) + String(repeating: "]", count: depth)).utf8) }

private func refusal(_ decode: () throws -> Any) -> String? {
    do {
        _ = try decode()
        return nil
    } catch {
        return Mirror(reflecting: error).children.first?.label
    }
}

@Suite
struct HexTests {
    @Test
    func roundTrip() async {
        await propertyCheck(input: Gen<UInt8>.value().array(of: 0...64), Gen.bool) { bytes, uppercase in
            expectNoDifference([UInt8](hexEncoded: bytes.hexEncoded(uppercase: uppercase)), bytes)
        }
    }

    @Test(arguments: [("", ""), ("f", "66"), ("fo", "666F"), ("foo", "666F6F"), ("foob", "666F6F62"), ("fooba", "666F6F6261"), ("foobar", "666F6F626172")] as [(String, String)])
    func matchesTheBase16Vectors(text: String, hex: String) {
        expectNoDifference(Array(text.utf8).hexEncoded(uppercase: true), hex)
        expectNoDifference([UInt8](hexEncoded: hex.lowercased()), Array(text.utf8))
    }

    @Test(arguments: ["0", "0g", "zz", "\u{FF21}\u{FF22}", "0 "])
    func refusesNonHexText(text: String) {
        expectNoDifference([UInt8](hexEncoded: text), nil)
    }
}

@Suite
struct JSONValueTests {
    @Test
    func decodesEveryKind() throws {
        let text = Data(#"{"a":null,"b":[true,1.5,"x",[2],{"c":[]}],"d":{"e":"f"}}"#.utf8)
        expectNoDifference(
            try JSONDecoder().decode(JSONValue.self, from: text),
            .object(["a": .null, "b": .array([.bool(true), .number(1.5), .string("x"), .array([.number(2)]), .object(["c": .array([])])]), "d": .object(["e": .string("f")])]),
        )
    }

    @Test
    func encodesEveryKind() throws {
        let value: JSONValue = .array([.null, .bool(false), .number(-3), .string("é"), .array([.array([])]), .object(["k": .array([.null])])])
        expectNoDifference(String(data: try JSONEncoder().encode(value), encoding: .utf8), #"[null,false,-3,"é",[[]],{"k":[null]}]"#)
    }

    @Test
    func refusesNestingDeeperThanOneHundredTwentyEightLevels() {
        expectNoDifference(refusal { try JSONDecoder().decode(JSONValue.self, from: nested(128)) }, nil)
        expectNoDifference(refusal { try JSONDecoder().decode(JSONValue.self, from: nested(129)) }, "dataCorrupted")
    }

    @Test
    func decodesModels() throws {
        let value: JSONValue = .object([
            "number": .number(7), "title": .string("Untitled"), "frame": .array([.number(0), .number(24.5)]), "link": .string("https://example.com/a"), "icon": .string("AAE="),
        ])
        expectNoDifference(
            try value.decode(Window.self),
            Window(number: 7, title: "Untitled", frame: [0, 24.5], link: try #require(URL(string: "https://example.com/a")), icon: Data([0, 1]), minimized: nil),
        )
    }

    @Test
    func keepsUnknownSpellings() throws {
        let read = try JSONDecoder().decode([Lenient<Role>].self, from: Data(#"["AXButton","AXSplitter"]"#.utf8))
        expectNoDifference(read.map(\.rawValue), ["AXButton", "AXSplitter"])
        expectNoDifference(read.map { role in if case .known(.button) = role { true } else { false } }, [true, false])
    }

    @Test
    func keepsFieldsBesideKnownKeys() throws {
        let document = try JSONDecoder().decode(JSONDocument<Numbered>.self, from: Data(#"{"number":1,"extra":"kept"}"#.utf8))
        expectNoDifference(document.fields, ["number": .number(1), "extra": .string("kept")])
        expectNoDifference(document.known, Numbered(number: 1))
    }
}

@Suite
struct CaseEncodingTests {
    @Test
    func keysEachValueByItsCase() throws {
        expectNoDifference(String(data: try JSONEncoder().encode(Event.opened(3)), encoding: .utf8), #"{"opened":3}"#)
        #expect(throws: EncodingError.self) { try JSONEncoder().encode(Event.closed) }
    }

    @Test
    func writesEachCaseAsItsLabel() throws {
        expectNoDifference(String(data: try JSONEncoder().encode([Phase.idle, Phase.waiting(seconds: 2)]), encoding: .utf8), #"["idle","waiting"]"#)
    }
}

@Suite
struct JSONLinesTests {
    @Test(arguments: [("{}\n\n[1]\n\"tail\"", ["{}", "", "[1]", "\"tail\""]), ("{}\n", ["{}"]), ("", [])] as [(String, [String?])])
    func splitsAtNewlines(text: String, expected: [String?]) async {
        var lines: [String?] = []
        for await line in Data(text.utf8).async.jsonLines { lines.append(String(data: line, encoding: .utf8)) }
        expectNoDifference(lines, expected)
    }

    @Test
    func writesOneSortedLineWithFractionalSecondDates() throws {
        let line = try JSONEncoder.lines.encodeLine(Stamp(path: "a/b", at: Date(timeIntervalSince1970: 0.25)))
        expectNoDifference(String(data: line, encoding: .utf8), "{\"at\":\"1970-01-01T00:00:00.250Z\",\"path\":\"a/b\"}\n")
    }
}
