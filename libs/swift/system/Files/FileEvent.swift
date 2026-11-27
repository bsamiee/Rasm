package import CoreServices
package import Dispatch
package import Foundation
package import System

// --- [MODELS] --------------------------------------------------------------------------

package struct FileEvent: Sendable {
    package struct Flags: OptionSet, Sendable {
        package static let mustScanSubDirs = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagMustScanSubDirs))
        package static let rootChanged = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagRootChanged))
        package static let itemCreated = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagItemCreated))
        package static let itemRemoved = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagItemRemoved))
        package static let itemRenamed = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagItemRenamed))
        package static let itemModified = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagItemModified))
        package static let itemIsFile = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagItemIsFile))
        package static let itemIsDir = Self(rawValue: FSEventStreamEventFlags(kFSEventStreamEventFlagItemIsDir))

        package let rawValue: FSEventStreamEventFlags

        package init(rawValue: FSEventStreamEventFlags) {
            self.rawValue = rawValue
        }
    }

    package let url: URL
    package let flags: Flags
    package let id: FSEventStreamEventId
    package let inode: Inode?

    package var mustScanSubDirs: Bool { !flags.isDisjoint(with: [.mustScanSubDirs, .rootChanged]) }

    package static func events(of files: [URL]) -> AsyncStream<Result<[Self], FileFailure>> {
        let (events, continuation) = AsyncStream<Result<[Self], FileFailure>>.makeStream()
        let watcher = Watcher(continuation: continuation)
        watcher.enqueue { watcher in watcher.start(files) }
        continuation.onTermination = { _ in watcher.enqueue { watcher in watcher.stream = nil } }
        return events
    }

    package static func events(of handle: FileHandle, eventMask: DispatchSource.FileSystemEvent) -> AsyncStream<DispatchSource.FileSystemEvent> {
        let (events, continuation) = AsyncStream<DispatchSource.FileSystemEvent>.makeStream()
        let source = DispatchSource.makeFileSystemObjectSource(fileDescriptor: handle.fileDescriptor, eventMask: eventMask)
        source.setRegistrationHandler { continuation.yield([]) }
        source.setEventHandler { continuation.yield(source.data) }
        source.setCancelHandler { withExtendedLifetime(handle) {} }
        continuation.onTermination = { _ in source.cancel() }
        source.activate()
        return events
    }

    package static func presence(of file: URL) -> some AsyncSequence<Result<Bool, FileFailure>, Never> & Sendable {
        events(of: [file]).map { batch in
            batch.flatMap { _ in Result { () throws(FileFailure) in try ifPresent(file) { try FilePath(file.path(percentEncoded: false)).stat(followTargetSymlink: false) } != nil } }
        }
    }

    package static func written<Value, Failure: Error>(
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

    private static func modifications(of document: URL) throws(FileFailure) -> [URL: Date?] {
        let keys: Set<URLResourceKey> = [.contentModificationDateKey]
        var document = document
        document.removeAllCachedResourceValues()
        return try ifPresent(document) { () throws -> [URL: Date?] in
            let root = try document.resourceValues(forKeys: keys.union([.isPackageKey]))
            var failure: (any Error)?
            let enumerator =
                root.isPackage == true
                ? FileManager.default.enumerator(
                    at: document,
                    includingPropertiesForKeys: Array(keys),
                    options: .producesRelativePathURLs,
                    errorHandler: { (_: URL, error: any Error) -> Bool in
                        failure = error
                        return false
                    },
                )
                : nil
            let entries: [URL] = enumerator?.compactMap { (entry: Any) -> URL? in entry as? URL } ?? []
            let inside = try entries.map { (url: URL) throws -> (URL, Date?) in (url, try url.resourceValues(forKeys: keys).contentModificationDate) }
            if let failure { throw failure }
            return Dictionary(uniqueKeysWithValues: [(document, root.contentModificationDate)] + inside)
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

        func start(_ files: [URL]) {
            guard !files.isEmpty else {
                continuation.finish()
                return
            }
            stream = Stream(watching: files, for: self)
            guard stream == nil else {
                continuation.yield(.success([]))
                return
            }
            for file in files { continuation.yield(.failure(FileFailure(file: file, reason: .unwatched))) }
            continuation.finish()
        }

        @safe struct Stream: ~Copyable {
            let handle: FSEventStreamRef

            init?(watching files: [URL], for watcher: Watcher) {
                var context = unsafe FSEventStreamContext(version: 0, info: Unmanaged.passUnretained(watcher).toOpaque(), retain: nil, release: nil, copyDescription: nil)
                guard
                    let created = unsafe FSEventStreamCreate(
                        nil,
                        { _, info, count, eventPaths, eventFlags, eventIds in
                            let entries = unsafe Unmanaged<AnyObject>.fromOpaque(eventPaths).takeUnretainedValue() as? [[String: Any]] ?? []
                            let flags = unsafe UnsafeBufferPointer(start: eventFlags, count: count)
                            let ids = unsafe UnsafeBufferPointer(start: eventIds, count: count)
                            let events = entries.indices.compactMap { (index: Int) -> FileEvent? in
                                let inode = (entries[index][kFSEventStreamEventExtendedFileIDKey] as? NSNumber).map { (number: NSNumber) -> Inode in Inode(number.uint64Value) }
                                return (entries[index][kFSEventStreamEventExtendedDataPathKey] as? String).map { (path: String) -> FileEvent in
                                    unsafe FileEvent(url: URL(filePath: path), flags: Flags(rawValue: flags[index]), id: ids[index], inode: inode)
                                }
                            }
                            unsafe info.map { (info: UnsafeMutableRawPointer) -> Watcher in unsafe Unmanaged<Watcher>.fromOpaque(info).takeUnretainedValue() }?.continuation.yield(.success(events))
                        },
                        &context,
                        files.map { (file: URL) -> String in file.path(percentEncoded: false) } as CFArray,
                        FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
                        0,
                        FSEventStreamCreateFlags(
                            kFSEventStreamCreateFlagUseCFTypes | kFSEventStreamCreateFlagUseExtendedData | kFSEventStreamCreateFlagFileEvents | kFSEventStreamCreateFlagWatchRoot
                        ),
                    )
                else { return nil }
                unsafe FSEventStreamSetDispatchQueue(created, watcher.queue)
                guard unsafe FSEventStreamStart(created) else {
                    unsafe FSEventStreamInvalidate(created)
                    unsafe FSEventStreamRelease(created)
                    return nil
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
