// --- [TYPES] ---------------------------------------------------------------------------

package protocol CaseTagged: Encodable {}

package protocol CaseLabeled: Encodable {}

// --- [MODELS] --------------------------------------------------------------------------

private struct Field: CodingKey {
    static let tag = Self(stringValue: "_tag")

    let stringValue: String
    let intValue: Int? = nil

    init(stringValue: String) { self.stringValue = stringValue }

    init?(intValue _: Int) { nil }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CaseTagged {
    package func encode(to encoder: any Encoder) throws {
        let held: Mirror.Child? = Mirror(reflecting: self).children.first
        let payload: Mirror? = held.map { held in Mirror(reflecting: held.value) }
        let refused = { (value: Any) in
            EncodingError.invalidValue(value, EncodingError.Context(codingPath: encoder.codingPath, debugDescription: "\(Self.self) case \(self) holds no Encodable value"))
        }
        var fields = encoder.container(keyedBy: Field.self)
        try fields.encode(held?.label ?? String(describing: self), forKey: .tag)
        switch (held?.value, payload) {
            case (.none, _): return
            case (.some, .some(let parts)) where parts.displayStyle == .tuple:
                for child in parts.children {
                    let wrapped = Mirror(reflecting: child.value)
                    let present: Any? = wrapped.displayStyle == .optional ? wrapped.children.first?.value : child.value
                    guard let present else { continue }
                    guard let value = present as? any Encodable else { throw refused(present) }
                    try fields.encode(value, forKey: Field(stringValue: child.label ?? ""))
                }
            case (.some(let value as any Encodable), _): try value.encode(to: encoder)
            case (.some(let value), _): throw refused(value)
        }
    }
}

extension CaseLabeled {
    var caseLabel: String { Mirror(reflecting: self).children.first?.label ?? String(describing: self) }

    package func encode(to encoder: any Encoder) throws { try caseLabel.encode(to: encoder) }
}
