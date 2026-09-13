import Foundation

actor CodexConnection {
  private enum PendingReply {
    case unclaimed
    case arrived(Result<JSONValue, CodexFailure>)
    case awaited(CheckedContinuation<Result<JSONValue, CodexFailure>, Never>)
  }

  private struct NotificationWaiter {
    let matches: @Sendable (JSONValue) -> Bool
    let continuation: CheckedContinuation<Result<JSONValue, CodexFailure>, Never>
  }

  private static let awaitedNotifications: Set<String> = [
    "account/login/completed", "turn/completed",
  ]

  private let process: ChildProcess
  private var reader: Task<Void, Never>?
  private var nextRequestID: Int = 0
  private var replies: [String: PendingReply] = [:]
  private var notifications: [String: [JSONValue]] = [:]
  private var waiters: [String: [NotificationWaiter]] = [:]
  private var terminalFailure: CodexFailure?

  init(process: ChildProcess) {
    self.process = process
  }

  func initialize() async -> Result<Void, CodexFailure> {
    let lines = process.lines
    reader = Task { [weak self] in
      for await line in lines {
        guard let self else { return }
        switch line {
        case .success(let data): await self.receive(data)
        case .failure(let error): await self.finish(.process(error))
        }
      }
      await self?.finish(.connectionClosed)
    }
    return await request(
      "initialize",
      params: .object([
        "clientInfo": .object([
          "name": .string("relay"),
          "title": .string("Relay"),
          "version": .string("1.0"),
        ]),
        "capabilities": .object(["experimentalApi": .bool(true)]),
      ])
    ).bind { _ in
      await write(.object(["method": .string("initialized")]))
    }
  }

  func request(_ method: String, params: JSONValue? = nil) async -> Result<
    JSONValue, CodexFailure
  > {
    return await ready.bind { _ in
      nextRequestID += 1
      let id: String = String(nextRequestID)
      var message: [String: JSONValue] = ["id": .string(id), "method": .string(method)]
      if let params { message["params"] = params }
      replies[id] = .unclaimed
      if case .failure(let error) = await write(.object(message)) {
        replies.removeValue(forKey: id)
        return .failure(error)
      }
      if let terminalFailure {
        replies.removeValue(forKey: id)
        return .failure(terminalFailure)
      }
      if case .arrived(let reply) = replies[id] {
        replies.removeValue(forKey: id)
        return reply
      }
      return await withTaskCancellationHandler {
        await withCheckedContinuation { continuation in
          replies[id] = .awaited(continuation)
        }
      } onCancel: {
        Task { await self.cancel() }
      }
    }
  }

  func notification(
    _ method: String,
    matching predicate: @escaping @Sendable (JSONValue) -> Bool
  ) async -> Result<JSONValue, CodexFailure> {
    return await ready.bind { _ in
      if let index: Int = notifications[method]?.firstIndex(where: predicate),
        let value: JSONValue = notifications[method]?.remove(at: index)
      {
        return .success(value)
      }
      return await withTaskCancellationHandler {
        await withCheckedContinuation { continuation in
          waiters[method, default: []].append(
            NotificationWaiter(matches: predicate, continuation: continuation))
        }
      } onCancel: {
        Task { await self.cancel() }
      }
    }
  }

  private var ready: Result<Void, CodexFailure> {
    terminalFailure.map { failure in .failure(failure) }
      ?? (Task.isCancelled ? .failure(.cancelled) : .success(()))
  }

  func cancel() async {
    await shutdown(.cancelled)
  }

  func close() async {
    await shutdown(.connectionClosed)
  }

  private func shutdown(_ failure: CodexFailure) async {
    finish(failure)
    await process.cancel()
    let source: Task<Void, Never>? = reader
    reader = nil
    source?.cancel()
    await source?.value
  }

  private func write(_ value: JSONValue) async -> Result<Void, CodexFailure> {
    return await Result { try JSONEncoder().encode(value) + [0x0A] }
      .mapError { _ in CodexFailure.invalidResponse(field: "request") }
      .bind { data in await process.send(data).mapError(CodexFailure.process) }
  }

  private func receive(_ data: Data) async {
    guard let message: JSONValue = try? JSONDecoder().decode(JSONValue.self, from: data) else {
      finish(.invalidResponse(field: "protocol message"))
      return
    }
    if let id: JSONValue = message["id"], message["method"] != nil {
      let refusal: Result<Void, CodexFailure> = await write(
        .object([
          "id": id,
          "error": .object([
            "code": .number(-32601),
            "message": .string("Relay does not provide agent tools."),
          ]),
        ]))
      if case .failure(let error) = refusal { finish(error) }
      return
    }
    if let id: String = message["id"]?.stringValue, let pending: PendingReply = replies[id] {
      let reply: Result<JSONValue, CodexFailure> = Self.reply(message)
      switch pending {
      case .unclaimed: replies[id] = .arrived(reply)
      case .arrived: break
      case .awaited(let continuation):
        replies.removeValue(forKey: id)
        continuation.resume(returning: reply)
      }
      return
    }
    guard let method: String = message["method"]?.stringValue,
      let params: JSONValue = message["params"],
      Self.awaitedNotifications.contains(method)
    else { return }
    if let index: Int = waiters[method]?.firstIndex(where: { waiter in waiter.matches(params) }),
      let waiter: NotificationWaiter = waiters[method]?.remove(at: index)
    {
      waiter.continuation.resume(returning: .success(params))
    } else {
      notifications[method, default: []].append(params)
    }
  }

  private static func reply(_ message: JSONValue) -> Result<JSONValue, CodexFailure> {
    if let error: JSONValue = message["error"] {
      let code: Result<Int, CodexFieldFailure> =
        error["code"]?.intValue.map(Result.success) ?? .failure(CodexFieldFailure("error code"))
      let description: Result<String, CodexFieldFailure> =
        error["message"]?.stringValue.map(Result.success)
        ?? .failure(CodexFieldFailure("error message"))
      return combine(code, description)
        .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
        .flatMap { code, description in .failure(.requestRejected(code: code, message: description))
        }
    }
    return message["result"].map { result in .success(result) }
      ?? .failure(.invalidResponse(field: "request result"))
  }

  private func finish(_ failure: CodexFailure) {
    guard terminalFailure == nil else { return }
    terminalFailure = failure
    let pendingReplies: [PendingReply] = Array(replies.values)
    replies.removeAll()
    for case .awaited(let continuation) in pendingReplies {
      continuation.resume(returning: .failure(failure))
    }
    let pendingWaiters: [NotificationWaiter] = Array(waiters.values.joined())
    waiters.removeAll()
    notifications.removeAll()
    for waiter: NotificationWaiter in pendingWaiters {
      waiter.continuation.resume(returning: .failure(failure))
    }
  }
}
