import AppKit
import AsyncAlgorithms
import CustomDump
import Foundation
import Processes
import Subprocess
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
                if outlived { try? await Task.sleep(for: .seconds(3600)) }
                return nil
            }),
        )
        let states = await Instance(application: .current, process: process).lifetime(running, clock: ContinuousClock()).reduce(into: [InstanceState]()) { states, state in states.append(state) }
        expectNoDifference(states.first, probe)
        let ends: [TerminationStatus?] = states.dropFirst().map { state -> TerminationStatus? in
            guard case .exited(let ended) = state else { return nil }
            return try? ended.exit.get().status
        }
        expectNoDifference(ends, [.signaled(SIGTERM)])
    }
}
