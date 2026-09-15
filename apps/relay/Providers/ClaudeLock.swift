import Foundation

private nonisolated struct ClaudeLockfileStat: Equatable, Sendable {
  let inode: Int
  let modified: Date
}

private nonisolated struct ClaudeLockfile: Sendable {
  let url: URL
  var stat: ClaudeLockfileStat

  static func acquire(at url: URL, staleAfter: TimeInterval) -> Result<
    ClaudeLockfile, ClaudeFailure
  > {
    create(at: url)
      .flatMapError { error -> Result<Void, ClaudeFailure> in
        guard case .lockHeld = error else { return .failure(error) }
        return fileStat(at: url).flatMap { existing in
          existing.modified < Date().addingTimeInterval(-staleAfter)
            ? remove(at: url).flatMap { _ in create(at: url) } : .failure(.lockHeld(url))
        }
      }
      .flatMap { _ in lockfile(at: url) }
  }

  private static func lockfile(at url: URL) -> Result<ClaudeLockfile, ClaudeFailure> {
    fileStat(at: url).map { stat in Self(url: url, stat: stat) }.mapError { error in
      error.releasing(remove(at: url))
    }
  }

  mutating func update() -> Result<Void, ClaudeFailure> {
    let updated: Result<ClaudeLockfileStat, ClaudeFailure> = unchangedStat()
      .flatMap { _ in
        Result {
          try FileManager.default.setAttributes(
            [.modificationDate: Date()], ofItemAtPath: url.path)
        }
        .mapError(ClaudeFailure.filesystem)
      }
      .flatMap { _ in Self.fileStat(at: url) }
      .flatMap { current in
        current.inode == stat.inode ? .success(current) : .failure(.lockCompromised(url))
      }
      .mapError { _ in .lockCompromised(url) }
    return updated.map { current in stat = current }
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
        at: url, withIntermediateDirectories: false, attributes: [.posixPermissions: 0o700])
    }
    .mapError { error in
      (error as? CocoaError)?.code == .fileWriteFileExists ? .lockHeld(url) : .filesystem(error)
    }
  }

  private static func remove(at url: URL) -> Result<Void, ClaudeFailure> {
    Result { try FileManager.default.removeItem(at: url) }.mapError(ClaudeFailure.filesystem)
  }

  private static func fileStat(at url: URL) -> Result<ClaudeLockfileStat, ClaudeFailure> {
    Result { try FileManager.default.attributesOfItem(atPath: url.path) }
      .mapError(ClaudeFailure.filesystem)
      .flatMap { attributes in
        guard attributes[.type] as? FileAttributeType == .typeDirectory,
          let inode: Int = attributes[.systemFileNumber] as? Int,
          let modified: Date = attributes[.modificationDate] as? Date
        else { return .failure(.lockCompromised(url)) }
        return .success(ClaudeLockfileStat(inode: inode, modified: modified))
      }
  }
}

private nonisolated struct ClaudeLockRequest: Sendable {
  let url: URL
  let stale: TimeInterval
}

actor ClaudeLock {
  private static let attempts: Int = 10
  private static let backoff: [Duration] = [
    .milliseconds(100), .milliseconds(200), .milliseconds(400), .milliseconds(800),
    .milliseconds(1000),
  ]

  private var lockfiles: [ClaudeLockfile]

  private init(lockfiles: [ClaudeLockfile]) {
    self.lockfiles = lockfiles
  }

  nonisolated static func refreshLockfile(in directory: URL) -> URL {
    directory.appending(path: ".oauth_refresh.lock")
  }

  static func oauthRefresh<Value: Sendable>(
    directories: [URL],
    _ body: @escaping @Sendable () async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    let ordered: [URL] = Set(directories).sorted { first, second in first.path < second.path }
    return await holding(
      ordered.flatMap { directory in
        [
          ClaudeLockRequest(url: refreshLockfile(in: directory), stale: 60),
          ClaudeLockRequest(
            url: URL(filePath: directory.resolvingSymlinksInPath().path + ".lock"), stale: 60),
        ]
      },
      updateEvery: .seconds(5), body)
  }

  private static func holding<Value: Sendable>(
    _ requests: [ClaudeLockRequest], updateEvery interval: Duration,
    _ body: @escaping @Sendable () async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    await createDirectories(requests.map { request in request.url.deletingLastPathComponent() })
      .bind { _ in await acquire(requests, attempt: 0) }
      .bind { lock in
        let outcome: Result<Value, ClaudeFailure> = await withTaskGroup { group in
          group.addTask(name: "locked-work") { await body() }
          group.addTask(name: "lock-heartbeat") { await lock.heartbeat(every: interval) }
          let first: Result<Value, ClaudeFailure> = await group.next() ?? .failure(.cancelled)
          group.cancelAll()
          await group.waitForAll()
          return first
        }
        return await lock.release(returning: outcome)
      }
  }

  private static func acquire(
    _ requests: [ClaudeLockRequest], attempt: Int
  ) async -> Result<ClaudeLock, ClaudeFailure> {
    let held: Result<[ClaudeLockfile], ClaudeFailure> = requests.reduce(.success([])) {
      held, request in
      held.flatMap { lockfiles in
        ClaudeLockfile.acquire(at: request.url, staleAfter: request.stale)
          .map { lockfile in lockfiles + [lockfile] }
          .mapError { error in release(lockfiles, after: error) }
      }
    }
    switch held {
    case .success(let lockfiles): return .success(ClaudeLock(lockfiles: lockfiles))
    case .failure(.lockHeld) where attempt < attempts:
      let delay: Duration = backoff[min(attempt, backoff.count - 1)]
      return await Result { try await Task.sleep(for: delay) }
        .mapError { _ in ClaudeFailure.cancelled }
        .bind { _ in await acquire(requests, attempt: attempt + 1) }
    case .failure(let error): return .failure(error)
    }
  }

  private static func release(
    _ lockfiles: [ClaudeLockfile], after error: ClaudeFailure
  ) -> ClaudeFailure {
    lockfiles.reversed().reduce(error) { outcome, lockfile in
      outcome.releasing(lockfile.release())
    }
  }

  private static func createDirectories(_ directories: [URL]) -> Result<Void, ClaudeFailure> {
    Result {
      for directory in directories {
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
      }
    }
    .mapError(ClaudeFailure.filesystem)
  }

  private func heartbeat<Value: Sendable>(every interval: Duration) async -> Result<
    Value, ClaudeFailure
  > {
    await Result { try await Task.sleep(for: interval) }
      .mapError { _ in ClaudeFailure.cancelled }
      .flatMap { _ in update() }
      .bind { _ in await heartbeat(every: interval) }
  }

  private func update() -> Result<Void, ClaudeFailure> {
    lockfiles.indices.reduce(.success(())) { outcome, index in
      outcome.flatMap { _ in lockfiles[index].update() }
    }
  }

  private func release<Value: Sendable>(
    returning outcome: Result<Value, ClaudeFailure>
  ) -> Result<Value, ClaudeFailure> {
    let released: Result<Void, ClaudeFailure> = lockfiles.reversed().reduce(.success(())) {
      released, lockfile in
      switch released {
      case .success: lockfile.release()
      case .failure(let error): .failure(error.releasing(lockfile.release()))
      }
    }
    lockfiles.removeAll()
    return switch outcome {
    case .success(let value): released.map { _ in value }
    case .failure(let error): .failure(error.releasing(released))
    }
  }
}
