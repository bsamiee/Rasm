import CoreServices
import Dispatch
public import Foundation
import Synchronization

// --- [ERRORS] --------------------------------------------------------------------------

public enum FileWatchFailure: CustomNSError, LocalizedError {
    case unstarted(URL)
    case unreadable(any Error)

    public var errorDescription: String? {
        switch self {
            case .unstarted(let file): "FSEvents could not start a stream on \(file.path(percentEncoded: false))"
            case .unreadable(let error): "Reading followed lines failed: \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .unreadable(let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum FileWatch {
    public static func events(of file: URL) -> AsyncStream<Result<[URL], FileWatchFailure>> {
        AsyncStream { continuation in
            let watcher: Watcher = Watcher(continuation: continuation)
            watcher.enqueue { watcher in watcher.start(file) }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
    }

    public static func lines(following handle: FileHandle) -> AsyncStream<Result<Data, FileWatchFailure>> {
        AsyncStream { continuation in
            let pending: Mutex<Data> = Mutex(Data())
            handle.readabilityHandler = { handle in
                switch Result(catching: { try handle.readToEnd() }) {
                    case .success(let appended):
                        let lines: [Data] = pending.withLock { pending in
                            let lines: [Data] = (pending + (appended ?? Data())).split(separator: UInt8(ascii: "\n"), omittingEmptySubsequences: false)
                            pending = lines[lines.endIndex - 1]
                            return lines.dropLast()
                        }
                        for line: Data in lines { continuation.yield(.success(line)) }
                    case .failure(let error):
                        continuation.yield(.failure(.unreadable(error)))
                        continuation.finish()
                }
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
            func started(_ stream: FSEventStreamRef) -> Bool {
                unsafe FSEventStreamSetDispatchQueue(stream, queue)
                return unsafe FSEventStreamStart(stream)
            }
            unsafe stream = FSEventStreamCreate(
                nil,
                changed,
                &context,
                [file.path(percentEncoded: false)] as CFArray,
                FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
                0,
                FSEventStreamCreateFlags(flags),
            )
            guard let created: FSEventStreamRef = unsafe stream, unsafe started(created) else {
                stop()
                continuation.yield(.failure(.unstarted(file)))
                continuation.finish()
                return
            }
            continuation.yield(.success([]))
        }

        func stop() {
            if let stream: FSEventStreamRef = (unsafe stream) {
                unsafe FSEventStreamStop(stream)
                unsafe FSEventStreamInvalidate(stream)
                unsafe FSEventStreamRelease(stream)
            }
            unsafe stream = nil
        }
    }

    private static let changed: FSEventStreamCallback = { _, info, count, paths, _, _ in
        guard let info: UnsafeMutableRawPointer = unsafe info else { return }
        unsafe Unmanaged<Watcher>.fromOpaque(info).takeUnretainedValue().continuation.yield(
            .success(unsafe UnsafeBufferPointer(start: paths.assumingMemoryBound(to: UnsafePointer<CChar>.self), count: count).map { path in unsafe URL(filePath: String(cString: path)) })
        )
    }
}
