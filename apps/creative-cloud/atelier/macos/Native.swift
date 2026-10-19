import Carbon.OpenScripting
import Foundation
import RasmMacOS

// --- [TYPES] ---------------------------------------------------------------------------

protocol Coded: CaseIterable {
    var code: FourCharCode { get }
}

extension Coded {
    init?(code: FourCharCode) {
        guard let found: Self = Self.allCases.first(where: { value in value.code == code }) else { return nil }
        self = found
    }
}

enum KeyForm: String, Codable, Coded, Sendable {
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

enum Ordinal: String, Coded, Sendable {
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

// --- [MODELS] --------------------------------------------------------------------------

indirect enum Native: Sendable {
    case null
    case bool(Bool)
    case number(Double)
    case text(String)
    case list([Self])
    case record([String: Self])
    case type(String)
    case enumerator(String)
    case date(Date)
    case file(URL)
    case specifier(want: String, form: KeyForm, seld: Self, from: Self)
    case opaque(type: DescType, data: Data, value: String?)
}

extension Native: Codable {
    private enum Tag: String, Codable { case type, enumerator, date, file, specifier, opaque }
    private enum Key: String, CodingKey {
        case tag = "_tag"
        case name, date, url, want, form, seld, from, type, data, value
    }

    init(from decoder: any Decoder) throws {
        let value: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        self =
            try value.decodeNil()
            ? .null
            : Result { try .bool(value.decode(Bool.self)) }
                .flatMapError { _ in Result { try .number(value.decode(Double.self)) } }
                .flatMapError { _ in Result { try .text(value.decode(String.self)) } }
                .flatMapError { _ in Result { try .list(value.decode([Self].self)) } }
                .flatMapError { _ in
                    Result {
                        let fields: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
                        let text: (Key) throws -> String = { key in try fields.decode(String.self, forKey: key) }
                        let type: () throws -> DescType = {
                            let code: String = try text(.type)
                            guard let type: DescType = FourCharCode(sdef: code) else {
                                throw DecodingError.dataCorruptedError(forKey: .type, in: fields, debugDescription: "\(code) is no four-character code")
                            }
                            return type
                        }
                        let from: () throws -> Self = { try fields.decodeIfPresent(Self.self, forKey: .from) ?? Self.null }
                        return switch try fields.decodeIfPresent(Tag.self, forKey: .tag) {
                            case .none: try Self.record(value.decode([String: Self].self))
                            case .type: try Self.type(text(.name))
                            case .enumerator: try Self.enumerator(text(.name))
                            case .date: try Self.date(Date.ISO8601FormatStyle().parse(text(.date)))
                            case .file: try Self.file(fields.decode(URL.self, forKey: .url))
                            case .specifier: try Self.specifier(want: text(.want), form: fields.decode(KeyForm.self, forKey: .form), seld: fields.decode(Self.self, forKey: .seld), from: from())
                            case .opaque: try Self.opaque(type: type(), data: fields.decode(Data.self, forKey: .data), value: fields.decodeIfPresent(String.self, forKey: .value))
                        }
                    }
                }
                .get()
    }

