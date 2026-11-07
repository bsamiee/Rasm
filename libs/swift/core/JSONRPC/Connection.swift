public import Encoding
public import Foundation
import Results

// --- [SERVICES] ------------------------------------------------------------------------

public actor JSONRPCConnection<Method: RawRepresentable<String> & Hashable & Sendable, Failure: Error> {
    // --- [STATE]

    private struct Listener: Sendable {
        let receive: @Sendable (JSONValue) throws -> Bool
        let close: @Sendable (Failure) -> Void
    }

    private let write: @Sendable (Data) async throws -> Void
    private let timeout: Duration
    private let failure: @Sendable (JSONRPCFailure) -> Failure
    private var calls: [JSONValue: AsyncStream<Result<Envelope<JSONValue, JSONRPCError>, Failure>>.Continuation] = [:]
    private var listeners: [Method: [UUID: Listener]] = [:]
    private var retained: [Method: [JSONValue]] = [:]
    private var endings: [AsyncStream<Failure>.Continuation] = []
    private var terminal: Failure?

    private init(write: @escaping @Sendable (Data) async throws -> Void, timeout: Duration, failure: @escaping @Sendable (JSONRPCFailure) -> Failure) {
        (self.write, self.timeout, self.failure) = (write, timeout, failure)
    }

    // --- [LIFECYCLE]

    public static func run<Value>(
        reading lines: sending some AsyncSequence<Data, any Error>,
        writing write: @escaping @Sendable (Data) async throws -> Void,
        timeout: Duration,
        retaining: Set<Method>,
        serving: @escaping @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue,
        failure: @escaping @Sendable (JSONRPCFailure) -> Failure,
        _ body: (JSONRPCConnection) async -> Value,
    ) async -> Value {
        let connection: JSONRPCConnection = JSONRPCConnection(write: write, timeout: timeout, failure: failure)
        async let _: Void = {
            let decoder: JSONDecoder = JSONDecoder()
            do {
                for try await line: Data in lines {
                    switch Result(catching: { try decoder.decode(Envelope<JSONValue, JSONRPCError>.self, from: line) }) {
                        case .success(let envelope): await connection.receive(envelope, retaining: retaining, serving: serving)
                        case .failure(let error): await connection.finish(failure(JSONRPCFailure(subject: "connection", reason: .unreadable(error))))
                    }
                }
                await connection.finish(failure(JSONRPCFailure(subject: "connection", reason: .closed)))
            } catch {
                await connection.finish(failure(JSONRPCFailure(subject: "connection", transport: error)))
            }
        }()
        return await body(connection)
    }

    public var isFinished: Bool { terminal != nil }

    public func finished() async -> Failure {
        if let terminal { return terminal }
        let (ends, end): (AsyncStream<Failure>, AsyncStream<Failure>.Continuation) = AsyncStream.makeStream()
        endings.append(end)
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
        calls[id] = reply
        defer { calls[id] = nil }
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

    public func notification<Value: Decodable & Sendable>(
        _ method: Method,
        returning _: Value.Type,
        matching predicate: @escaping @Sendable (Value) -> Bool,
    ) async throws(Failure) -> Value {
        let id: UUID = UUID()
        let (values, value): (AsyncStream<Result<Value, Failure>>, AsyncStream<Result<Value, Failure>>.Continuation) = AsyncStream.makeStream()
        let listener: Listener = Listener(
            receive: { params in
                let found: Value = try params.decode(Value.self)
                guard predicate(found) else { return false }
                value.yield(.success(found))
                return true
            },
            close: { failure in value.yield(.failure(failure)) },
        )
        defer { listeners[method]?[id] = nil }
        switch Result(catching: { try retained[method]?.firstIndex(where: listener.receive) }) {
            case .success(.some(let index)): retained[method]?.remove(at: index)
            case .success(.none): listeners[method, default: [:]][id] = listener
            case .failure(let error): finish(failure(JSONRPCFailure(subject: method.rawValue, reason: .unreadable(error))))
        }
        if let terminal { throw terminal }
        return try await (values.first { _ in true } ?? .failure(failure(JSONRPCFailure(subject: method.rawValue, reason: .cancelled(CancellationError()))))).get()
    }

    public func subscribe<Value: Decodable & Sendable>(_ method: Method, to _: Value.Type) -> AsyncStream<Value> {
        let (values, value): (AsyncStream<Value>, AsyncStream<Value>.Continuation) = AsyncStream.makeStream()
        let listener: Listener = Listener(
            receive: { params in
                try value.yield(params.decode(Value.self))
                return false
            },
            close: { _ in value.finish() },
        )
        if terminal == nil { listeners[method, default: [:]][UUID()] = listener } else { value.finish() }
        return values
    }

    // --- [TRANSPORT]

    private func receive(
        _ envelope: Envelope<JSONValue, JSONRPCError>,
        retaining: Set<Method>,
        serving: @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue,
    ) async {
        switch (envelope.id, envelope.method) {
            case (.some(let id), .some(let method)):
                let reply: Envelope<JSONValue, JSONRPCError> =
                    switch await Result(catching: { () async throws(JSONRPCError) -> JSONValue in try await serving(method, envelope.params) }) {
                        case .success(let result): Envelope(id: id, result: result)
                        case .failure(let error): Envelope(id: id, error: error)
                    }
                do { try await send(reply, subject: method) } catch { finish(error) }
            case (.some(let id), .none): calls.removeValue(forKey: id)?.yield(.success(envelope))
            case (.none, .some(let name)):
                guard let method: Method = Method(rawValue: name), let params: JSONValue = envelope.params else { return }
                let waiting: [UUID: Listener] = listeners[method] ?? [:]
                switch Result(catching: { try waiting.filter { _, listener in try !listener.receive(params) } }) {
                    case .success(let remaining) where remaining.count == waiting.count && retaining.contains(method):
                        listeners[method] = remaining
                        retained[method, default: []].append(params)
                    case .success(let remaining): listeners[method] = remaining
                    case .failure(let error): finish(failure(JSONRPCFailure(subject: name, reason: .unreadable(error))))
                }
            case (.none, .none):
                if let error: JSONRPCError = envelope.error { finish(failure(JSONRPCFailure(subject: "connection", reason: .refused(error)))) }
        }
    }

    private func send(_ envelope: Envelope<some Encodable & Sendable, some Encodable & Sendable>, subject: String) async throws(Failure) {
        if let terminal { throw terminal }
        let line: Data = try Result { try JSONEncoder().encodeLine(envelope) }.mapError { error in failure(JSONRPCFailure(subject: subject, reason: .unencodable(error))) }.get()
        try await Result { try await write(line) }.mapError { error in failure(JSONRPCFailure(subject: subject, transport: error)) }.get()
    }

    private func finish(_ ending: Failure) {
        guard terminal == nil else { return }
        terminal = ending
        for reply: AsyncStream<Result<Envelope<JSONValue, JSONRPCError>, Failure>>.Continuation in calls.values { reply.yield(.failure(ending)) }
        for listener: Listener in listeners.values.flatMap(\.values) { listener.close(ending) }
        for end: AsyncStream<Failure>.Continuation in endings { end.yield(ending) }
        (calls, listeners, retained, endings) = ([:], [:], [:], [])
    }
}
