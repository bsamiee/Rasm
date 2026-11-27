import AppKit
import Carbon.OpenScripting
import CasePathsCore
package import CoreServices
import Encoding
package import Foundation
import OrderedCollections
package import Results

// --- [MODELS] --------------------------------------------------------------------------

package final class ScriptingDictionary: Encodable, Sendable {
    enum Kind: String, CodingKey {
        case dictionary
        case suite
        case command
        case `class`
        case classExtension = "class-extension"
        case recordType = "record-type"
        case valueType = "value-type"
        case enumeration
        case enumerator
        case property
        case contents
        case element
        case parameter
        case directParameter = "direct-parameter"
        case synonym
        case type
        case cocoa
    }

    enum Attribute: String {
        case name
        case code
        case type
        case list
        case hidden
        case plural
        case inherits
        case extends
        case `class`

        static let yes = "yes"
    }

    package struct Term: Encodable, Hashable, Sendable {
        let name: String
        package let code: FourCharCode
        let types: Set<String>
        let list: Bool
        let members: [Self]
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

        package func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: Key.self)
            try fields.encode(name, forKey: .name)
            try fields.encode(code.sdef, forKey: .code)
        }
    }

    package struct Command: Encodable, Hashable, Sendable {
        package enum Query: CustomStringConvertible, Sendable {
            case name(String)
            case code(UInt64)

            package var description: String {
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

        let name: String
        package let eventClass: AEEventClass
        package let eventID: AEEventID
        package let parameters: [Term]
        var synonyms: [String] = []
        var hidden = false

        private enum Key: CodingKey {
            case name, code, parameters, types, list, enumerators
        }

        var code: UInt64 { UInt64(eventClass) << 32 | UInt64(eventID) }

        package func encode(to encoder: any Encoder) throws {
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
        let terms: [Term]
        let byName: OrderedDictionary<String, [Term]>
        let byCode: [FourCharCode: [Term]]
        let visibleNames: [String]

        init(_ terms: [Term]) {
            let aliased: [(name: String, term: Term)] = terms.flatMap { term in term.synonyms.map { name in (name: name, term: term) } }
            let recoded: [(code: FourCharCode, term: Term)] = terms.flatMap { term in term.codes.map { code in (code: code, term: term) } }
            let synonyms: OrderedDictionary<String, [Term]> = OrderedDictionary(grouping: aliased, by: \.name).mapValues { pairs in pairs.map(\.term) }
            let codes: [FourCharCode: [Term]] = Dictionary(grouping: recoded, by: \.code).mapValues { pairs in pairs.map(\.term) }
            self.terms = terms
            byName = OrderedDictionary(grouping: terms, by: \.name).merging(synonyms) { primary, _ in primary }
            byCode = Dictionary(grouping: terms, by: \.code).merging(codes) { primary, _ in primary }
            visibleNames = OrderedSet(terms.filter { term in !term.hidden }.map(\.name)).elements
        }
    }

    private enum Key: CodingKey {
        case script, open
    }

    package let commands: [Command]
    let script: Command?
    let declarations: OrderedDictionary<Kind, [Term]>
    let types: Scope
    let members: [FourCharCode: Scope]
    let contained: [FourCharCode: Scope]
    let properties: Scope
    let enumerators: Scope
    let enumerations: [String: Scope]

    package init(definition data: Data) throws(AppleEventFailure) {
        let document = try Result { try XMLDocument(data: data, options: .documentXInclude) }
            .mapError { error in AppleEventFailure(subject: "scripting definition XML", reason: .foundation(error as NSError)) }.get()
        let elements: [XMLElement] = (document.rootElement()?.elements(forName: Kind.suite.rawValue) ?? []).flatMap { (suite: XMLElement) -> [XMLElement] in
            [suite] + (suite.children ?? []).compactMap { child in child as? XMLElement }
        }
        let kinds: [Kind?: [XMLElement]] = Dictionary(grouping: elements) { element in element.name.flatMap(Kind.init(rawValue:)) }
        let classes: [String?: [XMLElement]] = Dictionary(grouping: kinds[.class, default: []]) { element in element[.name] }
        let extensions: [String?: [XMLElement]] = Dictionary(grouping: kinds[.classExtension, default: []]) { element in element[.extends] }
        func lineage(of element: XMLElement) -> [XMLElement] {
            let ancestors = sequence(state: (found: OrderedDictionary(uniqueKeysWithValues: [(ObjectIdentifier(element), element)]), next: 0)) {
                (walk: inout (found: OrderedDictionary<ObjectIdentifier, XMLElement>, next: Int)) -> XMLElement? in
                guard walk.next < walk.found.count else { return nil }
                let ancestor = walk.found.values[walk.next]
                walk.found.merge((ancestor[.inherits].flatMap { name in classes[name] } ?? []).map { parent in (ObjectIdentifier(parent), parent) }) { kept, _ in kept }
                walk.next += 1
                return ancestor
            }
            return ancestors.flatMap { (ancestor: XMLElement) -> [XMLElement] in [ancestor] + (ancestor[.name].flatMap { name in extensions[name] } ?? []) }
        }
        func declaredType(_ element: XMLElement) -> (types: Set<String>, list: Bool) {
            let declared = [element] + element.elements(forName: Kind.type.rawValue)
            return (Set(declared.compactMap { type in type[.type] }), declared.contains { type in type[.list] == Attribute.yes })
        }
        func term(_ element: XMLElement, _ type: (types: Set<String>, list: Bool), members: [Term], elements contained: [String] = []) -> Term? {
            let kind = element.name.flatMap(Kind.init(rawValue:))
            let identity: (name: String, code: FourCharCode)? =
                switch (element[.name], element[.code].flatMap(FourCharCode.init(sdef:)), kind) {
                    case (.some(let name), .some(let code), _): (name, code)
                    case (let name, let code, .contents): (name ?? Kind.contents.rawValue, code ?? FourCharCode(pContents))
                    default: nil
                }
            let pluralizes: Bool =
                switch kind {
                    case .class, .recordType, .valueType: true
                    default: false
                }
            let synonyms = element.elements(forName: Kind.synonym.rawValue)
            let named: [(name: String, plural: String?)] = synonyms.compactMap { synonym in synonym[.name].map { name in (name: name, plural: synonym[.plural]) } }
            return identity.map { identity in
                let plurals: [String] = ([(name: identity.name, plural: element[.plural])] + named).compactMap { (term: (name: String, plural: String?)) -> String? in
                    pluralizes ? term.plural ?? term.name + "s" : term.plural
                }
                return Term(
                    name: identity.name,
                    code: identity.code,
                    types: type.types,
                    list: type.list,
                    members: members,
                    synonyms: named.map(\.name) + plurals,
                    codes: synonyms.compactMap { synonym in synonym[.code].flatMap(FourCharCode.init(sdef:)) },
                    hidden: element[.hidden] == Attribute.yes,
                    elements: contained,
                )
            }
        }
        let enumerations = kinds[.enumeration, default: []]
        let enumerators: [ObjectIdentifier: [Term]] = Dictionary(
            uniqueKeysWithValues: enumerations.map { enumeration in
                (ObjectIdentifier(enumeration), enumeration.elements(forName: Kind.enumerator.rawValue).compactMap { enumerator in term(enumerator, ([], false), members: []) })
            }
        )
        let enumerated: [String: [Term]] = Dictionary(
            enumerations.compactMap { enumeration in enumeration[.name].map { name in (name, enumerators[ObjectIdentifier(enumeration)] ?? []) } }
        ) { first, next in OrderedSet(first + next).elements }
        func typed(_ element: XMLElement) -> Term? {
            let type = declaredType(element)
            return term(element, type, members: OrderedSet(type.types.sorted().flatMap { type in enumerated[type] ?? [] }).elements)
        }
        let properties: [(ObjectIdentifier, [Term])] = elements.map { element in
            (ObjectIdentifier(element), (element.elements(forName: Kind.property.rawValue) + element.elements(forName: Kind.contents.rawValue)).compactMap(typed))
        }
        let owned: [ObjectIdentifier: [Term]] = enumerators.merging(properties) { enumerators, _ in enumerators }
        let commands = OrderedSet(
            kinds[.command, default: []].compactMap { element in
                switch (element[.name], element[.code].flatMap(UInt64.init(sdef:))) {
                    case (.some(let name), .some(let code)):
                        Command(
                            name: name,
                            eventClass: AEEventClass(truncatingIfNeeded: code >> 32),
                            eventID: AEEventID(truncatingIfNeeded: code),
                            parameters: OrderedSet(element.elements(forName: Kind.parameter.rawValue).compactMap(typed)).elements
                                + element.elements(forName: Kind.directParameter.rawValue).map { parameter in Term.direct(declaredType(parameter)) },
                            synonyms: element.elements(forName: Kind.synonym.rawValue).compactMap { synonym in synonym[.name] },
                            hidden: element[.hidden] == Attribute.yes,
                        )
                    default: nil
                }
            }
        ).elements
        let scripting = commands.filter { command in command.parameters.contains { term in term.isDirect && term.acceptsText } }
        script =
            scripting.first { command in command.parameters.contains { term in !term.isDirect && term.list && term.acceptsText } }
            ?? scripting.first { command in command.eventID == AEEventID(kAEDoScript) }
        let declared: OrderedDictionary<Kind, [Term]> = OrderedDictionary(
            uniqueKeysWithValues: [Kind.suite, .class, .recordType, .valueType, .enumeration].map { kind in
                let terms = kinds[kind, default: []].compactMap { element in
                    let declaring = kind == .class ? lineage(of: element) : [element]
                    return term(
                        element,
                        declaredType(element),
                        members: OrderedSet(declaring.flatMap { declared in owned[ObjectIdentifier(declared)] ?? [] }).elements,
                        elements: declaring.flatMap { declared in declared.elements(forName: Kind.element.rawValue) }.compactMap { contained in contained[.type] },
                    )
                }
                return (kind, OrderedSet(terms).elements)
            }
        )
        let declaredTypes = [Kind.class, .recordType, .valueType].flatMap { kind in declared[kind, default: []] }
        let (coded, types) = (Dictionary(grouping: declaredTypes, by: \.code), Scope(declaredTypes))
        self.commands = commands
        declarations = declared
        self.types = types
        members = coded.mapValues { terms in Scope(terms.flatMap(\.members)) }
        contained = coded.mapValues { terms in Scope(terms.flatMap(\.elements).flatMap { name in types.byName[name] ?? [] }) }
        self.properties = Scope(OrderedSet(properties.flatMap(\.1)).elements)
        self.enumerators = Scope(declared[.enumeration, default: []].flatMap(\.members))
        self.enumerations = enumerated.mapValues(Scope.init)
    }

    package func encode(to encoder: any Encoder) throws {
        var listing = encoder.container(keyedBy: Kind.self)
        try listing.encode(commands.filter { command in !command.hidden }, forKey: .command)
        for (kind, terms) in declarations { try listing.encode(terms.filter { term in !term.hidden }, forKey: kind) }
        var derived = encoder.container(keyedBy: Key.self)
        try derived.encodeIfPresent(script, forKey: .script)
        try derived.encodeIfPresent(commands.first { command in command.eventClass == AEEventClass(kCoreEventClass) && command.eventID == AEEventID(kAEOpenDocuments) }, forKey: .open)
    }
}

private final class ScriptSuite: Decodable {
    private enum CodingKeys: String, CodingKey {
        case name = "Name"
        case appleEventCode = "AppleEventCode"
        case appleEventClassCode = "AppleEventClassCode"
        case superclass = "Superclass"
        case type = "Type"
        case classes = "Classes"
        case commands = "Commands"
        case enumerations = "Enumerations"
        case enumerators = "Enumerators"
        case synonyms = "Synonyms"
        case valueTypes = "ValueTypes"
        case attributes = "Attributes"
        case toOneRelationships = "ToOneRelationships"
        case toManyRelationships = "ToManyRelationships"
        case arguments = "Arguments"
        case unnamedArgument = "UnnamedArgument"
    }

    let name: String?
    let appleEventCode: String?
    let appleEventClassCode: String?
    let superclass: String?
    let type: String?
    let classes: [String: ScriptSuite]?
    let commands: [String: ScriptSuite]?
    let enumerations: [String: ScriptSuite]?
    let enumerators: [String: String]?
    let synonyms: [String: String]?
    let valueTypes: [String: String]?
    let attributes: [String: ScriptSuite]?
    let toOneRelationships: [String: ScriptSuite]?
    let toManyRelationships: [String: ScriptSuite]?
    let arguments: [String: ScriptSuite]?
    let unnamedArgument: ScriptSuite?
}

private struct ScriptTerminology: Decodable {
    private enum CodingKeys: String, CodingKey {
        case name = "Name"
        case pluralName = "PluralName"
        case classes = "Classes"
        case commands = "Commands"
        case enumerations = "Enumerations"
        case synonyms = "Synonyms"
        case attributes = "Attributes"
        case toOneRelationships = "ToOneRelationships"
        case toManyRelationships = "ToManyRelationships"
        case arguments = "Arguments"
    }

    let name: String?
    let pluralName: String?
    let classes: [String: Self]?
    let commands: [String: Self]?
    let enumerations: [String: [String: Self]]?
    let synonyms: [String: Self]?
    let attributes: [String: Self]?
    let toOneRelationships: [String: Self]?
    let toManyRelationships: [String: Self]?
    let arguments: [String: Self]?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ScriptingDictionary {
    // --- [DEFINITIONS]

    package static func definition(of target: sending NSAppleEventDescriptor, within deadline: Duration) async throws(AppleEventFailure) -> Data {
        let request = NSAppleEventDescriptor(
            eventClass: AEEventClass(kASAppleScriptSuite),
            eventID: "gsdf".utf8.reduce(0) { code, byte in code << 8 | AEEventID(byte) },
            targetDescriptor: target,
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        return try await NSAppleEventDescriptor.send(request, within: deadline) { reply in (reply.coerce(toDescriptorType: DescType(typeUTF8Text)) ?? reply).data }
    }

    @concurrent
    package static func definition(terminology resource: URL) async throws(AppleEventFailure) -> Data {
        let (files, subject) = (FileManager.default, resource.path(percentEncoded: false))
        let folder: URL
        do {
            folder = try files.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: files.temporaryDirectory, create: true)
        } catch {
            throw AppleEventFailure(subject: "scratch folder for \(subject)", reason: .foundation(error as NSError))
        }
        let bundle = folder.appending(component: resource.deletingPathExtension().lastPathComponent)
        let staged = Result { () throws(any Error) in
            try files.createDirectory(at: bundle.appending(component: "Resources"), withIntermediateDirectories: true)
            try files.copyItem(at: resource, to: bundle.appending(components: "Resources", resource.lastPathComponent))
        }
        let (status, definition): (OSStatus, CFData?) =
            switch staged {
                case .success:
                    await withTaskExecutorPreference(appleEventQueue) {
                        var definition: Unmanaged<CFData>?
                        return (unsafe OSACopyScriptingDefinitionFromURL(bundle as CFURL, 0, &definition), unsafe definition?.takeRetainedValue())
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

    package static func definition(suites bundle: Bundle) throws(AppleEventFailure) -> Data {
        let foundation = Bundle(for: NSScriptSuiteRegistry.self)
        func read<Value>(_ file: URL?, named name: String, _ decode: (Data) throws -> Value) throws(AppleEventFailure) -> Value {
            do {
                guard let file else { throw CocoaError(.fileReadNoSuchFile) }
                return try decode(Data(contentsOf: file))
            } catch {
                throw AppleEventFailure(subject: file?.path(percentEncoded: false) ?? name, reason: .foundation(error as NSError))
            }
        }
        func entries<Value>(_ table: [String: Value]?) -> [(key: String, value: Value)] { (table ?? [:]).sorted { left, right in left.key < right.key } }
        func node(_ kind: Kind, _ attributes: KeyValuePairs<Attribute, String?>, _ children: [XMLElement] = []) -> XMLElement {
            let element = XMLElement(name: kind.rawValue)
            element.attributes = attributes.compactMap { attribute, value in value.flatMap { value in XMLNode.attribute(withName: attribute.rawValue, stringValue: value) as? XMLNode } }
            element.setChildren(children)
            return element
        }
        let suites: [(name: String, declared: ScriptSuite, terms: ScriptTerminology?)] = try [foundation, Bundle(for: NSApplication.self), bundle].map { bundle throws(AppleEventFailure) in
            try (bundle.urls(forResourcesWithExtension: "scriptSuite", subdirectory: nil) ?? []).sorted { left, right in left.lastPathComponent < right.lastPathComponent }.map {
                file throws(AppleEventFailure) in
                let stem = file.deletingPathExtension().lastPathComponent
                let declared = try read(file, named: stem) { data in try PropertyListDecoder().decode(ScriptSuite.self, from: data) }
                let terms = try bundle.url(forResource: stem, withExtension: "scriptTerminology").map { terminology throws(AppleEventFailure) in
                    try read(terminology, named: stem) { data in try PropertyListDecoder().decode(ScriptTerminology.self, from: data) }
                }
                return (name: declared.name ?? stem, declared: declared, terms: terms)
            }
        }.flatMap(\.self)
        let declaredClasses: [(keys: [String], code: String?, name: String)] = suites.flatMap { suite -> [(keys: [String], code: String?, name: String)] in
            entries(suite.declared.classes).map { key, cocoa in (keys: [suite.name + "." + key, key], code: cocoa.appleEventCode, name: suite.terms?.classes?[key]?.name ?? key) }
        }
        let synonymClasses: [(keys: [String], code: String?, name: String)] = suites.flatMap { suite -> [(keys: [String], code: String?, name: String)] in
            entries(suite.declared.synonyms).map { code, _ in (keys: [], code: code, name: suite.terms?.synonyms?[code]?.name ?? code) }
        }
        let classes = declaredClasses + synonymClasses
        let classNames: [String: String] = Dictionary(classes.flatMap { row -> [(String, String)] in row.keys.map { key in (key, row.name) } }) { first, _ in first }
        let classCodes: [String: String] = Dictionary(classes.flatMap { row -> [(String, String)] in row.code.map { code in row.keys.map { key in (key, code) } } ?? [] }) { first, _ in first }
        let codeNames: [String: String] = Dictionary(classes.compactMap { row -> (String, String)? in row.code.map { code in (code, row.name) } }) { first, _ in first }
        let enumerationCodes: [String: String] = Dictionary(
            suites.flatMap { suite -> [(String, String)] in
                entries(suite.declared.enumerations).flatMap { key, cocoa -> [(String, String)] in [(suite.name + "." + key, cocoa.appleEventCode ?? key), (key, cocoa.appleEventCode ?? key)] }
            }
        ) { first, _ in first }
        let valueTypes: [String: FourCharCode] = Dictionary(
            suites.flatMap { suite -> [(String, FourCharCode)] in entries(suite.declared.valueTypes).compactMap { cocoa, code in FourCharCode(sdef: code).map { code in (cocoa, code) } } }
        ) { first, _ in first }
        let compatibility = try ScriptingDictionary(
            definition: read(Bundle(identifier: "com.apple.openscripting")?.url(forResource: "Compatibility", withExtension: "sdef"), named: "Compatibility.sdef") { data in data }
        )
        let intrinsics = try read(foundation.url(forResource: "Intrinsics", withExtension: "sdef"), named: "Intrinsics.sdef") { data in try XMLDocument(data: data) }
        let intrinsicTypes: [(cocoa: String, name: String)] = (intrinsics.rootElement()?.elements(forName: Kind.suite.rawValue) ?? []).flatMap { suite in
            suite.elements(forName: Kind.valueType.rawValue).compactMap { type in
                switch (type.elements(forName: Kind.cocoa.rawValue).first?[.class], type[.name]) {
                    case (.some(let cocoa), .some(let name)): (cocoa: cocoa, name: name)
                    default: nil
                }
            }
        }
        let cocoaTypes: [String: String] = Dictionary(grouping: intrinsicTypes, by: \.cocoa).compactMapValues { rows in rows.count == 1 ? rows.first?.name : nil }
        let numberCodes: [String: DescType] = [
            "NSNumber": DescType(typeSInt32), "NSNumber<Bool>": DescType(typeBoolean), "NSNumber<Short>": DescType(typeSInt16), "NSNumber<Int>": DescType(typeSInt32),
            "NSNumber<Long>": DescType(typeSInt32), "NSNumber<UnsignedInt>": DescType(typeUInt32), "NSNumber<UnsignedLong>": DescType(typeUInt32),
            "NSNumber<LongLong>": DescType(typeSInt64), "NSNumber<Float>": DescType(typeIEEE32BitFloatingPoint), "NSNumber<Double>": DescType(typeIEEE64BitFloatingPoint),
            "NSNumber<TypeCode>": DescType(typeType), "NSString<FilePath>": DescType(typeAlias),
        ]
        func typeName(_ code: FourCharCode) -> String? { compatibility.types.byCode[code]?.first?.name }
        let spelling = /(?<base>[\w.]+)(?:<(?<tag>[\w.]+)>)?/
        func resolved(_ cocoa: String?) -> (type: String, list: Bool) {
            let parts = cocoa.flatMap { spelled in spelled.wholeMatch(of: spelling) }
            let (base, tag): (String?, String?) = (parts.map { parts in String(parts.output.base) }, parts?.output.tag.map(String.init))
            let numbered: String? = cocoa.flatMap { spelled in numberCodes[spelled] }.flatMap(typeName)
            let tagged: String? = tag.flatMap { tag in enumerationCodes[tag] ?? valueTypes[tag].flatMap(typeName) }
            let classed: String? = base.flatMap { base in classNames[base] ?? cocoaTypes[base] }
            return (type: numbered ?? tagged ?? classed ?? "any", list: base == "NSArray")
        }
        func typed(_ kind: Kind, _ cocoa: String?, _ attributes: KeyValuePairs<Attribute, String?>) -> XMLElement {
            let (type, list) = resolved(cocoa)
            return node(kind, attributes, [node(.type, [.type: type, .list: list ? Attribute.yes : nil])])
        }
        let relationships: [(owner: String, many: Bool, key: String, declared: ScriptSuite, term: ScriptTerminology?)] = suites.flatMap { suite in
            entries(suite.declared.classes).flatMap { owner, cocoa -> [(owner: String, many: Bool, key: String, declared: ScriptSuite, term: ScriptTerminology?)] in
                let terms = suite.terms?.classes?[owner]
                let groups: [(many: Bool, keys: [String: ScriptSuite]?, terms: [String: ScriptTerminology]?)] = [
                    (false, cocoa.attributes, terms?.attributes), (false, cocoa.toOneRelationships, terms?.toOneRelationships), (true, cocoa.toManyRelationships, terms?.toManyRelationships),
                ]
                return groups.flatMap { many, keys, terms in entries(keys).map { key, declared in (owner: suite.name + "." + owner, many: many, key: key, declared: declared, term: terms?[key]) } }
            }
        }
        let owned: [String: [XMLElement]] = Dictionary(grouping: relationships, by: \.owner).mapValues { rows in
            rows.map { row in
                row.many
                    ? node(.element, [.type: row.declared.appleEventCode.flatMap { code in codeNames[code] } ?? resolved(row.declared.type).type])
                    : typed(.property, row.declared.type, [.name: row.term?.name ?? row.key, .code: row.declared.appleEventCode, .hidden: row.term == nil ? Attribute.yes : nil])
            }
        }
        let definitions: [XMLElement] = suites.map { name, declared, terms in
            let classes: [XMLElement] = entries(declared.classes).map { key, cocoa in
                let (named, children) = (terms?.classes?[key], owned[name + "." + key, default: []])
                let (inheritedCode, inherited) = (cocoa.superclass.flatMap { superclass in classCodes[superclass] }, cocoa.superclass.flatMap { superclass in classNames[superclass] })
                return switch (cocoa.appleEventCode == nil || cocoa.appleEventCode == inheritedCode, inherited) {
                    case (true, .some(let extended)): node(.classExtension, [.extends: extended], children)
                    case (_, let inherited):
                        node(
                            .class,
                            [.name: named?.name ?? key, .code: cocoa.appleEventCode, .plural: named?.pluralName, .inherits: inherited, .hidden: named == nil ? Attribute.yes : nil],
                            children,
                        )
                }
            }
            let synonyms: [XMLElement] = entries(declared.synonyms).map { code, synonym in
                let named = terms?.synonyms?[code]
                return node(.class, [.name: named?.name ?? code, .code: code, .plural: named?.pluralName, .inherits: classNames[synonym], .hidden: named == nil ? Attribute.yes : nil])
            }
            let commands: [XMLElement] = entries(declared.commands).map { key, cocoa in
                let named = terms?.commands?[key]
                let direct: [XMLElement] = cocoa.unnamedArgument.map { argument in [typed(.directParameter, argument.type, [:])] } ?? []
                let parameters: [XMLElement] = entries(cocoa.arguments).map { argument, declared in
                    typed(.parameter, declared.type, [.name: named?.arguments?[argument]?.name ?? argument, .code: declared.appleEventCode])
                }
                let code: String = [cocoa.appleEventClassCode, cocoa.appleEventCode].compactMap(\.self).joined()
                return node(.command, [.name: named?.name ?? key, .code: code, .hidden: named == nil ? Attribute.yes : nil], direct + parameters)
            }
            let enumerations: [XMLElement] = entries(declared.enumerations).map { key, cocoa in
                let enumerators: [XMLElement] = entries(cocoa.enumerators).map { item, code in node(.enumerator, [.name: terms?.enumerations?[key]?[item]?.name ?? item, .code: code]) }
                return node(.enumeration, [.name: cocoa.appleEventCode ?? key, .code: cocoa.appleEventCode], enumerators)
            }
            return node(.suite, [.name: terms?.name ?? name, .code: declared.appleEventCode, .hidden: terms == nil ? Attribute.yes : nil], classes + synonyms + commands + enumerations)
        }
        return XMLDocument(rootElement: node(.dictionary, [:], definitions)).xmlData
    }

    // --- [COMMANDS]

    func command(_ query: Command.Query?, taking arguments: Set<String>) throws(Unmatched) -> Command {
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

    func event(
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

    package func send(
        _ query: Command.Query?,
        arguments: [String: AppleEventValue],
        target: sending NSAppleEventDescriptor,
        within deadline: Duration = replyDeadline,
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
        let scopes = types.sorted().compactMap { type in enumerations[type] }
        return scopes.count == 1 ? scopes[0] : Scope(scopes.flatMap(\.terms))
    }
}

extension XMLElement {
    // --- [ATTRIBUTES]

    fileprivate subscript(_ name: ScriptingDictionary.Attribute) -> String? { attribute(forName: name.rawValue)?.stringValue }
}

extension FixedWidthInteger where Self: UnsignedInteger {
    // --- [CODES]

    init?(sdef text: String) {
        let width = Self.bitWidth / 8
        let hex: [UInt8]? = text.utf8.count == 2 + 2 * width && text.hasPrefix("0x") ? [UInt8](hexEncoded: text.dropFirst(2)) : nil
        switch hex ?? (text.unicodeScalars.count == width ? text.data(using: .macOSRoman).map { data in [UInt8](data) } : nil) {
            case .some(let bytes) where bytes.count == width: self = bytes.reduce(0) { code, byte in code << 8 | Self(byte) }
            case .some, .none: return nil
        }
    }

    var sdef: String {
        let bytes = stride(from: Self.bitWidth - 8, through: 0, by: -8).map { shift in UInt8(truncatingIfNeeded: self >> shift) }
        return (bytes.contains { byte in byte < 0x20 || byte == 0x7F } ? nil : String(bytes: bytes, encoding: .macOSRoman)) ?? "0x" + bytes.hexEncoded(uppercase: true)
    }
}
