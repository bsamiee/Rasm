import CustomDump
import Foundation
import InMemoryLogging
import Logging
import Telemetry
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct OnceLogHandlerTests {
    @Test
    func passesEachCauseOnceAndLeavesACauseTheWrappedHandlerRefusedUnseen() {
        let store = InMemoryLogHandler()
        let logger = Logger(label: String(reflecting: Self.self)) { _ in OnceLogHandler(store) }
        logger.debug("flush failed")
        logger.warning("flush failed", metadata: ["attempt": "1"])
        logger.warning("flush failed", metadata: ["attempt": "2"])
        logger.error("export failed", error: CancellationError())
        logger.error("export failed again", error: CancellationError())
        logger.error("export failed", error: URLError(.timedOut))
        expectNoDifference(store.entries.map { entry in "\(entry.level) \(entry.message)" }, ["warning flush failed", "error export failed", "error export failed"])
        expectNoDifference(store.entries.map { entry in entry.error.map { error in String(reflecting: type(of: error)) } }, [nil, "Swift.CancellationError", "Foundation.URLError"])
    }
}
