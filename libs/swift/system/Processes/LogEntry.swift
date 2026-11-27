import Darwin
import DequeModule
import Encoding
package import Foundation
import Security
import Subprocess
import System
import os

// --- [CONSTANTS] -----------------------------------------------------------------------

private let timestampFormat = Date.ISO8601FormatStyle(dateTimeSeparator: .space, includingFractionalSeconds: true)
private let decoder = JSONDecoder()
private let windowLead = 1
private let firstWindow: TimeInterval = 60
private let logger = Logger()
private let ticksPerSecond: Double = {
    var timebase = mach_timebase_info_data_t()
    unsafe mach_timebase_info(&timebase)
    return Double(NSEC_PER_SEC) * Double(timebase.denom) / Double(timebase.numer)
}()

// --- [MODELS] --------------------------------------------------------------------------

package struct LogEntry: Codable, Sendable {
    package enum Source: Sendable {
        case process(pid_t)
        case executable(URL)
    }

    package enum Level: String, Codable, Sendable {
        case debug = "Debug"
        case info = "Info"
        case notice = "Default"
        case error = "Error"
        case fault = "Fault"
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
    let level: Level
    let processIdentifier: pid_t
    let processImagePath: String
    let senderImagePath: String
    let subsystem: String
    let category: String
    let composedMessage: String

    package init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        level = try fields.decode(Level.self, forKey: .level)
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
        try fields.encode(level, forKey: .level)
        try fields.encode(processIdentifier, forKey: .processIdentifier)
        try fields.encode(processImagePath, forKey: .processImagePath)
        try fields.encode(senderImagePath, forKey: .senderImagePath)
        try fields.encode(subsystem, forKey: .subsystem)
        try fields.encode(category, forKey: .category)
        try fields.encode(composedMessage, forKey: .composedMessage)
    }
}

private struct Line: Decodable {
    let level: Lenient<LogEntry.Level>?
    let entry: LogEntry?

