import Carbon.OpenScripting
import CasePaths
public import CoreServices
public import Foundation
import OrderedCollections
public import Results

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ScriptingDictionary {
    // --- [COMMANDS]
    public func command(_ query: String, taking arguments: Set<String>) throws(Unmatched) -> Command {
        let code: UInt64? = UInt64(sdef: query)
        let matching: (Command) -> Bool = { command in command.name == query || command.code == code }
        let takes: (Command) -> Bool = { command in
            arguments.allSatisfy { argument in command.parameters.contains { term in term.name == argument || term.code == FourCharCode(sdef: argument) } }
        }
        let scopes: [[Command]] = [Command.language, commands].map { scope in scope.filter(takes) }
        let found: [Command] = scopes.first { scope in scope.contains(where: matching) }?.filter(matching) ?? []
        let subject: String = arguments.isEmpty ? "command" : "command taking \(arguments.sorted().joined(separator: ", "))"
        return try Unmatched.single(found, subject: subject, query: query, candidates: OrderedSet(scopes.joined().map(\.name)).elements, named: \.code.sdef).get()
    }

    public func command(handling eventIDs: [AEEventID]) throws(Unmatched) -> Command {
        guard let handler: Command = eventIDs.lazy.compactMap({ eventID in self.commands.first { command in command.eventID == eventID } }).first else {
            throw Unmatched(subject: "command handling \(eventIDs.map(\.sdef).joined(separator: ", "))", query: nil, candidates: OrderedSet(commands.map(\.name)).elements)
        }
        return handler
    }

    // --- [EVENTS]
    public func event(
        _ command: Command,
        arguments: [String: AppleEventValue],
        target: NSAppleEventDescriptor,
        refusing activating: Set<UInt64> = [],
    ) throws(AggregateError<AppleEventFailure>) -> NSAppleEventDescriptor {
        let event: NSAppleEventDescriptor = try record(
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
        let direct: NSAppleEventDescriptor? = event.paramDescriptor(forKeyword: AEKeyword(keyDirectObject))
        let assigned: Bool =
            switch direct?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue {
                case FourCharCode(pIsFrontProcess): true
                case FourCharCode(pProperties): event.paramDescriptor(forKeyword: AEKeyword(keyAEData))?.forKeyword(FourCharCode(pIsFrontProcess)) != nil
                case .some, .none: false
            }
        let frontmost: Bool =
            command.eventClass == AEEventClass(kAECoreSuite) && command.eventID == AEEventID(kAESetData)
            && direct?.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue == FourCharCode(formPropertyID) && assigned
        guard !activating.contains(command.code), !frontmost else {
            throw AggregateError(first: AppleEventFailure(subject: "\(command.name) (\(command.code.sdef))", reason: .activating))
        }
        return event
    }

    func record(_ fields: [String: AppleEventValue], in scopes: [Scope], into descriptor: NSAppleEventDescriptor) throws(AggregateError<AppleEventFailure>) -> NSAppleEventDescriptor {
        try traverse(fields.sorted { left, right in left.key < right.key }) { key, value in
            Result { () throws(AggregateError<AppleEventFailure>) in
                let term: Term = try Self.resolve(key, in: scopes).get()
                return (term.code, try value.descriptor(as: term.types, in: self))
            }
        }
        .get()
        .reduce(into: descriptor) { filled, item in filled.setDescriptor(item.1, forKeyword: item.0) }
    }

    // --- [TERMS]
    static func resolve(_ name: String, in scopes: [Scope]) -> Result<Term, AggregateError<AppleEventFailure>> {
        let named: [Term] = scopes.lazy.compactMap { scope in scope.named[name] }.first ?? []
        let codes: [FourCharCode] = named.isEmpty ? [FourCharCode(sdef: name)].compactMap(\.self) : OrderedSet(named.map(\.code)).elements
        return Unmatched.single(codes, subject: "term", query: name, candidates: OrderedSet(scopes.flatMap(\.named.keys)).elements, named: \.sdef)
            .map { code in Term(name: name, code: code, types: Set(named.flatMap(\.types)), members: []) }
            .aggregated { miss in AppleEventFailure(subject: name, reason: .unmatched(miss)) }
    }

    static func term(_ code: FourCharCode, in scopes: [Scope]) -> Term {
        let found: [Term] = scopes.lazy.compactMap { scope in scope.coded[code] }.first ?? []
        let name: String? = found.first { term in resolve(term.name, in: scopes)[case: \.success]?.code == code }?.name
        return Term(name: name ?? code.sdef, code: code, types: Set(found.flatMap(\.types)), members: [])
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
