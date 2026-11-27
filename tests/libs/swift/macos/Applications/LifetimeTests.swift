import AppKit
import AsyncAlgorithms
import CasePathsCore
import CustomDump
import Foundation
import Processes
import Subprocess
import System
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct LifetimeTests {
    @Test(arguments: [false, true])
    func endsWithTheExitOfAProcessKilledWhileRunning(runningOutlivesTheProcess outlived: Bool) async throws {
        let child = Process()
        child.executableURL = URL(filePath: "/bin/sleep")
        child.arguments = ["60"]
        try child.run()
        let process = try ProcessInstance(of: child.processIdentifier)
        let probe = InstanceState.failed(ApplicationFailure(subject: "probe", reason: .noWindow))
        let running = chain(
            [probe].async,
            AsyncStream<InstanceState>(unfolding: {
                do { try process.signal(SIGTERM) } catch { Issue.record(error) }
                if outlived {
                    do { try await Task.sleep(for: .seconds(3600)) } catch { return nil }
                    Issue.record("running sequence outlived the process exit uncancelled")
                }
                return nil
            }),
        )
        let states = await Instance(application: .current, process: process).lifetime(running, clock: ContinuousClock()).reduce(into: [InstanceState]()) { states, state in states.append(state) }
        expectNoDifference(states.first, probe)
        let ends: [Result<TerminationStatus, Errno>?] = states.dropFirst().map { state in state[case: \.exited]?.exit.status }
        expectNoDifference(ends, [.success(.signaled(SIGTERM))])
    }
}
