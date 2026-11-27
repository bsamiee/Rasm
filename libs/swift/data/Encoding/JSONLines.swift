package import Foundation

// --- [MODELS] --------------------------------------------------------------------------

package struct JSONLines<Bytes: AsyncIteratorProtocol<UInt8, Failure>, Failure: Error>: AsyncSequence, AsyncIteratorProtocol {
    var bytes: Bytes

    package func makeAsyncIterator() -> Self { self }

    package mutating func next(isolation actor: isolated any Actor?) async throws(Failure) -> Data? {
        var line = Data()
        repeat {
            switch try await bytes.next(isolation: actor) {
                case UInt8(ascii: "\n"): return line
                case let byte?: line.append(byte)
                case nil: return line.isEmpty ? nil : line
            }
        } while true
    }
}

extension JSONLines: Sendable where Bytes: Sendable {}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension JSONEncoder {
    package static var jsonLines: JSONEncoder {
        let encoder = JSONEncoder()
        encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
        encoder.dateEncodingStrategy = .custom { date, target in try date.formatted(Date.ISO8601FormatStyle(includingFractionalSeconds: true)).encode(to: target) }
        return encoder
    }

    package func encodeLine(_ value: some Encodable) throws -> Data {
        try encode(value) + [UInt8(ascii: "\n")]
    }
}

extension AsyncSequence where Element == UInt8 {
    package var jsonLines: JSONLines<AsyncIterator, Failure> { JSONLines(bytes: makeAsyncIterator()) }
}
