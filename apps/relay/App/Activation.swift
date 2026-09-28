import AppKit
import OSLog

// --- [OPERATIONS] ----------------------------------------------------------------------

enum Activation {
    static func bringToFront() async {
        let opened: Result<NSRunningApplication, any Error> = await Result {
            try await NSWorkspace.shared.openApplication(
                at: Bundle.main.bundleURL,
                configuration: NSWorkspace.OpenConfiguration(),
            )
        }
        if case .failure(let error) = opened {
            Logger(subsystem: "app.rasm.relay", category: "Activation")
                .error("openApplication: \(String(describing: error), privacy: .public)")
        }
    }
}
