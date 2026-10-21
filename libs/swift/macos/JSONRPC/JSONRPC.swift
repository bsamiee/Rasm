public import Foundation
public import JSON
public import Processes
public import Results
public import Subprocess

// --- [MODELS] --------------------------------------------------------------------------

private struct Message<Payload: Sendable>: Sendable {
    var jsonrpc: String? = "2.0"
    var id: JSONValue?
    var method: String?
    var params: Payload?
    var result: Payload?
    var error: JSONRPCRejection?
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

public struct JSONRPCRejection: Error, Codable {
    public let code: Int
    public let message: String

    public init(code: Int, message: String) { (self.code, self.message) = (code, message) }
}

public enum JSONRPCFailure<Method: RawRepresentable & Sendable>: CustomNSError, LocalizedError where Method.RawValue == String {
    case process(ProcessFailure)
    case closed
    case invalidMessage(Method?, any Error)
    case rejected(Method, JSONRPCRejection)

    public var errorDescription: String? {
        switch self {
            case .process(let failure): failure.localizedDescription
            case .closed: "Connection closed"
            case .invalidMessage(.some(let method), _): "Unreadable \(method.rawValue) message"
            case .invalidMessage(.none, _): "Unreadable message"
            case .rejected(let method, let rejection): "\(method.rawValue) rejected with \(rejection.code): \(rejection.message)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .process(let error as any Error), .invalidMessage(_, let error): [NSUnderlyingErrorKey: error]
            case .closed, .rejected: [:]
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

public actor JSONRPCConnection<Method: RawRepresentable & Hashable & Sendable, Failure: DeadlineFailure> where Method.RawValue == String {
    // --- [STATE]
    private struct Reply: Sendable {
        let method: Method
        let answer: AsyncStream<Result<Data, Failure>>.Continuation
    }

    private struct Listener: Sendable {
        let id: UUID = UUID()
        let receive: @Sendable (Data) throws -> Bool
        let close: @Sendable (Failure) -> Void
    }

    private let writer: StandardInputWriter
    private let decoder: JSONDecoder
    private let timeout: Duration
    private let failure: @Sendable (JSONRPCFailure<Method>) -> Failure
    private var replies: [JSONValue: Reply] = [:]
    private var listeners: [Method: [Listener]] = [:]
    private var retained: [Method: [Data]] = [:]
    private var terminal: Failure?

    private init(
        writer: StandardInputWriter,
        decoder: JSONDecoder,
        timeout: Duration,
        failure: @escaping @Sendable (JSONRPCFailure<Method>) -> Failure,
    ) {
        self.writer = writer
        self.decoder = decoder
        self.timeout = timeout
        self.failure = failure
    }

    public static func run<Value>(
        over execution: Execution<CustomWriteInput, SequenceOutput, some OutputProtocol>,
        decoder: JSONDecoder,
        timeout: Duration,
        retaining: Set<Method>,
        serve: @escaping @Sendable (Data) async -> Result<JSONValue, JSONRPCRejection>,
        failure: @escaping @Sendable (JSONRPCFailure<Method>) -> Failure,
        _ body: (JSONRPCConnection) async -> Value,
    ) async -> Value {
        let connection: JSONRPCConnection = JSONRPCConnection(
            writer: execution.standardInputWriter,
            decoder: decoder,
            timeout: timeout,
            failure: failure,
        )
        async let _: Void = {
            do {
                for try await line: Data in execution.standardOutput.jsonLines {
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
        replies[id] = Reply(method: method, answer: answer)
        defer { replies[id] = nil }
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
        let (values, value): (AsyncStream<Result<Value, Failure>>, AsyncStream<Result<Value, Failure>>.Continuation) = AsyncStream.makeStream()
        let listener: Listener = Listener(
            receive: { line in
                try decoder.decode(Message<Value>.self, from: line).params.flatMap { found in predicate(found) ? found : nil }.map { found in value.yield(.success(found)) } != nil
            },
            close: { failure in value.yield(.failure(failure)) },
        )
        defer { listeners[method]?.removeAll { other in other.id == listener.id } }
        switch Result(catching: { try retained[method]?.firstIndex(where: listener.receive) }) {
            case .success(.some(let index)): retained[method]?.remove(at: index)
            case .success(.none): listeners[method, default: []].append(listener)
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
        if terminal == nil { listeners[method, default: []].append(listener) } else { value.finish() }
        return values
    }

    // --- [TRANSPORT]
    private func receive(_ line: Data, retaining: Set<Method>, serve: @Sendable (Data) async -> Result<JSONValue, JSONRPCRejection>) async {
        switch Result(catching: { try decoder.decode(Message<Skip>.self, from: line) }) {
            case .failure(let error): finish(failure(.invalidMessage(nil, error)))
            case .success(let message):
                switch (message.id, message.method) {
                    case (.some(let id), .some):
                        let reply: Message<JSONValue> =
                            switch await serve(line) {
                                case .success(let result): Message(id: id, result: result)
                                case .failure(let rejection): Message(id: id, error: rejection)
                            }
                        if case .failure(let error) = await Result(catching: { () async throws(Failure) in try await send(reply) }) { finish(error) }
                    case (.some(let id), .none):
                        guard let reply: Reply = replies.removeValue(forKey: id) else { return }
                        reply.answer.yield(
                            message.error.map { rejection in .failure(failure(.rejected(reply.method, rejection))) } ?? .success(line)
                        )
                    case (.none, .some(let name)):
                        guard let method: Method = Method(rawValue: name) else { return }
                        do {
                            let done: [UUID] = try listeners[method, default: []].filter { listener in try listener.receive(line) }.map(\.id)
                            listeners[method]?.removeAll { listener in done.contains(listener.id) }
                            if done.isEmpty, retaining.contains(method) { retained[method, default: []].append(line) }
                        } catch {
                            finish(failure(.invalidMessage(method, error)))
                        }
                    case (.none, .none): return
                }
        }
    }

    private func send(_ message: some Encodable & Sendable) async throws(Failure) {
        if let terminal { throw terminal }
        try await Result { try await writer.writeLine(message) }.mapError(transport).get()
    }

    private nonisolated func transport(_ error: any Error) -> Failure {
        switch ProcessFailure(error) {
            case .deadline(let deadline): .deadline(deadline)
            case let process: failure(.process(process))
        }
    }

    private func finish(_ failure: Failure) {
        guard terminal == nil else { return }
        terminal = failure
        for reply: Reply in replies.values {
            reply.answer.yield(.failure(failure))
        }
        for listener: Listener in listeners.values.joined() {
            listener.close(failure)
        }
        replies.removeAll()
        listeners.removeAll()
        retained.removeAll()
    }
}
