import CustomDump
import Dispatch
import Encoding
import Foundation
import Subprocess
import System
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let memorySpellings: [(level: UInt, spelling: String)] = [(1, "normal"), (2, "warning"), (4, "critical")]
private let thermalSpellings: [(state: ProcessInfo.ThermalState, spelling: String)] = [(.nominal, "nominal"), (.fair, "fair"), (.serious, "serious"), (.critical, "critical")]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct SystemPressures {
    @Test
    func currentReadsTheKernelLevelAndTheThermalState() async throws {
        func kernelPressureLevel() async throws -> UInt {
            let result: ExecutionResult<Void, StringOutput<UTF8>, DiscardedOutput> = try await Subprocess.run(
                Configuration(executable: .path("/usr/sbin/sysctl"), arguments: ["-n", "kern.memorystatus_vm_pressure_level"], platformOptions: .session()),
                output: .string(limit: 64),
            )
            let text: String = result.standardOutput.trimmingCharacters(in: .whitespacesAndNewlines)
            let level: UInt? = UInt(text)
            return try #require(level)
        }
        let level: UInt = try await kernelPressureLevel()
        let thermal: ProcessInfo.ThermalState = ProcessInfo.processInfo.thermalState
        let pressure: SystemPressure = try SystemPressure.current()
        try #require(level == (try await kernelPressureLevel()), "kernel level changed during the read")
        expectNoDifference(pressure.memory, DispatchSource.MemoryPressureEvent(rawValue: level))
        expectNoDifference(pressure.thermal, thermal)
    }

    @Test(arguments: memorySpellings, thermalSpellings)
    func wireSpellsMemoryAndThermalAsTheReadersDecode(
        memory: (level: UInt, spelling: String),
        thermal: (state: ProcessInfo.ThermalState, spelling: String),
    )
        throws
    {
        let encoded: Data = try JSONEncoder().encode(SystemPressure(memory: DispatchSource.MemoryPressureEvent(rawValue: memory.level), thermal: thermal.state))
        let wire: JSONValue = try JSONDecoder().decode(JSONValue.self, from: encoded)
        expectNoDifference(wire, .object(["memory": .string(memory.spelling), "thermal": .string(thermal.spelling)]))
    }
}
