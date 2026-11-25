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
                return (level, message, report.environment.bundleIdentifier, report.environment.applicationVersion, report.environment.pid, level == .critical)
            }
            await log(manager.metricReports) { report in (.notice, "MetricKit metric report", report.environment?.bundleIdentifier, report.environment?.latestApplicationVersion, nil, true) }
            await diagnostics
        }
    }

    private func log<Report: Encodable>(_ reports: some AsyncSequence<Report, Never>, as entry: (Report) -> (Logging.Logger.Level, Logging.Logger.Message, String?, String?, pid_t?, Bool)) async {
        for await report in reports {
            let (level, message, service, version, pid, whole) = entry(report)
            let fields: [String: Logging.Logger.MetadataValue?] = [
                OTelAttribute.service.name: service.map { name in .attributed(name, attributes: [Privacy.public]) },
                OTelAttribute.service.version: version.map { stated in .attributed(stated, attributes: [Privacy.public]) },
                "process.pid": pid.map(Logging.Logger.MetadataValue.stringConvertible),
            ]
            switch Result(catching: { try whole ? String(bytes: JSONEncoder.lines.encode(report), encoding: .utf8) : nil }) {
                case .success(let text):
                    logger.log(level: level, message, metadata: fields.merging(["report": text.map(Logging.Logger.MetadataValue.string)]) { _, report in report }.compactMapValues(\.self))
                case .failure(let failure): logger.error("MetricKit report encoding failed", error: failure, metadata: fields.compactMapValues(\.self))
            }
        }
    }
}
