// --- [MODELS] --------------------------------------------------------------------------

public enum Lenient<Known: RawRepresentable<String> & Sendable>: RawRepresentable, Codable, Sendable {
    case known(Known)
    case unknown(String)

    public init(rawValue: String) { self = Known(rawValue: rawValue).map(Self.known) ?? .unknown(rawValue) }

    public var rawValue: String {
        switch self {
            case .known(let value): value.rawValue
            case .unknown(let text): text
        }
    }
}
