import Foundation
import Subprocess
import Synchronization

actor CodexConnection {
  private enum PendingReply: Sendable {
    case unclaimed
    case arrived(Result<JSONValue, CodexFailure>)
    case awaited(CheckedContinuation<Result<JSONValue, CodexFailure>, Never>)
  }

  private struct NotificationWaiter: Sendable {
    let id: UUID
    let matches: @Sendable (JSONValue) -> Bool
    let continuation: CheckedContinuation<Result<JSONValue, CodexFailure>, Never>
  }

  private struct Registry: Sendable {
    var replies: [String: PendingReply] = [:]
    var waiters: [String: [NotificationWaiter]] = [:]
    var terminalFailure: CodexFailure?
  }

  nonisolated let updates: AsyncStream<CodexProtocol.RateLimitsUpdated>
  private let updatesContinuation: AsyncStream<CodexProtocol.RateLimitsUpdated>.Continuation
  private let execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>
  private let registry: Mutex<Registry> = Mutex(Registry())
  private var nextRequestID: Int = 0
  private var methods: [String: String] = [:]
  private var serverVersion: String = "app-server"
  private var notifications: [String: [JSONValue]] = [:]

  private static let retainedNotifications: Set<String> = [
    "account/login/completed", "turn/completed", "account/updated",
  ]

  init(execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>) {
    self.execution = execution
    let stream = AsyncStream<CodexProtocol.RateLimitsUpdated>.makeStream()
    updates = stream.stream
    updatesContinuation = stream.continuation
  }

  var isFinished: Bool { terminalFailure != nil }

  func read() async {
    do {
      for try await line in execution.standardOutput.strings() {
        await receive(Data(line.utf8))
      }
      finish(.connectionClosed)
    } catch {
      finish(CodexFailure(process: ProcessRun.failure(error)))
    }
  }

  func initialize() async -> Result<Void, CodexFailure> {
    await request(
      "initialize",
      params: .object([
        "clientInfo": .object([
          "name": .string("relay"),
          "title": .string("Relay"),
          "version": .string("1.0"),
        ]),
        "capabilities": .object(["experimentalApi": .bool(true)]),
      ])
    ).bind { result in
      serverVersion = Self.version(in: result["userAgent"]?.stringValue) ?? serverVersion
      return await write(.object(["method": .string("initialized")]))
    }
  }

  private static func version(in userAgent: String?) -> String? {
    userAgent.flatMap { agent in
      agent.firstMatch(of: /^[^\/]+\/(\S+)/).map { match in String(match.1) }
    }
  }

  func request<Value: Decodable & Sendable>(
    _ type: Value.Type, _ method: String, params: JSONValue? = nil
  ) async -> Result<Value, CodexFailure> {
    await request(method, params: params).flatMap { value in
      Self.decode(type, value).map(Result.success)
        ?? .failure(.invalidResponse(field: "\(method) response"))
    }
  }

  func request(_ method: String, params: JSONValue? = nil) async -> Result<
    JSONValue, CodexFailure
  > {
    await ready.bind { _ in
      nextRequestID += 1
      let id: String = String(nextRequestID)
      var message: [String: JSONValue] = ["id": .string(id), "method": .string(method)]
      if let params { message["params"] = params }
      registry.withLock { state in state.replies[id] = .unclaimed }
      methods[id] = method
      if case .failure(let error) = await write(.object(message)) {
        registry.withLock { state in _ = state.replies.removeValue(forKey: id) }
        methods.removeValue(forKey: id)
        return .failure(error)
      }
      let answer: Result<JSONValue, CodexFailure> = await reply(to: id)
      methods.removeValue(forKey: id)
      return answer
    }
  }

  private func reply(to id: String) async -> Result<JSONValue, CodexFailure> {
    await withTaskCancellationHandler {
      await withCheckedContinuation { continuation in
        let settled: Result<JSONValue, CodexFailure>? = registry.withLock { state in
          Self.claim(id, for: continuation, in: &state)
        }
        if let settled { continuation.resume(returning: settled) }
      }
    } onCancel: {
      Self.cancelReply(id, in: self.registry)
    }
  }

  private static func claim(
    _ id: String, for continuation: CheckedContinuation<Result<JSONValue, CodexFailure>, Never>,
    in state: inout Registry
  ) -> Result<JSONValue, CodexFailure>? {
    if let failure: CodexFailure = state.terminalFailure {
      state.replies.removeValue(forKey: id)
      return .failure(failure)
    }
    switch state.replies[id] {
    case .arrived(let reply):
      state.replies.removeValue(forKey: id)
      return reply
    case .unclaimed where !Task.isCancelled:
      state.replies[id] = .awaited(continuation)
      return nil
    case .unclaimed, .awaited, .none:
      state.replies.removeValue(forKey: id)
      return .failure(.cancelled)
    }
  }

  func notification<Value: Decodable & Sendable>(
    _ type: Value.Type, _ method: String,
    matching predicate: @escaping @Sendable (Value) -> Bool
  ) async -> Result<Value, CodexFailure> {
    await notification(method) { params in
      Self.decode(type, params).map(predicate) ?? false
    }
    .flatMap { params in
      Self.decode(type, params).map(Result.success)
        ?? .failure(.invalidResponse(field: "\(method) notification"))
    }
  }

  func notification(
    _ method: String, matching predicate: @escaping @Sendable (JSONValue) -> Bool
  ) async -> Result<JSONValue, CodexFailure> {
    await ready.bind { _ in
      if let index: Int = notifications[method]?.firstIndex(where: predicate),
        let value: JSONValue = notifications[method]?.remove(at: index)
      {
        return .success(value)
      }
      return await notification(method, matching: predicate, waiter: UUID())
    }
  }

  private func notification(
    _ method: String, matching predicate: @escaping @Sendable (JSONValue) -> Bool, waiter id: UUID
  ) async -> Result<JSONValue, CodexFailure> {
    await withTaskCancellationHandler {
      await withCheckedContinuation { continuation in
        let settled: Result<JSONValue, CodexFailure>? = registry.withLock { state in
          Self.register(
            NotificationWaiter(id: id, matches: predicate, continuation: continuation),
            for: method, in: &state)
        }
        if let settled { continuation.resume(returning: settled) }
      }
    } onCancel: {
      Self.cancelWaiter(id, method: method, in: self.registry)
    }
  }

  private static func register(
    _ waiter: NotificationWaiter, for method: String, in state: inout Registry
  ) -> Result<JSONValue, CodexFailure>? {
    if let failure: CodexFailure = state.terminalFailure { return .failure(failure) }
    guard !Task.isCancelled else { return .failure(.cancelled) }
    state.waiters[method, default: []].append(waiter)
    return nil
  }

  func finish(_ failure: CodexFailure) {
    let pending: (replies: [PendingReply], waiters: [NotificationWaiter])? = registry.withLock {
      state in
      guard state.terminalFailure == nil else { return nil }
      state.terminalFailure = failure
      let replies: [PendingReply] = Array(state.replies.values)
      let waiters: [NotificationWaiter] = Array(state.waiters.values.joined())
      state.replies.removeAll()
      state.waiters.removeAll()
      return (replies, waiters)
    }
    guard let pending else { return }
    updatesContinuation.finish()
    notifications.removeAll()
    for case .awaited(let continuation) in pending.replies {
      continuation.resume(returning: .failure(failure))
    }
    for waiter: NotificationWaiter in pending.waiters {
      waiter.continuation.resume(returning: .failure(failure))
    }
  }

  private var terminalFailure: CodexFailure? {
    registry.withLock { state in state.terminalFailure }
  }

  private var ready: Result<Void, CodexFailure> {
    terminalFailure.map { failure in .failure(failure) }
      ?? (Task.isCancelled ? .failure(.cancelled) : .success(()))
  }

  private nonisolated static func cancelReply(
    _ id: String, in registry: borrowing Mutex<Registry>
  ) {
    let awaited: CheckedContinuation<Result<JSONValue, CodexFailure>, Never>? = registry.withLock {
      state in
      guard case .awaited(let continuation) = state.replies.removeValue(forKey: id) else {
        return nil
      }
      return continuation
    }
    awaited?.resume(returning: .failure(.cancelled))
  }

  private nonisolated static func cancelWaiter(
    _ id: UUID, method: String, in registry: borrowing Mutex<Registry>
  ) {
    let waiter: NotificationWaiter? = registry.withLock { state in
      state.waiters[method]?.firstIndex(where: { waiter in waiter.id == id })
        .flatMap { index in state.waiters[method]?.remove(at: index) }
    }
    waiter?.continuation.resume(returning: .failure(.cancelled))
  }

  private static func decode<Value: Decodable>(_ type: Value.Type, _ params: JSONValue) -> Value? {
    (try? JSONEncoder().encode(params)).flatMap { data in
      try? JSONDecoder().decode(type, from: data)
    }
  }

  private func write(_ value: JSONValue) async -> Result<Void, CodexFailure> {
    await Result { try JSONEncoder().encode(value) + [0x0A] }
      .mapError { _ in CodexFailure.invalidResponse(field: "request") }
      .bind { line in
        await Result { _ = try await execution.standardInputWriter.write(line.bytes) }
          .mapError { error in CodexFailure(process: ProcessRun.failure(error)) }
      }
  }

  private func receive(_ data: Data) async {
    guard let message: JSONValue = try? JSONDecoder().decode(JSONValue.self, from: data) else {
      return
    }
    if let id: JSONValue = message["id"], message["method"] != nil {
      let refusal: Result<Void, CodexFailure> = await write(
        .object([
          "id": id,
          "error": .object([
            "code": .number(-32601),
            "message": .string("Relay does not provide agent tools"),
          ]),
        ]))
      if case .failure(let error) = refusal { finish(error) }
      return
    }
    if let id: String = message["id"]?.stringValue {
      let reply: Result<JSONValue, CodexFailure> = Self.reply(
        message, method: methods[id] ?? "request", server: serverVersion)
      let awaited: CheckedContinuation<Result<JSONValue, CodexFailure>, Never>? =
        registry.withLock { state in
          switch state.replies[id] {
          case .unclaimed:
            state.replies[id] = .arrived(reply)
            return nil
          case .awaited(let continuation):
            state.replies.removeValue(forKey: id)
            return continuation
          case .arrived, .none: return nil
          }
        }
      awaited?.resume(returning: reply)
      return
    }
    guard let method: String = message["method"]?.stringValue,
      let params: JSONValue = message["params"]
    else { return }
    if method == "account/rateLimits/updated",
      let updated: CodexProtocol.RateLimitsUpdated = Self.decode(
        CodexProtocol.RateLimitsUpdated.self, params)
    {
      updatesContinuation.yield(updated)
      return
    }
    let waiter: NotificationWaiter? = registry.withLock { state in
      state.waiters[method]?.firstIndex(where: { waiter in waiter.matches(params) })
        .flatMap { index in state.waiters[method]?.remove(at: index) }
    }
    if let waiter {
      waiter.continuation.resume(returning: .success(params))
    } else if Self.retainedNotifications.contains(method) {
      notifications[method, default: []].append(params)
    }
  }

  private static func reply(
    _ message: JSONValue, method: String, server: String
  ) -> Result<JSONValue, CodexFailure> {
    if let error: JSONValue = message["error"] {
      let code: Result<Int, CodexFieldFailure> =
        error["code"]?.intValue.map(Result.success) ?? .failure(CodexFieldFailure("error code"))
      let description: Result<String, CodexFieldFailure> =
        error["message"]?.stringValue.map(Result.success)
        ?? .failure(CodexFieldFailure("error message"))
      return combine(code, description)
        .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
        .flatMap { code, description in
          .failure(
            .requestRejected(method: method, code: code, message: description, server: server))
        }
    }
    return message["result"].map { result in .success(result) }
      ?? .failure(.invalidResponse(field: "request result"))
  }
}
