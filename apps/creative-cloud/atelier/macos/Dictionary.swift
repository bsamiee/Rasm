import Carbon.OpenScripting
import Foundation
import RasmMacOS
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

struct Term: Sendable {
    let name: String
    let code: FourCharCode
    var types: Set<String> = []
    var members: [Self] = []
}

struct ScriptingDictionary: Sendable {
    // --- [TERMS]

    struct Selection: Decodable {
        let kind: String
        let name: String
    }

    struct Declaration: Encodable, Hashable {
        let name: String
        let code: String
    }

    static let held: Mutex<[URL: (process: pid_t, dictionary: ScriptingDictionary)]> = Mutex([:])

    let commands: [(name: String, eventClass: AEEventClass, eventID: AEEventID, parameters: [Term])]
    let classes: [Term]
    let properties: [Term]
    let enumerations: [Term]

    init(definition: XMLDocument) throws {
        let elements: (String) throws -> [XMLElement] = { path in try definition.nodes(forXPath: path).compactMap { node in node as? XMLElement } }
        let declarations: [XMLElement] = try elements("//class | //record-type | //value-type")
        let named: [String: [XMLElement]] = Dictionary(
            declarations.compactMap { element in element.attribute(forName: "name")?.stringValue.map { name in (name, [element]) } },
            uniquingKeysWith: +,
        )
        func inherited(_ element: XMLElement) -> [XMLElement] {
            element.elements(forName: "property") + (element.attribute(forName: "inherits")?.stringValue.flatMap { name in named[name] } ?? []).flatMap(inherited)
        }
        let specifier: Term = Term(name: "direct", code: AEKeyword(keyDirectObject), types: ["specifier"])
        let language: [(name: String, eventClass: AEEventClass, eventID: AEEventID, parameters: [Term])] = [
            (name: "activate", eventClass: AEEventClass(kAEMiscStandards), eventID: AEEventID(kAEActivate), parameters: []),
            (name: "get", eventClass: AEEventClass(kAECoreSuite), eventID: AEEventID(kAEGetData), parameters: [specifier]),
            (name: "set", eventClass: AEEventClass(kAECoreSuite), eventID: AEEventID(kAESetData), parameters: [specifier, Term(name: "to", code: AEKeyword(keyAEData), types: ["any"])]),
        ]
        commands =
            try elements("//command").compactMap { element in
                guard let name: String = element.attribute(forName: "name")?.stringValue,
                    let code: String = element.attribute(forName: "code")?.stringValue,
                    let eventClass: AEEventClass = Self.code("'\(code.prefix(4))'"),
                    let eventID: AEEventID = Self.code("'\(code.dropFirst(4))'")
                else { return nil }
                let direct: [Term] = element.elements(forName: "direct-parameter").map { parameter in
                    Term(name: "direct", code: AEKeyword(keyDirectObject), types: Self.types(of: parameter))
                }
                return (
                    name: name,
                    eventClass: eventClass,
                    eventID: eventID,
                    parameters: Self.terms(element.elements(forName: "parameter")) + direct,
                )
            } + language
        classes = Self.terms(declarations) { element in Self.terms(inherited(element)) }
        properties = try Self.terms(elements("//property"))
        enumerations = try Self.terms(elements("//enumeration")) { element in Self.terms(element.elements(forName: "enumerator")) }
    }

    static func read(_ application: Application, process: pid_t) -> Result<Self, AggregateError<Failure>> {
        held.withLock { held in held[application.url] }.flatMap { hit in hit.process == process ? Result.success(hit.dictionary) : nil }
            ?? definition(at: application.url)
            .flatMap { definition in Result { try Self(definition: definition) }.mapError { error in AggregateError(first: .unreadable(error)) } }
            .map { dictionary in
                held.withLock { held in held[application.url] = (process, dictionary) }
                return dictionary
            }
    }

