import AppKit
import AppleEvents
@unsafe @preconcurrency import ApplicationServices
import Darwin
import Foundation
import SPI

// --- [MODELS] --------------------------------------------------------------------------

package enum Grant: String, CaseIterable, CustomStringConvertible, Encodable, Sendable {
    case accessibility
    case screenCapture
    case postEvent

    package func isHeld(asking: Bool) -> Bool {
        switch (self, asking) {
            case (.accessibility, _): unsafe AXIsProcessTrustedWithOptions([kAXTrustedCheckOptionPrompt.takeUnretainedValue(): asking] as CFDictionary)
            case (.screenCapture, false): CGPreflightScreenCaptureAccess()
            case (.screenCapture, true): CGRequestScreenCaptureAccess()
            case (.postEvent, false): CGPreflightPostEventAccess()
            case (.postEvent, true): CGRequestPostEventAccess()
        }
    }

    package var description: String {
        let anchor: String =
            switch self {
                case .accessibility, .postEvent: "Privacy_Accessibility"
                case .screenCapture: "Privacy_ScreenCapture"
            }
        return "\(rawValue) \(privacySettings)?\(anchor)"
    }

    var missing: ApplicationFailure? {
        guard !isHeld(asking: false) else { return nil }
        let (process, responsible) = (getpid(), responsiblePID(getpid()))
        let parent: String? = NSRunningApplication(processIdentifier: responsible)?.localizedName
        return ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: responsible == process ? .noGrant(self) : .parentLacksGrant(self, parent))
    }

    package func check() throws(ApplicationFailure) {
        if let missing { throw missing }
    }
}
