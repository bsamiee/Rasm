import AsyncAlgorithms
import CasePathsCore
import CoreServices
public import Foundation

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
    public static func newest(in folders: [LogFolder], since earliest: Date?, limit: Int) -> (lines: [Self], failures: [FileFailure]) {
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
                for (position, text) in lines.lazy.filter({ (line: Data) -> Bool in !line.isEmpty }).prefix(limit).enumerated() {
                    guard let decoded = String(validating: text, as: UTF8.self) else { throw FileFailure(file: file, reason: .filesystem(CocoaError(.fileReadInapplicableStringEncoding) as NSError)) }
                    let line = Self(file: file, text: decoded)
                    let at = line.timestamp ?? modified
                    if at >= floor { ranked.append((line, at, position)) }
                }
            } catch {
                failures.append(error)
            }
        }
        return (Array(ranked.sorted { first, second in (first.at, second.position) > (second.at, first.position) }.prefix(limit).map(\.line)), failures)
    }

    public static func follow(in folder: LogFolder) -> some AsyncSequence<(lines: [Self], failures: [FileFailure]), Never> & Sendable {
        let present = Result { () throws(FileFailure) -> [URL] in
            try ifPresent(folder.url) { try FileManager.default.contentsOfDirectory(at: folder.url, includingPropertiesForKeys: [.fileSizeKey]) } ?? []
        }
        let sizes = (present[case: \.success] ?? []).filter(folder.matches).reduce(into: [String: Result<Int, FileFailure>]()) { (sizes: inout [String: Result<Int, FileFailure>], file: URL) in
            sizes[file.lastPathComponent] = Result { () throws(FileFailure) -> Int in try ifPresent(file) { try file.resourceValues(forKeys: [.fileSizeKey]).fileSize }.flatMap(\.self) ?? 0 }
        }
        let initial: (read: [String: Int], lines: [Self], failures: [FileFailure]) = (
            read: sizes.compactMapValues { (size: Result<Int, FileFailure>) -> Int? in size[case: \.success] }, lines: [],
            failures: [present[case: \.failure]].compactMap(\.self) + sizes.values.compactMap { (size: Result<Int, FileFailure>) -> FileFailure? in size[case: \.failure] },
        )
        return FileEvent.events(of: folder.url).reductions(into: initial) { held, batch in
            (held.lines, held.failures) = ([], batch[case: \.success]?.isEmpty == false ? [] : held.failures)
            if case .failure(let failure) = batch { held.failures.append(failure) }
            let files: [URL] = (batch[case: \.success] ?? []).filter { (event: FileEvent) -> Bool in event.flags & FSEventStreamEventFlags(kFSEventStreamEventFlagItemIsFile) != 0 }.map(\.url)
            for file in Set(files) where folder.matches(file) {
                do throws(FileFailure) {
                    let name = file.lastPathComponent
                    guard let text = try ifPresent(file, { try Data(contentsOf: file, options: .mappedIfSafe) }) else {
                        held.read[name] = nil
                        continue
                    }
                    let fresh = text.dropFirst((held.read[name] ?? 0) > text.count ? 0 : held.read[name] ?? 0)
                    let whole = fresh.lastIndex(of: UInt8(ascii: "\n")).map { end in fresh[...end] } ?? fresh.prefix(0)
                    guard let lines = String(validating: whole, as: UTF8.self) else { throw FileFailure(file: file, reason: .filesystem(CocoaError(.fileReadInapplicableStringEncoding) as NSError)) }
                    held.read[name] = whole.endIndex
                    held.lines += lines.split(whereSeparator: \.isNewline).map { line in Self(file: file, text: String(line)) }
                } catch {
                    held.failures.append(error)
                }
            }
        }
        .map { held in (held.lines, held.failures) }
    }
}
