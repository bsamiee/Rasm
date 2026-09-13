import Foundation

struct RelayPaths: Sendable {
  let applicationSupport: URL
  let defaultClaudeDirectory: URL

  var accountsFile: URL { applicationSupport.appending(path: "accounts.json") }
  var workingDirectory: URL {
    applicationSupport.appending(path: "Session", directoryHint: .isDirectory)
  }
  var claudeSelectionFile: URL { applicationSupport.appending(path: "claude-selection.json") }

  func accountDirectory(_ id: UUID) -> URL {
    applicationSupport.appending(path: "Accounts/\(id.uuidString)", directoryHint: .isDirectory)
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
