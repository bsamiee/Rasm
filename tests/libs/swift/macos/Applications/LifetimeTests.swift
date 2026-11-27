import AppKit
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
    @Test
    func endsWithTheExitOfAProcessKilledWhileRunning() async throws {
        let child = Process()
        child.executableURL = URL(filePath: "/bin/sleep")
        child.arguments = ["60"]
        try child.run()
        let process = try ProcessInstance(of: child.processIdentifier)
        let lifetime = Instance(application: .current, process: process).lifetime(clock: ContinuousClock())
        try process.signal(SIGTERM)
        let states = await lifetime.reduce(into: [InstanceState]()) { states, state in states.append(state) }
        let ends: [Result<TerminationStatus, Errno>?] = states.map { state in state[case: \.exited]?.exit.status }
        expectNoDifference(ends, [.success(.signaled(SIGTERM))])
    }
}
