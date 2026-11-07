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
            let watcher: Watcher = Watcher(continuation: continuation)
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
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: String(reflecting: FileEvent.self))
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
            var context: FSEventStreamContext = unsafe FSEventStreamContext(version: 0, info: Unmanaged.passUnretained(self).toOpaque(), retain: nil, release: nil, copyDescription: nil)
            let flags: Int = kFSEventStreamCreateFlagUseCFTypes | kFSEventStreamCreateFlagFileEvents | kFSEventStreamCreateFlagWatchRoot | kFSEventStreamCreateFlagNoDefer
            func unwatched() {
                continuation.yield(.failure(FileFailure(file: file, reason: .unwatched)))
                continuation.finish()
            }
            guard
                let created: FSEventStreamRef = unsafe FSEventStreamCreate(
                    nil,
                    Self.changed,
                    &context,
                    [file.path(percentEncoded: false)] as CFArray,
                    FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
                    0,
                    FSEventStreamCreateFlags(flags),
                )
            else {
                unwatched()
                return
            }
            unsafe FSEventStreamSetDispatchQueue(created, queue)
            guard unsafe FSEventStreamStart(created) else {
                unsafe Self.invalidate(created)
                unwatched()
                return
            }
            unsafe stream = created
            continuation.yield(.success([]))
        }

        func stop() {
            if let stream: FSEventStreamRef = unsafe stream {
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
            guard let info: UnsafeMutableRawPointer = unsafe info else { return }
            let reported: AnyObject = unsafe Unmanaged<AnyObject>.fromOpaque(eventPaths).takeUnretainedValue()
            guard let paths: [String] = reported as? [String] else { return }
            unsafe Unmanaged<Watcher>.fromOpaque(info).takeUnretainedValue().continuation.yield(
                .success(zip(paths, unsafe UnsafeBufferPointer(start: eventFlags, count: count)).map { path, flags in FileEvent(url: URL(filePath: path), flags: flags) })
            )
        }
    }
}
