import CoreServices
import CryptoKit
import Dispatch
import Foundation
import System

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum FileWatchFailure: Error {
    case creation
    case registration
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum FileWatch {
    // --- [VALUES]
    static func values<Value: Equatable & Sendable>(
        of file: URL,
        read: @escaping @Sendable (URL) -> Value,
        debounce: Duration?,
    ) -> AsyncThrowingStream<Value, any Error> {
        AsyncThrowingStream { continuation in
            let watcher: Watcher<Value> = Watcher(
                file: file,
                read: read,
                debounce: debounce,
                continuation: continuation,
            )
            watcher.enqueue { watcher in watcher.start() }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
    }

    private final class EventHandler: Sendable {
        let receive: @Sendable ([FilePath]) -> Void

        init(receive: @escaping @Sendable ([FilePath]) -> Void) {
            self.receive = receive
        }

        static let callback: FSEventStreamCallback = { _, context, count, paths, _, _ in
            _ = unsafe context.map { context in
                let handler: EventHandler = unsafe Unmanaged<EventHandler>.fromOpaque(context).takeUnretainedValue()
                let paths: [FilePath] = unsafe UnsafeBufferPointer(
                    start: paths.assumingMemoryBound(to: UnsafePointer<CChar>.self),
                    count: count,
                ).map(unsafe FilePath.init(platformString:))
                handler.receive(paths)
            }
        }
    }

    private actor Watcher<Value: Equatable & Sendable> {
        // --- [STATE]
        private let file: URL
        private let read: @Sendable (URL) -> Value
        private let debounce: Duration?
        private let continuation: AsyncThrowingStream<Value, any Error>.Continuation
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: "app.rasm.relay.filewatch")
        private var stopStream: (() -> Void)?
        private var pending: Task<Void, Never>?
        private var value: Value?

        init(
            file: URL,
            read: @escaping @Sendable (URL) -> Value,
            debounce: Duration?,
            continuation: AsyncThrowingStream<Value, any Error>.Continuation,
        ) {
            self.file = file
            self.read = read
            self.debounce = debounce
            self.continuation = continuation
        }

        nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

        // --- [LIFECYCLE]
        nonisolated func enqueue(_ work: @escaping @Sendable (isolated Watcher) -> Void) {
            queue.async { [self] in assumeIsolated(work) }
        }

        func start() {
            let handler: EventHandler = EventHandler { [weak self] paths in
                self?.assumeIsolated { watcher in
                    let file: FilePath = FilePath(watcher.file.path)
                    let resolved: FilePath = FilePath(watcher.file.resolvingSymlinksInPath().path)
                    if paths.contains(where: { file.starts(with: $0) || resolved.starts(with: $0) }) {
                        watcher.scheduleEmit()
                    }
                }
            }
            var context: FSEventStreamContext = unsafe FSEventStreamContext(
                version: 0,
                info: Unmanaged.passUnretained(handler).toOpaque(),
                retain: { context in
                    unsafe context.map { context in
                        unsafe UnsafeRawPointer(Unmanaged<EventHandler>.fromOpaque(context).retain().toOpaque())
                    }
                },
                release: { context in
                    _ = unsafe context.map { context in unsafe Unmanaged<EventHandler>.fromOpaque(context).release() }
                },
                copyDescription: nil,
            )
            let created: FSEventStreamRef? = unsafe withExtendedLifetime(handler) {
                unsafe FSEventStreamCreate(
                    nil,
                    EventHandler.callback,
                    &context,
                    [NSOpenStepRootDirectory()] as CFArray,
                    FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
                    0,
                    FSEventStreamCreateFlags(kFSEventStreamCreateFlagFileEvents),
                )
            }
            guard let stream: FSEventStreamRef = unsafe created else {
                continuation.finish(throwing: FileWatchFailure.creation)
                return
            }
            unsafe FSEventStreamSetDispatchQueue(stream, queue)
            guard unsafe FSEventStreamStart(stream) else {
                unsafe FSEventStreamInvalidate(stream)
                unsafe FSEventStreamRelease(stream)
                continuation.finish(throwing: FileWatchFailure.registration)
                return
            }
            stopStream = {
                unsafe FSEventStreamStop(stream)
                unsafe FSEventStreamInvalidate(stream)
                unsafe FSEventStreamRelease(stream)
            }
            let current: Value = read(file)
            value = .some(current)
            continuation.yield(current)
        }

        func stop() {
            pending?.cancel()
            stopStream?()
            stopStream = nil
        }

        // --- [EVENTS]
        private func scheduleEmit() {
            guard let debounce else {
                emitIfChanged()
                return
            }
            pending?.cancel()
            pending = Task { [self] in
                if case .success = await Result(catching: { try await Task.sleep(for: debounce) }) { emitIfChanged() }
            }
        }

        private func emitIfChanged() {
            let current: Value = read(file)
            guard .some(current) != value else { return }
            value = .some(current)
            continuation.yield(current)
        }
    }

    // --- [READS]
    static func contentDigest(_ file: URL) -> SHA256Digest? {
        (try? Data(contentsOf: file)).map(SHA256.hash(data:))
    }

    static func exists(_ entry: URL) -> Bool {
        FileManager.default.fileExists(atPath: entry.path)
    }
}
