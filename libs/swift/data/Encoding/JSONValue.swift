import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum JSONValue: Codable, Hashable, Sendable {
    case null
    case bool(Bool)
    case number(Double)
    case string(String)
    case array([Self])
    case object([String: Self])

    public init(from decoder: any Decoder) throws {
        guard decoder.codingPath.count < 128 else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "JSON nests deeper than 128 levels"))
        }
        let container = try decoder.singleValueContainer()
        self =
            if container.decodeNil() {
                .null
            } else {
                try Self.unlessMismatched { Self.bool(try container.decode(Bool.self)) } ?? Self.unlessMismatched { Self.number(try container.decode(Double.self)) }
                    ?? Self.unlessMismatched { Self.string(try container.decode(String.self)) } ?? Self.unlessMismatched { Self.array(try container.decode([Self].self)) }
                    ?? Self.object(try container.decode([String: Self].self))
            }
    }

    private static func unlessMismatched(_ read: () throws -> Self) throws -> Self? {
        do { return try read() } catch DecodingError.typeMismatch { return nil }
    }

    public func encode(to encoder: any Encoder) throws {
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

    public func decode<Value: Decodable>(_: Value.Type = Value.self) throws -> Value { try JSONDecoder().decode(Value.self, from: JSONEncoder().encode(self)) }
}
