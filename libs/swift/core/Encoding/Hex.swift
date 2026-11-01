// --- [OPERATIONS] ----------------------------------------------------------------------

extension Sequence<UInt8> {
    public func hexEncoded(uppercase: Bool = false) -> String {
        map { byte in String(byte >> 4, radix: 16, uppercase: uppercase) + String(byte & 0x0F, radix: 16, uppercase: uppercase) }.joined()
    }
}

extension RangeReplaceableCollection<UInt8> {
    public init?(hexEncoded text: some StringProtocol) {
        let nibbles: [Int] = text.compactMap { character in character.isASCII ? character.hexDigitValue : nil }
        guard nibbles.count == text.count, nibbles.count.isMultiple(of: 2) else { return nil }
        self.init(stride(from: 0, to: nibbles.count, by: 2).map { index in UInt8(nibbles[index] << 4 | nibbles[index + 1]) })
    }
}
