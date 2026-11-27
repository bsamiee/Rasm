import CustomDump
import Darwin
import Foundation
import IOKit.pwr_mgt
import System
import Testing

@testable import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ResourceUsageTests {
    private static let commands: [(command: [String], causes: Set<ResourceUsage.Interval.Cause>)] = [
        (["/bin/sh", "-c", "while :; do :; done"], [.running, .runnable]),
        (["/bin/sleep", "60"], [.idle]),
    ]

    @Test(arguments: commands)
    func namesWhatGrewOverAnIntervalAsItsCauseAndCountsItsRunningThreads(command: [String], causes: Set<ResourceUsage.Interval.Cause>) async throws {
        var child: pid_t = 0
        let arguments = unsafe command.map { argument in unsafe strdup(argument) } + [nil]
        defer { for unsafe argument in unsafe arguments { unsafe free(argument) } }
        try #require(unsafe posix_spawn(&child, command[0], nil, nil, arguments, nil) == 0)
        defer {
            kill(child, SIGKILL)
            var status: Int32 = 0
            _ = unsafe waitpid(child, &status, 0)
        }
        let process = try ProcessInstance(of: child)
        try await Task.sleep(for: .milliseconds(200))
        let earlier = try ResourceUsage(of: process)
        try await Task.sleep(for: .seconds(1))
        let interval = try ResourceUsage(of: process).since(earlier)
        let named: Bool = causes.contains(interval.cause)
        let threads: Int = interval.end.runningThreads
        let running: Bool = (threads > 0) == !causes.contains(.idle)
        #expect(named, "\(command) grew as \(interval.cause)")
        #expect(interval.cpu <= interval.duration + .milliseconds(50))
        #expect(running, "\(command) held \(threads) running threads")
    }

    @Test
    func refusesAZombieWhoseExitUsageTheKernelStillServes() throws {
        var child: pid_t = 0
        let arguments = unsafe [strdup("sleep"), strdup("60"), nil]
        defer { for unsafe argument in unsafe arguments { unsafe free(argument) } }
        try #require(unsafe posix_spawn(&child, "/bin/sleep", nil, nil, arguments, nil) == 0)
        defer {
            var status: Int32 = 0
            _ = unsafe waitpid(child, &status, 0)
        }
        let process = try ProcessInstance(of: child)
        kill(child, SIGKILL)
        var exited = unsafe siginfo_t()
        try #require(unsafe waitid(P_PID, id_t(child), &exited, WEXITED | WNOWAIT) == 0)
        #expect(throws: Errno.noSuchProcess) { try ResourceUsage(of: process) }
    }

    @Test
    func readsEveryInstanceApartAndRefusesOneWhoseStartTimeNoLongerMatches() throws {
        let current = try ProcessInstance(of: getpid())
        let reused = ProcessInstance(pid: current.pid, started: current.started - 1)
        let usages = ResourceUsage.all(of: [current, reused])
        expectNoDifference(Set(usages.keys), [current, reused])
        #expect(throws: Errno.noSuchProcess) { try usages[reused]?.get() }
        #expect(try (usages[current]?.get().threads ?? 0) > 0)
    }

    @Test
    func listsThePowerAssertionItsProcessHoldsByTypeAndNameUntilReleased() throws {
        let current = try ProcessInstance(of: getpid())
        let name = "\(Self.self) \(UUID())"
        var assertion: IOPMAssertionID = 0
        try #require(
            unsafe IOPMAssertionCreateWithName(kIOPMAssertionTypePreventUserIdleSystemSleep as CFString, IOPMAssertionLevel(kIOPMAssertionLevelOn), name as CFString, &assertion) == kIOReturnSuccess
        )
        let listed = "\(kIOPMAssertionTypePreventUserIdleSystemSleep) named \(name)"
        let held: Bool = try ResourceUsage(of: current).powerAssertions.get().contains(listed)
        IOPMAssertionRelease(assertion)
        let released: Bool = try ResourceUsage(of: current).powerAssertions.get().contains(listed)
        expectNoDifference([held, released], [true, false])
    }
}
