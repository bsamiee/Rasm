import CustomDump
import Encoding
import Foundation
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let account: String = #"{"name":"ada","plan":"pro","mcpOAuth":{"server":{"token":"t","expires":[1,2]}},"version":3,"flags":null}"#

// --- [MODELS] --------------------------------------------------------------------------

private struct Account: Codable, Equatable, Sendable {
    let name: String
    let plan: String
}

private struct Plan: Codable, Equatable, Sendable {
    let plan: String
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func value(of data: Data) throws -> JSONValue {
    try JSONDecoder().decode(JSONValue.self, from: data)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct JSONDocuments {
    @Test
    func decodesKnownFieldsAndKeepsEveryOtherField() throws {
        let document: JSONDocument<Account> = try JSONDecoder().decode(JSONDocument<Account>.self, from: Data(account.utf8))
        expectNoDifference(document.known, Account(name: "ada", plan: "pro"))
        expectNoDifference(document.fields["mcpOAuth"], .object(["server": .object(["token": .string("t"), "expires": .array([.number(1), .number(2)])])]))
        expectNoDifference(document.fields["version"], .number(3))
        expectNoDifference(document.fields["flags"], .null)
    }

    @Test
    func reencodesAnUnchangedDocumentToTheSameObject() throws {
        let original: Data = Data(account.utf8)
        let document: JSONDocument<Account> = try JSONDecoder().decode(JSONDocument<Account>.self, from: original)
        expectNoDifference(try value(of: sortedEncoder().encode(document)), try value(of: original))
    }

    @Test
    func replacesKnownFieldsAndKeepsTheRest() throws {
        let document: JSONDocument<Account> = try JSONDecoder().decode(JSONDocument<Account>.self, from: Data(account.utf8))
        let edited: JSONDocument<Account> = JSONDocument(fields: document.fields, known: Account(name: "grace", plan: "max"))
        let expected: Data = Data(#"{"name":"grace","plan":"max","mcpOAuth":{"server":{"token":"t","expires":[1,2]}},"version":3,"flags":null}"#.utf8)
        expectNoDifference(try value(of: sortedEncoder().encode(edited)), try value(of: expected))
    }

    @Test
    func knowsOnlyTheFieldsItsTypeDeclares() throws {
        let document: JSONDocument<Plan> = try JSONDecoder().decode(JSONDocument<Plan>.self, from: Data(account.utf8))
        let edited: JSONDocument<Plan> = JSONDocument(fields: document.fields, known: Plan(plan: "team"))
        let expected: Data = Data(#"{"name":"ada","plan":"team","mcpOAuth":{"server":{"token":"t","expires":[1,2]}},"version":3,"flags":null}"#.utf8)
        expectNoDifference(try value(of: sortedEncoder().encode(edited)), try value(of: expected))
    }

    @Test
    func rejectsTextWhoseKnownFieldsDoNotDecode() {
        for text: String in [#"{"name":"ada"}"#, #"{"name":1,"plan":"pro"}"#, #"["name","plan"]"#, #""ada""#] {
            #expect(throws: DecodingError.self, "\(text)") { try JSONDecoder().decode(JSONDocument<Account>.self, from: Data(text.utf8)) }
        }
    }
}
