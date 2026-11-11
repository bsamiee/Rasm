public import Instrumentation

// --- [MODELS] --------------------------------------------------------------------------

public struct EnvironmentCarrier: Extractor, Injector {
    public init() {}

    private func variable(_ key: String) -> String {
        let name = String(
            String.UnicodeScalarView(
                key.unicodeScalars.map { scalar -> Unicode.Scalar in
                    switch scalar {
                        case "a"..."z": Unicode.Scalar(UInt8(scalar.value) - 32)
                        case "A"..."Z", "0"..."9", "_": scalar
                        default: "_"
                    }
                }
            )
        )
        return switch name.first {
            case nil: "_"
            case let first? where ("0"..."9").contains(first): "_\(name)"
            case _?: name
        }
    }

    public func extract(key: String, from carrier: [String: String]) -> String? { carrier[variable(key)] }

    public func inject(_ value: String, forKey key: String, into carrier: inout [String: String]) { carrier[variable(key)] = value }
}
