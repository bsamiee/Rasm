import Carbon.OpenScripting
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

struct Term {
    let name: String
    let code: FourCharCode
    let types: Set<String>
    let members: [Self]
}

struct ScriptingDictionary {
    // --- [TERMS]

    let commands: [(name: String, eventClass: AEEventClass, eventID: AEEventID, parameters: [Term], result: Set<String>)]
    let classes: [Term]
    let properties: [Term]
    let enumerations: [Term]

    init(definition: Data) throws {
        let document: XMLDocument = try XMLDocument(data: definition)
        let elements: (String) throws -> [XMLElement] = { path in try document.nodes(forXPath: path).compactMap { node in node as? XMLElement } }
        let declarations: [XMLElement] = try elements("//class | //record-type | //value-type")
        let named: [String: [XMLElement]] = Dictionary(
            declarations.compactMap { element in element.attribute(forName: "name")?.stringValue.map { name in (name, [element]) } },
            uniquingKeysWith: +,
        )
        func declared(_ element: XMLElement) -> [Term] {
            Self.terms(element.elements(forName: "property")) + (element.attribute(forName: "inherits")?.stringValue.flatMap { name in named[name] } ?? []).flatMap(declared)
        }
        commands = try elements("//command").compactMap { element in
            guard let name: String = element.attribute(forName: "name")?.stringValue,
                let code: String = element.attribute(forName: "code")?.stringValue,
                let eventClass: AEEventClass = Self.code("'\(code.prefix(4))'"),
                let eventID: AEEventID = Self.code("'\(code.dropFirst(4))'")
            else { return nil }
            let direct: [Term] = element.elements(forName: "direct-parameter").map { parameter in
                Term(name: "direct", code: AEKeyword(keyDirectObject), types: Self.types(of: parameter), members: [])
            }
            return (
                name: name,
                eventClass: eventClass,
                eventID: eventID,
                parameters: Self.terms(element.elements(forName: "parameter")) + direct,
                result: element.elements(forName: "result").first.map(Self.types(of:)) ?? [],
            )
        }
        classes = Self.terms(declarations, members: declared)
        properties = try Self.terms(elements("//property"))
        enumerations = try Self.terms(elements("//enumeration")) { element in Self.terms(element.elements(forName: "enumerator")) }
    }

    static func read(_ application: Application) -> Result<Self, AggregateError<Failure>> {
        definition(of: application).flatMap { definition in Result { try Self(definition: definition) }.mapError { error in AggregateError(.unreadable(code: (error as NSError).code)) } }
    }

