import AppKit
import OSLog
import SwiftUI

@main
struct RelayApp: App {
  @NSApplicationDelegateAdaptor(AppDelegate.self) private var delegate: AppDelegate
  @Environment(\.openWindow) private var openWindow: OpenWindowAction

  var body: some Scene {
    MenuBarExtra {
      MenuBarExtraContent(store: delegate.store)
    } label: {
      Image(.relaySymbol)
        .accessibilityLabel("Relay")
    }
    .menuBarExtraStyle(.window)

    Window("Settings", id: "settings") {
      SettingsView(store: delegate.store)
    }
    .defaultSize(width: 680, height: 460)
    .windowResizability(.contentMinSize)
    .windowToolbarStyle(.unified)
    .commands {
      CommandGroup(replacing: .appSettings) {
        Button("Settings…") { openWindow(id: "settings") }
          .keyboardShortcut(",", modifiers: .command)
      }
    }
  }
}

private final class AppDelegate: NSObject, NSApplicationDelegate {
  let store: AccountStore

  override init() {
    let process: [String: String] = ProcessInfo.processInfo.environment
    switch LoginShell.environment(over: process) {
    case .success(let environment):
      store = AccountStore(environment: environment)
    case .failure(let error):
      Logger(subsystem: "app.rasm.relay", category: "Launch").error(
        "Login shell environment unavailable: \(String(describing: error), privacy: .public)")
      store = AccountStore(environment: process)
    }
  }

  func applicationDidFinishLaunching(_ notification: Notification) {
    store.start()
  }

  func applicationShouldTerminate(_ sender: NSApplication) -> NSApplication.TerminateReply {
    Task {
      await store.stop()
      sender.reply(toApplicationShouldTerminate: true)
    }
    return .terminateLater
  }
}
