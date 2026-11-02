import CustomDump
import Darwin
import Encoding
import Foundation
import Processes
import Subprocess
import System
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

private func footprint(running script: String) async throws -> Int {
    try await child(running: script + "; echo ready; read _") { process, _, execution in
        let usage: ResourceUsage = try ResourceUsage(of: process)
        try execution.send(signal: .kill)
        return usage.footprint
    }
    .value
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct ResourceUsages {
    @Test
    func callingProcessCPUTimeLiesBetweenRusageReadsAroundIt() throws {
        func consumed() -> Duration {
            var usage: rusage = rusage()
            _ = unsafe getrusage(RUSAGE_SELF, &usage)
            return [usage.ru_utime, usage.ru_stime].reduce(Duration.zero) { total, time in total + .seconds(time.tv_sec) + .microseconds(time.tv_usec) }
        }
        let before: Duration = consumed()
        let usage: ResourceUsage = try ResourceUsage(of: getpid())
        let after: Duration = consumed()
        #expect(before <= usage.cpu, "\(before) > \(usage.cpu)")
        #expect(usage.cpu <= after + .microseconds(1), "\(usage.cpu) > \(after)")
    }

    @Test
    func busyChildCPUTimeGrowsWhileItRuns() async throws {
        _ = try await child(running: "echo ready; while :; do :; done") { process, _, execution in
            let first: ResourceUsage = try ResourceUsage(of: process)
            _ = try await processState(of: process)
            let second: ResourceUsage = try ResourceUsage(of: process)
            try execution.send(signal: .kill)
            #expect(first.cpu < second.cpu, "\(first.cpu) then \(second.cpu)")
        }
    }

    @Test
    func singleThreadedChildReportsOneThread() async throws {
        _ = try await child(running: "echo ready; exec /bin/cat") { process, _, execution in
            expectNoDifference(try ResourceUsage(of: process).threads, 1)
            try execution.send(signal: .kill)
        }
    }

    @Test
    func footprintGrowsByMemoryChildHolds() async throws {
        let plain: Int = try await footprint(running: "true")
        let holding: Int = try await footprint(running: "held=$(/usr/bin/head -c 50000000 /dev/zero | /usr/bin/tr '\\0' a); [ ${#held} -eq 50000000 ]")
        #expect(holding - plain >= 50_000_000, "plain \(plain), holding \(holding)")
    }

    @Test
    func reapedProcessThrowsNoSuchProcess() async throws {
        let process: pid_t = try await reapedProcess()
        #expect(throws: Errno.noSuchProcess) { try ResourceUsage(of: process) }
    }

    @Test
    func wireHoldsCPUSecondsFootprintBytesAndThreads() throws {
        let usage: ResourceUsage = try ResourceUsage(of: getpid())
        let encoded: Data = try JSONEncoder().encode(usage)
        let wire: JSONValue = try JSONDecoder().decode(JSONValue.self, from: encoded)
        let fields: [String: JSONValue] = try wire.decode()
        let seconds: Double = Double(usage.cpu.components.seconds) + Double(usage.cpu.components.attoseconds) / 1e18
        let cpu: Double = try #require(fields["cpu"]).decode()
        expectNoDifference(fields.filter { key, _ in key != "cpu" }, ["footprint": .number(Double(usage.footprint)), "threads": .number(Double(usage.threads))])
        let drift: Double = abs(cpu - seconds)
        #expect(drift <= 1e-9, "cpu \(cpu) for \(usage.cpu)")
    }
}
