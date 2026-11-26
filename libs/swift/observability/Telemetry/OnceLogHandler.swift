public import Logging

import struct os.OSAllocatedUnfairLock

// --- [SERVICES] ------------------------------------------------------------------------

public struct OnceLogHandler: LogHandler {
    public var logLevel: Logger.Level = .trace
    private var handler: any LogHandler
    private let seen = OSAllocatedUnfairLock(initialState: Set<String>())

    public init(_ handler: any LogHandler) { self.handler = handler }

    public var metadata: Logger.Metadata {
        get { handler.metadata }
        set { handler.metadata = newValue }
    }
    public var metadataProvider: Logger.MetadataProvider? {
        get { handler.metadataProvider }
        set { handler.metadataProvider = newValue }
    }

    public subscript(metadataKey key: String) -> Logger.Metadata.Value? {
        get { handler[metadataKey: key] }
        set { handler[metadataKey: key] = newValue }
    }

    public func log(event: LogEvent) {
        let cause = event.error.map { error in String(reflecting: type(of: error)) } ?? "\(event.message) \(metadata.sorted { first, second in first.key < second.key })"
        guard handler.logLevel <= event.level, seen.withLock({ seen in seen.insert(cause).inserted }) else { return }
        handler.log(event: event)
    }
}
