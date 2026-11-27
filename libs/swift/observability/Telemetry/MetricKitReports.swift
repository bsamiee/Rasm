import Encoding
import Foundation
package import Logging
import MetricKit
import OTelSemanticConventions
package import ServiceLifecycle

// --- [SERVICES] ------------------------------------------------------------------------

package struct MetricKitReports: Service {
    private let manager = MetricManager()
    private let encoder = JSONEncoder.jsonLines
    private let logger: Logging.Logger

    package init(to logger: Logging.Logger) { self.logger = logger }

    package func run() async throws {
        await cancelWhenGracefulShutdown {
            async let diagnostics: Void = log(manager.diagnosticReports)
            await log(manager.metricReports) { report in (.notice, "Metrics reported by MetricKit", report.environment?.latestApplicationVersion, nil) }
            await diagnostics
        }
        try await gracefulShutdown()
    }

    func log(_ reports: some AsyncSequence<DiagnosticReport, Never>) async {
        for await report in reports {
            var fields: Logging.Logger.Metadata = [
                "time_range": "\(report.timeRange.start.formatted(.iso8601), privacy: .public)/\(report.timeRange.end.formatted(.iso8601), privacy: .public)",
                OTelAttribute.service.version: "\(report.environment.applicationVersion, privacy: .public)",
            ]
            fields[OTelAttribute.process.pid] = report.environment.pid.map(Logging.Logger.MetadataValue.stringConvertible)
            let entry: (level: Logging.Logger.Level, message: Logging.Logger.Message, tree: CallStackTree)
            switch report.result {
                case .crash: continue
                case .hang(let hang): entry = (.error, "Hang of \(hang.hangDuration.formatted()) reported by MetricKit", hang.callStackTree)
                case .cpuException(let exception):
                    let (cpu, sampled): (String, String) = (exception.totalCPUTime.formatted(), exception.totalSampledTime.formatted())
                    entry = (.error, "CPU exception reported by MetricKit: \(cpu) of CPU time over \(sampled) sampled", exception.callStackTree)
                case .diskWriteException(let exception):
                    entry = (.error, "Disk write exception reported by MetricKit: \(exception.totalBytesWritten.formatted(.byteCount(style: .file))) written", exception.callStackTree)
                case .appLaunch(let launch): entry = (.notice, "Launch of \(launch.launchDuration.formatted()) reported by MetricKit", launch.callStackTree)
                @unknown default:
                    logger.notice("Diagnostic of an unknown kind reported by MetricKit", metadata: fields)
                    continue
            }
            let heavier: (CallStackFrame, CallStackFrame) -> Bool = { first, second in (first.sampleCount ?? 0) < (second.sampleCount ?? 0) }
            let thread = entry.tree.callStackThreads.first { thread in thread.threadAttributed == true } ?? entry.tree.callStackThreads.first
            let frames = thread?.rootFrames.max(by: heavier).map { root in sequence(first: root) { frame in frame.subFrames.max(by: heavier) } }
            let stack = frames?.map { frame in
                let binary = frame.binaryName(from: entry.tree) ?? frame.binaryUUID?.uuidString ?? "unnamed binary"
                return frame.offsetIntoBinaryTextSegment.map { offset in "\(binary) + 0x\(String(offset, radix: 16))" } ?? binary
            }
            fields[OTelAttribute.code.stacktrace] = stack.map { lines in "\(lines.joined(separator: "\n"), privacy: .public)" }
            logger.log(level: entry.level, entry.message, metadata: fields)
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
