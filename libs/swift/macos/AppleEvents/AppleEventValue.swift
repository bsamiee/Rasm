import CoreServices
public import Foundation
public import Results

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

    var code: FourCharCode {
        switch self {
            case .property: FourCharCode(formPropertyID)
            case .index: FourCharCode(formAbsolutePosition)
            case .name: FourCharCode(formName)
            case .id: FourCharCode(formUniqueID)
            case .relative: FourCharCode(formRelativePosition)
            case .range: FourCharCode(formRange)
            case .test: FourCharCode(formTest)
            case .whose: FourCharCode(formWhose)
        }
    }
}

enum Ordinal: String, CaseIterable, Sendable {
    case first
    case middle
    case last
    case any
    case every

    var code: FourCharCode {
        switch self {
            case .first: FourCharCode(kAEFirst)
            case .middle: FourCharCode(kAEMiddle)
            case .last: FourCharCode(kAELast)
            case .any: FourCharCode(kAEAny)
            case .every: FourCharCode(kAEAll)
        }
    }

    var descriptor: NSAppleEventDescriptor? {
        NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: code).data)
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
        let value: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        func tagged() throws -> Self {
            let fields: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
            func code() throws -> DescType {
                let code: String = try fields.decode(String.self, forKey: .type)
                guard let type: DescType = FourCharCode(sdef: code) else { throw DecodingError.dataCorruptedError(forKey: .type, in: fields, debugDescription: "\(code) is no four-character code") }
                return type
            }
            return switch try fields.decodeIfPresent(Tag.self, forKey: .tag) {
                case .none: try .record(type: DescType(typeAERecord), fields: value.decode([String: Self].self))
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
            try value.decodeNil()
            ? .null
            : Result { try .bool(value.decode(Bool.self)) }
                .flatMapError { _ in Result { try .number(value.decode(Double.self)) } }
                .flatMapError { _ in Result { try .text(value.decode(String.self)) } }
                .flatMapError { _ in Result { try .list(value.decode([Self].self)) } }
                .flatMapError { _ in Result(catching: tagged) }
                .get()
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
            case .date(let date): try tagged(.date) { fields in try fields.encode(date.formatted(.iso8601), forKey: .date) }
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

extension AppleEventValue {
    public init(_ descriptor: NSAppleEventDescriptor, in dictionary: ScriptingDictionary) {
        let opaque: () -> Self = { .opaque(type: descriptor.descriptorType, data: descriptor.data, value: descriptor.stringValue) }
        func exact<Value: FixedWidthInteger & ConvertibleFromBytes>(_: Value.Type) -> Self {
            Double(exactly: descriptor.data.bytes.load(fromByteOffset: 0, as: Value.self)).map(Self.number) ?? opaque()
        }
        func specifier() -> Self? {
            guard let want: NSAppleEventDescriptor = descriptor.forKeyword(AEKeyword(keyAEDesiredClass)),
                let form: KeyForm = descriptor.forKeyword(AEKeyword(keyAEKeyForm)).flatMap({ form in KeyForm.allCases.first { candidate in candidate.code == form.enumCodeValue } }),
                let seld: NSAppleEventDescriptor = descriptor.forKeyword(AEKeyword(keyAEKeyData)),
                let from: NSAppleEventDescriptor = descriptor.forKeyword(AEKeyword(keyAEContainer))
            else { return nil }
            let container: FourCharCode = from.forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue ?? FourCharCode(cApplication)
            return .specifier(
                want: ScriptingDictionary.term(want.typeCodeValue, in: [dictionary.types, dictionary.properties]).name,
                form: form,
                seld: form == .property ? .type(ScriptingDictionary.term(seld.typeCodeValue, in: dictionary.scope(container)).name) : Self(seld, in: dictionary),
                from: Self(from, in: dictionary),
            )
        }
        func record() -> Self {
            let scope: [ScriptingDictionary.Scope] = dictionary.scope(descriptor.forKeyword(AEKeyword(pClass))?.typeCodeValue ?? descriptor.descriptorType)
            let fields: [(String, Self)] = (1..<descriptor.numberOfItems + 1).compactMap { index in
                descriptor.atIndex(index).map { value in (ScriptingDictionary.term(descriptor.keywordForDescriptor(at: index), in: scope).name, Self(value, in: dictionary)) }
            }
            return .record(type: descriptor.descriptorType, fields: Dictionary(fields) { first, _ in first })
        }
        self =
            switch descriptor.descriptorType {
                case DescType(typeObjectSpecifier): specifier() ?? record()
                case DescType(typeNull): .null
                case DescType(typeTrue), DescType(typeFalse), DescType(typeBoolean): .bool(descriptor.booleanValue)
                case DescType(typeSInt16), DescType(typeSInt32), DescType(typeUInt16), DescType(typeUInt32): .number(descriptor.doubleValue)
                case DescType(typeSInt64): exact(Int64.self)
                case DescType(typeUInt64): exact(UInt64.self)
                case DescType(typeIEEE32BitFloatingPoint) where descriptor.doubleValue.isFinite, DescType(typeIEEE64BitFloatingPoint) where descriptor.doubleValue.isFinite:
                    .number(descriptor.doubleValue)
                case DescType(typeChar), DescType(typeCString), DescType(typePString), DescType(typeUnicodeText), DescType(typeUTF8Text), DescType(typeUTF16ExternalRepresentation):
                    descriptor.stringValue.map(Self.text) ?? opaque()
                case DescType(typeAEList): .list((1..<descriptor.numberOfItems + 1).compactMap(descriptor.atIndex).map { item in Self(item, in: dictionary) })
                case DescType(typeType): .type(ScriptingDictionary.term(descriptor.typeCodeValue, in: [dictionary.types, dictionary.properties]).name)
                case DescType(typeEnumerated): .enumerator(ScriptingDictionary.term(descriptor.enumCodeValue, in: [dictionary.enumerators]).name)
                case DescType(typeAbsoluteOrdinal): Ordinal.allCases.first { ordinal in ordinal.code == descriptor.enumCodeValue }.map { ordinal in .text(ordinal.rawValue) } ?? opaque()
                case DescType(typeFileURL), DescType(typeAlias): descriptor.fileURLValue.map(Self.file) ?? opaque()
                case DescType(typeLongDateTime): descriptor.dateValue.map(Self.date) ?? opaque()
                case _ where descriptor.isRecordDescriptor: record()
                default: opaque()
            }
    }

    public func descriptor(as types: Set<String>, in dictionary: ScriptingDictionary) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
        switch self {
            case .null:
                return .success(.null())
            case .bool(let value):
                return .success(NSAppleEventDescriptor(boolean: value))
            case .number(let value):
                return .success((types.contains("integer") ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value))
            case .text(let text):
                return switch dictionary.enumerators(of: types) {
                    case let enumerators where enumerators.named[text] != nil || !enumerators.coded.isEmpty && FourCharCode(sdef: text) != nil:
                        ScriptingDictionary.resolve(text, in: [enumerators]).map { term in NSAppleEventDescriptor(enumCode: term.code) }
                    case _ where types.contains("type"):
                        Self.type(text).descriptor(as: types, in: dictionary)
                    case _ where types.contains("date"):
                        Result { try Date.ISO8601FormatStyle().parse(text) }.map(NSAppleEventDescriptor.init(date:)).aggregated { error in .undated(text, error) }
                    default:
                        switch URL(string: text) {
                            case .some(let url) where url.isFileURL && !types.isDisjoint(with: ["file", "alias", "file specification"]): .success(NSAppleEventDescriptor(fileURL: url))
                            case .some, .none: .success(NSAppleEventDescriptor(string: text))
                        }
                }
            case .list(let items):
                return traverse(items) { item in item.descriptor(as: types, in: dictionary) }.map { descriptors in
                    descriptors.reduce(into: .list()) { list, descriptor in list.insert(descriptor, at: list.numberOfItems + 1) }
                }
            case .record(let type, let fields):
                let named: Self? = fields.sorted { left, right in left.key < right.key }.first { key, _ in
                    if case .success(let term) = ScriptingDictionary.resolve(key, in: [dictionary.properties]) { term.code == FourCharCode(pClass) } else { false }
                }?.value
                return Result { () throws(AggregateError<DictionaryFailure>) in
                    let record: NSAppleEventDescriptor = try dictionary.record(fields, in: dictionary.scope(named.flatMap(dictionary.classCode(of:)) ?? type), into: .record()).get()
                    guard let typed: NSAppleEventDescriptor = record.coerce(toDescriptorType: type) else { throw AggregateError(first: .unencodable(type)) }
                    return typed
                }
            case .type(let name):
                return ScriptingDictionary.resolve(name, in: [dictionary.types, dictionary.properties]).map { term in NSAppleEventDescriptor(typeCode: term.code) }
            case .enumerator(let name):
                return ScriptingDictionary.resolve(name, in: [dictionary.enumerators(of: types), dictionary.enumerators]).map { term in NSAppleEventDescriptor(enumCode: term.code) }
            case .date(let date):
                return .success(NSAppleEventDescriptor(date: date))
            case .file(let url):
                return .success(NSAppleEventDescriptor(fileURL: url))
            case .specifier(let want, let form, let seld, let from):
                let key: Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> =
                    switch (form, seld) {
                        case (.property, .text(let name)), (.property, .type(let name)):
                            ScriptingDictionary.resolve(name, in: dictionary.scope(dictionary.classCode(of: from))).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                        case (.index, .text(let text)):
                            switch Ordinal(rawValue: text).map(\.descriptor) {
                                case .some(.some(let descriptor)): .success(descriptor)
                                case .some(.none): .failure(AggregateError(first: .unencodable(DescType(typeAbsoluteOrdinal))))
                                case .none: .failure(AggregateError(first: .unmatched(Unmatched(subject: "ordinal", query: text, candidates: Ordinal.allCases.map(\.rawValue)))))
                            }
                        default:
                            seld.descriptor(as: ["integer"], in: dictionary)
                    }
                return Result { () throws(AggregateError<DictionaryFailure>) in
                    let (desired, data, container): (ScriptingDictionary.Term, NSAppleEventDescriptor, NSAppleEventDescriptor) =
                        try combine(ScriptingDictionary.resolve(want, in: [dictionary.types, dictionary.properties]), key, from.descriptor(as: [], in: dictionary)).get()
                    let record: NSAppleEventDescriptor = [
                        (AEKeyword(keyAEDesiredClass), NSAppleEventDescriptor(typeCode: desired.code)),
                        (AEKeyword(keyAEKeyForm), NSAppleEventDescriptor(enumCode: form.code)),
                        (AEKeyword(keyAEKeyData), data),
                        (AEKeyword(keyAEContainer), container),
                    ]
                    .reduce(into: .record()) { record, item in record.setDescriptor(item.1, forKeyword: item.0) }
                    guard let specifier: NSAppleEventDescriptor = record.coerce(toDescriptorType: DescType(typeObjectSpecifier)) else {
                        throw AggregateError(first: .unencodable(DescType(typeObjectSpecifier)))
                    }
                    return specifier
                }
            case .opaque(let type, let data, _):
                return Result { () throws(AggregateError<DictionaryFailure>) in
                    guard let descriptor: NSAppleEventDescriptor = NSAppleEventDescriptor(descriptorType: type, data: data) else { throw AggregateError(first: .unencodable(type)) }
                    return descriptor
                }
        }
    }
}
