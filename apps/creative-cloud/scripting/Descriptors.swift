import Foundation

// --- [MODELS] --------------------------------------------------------------------------

enum Descriptor: Codable, Sendable {
    case null
    case boolean(Bool)
    case number(Double)
    case text(String)
    case list([Self])
    case record([String: Self])

    init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        self =
            try container.decodeNil()
            ? .null
            : Result { try .boolean(container.decode(Bool.self)) }
                .flatMapError { _ in Result { try .number(container.decode(Double.self)) } }
                .flatMapError { _ in Result { try .text(container.decode(String.self)) } }
                .flatMapError { _ in Result { try .list(container.decode([Self].self)) } }
                .flatMapError { _ in Result { try .record(container.decode([String: Self].self)) } }
                .get()
    }

    func encode(to encoder: any Encoder) throws {
        var container: any SingleValueEncodingContainer = encoder.singleValueContainer()
        switch self {
            case .null: try container.encodeNil()
            case .boolean(let value): try container.encode(value)
            case .number(let value): try container.encode(value)
            case .text(let value): try container.encode(value)
            case .list(let value): try container.encode(value)
            case .record(let value): try container.encode(value)
        }
    }
}
