public import Encoding
public import Foundation
import Results
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

public final class JSONRPCConnection<Method: RawRepresentable<String> & Hashable & Sendable, Failure: Error>: Sendable {
    // --- [STATE]

    private struct State {
        var calls: [JSONValue: AsyncStream<Result<Message<JSONValue, JSONRPCError>, Failure>>.Continuation] = [:]
        var listeners: [Method: [UUID: @Sendable (Result<JSONValue, Failure>) -> Void]] = [:]
        var endings: [UUID: AsyncStream<Failure>.Continuation] = [:]
        var closed: Failure?
    }

    private let write: @Sendable (Data) async throws -> Void
    private let timeout: Duration
    private let failure: @Sendable (JSONRPCFailure) -> Failure
    private let state: Mutex<State> = Mutex(State())
    private let encoder = JSONEncoder()

    private init(write: @escaping @Sendable (Data) async throws -> Void, timeout: Duration, failure: @escaping @Sendable (JSONRPCFailure) -> Failure) {
        (self.write, self.timeout, self.failure) = (write, timeout, failure)
    }

    // --- [LIFECYCLE]

    public static func run<Value>(
        reading lines: sending some AsyncSequence<Data, any Error>,
        writing write: @escaping @Sendable (Data) async throws -> Void,
        timeout: Duration,
        handler: @escaping @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue,
        failure: @escaping @Sendable (JSONRPCFailure) -> Failure,
        _ body: (JSONRPCConnection) async -> Value,
    ) async -> Value {
        let connection = JSONRPCConnection(write: write, timeout: timeout, failure: failure)
        async let _ = connection.read(lines, handler: handler)
        return await body(connection)
    }

    public var isClosed: Bool { state.withLock { state in state.closed != nil } }

    public func waitUntilClosed() async -> Failure {
        let (ends, end) = AsyncStream<Failure>.makeStream()
        let id = UUID()
        end.onTermination = { [weak self] _ in self?.state.withLock { state in state.endings[id] = nil } }
        let closed = state.withLock { state in
            if state.closed == nil { state.endings[id] = end }
            return state.closed
        }
        return if let closed { closed } else { await ends.first { _ in true } ?? failure(JSONRPCFailure(subject: "connection", reason: .cancelled)) }
    }

    // --- [REQUESTS]

    @discardableResult
    public func request<Value: Decodable & Sendable>(
        _ method: Method,
        params: (some Encodable & Sendable)? = Never?.none,
        returning _: Value.Type = JSONValue.self,
    ) async throws(Failure) -> Value {
        let (subject, failure): (String, @Sendable (JSONRPCFailure) -> Failure) = (method.rawValue, failure)
        let id: JSONValue = .string(UUID().uuidString)
        let (replies, reply) = AsyncStream<Result<Message<JSONValue, JSONRPCError>, Failure>>.makeStream()
        try state.withLock { state throws(Failure) in
            if let closed = state.closed { throw closed }
            state.calls[id] = reply
        }
        defer { state.withLock { state in state.calls[id] = nil } }
        try await send(Message<_, Never>(id: id, method: subject, params: params), subject: subject)
        let answer = try await withTimeout(timeout, expired: failure(JSONRPCFailure(subject: subject, reason: .timedOut(timeout)))) {
            () async throws(Failure) in
            try await (replies.first { _ in true } ?? .failure(failure(JSONRPCFailure(subject: subject, reason: .cancelled)))).get()
        }
        if let error = answer.error { throw failure(JSONRPCFailure(subject: subject, reason: .refused(error))) }
        return try Result { try (answer.result ?? .null).decode(Value.self) }.mapError { error in failure(JSONRPCFailure(subject: subject, reason: .unreadable(error))) }.get()
    }

    public func notify(_ method: Method, params: (some Encodable & Sendable)? = Never?.none) async throws(Failure) {
        try await send(Message<_, Never>(method: method.rawValue, params: params), subject: method.rawValue)
    }

    // --- [NOTIFICATIONS]

