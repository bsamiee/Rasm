import CustomDump
import Dispatch
import Encoding
import Foundation
import Testing

@testable import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

private let memorySpellings: [(level: UInt, spelling: String)] = [(1, "normal"), (2, "warning"), (4, "critical")]
private let thermalSpellings: [(state: ProcessInfo.ThermalState, spelling: String)] = [(.nominal, "nominal"), (.fair, "fair"), (.serious, "serious"), (.critical, "critical")]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct SystemPressures {
    @Test(arguments: memorySpellings, thermalSpellings)
    func wireSpellsMemoryAndThermalAsReadersDecode(
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
