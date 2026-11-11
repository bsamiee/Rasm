public import Logging
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

public struct OnceLogHandler: LogHandler {
    private final class Causes: Sendable {
        let seen: Mutex<Set<String>> = .init([])
    }

    public var metadata: Logger.Metadata = [:]
    public var metadataProvider: Logger.MetadataProvider?
    public var logLevel: Logger.Level = .trace
    private let handler: any LogHandler
    private let causes: Causes = .init()

    public init(_ handler: any LogHandler) { self.handler = handler }

    public subscript(metadataKey key: String) -> Logger.Metadata.Value? {
        get { metadata[key] }
        set { metadata[key] = newValue }
    }

    public func log(event: LogEvent) {
        let cause = event.error.map { error in String(reflecting: type(of: error)) } ?? "\(event.message) \(metadata.sorted { first, second in first.key < second.key })"
        guard handler.logLevel <= event.level, causes.seen.withLock({ seen in seen.insert(cause).inserted }) else { return }
        var first = event
        first.metadata = attributes(of: event)
        handler.log(event: first)
    }
}
