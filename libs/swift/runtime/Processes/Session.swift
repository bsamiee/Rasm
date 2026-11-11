import Results
public import Subprocess

// --- [OPERATIONS] ----------------------------------------------------------------------

extension PlatformOptions {
    public static func session(grace: Duration = .seconds(2)) -> Self {
        var options = Self()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: grace)]
        return options
    }
}

extension Configuration {
    public func run<Output: OutputProtocol>(
        output: Output,
        within timeout: Duration,
        input: some InputProtocol = .none,
    ) async throws(ProcessFailure) -> ExecutionResult<Void, Output, StringOutput<UTF8>> {
        let subject = executable.description
        let result = try await withTimeout(timeout, expired: ProcessFailure(subject: subject, reason: .timedOut(timeout))) {
            () async throws(ProcessFailure) in
            do {
                return try await Subprocess.run(self, input: input, output: output, error: .string(limit: 1 << 20))
            } catch {
                throw ProcessFailure(subject: subject, error)
            }
        }
        guard result.terminationStatus.isSuccess else { throw ProcessFailure(subject: subject, reason: .exit(result.terminationStatus, standardError: result.standardError)) }
        return result
    }
}
