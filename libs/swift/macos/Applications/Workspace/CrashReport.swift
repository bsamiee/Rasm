import AsyncAlgorithms
import CasePaths
import Files
public import Foundation
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

    private static let directory: URL = URL.libraryDirectory.appending(components: "Logs", "DiagnosticReports")

    public let url: URL
    public let pid: pid_t
    public let bundleIdentifier: String?
    public let exception: String?

    init?(contentsOf url: URL) throws {
        let data: Data = try Data(contentsOf: url)
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

    public static func reports(for identifier: String) -> (reports: [Self], failures: [ApplicationFailure]) {
        let keys: Set<URLResourceKey> = [.isRegularFileKey, .contentModificationDateKey]
        let unlisted: Mutex<[ApplicationFailure]> = Mutex([])
        let files: [URL] =
            FileManager.default.enumerator(at: directory, includingPropertiesForKeys: Array(keys), options: [.skipsHiddenFiles]) { url, error in
                if case .failure(let error) = Result(catching: { try ifPresent { () throws -> Void in throw error } }) {
                    unlisted.withLock { failures in failures.append(ApplicationFailure(subject: url.path(percentEncoded: false), reason: .host(error))) }
                }
                return true
            }?
            .compactMap { entry in (entry as? URL).flatMap { url in url.pathExtension == "ips" ? url : nil } } ?? []
        let reads: [Result<Self, ApplicationFailure>] = files.compactMap { url -> (read: Result<Self, ApplicationFailure>, date: Date?)? in
            switch Result(catching: { try url.resourceValues(forKeys: keys) }) {
                case .success(let values) where values.isRegularFile == true: read(url).map { read in (read: read, date: values.contentModificationDate) }
                case .success: nil
                case .failure(let error): (read: .failure(ApplicationFailure(subject: url.path(percentEncoded: false), reason: .host(error))), date: nil)
            }
        }
        .sorted(using: KeyPathComparator(\.date, order: .reverse)).map(\.read)
        let failures: [ApplicationFailure] = unlisted.withLock(\.self) + reads.compactMap { read in read[case: \.failure] }
        return (reads.compactMap { read in read[case: \.success] }.filter { report in report.bundleIdentifier == identifier }, failures)
    }

    public static func following() -> some AsyncSequence<Result<Self, ApplicationFailure>, Never> {
        FileEvent.events(of: directory).flatMap { change -> AsyncSyncSequence<[Result<Self, ApplicationFailure>]> in
            switch change {
                case .success(let batch): batch.map(\.url).filter { url in url.pathExtension == "ips" }.compactMap(read).async
                case .failure(let failure): [.failure(ApplicationFailure(subject: failure.subject, reason: .unwatched(failure)))].async
            }
        }
    }

    private static func read(_ file: URL) -> Result<Self, ApplicationFailure>? {
        switch Result(catching: { try ifPresent { try Self(contentsOf: file) }.flatMap(\.self) }) {
            case .success(.some(let report)): .success(report)
            case .success(.none): nil
            case .failure(let error): .failure(ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error)))
        }
    }
}
