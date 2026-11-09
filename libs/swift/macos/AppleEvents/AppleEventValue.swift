import CasePathsCore
import CoreServices
import Encoding
public import Foundation
import OrderedCollections
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
}

// --- [CONSTANTS] -----------------------------------------------------------------------

public let fileTypes: Set<String> = ["file", "alias", "file specification"]

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
                guard let type = DescType(sdef: code) else { throw DecodingError.dataCorruptedError(forKey: .type, in: fields, debugDescription: "\(code) is no four-character code") }
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

// --- [OPERATIONS] ----------------------------------------------------------------------

extension AppleEventValue {
    public init(_ descriptor: NSAppleEventDescriptor, in dictionary: ScriptingDictionary) {
        let opaque: () -> Self = { .opaque(type: descriptor.descriptorType, data: descriptor.data, value: descriptor.stringValue) }
        func exact<Value: FixedWidthInteger & ConvertibleFromBytes>(_: Value.Type) -> Self {
            Double(exactly: descriptor.data.bytes.load(fromByteOffset: 0, as: Value.self)).map(Self.number) ?? opaque()
        }
        func specifier() -> Self? {
            guard let want: NSAppleEventDescriptor = descriptor.forKeyword(AEKeyword(keyAEDesiredClass)),
                let form = descriptor.forKeyword(AEKeyword(keyAEKeyForm)).flatMap({ form in KeyForm.allCases.first { candidate in candidate.code == form.enumCodeValue } }),
                let seld = descriptor.forKeyword(AEKeyword(keyAEKeyData)),
                let from = descriptor.forKeyword(AEKeyword(keyAEContainer))
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

    public func descriptor(as types: Set<String>, in dictionary: ScriptingDictionary) throws(AggregateError<AppleEventFailure>) -> NSAppleEventDescriptor {
        func encoded(_ value: Self, as types: Set<String> = []) -> Result<NSAppleEventDescriptor, AggregateError<AppleEventFailure>> {
            Result { () throws(AggregateError<AppleEventFailure>) in try value.descriptor(as: types, in: dictionary) }
        }
        func unencodable(_ type: DescType) -> AggregateError<AppleEventFailure> {
            AggregateError(first: AppleEventFailure(subject: type.sdef, reason: .unencodable))
        }
        switch self {
            case .null:
                return .null()
            case .bool(let value):
                return NSAppleEventDescriptor(boolean: value)
            case .number(let value):
                return (types.contains("integer") ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value)
            case .text(let text):
                return switch dictionary.enumerators(of: types) {
                    case let enumerators where enumerators.named[text] != nil || !enumerators.coded.isEmpty && FourCharCode(sdef: text) != nil:
                        try NSAppleEventDescriptor(enumCode: ScriptingDictionary.resolve(text, in: [enumerators]).get().code)
                    case _ where types.contains("type"):
                        try Self.type(text).descriptor(as: types, in: dictionary)
                    case _ where types.contains("date"):
                        try NSAppleEventDescriptor(date: Result { try Date.ISO8601FormatStyle().parse(text) }.aggregated { error in AppleEventFailure(subject: text, reason: .host(error)) }.get())
                    default:
                        switch URL(string: text) {
                            case .some(let url) where url.isFileURL && !types.isDisjoint(with: fileTypes): NSAppleEventDescriptor(fileURL: url)
                            case .some, .none: NSAppleEventDescriptor(string: text)
                        }
                }
            case .list(let items):
                return try traverse(items) { item in encoded(item, as: types) }.get().reduce(into: .list()) { list, descriptor in list.insert(descriptor, at: list.numberOfItems + 1) }
            case .record(let type, let fields):
                let named: Self? = fields.sorted { left, right in left.key < right.key }.first { key, _ in
                    ScriptingDictionary.resolve(key, in: [dictionary.properties])[case: \.success]?.code == FourCharCode(pClass)
                }?.value
                let record: NSAppleEventDescriptor = try dictionary.record(fields, in: dictionary.scope(named.flatMap(dictionary.classCode(of:)) ?? type), into: .record())
                guard let typed: NSAppleEventDescriptor = record.coerce(toDescriptorType: type) else { throw unencodable(type) }
                return typed
            case .type(let name):
                return try NSAppleEventDescriptor(typeCode: ScriptingDictionary.resolve(name, in: [dictionary.types, dictionary.properties]).get().code)
            case .enumerator(let name):
                return try NSAppleEventDescriptor(enumCode: ScriptingDictionary.resolve(name, in: [dictionary.enumerators(of: types), dictionary.enumerators]).get().code)
            case .date(let date):
                return NSAppleEventDescriptor(date: date)
            case .file(let url):
                return NSAppleEventDescriptor(fileURL: url)
            case .specifier(let want, let form, let seld, let from):
                let key: Result<NSAppleEventDescriptor, AggregateError<AppleEventFailure>> =
                    switch (form, seld) {
                        case (.property, .text(let name)), (.property, .type(let name)):
                            ScriptingDictionary.resolve(name, in: dictionary.scope(dictionary.classCode(of: from))).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                        case (.index, .text(let text)):
                            Ordinal(rawValue: text).map { ordinal in
                                NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: ordinal.code).data).map(Result.success)
                                    ?? .failure(unencodable(DescType(typeAbsoluteOrdinal)))
                            }
                                ?? .failure(
                                    AggregateError(
                                        first: AppleEventFailure(subject: text, reason: .unmatched(Unmatched(subject: "ordinal", query: text, candidates: Ordinal.allCases.map(\.rawValue))))
                                    )
                                )
                        case (.index, .number(let value)):
                            Int(exactly: value).flatMap { offset in unsafe NSAppleEventDescriptor { made in unsafe CreateOffsetDescriptor(offset, made) } }.map(Result.success)
                                ?? .failure(unencodable(DescType(typeSInt32)))
                        default:
                            encoded(seld, as: ["integer"])
                    }
                let (desired, data, container): (ScriptingDictionary.Term, NSAppleEventDescriptor, NSAppleEventDescriptor) =
                    try combine(ScriptingDictionary.resolve(want, in: [dictionary.types, dictionary.properties]), key, encoded(from)).get()
                guard var containerDesc: AEDesc = unsafe container.aeDesc?.pointee, var keyDesc: AEDesc = unsafe data.aeDesc?.pointee,
                    let specifier = unsafe NSAppleEventDescriptor(made: { made in unsafe CreateObjSpecifier(desired.code, &containerDesc, form.code, &keyDesc, false, made) })
                else { throw unencodable(DescType(typeObjectSpecifier)) }
                return specifier
            case .opaque(let type, let data, _):
                guard let descriptor = NSAppleEventDescriptor(descriptorType: type, data: data) else { throw unencodable(type) }
                return descriptor
        }
    }
}

extension NSAppleEventDescriptor {
    convenience init?(made create: (UnsafeMutablePointer<AEDesc>) -> OSErr) {
        var made: AEDesc = unsafe AEDesc()
        guard unsafe create(&made) == noErr else { return nil }
        unsafe self.init(aeDescNoCopy: &made)
    }
}
