import Carbon.OpenScripting
public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum EventClause: Sendable {
    public enum Values: Sendable {
        case any
        case among(Set<String?>)
        case except(String)

        func contain(_ descriptor: NSAppleEventDescriptor?) -> Bool {
            let value: String? = descriptor.map { descriptor in
                descriptor.descriptorType == DescType(typeEnumerated)
                    ? descriptor.enumCodeValue.sdef
                    : descriptor.coerce(toDescriptorType: DescType(typeBoolean)).map { boolean in DescType(boolean.booleanValue ? typeTrue : typeFalse).sdef } ?? descriptor.descriptorType.sdef
            }
            return switch self {
                case .any: true
                case .among(let listed): listed.contains(value)
                case .except(let excluded): value != excluded
            }
        }
    }

    case always
    case parameter(String, Values)
    case property(String, of: Set<String>?, Values)
    case direct(of: Set<String>)

    func matches(_ event: NSAppleEventDescriptor) -> Bool {
        let direct: NSAppleEventDescriptor? = event.paramDescriptor(forKeyword: AEKeyword(keyDirectObject))
        switch self {
            case .always:
                return true
            case .parameter(let keyword, let values):
                return values.contain(FourCharCode(sdef: keyword).flatMap(event.paramDescriptor(forKeyword:)))
            case .property(let property, let containers, let values):
                let data: NSAppleEventDescriptor? = event.paramDescriptor(forKeyword: AEKeyword(keyAEData))
                let assigned: NSAppleEventDescriptor? =
                    switch direct?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue {
                        case FourCharCode(sdef: property): data
                        case FourCharCode(pProperties): FourCharCode(sdef: property).flatMap { code in data?.forKeyword(code) }
                        case .some, .none: nil
                    }
                let container: String? = direct?.forKeyword(AEKeyword(keyAEContainer))?.forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue.sdef
                return direct?.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue == FourCharCode(formPropertyID) && assigned != nil
                    && containers.map { containers in container.map(containers.contains) == true } != false && values.contain(assigned)
            case .direct(let classes):
                return direct?.forKeyword(AEKeyword(keyAEDesiredClass)).map { want in classes.contains(want.typeCodeValue.sdef) } == true
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ScriptingDictionary.Command {
    public func reason<Reason>(for event: NSAppleEventDescriptor, among rows: [(code: String, clause: EventClause, reason: Reason)]) -> Reason? {
        rows.first { row in row.code == code.sdef && row.clause.matches(event) }?.reason
    }
}
