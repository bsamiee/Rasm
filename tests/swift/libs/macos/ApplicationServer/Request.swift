import AppleEvents
import ApplicationServer
import Applications
import CustomDump
import Encoding
import Foundation
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let lines: [String] = [
    #"{"installations":{"prefix":"com.adobe."}}"#, #"{"installations":{"identifiers":["com.adobe.InDesign"]}}"#, #"{"instances":{}}"#, #"{"instances":{"prefix":"com.adobe."}}"#, #"{"pressure":{}}"#,
    #"{"applications":{"toOpen":"file:///a.indd"}}"#, #"{"read":{"pid":1}}"#, #"{"read":{"pid":1,"window":2}}"#,
    #"{"launch":{"url":"file:///A.app","documents":[],"arguments":[],"environment":{},"new":false}}"#,
    #"{"perform":{"pid":1,"actions":[{"perform":{"element":"AAEC","action":"AXPress"}},{"key":{"process":1,"code":36}}]}}"#, #"{"quit":{"pid":1,"force":false}}"#,
    #"{"terms":{"url":"file:///A.app"}}"#, #"{"definition":{"url":"file:///A.app","kind":"class","name":"document"}}"#,
    #"{"execute":{"pid":1,"command":"do script","arguments":{},"artifacts":[]}}"#, #"{"screenshot":{"pid":1,"directory":"file:///captures/"}}"#,
    #"{"screenshot":{"pid":1,"window":2,"longestSide":100,"directory":"file:///captures/"}}"#,
]
private let malformed: [String] = [#"{"status":{"pid":1}}"#, #"{"read":{}}"#, #"{"launch":{"url":"file:///A.app"}}"#, #"{"faces":{}}"#, #"{"show":{"pid":1}}"#]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Requests {
    @Test(arguments: lines)
    func wireRequestDecodes(line: String) throws {
        _ = try JSONDecoder().decode(Request.self, from: Data(line.utf8))
    }

    @Test(arguments: malformed)
    func removedOrIncompleteRequestIsRejected(line: String) {
        #expect(throws: DecodingError.self) { try JSONDecoder().decode(Request.self, from: Data(line.utf8)) }
    }

    @Test
    func launchingEncodesOneKeyPerStep() throws {
        let line: Data = try JSONEncoder().encodeLine(Launching.failed(ApplicationFailure(subject: "A", reason: .noIdentifier)))
        let decoded: JSONValue = try JSONDecoder().decode(JSONValue.self, from: line)
        guard case .object(let fields) = decoded else { throw DecodingError.dataCorrupted(.init(codingPath: [], debugDescription: "\(decoded)")) }
        expectNoDifference(Array(fields.keys), ["failed"])
    }
}
