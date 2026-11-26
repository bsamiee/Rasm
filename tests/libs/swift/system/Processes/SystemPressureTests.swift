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
        let pressure = try SystemPressure()
        let percent: ClosedRange<Int> = 0...100
        #expect(percent.contains(pressure.available))
        let levels: Set<String> = Set(
            [kIOSystemLoadAdvisoryCombinedLevelKey, kIOSystemLoadAdvisoryUserLevelKey, kIOSystemLoadAdvisoryBatteryLevelKey, kIOSystemLoadAdvisoryThermalLevelKey].map { key in key as String }
        )
        expectNoDifference(pressure.advisory.map { advisory in Set(advisory.keys) }, levels)
    }

    @Test
    func encodesLevelsInTheWireSpellingsAndOmitsAnUnreadAdvisory() throws {
        let combined = kIOSystemLoadAdvisoryCombinedLevelKey as String
        let user = kIOSystemLoadAdvisoryUserLevelKey as String
        let read = SystemPressure(memory: .warning, available: 40, thermal: .serious, lowPower: true, advisory: [combined: kIOSystemLoadAdvisoryLevelBad, user: 7])
        let unread = SystemPressure(memory: .critical, available: 3, thermal: .nominal, lowPower: false, advisory: nil)
        let encoder = JSONEncoder()
        encoder.outputFormatting = .sortedKeys
        let encoded: [String] = try [read, unread].map { pressure in try String(decoding: encoder.encode(pressure), as: UTF8.self) }
        let expected: [String] = [
            #"{"advisory":{"\#(combined)":"bad","\#(user)":"7"},"available":40,"lowPower":true,"memory":"warning","thermal":"serious"}"#,
            #"{"available":3,"lowPower":false,"memory":"critical","thermal":"nominal"}"#,
        ]
        expectNoDifference(encoded, expected)
    }

    @Test(.timeLimit(.minutes(1)))
    func readsOnSubscribeWithTheAdvisoryRegistered() async throws {
        let first = try #require(await SystemPressure.changes().first { _ in true })
        _ = try first.get()
    }

    @Test
    func countsDiskGrowthOfDriversPresentAtTheEndAlone() throws {
        let earlier = SystemLoad(
            processor: SIMD4(100, 100, 100, 0),
            pageins: 10,
            swapins: 10,
            swapouts: 10,
            decompressions: 10,
            disks: [1: (bytesRead: 100, bytesWritten: 50, time: .seconds(1)), 2: (bytesRead: 1 << 40, bytesWritten: 1 << 40, time: .seconds(900))],
        )
        let later = SystemLoad(
            processor: SIMD4(130, 110, 160, 0),
            pageins: 11,
            swapins: 12,
            swapouts: 13,
            decompressions: 14,
            disks: [1: (bytesRead: 150, bytesWritten: 70, time: .milliseconds(2500)), 3: (bytesRead: 5, bytesWritten: 6, time: .seconds(1))],
        )
        let interval = SystemLoad.Interval(growth: later.since(earlier), deviceUtilization: 12, availableCapacityForImportantUsage: nil)
        let encoder = JSONEncoder()
        encoder.outputFormatting = .sortedKeys
        let encoded = try String(decoding: encoder.encode(interval), as: UTF8.self)
        let expected = #"{"decompressions":4,"deviceUtilization":12,"diskBytesRead":55,"diskBytesWritten":26,"diskTime":2.5,"idle":0.6,"pageins":1,"swapins":2,"swapouts":3}"#
        expectNoDifference(encoded, expected)
    }

    @Test
    func sharesProcessorTicksOfAnIntervalBetweenIdleAndBusy() async throws {
        let earlier = try SystemLoad()
        try await Task.sleep(for: .milliseconds(500))
        let idle: Double = try #require(SystemLoad.Interval(since: earlier).idle)
        let share: ClosedRange<Double> = 0...1
        #expect(share.contains(idle))
    }
}
