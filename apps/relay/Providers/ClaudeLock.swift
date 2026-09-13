import Darwin
import Foundation

private nonisolated struct ClaudeLockfileStat: Equatable, Sendable {
  let inode: UInt64
  let seconds: Int
  let nanoseconds: Int
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
          let modified: TimeInterval =
            Double(existing.seconds) + Double(existing.nanoseconds) / 1_000_000_000
          return modified < Date().timeIntervalSince1970 - staleAfter
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
    let now: TimeInterval = Date().timeIntervalSince1970
    let seconds: Int = Int(now)
    let touched: timeval = timeval(
      tv_sec: seconds, tv_usec: Int32((now - Double(seconds)) * 1_000_000))
    let updated: Result<ClaudeLockfileStat, ClaudeFailure> = unchangedStat()
      .flatMap { _ in
        Darwin.utimes(url.path, [touched, touched]) == 0
          ? Self.fileStat(at: url) : .failure(.lockCompromised(url))
      }
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
    Darwin.mkdir(url.path, 0o700) == 0
      ? .success(()) : .failure(errno == EEXIST ? .lockHeld(url) : .systemCall(.mkdir, errno))
  }

  private static func remove(at url: URL) -> Result<Void, ClaudeFailure> {
    Darwin.rmdir(url.path) == 0 ? .success(()) : .failure(.systemCall(.rmdir, errno))
  }

  private static func fileStat(at url: URL) -> Result<ClaudeLockfileStat, ClaudeFailure> {
    var information: Darwin.stat = Darwin.stat()
    guard Darwin.lstat(url.path, &information) == 0 else {
      return .failure(.systemCall(.lstat, errno))
    }
    return information.st_mode & S_IFMT == S_IFDIR
      ? .success(
        ClaudeLockfileStat(
          inode: UInt64(information.st_ino),
          seconds: information.st_mtimespec.tv_sec,
          nanoseconds: information.st_mtimespec.tv_nsec
        ))
      : .failure(.lockCompromised(url))
  }
}

actor ClaudeLock {
  private var lockfiles: [ClaudeLockfile]
  private var updates: Task<Void, Never>?
  private var failure: ClaudeFailure?
  private let onCompromised: @Sendable () async -> Void

  private init(
    lockfiles: [ClaudeLockfile], onCompromised: @escaping @Sendable () async -> Void
  ) {
    self.lockfiles = lockfiles
    self.onCompromised = onCompromised
  }

  static func oauthRefresh(
    directories: [URL],
    onCompromised: @escaping @Sendable () async -> Void
  ) async -> Result<ClaudeLock, ClaudeFailure> {
    let ordered: [URL] = Set(directories).sorted { first, second in first.path < second.path }
    return await createDirectories(ordered).bind { _ in
      await acquire(
        lockfiles: ordered.flatMap { directory in
          [
            (directory.appending(path: ".oauth_refresh.lock"), 60),
            (URL(fileURLWithPath: directory.resolvingSymlinksInPath().path + ".lock"), 60),
          ]
        },
        updateEvery: .seconds(5), onCompromised: onCompromised
      )
    }
  }

  static func storageWrite(
    directory: URL,
    onCompromised: @escaping @Sendable () async -> Void
  ) async -> Result<ClaudeLock, ClaudeFailure> {
    await createDirectories([directory]).bind { _ in
      await acquire(
        lockfiles: [(directory.appending(path: ".storage-write.lock"), 15)],
        updateEvery: .milliseconds(7_500),
        onCompromised: onCompromised
      )
    }
  }

  func status() -> Result<Void, ClaudeFailure> {
    failure.map { error in .failure(error) } ?? .success(())
  }

  func release<Value: Sendable>(
    returning outcome: Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    let task: Task<Void, Never>? = updates
    updates = nil
    task?.cancel()
    await task?.value
    let released: Result<Void, ClaudeFailure> = lockfiles.reversed().reduce(status()) {
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

  private static func createDirectories(_ directories: [URL]) -> Result<Void, ClaudeFailure> {
    Result {
      for directory in directories {
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
      }
    }
    .mapError(ClaudeFailure.filesystem)
  }

  private static func acquire(
    lockfiles requests: [(url: URL, stale: TimeInterval)],
    updateEvery interval: Duration,
    onCompromised: @escaping @Sendable () async -> Void
  ) async -> Result<ClaudeLock, ClaudeFailure> {
    let held: Result<[ClaudeLockfile], ClaudeFailure> = requests.reduce(.success([])) {
      held, request in
      held.flatMap { lockfiles in
        ClaudeLockfile.acquire(at: request.url, staleAfter: request.stale)
          .map { lockfile in lockfiles + [lockfile] }
          .mapError { error in release(lockfiles, after: error) }
      }
    }
    return await held.bind { lockfiles in
      let lock: ClaudeLock = ClaudeLock(lockfiles: lockfiles, onCompromised: onCompromised)
      await lock.startUpdates(every: interval)
      return .success(lock)
    }
  }

  private static func release(
    _ lockfiles: [ClaudeLockfile], after error: ClaudeFailure
  ) -> ClaudeFailure {
    lockfiles.reversed().reduce(error) { outcome, lockfile in
      outcome.releasing(lockfile.release())
    }
  }

  private func startUpdates(every interval: Duration) {
    updates = Task { [weak self] in
      while !Task.isCancelled {
        guard (try? await Task.sleep(for: interval)) != nil, let self, await self.update() else {
          return
        }
      }
    }
  }

  private func update() async -> Bool {
    for index in lockfiles.indices {
      if case .failure(let error) = lockfiles[index].update() {
        failure = error
        await onCompromised()
        return false
      }
    }
    return true
  }
}
