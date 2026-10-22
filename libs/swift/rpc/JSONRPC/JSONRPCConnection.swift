public import Encoding
public import Foundation
import OrderedCollections
public import Results

// --- [MODELS] --------------------------------------------------------------------------

private struct Message<Payload: Sendable>: Sendable {
    var jsonrpc: String? = "2.0"
    var id: JSONValue?
    var method: String?
    var params: Payload?
    var result: Payload?
    var error: JSONRPCError?
}

extension Message: Decodable where Payload: Decodable {}

extension Message: Encodable where Payload: Encodable {}

private struct Answer<Value: Decodable>: Decodable {
    let result: Value
}

private struct Skip: Decodable, Sendable {
    init(from _: any Decoder) {}
}

// --- [ERRORS] --------------------------------------------------------------------------

public struct JSONRPCError: Error, Codable {
    public let code: Int
    public let message: String
    public let data: JSONValue?

    public init(code: Int, message: String, data: JSONValue?) { (self.code, self.message, self.data) = (code, message, data) }
}

public enum JSONRPCFailure<Method: RawRepresentable & Sendable>: CustomNSError, LocalizedError where Method.RawValue == String {
    case transport(any Error)
    case closed
    case invalidMessage(Method?, any Error)
    case error(Method?, JSONRPCError)

