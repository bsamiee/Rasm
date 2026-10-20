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

    public struct Entry: Encodable, Sendable {
        public let name: String
        public let code: String
    }

    public let commands: [Command]
    public let declarations: [Kind: [Term]]
    public let properties: [Term]

    public var classes: [Term] { [Kind.class, .recordType, .valueType].flatMap { kind in declarations[kind] ?? [] } }
    public var enumerations: [Term] { declarations[.enumeration] ?? [] }

    public init(definition: XMLDocument) throws(DefinitionFailure) {
        func elements(_ path: String) throws(DefinitionFailure) -> [XMLElement] {
            try Result { try definition.nodes(forXPath: path).compactMap { node in node as? XMLElement } }.mapError(DefinitionFailure.xml).get()
        }
        let named: [String: [XMLElement]] = try Dictionary(
            elements("//class | //record-type | //value-type").compactMap { element in element.attribute(forName: "name")?.stringValue.map { name in (name, [element]) } },
            uniquingKeysWith: +,
        )
        func inherited(_ element: XMLElement) -> [XMLElement] {
            element.elements(forName: "property") + (element.attribute(forName: "inherits")?.stringValue.flatMap { name in named[name] } ?? []).flatMap(inherited)
        }
        func types(_ element: XMLElement) -> Set<SdefType> {
            Set(
                ([element.attribute(forName: "type")] + element.elements(forName: "type").map { type in type.attribute(forName: "type") }).compactMap { attribute in
                    attribute?.stringValue.map(SdefType.init)
                }
            )
        }
        func terms(_ elements: [XMLElement], members: (XMLElement) -> [Term]) -> [Term] {
            Set(
                elements.compactMap { element in
                    switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap(FourCharCode.init(sdef:))) {
                        case (.some(let name), .some(let code)): Term(name: name, code: code, types: types(element), members: members(element))
                        default: nil
                    }
                }
            ).sorted { left, right in (left.name, left.code) < (right.name, right.code) }
        }
        commands = try Set(
            elements("//command").compactMap { element in
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
                            parameters: terms(element.elements(forName: "parameter")) { _ in [] }
                                + element.elements(forName: "direct-parameter").map { parameter in Term.direct(types(parameter)) },
                        )
                    default: nil
                }
            }
        ).sorted { left, right in (left.name, left.code) < (right.name, right.code) }
        declarations = try Dictionary(
            uniqueKeysWithValues: Kind.allCases.filter { kind in kind != .command }.map { kind throws(DefinitionFailure) in
                try (
                    kind,
                    terms(elements("//\(kind.rawValue)")) { element in
                        switch kind {
                            case .class, .recordType, .valueType: terms(inherited(element)) { _ in [] }
                            case .enumeration: terms(element.elements(forName: "enumerator")) { _ in [] }
                            case .suite, .command: []
                        }
                    },
                )
            }
        )
        properties = try terms(elements("//property")) { _ in [] }
    }

    public func terms(_ kind: Kind) -> [Entry] {
        switch kind {
            case .command: commands.map { command in Entry(name: command.name, code: command.code) }
            case .suite, .class, .recordType, .valueType, .enumeration: (declarations[kind] ?? []).map { term in Entry(name: term.name, code: term.code.sdef) }
        }
    }

    public func command(_ name: String, taking arguments: Set<String>) -> Result<Command, AggregateError<DictionaryFailure>> {
        let found: [Command] =
            [Command.language, commands].lazy.map { scope in scope.filter { command in (command.name == name || command.code == name) && arguments.isSubset(of: command.parameters.map(\.name)) } }
            .first { scope in !scope.isEmpty } ?? []
        return switch (found.first, found.count) {
            case (.some(let command), 1): .success(command)
            case (.none, _): .failure(AggregateError(first: .unknownCommand(name, arguments.sorted())))
            case (.some, _): .failure(AggregateError(first: .ambiguous(name, found.map(\.code))))
        }
    }

    public func event(_ name: String, arguments: [String: Native], target: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
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

    static func matches(in scopes: [[Term]], where predicate: (Term) -> Bool) -> [Term] {
        scopes.first { scope in scope.contains(where: predicate) }?.filter(predicate) ?? []
    }

    static func resolve(_ name: String, in scopes: [[Term]]) -> Result<Term, AggregateError<DictionaryFailure>> {
        let named: [Term] = matches(in: scopes) { term in term.name == name }
        let codes: [FourCharCode] = named.isEmpty ? [FourCharCode(sdef: name)].compactMap(\.self) : Set(named.map(\.code)).sorted()
        return switch (codes.first, codes.count) {
            case (.some(let code), 1): .success(Term(name: name, code: code, types: Set(named.flatMap(\.types)), members: []))
            case (.none, _): .failure(AggregateError(first: .unknownTerm(name)))
            case (.some, _): .failure(AggregateError(first: .ambiguous(name, codes.map(\.sdef))))
        }
    }

    static func term(_ code: FourCharCode, in scopes: [[Term]]) -> Term {
        let found: [Term] = matches(in: scopes) { term in term.code == code }
        let name: String? = found.map(\.name).filter { name in if case .success(let term) = resolve(name, in: scopes) { term.code == code } else { false } }.min()
        return Term(name: name ?? code.sdef, code: code, types: Set(found.flatMap(\.types)), members: [])
    }

    func scope(_ code: FourCharCode?) -> [[Term]] {
        [classes.filter { term in term.code == code }.flatMap(\.members), properties]
    }

    func classCode(of value: Native) -> FourCharCode? {
        switch value {
            case .text(let name), .type(let name), .specifier(want: let name, form: _, seld: _, from: _):
                if case .success(let term) = Self.resolve(name, in: [classes]) { term.code } else { nil }
            case .null: FourCharCode(cApplication)
            case .bool, .number, .list, .record, .enumerator, .date, .file, .opaque: nil
        }
    }

    func enumerators(of types: Set<SdefType>) -> [Term] {
        enumerations.filter { enumeration in types.contains(.declared(enumeration.name)) }.flatMap(\.members)
    }
}

