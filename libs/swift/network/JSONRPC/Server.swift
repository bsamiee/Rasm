import AsyncAlgorithms
public import CasePaths
import Encoding
public import Foundation
import OTelSemanticConventions
import Results
import Synchronization
import Tracing

// --- [TYPES] ---------------------------------------------------------------------------

private enum ControlMessage: String {
    case interrupt = "@effect/rpc/Interrupt"
    case ack = "@effect/rpc/Ack"
}

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum Response: Sendable {
    case value((any Encodable & Sendable)?)
    case stream(any AsyncSequence<any Encodable & Sendable, Never> & Sendable)
}

private struct TraceContextCarrier: Extractor {
    func extract(key: String, from carrier: [String: String]) -> String? { carrier[key] }
}

// --- [SERVICES] ------------------------------------------------------------------------

final class Server<Request: Decodable & Sendable>: Sendable {
    // --- [STATE]

    private struct Call {
        let task: Task<Void, Never>
        let acks: AsyncStream<Void>.Continuation
        var streaming = false

        func interrupt() {
            acks.finish()
            task.cancel()
        }
    }

    let output = AsyncChannel<Data>()
    private let encoder = JSONEncoder.jsonLines
    private let decoder = JSONDecoder()
    private let perform: @Sendable (Request) async throws -> Response
    private let onDrop: @Sendable (JSONRPCFailure) -> Void
    private let state = Mutex<(running: [JSONValue: Call], closed: Bool)>(([:], false))

    init(perform: @escaping @Sendable (Request) async throws -> Response, onDrop: @escaping @Sendable (JSONRPCFailure) -> Void) {
        (self.perform, self.onDrop) = (perform, onDrop)
    }

    // --- [CALLS]

    func receive(_ line: Data) {
        let message: Message<JSONValue, JSONValue>
        do { message = try decoder.decode(Message<JSONValue, JSONValue>.self, from: line) } catch {
            onDrop(JSONRPCFailure(subject: "connection", cause: error))
            return
        }
        let target: JSONValue? = if case .object(let fields)? = message.params { fields["requestId"] } else { nil }
        switch (message.id, message.method, message.method.flatMap(ControlMessage.init(rawValue:)), target) {
            case (_, _, .interrupt, .some(let target)): state.withLock { state in state.running[target] }?.interrupt()
            case (_, _, .ack, .some(let target)): state.withLock { state in state.running[target] }?.acks.yield()
            case (.some(let id), .some(let method), .none, _):
                state.withLock { state in
                    guard !state.closed, state.running[id] == nil else { return }
                    let (acks, ack) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
                    state.running[id] = Call(task: Task { await self.answer(id, method, message, acknowledged: acks) }, acks: ack)
                }
            case (_, _, .some, .none), (.none, _, .none, _), (.some, .none, .none, _): break
        }
    }

    func close(failed: Bool) async {
        let calls = state.withLock { state in
            state.closed = true
            return Array(state.running.values)
        }
        for call in calls where call.streaming || failed || Task.isCancelled { call.interrupt() }
        for call in calls { await call.task.value }
    }

    // --- [ANSWERS]

    private func answer(_ id: JSONValue, _ method: String, _ message: Message<JSONValue, JSONValue>, acknowledged acks: AsyncStream<Void>) async {
        var context = InstrumentationContext.current ?? .topLevel
        if let trace = message.traceId, let span = message.spanId {
            InstrumentationSystem.instrument.extract(["traceparent": "00-\(trace)-\(span)-\(message.sampled == false ? "00" : "01")"], into: &context, using: TraceContextCarrier())
        }
        let outcome = await Result { () async throws -> Response in
            try await withSpan(method, context: context, ofKind: .server) { span in
                span.attributes.rpc.system.name = .init(rawValue: "jsonrpc")
                span.attributes.rpc.method = method
                span.attributes.jsonrpc.protocol.version = message.jsonrpc?.rawValue
                span.attributes.jsonrpc.request.id =
                    switch id {
                        case .string(let text): text
                        case .number(let number): Int64(exactly: number)?.description ?? number.description
                        case .null, .bool, .array, .object: nil
                    }
                let request = try Result { try JSONValue.object([method: message.params ?? .object([:])]).decode(Request.self) }.aggregated(Reason.die).get()
                return try await perform(request)
            }
        }
        .mapError { error in error as? AggregateError<Reason> ?? AggregateError(failing: error) }
        switch outcome {
            case .success(.value(let value)): await send(id, answering: method) { try value.map { value in try encodeResult(id, value) } ?? encodeResult(id, JSONValue.null) }
            case .success(.stream(let chunks)):
                let streaming = state.withLock { state in
                    state.running[id]?.streaming = true
                    return !state.closed
                }
                guard streaming else { break }
                var acknowledgements = acks.makeAsyncIterator()
                let interrupted = await chunks.contains { chunk in
                    await send(id, answering: method) { try encodeResult(id, chunk, chunk: true) }
                    return await acknowledgements.next() == nil
                }
                if !interrupted, !Task.isCancelled { await send(id, answering: method) { try encodeResult(id, JSONValue.null) } }
            case .failure(let cause): await send(id, answering: method) { try encodeExit(id, cause) }
        }
        state.withLock { state in state.running[id] = nil }
    }

    private func send(_ id: JSONValue, answering method: String, _ line: () throws -> Data) async {
        do { await output.send(try line()) } catch {
            do { await output.send(try encodeExit(id, AggregateError(first: .die(error)))) } catch { onDrop(JSONRPCFailure(subject: method, cause: error)) }
        }
    }

    private func encodeResult(_ id: JSONValue, _ value: some Encodable & Sendable, chunk: Bool = false) throws -> Data {
        try chunk ? encoder.encodeLine(Message<_, Never>(id: id, chunk: true, result: [value])) : encoder.encodeLine(Message<_, Never>(id: id, result: value))
    }

    private func encodeExit(_ id: JSONValue, _ reasons: AggregateError<Reason>) throws -> Data {
        try encoder.encodeLine(Message<Never, Cause>(id: id, error: Cause(reasons: reasons)))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func serve<Request: Decodable & Sendable>(
    reading lines: @autoclosure @escaping @Sendable () -> any AsyncSequence<Data, any Error>,
    writing write: @escaping @Sendable (Data) async throws -> Void,
    onDrop: @escaping @Sendable (JSONRPCFailure) -> Void,
    _ perform: @escaping @Sendable (Request) async throws -> Response,
) async throws(JSONRPCFailure) {
    let server = Server(perform: perform, onDrop: onDrop)
    let ended = await withTaskGroup(of: Result<Void, any Error>.self) { group in
        group.addTask {
            defer { server.output.finish() }
            return await Result { () async throws in for await line in server.output { try await write(line) } }
        }
        group.addTask {
            let read = await Result { () async throws in for try await line in lines() { server.receive(line) } }
            let failed: Bool = if case .failure = read { true } else { false }
            await server.close(failed: failed)
            server.output.finish()
            return read
        }
        defer { group.cancelAll() }
        return await group.first { ending in if case .failure = ending { true } else { false } }
    }
    if case .failure(let error)? = ended { throw JSONRPCFailure(subject: "connection", cause: error) }
}
