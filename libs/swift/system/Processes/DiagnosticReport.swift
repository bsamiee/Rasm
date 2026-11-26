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
        let bundleID: String?
        let isSimulated: Int?
    }

    private struct Body: Decodable {
        struct Exception: Decodable {
            let type: String
            let signal: String?
        }

        let pid: pid_t
        let procName: String?
        let procPath: String?
        let procStartAbsTime: UInt64?
        let coalitionName: String?
        let exception: Exception?
        let termination: Termination?
        let bundleInfo: [String: String]?
    }

    public static let directories: [URL] = FileManager.default.urls(for: .libraryDirectory, in: [.userDomainMask, .localDomainMask]).map { library in
        library.appending(components: "Logs", "DiagnosticReports", directoryHint: .isDirectory)
    }

    public let url: URL
    public let kind: Kind
    public let date: Date
    public let pid: pid_t
    public let instance: ProcessInstance?
    public let name: String?
    public let executable: String?
    public let service: String?
    public let bundleIdentifier: String?
    public let event: String?
    public let exception: String?
    public let signal: String?
    public let termination: Termination?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension DiagnosticReport {
    init?(_ file: URL, written date: Date) throws {
        switch (file.pathExtension, Kind(rawValue: file.pathExtension)) {
            case ("ips", _):
                let data = try Data(contentsOf: file)
                let decoder = JSONDecoder()
                decoder.keyDecodingStrategy = .convertFromSnakeCase
                let end = data.firstIndex(of: UInt8(ascii: "\n")) ?? data.endIndex
                let header = try decoder.decode(Header.self, from: data[..<end])
                guard header.bugType == "309", header.isSimulated != 1 else { return nil }
                let body = try decoder.decode(Body.self, from: data[end...].dropFirst())
                self.init(
                    url: file,
                    kind: .crash,
                    date: date,
                    pid: body.pid,
                    instance: body.procStartAbsTime.map { started in ProcessInstance(pid: body.pid, started: started) },
                    name: body.procName,
                    executable: body.procPath,
                    service: body.coalitionName,
                    bundleIdentifier: header.bundleID ?? body.bundleInfo?[kCFBundleIdentifierKey as String],
                    event: nil,
                    exception: body.exception?.type,
                    signal: body.exception?.signal,
                    termination: body.termination,
                )
            case (_, .some(let kind)):
                let prefix = try Data(contentsOf: file, options: .alwaysMapped).prefix(1 << 14)
                guard let head = String(bytes: prefix.lastIndex(of: UInt8(ascii: "\n")).map { end in prefix[..<end] } ?? prefix, encoding: .utf8) else {
                    throw CocoaError(.fileReadInapplicableStringEncoding, userInfo: [NSURLErrorKey: file])
                }
                let pairs: [(String, String)] = head.split(whereSeparator: \.isNewline).compactMap { (line: Substring) -> (String, String)? in
                    line.firstIndex(of: ":").map { (colon: Substring.Index) -> (String, String) in
                        (String(line[..<colon]), String(line[line.index(after: colon)...]).trimmingCharacters(in: .whitespaces))
                    }
                }
                let fields: [String: String] = Dictionary(pairs) { first, _ in first }
                guard let pid = fields["PID"].flatMap(pid_t.init) else { return nil }
                self.init(
                    url: file,
                    kind: kind,
                    date: date,
                    pid: pid,
                    instance: nil,
                    name: fields["Command"] ?? fields["Process"],
                    executable: fields["Path"],
                    service: nil,
                    bundleIdentifier: fields["Identifier"],
                    event: fields["Event"],
                    exception: fields["Exception Type"],
                    signal: nil,
                    termination: nil,
                )
            case (_, .none): return nil
        }
    }

    public func isWritten(by process: ProcessInstance) -> Bool { instance.map { instance in instance == process } ?? (pid == process.pid) }

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
        return listed.flatMap(\.files).sorted(using: KeyPathComparator(\.date, order: .reverse))
            .reduce(into: (reports: [Self](), failures: listed.flatMap(\.failures))) { found, file in
                guard found.reports.count < limit else { return }
                switch read(file) {
                    case .success(let report) where matching(report): found.reports.append(report)
                    case .failure(let failure): found.failures.append(failure)
                    case .success, nil: break
                }
            }
    }

    public static func written(since earliest: Date, in directory: URL = directories[0]) -> some AsyncSequence<Result<Self, ProcessFailure>, Never> & Sendable {
        let initial: (parsed: Set<URL>, found: [Result<Self, ProcessFailure>]) = ([], [])
        return FileEvent.events(of: directory).reductions(into: initial) { held, change in
            let listed: (files: [(url: URL, date: Date)], failures: [ProcessFailure]) =
                switch change {
                    case .success(let batch) where batch.isEmpty || batch.contains(where: \.mustScanSubDirs): list(directory, since: earliest)
                    case .success(let batch): readDates(Set(batch.map(\.url)), since: earliest)
                    case .failure(let failure): ([], [ProcessFailure(subject: failure.subject, reason: .io(failure))])
                }
            let reads: [(url: URL, report: Result<Self, ProcessFailure>?)] = listed.files.filter { file in !held.parsed.contains(file.url) }.map { file in (file.url, read(file)) }
            held.parsed.formUnion(reads.filter { entry in entry.report?[case: \.failure] == nil }.map(\.url))
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
    ) async -> Result<Self, ProcessFailure> {
        let subject = "Crash report of process \(process.pid)"
        return await Result { () async throws(ProcessFailure) in
            try await withTimeout(limit, expired: ProcessFailure(subject: subject, reason: .timedOut(limit)), clock: clock) { () async throws(ProcessFailure) in
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
    }

    private static func list(_ directory: URL, since earliest: Date) -> (files: [(url: URL, date: Date)], failures: [ProcessFailure]) {
        let entries: [URL]
        do {
            entries = try ifPresent { try FileManager.default.contentsOfDirectory(at: directory, includingPropertiesForKeys: [.contentModificationDateKey], options: .skipsHiddenFiles) } ?? []
        } catch {
            return ([], [ProcessFailure(subject: directory.path(percentEncoded: false), reason: .io(error))])
        }
        return readDates(entries, since: earliest)
    }

    private static func readDates(_ entries: some Sequence<URL>, since earliest: Date) -> (files: [(url: URL, date: Date)], failures: [ProcessFailure]) {
        let stamped = entries.map { url in (url: url, date: Result { try ifPresent { try url.resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate }.flatMap(\.self) }) }
        let files = stamped.compactMap { entry in entry.date[case: \.success].flatMap(\.self).map { date in (url: entry.url, date: date) } }.filter { file in file.date >= earliest }
        let failures = stamped.compactMap { entry in entry.date[case: \.failure].map { error in ProcessFailure(subject: entry.url.path(percentEncoded: false), reason: .io(error)) } }
        return (files, failures)
    }

    private static func read(_ file: (url: URL, date: Date)) -> Result<Self, ProcessFailure>? {
        do {
            return try ifPresent { try Self(file.url, written: file.date) }.flatMap(\.self).map { report in .success(report) }
        } catch {
            return .failure(ProcessFailure(subject: file.url.path(percentEncoded: false), reason: .io(error)))
        }
    }
}
