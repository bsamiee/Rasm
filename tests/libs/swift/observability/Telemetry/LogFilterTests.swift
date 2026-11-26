import CustomDump
import Logging
import Telemetry
import Testing

// --- [TYPES] ---------------------------------------------------------------------------

private enum Root {}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct LogFilterTests {
    @Test
    func givesEachScopeTheLevelOfItsNearestNamedAncestor() {
        let filter = LogFilter(roots: [Root.self], rules: LogFilter.Rules(level: .debug, others: .error, scopes: ["TelemetryTests.Network": .trace, "Vendor.Client": .notice]))
        let scopes = ["TelemetryTests.Network.Socket", "TelemetryTests.Network", "TelemetryTests.Disk", "TelemetryTests", "Vendor.Client.Pool", "Vendor", "Other.Thing"]
        let expected: [Logger.Level] = [.trace, .trace, .debug, .debug, .notice, .error, .error]
        expectNoDifference(scopes.map(filter.level(for:)), expected)
    }

    @Test
    func resolvesAScopeAgainOnceTheRulesChange() {
        let filter = LogFilter(roots: [Root.self])
        let before = filter.level(for: "TelemetryTests.Disk")
        filter.rules = LogFilter.Rules(level: .critical)
        expectNoDifference([before, filter.level(for: "TelemetryTests.Disk")], [.info, .critical])
    }
}
