public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

struct JSONLines<Bytes: AsyncIteratorProtocol<UInt8, Failure>, Failure: Error>: AsyncSequence, AsyncIteratorProtocol {
    var bytes: Bytes

    func makeAsyncIterator() -> Self { self }

    mutating func next(isolation actor: isolated any Actor?) async throws(Failure) -> Data? {
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

// --- [OPERATIONS] ----------------------------------------------------------------------

extension JSONEncoder {
    public func encodeLine(_ value: some Encodable) throws -> Data {
        try encode(value) + [UInt8(ascii: "\n")]
    }
}

extension AsyncSequence where Element == UInt8 {
    public var jsonLines: some AsyncSequence<Data, Failure> { JSONLines(bytes: makeAsyncIterator()) }
}
