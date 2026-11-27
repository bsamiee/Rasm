import AsyncAlgorithms
import CasePathsCore
package import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

package struct LogFolder: Codable, Hashable, Sendable {
    let url: URL
    let glob: String?

    package init(url: URL, glob: String? = nil) { (self.url, self.glob) = (url, glob) }

    func matches(_ file: URL) -> Bool { glob.map { glob in unsafe fnmatch(glob, file.lastPathComponent, 0) == 0 } ?? true }
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
            Result { () throws(FileFailure) in
                try ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: Array(keys)) } ?? []
            }
            .map { files in files.filter(folder.matches) }
        }
        var (ranked, failures): ([(line: Self, at: Date, position: Int)], [FileFailure]) = ([], listed.compactMap { listing in listing[case: \.failure] })
        for file in listed.flatMap({ listing in listing[case: \.success] ?? [] }) {
            do throws(FileFailure) {
                let values = try ifPresent(file) { try file.resourceValues(forKeys: keys) }
                guard let values, values.isRegularFile == true, let modified = values.contentModificationDate, modified >= floor,
                    let data = try ifPresent(file, { try Data(contentsOf: file, options: .alwaysMapped) })
                else { continue }
                let lines = sequence(state: Optional(data.endIndex)) { (end: inout Data.Index?) -> Data? in
                    guard let last = end else { return nil }
                    end = data[..<last].lastIndex(of: UInt8(ascii: "\n"))
                    return data[(end.map { newline in newline + 1 } ?? data.startIndex)..<last]
                }
                var kept = 0
                for (position, bytes) in lines.lazy.filter({ (line: Data) -> Bool in !line.isEmpty }).enumerated() {
                    let line = Self(file: file, bytes: bytes)
                    let at = line.timestamp ?? modified
                    guard kept < limit, at >= floor else { break }
                    guard matching(line) else { continue }
                    ranked.append((line, at, position))
                    kept += 1
                }
            } catch {
                failures.append(error)
            }
        }
        return (Array(ranked.sorted { first, second in (first.at, second.position) > (second.at, first.position) }.prefix(limit).map(\.line)), failures)
    }

    static func follow(in folder: LogFolder) -> some AsyncSequence<(lines: [Self], failures: [FileFailure]), Never> & Sendable {
        let keys: Set<URLResourceKey> = [.fileIdentifierKey, .fileSizeKey]
        let baseline = Result { () throws(FileFailure) in
            let files = try ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: Array(keys)) } ?? []
            return try files.filter(folder.matches).map { file throws(FileFailure) in try ifPresent(file) { try file.resourceValues(forKeys: keys) } }
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
        let found = FileEvent.events(of: folder.url).reductions(into: initial) { held, batch in
            let listed: Result<[URL], FileFailure> =
                switch batch {
                    case .success(let events) where events.isEmpty || events.contains(where: \.mustScanSubDirs):
                        Result { () throws(FileFailure) in try ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: nil) } ?? [] }
                    case .success(let events): .success(events.map(\.url))
                    case .failure(let failure): .failure(failure)
                }
            var failures = [listed[case: \.failure]].compactMap(\.self)
            held.found = []
            for file in Set((listed[case: \.success] ?? []).map(\.lastPathComponent)).map({ name in folder.url.appending(component: name) }) where folder.matches(file) {
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
        let events = FileEvent.events(of: file, eventMask: [.write, .extend, .delete]).prefix { event in event[case: \.success]?.contains(.delete) != true }
        let initial: (offset: Int64, lines: [Self], failures: [FileFailure]) = (offset, [], [])
        return chain(events, [Result<DispatchSource.FileSystemEvent, FileFailure>.success(.delete)].async)
            .reductions(into: initial) { held, event in
                let descriptor = FileDescriptor(rawValue: handle.fileDescriptor)
                do {
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
                    (held.offset, held.failures) = (start + Int64(whole.count), [event[case: \.failure]].compactMap(\.self))
                    held.lines = whole.split(separator: UInt8(ascii: "\n")).map { line in Self(file: file, bytes: line) }
                } catch {
                    (held.lines, held.failures) = ([], [FileFailure(file: file, reason: .filesystem(error))])
                }
            }
            .map { held in (held.lines, held.failures) }
    }
}
