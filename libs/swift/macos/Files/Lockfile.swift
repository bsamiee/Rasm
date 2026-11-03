import AsyncAlgorithms
import CasePathsCore
import CurrentProcess
public import Foundation
import Results
import System
import os

// --- [TYPES] ---------------------------------------------------------------------------

public protocol LockFailure: Error {
    static func lock(_ failure: FileFailure) -> Self
    static func cancelled(_ error: CancellationError) -> Self
    func releasing(_ release: Result<Void, Self>) -> Self
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Lockfile: Sendable {
    fileprivate static let logger: Logger = Logger(for: Self.self)

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
        var generator: SystemRandomNumberGenerator = SystemRandomNumberGenerator()
        let holder: Holder<Failure> = try await acquire(lockfiles, delays: lockfiles.map { lockfile in lockfile.retry?.delays(using: &generator) ?? [] }, clock: clock)
        let work: @Sendable () async throws(Failure) -> Value = lockfiles.indices.reduce(body) { work, index in
            { @Sendable () async throws(Failure) -> Value in
                try await race(work) { () async throws(Failure) -> Value in try await holder.heartbeat(index, every: lockfiles[index].update, clock: clock) }
            }
        }
        let outcome: Result<Value, Failure> = await Result(catching: work)
        let released: Result<Void, Failure> = release(await holder.held)
        return try outcome.mapError { error in error.releasing(released) }.flatMap { value in released.map { _ in value } }.get()
    }

    private static func acquire<Failure: LockFailure>(_ lockfiles: [Self], delays: [[Duration]], clock: some Clock<Duration> & Sendable) async throws(Failure) -> Holder<Failure> {
        let acquired: (held: [Held], failure: (index: Int, error: FileFailure)?) = lockfiles.indices.reduce(into: ([], nil)) { acquired, index in
            guard acquired.failure == nil else { return }
            switch Result(catching: { () throws(FileFailure) in try Held.acquire(lockfiles[index]) }) {
                case .success(let held): acquired.held.append(held)
                case .failure(let error): acquired.failure = (index, error)
            }
        }
        guard case (let index, let error)? = acquired.failure else { return Holder(acquired.held) }
        let released: Result<Void, Failure> = release(acquired.held)
        guard error.reason.is(\.held), case .success = released, let delay: Duration = delays[index].first else { throw Failure.lock(error).releasing(released) }
        let blocked: URL = lockfiles[index].url
        logger.debug("Waiting up to \(delay, privacy: .public) for held lock \(blocked.path(percentEncoded: false), privacy: .private)")
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
        return try await acquire(lockfiles, delays: delays.indices.map { offset in offset == index ? Array(delays[offset].dropFirst()) : delays[offset] }, clock: clock)
    }

    private static func release<Failure: LockFailure>(_ held: [Held]) -> Result<Void, Failure> {
        held.reversed().reduce(.success(())) { released, lock in
            let current: Result<Void, Failure> = Result { () throws(FileFailure) in try lock.release() }.mapError(Failure.lock)
            return released.mapError { error in error.releasing(current) }.flatMap { _ in current }
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private struct Held: Equatable, Sendable {
    let url: URL
    let inode: Inode
    let modified: Duration

    static func acquire(_ lockfile: Lockfile) throws(FileFailure) -> Self {
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
            let now: Duration = .seconds(Date().timeIntervalSince1970)
            switch try stamped(lockfile.url) {
                case .some(let current) where current.modified >= now - lockfile.stale: throw failure
                case .some(let stale):
                    try remove(at: lockfile.url)
                    Lockfile.logger.notice("Removed stale lock \(lockfile.url.path(percentEncoded: false), privacy: .private) unchanged for \(now - stale.modified, privacy: .public)")
                case .none: break
            }
            try create()
        }
        do throws(FileFailure) {
            return try current(lockfile.url)
        } catch {
            try Result { () throws(FileFailure) in try remove(at: lockfile.url) }
                .mapError { cleanup in FileFailure(file: lockfile.url, reason: .filesystem(AggregateError(first: error, remaining: [cleanup]))) }
                .get()
            throw error
        }
    }

    func updated() throws(FileFailure) -> Self {
        try unchanged()
        var touched: URL = url
        var values: URLResourceValues = URLResourceValues()
        values.contentModificationDate = Date()
        let present: Void? = try ifPresent(url) { try touched.setResourceValues(values) }
        guard present != nil else { throw FileFailure(file: url, reason: .compromised) }
        let current: Self = try Self.current(url)
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
        guard let held: Self = try stamped(url) else { throw FileFailure(file: url, reason: .compromised) }
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
    func heartbeat(_ index: Int, every interval: Duration, clock: some Clock<Duration> & Sendable) async throws(Failure) -> Never {
        for await _ in AsyncTimerSequence(interval: interval, clock: clock) {
            do throws(FileFailure) { held[index] = try held[index].updated() } catch { throw .lock(error) }
        }
        throw .cancelled(CancellationError())
    }
}
