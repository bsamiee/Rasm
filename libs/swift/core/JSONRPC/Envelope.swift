public import Encoding

// --- [MODELS] --------------------------------------------------------------------------

struct Envelope<Payload: Sendable, Fault: Sendable>: Sendable {
    var jsonrpc: String? = "2.0"
    var id: JSONValue?
    var method: String?
    var params: Payload?
    var chunk: Bool?
    var result: Payload?
    var error: Fault?
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
