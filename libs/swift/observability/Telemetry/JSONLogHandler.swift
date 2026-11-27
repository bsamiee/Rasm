import Encoding
package import Files
import Foundation
package import Logging
package import OTel
import OTelSemanticConventions
import Runtime
import System
import os

// --- [MODELS] --------------------------------------------------------------------------

private struct LogRecord: Encodable {
    private enum CodingKeys: String, CodingKey {
        case timestamp, body, scope, attributes, resource
        case severityText = "severity_text"
        case severityNumber = "severity_number"
        case traceID = "trace_id"
        case spanID = "span_id"
        case traceFlags = "trace_flags"
    }

    struct Attribute: Encodable {
        let value: Logging.Logger.MetadataValue

        func encode(to encoder: any Encoder) throws {
            var attribute = encoder.singleValueContainer()
            switch value {
                case .string(let text): try attribute.encode(text)
                case .stringConvertible(let scalar as any Encodable) where scalar is Bool || scalar is any BinaryInteger || scalar is any BinaryFloatingPoint: try attribute.encode(scalar)
                case .stringConvertible(let described): try attribute.encode(described.description)
                case .dictionary(let fields): try attribute.encode(fields.mapValues(Self.init))
                case .array(let elements): try attribute.encode(elements.map(Self.init))
            }
        }
    }

    let timestamp: Date
    let severityText: String
    let severityNumber: Int
    let body: String
    let scope: String
    let traceID: Attribute?
    let spanID: Attribute?
    let traceFlags: Attribute?
    let attributes: [String: Attribute]?
    let resource: [String: Attribute]
}

// --- [SERVICES] ------------------------------------------------------------------------

