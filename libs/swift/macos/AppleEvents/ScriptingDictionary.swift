import Carbon.OpenScripting
import CasePathsCore
public import CoreServices
import Encoding
public import Foundation
import OrderedCollections
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
        public let list: Bool
        public let members: [Self]

        static func direct(_ type: (types: Set<String>, list: Bool)) -> Self {
            Self(name: "direct", code: AEKeyword(keyDirectObject), types: type.types, list: type.list, members: [])
        }

        var isDirect: Bool { code == AEKeyword(keyDirectObject) }

        var acceptsText: Bool { !types.isDisjoint(with: ["text", "any"]) }

        private enum Key: CodingKey {
            case name, code
        }

        public func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: Key.self)
            try fields.encode(name, forKey: .name)
            try fields.encode(code.sdef, forKey: .code)
        }
    }

    public struct Command: Encodable, Hashable, Sendable {
        static let language: [Self] = [
            Self(name: "get", eventClass: AEEventClass(kAECoreSuite), eventID: AEEventID(kAEGetData), parameters: [.direct((["specifier"], false))]),
            Self(
                name: "set",
                eventClass: AEEventClass(kAECoreSuite),
                eventID: AEEventID(kAESetData),
                parameters: [.direct((["specifier"], false)), Term(name: "to", code: AEKeyword(keyAEData), types: ["any"], list: false, members: [])],
            ),
        ]

        public let name: String
        public let eventClass: AEEventClass
        public let eventID: AEEventID
        public let parameters: [Term]

        private enum Key: CodingKey {
            case name, code, parameters, list, enumerators
        }

        public var code: UInt64 { UInt64(eventClass) << 32 | UInt64(eventID) }

        public func encode(to encoder: any Encoder) throws {
            var row = encoder.container(keyedBy: Key.self)
            try row.encode(name, forKey: .name)
            try row.encode(code.sdef, forKey: .code)
            var listed = row.nestedUnkeyedContainer(forKey: .parameters)
            for parameter in parameters {
                var fields = listed.nestedContainer(keyedBy: Key.self)
                try fields.encode(parameter.name, forKey: .name)
                try fields.encode(parameter.code.sdef, forKey: .code)
                try fields.encode(parameter.list, forKey: .list)
                try fields.encode(parameter.members, forKey: .enumerators)
            }
        }
    }

    struct Scope: Sendable {
        let named: OrderedDictionary<String, [Term]>
        let coded: [FourCharCode: [Term]]

        init(_ terms: [Term]) {
            named = OrderedDictionary(grouping: terms, by: \.name)
            coded = Dictionary(grouping: terms, by: \.code)
        }
    }

    private enum Key: CodingKey {
        case script
    }

    public let commands: [Command]
    public let script: Command?
    public let declarations: [Kind: [Term]]
    let types: Scope
    let members: [FourCharCode: Scope]
    let properties: Scope
    let enumerators: Scope
    let document: Mutex<XMLDocument>

    public init(definition data: Data) throws(AppleEventFailure) {
        let document = try Result { try XMLDocument(data: data, options: .documentXInclude) }
            .mapError { error in AppleEventFailure(subject: "XML of \(data.count)-byte scripting definition", reason: .host(error as NSError)) }.get()
        func elements(_ path: String) throws(AppleEventFailure) -> [XMLElement] {
            try Result { try document.nodes(forXPath: path) }.mapError { error in AppleEventFailure(subject: "scripting definition XPath \(path)", reason: .host(error as NSError)) }.get()
                .compactMap { node in node as? XMLElement }
        }
        let typedKinds: [Kind] = [.class, .recordType, .valueType]
        let named = try Dictionary(grouping: elements(typedKinds.map { kind in "//\(kind.rawValue)" }.joined(separator: " | "))) { element in
            element.attribute(forName: "name")?.stringValue
        }
        let extensions = try Dictionary(grouping: elements("//class-extension")) { element in element.attribute(forName: "extends")?.stringValue }
        let enumerations = try Dictionary(grouping: elements("//\(Kind.enumeration.rawValue)")) { element in element.attribute(forName: "name")?.stringValue }
        func lineage(_ level: OrderedSet<XMLElement>, _ seen: OrderedSet<XMLElement>) -> OrderedSet<XMLElement> {
            let parents = level.flatMap { (child: XMLElement) -> [XMLElement] in child.attribute(forName: "inherits")?.stringValue.flatMap { name in named[name] } ?? [] }
            let unseen = OrderedSet(parents).subtracting(seen)
            return unseen.isEmpty ? seen : lineage(unseen, seen.union(unseen))
        }
        func inherited(_ element: XMLElement) -> [XMLElement] {
            lineage([element], [element]).flatMap { (ancestor: XMLElement) -> [XMLElement] in
                let extended = ancestor.attribute(forName: "name")?.stringValue.flatMap { name in extensions[name] } ?? []
                return ancestor.elements(forName: "property") + extended.flatMap { element in element.elements(forName: "property") }
            }
        }
        func sdefType(_ element: XMLElement) -> (types: Set<String>, list: Bool) {
            let declared = [element] + element.elements(forName: "type")
            return (Set(declared.compactMap { type in type.attribute(forName: "type")?.stringValue }), declared.contains { type in type.attribute(forName: "list")?.stringValue == "yes" })
        }
        func terms(_ elements: [XMLElement]) -> [Term] {
            OrderedSet(
                elements.compactMap { element in
                    let (types, list) = sdefType(element)
                    let members: [XMLElement] =
                        switch (element.name.flatMap(Kind.init(rawValue:)), element.name) {
                            case (.class, _), (.recordType, _), (.valueType, _): inherited(element)
                            case (.enumeration, _): element.elements(forName: "enumerator")
                            case (.none, "parameter"): types.flatMap { type in enumerations[type] ?? [] }.flatMap { enumeration in enumeration.elements(forName: "enumerator") }
                            case (.suite, _), (.command, _), (.none, _): []
                        }
                    return switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap(FourCharCode.init(sdef:))) {
                        case (.some(let name), .some(let code)): Term(name: name, code: code, types: types, list: list, members: terms(members))
                        default: nil
                    }
                }
            ).elements
        }
        let commands = try OrderedSet(
            elements("//\(Kind.command.rawValue)").compactMap { element in
                switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap(UInt64.init(sdef:))) {
                    case (.some(let name), .some(let code)):
                        Command(
                            name: name,
                            eventClass: AEEventClass(truncatingIfNeeded: code >> 32),
                            eventID: AEEventID(truncatingIfNeeded: code),
                            parameters: terms(element.elements(forName: "parameter"))
                                + element.elements(forName: "direct-parameter").map { parameter in Term.direct(sdefType(parameter)) },
                        )
                    default: nil
                }
            }
        ).elements
        let scripting = commands.filter { command in command.parameters.contains { term in term.isDirect && term.acceptsText } }
        script =
            scripting.first { command in command.parameters.contains { term in !term.isDirect && term.list && term.acceptsText } }
            ?? scripting.first { command in command.eventID == AEEventID(kAEDoScript) }
        let declared = try Dictionary(
            uniqueKeysWithValues: Kind.allCases.filter { kind in kind != .command }.map { kind throws(AppleEventFailure) in (kind, terms(try elements("//\(kind.rawValue)"))) }
        )
        let typed = typedKinds.flatMap { kind in declared[kind, default: []] }
        self.commands = commands
        declarations = declared
        types = Scope(typed)
        members = Dictionary(grouping: typed, by: \.code).mapValues { terms in Scope(terms.flatMap(\.members)) }
        properties = try Scope(terms(elements("//property")))
        enumerators = Scope(declared[.enumeration, default: []].flatMap(\.members))
        self.document = Mutex(document)
    }

    public func encode(to encoder: any Encoder) throws {
        var listing = encoder.container(keyedBy: Kind.self)
        try listing.encode(commands, forKey: .command)
        for kind in Kind.allCases where kind != .command { try listing.encode(declarations[kind, default: []], forKey: kind) }
        var derived = encoder.container(keyedBy: Key.self)
        try derived.encodeIfPresent(script, forKey: .script)
    }

    public func text(of kind: Kind, named name: String) throws(AppleEventFailure) -> String {
        let query = """
            let $suites := for $suite in /dictionary/suite
                let $picked := if ($kind = "suite") then (for $child in $suite[@name = $name]/* return element { name($child) } { $child/@* }) else $suite/*[name() = $kind][@name = $name]
                where exists($picked) or ($kind = "suite" and $suite/@name = $name)
                return <suite>{ $suite/@*, $picked }</suite>
            return if (exists($suites)) then <dictionary>{ /dictionary/@*, $suites }</dictionary> else ()
            """
        let selected = try document.withLock { document throws(AppleEventFailure) in
            try Result { try document.objects(forXQuery: query, constants: ["kind": kind.rawValue, "name": name]) }
                .mapError { error in AppleEventFailure(subject: "scripting definition XQuery for \(kind.rawValue) \(name)", reason: .host(error as NSError)) }.get()
                .compactMap { node in (node as? XMLNode)?.xmlString }
        }
        guard !selected.isEmpty else {
            let listed = kind == .command ? commands.map(\.name) : declarations[kind, default: []].map(\.name)
            throw AppleEventFailure(subject: name, reason: .unmatched(Unmatched(subject: kind.rawValue, query: name, candidates: OrderedSet(listed).elements)))
        }
        return selected.joined()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ScriptingDictionary {
    // --- [DEFINITIONS]

    @concurrent
    public static func definition(of application: URL) async throws(AppleEventFailure) -> Data {
        let (status, definition) = await withTaskExecutorPreference(appleEvents) {
            var definition: Unmanaged<CFData>?
            return (unsafe OSACopyScriptingDefinitionFromURL(application as CFURL, 0, &definition), unsafe definition?.takeRetainedValue())
        }
        guard status == noErr, let definition else { throw AppleEventFailure(subject: application.path(percentEncoded: false), reason: .osa(status)) }
        return definition as Data
    }

    // --- [COMMANDS]

    public func command(_ query: String?, taking arguments: Set<String>) throws(Unmatched) -> Command {
        let matching: (Command) -> Bool =
            switch query {
                case .some(let query): { command in command.name == query || command.code == UInt64(sdef: query) }
                case .none: { command in command == self.script }
            }
        let takes: (Command) -> Bool = { command in
            arguments.allSatisfy { argument in command.parameters.contains { term in term.name == argument || term.code == FourCharCode(sdef: argument) } }
        }
        let scopes = [Command.language, commands].map { scope in scope.filter(takes) }
        let found = scopes.first { scope in scope.contains(where: matching) }?.filter(matching) ?? []
        let kind = query == nil ? "script command" : "command"
        let subject = arguments.isEmpty ? kind : "\(kind) taking \(arguments.sorted().joined(separator: ", "))"
        return try Unmatched.single(found, subject: subject, query: query, candidates: OrderedSet(scopes.joined().map(\.name)).elements, named: \.code.sdef).get()
    }

    // --- [EVENTS]

    public func event(
        _ command: Command,
        arguments: [String: AppleEventValue],
        target: NSAppleEventDescriptor,
        refusing activating: Set<UInt64> = [],
    ) throws(AggregateError<AppleEventFailure>) -> NSAppleEventDescriptor {
        let event = try record(
            arguments,
            in: [Scope(command.parameters)],
            into: NSAppleEventDescriptor(
                eventClass: command.eventClass,
                eventID: command.eventID,
                targetDescriptor: target,
                returnID: AEReturnID(kAutoGenerateReturnID),
                transactionID: AETransactionID(kAnyTransactionID),
            ),
        )
        event.setAttribute(.null(), forKeyword: AEKeyword(keySubjectAttr))
        let direct = event.paramDescriptor(forKeyword: AEKeyword(keyDirectObject))
        let frontmost =
            switch (command.eventClass, command.eventID, direct?.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue, direct?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue) {
                case (AEEventClass(kAECoreSuite), AEEventID(kAESetData), FourCharCode(formPropertyID), FourCharCode(pIsFrontProcess)): true
                case (AEEventClass(kAECoreSuite), AEEventID(kAESetData), FourCharCode(formPropertyID), FourCharCode(pProperties)):
                    event.paramDescriptor(forKeyword: AEKeyword(keyAEData))?.forKeyword(FourCharCode(pIsFrontProcess)) != nil
                default: false
            }
        guard !activating.contains(command.code), !frontmost else {
            throw AggregateError(first: AppleEventFailure(subject: "\(command.name) (\(command.code.sdef))", reason: .activating))
        }
        return event
    }

    public func reply(
        to query: String?,
        arguments: [String: AppleEventValue],
        target: sending NSAppleEventDescriptor,
        refusing activating: Set<UInt64> = [],
    ) async throws(AggregateError<AppleEventFailure>) -> AppleEventValue {
        let command = try Result { () throws(Unmatched) in try command(query, taking: Set(arguments.keys)) }
            .aggregated { miss in AppleEventFailure(subject: query ?? miss.subject, reason: .unmatched(miss)) }.get()
        let event = try event(command, arguments: arguments, target: target, refusing: activating)
        do { return AppleEventValue(try await event.send(), in: self) } catch { throw AggregateError(first: error) }
    }

    func record(_ fields: [String: AppleEventValue], in scopes: [Scope], into descriptor: NSAppleEventDescriptor) throws(AggregateError<AppleEventFailure>) -> NSAppleEventDescriptor {
        try traverse(fields.sorted { left, right in left.key < right.key }) { key, value in
            Result { () throws(AggregateError<AppleEventFailure>) in
                let term = try Self.resolve(key, in: scopes).get()
                return (term.code, try value.descriptor(as: term.types, in: self))
            }
        }
        .get()
        .reduce(into: descriptor) { filled, item in filled.setDescriptor(item.1, forKeyword: item.0) }
    }

    // --- [TERMS]

    static func resolve(_ name: String, in scopes: [Scope]) -> Result<Term, AggregateError<AppleEventFailure>> {
        let named = scopes.lazy.compactMap { scope in scope.named[name] }.first ?? []
        let codes = named.isEmpty ? [FourCharCode(sdef: name)].compactMap(\.self) : OrderedSet(named.map(\.code)).elements
        return Unmatched.single(codes, subject: "term", query: name, candidates: OrderedSet(scopes.flatMap(\.named.keys)).elements, named: \.sdef)
            .map { code in Term(name: name, code: code, types: Set(named.flatMap(\.types)), list: named.contains(where: \.list), members: []) }
            .aggregated { miss in AppleEventFailure(subject: name, reason: .unmatched(miss)) }
    }

    static func term(_ code: FourCharCode, in scopes: [Scope]) -> Term {
        let found = scopes.lazy.compactMap { scope in scope.coded[code] }.first ?? []
        let name = found.first { term in resolve(term.name, in: scopes)[case: \.success]?.code == code }?.name
        return Term(name: name ?? code.sdef, code: code, types: Set(found.flatMap(\.types)), list: found.contains(where: \.list), members: [])
    }

    func scope(_ code: FourCharCode?) -> [Scope] {
        [code.flatMap { code in members[code] }, properties].compactMap(\.self)
    }

    func classCode(of value: AppleEventValue) -> FourCharCode? {
        switch value {
            case .text(let name), .type(let name), .specifier(want: let name, form: _, seld: _, from: _):
                Self.resolve(name, in: [types])[case: \.success]?.code
            case .null: FourCharCode(cApplication)
            case .bool, .number, .list, .record, .enumerator, .date, .file, .opaque: nil
        }
    }

    func enumerators(of types: Set<String>) -> Scope {
        Scope(declarations[.enumeration, default: []].filter { enumeration in types.contains(enumeration.name) }.flatMap(\.members))
    }
}

extension FixedWidthInteger where Self: UnsignedInteger {
    // --- [CODES]

    public init?(sdef text: String) {
        switch (text.wholeMatch(of: /0x(.+)/).flatMap { match in [UInt8](hexEncoded: match.1) }, text.data(using: .macOSRoman).map { data in [UInt8](data) }) {
            case (.some(let bytes), _) where bytes.count == Self.bitWidth / 8, (_, .some(let bytes)) where bytes.count == Self.bitWidth / 8:
                self = bytes.reduce(0) { code, byte in code << 8 | Self(byte) }
            default: return nil
        }
    }

    public var sdef: String {
        let bytes = stride(from: Self.bitWidth - 8, through: 0, by: -8).map { shift in UInt8(truncatingIfNeeded: self >> shift) }
        return (bytes.contains { byte in byte < 0x20 || byte == 0x7F } ? nil : String(bytes: bytes, encoding: .macOSRoman)) ?? "0x" + bytes.hexEncoded(uppercase: true)
    }
}
