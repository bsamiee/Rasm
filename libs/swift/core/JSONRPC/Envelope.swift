public import Encoding

// --- [MODELS] --------------------------------------------------------------------------

package struct Envelope<Payload: Sendable, Fault: Sendable>: Sendable {
    var jsonrpc: String? = "2.0"
    package var id: JSONValue?
    package var method: String?
    package var params: Payload?
    var chunk: Bool?
    var result: Payload?
    var error: Fault?

    package init(id: JSONValue? = nil, method: String? = nil, params: Payload? = nil, chunk: Bool? = nil, result: Payload? = nil, error: Fault? = nil) {
        (self.id, self.method, self.params, self.chunk, self.result, self.error) = (id, method, params, chunk, result, error)
    }
}

extension Envelope: Decodable where Payload: Decodable, Fault: Decodable {}

extension Envelope: Encodable where Payload: Encodable, Fault: Encodable {}

// --- [ERRORS] --------------------------------------------------------------------------

public struct JSONRPCError: Error, Codable {
    public let code: Int
    public let message: String
    public let data: JSONValue?

    public init(code: Int, message: String, data: JSONValue? = nil) { (self.code, self.message, self.data) = (code, message, data) }
}
