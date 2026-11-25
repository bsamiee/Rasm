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

enum Key: String, CodingKey {
    case tag = "_tag"
    case success, failure, error, defect, name, message, cause, code, data
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Outcome<Success: Encodable & Sendable>: Encodable, Sendable {
    let result: Result<Success, any Error>

    public init(_ result: Result<Success, some Error>) { self.result = result.mapError { error -> any Error in error } }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        switch result {
            case .success(let value):
                try fields.encode("Success", forKey: .tag)
                try fields.encode(value, forKey: .success)
            case .failure(let error):
                try fields.encode("Failure", forKey: .tag)
                try fields.encode(EncodedError(error: error), forKey: .failure)
        }
    }
}

struct EncodedError: Encodable, Sendable {
    let error: any Error

    func encode(to encoder: any Encoder) throws {
        let bridged = error as NSError
        var fields = encoder.container(keyedBy: Key.self)
        switch error as? any TaggedError ?? bridged.underlyingErrors.first as? any TaggedError {
            case .some(let tagged):
                try fields.encode(type(of: tagged).tag, forKey: .tag)
                try tagged.encode(to: encoder)
            case .none:
                try fields.encode(bridged.domain, forKey: .name)
                try fields.encode(bridged.localizedDescription, forKey: .message)
                try fields.encodeIfPresent(bridged.underlyingErrors.first.map(Self.init(error:)) as Self?, forKey: .cause)
        }
    }
}
