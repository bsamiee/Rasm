import CustomDump
import Foundation
import IOKit.pwr_mgt
import Testing

@testable import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct SystemPressureTests {
    @Test
    func encodesLevelsInTheWireSpellingsAndOmitsAnUnreadAdvisory() throws {
        let combined = kIOSystemLoadAdvisoryCombinedLevelKey as String
        let user = kIOSystemLoadAdvisoryUserLevelKey as String
        let read = SystemPressure(memory: .warning, thermal: .serious, lowPower: true, advisory: [combined: kIOSystemLoadAdvisoryLevelBad, user: 7])
        let unread = SystemPressure(memory: .critical, thermal: .nominal, lowPower: false, advisory: nil)
        let encoder = JSONEncoder()
        encoder.outputFormatting = .sortedKeys
        let encoded: [String] = try [read, unread].map { pressure in try String(decoding: encoder.encode(pressure), as: UTF8.self) }
        let expected: [String] = [
            #"{"advisory":{"\#(combined)":"bad","\#(user)":"7"},"lowPower":true,"memory":"warning","thermal":"serious"}"#,
            #"{"lowPower":false,"memory":"critical","thermal":"nominal"}"#,
        ]
        expectNoDifference(encoded, expected)
    }

    @Test(.timeLimit(.minutes(1)))
    func readsOnSubscribeWithTheAdvisoryRegistered() async throws {
        let first = try #require(await SystemPressure.changes().first { _ in true })
        _ = try first.get()
    }

    @Test
    func countsDiskGrowthOfDriversPresentInBothReadingsAlone() throws {
        let readings: [(processor: (UInt64, UInt64, UInt64), paging: (UInt64, UInt64, UInt64, UInt64, UInt64), disks: [UInt64: (bytesRead: UInt64, bytesWritten: UInt64, time: Duration)])] = [
            ((100, 100, 100), (10, 10, 10, 10, 100), [1: (100, 50, .seconds(1)), 2: (1 << 40, 1 << 40, .seconds(900))]),
            ((130, 110, 160), (11, 12, 13, 14, 140), [1: (150, 70, .milliseconds(2500)), 3: (5, 6, .seconds(1))]),
        ]
        let loads = readings.map { reading -> SystemLoad in
            let processor = SIMD4<UInt32>(UInt32(reading.processor.0), UInt32(reading.processor.1), UInt32(reading.processor.2), 0)
            var memory = vm_statistics64()
            (memory.pageins, memory.swapins, memory.swapouts, memory.decompressions, memory.faults) = reading.paging
            return SystemLoad(processor: processor, memory: memory, disks: reading.disks)
        }
        let interval = SystemLoad.Interval(start: loads[0], end: loads[1], deviceUtilization: 12, availableCapacity: 7, availableMemory: 64)
        let encoder = JSONEncoder()
        encoder.outputFormatting = .sortedKeys
        let encoded = try String(decoding: encoder.encode(interval), as: UTF8.self)
        let expected =
            #"{"availableCapacity":7,"availableMemory":64,"decompressions":4,"deviceUtilization":12,"diskBytesRead":50,"diskBytesWritten":20,"diskTime":1.5,"idle":0.6,"pageins":1,"swapins":2,"swapouts":3}"#
        expectNoDifference(encoded, expected)
    }

    @Test
    func readsProcessorTicksOverAnIntervalSinceAnEarlierLoad() async throws {
        let earlier = try SystemLoad()
        try await Task.sleep(for: .milliseconds(100))
        _ = try #require(SystemLoad.Interval(since: earlier).idle)
    }
}
