import Applications
import CustomDump
import Encoding
import Foundation
import PropertyBased
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let action: Generator<ElementAction, AnySequence<Any>> = zip(
    Gen<UInt8>.value().array(of: 0...48).map { bytes in Data(bytes) },
    Gen<Character>.character(in: "\u{20}"..."\u{10FF}").string(of: 0...24),
    Gen<Bool>.bool,
)
.map { element, text, performs in performs ? ElementAction.perform(element: element, action: text) : ElementAction.set(element: element, value: text) }
.eraseToAny()
private let decoded: [(line: String, action: ElementAction)] = [
    (#"{"perform":{"element":"AAEC","action":"AXPress"}}"#, .perform(element: Data([0, 1, 2]), action: "AXPress")),
    (#"{"set":{"element":"/w==","value":"Größe ✓"}}"#, .set(element: Data([255]), value: "Größe ✓")),
]
private let malformed: [String] = [
    #"{"perform":{"element":"not base64!","action":"AXPress"}}"#, #"{"perform":{"element":"AAEC"}}"#, #"{"press":{"element":"AAEC","action":"AXPress"}}"#,
    #"{"set":{"element":"AAEC","value":7}}"#,
]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct ElementActions {
    @Test
    func actionEncodesWirePerformShapeAndRoundTrips() async {
        await propertyCheck(input: action) { action in
            let expected: JSONValue =
                switch action {
                    case .perform(let element, let name): .object(["perform": .object(["element": .string(element.base64EncodedString()), "action": .string(name)])])
                    case .set(let element, let value): .object(["set": .object(["element": .string(element.base64EncodedString()), "value": .string(value)])])
                }
            expectNoDifference(try wire(action), expected)
            expectNoDifference(try JSONDecoder().decode(ElementAction.self, from: JSONEncoder().encodeLine(action)), action)
        }
    }

    @Test(arguments: decoded)
    func wireActionDecodes(line: String, action: ElementAction) throws {
        expectNoDifference(try JSONDecoder().decode(ElementAction.self, from: Data(line.utf8)), action)
    }

    @Test(arguments: malformed)
    func malformedWireActionIsRejected(line: String) {
        #expect(throws: DecodingError.self) { try JSONDecoder().decode(ElementAction.self, from: Data(line.utf8)) }
    }
}