package struct JSONLogHandler: LogHandler {
    // --- [STATE]

    package var metadata: Logging.Logger.Metadata = [:] { didSet { terminal?.metadata = metadata } }
    package var metadataProvider: Logging.Logger.MetadataProvider?
    private var level: Logging.Logger.Level?
    private var terminal: any LogHandler?
    private let scope: String
    private let file: RotatingFile?
    private let export: any LogHandler?
    private let unified: os.Logger
    private let filter: LogFilter
    private let resource: [String: LogRecord.Attribute]
    private let trace: OTel.Configuration.LoggingMetadataProviderConfiguration
    private let encoder = JSONEncoder.jsonLines

    package init(
        label: String,
        to sinks: (file: RotatingFile?, export: any LogHandler?, terminal: any LogHandler?),
        filter: LogFilter,
        resource: Logging.Logger.Metadata,
        trace: OTel.Configuration.LoggingMetadataProviderConfiguration,
        metadataProvider: Logging.Logger.MetadataProvider? = nil,
    ) {
        (scope, (file, export, terminal), self.filter, self.trace, self.metadataProvider) = (label, sinks, filter, trace, metadataProvider)
        self.resource = resource.mapValues(LogRecord.Attribute.init)
        unified = resource[OTelAttribute.service.name].map { service in os.Logger(subsystem: service.description, category: label) } ?? os.Logger()
    }

    package var logLevel: Logging.Logger.Level {
        get {
            let recorded = level ?? filter.level(for: scope)
            return Logging.Logger.Level.allCases.first { candidate in candidate >= recorded || unified.isEnabled(type: candidate.logType) } ?? recorded
        }
        set { level = newValue }
    }

    package subscript(metadataKey key: String) -> Logging.Logger.Metadata.Value? {
        get { metadata[key] }
        set { metadata[key] = newValue }
    }

    // --- [RECORDS]

    package func log(event: LogEvent) {
        let now = Date.now
        var attributes = [metadataProvider?.get() ?? [:], event.metadata ?? [:], event.attributes].reduce(into: metadata) { merged, next in merged.merge(next) { _, last in last } }
        if unified.isEnabled(type: event.level.logType) {
            let fields = Dictionary(grouping: attributes.sorted { first, second in first.key < second.key }) { field in Privacy.of(field.value) == .public }
                .mapValues { marked in marked.map { field in " \(field.key)=\(field.value)" }.joined() }
            unified.log(level: event.level.logType, "\(event.message.description, privacy: .public)\(fields[true] ?? "", privacy: .public)\(fields[false] ?? "", privacy: .private(mask: .hash))")
        }
        guard (level ?? filter.level(for: scope)) <= event.level else { return }
        let record = LogRecord(
            timestamp: now,
            severityText: event.level.severity.text,
            severityNumber: event.level.severity.number,
            body: event.message.description,
            scope: scope,
            traceID: attributes.removeValue(forKey: trace.traceIDKey).map(LogRecord.Attribute.init),
            spanID: attributes.removeValue(forKey: trace.spanIDKey).map(LogRecord.Attribute.init),
            traceFlags: attributes.removeValue(forKey: trace.traceFlagsKey).map(LogRecord.Attribute.init),
            attributes: attributes.isEmpty ? nil : attributes.mapValues(LogRecord.Attribute.init),
            resource: resource,
        )
        if let terminal, terminal.logLevel <= event.level { terminal.log(event: event) }
        if let export {
            var exported = event
            exported.metadata = attributes.mapValues { value in Privacy.of(value) == .public ? value : "<private>" }
            export.log(event: exported)
        }
        let reported = file?.failure != nil
        do {
            let line = try encoder.encodeLine(record)
            if let file { try file.append(line, at: now) } else { try FileDescriptor.standardError.writeAll(line) }
        } catch {
            guard !reported else { return }
            StreamLogHandler.standardError(label: scope).log(
                event: LogEvent(level: .error, message: "Log record append failed", error: error, metadata: nil, source: event.source, file: event.file, function: event.function, line: event.line)
            )
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Logging.Logger.Level {
    package var severity: (text: String, number: Int) {
        switch self {
            case .trace: ("TRACE", 1)
            case .debug: ("DEBUG", 5)
            case .info: ("INFO", 9)
            case .notice: ("INFO", 10)
            case .warning: ("WARN", 13)
            case .error: ("ERROR", 17)
            case .critical: ("FATAL", 21)
        }
    }

    var logType: OSLogType {
        switch self {
            case .trace, .debug: .debug
            case .info: .info
            case .notice: .default
            case .warning, .error: .error
            case .critical: .fault
        }
    }
}

extension LogEvent {
    var attributes: Logging.Logger.Metadata {
        let location: Logging.Logger.Metadata =
            level >= .warning
            ? [
                OTelAttribute.code.file.path: .attributed(file, attributes: [Privacy.public]), OTelAttribute.code.function.name: .attributed(function, attributes: [Privacy.public]),
                OTelAttribute.code.line.number: .stringConvertible(line),
            ] : [:]
        guard let error else { return location }
        let causes = sequence(first: (error as NSError).underlyingErrors) { errors in errors.isEmpty ? nil : errors.flatMap { cause in (cause as NSError).underlyingErrors } }
            .joined().map { cause in "Caused by \((cause as NSError).domain) \((cause as NSError).code): \(cause.localizedDescription)" }
        let stack: [String] =
            switch level >= .error ? Result(catching: { try Backtrace.capture(images: ImageMap.capture()) }) : nil {
                case .success(let backtrace):
                    causes + backtrace.frames.map(\.description)
                        + (backtrace.images.map { images in
                            images[RangeSet(backtrace.frames.compactMap { frame in images.indexOfImage(at: frame.adjustedProgramCounter) }, within: images)].map(\.description)
                        } ?? [])
                case .failure(let failure): causes + ["Backtrace capture failed: \(failure)"]
                case nil: causes
            }
        let exception: [String: Logging.Logger.MetadataValue?] = [
            OTelAttribute.exception.type: .attributed(String(reflecting: type(of: error)), attributes: [Privacy.public]),
            OTelAttribute.exception.message: .attributed(error is any LocalizedError ? error.localizedDescription : String(describing: error), attributes: [Privacy.public]),
            OTelAttribute.exception.stacktrace: stack.isEmpty ? nil : .attributed(stack.joined(separator: "\n"), attributes: [Privacy.public]),
        ]
        return location.merging(exception.compactMapValues(\.self)) { _, stated in stated }
    }
}
