import Files
import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum ClaudeLock {
    static func storageWriteLockfile(in directory: URL) -> URL {
        directory.appending(path: ".storage-write.lock")
    }

    static func withLock<Value: Sendable>(
        directories: [URL],
        _ body: @escaping @Sendable () async throws(ClaudeFailure) -> Value,
    ) async throws(ClaudeFailure) -> Value {
        let refresh: Backoff = Backoff(count: 5, base: .seconds(1), growth: 1, jitter: 1)
        let storageWrite: Backoff = Backoff(count: 10, base: .milliseconds(100), cap: .seconds(1))
        let lockfiles: [[Lockfile]] = try Set(directories).sorted { first, second in first.path(percentEncoded: false) < second.path(percentEncoded: false) }
            .map { directory throws(ClaudeFailure) in
                try Result { try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true) }.mapError(ClaudeFailure.filesystem).get()
                let canonical: URL = try Result(catching: directory.canonical).mapError(ClaudeFailure.filesystem).get()
                return [directory.appending(path: ".oauth_refresh.lock"), canonical.appendingPathExtension("lock")].map { url in
                    Lockfile(url, stale: .seconds(60), update: .seconds(5), retry: refresh)
                }
                    + [Lockfile(storageWriteLockfile(in: directory), stale: .seconds(15), retry: storageWrite)]
            }
        return try await Lockfile.withLocks(lockfiles.flatMap(\.self), body)
    }
}
