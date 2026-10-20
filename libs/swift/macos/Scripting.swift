public import AppKit
import Carbon.OpenScripting
import CoreServices

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

public enum KeyForm: String, Codable, Coded, Sendable {
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

    static func descriptor(named name: String) throws(AggregateError<DictionaryFailure>) -> NSAppleEventDescriptor {
        guard let ordinal: Self = Self(rawValue: name) else { throw AggregateError(first: .unmatched(Unmatched(subject: "ordinal", query: name, candidates: allCases.map(\.rawValue)))) }
        guard let descriptor: NSAppleEventDescriptor = NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: ordinal.code).data) else {
            throw AggregateError(first: .unencodable(DescType(typeAbsoluteOrdinal)))
        }
        return descriptor
    }
}

// --- [MODELS] --------------------------------------------------------------------------

public enum SdefType: Hashable, Sendable {
    public enum Primitive: String, Sendable {
        case any
        case text
        case integer
        case real
        case number
        case boolean
        case specifier
        case locationSpecifier = "location specifier"
        case record
        case date
        case file
        case alias
        case fileSpecification = "file specification"
        case point
        case rectangle
        case type
        case missingValue = "missing value"
    }

    case primitive(Primitive)
    case declared(String)

    init(_ name: String) {
        self = Primitive(rawValue: name).map(Self.primitive) ?? .declared(name)
    }
}

public struct Term: Hashable, Sendable {
    public let name: String
    public let code: FourCharCode
    public let types: Set<SdefType>
    public let members: [Self]

    static func direct(_ types: Set<SdefType>) -> Self {
        Self(name: "direct", code: AEKeyword(keyDirectObject), types: types, members: [])
    }
}

public struct Command: Hashable, Sendable {
    static let language: [Self] = [
        Self(name: "activate", eventClass: AEEventClass(kAEMiscStandards), eventID: AEEventID(kAEActivate), parameters: []),
        Self(name: "get", eventClass: AEEventClass(kAECoreSuite), eventID: AEEventID(kAEGetData), parameters: [.direct([.primitive(.specifier)])]),
        Self(
            name: "set",
            eventClass: AEEventClass(kAECoreSuite),
            eventID: AEEventID(kAESetData),
            parameters: [.direct([.primitive(.specifier)]), Term(name: "to", code: AEKeyword(keyAEData), types: [.primitive(.any)], members: [])],
        ),
    ]

    public let name: String
    public let eventClass: AEEventClass
    public let eventID: AEEventID
    public let parameters: [Term]

    public var code: String { eventClass.sdef + eventID.sdef }
}

public struct ScriptingDictionary: Sendable {
    public enum Kind: String, CaseIterable, Decodable, Sendable {
        case suite
        case command
        case `class`
        case recordType = "record-type"
        case valueType = "value-type"
        case enumeration
    }

    public struct Selection: Decodable, Sendable {
        public let kind: Kind
        public let name: String
    }

    public let commands: [Command]
    public let suites: [Term]
    public let classes: [Term]
    public let recordTypes: [Term]
    public let valueTypes: [Term]
    public let enumerations: [Term]
    public let properties: [Term]
    var types: [Term] { classes + recordTypes + valueTypes }
    private let definition: Data

