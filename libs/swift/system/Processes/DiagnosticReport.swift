import AsyncAlgorithms
import CasePathsCore
import Files
package import Foundation
import Results
import System
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

package struct DiagnosticReport: Encodable, Hashable, Sendable {
    package enum Kind: String, Encodable, Sendable {
        case crash
        case hang
        case diag
        case spin
    }

    package struct Exception: Codable, Hashable, Sendable {
        package let type: String
        let signal: String?
        let subtype: String?
        let message: String?
        let rawCodes: [UInt64]?
    }

    struct Termination: Codable, Hashable, Sendable {
        let namespace: String
        let code: UInt64?
        let indicator: String?
        let byProc: String?
        let reasons: [String]?
        let details: [String]?
    }

    struct Frame: Encodable, Hashable, Sendable {
        let image: String?
        let imageOffset: UInt64
        let symbol: String?
        let symbolLocation: UInt64?
    }

    private struct Header: Decodable {
        enum CodingKeys: String, CodingKey {
            case bugType = "bug_type"
        }

        let bugType: String
    }

    private struct Body: Decodable {
        struct Thread: Decodable {
            enum CodingKeys: CodingKey {
                case triggered
                case frames
            }

            struct Frame: Decodable {
                let imageIndex: Int
                let imageOffset: UInt64
                let symbol: String?
                let symbolLocation: UInt64?
            }

            let frames: [Frame]?

            init(from decoder: any Decoder) throws {
                let fields = try decoder.container(keyedBy: CodingKeys.self)
                frames = try fields.decodeIfPresent(Bool.self, forKey: .triggered) == true ? fields.decode([Frame].self, forKey: .frames) : nil
            }
        }

        struct Image: Decodable {
            let name: String?
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
        let asi: [String: [String]]?
        let vmRegionInfo: String?
        let threads: [Thread]?
        let usedImages: [Image]?
    }

    package static let directories: [URL] = FileManager.default.urls(for: .libraryDirectory, in: [.userDomainMask, .localDomainMask]).map { library in
        library.appending(components: "Logs", "DiagnosticReports", directoryHint: .isDirectory)
    }

    private static let analyticsReport = UTType(importedAs: "com.apple.ips")
    private static let decoder = JSONDecoder()
    private static let keys: Set<URLResourceKey> = [.contentModificationDateKey, .isRegularFileKey, .isHiddenKey]
    private static let dateTime = Date.ParseStrategy(
        format: """
            \(year: .defaultDigits)-\(month: .twoDigits)-\(day: .twoDigits) \
            \(hour: .twoDigits(clock: .twentyFourHour, hourCycle: .zeroBased)):\(minute: .twoDigits):\(second: .twoDigits).\(secondFraction: .fractional(4)) \
            \(timeZone: .iso8601(.short))
            """,
        timeZone: .gmt,
    )

    package let url: URL
    package let kind: Kind
    let date: Date
    package let pid: pid_t
    let launched: Date?
    let name: String?
    let executable: String?
    let coalition: String?
    let bundleIdentifier: String?
    let event: String?
    package let exception: Exception?
    let termination: Termination?
    let applicationSpecificInformation: [String: [String]]?
    let virtualMemoryRegionInfo: String?
    let faultingThread: [Frame]?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension DiagnosticReport {
    init?(_ file: URL) throws(ProcessFailure) {
        let subject = file.path(percentEncoded: false)
        let read: Data?
        do { read = try ifPresent { try Data(contentsOf: file, options: .alwaysMapped) } } catch { throw ProcessFailure(subject: subject, error) }
        guard let data = read else { return nil }
        do {
            switch (UTType(filenameExtension: file.pathExtension) == Self.analyticsReport, Kind(rawValue: file.pathExtension)) {
                case (true, _):
                    let end = data.firstIndex(of: UInt8(ascii: "\n")) ?? data.endIndex
                    guard try Self.decoder.decode(Header.self, from: data[..<end]).bugType == "309" else { return nil }
                    let body = try Self.decoder.decode(Body.self, from: data[end...].dropFirst())
                    guard (body.isSimulated ?? 0) == 0 else { return nil }
                    let images = body.usedImages ?? []
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
                        exception: body.exception,
                        termination: body.termination,
                        applicationSpecificInformation: body.asi,
                        virtualMemoryRegionInfo: body.vmRegionInfo,
                        faultingThread: body.threads?.lazy.compactMap(\.frames).first?.map { frame in
                            Frame(
                                image: images.indices.contains(frame.imageIndex) ? images[frame.imageIndex].name : nil,
                                imageOffset: frame.imageOffset,
                                symbol: frame.symbol,
                                symbolLocation: frame.symbolLocation,
                            )
                        },
                    )
                case (false, let kind?) where kind != .crash:
                    let lines = String(decoding: data.prefix(1 << 14), as: UTF8.self).matches(of: /^([^:\n]+):[ \t]*(.*)$/.anchorsMatchLineEndings())
                    let fields: [String: String] = Dictionary(lines.map { line -> (String, String) in (String(line.output.1), String(line.output.2)) }, uniquingKeysWith: { first, _ in first })
                    guard let pid = fields["PID"].flatMap({ pid in pid_t(pid) }), let captured = fields["Date/Time"] else { throw CocoaError(.coderValueNotFound) }
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
                        termination: nil,
                        applicationSpecificInformation: nil,
                        virtualMemoryRegionInfo: nil,
                        faultingThread: nil,
                    )
                case (false, _): return nil
            }
        } catch {
            throw ProcessFailure(subject: subject, reason: .corrupt(error as NSError))
        }
    }

    package func isWritten(by process: ProcessInstance) -> Bool {
        pid == process.pid && (launched.map { launched in UInt64((launched.timeIntervalSince1970 * 1000).rounded()) == process.started / 1000 } ?? true)
    }

    package func isWritten(by bundle: Bundle) -> Bool {
        guard let reported = executable.map({ path in FilePath(path) }), let path = bundle.executablePath.map({ path in FilePath(path) }) else { return false }
        return switch reported.components.firstIndex(of: "*") {
            case .some(let marker): bundleIdentifier == bundle.bundleIdentifier && path.ends(with: FilePath(root: nil, reported.components[marker...].dropFirst()))
            case .none: reported == path
        }
    }

    package static func reports(
        since earliest: Date = .distantPast,
        limit: Int = .max,
        in directories: [URL] = directories,
        where matching: (Self) -> Bool,
    ) -> (reports: [Self], failures: [ProcessFailure]) {
        let listed = directories.map { directory in list(directory, since: earliest) }
        let found = listed.flatMap(\.files).reduce(into: (reports: [Self](), failures: listed.flatMap(\.failures))) { found, file in
            switch read(file, since: earliest) {
                case .success(let report) where matching(report): found.reports.append(report)
                case .failure(let failure): found.failures.append(failure)
                case .success, nil: break
            }
        }
        return (Array(found.reports.sorted(using: KeyPathComparator(\.date, order: .reverse)).prefix(limit)), found.failures)
    }

    package static func written(since earliest: Date, in directory: URL = directories[0]) -> some AsyncSequence<Result<Self, ProcessFailure>, Never> & Sendable {
        let initial: (read: Set<URL>, found: [Result<Self, ProcessFailure>]) = ([], [])
        return FileEvent.events(of: directory).reductions(into: initial) { held, change in
            let listed: (files: [URL], failures: [ProcessFailure]) =
                switch change {
                    case .success(let batch) where batch.isEmpty || batch.contains(where: \.mustScanSubDirs): list(directory, since: earliest)
                    case .success(let batch): modified(Set(batch.map(\.url)), since: earliest)
                    case .failure(let failure): ([], [ProcessFailure(subject: failure.subject, failure)])
                }
            let reads: [(url: URL, report: Result<Self, ProcessFailure>?)] = listed.files.filter { file in !held.read.contains(file) }.map { file in (file, read(file, since: earliest)) }
            held.read.formUnion(reads.filter { entry in entry.report?[case: \.failure] == nil }.map(\.url))
            held.found = listed.failures.map(Result<Self, ProcessFailure>.failure) + reads.compactMap(\.report)
        }
        .flatMap(\.found.async)
    }

    package static func first(
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
            throw (Task.isCancelled ? nil : ended) ?? ProcessFailure(subject: subject, reason: .cancelled)
        }
    }

    private static func list(_ directory: URL, since earliest: Date) -> (files: [URL], failures: [ProcessFailure]) {
        do {
            return modified(try ifPresent { try FileManager.default.contentsOfDirectory(at: directory, includingPropertiesForKeys: Array(keys)) } ?? [], since: earliest)
        } catch {
            return ([], [ProcessFailure(subject: directory.path(percentEncoded: false), error)])
        }
    }

    private static func modified(_ entries: some Sequence<URL>, since earliest: Date) -> (files: [URL], failures: [ProcessFailure]) {
        entries.reduce(into: (files: [URL](), failures: [ProcessFailure]())) { found, url in
            do {
                let values = try ifPresent { try url.resourceValues(forKeys: keys) }
                if values?.isRegularFile == true, values?.isHidden == false, let modified = values?.contentModificationDate, modified >= earliest { found.files.append(url) }
            } catch {
                found.failures.append(ProcessFailure(subject: url.path(percentEncoded: false), error))
            }
        }
    }

    private static func read(_ file: URL, since earliest: Date) -> Result<Self, ProcessFailure>? {
        do throws(ProcessFailure) {
            return try Self(file).flatMap { report in report.date >= earliest ? .success(report) : nil }
        } catch {
            return .failure(error)
        }
    }
}