    static func definition(of application: Application) async -> Result<XMLDocument, AggregateError<Failure>> {
        let readable: Result<Void, AggregateError<Failure>> =
            if application.running != nil {
                await application.open().map { _ in }
            } else if Bundle(url: application.url)?.object(forInfoDictionaryKey: "OSAScriptingDefinition") != nil {
                .success(())
            } else {
                .failure(AggregateError(first: .dictionaryUnavailable))
            }
        return readable.flatMap { definition(at: application.url) }
    }

    static func definition(at url: URL) -> Result<XMLDocument, AggregateError<Failure>> {
        var definition: Unmanaged<CFData>?
        let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(url as CFURL, 0, &definition)
        return if status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() {
            Result { try XMLDocument(data: data as Data, options: .documentXInclude) }.mapError { error in AggregateError(first: .unreadable(error)) }
        } else {
            .failure(AggregateError(first: .undefined(status: status)))
        }
    }

    static func definition(_ document: XMLDocument, selecting selection: Selection?) -> Result<String, AggregateError<Failure>> {
        let suites: [XMLElement] = document.rootElement()?.elements(forName: "suite") ?? []
        let selected: [XMLNode] =
            selection.map { selection in
                suites.flatMap { suite in [suite] + (suite.children?.compactMap { node in node as? XMLElement } ?? []) }
                    .filter { node in node.name == selection.kind && node.attribute(forName: "name")?.stringValue == selection.name }
            } ?? suites
        if let selection, selected.isEmpty { return .failure(AggregateError(first: .unknownTerm(name: "\(selection.kind)/\(selection.name)"))) }
        for suite: XMLElement in suites {
            switch selection {
                case .none: suite.setChildren(nil)
                case .some where selected.contains(suite):
                    for child: XMLNode in suite.children ?? [] { (child as? XMLElement)?.setChildren(nil) }
                case .some:
                    for child: XMLNode in suite.children ?? [] where !selected.contains(child) { child.detach() }
                    if suite.childCount == 0 { suite.detach() }
            }
        }
        return .success(document.xmlString)
    }

    static func terms(_ document: XMLDocument, kind: String) -> Result<[Declaration], AggregateError<Failure>> {
        Result {
            let declarations: [XMLElement] = try document.nodes(forXPath: "/dictionary/suite/\(kind)[@name]").compactMap { node in node as? XMLElement }
            return Set(
                declarations.compactMap { node in
                    switch (node.attribute(forName: "name")?.stringValue, node.attribute(forName: "code")?.stringValue) {
                        case (.some(let name), .some(let code)): Declaration(name: name, code: code)
                        default: nil
                    }
                }
            ).sorted { left, right in left.name < right.name }
        }.mapError { error in AggregateError(first: .unreadable(error)) }
    }

    static func code(_ text: String) -> FourCharCode? {
        let code: FourCharCode = NSHFSTypeCodeFromFileType(text)
        return code == 0 ? nil : code
    }

    static func types(of element: XMLElement) -> Set<String> {
        Set(([element.attribute(forName: "type")] + element.elements(forName: "type").map { type in type.attribute(forName: "type") }).compactMap { attribute in attribute?.stringValue })
    }

