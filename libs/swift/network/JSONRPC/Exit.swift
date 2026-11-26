import Foundation
import Results

// --- [TYPES] ---------------------------------------------------------------------------

private enum CodingKeys: String, CodingKey {
    case tag = "_tag"
    case success, failure, error, defect, fiberId, name, message, cause, code, data
}

// --- [MODELS] --------------------------------------------------------------------------

public struct EncodedResult<Success: Encodable & Sendable>: Encodable, Sendable {
    let result: Result<Success, any Error>

    public init(_ result: Result<Success, some Error>) { self.result = result.mapError { error -> any Error in error } }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
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

private struct EncodedError: Encodable {
    let error: any Error

    func encode(to encoder: any Encoder) throws {
        let chain = Array(sequence(first: error as NSError) { bridged in bridged.underlyingErrors.first as NSError? })
        var fields = encoder.container(keyedBy: CodingKeys.self)
        switch chain.lazy.compactMap({ bridged in bridged as? Unmatched }).first {
            case .some(let miss):
                try fields.encode(String(describing: Unmatched.self), forKey: .tag)
                try fields.encode(error.localizedDescription, forKey: .message)
                try miss.encode(to: encoder)
            case .none:
                if let typed = error as? any Encodable {
                    try fields.encode(String(describing: type(of: error)), forKey: .tag)
                    try typed.encode(to: encoder)
                }
                for (depth, bridged) in chain.enumerated() {
                    if depth > 0 { fields = fields.nestedContainer(keyedBy: CodingKeys.self, forKey: .cause) }
                    try fields.encode(bridged.domain, forKey: .name)
                    try fields.encode(bridged.localizedDescription, forKey: .message)
                }
        }
    }
}

enum Reason: Encodable, Sendable {
    case fail(any Error)
    case die(any Error)
    case interrupt(CancellationError)

    func encode(to encoder: any Encoder) throws {
        let (tag, key, error): (String, CodingKeys, (any Error)?) =
            switch self {
                case .fail(let error): ("Fail", .error, error)
                case .die(let error): ("Die", .defect, error)
                case .interrupt: ("Interrupt", .fiberId, nil)
            }
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(tag, forKey: .tag)
        try fields.encode(error.map(EncodedError.init(error:)), forKey: key)
    }
}

struct Cause: Encodable, Sendable {
    let reasons: AggregateError<Reason>

    func encode(to encoder: any Encoder) throws {
        let first: any Error =
            switch reasons.first {
                case .fail(let error), .die(let error), .interrupt(let error as any Error): error
            }
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode("Cause", forKey: .tag)
        try fields.encode(0, forKey: .code)
        try fields.encode(first.localizedDescription, forKey: .message)
        try fields.encode(reasons.errors, forKey: .data)
    }
}

extension AggregateError where Element == Reason {
    init(failing error: any Error) {
        let errors = (error as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [any Error] ?? [error]
        self = Self(errors.map { error in (error as? CancellationError).map(Reason.interrupt) ?? .fail(error) }) ?? Self(first: .fail(error))
    }
}
