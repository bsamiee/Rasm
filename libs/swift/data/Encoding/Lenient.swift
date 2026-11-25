// --- [MODELS] --------------------------------------------------------------------------

public enum Lenient<Known: RawRepresentable & Sendable>: RawRepresentable, Codable, Sendable where Known.RawValue: Codable & Sendable {
    case known(Known)
    case unknown(Known.RawValue)

    public init(rawValue: Known.RawValue) { self = Known(rawValue: rawValue).map(Self.known) ?? .unknown(rawValue) }

    public init(from decoder: any Decoder) throws { self.init(rawValue: try decoder.singleValueContainer().decode(Known.RawValue.self)) }

    public var rawValue: Known.RawValue {
        switch self {
            case .known(let value): value.rawValue
            case .unknown(let raw): raw
        }
    }

    public func encode(to encoder: any Encoder) throws {
        var container = encoder.singleValueContainer()
        try container.encode(rawValue)
    }
}

extension Lenient: Equatable where Known: Equatable, Known.RawValue: Equatable {}
