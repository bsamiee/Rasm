import AsyncAlgorithms
import Encoding
public import Foundation
import JSONRPC
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

// --- [ERRORS] --------------------------------------------------------------------------

public struct EffectRPCFailure: CustomNSError, LocalizedError {
    public enum Reason: Sendable {
        case transport(any Error)
        case unreadable(any Error)
        case unencodable(any Error)
        case cancelled(CancellationError)
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .transport(let error): error.localizedDescription
            case .unreadable: "Unreadable \(subject) message"
            case .unencodable: "Unencodable \(subject) message"
            case .cancelled: "\(subject) canceled"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .transport(let error), .unreadable(let error), .unencodable(let error): [NSUnderlyingErrorKey: error]
            case .cancelled: [:]
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Calls {
    private var running: [JSONValue: (task: Task<Void, Never>, acks: AsyncStream<Void>.Continuation, streaming: Bool)] = [:]
    private var closed: Bool = false

    func start(_ id: JSONValue, _ call: @escaping @concurrent @Sendable (AsyncStream<Void>) async -> Void) {
        guard !closed, running[id] == nil else { return }
        let (acks, ack) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        running[id] = (Task { await call(acks) }, ack, false)
    }

    func interrupt(_ id: JSONValue) {
        running[id]?.acks.finish()
        running[id]?.task.cancel()
    }

    func acknowledge(_ id: JSONValue) { running[id]?.acks.yield() }

    func streaming(_ id: JSONValue) -> Bool {
        running[id]?.streaming = true
        return !closed
    }

    func finished(_ id: JSONValue) { running[id] = nil }

    func close(interrupting selected: (_ streaming: Bool) -> Bool) async {
        closed = true
        let pending = running.map { id, call in
            if selected(call.streaming) { interrupt(id) }
            return call.task
        }
        for task in pending { await task.value }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func serve<Request: Decodable & Sendable, Failure: Error>(
    reading lines: @autoclosure @escaping @Sendable () -> any AsyncSequence<Data, any Error>,
    writing write: @escaping @Sendable (Data) async throws -> Void,
    dropped: @escaping @Sendable (EffectRPCFailure) -> Void,
    _ perform: @escaping @Sendable (Request) async throws(Failure) -> Response,
) async throws(EffectRPCFailure) {
    let output = AsyncChannel<Data>()
    let calls = Calls()
    let decoder = JSONDecoder()
    let encoder = JSONEncoder()
    @Sendable func exit(_ id: JSONValue, _ reasons: AggregateError<Reason>) throws -> Data {
        try encoder.encodeLine(Envelope<Never, Tagged<Cause>>(id: id, error: Tagged(tag: "Cause", value: Cause(reasons))))
    }
    @Sendable func result(_ id: JSONValue, _ value: some Encodable & Sendable, chunk: Bool = false) throws -> Data {
        try chunk
            ? encoder.encodeLine(Envelope<_, Never>(id: id, chunk: true, result: [value]))
            : encoder.encodeLine(Envelope<_, Never>(id: id, result: value))
    }
    @Sendable func send(_ id: JSONValue, answering method: String, _ line: () throws -> Data) async {
        do { await output.send(try line()) } catch {
            do { await output.send(try exit(id, AggregateError(first: .die(error)))) } catch { dropped(EffectRPCFailure(subject: method, reason: .unencodable(error))) }
        }
    }
    @concurrent @Sendable func answer(_ id: JSONValue, _ method: String, _ envelope: Envelope<JSONValue, JSONValue>, acknowledged acks: AsyncStream<Void>) async {
        var context = InstrumentationContext.current ?? .topLevel
        if let trace = envelope.traceId, let span = envelope.spanId {
            InstrumentationSystem.instrument.extract(["traceparent": "00-\(trace)-\(span)-\(envelope.sampled == false ? "00" : "01")"], into: &context, using: Headers())
        }
        let outcome = await Result { [context] () async throws(AggregateError<Reason>) -> Response in
            let request: Request
            do { request = try JSONValue.object([method: envelope.params ?? .object([:])]).decode(Request.self) } catch { throw AggregateError(first: .die(error)) }
            do {
                return try await withSpan(method, context: context, ofKind: .server) { span in
                    span.attributes["rpc.system"] = "jsonrpc"
                    span.attributes["rpc.method"] = method
                    return try await perform(request)
                }
            } catch {
                throw AggregateError(failing: error)
            }
        }
        switch outcome {
            case .success(.value(let value?)): await send(id, answering: method) { try result(id, value) }
            case .success(.value(nil)): await send(id, answering: method) { try result(id, JSONValue.null) }
            case .success(.stream(let chunks)):
                guard await calls.streaming(id) else { break }
                var acknowledgements = acks.makeAsyncIterator()
                for await chunk in chunks {
                    await send(id, answering: method) { try result(id, chunk, chunk: true) }
                    _ = await acknowledgements.next()
                }
                if !Task.isCancelled { await send(id, answering: method) { try result(id, JSONValue.null) } }
            case .failure(let cause): await send(id, answering: method) { try exit(id, cause) }
        }
        await calls.finished(id)
    }
    @concurrent @Sendable func receive(_ line: Data) async throws(EffectRPCFailure) {
        let envelope: Envelope<JSONValue, JSONValue>
        do { envelope = try decoder.decode(Envelope<JSONValue, JSONValue>.self, from: line) } catch { throw EffectRPCFailure(subject: "connection", reason: .unreadable(error)) }
        let target: JSONValue? = if case .object(let fields)? = envelope.params { fields["requestId"] } else { nil }
        switch (envelope.id, envelope.method, envelope.method.flatMap(Control.init(rawValue:)), target) {
            case (_, _, .interrupt, .some(let target)): await calls.interrupt(target)
            case (_, _, .ack, .some(let target)): await calls.acknowledge(target)
            case (.some(let id), .some(let method), .none, _): await calls.start(id) { acks in await answer(id, method, envelope, acknowledged: acks) }
            case (_, _, .some, .none), (.none, _, .none, _), (.some, .none, .none, _): break
        }
    }
    do {
        try await withThrowingTaskGroup(of: Void.self) { group in
            group.addTask {
                for await line in output { try await write(line) }
            }
            group.addTask {
                for try await line in lines() {
                    do throws(EffectRPCFailure) { try await receive(line) } catch { dropped(error) }
                }
                await calls.close { streaming in streaming || Task.isCancelled }
                output.finish()
            }
            do { while try await group.next() != nil {} } catch {
                group.cancelAll()
                output.finish()
                await calls.close { _ in true }
                throw error
            }
        }
    } catch {
        throw EffectRPCFailure(subject: "connection", reason: (error as? CancellationError).map(EffectRPCFailure.Reason.cancelled) ?? .transport(error))
    }
}
