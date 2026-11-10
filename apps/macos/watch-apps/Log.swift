import Applications
import ArgumentParser
import CurrentProcess
import Encoding
import Files
import Foundation
import System

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Log: ParsableCommand {
    static let configuration: CommandConfiguration = .init(
        abstract: "Prints the last line per key of each run sharing this WATCH_APPS_SESSION. A process ID prints every record of that run",
        discussion: """
            Each run writes its records to one file named by its session and process ID in \(Journal.folder.path(percentEncoded: false)), under a folder WATCH_APPS_LOG_DIR replaces. \
            A run file past \(Int64(runLimit.bytes).formatted(.byteCount(style: .memory))) keeps the run's start and its last line per key, and the newest \(runLimit.files) run files stay
            """,
    )

    @Argument(help: "Process ID of one \(Program.current.name) run")
    var pid: pid_t?

    func run() throws {
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in try dateFormat.parse(decoder.singleValueContainer().decode(String.self)) }
        let pid = pid
        let printed = Result { () throws(ApplicationFailure) in
            do {
                let runs = try Journal.files().compactMap { file in try ifPresent { try Data(contentsOf: file) } }.map { text in
                    (text: text, records: try text.split(separator: UInt8(ascii: "\n")).map { line in try decoder.decode(Record<[String: JSONValue]>.self, from: line) })
                }
                return switch pid {
                    case let pid?: runs.last { run in run.records.first?.pid == pid }?.text ?? Data()
                    case nil:
                        try runs.filter { run in run.records.first?.session == callerSession }.reduce(into: Data()) { text, run in
                            guard var summary = run.records.first else { return }
                            let lines = run.records.dropFirst().reduce(into: [String: JSONValue]()) { merged, record in merged.merge(record.line ?? [:]) { _, next in next } }
                            (summary.line, summary.exit) = (run.records.count > 1 ? lines : nil, run.records.last?.exit)
                            text += try encoder.encodeLine(summary)
                        }
                }
            } catch {
                throw ApplicationFailure(subject: Journal.folder.path(percentEncoded: false), reason: .host(error as NSError))
            }
        }
        switch printed {
            case .success(let text): try FileDescriptor.standardOutput.writeAll(text)
            case .failure(let failure):
                try FileDescriptor.standardOutput.writeAll(encoder.encodeLine([Program.current.name: Event.fail(failure)]))
                WatchApps.exit(withError: ExitCode.failure)
        }
    }
}
