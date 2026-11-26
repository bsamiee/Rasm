import AsyncAlgorithms
import CasePathsCore
import Files
public import Foundation
import Results
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct DiagnosticReport: Encodable, Hashable, Sendable {
    public enum Kind: String, Encodable, Sendable {
        case crash
        case hang
        case diag
        case spin
    }

    public struct Termination: Codable, Hashable, Sendable {
        public let namespace: String
        public let code: UInt64?
        public let indicator: String?
    }

    private struct Header: Decodable {
        let bugType: String
    }

    private struct Body: Decodable {
        struct Exception: Decodable {
            let type: String
            let signal: String?
        }

        let pid: pid_t
        let captureTime: String
        let isSimulated: Int?
        let procName: String?
        let procPath: String?
        let procLaunch: String?
        let coalitionName: String?
        let bundleInfo: [String: String]?
        let exception: Exception?
        let termination: Termination?
    }

    public static let directories: [URL] = FileManager.default.urls(for: .libraryDirectory, in: [.userDomainMask, .localDomainMask]).map { library in
        library.appending(components: "Logs", "DiagnosticReports", directoryHint: .isDirectory)
    }

    private static let dateTime = Date.ParseStrategy(
        format: """
            \(year: .defaultDigits)-\(month: .twoDigits)-\(day: .twoDigits) \
            \(hour: .twoDigits(clock: .twentyFourHour, hourCycle: .zeroBased)):\(minute: .twoDigits):\(second: .twoDigits).\(secondFraction: .fractional(4)) \
            \(timeZone: .iso8601(.short))
            """,
        timeZone: .gmt,
    )

    private static let keys: Set<URLResourceKey> = [.contentModificationDateKey, .isRegularFileKey, .isHiddenKey]

    public let url: URL
    public let kind: Kind
    public let date: Date
    public let pid: pid_t
    public let launched: Date?
    public let name: String?
    public let executable: String?
    public let coalition: String?
    public let bundleIdentifier: String?
    public let event: String?
    public let exception: String?
    public let signal: String?
    public let termination: Termination?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension DiagnosticReport {
    init?(_ file: URL) throws {
        let data = try Data(contentsOf: file, options: .alwaysMapped)
        switch (file.pathExtension, Kind(rawValue: file.pathExtension)) {
            case ("ips", _):
                let decoder = JSONDecoder()
                decoder.keyDecodingStrategy = .convertFromSnakeCase
                let end = data.firstIndex(of: UInt8(ascii: "\n")) ?? data.endIndex
                guard try decoder.decode(Header.self, from: data[..<end]).bugType == "309" else { return nil }
                let body = try decoder.decode(Body.self, from: data[end...].dropFirst())
                guard (body.isSimulated ?? 0) == 0 else { return nil }
                self.init(
                    url: file,
                    kind: .crash,
                    date: try Self.dateTime.parse(body.captureTime),
                    pid: body.pid,
                    launched: try body.procLaunch.map(Self.dateTime.parse),
                    name: body.procName,
                    executable: body.procPath,
                    coalition: body.coalitionName,
                    bundleIdentifier: body.bundleInfo?[kCFBundleIdentifierKey as String],
                    event: nil,
                    exception: body.exception?.type,
                    signal: body.exception?.signal,
                    termination: body.termination,
                )
            case (_, .some(let kind)):
                let lines = String(decoding: data.prefix(1 << 14), as: UTF8.self).matches(of: /^([^:\n]+):[ \t]*(.*)$/.anchorsMatchLineEndings())
                let fields: [String: String] = Dictionary(lines.map { line -> (String, String) in (String(line.output.1), String(line.output.2)) }, uniquingKeysWith: { first, _ in first })
                guard let pid = fields["PID"].flatMap({ pid in pid_t(pid) }), let captured = fields["Date/Time"] else { return nil }
                self.init(
                    url: file,
                    kind: kind,
                    date: try Self.dateTime.parse(captured),
                    pid: pid,
                    launched: nil,
                    name: fields["Command"],
                    executable: fields["Path"],
                    coalition: nil,
                    bundleIdentifier: fields["Identifier"],
                    event: fields["Event"],
                    exception: nil,
                    signal: nil,
                    termination: nil,
                )
            case (_, .none): return nil
        }
    }

    public func isWritten(by process: ProcessInstance) -> Bool {
        pid == process.pid && (launched.map { launched in UInt64((launched.timeIntervalSince1970 * 1000).rounded()) == process.started / 1000 } ?? true)
    }

    public func isWritten(by bundle: Bundle) -> Bool {
        guard let reported = executable.map({ path in FilePath(path) }), let path = bundle.executablePath.map({ path in FilePath(path) }) else { return false }
        return switch reported.components.firstIndex(of: "*") {
            case .some(let marker): bundleIdentifier == bundle.bundleIdentifier && path.ends(with: FilePath(root: nil, reported.components[marker...].dropFirst()))
            case .none: reported == path
        }
    }

    public static func reports(
        since earliest: Date = .distantPast,
        limit: Int = .max,
        in directories: [URL] = directories,
        where matching: (Self) -> Bool,
    ) -> (reports: [Self], failures: [ProcessFailure]) {
        let listed = directories.map { directory in list(directory, since: earliest) }
        let found = listed.flatMap(\.files).compactMap { file in read(file, since: earliest) }
        let reports = found.compactMap { report in report[case: \.success] }.filter(matching).sorted(using: KeyPathComparator(\.date, order: .reverse))
        return (Array(reports.prefix(limit)), listed.flatMap(\.failures) + found.compactMap { report in report[case: \.failure] })
    }

    public static func written(since earliest: Date, in directory: URL = directories[0]) -> some AsyncSequence<Result<Self, ProcessFailure>, Never> & Sendable {
        let initial: (read: Set<URL>, found: [Result<Self, ProcessFailure>]) = ([], [])
        return FileEvent.events(of: directory).reductions(into: initial) { held, change in
            let listed: (files: [URL], failures: [ProcessFailure]) =
                switch change {
                    case .success(let batch) where batch.isEmpty || batch.contains(where: \.mustScanSubDirs): list(directory, since: earliest)
                    case .success(let batch): modified(Set(batch.map(\.url)), since: earliest)
                    case .failure(let failure): ([], [ProcessFailure(subject: failure.subject, reason: .io(failure))])
                }
            let reads: [(url: URL, report: Result<Self, ProcessFailure>?)] = listed.files.filter { file in !held.read.contains(file) }.map { file in (file, read(file, since: earliest)) }
            held.read.formUnion(reads.filter { entry in entry.report?[case: \.failure] == nil }.map(\.url))
            held.found = listed.failures.map(Result<Self, ProcessFailure>.failure) + reads.compactMap(\.report)
        }
        .flatMap(\.found.async)
    }

    public static func first(
        writtenBy process: ProcessInstance,
        since launch: Date,
        within limit: Duration = .seconds(60),
        in directory: URL = directories[0],
        clock: some Clock<Duration> = ContinuousClock(),
    ) async throws(ProcessFailure) -> Self {
        let subject = "Crash report of process \(process.pid)"
        return try await withTimeout(limit, expired: ProcessFailure(subject: subject, reason: .timedOut(limit)), clock: clock) { () async throws(ProcessFailure) in
            var ended: ProcessFailure?
            for await found in written(since: launch, in: directory) {
                switch found {
                    case .success(let report) where report.kind == .crash && report.isWritten(by: process): return report
                    case .success: continue
                    case .failure(let failure): ended = failure
                }
            }
            throw (Task.isCancelled ? nil : ended) ?? ProcessFailure(subject: subject, reason: .cancelled(CancellationError()))
        }
    }

    private static func list(_ directory: URL, since earliest: Date) -> (files: [URL], failures: [ProcessFailure]) {
        do {
            return modified(try ifPresent { try FileManager.default.contentsOfDirectory(at: directory, includingPropertiesForKeys: Array(keys)) } ?? [], since: earliest)
        } catch {
            return ([], [ProcessFailure(subject: directory.path(percentEncoded: false), reason: .io(error))])
        }
    }

    private static func modified(_ entries: some Sequence<URL>, since earliest: Date) -> (files: [URL], failures: [ProcessFailure]) {
        let read: [(url: URL, values: Result<URLResourceValues?, any Error>)] = entries.map { url in (url, Result { try ifPresent { try url.resourceValues(forKeys: keys) } }) }
        let files: [URL] = read.compactMap { url, values in
            guard case .success(let values?) = values, values.isRegularFile == true, values.isHidden == false, let modified = values.contentModificationDate, modified >= earliest else { return nil }
            return url
        }
        return (files, read.compactMap { url, values in values[case: \.failure].map { error in ProcessFailure(subject: url.path(percentEncoded: false), reason: .io(error)) } })
    }

    private static func read(_ file: URL, since earliest: Date) -> Result<Self, ProcessFailure>? {
        do {
            return try ifPresent { try Self(file) }.flatMap(\.self).flatMap { report in report.date >= earliest ? .success(report) : nil }
        } catch {
            return .failure(ProcessFailure(subject: file.path(percentEncoded: false), reason: .io(error)))
        }
    }
}
