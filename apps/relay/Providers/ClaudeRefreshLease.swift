import Darwin
import Foundation

private struct ClaudeLockStamp: Equatable, Sendable {
  let inode: UInt64
  let seconds: Int
  let nanoseconds: Int
}

private struct ClaudeDirectoryLock: Sendable {
  let url: URL
  var stamp: ClaudeLockStamp

  static func acquire(at url: URL, staleAfter: TimeInterval) -> Result<
    ClaudeDirectoryLock, ClaudeFailure
  > {
    let created: Int32 = url.path.withCString { path in Darwin.mkdir(path, 0o700) }
    if created == 0 { return claimed(at: url) }
    let code: Int32 = errno
    guard code == EEXIST else { return .failure(.systemCall(operation: "mkdir", code: code)) }
    switch stamp(at: url) {
    case .failure(let error): return .failure(error)
    case .success(let existing):
      let modified: TimeInterval =
        Double(existing.seconds) + Double(existing.nanoseconds) / 1_000_000_000
      guard modified < Date().timeIntervalSince1970 - staleAfter else {
        return .failure(.leaseBusy(url))
      }
      let removed: Int32 = url.path.withCString { path in Darwin.rmdir(path) }
      guard removed == 0 else { return .failure(.systemCall(operation: "rmdir", code: errno)) }
      let acquired: Int32 = url.path.withCString { path in Darwin.mkdir(path, 0o700) }
      guard acquired == 0 else {
        return errno == EEXIST
          ? .failure(.leaseBusy(url)) : .failure(.systemCall(operation: "mkdir", code: errno))
      }
      return claimed(at: url)
    }
  }

  private static func claimed(at url: URL) -> Result<ClaudeDirectoryLock, ClaudeFailure> {
    switch stamp(at: url) {
    case .success(let stamp): return .success(Self(url: url, stamp: stamp))
    case .failure(let error):
      let removed: Int32 = url.path.withCString { path in Darwin.rmdir(path) }
      guard removed == 0 else {
        return .failure(
          .cleanup(operation: error, release: .systemCall(operation: "rmdir", code: errno)))
      }
      return .failure(error)
    }
  }

  mutating func update() -> Result<Void, ClaudeFailure> {
    switch Self.stamp(at: url) {
    case .failure: return .failure(.leaseCompromised(url))
    case .success(let current):
      guard current == stamp else { return .failure(.leaseCompromised(url)) }
    }
    let now: TimeInterval = Date().timeIntervalSince1970
    let seconds: Int = Int(now)
    let microseconds: Int32 = Int32((now - Double(seconds)) * 1_000_000)
    var times: [timeval] = [
      timeval(tv_sec: seconds, tv_usec: microseconds),
      timeval(tv_sec: seconds, tv_usec: microseconds),
    ]
    let changed: Int32 = times.withUnsafeMutableBufferPointer { buffer in
      url.path.withCString { path in Darwin.utimes(path, buffer.baseAddress) }
    }
    guard changed == 0 else { return .failure(.leaseCompromised(url)) }
    switch Self.stamp(at: url) {
    case .failure: return .failure(.leaseCompromised(url))
    case .success(let updated):
      guard updated.inode == stamp.inode else { return .failure(.leaseCompromised(url)) }
      stamp = updated
      return .success(())
    }
  }

  func release() -> Result<Void, ClaudeFailure> {
    switch Self.stamp(at: url) {
    case .failure: return .failure(.leaseCompromised(url))
    case .success(let current):
      guard current == stamp else { return .failure(.leaseCompromised(url)) }
      let removed: Int32 = url.path.withCString { path in Darwin.rmdir(path) }
      return removed == 0 ? .success(()) : .failure(.systemCall(operation: "rmdir", code: errno))
    }
  }

  private static func stamp(at url: URL) -> Result<ClaudeLockStamp, ClaudeFailure> {
    var information: stat = stat()
    let status: Int32 = url.path.withCString { path in Darwin.lstat(path, &information) }
    guard status == 0 else { return .failure(.systemCall(operation: "lstat", code: errno)) }
    guard information.st_mode & S_IFMT == S_IFDIR else { return .failure(.leaseCompromised(url)) }
    return .success(
      ClaudeLockStamp(
        inode: UInt64(information.st_ino),
        seconds: information.st_mtimespec.tv_sec,
        nanoseconds: information.st_mtimespec.tv_nsec
      ))
  }
}

