import AsyncAlgorithms
import CustomDump
import EffectRPC
import Encoding
import Foundation
import Results
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private enum Request: Decodable, Sendable {
    case add(left: Int, right: Int)
    case find(name: String)
    case count(to: Int)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func served(_ requests: [JSONValue]) async throws -> [JSONValue] {
    let lines: [Data] = try requests.map { request in try JSONEncoder().encodeLine(request) }
    let (written, writer) = AsyncStream<JSONValue>.makeStream()
    try await serve(
        reading: AsyncThrowingStream<Data, any Error> { input in
            for line: Data in lines { input.yield(line) }
            input.finish()
        },
        writing: { line in writer.yield(try JSONDecoder().decode(JSONValue.self, from: line)) },
        dropped: { failure in Issue.record(failure) },
        { (request: Request) async throws(Unmatched) -> Response in
            switch request {
                case .add(let left, let right): .value(left + right)
                case .find(let name): throw Unmatched(subject: "application", query: name, candidates: ["Finder"])
                case .count(let limit): .stream([any Encodable & Sendable](Array(1...limit)).async)
            }
        },
    )
    writer.finish()
    return await Array(written)
}

private func call(_ id: Int, _ method: String, _ params: JSONValue) -> JSONValue {
    .object(["jsonrpc": .string("2.0"), "id": .number(Double(id)), "method": .string(method), "params": params])
}

@Suite
struct ServeTests {
    @Test
    func answersEachRequestBeforeInputEnds() async throws {
        let replies: [JSONValue] = try await served([call(1, "add", .object(["left": .number(2), "right": .number(3)]))])
        expectNoDifference(replies, [.object(["jsonrpc": .string("2.0"), "id": .number(1), "result": .number(5)])])
    }

    @Test
    func encodesTaggedFailures() async throws {
        let replies: [JSONValue] = try await served([call(2, "find", .object(["name": .string("Xcode")]))])
        guard case .object(let reply)? = replies.first, case .object(let cause)? = reply["error"], case .array(let reasons)? = cause["data"] else {
            Issue.record("Unexpected \(String(customDumping: replies))")
            return
        }
        expectNoDifference(cause["_tag"], .string("Cause"))
        expectNoDifference(
            reasons,
            [
                .object([
                    "_tag": .string("Fail"), "error": .object(["_tag": .string("Unmatched"), "subject": .string("application"), "query": .string("Xcode"), "candidates": .array([.string("Finder")])]),
                ])
            ],
        )
    }

    @Test
    func diesOnUndecodableRequests() async throws {
        let replies: [JSONValue] = try await served([call(3, "missing", .object([:]))])
        guard case .object(let reply)? = replies.first, case .object(let cause)? = reply["error"], case .array(let reasons)? = cause["data"], case .object(let reason)? = reasons.first else {
            Issue.record("Unexpected \(String(customDumping: replies))")
            return
        }
        expectNoDifference(reason["_tag"], .string("Die"))
    }

    @Test
    func interruptsStreamsWhenInputEnds() async throws {
        let replies: [JSONValue] = try await served([call(4, "count", .object(["to": .number(3)]))])
        expectNoDifference(replies.filter { reply in if case .object(let fields) = reply { fields["chunk"] != .bool(true) } else { true } }, [])
    }

    @Test
    func skipsControlMessagesForUnknownCalls() async throws {
        let replies: [JSONValue] = try await served([.object(["jsonrpc": .string("2.0"), "method": .string("@effect/rpc/Ack"), "params": .object(["requestId": .number(9)])])])
        expectNoDifference(replies, [])
    }
}
