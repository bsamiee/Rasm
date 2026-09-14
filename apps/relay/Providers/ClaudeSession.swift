import Foundation
import Subprocess

nonisolated enum ClaudeSessionAction: Sendable {
  case refreshCredentials
  case greeting

  var deadline: Duration {
    switch self {
    case .refreshCredentials: .seconds(60)
    case .greeting: .seconds(120)
    }
  }
}

private nonisolated enum ClaudeSessionPhase: Sendable {
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
}

private nonisolated enum ClaudeSessionStep: Sendable {
  case awaiting(ClaudeSessionPhase, sessionReset: Date?)
  case finished(sessionReset: Date?)
}

nonisolated enum ClaudeSession {
  static func run(
    invocation: ProcessInvocation, action: ClaudeSessionAction, sessionID: UUID
  ) async -> Result<Date?, ClaudeFailure> {
    let outcome: Result<(Result<Date?, ClaudeFailure>, TerminationStatus), ProcessFailure> =
      await ProcessRun.stream(invocation, deadline: action.deadline) { execution in
        await readLoop(execution: execution, action: action, sessionID: sessionID)
      }
    return outcome.claude().flatMap { result, status in
      result.flatMap { reset in
        status.isSuccess ? .success(reset) : .failure(.process(.exit(status)))
      }
    }
  }

  private static func readLoop(
    execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>,
    action: ClaudeSessionAction, sessionID: UUID
  ) async -> Result<Result<Date?, ClaudeFailure>, ProcessFailure> {
    let initial: ClaudeSessionPhase = .initializing(UUID().uuidString)
    var step: Result<ClaudeSessionStep, ClaudeFailure> = await sendControlRequest(
      ["subtype": .string("initialize")], for: initial, sessionReset: nil, to: execution)
    let lines: SubprocessOutputSequence.StringSequence<UTF8> = execution.standardOutput.strings()
    let read: Result<Void, any Error> = await Result {
      for try await line in lines {
        guard case .success(.awaiting(let phase, let reset)) = step else { break }
        step = await advance(
          phase, sessionReset: reset, line: line, execution: execution, action: action,
          sessionID: sessionID)
        if case .success(.finished) = step { break }
      }
    }
    let outcome: Result<Date?, ClaudeFailure> = step.flatMap { step in
      switch step {
      case .finished(let reset): .success(reset)
      case .awaiting: .failure(Task.isCancelled ? .cancelled : .protocolFailure)
      }
    }
    return read.mapError(ProcessRun.failure).map { _ in outcome }
  }

  private static func advance(
    _ phase: ClaudeSessionPhase, sessionReset: Date?, line: String,
    execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>,
    action: ClaudeSessionAction, sessionID: UUID
  ) async -> Result<ClaudeSessionStep, ClaudeFailure> {
    guard !Task.isCancelled else { return .failure(.cancelled) }
    guard let event: JSONValue = try? JSONDecoder().decode(JSONValue.self, from: Data(line.utf8))
    else { return .success(.awaiting(phase, sessionReset: sessionReset)) }
    if event["type"]?.stringValue == "auth_status", event["error"]?.stringValue != nil {
      return .failure(.signInRequired)
    }
    if event["type"]?.stringValue == "rate_limit_event",
      let info: JSONValue = event["rate_limit_info"],
      info["rateLimitType"]?.stringValue == "five_hour",
      let seconds: Double = info["resetsAt"]?.doubleValue
    {
      return .success(
        .awaiting(phase, sessionReset: Date(timeIntervalSince1970: seconds)))
    }
    if case .greeting = phase, event["type"]?.stringValue == "result" {
      return event["subtype"]?.stringValue == "success" && event["is_error"]?.boolValue != true
        ? .success(.finished(sessionReset: sessionReset)) : .failure(.requestFailed)
    }
    guard event["type"]?.stringValue == "control_response",
      let response: JSONValue = event["response"],
      response["request_id"]?.stringValue == phase.requestID
    else { return .success(.awaiting(phase, sessionReset: sessionReset)) }
    guard response["subtype"]?.stringValue == "success" else { return .failure(.protocolFailure) }
    switch phase {
    case .initializing:
      let id: String = UUID().uuidString
      switch action {
      case .refreshCredentials:
        return await sendControlRequest(
          ["subtype": .string("get_usage"), "skip_behaviors": .bool(true)],
          for: .readingUsage(id), sessionReset: sessionReset, to: execution, closingInput: true)
      case .greeting:
        return await sendControlRequest(
          ["subtype": .string("list_models")], for: .readingModels(id), sessionReset: sessionReset,
          to: execution)
      }
    case .readingUsage:
      return .success(.finished(sessionReset: sessionReset))
    case .readingModels:
      guard let models: [JSONValue] = response["response"]?["models"]?.arrayValue,
        let model: JSONValue = models.first(where: { item in
          item["disabled"]?.boolValue != true
            && (item["value"]?.stringValue == "haiku"
              || item["resolvedModel"]?.stringValue?.hasPrefix("claude-haiku-") == true)
        }),
        let resolved: String = model["resolvedModel"]?.stringValue
      else {
        return .failure(.modelUnavailable)
      }
      return await sendControlRequest(
        ["subtype": .string("set_model"), "model": .string(resolved)],
        for: .settingModel(UUID().uuidString), sessionReset: sessionReset, to: execution)
    case .settingModel:
      let message: JSONValue = .object([
        "type": .string("user"),
        "session_id": .string(sessionID.uuidString),
        "parent_tool_use_id": .null,
        "message": .object(["role": .string("user"), "content": .string("hi")]),
      ])
      return await send(message, to: execution, closingInput: true).map { _ in
        .awaiting(.greeting, sessionReset: sessionReset)
      }
    case .greeting:
      return .failure(.protocolFailure)
    }
  }

  private static func sendControlRequest(
    _ request: [String: JSONValue], for phase: ClaudeSessionPhase, sessionReset: Date?,
    to execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>,
    closingInput: Bool = false
  ) async -> Result<ClaudeSessionStep, ClaudeFailure> {
    await (phase.requestID.map(Result<String, ClaudeFailure>.success) ?? .failure(.protocolFailure))
      .bind { id -> Result<ClaudeSessionStep, ClaudeFailure> in
        await send(
          .object([
            "type": .string("control_request"), "request_id": .string(id),
            "request": .object(request),
          ]), to: execution, closingInput: closingInput
        )
        .map { _ in .awaiting(phase, sessionReset: sessionReset) }
      }
  }

  private static func send(
    _ message: JSONValue,
    to execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>, closingInput: Bool
  ) async -> Result<Void, ClaudeFailure> {
    await Result { try JSONEncoder().encode(message) + [0x0a] }
      .mapError { _ in ClaudeFailure.protocolFailure }
      .bind { line in
        await Result {
          _ = try await execution.standardInputWriter.write(line.bytes)
          if closingInput { try await execution.standardInputWriter.finish() }
        }
        .mapError { error in ClaudeFailure(process: ProcessRun.failure(error)) }
      }
  }
}
