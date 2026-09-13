import Darwin
import Dispatch
import Foundation

nonisolated struct ProcessInvocation: Sendable {
  let executable: URL
  let arguments: [String]
  let environment: [String: String]
  let workingDirectory: URL
}

nonisolated struct ProcessTermination: Sendable {
  let status: Int32
}

nonisolated enum ProcessFailure: LocalizedError, Sendable {
  case launch(any Error)
  case write(any Error)
  case read(any Error)
  case close(any Error)
  case protocolViolation(String)
  case exit(Int32)
  case cancelled
  indirect case failures(first: ProcessFailure, remaining: [ProcessFailure])

  var errorDescription: String? {
    switch self {
    case .launch(let error): "Could not start the provider: \(error.localizedDescription)"
    case .write(let error): "Could not send the provider request: \(error.localizedDescription)"
    case .read(let error): "Could not read the provider response: \(error.localizedDescription)"
    case .close(let error): "Could not close the provider connection: \(error.localizedDescription)"
    case .protocolViolation(let message): message
    case .exit(let status): "The provider exited with status \(status)."
    case .cancelled: "Canceled."
    case .failures(let first, let remaining):
      ([first] + remaining).map { failure in failure.localizedDescription }.joined(separator: "\n")
    }
  }
}

actor ChildProcess {
  nonisolated let lines: AsyncStream<Result<Data, ProcessFailure>>
  nonisolated let processIdentifier: Int32

  private enum State {
    case running
    case cancelling
    case finished(Result<ProcessTermination, ProcessFailure>)
  }

  private enum Input {
    case open(FileHandle)
    case closing(Task<Result<Void, ProcessFailure>, Never>)
    case closed(Result<Void, ProcessFailure>)
  }

  private let process: Process
  private let continuation: AsyncStream<Result<Data, ProcessFailure>>.Continuation
  private let completion: Task<Result<ProcessTermination, ProcessFailure>, Never>
  private let inputQueue: DispatchQueue
  private var input: Input
  private var state: State = .running

  private init(
    process: Process,
    input: FileHandle,
    lines: AsyncStream<Result<Data, ProcessFailure>>,
    continuation: AsyncStream<Result<Data, ProcessFailure>>.Continuation,
    completion: Task<Result<ProcessTermination, ProcessFailure>, Never>
  ) {
    self.process = process
    self.processIdentifier = process.processIdentifier
    self.input = .open(input)
    self.lines = lines
    self.continuation = continuation
    self.completion = completion
    self.inputQueue = DispatchQueue(label: "relay.process.input.\(process.processIdentifier)")
  }

  static func launch(_ invocation: ProcessInvocation) async -> Result<ChildProcess, ProcessFailure>
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

    let stdout: Task<[ProcessFailure], Never> = Task.detached {
      await ChildProcess.readLines(from: output.fileHandleForReading, into: lines.continuation)
    }
    let stderr: Task<[ProcessFailure], Never> = Task.detached {
      await ChildProcess.readLines(from: errors.fileHandleForReading, into: nil)
    }
    let completion: Task<Result<ProcessTermination, ProcessFailure>, Never> = Task.detached {
      var iterator: AsyncStream<Int32>.Iterator = termination.stream.makeAsyncIterator()
      let status: Int32? = await iterator.next()
      let failures: [ProcessFailure] = await stdout.value + stderr.value
      return failures.first.map { first -> Result<ProcessTermination, ProcessFailure> in
        .failure(.failures(first: first, remaining: Array(failures.dropFirst())))
      }
        ?? status.map { status in .success(ProcessTermination(status: status)) }
        ?? .failure(.protocolViolation("The provider did not report its exit status."))
    }
    let child: ChildProcess = ChildProcess(
      process: process,
      input: input.fileHandleForWriting,
      lines: lines.stream,
      continuation: lines.continuation,
      completion: completion
    )
    lines.continuation.onTermination = { [weak child] termination in
      if case .cancelled = termination {
        Task { await child?.cancel() }
      }
    }
    if Task.isCancelled {
      await child.cancel()
      return .failure(.cancelled)
    }
    return .success(child)
  }

  func send(_ data: Data) async -> Result<Void, ProcessFailure> {
    guard !Task.isCancelled else {
      await cancel()
      return .failure(.cancelled)
    }
    switch state {
    case .cancelling: return .failure(.cancelled)
    case .finished(let result):
      return result.flatMap { termination in .failure(.exit(termination.status)) }
    case .running: break
    }
    guard case .open(let handle) = input else {
      return .failure(.protocolViolation("The provider input is closed."))
    }
    let result: Result<Void, ProcessFailure> = await withTaskCancellationHandler {
      await onInputQueue { Self.write(data, to: handle) }
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
      let closing: Task<Result<Void, ProcessFailure>, Never> = Task {
        await self.onInputQueue { Self.close(handle) }
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

  func waitUntilExit() async -> Result<ProcessTermination, ProcessFailure> {
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

  private func onInputQueue<Value: Sendable>(
    _ work: @escaping @Sendable () -> Value
  ) async -> Value {
    await withCheckedContinuation { continuation in
      inputQueue.async { continuation.resume(returning: work()) }
    }
  }

  private func finish() async -> Result<ProcessTermination, ProcessFailure> {
    let captured: Result<ProcessTermination, ProcessFailure> = await completion.value
    await closeInput()
    let result: Result<ProcessTermination, ProcessFailure>
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
      return .failure(.protocolViolation("The provider input did not close."))
    }
    let finished: Result<ProcessTermination, ProcessFailure>
    switch (result, closed) {
    case (.success(let termination), .success): finished = .success(termination)
    case (.failure(let failure), .success): finished = .failure(failure)
    case (.success, .failure(let failure)): finished = .failure(failure)
    case (.failure(let first), .failure(let second)):
      finished = .failure(.failures(first: first, remaining: [second]))
    }
    process.terminationHandler = nil
    state = .finished(finished)
    return finished
  }

  private nonisolated static func write(_ data: Data, to handle: FileHandle) -> Result<
    Void, ProcessFailure
  > {
    Result { try handle.write(contentsOf: data) }.mapError(ProcessFailure.write)
  }

  private nonisolated static func close(_ handle: FileHandle) -> Result<Void, ProcessFailure> {
    Result { try handle.close() }.mapError(ProcessFailure.close)
  }

  private nonisolated static func readLines(
    from handle: FileHandle,
    into lines: AsyncStream<Result<Data, ProcessFailure>>.Continuation?
  ) async -> [ProcessFailure] {
    var line: Data = Data()
    var failures: [ProcessFailure] = []
    do {
      for try await byte in handle.bytes where lines != nil {
        if byte == 0x0A {
          failures.append(contentsOf: yield(line, into: lines))
          line.removeAll(keepingCapacity: true)
        } else {
          line.append(byte)
        }
      }
      if !line.isEmpty { failures.append(contentsOf: yield(line, into: lines)) }
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
    return failures
  }

  private nonisolated static func yield(
    _ line: Data, into lines: AsyncStream<Result<Data, ProcessFailure>>.Continuation?
  ) -> [ProcessFailure] {
    let result: Result<Data, ProcessFailure> = validateLine(line)
    lines?.yield(result)
    return switch result {
    case .success: []
    case .failure(let failure): [failure]
    }
  }

  private nonisolated static func validateLine(_ data: Data) -> Result<Data, ProcessFailure> {
    let line: Data = data.last == 0x0D ? Data(data.dropLast()) : data
    return String(data: line, encoding: .utf8) == nil
      ? .failure(.protocolViolation("The provider returned text that is not UTF-8."))
      : .success(line)
  }
}
