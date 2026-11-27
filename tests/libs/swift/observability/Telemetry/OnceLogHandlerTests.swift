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
    func countsEachCauseByErrorOrMessageAndLeavesACauseTheWrappedHandlerRefusedUncounted() {
        let store = InMemoryLogHandler()
        let logger = Logger(label: String(reflecting: Self.self)) { _ in OnceLogHandler(store) }
        logger.debug("flush failed")
        logger.warning("flush failed", metadata: ["attempt": "1"])
        logger.warning("flush failed", metadata: ["attempt": "2"])
        logger.error("export failed", error: CancellationError())
        logger.error("export failed again", error: CancellationError())
        logger.error("export failed", error: URLError(.timedOut))
        expectNoDifference(
            store.entries.map { entry in "\(entry.level) \(entry.message)" },
            ["warning flush failed", "warning flush failed", "error export failed", "error export failed again", "error export failed"],
        )
        expectNoDifference(store.entries.map { entry in entry.error.map { error in (error as NSError).domain } }, [nil, nil, "Swift.CancellationError", "Swift.CancellationError", NSURLErrorDomain])
        expectNoDifference(store.entries.map { entry in entry.metadata["occurrences"]?.description }, [nil, "2", nil, "2", nil])
    }

    @Test
    func passesARepeatedCauseOnItsFirstSecondFourthAndEighthOccurrence() {
        let store = InMemoryLogHandler()
        let logger = Logger(label: String(reflecting: Self.self)) { _ in OnceLogHandler(store) }
        for _ in 1...9 { logger.error("export failed", error: URLError(.timedOut)) }
        expectNoDifference(store.entries.map { entry in entry.metadata["occurrences"]?.description }, [nil, "2", "4", "8"])
    }

    @Test
    func countsAnErrorApartFromOneOfTheSameDomainAndCodeWithAnotherUnderlyingCause() {
        let store = InMemoryLogHandler()
        let logger = Logger(label: String(reflecting: Self.self)) { _ in OnceLogHandler(store) }
        let timedOut = NSError(domain: "Export", code: 7, userInfo: [NSUnderlyingErrorKey: URLError(.timedOut)])
        let unresolved = NSError(domain: "Export", code: 7, userInfo: [NSUnderlyingErrorKey: URLError(.cannotFindHost)])
        for error in [timedOut, unresolved, timedOut] { logger.error("export failed", error: error) }
        expectNoDifference(store.entries.map { entry in entry.metadata["occurrences"]?.description }, [nil, nil, "2"])
    }
}
