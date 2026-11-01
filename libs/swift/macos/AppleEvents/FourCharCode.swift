import Encoding
import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

extension FixedWidthInteger where Self: UnsignedInteger {
    public init?(sdef text: String) {
        switch (text.wholeMatch(of: /0x(.+)/).flatMap { match in [UInt8](hexEncoded: match.1) }, text.data(using: .macOSRoman).map { data in [UInt8](data) }) {
            case (.some(let bytes), _) where bytes.count == Self.bitWidth / 8, (_, .some(let bytes)) where bytes.count == Self.bitWidth / 8:
                self = bytes.reduce(0) { code, byte in code << 8 | Self(byte) }
            default: return nil
        }
    }

    public var sdef: String {
        let bytes: [UInt8] = stride(from: Self.bitWidth - 8, through: 0, by: -8).map { shift in UInt8(truncatingIfNeeded: self >> shift) }
        return (bytes.contains { byte in byte < 0x20 || byte == 0x7F } ? nil : String(bytes: bytes, encoding: .macOSRoman)) ?? "0x" + bytes.hexEncoded(uppercase: true)
    }
}