/// Claude Code's own credential locks, held the way its proper-lockfile options hold them:
/// the refresh lock is `<store>/.oauth_refresh.lock` beside a legacy `<realpath>.lock`, stale
/// after 60 s and touched every 5 s, and its refresh retries a held lock five times before
/// giving up; the storage-write lock is `<store>/.storage-write.lock`, stale after 15 s, and
/// its writer retries ten times. Each lock is a directory whose mtime is the heartbeat.
actor ClaudeRefreshLease {
  private var locks: [ClaudeDirectoryLock]
  private var heartbeat: Task<Void, Never>?
  private var failure: ClaudeFailure?
  private let onCompromise: @Sendable () async -> Void

  private init(locks: [ClaudeDirectoryLock], onCompromise: @escaping @Sendable () async -> Void) {
    self.locks = locks
    self.onCompromise = onCompromise
  }

  static func acquire(
    directories: [URL],
    onCompromise: @escaping @Sendable () async -> Void
  ) async -> Result<ClaudeRefreshLease, ClaudeFailure> {
    let ordered: [URL] = Array(Set(directories)).sorted { first, second in first.path < second.path
    }
    var requests: [(URL, TimeInterval)] = []
    for directory in ordered {
      do {
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
      } catch {
        return .failure(.filesystem(error))
      }
      requests.append((directory.appending(path: ".oauth_refresh.lock"), 60))
      requests.append(
        (URL(fileURLWithPath: directory.resolvingSymlinksInPath().path + ".lock"), 60))
    }
    return await acquire(
      requests: requests, heartbeatEvery: .seconds(5), onCompromise: onCompromise)
  }

  static func storageWrite(
    directory: URL,
    onCompromise: @escaping @Sendable () async -> Void
  ) async -> Result<ClaudeRefreshLease, ClaudeFailure> {
    do {
      try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
    } catch {
      return .failure(.filesystem(error))
    }
    return await acquire(
      requests: [(directory.appending(path: ".storage-write.lock"), 15)],
      heartbeatEvery: .milliseconds(7_500),
      onCompromise: onCompromise
    )
  }

  func status() -> Result<Void, ClaudeFailure> {
    switch failure {
    case .some(let error): .failure(error)
    case .none: .success(())
    }
  }

  func finish<Value: Sendable>(_ outcome: Result<Value, ClaudeFailure>) async -> Result<
    Value, ClaudeFailure
  > {
    let task: Task<Void, Never>? = heartbeat
    heartbeat = nil
    task?.cancel()
    await task?.value
    var releaseFailure: ClaudeFailure? = failure
    for lock in locks.reversed() {
      if case .failure(let error) = lock.release() {
        releaseFailure =
          switch releaseFailure {
          case .some(let previous): .cleanup(operation: previous, release: error)
          case .none: error
          }
      }
    }
    locks.removeAll()
    return switch (outcome, releaseFailure) {
    case (.success(let value), .none): .success(value)
    case (.success, let .some(error)): .failure(error)
    case (.failure(let error), .none): .failure(error)
    case (.failure(let error), .some(let release)):
      .failure(.cleanup(operation: error, release: release))
    }
  }

  private static func acquire(
    requests: [(URL, TimeInterval)],
    heartbeatEvery interval: Duration,
    onCompromise: @escaping @Sendable () async -> Void
  ) async -> Result<ClaudeRefreshLease, ClaudeFailure> {
    var held: [ClaudeDirectoryLock] = []
    for (url, stale) in requests {
      switch ClaudeDirectoryLock.acquire(at: url, staleAfter: stale) {
      case .success(let lock): held.append(lock)
      case .failure(let error):
        var outcome: ClaudeFailure = error
        for lock in held.reversed() {
          if case .failure(let release) = lock.release() {
            outcome = .cleanup(operation: outcome, release: release)
          }
        }
        return .failure(outcome)
      }
    }
    let lease: ClaudeRefreshLease = ClaudeRefreshLease(locks: held, onCompromise: onCompromise)
    await lease.startHeartbeat(every: interval)
    return .success(lease)
  }

  private func startHeartbeat(every interval: Duration) {
    heartbeat = Task { [weak self] in
      while !Task.isCancelled {
        do {
          try await ContinuousClock().sleep(for: interval)
        } catch {
          return
        }
        guard let self else { return }
        guard await self.update() else { return }
      }
    }
  }

  private func update() async -> Bool {
    for index in locks.indices {
      if case .failure(let error) = locks[index].update() {
        failure = error
        await onCompromise()
        return false
      }
    }
    return true
  }
}
