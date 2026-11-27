package import Instrumentation

// --- [MODELS] --------------------------------------------------------------------------

package struct EnvironmentCarrier: Extractor, Injector {
    package init() {}

    private func variable(_ key: String) -> String {
        let name = key.map { character in character.isASCII && (character.isLetter || character.isNumber) ? character.uppercased() : "_" }.joined()
        return name.first?.isNumber ?? true ? "_\(name)" : name
    }

    package func extract(key: String, from carrier: [String: String]) -> String? { carrier[variable(key)] }

    package func inject(_ value: String, forKey key: String, into carrier: inout [String: String]) { carrier[variable(key)] = value }
}
