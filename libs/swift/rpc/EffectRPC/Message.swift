import Encoding
import Foundation
import Results

// --- [TYPES] ---------------------------------------------------------------------------

private enum Key: String, CodingKey {
    case tag = "_tag"
    case id, method, params, requestId, result, chunk, error, defect, data, success, failure, name, message, cause
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Outcome<Success: Encodable & Sendable>: Encodable, Sendable {
    let result: Result<Success, any Error>

    public init(_ result: Result<Success, some Error>) {
        self.result = result.mapError { error -> any Error in error }
    }

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        let (tag, key, value): (String, Key, any Encodable) =
            switch result {
                case .success(let value): ("Success", .success, value)
                case .failure(let error): ("Failure", .failure, Encoded(error: error))
            }
        try fields.encode(tag, forKey: .tag)
        try fields.encode(value, forKey: key)
    }
}

struct Encoded: Encodable {
    let error: any Error

    func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        switch error as? any TaggedError ?? (error as NSError).underlyingErrors.first as? any TaggedError {
            case .some(let tagged):
                try fields.encode(String(describing: type(of: tagged)), forKey: .tag)
                try tagged.encode(to: encoder)
            case .none:
                let mirror: Mirror = Mirror(reflecting: error)
                let (name, message): (String, String) =
                    mirror.displayStyle == .enum
                    ? (mirror.children.first?.label ?? String(describing: error), (error as? any LocalizedError)?.errorDescription ?? String(describing: error))
                    : ((error as NSError).domain, error.localizedDescription)
                try fields.encode(name, forKey: .name)
                try fields.encode(message, forKey: .message)
                let cause: Self? = (error as NSError).underlyingErrors.first.map(Self.init)
                try fields.encodeIfPresent(cause, forKey: .cause)
        }
    }
}

private struct Call<Method: CodingKey>: Decoder, KeyedDecodingContainerProtocol {
    let codingPath: [any CodingKey]
    let method: String?
    let params: any Decoder?

    var userInfo: [CodingUserInfoKey: Any] { [:] }
    var allKeys: [Method] { [method.flatMap(Method.init(stringValue:))].compactMap(\.self) }

    func container<Nested: CodingKey>(keyedBy _: Nested.Type) -> KeyedDecodingContainer<Nested> {
        KeyedDecodingContainer(Call<Nested>(codingPath: codingPath, method: method, params: params))
    }

    func unkeyedContainer() throws -> any UnkeyedDecodingContainer {
        throw DecodingError.typeMismatch([JSONValue].self, DecodingError.Context(codingPath: codingPath, debugDescription: "Request is an object"))
    }

    func singleValueContainer() throws -> any SingleValueDecodingContainer {
        throw DecodingError.typeMismatch(JSONValue.self, DecodingError.Context(codingPath: codingPath, debugDescription: "Request is an object"))
    }

    func contains(_ key: Method) -> Bool { key.stringValue == method }

    func decodeNil(forKey key: Method) throws -> Bool {
        _ = try superDecoder(forKey: key)
        return try params?.singleValueContainer().decodeNil() ?? false
    }

    func decode<Value: Decodable>(_: Value.Type, forKey key: Method) throws -> Value { try Value(from: superDecoder(forKey: key)) }

    func nestedContainer<Nested: CodingKey>(keyedBy _: Nested.Type, forKey key: Method) throws -> KeyedDecodingContainer<Nested> {
        try superDecoder(forKey: key).container(keyedBy: Nested.self)
    }

    func nestedUnkeyedContainer(forKey key: Method) throws -> any UnkeyedDecodingContainer { try superDecoder(forKey: key).unkeyedContainer() }

    func superDecoder() throws -> any Decoder {
        throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: codingPath, debugDescription: "Request holds no super key"))
    }

    func superDecoder(forKey key: Method) throws -> any Decoder {
        guard contains(key) else { throw DecodingError.keyNotFound(key, DecodingError.Context(codingPath: codingPath, debugDescription: "Request holds \(method ?? "no params")")) }
        return params ?? Self(codingPath: codingPath + [key], method: nil, params: nil)
    }
}

enum FromClient<Request: Decodable & Sendable>: Decodable {
    private enum Control: String {
        case interrupt = "@effect/rpc/Interrupt"
        case ack = "@effect/rpc/Ack"
    }

    case request(id: JSONValue, Result<Request, any Error>)
    case interrupt(requestId: JSONValue)
    case ack(requestId: JSONValue)

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let method: String = try container.decode(String.self, forKey: .method)
        let params: any Decoder? = try container.contains(.params) && !container.decodeNil(forKey: .params) ? container.superDecoder(forKey: .params) : nil
        let requested: () throws -> JSONValue = { try container.nestedContainer(keyedBy: Key.self, forKey: .params).decode(JSONValue.self, forKey: .requestId) }
        self =
            switch Control(rawValue: method) {
                case .interrupt: try .interrupt(requestId: requested())
                case .ack: try .ack(requestId: requested())
                case .none:
                    try .request(
                        id: container.decode(JSONValue.self, forKey: .id),
                        Result { try Request(from: Call<Key>(codingPath: [], method: method, params: params)) },
                    )
            }
    }
}

enum FromServer: Encodable {
    enum Reason: Encodable {
        case fail(any Error)
        case die(any Error)

        func encode(to encoder: any Encoder) throws {
            var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            let (tag, key, error): (String, Key, any Error) =
                switch self {
                    case .fail(let error): ("Fail", .error, error)
                    case .die(let error): ("Die", .defect, error)
                }
            try fields.encode(tag, forKey: .tag)
            try fields.encode(Encoded(error: error), forKey: key)
        }
    }

    case exit(requestId: JSONValue, Result<any Encodable?, AggregateError<Reason>>)
    case chunk(requestId: JSONValue, any Encodable)

    var requestId: JSONValue {
        switch self {
            case .exit(let id, _), .chunk(let id, _): id
        }
    }

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode(requestId, forKey: .id)
        switch self {
            case .exit(_, .success(.some(let value))): try container.encode(value, forKey: .result)
            case .exit(_, .success(.none)): try container.encodeNil(forKey: .result)
            case .exit(_, .failure(let cause)):
                var error: KeyedEncodingContainer<Key> = container.nestedContainer(keyedBy: Key.self, forKey: .error)
                try error.encode("Cause", forKey: .tag)
                try error.encode(cause.errors, forKey: .data)
            case .chunk(_, let value):
                try container.encode(true, forKey: .chunk)
                var values: any UnkeyedEncodingContainer = container.nestedUnkeyedContainer(forKey: .result)
                try values.encode(value)
        }
    }

    func line(_ encoder: JSONEncoder) -> Result<Data, any Error> {
        Result { try encoder.encodeLine(self) }
            .flatMapError { error in Result { try encoder.encodeLine(Self.exit(requestId: requestId, .failure(AggregateError(first: .die(error))))) } }
    }
}
