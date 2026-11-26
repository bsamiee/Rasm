import Foundation
import Results

// --- [TYPES] ---------------------------------------------------------------------------

enum Key: String, CodingKey {
    case tag = "_tag"
    case success, failure, error, defect, name, message, cause, code, data
}

// --- [MODELS] --------------------------------------------------------------------------

public struct EncodedResult<Success: Encodable & Sendable>: Encodable, Sendable {
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
        switch error as? any Encodable & Error ?? bridged.underlyingErrors.first as? any Encodable & Error {
            case .some(let tagged):
                try fields.encode(String(describing: type(of: tagged)), forKey: .tag)
                try fields.encode(tagged.localizedDescription, forKey: .message)
                try tagged.encode(to: encoder)
            case .none:
                try fields.encode(bridged.domain, forKey: .name)
                try fields.encode(bridged.localizedDescription, forKey: .message)
                try fields.encodeIfPresent(bridged.underlyingErrors.first.map(Self.init(error:)) as Self?, forKey: .cause)
        }
    }
}

enum Reason: Encodable, Sendable {
    case fail(any Error)
    case die(any Error)

    func encode(to encoder: any Encoder) throws {
        let (tag, key, error): (String, Key, any Error) =
            switch self {
                case .fail(let error): ("Fail", .error, error)
                case .die(let error): ("Die", .defect, error)
            }
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(tag, forKey: .tag)
        try fields.encode(EncodedError(error: error), forKey: key)
    }
}

struct Cause: Encodable, Sendable {
    let reasons: AggregateError<Reason>

    func encode(to encoder: any Encoder) throws {
        let first: any Error =
            switch reasons.first {
                case .fail(let error), .die(let error): error
            }
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode("Cause", forKey: .tag)
        try fields.encode(0, forKey: .code)
        try fields.encode(first.localizedDescription, forKey: .message)
        try fields.encode(reasons.errors, forKey: .data)
    }
}

extension AggregateError where Element == Reason {
    init(failing error: any Error) { self = Self(((error as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [any Error] ?? []).map(Reason.fail)) ?? Self(first: .fail(error)) }
}
