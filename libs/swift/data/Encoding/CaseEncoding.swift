// --- [TYPES] ---------------------------------------------------------------------------

public protocol CaseKeyed: Encodable {}

public protocol CaseLabeled: Encodable {}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CaseKeyed {
    public func encode(to encoder: any Encoder) throws {
        guard let held = Mirror(reflecting: self).children.first, let label = held.label, let value = held.value as? any Encodable else {
            throw EncodingError.invalidValue(self, EncodingError.Context(codingPath: encoder.codingPath, debugDescription: "\(Self.self) case \(self) holds no Encodable value"))
        }
        func encodeKeyed(_ value: some Encodable) throws {
            var fields = encoder.singleValueContainer()
            try fields.encode([label: value])
        }
        try encodeKeyed(value)
    }
}

extension CaseLabeled {
    public var caseLabel: String { Mirror(reflecting: self).children.first?.label ?? String(describing: self) }

    public func encode(to encoder: any Encoder) throws { try caseLabel.encode(to: encoder) }
}
