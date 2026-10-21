import Files
import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum ClaudeLock {
    static func refreshLockfile(in directory: URL) -> URL {
        directory.appending(path: ".oauth_refresh.lock")
    }

    static func withLock<Value: Sendable>(
        directories: [URL],
        _ body: @escaping @Sendable () async throws(ClaudeFailure) -> Value,
    ) async throws(ClaudeFailure) -> Value {
        let lockfiles: [[Lockfile]] = try Set(directories).sorted { first, second in first.path(percentEncoded: false) < second.path(percentEncoded: false) }
            .map { directory throws(ClaudeFailure) in
                try Result { try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true) }.mapError(ClaudeFailure.filesystem).get()
                let canonical: URL = try Result(catching: directory.canonical).mapError(ClaudeFailure.filesystem).get()
                let retries: [Duration] = (0..<5).map { _ in .milliseconds(Double.random(in: 1000..<2000)) }
                return [refreshLockfile(in: directory), canonical.appendingPathExtension("lock")].map { url in Lockfile(url, stale: .seconds(60), update: .seconds(5), retries: retries) }
                    + [Lockfile(directory.appending(path: ".storage-write.lock"), stale: .seconds(15), retries: (0..<10).map { attempt in min(.milliseconds(100 << attempt), .seconds(1)) })]
            }
        return try await Lockfile.withLocks(lockfiles.flatMap(\.self), body)
    }
}
