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
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func served(_ requests: JSONValue...) async throws -> [JSONValue] {
    let (input, reading) = AsyncThrowingStream<Data, any Error>.makeStream()
    let (written, writer) = AsyncStream<JSONValue>.makeStream()
    for request in requests { reading.yield(try JSONEncoder().encodeLine(request)) }
    try await serve(
        reading: input,
        writing: { line in
            writer.yield(try JSONDecoder().decode(JSONValue.self, from: line))
            reading.finish()
        },
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
    func answersARequestWithItsResult() async throws {
        let replies = try await served(call(1, "add", .object(["left": .number(2), "right": .number(3)])))
        expectNoDifference(replies, [.object(["jsonrpc": .string("2.0"), "id": .number(1), "result": .number(5)])])
    }

    @Test
    func encodesTaggedFailures() async throws {
        let replies = try await served(call(2, "find", .object(["name": .string("Xcode")])))
        guard case .object(let reply)? = replies.first, case .object(let cause)? = reply["error"], case .array(let reasons)? = cause["data"] else {
            Issue.record("Unexpected \(String(customDumping: replies))")
            return
        }
        expectNoDifference(cause["_tag"], .string("Cause"))
        expectNoDifference(
            reasons,
            [
                .object([
                    "_tag": .string("Fail"),
                    "error": .object([
                        "_tag": .string("Unmatched"), "subject": .string("application"), "query": .string("Xcode"), "candidates": .array([.string("Finder")]),
                        "message": .string("No application matching Xcode, candidates: Finder"),
                    ]),
                ])
            ],
        )
    }

    @Test
    func diesOnUndecodableRequests() async throws {
        let replies = try await served(call(3, "missing", .object([:])))
        guard case .object(let reply)? = replies.first, case .object(let cause)? = reply["error"], case .array(let reasons)? = cause["data"], case .object(let reason)? = reasons.first else {
            Issue.record("Unexpected \(String(customDumping: replies))")
            return
        }
        expectNoDifference(reason["_tag"], .string("Die"))
    }

    @Test(arguments: ["@effect/rpc/Ack", "@effect/rpc/Interrupt"])
    func skipsControlMessagesForUnknownCalls(control: String) async throws {
        let replies = try await served(
            .object(["jsonrpc": .string("2.0"), "method": .string(control), "params": .object(["requestId": .number(9)])]),
            call(5, "add", .object(["left": .number(1), "right": .number(1)])),
        )
        expectNoDifference(replies, [.object(["jsonrpc": .string("2.0"), "id": .number(5), "result": .number(2)])])
    }

    @Test
    func endsAStreamWithoutItsCompletionWhenInputEnds() async throws {
        let replies = try await served(call(4, "count", .object(["to": .number(3)])))
        let chunk: (JSONValue) -> Bool = { reply in if case .object(let fields) = reply { fields["chunk"] == .bool(true) } else { false } }
        expectNoDifference(replies.first, .object(["jsonrpc": .string("2.0"), "id": .number(4), "chunk": .bool(true), "result": .array([.number(1)])]))
        expectNoDifference(replies.filter { reply in !chunk(reply) }, [])
    }
}
