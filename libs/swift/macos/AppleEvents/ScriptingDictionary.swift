import CoreServices
import Encoding
public import Foundation
import OrderedCollections
import Results
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
        let named: OrderedDictionary<String, [Term]>
        let coded: [FourCharCode: [Term]]

        init(_ terms: [Term]) {
            named = OrderedDictionary(grouping: terms, by: \.name)
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

    public init(definition data: Data) throws(AppleEventFailure) {
        let document: XMLDocument = try Result { try XMLDocument(data: data, options: .documentXInclude) }
            .mapError { error in AppleEventFailure(subject: "\(data.count)-byte definition", reason: .xml(error)) }.get()
        func elements(_ path: String) throws(AppleEventFailure) -> [XMLElement] {
            try Result { try document.nodes(forXPath: path) }.mapError { error in AppleEventFailure(subject: path, reason: .xml(error)) }.get()
                .compactMap { node in node as? XMLElement }
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
            OrderedSet(
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
            ).elements
        }
        commands = try OrderedSet(
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
        ).elements
        let declared: [Kind: [Term]] = try Dictionary(
            uniqueKeysWithValues: Kind.allCases.filter { kind in kind != .command }.map { kind throws(AppleEventFailure) in (kind, terms(try elements("//\(kind.rawValue)"))) }
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

    public func text(of kind: Kind, named name: String) throws(AppleEventFailure) -> String {
        let query: String = """
            let $suites := for $suite in /dictionary/suite
                let $picked := if ($kind = "suite") then (for $child in $suite[@name = $name]/* return element { name($child) } { $child/@* }) else $suite/*[name() = $kind][@name = $name]
                where exists($picked) or ($kind = "suite" and $suite/@name = $name)
                return <suite>{ $suite/@*, $picked }</suite>
            return if (exists($suites)) then <dictionary>{ /dictionary/@*, $suites }</dictionary> else ()
            """
        let selected: [String] = try document.withLock { document throws(AppleEventFailure) in
            try Result { try document.objects(forXQuery: query, constants: ["kind": kind.rawValue, "name": name]) }
                .mapError { error in AppleEventFailure(subject: "\(kind.rawValue) \(name)", reason: .xml(error)) }.get()
                .compactMap { node in (node as? XMLNode)?.xmlString }
        }
        guard !selected.isEmpty else {
            let listed: [String] = kind == .command ? commands.map(\.name) : declarations[kind, default: []].map(\.name)
            throw AppleEventFailure(subject: name, reason: .unmatched(Unmatched(subject: kind.rawValue, query: name, candidates: OrderedSet(listed).elements)))
        }
        return selected.joined()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension FixedWidthInteger where Self: UnsignedInteger {
    public init?(sdef text: String) {
        switch (text.wholeMatch(of: /0x(.+)/).flatMap { match in [UInt8](hexEncoded: match.1) }, text.data(using: .macOSRoman).map { data in [UInt8](data) }) {
            case (.some(let bytes), _) where bytes.count == Self.bitWidth / 8, (_, .some(let bytes)) where bytes.count == Self.bitWidth / 8:
                self = bytes.reduce(0) { code, byte in code << 8 | Self(byte) }
            default: return nil
        }
    }

    public var sdef: String {
        let bytes: [UInt8] = stride(from: Self.bitWidth - 8, through: 0, by: -8).map { shift in UInt8(truncatingIfNeeded: self >> shift) }
        return (bytes.contains { byte in byte < 0x20 || byte == 0x7F } ? nil : String(bytes: bytes, encoding: .macOSRoman)) ?? "0x" + bytes.hexEncoded(uppercase: true)
    }
}
