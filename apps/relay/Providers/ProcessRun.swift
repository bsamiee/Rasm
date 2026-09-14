import Darwin
import Foundation
import Subprocess
import System

nonisolated struct ProcessInvocation: Sendable {
  let executable: URL
  let arguments: [String]
  let environment: [String: String]
  let workingDirectory: URL
  var inheritedInput: FileDescriptor? = nil
}

nonisolated struct ProcessOutput: Sendable {
  let status: TerminationStatus
  let standardOutput: String
  let standardError: String
}

nonisolated enum ProcessFailure: LocalizedError, Sendable {
  case launch(SubprocessError)
  case io(any Error)
  case exit(TerminationStatus)
  case timedOut
  case cancelled
  case protocolViolation(String)

  var errorDescription: String? {
    switch self {
    case .launch(let error): "Could not start the provider: \(error.description)"
    case .io(let error): "The provider connection failed: \(error.localizedDescription)"
    case .exit(.exited(let code)): "The provider exited with status \(code)."
    case .exit(.signaled(let signal)): "The provider was stopped by signal \(signal)."
    case .timedOut: "The provider did not answer in time."
    case .cancelled: "Canceled."
    case .protocolViolation(let message): message
    }
  }
}

nonisolated enum ProcessRun {
  static let teardown: [TeardownStep] = [
    .gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))
  ]

  static func withDeadline<Value: Sendable>(
    _ deadline: Duration,
    _ work: @escaping @Sendable () async -> Result<Value, ProcessFailure>
  ) async -> Result<Value, ProcessFailure> {
    await withDeadline(deadline, timedOut: .timedOut, cancelled: .cancelled, work)
  }

  static func withDeadline<Value: Sendable, Failure: Error>(
    _ deadline: Duration, timedOut: Failure, cancelled: Failure,
    _ work: @escaping @Sendable () async -> Result<Value, Failure>
  ) async -> Result<Value, Failure> {
    await withTaskGroup { group in
      group.addTask(name: "work") { await work() }
      group.addTask(name: "deadline") {
        await Result { try await Task.sleep(for: deadline) }
          .mapError { _ in cancelled }
          .flatMap { _ in .failure(timedOut) }
      }
      let first: Result<Value, Failure> = await group.next() ?? .failure(cancelled)
      group.cancelAll()
      await group.waitForAll()
      return first
    }
  }

  static func stream<Value: Sendable>(
    _ invocation: ProcessInvocation, deadline: Duration?,
    _ body:
      @escaping @Sendable (Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>)
      async -> Result<Value, ProcessFailure>
  ) async -> Result<(Value, TerminationStatus), ProcessFailure> {
    let work: @Sendable () async -> Result<(Value, TerminationStatus), ProcessFailure> = {
      guard !Task.isCancelled else { return .failure(.cancelled) }
      let outcome:
        Result<
          ExecutionResult<Result<Value, ProcessFailure>, SequenceOutput, DiscardedOutput>, any Error
        > = await Result {
          try await Subprocess.run(
            .path(FilePath(invocation.executable.path)),
            arguments: Arguments(invocation.arguments),
            environment: environment(invocation.environment),
            workingDirectory: FilePath(invocation.workingDirectory.path),
            platformOptions: platformOptions(inheriting: invocation.inheritedInput),
            input: .inputWriter, output: .sequence, error: .discarded, body: body
          )
        }
      return outcome.mapError(failure).flatMap { result in
        result.closureResult.map { value in (value, result.terminationStatus) }
      }
    }
    return switch deadline {
    case .some(let deadline): await withDeadline(deadline, work)
    case .none: await work()
    }
  }

  static func collect<Input: InputProtocol>(
    _ invocation: ProcessInvocation, input: Input = .none, deadline: Duration
  ) async -> Result<ProcessOutput, ProcessFailure> {
    await withDeadline(deadline) {
      guard !Task.isCancelled else { return .failure(.cancelled) }
      let outcome:
        Result<
          ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>>, any Error
        > = await Result {
          try await Subprocess.run(
            .path(FilePath(invocation.executable.path)),
            arguments: Arguments(invocation.arguments),
            environment: environment(invocation.environment),
            workingDirectory: FilePath(invocation.workingDirectory.path),
            platformOptions: platformOptions(inheriting: invocation.inheritedInput),
            input: input,
            output: .string(limit: 1 << 20), error: .string(limit: 1 << 20)
          )
        }
      return outcome.mapError(failure).map { result in
        ProcessOutput(
          status: result.terminationStatus,
          standardOutput: result.standardOutput,
          standardError: result.standardError
        )
      }
    }
  }

  static func failure(_ error: any Error) -> ProcessFailure {
    switch error {
    case is CancellationError: .cancelled
    case let error as ProcessFailure: error
    case let error as SubprocessError: .launch(error)
    default: .io(error)
    }
  }

  private static func environment(_ variables: [String: String]) -> Environment {
    .custom(
      Dictionary(
        uniqueKeysWithValues: variables.compactMap { key, value in
          Environment.Key(rawValue: key).map { key in (key, value) }
        }))
  }

  private static func platformOptions(inheriting descriptor: FileDescriptor?) -> PlatformOptions {
    var options: PlatformOptions = PlatformOptions()
    options.createSession = true
    options.teardownSequence = teardown
    if let descriptor {
      let source: Int32 = descriptor.rawValue
      options.preSpawnProcessConfigurator = { _, actions in
        guard posix_spawn_file_actions_addinherit_np(&actions, source) == 0 else {
          throw ProcessFailure.protocolViolation(
            "The provider input descriptor could not be shared.")
        }
      }
    }
    return options
  }
}
