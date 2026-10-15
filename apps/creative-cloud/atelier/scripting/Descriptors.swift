import Carbon.OpenScripting
import Foundation
import RasmMacOS

// --- [MODELS] --------------------------------------------------------------------------

enum Descriptor: Codable, Sendable {
    case null
    case boolean(Bool)
    case number(Double)
    case text(String)
    case list([Self])
    case record([String: Self])

    init(from decoder: any Decoder) throws {
        let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        self =
            try container.decodeNil()
            ? .null
            : Result { try .boolean(container.decode(Bool.self)) }
                .flatMapError { _ in Result { try .number(container.decode(Double.self)) } }
                .flatMapError { _ in Result { try .text(container.decode(String.self)) } }
                .flatMapError { _ in Result { try .list(container.decode([Self].self)) } }
                .flatMapError { _ in Result { try .record(container.decode([String: Self].self)) } }
                .get()
    }

    func encode(to encoder: any Encoder) throws {
        var container: any SingleValueEncodingContainer = encoder.singleValueContainer()
        switch self {
            case .null: try container.encodeNil()
            case .boolean(let value): try container.encode(value)
            case .number(let value): try container.encode(value)
            case .text(let value): try container.encode(value)
            case .list(let value): try container.encode(value)
            case .record(let value): try container.encode(value)
        }
    }
}

extension ScriptingDictionary {
    // --- [ENCODING]

    func events(
        _ command: String,
        arguments: [String: Descriptor],
        target: NSAppleEventDescriptor,
    ) -> Result<[(event: NSAppleEventDescriptor, result: Set<String>)], AggregateError<Failure>> {
        let skipWarnings: [(String, [String: Descriptor])] =
            classes.contains { term in term.code == FourCharCode(cApplication) && term.members.contains { member in member.name == "skip warnings" } }
            ? [("set", ["direct": .record(["want": .text("'prop'"), "form": .text("'prop'"), "seld": .text("skip warnings")]), "to": .boolean(true)])] : []
        return traverse(skipWarnings + [(command, arguments)]) { command, arguments in
            let wanted: Set<String> = if case .record(let fields) = arguments["direct"], case .text(let want) = fields["want"] { [want, "specifier"] } else { [] }
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
            } ?? .failure(AggregateError(first: .absentCommand(name: command)))
        }
    }

    func encode(_ fields: [String: Descriptor], in scopes: [[Term]], into descriptor: NSAppleEventDescriptor) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        traverse(fields.sorted { left, right in left.key < right.key }) { key, value in
            Self.resolve(key, in: scopes).flatMap { term in encode(value, as: term.types).map { encoded in (term.code, encoded) } }
        }
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
                switch enumerators(of: types) {
                    case let enumerators where enumerators.contains(where: { term in term.name == text }) || !enumerators.isEmpty && Self.code(text) != nil:
                        Self.resolve(text, in: [enumerators]).map { term in NSAppleEventDescriptor(enumCode: term.code) }
                    case _ where types.contains("type"):
                        Self.resolve(text, in: [classes, properties]).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                    default:
                        if let file: URL = URL(string: text), file.isFileURL, !types.isDisjoint(with: ["file", "alias", "file specification"]) {
                            .success(NSAppleEventDescriptor(fileURL: file))
                        } else {
                            .success(NSAppleEventDescriptor(string: text))
                        }
                }
            case .list(let items):
                traverse(items) { item in encode(item, as: types) }.map { descriptors in
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

    func specifier(_ want: Descriptor, form: String, _ fields: [String: Descriptor]) -> Result<NSAppleEventDescriptor, AggregateError<Failure>> {
        let code: FourCharCode? = Self.code(form)
        let container: FourCharCode? = if case .record(let from) = fields["from"] { classCode(from["want"]) } else { FourCharCode(cApplication) }
        let key: Result<NSAppleEventDescriptor, AggregateError<Failure>> =
            switch (code, fields["seld"]) {
                case (FourCharCode(formPropertyID), .text(let name)):
                    Self.resolve(name, in: scope(container)).map { term in NSAppleEventDescriptor(typeCode: term.code) }
                case (FourCharCode(formAbsolutePosition), .text(let ordinal)):
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
