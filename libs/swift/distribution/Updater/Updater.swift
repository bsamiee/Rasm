public import AppKit
import Logging
import Processes
public import Sparkle
import System

// --- [SERVICES] ------------------------------------------------------------------------

@MainActor
public struct Updater {
    private final class Delegate: NSObject, SPUUpdaterDelegate, SPUStandardUserDriverDelegate {
        // --- [UPDATE_CYCLE]

        let logger = Logger(label: String(reflecting: Updater.self))

        func updater(_: SPUUpdater, didFinishUpdateCycleFor _: SPUUpdateCheck, error _: (any Error)?) {
            let running = Result { () throws(Errno) in try ProcessInstance(of: getpid()).runsInstalledCode() }
            guard running != .success(true) else { return }
            if case .failure(let error) = running { logger.notice("Running code could not be checked against the installed bundle", error: error) }
            let background = NSWorkspace.OpenConfiguration()
            background.activates = false
            NSWorkspace.shared.openApplication(at: Bundle.main.bundleURL, configuration: background) { [logger] _, error in
                if let error { logger.error("Installed bundle could not be opened over the running process", error: error) }
            }
        }

        // --- [USER_DRIVER]

        private lazy var activationPolicy: NSApplication.ActivationPolicy = NSApplication.shared.activationPolicy()

        var supportsGentleScheduledUpdateReminders: Bool { true }

        func standardUserDriverWillHandleShowingUpdate(_: Bool, forUpdate _: SUAppcastItem, state: SPUUserUpdateState) {
            if activationPolicy != .regular { NSApplication.shared.setActivationPolicy(.regular) }
            if !state.userInitiated { NSApplication.shared.dockTile.badgeLabel = "1" }
        }

        func standardUserDriverWillFinishUpdateSession() {
            NSApplication.shared.dockTile.badgeLabel = nil
            NSApplication.shared.setActivationPolicy(activationPolicy)
        }
    }

    public let updater: SPUUpdater
    private let delegate = Delegate()

    public init() {
        updater = SPUUpdater(hostBundle: .main, applicationBundle: .main, userDriver: SPUStandardUserDriver(hostBundle: .main, delegate: delegate), delegate: delegate)
        do { try updater.start() } catch { delegate.logger.error("Updater could not start", error: error) }
    }
}
