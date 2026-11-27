import Foundation
import Results

// --- [TYPES] ---------------------------------------------------------------------------

private enum CodingKeys: String, CodingKey {
    case tag = "_tag"
    case error, defect, fiberId, code, message, data
}

// --- [MODELS] --------------------------------------------------------------------------

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
