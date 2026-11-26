import CasePathsCore
public import CoreServices
import Dispatch
public import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct FileEvent: Sendable {
    public let url: URL
    public let flags: FSEventStreamEventFlags

    public var mustScanSubDirs: Bool { flags & FSEventStreamEventFlags(kFSEventStreamEventFlagMustScanSubDirs) != 0 }

    public static func events(of file: URL) -> AsyncStream<Result<[Self], FileFailure>> {
        AsyncStream { continuation in
            let watcher = Watcher(continuation: continuation)
            watcher.enqueue { watcher in watcher.start([file]) }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
    }

    public static func written<Value, Failure: Error>(
        _ files: [URL],
        during work: () async throws(Failure) -> Value,
    ) async throws(Failure) -> (value: Value, written: Result<[URL], FileFailure>) {
        let located = Result { () throws(FileFailure) in
            try files.filter(\.isFileURL).map { file throws(FileFailure) in
                do throws(Errno) {
                    return (file: file, path: try file.deletingLastPathComponent().canonical().appending(component: file.lastPathComponent))
                } catch {
                    throw FileFailure(file: file, reason: .filesystem(error as NSError))
                }
            }
        }
        guard case .success(let watched) = located, !watched.isEmpty else { return (try await work(), located.map { _ in [] }) }
        let (batches, continuation) = AsyncStream<Result<[Self], FileFailure>>.makeStream()
        let watcher = Watcher(continuation: continuation)
        defer { watcher.enqueue { watcher in watcher.stop() } }
        watcher.enqueue { watcher in watcher.start(watched.map(\.path)) }
        var received = batches.makeAsyncIterator()
        if case .failure(let failure)? = await received.next() { return (try await work(), .failure(failure)) }
        let value = try await work()
        await watcher.flush()
        continuation.finish()
        var events: [Self] = []
        while let batch = await received.next() { events += batch[case: \.success] ?? [] }
        let changed = FSEventStreamEventFlags(kFSEventStreamEventFlagItemCreated | kFSEventStreamEventFlagItemModified | kFSEventStreamEventFlagItemRenamed)
        let paths: [FilePath] = events.filter { event in event.flags & changed != 0 }.map { event in FilePath(event.url.path(percentEncoded: false)) }
        return (value, .success(watched.filter { _, document in paths.contains { path in path.starts(with: FilePath(document.path(percentEncoded: false))) } }.map(\.file)))
    }

    public static func presence(of file: URL) -> some AsyncSequence<Result<Bool, FileFailure>, Never> & Sendable {
        events(of: file).map { batch in
            batch.flatMap { _ in Result { () throws(FileFailure) in try ifPresent(file) { try FilePath(file.path(percentEncoded: false)).stat() } != nil } }
        }
    }

    @safe private actor Watcher {
        // --- [STATE]

        private let continuation: AsyncStream<Result<[FileEvent], FileFailure>>.Continuation
        private let queue = DispatchSerialQueue(label: String(reflecting: FileEvent.self))
        private var stream: FSEventStreamRef?

        init(continuation: AsyncStream<Result<[FileEvent], FileFailure>>.Continuation) {
            self.continuation = continuation
        }

        nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

        // --- [LIFECYCLE]

        nonisolated func enqueue(_ work: @escaping @Sendable (isolated Watcher) -> Void) {
            queue.async { [self] in assumeIsolated(work) }
        }

        func start(_ files: [URL]) {
            var context = unsafe FSEventStreamContext(version: 0, info: Unmanaged.passUnretained(self).toOpaque(), retain: nil, release: nil, copyDescription: nil)
            let flags = kFSEventStreamCreateFlagFileEvents | kFSEventStreamCreateFlagWatchRoot | kFSEventStreamCreateFlagNoDefer
            let created = unsafe FSEventStreamCreate(
                nil,
                Self.changed,
                &context,
                files.map { file in file.path(percentEncoded: false) } as CFArray,
                FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
                0,
                FSEventStreamCreateFlags(flags),
            )
            if let created = unsafe created { unsafe FSEventStreamSetDispatchQueue(created, queue) }
            switch unsafe created.map(FSEventStreamStart) {
                case true?:
                    unsafe stream = created
                    continuation.yield(.success([]))
                case false?, nil:
                    if let created = unsafe created { unsafe Self.invalidate(created) }
                    continuation.yield(.failure(FileFailure(files: files, reason: .unwatched)))
                    continuation.finish()
            }
        }

        func stop() {
            if let stream = unsafe stream {
                unsafe FSEventStreamStop(stream)
                unsafe Self.invalidate(stream)
            }
            unsafe stream = nil
        }

        func flush() async {
            guard let started = unsafe stream else { return }
            nonisolated(unsafe) let flushing = unsafe started
            await withCheckedContinuation { (flushed: CheckedContinuation<Void, Never>) in
                DispatchQueue.global().async {
                    unsafe FSEventStreamFlushSync(flushing)
                    flushed.resume()
                }
            }
        }

        private static func invalidate(_ stream: FSEventStreamRef) {
            unsafe FSEventStreamInvalidate(stream)
            unsafe FSEventStreamRelease(stream)
        }

        // --- [CALLBACKS]

        private static let changed: FSEventStreamCallback = { _, info, count, eventPaths, eventFlags, _ in
            let paths = unsafe UnsafeBufferPointer(start: eventPaths.assumingMemoryBound(to: UnsafeMutablePointer<CChar>.self), count: count)
            unsafe info.map { (info: UnsafeMutableRawPointer) -> Watcher in unsafe Unmanaged<Watcher>.fromOpaque(info).takeUnretainedValue() }?.continuation.yield(
                .success(
                    unsafe zip(paths, UnsafeBufferPointer(start: eventFlags, count: count)).map { path, flags -> FileEvent in FileEvent(url: URL(filePath: unsafe String(cString: path)), flags: flags)
                    }
                )
            )
        }
    }
}