    func encode(to encoder: any Encoder) throws {
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
            case .record(let entries): try entries.encode(to: encoder)
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

extension Native {
    init(_ descriptor: NSAppleEventDescriptor, in dictionary: ScriptingDictionary) {
        let opaque: () -> Self = { .opaque(type: descriptor.descriptorType, data: descriptor.data, value: descriptor.stringValue) }
        let reference: (want: NSAppleEventDescriptor?, form: KeyForm?, seld: NSAppleEventDescriptor?, from: NSAppleEventDescriptor?) = (
            descriptor.forKeyword(AEKeyword(keyAEDesiredClass)), descriptor.forKeyword(AEKeyword(keyAEKeyForm)).flatMap { form in KeyForm(code: form.enumCodeValue) },
            descriptor.forKeyword(AEKeyword(keyAEKeyData)), descriptor.forKeyword(AEKeyword(keyAEContainer)),
        )
        self =
            switch (descriptor.descriptorType, reference) {
                case (DescType(typeObjectSpecifier), (.some(let want), .some(let form), .some(let seld), .some(let from))):
                    .specifier(
                        want: ScriptingDictionary.term(want.typeCodeValue, in: [dictionary.classes, dictionary.properties]).name,
                        form: form,
                        seld: form == .property
                            ? .type(
                                ScriptingDictionary.term(seld.typeCodeValue, in: dictionary.scope(from.forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue ?? FourCharCode(cApplication))).name
                            )
                            : Self(seld, in: dictionary),
                        from: Self(from, in: dictionary),
                    )
                case (DescType(typeNull), _): .null
                case (DescType(typeTrue), _), (DescType(typeFalse), _), (DescType(typeBoolean), _): .bool(descriptor.booleanValue)
                case (DescType(typeSInt16), _), (DescType(typeSInt32), _), (DescType(typeUInt16), _), (DescType(typeUInt32), _): .number(descriptor.doubleValue)
                case (DescType(typeIEEE32BitFloatingPoint), _) where descriptor.doubleValue.isFinite, (DescType(typeIEEE64BitFloatingPoint), _) where descriptor.doubleValue.isFinite:
                    .number(descriptor.doubleValue)
                case (DescType(typeChar), _), (DescType(typeCString), _), (DescType(typePString), _), (DescType(typeUnicodeText), _), (DescType(typeUTF8Text), _),
                    (DescType(typeUTF16ExternalRepresentation), _):
                    descriptor.stringValue.map(Self.text) ?? opaque()
                case (DescType(typeAEList), _): .list((1..<descriptor.numberOfItems + 1).compactMap(descriptor.atIndex).map { item in Self(item, in: dictionary) })
                case (DescType(typeAERecord), _):
                    .record(
                        Dictionary(
                            (1..<descriptor.numberOfItems + 1).compactMap { index in
                                descriptor.atIndex(index).map { value in
                                    (
                                        ScriptingDictionary.term(descriptor.keywordForDescriptor(at: index), in: dictionary.scope(descriptor.forKeyword(AEKeyword(pClass))?.typeCodeValue)).name,
                                        Self(value, in: dictionary),
                                    )
                                }
                            }
                        ) { first, _ in first }
                    )
                case (DescType(typeType), _): .type(ScriptingDictionary.term(descriptor.typeCodeValue, in: [dictionary.classes, dictionary.properties]).name)
                case (DescType(typeEnumerated), _): .enumerator(ScriptingDictionary.term(descriptor.enumCodeValue, in: [dictionary.enumerations.flatMap(\.members)]).name)
                case (DescType(typeAbsoluteOrdinal), _): Ordinal(code: descriptor.enumCodeValue).map { ordinal in .text(ordinal.rawValue) } ?? opaque()
                case (DescType(typeFileURL), _), (DescType(typeAlias), _): descriptor.fileURLValue.map(Self.file) ?? opaque()
                case (DescType(typeLongDateTime), _): descriptor.dateValue.map(Self.date) ?? opaque()
                default: opaque()
            }
    }

    func descriptor(as types: Set<SdefType>, in dictionary: ScriptingDictionary) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
        switch self {
            case .null:
                return .success(.null())
            case .bool(let value):
                return .success(NSAppleEventDescriptor(boolean: value))
            case .number(let value):
                return .success((types.contains(.primitive(.integer)) ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value))
            case .text(let text):
                return switch dictionary.enumerators(of: types) {
                    case let enumerators where enumerators.contains(where: { term in term.name == text }) || !enumerators.isEmpty && FourCharCode(sdef: text) != nil:
                        ScriptingDictionary.resolve(text, in: [enumerators]).map { term in NSAppleEventDescriptor(enumCode: term.code) }
                    case _ where types.contains(.primitive(.type)):
                        Self.type(text).descriptor(as: types, in: dictionary)
                    case _ where types.contains(.primitive(.date)):
                        Result { try Date.ISO8601FormatStyle().parse(text) }.map(NSAppleEventDescriptor.init(date:)).aggregated { _ in .date(text) }
                    case _ where !types.isDisjoint(with: [.primitive(.file), .primitive(.alias), .primitive(.fileSpecification)]):
                        .success(URL(string: text).flatMap { url in url.isFileURL ? NSAppleEventDescriptor(fileURL: url) : nil } ?? NSAppleEventDescriptor(string: text))
                    default:
                        .success(NSAppleEventDescriptor(string: text))
                }
            case .list(let items):
                return traverse(items) { item in item.descriptor(as: types, in: dictionary) }.map { descriptors in
                    descriptors.reduce(into: .list()) { list, descriptor in list.insert(descriptor, at: list.numberOfItems + 1) }
                }
            case .record(let fields):
                return dictionary.record(fields, in: dictionary.scope(fields["class"].flatMap(dictionary.classCode(of:))), into: .record())
            case .type(let name):
                return ScriptingDictionary.resolve(name, in: [dictionary.classes, dictionary.properties]).map { term in NSAppleEventDescriptor(typeCode: term.code) }
            case .enumerator(let name):
                return ScriptingDictionary.resolve(name, in: [dictionary.enumerators(of: types), dictionary.enumerations.flatMap(\.members)]).map { term in NSAppleEventDescriptor(enumCode: term.code)
                }
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
                            Ordinal(rawValue: text).flatMap { ordinal in
                                NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: ordinal.code).data)
                            }
                            .map(Result.success) ?? .failure(AggregateError(first: .unknownTerm(text)))
                        default:
                            seld.descriptor(as: [.primitive(.integer)], in: dictionary)
                    }
                return combine(ScriptingDictionary.resolve(want, in: [dictionary.classes, dictionary.properties]), key, from.descriptor(as: [], in: dictionary)).flatMap { want, key, container in
                    [
                        (AEKeyword(keyAEDesiredClass), NSAppleEventDescriptor(typeCode: want.code)),
                        (AEKeyword(keyAEKeyForm), NSAppleEventDescriptor(enumCode: form.code)),
                        (AEKeyword(keyAEKeyData), key),
                        (AEKeyword(keyAEContainer), container),
                    ]
                    .reduce(into: NSAppleEventDescriptor.record()) { record, item in record.setDescriptor(item.1, forKeyword: item.0) }
                    .coerce(toDescriptorType: DescType(typeObjectSpecifier)).map(Result.success) ?? .failure(AggregateError(first: .unencodable(DescType(typeObjectSpecifier))))
                }
            case .opaque(let type, let data, _):
                return NSAppleEventDescriptor(descriptorType: type, data: data).map(Result.success) ?? .failure(AggregateError(first: .unencodable(type)))
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ScriptingDictionary {
    func event(_ name: String, arguments: [String: Native], target: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
        command(name, taking: Set(arguments.keys)).flatMap { command in
            let event: NSAppleEventDescriptor = NSAppleEventDescriptor(
                eventClass: command.eventClass,
                eventID: command.eventID,
                targetDescriptor: target,
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            )
            event.setAttribute(.null(), forKeyword: AEKeyword(keySubjectAttr))
            return record(arguments, in: [command.parameters], into: event)
        }
    }

    func record(_ fields: [String: Native], in scopes: [[Term]], into descriptor: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
        traverse(fields.sorted { left, right in left.key < right.key }) { key, value in
            Self.resolve(key, in: scopes).flatMap { term in value.descriptor(as: term.types, in: self).map { encoded in (term.code, encoded) } }
        }
        .map { items in items.reduce(into: descriptor) { descriptor, item in descriptor.setDescriptor(item.1, forKeyword: item.0) } }
    }
}
