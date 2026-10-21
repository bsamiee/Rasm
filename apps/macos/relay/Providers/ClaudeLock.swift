import AsyncAlgorithms
import Files
import Foundation
import Results
import System

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct ClaudeLockfileStat: Equatable, Sendable {
    let inode: Inode
    let modified: Duration
}

// --- [SERVICES] ------------------------------------------------------------------------

private nonisolated struct ClaudeLockfile: Sendable {
    let url: URL
    let stat: ClaudeLockfileStat

    static func acquire(at url: URL, staleAfter stale: Duration) throws(ClaudeFailure) -> Self {
        func create() throws(ClaudeFailure) {
            try Result { try FileManager.default.createDirectory(at: url, withIntermediateDirectories: false, attributes: [.posixPermissions: 0o700]) }
                .mapError { error -> ClaudeFailure in if case CocoaError.fileWriteFileExists = error { .lockHeld(url) } else { .filesystem(error) } }
                .get()
        }
        do throws(ClaudeFailure) {
            try create()
        } catch .lockHeld {
            guard try fileStat(at: url).modified < .seconds(Date().timeIntervalSince1970) - stale else { throw .lockHeld(url) }
            try remove(at: url)
            try create()
        }
        do throws(ClaudeFailure) {
            return Self(url: url, stat: try fileStat(at: url))
        } catch {
            throw error.releasing(Result { () throws(ClaudeFailure) in try remove(at: url) })
        }
    }

    func updated() throws(ClaudeFailure) -> Self {
        try unchanged()
        var touched: URL = url
        var values: URLResourceValues = URLResourceValues()
        values.contentModificationDate = Date()
        try Result { try touched.setResourceValues(values) }.mapError(ClaudeFailure.filesystem).get()
        let current: ClaudeLockfileStat = try Self.fileStat(at: url)
        guard current.inode == stat.inode else { throw .lockCompromised(url) }
        return Self(url: url, stat: current)
    }

    func release() throws(ClaudeFailure) {
        try unchanged()
        try Self.remove(at: url)
    }

    private func unchanged() throws(ClaudeFailure) {
        guard try Self.fileStat(at: url) == stat else { throw .lockCompromised(url) }
    }

    private static func remove(at url: URL) throws(ClaudeFailure) {
        try Result { try FileManager.default.removeItem(at: url) }.mapError(ClaudeFailure.filesystem).get()
    }

    private static func fileStat(at url: URL) throws(ClaudeFailure) -> ClaudeLockfileStat {
        let stat: Stat = try Result { () throws(Errno) in try FilePath(url.path(percentEncoded: false)).stat() }.mapError(ClaudeFailure.filesystem).get()
        guard stat.type == .directory else { throw .lockCompromised(url) }
        return ClaudeLockfileStat(inode: stat.inode, modified: Duration(stat.st_mtim))
    }
}