public indirect enum Native: Sendable {
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

    public init(from decoder: any Decoder) throws {
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
    public init(_ descriptor: NSAppleEventDescriptor, in dictionary: ScriptingDictionary) {
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

// --- [ERRORS] --------------------------------------------------------------------------

public enum DefinitionFailure: LocalizedError {
    case requiresRunning(URL)
    case launch(LaunchFailure)
    case osa(OSStatus)
    case xml(any Error)
    case unselected(ScriptingDictionary.Selection)

    public var errorDescription: String? {
        switch self {
            case .requiresRunning(let url): "\(url.path(percentEncoded: false)) declares no scripting definition file, launch it to read its dynamic dictionary"
            case .launch(let error): error.localizedDescription
            case .osa(let status): "Open Scripting read no scripting definition (\(status))"
            case .xml(let error): "The scripting definition is not readable XML: \(error.localizedDescription)"
            case .unselected(let selection): "The scripting definition declares no \(selection.kind.rawValue) named \(selection.name)"
        }
    }
}

public enum DictionaryFailure: LocalizedError {
    case unknownCommand(String, [String])
    case unknownTerm(String)
    case ambiguous(String, [String])
    case date(String)
    case unencodable(DescType)

    public var errorDescription: String? {
        switch self {
            case .unknownCommand(let name, let parameters): "No command \(name) in the scripting dictionary takes the parameters \(parameters.joined(separator: ", "))"
            case .unknownTerm(let name): "No term \(name) in the scripting dictionary"
            case .ambiguous(let name, let codes): "\(name) names several codes, pass one of \(codes.joined(separator: ", "))"
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
    private var held: [URL: (process: pid_t?, document: XMLDocument, dictionary: ScriptingDictionary)] = [:]

    public init() {}

    private func read(_ installation: Installation, within deadline: Duration) async -> Result<(document: XMLDocument, dictionary: ScriptingDictionary), DefinitionFailure> {
        switch held[installation.url] {
            case .some(let hit) where hit.process == installation.running?.pid: .success((hit.document, hit.dictionary))
            case .some, .none:
                await installation.scriptingDefinition(within: deadline)
                    .flatMap { document in Result { () throws(DefinitionFailure) in try (document, ScriptingDictionary(definition: document)) } }
                    .map { document, dictionary in
                        held[installation.url] = (installation.running?.pid, document, dictionary)
                        return (document, dictionary)
                    }
        }
    }

    public func dictionary(of installation: Installation, within deadline: Duration) async -> Result<ScriptingDictionary, DefinitionFailure> {
        await read(installation, within: deadline).map(\.dictionary)
    }

    public func text(of installation: Installation, selecting selection: ScriptingDictionary.Selection?, within deadline: Duration) async -> Result<String, DefinitionFailure> {
        func shell(_ element: XMLElement, _ children: [XMLElement]) -> XMLElement {
            let made: XMLElement = XMLElement(kind: .element)
            made.name = element.name
            made.setAttributesWith(
                Dictionary((element.attributes ?? []).compactMap { attribute in attribute.name.flatMap { name in attribute.stringValue.map { value in (name, value) } } }) { first, _ in first }
            )
            made.setChildren(children)
            return made
        }
        func named(_ element: XMLElement) -> Bool {
            element.attribute(forName: "name")?.stringValue == selection?.name
        }
        return await read(installation, within: deadline).flatMap { document, _ in
            let suites: [XMLElement] = document.rootElement()?.elements(forName: "suite") ?? []
            let kept: Result<[XMLElement], DefinitionFailure> =
                switch selection?.kind {
                    case .none: .success(suites.map { suite in shell(suite, []) })
                    case .some(.suite):
                        .success(suites.filter(named).map { suite in shell(suite, (suite.children ?? []).compactMap { node in node as? XMLElement }.map { child in shell(child, []) }) })
                    case .some(let kind):
                        Result {
                            try suites.map { suite in try (suite, suite.elements(forName: kind.rawValue).filter(named).map { element in try XMLElement(xmlString: element.xmlString) }) }
                                .filter { _, children in !children.isEmpty }
                                .map(shell)
                        }
                        .mapError(DefinitionFailure.xml)
                }
            return kept.flatMap { suites in
                switch (selection, suites.isEmpty) {
                    case (.some(let selection), true): .failure(.unselected(selection))
                    case (_, _): .success(XMLDocument(rootElement: document.rootElement().map { root in shell(root, suites) }).xmlString)
                }
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    public func scriptingDefinition(within deadline: Duration) async -> Result<XMLDocument, DefinitionFailure> {
        await Result { () async throws(DefinitionFailure) in
            switch (runningApplication, Bundle(url: url)?.object(forInfoDictionaryKey: "OSAScriptingDefinition")) {
                case (.success(let running), _): try await running.reaching(\.isFinishedLaunching, within: deadline).mapError(DefinitionFailure.launch).get()
                case (.failure, .some): break
                case (.failure, .none): throw .requiresRunning(url)
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

    var sdef: String { String(NSFileTypeForHFSTypeCode(self).dropFirst().dropLast()) }
}