    static func terms(_ elements: [XMLElement], members: (XMLElement) -> [Term] = { _ in [] }) -> [Term] {
        elements.compactMap { element in
            switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap { code in Self.code("'\(code)'") }) {
                case (.some(let name), .some(let code)): Term(name: name, code: code, types: types(of: element), members: members(element))
                default: nil
            }
        }
    }

    static func matches(in scopes: [[Term]], where predicate: (Term) -> Bool) -> [Term] {
        scopes.first { scope in scope.contains(where: predicate) }?.filter(predicate) ?? []
    }

    static func resolve(_ name: String, in scopes: [[Term]]) -> Result<Term, AggregateError<Failure>> {
        let raw: FourCharCode? = code(name)
        let found: [Term] = matches(in: scopes) { term in raw.map { code in term.code == code } ?? (term.name == name) }
        let codes: [FourCharCode] = raw.map { code in [code] } ?? Set(found.map(\.code)).sorted()
        return switch (codes.first, codes.count) {
            case (.some(let code), 1): .success(Term(name: name, code: code, types: Set(found.flatMap(\.types))))
            case (.none, _): .failure(AggregateError(first: .unknownTerm(name: name)))
            case (.some, _): .failure(AggregateError(first: .ambiguousTerm(name: name, codes: codes.compactMap(NSFileTypeForHFSTypeCode))))
        }
    }

    static func term(_ code: FourCharCode, in scopes: [[Term]]) -> Term {
        let found: [Term] = matches(in: scopes) { term in term.code == code }
        let name: String? = found.map(\.name).filter { name in if case .success(let term) = resolve(name, in: scopes) { term.code == code } else { false } }.min()
        return Term(name: name ?? NSFileTypeForHFSTypeCode(code), code: code, types: Set(found.flatMap(\.types)))
    }

    func scope(_ code: FourCharCode?) -> [[Term]] {
        [classes.filter { term in term.code == code }.flatMap(\.members), properties]
    }

    func classCode(_ want: JSONValue?) -> FourCharCode? {
        switch want {
            case .string(let name):
                if case .success(let term) = Self.resolve(name, in: [classes]) { term.code } else { nil }
            case .object(let fields) where fields["type"] == .string(NSFileTypeForHFSTypeCode(DescType(typeType))):
                if case .success(let descriptor) = encode(.object(fields), as: []) { descriptor.typeCodeValue } else { nil }
            default: nil
        }
    }

    func enumerators(of types: Set<String>) -> [Term] {
        enumerations.filter { enumeration in types.contains(enumeration.name) }.flatMap(\.members)
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: LocalizedError {
    case notApplication(URL)
    case notRunning(URL)
    case element(ElementFailure)
    case dictionaryUnavailable
    case unreadable(any Error)
    case undefined(status: OSStatus)
    case absentCommand(name: String)
    case unknownTerm(name: String)
    case ambiguousTerm(name: String, codes: [String])
    case unspecified
    case undelivered(status: OSStatus)
    case unanswered(status: OSStatus)
    case reply(status: OSStatus, message: String?)

    var errorDescription: String? {
        switch self {
            case .notApplication(let url): "No Adobe application at \(url.absoluteString)"
            case .notRunning(let url): "No running instance of \(url.absoluteString), launch it first"
            case .element(let error): error.localizedDescription
            case .dictionaryUnavailable: "The application must be running to read its dynamic scripting dictionary"
            case .unreadable(let error): error.localizedDescription
            case .undefined(let status): "The application gave no scripting dictionary (\(status))"
            case .absentCommand(let name): "No command \(name) in the scripting dictionary"
            case .unknownTerm(let name): "No term \(name) in the scripting dictionary"
            case .ambiguousTerm(let name, let codes): "Term \(name) names several codes, pass one of \(codes.joined(separator: ", "))"
            case .unspecified: "The supplied record does not make an Apple Event descriptor"
            case .undelivered(OSStatus(errAEEventNotPermitted)):
                "macOS Automation does not let Atelier control the application, nothing ran: allow it under System Settings, Privacy & Security, Automation"
            case .undelivered(let status): "The application did not receive the event (\(status)), nothing ran"
            case .unanswered(let status): "The application did not answer the event (\(status)), its outcome in the application is unknown"
            case .reply(let status, let message): "\(message ?? "The application answered with an error") (\(status))"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func put(_ items: [(keyword: AEKeyword, value: NSAppleEventDescriptor)], into descriptor: NSAppleEventDescriptor) -> NSAppleEventDescriptor {
    items.reduce(into: descriptor) { descriptor, item in descriptor.setDescriptor(item.value, forKeyword: item.keyword) }
}
