import Foundation

// --- [MODELS] --------------------------------------------------------------------------

package enum JSONValue: Codable, Hashable, Sendable {
    case null
    case bool(Bool)
    case number(Double)
    case string(String)
    case array([Self])
    case object([String: Self])

    package init(from decoder: any Decoder) throws {
        guard decoder.codingPath.count < 128 else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "JSON nests deeper than 128 levels"))
        }
        let container = try decoder.singleValueContainer()
        self =
            if container.decodeNil() {
                .null
            } else {
                try container.decodeIfMatching(Bool.self).map(Self.bool) ?? container.decodeIfMatching(Double.self).map(Self.number)
                    ?? container.decodeIfMatching(String.self).map(Self.string) ?? container.decodeIfMatching([Self].self).map(Self.array)
                    ?? .object(container.decode([String: Self].self))
            }
    }

    package func encode(to encoder: any Encoder) throws {
        var container = encoder.singleValueContainer()
        switch self {
            case .null: try container.encodeNil()
            case .bool(let flag): try container.encode(flag)
            case .number(let number): try container.encode(number)
            case .string(let text): try container.encode(text)
            case .array(let elements): try container.encode(elements)
            case .object(let fields): try container.encode(fields)
        }
    }

    package func decode<Value: Decodable>(_: Value.Type = Value.self) throws -> Value { try JSONDecoder().decode(Value.self, from: JSONEncoder().encode(self)) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension SingleValueDecodingContainer {
    package func decodeIfMatching<Value: Decodable>(_: Value.Type) throws -> Value? {
        do { return try decode(Value.self) } catch DecodingError.typeMismatch { return nil }
    }
}
