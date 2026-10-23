import CurrentProcess
import Encoding
public import Foundation
import Results
import os

// --- [MODELS] --------------------------------------------------------------------------

public enum Response: Sendable {
    case value((any Encodable & Sendable)?)
    case stream(any AsyncSequence<any Encodable & Sendable, Never> & Sendable)
}

// --- [SERVICES] ------------------------------------------------------------------------

let effectRPC: DispatchQueue = DispatchQueue(label: "EffectRPC", attributes: .concurrent)

actor Calls {
    private var running: [JSONValue: (task: Task<Void, Never>, acks: AsyncStream<Void>.Continuation)] = [:]

    func start(_ id: JSONValue, _ call: @escaping @concurrent @Sendable (AsyncStream<Void>) async -> Void) {
        let (acks, ack): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream()
        let task: Task<Void, Never> = Task(executorPreference: effectRPC) {
            await call(acks)
            await self.interrupt(id)
        }
        running[id] = (task, ack)
    }

    func interrupt(_ id: JSONValue) {
        running.removeValue(forKey: id)?.task.cancel()
    }

    func acknowledge(_ id: JSONValue) {
        running[id]?.acks.yield()
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public enum EffectRPC: Sendable {
    public static func serve<Request: Decodable & Sendable>(
        reading input: FileHandle,
        writing output: FileHandle,
        _ perform: @escaping @Sendable (Request) async -> Result<Response, any Error>,
    ) async throws {
        let logger: Logger = Logger(for: Self.self)
        let (lines, sink): (AsyncStream<Data>, AsyncStream<Data>.Continuation) = AsyncStream.makeStream()
        let calls: Calls = Calls()
        let decoder: JSONDecoder = JSONDecoder()
        let encoder: JSONEncoder = JSONEncoder()
        let send: @Sendable (FromServer) -> Void = { message in
            switch message.line(encoder) {
                case .success(let data): sink.yield(data)
                case .failure(let error): logger.debug("Response encode failed: \(error, privacy: .public)")
            }
        }
        @concurrent @Sendable func answer(_ id: JSONValue, _ request: Result<Request, any Error>, acknowledged acks: AsyncStream<Void>) async {
            let outcome: Result<Response, AggregateError<FromServer.Reason>> = await Result { () async throws(AggregateError<FromServer.Reason>) in
                try await perform(request.aggregated(FromServer.Reason.die).get())
                    .mapError { error in
                        (((error as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [any Error]).flatMap(AggregateError.init) ?? AggregateError(first: error)).map(FromServer.Reason.fail)
                    }
                    .get()
            }
            switch outcome {
                case .success(.value(let value)): send(.exit(requestId: id, .success(value)))
                case .success(.stream(let chunks)):
                    var acknowledgements: AsyncStream<Void>.Iterator = acks.makeAsyncIterator()
                    for await chunk: any Encodable & Sendable in chunks {
                        send(.chunk(requestId: id, chunk))
                        await acknowledgements.next()
                    }
                    send(.exit(requestId: id, .success(nil)))
                case .failure(let cause): send(.exit(requestId: id, .failure(cause)))
            }
        }
        try await withThrowingDiscardingTaskGroup { group in
            group.addTask(executorPreference: effectRPC) {
                for await line: Data in lines { try output.write(contentsOf: line) }
            }
            var line: Data = Data()
            for try await byte: UInt8 in input.bytes {
                line.append(byte)
                guard byte == UInt8(ascii: "\n") else { continue }
                defer { line = Data() }
                switch Result(catching: { try decoder.decode(FromClient<Request>.self, from: line) }) {
                    case .success(.request(let id, let request)): await calls.start(id) { acks in await answer(id, request, acknowledged: acks) }
                    case .success(.interrupt(let id)): await calls.interrupt(id)
                    case .success(.ack(let id)): await calls.acknowledge(id)
                    case .failure(let error): logger.debug("Message decode failed: \(error, privacy: .public)")
                }
            }
            sink.finish()
        }
    }
}
