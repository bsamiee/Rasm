import CustomDump
import Darwin
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
    func namesWhatGrewOverAnIntervalAsItsCauseInProcessorTime(command: [String], causes: Set<ResourceUsage.Interval.Cause>) async throws {
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
        #expect(named, "\(command) grew as \(interval.cause)")
        #expect(interval.cpu <= interval.duration + .milliseconds(50))
    }
}
