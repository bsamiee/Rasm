import Encoding
import Foundation
public import Logging
public import MetricKit
import OTelSemanticConventions
public import ServiceLifecycle

// --- [SERVICES] ------------------------------------------------------------------------

public struct MetricKitReports: Service {
    private let manager = MetricManager()
    private let encoder = JSONEncoder.jsonLines
    private let logger: Logging.Logger

    public init(to logger: Logging.Logger) { self.logger = logger }

    public func run() async throws {
        await cancelWhenGracefulShutdown {
            async let diagnostics: Void = log(manager.diagnosticReports)
            await log(manager.metricReports) { report in (.notice, "MetricKit metric report", report.environment?.latestApplicationVersion, nil) }
            await diagnostics
        }
        try await gracefulShutdown()
    }

    func log(_ reports: some AsyncSequence<DiagnosticReport, Never>) async {
        await log(reports) { report in
            let entry: (level: Logging.Logger.Level, message: Logging.Logger.Message)? =
                switch report.result {
                    case .crash: nil
                    case .hang: (.error, "MetricKit hang report")
                    case .cpuException: (.error, "MetricKit CPU exception report")
                    case .diskWriteException: (.error, "MetricKit disk write exception report")
                    case .appLaunch: (.notice, "MetricKit launch report")
                    @unknown default: (.notice, "MetricKit diagnostic report")
                }
            return entry.map { entry in (entry.level, entry.message, report.environment.applicationVersion, report.environment.pid) }
        }
    }

    func log<Report: Encodable>(
        _ reports: some AsyncSequence<Report, Never>,
        as entry: (Report) -> (level: Logging.Logger.Level, message: Logging.Logger.Message, version: String?, pid: pid_t?)?,
    ) async {
        for await report in reports {
            guard case (let level, let message, let version, let pid)? = entry(report) else { continue }
            let fields: Logging.Logger.Metadata = [
                OTelAttribute.service.version: version.map { stated in "\(stated, privacy: .public)" },
                OTelAttribute.process.pid: pid.map(Logging.Logger.MetadataValue.stringConvertible),
            ]
            .compactMapValues(\.self)
            switch Result(catching: { try encoder.encode(report) }) {
                case .success(let body): logger.log(level: level, message, metadata: fields.merging(["report": .string(String(decoding: body, as: UTF8.self))]) { _, body in body })
                case .failure(let failure): logger.log(level: level, message, error: failure, metadata: fields)
            }
        }
    }
}
