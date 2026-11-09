public import Encoding
public import Foundation
import Results
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

public final class JSONRPCConnection<Method: RawRepresentable<String> & Hashable & Sendable, Failure: Error>: Sendable {
    // --- [STATE]

    private struct State {
        var calls: [JSONValue: AsyncStream<Result<Envelope<JSONValue, JSONRPCError>, Failure>>.Continuation] = [:]
        var listeners: [Method: [UUID: @Sendable (Result<JSONValue, Failure>) -> Void]] = [:]
        var endings: [UUID: AsyncStream<Failure>.Continuation] = [:]
        var terminal: Failure?
    }

    private let write: @Sendable (Data) async throws -> Void
    private let timeout: Duration
    private let failure: @Sendable (JSONRPCFailure) -> Failure
    private let state: Mutex<State> = Mutex(State())

    private init(write: @escaping @Sendable (Data) async throws -> Void, timeout: Duration, failure: @escaping @Sendable (JSONRPCFailure) -> Failure) {
        (self.write, self.timeout, self.failure) = (write, timeout, failure)
    }

    // --- [LIFECYCLE]

    public static func run<Value>(
        reading lines: sending some AsyncSequence<Data, any Error>,
        writing write: @escaping @Sendable (Data) async throws -> Void,
        timeout: Duration,
        serving: @escaping @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue,
        failure: @escaping @Sendable (JSONRPCFailure) -> Failure,
        _ body: (JSONRPCConnection) async -> Value,
    ) async -> Value {
        let connection: JSONRPCConnection = JSONRPCConnection(write: write, timeout: timeout, failure: failure)
        async let _: Void = connection.read(lines, serving: serving)
        return await body(connection)
    }

    public var isFinished: Bool { state.withLock { state in state.terminal != nil } }

    public func finished() async -> Failure {
        let (ends, end): (AsyncStream<Failure>, AsyncStream<Failure>.Continuation) = AsyncStream.makeStream()
        let id: UUID = UUID()
        end.onTermination = { [weak self] _ in self?.state.withLock { state in state.endings[id] = nil } }
        let terminal: Failure? = state.withLock { state in
            if state.terminal == nil { state.endings[id] = end }
            return state.terminal
        }
        if let terminal { return terminal }
        return await ends.first { _ in true } ?? failure(JSONRPCFailure(subject: "connection", reason: .cancelled(CancellationError())))
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
        let (replies, reply): (AsyncStream<Result<Envelope<JSONValue, JSONRPCError>, Failure>>, AsyncStream<Result<Envelope<JSONValue, JSONRPCError>, Failure>>.Continuation) =
            AsyncStream.makeStream()
        try state.withLock { state throws(Failure) in
            if let terminal: Failure = state.terminal { throw terminal }
            state.calls[id] = reply
        }
        defer { state.withLock { state in state.calls[id] = nil } }
        try await send(Envelope<_, Never>(id: id, method: subject, params: params), subject: subject)
        let answer: Envelope<JSONValue, JSONRPCError> = try await withTimeout(timeout, expired: failure(JSONRPCFailure(subject: subject, reason: .timedOut(timeout)))) {
            () async throws(Failure) in
            try await (replies.first { _ in true } ?? .failure(failure(JSONRPCFailure(subject: subject, reason: .cancelled(CancellationError()))))).get()
        }
        if let error: JSONRPCError = answer.error { throw failure(JSONRPCFailure(subject: subject, reason: .refused(error))) }
        return try Result { try (answer.result ?? .null).decode(Value.self) }.mapError { error in failure(JSONRPCFailure(subject: subject, reason: .unreadable(error))) }.get()
    }

    public func notify(_ method: Method, params: (some Encodable & Sendable)? = Never?.none) async throws(Failure) {
        try await send(Envelope<_, Never>(method: method.rawValue, params: params), subject: method.rawValue)
    }

    // --- [NOTIFICATIONS]

