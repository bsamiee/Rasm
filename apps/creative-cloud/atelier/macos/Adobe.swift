import AppKit
import AppleEvents
import Applications
import Carbon.OpenScripting
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

let adobePrefix: String = "com.adobe."

let activatingCommands: KeyValuePairs<String, [String]> = [
    "com.adobe.InDesign": ["UIUIshow", "K2LKrvlO", "K2LKrvlB"],
    "com.adobe.Acrobat.Pro": ["CARObfrt"],
    "com.adobe.PremierePro": ["CR8RCRPr"],
]

let scriptEvents: [AEEventID] = [AEEventID(sdef: "DjxM"), AEEventID(kAEDoScript)].compactMap(\.self)

let scriptArguments: [String: AppleEventValue] = ["doLg": .enumerator("JSLg")]

// --- [OPERATIONS] ----------------------------------------------------------------------

func adobeInstallation(at url: URL) throws -> Installation {
    let installation: Installation = try Installation(at: url)
    guard installation.identifier.hasPrefix(adobePrefix) else { throw AtelierFailure(subject: url.path(percentEncoded: false), reason: .notAdobe) }
    return installation
}

func running(_ pid: pid_t) throws -> (instance: NSRunningApplication, installation: Installation) {
    guard let instance: NSRunningApplication = NSRunningApplication(processIdentifier: pid), let url: URL = instance.bundleURL else {
        throw AtelierFailure(subject: String(pid), reason: .noProcess)
    }
    return (instance, try adobeInstallation(at: url))
}

func activating(of installation: Installation) -> Set<UInt64> {
    Set(activatingCommands.first { stem, _ in installation.identifier.hasPrefix(stem) }?.value.compactMap(UInt64.init(sdef:)) ?? [])
}
