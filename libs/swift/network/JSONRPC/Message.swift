public import Encoding

// --- [MODELS] --------------------------------------------------------------------------

struct Message<Payload: Sendable, Fault: Sendable>: Sendable {
    enum Version: String, Codable, Sendable {
        case two = "2.0"
    }

    var jsonrpc: Version? = .two
    var id: JSONValue?
    var method: String?
    var params: Payload?
    var chunk: Bool?
    var result: Payload?
    var error: Fault?
    var traceId: String?
    var spanId: String?
    var sampled: Bool?
}

extension Message: Decodable where Payload: Decodable, Fault: Decodable {
    init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        jsonrpc = try fields.decodeIfPresent(Version.self, forKey: .jsonrpc)
        id = try fields.decodeIfPresent(JSONValue.self, forKey: .id)
        method = try fields.decodeIfPresent(String.self, forKey: .method)
        params = try fields.decodeIfPresent(Payload.self, forKey: .params)
        result = fields.contains(.result) ? try fields.decode(Payload.self, forKey: .result) : nil
        error = fields.contains(.error) ? try fields.decode(Fault.self, forKey: .error) : nil
        traceId = try fields.decodeIfPresent(String.self, forKey: .traceId)
        spanId = try fields.decodeIfPresent(String.self, forKey: .spanId)
        sampled = try fields.decodeIfPresent(Bool.self, forKey: .sampled)
        guard id == nil || method != nil || fields.contains(.result) != fields.contains(.error) else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "Response holds both or neither of result and error"))
        }
    }
}

extension Message: Encodable where Payload: Encodable, Fault: Encodable {}

// --- [ERRORS] --------------------------------------------------------------------------

public struct JSONRPCError: Error, Codable {
    public let code: Int
    public let message: String
    public let data: JSONValue?

    public init(code: Int, message: String, data: JSONValue? = nil) { (self.code, self.message, self.data) = (code, message, data) }
}
