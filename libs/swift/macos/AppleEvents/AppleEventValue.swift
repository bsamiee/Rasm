import CoreServices
import Encoding
public import Foundation

// --- [TYPES] ---------------------------------------------------------------------------

public enum KeyForm: String, CaseIterable, Codable, Sendable {
    case property
    case index
    case name
    case id
    case relative
    case range
    case test
    case whose

    var code: DescType {
        switch self {
            case .property: DescType(formPropertyID)
            case .index: DescType(formAbsolutePosition)
            case .name: DescType(formName)
            case .id: DescType(formUniqueID)
            case .relative: DescType(formRelativePosition)
            case .range: DescType(formRange)
            case .test: DescType(formTest)
            case .whose: DescType(formWhose)
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

public indirect enum AppleEventValue: Sendable {
    case null
    case bool(Bool)
    case number(Double)
    case text(String)
    case list([Self])
    case record(type: DescType, fields: [String: Self])
    case type(String)
    case enumerator(String)
    case date(Date)
    case file(URL)
    case specifier(want: String, form: KeyForm, seld: Self, from: Self)
    case opaque(type: DescType, data: Data, value: String?)
}

extension AppleEventValue: Codable {
    private enum Tag: String, Codable { case record, type, enumerator, date, file, specifier, opaque }

    private enum Key: String, CodingKey {
        case tag = "_tag"
        case fields, name, date, url, want, form, seld, from, type, data, value
    }

    public init(from decoder: any Decoder) throws {
        func tagged(_ object: [String: JSONValue]) throws -> Self {
            let fields: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
            func code() throws -> DescType {
                let code: String = try fields.decode(String.self, forKey: .type)
                guard let type: DescType = DescType(sdef: code) else { throw DecodingError.dataCorruptedError(forKey: .type, in: fields, debugDescription: "\(code) is no four-character code") }
                return type
            }
            return switch try fields.decodeIfPresent(Tag.self, forKey: .tag) {
                case .none: try .record(type: DescType(typeAERecord), fields: object.mapValues { field in try field.decode(Self.self) })
                case .record: try .record(type: code(), fields: fields.decode([String: Self].self, forKey: .fields))
                case .type: try .type(fields.decode(String.self, forKey: .name))
                case .enumerator: try .enumerator(fields.decode(String.self, forKey: .name))
                case .date: try .date(Date.ISO8601FormatStyle().parse(fields.decode(String.self, forKey: .date)))
                case .file: try .file(fields.decode(URL.self, forKey: .url))
                case .specifier:
                    try .specifier(
                        want: fields.decode(String.self, forKey: .want),
                        form: fields.decode(KeyForm.self, forKey: .form),
                        seld: fields.decode(Self.self, forKey: .seld),
                        from: fields.decodeIfPresent(Self.self, forKey: .from) ?? .null,
                    )
                case .opaque: try .opaque(type: code(), data: fields.decode(Data.self, forKey: .data), value: fields.decodeIfPresent(String.self, forKey: .value))
            }
        }
        self =
            switch try decoder.singleValueContainer().decode(JSONValue.self) {
                case .null: .null
                case .bool(let value): .bool(value)
                case .number(let value): .number(value)
                case .string(let text): .text(text)
                case .array(let items): try .list(items.map { item in try item.decode(Self.self) })
                case .object(let object): try tagged(object)
            }
    }

    public func encode(to encoder: any Encoder) throws {
        func tagged(_ tag: Tag, _ fill: (inout KeyedEncodingContainer<Key>) throws -> Void) throws {
            var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            try fields.encode(tag, forKey: .tag)
            try fill(&fields)
        }
        switch self {
            case .null:
                var value: any SingleValueEncodingContainer = encoder.singleValueContainer()
                try value.encodeNil()
            case .bool(let value): try value.encode(to: encoder)
            case .number(let value): try value.encode(to: encoder)
            case .text(let value): try value.encode(to: encoder)
            case .list(let items): try items.encode(to: encoder)
            case .record(DescType(typeAERecord), let entries): try entries.encode(to: encoder)
            case .record(let type, let entries):
                try tagged(.record) { fields in
                    try fields.encode(type.sdef, forKey: .type)
                    try fields.encode(entries, forKey: .fields)
                }
            case .type(let name): try tagged(.type) { fields in try fields.encode(name, forKey: .name) }
            case .enumerator(let name): try tagged(.enumerator) { fields in try fields.encode(name, forKey: .name) }
            case .date(let date): try tagged(.date) { fields in try fields.encode(date.ISO8601Format(), forKey: .date) }
            case .file(let url): try tagged(.file) { fields in try fields.encode(url, forKey: .url) }
            case .specifier(let want, let form, let seld, let from):
                try tagged(.specifier) { fields in
                    try fields.encode(want, forKey: .want)
                    try fields.encode(form, forKey: .form)
                    try fields.encode(seld, forKey: .seld)
                    try fields.encode(from, forKey: .from)
                }
            case .opaque(let type, let data, let value):
                try tagged(.opaque) { fields in
                    try fields.encode(type.sdef, forKey: .type)
                    try fields.encode(data, forKey: .data)
                    try fields.encodeIfPresent(value, forKey: .value)
                }
        }
    }
}
