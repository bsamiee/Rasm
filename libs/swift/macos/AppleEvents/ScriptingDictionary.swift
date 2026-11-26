import AppKit
import Carbon.OpenScripting
import CasePathsCore
public import CoreServices
import Encoding
public import Foundation
import OrderedCollections
public import Results

// --- [MODELS] --------------------------------------------------------------------------

public final class ScriptingDictionary: Encodable, Sendable {
    public enum Kind: String, CaseIterable, CodingKey {
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
        var synonyms: [String] = []
        var codes: [FourCharCode] = []
        var hidden = false
        var elements: [String] = []

        static func direct(_ type: (types: Set<String>, list: Bool)) -> Self { Self(name: "direct", code: AEKeyword(keyDirectObject), types: type.types, list: type.list, members: []) }

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
        public enum Query: CustomStringConvertible, Sendable {
            case name(String)
            case code(UInt64)

            public var description: String {
                switch self {
                    case .name(let name): name
                    case .code(let code): code.sdef
                }
            }
        }

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
        var synonyms: [String] = []
        var hidden = false

        private enum Key: CodingKey {
            case name, code, parameters, types, list, enumerators
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
                try fields.encode(parameter.types.sorted(), forKey: .types)
                try fields.encode(parameter.list, forKey: .list)
                try fields.encode(parameter.members, forKey: .enumerators)
            }
        }
    }

    struct Scope: Sendable {
        let byName: OrderedDictionary<String, [Term]>
        let byCode: [FourCharCode: [Term]]
        let visibleNames: [String]

        init(_ terms: [Term]) {
            let aliased: [(name: String, term: Term)] = terms.flatMap { term in term.synonyms.map { name in (name: name, term: term) } }
            let recoded: [(code: FourCharCode, term: Term)] = terms.flatMap { term in term.codes.map { code in (code: code, term: term) } }
            let synonyms: OrderedDictionary<String, [Term]> = OrderedDictionary(grouping: aliased, by: \.name).mapValues { pairs in pairs.map(\.term) }
            let codes: [FourCharCode: [Term]] = Dictionary(grouping: recoded, by: \.code).mapValues { pairs in pairs.map(\.term) }
            byName = OrderedDictionary(grouping: terms, by: \.name).merging(synonyms) { primary, _ in primary }
            byCode = Dictionary(grouping: terms, by: \.code).merging(codes) { primary, _ in primary }
            visibleNames = OrderedSet(terms.filter { term in !term.hidden }.map(\.name)).elements
        }
    }

    private enum Key: CodingKey {
        case script, open
    }

    public let commands: [Command]
    public let script: Command?
    public let declarations: [Kind: [Term]]
    let types: Scope
    let members: [FourCharCode: Scope]
    let contained: [FourCharCode: Scope]
    let properties: Scope
    let enumerators: Scope

    public init(definition data: Data) throws(AppleEventFailure) {
        let document = try Result { try XMLDocument(data: data, options: .documentXInclude) }
            .mapError { error in AppleEventFailure(subject: "XML of \(data.count)-byte scripting definition", reason: .foundation(error as NSError)) }.get()
        let query: String = Kind.allCases.map { kind in "//\(kind.rawValue)" }.joined(separator: " | ") + " | //class-extension"
        let elements: [XMLElement] = try Result { try document.nodes(forXPath: query) }
            .mapError { error in AppleEventFailure(subject: "scripting definition XPath \(query)", reason: .foundation(error as NSError)) }.get().compactMap { node in node as? XMLElement }
        let kinds: [Kind?: [XMLElement]] = Dictionary(grouping: elements) { element in element.name.flatMap(Kind.init(rawValue:)) }
        let typedKinds: [Kind] = [.class, .recordType, .valueType]
        let named: [String?: [XMLElement]] = Dictionary(grouping: typedKinds.flatMap { kind in kinds[kind, default: []] }) { element in element.attribute(forName: "name")?.stringValue }
        let extensions: [String?: [XMLElement]] = Dictionary(grouping: kinds[.none, default: []]) { element in element.attribute(forName: "extends")?.stringValue }
        let enumerations: [String?: [XMLElement]] = Dictionary(grouping: kinds[.enumeration, default: []]) { element in element.attribute(forName: "name")?.stringValue }
        func declaringElements(of element: XMLElement) -> [XMLElement] {
            let ancestors = sequence(state: (found: OrderedSet([element]), next: 0)) { (walk: inout (found: OrderedSet<XMLElement>, next: Int)) -> XMLElement? in
                guard walk.next < walk.found.count else { return nil }
                let ancestor = walk.found[walk.next]
                walk.found.append(contentsOf: ancestor.attribute(forName: "inherits")?.stringValue.flatMap { name in named[name] } ?? [])
                walk.next += 1
                return ancestor
            }
            return ancestors.flatMap { (ancestor: XMLElement) -> [XMLElement] in
                [ancestor] + (ancestor.attribute(forName: "name")?.stringValue.flatMap { name in extensions[name] } ?? [])
            }
        }
        func sdefType(_ element: XMLElement) -> (types: Set<String>, list: Bool) {
            let declared = [element] + element.elements(forName: "type")
            return (Set(declared.compactMap { type in type.attribute(forName: "type")?.stringValue }), declared.contains { type in type.attribute(forName: "list")?.stringValue == "yes" })
        }
        func terms(_ elements: [XMLElement]) -> [Term] {
            OrderedSet(
                elements.compactMap { element in
                    let ((types, list), kind, synonyms) = (sdefType(element), element.name.flatMap(Kind.init(rawValue:)), element.elements(forName: "synonym"))
                    let declaring = kind == .class ? declaringElements(of: element) : [element]
                    let members: [XMLElement] =
                        switch kind {
                            case .class, .recordType, .valueType: declaring.flatMap { declared in declared.elements(forName: "property") }
                            case .enumeration: element.elements(forName: "enumerator")
                            case .suite, .command: []
                            case .none: types.flatMap { type in enumerations[type] ?? [] }.flatMap { enumeration in enumeration.elements(forName: "enumerator") }
                        }
                    return switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap(FourCharCode.init(sdef:))) {
                        case (.some(let name), .some(let code)):
                            Term(
                                name: name,
                                code: code,
                                types: types,
                                list: list,
                                members: terms(members),
                                synonyms: synonyms.compactMap { synonym in synonym.attribute(forName: "name")?.stringValue }
                                    + [element.attribute(forName: "plural")?.stringValue ?? (kind == .class ? name + "s" : nil)].compactMap(\.self),
                                codes: synonyms.compactMap { synonym in synonym.attribute(forName: "code")?.stringValue.flatMap(FourCharCode.init(sdef:)) },
                                hidden: element.attribute(forName: "hidden")?.stringValue == "yes",
                                elements: declaring.flatMap { declared in declared.elements(forName: "element") }.compactMap { contained in contained.attribute(forName: "type")?.stringValue },
                            )
                        default: nil
                    }
                }
            ).elements
        }
        let commands = OrderedSet(
            kinds[.command, default: []].compactMap { element in
                switch (element.attribute(forName: "name")?.stringValue, element.attribute(forName: "code")?.stringValue.flatMap(UInt64.init(sdef:))) {
                    case (.some(let name), .some(let code)):
                        Command(
                            name: name,
                            eventClass: AEEventClass(truncatingIfNeeded: code >> 32),
                            eventID: AEEventID(truncatingIfNeeded: code),
                            parameters: terms(element.elements(forName: "parameter"))
                                + element.elements(forName: "direct-parameter").map { parameter in Term.direct(sdefType(parameter)) },
                            synonyms: element.elements(forName: "synonym").compactMap { synonym in synonym.attribute(forName: "name")?.stringValue },
                            hidden: element.attribute(forName: "hidden")?.stringValue == "yes",
                        )
                    default: nil
                }
            }
        ).elements
        let scripting = commands.filter { command in command.parameters.contains { term in term.isDirect && term.acceptsText } }
        script =
            scripting.first { command in command.parameters.contains { term in !term.isDirect && term.list && term.acceptsText } }
            ?? scripting.first { command in command.eventID == AEEventID(kAEDoScript) }
        let declared: [Kind: [Term]] = Dictionary(uniqueKeysWithValues: Kind.allCases.filter { kind in kind != .command }.map { kind in (kind, terms(kinds[kind, default: []])) })
        let typed = typedKinds.flatMap { kind in declared[kind, default: []] }
        let (coded, types) = (Dictionary(grouping: typed, by: \.code), Scope(typed))
        self.commands = commands
        declarations = declared
        self.types = types
        members = coded.mapValues { terms in Scope(terms.flatMap(\.members)) }
        contained = coded.mapValues { terms in Scope(terms.flatMap(\.elements).flatMap { name in types.byName[name] ?? [] }) }
        properties = Scope(terms(elements.flatMap { element in element.elements(forName: "property") }))
        enumerators = Scope(declared[.enumeration, default: []].flatMap(\.members))
    }

    public func encode(to encoder: any Encoder) throws {
        var listing = encoder.container(keyedBy: Kind.self)
        try listing.encode(commands.filter { command in !command.hidden }, forKey: .command)
        for kind in Kind.allCases where kind != .command { try listing.encode(declarations[kind, default: []].filter { term in !term.hidden }, forKey: kind) }
        var derived = encoder.container(keyedBy: Key.self)
        try derived.encodeIfPresent(script, forKey: .script)
        try derived.encodeIfPresent(commands.first { command in command.eventClass == AEEventClass(kCoreEventClass) && command.eventID == AEEventID(kAEOpenDocuments) }, forKey: .open)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ScriptingDictionary {
    // --- [DEFINITIONS]

    public static func definition(of target: sending NSAppleEventDescriptor, within deadline: Duration) async throws(AppleEventFailure) -> Data {
        let request = NSAppleEventDescriptor(
            eventClass: AEEventClass(kASAppleScriptSuite),
            eventID: "gsdf".utf8.reduce(0) { code, byte in code << 8 | AEEventID(byte) },
            targetDescriptor: target,
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        return try await NSAppleEventDescriptor.send(request, within: deadline) { reply in reply.stringValue.map { text in Data(text.utf8) } ?? reply.data }
    }

    @concurrent
    public static func definition(terminology resource: URL) async throws(AppleEventFailure) -> Data {
        let (files, subject) = (FileManager.default, resource.path(percentEncoded: false))
        let folder = try Result { () throws(any Error) in try files.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: files.temporaryDirectory, create: true) }
            .mapError { error in AppleEventFailure(subject: "folder to stage \(subject)", reason: .foundation(error as NSError)) }.get()
        let contents = folder.appending(component: "Contents")
        let staged = Result { () throws(any Error) in
            try files.createDirectory(at: contents.appending(component: "Resources"), withIntermediateDirectories: true)
            try PropertyListSerialization.data(fromPropertyList: [kCFBundleExecutableKey as String: resource.deletingPathExtension().lastPathComponent], format: .xml, options: 0)
                .write(to: contents.appending(component: "Info.plist"))
            try files.copyItem(at: resource, to: contents.appending(components: "Resources", resource.lastPathComponent))
        }
        let (status, definition): (OSStatus, CFData?) =
            switch staged {
                case .success:
                    await withTaskExecutorPreference(appleEventQueue) {
                        var definition: Unmanaged<CFData>?
                        return (unsafe OSACopyScriptingDefinitionFromURL(folder as CFURL, 0, &definition), unsafe definition?.takeRetainedValue())
                    }
                case .failure: (noErr, nil)
            }
        switch (staged, Result { () throws(any Error) in try files.removeItem(at: folder) }) {
            case (.failure(let error), _), (.success, .failure(let error)): throw AppleEventFailure(subject: "staged copy of \(subject)", reason: .foundation(error as NSError))
            case (.success, .success): break
        }
        guard status == noErr, let definition else { throw AppleEventFailure(subject: subject, reason: .osa(status)) }
        return definition as Data
    }

    public static func definition(suites bundle: Bundle) throws(AppleEventFailure) -> Data {
        let foundation = Bundle(for: NSScriptSuiteRegistry.self)
        let bundles: [Bundle] = [foundation, Bundle(for: NSApplication.self), bundle]
        func plist(_ file: URL) throws(AppleEventFailure) -> [String: Any] {
            do {
                guard let read = try unsafe PropertyListSerialization.propertyList(from: Data(contentsOf: file), format: nil) as? [String: Any] else { throw CocoaError(.propertyListReadCorrupt) }
                return read
            } catch {
                throw AppleEventFailure(subject: file.path(percentEncoded: false), reason: .foundation(error as NSError))
            }
        }
        func declared(_ file: URL?, _ query: String) throws(AppleEventFailure) -> [XMLElement] {
            do {
                guard let file else { throw CocoaError(.fileReadNoSuchFile) }
                return try XMLDocument(contentsOf: file).nodes(forXPath: query).compactMap { node in node as? XMLElement }
            } catch {
                throw AppleEventFailure(subject: "scripting definition XPath \(query)", reason: .foundation(error as NSError))
            }
        }
        func field(_ value: Any?, _ keys: String...) -> [String: Any] { keys.reduce(value) { found, key in (found as? [String: Any])?[key] } as? [String: Any] ?? [:] }
        func table(_ value: Any?) -> [(key: String, value: [String: Any])] { field(value).map { key, value in (key, field(value)) }.sorted { left, right in left.key < right.key } }
        func codes(_ value: Any?) -> [(key: String, value: String)] { (value as? [String: String] ?? [:]).sorted { left, right in left.key < right.key } }
        func flag(_ value: Any?) -> String? {
            let set: Bool = (value as? NSString)?.boolValue ?? (value as? NSNumber)?.boolValue ?? false
            return set ? "yes" : nil
        }
        func node(_ name: String, _ attributes: [String: Any?], _ children: [XMLElement] = []) -> XMLElement {
            let element = XMLElement(name: name)
            element.setAttributesWith(attributes.compactMapValues { value in value as? String })
            element.setChildren(children)
            return element
        }
        let pairs: [(name: String, suite: [String: Any], terms: [String: Any])] = try bundles.map { bundle throws(AppleEventFailure) in
            try (bundle.urls(forResourcesWithExtension: "scriptSuite", subdirectory: nil) ?? []).sorted { left, right in left.lastPathComponent < right.lastPathComponent }.map {
                file throws(AppleEventFailure) in
                let (suite, stem) = (try plist(file), file.deletingPathExtension().lastPathComponent)
                return (name: suite["Name"] as? String ?? stem, suite: suite, terms: try bundle.url(forResource: stem, withExtension: "scriptTerminology").map(plist) ?? [:])
            }
        }.flatMap(\.self)
        let classes: [(keys: [String], code: String?, name: String)] = pairs.flatMap { pair -> [(keys: [String], code: String?, name: String)] in
            let declared: [(keys: [String], code: String?, name: String)] = table(pair.suite["Classes"]).map { key, cocoa in
                let name: String = field(pair.terms, "Classes", key)["Name"] as? String ?? key
                return (keys: [pair.name + "." + key, key], code: cocoa["AppleEventCode"] as? String, name: name)
            }
            let synonyms: [(keys: [String], code: String?, name: String)] = codes(pair.suite["Synonyms"]).map { code, _ in
                let name: String = field(pair.terms, "Synonyms", code)["Name"] as? String ?? code
                return (keys: [], code: code, name: name)
            }
            return declared + synonyms
        }
        let classNames: [String: String] = Dictionary(classes.flatMap { row -> [(String, String)] in row.keys.map { key in (key, row.name) } }, uniquingKeysWith: { first, _ in first })
        let classCodes: [String: String] = Dictionary(
            classes.flatMap { row -> [(String, String)] in row.code.map { code in row.keys.map { key in (key, code) } } ?? [] },
            uniquingKeysWith: { first, _ in first },
        )
        let codeNames: [String: String] = Dictionary(classes.compactMap { row -> (String, String)? in row.code.map { code in (code, row.name) } }, uniquingKeysWith: { first, _ in first })
        let enumerated: [(String, String)] = pairs.flatMap { pair -> [(String, String)] in
            table(pair.suite["Enumerations"]).flatMap { key, cocoa -> [(String, String)] in
                let code: String = cocoa["AppleEventCode"] as? String ?? key
                return [(pair.name + "." + key, code), (key, code)]
            }
        }
        let enumerationCodes: [String: String] = Dictionary(enumerated, uniquingKeysWith: { first, _ in first })
        let valueTypes: [String: String] = Dictionary(pairs.flatMap { pair -> [(key: String, value: String)] in codes(pair.suite["ValueTypes"]) }, uniquingKeysWith: { first, _ in first })
        let typeNames: [String: String] = Dictionary(
            try declared(Bundle(identifier: "com.apple.openscripting")?.url(forResource: "Compatibility", withExtension: "sdef"), "//class[@name]").flatMap {
                (type: XMLElement) -> [(String, String)] in
                let codes: [String] = ([type] + type.elements(forName: "synonym")).compactMap { coded in coded.attribute(forName: "code")?.stringValue }
                return type.attribute(forName: "name")?.stringValue.map { name in codes.map { code in (code, name) } } ?? []
            },
            uniquingKeysWith: { first, _ in first },
        )
        let intrinsics: [(cocoa: String, name: String)] = try declared(foundation.url(forResource: "Intrinsics", withExtension: "sdef"), "//value-type").compactMap { type in
            switch (type.elements(forName: "cocoa").first?.attribute(forName: "class")?.stringValue, type.attribute(forName: "name")?.stringValue) {
                case (.some(let cocoa), .some(let name)): (cocoa: cocoa, name: name)
                default: nil
            }
        }
        let cocoaTypes: [String: String] = Dictionary(grouping: intrinsics, by: \.cocoa).compactMapValues { rows in rows.count == 1 ? rows.first?.name : nil }
        let numberCodes: [String: DescType] = [
            "NSNumber": DescType(typeSInt32), "NSNumber<Bool>": DescType(typeBoolean), "NSNumber<Short>": DescType(typeSInt16), "NSNumber<Int>": DescType(typeSInt32),
            "NSNumber<Long>": DescType(typeSInt32), "NSNumber<UnsignedInt>": DescType(typeUInt32), "NSNumber<UnsignedLong>": DescType(typeUInt32),
            "NSNumber<LongLong>": DescType(typeSInt64), "NSNumber<Float>": DescType(typeIEEE32BitFloatingPoint), "NSNumber<Double>": DescType(typeIEEE64BitFloatingPoint),
            "NSNumber<TypeCode>": DescType(typeType), "NSString<FilePath>": DescType(typeAlias),
        ]
        func resolved(_ cocoa: Any?) -> (type: String, list: Bool) {
            let spelled: String? = cocoa as? String
            let parts = spelled.flatMap { spelled in spelled.wholeMatch(of: /(?<base>[\w.]+)(?:<(?<tag>[\w.]+)>)?/) }
            let base: String? = parts.map { parts in String(parts.output.base) }
            let tag: String? = parts?.output.tag.map(String.init)
            let numbered: String? = spelled.flatMap { spelled in numberCodes[spelled].flatMap { code in typeNames[code.sdef] } }
            let tagged: String? = tag.flatMap { tag in enumerationCodes[tag] ?? valueTypes[tag].flatMap { code in typeNames[code] } }
            let classed: String? = base.flatMap { base in classNames[base] ?? cocoaTypes[base] }
            let named: String = numbered ?? tagged ?? classed ?? "any"
            return (type: named, list: base == "NSArray")
        }
        func typed(_ name: String, _ cocoa: Any?, _ attributes: [String: Any?]) -> XMLElement {
            let (type, list) = resolved(cocoa)
            return list ? node(name, attributes, [node("type", ["type": type, "list": "yes"])]) : node(name, attributes.merging(["type": type]) { _, type in type })
        }
        let groups: [(group: String, many: Bool)] = [("Attributes", false), ("ToOneRelationships", false), ("ToManyRelationships", true)]
        let rows: [(owner: String, many: Bool, key: String, declared: [String: Any], term: [String: Any])] = pairs.flatMap { pair in
            table(pair.suite["Classes"]).flatMap { owner, cocoa -> [(owner: String, many: Bool, key: String, declared: [String: Any], term: [String: Any])] in
                groups.flatMap { group, many in
                    table(cocoa[group]).map { key, declared in (owner: pair.name + "." + owner, many: many, key: key, declared: declared, term: field(pair.terms, "Classes", owner, group, key)) }
                }
            }
        }
        let owned: [String: [XMLElement]] = Dictionary(grouping: rows, by: \.owner).mapValues { owned in
            owned.map { row in
                let (code, access): (String?, String?) = (row.declared["AppleEventCode"] as? String, flag(row.declared["ReadOnly"]).map { _ in "r" })
                let contained: String = code.flatMap { code in codeNames[code] } ?? resolved(row.declared["Type"]).type
                return row.many
                    ? node("element", ["type": contained, "access": access])
                    : typed(
                        "property",
                        row.declared["Type"],
                        ["name": row.term["Name"] ?? row.key, "code": code, "access": access, "description": row.term["Description"], "hidden": row.term.isEmpty ? "yes" : nil],
                    )
            }
        }
        let suites: [XMLElement] = pairs.map { name, suite, terms in
            let declared: [XMLElement] = table(suite["Classes"]).map { key, cocoa in
                let (named, superclass, children) = (field(terms, "Classes", key), cocoa["Superclass"] as? String, owned[name + "." + key, default: []])
                let (code, inheritedCode, inherited): (String?, String?, String?) = (
                    cocoa["AppleEventCode"] as? String, superclass.flatMap { superclass in classCodes[superclass] }, superclass.flatMap { superclass in classNames[superclass] },
                )
                return switch (code == nil || code == inheritedCode, inherited) {
                    case (true, .some(let extended)): node("class-extension", ["extends": extended, "description": named["Description"]], children)
                    case (_, let inherited):
                        node(
                            "class",
                            [
                                "name": named["Name"] ?? key, "code": code, "plural": named["PluralName"], "inherits": inherited, "description": named["Description"],
                                "hidden": named.isEmpty ? "yes" : nil,
                            ],
                            children,
                        )
                }
            }
            let synonyms: [XMLElement] = codes(suite["Synonyms"]).map { code, synonym in
                let term = field(terms, "Synonyms", code)
                return node(
                    "class",
                    [
                        "name": term["Name"] ?? code, "code": code, "plural": term["PluralName"], "inherits": classNames[synonym], "description": term["Description"],
                        "hidden": term.isEmpty ? "yes" : nil,
                    ],
                )
            }
            let commands: [XMLElement] = table(suite["Commands"]).map { key, cocoa in
                let (named, unnamed) = (field(terms, "Commands", key), field(cocoa, "UnnamedArgument"))
                let direct: [XMLElement] =
                    unnamed.isEmpty ? [] : [typed("direct-parameter", unnamed["Type"], ["optional": flag(unnamed["Optional"]), "description": field(named, "UnnamedArgument")["Description"]])]
                let parameters: [XMLElement] = table(cocoa["Arguments"]).map { argument, declared in
                    let term = field(named, "Arguments", argument)
                    return typed(
                        "parameter",
                        declared["Type"],
                        ["name": term["Name"] ?? argument, "code": declared["AppleEventCode"], "optional": flag(declared["Optional"]), "description": term["Description"]],
                    )
                }
                let result: [XMLElement] = cocoa["Type"].map { type in [typed("result", type, [:])] } ?? []
                let code: String = [cocoa["AppleEventClassCode"], cocoa["AppleEventCode"]].compactMap { part in part as? String }.joined()
                return node("command", ["name": named["Name"] ?? key, "code": code, "description": named["Description"], "hidden": named.isEmpty ? "yes" : nil], direct + parameters + result)
            }
            let enumerations: [XMLElement] = table(suite["Enumerations"]).map { key, cocoa in
                let enumerators: [XMLElement] = codes(cocoa["Enumerators"]).map { item, code in
                    let term = field(terms, "Enumerations", key, item)
                    return node("enumerator", ["name": term["Name"] ?? item, "code": code, "description": term["Description"]])
                }
                return node("enumeration", ["name": cocoa["AppleEventCode"] ?? key, "code": cocoa["AppleEventCode"]], enumerators)
            }
            let attributes: [String: Any?] = ["name": terms["Name"] ?? name, "code": suite["AppleEventCode"], "description": terms["Description"], "hidden": terms.isEmpty ? "yes" : nil]
            return node("suite", attributes, declared + synonyms + commands + enumerations)
        }
        return XMLDocument(rootElement: node("dictionary", [:], suites)).xmlData
    }

    // --- [COMMANDS]

    public func command(_ query: Command.Query?, taking arguments: Set<String>) throws(Unmatched) -> Command {
        let matching: (Command) -> Bool =
            switch query {
                case .some(.name(let name)): { command in command.name == name || command.synonyms.contains(name) }
                case .some(.code(let code)): { command in command.code == code }
                case .none: { command in command == self.script }
            }
        let codes: [(argument: String, code: FourCharCode?)] = arguments.map { argument in (argument, FourCharCode(sdef: argument)) }
        let takes: (Command) -> Bool = { command in
            codes.allSatisfy { argument, code in command.parameters.contains { term in term.name == argument || term.synonyms.contains(argument) || term.code == code } }
        }
        let named: [[Command]] = [Command.language, commands].map { scope in scope.filter(matching) }
        let found: [Command] = named.map { scope in scope.filter(takes) }.first { scope in !scope.isEmpty } ?? []
        let kind = query == nil ? "script command" : "command"
        return try Unmatched.single(
            found,
            subject: arguments.isEmpty ? kind : "\(kind) taking \(arguments.sorted().joined(separator: ", "))",
            query: query?.description,
            candidates: named.joined().isEmpty
                ? OrderedSet<String>([Command.language, commands].joined().filter { command in takes(command) && !command.hidden }.map(\.name)).elements
                : Scope(named.joined().flatMap(\.parameters)).visibleNames,
            named: \.code.sdef,
        )
        .get()
    }

    // --- [EVENTS]

    public func event(
        _ command: Command,
        arguments: [String: AppleEventValue],
        target: NSAppleEventDescriptor,
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
        let activates: Bool =
            switch (command.eventClass, command.eventID, direct?.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue, direct?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue) {
                case (AEEventClass(kAEMiscStandards), AEEventID(kAEActivate), _, _), (AEEventClass(kAECoreSuite), AEEventID(kAESetData), FourCharCode(formPropertyID), FourCharCode(pIsFrontProcess)):
                    true
                case (AEEventClass(kAECoreSuite), AEEventID(kAESetData), FourCharCode(formPropertyID), FourCharCode(pProperties)):
                    event.paramDescriptor(forKeyword: AEKeyword(keyAEData))?.forKeyword(FourCharCode(pIsFrontProcess)) != nil
                default: false
            }
        guard !activates else {
            throw AggregateError(first: AppleEventFailure(subject: "\(command.name) (\(command.code.sdef))", reason: .activating))
        }
        return event
    }

    public func send(
        _ query: Command.Query?,
        arguments: [String: AppleEventValue],
        target: sending NSAppleEventDescriptor,
        within deadline: Duration,
    ) async throws(AggregateError<AppleEventFailure>) -> AppleEventValue {
        let command = try Result { () throws(Unmatched) in try command(query, taking: Set(arguments.keys)) }
            .aggregated { miss in AppleEventFailure(subject: query?.description ?? miss.subject, reason: .unmatched(miss)) }.get()
        let event = try event(command, arguments: arguments, target: target)
        do { return try await NSAppleEventDescriptor.send(event, within: deadline) { reply in AppleEventValue(reply, in: self) } } catch { throw AggregateError(first: error) }
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
        let named = scopes.lazy.compactMap { scope in scope.byName[name] }.first ?? []
        let codes = named.isEmpty ? [FourCharCode(sdef: name)].compactMap(\.self) : OrderedSet(named.map(\.code)).elements
        return Unmatched.single(codes, subject: "term", query: name, candidates: OrderedSet(scopes.flatMap(\.visibleNames)).elements, named: \.sdef)
            .map { code in Term(name: name, code: code, types: Set(named.flatMap(\.types)), list: named.contains(where: \.list), members: []) }
            .aggregated { miss in AppleEventFailure(subject: name, reason: .unmatched(miss)) }
    }

    static func name(of code: FourCharCode, in scopes: [Scope]) -> String {
        let found: [Term] = scopes.lazy.compactMap { scope in scope.byCode[code] }.first ?? []
        return found.first { term in resolve(term.name, in: scopes)[case: \.success]?.code == term.code }?.name ?? code.sdef
    }

    func scope(_ code: FourCharCode?) -> [Scope] {
        [code.flatMap { code in members[code] }, properties].compactMap(\.self)
    }

    func classes(in code: FourCharCode?) -> [Scope] {
        [code.flatMap { code in contained[code] }, types, properties].compactMap(\.self)
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
