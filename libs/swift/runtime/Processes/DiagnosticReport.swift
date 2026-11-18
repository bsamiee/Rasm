import CasePathsCore
import Encoding
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
        enum BugType: String {
            case crash = "309"
        }

        let bugType: Lenient<BugType>
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

    private enum CodingKeys: CodingKey {
        case url, kind, date, pid, instance, name, executable, service, bundleIdentifier, event, exception, signal, termination
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

    init?(_ file: URL, written date: Date) throws {
        switch (file.pathExtension, Kind(rawValue: file.pathExtension)) {
            case ("ips", _):
                let data = try Data(contentsOf: file)
                let decoder = JSONDecoder()
                decoder.keyDecodingStrategy = .convertFromSnakeCase
                let end = data.firstIndex(of: UInt8(ascii: "\n")) ?? data.endIndex
                let header = try decoder.decode(Header.self, from: data[..<end])
                guard case .known(.crash) = header.bugType, header.isSimulated != 1 else { return nil }
                let body = try decoder.decode(Body.self, from: data[end...].dropFirst())
                let identifier = [header.bundleID, body.bundleInfo?[kCFBundleIdentifierKey as String]].lazy.compactMap(\.self).first { identifier in !["", "unknown"].contains(identifier) }
                self.init(
                    url: file,
                    kind: .crash,
                    date: date,
                    pid: body.pid,
                    instance: body.procStartAbsTime.map { started in ProcessInstance(pid: body.pid, started: started) },
                    name: body.procName,
                    executable: body.procPath,
                    service: body.coalitionName,
                    bundleIdentifier: identifier,
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

    private init(
        url: URL,
        kind: Kind,
        date: Date,
        pid: pid_t,
        instance: ProcessInstance?,
        name: String?,
        executable: String?,
        service: String?,
        bundleIdentifier: String?,
        event: String?,
        exception: String?,
        signal: String?,
        termination: Termination?,
    ) {
        (self.url, self.kind, self.date, self.pid, self.instance, self.name, self.executable) = (url, kind, date, pid, instance, name, executable)
        (self.service, self.bundleIdentifier, self.event, self.exception, self.signal, self.termination) = (service, bundleIdentifier, event, exception, signal, termination)
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(url, forKey: .url)
        try fields.encode(kind, forKey: .kind)
        try fields.encode(date.formatted(Date.ISO8601FormatStyle()), forKey: .date)
        try fields.encode(pid, forKey: .pid)
        try fields.encodeIfPresent(instance, forKey: .instance)
        try fields.encodeIfPresent(name, forKey: .name)
        try fields.encodeIfPresent(executable, forKey: .executable)
        try fields.encodeIfPresent(service, forKey: .service)
        try fields.encodeIfPresent(bundleIdentifier, forKey: .bundleIdentifier)
        try fields.encodeIfPresent(event, forKey: .event)
        try fields.encodeIfPresent(exception, forKey: .exception)
        try fields.encodeIfPresent(signal, forKey: .signal)
        try fields.encodeIfPresent(termination, forKey: .termination)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension DiagnosticReport {
    public func written(by process: ProcessInstance) -> Bool { instance.map { instance in instance == process } ?? (pid == process.pid) }

    public func written(by bundle: Bundle) -> Bool {
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
        let listed = directories.map { directory in listing(directory, since: earliest) }
        return listed.flatMap(\.files).sorted(using: KeyPathComparator(\.date, order: .reverse))
            .reduce(into: (reports: [Self](), failures: listed.flatMap(\.failures))) { found, file in
                guard found.reports.count < limit else { return }
                switch read(file) {
                    case .success(.some(let report)) where matching(report): found.reports.append(report)
                    case .success: break
                    case .failure(let failure): found.failures.append(failure)
                }
            }
    }

    public static func written(
        by process: ProcessInstance,
        since launch: Date,
        within limit: Duration = .seconds(60),
        in directory: URL = .libraryDirectory.appending(components: "Logs", "DiagnosticReports", directoryHint: .isDirectory),
        clock: some Clock<Duration> = ContinuousClock(),
    ) async -> Result<Self, ProcessFailure> {
        let subject = "Crash report of process \(process.pid)"
        return await Result { () async throws(ProcessFailure) in
            try await withTimeout(limit, expired: ProcessFailure(subject: subject, reason: .timedOut(limit)), clock: clock) { () async throws(ProcessFailure) in
                var listed = false
                for await change in FileEvent.events(of: directory) {
                    let batch =
                        switch change {
                            case .success(let events): events
                            case .failure(let failure): throw ProcessFailure(subject: failure.subject, reason: .io(failure))
                        }
                    let files = listed ? dated(batch.map(\.url), since: launch).files : listing(directory, since: launch).files
                    listed = true
                    if let report = files.lazy.compactMap({ file in read(file)[case: \.success].flatMap(\.self) }).first(where: { report in report.kind == .crash && report.written(by: process) }) {
                        return report
                    }
                }
                throw ProcessFailure(subject: subject, reason: .cancelled(CancellationError()))
            }
        }
    }

    private static func listing(_ directory: URL, since earliest: Date) -> (files: [(url: URL, date: Date)], failures: [ProcessFailure]) {
        let entries: [URL]
        do {
            entries = try ifPresent { try FileManager.default.contentsOfDirectory(at: directory, includingPropertiesForKeys: [.contentModificationDateKey], options: .skipsHiddenFiles) } ?? []
        } catch {
            return ([], [ProcessFailure(subject: directory.path(percentEncoded: false), reason: .io(error))])
        }
        return dated(entries, since: earliest)
    }

    private static func dated(_ entries: [URL], since earliest: Date) -> (files: [(url: URL, date: Date)], failures: [ProcessFailure]) {
        let stamped = entries.filter { url in url.pathExtension == "ips" || Kind(rawValue: url.pathExtension) != nil }.map { url in
            (url: url, date: Result { try url.resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate })
        }
        let files = stamped.compactMap { entry in entry.date[case: \.success].map { date in (url: entry.url, date: date ?? earliest) } }.filter { file in file.date >= earliest }
        let failures = stamped.compactMap { entry in entry.date[case: \.failure].map { error in ProcessFailure(subject: entry.url.path(percentEncoded: false), reason: .io(error)) } }
        return (files.sorted(using: KeyPathComparator(\.date, order: .reverse)), failures)
    }

    private static func read(_ file: (url: URL, date: Date)) -> Result<Self?, ProcessFailure> {
        Result { try ifPresent { try Self(file.url, written: file.date) }.flatMap(\.self) }.mapError { error in ProcessFailure(subject: file.url.path(percentEncoded: false), reason: .io(error)) }
    }
}
