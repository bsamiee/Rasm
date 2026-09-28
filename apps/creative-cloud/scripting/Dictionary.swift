import Carbon.OpenScripting
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

struct ScriptingDictionary {
    // --- [TERMS]

    let commands: [XMLElement]
    let classes: [String: XMLElement]
    let properties: [String: XMLElement]
    let enumerations: [String: [String: FourCharCode]]
    let terms: [FourCharCode: String]
    let enumerators: [FourCharCode: String]

    init(application: URL) throws {
        let document: XMLDocument = try XMLDocument(data: Self.definition(of: application))
        let elements: (String) throws -> [XMLElement] = { path in try document.nodes(forXPath: path).compactMap { node in node as? XMLElement } }
        let codes: ([XMLElement]) -> [(String, FourCharCode)] = { elements in
            elements.compactMap { element in
                switch (element.attribute(forName: "name")?.stringValue, Self.code(of: element)) {
                    case (.some(let name), .some(let code)): (name, code)
                    default: nil
                }
            }
        }
        let classElements: [XMLElement] = try elements("//class | //record-type | //value-type")
        let propertyElements: [XMLElement] = try elements("//property")
        commands = try elements("//command")
        classes = Dictionary(
            classElements.flatMap { element in ["name", "id"].compactMap { key in element.attribute(forName: key)?.stringValue.map { term in (term, element) } } },
            uniquingKeysWith: { first, _ in first },
        )
        properties = Dictionary(
            propertyElements.compactMap { element in element.attribute(forName: "name")?.stringValue.map { name in (name, element) } },
            uniquingKeysWith: { first, _ in first },
        )
        enumerations = try Dictionary(
            elements("//enumeration").flatMap { enumeration in
                let enumerators: [String: FourCharCode] = Dictionary(codes(enumeration.elements(forName: "enumerator")), uniquingKeysWith: { first, _ in first })
                return ["name", "code"].compactMap { key in enumeration.attribute(forName: key)?.stringValue.map { term in (term, enumerators) } }
            },
            uniquingKeysWith: { first, _ in first },
        )
        terms = Dictionary(codes(classElements + propertyElements).map { name, code in (code, name) }, uniquingKeysWith: { first, _ in first })
        enumerators = try Dictionary(codes(elements("//enumerator")).map { name, code in (code, name) }, uniquingKeysWith: { first, _ in first })
    }

    static func definition(of application: URL) throws -> Data {
        var definition: Unmanaged<CFData>?
        let status: OSStatus = unsafe OSACopyScriptingDefinitionFromURL(application as CFURL, 0, &definition)
        guard status == noErr, let data: CFData = unsafe definition?.takeRetainedValue() else {
            throw NSError(domain: NSOSStatusErrorDomain, code: Int(status))
        }
        return data as Data
    }

    static func code(_ text: String) -> FourCharCode? {
        let code: FourCharCode = NSHFSTypeCodeFromFileType(text)
        return code == 0 ? nil : code
    }

    static func code(of element: XMLElement) -> FourCharCode? {
        element.attribute(forName: "code")?.stringValue.flatMap { code in Self.code("'\(code)'") }
    }

    static func types(of element: XMLElement) -> Set<String> {
        Set(([element.attribute(forName: "type")] + element.elements(forName: "type").map { type in type.attribute(forName: "type") }).compactMap { attribute in attribute?.stringValue })
    }

    func declarations(of className: String?) -> [XMLElement] {
        className.flatMap { name in classes[name] }.map { element in element.elements(forName: "property") + declarations(of: element.attribute(forName: "inherits")?.stringValue) } ?? []
    }

    func property(_ name: String, of className: String?) -> XMLElement? {
        declarations(of: className).first { element in element.attribute(forName: "name")?.stringValue == name } ?? properties[name]
    }

    func term(_ code: FourCharCode, of className: String?) -> String {
        declarations(of: className).first { element in Self.code(of: element) == code }?.attribute(forName: "name")?.stringValue ?? terms[code] ?? NSFileTypeForHFSTypeCode(code)
    }

    // --- [ENCODING]

