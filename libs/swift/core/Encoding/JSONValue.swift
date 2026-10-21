public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum JSONValue: Codable, Hashable, Sendable {
    case null
    case bool(Bool)
    case number(Decimal)
    case string(String)
    case array([Self])
    case object([String: Self])

    public init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        self =
            try container.decodeNil()
            ? .null
            : Result { try .bool(container.decode(Bool.self)) }
                .flatMapError { _ in Result { try .number(container.decode(Decimal.self)) } }
                .flatMapError { _ in Result { try .string(container.decode(String.self)) } }
                .flatMapError { _ in Result { try .array(container.decode([Self].self)) } }
                .flatMapError { _ in Result { try .object(container.decode([String: Self].self)) } }
                .get()
    }

    public func encode(to encoder: any Encoder) throws {
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
}
