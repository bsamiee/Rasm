import CasePaths
import Files
public import Foundation
import Results
import Synchronization

// --- [MODELS] --------------------------------------------------------------------------

public struct CrashReport: Codable, Hashable, Sendable {
    private struct Header: Decodable {
        let bugType: String
        let bundleID: String?
    }

    private struct Body: Decodable {
        struct Exception: Decodable {
            let type: String
        }

        let pid: pid_t
        let exception: Exception?
        let bundleInfo: [String: String]?
    }

    public static let directory: URL = URL.libraryDirectory.appending(components: "Logs", "DiagnosticReports")

    public let url: URL
    public let pid: pid_t
    public let bundleIdentifier: String?
    public let exception: String?

    init?(_ data: Data, at url: URL) throws {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        let header: Header = try decoder.decode(Header.self, from: data.prefix { byte in byte != UInt8(ascii: "\n") })
        guard header.bugType == "309" else { return nil }
        let body: Body = try decoder.decode(Body.self, from: data.drop { byte in byte != UInt8(ascii: "\n") }.dropFirst())
        self.url = url
        pid = body.pid
        bundleIdentifier = [header.bundleID, body.bundleInfo?["CFBundleIdentifier"]].lazy.compactMap(\.self).first { identifier in !["", "unknown"].contains(identifier) }
        exception = body.exception?.type
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CrashReport {
    public static func reports(for identifier: String, in directory: URL = directory) -> (reports: [Self], failures: [ApplicationFailure]) {
        let listed: (files: [URL], failures: [ApplicationFailure]) = listing(directory)
        let reads: [Result<Self, ApplicationFailure>] = listed.files.compactMap(read)
        return (reads.compactMap { read in read[case: \.success] }.filter { report in report.bundleIdentifier == identifier }, listed.failures + reads.compactMap { read in read[case: \.failure] })
    }

    public static func written(
        by process: pid_t,
        within limit: Duration,
        in directory: URL = directory,
        clock: some Clock<Duration> = ContinuousClock(),
    ) async -> Result<Self, ApplicationFailure> {
        let subject: String = "process \(process)"
        return await Result { () async throws(ApplicationFailure) in
            try await withTimeout(limit, expired: ApplicationFailure(subject: subject, reason: .absent("crash report within \(limit)")), clock: clock) { () async throws(ApplicationFailure) in
                for await change: Result<[FileEvent], FileFailure> in FileEvent.events(of: directory) {
                    let files: [URL] =
                        switch change {
                            case .success(let batch) where batch.isEmpty: listing(directory).files
                            case .success(let batch): batch.map(\.url).filter { url in url.pathExtension == "ips" }
                            case .failure(let failure): throw ApplicationFailure(subject: failure.subject, reason: .unwatched(failure))
                        }
                    if let report: Self = files.lazy.compactMap({ file in read(file)?[case: \.success] }).first(where: { report in report.pid == process }) { return report }
                }
                throw ApplicationFailure(subject: "crash report of \(subject)", reason: .cancelled(CancellationError()))
            }
        }
    }

    private static func listing(_ directory: URL) -> (files: [URL], failures: [ApplicationFailure]) {
        let keys: Set<URLResourceKey> = [.isRegularFileKey, .contentModificationDateKey]
        let unlisted: Mutex<[ApplicationFailure]> = Mutex([])
        let entries: [URL] =
            FileManager.default.enumerator(at: directory, includingPropertiesForKeys: Array(keys), options: [.skipsHiddenFiles]) { url, error in
                if case .failure(let error) = Result(catching: { try ifPresent { () throws -> Void in throw error } }) {
                    unlisted.withLock { failures in failures.append(ApplicationFailure(subject: url.path(percentEncoded: false), reason: .host(error))) }
                }
                return true
            }?
            .compactMap { entry in (entry as? URL).flatMap { url in url.pathExtension == "ips" ? url : nil } } ?? []
        let read: [(url: URL, values: Result<URLResourceValues, any Error>)] = entries.map { url in (url: url, values: Result { try url.resourceValues(forKeys: keys) }) }
        let files: [(url: URL, date: Date?)] = read.compactMap { entry in
            entry.values[case: \.success].flatMap { values in values.isRegularFile == true ? (url: entry.url, date: values.contentModificationDate) : nil }
        }
        let failures: [ApplicationFailure] = read.compactMap { entry in
            entry.values[case: \.failure].map { error in ApplicationFailure(subject: entry.url.path(percentEncoded: false), reason: .host(error)) }
        }
        return (files.sorted(using: KeyPathComparator(\.date, order: .reverse)).map(\.url), unlisted.withLock(\.self) + failures)
    }

    private static func read(_ file: URL) -> Result<Self, ApplicationFailure>? {
        switch Result(catching: { try ifPresent { try Self(Data(contentsOf: file), at: file) }.flatMap(\.self) }) {
            case .success(.some(let report)): .success(report)
            case .success(.none): nil
            case .failure(let error): .failure(ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error)))
        }
    }
}