    func event(_ command: String, arguments: [String: Descriptor], target: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, any Error> {
        let parameters: (XMLElement) -> [String: XMLElement] = { declaration in
            Dictionary(
                (declaration.elements(forName: "parameter") + declaration.elements(forName: "direct-parameter")).map { parameter in
                    (parameter.attribute(forName: "name")?.stringValue ?? "direct", parameter)
                },
                uniquingKeysWith: { first, _ in first },
            )
        }
        let candidates: [XMLElement] = commands.filter { declaration in declaration.attribute(forName: "name")?.stringValue == command }
        let accepting: [XMLElement] = candidates.filter { declaration in Set(arguments.keys).isSubset(of: parameters(declaration).keys) }
        let direct: String? = if case .record(let fields) = arguments["direct"], case .text(let want) = fields["want"] { want } else { nil }
        let overload: XMLElement? = direct.flatMap { want in
            accepting.first { declaration in declaration.elements(forName: "direct-parameter").contains { parameter in Self.types(of: parameter).contains(want) } }
        }
        guard let declaration: XMLElement = overload ?? accepting.first ?? candidates.first,
            let eventCode: String = declaration.attribute(forName: "code")?.stringValue,
            let eventClass: AEEventClass = Self.code("'\(eventCode.prefix(4))'"),
            let eventID: AEEventID = Self.code("'\(eventCode.dropFirst(4))'")
        else {
            return .failure(ScriptingFailure.absentCommand(command))
        }
        let table: [String: XMLElement] = parameters(declaration)
        let event: NSAppleEventDescriptor = NSAppleEventDescriptor(
            eventClass: eventClass,
            eventID: eventID,
            targetDescriptor: target,
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        event.setAttribute(.null(), forKeyword: AEKeyword(keySubjectAttr))
        return collect(
            arguments.sorted { left, right in left.key < right.key }.map { name, argument in
                table[name].map { parameter in encode(argument, as: Self.types(of: parameter)).map { value in (Self.code(of: parameter) ?? AEKeyword(keyDirectObject), value) } }
                    ?? .failure(ScriptingFailure.unknownParameter(name))
            },
            into: event,
        )
    }

    func encode(_ value: Descriptor, as types: Set<String>) -> Result<NSAppleEventDescriptor, any Error> {
        switch value {
            case .null:
                .success(.null())
            case .boolean(let value):
                .success(NSAppleEventDescriptor(boolean: value))
            case .number(let value):
                .success((types.contains("integer") ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value))
            case .text(let text):
                .success(
                    types.lazy.compactMap { type in enumerations[type]?[text] }.first.map(NSAppleEventDescriptor.init(enumCode:))
                        ?? (types.contains("type") ? (classes[text] ?? properties[text]).flatMap(Self.code(of:)) ?? Self.code(text) : nil).map(NSAppleEventDescriptor.init(typeCode:))
                        ?? (types == ["text"] ? nil : URL(string: text)).flatMap { url in url.isFileURL ? NSAppleEventDescriptor(fileURL: url) : nil }
                        ?? NSAppleEventDescriptor(string: text)
                )
            case .list(let items):
                collect(items.map { item in encode(item, as: types) }).map { descriptors in
                    descriptors.reduce(into: .list()) { list, descriptor in list.insert(descriptor, at: list.numberOfItems + 1) }
                }
            case .record(let fields):
                fields["want"].map { want in specifier(want, fields) } ?? record(fields)
        }
    }

    func record(_ fields: [String: Descriptor]) -> Result<NSAppleEventDescriptor, any Error> {
        let className: String? = if case .text(let name) = fields["class"] { name } else { nil }
        return collect(
            fields.sorted { left, right in left.key < right.key }.map { key, value in
                let declaration: XMLElement? = property(key, of: className)
                return (declaration.flatMap(Self.code(of:)) ?? Self.code(key)).map { keyword in
                    encode(value, as: declaration.map(Self.types(of:)) ?? []).map { descriptor in (keyword, descriptor) }
                }
                    ?? .failure(ScriptingFailure.unknownProperty(key))
            },
            into: .record(),
        )
    }

    func specifier(_ want: Descriptor, _ fields: [String: Descriptor]) -> Result<NSAppleEventDescriptor, any Error> {
        let form: FourCharCode? = if case .text(let text) = fields["form"] { Self.code(text) } else { nil }
        let container: String? = if case .record(let from) = fields["from"], case .text(let name) = from["want"] { name } else { nil }
        let key: Result<NSAppleEventDescriptor, any Error> =
            switch (form, fields["seld"]) {
                case (FourCharCode(formPropertyID), .text(let name)):
                    property(name, of: container).flatMap(Self.code(of:)).map { code in .success(NSAppleEventDescriptor(typeCode: code)) }
                        ?? .failure(ScriptingFailure.unknownProperty(name))
                case (FourCharCode(formAbsolutePosition), .text(let ordinal)):
                    Self.code(ordinal).flatMap { code in NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: code).data) }
                        .map(Result.success) ?? .failure(ScriptingFailure.unknownOrdinal(ordinal))
                case (_, let seld):
                    encode(seld ?? .null, as: ["integer"])
            }
        let keywords: [(AEKeyword, Result<NSAppleEventDescriptor, any Error>)] = [
            (AEKeyword(keyAEDesiredClass), encode(want, as: ["type"])),
            (
                AEKeyword(keyAEKeyForm),
                form.map { code in .success(NSAppleEventDescriptor(enumCode: code)) }
                    ?? .failure(ScriptingFailure.unquotedForm),
            ),
            (AEKeyword(keyAEKeyData), key),
            (AEKeyword(keyAEContainer), encode(fields["from"] ?? .null, as: [])),
        ]
        return collect(keywords.map { keyword, result in result.map { descriptor in (keyword, descriptor) } }, into: .record()).flatMap { record in
            record.coerce(toDescriptorType: DescType(typeObjectSpecifier)).map(Result.success)
                ?? .failure(ScriptingFailure.notObjectSpecifier)
        }
    }

