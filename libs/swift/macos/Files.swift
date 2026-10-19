public import CryptoKit
import Dispatch
public import Foundation
import System

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
    // --- [EVENTS]
    public static func events(of file: URL) -> AsyncThrowingStream<Void, any Error> {
        AsyncThrowingStream { continuation in
            let watcher: Watcher = Watcher(file: file, continuation: continuation)
            watcher.enqueue { watcher in watcher.arm() }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stop() } }
        }
    }

    private actor Watcher {
        // --- [STATE]
        private let file: URL
        private let continuation: AsyncThrowingStream<Void, any Error>.Continuation
        private let queue: DispatchSerialQueue = DispatchSerialQueue(label: "RasmMacOS.FileWatch")
        private var sources: [any DispatchSourceFileSystemObject] = []

        init(file: URL, continuation: AsyncThrowingStream<Void, any Error>.Continuation) {
            self.file = file
            self.continuation = continuation
        }

        nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

        // --- [LIFECYCLE]
        nonisolated func enqueue(_ work: @escaping @Sendable (isolated Watcher) -> Void) {
            queue.async { [self] in assumeIsolated(work) }
        }

        func arm() {
            stop()
            let paths: Set<FilePath> = [FilePath(file.path(percentEncoded: false)), FilePath(file.resolvingSymlinksInPath().path(percentEncoded: false))]
            let armed: Result<Void, any Error> = Result {
                for path: FilePath in paths {
                    _ = try watch(path)
                    _ = try sequence(first: path.removingLastComponent()) { directory in directory.components.isEmpty ? nil : directory.removingLastComponent() }.first(where: watch)
                }
            }
            switch armed {
                case .success: continuation.yield(())
                case .failure(let error):
                    stop()
                    continuation.finish(throwing: error)
            }
            func watch(_ path: FilePath) throws -> Bool {
                guard let descriptor: FileDescriptor = try ifPresent({ try FileDescriptor.open(path, .readOnly, options: [.eventOnly, .closeOnExec]) }).get() else { return false }
                let source: any DispatchSourceFileSystemObject = DispatchSource.makeFileSystemObjectSource(fileDescriptor: descriptor.rawValue, eventMask: .all, queue: queue)
                source.setEventHandler { [weak self] in self?.assumeIsolated { watcher in watcher.arm() } }
                source.setCancelHandler { _ = try? descriptor.close() }
                source.activate()
                sources.append(source)
                return true
            }
        }

        func stop() {
            for source: any DispatchSourceFileSystemObject in sources { source.cancel() }
            sources = []
        }
    }

    // --- [READS]
    public static func contentDigest(_ file: URL) throws -> SHA256Digest? {
        try ifPresent { try Data(contentsOf: file) }.get().map(SHA256.hash(data:))
    }

    public static func exists(_ entry: URL) -> Bool {
        FileManager.default.fileExists(atPath: entry.path(percentEncoded: false))
    }
}
