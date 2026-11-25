import AsyncAlgorithms
import CustomDump
import Encoding
import Foundation
import JSONRPC
import Results
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private enum Request: Decodable, Sendable {
    case add(left: Int, right: Int)
    case find(name: String)
    case count(to: Int)
    case buffered(to: Int)
    case wait(id: Int)
}

private struct Session {
    let input: AsyncThrowingStream<Data, any Error>.Continuation
    var output: AsyncStream<JSONValue>.Iterator
    var events: AsyncStream<String>.Iterator
    let serving: Task<Void, any Error>

    init() {
        let (lines, input) = AsyncThrowingStream<Data, any Error>.makeStream()
        let (written, writer) = AsyncStream<JSONValue>.makeStream()
        let (events, event) = AsyncStream<String>.makeStream()
        (self.input, output, self.events) = (input, written.makeAsyncIterator(), events.makeAsyncIterator())
        serving = Task {
            defer {
                writer.finish()
                event.finish()
            }
            try await serve(
                reading: lines,
                writing: { line in writer.yield(try JSONDecoder().decode(JSONValue.self, from: line)) },
                dropped: { failure in Issue.record(failure) },
                { (request: Request) async throws -> Response in
                    switch request {
                        case .add(let left, let right): return .value(left - right)
                        case .find(let name): throw Unmatched(subject: "application", query: name, candidates: ["Finder"])
                        case .count(let limit): return .stream([any Encodable & Sendable](Array(1...limit)).async)
                        case .buffered(let limit):
                            let (chunks, chunk) = AsyncStream<Int>.makeStream()
                            for value in 1...limit { chunk.yield(value) }
                            chunk.finish()
                            return .stream(
                                chunks.map { value -> any Encodable & Sendable in
                                    event.yield("pulled \(value)")
                                    return value
                                }
                            )
                        case .wait(let id):
                            event.yield("started \(id)")
                            do { try await Task.sleep(for: .seconds(30)) } catch { event.yield("cancelled \(id)") }
                            throw CancellationError()
                    }
                },
            )
        }
    }

    func send(_ message: JSONValue) throws { input.yield(try JSONEncoder().encodeLine(message)) }

    func call(_ id: Int, _ method: String, _ params: [String: JSONValue]) throws {
        try send(.object(["jsonrpc": .string("2.0"), "id": .number(Double(id)), "method": .string(method), "params": .object(params)]))
    }

    func control(_ method: String, _ id: Int) throws {
        try send(.object(["jsonrpc": .string("2.0"), "method": .string(method), "params": .object(["requestId": .number(Double(id))])]))
    }

    mutating func reply() async throws -> JSONValue {
        guard let line = await output.next(isolation: nil) else { throw CancellationError() }
        return line
    }

    mutating func event() async -> String? { await events.next(isolation: nil) }

