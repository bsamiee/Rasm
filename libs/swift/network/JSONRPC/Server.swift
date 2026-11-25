import AsyncAlgorithms
import Encoding
public import Foundation
import Results
import Tracing

// --- [TYPES] ---------------------------------------------------------------------------

enum ControlMessage: String {
    case interrupt = "@effect/rpc/Interrupt"
    case ack = "@effect/rpc/Ack"
}

// --- [MODELS] --------------------------------------------------------------------------

public enum Response: Sendable {
    case value((any Encodable & Sendable)?)
    case stream(any AsyncSequence<any Encodable & Sendable, Never> & Sendable)
}

private struct TraceContextCarrier: Extractor {
    func extract(key: String, from carrier: [String: String]) -> String? { carrier[key] }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Server<Request: Decodable & Sendable, Failure: Error> {
    // --- [STATE]

    let output = AsyncChannel<Data>()
    private let encoder = JSONEncoder()
    private let decoder = JSONDecoder()
    private let perform: @Sendable (Request) async throws(Failure) -> Response
    private let onDrop: @Sendable (JSONRPCFailure) -> Void
    private var running: [JSONValue: (task: Task<Void, Never>, acks: AsyncStream<Void>.Continuation, streaming: Bool)] = [:]
    private var closed: Bool = false

    init(perform: @escaping @Sendable (Request) async throws(Failure) -> Response, onDrop: @escaping @Sendable (JSONRPCFailure) -> Void) {
        (self.perform, self.onDrop) = (perform, onDrop)
    }

    // --- [CALLS]

    func receive(_ line: Data) {
        let message: Message<JSONValue, JSONValue>
        do { message = try decoder.decode(Message<JSONValue, JSONValue>.self, from: line) } catch {
            onDrop(JSONRPCFailure(subject: "connection", reason: .unreadable(error)))
            return
        }
        let target: JSONValue? = if case .object(let fields)? = message.params { fields["requestId"] } else { nil }
        switch (message.id, message.method, message.method.flatMap(ControlMessage.init(rawValue:)), target) {
            case (_, _, .interrupt, .some(let target)): interrupt(target)
            case (_, _, .ack, .some(let target)): running[target]?.acks.yield()
            case (.some(let id), .some(let method), .none, _) where !closed && running[id] == nil:
                let (acks, ack) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
                running[id] = (Task { await self.answer(id, method, message, acknowledged: acks) }, ack, false)
            case (_, _, .some, .none), (.none, _, .none, _), (.some, .none, .none, _), (.some, .some, .none, _): break
        }
    }

    private func interrupt(_ id: JSONValue) {
        running[id]?.acks.finish()
        running[id]?.task.cancel()
    }

    private func markStreaming(_ id: JSONValue) -> Bool {
        running[id]?.streaming = true
        return !closed
    }

    private func remove(_ id: JSONValue) { running[id] = nil }

    func close(failed: Bool) async {
        closed = true
        let pending = running.map { id, call in
            if call.streaming || failed || Task.isCancelled { interrupt(id) }
            return call.task
        }
        for task in pending { await task.value }
    }

    // --- [ANSWERS]

    @concurrent
    private nonisolated func answer(_ id: JSONValue, _ method: String, _ message: Message<JSONValue, JSONValue>, acknowledged acks: AsyncStream<Void>) async {
        var context = InstrumentationContext.current ?? .topLevel
        if let trace = message.traceId, let span = message.spanId {
            InstrumentationSystem.instrument.extract(["traceparent": "00-\(trace)-\(span)-\(message.sampled == false ? "00" : "01")"], into: &context, using: TraceContextCarrier())
        }
        let outcome = await Result { [context] () async throws -> Response in
            let request = try Result { try JSONValue.object([method: message.params ?? .object([:])]).decode(Request.self) }.aggregated(Reason.die).get()
            return try await withSpan(method, context: context, ofKind: .server) { span in
                span.attributes["rpc.system"] = "jsonrpc"
                span.attributes["rpc.method"] = method
                return try await self.perform(request)
            }
        }
        .mapError { error in error as? AggregateError<Reason> ?? AggregateError(failing: error) }
        switch outcome {
            case .success(.value(let value)): await send(id, answering: method) { try value.map { value in try encodeResult(id, value) } ?? encodeResult(id, JSONValue.null) }
            case .success(.stream(let chunks)) where await markStreaming(id):
                var acknowledgements = acks.makeAsyncIterator()
                let interrupted = await chunks.contains { chunk in
                    await send(id, answering: method) { try encodeResult(id, chunk, chunk: true) }
                    return await acknowledgements.next() == nil
                }
                if !interrupted, !Task.isCancelled { await send(id, answering: method) { try encodeResult(id, JSONValue.null) } }
            case .success(.stream): break
            case .failure(let cause): await send(id, answering: method) { try encodeExit(id, cause) }
        }
        await remove(id)
    }

    private nonisolated func send(_ id: JSONValue, answering method: String, _ line: () throws -> Data) async {
        do { await output.send(try line()) } catch {
            do { await output.send(try encodeExit(id, AggregateError(first: .die(error)))) } catch { onDrop(JSONRPCFailure(subject: method, reason: .unencodable(error))) }
        }
    }

    private nonisolated func encodeResult(_ id: JSONValue, _ value: some Encodable & Sendable, chunk: Bool = false) throws -> Data {
        try chunk ? encoder.encodeLine(Message<_, Never>(id: id, chunk: true, result: [value])) : encoder.encodeLine(Message<_, Never>(id: id, result: value))
    }

    private nonisolated func encodeExit(_ id: JSONValue, _ reasons: AggregateError<Reason>) throws -> Data {
        try encoder.encodeLine(Message<Never, Cause>(id: id, error: Cause(reasons: reasons)))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func serve<Request: Decodable & Sendable, Failure: Error>(
    reading lines: @autoclosure @escaping @Sendable () -> any AsyncSequence<Data, any Error>,
    writing write: @escaping @Sendable (Data) async throws -> Void,
    onDrop: @escaping @Sendable (JSONRPCFailure) -> Void,
    _ perform: @escaping @Sendable (Request) async throws(Failure) -> Response,
) async throws(JSONRPCFailure) {
    let server = Server(perform: perform, onDrop: onDrop)
    let ended = await withTaskGroup(of: Result<Void, any Error>.self) { group in
        group.addTask {
            defer { server.output.finish() }
            return await Result { () async throws in try await server.output.reduce(()) { _, line in try await write(line) } }
        }
        group.addTask {
            let read = await Result { () async throws in try await lines().reduce(()) { _, line in await server.receive(line) } }
            let failed: Bool = if case .failure = read { true } else { false }
            await server.close(failed: failed)
            server.output.finish()
            return read
        }
        defer { group.cancelAll() }
        return await group.first { ending in if case .failure = ending { true } else { false } }
    }
    if case .failure(let error)? = ended { throw JSONRPCFailure(subject: "connection", transport: error) }
}
