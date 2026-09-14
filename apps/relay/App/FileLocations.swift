import Foundation

nonisolated struct FileLocations: Sendable {
  let applicationSupportDirectory: URL
  let defaultClaudeDirectory: URL
  let defaultCodexHome: URL

  var accountsFile: URL { applicationSupportDirectory.appending(path: "accounts.json") }
  var workingDirectory: URL {
    applicationSupportDirectory.appending(path: "Session", directoryHint: .isDirectory)
  }
  var claudeSelectionFile: URL {
    applicationSupportDirectory.appending(path: "claude-selection.json")
  }
  var accountsDirectory: URL {
    applicationSupportDirectory.appending(path: "Accounts", directoryHint: .isDirectory)
  }

  func accountDirectory(_ id: UUID) -> URL {
    accountsDirectory.appending(path: id.uuidString, directoryHint: .isDirectory)
  }

  func claudeDirectory(_ id: UUID) -> URL {
    accountDirectory(id).appending(path: "Claude", directoryHint: .isDirectory)
  }

  func codexHome(_ id: UUID) -> URL {
    accountDirectory(id).appending(path: "Codex", directoryHint: .isDirectory)
  }

  func orphanAccountDirectories(excluding known: Set<UUID>) -> Result<[UUID], any Error> {
    Result {
      try FileManager.default.contentsOfDirectory(
        at: accountsDirectory, includingPropertiesForKeys: [])
    }
    .flatMapError { error in
      (error as? CocoaError)?.code == .fileReadNoSuchFile ? .success([]) : .failure(error)
    }
    .map { entries in
      entries.compactMap { entry in UUID(uuidString: entry.lastPathComponent) }
        .filter { id in !known.contains(id) }
    }
  }
}
