import AsyncAlgorithms
import CasePathsCore
public import Foundation
import Logging
public import Results
import System

// --- [TYPES] ---------------------------------------------------------------------------

public protocol LockFailure: Error {
    static func lock(_ failure: FileFailure) -> Self
    static var cancelled: Self { get }
    func releasing(_ release: Result<Void, Self>) -> Self
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Lockfile: Sendable {
    let url: URL
    let stale: Duration
    let update: Duration
    let retries: Backoff?

    public init(_ url: URL, stale: Duration = .seconds(10), update: Duration? = nil, retries: Backoff? = nil) {
        self.url = url
        self.stale = max(stale, .seconds(2))
        self.update = max(min(update ?? self.stale / 2, self.stale / 2), .seconds(1))
        self.retries = retries
    }

    public static func withLocks<Value: Sendable, Failure: LockFailure>(
        _ lockfiles: [Self],
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
        _ body: @escaping @Sendable () async throws(Failure) -> Value,
    ) async throws(Failure) -> Value {
        let holder: Holder<Failure> = try await acquire(lockfiles, clock: clock)
        let outcome = await Result { () async throws(Failure) -> Value in
            guard let interval = lockfiles.map(\.update).min() else { return try await body() }
            return try await race(body) { () async throws(Failure) -> Value in try await holder.heartbeat(every: interval, clock: clock) }
        }
        let released: Result<Void, Failure> = release(await holder.locks)
        return try outcome.mapError { error in error.releasing(released) }.flatMap { value in released.map { _ in value } }.get()
    }

    private static func acquire<Failure: LockFailure>(_ lockfiles: [Self], clock: some Clock<Duration> & Sendable) async throws(Failure) -> Holder<Failure> {
        var generator = SystemRandomNumberGenerator()
        var delays = lockfiles.map { lockfile in (lockfile.retries?.delays(using: &generator) ?? []).makeIterator() }
        while true {
            var locks: [Lock] = []
            let acquired = Result { () throws(FileFailure) in for lockfile in lockfiles { locks.append(try Lock.acquire(lockfile)) } }
            guard case .failure(let failure) = acquired else { return Holder(locks) }
            let (blocked, released): (URL, Result<Void, Failure>) = (lockfiles[locks.count].url, release(locks))
            guard failure.reason.is(\.held), case .success = released, let delay = delays[locks.count].next() else { throw Failure.lock(failure).releasing(released) }
            Logger.current.debug("Waiting for held lock", metadata: ["lock": .string(blocked.path(percentEncoded: false)), "delay": .stringConvertible(delay)])
            try await race {
                () async throws(Failure) in
                switch await FileEvent.presence(of: blocked).first(where: { presence in presence[case: \.success] != true }) {
                    case .some(.failure(let error)): throw .lock(error)
                    case .some(.success): return
                    case .none: throw .cancelled
                }
            } against: {
                () async throws(Failure) in
                do { try await clock.sleep(for: delay) } catch { throw .cancelled }
            }
        }
    }

    private static func release<Failure: LockFailure>(_ locks: [Lock]) -> Result<Void, Failure> {
        locks.reversed().reduce(.success(())) { released, lock in
            let current = Result { () throws(FileFailure) in try lock.release() }.mapError(Failure.lock)
            return released.mapError { error in error.releasing(current) }.flatMap { _ in current }
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private struct Lock: Equatable, Sendable {
    let url: URL
    let inode: Inode
    let modified: Duration

    static func acquire(_ lockfile: Lockfile) throws(FileFailure) -> Self {
        do throws(FileFailure) {
            try create(lockfile.url)
        } catch let failure where failure.reason.is(\.held) {
            let now = Duration.seconds(Date.now.timeIntervalSince1970)
            switch try read(lockfile.url) {
                case .some(let current) where current.modified >= now - lockfile.stale: throw failure
                case .some(let stale):
                    try remove(lockfile.url)
                    Logger.current.info("Removed stale lock", metadata: ["lock": .string(lockfile.url.path(percentEncoded: false)), "unchanged": .stringConvertible(now - stale.modified)])
                case .none: break
            }
            try create(lockfile.url)
        }
        do throws(FileFailure) {
            guard let lock = try read(lockfile.url) else { throw FileFailure(file: lockfile.url, reason: .compromised) }
            return lock
        } catch {
            try Result { () throws(FileFailure) in try remove(lockfile.url) }
                .mapError { cleanup in FileFailure(file: lockfile.url, reason: .filesystem(AggregateError(first: error, remaining: [cleanup]))) }.get()
            throw error
        }
    }

    func update() throws(FileFailure) -> Self {
        guard try Self.read(url) == self else { throw FileFailure(file: url, reason: .compromised) }
        _ = try ifPresent(url) { guard unsafe utimes(url.path(percentEncoded: false), nil) == 0 else { throw Errno(rawValue: errno) } }
        guard let current = try Self.read(url), current.inode == inode else { throw FileFailure(file: url, reason: .compromised) }
        return current
    }

    func release() throws(FileFailure) {
        guard try Self.read(url) == self else { throw FileFailure(file: url, reason: .compromised) }
        try Self.remove(url)
    }

    private static func read(_ url: URL) throws(FileFailure) -> Self? {
        try ifPresent(url) { try FilePath(url.path(percentEncoded: false)).stat() }.map { stat throws(FileFailure) in
            guard stat.type == .directory else { throw FileFailure(file: url, reason: .compromised) }
            return Self(url: url, inode: stat.inode, modified: Duration(stat.st_mtim))
        }
    }

    private static func create(_ url: URL) throws(FileFailure) {
        guard unsafe mkdir(url.path(percentEncoded: false), FilePermissions.ownerReadWriteExecute.rawValue) != 0 else { return }
        let error = Errno(rawValue: errno)
        throw FileFailure(file: url, reason: error == .fileExists ? .held : .filesystem(error))
    }

    private static func remove(_ url: URL) throws(FileFailure) {
        _ = try ifPresent(url) { guard unsafe rmdir(url.path(percentEncoded: false)) == 0 else { throw Errno(rawValue: errno) } }
    }
}

private actor Holder<Failure: LockFailure> {
    // --- [STATE]

    private(set) var locks: [Lock]

    init(_ locks: [Lock]) {
        self.locks = locks
    }

    // --- [HEARTBEAT]

    func heartbeat(every interval: Duration, clock: some Clock<Duration> & Sendable) async throws(Failure) -> Never {
        for await _ in AsyncTimerSequence(interval: interval, clock: clock) {
            do throws(FileFailure) {
                for index in locks.indices { locks[index] = try locks[index].update() }
            } catch {
                throw .lock(error)
            }
        }
        throw .cancelled
    }
}
