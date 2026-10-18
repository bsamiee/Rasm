import Carbon.OpenScripting
import Foundation
import RasmMacOS

// --- [MODELS] --------------------------------------------------------------------------

extension ScriptingDictionary {
    // --- [ENCODING]

    func events(
        _ command: String,
        arguments: [String: JSONValue],
        target: NSAppleEventDescriptor,
    ) -> Result<[NSAppleEventDescriptor], AggregateError<Failure>> {
        let skipWarnings: [Term] = classes.filter { term in term.code == FourCharCode(cApplication) }.flatMap(\.members).filter { term in term.name == "skip warnings" }
        let declarations: [(name: String, arguments: [String: JSONValue], property: Term?)] = skipWarnings.map { ("set", [:], $0) } + [(command, arguments, nil)]
        return traverse(declarations) { command, arguments, property in
            let wanted: Set<String> = if case .object(let fields) = arguments["direct"], case .string(let want) = fields["want"] { [want, "specifier"] } else { [] }
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
                if let property {
                    let specifier: NSAppleEventDescriptor? = put(
                        [
                            (AEKeyword(keyAEDesiredClass), NSAppleEventDescriptor(typeCode: DescType(typeProperty))),
                            (AEKeyword(keyAEKeyForm), NSAppleEventDescriptor(enumCode: FourCharCode(formPropertyID))),
                            (AEKeyword(keyAEKeyData), NSAppleEventDescriptor(typeCode: property.code)),
                            (AEKeyword(keyAEContainer), .null()),
                        ],
                        into: .record(),
                    ).coerce(toDescriptorType: DescType(typeObjectSpecifier))
                    return specifier.map { specifier in put([(AEKeyword(keyDirectObject), specifier), (AEKeyword(keyAEData), NSAppleEventDescriptor(boolean: true))], into: event) }
                        .map(Result.success) ?? .failure(AggregateError(first: .unspecified))
                }
                return encode(arguments, in: [declaration.parameters], into: event)
            } ?? .failure(AggregateError(first: .absentCommand(name: command)))
        }
    }

    func encode(_ fields: [String: JSONValue], in scopes: [[Term]], into descriptor: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        traverse(fields.sorted { left, right in left.key < right.key }) { key, value in
            Self.resolve(key, in: scopes).flatMap { term in encode(value, as: term.types).map { encoded in (term.code, encoded) } }
        }
        .map { items in put(items, into: descriptor) }
    }

    func encode(_ value: JSONValue, as types: Set<String>) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        switch value {
            case .null:
                .success(.null())
            case .bool(let value):
                .success(NSAppleEventDescriptor(boolean: value))
            case .number(let value):
                .success((types.contains("integer") ? Int32(exactly: value) : nil).map(NSAppleEventDescriptor.init(int32:)) ?? NSAppleEventDescriptor(double: value))
            case .string(let text):
                switch enumerators(of: types) {
                    case let enumerators where enumerators.contains(where: { term in term.name == text }) || !enumerators.isEmpty && Self.code(text) != nil:
                        Self.resolve(text, in: [enumerators]).map { term in NSAppleEventDescriptor(enumCode: term.code) }
                    case _ where types.contains("type"):
                        Self.resolve(text, in: [classes, properties]).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                    case _ where types.contains("date"):
                        Result { try Date.ISO8601FormatStyle().parse(text) }.map(NSAppleEventDescriptor.init(date:)).mapError { error in AggregateError(first: .unreadable(error)) }
                    default:
                        if let file: URL = URL(string: text), file.isFileURL, !types.isDisjoint(with: ["file", "alias", "file specification"]) {
                            .success(NSAppleEventDescriptor(fileURL: file))
                        } else {
                            .success(NSAppleEventDescriptor(string: text))
                        }
                }
            case .array(let items):
                traverse(items) { item in encode(item, as: types) }.map { descriptors in
                    descriptors.reduce(into: .list()) { list, descriptor in list.insert(descriptor, at: list.numberOfItems + 1) }
                }
            case .object(let fields):
                if case .string(let type) = fields["type"], case .string(let data) = fields["data"], let code: FourCharCode = Self.code(type) {
                    Data(base64Encoded: data).flatMap { data in NSAppleEventDescriptor(descriptorType: DescType(code), data: data) }.map(Result.success)
                        ?? .failure(AggregateError(first: .unspecified))
                } else if let want: JSONValue = fields["want"], case .string(let form) = fields["form"] {
                    specifier(want, form: form, fields)
                } else {
                    encode(fields, in: scope(classCode(fields["class"])), into: .record())
                }
        }
    }

    func specifier(_ want: JSONValue, form: String, _ fields: [String: JSONValue]) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        let code: FourCharCode? = Self.code(form)
        let container: FourCharCode? = if case .object(let from) = fields["from"] { classCode(from["want"]) } else { FourCharCode(cApplication) }
        let key: Result<NSAppleEventDescriptor, AggregateError<Failure>> =
            switch (code, fields["seld"]) {
                case (FourCharCode(formPropertyID), .string(let name)):
                    Self.resolve(name, in: scope(container)).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                case (FourCharCode(formAbsolutePosition), .string(let ordinal)):
                    Self.code(ordinal).flatMap { code in NSAppleEventDescriptor(descriptorType: DescType(typeAbsoluteOrdinal), data: NSAppleEventDescriptor(enumCode: code).data) }
                        .map(Result.success) ?? .failure(AggregateError(first: .unknownTerm(name: ordinal)))
                case (_, let seld):
                    encode(seld ?? .null, as: ["integer"])
            }
        let keywords: [(AEKeyword, Result<NSAppleEventDescriptor, AggregateError<Failure>>)] = [
            (AEKeyword(keyAEDesiredClass), encode(want, as: ["type"])),
            (AEKeyword(keyAEKeyForm), code.map { code in .success(NSAppleEventDescriptor(enumCode: code)) } ?? .failure(AggregateError(first: .unknownTerm(name: form)))),
            (AEKeyword(keyAEKeyData), key),
            (AEKeyword(keyAEContainer), encode(fields["from"] ?? .null, as: [])),
        ]
        return traverse(keywords) { keyword, result in result.map { descriptor in (keyword, descriptor) } }.flatMap { items in
            put(items, into: .record()).coerce(toDescriptorType: DescType(typeObjectSpecifier)).map(Result.success) ?? .failure(AggregateError(first: .unspecified))
        }
    }
}

