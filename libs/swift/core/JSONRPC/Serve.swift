import Encoding
public import Foundation
import Results

// --- [TYPES] ---------------------------------------------------------------------------

enum Control: String {
    case interrupt = "@effect/rpc/Interrupt"
    case ack = "@effect/rpc/Ack"
}

// --- [MODELS] --------------------------------------------------------------------------

public enum Response: Sendable {
    case value((any Encodable & Sendable)?)
    case stream(any AsyncSequence<any Encodable & Sendable, Never> & Sendable)
}

enum Reason: Encodable, Sendable {
    case fail(any Error)
    case die(any Error)

    func encode(to encoder: any Encoder) throws {
        switch self {
            case .fail(let error): try Tagged(tag: "Fail", value: ["error": EncodedError(error: error)]).encode(to: encoder)
            case .die(let error): try Tagged(tag: "Die", value: ["defect": EncodedError(error: error)]).encode(to: encoder)
        }
    }
}

extension AggregateError where Element == Reason {
    init(failing error: any Error) { self = Self(((error as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [any Error] ?? []).map(Reason.fail)) ?? Self(first: .fail(error)) }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Calls {
    private var running: [JSONValue: (task: Task<Void, Never>, acks: AsyncStream<Void>.Continuation)] = [:]

    func start(_ id: JSONValue, _ call: @escaping @concurrent @Sendable (AsyncStream<Void>) async -> Void) {
        let (acks, ack): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream()
        running[id] = (Task { await call(acks) }, ack)
    }

    func interrupt(_ id: JSONValue) { running.removeValue(forKey: id)?.task.cancel() }

    func acknowledge(_ id: JSONValue) { running[id]?.acks.yield() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func serve<Request: Decodable & Sendable, Failure: Error>(
    reading lines: some AsyncSequence<Data, any Error>,
    writing write: @escaping @Sendable (Data) async throws -> Void,
    dropped: @escaping @Sendable (JSONRPCFailure) -> Void,
    _ perform: @escaping @Sendable (Request) async throws(Failure) -> Response,
) async throws(JSONRPCFailure) {
    let (output, sink): (AsyncStream<Result<Data, JSONRPCFailure>>, AsyncStream<Result<Data, JSONRPCFailure>>.Continuation) = AsyncStream.makeStream()
    let calls: Calls = Calls()
    let decoder: JSONDecoder = JSONDecoder()
    let encoder: JSONEncoder = JSONEncoder()
    @Sendable func exit(_ id: JSONValue, _ reasons: [Reason]) throws -> Data {
        try encoder.encodeLine(Envelope<Never, Tagged<[String: [Reason]]>>(jsonrpc: nil, id: id, error: Tagged(tag: "Cause", value: ["data": reasons])))
    }
    @Sendable func result(_ id: JSONValue, _ value: some Encodable & Sendable, chunk: Bool = false) throws -> Data {
        try chunk
            ? encoder.encodeLine(Envelope<_, Never>(jsonrpc: nil, id: id, chunk: true, result: [value]))
            : encoder.encodeLine(Envelope<_, Never>(jsonrpc: nil, id: id, result: value))
    }
    @Sendable func send(_ id: JSONValue, answering method: String, _ line: () throws -> Data) {
        sink.yield(Result(catching: line).flatMapError { error in Result { try exit(id, [.die(error)]) } }.mapError { error in JSONRPCFailure(subject: method, reason: .unencodable(error)) })
    }
    @concurrent @Sendable func answer(_ id: JSONValue, _ method: String, _ params: JSONValue?, acknowledged acks: AsyncStream<Void>) async {
        let outcome: Result<Response, AggregateError<Reason>> = await Result { () async throws(AggregateError<Reason>) -> Response in
            let request: Request = try Result { try JSONValue.object([method: params ?? .object([:])]).decode(Request.self) }.aggregated(Reason.die).get()
            return try await Result { () async throws(Failure) in try await perform(request) }
                .mapError(AggregateError.init(failing:))
                .get()
        }
        switch outcome {
            case .success(.value(let value?)): send(id, answering: method) { try result(id, value) }
            case .success(.value(nil)): send(id, answering: method) { try result(id, JSONValue.null) }
            case .success(.stream(let chunks)):
                var acknowledgements: AsyncStream<Void>.Iterator = acks.makeAsyncIterator()
                for await chunk: any Encodable & Sendable in chunks {
                    send(id, answering: method) { try result(id, chunk, chunk: true) }
                    await acknowledgements.next()
                }
                send(id, answering: method) { try result(id, JSONValue.null) }
            case .failure(let cause): send(id, answering: method) { try exit(id, cause.errors) }
        }
        await calls.interrupt(id)
    }
    func receive(_ line: Data) async {
        switch Result(catching: { try decoder.decode(Envelope<JSONValue, JSONValue>.self, from: line) }) {
            case .failure(let error): sink.yield(.failure(JSONRPCFailure(subject: "connection", reason: .unreadable(error))))
            case .success(let envelope):
                let target: JSONValue? = if case .object(let fields)? = envelope.params { fields["requestId"] } else { nil }
                switch (envelope.id, envelope.method, envelope.method.flatMap(Control.init(rawValue:)), target) {
                    case (_, _, .interrupt, .some(let target)): await calls.interrupt(target)
                    case (_, _, .ack, .some(let target)): await calls.acknowledge(target)
                    case (.some(let id), .some(let method), .none, _): await calls.start(id) { acks in await answer(id, method, envelope.params, acknowledged: acks) }
                    case (_, _, .some, .none), (.none, _, .none, _), (.some, .none, .none, _): break
                }
        }
    }
    do {
        try await withThrowingDiscardingTaskGroup { group in
            group.addTask {
                for await line: Result<Data, JSONRPCFailure> in output {
                    switch line {
                        case .success(let data): try await write(data)
                        case .failure(let failure): dropped(failure)
                    }
                }
            }
            for try await line: Data in lines { await receive(line) }
            sink.finish()
        }
    } catch {
        throw JSONRPCFailure(subject: "connection", transport: error)
    }
}
