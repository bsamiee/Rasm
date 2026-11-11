import Foundation

// --- [MODELS] --------------------------------------------------------------------------

struct JSONValueDecoder: Decoder, SingleValueDecodingContainer {
    struct Keyed<Key: CodingKey>: KeyedDecodingContainerProtocol {
        let fields: [String: JSONValue]
        let codingPath: [any CodingKey]

        var allKeys: [Key] { fields.keys.compactMap(Key.init(stringValue:)) }

        func contains(_ key: Key) -> Bool { fields[key.stringValue] != nil }

        func decodeNil(forKey key: Key) throws(DecodingError) -> Bool { try child(key).decodeNil() }

        func decode<Value: Decodable>(_: Value.Type, forKey key: Key) throws -> Value { try child(key).decode(Value.self) }

        func nestedContainer<Nested: CodingKey>(keyedBy _: Nested.Type, forKey key: Key) throws(DecodingError) -> KeyedDecodingContainer<Nested> {
            try child(key).container(keyedBy: Nested.self)
        }

        func nestedUnkeyedContainer(forKey key: Key) throws(DecodingError) -> any UnkeyedDecodingContainer { try child(key).unkeyedContainer() }

        func superDecoder() -> any Decoder { JSONValueDecoder(value: fields["super"] ?? .null, codingPath: codingPath + ["super".codingKey]) }

        func superDecoder(forKey key: Key) -> any Decoder { JSONValueDecoder(value: fields[key.stringValue] ?? .null, codingPath: codingPath + [key]) }

        private func child(_ key: Key) throws(DecodingError) -> JSONValueDecoder {
            guard let value = fields[key.stringValue] else {
                throw DecodingError.keyNotFound(key, DecodingError.Context(codingPath: codingPath, debugDescription: "No value for key \(key.stringValue)"))
            }
            return JSONValueDecoder(value: value, codingPath: codingPath + [key])
        }
    }

    struct Unkeyed: UnkeyedDecodingContainer {
        let codingPath: [any CodingKey]
        let count: Int?
        private let elements: [JSONValue]
        private(set) var currentIndex: Int = 0

        init(_ elements: [JSONValue], codingPath: [any CodingKey]) { (self.elements, self.codingPath, count) = (elements, codingPath, elements.count) }

        var isAtEnd: Bool { currentIndex == count }

        mutating func decodeNil() throws(DecodingError) -> Bool {
            let null = try element().decodeNil()
            currentIndex += null ? 1 : 0
            return null
        }

        mutating func decode<Value: Decodable>(_: Value.Type) throws -> Value { try next { element in try element.decode(Value.self) } }

        mutating func nestedContainer<Nested: CodingKey>(keyedBy _: Nested.Type) throws -> KeyedDecodingContainer<Nested> {
            try next { element in try element.container(keyedBy: Nested.self) }
        }

        mutating func nestedUnkeyedContainer() throws -> any UnkeyedDecodingContainer { try next { element in try element.unkeyedContainer() } }

        mutating func superDecoder() throws -> any Decoder { try next { element in element } }

        private func element() throws(DecodingError) -> JSONValueDecoder {
            let path: [any CodingKey] = codingPath + [currentIndex.codingKey]
            guard !isAtEnd else { throw DecodingError.valueNotFound(JSONValue.self, DecodingError.Context(codingPath: path, debugDescription: "Unkeyed container is at end")) }
            return JSONValueDecoder(value: elements[currentIndex], codingPath: path)
        }

        private mutating func next<Value>(_ read: (JSONValueDecoder) throws -> Value) throws -> Value {
            let value = try read(element())
            currentIndex += 1
            return value
        }
    }

    let value: JSONValue
    let codingPath: [any CodingKey]

    var userInfo: [CodingUserInfoKey: Any] { [:] }

    func container<Key: CodingKey>(keyedBy _: Key.Type) throws(DecodingError) -> KeyedDecodingContainer<Key> {
        guard case .object(let fields) = value else { throw mismatch([String: JSONValue].self) }
        return KeyedDecodingContainer(Keyed(fields: fields, codingPath: codingPath))
    }

    func unkeyedContainer() throws(DecodingError) -> any UnkeyedDecodingContainer {
        guard case .array(let elements) = value else { throw mismatch([JSONValue].self) }
        return Unkeyed(elements, codingPath: codingPath)
    }

    func singleValueContainer() -> any SingleValueDecodingContainer { self }

    func decodeNil() -> Bool { value == .null }

    func decode<Decoded: Decodable>(_: Decoded.Type) throws -> Decoded {
        let read: Any? =
            switch (value, Decoded.self) {
                case (_, is JSONValue.Type): value
                case (.bool(let flag), is Bool.Type): flag
                case (.string(let text), is String.Type): text
                case (.string(let text), is URL.Type): URL(string: text)
                case (.string(let text), is Data.Type): Data(base64Encoded: text)
                case (.number(let number), let floating as any BinaryFloatingPoint.Type): floating.init(number)
                case (.number(let number), let integer as any BinaryInteger.Type): integer.init(exactly: number)
                case (_, is Bool.Type), (_, is String.Type), (_, is URL.Type), (_, is Data.Type), (_, is any BinaryFloatingPoint.Type), (_, is any BinaryInteger.Type): nil
                default: try Decoded(from: self)
            }
        guard let decoded = read as? Decoded else { throw mismatch(Decoded.self) }
        return decoded
    }

    private func mismatch(_ type: Any.Type) -> DecodingError {
        switch value {
            case .null: DecodingError.valueNotFound(type, DecodingError.Context(codingPath: codingPath, debugDescription: "Expected \(type) value but found null"))
            case _: DecodingError.typeMismatch(type, DecodingError.Context(codingPath: codingPath, debugDescription: "Expected to decode \(type)"))
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension JSONValue {
    public func decode<Value: Decodable>(_: Value.Type = Value.self) throws -> Value { try JSONValueDecoder(value: self, codingPath: []).decode(Value.self) }
}
