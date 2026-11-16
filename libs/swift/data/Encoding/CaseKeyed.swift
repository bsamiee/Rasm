// --- [TYPES] ---------------------------------------------------------------------------

public protocol CaseKeyed: Encodable {}

public protocol CaseLabeled: Encodable {}

// --- [MODELS] --------------------------------------------------------------------------

private struct CaseKey: CodingKey {
    let stringValue: String
    let intValue: Int? = nil

    init(stringValue: String) { self.stringValue = stringValue }

    init?(intValue _: Int) { nil }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CaseKeyed {
    public func encode(to encoder: any Encoder) throws {
        guard let held = Mirror(reflecting: self).children.first, let label = held.label, let value = held.value as? any Encodable else {
            throw EncodingError.invalidValue(self, EncodingError.Context(codingPath: encoder.codingPath, debugDescription: "\(Self.self) case \(self) holds no Encodable value"))
        }
        var fields = encoder.container(keyedBy: CaseKey.self)
        try fields.encode(value, forKey: CaseKey(stringValue: label))
    }
}

extension CaseLabeled {
    public var caseLabel: String { Mirror(reflecting: self).children.first?.label ?? String(describing: self) }

    public func encode(to encoder: any Encoder) throws {
        var label = encoder.singleValueContainer()
        try label.encode(caseLabel)
    }
}
