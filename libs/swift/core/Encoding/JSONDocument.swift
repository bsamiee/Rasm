// --- [MODELS] --------------------------------------------------------------------------

public struct JSONDocument<Known: Sendable>: Sendable {
    public let fields: [String: JSONValue]
    public let known: Known

    public init(fields: [String: JSONValue], known: Known) {
        self.fields = fields
        self.known = known
    }
}

extension JSONDocument: Decodable where Known: Decodable {
    public init(from decoder: any Decoder) throws {
        fields = try decoder.singleValueContainer().decode([String: JSONValue].self)
        known = try Known(from: decoder)
    }
}

extension JSONDocument: Encodable where Known: Encodable {
    public func encode(to encoder: any Encoder) throws {
        try fields.encode(to: encoder)
        try known.encode(to: encoder)
    }
}
