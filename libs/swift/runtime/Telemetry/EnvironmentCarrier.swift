public import Instrumentation

// --- [MODELS] --------------------------------------------------------------------------

public struct EnvironmentCarrier: Extractor, Injector {
    public init() {}

    private func variable(_ key: String) -> String {
        let name = String(key.uppercased().map { character in character.isASCII && (character.isLetter || character.isNumber) ? character : "_" })
        return name.first?.isNumber ?? true ? "_\(name)" : name
    }

    public func extract(key: String, from carrier: [String: String]) -> String? { carrier[variable(key)] }

    public func inject(_ value: String, forKey key: String, into carrier: inout [String: String]) { carrier[variable(key)] = value }
}