    static func definition(of application: Application) -> Result<Data, AggregateError<Failure>> {
        guard !application.processIdentifiers.isEmpty || Bundle(url: application.url)?.object(forInfoDictionaryKey: "OSAScriptingDefinition") != nil else {
            return .failure(AggregateError(.runningInstances(count: 0)))
        }
        var definition: Unmanaged<CFData>?
        let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(application.url as CFURL, 0, &definition)
        return if status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() { .success(data as Data) } else { .failure(AggregateError(.unreadable(code: Int(status)))) }
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
            case (.some(let code), 1): .success(Term(name: name, code: code, types: Set(found.flatMap(\.types)), members: []))
            case (.none, _): .failure(AggregateError(.unknownTerm(name: name)))
            case (.some, _): .failure(AggregateError(.ambiguousTerm(name: name, codes: codes.compactMap(NSFileTypeForHFSTypeCode))))
        }
    }

    static func term(_ code: FourCharCode, in scopes: [[Term]]) -> Term {
        let found: [Term] = matches(in: scopes) { term in term.code == code }
        let name: String? = found.map(\.name).filter { name in if case .success(let term) = resolve(name, in: scopes) { term.code == code } else { false } }.min()
        return Term(name: name ?? NSFileTypeForHFSTypeCode(code), code: code, types: Set(found.flatMap(\.types)), members: [])
    }

    func scope(_ code: FourCharCode?) -> [[Term]] {
        [classes.filter { term in term.code == code }.flatMap(\.members), properties]
    }

    func classCode(_ want: Descriptor?) -> FourCharCode? {
        if case .text(let name) = want, case .success(let term) = Self.resolve(name, in: [classes]) { term.code } else { nil }
    }

    func enumerators(of types: Set<String>) -> [Term] {
        enumerations.filter { enumeration in types.contains("any") || types.contains(enumeration.name) }.flatMap(\.members)
    }

    // --- [ENCODING]

    func events(
        _ command: String,
        arguments: [String: Descriptor],
        target: NSAppleEventDescriptor,
    ) -> Result<[(event: NSAppleEventDescriptor, result: Set<String>)], AggregateError<Failure>> {
        let skipWarnings: [(String, [String: Descriptor])] =
            classes.contains { term in term.code == FourCharCode(cApplication) && term.members.contains { member in member.name == "skip warnings" } }
            ? [("set", ["direct": .record(["want": .text("'prop'"), "form": .text("'prop'"), "seld": .text("skip warnings")]), "to": .boolean(true)])] : []
        return collect(
            (skipWarnings + [(command, arguments)]).map { command, arguments in
                let wanted: Set<String> = if case .record(let fields) = arguments["direct"], case .text(let want) = fields["want"] { [want] } else { [] }
                func fit(_ parameters: [Term]) -> Int {
                    let direct: Bool = parameters.contains { parameter in parameter.name == "direct" && !parameter.types.isDisjoint(with: wanted) }
                    return (Set(arguments.keys).isSubset(of: parameters.map(\.name)) ? 2 : 0) + (direct ? 1 : 0)
                }
                return commands.filter { declaration in declaration.name == command }.max { left, right in fit(left.parameters) < fit(right.parameters) }.map { declaration in
                    let event: NSAppleEventDescriptor = NSAppleEventDescriptor(
                        eventClass: declaration.eventClass,
                        eventID: declaration.eventID,
                        targetDescriptor: target,
                        returnID: AEReturnID(kAutoGenerateReturnID),
                        transactionID: AETransactionID(kAnyTransactionID),
                    )
                    event.setAttribute(.null(), forKeyword: AEKeyword(keySubjectAttr))
                    return encode(arguments, in: [declaration.parameters], into: event).map { event in (event: event, result: declaration.result) }
                } ?? .failure(AggregateError(.absentCommand))
            }
        )
    }

    func encode(_ fields: [String: Descriptor], in scopes: [[Term]], into descriptor: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        collect(
            fields.sorted { left, right in left.key < right.key }.map { key, value in
                Self.resolve(key, in: scopes).flatMap { term in encode(value, as: term.types).map { encoded in (term.code, encoded) } }
            }
        )
        .map { items in put(items, into: descriptor) }
    }

    func encode(_ value: Descriptor, as types: Set<String>) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        switch value {
            case .null:
                .success(.null())
            case .boolean(let value):
                .success(NSAppleEventDescriptor(boolean: value))
            case .number(let value):
                .success((types.contains("integer") ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value))
            case .text(let text):
                encode(text: text, as: types)
            case .list(let items):
                collect(items.map { item in encode(item, as: types) }).map { descriptors in
                    descriptors.reduce(into: .list()) { list, descriptor in list.insert(descriptor, at: list.numberOfItems + 1) }
                }
            case .record(let fields):
                if let want: Descriptor = fields["want"], case .text(let form) = fields["form"] {
                    specifier(want, form: form, fields)
                } else {
                    encode(fields, in: scope(classCode(fields["class"])), into: .record())
                }
        }
    }

    func encode(text: String, as types: Set<String>) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        let enumerators: [Term] = enumerators(of: types)
        let file: URL? = URL(string: text).flatMap { url in url.isFileURL && !types.isDisjoint(with: ["file", "alias", "file specification"]) ? url : nil }
        return if !enumerators.isEmpty, enumerators.contains(where: { term in term.name == text }) || Self.code(text) != nil {
            Self.resolve(text, in: [enumerators]).map { term in NSAppleEventDescriptor(enumCode: term.code) }
        } else if types.contains("type") {
            Self.resolve(text, in: [classes, properties]).map { term in NSAppleEventDescriptor(typeCode: term.code) }
        } else {
            .success(file.map(NSAppleEventDescriptor.init(fileURL:)) ?? NSAppleEventDescriptor(string: text))
        }
    }

    func specifier(_ want: Descriptor, form: String, _ fields: [String: Descriptor]) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        let code: FourCharCode? = Self.code(form)
        let container: FourCharCode? = if case .record(let from) = fields["from"] { classCode(from["want"]) } else { FourCharCode(cApplication) }
        let key: Result<NSAppleEventDescriptor, AggregateError<Failure>> =
            switch (code, fields["seld"]) {
                case (FourCharCode(formPropertyID), .text(let name)):
                    Self.resolve(name, in: scope(container)).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                case (FourCharCode(formAbsolutePosition), .text(let ordinal)):
                    Self.code(ordinal).flatMap { code in NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: code).data) }
                        .map(Result.success) ?? .failure(AggregateError(.unknownTerm(name: ordinal)))
                case (_, let seld):
                    encode(seld ?? .null, as: ["integer"])
            }
        let keywords: [(AEKeyword, Result<NSAppleEventDescriptor, AggregateError<Failure>>)] = [
            (AEKeyword(keyAEDesiredClass), encode(want, as: ["type"])),
            (AEKeyword(keyAEKeyForm), code.map { code in .success(NSAppleEventDescriptor(enumCode: code)) } ?? .failure(AggregateError(.unknownTerm(name: form)))),
            (AEKeyword(keyAEKeyData), key),
            (AEKeyword(keyAEContainer), encode(fields["from"] ?? .null, as: [])),
        ]
        return collect(keywords.map { keyword, result in result.map { descriptor in (keyword, descriptor) } }).map { items in
            guard let specifier: NSAppleEventDescriptor = put(items, into: .record()).coerce(toDescriptorType: DescType(typeObjectSpecifier)) else {
                preconditionFailure("The Apple Event Manager coerces every record to an object specifier")
            }
            return specifier
        }
    }

    // --- [DECODING]

    func decode(_ descriptor: NSAppleEventDescriptor, as types: Set<String>) -> Descriptor {
        let value: Descriptor? =
            switch descriptor.descriptorType {
                case DescType(typeNull):
                    .null
                case DescType(typeType) where descriptor.typeCodeValue == FourCharCode(cMissingValue):
                    .null
                case DescType(typeTrue), DescType(typeFalse), DescType(typeBoolean):
                    .boolean(descriptor.booleanValue)
                case DescType(typeSInt16), DescType(typeSInt32), DescType(typeSInt64), DescType(typeUInt16), DescType(typeUInt32), DescType(typeUInt64),
                    DescType(typeIEEE32BitFloatingPoint), DescType(typeIEEE64BitFloatingPoint):
                    .number(descriptor.doubleValue)
                case DescType(typeType):
                    .text(Self.term(descriptor.typeCodeValue, in: [classes, properties]).name)
                case DescType(typeEnumerated):
                    .text(Self.term(descriptor.enumCodeValue, in: [enumerators(of: types), enumerations.flatMap(\.members)]).name)
                case DescType(typeAbsoluteOrdinal):
                    .text(NSFileTypeForHFSTypeCode(descriptor.enumCodeValue))
                case DescType(typeFileURL), DescType(typeAlias):
                    descriptor.fileURLValue.map { url in .text(url.absoluteString) }
                case DescType(typeLongDateTime):
                    descriptor.dateValue.map { date in .text(date.formatted(.iso8601)) }
                case DescType(typeAEList):
                    .list((0..<descriptor.numberOfItems).compactMap { index in descriptor.atIndex(index + 1) }.map { item in decode(item, as: types) })
                case DescType(typeObjectSpecifier):
                    specifier(descriptor)
                case _ where descriptor.isRecordDescriptor:
                    record(descriptor)
                default:
                    descriptor.stringValue.map(Descriptor.text)
            }
        return value ?? .record(["type": .text(NSFileTypeForHFSTypeCode(descriptor.descriptorType)), "data": .text(descriptor.data.base64EncodedString())])
    }

    func record(_ descriptor: NSAppleEventDescriptor) -> Descriptor {
        let scopes: [[Term]] = scope(descriptor.forKeyword(AEKeyword(pClass))?.typeCodeValue)
        return .record(
            Dictionary(
                uniqueKeysWithValues: (0..<descriptor.numberOfItems).compactMap { index in
                    descriptor.atIndex(index + 1).map { value in
                        let term: Term = Self.term(descriptor.keywordForDescriptor(at: index + 1), in: scopes)
                        return (term.name, decode(value, as: term.types))
                    }
                }
            )
        )
    }

    func specifier(_ descriptor: NSAppleEventDescriptor) -> Descriptor {
        let from: NSAppleEventDescriptor? = descriptor.forKeyword(AEKeyword(keyAEContainer))
        let form: FourCharCode? = descriptor.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue
        let scopes: [[Term]] = scope(from?.forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue ?? FourCharCode(cApplication))
        return .record(
            [
                "want": descriptor.forKeyword(AEKeyword(keyAEDesiredClass)).map { want in decode(want, as: []) },
                "form": form.map { code in .text(NSFileTypeForHFSTypeCode(code)) },
                "seld": descriptor.forKeyword(AEKeyword(keyAEKeyData)).map { key in
                    form == FourCharCode(formPropertyID) ? .text(Self.term(key.typeCodeValue, in: scopes).name) : decode(key, as: [])
                },
                "from": from.map { container in decode(container, as: []) },
            ]
            .compactMapValues(\.self)
        )
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: Encodable {
    case notApplication
    case runningInstances(count: Int)
    case unreadable(code: Int)
    case absentCommand
    case unknownTerm(name: String)
    case ambiguousTerm(name: String, codes: [String])
    case undelivered(status: OSStatus)
    case unanswered(status: OSStatus)
    case reply(status: OSStatus, message: String?, reply: Descriptor?)
}

struct AggregateError<Element: Sendable>: Error {
    let first: Element
    let remaining: [Element]

    init(_ first: Element, remaining: [Element] = []) {
        self.first = first
        self.remaining = remaining
    }

    static func + (left: Self, right: Self) -> Self {
        Self(left.first, remaining: left.remaining + [right.first] + right.remaining)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func collect<Value, Element>(_ results: [Result<Value, AggregateError<Element>>]) -> Result<[Value], AggregateError<Element>> {
    results.reduce(.success([])) { collected, result in
        switch (collected, result) {
            case (.success(let values), .success(let value)): .success(values + [value])
            case (.failure(let failures), .success), (.success, .failure(let failures)): .failure(failures)
            case (.failure(let left), .failure(let right)): .failure(left + right)
        }
    }
}

func put(_ items: [(keyword: AEKeyword, value: NSAppleEventDescriptor)], into descriptor: NSAppleEventDescriptor) -> NSAppleEventDescriptor {
    items.reduce(into: descriptor) { descriptor, item in descriptor.setDescriptor(item.value, forKeyword: item.keyword) }
}
