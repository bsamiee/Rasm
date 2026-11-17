import DequeModule
public import Foundation
import Subprocess

// --- [CONSTANTS] -----------------------------------------------------------------------

private let logTime = Date.ISO8601FormatStyle(includingFractionalSeconds: true).year().month().day().dateTimeSeparator(.space).time(includingFractionalSeconds: true)
    .timeZone(separator: .omitted)

// --- [MODELS] --------------------------------------------------------------------------

public struct LogEvent: Codable, Sendable {
    private enum CodingKeys: CodingKey {
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

    public init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        timestamp = try Date(fields.decode(String.self, forKey: .timestamp), strategy: logTime)
        processID = try fields.decodeIfPresent(pid_t.self, forKey: .processID)
        processImagePath = try fields.decodeIfPresent(String.self, forKey: .processImagePath)
        messageType = try fields.decodeIfPresent(String.self, forKey: .messageType)
        subsystem = try fields.decodeIfPresent(String.self, forKey: .subsystem)
        category = try fields.decodeIfPresent(String.self, forKey: .category)
        eventMessage = try fields.decodeIfPresent(String.self, forKey: .eventMessage)
        senderImagePath = try fields.decodeIfPresent(String.self, forKey: .senderImagePath)
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(timestamp.formatted(logTime), forKey: .timestamp)
        try fields.encodeIfPresent(processID, forKey: .processID)
        try fields.encodeIfPresent(processImagePath, forKey: .processImagePath)
        try fields.encodeIfPresent(messageType, forKey: .messageType)
        try fields.encodeIfPresent(subsystem, forKey: .subsystem)
        try fields.encodeIfPresent(category, forKey: .category)
        try fields.encodeIfPresent(eventMessage, forKey: .eventMessage)
        try fields.encodeIfPresent(senderImagePath, forKey: .senderImagePath)
    }
}

private struct Printed: Decodable {
    private enum CodingKeys: CodingKey {
        case timestamp
    }

    let event: LogEvent?

    init(from decoder: any Decoder) throws {
        event = try decoder.container(keyedBy: CodingKeys.self).contains(.timestamp) ? LogEvent(from: decoder) : nil
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LogEvent {
    public static func history(of process: String, since start: Date?, limit: Int) async throws(ProcessFailure) -> [Self] {
        let window = start.map { start in ["--start", "@\(Int(start.timeIntervalSince1970))"] } ?? ["--last", "boot"]
        let subject = "log show --process \(process)"
        let decoder = JSONDecoder()
        let shown: ExecutionResult<Deque<Self>, SequenceOutput, StringOutput<UTF8>>
        do {
            shown = try await Subprocess.run(
                Configuration(executable: .name("log"), arguments: Arguments(["show", "--process", process, "--style", "ndjson"] + window)),
                input: .none,
                output: .sequence,
                error: .string(limit: 1 << 16),
            ) { execution in
                try await execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])).reduce(into: Deque<Self>()) { held, line in
                    guard let event = try decoder.decode(Printed.self, from: Data(line.utf8)).event else { return }
                    held.append(event)
                    if held.count > limit { held.removeFirst() }
                }
            }
        } catch {
            throw ProcessFailure(subject: subject, error)
        }
        guard shown.terminationStatus.isSuccess else { throw ProcessFailure(subject: subject, reason: .exit(shown.terminationStatus, standardError: shown.standardError)) }
        return shown.closureResult.reversed()
    }

    public static func stream(of process: String) -> AsyncStream<Result<Self, ProcessFailure>> {
        AsyncStream { continuation in
            let following = Task {
                let decoder = JSONDecoder()
                let subject = "log stream --process \(process)"
                do {
                    _ = try await Subprocess.run(
                        Configuration(executable: .name("log"), arguments: ["stream", "--process", process, "--style", "ndjson"]),
                        input: .none,
                        output: .sequence,
                        error: .discarded,
                    ) { execution in
                        let printed = execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])).filter { line in line.hasPrefix("{") }
                        let decoded = printed.compactMap { line -> Result<Self, ProcessFailure>? in
                            switch Result(catching: { try decoder.decode(Printed.self, from: Data(line.utf8)).event }) {
                                case .success(let event): event.map(Result.success)
                                case .failure(let error): .failure(ProcessFailure(subject: line, error))
                            }
                        }
                        for try await outcome in decoded { continuation.yield(outcome) }
                    }
                } catch  where !Task.isCancelled {
                    continuation.yield(.failure(ProcessFailure(subject: subject, error)))
                } catch {}
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }
}
