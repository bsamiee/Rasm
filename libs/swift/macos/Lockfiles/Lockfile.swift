import AsyncAlgorithms
import Files
public import Foundation
public import Results
public import System

// --- [TYPES] ---------------------------------------------------------------------------

public protocol LockFailure: DeadlineFailure {
    static func lock(_ failure: LockfileFailure) -> Self
    func releasing(_ release: Result<Void, Self>) -> Self
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Lockfile: Equatable, Sendable {
    let url: URL
    let stale: Duration
    let update: Duration
    let retries: [Duration]

    public init(_ url: URL, stale: Duration = .seconds(10), update: Duration? = nil, retries: [Duration] = []) {
        self.url = url
        self.stale = max(stale, .seconds(2))
        self.update = max(min(update ?? self.stale / 2, self.stale / 2), .seconds(1))
        self.retries = retries
    }

    public static func withLocks<Value: Sendable, Failure: LockFailure>(
        _ lockfiles: [Self],
        _ body: @escaping @Sendable () async throws(Failure) -> Value,
    ) async throws(Failure) -> Value {
        let holder: Holder<Failure> = try await acquire(lockfiles)
        let work: @Sendable () async -> Result<Value, Failure> = lockfiles.indices.reduce(
            { @Sendable in await Result(catching: body) },
            { work, index in
                { @Sendable in await race(work) { await Result { () async throws(Failure) -> Value in try await holder.heartbeat(index) } } }
            },
        )
        let outcome: Result<Value, Failure> = await work()
        let released: Result<Void, Failure> = release(await holder.lockfiles)
        return try outcome.mapError { error in error.releasing(released) }.flatMap { value in released.map { _ in value } }.get()
    }

    private static func acquire<Failure: LockFailure>(_ lockfiles: [Self]) async throws(Failure) -> Holder<Failure> {
        let acquired: (lockfiles: [Held], failure: (lockfile: Self, error: LockfileFailure)?) = lockfiles.reduce(into: ([], nil)) { acquired, lockfile in
            guard acquired.failure == nil else { return }
            switch Result(catching: { () throws(LockfileFailure) in try Held.acquire(lockfile) }) {
                case .success(let held): acquired.lockfiles.append(held)
                case .failure(let error): acquired.failure = (lockfile, error)
            }
        }
        guard case (let blocked, let error)? = acquired.failure else { return Holder(acquired.lockfiles) }
        let released: Result<Void, Failure> = release(acquired.lockfiles)
        guard case .held = error, case .success = released, let delay: Duration = blocked.retries.first else { throw Failure.lock(error).releasing(released) }
        try await race {
            await FileWatch.events(of: blocked.url).first { next in if case .failure = next { true } else { !FileManager.default.fileExists(atPath: blocked.url.path(percentEncoded: false)) } }
                .map { next in next.map { _ in }.mapError { error in Failure.lock(.filesystem(error)) } } ?? .failure(.deadline(.cancelled))
        } against: {
            await Result { try await Task.sleep(for: delay) }.mapError { _ in Failure.deadline(.cancelled) }
        }
        .get()
        return try await acquire(
            lockfiles.map { lockfile in lockfile == blocked ? Self(blocked.url, stale: blocked.stale, update: blocked.update, retries: Array(blocked.retries.dropFirst())) : lockfile }
        )
    }

    private static func release<Failure: LockFailure>(_ lockfiles: [Held]) -> Result<Void, Failure> {
        lockfiles.reversed().reduce(.success(())) { released, held in
            let current: Result<Void, Failure> = Result { () throws(LockfileFailure) in try held.release() }.mapError(Failure.lock)
            return released.mapError { error in error.releasing(current) }.flatMap { _ in current }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum LockfileFailure: CustomNSError, LocalizedError {
    case held(URL)
    case compromised(URL)
    case filesystem(any Error)

    public var errorDescription: String? {
        switch self {
            case .held(let url): "\(url.path(percentEncoded: false)) is already held"
            case .compromised(let url): "\(url.path(percentEncoded: false)) changed while this process held it"
            case .filesystem(let error): "A lockfile operation failed: \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .filesystem(let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private struct Held: Equatable, Sendable {
    let lockfile: Lockfile
    let inode: Inode
    let modified: Duration

    static func acquire(_ lockfile: Lockfile) throws(LockfileFailure) -> Self {
        func create() throws(LockfileFailure) {
            try Result { try FileManager.default.createDirectory(at: lockfile.url, withIntermediateDirectories: false, attributes: [.posixPermissions: 0o700]) }
                .mapError { error -> LockfileFailure in if case CocoaError.fileWriteFileExists = error { .held(lockfile.url) } else { .filesystem(error) } }
                .get()
        }
        do throws(LockfileFailure) {
            try create()
        } catch .held {
            guard try stamped(lockfile).modified < .seconds(Date().timeIntervalSince1970) - lockfile.stale else { throw .held(lockfile.url) }
            try remove(at: lockfile.url)
            try create()
        }
        return try Result { () throws(LockfileFailure) in try stamped(lockfile) }
            .flatMapError { error in
                Result { () throws(LockfileFailure) in try remove(at: lockfile.url) }
                    .mapError { cleanup in .filesystem(AggregateError(first: error, remaining: [cleanup])) }
                    .flatMap { _ in .failure(error) }
            }
            .get()
    }

    func updated() throws(LockfileFailure) -> Self {
        try unchanged()
        var touched: URL = lockfile.url
        var values: URLResourceValues = URLResourceValues()
        values.contentModificationDate = Date()
        try Result { try touched.setResourceValues(values) }.mapError(LockfileFailure.filesystem).get()
        let current: Self = try Self.stamped(lockfile)
        guard current.inode == inode else { throw .compromised(lockfile.url) }
        return current
    }

    func release() throws(LockfileFailure) {
        try unchanged()
        try Self.remove(at: lockfile.url)
    }

    private func unchanged() throws(LockfileFailure) {
        guard try Self.stamped(lockfile) == self else { throw .compromised(lockfile.url) }
    }

    private static func stamped(_ lockfile: Lockfile) throws(LockfileFailure) -> Self {
        let stat: Stat = try Result { () throws(Errno) in try FilePath(lockfile.url.path(percentEncoded: false)).stat() }.mapError(LockfileFailure.filesystem).get()
        guard stat.type == .directory else { throw .compromised(lockfile.url) }
        return Self(lockfile: lockfile, inode: stat.inode, modified: Duration(stat.st_mtim))
    }

    private static func remove(at url: URL) throws(LockfileFailure) {
        try Result { try FileManager.default.removeItem(at: url) }.mapError(LockfileFailure.filesystem).get()
    }
}

private actor Holder<Failure: LockFailure> {
    // --- [STATE]
    private(set) var lockfiles: [Held]

    init(_ lockfiles: [Held]) {
        self.lockfiles = lockfiles
    }

    // --- [HEARTBEAT]
    func heartbeat(_ index: Int) async throws(Failure) -> Never {
        for await _ in AsyncTimerSequence(interval: lockfiles[index].lockfile.update, clock: .continuous) {
            lockfiles[index] = try Result { () throws(LockfileFailure) in try lockfiles[index].updated() }.mapError(Failure.lock).get()
        }
        throw .deadline(.cancelled)
    }
}
