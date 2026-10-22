public import AppKit
public import AppleEvents
public import Applications
import Carbon.OpenScripting
public import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

public enum AdobeFailure: LocalizedError {
    case notAdobe(URL)
    case noProcess(pid_t)

    public var errorDescription: String? {
        switch self {
            case .notAdobe(let url): "\(url.path(percentEncoded: false)) has no bundle identifier starting \(Adobe.identifierPrefix)"
            case .noProcess(let pid): "No running application bundle has pid \(pid)"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum Adobe: Sendable {
    public static let identifierPrefix: String = "com.adobe."

    public static func installation(at url: URL) -> Result<Installation, any Error> {
        Installation.at(url)
            .mapError { error -> any Error in error }
            .flatMap { installation in installation.identifier.hasPrefix(identifierPrefix) ? .success(installation) : .failure(AdobeFailure.notAdobe(url)) }
    }

    public static func running(_ pid: pid_t) -> Result<(instance: NSRunningApplication, installation: Installation), any Error> {
        switch NSRunningApplication(processIdentifier: pid).flatMap({ instance in instance.bundleURL.map { url in (instance, url) } }) {
            case .some((let instance, let url)): installation(at: url).map { installation in (instance, installation) }
            case .none: .failure(AdobeFailure.noProcess(pid))
        }
    }
}

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
        let eventIDs: [AEEventID?] = [AEEventID(sdef: "DjxM"), AEEventID(kAEDoScript)]
        return eventIDs.lazy.compactMap { id in self.commands.first { command in command.eventID == id } }.first.map { command in
            let arguments: [String: AppleEventValue] =
                command.parameters.contains { term in term.code == FourCharCode(sdef: "doLg") } ? ["direct": .text(source), "doLg": .enumerator("JSLg")] : ["direct": .text(source)]
            return (command, arguments)
        }
    }
}
