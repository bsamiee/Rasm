package import Logging

// --- [TYPES] ---------------------------------------------------------------------------

package enum Privacy: Int64, Logger.MetadataValueAttributes.Attribute {
    case `private`
    case `public`

    static func of(_ value: Logger.MetadataValue) -> Self {
        switch value {
            case .stringConvertible(is Bool), .stringConvertible(is any BinaryInteger), .stringConvertible(is any BinaryFloatingPoint): .public
            case .string, .stringConvertible, .dictionary, .array: value.attributes[Self.self] ?? .private
        }
    }
}

extension Logger.MetadataValue.StringInterpolation {
    package mutating func appendInterpolation(_ value: some Any, privacy: Privacy) {
        appendInterpolation(value) { attributes in attributes[Privacy.self] = privacy }
    }
}
