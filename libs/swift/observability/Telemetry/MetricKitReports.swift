import Encoding
import Foundation
public import Logging
public import MetricKit
import OTelSemanticConventions
public import ServiceLifecycle

// --- [SERVICES] ------------------------------------------------------------------------

public struct MetricKitReports: Service {
    private let manager = MetricManager()
    private let logger: Logging.Logger

    public init(to logger: Logging.Logger) { self.logger = logger }

    public func run() async {
        await cancelWhenGracefulShutdown {
            async let diagnostics: Void = log(manager.diagnosticReports) { report in
                let (level, message): (Logging.Logger.Level, Logging.Logger.Message) =
                    switch report.result {
                        case .crash: (.critical, "MetricKit crash report")
                        case .hang: (.error, "MetricKit hang report")
                        case .cpuException: (.error, "MetricKit CPU exception report")
                        case .diskWriteException: (.error, "MetricKit disk write exception report")
                        case .appLaunch: (.notice, "MetricKit launch report")
                        @unknown default: (.notice, "MetricKit diagnostic report")
                    }
                return (level, message, report.environment.bundleIdentifier, report.environment.applicationVersion, report.environment.pid)
            }
            await log(manager.metricReports) { report in (.notice, "MetricKit metric report", report.environment?.bundleIdentifier, report.environment?.latestApplicationVersion, nil) }
            await diagnostics
        }
    }

    private func log<Report: Encodable>(_ reports: some AsyncSequence<Report, Never>, as entry: (Report) -> (Logging.Logger.Level, Logging.Logger.Message, String?, String?, pid_t?)) async {
        for await report in reports {
            let (level, message, service, version, pid) = entry(report)
            let fields: Logging.Logger.Metadata = [
                OTelAttribute.service.name: service.map { name in "\(name, privacy: .public)" },
                OTelAttribute.service.version: version.map { stated in "\(stated, privacy: .public)" },
                OTelAttribute.process.pid: pid.map(Logging.Logger.MetadataValue.stringConvertible),
            ]
            .compactMapValues(\.self)
            switch Result(catching: { try JSONEncoder.jsonLines.encode(report) }) {
                case .success(let body): logger.log(level: level, message, metadata: fields.merging(["report": .string(String(decoding: body, as: UTF8.self))]) { _, body in body })
                case .failure(let failure): logger.error("MetricKit report encoding failed", error: failure, metadata: fields)
            }
        }
    }
}
