// --- [MODELS] --------------------------------------------------------------------------

@propertyWrapper
public struct Lenient<Value: Decodable & Sendable>: Decodable, Sendable {
    public let wrappedValue: Value?

    public init(wrappedValue: Value?) {
        self.wrappedValue = wrappedValue
    }

    public init(from decoder: any Decoder) {
        wrappedValue = try? Value(from: decoder)
    }
}

extension Lenient: Encodable where Value: Encodable {
    public func encode(to encoder: any Encoder) throws {
        var container: any SingleValueEncodingContainer = encoder.singleValueContainer()
        try container.encode(wrappedValue)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension KeyedDecodingContainer {
    public func decode<Value>(_: Lenient<Value>.Type, forKey key: Key) -> Lenient<Value> {
        Lenient(wrappedValue: try? decode(Value.self, forKey: key))
    }
}

extension KeyedEncodingContainer {
    public mutating func encode<Value: Encodable>(_ value: Lenient<Value>, forKey key: Key) throws {
        try encodeIfPresent(value.wrappedValue, forKey: key)
    }
}
