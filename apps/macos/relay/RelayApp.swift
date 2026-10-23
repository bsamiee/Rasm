import AppKit
import CurrentProcess
import Observation
import Processes
import Results
import Sparkle
import SwiftUI
import os

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct RelayApp: App {
    static let settingsWindow: String = "settings"

    @NSApplicationDelegateAdaptor(AppDelegate.self) private var delegate: AppDelegate
    @SwiftUI.Environment(\.openWindow) private var openWindow: OpenWindowAction
    private let userDriverDelegate: UserDriverDelegate = UserDriverDelegate()
    private let updater: SPUStandardUpdaterController

    init() {
        updater = SPUStandardUpdaterController(startingUpdater: true, updaterDelegate: nil, userDriverDelegate: userDriverDelegate)
    }

    var body: some Scene {
        MenuBarExtra {
            MenuBarExtraContent(store: delegate.store, updater: updater.updater)
        } label: {
            Image(.relaySymbol)
                .accessibilityLabel("Relay")
        }
        .menuBarExtraStyle(.window)

        Window("Settings", id: Self.settingsWindow) {
            switch delegate.store {
                case .success(let store): SettingsView(store: store)
                case .failure(let error): Text(error.localizedDescription).padding(24)
                case .none: ProgressView().controlSize(.small)
            }
        }
        .defaultSize(width: 680, height: 460)
        .windowResizability(.contentMinSize)
        .windowToolbarStyle(.unified)
        .commands {
            CommandGroup(replacing: .appSettings) {
                Button("Settings") { openWindow(id: Self.settingsWindow) }
                    .keyboardShortcut(",", modifiers: .command)
            }
        }
    }
}

@Observable
final class AppDelegate: NSObject, NSApplicationDelegate {
    private(set) var store: Result<AccountStore, LoginShellFailure>?
    @ObservationIgnored private var launch: Task<Void, Never>?

    func applicationDidFinishLaunching(_: Notification) {
        let logger: Logger = Logger(for: AppDelegate.self)
        launch = Task(name: "Launch Relay") { [self] in
            switch await LoginShell.environment(within: .seconds(5)).map(AccountStore.init(environment:)) {
                case .failure(.run(.deadline(.cancelled))): logger.info("Quit canceled launch during login shell read")
                case let launched:
                    store = launched
                    switch launched {
                        case .success(let started):
                            logger.notice("Launched with login shell environment")
                            started.start()
                        case .failure(let error): logger.error("Login shell environment read failed with \(error, privacy: .public), launch stopped")
                    }
            }
        }
    }

    func applicationShouldTerminate(_ sender: NSApplication) -> NSApplication.TerminateReply {
        Task(name: "Quit Relay") { [self] in
            launch?.cancel()
            await launch?.value
            if case .success(let launched) = store { await launched.stop() }
            sender.reply(toApplicationShouldTerminate: true)
        }
        return .terminateLater
    }
}

final class UserDriverDelegate: NSObject, SPUStandardUserDriverDelegate {
    var supportsGentleScheduledUpdateReminders: Bool { true }

    func standardUserDriverWillHandleShowingUpdate(_: Bool, forUpdate _: SUAppcastItem, state: SPUUserUpdateState) {
        NSApplication.shared.setActivationPolicy(.regular)
        if !state.userInitiated { NSApplication.shared.dockTile.badgeLabel = "1" }
    }

    func standardUserDriverWillFinishUpdateSession() {
        NSApplication.shared.dockTile.badgeLabel = nil
        NSApplication.shared.setActivationPolicy(.accessory)
    }
}
