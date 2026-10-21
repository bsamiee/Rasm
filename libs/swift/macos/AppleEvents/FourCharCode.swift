import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

extension FixedWidthInteger where Self: UnsignedInteger {
    public init?(sdef text: String) {
        switch (text.wholeMatch(of: /0x([0-9A-Fa-f]+)/)?.1, text.data(using: .macOSRoman)) {
            case (.some(let hex), _) where hex.count == Self.bitWidth / 4: self.init(hex, radix: 16)
            case (_, .some(let bytes)) where bytes.count == Self.bitWidth / 8: self = bytes.reduce(0) { code, byte in code << 8 | Self(byte) }
            default: return nil
        }
    }

    public var sdef: String {
        let bytes: [UInt8] = stride(from: Self.bitWidth - 8, through: 0, by: -8).map { shift in UInt8(truncatingIfNeeded: self >> shift) }
        let hex: String = String(self, radix: 16, uppercase: true)
        return (bytes.contains { byte in byte < 0x20 || byte == 0x7F } ? nil : String(bytes: bytes, encoding: .macOSRoman))
            ?? "0x" + String(repeating: "0", count: Self.bitWidth / 4 - hex.count) + hex
    }
}
