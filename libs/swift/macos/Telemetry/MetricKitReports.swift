import Foundation
public import Logging
public import MetricKit
public import ServiceLifecycle

// --- [SERVICES] ------------------------------------------------------------------------

public struct MetricKitReports: Service {
    private let manager: MetricManager
    private let logger: Logging.Logger

    public init(to logger: Logging.Logger, from manager: MetricManager = .init()) {
        (self.manager, self.logger) = (manager, logger)
    }

    public func run() async {
        await cancelWhenGracefulShutdown {
            async let diagnostics: Void = log(manager.diagnosticReports)
            for await report in manager.metricReports { log(.notice, "MetricKit metric report", [:], whole: report) }
            await diagnostics
        }
    }

    private func log(_ reports: some AsyncSequence<DiagnosticReport, Never>) async {
        for await report in reports {
            let (level, message): (Logging.Logger.Level, Logging.Logger.Message) =
                switch report.result {
                    case .crash: (.critical, "MetricKit crash report")
                    case .hang: (.error, "MetricKit hang report")
                    case .cpuException: (.error, "MetricKit CPU exception report")
                    case .diskWriteException: (.error, "MetricKit disk write exception report")
                    case .appLaunch: (.notice, "MetricKit launch report")
                    @unknown default: (.notice, "MetricKit diagnostic report")
                }
            let fields: [String: Logging.Logger.MetadataValue?] = [
                "service.name": .attributed(report.environment.bundleIdentifier, attributes: [Privacy.public]),
                "service.version": .attributed(report.environment.applicationVersion, attributes: [Privacy.public]),
                "process.pid": report.environment.pid.map(Logging.Logger.MetadataValue.stringConvertible),
            ]
            log(level, message, fields.compactMapValues(\.self), whole: level == .critical ? report : nil)
        }
    }

    private func log(_ level: Logging.Logger.Level, _ message: Logging.Logger.Message, _ fields: Logging.Logger.Metadata, whole report: some Encodable?) {
        do {
            let text = try report.flatMap { report in String(bytes: try JSONEncoder().encode(report), encoding: .utf8) }.map(Logging.Logger.MetadataValue.string)
            logger.log(level: level, message, metadata: fields.merging(["report": text].compactMapValues(\.self)) { _, report in report })
        } catch {
            logger.error("MetricKit report encoding failed", error: error, metadata: fields)
        }
    }
}
