import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let splits: [(text: String, lines: [String])] = [
    (text: "{\"a\":1}\r\n\"\u{2028}\"\n\r\n[\u{2029}]", lines: ["{\"a\":1}\r", "\"\u{2028}\"", "\r", "[\u{2029}]"]),
    (text: "1\n2", lines: ["1", "2"]),
    (text: "1\n2\n", lines: ["1", "2"]),
    (text: "a", lines: ["a"]),
    (text: "", lines: []),
    (text: "\n", lines: [""]),
    (text: "\n\na\n\n", lines: ["", "", "a", ""]),
    (text: "\r\r\n\u{85}\u{2029}", lines: ["\r\r", "\u{85}\u{2029}"]),
]
private let lineBytes: [UInt8] = [0x00, 0x09, 0x0B, 0x0C, 0x0D, 0x20, 0x22, 0x5C, 0x7B, 0x7D, 0x61, 0x7F, 0x80, 0xC3, 0xA9, 0xE2, 0x80, 0xA8, 0xA9, 0x85, 0xFE, 0xFF]

// --- [MODELS] --------------------------------------------------------------------------

private enum Interruption: Error, Equatable {
    case closed
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func lines(_ content: [UInt8]) async -> [[UInt8]] {
    let (bytes, continuation): (AsyncStream<UInt8>, AsyncStream<UInt8>.Continuation) = AsyncStream<UInt8>.makeStream()
    for byte: UInt8 in content { continuation.yield(byte) }
    continuation.finish()
    var collected: [[UInt8]] = []
    for await line: Data in bytes.jsonLines { collected.append([UInt8](line)) }
    return collected
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct JSONLines {
    @Test(arguments: splits)
    func splitsOnLineFeedAlone(text: String, expected: [String]) async {
        let actual: [[UInt8]] = await lines(Array(text.utf8))
        expectNoDifference(actual, expected.map { line in Array(line.utf8) })
    }

    @Test
    func splitMatchesReferenceOverArbitraryBytes() async {
        await propertyCheck(input: Gen.element(of: lineBytes).compactMap(\.self).array(of: 0...24).array(of: 0...10), Gen.bool) { pieces, terminated in
            let content: [UInt8] = Array(pieces.joined(separator: [0x0A])) + (terminated ? [0x0A] : [])
            let split: [[UInt8]] = content.split(separator: 0x0A, omittingEmptySubsequences: false).map(Array.init)
            let expected: [[UInt8]] = split.last?.isEmpty == true ? Array(split.dropLast()) : split
            let actual: [[UInt8]] = await lines(content)
            expectNoDifference(actual, expected)
        }
    }

    @Test
    func encodedLinesDecodeBackToTheirValues() async {
        let encoder: JSONEncoder = sortedEncoder()
        await propertyCheck(input: jsonValues().array(of: 0...8)) { values in
            let content: [UInt8] = try values.flatMap { value in try encoder.encodeLine(value) }
            let decoded: [JSONValue] = try await lines(content).map { line in try JSONDecoder().decode(JSONValue.self, from: Data(line)) }
            expectNoDifference(decoded, values)
        }
    }

    @Test
    func encodeLineAppendsOneLineFeedToEncoderOutput() async {
        let encoder: JSONEncoder = sortedEncoder()
        await propertyCheck(input: jsonValues()) { value in
            let line: Data = try encoder.encodeLine(value)
            expectNoDifference(line, try encoder.encode(value) + [0x0A])
            expectNoDifference(line.firstIndex(of: 0x0A), line.index(before: line.endIndex))
        }
    }

    @Test
    func deliversCompleteLinesThenUpstreamFailure() async throws {
        let (stream, continuation): (AsyncThrowingStream<UInt8, any Error>, AsyncThrowingStream<UInt8, any Error>.Continuation) = AsyncThrowingStream<UInt8, any Error>.makeStream()
        for byte: UInt8 in Array("a\nb\nc".utf8) { continuation.yield(byte) }
        continuation.finish(throwing: Interruption.closed)
        var received: [Data] = []
        let failure: Interruption? = await #expect(throws: Interruption.self) {
            for try await line: Data in stream.jsonLines { received.append(line) }
        }
        expectNoDifference(failure, .closed)
        expectNoDifference(received, [Data("a".utf8), Data("b".utf8)])
    }
}
