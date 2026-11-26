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
    let retry: Backoff?

    public init(_ url: URL, stale: Duration = .seconds(10), update: Duration? = nil, retry: Backoff? = nil) {
        self.url = url
        self.stale = max(stale, .seconds(2))
        self.update = max(min(update ?? self.stale / 2, self.stale / 2), .seconds(1))
        self.retry = retry
    }

    public static func withLocks<Value: Sendable, Failure: LockFailure>(
        _ lockfiles: [Self],
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
        _ body: @escaping @Sendable () async throws(Failure) -> Value,
    ) async throws(Failure) -> Value {
        let (date, start) = (Date(), clock.now)
        let now: @Sendable () -> Duration = { .seconds(date.timeIntervalSince1970) + start.duration(to: clock.now) }
        let holder: Holder<Failure> = try await acquire(lockfiles, clock: clock, now: now)
        let outcome = await Result { () async throws(Failure) -> Value in
            guard let interval = lockfiles.map(\.update).min() else { return try await body() }
            return try await race(body) { () async throws(Failure) -> Value in try await holder.heartbeat(every: interval, clock: clock, now: now) }
        }
        let released: Result<Void, Failure> = release(await holder.locks)
        return try outcome.mapError { error in error.releasing(released) }.flatMap { value in released.map { _ in value } }.get()
    }

    private static func acquire<Failure: LockFailure>(_ lockfiles: [Self], clock: some Clock<Duration> & Sendable, now: () -> Duration) async throws(Failure) -> Holder<Failure> {
        var generator = SystemRandomNumberGenerator()
        var delays: [IndexingIterator<[Duration]>] = lockfiles.map { (lockfile: Self) -> IndexingIterator<[Duration]> in (lockfile.retry?.delays(using: &generator) ?? []).makeIterator() }
        while true {
            var locks: [Lock] = []
            let failure: FileFailure
            do throws(FileFailure) {
                for lockfile in lockfiles { locks.append(try Lock.acquire(lockfile, at: now())) }
                return Holder(locks)
            } catch { failure = error }
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

    static func acquire(_ lockfile: Lockfile, at now: Duration) throws(FileFailure) -> Self {
        func create() throws(FileFailure) {
            do {
                try FileManager.default.createDirectory(at: lockfile.url, withIntermediateDirectories: false, attributes: [.posixPermissions: 0o700])
            } catch CocoaError.fileWriteFileExists {
                throw FileFailure(file: lockfile.url, reason: .held)
            } catch {
                throw FileFailure(file: lockfile.url, reason: .filesystem(error))
            }
        }
        do throws(FileFailure) {
            try create()
        } catch let failure where failure.reason.is(\.held) {
            switch try read(lockfile.url) {
                case .some(let current) where current.modified >= now - lockfile.stale: throw failure
                case .some(let stale):
                    try remove(at: lockfile.url)
                    Logger.current.info("Removed stale lock", metadata: ["lock": .string(lockfile.url.path(percentEncoded: false)), "unchanged": .stringConvertible(now - stale.modified)])
                case .none: break
            }
            try create()
        }
        do throws(FileFailure) { return try current(lockfile.url) } catch {
            try Result { () throws(FileFailure) in try remove(at: lockfile.url) }
                .mapError { cleanup in FileFailure(file: lockfile.url, reason: .filesystem(AggregateError(first: error, remaining: [cleanup]))) }.get()
            throw error
        }
    }

    func update(at now: Duration) throws(FileFailure) -> Self {
        try verifyUnchanged()
        _ = try ifPresent(url) { try FileManager.default.setAttributes([.modificationDate: Date(timeIntervalSince1970: now / .seconds(1))], ofItemAtPath: url.path(percentEncoded: false)) }
        let current = try Self.current(url)
        guard current.inode == inode else { throw FileFailure(file: url, reason: .compromised) }
        return current
    }

    func release() throws(FileFailure) {
        try verifyUnchanged()
        try Self.remove(at: url)
    }

    private func verifyUnchanged() throws(FileFailure) {
        guard try Self.read(url) == self else { throw FileFailure(file: url, reason: .compromised) }
    }

    private static func current(_ url: URL) throws(FileFailure) -> Self {
        guard let lock = try read(url) else { throw FileFailure(file: url, reason: .compromised) }
        return lock
    }

    private static func read(_ url: URL) throws(FileFailure) -> Self? {
        try ifPresent(url) { try FilePath(url.path(percentEncoded: false)).stat() }.map { stat throws(FileFailure) in
            guard stat.type == .directory else { throw FileFailure(file: url, reason: .compromised) }
            return Self(url: url, inode: stat.inode, modified: Duration(stat.st_mtim))
        }
    }

    private static func remove(at url: URL) throws(FileFailure) {
        _ = try ifPresent(url) { try FileManager.default.removeItem(at: url) }
    }
}

private actor Holder<Failure: LockFailure> {
    // --- [STATE]

    private(set) var locks: [Lock]

    init(_ locks: [Lock]) {
        self.locks = locks
    }

    // --- [HEARTBEAT]

    func heartbeat(every interval: Duration, clock: some Clock<Duration> & Sendable, now: @Sendable () -> Duration) async throws(Failure) -> Never {
        for await _ in AsyncTimerSequence(interval: interval, clock: clock) {
            do throws(FileFailure) { locks = try locks.map { lock throws(FileFailure) in try lock.update(at: now()) } } catch { throw .lock(error) }
        }
        throw .cancelled
    }
}
