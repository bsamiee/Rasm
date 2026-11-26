import AsyncAlgorithms
import CasePathsCore
public import Foundation
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct LogFolder: Codable, Hashable, Sendable {
    public let url: URL
    public let glob: String?

    public init(url: URL, glob: String? = nil) { (self.url, self.glob) = (url, glob) }

    func matches(_ file: URL) -> Bool { glob.map { glob in unsafe fnmatch(glob, file.lastPathComponent, 0) == 0 } ?? true }
}

public struct LogLine: Encodable, Sendable {
    private struct Record: Decodable {
        let timestamp: String
    }

    public let file: URL
    public let text: String

    public var timestamp: Date? {
        Result { try JSONDecoder().decode(Record.self, from: Data(text.utf8)) }[case: \.success].flatMap { record in
            Result { try Date.ISO8601FormatStyle(includingFractionalSeconds: true).parse(record.timestamp) }[case: \.success]
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LogLine {
    public static func newest(in folders: [LogFolder], since earliest: Date?, limit: Int, where matching: (Self) -> Bool) -> (lines: [Self], failures: [FileFailure]) {
        let floor = earliest ?? .distantPast
        let listed = folders.map { folder in
            Result { () throws(FileFailure) in
                try ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: [.contentModificationDateKey, .isRegularFileKey]) } ?? []
            }
            .map { files in files.filter(folder.matches) }
        }
        var (ranked, failures): ([(line: Self, at: Date, position: Int)], [FileFailure]) = ([], listed.compactMap { listing in listing[case: \.failure] })
        for file in listed.flatMap({ listing in listing[case: \.success] ?? [] }) {
            do throws(FileFailure) {
                let values = try ifPresent(file) { try file.resourceValues(forKeys: [.contentModificationDateKey, .isRegularFileKey]) }
                guard let values, values.isRegularFile == true, let modified = values.contentModificationDate, modified >= floor,
                    let data = try ifPresent(file, { try Data(contentsOf: file, options: .alwaysMapped) })
                else { continue }
                let lines = sequence(state: data.endIndex) { (end: inout Data.Index) -> Data.SubSequence? in
                    guard end > data.startIndex else { return nil }
                    let body = data[..<end].last == UInt8(ascii: "\n") ? data[..<(end - 1)] : data[..<end]
                    end = body.lastIndex(of: UInt8(ascii: "\n")).map { newline in newline + 1 } ?? body.startIndex
                    return body[end...]
                }
                var kept = 0
                for (position, text) in lines.lazy.filter({ (line: Data) -> Bool in !line.isEmpty }).enumerated() {
                    guard kept < limit else { break }
                    guard let decoded = String(validating: text, as: UTF8.self) else { throw FileFailure(file: file, reason: .filesystem(CocoaError(.fileReadInapplicableStringEncoding) as NSError)) }
                    let line = Self(file: file, text: decoded)
                    let at = line.timestamp ?? modified
                    guard at >= floor else { break }
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

    public static func follow(in folder: LogFolder) -> some AsyncSequence<(lines: [Self], failures: [FileFailure]), Never> & Sendable {
        let initial: (read: [Inode: Int64]?, lines: [Self], failures: [FileFailure]) = (nil, [], [])
        return FileEvent.events(of: folder.url).reductions(into: initial) { held, batch in
            let (prior, events): ([Inode: Int64]?, [FileEvent]) = (held.read, batch[case: \.success] ?? [])
            let listed = Result { () throws(FileFailure) -> [URL] in
                guard prior == nil || events.contains(where: \.mustScanSubDirs) else { return events.map(\.url) }
                return try ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: nil) } ?? []
            }
            (held.read, held.lines, held.failures) = (prior ?? [:], [], [batch[case: \.failure], listed[case: \.failure]].compactMap(\.self))
            for file in Set(listed[case: \.success] ?? events.map(\.url)) where folder.matches(file) {
                do {
                    let path = FilePath(file.path(percentEncoded: false))
                    guard let descriptor = try ifPresent({ try FileDescriptor.open(path, .readOnly, options: [.nonBlocking, .closeOnExec]) }) else { continue }
                    let handle = FileHandle(fileDescriptor: descriptor.rawValue, closeOnDealloc: true)
                    let stat = try descriptor.stat()
                    guard stat.type == .regular else { continue }
                    let from = prior.map { (read: [Inode: Int64]) -> Int64 in read[stat.inode].flatMap { offset in offset <= stat.size ? offset : nil } ?? 0 } ?? stat.size
                    try descriptor.seek(offset: from, from: .start)
                    let fresh = try handle.readToEnd() ?? Data()
                    let whole = fresh.lastIndex(of: UInt8(ascii: "\n")).map { end in fresh[...end] } ?? fresh.prefix(0)
                    guard let lines = String(validating: whole, as: UTF8.self) else { throw CocoaError(.fileReadInapplicableStringEncoding) }
                    held.read?[stat.inode] = from + Int64(whole.count)
                    held.lines += lines.split(whereSeparator: \.isNewline).map { line in Self(file: file, text: String(line)) }
                } catch {
                    held.failures.append(FileFailure(file: file, reason: .filesystem(error as NSError)))
                }
            }
        }
        .map { held in (held.lines, held.failures) }
    }
}
