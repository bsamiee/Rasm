import AppKit
import AppleEvents
import Applications
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

let adobePrefix: String = "com.adobe."

let activatingCommands: KeyValuePairs<String, [String]> = [
    "\(adobePrefix)InDesign": ["UIUIshow", "K2LKrvlO", "K2LKrvlB"],
    "\(adobePrefix)Acrobat.Pro": ["CARObfrt"],
    "\(adobePrefix)PremierePro": ["CR8RCRPr"],
]

// --- [OPERATIONS] ----------------------------------------------------------------------

func adobeInstallation(at url: URL) throws(ApplicationFailure) -> Installation {
    let installation: Installation = try Installation(at: url)
    guard installation.identifier.hasPrefix(adobePrefix) else {
        throw ApplicationFailure(subject: url.path(percentEncoded: false), reason: .absent("bundle identifier starting \(adobePrefix)"))
    }
    return installation
}

func running(_ pid: pid_t) throws(ApplicationFailure) -> (instance: NSRunningApplication, installation: Installation) {
    guard let instance: NSRunningApplication = NSRunningApplication(processIdentifier: pid), let url: URL = instance.bundleURL else {
        throw ApplicationFailure(subject: String(pid), reason: .noProcess)
    }
    return (instance, try adobeInstallation(at: url))
}