    mutating func closed() async throws -> [JSONValue] {
        input.finish()
        try await serving.value
        var rest: [JSONValue] = []
        while let line = await output.next(isolation: nil) { rest.append(line) }
        return rest
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func answer(_ id: Int, _ result: JSONValue) -> JSONValue {
    .object(["jsonrpc": .string("2.0"), "id": .number(Double(id)), "result": result])
}

private func chunk(_ id: Int, _ value: Double) -> JSONValue {
    .object(["jsonrpc": .string("2.0"), "id": .number(Double(id)), "chunk": .bool(true), "result": .array([.number(value)])])
}

private func cause(of reply: JSONValue) -> (id: JSONValue?, reasons: [[String: JSONValue]]) {
    guard case .object(let fields) = reply, case .object(let cause)? = fields["error"], cause["_tag"] == .string("Cause"), case .array(let reasons)? = cause["data"] else { return (nil, []) }
    return (fields["id"], reasons.map { reason in if case .object(let fields) = reason { fields } else { [:] } })
}

@Suite(.timeLimit(.minutes(1)))
struct ServeTests {
    @Test
    func answersARequestWithItsResultUnderItsId() async throws {
        var session = Session()
        try session.call(31, "add", ["left": .number(9), "right": .number(4)])
        let replies = [try await session.reply()] + (try await session.closed())
        expectNoDifference(replies, [answer(31, .number(5))])
    }

    @Test
    func encodesAThrownTaggedErrorAsAFailCauseHoldingItsFields() async throws {
        var session = Session()
        try session.call(2, "find", ["name": .string("Xcode")])
        let failed = cause(of: try await session.reply())
        let rest = try await session.closed()
        let error: [String: JSONValue]? = if case .object(let fields)? = failed.reasons.first?["error"] { fields.filter { key, _ in key != "message" } } else { nil }
        expectNoDifference(failed.id, .number(2))
        expectNoDifference(failed.reasons.map { reason in reason["_tag"] }, [.string("Fail")])
        expectNoDifference(error, ["_tag": .string("Unmatched"), "subject": .string("application"), "query": .string("Xcode"), "candidates": .array([.string("Finder")])])
        expectNoDifference(rest, [])
    }

    @Test
    func encodesAnUndecodableRequestAsADieCauseHoldingItsDefect() async throws {
        var session = Session()
        try session.call(3, "missing", [:])
        let died = cause(of: try await session.reply())
        let rest = try await session.closed()
        expectNoDifference(died.id, .number(3))
        expectNoDifference(died.reasons.map { reason in reason.keys.sorted() }, [["_tag", "defect"]])
        expectNoDifference(died.reasons.map { reason in reason["_tag"] }, [.string("Die")])
        expectNoDifference(rest, [])
    }

    @Test
    func sendsEachStreamChunkOnlyAfterTheAckOfThePreviousOne() async throws {
        var session = Session()
        try session.call(4, "count", ["to": .number(3)])
        var replies = [try await session.reply()]
        try session.control("@effect/rpc/Ack", 40)
        try session.call(5, "add", ["left": .number(3), "right": .number(1)])
        replies.append(try await session.reply())
        for _ in 1...3 {
            try session.control("@effect/rpc/Ack", 4)
            replies.append(try await session.reply())
        }
        replies += try await session.closed()
        expectNoDifference(replies, [chunk(4, 1), answer(5, .number(2)), chunk(4, 2), chunk(4, 3), answer(4, .null)])
    }

    @Test
    func interruptCancelsTheRunningCall() async throws {
        var session = Session()
        try session.call(6, "wait", ["id": .number(6)])
        let started = await session.event()
        try session.control("@effect/rpc/Interrupt", 6)
        let cancelled = await session.event()
        _ = try await session.closed()
        expectNoDifference([started, cancelled], ["started 6", "cancelled 6"])
    }

    @Test
    func dropsACallReusingTheIdOfARunningOne() async throws {
        var session = Session()
        try session.call(7, "wait", ["id": .number(7)])
        try session.call(7, "wait", ["id": .number(70)])
        try session.call(8, "add", ["left": .number(15), "right": .number(5)])
        let first = try await session.reply()
        try session.control("@effect/rpc/Interrupt", 7)
        let rest = try await session.closed()
        var events: [String] = []
        while let event = await session.event() { events.append(event) }
        expectNoDifference(first, answer(8, .number(10)))
        expectNoDifference(rest.map { reply in cause(of: reply).id }, [.number(7)])
        expectNoDifference(events, ["started 7", "cancelled 7"])
    }

    @Test
    func closingTheInputEndsAStreamWithoutItsNextChunkOrCompletion() async throws {
        var session = Session()
        try session.call(9, "count", ["to": .number(3)])
        let first = try await session.reply()
        let rest = try await session.closed()
        expectNoDifference([first] + rest, [chunk(9, 1)])
    }

    @Test
    func interruptStopsAStreamBeforeItsNextChunkWhateverTheSourceHolds() async throws {
        var session = Session()
        try session.call(10, "buffered", ["to": .number(3)])
        let first = try await session.reply()
        try session.control("@effect/rpc/Interrupt", 10)
        let rest = try await session.closed()
        var events: [String] = []
        while let event = await session.event() { events.append(event) }
        expectNoDifference([first] + rest, [chunk(10, 1)])
        expectNoDifference(events, ["pulled 1"])
    }

    @Test(arguments: [true, false])
    func endsWithTheWriterFailureWhetherOrNotTheInputCloses(closing: Bool) async throws {
        let (lines, input) = AsyncThrowingStream<Data, any Error>.makeStream()
        for id in [11, 12] {
            let call = JSONValue.object(["jsonrpc": .string("2.0"), "id": .number(Double(id)), "method": .string("add"), "params": .object(["left": .number(2), "right": .number(1)])])
            input.yield(try JSONEncoder().encodeLine(call))
        }
        if closing { input.finish() }
        let ended = await Result { () async throws(JSONRPCFailure) in
            try await serve(reading: lines, writing: { _ in throw POSIXError(.EPIPE) }, dropped: { failure in Issue.record(failure) }) { (_: Request) in Response.value(nil) }
        }
        let cause: POSIXError.Code? = if case .failure(let failure) = ended, case .transport(let error as POSIXError) = failure.reason { error.code } else { nil }
        expectNoDifference(cause, .EPIPE)
    }
}
