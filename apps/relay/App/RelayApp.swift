import AppKit
import OSLog
import Observation
import SwiftUI

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct RelayApp: App {
    static let settingsWindow: String = "settings"

    @NSApplicationDelegateAdaptor(AppDelegate.self) private var delegate: AppDelegate
    @Environment(\.openWindow) private var openWindow: OpenWindowAction

    var body: some Scene {
        MenuBarExtra {
            if let store: AccountStore = delegate.store {
                MenuBarExtraContent(store: store)
            } else {
                ProgressView().controlSize(.small).padding(16)
            }
        } label: {
            Image(.relaySymbol)
                .accessibilityLabel("Relay")
        }
        .menuBarExtraStyle(.window)

        Window("Settings", id: Self.settingsWindow) {
            if let store: AccountStore = delegate.store {
                SettingsView(store: store)
            } else {
                ProgressView().controlSize(.small)
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
private final class AppDelegate: NSObject, NSApplicationDelegate {
    private(set) var store: AccountStore?
    @ObservationIgnored private var launch: Task<Void, Never>?
    @ObservationIgnored private let logger: Logger = Logger(subsystem: "app.rasm.relay", category: "Launch")

    func applicationDidFinishLaunching(_: Notification) {
        launch = Task(name: "Launch Relay") { [self] in
            let process: [String: String] = ProcessInfo.processInfo.environment
            let environment: [String: String]
            switch await LoginShell.exports(over: process) {
                case .success(let exported): environment = exported
                case .failure(.run(.cancelled)): return
                case .failure(let error):
                    logger.error("Login shell exports: \(String(describing: error), privacy: .public)")
                    environment = process
            }
            let launched: AccountStore = AccountStore(environment: environment)
            store = launched
            launched.start()
        }
    }

    func applicationShouldTerminate(_ sender: NSApplication) -> NSApplication.TerminateReply {
        Task(name: "Quit Relay") { [self] in
            launch?.cancel()
            await launch?.value
            await store?.stop()
            sender.reply(toApplicationShouldTerminate: true)
        }
        return .terminateLater
    }
}
