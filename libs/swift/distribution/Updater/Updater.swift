public import AppKit
import Logging
import Security
public import Sparkle

// --- [SERVICES] ------------------------------------------------------------------------

@MainActor
public struct Updater {
    private final class Delegate: NSObject, SPUUpdaterDelegate, SPUStandardUserDriverDelegate {
        private let resting: NSApplication.ActivationPolicy

        init(resting: NSApplication.ActivationPolicy) { self.resting = resting }

        var supportsGentleScheduledUpdateReminders: Bool { true }

        func updater(_: SPUUpdater, willInstallUpdateOnQuit _: SUAppcastItem, immediateInstallationBlock: () -> Void) -> Bool {
            guard resting == .prohibited else { return false }
            immediateInstallationBlock()
            return true
        }

        func updater(_: SPUUpdater, didFinishUpdateCycleFor _: SPUUpdateCheck, error _: (any Error)?) {
            var code: SecCode?
            let copied = unsafe SecCodeCopySelf([], &code)
            guard (code.map { code in SecCodeCheckValidity(code, [], nil) } ?? copied) != errSecSuccess else { return }
            let background = NSWorkspace.OpenConfiguration()
            background.activates = false
            NSWorkspace.shared.openApplication(at: Bundle.main.bundleURL, configuration: background) { _, error in
                if let error { Logger(label: String(reflecting: Updater.self)).error("Installed update could not be opened over the running process", error: error) }
            }
        }

        func standardUserDriverWillHandleShowingUpdate(_: Bool, forUpdate _: SUAppcastItem, state: SPUUserUpdateState) {
            NSApplication.shared.setActivationPolicy(.regular)
            if !state.userInitiated { NSApplication.shared.dockTile.badgeLabel = "1" }
        }

        func standardUserDriverWillFinishUpdateSession() {
            NSApplication.shared.dockTile.badgeLabel = nil
            NSApplication.shared.setActivationPolicy(resting)
        }
    }

    public let controller: SPUStandardUpdaterController
    private let delegate: Delegate

    public init(resting: NSApplication.ActivationPolicy) {
        delegate = Delegate(resting: resting)
        controller = SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: delegate, userDriverDelegate: delegate)
    }
}
