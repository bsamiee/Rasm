import Foundation

// --- [TYPES] ---------------------------------------------------------------------------

private enum CodingKeys: String, CodingKey {
    case tag = "_tag"
    case success, failure, name, message, cause
}

// --- [MODELS] --------------------------------------------------------------------------

package struct EncodedResult<Success: Encodable & Sendable>: Encodable, Sendable {
    let result: Result<Success, any Error>

    package init(_ result: Result<Success, some Error>) { self.result = result.mapError { error -> any Error in error } }

    package func encode(to encoder: any Encoder) throws {
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

package struct EncodedError: Encodable {
    let error: any Error

    package init(error: any Error) { self.error = error }

    package func encode(to encoder: any Encoder) throws {
        let chain = Array(sequence(first: error as NSError) { bridged in bridged.underlyingErrors.first as NSError? })
        let miss: Unmatched? = chain.lazy.compactMap { bridged in bridged as? Unmatched }.first
        var fields = encoder.container(keyedBy: CodingKeys.self)
        switch (miss, error) {
            case (let miss?, _):
                try fields.encode(String(describing: Unmatched.self), forKey: .tag)
                try fields.encode(error.localizedDescription, forKey: .message)
                try miss.encode(to: encoder)
            case (nil, let failure as any ModuleError):
                try fields.encode(String(describing: type(of: error)), forKey: .tag)
                try failure.encode(to: encoder)
            case (nil, _):
                for (depth, bridged) in chain.enumerated() {
                    if depth > 0 { fields = fields.nestedContainer(keyedBy: CodingKeys.self, forKey: .cause) }
                    try fields.encode(bridged.domain, forKey: .name)
                    try fields.encode(bridged.localizedDescription, forKey: .message)
                }
        }
    }
}
