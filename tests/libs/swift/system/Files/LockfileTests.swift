import Foundation
import Results
import System
import Testing

@testable import Files

// --- [ERRORS] --------------------------------------------------------------------------

private enum LockError: LockFailure {
    case lock(FileFailure)
    case cancelled
    indirect case cleanup(operation: Self, release: Self)

    func releasing(_ release: Result<Void, Self>) -> Self {
        switch release {
            case .success: self
            case .failure(let error): .cleanup(operation: self, release: error)
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct LockfileTests {
    let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString).resolvingSymlinksInPath()

    init() throws {
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
    }

    @Test
    func heartbeatFailureStillReleasesTheLocksItUpdated() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let (kept, lost) = (folder.appending(component: "kept.lock"), folder.appending(component: "lost.lock"))
        let outcome = await Result { () async throws(LockError) in
            try await Lockfile.withLocks([Lockfile(kept, stale: .seconds(2)), Lockfile(lost, stale: .seconds(2))]) { () async throws(LockError) in
                do { try FileManager.default.removeItem(at: lost) } catch { Issue.record(error) }
                do { try await ContinuousClock().sleep(for: .seconds(30)) } catch { throw .cancelled }
            }
        }
        guard case .failure(.cleanup(operation: .lock(let failure), release: _)) = outcome, case .compromised = failure.reason else {
            Issue.record("withLocks ended with \(outcome)")
            return
        }
        #expect(!FileManager.default.fileExists(atPath: kept.path(percentEncoded: false)))
    }

    @Test
    func staleTakeoverLeavesADirectoryWithContentsWhole() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let lock = folder.appending(component: "state.lock")
        let inside = lock.appending(component: "record")
        try FileManager.default.createDirectory(at: lock, withIntermediateDirectories: false)
        try Data().write(to: inside)
        try FileManager.default.setAttributes([.modificationDate: Date.now.addingTimeInterval(-60)], ofItemAtPath: lock.path(percentEncoded: false))
        let outcome = await Result { () async throws(LockError) in try await Lockfile.withLocks([Lockfile(lock)]) { () async throws(LockError) in } }
        guard case .failure(.lock(let failure)) = outcome, case .filesystem(let error as Errno) = failure.reason, error == Errno.directoryNotEmpty else {
            Issue.record("withLocks ended with \(outcome)")
            return
        }
        #expect(FileManager.default.fileExists(atPath: inside.path(percentEncoded: false)))
    }

    @Test
    func waiterAcquiresWhenTheHolderReleasesBeforeItsDelay() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let lock = folder.appending(component: "held.lock")
        try FileManager.default.createDirectory(at: lock, withIntermediateDirectories: false)
        let holder = Task {
            try await ContinuousClock().sleep(for: .milliseconds(200))
            try FileManager.default.removeItem(at: lock)
        }
        let elapsed = try await ContinuousClock().measure { () async throws(LockError) in
            try await Lockfile.withLocks([Lockfile(lock, retries: Backoff(count: 1, base: .seconds(30)))]) { () async throws(LockError) in }
        }
        try await holder.value
        #expect(elapsed < Duration.seconds(10))
        #expect(!FileManager.default.fileExists(atPath: lock.path(percentEncoded: false)))
    }
}
