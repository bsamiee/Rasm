import Carbon.OpenScripting
import CasePathsCore
import CoreServices
import Encoding
package import Foundation
import OrderedCollections
import Results

// --- [TYPES] ---------------------------------------------------------------------------

package enum KeyForm: String, CaseIterable, Codable, Sendable {
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
    case next
    case previous

    init?(_ descriptor: NSAppleEventDescriptor) {
        guard let ordinal = Self.allCases.first(where: { ordinal in ordinal.type == descriptor.descriptorType && ordinal.code == descriptor.enumCodeValue }) else { return nil }
        self = ordinal
    }

    var form: KeyForm {
        switch self {
            case .first, .middle, .last, .any, .every: .index
            case .next, .previous: .relative
        }
    }

    var type: DescType { form == .index ? DescType(typeAbsoluteOrdinal) : DescType(typeEnumerated) }

    var code: FourCharCode {
        switch self {
            case .first: FourCharCode(kAEFirst)
            case .middle: FourCharCode(kAEMiddle)
            case .last: FourCharCode(kAELast)
            case .any: FourCharCode(kAEAny)
            case .every: FourCharCode(kAEAll)
            case .next: FourCharCode(kAENext)
            case .previous: FourCharCode(kAEPrevious)
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

package enum AppleEventValue: Sendable {
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
    indirect case specifier(want: String, form: KeyForm, seld: Self, from: Self)
    case opaque(type: DescType, data: Data, value: String?)
}

extension AppleEventValue: Codable {
    private enum Tag: String, Codable { case record, type, enumerator, date, file, specifier, opaque }

    private enum Key: String, CodingKey {
        case tag = "_tag"
        case fields, name, date, url, want, form, seld, from, type, data, value
    }

    package init(from decoder: any Decoder) throws {
        let value = try decoder.singleValueContainer()
        func decodeTagged() throws -> Self {
            let fields = try decoder.container(keyedBy: Key.self)
            func code() throws -> DescType {
                let code = try fields.decode(String.self, forKey: .type)
                guard let type = DescType(sdef: code) else { throw DecodingError.dataCorruptedError(forKey: .type, in: fields, debugDescription: "\(code) is no four-character code") }
                return type
            }
            return switch try fields.decodeIfPresent(Tag.self, forKey: .tag) {
                case .none: try .record(type: DescType(typeAERecord), fields: value.decode([String: Self].self))
                case .record: try .record(type: code(), fields: fields.decode([String: Self].self, forKey: .fields))
                case .type: try .type(fields.decode(String.self, forKey: .name))
                case .enumerator: try .enumerator(fields.decode(String.self, forKey: .name))
                case .date: try .date(Date(fields.decode(String.self, forKey: .date), strategy: .iso8601))
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
            if value.decodeNil() {
                .null
            } else {
                try value.decodeIfMatching(Bool.self).map(Self.bool) ?? value.decodeIfMatching(Double.self).map(Self.number)
                    ?? value.decodeIfMatching(String.self).map(Self.text) ?? value.decodeIfMatching([Self].self).map(Self.list) ?? decodeTagged()
            }
    }

    package func encode(to encoder: any Encoder) throws {
        func encodeTagged(_ tag: Tag, _ fill: (inout KeyedEncodingContainer<Key>) throws -> Void) throws {
            var fields = encoder.container(keyedBy: Key.self)
            try fields.encode(tag, forKey: .tag)
            try fill(&fields)
        }
        switch self {
            case .null:
                var value = encoder.singleValueContainer()
                try value.encodeNil()
            case .bool(let value): try value.encode(to: encoder)
            case .number(let value): try value.encode(to: encoder)
            case .text(let value): try value.encode(to: encoder)
            case .list(let items): try items.encode(to: encoder)
            case .record(DescType(typeAERecord), let entries): try entries.encode(to: encoder)
            case .record(let type, let entries):
                try encodeTagged(.record) { fields in
                    try fields.encode(type.sdef, forKey: .type)
                    try fields.encode(entries, forKey: .fields)
                }
            case .type(let name): try encodeTagged(.type) { fields in try fields.encode(name, forKey: .name) }
            case .enumerator(let name): try encodeTagged(.enumerator) { fields in try fields.encode(name, forKey: .name) }
            case .date(let date): try encodeTagged(.date) { fields in try fields.encode(date.formatted(Date.ISO8601FormatStyle(includingFractionalSeconds: true)), forKey: .date) }
            case .file(let url): try encodeTagged(.file) { fields in try fields.encode(url, forKey: .url) }
            case .specifier(let want, let form, let seld, let from):
                try encodeTagged(.specifier) { fields in
                    try fields.encode(want, forKey: .want)
                    try fields.encode(form, forKey: .form)
                    try fields.encode(seld, forKey: .seld)
                    try fields.encode(from, forKey: .from)
                }
            case .opaque(let type, let data, let value):
                try encodeTagged(.opaque) { fields in
                    try fields.encode(type.sdef, forKey: .type)
                    try fields.encode(data, forKey: .data)
                    try fields.encodeIfPresent(value, forKey: .value)
                }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension AppleEventValue {
    init(_ descriptor: NSAppleEventDescriptor, in dictionary: ScriptingDictionary) {
        func opaque() -> Self {
            .opaque(
                type: descriptor.descriptorType,
                data: descriptor.data,
                value: descriptor.stringValue ?? descriptor.coerce(toDescriptorType: DescType(typeIEEE64BitFloatingPoint)).map { real in "\(real.doubleValue)" },
            )
        }
        func exact<Value: FixedWidthInteger & ConvertibleFromBytes>(_: Value.Type) -> Self {
            (descriptor.data.count == MemoryLayout<Value>.size ? Double(exactly: descriptor.data.bytes.load(fromByteOffset: 0, as: Value.self)) : nil).map(Self.number) ?? opaque()
        }
        func record() -> Self {
            let scope = dictionary.scope(descriptor.forKeyword(AEKeyword(pClass))?.typeCodeValue ?? descriptor.descriptorType)
            let labelled = descriptor.forKeyword(AEKeyword(keyASUserRecordFields))
            let keyed: [(String, Self)] = (1..<descriptor.numberOfItems + 1).compactMap { index in
                let keyword = descriptor.keywordForDescriptor(at: index)
                return keyword == AEKeyword(keyASUserRecordFields)
                    ? nil : descriptor.atIndex(index).map { value in (ScriptingDictionary.name(of: keyword, in: scope), Self(value, in: dictionary)) }
            }
            let user: [(String, Self)] = stride(from: 1, to: labelled?.numberOfItems ?? 0, by: 2).compactMap { index in
                switch (labelled?.atIndex(index)?.stringValue, labelled?.atIndex(index + 1)) {
                    case (.some(let label), .some(let value)): (label, Self(value, in: dictionary))
                    default: nil
                }
            }
            return .record(type: descriptor.descriptorType, fields: Dictionary(keyed + user) { first, _ in first })
        }
        self =
            switch descriptor.descriptorType {
                case DescType(typeObjectSpecifier):
                    switch (
                        descriptor.forKeyword(AEKeyword(keyAEDesiredClass)),
                        descriptor.forKeyword(AEKeyword(keyAEKeyForm)).flatMap { code in KeyForm.allCases.first { form in form.code == code.enumCodeValue } },
                        descriptor.forKeyword(AEKeyword(keyAEKeyData)),
                        descriptor.forKeyword(AEKeyword(keyAEContainer)).map { from in (from, from.forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue ?? FourCharCode(cApplication)) },
                    ) {
                        case (.some(let want), .some(let form), .some(let seld), .some(let (from, container))):
                            .specifier(
                                want: ScriptingDictionary.name(of: want.typeCodeValue, in: [dictionary.types, dictionary.properties]),
                                form: form,
                                seld: form == .property
                                    ? .type(ScriptingDictionary.name(of: seld.typeCodeValue, in: dictionary.scope(container)))
                                    : Ordinal(seld).map { ordinal in Self.text(ordinal.rawValue) } ?? Self(seld, in: dictionary),
                                from: Self(from, in: dictionary),
                            )
                        default: record()
                    }
                case DescType(typeNull): .null
                case DescType(typeType) where descriptor.typeCodeValue == FourCharCode(cMissingValue): .null
                case DescType(typeTrue), DescType(typeFalse), DescType(typeBoolean): .bool(descriptor.booleanValue)
                case DescType(typeSInt64): exact(Int64.self)
                case DescType(typeUInt64): exact(UInt64.self)
                case DescType(typeSInt16), DescType(typeUInt16), DescType(typeSInt32), DescType(typeUInt32), DescType(typeIEEE32BitFloatingPoint), DescType(typeIEEE64BitFloatingPoint),
                    DescType(type128BitFloatingPoint), DescType(typeDecimalStruct), DescType(typeFixed), DescType(typeLongFixed):
                    descriptor.coerce(toDescriptorType: DescType(typeIEEE64BitFloatingPoint)).flatMap { real in real.doubleValue.isFinite ? Self.number(real.doubleValue) : nil } ?? opaque()
                case DescType(typeChar), DescType(typeCString), DescType(typePString), DescType(typeUnicodeText), DescType(typeUTF8Text), DescType(typeUTF16ExternalRepresentation),
                    DescType(typeStyledText), DescType(typeIntlText):
                    descriptor.stringValue.map(Self.text) ?? opaque()
                case DescType(typeAEList): .list((1..<descriptor.numberOfItems + 1).compactMap(descriptor.atIndex).map { item in Self(item, in: dictionary) })
                case DescType(typeType), DescType(typeKeyword): .type(ScriptingDictionary.name(of: descriptor.typeCodeValue, in: [dictionary.types, dictionary.properties]))
                case DescType(typeEnumerated): .enumerator(ScriptingDictionary.name(of: descriptor.enumCodeValue, in: [dictionary.enumerators]))
                case DescType(typeAbsoluteOrdinal): Ordinal(descriptor).map { ordinal in Self.text(ordinal.rawValue) } ?? opaque()
                case DescType(typeFileURL), DescType(typeBookmarkData), DescType(typeAlias), DescType(typeFSRef): descriptor.fileURLValue.map(Self.file) ?? opaque()
                case DescType(typeLongDateTime), DescType(typeISO8601DateTime), DescType(typeCFAbsoluteTime): descriptor.dateValue.map(Self.date) ?? opaque()
                case _ where descriptor.isRecordDescriptor: record()
                default: opaque()
            }
    }

    func descriptor(as types: Set<String>, in dictionary: ScriptingDictionary) throws(AggregateError<AppleEventFailure>) -> NSAppleEventDescriptor {
        func makeDescriptor(_ value: Self, as types: Set<String> = []) -> Result<NSAppleEventDescriptor, AggregateError<AppleEventFailure>> {
            Result { () throws(AggregateError<AppleEventFailure>) in try value.descriptor(as: types, in: dictionary) }
        }
        func unencodable(_ subject: String) -> AggregateError<AppleEventFailure> {
            AggregateError(first: AppleEventFailure(subject: subject, reason: .unencodable))
        }
        func makeFileDescriptor(_ url: URL) throws(AggregateError<AppleEventFailure>) -> NSAppleEventDescriptor {
            guard types.contains("alias") else { return NSAppleEventDescriptor(fileURL: url) }
            let path = url.path(percentEncoded: false)
            let bookmark = try Result { try url.bookmarkData() }.aggregated { error in AppleEventFailure(subject: path, reason: .foundation(error as NSError)) }.get()
            guard let alias = NSAppleEventDescriptor(descriptorType: DescType(typeBookmarkData), data: bookmark)?.coerce(toDescriptorType: DescType(typeAlias)) else {
                throw unencodable("an alias of \(path)")
            }
            return alias
        }
        switch self {
            case .null:
                return .null()
            case .bool(let value):
                return NSAppleEventDescriptor(boolean: value)
            case .number(let value):
                return (types.contains("integer") ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value)
            case .text(let text):
                let enumerators = dictionary.enumerators(of: types)
                let enumerated = enumerators.byName[text] != nil || FourCharCode(sdef: text).flatMap { code in enumerators.byCode[code] } != nil
                return switch types.isDisjoint(with: ["file", "alias", "file specification"]) ? nil : URL(string: text) {
                    case _ where enumerated: try NSAppleEventDescriptor(enumCode: ScriptingDictionary.resolve(text, in: [enumerators]).get().code)
                    case _ where types.contains("type"): try Self.type(text).descriptor(as: types, in: dictionary)
                    case _ where types.contains("date"):
                        try NSAppleEventDescriptor(
                            date: Result { try Date(text, strategy: .iso8601) }.aggregated { error in AppleEventFailure(subject: text, reason: .foundation(error as NSError)) }.get()
                        )
                    case .some(let url) where url.isFileURL: try makeFileDescriptor(url)
                    case .some, .none: NSAppleEventDescriptor(string: text)
                }
            case .list(let items):
                return try traverse(items) { item in makeDescriptor(item, as: types) }.get().reduce(into: .list()) { list, descriptor in list.insert(descriptor, at: 0) }
            case .record(let type, let fields):
                let classed = fields.lazy.filter { key, _ in ScriptingDictionary.resolve(key, in: [dictionary.properties])[case: \.success]?.code == FourCharCode(pClass) }
                    .min { left, right in left.key < right.key }?.value
                let scopes = dictionary.scope(classed.flatMap(dictionary.classCode(of:)) ?? type)
                let entries = try traverse(fields.sorted { left, right in left.key < right.key }) { label, value in
                    let term = ScriptingDictionary.resolve(label, in: scopes)[case: \.success]
                    return makeDescriptor(value, as: term?.types ?? []).map { descriptor in (keyword: term?.code, label: label, descriptor: descriptor) }
                }
                .get()
                let (record, user) = entries.reduce(into: (NSAppleEventDescriptor.record(), NSAppleEventDescriptor.list())) { built, entry in
                    switch entry.keyword {
                        case .some(let keyword): built.0.setDescriptor(entry.descriptor, forKeyword: keyword)
                        case .none: for item in [NSAppleEventDescriptor(string: entry.label), entry.descriptor] { built.1.insert(item, at: 0) }
                    }
                }
                if user.numberOfItems > 0 { record.setDescriptor(user, forKeyword: AEKeyword(keyASUserRecordFields)) }
                guard let typed = record.coerce(toDescriptorType: type) else { throw unencodable("a record of type \(ScriptingDictionary.name(of: type, in: [dictionary.types]))") }
                return typed
            case .type(let name):
                return try NSAppleEventDescriptor(typeCode: ScriptingDictionary.resolve(name, in: [dictionary.types, dictionary.properties]).get().code)
            case .enumerator(let name):
                return try NSAppleEventDescriptor(enumCode: ScriptingDictionary.resolve(name, in: [dictionary.enumerators(of: types), dictionary.enumerators]).get().code)
            case .date(let date):
                return NSAppleEventDescriptor(date: date)
            case .file(let url):
                return try makeFileDescriptor(url)
            case .specifier(let want, let form, let seld, let from):
                let containerClass = dictionary.classCode(of: from)
                let desired: Result<FourCharCode, AggregateError<AppleEventFailure>> =
                    form == .property ? .success(FourCharCode(cProperty)) : ScriptingDictionary.resolve(want, in: dictionary.classes(in: containerClass)).map(\.code)
                let key: Result<NSAppleEventDescriptor, AggregateError<AppleEventFailure>> =
                    switch (form, seld) {
                        case (.property, .text(let name)), (.property, .type(let name)):
                            ScriptingDictionary.resolve(name, in: dictionary.scope(containerClass)).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                        case (.index, .text(let text)), (.relative, .text(let text)):
                            Unmatched.single(
                                Ordinal.allCases.filter { ordinal in ordinal.form == form && ordinal.rawValue == text },
                                subject: "ordinal of the \(form.rawValue) form",
                                query: text,
                                candidates: Ordinal.allCases.filter { ordinal in ordinal.form == form }.map(\.rawValue),
                                named: \.rawValue,
                            )
                            .aggregated { miss in AppleEventFailure(subject: text, reason: .unmatched(miss)) }
                            .flatMap { ordinal in
                                NSAppleEventDescriptor(descriptorType: ordinal.type, data: NSAppleEventDescriptor(enumCode: ordinal.code).data).map(Result.success)
                                    ?? .failure(unencodable("the ordinal \(text)"))
                            }
                        case (.index, .number(let value)):
                            Int32(exactly: value).map { offset in .success(NSAppleEventDescriptor(int32: offset)) }
                                ?? .failure(unencodable("the index \(value), as an index is a whole number of 32 bits"))
                        default:
                            makeDescriptor(seld, as: ["integer"])
                    }
                let (desiredClass, data, container) = try combine(desired, key, makeDescriptor(from)).get()
                let fields: KeyValuePairs<Int, NSAppleEventDescriptor> = [
                    keyAEDesiredClass: NSAppleEventDescriptor(typeCode: desiredClass), keyAEContainer: container, keyAEKeyForm: NSAppleEventDescriptor(enumCode: form.code),
                    keyAEKeyData: data,
                ]
                let record = fields.reduce(into: NSAppleEventDescriptor.record()) { record, field in record.setDescriptor(field.value, forKeyword: AEKeyword(field.key)) }
                guard let specifier = record.coerce(toDescriptorType: DescType(typeObjectSpecifier)) else { throw unencodable("the object specifier of \(want)") }
                return specifier
            case .opaque(let type, let data, _):
                guard let descriptor = NSAppleEventDescriptor(descriptorType: type, data: data) else { throw unencodable("the \(data.count)-byte value of descriptor type \(type.sdef)") }
                return descriptor
        }
    }
}
