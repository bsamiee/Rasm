public import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

extension JSONEncoder {
    public func encodeLine(_ value: some Encodable) throws -> Data {
        try encode(value) + [UInt8(ascii: "\n")]
    }
}
