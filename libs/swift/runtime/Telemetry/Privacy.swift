public import Logging

// --- [TYPES] ---------------------------------------------------------------------------

public enum Privacy: Int64, Logger.MetadataValueAttributes.Attribute {
    case `private`
    case `public`

    public static func of(_ value: Logger.MetadataValue) -> Self? {
        switch value {
            case .stringConvertible(is Bool), .stringConvertible(is any BinaryInteger), .stringConvertible(is any BinaryFloatingPoint): .public
            case .string, .stringConvertible, .dictionary, .array: value.attributes[Self.self]
        }
    }
}

extension Logger.MetadataValue.StringInterpolation {
    public mutating func appendInterpolation(_ value: some Sendable, privacy: Privacy) {
        appendInterpolation(value, attributes: [privacy])
    }
}
