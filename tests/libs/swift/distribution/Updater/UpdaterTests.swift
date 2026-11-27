import InMemoryLogging
import Logging
import Testing
import Updater

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct UpdaterTests {
    @MainActor
    @Test
    func logsTheRefusalOfAHostBundleStatingNoFeedAsAnErrorWithItsCause() {
        let store = InMemoryLogHandler()
        _ = Updater(logger: Logger(label: String(reflecting: Self.self)) { _ in store })
        let refusals: [InMemoryLogHandler.Entry] = store.entries.filter { entry in entry.message == "Updater could not start" }
        let reported: Bool = refusals.map(\.level) == [.error] && refusals.allSatisfy { entry in entry.error != nil }
        let comment: Comment = "Updater started over a test bundle with no SUFeedURL and logged \(store.entries.map { entry in "\(entry.level) \(entry.message)" })"
        #expect(reported, comment)
    }
}
