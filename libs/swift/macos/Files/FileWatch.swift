import CoreServices
import Dispatch
public import Foundation
import Synchronization

// --- [ERRORS] --------------------------------------------------------------------------

public enum FileWatchFailure: LocalizedError {
    case unstarted(URL)

    public var errorDescription: String? {
        switch self {
            case .unstarted(let file): "FSEvents stream on \(file.path(percentEncoded: false)) failed to start"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum FileWatch: Sendable {
    public static func events(of file: URL) -> AsyncStream<Result<[URL], FileWatchFailure>> {
        AsyncStream { continuation in
            let watcher: Watcher = Watcher(continuation: continuation)
            watcher.enqueue { watcher in watcher.start(file) }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
    }

    public static func lines(following handle: FileHandle) -> AsyncStream<Data> {
        AsyncStream { continuation in
            let pending: Mutex<Data> = Mutex(Data())
            handle.readabilityHandler = { handle in
                let appended: Data = handle.availableData
                let lines: [Data] = pending.withLock { pending in
                    let lines: [Data] = (pending + appended).split(separator: UInt8(ascii: "\n"), omittingEmptySubsequences: false)
                    pending = appended.isEmpty ? Data() : lines[lines.endIndex - 1]
                    return appended.isEmpty ? lines.filter { line in !line.isEmpty } : lines.dropLast()
                }
                for line: Data in lines { continuation.yield(line) }
                if appended.isEmpty { continuation.finish() }
            }
            continuation.onTermination = { _ in handle.readabilityHandler = nil }
        }
    }

    @safe private actor Watcher {
        // --- [STATE]
        let continuation: AsyncStream<Result<[URL], FileWatchFailure>>.Continuation
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: "Files.FileWatch")
        private var stream: FSEventStreamRef?

        init(continuation: AsyncStream<Result<[URL], FileWatchFailure>>.Continuation) {
            self.continuation = continuation
        }

        nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

        // --- [LIFECYCLE]
        nonisolated func enqueue(_ work: @escaping @Sendable (isolated Watcher) -> Void) {
            queue.async { [self] in assumeIsolated(work) }
        }

        func start(_ file: URL) {
            var context: FSEventStreamContext = unsafe FSEventStreamContext(version: 0, info: Unmanaged.passUnretained(self).toOpaque(), retain: nil, release: nil, copyDescription: nil)
            let flags: Int = kFSEventStreamCreateFlagFileEvents | kFSEventStreamCreateFlagWatchRoot | kFSEventStreamCreateFlagNoDefer
            func unstarted() {
                continuation.yield(.failure(.unstarted(file)))
                continuation.finish()
            }
            guard
                let created: FSEventStreamRef = unsafe FSEventStreamCreate(
                    nil,
                    changed,
                    &context,
                    [file.path(percentEncoded: false)] as CFArray,
                    FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
                    0,
                    FSEventStreamCreateFlags(flags),
                )
            else {
                unstarted()
                return
            }
            unsafe FSEventStreamSetDispatchQueue(created, queue)
            guard unsafe FSEventStreamStart(created) else {
                unsafe Self.invalidate(created)
                unstarted()
                return
            }
            unsafe stream = created
            continuation.yield(.success([]))
        }

        func stop() {
            if let stream: FSEventStreamRef = (unsafe stream) {
                unsafe FSEventStreamStop(stream)
                unsafe Self.invalidate(stream)
            }
            unsafe stream = nil
        }

        private static func invalidate(_ stream: FSEventStreamRef) {
            unsafe FSEventStreamInvalidate(stream)
            unsafe FSEventStreamRelease(stream)
        }
    }

    private static let changed: FSEventStreamCallback = { _, info, count, paths, _, _ in
        guard let info: UnsafeMutableRawPointer = unsafe info else { return }
        unsafe Unmanaged<Watcher>.fromOpaque(info).takeUnretainedValue().continuation.yield(
            .success(unsafe UnsafeBufferPointer(start: paths.assumingMemoryBound(to: UnsafePointer<CChar>.self), count: count).map { path in unsafe URL(filePath: String(cString: path)) })
        )
    }
}
