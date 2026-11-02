import CasePaths
import CustomDump
import Files
import Foundation
import Synchronization
import TestSupport
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private final class Transitions: Sendable {
    let entries: Mutex<[String]> = Mutex([])

    func record(_ entry: String) {
        entries.withLock { entries in entries.append(entry) }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

@CasePathable
private enum LockOutcome: LockFailure {
    case lock(FileFailure)
    case cancelled(CancellationError)
    case body(Int)
    indirect case cleanup(operation: Self, release: Self)

    var operation: Self {
        switch self {
            case .cleanup(let operation, _): operation
            case .lock, .cancelled, .body: self
        }
    }

    var lockReason: FileFailure.Reason? { operation[case: \.lock]?.reason }

    func releasing(_ release: Result<Void, Self>) -> Self {
        switch release {
            case .success: self
            case .failure(let failure): .cleanup(operation: self, release: failure)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func lockPath(_ name: String = "state.json.lock") throws -> URL {
    try #require(TestDirectory.current).appending(path: name)
}

private func holdsDirectory(at url: URL) throws -> Bool {
    try ifPresent { try URL(filePath: url.path(percentEncoded: false)).resourceValues(forKeys: [.isDirectoryKey]).isDirectory } == true
}

private func touch(_ url: URL, at date: Date) throws {
    var target: URL = URL(filePath: url.path(percentEncoded: false))
    var values: URLResourceValues = URLResourceValues()
    values.contentModificationDate = date
    try target.setResourceValues(values)
}

private func createLock(at url: URL, age: Duration) throws {
    try FileManager.default.createDirectory(at: url, withIntermediateDirectories: false)
    try touch(url, at: Date(timeIntervalSinceNow: -(age / .seconds(1))))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.testDirectory, .timeLimit(.minutes(1)))
struct Lockfiles {
    @Test
    func holdsDirectoryAtLockPathOnlyWhileBodyRuns() async throws {
        let lock: URL = try lockPath()
        let held: Bool = try await Lockfile.withLocks([Lockfile(lock)]) { () throws(LockOutcome) -> Result<Bool, any Error> in Result { try holdsDirectory(at: lock) } }.get()
        #expect(held)
        #expect(try !holdsDirectory(at: lock))
    }

    @Test
    func heldLockFailsSecondHolderWithoutRetry() async throws {
        let lock: URL = try lockPath()
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks([Lockfile(lock)]) { () throws(LockOutcome) -> Int in
                try await Lockfile.withLocks([Lockfile(lock)]) { () throws(LockOutcome) -> Int in 1 }
            }
        }
        expectNoDifference(failure.lockReason?.is(\.held), true)
        #expect(try !holdsDirectory(at: lock))
    }

    @Test
    func exhaustedRetriesFailHeld() async throws {
        let lock: URL = try lockPath()
        let retry: Backoff = Backoff(count: 3, base: .milliseconds(20))
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks([Lockfile(lock)]) { () throws(LockOutcome) -> Int in
                try await Lockfile.withLocks([Lockfile(lock, retry: retry)]) { () throws(LockOutcome) -> Int in 1 }
            }
        }
        expectNoDifference(failure.lockReason?.is(\.held), true)
    }

    @Test
    func contendingHoldersRunOneAtATime() async throws {
        let lock: URL = try lockPath()
        let transitions: Transitions = Transitions()
        let retry: Backoff = Backoff(count: 400, base: .milliseconds(5), growth: 1, jitter: 1)
        try await withThrowingTaskGroup(of: Void.self) { group in
            for holder: Int in 0..<8 {
                group.addTask {
                    try await Lockfile.withLocks([Lockfile(lock, retry: retry)]) { () throws(LockOutcome) in
                        transitions.record("enter \(holder)")
                        await Task.yield()
                        transitions.record("leave \(holder)")
                    }
                }
            }
            try await group.waitForAll()
        }
        let entries: [String] = transitions.entries.withLock(\.self)
        let alternating: [String] = entries.indices.map { index in index.isMultiple(of: 2) ? entries[index] : "leave" + entries[index - 1].dropFirst(5) }
        let entered: Set<String> = Set(entries.filter { entry in entry.hasPrefix("enter") })
        expectNoDifference(entries, alternating)
        expectNoDifference(entered, Set((0..<8).map { holder in "enter \(holder)" }))
        expectNoDifference(entries.count, 16)
        #expect(try !holdsDirectory(at: lock))
    }

    @Test
    func staleLockIsTakenOver() async throws {
        let lock: URL = try lockPath()
        try createLock(at: lock, age: .seconds(60))
        let value: Int = try await Lockfile.withLocks([Lockfile(lock, stale: .seconds(10))]) { () throws(LockOutcome) -> Int in 7 }
        expectNoDifference(value, 7)
        #expect(try !holdsDirectory(at: lock))
    }

    @Test
    func staleThresholdBelowTwoSecondsCountsAsTwoSeconds() async throws {
        let lock: URL = try lockPath()
        try createLock(at: lock, age: .milliseconds(500))
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks([Lockfile(lock, stale: .zero)]) { () throws(LockOutcome) -> Int in 1 }
        }
        expectNoDifference(failure.lockReason?.is(\.held), true)
        #expect(try holdsDirectory(at: lock))
    }

    @Test
    func heartbeatAdvancesModificationDateWhileHeld() async throws {
        let lock: URL = try lockPath()
        let modified: @Sendable () throws -> Date? = {
            try URL(filePath: lock.path(percentEncoded: false)).resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate
        }
        let reads: [Result<Date?, any Error>] = try await Lockfile.withLocks([Lockfile(lock, stale: .seconds(2))]) { () throws(LockOutcome) -> [Result<Date?, any Error>] in
            let first: Result<Date?, any Error> = Result { try modified() }
            _ = await FileEvent.events(of: lock).dropFirst().first { _ in true }
            return [first, Result { try modified() }]
        }
        let dates: [Date] = try reads.compactMap { read in try read.get() }
        expectNoDifference(dates.count, 2)
        let advanced: Bool = try #require(dates.last) > dates[0]
        #expect(advanced, "dates \(dates)")
    }

    @Test
    func changedModificationDateCompromisesLockAndCancelsBody() async throws {
        let lock: URL = try lockPath()
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks([Lockfile(lock, stale: .seconds(2))]) { () throws(LockOutcome) -> Int in
                do { try touch(lock, at: Date(timeIntervalSinceNow: -3600)) } catch { throw .body(-1) }
                do { try await Task.sleep(for: .seconds(3600)) } catch { throw .cancelled(CancellationError()) }
                return 0
            }
        }
        expectNoDifference(failure.lockReason?.is(\.compromised), true)
    }

    @Test
    func bodyFailurePropagatesAndReleasesLock() async throws {
        let lock: URL = try lockPath()
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks([Lockfile(lock)]) { () throws(LockOutcome) -> Int in throw .body(7) }
        }
        expectNoDifference(failure.operation[case: \.body], 7)
        #expect(try !holdsDirectory(at: lock))
    }

    @Test
    func canceledRetryWaitThrowsCancelledCase() async throws {
        let lock: URL = try lockPath()
        let retry: Backoff = Backoff(count: 100, base: .seconds(1), growth: 1)
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks([Lockfile(lock)]) { () throws(LockOutcome) -> Int in
                let waited: Result<Int, LockOutcome>? = await withTaskGroup(of: Result<Int, LockOutcome>.self) { group in
                    group.addTask { await Result { () async throws(LockOutcome) -> Int in try await Lockfile.withLocks([Lockfile(lock, retry: retry)]) { () throws(LockOutcome) -> Int in 1 } } }
                    group.cancelAll()
                    return await group.next()
                }
                return try waited?.get() ?? 0
            }
        }
        expectNoDifference(failure.operation.is(\.cancelled), true)
    }

    @Test
    func allLocksAreHeldTogetherAndReleasedTogether() async throws {
        let locks: [URL] = try ["a.lock", "b.lock", "c.lock"].map(lockPath)
        let held: [Bool] = try await Lockfile.withLocks(locks.map { lock in Lockfile(lock) }) { () throws(LockOutcome) -> Result<[Bool], any Error> in Result { try locks.map(holdsDirectory) } }.get()
        expectNoDifference(held, [true, true, true])
        expectNoDifference(try locks.map(holdsDirectory), [false, false, false])
    }

    @Test
    func oneHeldLockReleasesOthersAlreadyAcquired() async throws {
        let locks: [URL] = try ["a.lock", "b.lock", "c.lock"].map(lockPath)
        try createLock(at: locks[1], age: .zero)
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks(locks.map { lock in Lockfile(lock) }) { () throws(LockOutcome) -> Int in 1 }
        }
        expectNoDifference(failure.lockReason?.is(\.held), true)
        expectNoDifference(try locks.map(holdsDirectory), [false, true, false])
    }

    @Test
    func missingParentFailsWithoutCreatingIt() async throws {
        let lock: URL = try lockPath("absent/state.json.lock")
        let failure: LockOutcome = try await #require(throws: LockOutcome.self) {
            try await Lockfile.withLocks([Lockfile(lock)]) { () throws(LockOutcome) -> Int in 1 }
        }
        expectNoDifference(failure.lockReason?.is(\.filesystem), true)
        #expect(try !holdsDirectory(at: lock.deletingLastPathComponent()))
    }
}
