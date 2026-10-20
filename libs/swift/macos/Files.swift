import CoreServices
public import CryptoKit
import Dispatch
public import Foundation
import System

// --- [ERRORS] --------------------------------------------------------------------------

public enum FileWatchFailure: LocalizedError {
    case unstarted(URL)

    public var errorDescription: String? {
        switch self {
            case .unstarted(let file): "FSEvents could not start a stream on \(file.path(percentEncoded: false))"
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

public func contentDigest(_ file: URL) throws -> SHA256Digest? {
    try ifPresent { try Data(contentsOf: file) }.get().map(SHA256.hash(data:))
}

public enum FileWatch {
    public static func events(of file: URL) -> AsyncStream<Result<Void, FileWatchFailure>> {
        AsyncStream { continuation in
            let watcher: Watcher = Watcher(continuation: continuation)
            watcher.enqueue { watcher in watcher.start(file) }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
    }

    @safe private actor Watcher {
        // --- [STATE]
        let continuation: AsyncStream<Result<Void, FileWatchFailure>>.Continuation
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: "RasmMacOS.FileWatch")
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
            let paths: Set<String> = [file.path(percentEncoded: false), file.resolvingSymlinksInPath().path(percentEncoded: false)]
            var context: FSEventStreamContext = unsafe FSEventStreamContext(version: 0, info: Unmanaged.passUnretained(self).toOpaque(), retain: nil, release: nil, copyDescription: nil)
            let flags: Int = kFSEventStreamCreateFlagFileEvents | kFSEventStreamCreateFlagWatchRoot | kFSEventStreamCreateFlagNoDefer
            func started(_ stream: FSEventStreamRef) -> Bool {
                unsafe FSEventStreamSetDispatchQueue(stream, queue)
                return unsafe FSEventStreamStart(stream)
            }
            unsafe stream = FSEventStreamCreate(nil, changed, &context, Array(paths) as CFArray, FSEventStreamEventId(kFSEventStreamEventIdSinceNow), 0, FSEventStreamCreateFlags(flags))
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
