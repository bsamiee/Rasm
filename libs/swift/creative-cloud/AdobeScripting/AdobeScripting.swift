public import AppleEvents
public import Applications
import Carbon.OpenScripting

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    private static let activating: KeyValuePairs<String, Set<UInt64>> = [
        "com.adobe.InDesign": Set(["UIUIshow", "K2LKrvlO", "K2LKrvlB"].compactMap(UInt64.init(sdef:))),
        "com.adobe.Acrobat.Pro": Set(["CARObfrt"].compactMap(UInt64.init(sdef:))),
        "com.adobe.PremierePro": Set(["CR8RCRPr"].compactMap(UInt64.init(sdef:))),
    ]

    public var activating: Set<UInt64> {
        Self.activating.first { stem, _ in identifier.hasPrefix(stem) }?.value ?? []
    }
}

extension ScriptingDictionary {
    public func script(running source: String) -> (command: Command, arguments: [String: AppleEventValue])? {
        [AEEventID(sdef: "DjxM"), AEEventID(kAEDoScript)].lazy.compactMap { id in self.commands.first { command in command.eventID == id } }.first.map { command in
            (command, command.parameters.contains { term in term.code == FourCharCode(sdef: "doLg") } ? ["direct": .text(source), "doLg": .enumerator("JSLg")] : ["direct": .text(source)])
        }
    }
}
