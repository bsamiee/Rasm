import AppKit
import Observation
import Processes
import Results
import SwiftUI

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct RelayApp: App {
    static let settingsWindow: String = "settings"

    @NSApplicationDelegateAdaptor(AppDelegate.self) private var delegate: AppDelegate
    @SwiftUI.Environment(\.openWindow) private var openWindow: OpenWindowAction

    var body: some Scene {
        MenuBarExtra {
            MenuBarExtraContent(store: delegate.store)
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
private final class AppDelegate: NSObject, NSApplicationDelegate {
    private(set) var store: Result<AccountStore, LoginShellFailure>?
    @ObservationIgnored private var launch: Task<Void, Never>?

    func applicationDidFinishLaunching(_: Notification) {
        launch = Task(name: "Launch Relay") { [self] in
            switch await LoginShell.environment(within: .seconds(5)).map(AccountStore.init(environment:)) {
                case .failure(.run(.deadline(.cancelled))): return
                case let launched:
                    store = launched
                    if case .success(let started) = launched { started.start() }
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
