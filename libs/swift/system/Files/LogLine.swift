import AsyncAlgorithms
import CasePathsCore
package import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

package struct LogFolder: Codable, Hashable, Sendable {
    let url: URL
    let glob: String?

    package init(url: URL, glob: String? = nil) { (self.url, self.glob) = (url, glob) }

    func match(_ files: [URL]) throws(FileFailure) -> [URL] {
        guard let glob else { return files }
        let folded = try ifPresent(url) { try url.resourceValues(forKeys: [.volumeSupportsCaseSensitiveNamesKey]).volumeSupportsCaseSensitiveNames } == false
        return files.filter { file in unsafe fnmatch(glob, file.lastPathComponent, folded ? FNM_CASEFOLD : 0) == 0 }
    }
}

package struct LogLine: Encodable, Sendable {
    private struct Record: Decodable {
        let timestamp: Date
    }

    private static let decoder: JSONDecoder = {
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        return decoder
    }()

    let file: URL
    package let text: String

    init(file: URL, bytes: some Collection<UInt8>) { (self.file, text) = (file, String(decoding: bytes, as: UTF8.self)) }

    var timestamp: Date? { Result { try Self.decoder.decode(Record.self, from: Data(text.utf8)) }[case: \.success]?.timestamp }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LogLine {
    package static func newest(in folders: [LogFolder], since earliest: Date?, limit: Int, where matching: (Self) -> Bool) -> (lines: [Self], failures: [FileFailure]) {
        let floor = earliest ?? .distantPast
        let keys: Set<URLResourceKey> = [.contentModificationDateKey, .isRegularFileKey]
        let listed = folders.map { folder in
            Result { () throws(FileFailure) in try folder.match(ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: Array(keys)) } ?? []) }
        }
        var (ranked, failures): ([(line: Self, at: Date)], [FileFailure]) = ([], listed.compactMap { listing in listing[case: \.failure] })
        for file in listed.flatMap({ listing in listing[case: \.success] ?? [] }) {
            do {
                let values = try ifPresent { try file.resourceValues(forKeys: keys) }
                guard let values, values.isRegularFile == true, let modified = values.contentModificationDate, modified >= floor,
                    let descriptor = try ifPresent({ try FileDescriptor.open(FilePath(file.path(percentEncoded: false)), .readOnly, options: .closeOnExec) })
                else { continue }
                let handle = FileHandle(fileDescriptor: descriptor.rawValue, closeOnDealloc: true)
                let stat = try FileDescriptor(rawValue: handle.fileDescriptor).stat()
                var (end, carried, kept, older) = (stat.size, [UInt8](), 0, false)
                while end > 0, kept < limit, !older {
                    let start = max(end - Int64(stat.preferredIOBlockSize), 0)
                    let read = try unsafe [UInt8](unsafeUninitializedCapacity: Int(end - start)) { buffer, count in
                        count = try unsafe FileDescriptor(rawValue: handle.fileDescriptor).read(fromAbsoluteOffset: start, into: UnsafeMutableRawBufferPointer(buffer))
                    }
                    let pieces = (read + carried).split(separator: UInt8(ascii: "\n"), omittingEmptySubsequences: false)
                    let lines = pieces.dropFirst(start > 0 ? 1 : 0).reversed().filter { (piece: ArraySlice<UInt8>) -> Bool in !piece.isEmpty }
                        .map { (piece: ArraySlice<UInt8>) -> Self in Self(file: file, bytes: piece) }
                        .map { (line: Self) -> (line: Self, at: Date) in (line, line.timestamp ?? modified) }
                    let current = lines.prefix { line in line.at >= floor }
                    let found = current.filter { line in matching(line.line) }.prefix(limit - kept)
                    (end, carried, kept, older) = (start, start > 0 ? Array(pieces[0]) : [], kept + found.count, current.count < lines.count)
                    ranked += found
                }
            } catch {
                failures.append(FileFailure(file: file, reason: .filesystem(error)))
            }
        }
        return (ranked.sorted { first, second in first.at > second.at }.prefix(limit).map(\.line), failures)
    }

    static func follow(in folder: LogFolder) -> some AsyncSequence<(lines: [Self], failures: [FileFailure]), Never> & Sendable {
        let keys: Set<URLResourceKey> = [.fileIdentifierKey, .fileSizeKey]
        let baseline = Result { () throws(FileFailure) in
            let files = try folder.match(ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: Array(keys)) } ?? [])
            return try files.map { file throws(FileFailure) in try ifPresent(file) { try file.resourceValues(forKeys: keys) } }
        }
        let sizes = Dictionary(
            (baseline[case: \.success] ?? []).compactMap { values -> (Inode, Int64)? in
                guard let inode = values?.fileIdentifier, let size = values?.fileSize else { return nil }
                return (Inode(inode), Int64(size))
            },
            uniquingKeysWith: max,
        )
        let (batches, delivery) = AsyncStream<(lines: [Self], failures: [FileFailure])>.makeStream()
        let initial: (followed: Set<Inode>, found: [(file: URL, handle: FileHandle, from: Int64)]) = ([], [])
        let found = FileEvent.events(of: [folder.url]).reductions(into: initial) { held, batch in
            let listed = Result { () throws(FileFailure) -> [URL] in
                let events = try batch.get()
                guard !events.isEmpty, !events.contains(where: \.mustScanSubDirs) else {
                    return try folder.match(ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: nil) } ?? [])
                }
                let root = try ifPresent(folder.url) { try FilePath(folder.url.canonical().path(percentEncoded: false)) }
                let names = Set(
                    events.compactMap { (event: FileEvent) -> FilePath.Component? in
                        let path = FilePath(event.url.path(percentEncoded: false))
                        return path.removingLastComponent() == root && event.inode.map { inode in held.followed.contains(inode) } != true ? path.lastComponent : nil
                    }
                )
                return try folder.match(names.map { name in folder.url.appending(component: name.string) })
            }
            var failures = [listed[case: \.failure]].compactMap(\.self)
            held.found = []
            for file in listed[case: \.success] ?? [] {
                do throws(FileFailure) {
                    guard let descriptor = try ifPresent(file, { try FileDescriptor.open(FilePath(file.path(percentEncoded: false)), .readOnly, options: [.nonBlocking, .closeOnExec]) }) else {
                        continue
                    }
                    let handle = FileHandle(fileDescriptor: descriptor.rawValue, closeOnDealloc: true)
                    guard let stat = try ifPresent(file, { try descriptor.stat() }), stat.type == .regular, held.followed.insert(stat.inode).inserted else { continue }
                    held.found.append((file, handle, sizes[stat.inode] ?? 0))
                } catch {
                    failures.append(error)
                }
            }
            if !failures.isEmpty { delivery.yield(([], failures)) }
        }
        let following = Task {
            await withDiscardingTaskGroup { group in
                for await (file, handle, from) in found.flatMap(\.found.async) {
                    group.addTask { for await batch in follow(file, through: handle, from: from) where !batch.lines.isEmpty || !batch.failures.isEmpty { delivery.yield(batch) } }
                }
            }
            delivery.finish()
        }
        delivery.onTermination = { _ in following.cancel() }
        delivery.yield(([], [baseline[case: \.failure]].compactMap(\.self)))
        return batches
    }

    static func follow(_ file: URL, through handle: FileHandle, from offset: Int64) -> some AsyncSequence<(lines: [Self], failures: [FileFailure]), Never> {
        let events = FileEvent.events(of: handle, eventMask: [.write, .extend, .rename, .delete]).prefix { event in !event.contains(.delete) }
        let initial: (file: URL, offset: Int64, lines: [Self], failures: [FileFailure]) = (file, offset, [], [])
        return chain(events, [DispatchSource.FileSystemEvent.delete].async)
            .reductions(into: initial) { held, event in
                let descriptor = FileDescriptor(rawValue: handle.fileDescriptor)
                do {
                    if event.contains(.rename) {
                        var path = [UInt8](repeating: 0, count: Int(MAXPATHLEN))
                        guard unsafe fcntl(descriptor.rawValue, F_GETPATH, &path) != -1 else { throw Errno(rawValue: errno) }
                        held.file = URL(filePath: String(decoding: path.prefix { byte in byte != 0 }, as: UTF8.self))
                    }
                    let stat = try descriptor.stat()
                    let start = held.offset <= stat.size ? held.offset : 0
                    let fresh: [UInt8] =
                        if stat.size > start {
                            try unsafe [UInt8](unsafeUninitializedCapacity: Int(stat.size - start)) { buffer, read in
                                read = try unsafe descriptor.read(fromAbsoluteOffset: start, into: UnsafeMutableRawBufferPointer(buffer))
                            }
                        } else {
                            []
                        }
                    let whole = fresh[..<(fresh.lastIndex(of: UInt8(ascii: "\n")).map { end in end + 1 } ?? 0)]
                    let current = held.file
                    (held.offset, held.lines, held.failures) = (start + Int64(whole.count), whole.split(separator: UInt8(ascii: "\n")).map { line in Self(file: current, bytes: line) }, [])
                } catch {
                    (held.lines, held.failures) = ([], [FileFailure(file: held.file, reason: .filesystem(error))])
                }
            }
            .map { held in (held.lines, held.failures) }
    }
}
