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

private enum Role: String, Sendable {
    case button = "AXButton"
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct HexTests {
    @Test
    func roundTrip() async {
        await propertyCheck(input: Gen<UInt8>.value().array(of: 0...64), Gen.bool) { bytes, uppercase in
            expectNoDifference([UInt8](hexEncoded: bytes.hexEncoded(uppercase: uppercase)), bytes)
        }
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
    func boundsNesting() throws {
        let nested: (Int) -> Data = { depth in Data((String(repeating: "[", count: depth) + String(repeating: "]", count: depth)).utf8) }
        _ = try JSONDecoder().decode(JSONValue.self, from: nested(150))
        #expect(throws: DecodingError.self) { try JSONDecoder().decode(JSONValue.self, from: nested(250)) }
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
    func refusesInexactIntegers() {
        #expect(throws: DecodingError.self) { try JSONValue.number(1.5).decode(Int.self) }
        #expect(throws: DecodingError.self) { try JSONValue.object([:]).decode(Window.self) }
        #expect(throws: DecodingError.self) { try JSONValue.null.decode(String.self) }
    }

    @Test
    func keepsUnknownSpellings() throws {
        let read: [Lenient<Role>] = try JSONDecoder().decode([Lenient<Role>].self, from: Data(#"["AXButton","AXSplitter"]"#.utf8))
        expectNoDifference(read.map(\.rawValue), ["AXButton", "AXSplitter"])
        expectNoDifference(read.map { role in if case .known(.button) = role { true } else { false } }, [true, false])
    }

    @Test
    func keepsFieldsBesideKnownKeys() throws {
        let document: JSONDocument<[String: Int]> = try JSONDecoder().decode(JSONDocument<[String: Int]>.self, from: Data(#"{"a":1,"b":2}"#.utf8))
        expectNoDifference(document.fields, ["a": .number(1), "b": .number(2)])
        expectNoDifference(document.known, ["a": 1, "b": 2])
    }
}

@Suite
struct JSONLinesTests {
    @Test
    func splitsAtNewlines() async {
        var lines: [Data] = []
        for await line: Data in Data("{}\n\n[1]\n\"tail\"".utf8).async.jsonLines { lines.append(line) }
        expectNoDifference(lines.map { line in String(data: line, encoding: .utf8) }, ["{}", "", "[1]", "\"tail\""])
    }

    @Test
    func writesOneLinePerValue() throws {
        expectNoDifference(try JSONEncoder().encodeLine([1, 2]), Data("[1,2]\n".utf8))
    }
}