struct AppleEventReply: Encodable {
    private enum Key: CodingKey { case want, form, seld, from, type, data, value }

    let descriptor: NSAppleEventDescriptor
    let dictionary: ScriptingDictionary

    func encode(to encoder: any Encoder) throws {
        if descriptor.descriptorType == DescType(typeObjectSpecifier) {
            var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            let from: NSAppleEventDescriptor? = descriptor.forKeyword(AEKeyword(keyAEContainer))
            let form: FourCharCode? = descriptor.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue
            let scopes: [[Term]] = dictionary.scope(from?.forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue ?? FourCharCode(cApplication))
            try container.encodeIfPresent(
                descriptor.forKeyword(AEKeyword(keyAEDesiredClass)).map { want in ScriptingDictionary.term(want.typeCodeValue, in: [dictionary.classes, dictionary.properties]).name },
                forKey: .want,
            )
            try container.encodeIfPresent(form.map(NSFileTypeForHFSTypeCode), forKey: .form)
            if form == FourCharCode(formPropertyID) {
                try container.encodeIfPresent(descriptor.forKeyword(AEKeyword(keyAEKeyData)).map { key in ScriptingDictionary.term(key.typeCodeValue, in: scopes).name }, forKey: .seld)
            } else {
                try container.encodeIfPresent(descriptor.forKeyword(AEKeyword(keyAEKeyData)).map { Self(descriptor: $0, dictionary: dictionary) }, forKey: .seld)
            }
            try container.encodeIfPresent(from.map { Self(descriptor: $0, dictionary: dictionary) }, forKey: .from)
            return
        }
        let value: any Encodable? =
            switch descriptor.descriptorType {
                case DescType(typeNull): JSONValue.null
                case DescType(typeTrue), DescType(typeFalse), DescType(typeBoolean): descriptor.booleanValue
                case DescType(typeSInt16), DescType(typeSInt32), DescType(typeUInt16), DescType(typeUInt32):
                    descriptor.doubleValue
                case let type where [DescType(typeIEEE32BitFloatingPoint), DescType(typeIEEE64BitFloatingPoint)].contains(type) && descriptor.doubleValue.isFinite: descriptor.doubleValue
                case DescType(typeChar), DescType(typeCString), DescType(typePString), DescType(typeUnicodeText), DescType(typeUTF8Text), DescType(typeUTF16ExternalRepresentation):
                    descriptor.stringValue
                case DescType(typeAEList): (0..<descriptor.numberOfItems).compactMap { descriptor.atIndex($0 + 1) }.map { Self(descriptor: $0, dictionary: dictionary) }
                case DescType(typeAERecord):
                    Dictionary(
                        uniqueKeysWithValues: (0..<descriptor.numberOfItems).compactMap { index in
                            descriptor.atIndex(index + 1).map { value in
                                let term: Term = ScriptingDictionary.term(
                                    descriptor.keywordForDescriptor(at: index + 1),
                                    in: dictionary.scope(descriptor.forKeyword(AEKeyword(pClass))?.typeCodeValue),
                                )
                                return (term.name, Self(descriptor: value, dictionary: dictionary))
                            }
                        }
                    )
                default: nil
            }
        if let value {
            try value.encode(to: encoder)
        } else {
            var container: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
            try container.encode(NSFileTypeForHFSTypeCode(descriptor.descriptorType), forKey: .type)
            try container.encode(descriptor.data.base64EncodedString(), forKey: .data)
            let value: String? =
                switch descriptor.descriptorType {
                    case DescType(typeType): ScriptingDictionary.term(descriptor.typeCodeValue, in: [dictionary.classes, dictionary.properties]).name
                    case DescType(typeEnumerated): ScriptingDictionary.term(descriptor.enumCodeValue, in: [dictionary.enumerations.flatMap(\.members)]).name
                    case DescType(typeFileURL), DescType(typeAlias): descriptor.fileURLValue?.absoluteString
                    case DescType(typeLongDateTime): descriptor.dateValue?.formatted(.iso8601)
                    default: descriptor.stringValue
                }
            try container.encodeIfPresent(value, forKey: .value)
        }
    }
}
