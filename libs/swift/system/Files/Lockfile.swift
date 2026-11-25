import AsyncAlgorithms
import CasePathsCore
public import Foundation
import Logging
import Results
import System

// --- [TYPES] ---------------------------------------------------------------------------

public protocol LockFailure: Error {
    static func lock(_ failure: FileFailure) -> Self
    static func cancelled(_ error: CancellationError) -> Self
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
        let work: @Sendable () async throws(Failure) -> Value = lockfiles.indices.reduce(body) { work, index in
            { @Sendable () async throws(Failure) -> Value in
                try await race(work) { () async throws(Failure) -> Value in try await holder.heartbeat(index, every: lockfiles[index].update, clock: clock, now: now) }
            }
        }
        let outcome = await Result(catching: work)
        let released: Result<Void, Failure> = release(await holder.held)
        return try outcome.mapError { error in error.releasing(released) }.flatMap { value in released.map { _ in value } }.get()
    }

    private static func acquire<Failure: LockFailure>(_ lockfiles: [Self], clock: some Clock<Duration> & Sendable, now: () -> Duration) async throws(Failure) -> Holder<Failure> {
        var generator = SystemRandomNumberGenerator()
        var delays: [[Duration]] = lockfiles.map { (lockfile: Self) -> [Duration] in lockfile.retry?.delays(using: &generator) ?? [] }
        while true {
            var held: [Held] = []
            let failure: FileFailure
            do throws(FileFailure) {
                for lockfile in lockfiles { held.append(try Held.acquire(lockfile, at: now())) }
                return Holder(held)
            } catch { failure = error }
            let (blocked, released): (URL, Result<Void, Failure>) = (lockfiles[held.count].url, release(held))
            guard failure.reason.is(\.held), case .success = released, let delay = delays[held.count].first else { throw Failure.lock(failure).releasing(released) }
            delays[held.count].removeFirst()
            Logger.current.debug("Waiting for held lock", metadata: ["lock": .string(blocked.path(percentEncoded: false)), "delay": .stringConvertible(delay)])
            try await race {
                () async throws(Failure) in
                switch await FileEvent.presence(of: blocked).first(where: { presence in presence[case: \.success] != true }) {
                    case .some(.failure(let error)): throw .lock(error)
                    case .some(.success): return
                    case .none: throw .cancelled(CancellationError())
                }
            } against: {
                () async throws(Failure) in
                do { try await clock.sleep(for: delay) } catch { throw .cancelled(CancellationError()) }
            }
        }
    }

    private static func release<Failure: LockFailure>(_ held: [Held]) -> Result<Void, Failure> {
        held.reversed().reduce(.success(())) { released, lock in
            let current = Result { () throws(FileFailure) in try lock.release() }.mapError(Failure.lock)
            return released.mapError { error in error.releasing(current) }.flatMap { _ in current }
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private struct Held: Equatable, Sendable {
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
                throw FileFailure(file: lockfile.url, reason: .filesystem(error as NSError))
            }
        }
        do throws(FileFailure) {
            try create()
        } catch let failure where failure.reason.is(\.held) {
            switch try stamped(lockfile.url) {
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
                .mapError { cleanup in FileFailure(file: lockfile.url, reason: .filesystem(AggregateError(first: error, remaining: [cleanup]) as NSError)) }.get()
            throw error
        }
    }

    func updated(at now: Duration) throws(FileFailure) -> Self {
        try unchanged()
        _ = try ifPresent(url) { try FileManager.default.setAttributes([.modificationDate: Date(timeIntervalSince1970: now / .seconds(1))], ofItemAtPath: url.path(percentEncoded: false)) }
        let current = try Self.current(url)
        guard current.inode == inode else { throw FileFailure(file: url, reason: .compromised) }
        return current
    }

    func release() throws(FileFailure) {
        try unchanged()
        try Self.remove(at: url)
    }

    private func unchanged() throws(FileFailure) {
        guard try Self.stamped(url) == self else { throw FileFailure(file: url, reason: .compromised) }
    }

    private static func current(_ url: URL) throws(FileFailure) -> Self {
        guard let held = try stamped(url) else { throw FileFailure(file: url, reason: .compromised) }
        return held
    }

    private static func stamped(_ url: URL) throws(FileFailure) -> Self? {
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

    private(set) var held: [Held]

    init(_ held: [Held]) {
        self.held = held
    }

    // --- [HEARTBEAT]

    func heartbeat(_ index: Int, every interval: Duration, clock: some Clock<Duration> & Sendable, now: @Sendable () -> Duration) async throws(Failure) -> Never {
        for await _ in AsyncTimerSequence(interval: interval, clock: clock) {
            do throws(FileFailure) { held[index] = try held[index].updated(at: now()) } catch { throw .lock(error) }
        }
        throw .cancelled(CancellationError())
    }
}
