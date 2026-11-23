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
        let (written, outgoing) = AsyncStream<JSONValue>.makeStream()
        var peer = Self(incoming: incoming, outgoing: written.makeAsyncIterator())
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

@Suite(.timeLimit(.minutes(1)))
struct JSONRPCConnectionTests {
    @Test
    func answersConcurrentRequestsByTheirIdsWhateverTheReplyOrder() async throws {
        let (totals, sent) = try await Peer.connected { connection, peer in
            async let first = connection.request(.sum, params: [2, 3], returning: Int.self)
            async let second = connection.request(.sum, params: [10, 20], returning: Int.self)
            let requests = [try await peer.sent(), try await peer.sent()]
            for request in requests.reversed() {
                let total: Double = if case .array(let terms)? = request["params"] { terms.reduce(0) { sum, term in if case .number(let value) = term { sum + value } else { sum } } } else { 0 }
                try peer.reply(["id": try #require(request["id"]), "result": .number(total)])
            }
            return (try await [first, second], Set(requests.map { request in [request["method"], request["params"]] }))
        }
        expectNoDifference(totals, [5, 30])
        expectNoDifference(sent, [[.string("sum"), .array([.number(2), .number(3)])], [.string("sum"), .array([.number(10), .number(20)])]])
    }

    @Test
    func surfacesRefusals() async throws {
        let answer = try await Peer.connected { connection, peer in
            async let answer = Result { () async throws(JSONRPCFailure) in try await connection.request(.sum) }
            let request = try await peer.sent()
            try peer.reply(["id": try #require(request["id"]), "error": .object(["code": .number(-32_602), "message": .string("Invalid params")])])
            return await answer
        }
        guard case .failure(let failure) = answer, case .refused(let error) = failure.reason else {
            Issue.record("Unexpected \(String(customDumping: answer))")
            return
        }
        expectNoDifference(error.code, -32_602)
    }

    @Test
    func endsPendingRequestsWhenInputCloses() async throws {
        let (answer, ending) = try await Peer.connected { connection, peer in
            async let answer = Result { () async throws(JSONRPCFailure) in try await connection.request(.sum) }
            _ = try await peer.sent()
            peer.incoming.finish()
            return (await answer, await connection.finished())
        }
        guard case .failure(let pending) = answer, case .closed = pending.reason, case .closed = ending.reason else {
            Issue.record("Unexpected \(String(customDumping: answer))")
            return
        }
    }

    @Test
    func servesPeerRequests() async throws {
        let reply = try await Peer.connected(
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
        let progress = try await Peer.connected { connection, peer in
            var values = connection.subscribe(.progress, to: [String: Int].self).makeAsyncIterator()
            try peer.reply(["method": .string("progress"), "params": .object(["done": .number(4)])])
            return await values.next(isolation: nil)
        }
        expectNoDifference(try progress?.get(), ["done": 4])
    }
}
