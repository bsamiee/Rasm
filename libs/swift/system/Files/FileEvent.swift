public import CoreServices
import Dispatch
public import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct FileEvent: Sendable {
    public let url: URL
    public let flags: FSEventStreamEventFlags

    public var mustScanSubDirs: Bool { flags & FSEventStreamEventFlags(kFSEventStreamEventFlagMustScanSubDirs) != 0 }

    public static func events(of file: URL) -> AsyncStream<Result<[Self], FileFailure>> {
        AsyncStream { continuation in
            let watcher = Watcher(continuation: continuation)
            watcher.enqueue { watcher in watcher.start(file) }
            continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stream = nil } }
        }
    }

    public static func presence(of file: URL) -> some AsyncSequence<Result<Bool, FileFailure>, Never> & Sendable {
        events(of: file).map { batch in
            batch.flatMap { _ in Result { () throws(FileFailure) in try ifPresent(file) { try FilePath(file.path(percentEncoded: false)).stat() } != nil } }
        }
    }

    public static func written<Value, Failure: Error>(
        _ files: [URL],
        during work: () async throws(Failure) -> Value,
    ) async throws(Failure) -> (value: Value, written: Result<[URL], FileFailure>) {
        let before = Result { () throws(FileFailure) in try files.map(modifications(of:)) }
        let value = try await work()
        let written = before.flatMap { before in
            Result { () throws(FileFailure) in try zip(files, before).filter { file, held throws(FileFailure) in try modifications(of: file) != held }.map(\.0) }
        }
        return (value, written)
    }

    private static func modifications(of file: URL) throws(FileFailure) -> [FilePath: Duration] {
        let root = FilePath(file.path(percentEncoded: false))
        return try ifPresent(file) {
            let inside = try file.resourceValues(forKeys: [.isPackageKey]).isPackage == true ? FileManager.default.subpathsOfDirectory(atPath: root.string) : []
            return try Dictionary(uniqueKeysWithValues: ([root] + inside.map { name in root.appending(name) }).map { path in try (path, Duration(path.stat().st_mtim)) })
        } ?? [:]
    }

    @safe private actor Watcher {
        // --- [STATE]

        private let continuation: AsyncStream<Result<[FileEvent], FileFailure>>.Continuation
        private let queue = DispatchSerialQueue(label: String(reflecting: FileEvent.self))
        var stream: Stream?

        init(continuation: AsyncStream<Result<[FileEvent], FileFailure>>.Continuation) {
            self.continuation = continuation
        }

        nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

        // --- [LIFECYCLE]

        nonisolated func enqueue(_ work: @escaping @Sendable (isolated Watcher) -> Void) {
            queue.async { [self] in assumeIsolated(work) }
        }

        func start(_ file: URL) {
            do throws(FileFailure) {
                stream = try Stream(watching: file, for: self)
                continuation.yield(.success([]))
            } catch {
                continuation.yield(.failure(error))
                continuation.finish()
            }
        }

        @safe struct Stream: ~Copyable {
            let handle: FSEventStreamRef

            init(watching file: URL, for watcher: Watcher) throws(FileFailure) {
                var context = unsafe FSEventStreamContext(version: 0, info: Unmanaged.passUnretained(watcher).toOpaque(), retain: nil, release: nil, copyDescription: nil)
                guard
                    let created = unsafe FSEventStreamCreate(
                        nil,
                        { _, info, count, eventPaths, eventFlags, _ in
                            let paths = unsafe UnsafeBufferPointer(start: eventPaths.assumingMemoryBound(to: UnsafeMutablePointer<CChar>.self), count: count)
                            let flags = unsafe UnsafeBufferPointer(start: eventFlags, count: count)
                            let events = unsafe zip(paths, flags).map { path, flag in FileEvent(url: URL(filePath: unsafe String(cString: path)), flags: flag) }
                            unsafe info.map { (info: UnsafeMutableRawPointer) -> Watcher in unsafe Unmanaged<Watcher>.fromOpaque(info).takeUnretainedValue() }?.continuation.yield(.success(events))
                        },
                        &context,
                        [file.path(percentEncoded: false)] as CFArray,
                        FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
                        0,
                        FSEventStreamCreateFlags(kFSEventStreamCreateFlagFileEvents | kFSEventStreamCreateFlagWatchRoot | kFSEventStreamCreateFlagNoDefer),
                    )
                else { throw FileFailure(file: file, reason: .unwatched) }
                unsafe FSEventStreamSetDispatchQueue(created, watcher.queue)
                guard unsafe FSEventStreamStart(created) else {
                    unsafe FSEventStreamInvalidate(created)
                    unsafe FSEventStreamRelease(created)
                    throw FileFailure(file: file, reason: .unwatched)
                }
                unsafe handle = created
            }

            deinit {
                unsafe FSEventStreamStop(handle)
                unsafe FSEventStreamInvalidate(handle)
                unsafe FSEventStreamRelease(handle)
            }
        }
    }
}
