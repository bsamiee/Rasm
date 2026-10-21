public import Foundation
public import Subprocess

// --- [OPERATIONS] ----------------------------------------------------------------------

extension SubprocessOutputSequence {
    public var jsonLines: some AsyncSequence<Data, any Error> {
        strings(separatedBy: .unicodeScalarSequence(["\n"]), bufferingPolicy: .unbounded).map { line in Data(line.utf8) }
    }
}

extension StandardInputWriter {
    public nonisolated func writeLine(_ value: some Encodable, encoder: JSONEncoder = JSONEncoder()) async throws {
        let line: Data = try encoder.encode(value) + [UInt8(ascii: "\n")]
        _ = try await write(line)
    }
}
