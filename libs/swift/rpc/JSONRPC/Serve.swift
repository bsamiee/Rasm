import AsyncAlgorithms
import Encoding
public import Foundation
import Results
import Tracing

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

private struct Headers: Extractor {
    func extract(key: String, from carrier: [String: String]) -> String? { carrier[key] }
}

enum Reason: Encodable, Sendable {
    case fail(any Error)
    case die(any Error)

    private enum Key: String, CodingKey {
        case tag = "_tag"
        case error, defect
    }

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        switch self {
            case .fail(let error):
                try fields.encode("Fail", forKey: .tag)
                try fields.encode(EncodedError(error: error), forKey: .error)
            case .die(let error):
                try fields.encode("Die", forKey: .tag)
                try fields.encode(EncodedError(error: error), forKey: .defect)
        }
    }
}

struct Cause: Encodable, Sendable {
    let code: Int = 0
    let message: String
    let data: [Reason]

    init(_ reasons: AggregateError<Reason>) {
        let error: any Error =
            switch reasons.first {
                case .fail(let error), .die(let error): error
            }
        (message, data) = (error.localizedDescription, reasons.errors)
    }
}

extension AggregateError where Element == Reason {
    init(failing error: any Error) { self = Self(((error as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [any Error] ?? []).map(Reason.fail)) ?? Self(first: .fail(error)) }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Server<Request: Decodable & Sendable, Failure: Error> {
    // --- [STATE]

    let output: AsyncChannel<Data> = .init()
    private let encoder: JSONEncoder = .init()
    private let decoder: JSONDecoder = .init()
    private let perform: @Sendable (Request) async throws(Failure) -> Response
    private let dropped: @Sendable (JSONRPCFailure) -> Void
    private var running: [JSONValue: (task: Task<Void, Never>, acks: AsyncStream<Void>.Continuation, streaming: Bool)] = [:]
    private var closed: Bool = false

    init(perform: @escaping @Sendable (Request) async throws(Failure) -> Response, dropped: @escaping @Sendable (JSONRPCFailure) -> Void) {
        (self.perform, self.dropped) = (perform, dropped)
    }

    // --- [CALLS]

    func receive(_ line: Data) {
        let envelope: Envelope<JSONValue, JSONValue>
        do { envelope = try decoder.decode(Envelope<JSONValue, JSONValue>.self, from: line) } catch {
            dropped(JSONRPCFailure(subject: "connection", reason: .unreadable(error)))
            return
        }
        let target: JSONValue? = if case .object(let fields)? = envelope.params { fields["requestId"] } else { nil }
        switch (envelope.id, envelope.method, envelope.method.flatMap(Control.init(rawValue:)), target) {
            case (_, _, .interrupt, .some(let target)): interrupt(target)
            case (_, _, .ack, .some(let target)): running[target]?.acks.yield()
            case (.some(let id), .some(let method), .none, _) where !closed && running[id] == nil:
                let (acks, ack) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
                running[id] = (Task { await self.answer(id, method, envelope, acknowledged: acks) }, ack, false)
            case (_, _, .some, .none), (.none, _, .none, _), (.some, .none, .none, _), (.some, .some, .none, _): break
        }
    }

    private func interrupt(_ id: JSONValue) {
        running[id]?.acks.finish()
        running[id]?.task.cancel()
    }

    private func streaming(_ id: JSONValue) -> Bool {
        running[id]?.streaming = true
        return !closed
    }

    private func finished(_ id: JSONValue) { running[id] = nil }

    func close(interrupting selected: (_ streaming: Bool) -> Bool) async {
        closed = true
        let pending = running.map { id, call in
            if selected(call.streaming) { interrupt(id) }
            return call.task
        }
        for task in pending { await task.value }
    }

    // --- [ANSWERS]

    @concurrent
    private nonisolated func answer(_ id: JSONValue, _ method: String, _ envelope: Envelope<JSONValue, JSONValue>, acknowledged acks: AsyncStream<Void>) async {
        var context = InstrumentationContext.current ?? .topLevel
        if let trace = envelope.traceId, let span = envelope.spanId {
            InstrumentationSystem.instrument.extract(["traceparent": "00-\(trace)-\(span)-\(envelope.sampled == false ? "00" : "01")"], into: &context, using: Headers())
        }
        let outcome = await Result { [context] () async throws -> Response in
            let request = try Result { try JSONValue.object([method: envelope.params ?? .object([:])]).decode(Request.self) }.mapError { error in AggregateError(first: Reason.die(error)) }.get()
            return try await withSpan(method, context: context, ofKind: .server) { span in
                span.attributes["rpc.system"] = "jsonrpc"
                span.attributes["rpc.method"] = method
                return try await self.perform(request)
            }
        }
        .mapError { error in error as? AggregateError<Reason> ?? AggregateError(failing: error) }
        switch outcome {
            case .success(.value(let value?)): await send(id, answering: method) { try result(id, value) }
            case .success(.value(nil)): await send(id, answering: method) { try result(id, JSONValue.null) }
            case .success(.stream(let chunks)) where await streaming(id):
                var acknowledgements = acks.makeAsyncIterator()
                for await chunk in chunks {
                    await send(id, answering: method) { try result(id, chunk, chunk: true) }
                    _ = await acknowledgements.next()
                }
                if !Task.isCancelled { await send(id, answering: method) { try result(id, JSONValue.null) } }
            case .success(.stream): break
            case .failure(let cause): await send(id, answering: method) { try exit(id, cause) }
        }
        await finished(id)
    }

    private nonisolated func send(_ id: JSONValue, answering method: String, _ line: () throws -> Data) async {
        do { await output.send(try line()) } catch {
            do { await output.send(try exit(id, AggregateError(first: .die(error)))) } catch { dropped(JSONRPCFailure(subject: method, reason: .unencodable(error))) }
        }
    }

    private nonisolated func result(_ id: JSONValue, _ value: some Encodable & Sendable, chunk: Bool = false) throws -> Data {
        try chunk ? encoder.encodeLine(Envelope<_, Never>(id: id, chunk: true, result: [value])) : encoder.encodeLine(Envelope<_, Never>(id: id, result: value))
    }

    private nonisolated func exit(_ id: JSONValue, _ reasons: AggregateError<Reason>) throws -> Data {
        try encoder.encodeLine(Envelope<Never, Tagged<Cause>>(id: id, error: Tagged(tag: "Cause", value: Cause(reasons))))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func serve<Request: Decodable & Sendable, Failure: Error>(
    reading lines: @autoclosure @escaping @Sendable () -> any AsyncSequence<Data, any Error>,
    writing write: @escaping @Sendable (Data) async throws -> Void,
    dropped: @escaping @Sendable (JSONRPCFailure) -> Void,
    _ perform: @escaping @Sendable (Request) async throws(Failure) -> Response,
) async throws(JSONRPCFailure) {
    let server = Server(perform: perform, dropped: dropped)
    async let written: Void = server.output.reduce(()) { _, line in try await write(line) }
    let read = await Result { () async throws in
        for try await line in lines() { await server.receive(line) }
    }
    let failed: Bool = if case .failure = read { true } else { false }
    await server.close { streaming in streaming || failed || Task.isCancelled }
    server.output.finish()
    let wrote: Result<Void, any Error>
    do { wrote = .success(try await written) } catch { wrote = .failure(error) }
    for case .failure(let error) in [read, wrote] { throw JSONRPCFailure(subject: "connection", transport: error) }
}
