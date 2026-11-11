import CasePathsCore
import Encoding
import Files
public import Foundation
public import Processes
import Results

// --- [MODELS] --------------------------------------------------------------------------

public struct CrashReport: Codable, Hashable, Sendable {
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
        let procPath: String?
        let procStartAbsTime: UInt64?
        let coalitionName: String?
        let exception: Exception?
        let termination: Termination?
        let bundleInfo: [String: String]?
    }

    public static let directory: URL = .libraryDirectory.appending(components: "Logs", "DiagnosticReports", directoryHint: .isDirectory)

    public let url: URL
    public let pid: pid_t
    public let instance: ProcessInstance?
    public let executable: String?
    public let service: String?
    public let bundleIdentifier: String?
    public let exception: String?
    public let signal: String?
    public let termination: Termination?

    init?(_ data: Data, at url: URL) throws {
        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        let end = data.firstIndex(of: UInt8(ascii: "\n")) ?? data.endIndex
        let header = try decoder.decode(Header.self, from: data[..<end])
        guard case .known(.crash) = header.bugType, header.isSimulated != 1 else { return nil }
        let body = try decoder.decode(Body.self, from: data[end...].dropFirst())
        self.url = url
        pid = body.pid
        instance = body.procStartAbsTime.map { started in ProcessInstance(pid: body.pid, started: started) }
        executable = body.procPath
        service = body.coalitionName
        bundleIdentifier = [header.bundleID, body.bundleInfo?[kCFBundleIdentifierKey as String]].lazy.compactMap(\.self).first { identifier in !["", "unknown"].contains(identifier) }
        exception = body.exception?.type
        signal = body.exception?.signal
        termination = body.termination
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CrashReport {
    public static func reports(since earliest: Date = .distantPast, in directory: URL = directory, where matching: (Self) -> Bool) -> (reports: [Self], failures: [ApplicationFailure]) {
        let listed = listing(directory, since: earliest)
        let reads = listed.files.map(read)
        return (
            reads.compactMap { read in read[case: \.success].flatMap(\.self) }.filter(matching),
            listed.failures + reads.compactMap { read in read[case: \.failure] },
        )
    }

    public static func written(
        by process: ProcessInstance,
        since launch: Date,
        within limit: Duration = .seconds(60),
        in directory: URL = directory,
        clock: some Clock<Duration> = ContinuousClock(),
    ) async -> Result<Self, ApplicationFailure> {
        await Result { () async throws(ApplicationFailure) in
            try await withTimeout(limit, expired: ApplicationFailure(subject: String(process.pid), reason: .noReport(limit)), clock: clock) { () async throws(ApplicationFailure) in
                var listed = false
                for await change in FileEvent.events(of: directory) {
                    let batch =
                        switch change {
                            case .success(let events): events
                            case .failure(let failure): throw ApplicationFailure(subject: failure.subject, reason: .unwatched(failure))
                        }
                    let files = listed ? dated(batch.map(\.url), since: launch).files : listing(directory, since: launch).files
                    listed = true
                    if let report = files.lazy.compactMap({ file in read(file)[case: \.success].flatMap(\.self) }).first(where: { report in
                        report.instance.map { instance in instance == process } ?? (report.pid == process.pid)
                    }) {
                        return report
                    }
                }
                throw ApplicationFailure(subject: String(process.pid), reason: .cancelled)
            }
        }
    }

    private static func listing(_ directory: URL, since earliest: Date) -> (files: [URL], failures: [ApplicationFailure]) {
        let entries: [URL]
        do {
            entries = try ifPresent { try FileManager.default.contentsOfDirectory(at: directory, includingPropertiesForKeys: [.contentModificationDateKey], options: .skipsHiddenFiles) } ?? []
        } catch {
            return ([], [unread(directory, error)])
        }
        return dated(entries, since: earliest)
    }

    private static func dated(_ entries: [URL], since earliest: Date) -> (files: [URL], failures: [ApplicationFailure]) {
        let stamped = entries.filter { url in url.pathExtension == "ips" }.map { url in
            (url: url, date: Result { try url.resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate })
        }
        let files = stamped.compactMap { entry in entry.date[case: \.success].map { date in (url: entry.url, date: date) } }.filter { file in
            file.date.map { date in date >= earliest } ?? true
        }
        let failures = stamped.compactMap { entry in entry.date[case: \.failure].map { error in unread(entry.url, error) } }
        return (files.sorted(using: KeyPathComparator(\.date, order: .reverse)).map(\.url), failures)
    }

    private static func read(_ file: URL) -> Result<Self?, ApplicationFailure> {
        Result { try ifPresent { try Self(Data(contentsOf: file), at: file) }.flatMap(\.self) }.mapError { error in unread(file, error) }
    }

    private static func unread(_ file: URL, _ error: any Error) -> ApplicationFailure {
        ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error as NSError))
    }
}