    public var errorDescription: String? {
        switch self {
            case .transport(let error): error.localizedDescription
            case .closed: "Connection closed"
            case .invalidMessage(.some(let method), _): "Unreadable \(method.rawValue) message"
            case .invalidMessage(.none, _): "Unreadable message"
            case .error(let method, let error): "\(method?.rawValue ?? "Request") error \(error.code): \(error.message)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .transport(let error), .invalidMessage(_, let error): [NSUnderlyingErrorKey: error]
            case .closed, .error: [:]
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

public actor JSONRPCConnection<Method: RawRepresentable & Hashable & Sendable, Failure: DeadlineFailure> where Method.RawValue == String {
    // --- [STATE]
    private struct Call: Sendable {
        let method: Method
        let answer: AsyncStream<Result<Data, Failure>>.Continuation
    }

    private struct Listener: Sendable {
        let receive: @Sendable (Data) throws -> Bool
        let close: @Sendable (Failure) -> Void
    }

    private let write: @Sendable (Data) async throws -> Void
    private let decoder: JSONDecoder
    private let timeout: Duration
    private let failure: @Sendable (JSONRPCFailure<Method>) -> Failure
    private var calls: [JSONValue: Call] = [:]
    private var listeners: [Method: OrderedDictionary<UUID, Listener>] = [:]
    private var retained: [Method: [Data]] = [:]
    private var terminal: Failure?

    private init(
        write: @escaping @Sendable (Data) async throws -> Void,
        decoder: JSONDecoder,
        timeout: Duration,
        failure: @escaping @Sendable (JSONRPCFailure<Method>) -> Failure,
    ) {
        self.write = write
        self.decoder = decoder
        self.timeout = timeout
        self.failure = failure
    }

    public static func run<Value>(
        reading lines: sending some AsyncSequence<Data, any Error>,
        writing write: @escaping @Sendable (Data) async throws -> Void,
        decoder: JSONDecoder,
        timeout: Duration,
        retaining: Set<Method>,
        serve: @escaping @Sendable (Data) async -> Result<JSONValue, JSONRPCError>,
        failure: @escaping @Sendable (JSONRPCFailure<Method>) -> Failure,
        _ body: (JSONRPCConnection) async -> Value,
    ) async -> Value {
        let connection: JSONRPCConnection = JSONRPCConnection(write: write, decoder: decoder, timeout: timeout, failure: failure)
        async let _: Void = {
            do {
                for try await line: Data in lines {
                    await connection.receive(line, retaining: retaining, serve: serve)
                }
                await connection.finish(failure(.closed))
            } catch {
                await connection.finish(connection.transport(error))
            }
        }()
        return await body(connection)
    }

    public var isFinished: Bool { terminal != nil }

    // --- [REQUESTS]
    @discardableResult
    public func request<Value: Decodable & Sendable>(
        _ method: Method,
        params: (some Encodable & Sendable)? = Never?.none,
        returning _: Value.Type = JSONValue.self,
    ) async throws(Failure) -> Value {
        let id: JSONValue = .string(UUID().uuidString)
        let (answers, answer): (AsyncStream<Result<Data, Failure>>, AsyncStream<Result<Data, Failure>>.Continuation) = AsyncStream.makeStream()
        calls[id] = Call(method: method, answer: answer)
        defer { calls[id] = nil }
        try await send(Message(id: id, method: method.rawValue, params: params))
        let line: Data = try await withTimeout(timeout) { await answers.first(where: { _ in true }) ?? .failure(.deadline(.cancelled)) }.get()
        return try Result { try decoder.decode(Answer<Value>.self, from: line).result }.mapError { error in failure(.invalidMessage(method, error)) }.get()
    }

    public func notify(_ method: Method, params: (some Encodable & Sendable)? = Never?.none) async throws(Failure) {
        try await send(Message(method: method.rawValue, params: params))
    }

    // --- [NOTIFICATIONS]
    public func notification<Value: Decodable & Sendable>(
        _ method: Method,
        returning _: Value.Type,
        matching predicate: @escaping @Sendable (Value) -> Bool,
    ) async throws(Failure) -> Value {
        let decoder: JSONDecoder = decoder
        let id: UUID = UUID()
        let (values, value): (AsyncStream<Result<Value, Failure>>, AsyncStream<Result<Value, Failure>>.Continuation) = AsyncStream.makeStream()
        let listener: Listener = Listener(
            receive: { line in
                guard let found: Value = try decoder.decode(Message<Value>.self, from: line).params, predicate(found) else { return false }
                value.yield(.success(found))
                return true
            },
            close: { failure in value.yield(.failure(failure)) },
        )
        defer { listeners[method]?[id] = nil }
        switch Result(catching: { try retained[method]?.firstIndex(where: listener.receive) }) {
            case .success(.some(let index)): retained[method]?.remove(at: index)
            case .success(.none): listeners[method, default: [:]][id] = listener
            case .failure(let error): finish(failure(.invalidMessage(method, error)))
        }
        if let terminal { throw terminal }
        return try await (values.first(where: { _ in true }) ?? .failure(.deadline(.cancelled))).get()
    }

    public func subscribe<Value: Decodable & Sendable>(_ method: Method, to _: Value.Type) -> AsyncStream<Value> {
        let decoder: JSONDecoder = decoder
        let (values, value): (AsyncStream<Value>, AsyncStream<Value>.Continuation) = AsyncStream.makeStream()
        let listener: Listener = Listener(
            receive: { line in
                if let found: Value = try decoder.decode(Message<Value>.self, from: line).params { value.yield(found) }
                return false
            },
            close: { _ in value.finish() },
        )
        if terminal == nil { listeners[method, default: [:]][UUID()] = listener } else { value.finish() }
        return values
    }

    // --- [TRANSPORT]
    private func receive(_ line: Data, retaining: Set<Method>, serve: @Sendable (Data) async -> Result<JSONValue, JSONRPCError>) async {
        switch Result(catching: { try decoder.decode(Message<Skip>.self, from: line) }) {
            case .failure(let error): finish(failure(.invalidMessage(nil, error)))
            case .success(let message):
                switch (message.id, message.method) {
                    case (.some(let id), .some):
                        let reply: Message<JSONValue> =
                            switch await serve(line) {
                                case .success(let result): Message(id: id, result: result)
                                case .failure(let error): Message(id: id, error: error)
                            }
                        if case .failure(let error) = await Result(catching: { () async throws(Failure) in try await send(reply) }) { finish(error) }
                    case (.some(let id), .none):
                        guard let call: Call = calls.removeValue(forKey: id) else { return }
                        call.answer.yield(
                            message.error.map { error in .failure(failure(.error(call.method, error))) } ?? .success(line)
                        )
                    case (.none, .some(let name)):
                        guard let method: Method = Method(rawValue: name) else { return }
                        let waiting: Int? = listeners[method]?.count
                        do {
                            try listeners[method]?.removeAll { _, listener in try listener.receive(line) }
                            if listeners[method]?.count == waiting, retaining.contains(method) { retained[method, default: []].append(line) }
                        } catch {
                            finish(failure(.invalidMessage(method, error)))
                        }
                    case (.none, .none):
                        if let error: JSONRPCError = message.error { finish(failure(.error(nil, error))) }
                }
        }
    }

    private func send(_ message: some Encodable & Sendable) async throws(Failure) {
        if let terminal { throw terminal }
        try await Result { try await write(JSONEncoder().encodeLine(message)) }.mapError(transport).get()
    }

    private nonisolated func transport(_ error: any Error) -> Failure {
        error is CancellationError ? .deadline(.cancelled) : failure(.transport(error))
    }

    private func finish(_ failure: Failure) {
        guard terminal == nil else { return }
        terminal = failure
        for call: Call in calls.values {
            call.answer.yield(.failure(failure))
        }
        for listener: Listener in listeners.values.flatMap(\.values) {
            listener.close(failure)
        }
        calls.removeAll()
        listeners.removeAll()
        retained.removeAll()
    }
}
