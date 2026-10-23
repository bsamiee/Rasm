import Carbon.OpenScripting
public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public struct Activation: LocalizedError, Sendable {
    public let command: ScriptingDictionary.Command

    public var errorDescription: String? { "Activating command \(command.name) (\(command.code.sdef)) refused" }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ScriptingDictionary.Command {
    public func refusingActivation(of event: NSAppleEventDescriptor, among activating: Set<UInt64>) -> Result<NSAppleEventDescriptor, Activation> {
        let direct: NSAppleEventDescriptor? = event.paramDescriptor(forKeyword: AEKeyword(keyDirectObject))
        let assigned: Bool =
            switch direct?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue {
                case FourCharCode(pIsFrontProcess): true
                case FourCharCode(pProperties): event.paramDescriptor(forKeyword: AEKeyword(keyAEData))?.forKeyword(FourCharCode(pIsFrontProcess)) != nil
                case .some, .none: false
            }
        let frontmost: Bool =
            eventClass == AEEventClass(kAECoreSuite) && eventID == AEEventID(kAESetData) && direct?.forKeyword(AEKeyword(keyAEKeyForm))?.enumCodeValue == FourCharCode(formPropertyID) && assigned
        return activating.contains(code) || frontmost ? .failure(Activation(command: self)) : .success(event)
    }
}
