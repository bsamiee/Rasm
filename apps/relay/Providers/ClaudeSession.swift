import Foundation

nonisolated enum ClaudeSessionAction: Sendable {
  case refreshCredentials
  case greeting
}

private nonisolated enum ClaudeSessionPhase {
  case initializing(String)
  case readingUsage(String)
  case readingModels(String)
  case settingModel(String)
  case greeting

  var requestID: String? {
    switch self {
    case .initializing(let id), .readingUsage(let id), .readingModels(let id),
      .settingModel(let id):
      id
    case .greeting: nil
    }
  }

  func classify(_ failure: ClaudeFailure) -> ClaudeFailure {
    switch self {
    case .initializing, .readingUsage, .readingModels, .settingModel: failure
    case .greeting: .sessionConfirmationPending(failure)
    }
  }
}

private nonisolated enum ClaudeSessionStep {
  case awaiting(ClaudeSessionPhase)
  case finished
}

nonisolated struct ClaudeSession {
  static func run(
    invocation: ProcessInvocation,
    action: ClaudeSessionAction,
    sessionID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    await ChildProcess.launch(invocation).mapError(ClaudeFailure.process).bind { process in
      await withTaskCancellationHandler {
        let result: Result<Void, ClaudeFailure> = await readLoop(
          process: process, action: action, sessionID: sessionID
        )
        await process.closeInput()
        let cleanup: Result<Void, ClaudeFailure> = await process.waitUntilExit().exited()
          .flatMap { _ in Task.isCancelled ? .failure(.cancelled) : .success(()) }
        return switch result {
        case .failure(let error): .failure(error.releasing(cleanup))
        case .success:
          cleanup.mapError { error in
            switch action {
            case .refreshCredentials: error
            case .greeting: .sessionConfirmationPending(error)
            }
          }
        }
      } onCancel: {
        Task { await process.cancel() }
      }
    }
  }

  private static func readLoop(
    process: ChildProcess, action: ClaudeSessionAction, sessionID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    let initial: ClaudeSessionPhase = .initializing(UUID().uuidString)
    var step: Result<ClaudeSessionStep, ClaudeFailure> = await sendControlRequest(
      ["subtype": .string("initialize")], for: initial, to: process)
    for await line in process.lines {
      guard case .success(.awaiting(let phase)) = step else { break }
      step = await advance(
        phase, line: line, process: process, action: action, sessionID: sessionID)
    }
    return step.flatMap { step in
      switch step {
      case .finished: .success(())
      case .awaiting(let phase):
        .failure(phase.classify(Task.isCancelled ? .cancelled : .protocolFailure))
      }
    }
  }

  private static func advance(
    _ phase: ClaudeSessionPhase, line: Result<Data, ProcessFailure>, process: ChildProcess,
    action: ClaudeSessionAction, sessionID: UUID
  ) async -> Result<ClaudeSessionStep, ClaudeFailure> {
    return await (Task.isCancelled ? .failure(.cancelled) : line.mapError(ClaudeFailure.process))
      .flatMap(decode).mapError(phase.classify).bind {
        event -> Result<ClaudeSessionStep, ClaudeFailure> in
        if event["type"]?.stringValue == "auth_status", event["error"]?.stringValue != nil {
          return .failure(phase.classify(.signInRequired))
        }
        if case .greeting = phase, event["type"]?.stringValue == "result" {
          return event["subtype"]?.stringValue == "success" && event["is_error"]?.boolValue != true
            ? .success(.finished) : .failure(phase.classify(.requestFailed))
        }
        guard event["type"]?.stringValue == "control_response",
          let response: JSONValue = event["response"],
          response["request_id"]?.stringValue == phase.requestID
        else { return .success(.awaiting(phase)) }
        guard response["subtype"]?.stringValue == "success" else {
          return .failure(phase.classify(.protocolFailure))
        }
        switch phase {
        case .initializing:
          let id: String = UUID().uuidString
          switch action {
          case .refreshCredentials:
            return await sendControlRequest(
              ["subtype": .string("get_usage"), "skip_behaviors": .bool(true)],
              for: .readingUsage(id), to: process)
          case .greeting:
            return await sendControlRequest(
              ["subtype": .string("list_models")], for: .readingModels(id), to: process)
          }
        case .readingUsage:
          return .success(.finished)
        case .readingModels:
          guard let models: [JSONValue] = response["response"]?["models"]?.arrayValue,
            let model: JSONValue = models.first(where: { item in
              item["disabled"]?.boolValue != true
                && (item["value"]?.stringValue == "haiku"
                  || item["resolvedModel"]?.stringValue?.hasPrefix("claude-haiku-") == true)
            }),
            let resolved: String = model["resolvedModel"]?.stringValue
          else {
            return .failure(phase.classify(.modelUnavailable))
          }
          return await sendControlRequest(
            ["subtype": .string("set_model"), "model": .string(resolved)],
            for: .settingModel(UUID().uuidString), to: process)
        case .settingModel:
          let message: JSONValue = .object([
            "type": .string("user"),
            "session_id": .string(sessionID.uuidString),
            "parent_tool_use_id": .null,
            "message": .object(["role": .string("user"), "content": .string("hi")]),
          ])
          return await send(message, to: process)
            .mapError(ClaudeSessionPhase.greeting.classify)
            .map { _ in .awaiting(.greeting) }
        case .greeting:
          return .failure(phase.classify(.protocolFailure))
        }
      }
  }

  private static func decode(_ data: Data) -> Result<JSONValue, ClaudeFailure> {
    Result { try JSONDecoder().decode(JSONValue.self, from: data) }
      .mapError { _ in .protocolFailure }
  }

  private static func sendControlRequest(
    _ request: [String: JSONValue], for phase: ClaudeSessionPhase, to process: ChildProcess
  ) async -> Result<ClaudeSessionStep, ClaudeFailure> {
    await
      (phase.requestID.map(Result<String, ClaudeFailure>.success)
      ?? .failure(phase.classify(.protocolFailure)))
      .bind { id -> Result<ClaudeSessionStep, ClaudeFailure> in
        await send(
          .object([
            "type": .string("control_request"), "request_id": .string(id),
            "request": .object(request),
          ]), to: process
        )
        .mapError(phase.classify)
        .map { _ in .awaiting(phase) }
      }
  }

  private static func send(_ message: JSONValue, to process: ChildProcess) async -> Result<
    Void, ClaudeFailure
  > {
    await Result { try JSONEncoder().encode(message) }
      .mapError { _ in ClaudeFailure.protocolFailure }
      .bind { encoded in await process.send(encoded + [0x0a]).mapError(ClaudeFailure.process) }
  }
}
