import CasePaths
import CoreServices
public import Foundation
import OrderedCollections
public import Results

// --- [TYPES] ---------------------------------------------------------------------------

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

// --- [OPERATIONS] ----------------------------------------------------------------------

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
                        try NSAppleEventDescriptor(
                            date: Result { try Date.ISO8601FormatStyle().parse(text) }.mapError { error in AggregateError(first: AppleEventFailure(subject: text, reason: .undated(error))) }.get()
                        )
                    default:
                        switch URL(string: text) {
                            case .some(let url) where url.isFileURL && !types.isDisjoint(with: ["file", "alias", "file specification"]): NSAppleEventDescriptor(fileURL: url)
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
                            switch Ordinal(rawValue: text) {
                                case .some(let ordinal):
                                    NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: ordinal.code).data).map(Result.success)
                                        ?? .failure(unencodable(DescType(typeAbsoluteOrdinal)))
                                case .none:
                                    .failure(
                                        AggregateError(
                                            first: AppleEventFailure(subject: text, reason: .unmatched(Unmatched(subject: "ordinal", query: text, candidates: Ordinal.allCases.map(\.rawValue))))
                                        )
                                    )
                            }
                        case (.index, .number(let value)):
                            Int(exactly: value).flatMap(NSAppleEventDescriptor.init(offset:)).map(Result.success) ?? .failure(unencodable(DescType(typeSInt32)))
                        default:
                            encoded(seld, as: ["integer"])
                    }
                let (desired, data, container): (ScriptingDictionary.Term, NSAppleEventDescriptor, NSAppleEventDescriptor) =
                    try combine(ScriptingDictionary.resolve(want, in: [dictionary.types, dictionary.properties]), key, encoded(from)).get()
                guard let specifier: NSAppleEventDescriptor = NSAppleEventDescriptor(want: desired.code, form: form.code, data: data, from: container) else {
                    throw unencodable(DescType(typeObjectSpecifier))
                }
                return specifier
            case .opaque(let type, let data, _):
                guard let descriptor: NSAppleEventDescriptor = NSAppleEventDescriptor(descriptorType: type, data: data) else { throw unencodable(type) }
                return descriptor
        }
    }
}

extension NSAppleEventDescriptor {
    convenience init?(want: DescType, form: DescType, data: NSAppleEventDescriptor, from container: NSAppleEventDescriptor) {
        guard var from: AEDesc = unsafe container.aeDesc?.pointee, var key: AEDesc = unsafe data.aeDesc?.pointee else { return nil }
        var made: AEDesc = unsafe AEDesc()
        guard unsafe CreateObjSpecifier(want, &from, form, &key, false, &made) == noErr else { return nil }
        unsafe self.init(aeDescNoCopy: &made)
    }

    convenience init?(offset: Int) {
        var made: AEDesc = unsafe AEDesc()
        guard unsafe CreateOffsetDescriptor(offset, &made) == noErr else { return nil }
        unsafe self.init(aeDescNoCopy: &made)
    }
}