    public init(definition document: XMLDocument) throws(DefinitionFailure) {
        func elements(_ path: String) throws(DefinitionFailure) -> [XMLElement] {
            try Result { try document.nodes(forXPath: path).compactMap { node in node as? XMLElement } }.mapError(DefinitionFailure.xml).get()
        }
        let named: [String: [XMLElement]] = try Dictionary(
            elements("//class | //record-type | //value-type").compactMap { element in element.attribute(forName: "name")?.stringValue.map { name in (name, [element]) } },
            uniquingKeysWith: +,
        )
        func inherited(_ element: XMLElement) -> [XMLElement] {
            element.elements(forName: "property") + (element.attribute(forName: "inherits")?.stringValue.flatMap { name in named[name] } ?? []).flatMap(inherited)
        }
        func sdefTypes(_ element: XMLElement) -> Set<SdefType> {
            Set(
                ([element.attribute(forName: "type")] + element.elements(forName: "type").map { type in type.attribute(forName: "type") }).compactMap { attribute in
                    attribute?.stringValue.map(SdefType.init)
                }
            )
        }
        func terms(_ elements: [XMLElement]) -> [Term] {
            Set(
                elements.compactMap { element in
                    let members: [XMLElement] =
                        switch element.name.flatMap(Kind.init(rawValue:)) {
                            case .class, .recordType, .valueType: inherited(element)
                            case .enumeration: element.elements(forName: "enumerator")
                            case .suite, .command, .none: []
                        }
                    return switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap(FourCharCode.init(sdef:))) {
                        case (.some(let name), .some(let code)): Term(name: name, code: code, types: sdefTypes(element), members: terms(members))
                        default: nil
                    }
                }
            ).sorted { left, right in (left.name, left.code) < (right.name, right.code) }
        }
        func declared(_ kind: Kind) throws(DefinitionFailure) -> [Term] {
            try terms(elements("//\(kind.rawValue)"))
        }
        commands = try Set(
            elements("//\(Kind.command.rawValue)").compactMap { element in
                let code: String? = element.attribute(forName: "code")?.stringValue
                return
                    switch (
                        element.attribute(forName: "name")?.stringValue, code.flatMap { code in FourCharCode(sdef: String(code.prefix(4))) },
                        code.flatMap { code in FourCharCode(sdef: String(code.dropFirst(4))) },
                    )
                {
                    case (.some(let name), .some(let eventClass), .some(let eventID)):
                        Command(
                            name: name,
                            eventClass: eventClass,
                            eventID: eventID,
                            parameters: terms(element.elements(forName: "parameter")) + element.elements(forName: "direct-parameter").map { parameter in Term.direct(sdefTypes(parameter)) },
                        )
                    default: nil
                }
            }
        ).sorted { left, right in (left.name, left.code) < (right.name, right.code) }
        suites = try declared(.suite)
        classes = try declared(.class)
        recordTypes = try declared(.recordType)
        valueTypes = try declared(.valueType)
        enumerations = try declared(.enumeration)
        properties = try terms(elements("//property"))
        definition = document.xmlData
    }

    public func text(selecting selection: Selection?) -> Result<String, DefinitionFailure> {
        let query: String = """
            let $suites := for $suite in /dictionary/suite
                let $picked := if ($kind = "suite") then (for $child in $suite[@name = $name]/* return element { name($child) } { $child/@* }) else $suite/*[name() = $kind][@name = $name]
                where count($kind) = 0 or exists($picked) or ($kind = "suite" and $suite/@name = $name)
                return <suite>{ $suite/@*, $picked }</suite>
            return if (count($kind) = 0 or exists($suites)) then <dictionary>{ /dictionary/@*, $suites }</dictionary> else ()
            """
        let constants: [String: Any] = selection.map { selection in ["kind": selection.kind.rawValue, "name": selection.name] } ?? ["kind": [String](), "name": [String]()]
        return Result { () throws(DefinitionFailure) in
            let document: XMLDocument = try Result { try XMLDocument(data: definition) }.mapError(DefinitionFailure.xml).get()
            let selected: [XMLNode] = try Result { try document.objects(forXQuery: query, constants: constants) }.mapError(DefinitionFailure.xml).get().compactMap { node in node as? XMLNode }
            if let selection, selected.isEmpty {
                let names: [XMLNode] = try Result { try document.nodes(forXPath: "//\(selection.kind.rawValue)/@name") }.mapError(DefinitionFailure.xml).get()
                throw .unmatched(Unmatched(subject: selection.kind.rawValue, query: selection.name, candidates: Set(names.compactMap(\.stringValue)).sorted()))
            }
            return selected.map(\.xmlString).joined()
        }
    }

    public func command(_ name: String, taking arguments: Set<String>) -> Result<Command, AggregateError<DictionaryFailure>> {
        let scopes: [[Command]] = [Command.language, commands].map { scope in scope.filter { command in arguments.isSubset(of: command.parameters.map(\.name)) } }
        let found: [Command] = Self.matches(in: scopes) { command in command.name == name || command.code == name }
        let subject: String = arguments.isEmpty ? "command" : "command taking \(arguments.sorted().joined(separator: ", "))"
        return switch (found.first, found.count) {
            case (.some(let command), 1): .success(command)
            case (.none, _): .failure(AggregateError(first: .unmatched(Unmatched(subject: subject, query: name, candidates: Set(scopes.joined().map(\.name)).sorted()))))
            case (.some, _): .failure(AggregateError(first: .unmatched(Unmatched(subject: "single command", query: name, candidates: found.map(\.code)))))
        }
    }

    public func event(_ name: String, arguments: [String: AppleEventValue], target: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
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

    func record(_ fields: [String: AppleEventValue], in scopes: [[Term]], into descriptor: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
        traverse(fields.sorted { left, right in left.key < right.key }) { key, value in
            Self.resolve(key, in: scopes).flatMap { term in value.descriptor(as: term.types, in: self).map { encoded in (term.code, encoded) } }
        }
        .map { items in items.reduce(into: descriptor) { descriptor, item in descriptor.setDescriptor(item.1, forKeyword: item.0) } }
    }

    static func matches<Element>(in scopes: [[Element]], where predicate: (Element) -> Bool) -> [Element] {
        scopes.first { scope in scope.contains(where: predicate) }?.filter(predicate) ?? []
    }

    static func resolve(_ name: String, in scopes: [[Term]]) -> Result<Term, AggregateError<DictionaryFailure>> {
        let named: [Term] = matches(in: scopes) { term in term.name == name }
        let codes: [FourCharCode] = named.isEmpty ? [FourCharCode(sdef: name)].compactMap(\.self) : Set(named.map(\.code)).sorted()
        return switch (codes.first, codes.count) {
            case (.some(let code), 1): .success(Term(name: name, code: code, types: Set(named.flatMap(\.types)), members: []))
            case (.none, _): .failure(AggregateError(first: .unmatched(Unmatched(subject: "term", query: name, candidates: Set(scopes.joined().map(\.name)).sorted()))))
            case (.some, _): .failure(AggregateError(first: .unmatched(Unmatched(subject: "single term", query: name, candidates: codes.map(\.sdef)))))
        }
    }

    static func term(_ code: FourCharCode, in scopes: [[Term]]) -> Term {
        let found: [Term] = matches(in: scopes) { term in term.code == code }
        let name: String? = found.map(\.name).filter { name in if case .success(let term) = resolve(name, in: scopes) { term.code == code } else { false } }.min()
        return Term(name: name ?? code.sdef, code: code, types: Set(found.flatMap(\.types)), members: [])
    }

    func scope(_ code: FourCharCode?) -> [[Term]] {
        [types.filter { term in term.code == code }.flatMap(\.members), properties]
    }

    func classCode(of value: AppleEventValue) -> FourCharCode? {
        switch value {
            case .text(let name), .type(let name), .specifier(want: let name, form: _, seld: _, from: _):
                if case .success(let term) = Self.resolve(name, in: [types]) { term.code } else { nil }
            case .null: FourCharCode(cApplication)
            case .bool, .number, .list, .record, .enumerator, .date, .file, .opaque: nil
        }
    }

    func enumerators(of types: Set<SdefType>) -> [Term] {
        enumerations.filter { enumeration in types.contains(.declared(enumeration.name)) }.flatMap(\.members)
    }
}

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
        func exact<Value: BinaryInteger & BitwiseCopyable>(_: Value.Type) -> Self {
            Double(exactly: unsafe descriptor.data.withUnsafeBytes { bytes in unsafe bytes.loadUnaligned(as: Value.self) }).map(Self.number) ?? opaque()
        }
        func specifier() -> Self? {
            guard let want: NSAppleEventDescriptor = descriptor.forKeyword(AEKeyword(keyAEDesiredClass)),
                let form: KeyForm = descriptor.forKeyword(AEKeyword(keyAEKeyForm)).flatMap({ form in KeyForm(code: form.enumCodeValue) }),
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
            let scope: [[Term]] = dictionary.scope(descriptor.forKeyword(AEKeyword(pClass))?.typeCodeValue ?? descriptor.descriptorType)
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
                case DescType(typeEnumerated): .enumerator(ScriptingDictionary.term(descriptor.enumCodeValue, in: [dictionary.enumerations.flatMap(\.members)]).name)
                case DescType(typeAbsoluteOrdinal): Ordinal(code: descriptor.enumCodeValue).map { ordinal in .text(ordinal.rawValue) } ?? opaque()
                case DescType(typeFileURL), DescType(typeAlias): descriptor.fileURLValue.map(Self.file) ?? opaque()
                case DescType(typeLongDateTime): descriptor.dateValue.map(Self.date) ?? opaque()
                case _ where descriptor.isRecordDescriptor: record()
                default: opaque()
            }
    }

    public func descriptor(as types: Set<SdefType>, in dictionary: ScriptingDictionary) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
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
                    default:
                        switch URL(string: text) {
                            case .some(let url) where url.isFileURL && !types.isDisjoint(with: [.primitive(.file), .primitive(.alias), .primitive(.fileSpecification)]):
                                .success(NSAppleEventDescriptor(fileURL: url))
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
                        case (.index, .text(let text)): Result { () throws(AggregateError<DictionaryFailure>) in try Ordinal.descriptor(named: text) }
                        default:
                            seld.descriptor(as: [.primitive(.integer)], in: dictionary)
                    }
                return Result { () throws(AggregateError<DictionaryFailure>) in
                    let (desired, data, container): (Term, NSAppleEventDescriptor, NSAppleEventDescriptor) =
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

// --- [ERRORS] --------------------------------------------------------------------------

public enum DefinitionFailure: CustomNSError, LocalizedError {
    case requiresRunning(URL)
    case launch(LaunchFailure)
    case osa(OSStatus)
    case xml(any Error)
    case unmatched(Unmatched)

    public var errorDescription: String? {
        switch self {
            case .requiresRunning(let url): "\(url.path(percentEncoded: false)) declares no scripting definition file, launch it to read its dynamic dictionary"
            case .launch(let error as any Error), .unmatched(let error as any Error): error.localizedDescription
            case .osa(let status): "Open Scripting read no scripting definition (\(status))"
            case .xml(let error): "The scripting definition is not readable XML: \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .launch(let error as any Error), .xml(let error): [NSUnderlyingErrorKey: error]
            case .requiresRunning, .osa, .unmatched: [:]
        }
    }
}

public enum DictionaryFailure: LocalizedError {
    case unmatched(Unmatched)
    case date(String)
    case unencodable(DescType)

    public var errorDescription: String? {
        switch self {
            case .unmatched(let miss): miss.errorDescription
            case .date(let text): "\(text) is not an ISO 8601 date"
            case .unencodable(let type): "No Apple Event descriptor of type \(type.sdef) holds the value"
        }
    }
}

public enum AppleEventFailure: LocalizedError {
    case undelivered(OSStatus)
    case unanswered(OSStatus)
    case replied(OSStatus, String?)

    public var errorDescription: String? {
        switch self {
            case .undelivered(OSStatus(errAEEventNotPermitted)):
                "macOS Automation does not let this process control the application, nothing ran: allow it under System Settings, Privacy & Security, Automation"
            case .undelivered(let status): "The application did not receive the event (\(status)), nothing ran"
            case .unanswered(let status): "The application did not answer the event (\(status)), its outcome in the application is unknown"
            case .replied(let status, let message): "\(message ?? "The application answered with an error") (\(status))"
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

public actor ScriptingDefinitions {
    private struct Held {
        let processes: Set<pid_t>
        let reading: Task<Result<ScriptingDictionary, DefinitionFailure>, Never>
    }

    private var held: [URL: Held] = [:]

    public init() {}

    public func dictionary(of installation: Installation, within deadline: Duration) async -> Result<ScriptingDictionary, DefinitionFailure> {
        let processes: Set<pid_t> = Set(installation.instances.map(\.pid))
        let reading: Task<Result<ScriptingDictionary, DefinitionFailure>, Never> =
            switch held[installation.url] {
                case .some(let hit) where hit.processes == processes: hit.reading
                case .some, .none:
                    Task { await installation.scriptingDefinition(within: deadline).flatMap { document in Result { () throws(DefinitionFailure) in try ScriptingDictionary(definition: document) } } }
            }
        held[installation.url] = Held(processes: processes, reading: reading)
        let read: Result<ScriptingDictionary, DefinitionFailure> = await reading.value
        if case .failure = read, held[installation.url]?.reading == reading { held[installation.url] = nil }
        return read
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public func scriptingDefinition(within deadline: Duration) async -> Result<XMLDocument, DefinitionFailure> {
        await Result { () async throws(DefinitionFailure) in
            guard !instances.isEmpty || Bundle(url: url)?.object(forInfoDictionaryKey: "OSAScriptingDefinition") != nil else { throw .requiresRunning(url) }
            for instance: NSRunningApplication in instances.compactMap({ instance in NSRunningApplication(processIdentifier: instance.pid) }) {
                try await instance.reaching(\.isFinishedLaunching, within: deadline).mapError(DefinitionFailure.launch).get()
            }
            var definition: Unmanaged<CFData>?
            let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(url as CFURL, 0, &definition)
            guard status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() else { throw .osa(status) }
            return try Result { try XMLDocument(data: data as Data, options: .documentXInclude) }.mapError(DefinitionFailure.xml).get()
        }
    }
}

extension NSAppleEventDescriptor {
    public func sent() -> Result<NSAppleEventDescriptor, AppleEventFailure> {
        let undelivered: Set<OSStatus> = [OSStatus(procNotFound), OSStatus(errAEEventNotPermitted)]
        var reply: AppleEvent = unsafe AppleEvent()
        let status: OSStatus = unsafe AESendMessage(aeDesc, &reply, AESendMode(kAEWaitReply | kAENeverInteract), kNoTimeOut)
        guard status == noErr else { return .failure(undelivered.contains(status) ? .undelivered(status) : .unanswered(status)) }
        let answer: NSAppleEventDescriptor = unsafe NSAppleEventDescriptor(aeDescNoCopy: &reply)
        return switch answer.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
            case .some(let code) where undelivered.contains(code): .failure(.undelivered(code))
            case .some(let code) where code != noErr: .failure(.replied(code, answer.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue))
            case .some, .none: .success(answer.paramDescriptor(forKeyword: AEKeyword(keyDirectObject)) ?? .null())
        }
    }
}

extension FourCharCode {
    init?(sdef text: String) {
        let code: FourCharCode = NSHFSTypeCodeFromFileType("'\(text)'")
        guard code != 0 else { return nil }
        self = code
    }

    public var sdef: String { String(NSFileTypeForHFSTypeCode(self).dropFirst().dropLast()) }
}