    // --- [DECODING]

    func decode(_ descriptor: NSAppleEventDescriptor) -> Descriptor {
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
                    .text(terms[descriptor.typeCodeValue] ?? NSFileTypeForHFSTypeCode(descriptor.typeCodeValue))
                case DescType(typeEnumerated):
                    .text(enumerators[descriptor.enumCodeValue] ?? NSFileTypeForHFSTypeCode(descriptor.enumCodeValue))
                case DescType(typeAbsoluteOrdinal):
                    .text(NSFileTypeForHFSTypeCode(descriptor.enumCodeValue))
                case DescType(typeFileURL), DescType(typeAlias):
                    descriptor.fileURLValue.map { url in .text(url.absoluteString) }
                case DescType(typeLongDateTime):
                    descriptor.dateValue.map { date in .text(date.formatted(.iso8601)) }
                case DescType(typeAEList):
                    .list((0..<descriptor.numberOfItems).compactMap { index in descriptor.atIndex(index + 1) }.map(decode))
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
        let className: String? = descriptor.forKeyword(AEKeyword(pClass)).flatMap { type in terms[type.typeCodeValue] }
        return .record(
            Dictionary(
                (0..<descriptor.numberOfItems).compactMap { index in
                    descriptor.atIndex(index + 1).map { value in (term(descriptor.keywordForDescriptor(at: index + 1), of: className), decode(value)) }
                },
                uniquingKeysWith: { first, _ in first },
            )
        )
    }

    func specifier(_ descriptor: NSAppleEventDescriptor) -> Descriptor {
        let from: NSAppleEventDescriptor? = descriptor.forKeyword(AEKeyword(keyAEContainer))
        let form: FourCharCode? = descriptor.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue
        let container: String? = from?.forKeyword(AEKeyword(keyAEDesiredClass)).flatMap { want in terms[want.typeCodeValue] }
        return .record(
            [
                "want": descriptor.forKeyword(AEKeyword(keyAEDesiredClass)).map(decode),
                "form": form.map { code in .text(NSFileTypeForHFSTypeCode(code)) },
                "seld": descriptor.forKeyword(AEKeyword(keyAEKeyData)).map { key in form == FourCharCode(formPropertyID) ? .text(term(key.typeCodeValue, of: container)) : decode(key) },
                "from": from.map(decode),
            ]
            .compactMapValues(\.self)
        )
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

enum ScriptingFailure: CustomNSError {
    case runningInstances(URL)
    case terminated(URL)
    case absentCommand(String)
    case unknownParameter(String)
    case unknownProperty(String)
    case unknownOrdinal(String)
    case unquotedForm
    case notObjectSpecifier

    static var errorDomain: String { NSOSStatusErrorDomain }

    var errorCode: Int {
        switch self {
            case .runningInstances, .terminated: Int(procNotFound)
            case .absentCommand: Int(errAEEventNotHandled)
            case .unknownParameter: Int(errAEParamMissed)
            case .unknownProperty, .unknownOrdinal, .notObjectSpecifier: Int(errAECoercionFail)
            case .unquotedForm: Int(errAEBadKeyForm)
        }
    }

    var errorUserInfo: [String: Any] {
        let description: String =
            switch self {
                case .runningInstances(let url): "Execution requires exactly one running instance of \(url.path(percentEncoded: false))"
                case .terminated(let url): "\(url.path(percentEncoded: false)) terminated before it finished launching"
                case .absentCommand(let command): "Command is absent from the installed dictionary: \(command)"
                case .unknownParameter(let name): "Unknown parameter: \(name)"
                case .unknownProperty(let name): "Unknown property: \(name)"
                case .unknownOrdinal(let ordinal): "Unknown ordinal: \(ordinal)"
                case .unquotedForm: "Object specifiers require a quoted four-character form"
                case .notObjectSpecifier: "Record does not coerce to an object specifier"
            }
        return [NSLocalizedDescriptionKey: description]
    }
}

struct AggregateError<Element: Error>: LocalizedError {
    let first: Element
    let remaining: [Element]

    init?(collecting errors: [Element]) {
        guard let first: Element = errors.first else { return nil }
        self.first = first
        remaining = Array(errors.dropFirst())
    }

    var errorDescription: String? { ([first] + remaining).map(\.localizedDescription).joined(separator: "; ") }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func collect<Value>(_ results: [Result<Value, any Error>]) -> Result<[Value], any Error> {
    AggregateError(collecting: results.compactMap { result in if case .failure(let error) = result { error } else { nil } }).map(Result.failure)
        ?? .success(results.compactMap { result in if case .success(let value) = result { value } else { nil } })
}

func collect(
    _ keywords: [Result<(keyword: AEKeyword, value: NSAppleEventDescriptor), any Error>],
    into descriptor: NSAppleEventDescriptor,
) -> Result<NSAppleEventDescriptor, any Error> {
    collect(keywords).map { items in items.reduce(into: descriptor) { descriptor, item in descriptor.setDescriptor(item.value, forKeyword: item.keyword) } }
}
