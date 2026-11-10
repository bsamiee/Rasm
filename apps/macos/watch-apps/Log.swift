import Applications
import ArgumentParser
import Encoding
import Files
import Foundation
import System

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Log: ParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Prints the last line per key of each run sharing this \(sessionVariable). A process ID prints every record of that run",
        discussion: """
            Runs append records to \(logFile.url.path(percentEncoded: false)), a path \(logVariable) replaces. \
            A file of \(logFile.limit.formatted(.byteCount(style: .memory))) or more rotates to .1 through .\(logFile.kept) beside it, the oldest replaced
            """,
    )

    @Argument(help: "Process ID of one \(program) run")
    var pid: pid_t?

    func run() throws {
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in try dateFormat.parse(decoder.singleValueContainer().decode(String.self)) }
        let (pid, session) = (pid, ProcessInfo.processInfo.environment[sessionVariable])
        let printed = Result { () throws(ApplicationFailure) in
            do {
                let texts = try logFile.files.compactMap { file in try ifPresent { try Data(contentsOf: file) } }
                let lines = try (texts.isEmpty ? [Data(contentsOf: logFile.url)] : texts).flatMap { text in text.split(separator: UInt8(ascii: "\n")) }
                let records = try lines.map { line in try decoder.decode(Record<[String: JSONValue]>.self, from: line) }
                if let pid {
                    let start = records.lastIndex { record in record.pid == pid && record.argv != nil } ?? records.endIndex
                    let end = records[start...].firstIndex { record in record.pid == pid && record.exit != nil }.map { end in end + 1 } ?? records.endIndex
                    return records.indices[start..<end].filter { index in records[index].pid == pid }.reduce(into: Data()) { text, index in text += lines[index] + [UInt8(ascii: "\n")] }
                }
                return try records.reduce(into: (runs: [Record<[String: JSONValue]>](), active: [pid_t: Int]())) { folded, record in
                    switch (record.argv, folded.active[record.pid]) {
                        case (.some, _) where record.session == session:
                            folded.active[record.pid] = folded.runs.endIndex
                            folded.runs.append(record)
                        case (.some, _): folded.active[record.pid] = nil
                        case (.none, .some(let index)):
                            folded.runs[index].line = (folded.runs[index].line ?? [:]).merging(record.line ?? [:]) { _, next in next }
                            folded.runs[index].exit = record.exit
                            folded.active[record.pid] = record.exit == nil ? index : nil
                        case (.none, .none): break
                    }
                }
                .runs
                .reduce(into: Data()) { text, run in text += try encoder.encodeLine(run) }
            } catch {
                throw ApplicationFailure(subject: logFile.url.path(percentEncoded: false), reason: .host(error as NSError))
            }
        }
        switch printed {
            case .success(let text): try FileDescriptor.standardOutput.writeAll(text)
            case .failure(let failure):
                try FileDescriptor.standardOutput.writeAll(encoder.encodeLine([program: Event.fail(failure)]))
                WatchApps.exit(withError: ExitCode.failure)
        }
    }
}
