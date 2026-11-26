import DequeModule
public import Foundation
import Subprocess
public import os

// --- [CONSTANTS] -----------------------------------------------------------------------

private let logDecoder: JSONDecoder = {
    let decoder = JSONDecoder()
    let time = Date.ISO8601FormatStyle(dateTimeSeparator: .space, includingFractionalSeconds: true)
    decoder.dateDecodingStrategy = .custom { field in try Date(field.singleValueContainer().decode(String.self), strategy: time) }
    return decoder
}()

// --- [MODELS] --------------------------------------------------------------------------

public struct LogEntry: Codable, Sendable {
    public enum Source: Sendable {
        case process(pid_t)
        case executable(URL)

        func arguments(levels: [OSLogType]?, containing text: String?) -> [String] {
            let (key, value): (CodingKeys, Any) =
                switch self {
                    case .process(let pid): (.processID, pid)
                    case .executable(let url): (.processImagePath, url.path(percentEncoded: false))
                }
            let levelClause: NSPredicate? = levels.map { levels in
                NSCompoundPredicate(orPredicateWithSubpredicates: levels.map { level in NSPredicate(format: "%K == %d", argumentArray: [CodingKeys.messageType.stringValue, level.rawValue]) })
            }
            let textClause: NSPredicate? = text.map { text in NSPredicate(format: "%K CONTAINS[c] %@", argumentArray: [CodingKeys.eventMessage.stringValue, text]) }
            let clauses: [NSPredicate] = [NSPredicate(format: "%K == %@", argumentArray: [key.stringValue, value]), levelClause, textClause].compactMap(\.self)
            let flags: [String] = [(OSLogType.info, "--info"), (OSLogType.debug, "--debug")].filter { level, _ in levels?.contains(level) == true }.map(\.1)
            return ["--predicate", NSCompoundPredicate(andPredicateWithSubpredicates: clauses).predicateFormat, "--style", "ndjson"] + flags
        }
    }

    fileprivate enum CodingKeys: CodingKey {
        case timestamp, processID, processImagePath, messageType, subsystem, category, eventMessage, senderImagePath
    }

    public let timestamp: Date
    public let processID: pid_t?
    public let processImagePath: String?
    public let messageType: String?
    public let subsystem: String?
    public let category: String?
    public let eventMessage: String?
    public let senderImagePath: String?
}

private struct Line: Decodable {
    let entry: LogEntry?

    init(from decoder: any Decoder) throws {
        entry = try decoder.container(keyedBy: LogEntry.CodingKeys.self).contains(.timestamp) ? LogEntry(from: decoder) : nil
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LogEntry {
    public static func history(of source: Source, since start: Date?, limit: Int, levels: [OSLogType]? = nil, containing text: String? = nil) async throws(ProcessFailure) -> [Self] {
        let window = start.map { start in ["--start", "@\(Int(start.timeIntervalSince1970))"] } ?? ["--last", "boot"]
        let arguments = ["show"] + source.arguments(levels: levels, containing: text) + window
        let subject = "log \(arguments.joined(separator: " "))"
        let shown: ExecutionResult<Deque<Self>, SequenceOutput, StringOutput<UTF8>>
        do {
            shown = try await Subprocess.run(.name("log"), arguments: Arguments(arguments), input: .none, output: .sequence, error: .string(limit: 1 << 16)) { execution in
                try await execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])).reduce(into: Deque<Self>()) { held, line in
                    guard let entry = try logDecoder.decode(Line.self, from: Data(line.utf8)).entry else { return }
                    held.append(entry)
                    if held.count > limit { held.removeFirst() }
                }
            }
        } catch {
            throw ProcessFailure(subject: subject, error)
        }
        guard shown.terminationStatus.isSuccess else { throw ProcessFailure(subject: subject, reason: .exit(shown.terminationStatus, standardError: shown.standardError)) }
        return shown.closureResult.reversed()
    }

    public static func stream(of source: Source, levels: [OSLogType]? = nil) -> AsyncStream<Result<Self, ProcessFailure>> {
        let arguments = ["stream"] + source.arguments(levels: levels, containing: nil)
        let subject = "log \(arguments.joined(separator: " "))"
        let (entries, continuation) = AsyncStream<Result<Self, ProcessFailure>>.makeStream()
        let following = Task {
            do {
                let ended = try await Subprocess.run(.name("log"), arguments: Arguments(arguments), input: .none, output: .sequence, error: .string(limit: 1 << 16)) { execution in
                    for try await line in execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])) where line.hasPrefix("{") {
                        switch Result(catching: { try logDecoder.decode(Line.self, from: Data(line.utf8)).entry }) {
                            case .success(let entry?): continuation.yield(.success(entry))
                            case .success(nil): continue
                            case .failure(let error): continuation.yield(.failure(ProcessFailure(subject: line, error)))
                        }
                    }
                }
                if !ended.terminationStatus.isSuccess { continuation.yield(.failure(ProcessFailure(subject: subject, reason: .exit(ended.terminationStatus, standardError: ended.standardError)))) }
            } catch {
                continuation.yield(.failure(ProcessFailure(subject: subject, error)))
            }
            continuation.finish()
        }
        continuation.onTermination = { _ in following.cancel() }
        return entries
    }
}
