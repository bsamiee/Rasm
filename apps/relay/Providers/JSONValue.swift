import Foundation

// --- [MODELS] --------------------------------------------------------------------------

nonisolated enum JSONValue: Codable, Equatable, Sendable {
    case null
    case bool(Bool)
    case number(Double)
    case string(String)
    case array([Self])
    case object([String: Self])

    init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        self =
            try container.decodeNil()
            ? .null
            : Result { try .bool(container.decode(Bool.self)) }
                .flatMapError { _ in Result { try .number(container.decode(Double.self)) } }
                .flatMapError { _ in Result { try .string(container.decode(String.self)) } }
                .flatMapError { _ in Result { try .array(container.decode([Self].self)) } }
                .flatMapError { _ in Result { try .object(container.decode([String: Self].self)) } }
                .get()
    }

    func encode(to encoder: any Encoder) throws {
        var container: any SingleValueEncodingContainer = encoder.singleValueContainer()
        switch self {
            case .null: try container.encodeNil()
            case .bool(let value): try container.encode(value)
            case .number(let value): try container.encode(value)
            case .string(let value): try container.encode(value)
            case .array(let value): try container.encode(value)
            case .object(let value): try container.encode(value)
        }
    }

    func decode<Value: Decodable>(as type: Value.Type, by decoder: JSONDecoder = JSONDecoder()) throws -> Value {
        try decoder.decode(type, from: JSONEncoder().encode(self))
    }
}

nonisolated struct JSONDocument<Known: Sendable>: Sendable {
    let fields: [String: JSONValue]
    let known: Known
}

nonisolated extension JSONDocument: Decodable where Known: Decodable {
    init(from decoder: any Decoder) throws {
        fields = try decoder.singleValueContainer().decode([String: JSONValue].self)
        known = try Known(from: decoder)
    }
}

nonisolated extension JSONDocument: Encodable where Known: Encodable {
    func encode(to encoder: any Encoder) throws {
        try fields.encode(to: encoder)
        try known.encode(to: encoder)
    }
}

nonisolated struct Lenient<Value: Decodable & Sendable>: Decodable, Sendable {
    let value: Value?

    init(value: Value?) {
        self.value = value
    }

    init(from decoder: any Decoder) {
        value = try? Value(from: decoder)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated extension KeyedDecodingContainer {
    func decode<Value>(_: Lenient<Value>.Type, forKey key: Key) -> Lenient<Value> {
        Lenient(value: try? decode(Value.self, forKey: key))
    }
}