    public func subscribe<Value: Decodable & Sendable>(
        _ method: Method,
        to _: Value.Type,
        bufferingPolicy: AsyncStream<Result<Value, Failure>>.Continuation.BufferingPolicy = .unbounded,
    ) -> AsyncStream<Result<Value, Failure>> {
        let (values, value) = AsyncStream<Result<Value, Failure>>.makeStream(bufferingPolicy: bufferingPolicy)
        let (id, failure): (UUID, @Sendable (JSONRPCFailure) -> Failure) = (UUID(), failure)
        let listener: @Sendable (Result<JSONValue, Failure>) -> Void = { received in
            switch received {
                case .success(let params):
                    value.yield(Result { try params.decode(Value.self) }.mapError { error in failure(JSONRPCFailure(subject: method.rawValue, reason: .unreadable(error))) })
                case .failure(let ending):
                    value.yield(.failure(ending))
                    value.finish()
            }
        }
        value.onTermination = { [weak self] _ in self?.state.withLock { state in state.listeners[method]?[id] = nil } }
        let closed = state.withLock { state in
            if state.closed == nil { state.listeners[method, default: [:]][id] = listener }
            return state.closed
        }
        if let closed { listener(.failure(closed)) }
        return values
    }

    // --- [TRANSPORT]

    private func read(_ lines: sending some AsyncSequence<Data, any Error>, handler: @escaping @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue) async {
        let decoder = JSONDecoder()
        let ending = await withDiscardingTaskGroup { group in
            defer { group.cancelAll() }
            do {
                for try await request in lines.map({ line in try decoder.decode(Message<JSONValue, JSONRPCError>.self, from: line) }).compactMap(receive) {
                    group.addTask { await self.answer(request, handler: handler) }
                }
                return JSONRPCFailure(subject: "connection", reason: .closed)
            } catch let error as DecodingError {
                return JSONRPCFailure(subject: "connection", reason: .unreadable(error))
            } catch {
                return JSONRPCFailure(subject: "connection", transport: error)
            }
        }
        finish(failure(ending))
    }

    private func receive(_ message: Message<JSONValue, JSONRPCError>) -> (id: JSONValue, method: String, params: JSONValue?)? {
        switch (message.id, message.method) {
            case (.some(let id), .some(let method)): return isClosed ? nil : (id, method, message.params)
            case (.some(let id), .none): state.withLock { state in state.calls.removeValue(forKey: id) }?.yield(.success(message))
            case (.none, .some(let name)):
                let listeners = Method(rawValue: name).map { method in state.withLock { state in Array((state.listeners[method] ?? [:]).values) } } ?? []
                for listener in listeners { listener(.success(message.params ?? .null)) }
            case (.none, .none):
                if let error = message.error { finish(failure(JSONRPCFailure(subject: "connection", reason: .refused(error)))) }
        }
        return nil
    }

    private func answer(
        _ request: (id: JSONValue, method: String, params: JSONValue?),
        handler: @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue,
    ) async {
        let reply: Message<JSONValue, JSONRPCError> =
            switch await Result(catching: { () async throws(JSONRPCError) -> JSONValue in try await handler(request.method, request.params) }) {
                case .success(let result): Message(id: request.id, result: result)
                case .failure(let error): Message(id: request.id, error: error)
            }
        do { try await send(reply, subject: request.method) } catch { finish(error) }
    }

    private func send(_ message: Message<some Encodable & Sendable, some Encodable & Sendable>, subject: String) async throws(Failure) {
        if let closed = state.withLock({ state in state.closed }) { throw closed }
        let line = try Result { try encoder.encodeLine(message) }.mapError { error in failure(JSONRPCFailure(subject: subject, reason: .unencodable(error))) }.get()
        try await Result { try await write(line) }.mapError { error in failure(JSONRPCFailure(subject: subject, transport: error)) }.get()
    }

    private func finish(_ ending: Failure) {
        let taken: State? = state.withLock { state in
            guard state.closed == nil else { return nil }
            defer { state = State(closed: ending) }
            return state
        }
        guard let taken else { return }
        for reply in taken.calls.values { reply.yield(.failure(ending)) }
        for listener in taken.listeners.values.flatMap(\.values) { listener(.failure(ending)) }
        for end in taken.endings.values { end.yield(ending) }
    }
}
