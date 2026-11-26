import DequeModule
public import Foundation
public import OSLog
import Subprocess

// --- [CONSTANTS] -----------------------------------------------------------------------

private let logTypes: [String: OSLogEntryLog.Level] = ["Debug": .debug, "Info": .info, "Default": .notice, "Error": .error, "Fault": .fault]
private let logTypeNames: [OSLogEntryLog.Level: String] = Dictionary(uniqueKeysWithValues: logTypes.map { name, level in (level, name) })
private let timestampFormat = Date.ISO8601FormatStyle(dateTimeSeparator: .space, includingFractionalSeconds: true)

// --- [MODELS] --------------------------------------------------------------------------

public struct LogEntry: Codable, Sendable {
    public enum Source: Sendable {
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

    public let date: Date
    public let level: OSLogEntryLog.Level?
    public let processIdentifier: pid_t?
    public let processImagePath: String?
    public let senderImagePath: String?
    public let subsystem: String?
    public let category: String?
    public let composedMessage: String?

    public init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        date = try Date(fields.decode(String.self, forKey: .date), strategy: timestampFormat)
        level = try fields.decodeIfPresent(String.self, forKey: .level).map { logType throws(DecodingError) in
            guard let level = logTypes[logType] else { throw .dataCorruptedError(forKey: .level, in: fields, debugDescription: "unknown log type \(logType)") }
            return level
        }
        processIdentifier = try fields.decodeIfPresent(pid_t.self, forKey: .processIdentifier)
        processImagePath = try fields.decodeIfPresent(String.self, forKey: .processImagePath)
        senderImagePath = try fields.decodeIfPresent(String.self, forKey: .senderImagePath)
        subsystem = try fields.decodeIfPresent(String.self, forKey: .subsystem)
        category = try fields.decodeIfPresent(String.self, forKey: .category)
        composedMessage = try fields.decodeIfPresent(String.self, forKey: .composedMessage)
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(date, forKey: .date)
        try fields.encodeIfPresent(level.flatMap { level in logTypeNames[level] }, forKey: .level)
        try fields.encodeIfPresent(processIdentifier, forKey: .processIdentifier)
        try fields.encodeIfPresent(processImagePath, forKey: .processImagePath)
        try fields.encodeIfPresent(senderImagePath, forKey: .senderImagePath)
        try fields.encodeIfPresent(subsystem, forKey: .subsystem)
        try fields.encodeIfPresent(category, forKey: .category)
        try fields.encodeIfPresent(composedMessage, forKey: .composedMessage)
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
    public static func history(of source: Source, since start: Date?, limit: Int, containing text: String? = nil) async throws(ProcessFailure) -> [Self] {
        var held = Deque<Self>()
        let window = start.map { start in ["--start", "@\(Int(start.timeIntervalSince1970))"] } ?? ["--last", "boot"]
        try await read(["show"] + window, of: source, levels: nil, containing: text) { line throws(ProcessFailure) in
            held.append(try line.get())
            if held.count > limit { held.removeFirst() }
        }
        return held.reversed()
    }

    public static func stream(of source: Source, levels: [OSLogEntryLog.Level]? = nil) -> AsyncStream<Result<Self, ProcessFailure>> {
        let (entries, continuation) = AsyncStream<Result<Self, ProcessFailure>>.makeStream()
        let following = Task {
            do throws(ProcessFailure) {
                try await read(["stream"], of: source, levels: levels, containing: nil) { line in continuation.yield(line) }
            } catch {
                continuation.yield(.failure(error))
            }
            continuation.finish()
        }
        continuation.onTermination = { _ in following.cancel() }
        return entries
    }

    private static func read(
        _ command: [String],
        of source: Source,
        levels: [OSLogEntryLog.Level]?,
        containing text: String?,
        into sink: (Result<Self, ProcessFailure>) throws(ProcessFailure) -> Void,
    ) async throws(ProcessFailure) {
        let (key, value): (CodingKeys, Any) =
            switch source {
                case .process(let pid): (.processIdentifier, pid)
                case .executable(let url): (.processImagePath, url.path(percentEncoded: false))
            }
        let levelClause: NSPredicate? = levels.map { levels in
            NSCompoundPredicate(
                orPredicateWithSubpredicates: logTypes.filter { _, level in levels.contains(level) }.keys.map { logType in
                    NSPredicate(format: "%K == %@", argumentArray: [CodingKeys.level.stringValue, logType])
                }
            )
        }
        let textClause: NSPredicate? = text.map { text in NSPredicate(format: "%K CONTAINS[c] %@", argumentArray: [CodingKeys.composedMessage.stringValue, text]) }
        let predicate = NSCompoundPredicate(andPredicateWithSubpredicates: [NSPredicate(format: "%K == %@", argumentArray: [key.stringValue, value]), levelClause, textClause].compactMap(\.self))
        let arguments = command + ["--predicate", predicate.predicateFormat, "--style", "ndjson"]
        let subject = "log \(arguments.joined(separator: " "))"
        let decoder = JSONDecoder()
        let ended: ExecutionResult<Void, SequenceOutput, StringOutput<UTF8>>
        do {
            ended = try await Subprocess.run(.name("log"), arguments: Arguments(arguments), input: .none, output: .sequence, error: .string(limit: 1 << 16)) { execution in
                for try await line in execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])) where line.hasPrefix("{") {
                    switch Result(catching: { try decoder.decode(Line.self, from: Data(line.utf8)).entry }) {
                        case .success(let entry?): try sink(.success(entry))
                        case .success(nil): continue
                        case .failure(let error): try sink(.failure(ProcessFailure(subject: line, error)))
                    }
                }
            }
        } catch {
            throw ProcessFailure(subject: subject, error)
        }
        guard ended.terminationStatus.isSuccess else { throw ProcessFailure(subject: subject, reason: .exit(ended.terminationStatus, standardError: ended.standardError)) }
    }
}
