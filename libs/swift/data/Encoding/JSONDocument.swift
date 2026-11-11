// --- [MODELS] --------------------------------------------------------------------------

public struct JSONDocument<Known: Codable & Sendable>: Codable, Sendable {
    public let fields: [String: JSONValue]
    public let known: Known

    public init(fields: [String: JSONValue], known: Known) {
        self.fields = fields
        self.known = known
    }

    public init(from decoder: any Decoder) throws {
        fields = try decoder.singleValueContainer().decode([String: JSONValue].self)
        known = try Known(from: decoder)
    }

    public func encode(to encoder: any Encoder) throws {
        try fields.encode(to: encoder)
        try known.encode(to: encoder)
    }
}
