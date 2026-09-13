import Foundation

actor CodexConnection {
  typealias Reply = Result<JSONValue, CodexFailure>

  // A request's reply slot exists from before its write until its reply is consumed, so a reply
  // that lands while the write is still in flight is kept instead of dropped.
  private enum PendingReply {
    case unclaimed
    case arrived(Reply)
    case awaited(CheckedContinuation<Reply, Never>)
  }

  private struct NotificationWaiter {
    let matches: @Sendable (JSONValue) -> Bool
    let continuation: CheckedContinuation<Reply, Never>
  }

  private static let awaitedNotifications: Set<String> = [
    "account/login/completed", "turn/completed",
  ]

  private let process: NativeProcess
  private var reader: Task<Void, Never>?
  private var nextRequestID: Int = 0
  private var replies: [String: PendingReply] = [:]
  private var notifications: [String: [JSONValue]] = [:]
  private var waiters: [String: [NotificationWaiter]] = [:]
  private var terminalFailure: CodexFailure?

  init(process: NativeProcess) {
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

  func request(_ method: String, params: JSONValue? = nil) async -> Reply {
    if let terminalFailure { return .failure(terminalFailure) }
    if Task.isCancelled { return .failure(.cancelled) }
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

  func notification(
    _ method: String,
    matching predicate: @escaping @Sendable (JSONValue) -> Bool
  ) async -> Reply {
    if let terminalFailure { return .failure(terminalFailure) }
    if Task.isCancelled { return .failure(.cancelled) }
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

  func cancel() async {
    finish(.cancelled)
    await process.cancel()
    await stopReading()
  }

  func close() async {
    finish(.connectionClosed)
    await process.closeInput()
    await process.cancel()
    await stopReading()
  }

  private func stopReading() async {
    let source: Task<Void, Never>? = reader
    reader = nil
    source?.cancel()
    await source?.value
  }

  private func write(_ value: JSONValue) async -> Result<Void, CodexFailure> {
    let data: Data
    do {
      var encoded: Data = try JSONEncoder().encode(value)
      encoded.append(0x0A)
      data = encoded
    } catch {
      return .failure(.invalidResponse(field: "request"))
    }
    return await process.send(data).mapError(CodexFailure.process)
  }

  private func receive(_ data: Data) async {
    let message: JSONValue
    do {
      message = try JSONDecoder().decode(JSONValue.self, from: data)
    } catch {
      finish(.invalidResponse(field: "protocol message"))
      return
    }
    if let id: JSONValue = message["id"], message["method"] != nil {
      _ = await write(
        .object([
          "id": id,
          "error": .object([
            "code": .number(-32601),
            "message": .string("Relay does not provide agent tools."),
          ]),
        ]))
      return
    }
    if let id: String = message["id"]?.stringValue, let pending: PendingReply = replies[id] {
      let reply: Reply = Self.reply(message)
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

  private static func reply(_ message: JSONValue) -> Reply {
    if let error: JSONValue = message["error"] {
      guard let code: Int = error["code"]?.intValue,
        let description: String = error["message"]?.stringValue
      else { return .failure(.invalidResponse(field: "request error")) }
      return .failure(.requestRejected(code: code, message: description))
    }
    guard let result: JSONValue = message["result"] else {
      return .failure(.invalidResponse(field: "request result"))
    }
    return .success(result)
  }

  private func finish(_ failure: CodexFailure) {
    guard terminalFailure == nil else { return }
    terminalFailure = failure
    let pendingReplies: [PendingReply] = Array(replies.values)
    replies.removeAll()
    for pending: PendingReply in pendingReplies {
      if case .awaited(let continuation) = pending {
        continuation.resume(returning: .failure(failure))
      }
    }
    let pendingWaiters: [NotificationWaiter] = waiters.values.flatMap { values in values }
    waiters.removeAll()
    notifications.removeAll()
    for waiter: NotificationWaiter in pendingWaiters {
      waiter.continuation.resume(returning: .failure(failure))
    }
  }
}

extension Result {
  // The asynchronous bind, `flatMap` stays the synchronous form the standard library declares.
  func bind<NewSuccess>(
    _ transform: (Success) async -> Result<NewSuccess, Failure>
  ) async -> Result<NewSuccess, Failure> {
    switch self {
    case .success(let value): await transform(value)
    case .failure(let error): .failure(error)
    }
  }
}
