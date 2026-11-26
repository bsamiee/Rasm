import CustomDump
import Foundation
import IOKit.pwr_mgt
import Testing

@testable import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct SystemPressureTests {
    @Test
    func readsAvailableMemoryAndEveryLoadAdvisoryLevel() throws {
        let pressure = try SystemPressure.current()
        let percent: ClosedRange<Int> = 0...100
        #expect(percent.contains(pressure.available))
        let levels: Set<String> = Set(
            [kIOSystemLoadAdvisoryCombinedLevelKey, kIOSystemLoadAdvisoryUserLevelKey, kIOSystemLoadAdvisoryBatteryLevelKey, kIOSystemLoadAdvisoryThermalLevelKey].map { key in key as String }
        )
        expectNoDifference(Set(pressure.advisory.keys), levels)
    }

    @Test
    func sharesProcessorTicksOfAnIntervalBetweenIdleAndBusy() async throws {
        let earlier = try SystemLoad.current()
        try await Task.sleep(for: .milliseconds(500))
        let interval = try SystemLoad.current().since(earlier)
        let encoded = try JSONSerialization.jsonObject(with: JSONEncoder().encode(interval)) as? [String: Any]
        let idle: Double = try #require(encoded?["idle"] as? Double)
        let share: ClosedRange<Double> = 0...1
        #expect(share.contains(idle))
    }
}
