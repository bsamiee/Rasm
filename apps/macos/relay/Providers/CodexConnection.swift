import Foundation
import JSON
import Results
import Subprocess

// --- [SERVICES] ------------------------------------------------------------------------

actor CodexConnection {
    // --- [STATE]
    private struct PendingReply: Sendable {
        let method: CodexProtocol.Method
        let continuation: AsyncStream<Result<Data, CodexFailure>>.Continuation
    }

    private struct NotificationWaiter: Sendable {
        let id: UUID
        let accept: @Sendable (Data) -> Bool
        let fail: @Sendable (CodexFailure) -> Void
    }

    static let decoder: JSONDecoder = {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.dateDecodingStrategy = .secondsSince1970
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return decoder
    }()

    nonisolated let updates: AsyncStream<CodexProtocol.RateLimitsUpdated>
    private let updatesContinuation: AsyncStream<CodexProtocol.RateLimitsUpdated>.Continuation
    private let execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>
    private var nextRequestID: Int = 0
    private var replies: [JSONValue: PendingReply] = [:]
    private var waiters: [CodexProtocol.Method: [NotificationWaiter]] = [:]
    private var notifications: [CodexProtocol.Method: [Data]] = [:]
    private var serverVersion: String?
    private var terminalFailure: CodexFailure?

    init(execution: Execution<CustomWriteInput, SequenceOutput, DiscardedOutput>) {
        self.execution = execution
        (updates, updatesContinuation) = AsyncStream<CodexProtocol.RateLimitsUpdated>.makeStream()
    }

    var isFinished: Bool { terminalFailure != nil }

    private func checkOpen() throws(CodexFailure) {
        if let terminalFailure { throw terminalFailure }
        guard !Task.isCancelled else { throw .cancelled }
    }

    // --- [REQUESTS]
    func initialize() async throws(CodexFailure) {
        let result: CodexProtocol.InitializeResult = try await request(
            CodexProtocol.InitializeResult.self,
            .initialize,
            params: CodexProtocol.InitializeParams(
                clientInfo: CodexProtocol.InitializeParams.ClientInfo(name: "relay", title: "Relay", version: "1.0"),
                capabilities: CodexProtocol.InitializeParams.Capabilities(experimentalApi: true),
            ),
        )
        serverVersion = result.userAgent?.firstMatch(of: #/^[^/]+/(\S+)/#).map { match in String(match.1) }
        try await write(CodexProtocol.Message<JSONValue>(method: Lenient(value: .initialized)))
    }

    func request<Value: Decodable & Sendable, Params: Encodable & Sendable>(
        _: Value.Type,
        _ method: CodexProtocol.Method,
        params: Params?,
    ) async throws(CodexFailure) -> Value {
        let line: Data = try await exchange(method, params: params)
        let message: CodexProtocol.Message<Value> = try Result { try Self.decoder.decode(CodexProtocol.Message<Value>.self, from: line) }
            .mapError { error in CodexFailure.invalidResponse(method, error) }
            .get()
        guard let value: Value = message.result else { throw .invalidResponse(method, nil) }
        return value
    }

    func request<Params: Encodable & Sendable>(
        _ method: CodexProtocol.Method,
        params: Params? = Never?.none,
    ) async throws(CodexFailure) {
        _ = try await exchange(method, params: params)
    }

    func pages<Item: Decodable & Sendable>(
        _: Item.Type,
        _ method: CodexProtocol.Method,
        cursor: String? = nil,
    ) async throws(CodexFailure) -> [Item] {
        let page: CodexProtocol.Page<Item> = try await request(CodexProtocol.Page<Item>.self, method, params: CodexProtocol.PageParams(cursor: cursor))
        let rest: [Item] = if let next: String = page.nextCursor { try await pages(Item.self, method, cursor: next) } else { [] }
        return page.data + rest
    }

    private func exchange<Params: Encodable & Sendable>(
        _ method: CodexProtocol.Method,
        params: Params?,
    ) async throws(CodexFailure) -> Data {
        try checkOpen()
        nextRequestID += 1
        let id: JSONValue = .string(String(nextRequestID))
        let (answers, answer): (AsyncStream<Result<Data, CodexFailure>>, AsyncStream<Result<Data, CodexFailure>>.Continuation) = AsyncStream.makeStream()
        replies[id] = PendingReply(method: method, continuation: answer)
        defer { replies[id] = nil }
        try await write(CodexProtocol.Message(id: id, method: Lenient(value: method), params: params))
        return try await withTimeout(.seconds(60)) { await answers.first(where: { _ in true }) ?? .failure(.cancelled) }.get()
    }

    // --- [NOTIFICATIONS]
    func notification<Value: Decodable & Sendable>(
        _: Value.Type,
        _ method: CodexProtocol.Method,
        matching predicate: @escaping @Sendable (Value) -> Bool,
    ) async throws(CodexFailure) -> Value {
        let matched: @Sendable (Data) -> Value? = { line in
            (try? Self.decoder.decode(CodexProtocol.Message<Value>.self, from: line))?.params.flatMap { value in predicate(value) ? value : nil }
        }
        try checkOpen()
        if let retained: (index: Int, value: Value) = notifications[method]?.enumerated().lazy
            .compactMap({ index, line in matched(line).map { value in (index, value) } }).first
        {
            notifications[method]?.remove(at: retained.index)
            return retained.value
        }
        let id: UUID = UUID()
        let (values, value): (AsyncStream<Result<Value, CodexFailure>>, AsyncStream<Result<Value, CodexFailure>>.Continuation) = AsyncStream.makeStream()
        waiters[method, default: []].append(
            NotificationWaiter(
                id: id,
                accept: { line in matched(line).map { found in value.yield(.success(found)) } != nil },
                fail: { failure in value.yield(.failure(failure)) },
            )
        )
        defer { waiters[method]?.removeAll { waiter in waiter.id == id } }
        return try await (values.first(where: { _ in true }) ?? .failure(.cancelled)).get()
    }

    // --- [TRANSPORT]
    func read() async {
        do {
            for try await text: String in execution.standardOutput.strings() {
                let line: Data = Data(text.utf8)
                let decoded: Result<CodexProtocol.Message<CodexProtocol.Skip>, any Error> = Result { try Self.decoder.decode(CodexProtocol.Message<CodexProtocol.Skip>.self, from: line) }
                switch decoded.map({ message in (message.id, message.method.value, message.error) }) {
                    case .success((.some(let id), .some, _)):
                        do throws(CodexFailure) {
                            try await write(CodexProtocol.Message<JSONValue>(id: id, error: CodexProtocol.ErrorObject(code: -32601, message: "Relay does not provide agent tools")))
                        } catch {
                            finish(error)
                        }
                    case .success((.some(let id), .none, let error)):
                        guard let pending: PendingReply = replies.removeValue(forKey: id) else { continue }
                        pending.continuation.yield(
                            error.map { error in .failure(.requestRejected(method: pending.method, code: error.code, message: error.message, serverVersion: serverVersion)) }
                                ?? .success(line)
                        )
                    case .success((.none, .accountRateLimitsUpdated?, _)):
                        switch Result(catching: { try Self.decoder.decode(CodexProtocol.Message<CodexProtocol.RateLimitsUpdated>.self, from: line).params }) {
                            case .success(.some(let updated)): updatesContinuation.yield(updated)
                            case .success(.none): continue
                            case .failure(let error): finish(.invalidResponse(.accountRateLimitsUpdated, error))
                        }
                    case .success((.none, .some(let method), _)):
                        if let index: Int = waiters[method]?.firstIndex(where: { waiter in waiter.accept(line) }) {
                            waiters[method]?.remove(at: index)
                        } else if [.accountLoginCompleted, .turnCompleted].contains(method) {
                            notifications[method, default: []].append(line)
                        }
                    case .success((.none, .none, _)): continue
                    case .failure(let error): finish(.invalidResponse(nil, error))
                }
            }
            finish(.connectionClosed)
        } catch {
            finish(CodexFailure(process: error))
        }
    }

    func finish(_ failure: CodexFailure) {
        guard terminalFailure == nil else { return }
        terminalFailure = failure
        updatesContinuation.finish()
        notifications.removeAll()
        for pending: PendingReply in replies.values {
            pending.continuation.yield(.failure(failure))
        }
        replies.removeAll()
        for waiter: NotificationWaiter in waiters.values.joined() {
            waiter.fail(failure)
        }
        waiters.removeAll()
    }

    private func write(_ value: some Encodable) async throws(CodexFailure) {
        do {
            _ = try await execution.standardInputWriter.write((JSONEncoder().encode(value) + [UInt8(ascii: "\n")]).bytes)
        } catch {
            throw CodexFailure(process: error)
        }
    }
}
