public import Foundation
import Subprocess
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let unifiedLog: FilePath = "/usr/bin/log"
private let logTime = Date.ISO8601FormatStyle(includingFractionalSeconds: true).year().month().day().dateTimeSeparator(.space).time(includingFractionalSeconds: true)
    .timeZone(separator: .omitted)

// --- [MODELS] --------------------------------------------------------------------------

public struct LogEntry: Codable, Hashable, Sendable {
    public let timestamp: Date
    public let processID: pid_t?
    public let messageType: String?
    public let subsystem: String?
    public let category: String?
    public let eventMessage: String?
    public let senderImagePath: String?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ProcessInstance {
    public func logEntries(matching predicate: NSPredicate? = nil) -> AsyncStream<Result<LogEntry, ProcessFailure>> {
        let subject = "\(unifiedLog) stream --process \(pid)"
        let arguments = ["stream", "--process", String(pid), "--style", "ndjson"] + (predicate.map { predicate in ["--predicate", predicate.predicateFormat] } ?? [])
        let configuration = Configuration(executable: .path(unifiedLog), arguments: Arguments(arguments), platformOptions: .session())
        return AsyncStream { continuation in
            let streaming = Task {
                let decoder = JSONDecoder()
                decoder.dateDecodingStrategy = .custom { decoder in try Date(decoder.singleValueContainer().decode(String.self), strategy: logTime) }
                do {
                    _ = try await Subprocess.run(configuration, input: .none, output: .sequence, error: .discarded) { execution in
                        for try await line in execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])) where line.hasPrefix("{") {
                            continuation.yield(
                                Result { () throws(ProcessFailure) in
                                    do { return try decoder.decode(LogEntry.self, from: Data(line.utf8)) } catch { throw ProcessFailure(subject: line, error) }
                                }
                            )
                        }
                    }
                } catch  where !Task.isCancelled {
                    continuation.yield(.failure(ProcessFailure(subject: subject, error)))
                } catch {}
                continuation.finish()
            }
            continuation.onTermination = { _ in streaming.cancel() }
        }
    }
}
