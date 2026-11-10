// --- [MODELS] --------------------------------------------------------------------------

public enum JSONValue: Codable, Hashable, Sendable {
    case null
    case bool(Bool)
    case number(Double)
    case string(String)
    case array([Self])
    case object([String: Self])

    public init(from decoder: any Decoder) throws {
        let container = try decoder.singleValueContainer()
        if container.decodeNil() {
            self = .null
        } else if let flag = try Self.unlessMismatched({ try container.decode(Bool.self) }) {
            self = .bool(flag)
        } else if let number = try Self.unlessMismatched({ try container.decode(Double.self) }) {
            self = .number(number)
        } else if let text = try Self.unlessMismatched({ try container.decode(String.self) }) {
            self = .string(text)
        } else if var elements = try Self.unlessMismatched(decoder.unkeyedContainer) {
            try Self.checkNesting(at: decoder.codingPath)
            var decoded: [Self] = []
            while !elements.isAtEnd { decoded.append(try Self(from: elements.superDecoder())) }
            self = .array(decoded)
        } else {
            try Self.checkNesting(at: decoder.codingPath)
            self = .object(try container.decode([String: Self].self))
        }
    }

    private static func unlessMismatched<Value>(_ read: () throws -> Value) throws -> Value? {
        do { return try read() } catch DecodingError.typeMismatch { return nil }
    }

    private static func checkNesting(at path: [any CodingKey]) throws(DecodingError) {
        guard path.count <= 200 else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: path, debugDescription: "JSON nests deeper than 200 levels"))
        }
    }

    public func encode(to encoder: any Encoder) throws {
        switch self {
            case .null:
                var container = encoder.singleValueContainer()
                try container.encodeNil()
            case .bool(let flag): try flag.encode(to: encoder)
            case .number(let number): try number.encode(to: encoder)
            case .string(let text): try text.encode(to: encoder)
            case .array(let elements):
                var container = encoder.unkeyedContainer()
                for element in elements {
                    switch element {
                        case .null: try container.encodeNil()
                        case .bool(let flag): try container.encode(flag)
                        case .number(let number): try container.encode(number)
                        case .string(let text): try container.encode(text)
                        case .array: try element.encode(to: container.superEncoder())
                        case .object(let fields): try container.encode(fields)
                    }
                }
            case .object(let fields):
                var container = encoder.singleValueContainer()
                try container.encode(fields)
        }
    }
}
