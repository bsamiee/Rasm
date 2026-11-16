public import AppKit
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
