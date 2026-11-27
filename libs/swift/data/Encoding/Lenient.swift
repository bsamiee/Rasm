// --- [MODELS] --------------------------------------------------------------------------

package enum Lenient<Known: RawRepresentable & Sendable>: RawRepresentable, Codable, Sendable where Known.RawValue: Codable & Sendable {
    case known(Known)
    case unknown(Known.RawValue)

    package init(rawValue: Known.RawValue) { self = Known(rawValue: rawValue).map(Self.known) ?? .unknown(rawValue) }

    package init(from decoder: any Decoder) throws { self.init(rawValue: try decoder.singleValueContainer().decode(Known.RawValue.self)) }

    package var rawValue: Known.RawValue {
        switch self {
            case .known(let value): value.rawValue
            case .unknown(let raw): raw
        }
    }

    package func encode(to encoder: any Encoder) throws {
        var container = encoder.singleValueContainer()
        try container.encode(rawValue)
    }
}

extension Lenient: Equatable where Known: Equatable, Known.RawValue: Equatable {}

extension Lenient: Hashable where Known: Hashable, Known.RawValue: Hashable {}
