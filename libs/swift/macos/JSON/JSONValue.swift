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

public struct JSONDocument<Known: Sendable>: Sendable {
    public let fields: [String: JSONValue]
    public let known: Known

    public init(fields: [String: JSONValue], known: Known) {
        self.fields = fields
        self.known = known
    }
}

extension JSONDocument: Decodable where Known: Decodable {
    public init(from decoder: any Decoder) throws {
        fields = try decoder.singleValueContainer().decode([String: JSONValue].self)
        known = try Known(from: decoder)
    }
}

extension JSONDocument: Encodable where Known: Encodable {
    public func encode(to encoder: any Encoder) throws {
        try fields.encode(to: encoder)
        try known.encode(to: encoder)
    }
}

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

public struct Discriminator: CodingKey {
    private static let key: CodingUserInfoKey? = CodingUserInfoKey(rawValue: "Discriminator")

    public let stringValue: String
    public var intValue: Int? { nil }
    public var userInfo: [CodingUserInfoKey: any Sendable] { Self.key.map { key in [key: self] } ?? [:] }

    public init(stringValue: String) { self.stringValue = stringValue }
    public init?(intValue _: Int) { nil }

    public init(_ userInfo: [CodingUserInfoKey: Any]) throws(CocoaError) {
        guard let carried: Self = Self.key.flatMap({ key in userInfo[key] }) as? Self else { throw CocoaError(.coderValueNotFound) }
        self = carried
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

extension Encoder {
    public func encode<Key: CodingKey>(case name: some Encodable, keyedBy _: Key.Type, _ fill: (inout KeyedEncodingContainer<Key>) throws -> Void = { _ in }) throws {
        try [Discriminator(userInfo).stringValue: name].encode(to: self)
        var fields: KeyedEncodingContainer<Key> = container(keyedBy: Key.self)
        try fill(&fields)
    }
}
