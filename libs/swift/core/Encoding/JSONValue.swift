// --- [MODELS] --------------------------------------------------------------------------

public enum JSONValue: Codable, Hashable, Sendable {
    case null
    case bool(Bool)
    case number(Double)
    case string(String)
    case array([Self])
    case object([String: Self])

    public init(from decoder: any Decoder) throws {
        func scalar(in container: any SingleValueDecodingContainer) throws -> Self? {
            let read: (() throws -> Self) throws -> Self? = Self.unlessMismatched
            return try read { .bool(try container.decode(Bool.self)) } ?? read { .number(try container.decode(Double.self)) } ?? read { .string(try container.decode(String.self)) }
        }
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        if container.decodeNil() {
            self = .null
        } else if var elements = try Self.unlessMismatched(decoder.unkeyedContainer) {
            self = .array(try Self.decode(&elements))
        } else if let scalar: Self = try scalar(in: container) {
            self = scalar
        } else {
            try Self.checkNesting(at: container.codingPath)
            self = .object(try container.decode([String: Self].self))
        }
    }

    private static func unlessMismatched<Value>(_ read: () throws -> Value) throws -> Value? {
        do { return try read() } catch DecodingError.typeMismatch { return nil }
    }

    private static func checkNesting(at path: [any CodingKey], limit: Int = 200) throws(DecodingError) {
        guard path.count <= limit else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: path, debugDescription: "JSON nests deeper than \(limit) levels"))
        }
    }

    private static func decode(_ elements: inout any UnkeyedDecodingContainer) throws -> [Self] {
        func element(of elements: inout any UnkeyedDecodingContainer) throws -> Self {
            if try elements.decodeNil() { return .null }
            if var inner: any UnkeyedDecodingContainer = try unlessMismatched({ try elements.nestedUnkeyedContainer() }) { return .array(try decode(&inner)) }
            if let flag: Bool = try unlessMismatched({ try elements.decode(Bool.self) }) { return .bool(flag) }
            if let number: Double = try unlessMismatched({ try elements.decode(Double.self) }) { return .number(number) }
            if let text: String = try unlessMismatched({ try elements.decode(String.self) }) { return .string(text) }
            try checkNesting(at: elements.codingPath)
            return .object(try elements.decode([String: Self].self))
        }
        try checkNesting(at: elements.codingPath)
        guard let count: Int = elements.count else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: elements.codingPath, debugDescription: "Unkeyed container holds an unknown element count"))
        }
        return try (0..<count).map { _ in try element(of: &elements) }
    }

    public func encode(to encoder: any Encoder) throws {
        func encodeValue() throws {
            var container: any SingleValueEncodingContainer = encoder.singleValueContainer()
            switch self {
                case .null: try container.encodeNil()
                case .bool(let flag): try container.encode(flag)
                case .number(let number): try container.encode(number)
                case .string(let text): try container.encode(text)
                case .array(let elements): try container.encode(elements)
                case .object(let fields): try container.encode(fields)
            }
        }
        switch self {
            case .array(let elements):
                var container: any UnkeyedEncodingContainer = encoder.unkeyedContainer()
                try Self.encode(elements, into: &container)
            case .null, .bool, .number, .string, .object: try encodeValue()
        }
    }

    private static func encode(_ elements: [Self], into container: inout any UnkeyedEncodingContainer) throws {
        for element: Self in elements { try element.encode(into: &container) }
    }

    private func encode(into container: inout any UnkeyedEncodingContainer) throws {
        switch self {
            case .null: try container.encodeNil()
            case .bool(let flag): try container.encode(flag)
            case .number(let number): try container.encode(number)
            case .string(let text): try container.encode(text)
            case .array(let elements):
                var inner: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer()
                try Self.encode(elements, into: &inner)
            case .object(let fields): try container.encode(fields)
        }
    }
}