    init(from decoder: any Decoder) throws {
        level = try decoder.container(keyedBy: LogEntry.CodingKeys.self).decodeIfPresent(Lenient<LogEntry.Level>.self, forKey: .level)
        entry =
            switch level {
                case .some(.known): try LogEntry(from: decoder)
                case .some(.unknown), .none: nil
            }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LogEntry.Source {
    fileprivate func filter(levels: [LogEntry.Level]?) throws(ProcessFailure) -> NSPredicate {
        let shown = (levels ?? [.error, .fault]).map { level in NSPredicate(format: "logType == %@", level.rawValue.lowercased()) }
        guard levels == nil else { return NSCompoundPredicate(orPredicateWithSubpredicates: shown) }
        let executable: URL =
            switch self {
                case .process(let pid):
                    try Result { () throws(Errno) in try URL(filePath: ProcessInstance(of: pid).executable().string) }
                        .mapError { error in ProcessFailure(subject: "Executable of the logging process", reason: .system(error)) }.get()
                case .executable(let url): url
            }
        var (code, location): (SecStaticCode?, CFURL?) = (nil, nil)
        let created = unsafe SecStaticCodeCreateWithPath(executable as CFURL, [], &code)
        let status = code.map { code in unsafe SecCodeCopyPath(code, [], &location) } ?? created
        guard status == errSecSuccess, let location else {
            throw ProcessFailure(subject: "Code at \(executable.path(percentEncoded: false))", reason: .foundation(NSError(domain: NSOSStatusErrorDomain, code: Int(status))))
        }
        let path = FilePath((location as URL).path(percentEncoded: false)).string
        let sender = NSPredicate(format: "senderImagePath == %@ OR senderImagePath BEGINSWITH %@", path, path + "/")
        let subsystem = Bundle(url: location as URL)?.bundleIdentifier.map { identifier in NSPredicate(format: "subsystem == %@ OR subsystem BEGINSWITH %@", identifier, identifier + ".") }
        return NSCompoundPredicate(orPredicateWithSubpredicates: shown + [sender, subsystem].compactMap(\.self))
    }
}

extension LogEntry {
    package static func history(of source: Source, since start: Date?, limit: Int, containing text: String? = nil) async throws(ProcessFailure) -> [Self] {
        let (now, tick) = (Date.now, Int64(mach_continuous_time()))
        let earliest = min(max(0, start.map { start in tick - Int64(now.timeIntervalSince(start) * ticksPerSecond) } ?? 0), tick)
        let windows = sequence(first: max(earliest, tick - Int64(firstWindow * ticksPerSecond))..<tick) { window in
            window.lowerBound > earliest ? max(earliest, 2 * window.lowerBound - tick)..<window.lowerBound : nil
        }
        var entries: [Self] = []
        for window in windows where entries.count < limit {
            var held = Deque<Self>()
            let wanted = limit - entries.count
            try await read(during: window, of: source, levels: nil, containing: text) { (line: String) throws(ProcessFailure) in
                if let entry = try entry(in: line)?.get() {
                    held.append(entry)
                    held.removeFirst(max(0, held.count - wanted))
                }
            }
            entries += held.reversed()
        }
        return entries
    }

    package static func stream(of source: Source, levels: [Level]? = nil) -> AsyncStream<Result<Self, ProcessFailure>> {
        let (entries, continuation) = AsyncStream<Result<Self, ProcessFailure>>.makeStream()
        let following = Task {
            do throws(ProcessFailure) {
                try await read(during: nil, of: source, levels: levels, containing: nil) { line in if let entry = entry(in: line) { continuation.yield(entry) } }
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
            let decoded = try decoder.decode(Line.self, from: Data(line.utf8))
            if case .some(.unknown(let level)) = decoded.level { logger.error("Unified log event left out, as log names its level \(level, privacy: .public)") }
            return decoded.entry.map(Result.success)
        } catch {
            let key: (any CodingKey)? =
                switch error as? DecodingError {
                    case .some(.keyNotFound(let key, _)): key
                    case .some(.typeMismatch(_, let context)), .some(.valueNotFound(_, let context)), .some(.dataCorrupted(let context)): context.codingPath.last
                    case .some, .none: nil
                }
            return .failure(ProcessFailure(subject: key.map { key in "Unified log event field \(key.stringValue)" } ?? "Unified log event", error))
        }
    }

    private static func read(
        during window: Range<Int64>?,
        of source: Source,
        levels: [Level]?,
        containing text: String?,
        into sink: (String) throws(ProcessFailure) -> Void,
    ) async throws(ProcessFailure) {
        let process: NSPredicate =
            switch source {
                case .process(let pid): NSPredicate(format: "processIdentifier == %d", pid)
                case .executable(let url): NSPredicate(format: "processImagePath == %@", url.path(percentEncoded: false))
            }
        let filter: NSPredicate =
            switch text {
                case .some(let text): NSPredicate(format: "composedMessage CONTAINS[c] %@", text)
                case .none: try source.filter(levels: levels)
            }
        let clauses: [NSPredicate?] = [
            NSPredicate(format: "type == logEvent"),
            process,
            window.map { window in NSPredicate(format: "machContinuousTimestamp >= %@ AND machContinuousTimestamp < %@", argumentArray: [window.lowerBound, window.upperBound]) },
            filter,
        ]
        let boot = Date.now.timeIntervalSince1970 - Double(mach_continuous_time()) / ticksPerSecond
        let command =
            window.map { window in
                [
                    "show",
                    "--start", "@\(Int(boot + Double(window.lowerBound) / ticksPerSecond) - windowLead)",
                    "--end", "@\(Int((boot + Double(window.upperBound) / ticksPerSecond).rounded(.up)) + windowLead)",
                ]
            } ?? ["stream"]
        let shown = [Level.info, .debug].filter { level in levels?.contains(level) ?? true }.map { level in "--\(level.rawValue.lowercased())" }
        let predicate = NSCompoundPredicate(andPredicateWithSubpredicates: clauses.compactMap(\.self))
        let log = Configuration(executable: .path("/usr/bin/log"), arguments: Arguments(command + shown + ["--predicate", predicate.predicateFormat, "--style", "ndjson"]))
        try await log.run(input: .none, output: .sequence) { execution in
            for try await line in execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])) where line.hasPrefix("{") { try sink(line) }
        }
    }
}
