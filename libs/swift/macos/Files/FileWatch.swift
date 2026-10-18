public import CryptoKit
import Dispatch
public import Foundation
import System

// --- [ERRORS] --------------------------------------------------------------------------

public enum FileWatchFailure: Error {
    case registration
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum FileWatch {
    // --- [VALUES]
    public static func values<Value: Equatable & Sendable>(
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

    private actor Watcher<Value: Equatable & Sendable> {
        // --- [STATE]
        private let file: URL
        private let read: @Sendable (URL) -> Value
        private let debounce: Duration?
        private let continuation: AsyncThrowingStream<Value, any Error>.Continuation
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: "\(Bundle.identity).filewatch")
        private var sources: [any DispatchSourceFileSystemObject] = []
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
            arm()
            guard !sources.isEmpty else {
                continuation.finish(throwing: FileWatchFailure.registration)
                return
            }
            emitIfChanged()
        }

        func stop() {
            pending?.cancel()
            for source: any DispatchSourceFileSystemObject in sources { source.cancel() }
            sources = []
        }

        // --- [SOURCES]
        private func arm() {
            for source: any DispatchSourceFileSystemObject in sources { source.cancel() }
            let paths: Set<FilePath> = [
                FilePath(file.path(percentEncoded: false)), FilePath(file.resolvingSymlinksInPath().path(percentEncoded: false)),
            ]
            sources = paths.flatMap { path in
                let ancestors: UnfoldFirstSequence<FilePath> = sequence(first: path.removingLastComponent()) { directory in
                    directory.components.isEmpty ? nil : directory.removingLastComponent()
                }
                return [source(path), ancestors.lazy.compactMap(source).first { _ in true }].compactMap(\.self)
            }
            func source(_ path: FilePath) -> any DispatchSourceFileSystemObject? {
                (try? FileDescriptor.open(path, .readOnly, options: [.eventOnly, .closeOnExec])).map { descriptor in
                    let source: any DispatchSourceFileSystemObject = DispatchSource.makeFileSystemObjectSource(
                        fileDescriptor: descriptor.rawValue,
                        eventMask: .all,
                        queue: queue,
                    )
                    source.setEventHandler { [weak self] in
                        self?.assumeIsolated { watcher in
                            watcher.arm()
                            guard let debounce: Duration = watcher.debounce else {
                                watcher.emitIfChanged()
                                return
                            }
                            watcher.pending?.cancel()
                            watcher.pending = Task {
                                if case .success = await Result(catching: { try await Task.sleep(for: debounce) }) { watcher.emitIfChanged() }
                            }
                        }
                    }
                    source.setCancelHandler { _ = try? descriptor.close() }
                    source.activate()
                    return source
                }
            }
        }

        // --- [EVENTS]
        private func emitIfChanged() {
            let current: Value = read(file)
            guard .some(current) != value else { return }
            value = .some(current)
            continuation.yield(current)
        }
    }

    // --- [READS]
    public static func contentDigest(_ file: URL) -> SHA256Digest? {
        (try? Data(contentsOf: file)).map(SHA256.hash(data:))
    }

    public static func exists(_ entry: URL) -> Bool {
        FileManager.default.fileExists(atPath: entry.path(percentEncoded: false))
    }
}
