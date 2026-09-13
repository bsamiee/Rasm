import Foundation

enum ClaudeHeadlessAction: Sendable {
  case refreshCredentials
  case greeting
}

private enum ClaudeHeadlessPhase {
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

struct ClaudeHeadless {
  static func run(
    invocation: ProcessInvocation,
    action: ClaudeHeadlessAction,
    sessionID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    let process: NativeProcess
    switch await NativeProcess.launch(invocation) {
    case .success(let value): process = value
    case .failure(let error): return .failure(.native(error))
    }
    return await withTaskCancellationHandler {
      let result: Result<Void, ClaudeFailure> = await exchange(
        process: process, action: action, sessionID: sessionID
      )
      await process.closeInput()
      let exit: Result<ProcessOutput, ProcessFailure> = await process.waitForExit()
      let cleanup: Result<Void, ClaudeFailure>
      switch exit {
      case .failure(let error): cleanup = .failure(.native(error))
      case .success(let output) where output.exitCode != 0:
        cleanup = .failure(.native(.exit(output.exitCode)))
      case .success:
        cleanup = Task.isCancelled ? .failure(.cancelled) : .success(())
      }
      switch (result, cleanup) {
      case (.failure(let operation), .failure(let release)):
        return .failure(.cleanup(operation: operation, release: release))
      case (.failure(let error), .success):
        return .failure(error)
      case (.success, .failure(let error)):
        switch action {
        case .refreshCredentials: return .failure(error)
        case .greeting: return .failure(.sessionConfirmationPending(error))
        }
      case (.success, .success):
        return .success(())
      }
    } onCancel: {
      Task { await process.cancel() }
    }
  }

  private static func exchange(
    process: NativeProcess, action: ClaudeHeadlessAction, sessionID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    let initialID: String = UUID().uuidString
    var phase: ClaudeHeadlessPhase = .initializing(initialID)
    if case .failure(let error) = await control(
      process: process, id: initialID, request: ["subtype": .string("initialize")]
    ) {
      return .failure(phase.classify(error))
    }

    for await line in process.lines {
      if Task.isCancelled { return .failure(phase.classify(.cancelled)) }
      let data: Data
      switch line {
      case .success(let value): data = value
      case .failure(let error): return .failure(phase.classify(.native(error)))
      }
      let event: JSONValue
      do {
        event = try JSONDecoder().decode(JSONValue.self, from: data)
      } catch {
        return .failure(phase.classify(.protocolFailure))
      }
      if event["type"]?.stringValue == "auth_status", event["error"]?.stringValue != nil {
        return .failure(phase.classify(.signInRequired))
      }
      if case .greeting = phase, event["type"]?.stringValue == "result" {
        return event["subtype"]?.stringValue == "success" && event["is_error"]?.boolValue != true
          ? .success(()) : .failure(phase.classify(.requestFailed))
      }
      guard event["type"]?.stringValue == "control_response",
        let response: JSONValue = event["response"],
        response["request_id"]?.stringValue == phase.requestID
      else { continue }
      guard response["subtype"]?.stringValue == "success" else {
        return .failure(phase.classify(.protocolFailure))
      }

      switch phase {
      case .initializing:
        let id: String = UUID().uuidString
        let request: [String: JSONValue]
        switch action {
        case .refreshCredentials:
          phase = .readingUsage(id)
          request = ["subtype": .string("get_usage"), "skip_behaviors": .bool(true)]
        case .greeting:
          phase = .readingModels(id)
          request = ["subtype": .string("list_models")]
        }
        if case .failure(let error) = await control(process: process, id: id, request: request) {
          return .failure(phase.classify(error))
        }
      case .readingUsage:
        return .success(())
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
        let id: String = UUID().uuidString
        phase = .settingModel(id)
        if case .failure(let error) = await control(
          process: process, id: id,
          request: ["subtype": .string("set_model"), "model": .string(resolved)]
        ) {
          return .failure(phase.classify(error))
        }
      case .settingModel:
        let message: JSONValue = .object([
          "type": .string("user"),
          "session_id": .string(sessionID.uuidString),
          "parent_tool_use_id": .null,
          "message": .object(["role": .string("user"), "content": .string("hi")]),
        ])
        // A failed write can still submit part or all of the greeting.
        phase = .greeting
        if case .failure(let error) = await send(message, to: process) {
          return .failure(phase.classify(error))
        }
      case .greeting:
        return .failure(phase.classify(.protocolFailure))
      }
    }
    return .failure(phase.classify(Task.isCancelled ? .cancelled : .protocolFailure))
  }

  private static func control(
    process: NativeProcess, id: String, request: [String: JSONValue]
  ) async -> Result<Void, ClaudeFailure> {
    await send(
      .object([
        "type": .string("control_request"), "request_id": .string(id), "request": .object(request),
      ]), to: process)
  }

  private static func send(_ message: JSONValue, to process: NativeProcess) async -> Result<
    Void, ClaudeFailure
  > {
    let data: Data
    do {
      var encoded: Data = try JSONEncoder().encode(message)
      encoded.append(0x0a)
      data = encoded
    } catch {
      return .failure(.protocolFailure)
    }
    return await process.send(data).mapError(ClaudeFailure.native)
  }
}
