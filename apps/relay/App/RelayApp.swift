import AppKit
import SwiftUI

@main
struct RelayApp: App {
  @NSApplicationDelegateAdaptor(RelayAppDelegate.self) private var delegate: RelayAppDelegate

  var body: some Scene {
    MenuBarExtra {
      RelayMenu(store: delegate.store)
    } label: {
      Image("RelaySymbol")
        .accessibilityLabel("Relay")
    }
    .menuBarExtraStyle(.window)

    Settings {
      RelaySettings(store: delegate.store)
    }
    .defaultSize(width: 680, height: 460)
    .windowResizability(.contentMinSize)
  }
}

@MainActor
private final class RelayAppDelegate: NSObject, NSApplicationDelegate {
  let store: RelayStore = RelayStore()

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
