import CasePaths
import CustomDump
import Foundation
import Processes
import Subprocess
import Testing

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct LoginShells {
    @Test
    func environmentHoldsUserHomeAndSystemPath() async throws {
        let environment: [Environment.Key: String] = try await loginShellEnvironment(within: .seconds(30))
        expectNoDifference(environment["HOME"].map { home in URL(filePath: home, directoryHint: .isDirectory).path(percentEncoded: false) }, URL.homeDirectory.path(percentEncoded: false))
        let path: [Substring] = try #require(environment["PATH"]).split(separator: ":")
        #expect(path.contains("/usr/bin") && path.contains("/bin"), "PATH \(path)")
    }

    @Test
    func expiredTimeoutThrowsTimedOut() async throws {
        let failure: ProcessFailure = try await #require(throws: ProcessFailure.self) { try await loginShellEnvironment(within: .nanoseconds(1)) }
        expectNoDifference(failure.reason[case: \.timedOut], .nanoseconds(1))
    }
}
