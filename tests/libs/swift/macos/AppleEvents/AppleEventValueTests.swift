import AppleEvents
import CoreServices
import CustomDump
import Foundation
import PropertyBased
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let dictionary = Result { try ScriptingDictionary(definition: Data(#"<dictionary title="Fixture"><suite name="Fixture" code="Fixt"/></dictionary>"#.utf8)) }

private let scalar = Gen<AppleEventValue>.oneOf(
    Gen.always(AppleEventValue.null),
    Gen.bool.map(AppleEventValue.bool),
    Gen.double(in: -1e15...1e15).map(AppleEventValue.number),
    Gen.letterOrNumber.string(of: 0...12).map(AppleEventValue.text),
)

private let wire: [(AppleEventValue, String)] = [
    (.list([.null, .bool(true), .number(2.5), .text("A4")]), #"[null,true,2.5,"A4"]"#),
    (.record(type: DescType(typeAERecord), fields: ["name": .text("A4")]), #"{"name":"A4"}"#),
    (.record(type: 0x4664_6F63, fields: ["name": .text("A4")]), #"{"_tag":"record","fields":{"name":"A4"},"type":"Fdoc"}"#),
    (.type("page"), #"{"_tag":"type","name":"page"}"#),
    (.enumerator("javascript"), #"{"_tag":"enumerator","name":"javascript"}"#),
    (.date(Date(timeIntervalSince1970: 86_400)), #"{"_tag":"date","date":"1970-01-02T00:00:00Z"}"#),
    (.file(URL(filePath: "/tmp/proof.pdf")), #"{"_tag":"file","url":"file:///tmp/proof.pdf"}"#),
    (.opaque(type: 0x0000_0001, data: Data("A4".utf8), value: nil), #"{"_tag":"opaque","data":"QTQ=","type":"0x00000001"}"#),
    (.specifier(want: "page", form: .index, seld: .number(2), from: .null), #"{"_tag":"specifier","form":"index","from":null,"seld":2,"want":"page"}"#),
]

private let integers: [(DescType, [UInt8], Double?)] = [
    (DescType(typeSInt64), [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00], 0x1p53),
    (DescType(typeSInt64), [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00], nil),
    (DescType(typeSInt64), [0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00], 0x1p53 + 2),
    (DescType(typeSInt64), [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80], -0x1p63),
    (DescType(typeSInt64), [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F], nil),
    (DescType(typeSInt64), [0x01, 0x00, 0x00, 0x00], nil),
    (DescType(typeUInt64), [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80], 0x1p63),
    (DescType(typeUInt64), [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF], nil),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func json(_ value: AppleEventValue) throws -> String? {
    let encoder = JSONEncoder()
    encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
    return String(bytes: try encoder.encode(value), encoding: .utf8)
}

@Suite
struct AppleEventValueTests {
    @Test
    func readsBackEveryScalarAndListItEncodesAsADescriptor() async throws {
        let dictionary = try dictionary.get()
        await propertyCheck(input: Gen.oneOf(scalar, scalar.array(of: 0...6).map(AppleEventValue.list))) { value in
            let read = AppleEventValue(try value.descriptor(as: [], in: dictionary), in: dictionary)
            expectNoDifference(try json(read), try json(value))
        }
    }

    @Test(arguments: wire)
    func encodesAndDecodesTheWireFormOfTheClientContract(value: AppleEventValue, form: String) throws {
        expectNoDifference(try json(value), form)
        expectNoDifference(try json(JSONDecoder().decode(AppleEventValue.self, from: Data(form.utf8))), form)
    }

    @Test
    func decodesASpecifierWithoutAContainerAsOneOnTheApplication() throws {
        let read = try JSONDecoder().decode(AppleEventValue.self, from: Data(#"{"_tag":"specifier","form":"index","seld":2,"want":"page"}"#.utf8))
        expectNoDifference(try json(read), #"{"_tag":"specifier","form":"index","from":null,"seld":2,"want":"page"}"#)
    }

    @Test(arguments: integers)
    func readsA64BitIntegerAsANumberOnlyWhenADoubleHoldsItExactly(type: DescType, bytes: [UInt8], number: Double?) throws {
        let data = Data(bytes)
        let read = AppleEventValue(try #require(NSAppleEventDescriptor(descriptorType: type, data: data)), in: try dictionary.get())
        switch read {
            case .number(let value): expectNoDifference(value, number)
            case .opaque(type, data, _): expectNoDifference(nil, number)
            default: Issue.record("Read \(String(customDumping: read))")
        }
    }
}
