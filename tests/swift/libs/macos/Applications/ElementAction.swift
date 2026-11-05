import Applications
import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let action: Generator<ElementAction, AnySequence<Any>> = zip(
    Gen<UInt8>.value().array(of: 0...48).map { bytes in Data(bytes).base64EncodedString() },
    Gen<Character>.character(in: "\u{20}"..."\u{10FF}").string(of: 0...24),
    Gen<Bool>.bool,
)
.compactMap { text, value, performs in
    Element.ID(base64Encoded: text).map { element in performs ? ElementAction.perform(element: element, action: value) : ElementAction.set(element: element, value: value) }
}
.eraseToAny()
private let malformed: [String] = [
    #"{"perform":{"element":"not base64!","action":"AXPress"}}"#, #"{"perform":{"element":"AAEC"}}"#, #"{"press":{"element":"AAEC","action":"AXPress"}}"#,
    #"{"set":{"element":"AAEC","value":7}}"#, #"{"key":{"process":1}}"#,
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func id(_ text: String) throws -> Element.ID { try #require(Element.ID(base64Encoded: text)) }

// --- [COMPOSITION] ---------------------------------------------------------------------

struct ElementActions {
    @Test
    func actionEncodesWireShapeAndRoundTrips() async {
        await propertyCheck(input: action) { action in
            let expected: JSONValue =
                switch action {
                    case .perform(let element, let name): .object(["perform": .object(["element": .string(element.description), "action": .string(name)])])
                    case .set(let element, let value): .object(["set": .object(["element": .string(element.description), "value": .string(value)])])
                    case .key(let process, let code): .object(["key": .object(["process": .number(Double(process)), "code": .number(Double(code))])])
                }
            expectNoDifference(try wire(action), expected)
            expectNoDifference(try JSONDecoder().decode(ElementAction.self, from: JSONEncoder().encodeLine(action)), action)
        }
    }

    @Test
    func wireActionsDecode() throws {
        let decoded: [(line: String, action: ElementAction)] = [
            (#"{"perform":{"element":"AAEC","action":"AXPress"}}"#, .perform(element: try id("AAEC"), action: "AXPress")),
            (#"{"set":{"element":"/w==","value":"Größe ✓"}}"#, .set(element: try id("/w=="), value: "Größe ✓")),
            (#"{"key":{"process":4242,"code":36}}"#, .key(process: 4242, code: 36)),
        ]
        for (line, action): (String, ElementAction) in decoded {
            expectNoDifference(try JSONDecoder().decode(ElementAction.self, from: Data(line.utf8)), action)
        }
    }

    @Test(arguments: malformed)
    func malformedWireActionIsRejected(line: String) {
        #expect(throws: DecodingError.self) { try JSONDecoder().decode(ElementAction.self, from: Data(line.utf8)) }
    }
}
