import CurrentProcess
import Files
import Foundation
import Subprocess

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct FileLocations: Sendable {
    let home: URL
    let applicationSupportDirectory: URL = Program.current.directory(
        .state,
        default: (Bundle.main.object(forInfoDictionaryKey: kCFBundleNameKey as String) as? String).map { name in URL.applicationSupportDirectory.appending(path: name, directoryHint: .isDirectory) },
    )

    init(environment: [Environment.Key: String]) {
        home =
            environment["HOME"].flatMap { value in
                value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory)
            } ?? URL.homeDirectory
    }

    var accountsFile: URL { applicationSupportDirectory.appending(path: "accounts.json") }
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
            try ifPresent {
                try FileManager.default.contentsOfDirectory(
                    at: accountsDirectory,
                    includingPropertiesForKeys: [],
                )
            }
        }
        .map { entries in
            (entries ?? []).compactMap { entry in UUID(uuidString: entry.lastPathComponent) }
                .filter { id in !known.contains(id) }
        }
    }
}
