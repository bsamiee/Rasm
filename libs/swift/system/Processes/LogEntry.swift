import DequeModule
package import Foundation
package import OSLog
import Subprocess

// --- [CONSTANTS] -----------------------------------------------------------------------

private let logTypes: [String: OSLogEntryLog.Level] = ["Debug": .debug, "Info": .info, "Default": .notice, "Error": .error, "Fault": .fault]
private let logTypeNames: [OSLogEntryLog.Level: String] = Dictionary(uniqueKeysWithValues: logTypes.map { name, level in (level, name) })
private let timestampFormat = Date.ISO8601FormatStyle(dateTimeSeparator: .space, includingFractionalSeconds: true)
private let decoder = JSONDecoder()

// --- [MODELS] --------------------------------------------------------------------------

package struct LogEntry: Codable, Sendable {
    package enum Source: Sendable {
        case process(pid_t)
        case executable(URL)
    }

    fileprivate enum CodingKeys: String, CodingKey {
        case date = "timestamp"
        case level = "messageType"
        case processIdentifier = "processID"
        case processImagePath
        case senderImagePath
        case subsystem
        case category
        case composedMessage = "eventMessage"
    }

    let date: Date
    let level: OSLogEntryLog.Level
    let processIdentifier: pid_t
    let processImagePath: String
    let senderImagePath: String
    let subsystem: String
    let category: String
    let composedMessage: String

    package init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        let logType = try fields.decode(String.self, forKey: .level)
        guard let level = logTypes[logType] else { throw DecodingError.dataCorruptedError(forKey: .level, in: fields, debugDescription: "unknown log type \(logType)") }
        self.level = level
        date = try Date(fields.decode(String.self, forKey: .date), strategy: timestampFormat)
        processIdentifier = try fields.decode(pid_t.self, forKey: .processIdentifier)
        processImagePath = try fields.decode(String.self, forKey: .processImagePath)
        senderImagePath = try fields.decode(String.self, forKey: .senderImagePath)
        subsystem = try fields.decode(String.self, forKey: .subsystem)
        category = try fields.decode(String.self, forKey: .category)
        composedMessage = try fields.decode(String.self, forKey: .composedMessage)
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(date, forKey: .date)
        try fields.encode(logTypeNames[level], forKey: .level)
        try fields.encode(processIdentifier, forKey: .processIdentifier)
        try fields.encode(processImagePath, forKey: .processImagePath)
        try fields.encode(senderImagePath, forKey: .senderImagePath)
        try fields.encode(subsystem, forKey: .subsystem)
        try fields.encode(category, forKey: .category)
        try fields.encode(composedMessage, forKey: .composedMessage)
    }
}

private struct Line: Decodable {
    let entry: LogEntry?

    init(from decoder: any Decoder) throws {
        entry = try decoder.container(keyedBy: LogEntry.CodingKeys.self).contains(.date) ? LogEntry(from: decoder) : nil
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LogEntry {
    package static func history(of source: Source, since start: Date?, limit: Int, containing text: String? = nil) async throws(ProcessFailure) -> [Self] {
        var held = Deque<String>()
        let window = start.map { start in ["--start", "@\(Int(start.timeIntervalSince1970))"] } ?? ["--last", "boot"]
        try await read(["show"] + window, of: source, since: start, levels: nil, containing: text) { line in
            held.append(line)
            if held.count > limit + 1 { held.removeFirst() }
        }
        return try held.compactMap(entry(in:)).map { decoded throws(ProcessFailure) in try decoded.get() }.suffix(limit).reversed()
    }

    package static func stream(of source: Source, levels: [OSLogEntryLog.Level]? = nil) -> AsyncStream<Result<Self, ProcessFailure>> {
        let (entries, continuation) = AsyncStream<Result<Self, ProcessFailure>>.makeStream()
        let following = Task {
            do throws(ProcessFailure) {
                try await read(["stream"], of: source, since: nil, levels: levels, containing: nil) { line in if let entry = entry(in: line) { continuation.yield(entry) } }
            } catch {
                continuation.yield(.failure(error))
            }
            continuation.finish()
        }
        continuation.onTermination = { _ in following.cancel() }
        return entries
    }

    private static func entry(in line: String) -> Result<Self, ProcessFailure>? {
        do {
            return try decoder.decode(Line.self, from: Data(line.utf8)).entry.map(Result.success)
        } catch {
            return .failure(ProcessFailure(subject: line, error))
        }
    }

    private static func read(
        _ command: [String],
        of source: Source,
        since start: Date?,
        levels: [OSLogEntryLog.Level]?,
        containing text: String?,
        into sink: (String) throws(ProcessFailure) -> Void,
    ) async throws(ProcessFailure) {
        let process: NSPredicate =
            switch source {
                case .process(let pid): NSPredicate(format: "processIdentifier == %d", pid)
                case .executable(let url): NSPredicate(format: "processImagePath == %@", url.path(percentEncoded: false))
            }
        let clauses: [NSPredicate?] = [
            NSPredicate(format: "type == logEvent"),
            process,
            start.map { start in NSPredicate(format: "date >= %@", argumentArray: [start]) },
            levels.map { levels in NSCompoundPredicate(orPredicateWithSubpredicates: levels.compactMap { level in logTypeNames[level] }.map { name in NSPredicate(format: "logType == %@", name) }) },
            text.map { text in NSPredicate(format: "composedMessage CONTAINS[c] %@", text) },
        ]
        let predicate = NSCompoundPredicate(andPredicateWithSubpredicates: clauses.compactMap(\.self))
        let log = Configuration(executable: .name("log"), arguments: Arguments(command + ["--predicate", predicate.predicateFormat, "--style", "ndjson"]))
        try await log.run(input: .none, output: .sequence) { execution in
            for try await line in execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])) where line.hasPrefix("{") { try sink(line) }
        }
    }
}
