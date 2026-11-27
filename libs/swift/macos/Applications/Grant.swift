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

    private var section: (anchor: String, key: String) {
        switch self {
            case .accessibility, .postEvent: ("Privacy_Accessibility", "ACCESSIBILITY")
            case .screenCapture: ("Privacy_ScreenCapture", "SCREENANDAUDIOCAPTURE")
        }
    }

    var settings: String { "\(privacySettings)?\(section.anchor)" }

    var pane: String {
        let privacy = FileManager.default.urls(for: .libraryDirectory, in: .systemDomainMask).first.flatMap { library in
            Bundle(url: library.appending(components: "ExtensionKit", "Extensions", "SecurityPrivacyExtension.appex", directoryHint: .isDirectory))
        }
        return privacy?.localizedString(forKey: section.key, value: nil, table: nil) ?? section.key
    }

    var missing: ApplicationFailure? {
        guard !isHeld(asking: false) else { return nil }
        let (process, responsible) = (getpid(), responsiblePID(getpid()))
        return ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: responsible == process ? .noGrant(self) : .notResponsible(self, responsible))
    }

    package func check() throws(ApplicationFailure) {
        if let missing { throw missing }
    }
}
