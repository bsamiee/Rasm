@unsafe @preconcurrency import ApplicationServices
package import Darwin

// --- [MODELS] --------------------------------------------------------------------------

public enum Grant: String, CaseIterable, Encodable, Sendable {
    case accessibility
    case screenCapture
    case postEvent

    public var isHeld: Bool {
        switch self {
            case .accessibility: AXIsProcessTrusted()
            case .screenCapture: CGPreflightScreenCaptureAccess()
            case .postEvent: CGPreflightPostEventAccess()
        }
    }

    var missing: ApplicationFailure {
        let (process, responsible) = (getpid(), responsiblePID(getpid()))
        return ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: responsible == process ? .noGrant(self) : .notResponsible(self, responsible))
    }

    public func check() throws(ApplicationFailure) {
        guard isHeld else { throw missing }
    }

    public func request() {
        switch self {
            case .accessibility: _ = unsafe AXIsProcessTrustedWithOptions([kAXTrustedCheckOptionPrompt.takeUnretainedValue(): true] as CFDictionary)
            case .screenCapture: _ = CGRequestScreenCaptureAccess()
            case .postEvent: _ = CGRequestPostEventAccess()
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("responsibility_get_pid_responsible_for_pid")
package func responsiblePID(_: pid_t) -> pid_t
