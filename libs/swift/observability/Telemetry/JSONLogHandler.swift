import Encoding
public import Files
import Foundation
public import Logging
import OTelSemanticConventions
import Runtime
import System
import Tracing

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
        let value: Logger.MetadataValue

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

    let timestamp: String
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

public struct JSONLogHandler: LogHandler {
    // --- [STATE]

    public var metadata: Logger.Metadata = [:] { didSet { terminal?.metadata = metadata } }
    public var metadataProvider: Logger.MetadataProvider?
    private var level: Logger.Level?
    private var terminal: any LogHandler?
    private let scope: String
    private let file: RotatingFile?
    private let export: any LogHandler?
    private let filter: LogFilter
    private let resource: [String: LogRecord.Attribute]
    private let trace: (id: String, span: String, flags: String)
    private let encoder = JSONEncoder.lines

    public init(
        label: String,
        to sinks: (file: RotatingFile?, export: any LogHandler?, terminal: any LogHandler?),
        filter: LogFilter,
        resource: Logger.Metadata,
        trace: (id: String, span: String, flags: String),
        metadataProvider: Logger.MetadataProvider? = nil,
    ) {
        (scope, (file, export, terminal), self.filter, self.trace, self.metadataProvider) = (label, sinks, filter, trace, metadataProvider)
        self.resource = resource.mapValues(LogRecord.Attribute.init)
    }

    public var logLevel: Logger.Level {
        get { level ?? filter.level(for: scope) }
        set { level = newValue }
    }

    public subscript(metadataKey key: String) -> Logger.Metadata.Value? {
        get { metadata[key] }
        set { metadata[key] = newValue }
    }

    // --- [RECORDS]

    public func log(event: LogEvent) {
        let instant = DefaultTracerClock.now.nanosecondsSinceEpoch
        var attributes = attributes(of: event)
        let record = LogRecord(
            timestamp: "\(Date(timeIntervalSince1970: TimeInterval(instant / 1_000_000_000)).formatted(.iso8601).dropLast()).\(String(1_000_000 + instant / 1000 % 1_000_000).dropFirst())Z",
            severityText: event.level.severity.text,
            severityNumber: event.level.severity.number,
            body: event.message.description,
            scope: scope,
            traceID: attributes.removeValue(forKey: trace.id).map(LogRecord.Attribute.init),
            spanID: attributes.removeValue(forKey: trace.span).map(LogRecord.Attribute.init),
            traceFlags: attributes.removeValue(forKey: trace.flags).map(LogRecord.Attribute.init),
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
            if let file { try file.append(line) } else { try FileDescriptor.standardError.writeAll(line) }
        } catch {
            guard !reported else { return }
            StreamLogHandler.standardError(label: scope).log(
                event: LogEvent(level: .error, message: "Log record append failed", error: error, metadata: nil, source: event.source, file: event.file, function: event.function, line: event.line)
            )
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Logger.Level {
    public var severity: (text: String, number: Int) {
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
}

extension LogEvent {
    var attributes: Logger.Metadata {
        let location: Logger.Metadata =
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
        let exception: [String: Logger.MetadataValue?] = [
            OTelAttribute.exception.type: .attributed(String(reflecting: type(of: error)), attributes: [Privacy.public]),
            OTelAttribute.exception.message: .attributed(error is any LocalizedError ? error.localizedDescription : String(describing: error), attributes: [Privacy.public]),
            OTelAttribute.exception.stacktrace: stack.isEmpty ? nil : .attributed(stack.joined(separator: "\n"), attributes: [Privacy.public]),
        ]
        return location.merging(exception.compactMapValues(\.self)) { _, stated in stated }
    }
}
