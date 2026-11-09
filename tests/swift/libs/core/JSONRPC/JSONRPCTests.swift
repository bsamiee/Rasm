import CustomDump
import Encoding
import Foundation
import JSONRPC
import Testing

// --- [TYPES] ---------------------------------------------------------------------------

private enum Method: String, Sendable {
    case sum
    case progress
}

// --- [MODELS] --------------------------------------------------------------------------

private struct Peer {
    let incoming: AsyncThrowingStream<Data, any Error>.Continuation
    var outgoing: AsyncStream<JSONValue>.Iterator

    static func connected<Value>(
        serving: @escaping @Sendable (_ method: String, _ params: JSONValue?) async throws(JSONRPCError) -> JSONValue = { method, _ in throw JSONRPCError(code: -32_601, message: method) },
        _ body: (JSONRPCConnection<Method, JSONRPCFailure>, inout Self) async throws -> Value,
    ) async throws -> Value {
        let (lines, incoming): (AsyncThrowingStream<Data, any Error>, AsyncThrowingStream<Data, any Error>.Continuation) = AsyncThrowingStream.makeStream()
        let (written, outgoing): (AsyncStream<JSONValue>, AsyncStream<JSONValue>.Continuation) = AsyncStream.makeStream()
        var peer: Self = Self(incoming: incoming, outgoing: written.makeAsyncIterator())
        return try await JSONRPCConnection<Method, JSONRPCFailure>.run(
            reading: lines,
            writing: { line in outgoing.yield(try JSONDecoder().decode(JSONValue.self, from: line)) },
            timeout: .seconds(5),
            serving: serving,
            failure: { failure in failure },
        ) { connection in await Result { () async throws -> Value in try await body(connection, &peer) } }.get()
    }

    mutating func sent() async throws -> [String: JSONValue] {
        guard case .object(let fields)? = await outgoing.next(isolation: nil) else { throw CancellationError() }
        return fields
    }

    func reply(_ fields: [String: JSONValue]) throws {
        incoming.yield(try JSONEncoder().encode(JSONValue.object(fields.merging(["jsonrpc": .string("2.0")]) { field, _ in field })))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct JSONRPCConnectionTests {
    @Test
    func answersRequestsById() async throws {
        let total: Int = try await Peer.connected { connection, peer in
            async let total: Int = connection.request(.sum, params: [2, 3], returning: Int.self)
            let request: [String: JSONValue] = try await peer.sent()
            expectNoDifference(request["method"], .string("sum"))
            expectNoDifference(request["params"], .array([.number(2), .number(3)]))
            try peer.reply(["id": try #require(request["id"]), "result": .number(5)])
            return try await total
        }
        expectNoDifference(total, 5)
    }

    @Test
    func surfacesRefusals() async throws {
        let answer: Result<JSONValue, JSONRPCFailure> = try await Peer.connected { connection, peer in
            async let answer: Result<JSONValue, JSONRPCFailure> = Result { () async throws(JSONRPCFailure) in try await connection.request(.sum) }
            let request: [String: JSONValue] = try await peer.sent()
            try peer.reply(["id": try #require(request["id"]), "error": .object(["code": .number(-32_602), "message": .string("Invalid params")])])
            return await answer
        }
        guard case .failure(let failure) = answer, case .refused(let error) = failure.reason else {
            Issue.record("Unexpected \(String(customDumping: answer))")
            return
        }
        expectNoDifference(error.code, -32_602)
        expectNoDifference(failure.localizedDescription, "sum error -32602 Invalid params")
    }

    @Test
    func endsPendingRequestsWhenInputCloses() async throws {
        let (answer, ending): (Result<JSONValue, JSONRPCFailure>, JSONRPCFailure) = try await Peer.connected { connection, peer in
            async let answer: Result<JSONValue, JSONRPCFailure> = Result { () async throws(JSONRPCFailure) in try await connection.request(.sum) }
            _ = try await peer.sent()
            peer.incoming.finish()
            return (await answer, await connection.finished())
        }
        guard case .failure(let pending) = answer, case .closed = pending.reason, case .closed = ending.reason else {
            Issue.record("Unexpected \(String(customDumping: answer))")
            return
        }
        expectNoDifference(pending.localizedDescription, "Connection closed")
    }

    @Test
    func servesPeerRequests() async throws {
        let reply: [String: JSONValue] = try await Peer.connected(
            serving: { (method: String, params: JSONValue?) async throws(JSONRPCError) -> JSONValue in
                guard method == Method.sum.rawValue, case .array(let terms)? = params else { throw JSONRPCError(code: -32_601, message: method) }
                return .number(terms.reduce(0) { total, term in if case .number(let value) = term { total + value } else { total } })
            },
            { _, peer in
                try peer.reply(["id": .number(7), "method": .string("sum"), "params": .array([.number(1), .number(2)])])
                return try await peer.sent()
            },
        )
        expectNoDifference(reply["id"], .number(7))
        expectNoDifference(reply["result"], .number(3))
    }

    @Test
    func deliversNotifications() async throws {
        let progress: Result<[String: Int], JSONRPCFailure>? = try await Peer.connected { connection, peer in
            var values: AsyncStream<Result<[String: Int], JSONRPCFailure>>.Iterator = connection.subscribe(.progress, to: [String: Int].self).makeAsyncIterator()
            try peer.reply(["method": .string("progress"), "params": .object(["done": .number(4)])])
            return await values.next(isolation: nil)
        }
        expectNoDifference(try progress?.get(), ["done": 4])
    }
}
