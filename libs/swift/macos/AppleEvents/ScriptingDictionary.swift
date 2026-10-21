import Carbon.OpenScripting
import CoreServices
public import Foundation
public import Results
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

public final class ScriptingDictionary: Encodable, Sendable {
    public enum Kind: String, CaseIterable, CodingKey, Decodable {
        case suite
        case command
        case `class`
        case recordType = "record-type"
        case valueType = "value-type"
        case enumeration
    }

    public struct Term: Encodable, Hashable, Sendable {
        public let name: String
        public let code: FourCharCode
        public let types: Set<String>
        public let members: [Self]

        static func direct(_ types: Set<String>) -> Self {
            Self(name: "direct", code: AEKeyword(keyDirectObject), types: types, members: [])
        }

        public func encode(to encoder: any Encoder) throws {
            try ["name": name, "code": code.sdef].encode(to: encoder)
        }
    }

    public struct Command: Encodable, Hashable, Sendable {
        static let language: [Self] = [
            Self(name: "get", eventClass: AEEventClass(kAECoreSuite), eventID: AEEventID(kAEGetData), parameters: [.direct(["specifier"])]),
            Self(
                name: "set",
                eventClass: AEEventClass(kAECoreSuite),
                eventID: AEEventID(kAESetData),
                parameters: [.direct(["specifier"]), Term(name: "to", code: AEKeyword(keyAEData), types: ["any"], members: [])],
            ),
        ]

        public let name: String
        public let eventClass: AEEventClass
        public let eventID: AEEventID
        public let parameters: [Term]

        public var code: UInt64 { UInt64(eventClass) << 32 | UInt64(eventID) }

        public func encode(to encoder: any Encoder) throws {
            try ["name": name, "code": code.sdef].encode(to: encoder)
        }
    }

    struct Scope: Sendable {
        let named: [String: [Term]]
        let coded: [FourCharCode: [Term]]

        init(_ terms: [Term]) {
            named = Dictionary(grouping: terms, by: \.name)
            coded = Dictionary(grouping: terms, by: \.code)
        }
    }

    public let commands: [Command]
    public let declarations: [Kind: [Term]]
    let types: Scope
    let members: [FourCharCode: Scope]
    let properties: Scope
    let enumerators: Scope
    let document: Mutex<XMLDocument>

    public init(definition data: Data) throws(DefinitionFailure) {
        let document: XMLDocument = try Result { try XMLDocument(data: data, options: .documentXInclude) }.mapError(DefinitionFailure.xml).get()
        func elements(_ path: String) throws(DefinitionFailure) -> [XMLElement] {
            try Result { try document.nodes(forXPath: path).compactMap { node in node as? XMLElement } }.mapError(DefinitionFailure.xml).get()
        }
        let typedKinds: [Kind] = [.class, .recordType, .valueType]
        let named: [String: [XMLElement]] = try Dictionary(
            elements(typedKinds.map { kind in "//\(kind.rawValue)" }.joined(separator: " | ")).compactMap { element in
                element.attribute(forName: "name")?.stringValue.map { name in (name, [element]) }
            },
            uniquingKeysWith: +,
        )
        let extensions: [String: [XMLElement]] = try Dictionary(
            elements("//class-extension").compactMap { element in element.attribute(forName: "extends")?.stringValue.map { name in (name, [element]) } },
            uniquingKeysWith: +,
        )
        func inherited(_ element: XMLElement) -> [XMLElement] {
            let name: String? = element.attribute(forName: "name")?.stringValue
            return element.elements(forName: "property") + (name.flatMap { name in extensions[name] } ?? []).flatMap { extended in extended.elements(forName: "property") }
                + (element.attribute(forName: "inherits")?.stringValue.flatMap { name in named[name] } ?? []).flatMap(inherited)
        }
        func sdefTypes(_ element: XMLElement) -> Set<String> {
            Set(([element.attribute(forName: "type")] + element.elements(forName: "type").map { type in type.attribute(forName: "type") }).compactMap { attribute in attribute?.stringValue })
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
        commands = try Set(
            elements("//\(Kind.command.rawValue)").compactMap { element in
                switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap(UInt64.init(sdef:))) {
                    case (.some(let name), .some(let code)):
                        Command(
                            name: name,
                            eventClass: AEEventClass(truncatingIfNeeded: code >> 32),
                            eventID: AEEventID(truncatingIfNeeded: code),
                            parameters: terms(element.elements(forName: "parameter"))
                                + element.elements(forName: "direct-parameter").map { parameter in Term.direct(sdefTypes(parameter)) },
                        )
                    default: nil
                }
            }
        ).sorted { left, right in (left.name, left.code) < (right.name, right.code) }
        let declared: [Kind: [Term]] = try Dictionary(
            uniqueKeysWithValues: Kind.allCases.filter { kind in kind != .command }.map { kind throws(DefinitionFailure) in (kind, terms(try elements("//\(kind.rawValue)"))) }
        )
        let typed: [Term] = typedKinds.flatMap { kind in declared[kind, default: []] }
        declarations = declared
        types = Scope(typed)
        members = Dictionary(grouping: typed, by: \.code).mapValues { terms in Scope(terms.flatMap(\.members)) }
        properties = try Scope(terms(elements("//property")))
        enumerators = Scope(declared[.enumeration, default: []].flatMap(\.members))
        self.document = Mutex(document)
    }

    public func encode(to encoder: any Encoder) throws {
        var listing: KeyedEncodingContainer<Kind> = encoder.container(keyedBy: Kind.self)
        try listing.encode(commands, forKey: .command)
        for kind: Kind in Kind.allCases where kind != .command { try listing.encode(declarations[kind, default: []], forKey: kind) }
    }