    public func subscribe<Value: Decodable & Sendable>(
        _ method: Method,
        to _: Value.Type,
        bufferingPolicy: AsyncStream<Result<Value, Failure>>.Continuation.BufferingPolicy = .unbounded,
    ) -> AsyncStream<Result<Value, Failure>> {
        let (values, value): (AsyncStream<Result<Value, Failure>>, AsyncStream<Result<Value, Failure>>.Continuation) = AsyncStream.makeStream(bufferingPolicy: bufferingPolicy)
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
        let terminal: Failure? = state.withLock { state in
            if state.terminal == nil { state.listeners[method, default: [:]][id] = listener }
            return state.terminal
        }
        if let terminal { listener(.failure(terminal)) }
        return values
    }

    // --- [TRANSPORT]

    private func read(_ lines: sending some AsyncSequence<Data, any Error>, serving: @escaping @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue) async {
        let decoder: JSONDecoder = JSONDecoder()
        let ending: JSONRPCFailure = await withDiscardingTaskGroup { group in
            defer { group.cancelAll() }
            do {
                for try await request in lines.map({ line in try decoder.decode(Envelope<JSONValue, JSONRPCError>.self, from: line) }).compactMap(receive) {
                    group.addTask { await self.answer(request, serving: serving) }
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

    private func receive(_ envelope: Envelope<JSONValue, JSONRPCError>) -> (id: JSONValue, method: String, params: JSONValue?)? {
        switch (envelope.id, envelope.method) {
            case (.some(let id), .some(let method)): return isFinished ? nil : (id, method, envelope.params)
            case (.some(let id), .none): state.withLock { state in state.calls.removeValue(forKey: id) }?.yield(.success(envelope))
            case (.none, .some(let name)):
                let listeners: [@Sendable (Result<JSONValue, Failure>) -> Void] =
                    Method(rawValue: name).map { method in state.withLock { state in Array((state.listeners[method] ?? [:]).values) } } ?? []
                for listener: @Sendable (Result<JSONValue, Failure>) -> Void in listeners { listener(.success(envelope.params ?? .null)) }
            case (.none, .none):
                if let error: JSONRPCError = envelope.error { finish(failure(JSONRPCFailure(subject: "connection", reason: .refused(error)))) }
        }
        return nil
    }

    private func answer(
        _ request: (id: JSONValue, method: String, params: JSONValue?),
        serving: @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue,
    ) async {
        let reply: Envelope<JSONValue, JSONRPCError> =
            switch await Result(catching: { () async throws(JSONRPCError) -> JSONValue in try await serving(request.method, request.params) }) {
                case .success(let result): Envelope(id: request.id, result: result)
                case .failure(let error): Envelope(id: request.id, error: error)
            }
        do { try await send(reply, subject: request.method) } catch { finish(error) }
    }

    private func send(_ envelope: Envelope<some Encodable & Sendable, some Encodable & Sendable>, subject: String) async throws(Failure) {
        if let terminal: Failure = state.withLock({ state in state.terminal }) { throw terminal }
        let line: Data = try Result { try JSONEncoder().encodeLine(envelope) }.mapError { error in failure(JSONRPCFailure(subject: subject, reason: .unencodable(error))) }.get()
        try await Result { try await write(line) }.mapError { error in failure(JSONRPCFailure(subject: subject, transport: error)) }.get()
    }

    private func finish(_ ending: Failure) {
        let taken: State? = state.withLock { state in
            guard state.terminal == nil else { return nil }
            defer { state = State(terminal: ending) }
            return state
        }
        guard let taken else { return }
        for reply: AsyncStream<Result<Envelope<JSONValue, JSONRPCError>, Failure>>.Continuation in taken.calls.values { reply.yield(.failure(ending)) }
        for listener: @Sendable (Result<JSONValue, Failure>) -> Void in taken.listeners.values.flatMap(\.values) { listener(.failure(ending)) }
        for end: AsyncStream<Failure>.Continuation in taken.endings.values { end.yield(ending) }
    }
}
