import Applications
import ArgumentParser
import CurrentProcess
import Encoding
import Files
import Foundation
import System

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Log: ParsableCommand {
    static let configuration = CommandConfiguration(
        abstract: "Prints the last line per key of each run sharing this session. A process ID prints every record of that run",
        discussion: """
            Runs append their records to \(journal.path(percentEncoded: false)), one file per WATCH_APPS_SESSION setting, rotated at \
            \(Int64(RotatingFile(journal).size).formatted(.byteCount(style: .memory))) or \(RotatingFile(journal).age.formatted(.units(width: .wide))) to \
            \(RotatingFile(journal).files) numbered files beside it
            """,
    )

    @Argument(help: "Process ID of one \(Program.current.name) run")
    var pid: pid_t?

    func run() throws {
        let text: Data
        do {
            let entries = try RotatingFile(journal).urls.compactMap { file in try ifPresent { try Data(contentsOf: file) } }.flatMap(Record.journaled)
            let runs = entries.reduce(into: (runs: [[(text: Data, record: Record<[String: JSONValue]>)]](), open: [pid_t: Int]())) { held, entry in
                switch (entry.record.argv, held.open[entry.record.pid]) {
                    case (nil, let index?): held.runs[index].append(entry)
                    case (_, _):
                        held.open[entry.record.pid] = held.runs.count
                        held.runs.append([entry])
                }
            }
            .runs
            text =
                switch pid {
                    case let pid?: runs.last { run in run.first?.record.pid == pid }?.reduce(into: Data()) { text, entry in text += entry.text } ?? Data()
                    case nil:
                        try runs.reduce(into: Data()) { text, run in
                            guard var summary = run.first?.record else { return }
                            let lines = run.dropFirst().reduce(into: [String: JSONValue]()) { merged, entry in merged.merge(entry.record.line ?? [:]) { _, next in next } }
                            (summary.line, summary.exit) = (run.count > 1 ? lines : nil, run.last?.record.exit)
                            text += try JSONEncoder.lines.encodeLine(summary)
                        }
                }
        } catch {
            let failure = ApplicationFailure(subject: journal.path(percentEncoded: false), reason: .host(error as NSError))
            try FileDescriptor.standardOutput.writeAll(JSONEncoder.lines.encodeLine([Program.current.name: Event.fail(failure)]))
            WatchApps.exit(withError: ExitCode.failure)
        }
        try FileDescriptor.standardOutput.writeAll(text)
    }
}
