public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum JSONValue: Codable, Hashable, Sendable {
    case null
    case bool(Bool)
    case number(Decimal)
    case double(Double)
    case string(String)
    case array([Self])
    case object([String: Self])

    public init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        func otherwise(_ next: @escaping () throws -> Self) -> (any Error) -> Result<Self, any Error> {
            { error in if case DecodingError.typeMismatch = error { Result(catching: next) } else { .failure(error) } }
        }
        self =
            try container.decodeNil()
            ? .null
            : Result { try .bool(container.decode(Bool.self)) }
                .flatMapError(
                    otherwise {
                        let double: Double = try container.decode(Double.self)
                        return switch (Result { try container.decode(Decimal.self) }) {
                            case .success(let decimal) where !(double.isZero && double.sign == .minus): .number(decimal)
                            case .success, .failure: .double(double)
                        }
                    }
                )
                .flatMapError(otherwise { try .string(container.decode(String.self)) })
                .flatMapError(otherwise { try .array(container.decode([Self].self)) })
                .flatMapError(otherwise { try .object(container.decode([String: Self].self)) })
                .get()
    }

    public func encode(to encoder: any Encoder) throws {
        var container: any SingleValueEncodingContainer = encoder.singleValueContainer()
        switch self {
            case .null: try container.encodeNil()
            case .bool(let value): try container.encode(value)
            case .number(let value): try container.encode(value)
            case .double(let value): try container.encode(value)
            case .string(let value): try container.encode(value)
            case .array(let value): try container.encode(value)
            case .object(let value): try container.encode(value)
        }
    }
}
