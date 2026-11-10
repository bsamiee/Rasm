import Encoding
public import Files
import Foundation
public import Logging
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
                case .stringConvertible(let flag as Bool): try attribute.encode(flag)
                case .stringConvertible(let integer as any BinaryInteger & Encodable): try attribute.encode(integer)
                case .stringConvertible(let number as any BinaryFloatingPoint & Encodable): try attribute.encode(number)
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

    private static let encoder: JSONEncoder = {
        let encoder = JSONEncoder()
        encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
        return encoder
    }()

    public var metadata: Logger.Metadata = [:]
    public var metadataProvider: Logger.MetadataProvider?
    private var level: Logger.Level?
    private let scope: String
    private let file: RotatingFile?
    private let export: any LogHandler?
    private let terminal: any LogHandler?
    private let filter: LogFilter
    private let resource: Logger.Metadata
    private let trace: (id: String, span: String, flags: String)

    public init(
        label: String,
        to sinks: (file: RotatingFile?, export: any LogHandler?, terminal: any LogHandler?),
        filter: LogFilter,
        resource: Logger.Metadata,
        trace: (id: String, span: String, flags: String),
        metadataProvider: Logger.MetadataProvider? = nil,
    ) {
        (scope, (file, export, terminal), self.filter, self.resource, self.trace, self.metadataProvider) = (label, sinks, filter, resource, trace, metadataProvider)
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
        let second = Date(timeIntervalSince1970: TimeInterval(instant / 1_000_000_000)).formatted(.iso8601)
        let location: Logger.Metadata =
            event.level >= .warning
            ? [
                "code.file.path": .attributed(event.file, attributes: [Privacy.public]), "code.function.name": .attributed(event.function, attributes: [Privacy.public]),
                "code.line.number": .stringConvertible(event.line),
            ] : [:]
        let exception: Logger.Metadata = event.error.map { error in Self.exception(error, stack: event.level >= .error) } ?? [:]
        var attributes = [location, exception].reduce(into: attributes(of: event)) { merged, next in merged.merge(next) { _, last in last } }
        let (traceID, spanID, traceFlags) = (attributes.removeValue(forKey: trace.id), attributes.removeValue(forKey: trace.span), attributes.removeValue(forKey: trace.flags))
        let record = LogRecord(
            timestamp: "\(second.dropLast()).\(String(1_000_000 + instant / 1000 % 1_000_000).dropFirst())Z",
            severityText: event.level.severity.text,
            severityNumber: event.level.severity.number,
            body: event.message.description,
            scope: scope,
            traceID: traceID.map(LogRecord.Attribute.init),
            spanID: spanID.map(LogRecord.Attribute.init),
            traceFlags: traceFlags.map(LogRecord.Attribute.init),
            attributes: attributes.isEmpty ? nil : attributes.mapValues(LogRecord.Attribute.init),
            resource: resource.mapValues(LogRecord.Attribute.init),
        )
        if let terminal, terminal.logLevel <= event.level { terminal.log(event: event) }
        if let export {
            var exported = event
            exported.metadata = attributes.mapValues { value in Privacy.of(value) == .public ? value : "<private>" }
            export.log(event: exported)
        }
        let reported = file?.failure != nil
        do {
            let line = try Self.encoder.encodeLine(record)
            if let file { try file.append(line) } else { try FileDescriptor.standardError.writeAll(line) }
        } catch {
            guard !reported else { return }
            StreamLogHandler.standardError(label: scope).log(
                event: LogEvent(level: .error, message: "Log record append failed", error: error, metadata: nil, source: event.source, file: event.file, function: event.function, line: event.line)
            )
        }
    }

    private static func exception(_ error: any Error, stack captured: Bool) -> Logger.Metadata {
        let causes = sequence(first: (error as NSError).underlyingErrors) { errors in
            let next = errors.flatMap { cause in (cause as NSError).underlyingErrors }
            return next.isEmpty ? nil : next
        }
        .joined().map { cause in "Caused by \((cause as NSError).domain) \((cause as NSError).code): \(cause.localizedDescription)" }
        let frames = (captured ? try? Backtrace.capture(images: ImageMap.capture()) : nil).map { backtrace in
            let images = backtrace.images.map { images in
                Set(backtrace.frames.compactMap { frame in images.indexOfImage(at: frame.adjustedProgramCounter) }).sorted().map { index in images[index].description }
            }
            return backtrace.frames.map(\.description) + (images ?? [])
        }
        let stack = causes + (frames ?? [])
        let message = error is any LocalizedError ? error.localizedDescription : String(describing: error)
        return [
            "exception.type": .attributed(String(reflecting: type(of: error)), attributes: [Privacy.public]),
            "exception.message": .attributed(message, attributes: [Privacy.public]),
            "exception.stacktrace": stack.isEmpty ? nil : .attributed(stack.joined(separator: "\n"), attributes: [Privacy.public]),
        ]
        .compactMapValues(\.self)
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
