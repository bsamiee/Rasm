import AppKit
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
        Button("Settings") { openWindow(id: "settings") }
          .keyboardShortcut(",", modifiers: .command)
      }
    }
  }
}

private final class AppDelegate: NSObject, NSApplicationDelegate {
  let store: AccountStore = AccountStore(process: ProcessInfo.processInfo.environment)

  func applicationDidFinishLaunching(_ notification: Notification) {
    store.start()
  }

  func applicationShouldTerminate(_ sender: NSApplication) -> NSApplication.TerminateReply {
    Task(name: "Quit Relay") {
      await store.stop()
      sender.reply(toApplicationShouldTerminate: true)
    }
    return .terminateLater
  }
}
