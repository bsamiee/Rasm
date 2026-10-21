import Encoding
import Foundation
import Results

// --- [MODELS] --------------------------------------------------------------------------

public struct Outcome<Success: Encodable & Sendable>: Encodable, Sendable {
    private enum Key: CodingKey { case success, failure }

    let result: Result<Success, any Error>

    public init(_ result: Result<Success, some Error>) {
        self.result = result.mapError { error -> any Error in error }
    }

    public func encode(to encoder: any Encoder) throws {
        switch result {
            case .success(let value): try encoder.encode(case: "Success", keyedBy: Key.self) { fields in try fields.encode(value, forKey: .success) }
            case .failure(let error): try encoder.encode(case: "Failure", keyedBy: Key.self) { fields in try fields.encode(Encoded(error: error), forKey: .failure) }
        }
    }
}

struct Encoded: Encodable {
    private enum Key: CodingKey { case name, message, cause }

    let error: any Error

    func encode(to encoder: any Encoder) throws {
        switch error as? any TaggedError ?? (error as NSError).underlyingErrors.first as? any TaggedError {
            case .some(let tagged):
                try encoder.encode(case: String(describing: type(of: tagged)), keyedBy: Key.self)
                try tagged.encode(to: encoder)
            case .none:
                var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
                let mirror: Mirror = Mirror(reflecting: error)
                let (name, message): (String, String) =
                    mirror.displayStyle == .enum
                    ? (mirror.children.first?.label ?? String(describing: error), (error as? any LocalizedError)?.errorDescription ?? String(describing: error))
                    : ((error as NSError).domain, error.localizedDescription)
                try fields.encode(name, forKey: .name)
                try fields.encode(message, forKey: .message)
                try fields.encodeIfPresent((error as NSError).underlyingErrors.first.map(Self.init), forKey: .cause)
        }
    }
}

enum FromClient<Request: Decodable & Sendable>: Decodable {
    private enum Control: String {
        case interrupt = "@effect/rpc/Interrupt"
        case ack = "@effect/rpc/Ack"
    }

    private enum Key: CodingKey { case id, method, params, requestId }

    case request(id: Int, Result<Request, any Error>)
    case interrupt(requestId: Int)
    case ack

    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        self =
            switch Control(rawValue: try container.decode(String.self, forKey: .method)) {
                case .interrupt: try .interrupt(requestId: container.nestedContainer(keyedBy: Key.self, forKey: .params).decode(Int.self, forKey: .requestId))
                case .ack: .ack
                case .none: try .request(id: container.decode(Int.self, forKey: .id), Result { try Request(from: decoder) })
            }
    }
}

enum FromServer: Encodable {
    enum Reason: Encodable {
        private enum Key: CodingKey { case error, defect }

        case fail(any Error)
        case die(any Error)

        func encode(to encoder: any Encoder) throws {
            switch self {
                case .fail(let error): try encoder.encode(case: "Fail", keyedBy: Key.self) { fields in try fields.encode(Encoded(error: error), forKey: .error) }
                case .die(let error): try encoder.encode(case: "Die", keyedBy: Key.self) { fields in try fields.encode(Encoded(error: error), forKey: .defect) }
            }
        }
    }

    private enum Key: CodingKey { case id, result, chunk, error, data }

    case exit(requestId: Int, Result<any Encodable?, AggregateError<Reason>>)
    case chunk(requestId: Int, any Encodable)

    var requestId: Int {
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
                try container.superEncoder(forKey: .error).encode(case: "Cause", keyedBy: Key.self) { fields in try fields.encode(cause.errors, forKey: .data) }
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
