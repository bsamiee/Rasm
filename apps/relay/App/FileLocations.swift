import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct FileLocations: Sendable {
    let home: URL

    init(environment: [String: String]) {
        home =
            environment["HOME"].flatMap { value in
                value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory)
            } ?? URL.homeDirectory
    }

    var applicationSupportDirectory: URL {
        URL.applicationSupportDirectory.appending(path: "Relay", directoryHint: .isDirectory)
    }
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
        ifPresent {
            try FileManager.default.contentsOfDirectory(
                at: accountsDirectory,
                includingPropertiesForKeys: [],
            )
        }
        .map { entries in
            (entries ?? []).compactMap { entry in UUID(uuidString: entry.lastPathComponent) }
                .filter { id in !known.contains(id) }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated func ifPresent<Value>(_ operation: () throws -> Value) -> Result<Value?, any Error> {
    Result { try operation() }.map(Optional.some).flatMapError { error in
        switch error {
            case CocoaError.fileNoSuchFile, CocoaError.fileReadNoSuchFile, .noSuchFileOrDirectory as Errno: .success(nil)
            default: .failure(error)
        }
    }
}
