// --- [OPERATIONS] ----------------------------------------------------------------------

extension Sequence<UInt8> {
    public var hexEncoded: String {
        map { byte in String(byte >> 4, radix: 16) + String(byte & 0x0F, radix: 16) }.joined()
    }
}
