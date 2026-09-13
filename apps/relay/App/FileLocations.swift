import Foundation

nonisolated struct FileLocations: Sendable {
  let applicationSupportDirectory: URL
  let defaultClaudeDirectory: URL

  var accountsFile: URL { applicationSupportDirectory.appending(path: "accounts.json") }
  var workingDirectory: URL {
    applicationSupportDirectory.appending(path: "Session", directoryHint: .isDirectory)
  }
  var claudeSelectionFile: URL {
    applicationSupportDirectory.appending(path: "claude-selection.json")
  }

  func accountDirectory(_ id: UUID) -> URL {
    applicationSupportDirectory.appending(
      path: "Accounts/\(id.uuidString)", directoryHint: .isDirectory)
  }

  func claudeDirectory(_ id: UUID) -> URL {
    accountDirectory(id).appending(path: "Claude", directoryHint: .isDirectory)
  }

  func codexHome(_ id: UUID) -> URL {
    accountDirectory(id).appending(path: "Codex", directoryHint: .isDirectory)
  }

  func codexDesktopDirectory(_ id: UUID) -> URL {
    accountDirectory(id).appending(path: "Desktop", directoryHint: .isDirectory)
  }
}
