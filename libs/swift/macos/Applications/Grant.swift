import AppKit
import AppleEvents
@unsafe @preconcurrency import ApplicationServices
import Darwin
import Foundation
import SPI

// --- [MODELS] --------------------------------------------------------------------------

package enum Grant: String, CaseIterable, Encodable, Sendable {
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

    var missing: ApplicationFailure? {
        guard !isHeld(asking: false) else { return nil }
        let (process, responsible) = (getpid(), responsiblePID(getpid()))
        let parent: String? = NSRunningApplication(processIdentifier: responsible)?.localizedName
        let pane: String =
            switch self {
                case .accessibility, .postEvent: "Accessibility"
                case .screenCapture: "ScreenCapture"
            }
        let settings: String = "\(privacySettings)?Privacy_\(pane)"
        let reason: ApplicationFailure.Reason =
            responsible == process ? .noGrant(pane: pane, settings: settings) : .parentLacksGrant(pane: pane, parent: parent, settings: settings)
        return ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: reason)
    }

    package func check() throws(ApplicationFailure) {
        if let missing { throw missing }
    }
}
