public import Logging
public import os

// --- [SERVICES] ------------------------------------------------------------------------

public struct OSLogHandler: LogHandler {
    // --- [STATE]

    public var metadata: Logging.Logger.Metadata = [:]
    public var metadataProvider: Logging.Logger.MetadataProvider?
    public var logLevel: Logging.Logger.Level = .trace
    private let logger: os.Logger

    public init(logger: os.Logger, metadataProvider: Logging.Logger.MetadataProvider? = nil) {
        (self.logger, self.metadataProvider) = (logger, metadataProvider)
    }

    public subscript(metadataKey key: String) -> Logging.Logger.Metadata.Value? {
        get { metadata[key] }
        set { metadata[key] = newValue }
    }

    // --- [ENTRIES]

    public func log(event: LogEvent) {
        let type: OSLogType =
            switch event.level {
                case .trace, .debug: .debug
                case .info: .info
                case .notice: .default
                case .warning, .error: .error
                case .critical: .fault
            }
        guard logger.isEnabled(type: type) else { return }
        let exception: Logging.Logger.Metadata =
            event.error.map { error in
                ["exception.type": .attributed(String(reflecting: Swift.type(of: error)), attributes: [Privacy.public]), "exception.message": .string(String(describing: error))]
            } ?? [:]
        let fields = Dictionary(grouping: attributes(of: event).merging(exception) { _, stated in stated }.sorted { first, second in first.key < second.key }) { field in
            Privacy.of(field.value) == .public
        }
        .mapValues { marked in marked.map { field in "\(field.key)=\(field.value)" }.joined(separator: " ") }
        logger.log(level: type, "\(event.message.description, privacy: .public) \(fields[true] ?? "", privacy: .public) \(fields[false] ?? "", privacy: .private(mask: .hash))")
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension LogHandler {
    func attributes(of event: LogEvent) -> Logging.Logger.Metadata {
        [metadataProvider?.get() ?? [:], event.metadata ?? [:]].reduce(into: metadata) { merged, next in merged.merge(next) { _, last in last } }
    }
}
