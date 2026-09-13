import Darwin
import Dispatch
import Foundation

struct ProcessInvocation: Sendable {
  let executable: URL
  let arguments: [String]
  let environment: [String: String]
  let workingDirectory: URL
}

struct ProcessOutput: Sendable {
  let exitCode: Int32
  let stdout: Data
  let stderr: Data
}

enum ProcessFailure: Error, Sendable {
  case launch(any Error)
  case write(any Error)
  case read(any Error)
  case close(any Error)
  case protocolViolation(String)
  case exit(Int32)
  case cancelled
  indirect case failures(first: ProcessFailure, remaining: [ProcessFailure])

  var userMessage: String {
    switch self {
    case .launch(let error): "Could not start the provider: \(error.localizedDescription)"
    case .write(let error): "Could not send the provider request: \(error.localizedDescription)"
    case .read(let error): "Could not read the provider response: \(error.localizedDescription)"
    case .close(let error): "Could not close the provider connection: \(error.localizedDescription)"
    case .protocolViolation(let message): message
    case .exit(let code): "The provider exited with status \(code)"
    case .cancelled: "The operation was cancelled"
    case .failures(let first, let remaining):
      ([first] + remaining).map { failure in failure.userMessage }.joined(separator: "\n")
    }
  }
}

actor NativeProcess {
  nonisolated let lines: AsyncStream<Result<Data, ProcessFailure>>
  nonisolated let processIdentifier: Int32

  private enum State {
    case running
    case cancelling
    case finished(Result<ProcessOutput, ProcessFailure>)
  }

  private enum Input {
    case open(FileHandle)
    case closing(Task<Result<Void, ProcessFailure>, Never>)
    case closed(Result<Void, ProcessFailure>)
  }

  private struct Capture: Sendable {
    let data: Data
    let failures: [ProcessFailure]
  }

  private let process: Process
  private let continuation: AsyncStream<Result<Data, ProcessFailure>>.Continuation
  private let completion: Task<Result<ProcessOutput, ProcessFailure>, Never>
  private let inputQueue: DispatchQueue
  private var input: Input
  private var state: State = .running

  private init(
    process: Process,
    input: FileHandle,
    lines: AsyncStream<Result<Data, ProcessFailure>>,
    continuation: AsyncStream<Result<Data, ProcessFailure>>.Continuation,
    completion: Task<Result<ProcessOutput, ProcessFailure>, Never>
  ) {
    self.process = process
    self.processIdentifier = process.processIdentifier
    self.input = .open(input)
    self.lines = lines
    self.continuation = continuation
    self.completion = completion
    self.inputQueue = DispatchQueue(label: "relay.process.input.\(process.processIdentifier)")
  }

  static func launch(_ invocation: ProcessInvocation) async -> Result<NativeProcess, ProcessFailure>
  {
    guard !Task.isCancelled else { return .failure(.cancelled) }
    let process: Process = Process()
    let input: Pipe = Pipe()
    let output: Pipe = Pipe()
    let errors: Pipe = Pipe()
    let termination = AsyncStream<Int32>.makeStream()
    let lines = AsyncStream<Result<Data, ProcessFailure>>.makeStream()
    process.executableURL = invocation.executable
    process.arguments = invocation.arguments
    process.environment = invocation.environment
    process.currentDirectoryURL = invocation.workingDirectory
    process.standardInput = input
    process.standardOutput = output
    process.standardError = errors
    process.terminationHandler = { child in
      termination.continuation.yield(child.terminationStatus)
      termination.continuation.finish()
    }

    do {
      // A closed child pipe must report EPIPE without sending SIGPIPE to the app
      if fcntl(input.fileHandleForWriting.fileDescriptor, F_SETNOSIGPIPE, 1) == -1 {
        throw NSError(domain: NSPOSIXErrorDomain, code: Int(errno))
      }
      try process.run()
    } catch {
      process.terminationHandler = nil
      termination.continuation.finish()
      lines.continuation.finish()
      let handles: [FileHandle] = [
        input.fileHandleForReading, input.fileHandleForWriting,
        output.fileHandleForReading, output.fileHandleForWriting,
        errors.fileHandleForReading, errors.fileHandleForWriting,
      ]
      let failures: [ProcessFailure] = handles.reduce(into: []) { failures, handle in
        do { try handle.close() } catch { failures.append(.close(error)) }
      }
      return .failure(.failures(first: .launch(error), remaining: failures))
    }

    let stdout: Task<Capture, Never> = Task.detached {
      await NativeProcess.capture(output.fileHandleForReading, lines: lines.continuation)
    }
    let stderr: Task<Capture, Never> = Task.detached {
      await NativeProcess.capture(errors.fileHandleForReading, lines: nil)
    }
    let completion: Task<Result<ProcessOutput, ProcessFailure>, Never> = Task.detached {
      var iterator: AsyncStream<Int32>.Iterator = termination.stream.makeAsyncIterator()
      let exitCode: Int32? = await iterator.next()
      let capturedOutput: Capture = await stdout.value
      let capturedErrors: Capture = await stderr.value
      let failures: [ProcessFailure] = capturedOutput.failures + capturedErrors.failures
      if let first: ProcessFailure = failures.first {
        return .failure(.failures(first: first, remaining: Array(failures.dropFirst())))
      }
      guard let exitCode else {
        return .failure(.protocolViolation("The provider did not report its exit status"))
      }
      return .success(
        ProcessOutput(
          exitCode: exitCode,
          stdout: capturedOutput.data,
          stderr: capturedErrors.data
        ))
    }
    let native: NativeProcess = NativeProcess(
      process: process,
      input: input.fileHandleForWriting,
      lines: lines.stream,
      continuation: lines.continuation,
      completion: completion
    )
    lines.continuation.onTermination = { [weak native] termination in
      if case .cancelled = termination {
        Task { await native?.cancel() }
      }
    }
    if Task.isCancelled {
      await native.cancel()
      return .failure(.cancelled)
    }
    return .success(native)
  }

  func send(_ data: Data) async -> Result<Void, ProcessFailure> {
    guard !Task.isCancelled else {
      await cancel()
      return .failure(.cancelled)
    }
    switch state {
    case .cancelling: return .failure(.cancelled)
    case .finished(let result):
      return result.flatMap { output in .failure(.exit(output.exitCode)) }
    case .running: break
    }
    guard case .open(let handle) = input else {
      return .failure(.protocolViolation("The provider input is closed"))
    }
    let result: Result<Void, ProcessFailure> = await withTaskCancellationHandler {
      await withCheckedContinuation { continuation in
        inputQueue.async {
          continuation.resume(
            returning: Result {
              try handle.write(contentsOf: data)
            }.mapError { error in ProcessFailure.write(error) })
        }
      }
    } onCancel: {
      Task { await self.cancel() }
    }
    guard Task.isCancelled else { return result }
    await cancel()
    switch result {
    case .success: return .failure(.cancelled)
    case .failure(let failure):
      return .failure(.failures(first: .cancelled, remaining: [failure]))
    }
  }

  func closeInput() async {
    switch input {
    case .open(let handle):
      let queue: DispatchQueue = inputQueue
      let closing: Task<Result<Void, ProcessFailure>, Never> = Task {
        await withCheckedContinuation { continuation in
          queue.async {
            continuation.resume(
              returning: Result {
                try handle.close()
              }.mapError { error in ProcessFailure.close(error) })
          }
        }
      }
      input = .closing(closing)
      let result: Result<Void, ProcessFailure> = await closing.value
      input = .closed(result)
    case .closing(let closing):
      let result: Result<Void, ProcessFailure> = await closing.value
      input = .closed(result)
    case .closed: break
    }
  }

  func waitForExit() async -> Result<ProcessOutput, ProcessFailure> {
    if Task.isCancelled { await cancel() }
    if case .finished(let result) = state { return result }
    return await withTaskCancellationHandler {
      await finish()
    } onCancel: {
      Task { await self.cancel() }
    }
  }

  func cancel() async {
    switch state {
    case .running:
      state = .cancelling
      continuation.finish()
      if process.isRunning { process.terminate() }
    case .cancelling: break
    case .finished: return
    }
    await closeInput()
    _ = await finish()
  }

  private func finish() async -> Result<ProcessOutput, ProcessFailure> {
    let captured: Result<ProcessOutput, ProcessFailure> = await completion.value
    await closeInput()
    if case .finished(let result) = state { return result }
    let result: Result<ProcessOutput, ProcessFailure>
    switch state {
    case .cancelling:
      switch captured {
      case .success: result = .failure(.cancelled)
      case .failure(let failure):
        result = .failure(.failures(first: .cancelled, remaining: [failure]))
      }
    case .running: result = captured
    case .finished(let result): return result
    }
    guard case .closed(let closed) = input else {
      return .failure(.protocolViolation("The provider input did not close"))
    }
    let finished: Result<ProcessOutput, ProcessFailure>
    switch (result, closed) {
    case (.success(let output), .success): finished = .success(output)
    case (.failure(let failure), .success): finished = .failure(failure)
    case (.success, .failure(let failure)): finished = .failure(failure)
    case (.failure(let first), .failure(let second)):
      finished = .failure(.failures(first: first, remaining: [second]))
    }
    process.terminationHandler = nil
    state = .finished(finished)
    return finished
  }

  private nonisolated static func capture(
    _ handle: FileHandle,
    lines: AsyncStream<Result<Data, ProcessFailure>>.Continuation?
  ) async -> Capture {
    var data: Data = Data()
    var line: Data = Data()
    var failures: [ProcessFailure] = []
    do {
      for try await byte in handle.bytes {
        data.append(byte)
        if let lines {
          if byte == 0x0A {
            let result: Result<Data, ProcessFailure> = NativeProcess.completeLine(line)
            if case .failure(let failure) = result { failures.append(failure) }
            lines.yield(result)
            line.removeAll(keepingCapacity: true)
          } else {
            line.append(byte)
          }
        }
      }
      if let lines, !line.isEmpty {
        let result: Result<Data, ProcessFailure> = NativeProcess.completeLine(line)
        if case .failure(let failure) = result { failures.append(failure) }
        lines.yield(result)
      }
    } catch {
      let failure: ProcessFailure = .read(error)
      failures.append(failure)
      lines?.yield(.failure(failure))
    }
    do { try handle.close() } catch {
      let failure: ProcessFailure = .close(error)
      failures.append(failure)
      lines?.yield(.failure(failure))
    }
    lines?.finish()
    return Capture(data: data, failures: failures)
  }

  private nonisolated static func completeLine(_ data: Data) -> Result<Data, ProcessFailure> {
    let line: Data = data.last == 0x0D ? Data(data.dropLast()) : data
    guard String(data: line, encoding: .utf8) != nil else {
      return .failure(.protocolViolation("The provider returned text that is not UTF-8"))
    }
    return .success(line)
  }
}
