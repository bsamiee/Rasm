@unsafe @preconcurrency import ApplicationServices
package import Darwin

// --- [MODELS] --------------------------------------------------------------------------

public enum Grant: String, CaseIterable, Encodable, Sendable {
    case accessibility
    case screenCapture
    case postEvent

    public func isHeld(asking: Bool) -> Bool {
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
        return ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: responsible == process ? .noGrant(self) : .notResponsible(self, responsible))
    }

    public func check() throws(ApplicationFailure) {
        if let missing { throw missing }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("responsibility_get_pid_responsible_for_pid")
package func responsiblePID(_: pid_t) -> pid_t
