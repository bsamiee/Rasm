import Foundation
public import Results

// --- [TYPES] ---------------------------------------------------------------------------

public protocol TaggedError: Error, Encodable {
    static var tag: String { get }
}

extension TaggedError {
    public static var tag: String { String(describing: Self.self) }
}

extension Unmatched: TaggedError {}

// --- [MODELS] --------------------------------------------------------------------------

struct Tagged<Value: Encodable & Sendable>: Encodable, Sendable {
    private enum Key: String, CodingKey {
        case tag = "_tag"
    }

    let tag: String
    let value: Value

    func encode(to encoder: any Encoder) throws {
        var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try container.encode(tag, forKey: .tag)
        try value.encode(to: encoder)
    }
}

public struct Outcome<Success: Encodable & Sendable>: Encodable, Sendable {
    let result: Result<Success, any Error>

    public init(_ result: Result<Success, some Error>) { self.result = result.mapError { error -> any Error in error } }

    public func encode(to encoder: any Encoder) throws {
        switch result {
            case .success(let value): try Tagged(tag: "Success", value: ["success": value]).encode(to: encoder)
            case .failure(let error): try Tagged(tag: "Failure", value: ["failure": EncodedError(error: error)]).encode(to: encoder)
        }
    }
}

struct EncodedError: Encodable, Sendable {
    private struct Instance: Encodable {
        let name: String
        let message: String
        let cause: EncodedError?
    }

    let error: any Error

    func encode(to encoder: any Encoder) throws {
        func encode(_ tagged: some TaggedError) throws { try Tagged(tag: type(of: tagged).tag, value: tagged).encode(to: encoder) }
        let bridged: NSError = error as NSError
        switch error as? any TaggedError ?? bridged.underlyingErrors.first as? any TaggedError {
            case .some(let tagged): try encode(tagged)
            case .none: try Instance(name: bridged.domain, message: bridged.localizedDescription, cause: bridged.underlyingErrors.first.map(Self.init)).encode(to: encoder)
        }
    }
}
