public import Logging

import struct os.OSAllocatedUnfairLock

// --- [SERVICES] ------------------------------------------------------------------------

public struct OnceLogHandler: LogHandler {
    public var metadata: Logger.Metadata = [:] { didSet { handler.metadata = metadata } }
    public var metadataProvider: Logger.MetadataProvider? { didSet { handler.metadataProvider = metadataProvider } }
    public var logLevel: Logger.Level = .trace
    private var handler: any LogHandler
    private let seen = OSAllocatedUnfairLock(initialState: Set<String>())

    public init(_ handler: any LogHandler) { self.handler = handler }

    public subscript(metadataKey key: String) -> Logger.Metadata.Value? {
        get { metadata[key] }
        set { metadata[key] = newValue }
    }

    public func log(event: LogEvent) {
        let cause = event.error.map { error in String(reflecting: type(of: error)) } ?? "\(event.message) \(metadata.sorted { first, second in first.key < second.key })"
        guard handler.logLevel <= event.level, seen.withLock({ seen in seen.insert(cause).inserted }) else { return }
        handler.log(event: event)
    }
}
