// --- [MODELS] --------------------------------------------------------------------------

package struct JSONDocument<Known: Codable & Sendable>: Codable, Sendable {
    package let fields: [String: JSONValue]
    package let known: Known

    package init(fields: [String: JSONValue], known: Known) {
        self.fields = fields
        self.known = known
    }

    package init(from decoder: any Decoder) throws {
        fields = try decoder.singleValueContainer().decode([String: JSONValue].self)
        known = try Known(from: decoder)
    }

    package func encode(to encoder: any Encoder) throws {
        try fields.encode(to: encoder)
        try known.encode(to: encoder)
    }
}
