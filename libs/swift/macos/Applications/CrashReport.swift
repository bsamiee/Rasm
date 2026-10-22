import AsyncAlgorithms
public import Files
public import Foundation

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

    private static let logs: URL = URL.libraryDirectory.appending(path: "Logs")

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

    public static func reports(for identifier: String) -> (reports: [Self], failures: [CrashReportFailure]) {
        let keys: Set<URLResourceKey> = [.isDirectoryKey, .isRegularFileKey, .contentModificationDateKey]
        func walk(_ url: URL) -> [Result<(report: Self, date: Date?), CrashReportFailure>] {
            let read: Result<[Result<(report: Self, date: Date?), CrashReportFailure>], any Error> = ifPresent { try url.resourceValues(forKeys: keys) }.flatMap { values in
                switch (values?.isDirectory, values?.isRegularFile, url.pathExtension) {
                    case (true?, _, _):
                        ifPresent { try FileManager.default.contentsOfDirectory(at: url, includingPropertiesForKeys: Array(keys), options: [.skipsHiddenFiles]) }.map { entries in
                            (entries ?? []).flatMap(walk)
                        }
                    case (_, true?, "ips"): .success(read(url).map { read in read.map { report in (report, values?.contentModificationDate) } })
                    default: .success([])
                }
            }
            return switch read {
                case .success(let found): found
                case .failure(let error): [.failure(.unreadable(url, error))]
            }
        }
        let found: [Result<(report: Self, date: Date?), CrashReportFailure>] = walk(logs)
        return (
            found.compactMap { read in if case .success(let found) = read, found.report.bundleIdentifier == identifier { found } else { nil } }.sorted(
                using: KeyPathComparator(\.date, order: .reverse)
            ).map(\.report),
            found.compactMap { read in if case .failure(let failure) = read { failure } else { nil } },
        )
    }

    static func read(_ file: URL) -> [Result<Self, CrashReportFailure>] {
        switch ifPresent({ try Self(contentsOf: file) }) {
            case .success(let report): [report.flatMap(\.self)].compactMap { report in report.map(Result.success) }
            case .failure(let error): [.failure(.unreadable(file, error))]
        }
    }

    public static func following() -> some AsyncSequence<Result<Self, CrashReportFailure>, Never> {
        FileWatch.events(of: logs.appending(path: "DiagnosticReports")).flatMap { change -> AsyncSyncSequence<[Result<Self, CrashReportFailure>]> in
            switch change {
                case .success(let files): files.filter { file in file.pathExtension == "ips" }.flatMap(read).async
                case .failure(let error): [Result.failure(.unwatched(error))].async
            }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum CrashReportFailure: CustomNSError, LocalizedError {
    case unwatched(FileWatchFailure)
    case unreadable(URL, any Error)

    public var errorDescription: String? {
        switch self {
            case .unwatched(let error): error.localizedDescription
            case .unreadable(let url, let error): "\(url.path(percentEncoded: false)): \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .unwatched(let error as any Error), .unreadable(_, let error): [NSUnderlyingErrorKey: error]
        }
    }
}
