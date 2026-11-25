public import CoreServices
import Dispatch
public import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct FileEvent: Sendable {
    public let url: URL
    public let flags: FSEventStreamEventFlags

    public static func events(of file: URL) -> AsyncStream<Result<[Self], FileFailure>> {
        AsyncStream { continuation in
            let watcher = Watcher(continuation: continuation)
            watcher.enqueue { watcher in watcher.start(file) }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
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

        func start(_ file: URL) {
            var context = unsafe FSEventStreamContext(version: 0, info: Unmanaged.passUnretained(self).toOpaque(), retain: nil, release: nil, copyDescription: nil)
            let flags = kFSEventStreamCreateFlagFileEvents | kFSEventStreamCreateFlagWatchRoot | kFSEventStreamCreateFlagNoDefer
            let created = unsafe FSEventStreamCreate(
                nil,
                Self.changed,
                &context,
                [file.path(percentEncoded: false)] as CFArray,
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
                    continuation.yield(.failure(FileFailure(file: file, reason: .unwatched)))
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
