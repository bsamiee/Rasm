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
        let read = SystemPressure(memory: .warning, thermal: .serious, advisory: [combined: kIOSystemLoadAdvisoryLevelBad, user: 7])
        let unread = SystemPressure(memory: .critical, thermal: .nominal, advisory: nil)
        let encoder = JSONEncoder()
        encoder.outputFormatting = .sortedKeys
        let encoded: [String] = try [read, unread].map { pressure in try String(decoding: encoder.encode(pressure), as: UTF8.self) }
        let expected: [String] = [
            #"{"advisory":{"\#(combined)":"bad","\#(user)":"7"},"memory":"warning","thermal":"serious"}"#,
            #"{"memory":"critical","thermal":"nominal"}"#,
        ]
        expectNoDifference(encoded, expected)
    }

    @Test(.timeLimit(.minutes(1)))
    func readsOnSubscribeWithTheAdvisoryRegistered() async throws {
        let first = try #require(await SystemPressure.changes().first { _ in true })
        _ = try first.get()
    }

    @Test
    func encodesTheIdleShareOfProcessorTicksAndSwapGrowthBetweenTwoReadings() throws {
        let readings: [(processor: (UInt64, UInt64, UInt64), paging: (UInt64, UInt64, UInt64))] = [((100, 100, 100), (10, 10, 100)), ((130, 110, 160), (12, 13, 140))]
        let loads = readings.map { reading -> SystemLoad in
            let processor = SIMD4<UInt32>(UInt32(reading.processor.0), UInt32(reading.processor.1), UInt32(reading.processor.2), 0)
            var memory = vm_statistics64()
            (memory.swapins, memory.swapouts, memory.faults) = reading.paging
            return SystemLoad(processor: processor, memory: memory)
        }
        let encoder = JSONEncoder()
        encoder.outputFormatting = .sortedKeys
        let encoded = try String(decoding: encoder.encode(SystemLoad.Interval(start: loads[0], end: loads[1])), as: UTF8.self)
        expectNoDifference(encoded, #"{"processorIdle":0.6,"swapins":2,"swapouts":3}"#)
    }

    @Test
    func readsProcessorTicksOverAnIntervalSinceAnEarlierLoad() async throws {
        let earlier = try SystemLoad()
        try await Task.sleep(for: .milliseconds(100))
        _ = try #require(SystemLoad.Interval(since: earlier).processorIdle)
    }
}
