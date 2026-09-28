import CryptoKit
import Dispatch
import Foundation
import System

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum FileWatch {
    // --- [VALUES]
    static func values<Value: Equatable & Sendable>(
        of file: URL,
        probe: @escaping @Sendable (URL) -> Value,
        debounce: Duration?,
    ) -> AsyncThrowingStream<Value, any Error> {
        AsyncThrowingStream { continuation in
            let watcher: Watcher<Value> = Watcher(
                file: file,
                probe: probe,
                debounce: debounce,
                continuation: continuation,
            )
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
            watcher.enqueue { watcher in watcher.start() }
        }
    }

    private actor Watcher<Value: Equatable & Sendable> {
        // --- [STATE]
        private let file: URL
        private let probe: @Sendable (URL) -> Value
        private let debounce: Duration?
        private let continuation: AsyncThrowingStream<Value, any Error>.Continuation
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: "app.rasm.relay.filewatch")
        private var directorySource: (any DispatchSourceFileSystemObject)?
        private var fileSource: (any DispatchSourceFileSystemObject)?
        private var pending: Task<Void, Never>?
        private var value: Value?
        private var stopped: Bool = false

        init(
            file: URL,
            probe: @escaping @Sendable (URL) -> Value,
            debounce: Duration?,
            continuation: AsyncThrowingStream<Value, any Error>.Continuation,
        ) {
            self.file = file
            self.probe = probe
            self.debounce = debounce
            self.continuation = continuation
        }

        nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

        // --- [LIFECYCLE]
        nonisolated func enqueue(_ work: @escaping @Sendable (isolated Watcher) -> Void) {
            queue.async { [self] in assumeIsolated(work) }
        }

        func start() {
            value = probe(file)
            let directory: Result<any DispatchSourceFileSystemObject, any Error> = Result {
                try source(path: file.deletingLastPathComponent().path, events: .write) { watcher in
                    watcher.openFileSource()
                    watcher.scheduleEmit()
                }
            }
            switch directory {
                case .success(let source):
                    directorySource = source
                    openFileSource()
                case .failure(let error): continuation.finish(throwing: error)
            }
        }

        func stop() {
            stopped = true
            pending?.cancel()
            directorySource?.cancel()
            fileSource?.cancel()
            directorySource = nil
            fileSource = nil
        }

        // --- [EVENTS]
        private func openFileSource() {
            guard !stopped, fileSource == nil else { return }
            let opened: Result<(any DispatchSourceFileSystemObject)?, any Error> = ifPresent {
                try source(path: file.path, events: [.write, .extend, .delete, .rename, .attrib]) { watcher in
                    if let source: any DispatchSourceFileSystemObject = watcher.fileSource,
                        !source.data.isDisjoint(with: [.delete, .rename])
                    {
                        source.cancel()
                        watcher.fileSource = nil
                    }
                    watcher.scheduleEmit()
                }
            }
            switch opened {
                case .success(let source): fileSource = source
                case .failure(let error): continuation.finish(throwing: error)
            }
        }

        private func source(
            path: String,
            events: DispatchSource.FileSystemEvent,
            handler: @escaping @Sendable (isolated Watcher) -> Void,
        ) throws -> any DispatchSourceFileSystemObject {
            let descriptor: FileDescriptor = try FileDescriptor.open(FilePath(path), .readOnly, options: .eventOnly)
            let source: any DispatchSourceFileSystemObject = DispatchSource.makeFileSystemObjectSource(
                fileDescriptor: descriptor.rawValue,
                eventMask: events,
                queue: queue,
            )
            source.setEventHandler { [weak self] in self?.assumeIsolated(handler) }
            source.setCancelHandler { try? descriptor.close() }
            source.activate()
            return source
        }

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
            guard !stopped else { return }
            let current: Value = probe(file)
            guard current != value else { return }
            value = current
            continuation.yield(current)
        }
    }

    // --- [PROBES]
    static func contentDigest(_ file: URL) -> SHA256Digest? {
        (try? Data(contentsOf: file)).map(SHA256.hash(data:))
    }

    static func exists(_ entry: URL) -> Bool {
        FileManager.default.fileExists(atPath: entry.path)
    }
}
