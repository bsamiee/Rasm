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
        self = try container.decodeNil() ? .null : Self.value(in: container)
    }

    private static func value(in container: any SingleValueDecodingContainer) throws -> Self {
        let first: Result<Self, any Error> = Result { try .bool(container.decode(Bool.self)) }
        let candidates: [() throws -> Self] = [
            { try .number(container.decode(Double.self)) },
            { try .string(container.decode(String.self)) },
            { try .array(container.decode([Self].self)) },
            { try .object(container.decode([String: Self].self)) },
        ]
        return try candidates.reduce(first) { outcome, candidate in
            outcome.flatMapError { _ in Result(catching: candidate) }
        }
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