    public func text(of kind: Kind, named name: String) -> Result<String, DefinitionFailure> {
        let query: String = """
            let $suites := for $suite in /dictionary/suite
                let $picked := if ($kind = "suite") then (for $child in $suite[@name = $name]/* return element { name($child) } { $child/@* }) else $suite/*[name() = $kind][@name = $name]
                where exists($picked) or ($kind = "suite" and $suite/@name = $name)
                return <suite>{ $suite/@*, $picked }</suite>
            return if (exists($suites)) then <dictionary>{ /dictionary/@*, $suites }</dictionary> else ()
            """
        let listed: [String] = kind == .command ? commands.map(\.name) : declarations[kind, default: []].map(\.name)
        return document.withLock { document in
            Result { try document.objects(forXQuery: query, constants: ["kind": kind.rawValue, "name": name]) }
                .mapError(DefinitionFailure.xml)
                .flatMap { nodes in
                    let selected: [String] = nodes.compactMap { node in (node as? XMLNode)?.xmlString }
                    return selected.isEmpty ? .failure(.unmatched(Unmatched(subject: kind.rawValue, query: name, candidates: Set(listed).sorted()))) : .success(selected.joined())
                }
        }
    }

    public func command(_ name: String, taking arguments: Set<String>) -> Result<Command, DictionaryFailure> {
        let code: UInt64? = UInt64(sdef: name)
        let matching: (Command) -> Bool = { command in command.name == name || command.code == code }
        let scopes: [[Command]] = [Command.language, commands].map { scope in scope.filter { command in arguments.isSubset(of: command.parameters.map(\.name)) } }
        let found: [Command] = scopes.first { scope in scope.contains(where: matching) }?.filter(matching) ?? []
        let subject: String = arguments.isEmpty ? "command" : "command taking \(arguments.sorted().joined(separator: ", "))"
        return Unmatched.single(found, subject: subject, query: name, candidates: Set(scopes.joined().map(\.name)).sorted(), named: \.code.sdef).mapError(DictionaryFailure.unmatched)
    }

    public func event(_ command: Command, arguments: [String: AppleEventValue], target: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
        let event: NSAppleEventDescriptor = NSAppleEventDescriptor(
            eventClass: command.eventClass,
            eventID: command.eventID,
            targetDescriptor: target,
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        event.setAttribute(.null(), forKeyword: AEKeyword(keySubjectAttr))
        return record(arguments, in: [Scope(command.parameters)], into: event)
    }

    func record(_ fields: [String: AppleEventValue], in scopes: [Scope], into descriptor: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<DictionaryFailure>> {
        traverse(fields.sorted { left, right in left.key < right.key }) { key, value in
            Self.resolve(key, in: scopes).flatMap { term in value.descriptor(as: term.types, in: self).map { encoded in (term.code, encoded) } }
        }
        .map { items in items.reduce(into: descriptor) { descriptor, item in descriptor.setDescriptor(item.1, forKeyword: item.0) } }
    }

    static func resolve(_ name: String, in scopes: [Scope]) -> Result<Term, AggregateError<DictionaryFailure>> {
        let named: [Term] = scopes.lazy.compactMap { scope in scope.named[name] }.first ?? []
        let codes: [FourCharCode] = named.isEmpty ? [FourCharCode(sdef: name)].compactMap(\.self) : Set(named.map(\.code)).sorted()
        return Unmatched.single(codes, subject: "term", query: name, candidates: Set(scopes.flatMap(\.named.keys)).sorted(), named: \.sdef)
            .map { code in Term(name: name, code: code, types: Set(named.flatMap(\.types)), members: []) }
            .mapError { miss in AggregateError(first: .unmatched(miss)) }
    }

    static func term(_ code: FourCharCode, in scopes: [Scope]) -> Term {
        let found: [Term] = scopes.lazy.compactMap { scope in scope.coded[code] }.first ?? []
        let name: String? = found.map(\.name).filter { name in if case .success(let term) = resolve(name, in: scopes) { term.code == code } else { false } }.min()
        return Term(name: name ?? code.sdef, code: code, types: Set(found.flatMap(\.types)), members: [])
    }

    func scope(_ code: FourCharCode?) -> [Scope] {
        [code.flatMap { code in members[code] }, properties].compactMap(\.self)
    }

    func classCode(of value: AppleEventValue) -> FourCharCode? {
        switch value {
            case .text(let name), .type(let name), .specifier(want: let name, form: _, seld: _, from: _):
                if case .success(let term) = Self.resolve(name, in: [types]) { term.code } else { nil }
            case .null: FourCharCode(cApplication)
            case .bool, .number, .list, .record, .enumerator, .date, .file, .opaque: nil
        }
    }

    func enumerators(of types: Set<String>) -> Scope {
        Scope(declarations[.enumeration, default: []].filter { enumeration in types.contains(enumeration.name) }.flatMap(\.members))
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum DictionaryFailure: CustomNSError, LocalizedError {
    case unmatched(Unmatched)
    case unencodable(DescType)
    case undated(String, any Error)

    public var errorDescription: String? {
        switch self {
            case .unmatched(let miss): miss.errorDescription
            case .unencodable(let type): "No Apple Event descriptor of type \(type.sdef) holds the value"
            case .undated(let text, let error): "\(text) is no ISO 8601 date: \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .unmatched(let error as any Error), .undated(_, let error): [NSUnderlyingErrorKey: error]
            case .unencodable: [:]
        }
    }
}
