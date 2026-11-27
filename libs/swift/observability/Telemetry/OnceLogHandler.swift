import Foundation
package import Logging

import struct os.OSAllocatedUnfairLock

// --- [MODELS] --------------------------------------------------------------------------

private enum Diagnostic: Hashable, Sendable {
    case error(domain: String, code: Int, underlying: [Self])
    case message(String, metadata: [String: String])

    init(_ error: NSError) {
        self = .error(domain: error.domain, code: error.code, underlying: error.underlyingErrors.map { cause in Self(cause as NSError) })
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

package struct OnceLogHandler: LogHandler {
    // --- [STATE]

    package var logLevel: Logger.Level = .trace
    private var handler: any LogHandler
    private let occurrences = OSAllocatedUnfairLock(initialState: [Diagnostic: Int]())

    package init(_ handler: any LogHandler) { self.handler = handler }

    package var metadata: Logger.Metadata {
        get { handler.metadata }
        set { handler.metadata = newValue }
    }
    package var metadataProvider: Logger.MetadataProvider? {
        get { handler.metadataProvider }
        set { handler.metadataProvider = newValue }
    }

    package subscript(metadataKey key: String) -> Logger.Metadata.Value? {
        get { handler[metadataKey: key] }
        set { handler[metadataKey: key] = newValue }
    }

    // --- [RECORDS]

    package func log(event: LogEvent) {
        guard handler.logLevel <= event.level else { return }
        let diagnostic = event.error.map { error in Diagnostic(error as NSError) } ?? .message(event.message.description, metadata: metadata.mapValues(\.description))
        let count = occurrences.withLock { occurrences in
            occurrences[diagnostic, default: 0] += 1
            return occurrences[diagnostic, default: 0]
        }
        guard count.nonzeroBitCount == 1 else { return }
        var passed = event
        if count > 1 { passed.metadata = (event.metadata ?? [:]).merging(["occurrences": .stringConvertible(count)]) { _, counted in counted } }
        handler.log(event: passed)
    }
}
