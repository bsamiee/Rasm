import CustomDump
import Darwin
import Processes
import Subprocess
import System
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

private func usage(running script: String) async throws -> ResourceUsage {
    try await child(running: script + "; echo ready; read _") { process, _, execution in
        let usage: ResourceUsage = try ResourceUsage(of: process)
        try execution.send(signal: .kill)
        return usage
    }
    .value
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct ResourceUsages {
    @Test
    func singleThreadedChildReportsOneThread() async throws {
        _ = try await child(running: "echo ready; exec /bin/cat") { process, _, execution in
            expectNoDifference(try ResourceUsage(of: process).threads, 1)
            try execution.send(signal: .kill)
        }
    }

    @Test
    func footprintAndCPUTimeGrowByWorkChildDid() async throws {
        let plain: ResourceUsage = try await usage(running: "true")
        let holding: ResourceUsage = try await usage(running: "held=$(/usr/bin/head -c 50000000 /dev/zero | /usr/bin/tr '\\0' a); [ ${#held} -eq 50000000 ]")
        #expect(holding.footprint - plain.footprint >= 50_000_000, "plain \(plain.footprint), holding \(holding.footprint)")
        #expect(holding.cpu > plain.cpu, "plain \(plain.cpu), holding \(holding.cpu)")
    }

    @Test
    func reapedProcessThrowsNoSuchProcess() async throws {
        let process: pid_t = try await reapedProcess()
        #expect(throws: Errno.noSuchProcess) { try ResourceUsage(of: process) }
    }
}
