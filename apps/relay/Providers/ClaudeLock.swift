import Foundation

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct ClaudeLockfileStat: Equatable, Sendable {
    let inode: Int
    let modified: Date
}

// --- [SERVICES] ------------------------------------------------------------------------

private nonisolated struct ClaudeLockfile: Sendable {
    static let staleAfter: TimeInterval = 60
    let url: URL
    let stat: ClaudeLockfileStat

    static func acquire(at url: URL) -> Result<Self, ClaudeFailure> {
        create(at: url)
            .flatMapError { error -> Result<Void, ClaudeFailure> in
                guard case .lockHeld = error else { return .failure(error) }
                return fileStat(at: url)
                    .flatMap { existing in
                        existing.modified < Date().addingTimeInterval(-staleAfter)
                            ? remove(at: url) : .failure(.lockHeld(url))
                    }
                    .flatMap { _ in create(at: url) }
            }
            .flatMap { _ in lockfile(at: url) }
    }

    private static func lockfile(at url: URL) -> Result<Self, ClaudeFailure> {
        fileStat(at: url).map { stat in Self(url: url, stat: stat) }.mapError { error in
            error.releasing(remove(at: url))
        }
    }

    func updated() -> Result<Self, ClaudeFailure> {
        unchangedStat()
            .flatMap { _ in
                Result {
                    try FileManager.default.setAttributes(
                        [.modificationDate: Date()],
                        ofItemAtPath: url.path,
                    )
                }
                .mapError(ClaudeFailure.filesystem)
            }
            .flatMap { _ in Self.fileStat(at: url) }
            .flatMap { current in
                current.inode == stat.inode ? .success(Self(url: url, stat: current)) : .failure(.lockCompromised(url))
            }
            .mapError { _ in .lockCompromised(url) }
    }

    func release() -> Result<Void, ClaudeFailure> {
        unchangedStat().flatMap { _ in Self.remove(at: url) }
    }

    private func unchangedStat() -> Result<ClaudeLockfileStat, ClaudeFailure> {
        Self.fileStat(at: url)
            .flatMap { current in current == stat ? .success(current) : .failure(.lockCompromised(url)) }
            .mapError { _ in .lockCompromised(url) }
    }

    private static func create(at url: URL) -> Result<Void, ClaudeFailure> {
        Result {
            try FileManager.default.createDirectory(
                at: url,
                withIntermediateDirectories: false,
                attributes: [.posixPermissions: 0o700],
            )
        }
        .mapError { error in
            if case CocoaError.fileWriteFileExists = error { .lockHeld(url) } else { .filesystem(error) }
        }
    }

    private static func remove(at url: URL) -> Result<Void, ClaudeFailure> {
        Result { try FileManager.default.removeItem(at: url) }.mapError(ClaudeFailure.filesystem)
    }

    private static func fileStat(at url: URL) -> Result<ClaudeLockfileStat, ClaudeFailure> {
        Result { try FileManager.default.attributesOfItem(atPath: url.path) }
            .mapError(ClaudeFailure.filesystem)
            .flatMap { attributes in
                if attributes[.type] as? FileAttributeType == .typeDirectory,
                    let inode: Int = attributes[.systemFileNumber] as? Int,
                    let modified: Date = attributes[.modificationDate] as? Date
                {
                    .success(ClaudeLockfileStat(inode: inode, modified: modified))
                } else {
                    .failure(.lockCompromised(url))
                }
            }
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
        _ body: @escaping @Sendable () async -> Result<Value, ClaudeFailure>,
    ) async -> Result<Value, ClaudeFailure> {
        let urls: [URL] = Set(directories).sorted { first, second in first.path < second.path }
            .flatMap { directory in
                [refreshLockfile(in: directory), URL(filePath: directory.resolvingSymlinksInPath().path + ".lock")]
            }
        return await Result {
            for url: URL in urls {
                try FileManager.default.createDirectory(
                    at: url.deletingLastPathComponent(),
                    withIntermediateDirectories: true,
                )
            }
        }
        .mapError(ClaudeFailure.filesystem)
        .bind { _ in await acquire(urls) }
        .bind { lock in
            let outcome: Result<Value, ClaudeFailure> = await withTaskGroup { group in
                group.addTask(name: "locked-work") { await body() }
                group.addTask(name: "lock-heartbeat") { await lock.heartbeat() }
                let first: Result<Value, ClaudeFailure> = await group.next() ?? .failure(.cancelled)
                group.cancelAll()
                return first
            }
            return await lock.release(returning: outcome)
        }
    }

    private static func acquire(_ urls: [URL]) async -> Result<ClaudeLock, ClaudeFailure> {
        let held: Result<[ClaudeLockfile], ClaudeFailure> = urls.reduce(.success([])) { held, url in
            held.flatMap { lockfiles in
                ClaudeLockfile.acquire(at: url)
                    .map { lockfile in lockfiles + [lockfile] }
                    .mapError { error in release(lockfiles, after: error) }
            }
        }
        switch held {
            case .success(let lockfiles): return .success(ClaudeLock(lockfiles: lockfiles))
            case .failure(.lockHeld(let url)):
                return await ProcessRun.withDeadline(.seconds(ClaudeLockfile.staleAfter)) {
                    await Result { try await FileWatch.values(of: url, read: FileWatch.exists, debounce: nil).first { present in !present } }
                        .mapError(ClaudeFailure.filesystem)
                        .flatMap { removed in removed.map { _ in .success(()) } ?? .failure(.cancelled) }
                }
                .flatMapError { error in if case .timedOut = error { .success(()) } else { .failure(error) } }
                .bind { _ in await acquire(urls) }
            case .failure(let error): return .failure(error)
        }
    }

    private static func release(
        _ lockfiles: [ClaudeLockfile],
        after error: ClaudeFailure,
    ) -> ClaudeFailure {
        lockfiles.reversed().reduce(error) { outcome, lockfile in
            outcome.releasing(lockfile.release())
        }
    }

    // --- [HOLD]
    private func heartbeat<Value: Sendable>() async -> Result<Value, ClaudeFailure> {
        await Result { try await Task.sleep(for: .seconds(5)) }
            .mapError { _ in ClaudeFailure.cancelled }
            .flatMap { _ -> Result<Void, ClaudeFailure> in
                lockfiles.indices.reduce(.success(())) { outcome, index in
                    outcome.flatMap { _ in lockfiles[index].updated() }.map { current in lockfiles[index] = current }
                }
            }
            .bind { _ in await heartbeat() }
    }

    private func release<Value: Sendable>(
        returning outcome: Result<Value, ClaudeFailure>
    ) -> Result<Value, ClaudeFailure> {
        let released: Result<Void, ClaudeFailure> = lockfiles.reversed().reduce(.success(())) { released, lockfile in
            let current: Result<Void, ClaudeFailure> = lockfile.release()
            return released.mapError { error in error.releasing(current) }.flatMap { _ in current }
        }
        lockfiles.removeAll()
        return outcome.mapError { error in error.releasing(released) }.flatMap { value in released.map { _ in value } }
    }
}
