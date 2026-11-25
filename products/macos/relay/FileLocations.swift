import CurrentProcess
import Files
import Foundation
import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct FileLocations: Sendable {
    let home: URL
    let applicationSupportDirectory: URL = Program.current.directory(.state)

    init(environment: [Environment.Key: String]) {
        home = environment["HOME"].flatMap { value in value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory) } ?? URL.homeDirectory
    }

    var accountsFile: URL { applicationSupportDirectory.appending(path: "accounts.json") }
    var claudeSelectionFile: URL { applicationSupportDirectory.appending(path: "claude-selection.json") }
    var accountsDirectory: URL { applicationSupportDirectory.appending(path: "Accounts", directoryHint: .isDirectory) }

    func accountDirectory(_ id: UUID) -> URL { accountsDirectory.appending(path: id.uuidString, directoryHint: .isDirectory) }
    func claudeDirectory(_ id: UUID) -> URL { accountDirectory(id).appending(path: "Claude", directoryHint: .isDirectory) }
    func codexHome(_ id: UUID) -> URL { accountDirectory(id).appending(path: "Codex", directoryHint: .isDirectory) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated extension URL {
    func contents() throws(AccountFailure) -> Data? {
        do { return try ifPresent { try Data(contentsOf: self) } } catch { throw AccountFailure(subject: lastPathComponent, reason: .filesystem(error)) }
    }

    func createDirectory() throws(AccountFailure) {
        do { try FileManager.default.createDirectory(at: self, withIntermediateDirectories: true) } catch { throw AccountFailure(subject: lastPathComponent, reason: .filesystem(error)) }
    }

    func remove() throws(AccountFailure) {
        do { _ = try ifPresent { try FileManager.default.removeItem(at: self) } } catch { throw AccountFailure(subject: lastPathComponent, reason: .filesystem(error)) }
    }
}

nonisolated extension Data {
    @concurrent func write(to file: URL, permissions: FilePermissions = .ownerReadWrite) async throws(AccountFailure) {
        try file.deletingLastPathComponent().createDirectory()
        let document = FileWrapper(regularFileWithContents: self)
        document.fileAttributes[FileAttributeKey.posixPermissions.rawValue] = Int(permissions.rawValue)
        do { try document.write(to: file, options: .atomic, originalContentsURL: nil) } catch { throw AccountFailure(subject: file.lastPathComponent, reason: .filesystem(error)) }
    }
}

nonisolated extension JSONDecoder {
    func decode<Value: Decodable>(_ type: Value.Type, from data: Data, subject: String) throws(AccountFailure) -> Value {
        do { return try decode(type, from: data) } catch { throw AccountFailure(subject: subject, reason: .unreadable(error)) }
    }
}

nonisolated extension JSONEncoder {
    func encode(_ value: some Encodable, subject: String) throws(AccountFailure) -> Data {
        do { return try encode(value) } catch { throw AccountFailure(subject: subject, reason: .unreadable(error)) }
    }
}
