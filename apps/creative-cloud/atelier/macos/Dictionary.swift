import Carbon.OpenScripting
import Foundation
import RasmMacOS

// --- [TYPES] ---------------------------------------------------------------------------

enum Kind: String, CaseIterable, Decodable, Sendable {
    case suite
    case command
    case `class`
    case recordType = "record-type"
    case valueType = "value-type"
    case enumeration
}

enum SdefType: Hashable, Sendable {
    enum Primitive: String, Sendable {
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

// --- [MODELS] --------------------------------------------------------------------------

struct Term: Hashable, Sendable {
    let name: String
    let code: FourCharCode
    let types: Set<SdefType>
    let members: [Self]

    static func direct(_ types: Set<SdefType>) -> Self {
        Self(name: "direct", code: AEKeyword(keyDirectObject), types: types, members: [])
    }
}

struct Command: Hashable, Sendable {
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

    let name: String
    let eventClass: AEEventClass
    let eventID: AEEventID
    let parameters: [Term]

    var code: String { eventClass.sdef + eventID.sdef }
}

struct Entry: Encodable, Sendable {
    let name: String
    let code: String
}

struct ScriptingDictionary: Sendable {
    struct Selection: Decodable, Sendable {
        let kind: Kind
        let name: String
    }

    let commands: [Command]
    let declarations: [Kind: [Term]]
    let properties: [Term]

    var classes: [Term] { [Kind.class, .recordType, .valueType].flatMap { kind in declarations[kind] ?? [] } }
    var enumerations: [Term] { declarations[.enumeration] ?? [] }

    init(definition: XMLDocument) throws(DefinitionFailure) {
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

    func terms(_ kind: Kind) -> [Entry] {
        switch kind {
            case .command: commands.map { command in Entry(name: command.name, code: command.code) }
            case .suite, .class, .recordType, .valueType, .enumeration: (declarations[kind] ?? []).map { term in Entry(name: term.name, code: term.code.sdef) }
        }
    }

    func command(_ name: String, taking arguments: Set<String>) -> Result<Command, AggregateError<DictionaryFailure>> {
        let found: [Command] =
            [Command.language, commands].lazy.map { scope in scope.filter { command in (command.name == name || command.code == name) && arguments.isSubset(of: command.parameters.map(\.name)) } }
            .first { scope in !scope.isEmpty } ?? []
        return switch (found.first, found.count) {
            case (.some(let command), 1): .success(command)
            case (.none, _): .failure(AggregateError(first: .unknownCommand(name, arguments.sorted())))
            case (.some, _): .failure(AggregateError(first: .ambiguous(name, found.map(\.code))))
        }
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

// --- [ERRORS] --------------------------------------------------------------------------

enum DictionaryFailure: LocalizedError {
    case unknownCommand(String, [String])
    case unknownTerm(String)
    case ambiguous(String, [String])
    case date(String)
    case unencodable(DescType)

    var errorDescription: String? {
        switch self {
            case .unknownCommand(let name, let parameters): "No command \(name) in the scripting dictionary takes the parameters \(parameters.joined(separator: ", "))"
            case .unknownTerm(let name): "No term \(name) in the scripting dictionary"
            case .ambiguous(let name, let codes): "\(name) names several codes, pass one of \(codes.joined(separator: ", "))"
            case .date(let text): "\(text) is not an ISO 8601 date"
            case .unencodable(let type): "No Apple Event descriptor of type \(type.sdef) holds the value"
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Dictionaries {
    private var held: [URL: (process: pid_t?, document: XMLDocument, dictionary: ScriptingDictionary)] = [:]

    private func read(_ installation: Installation) async -> Result<(document: XMLDocument, dictionary: ScriptingDictionary), DefinitionFailure> {
        switch held[installation.url] {
            case .some(let hit) where hit.process == installation.running?.pid: .success((hit.document, hit.dictionary))
            case .some, .none:
                await installation.scriptingDefinition()
                    .flatMap { document in Result { () throws(DefinitionFailure) in try (document, ScriptingDictionary(definition: document)) } }
                    .map { document, dictionary in
                        held[installation.url] = (installation.running?.pid, document, dictionary)
                        return (document, dictionary)
                    }
        }
    }

    func dictionary(of installation: Installation) async -> Result<ScriptingDictionary, DefinitionFailure> {
        await read(installation).map(\.dictionary)
    }

    func text(of installation: Installation, selecting selection: ScriptingDictionary.Selection?) async -> Result<String, AggregateError<Failure>> {
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
        return await read(installation)
            .aggregated(Failure.definition)
            .flatMap { document, _ in
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
                return kept.aggregated(Failure.definition).flatMap { suites in
                    switch (selection, suites.isEmpty) {
                        case (.some(let selection), true): .failure(AggregateError(first: .dictionary(.unknownTerm("\(selection.kind.rawValue) \(selection.name)"))))
                        case (_, _): .success(XMLDocument(rootElement: document.rootElement().map { root in shell(root, suites) }).xmlString)
                    }
                }
            }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension FourCharCode {
    init?(sdef text: String) {
        let code: FourCharCode = NSHFSTypeCodeFromFileType("'\(text)'")
        guard code != 0 else { return nil }
        self = code
    }

    var sdef: String { String(NSFileTypeForHFSTypeCode(self).dropFirst().dropLast()) }
}