actor ClaudeLock {
    // --- [STATE]
    private var lockfiles: [ClaudeLockfile]

    private init(lockfiles: [ClaudeLockfile]) {
        self.lockfiles = lockfiles
    }

    // --- [ACQUISITION]
    nonisolated static func refreshLockfile(in directory: URL) -> URL {
        directory.appending(path: ".oauth_refresh.lock")
    }

    static func withLock<Value: Sendable>(
        directories: [URL],
        _ body: @escaping @Sendable () async throws(ClaudeFailure) -> Value,
    ) async throws(ClaudeFailure) -> Value {
        let locks: [[(url: URL, stale: Duration)]] = try Set(directories).sorted { first, second in first.path(percentEncoded: false) < second.path(percentEncoded: false) }
            .map { directory throws(ClaudeFailure) in
                try Result { try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true) }.mapError(ClaudeFailure.filesystem).get()
                guard let resolved: UnsafeMutablePointer<CChar> = unsafe realpath(directory.path(percentEncoded: false), nil) else { throw .filesystem(Errno(rawValue: errno)) }
                defer { unsafe free(resolved) }
                return [
                    (refreshLockfile(in: directory), .seconds(60)),
                    (URL(filePath: unsafe String(cString: resolved)).appendingPathExtension("lock"), .seconds(60)),
                    (directory.appending(path: ".storage-write.lock"), .seconds(15)),
                ]
            }
        return try await hold(locks.flatMap(\.self), body)
    }

    static func withLock<Value: Sendable>(
        file: URL,
        _ body: @escaping @Sendable () async throws(ClaudeFailure) -> Value,
    ) async throws(ClaudeFailure) -> Value {
        try await hold([(file.appendingPathExtension("lock"), .seconds(10))], body)
    }

    private static func hold<Value: Sendable>(
        _ locks: [(url: URL, stale: Duration)],
        _ body: @escaping @Sendable () async throws(ClaudeFailure) -> Value,
    ) async throws(ClaudeFailure) -> Value {
        let lock: ClaudeLock = try await acquire(locks, attempt: 1)
        let outcome: Result<Value, ClaudeFailure> = await withTaskGroup { group in
            group.addTask(name: "locked-work") { await Result(catching: body) }
            group.addTask(name: "lock-heartbeat") { await Result { () async throws(ClaudeFailure) -> Value in try await lock.heartbeat() } }
            let first: Result<Value, ClaudeFailure> = await group.next() ?? .failure(.cancelled)
            group.cancelAll()
            return first
        }
        return try await lock.release(returning: outcome).get()
    }

    private static func acquire(_ locks: [(url: URL, stale: Duration)], attempt: Int) async throws(ClaudeFailure) -> ClaudeLock {
        let acquired: (lockfiles: [ClaudeLockfile], failure: ClaudeFailure?) = locks.reduce(into: ([], nil)) { acquired, lock in
            guard acquired.failure == nil else { return }
            switch Result(catching: { () throws(ClaudeFailure) in try ClaudeLockfile.acquire(at: lock.url, staleAfter: lock.stale) }) {
                case .success(let lockfile): acquired.lockfiles.append(lockfile)
                case .failure(let error): acquired.failure = release(acquired.lockfiles, after: error)
            }
        }
        switch acquired.failure {
            case .none: return ClaudeLock(lockfiles: acquired.lockfiles)
            case .lockHeld(let url) where attempt < 5:
                let path: String = url.path(percentEncoded: false)
                let removal: Result<Void, ClaudeFailure> = await withTimeout(.seconds(2)) {
                    await FileWatch.events(of: url).first { next in if case .failure = next { true } else { !FileManager.default.fileExists(atPath: path) } }
                        .map { next in next.mapError(ClaudeFailure.filesystem) } ?? .failure(.cancelled)
                }
                return switch removal {
                    case .success, .failure(.timedOut): try await acquire(locks, attempt: attempt + 1)
                    case .failure(let error): throw error
                }
            case .some(let error): throw error
        }
    }

    private static func release(
        _ lockfiles: [ClaudeLockfile],
        after error: ClaudeFailure,
    ) -> ClaudeFailure {
        lockfiles.reversed().reduce(error) { outcome, lockfile in
            outcome.releasing(Result(catching: lockfile.release))
        }
    }

    // --- [HOLD]
    private func heartbeat() async throws(ClaudeFailure) -> Never {
        for await _ in AsyncTimerSequence(interval: .seconds(5), clock: .continuous) {
            for index: Int in lockfiles.indices { lockfiles[index] = try lockfiles[index].updated() }
        }
        throw .cancelled
    }

    private func release<Value>(
        returning outcome: Result<Value, ClaudeFailure>
    ) -> Result<Value, ClaudeFailure> {
        let released: Result<Void, ClaudeFailure> = lockfiles.reversed().reduce(.success(())) { released, lockfile in
            let current: Result<Void, ClaudeFailure> = Result(catching: lockfile.release)
            return released.mapError { error in error.releasing(current) }.flatMap { _ in current }
        }
        lockfiles.removeAll()
        return outcome.mapError { error in error.releasing(released) }.flatMap { value in released.map { _ in value } }
    }
}
