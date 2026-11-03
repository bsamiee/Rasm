import Results
public import Subprocess

// --- [OPERATIONS] ----------------------------------------------------------------------

extension PlatformOptions {
    public static func session(grace: Duration = .seconds(2)) -> Self {
        var options: Self = Self()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: grace)]
        return options
    }
}

extension Configuration {
    public func run<Output: OutputProtocol, Error: ErrorOutputProtocol>(
        output: Output,
        within timeout: Duration,
        input: some InputProtocol = .none,
        error: Error = .discarded,
    ) async throws(ProcessFailure) -> ExecutionResult<Void, Output, Error> {
        let subject: String = executable.description
        let result: ExecutionResult<Void, Output, Error> = try await withTimeout(timeout, expired: ProcessFailure(subject: subject, reason: .timedOut(timeout))) { () async throws(ProcessFailure) in
            do {
                return try await Subprocess.run(self, input: input, output: output, error: error)
            } catch {
                throw ProcessFailure(subject: subject, error)
            }
        }
        guard result.terminationStatus.isSuccess else { throw ProcessFailure(subject: subject, reason: .exit(result.terminationStatus)) }
        return result
    }
}
