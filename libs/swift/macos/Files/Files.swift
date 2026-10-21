import CoreServices
import Dispatch
public import Foundation
import System

// --- [ERRORS] --------------------------------------------------------------------------

public enum FileWatchFailure: LocalizedError {
    case unstarted(URL)
    case unreadable(URL, any Error)

    public var errorDescription: String? {
        switch self {
            case .unstarted(let file): "FSEvents could not start a stream on \(file.path(percentEncoded: false))"
            case .unreadable(let file, let error): "Reading \(file.path(percentEncoded: false)) failed: \(error.localizedDescription)"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func ifPresent<Value>(_ operation: () throws -> Value) -> Result<Value?, any Error> {
    Result { try operation() }.map(Optional.some).flatMapError { error in
        switch error {
            case CocoaError.fileNoSuchFile, CocoaError.fileReadNoSuchFile, .noSuchFileOrDirectory as Errno: .success(nil)
            default: .failure(error)
        }
    }
}

public enum FileWatch {
    public static func events(of file: URL) -> AsyncStream<Result<Void, FileWatchFailure>> {
        AsyncStream { continuation in
            let watcher: Watcher = Watcher(continuation: continuation)
            watcher.enqueue { watcher in watcher.start(file) }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
    }

    public static func lines(appendedTo file: URL) -> AsyncStream<Result<Data, FileWatchFailure>> {
        let (chunks, chunk): (AsyncStream<Result<Data?, FileWatchFailure>>, AsyncStream<Result<Data?, FileWatchFailure>>.Continuation) = AsyncStream.makeStream()
        let opened: Result<FileHandle, any Error> = Result {
            let handle: FileHandle = try FileHandle(
                fileDescriptor: FileDescriptor.open(FilePath(file.path(percentEncoded: false)), .readOnly, options: [.create, .closeOnExec], permissions: [.ownerReadWrite]).rawValue,
                closeOnDealloc: true,
            )
            try handle.seekToEnd()
            return handle
        }
        switch opened {
            case .success(let handle):
                let source: any DispatchSourceRead = DispatchSource.makeReadSource(fileDescriptor: handle.fileDescriptor, queue: DispatchQueue(label: "Files.FileWatch.lines"))
                source.setEventHandler { chunk.yield(Result { try handle.readToEnd() }.mapError { error in .unreadable(file, error) }) }
                source.setCancelHandler { withExtendedLifetime(handle) { chunk.finish() } }
                chunk.onTermination = { _ in source.cancel() }
                source.activate()
            case .failure(let error):
                chunk.yield(.failure(.unreadable(file, error)))
                chunk.finish()
        }
        return AsyncStream { continuation in
            let following: Task<Void, Never> = Task {
                defer {
                    chunk.finish()
                    continuation.finish()
                }
                var pending: Data = Data()
                for await read: Result<Data?, FileWatchFailure> in chunks {
                    switch read {
                        case .success(let appended):
                            let lines: [Data] = (pending + (appended ?? Data())).split(separator: UInt8(ascii: "\n"), omittingEmptySubsequences: false)
                            pending = lines[lines.endIndex - 1]
                            for line: Data in lines.dropLast() { continuation.yield(.success(line)) }
                        case .failure(let error):
                            continuation.yield(.failure(error))
                            return
                    }
                }
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }

    @safe private actor Watcher {
        // --- [STATE]
        let continuation: AsyncStream<Result<Void, FileWatchFailure>>.Continuation
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: "Files.FileWatch")
        private var stream: FSEventStreamRef?

        init(continuation: AsyncStream<Result<Void, FileWatchFailure>>.Continuation) {
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
            continuation.yield(.success(()))
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

    private static let changed: FSEventStreamCallback = { _, info, _, _, _, _ in
        guard let info: UnsafeMutableRawPointer = unsafe info else { return }
        unsafe Unmanaged<Watcher>.fromOpaque(info).takeUnretainedValue().continuation.yield(.success(()))
    }
}
