import Files
import Foundation
import Logging
import Results
import System
import Testing

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

// --- [MODELS] --------------------------------------------------------------------------

private struct Logged: LogHandler {
    let events: AsyncStream<Void>.Continuation
    var logLevel: Logger.Level = .trace
    var metadata: Logger.Metadata = [:]

    subscript(metadataKey key: String) -> Logger.Metadata.Value? {
        get { metadata[key] }
        set { metadata[key] = newValue }
    }

    func log(event _: LogEvent) { events.yield() }
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
    func waiterAcquiresOnTheReleaseOfTheHolderBeforeItsDelayEnds() async throws {
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let lock = folder.appending(component: "held.lock")
        try FileManager.default.createDirectory(at: lock, withIntermediateDirectories: false)
        let (waits, waiting) = AsyncStream<Void>.makeStream()
        let holder = Task {
            _ = await waits.first { _ in true }
            try FileManager.default.removeItem(at: lock)
        }
        try await withLogger(Logger(label: String(reflecting: Self.self)) { _ in Logged(events: waiting) }) { _ async throws(LockError) in
            try await Lockfile.withLocks([Lockfile(lock, retries: Backoff(count: 1, base: .seconds(3600)))]) { () async throws(LockError) in }
        }
        try await holder.value
        #expect(!FileManager.default.fileExists(atPath: lock.path(percentEncoded: false)))
    }
}
