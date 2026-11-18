@unsafe @preconcurrency import ApplicationServices

// --- [MODELS] --------------------------------------------------------------------------

public enum Grant: String, Encodable, Sendable {
    case accessibility
    case screenCapture
    case postEvent

    public var held: Bool {
        switch self {
            case .accessibility: AXIsProcessTrusted()
            case .screenCapture: CGPreflightScreenCaptureAccess()
            case .postEvent: CGPreflightPostEventAccess()
        }
    }

    public func check() throws(ApplicationFailure) {
        guard held else { throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .noGrant(self)) }
    }

    public func request() {
        switch self {
            case .accessibility: _ = unsafe AXIsProcessTrustedWithOptions([kAXTrustedCheckOptionPrompt.takeUnretainedValue(): true] as CFDictionary)
            case .screenCapture: _ = CGRequestScreenCaptureAccess()
            case .postEvent: _ = CGRequestPostEventAccess()
        }
    }
}
