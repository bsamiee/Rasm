import AppleEvents
import CasePathsCore
import CoreServices
import CustomDump
import Encoding
import Foundation
import Results
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let definition = Data(
    """
    <?xml version="1.0" encoding="UTF-8"?>
    <dictionary title="Fixture">
        <suite name="Standard Suite" code="core">
            <command name="open" code="aevtodoc">
                <synonym name="launch file"/>
                <direct-parameter type="file"/>
            </command>
            <command name="purge" code="Fixtprge" hidden="yes"/>
            <command name="do script" code="miscdosc">
                <direct-parameter type="text"/>
                <parameter name="language" code="Lang" type="script language" optional="yes">
                    <synonym name="runtime"/>
                </parameter>
            </command>
            <class name="application" code="capp">
                <element type="document"/>
                <property name="name" code="pnam" type="text"/>
            </class>
            <class name="item" code="cobj">
                <property name="name" code="pnam" type="text"/>
            </class>
            <class name="document" code="docu" inherits="item">
                <synonym name="file document"/>
                <element type="page"/>
                <property name="modified" code="imod" type="boolean">
                    <synonym code="Fmod"/>
                </property>
            </class>
            <class name="page" code="Fpag" plural="page list" inherits="item"/>
            <class name="spread" code="Fspr" hidden="yes"/>
            <class-extension extends="document">
                <element type="spread"/>
                <property name="page count" code="Fpgc" type="integer">
                    <synonym name="pages total"/>
                </property>
            </class-extension>
            <enumeration name="script language" code="Fsla">
                <enumerator name="javascript" code="Fjvs"/>
                <enumerator name="uxpscript" code="USLg"/>
            </enumeration>
        </suite>
    </dictionary>
    """.utf8
)

// --- [MODELS] --------------------------------------------------------------------------

private struct Row: Decodable {
    let name: String
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func desiredClass(_ value: AppleEventValue, in dictionary: ScriptingDictionary) throws -> String? {
    try value.descriptor(as: [], in: dictionary).forKeyword(AEKeyword(keyAEDesiredClass))?.typeCodeValue.sdef
}

private func candidates(_ value: AppleEventValue, in dictionary: ScriptingDictionary) -> [String] {
    do {
        _ = try value.descriptor(as: [], in: dictionary)
        return []
    } catch {
        return error.errors.flatMap { failure in failure.reason[case: \.unmatched]?.candidates ?? [] }
    }
}

@Suite
struct ScriptingDictionaryTests {
    let dictionary: ScriptingDictionary

    init() throws { dictionary = try ScriptingDictionary(definition: definition) }

    @Test
    func listsVisibleTermsAlone() throws {
        let listing = try JSONDecoder().decode([String: JSONValue].self, from: JSONEncoder().encode(dictionary))
        expectNoDifference(try listing["command"]?.decode([Row].self).map(\.name), ["open", "do script"])
        expectNoDifference(try listing["class"]?.decode([Row].self).map(\.name), ["application", "item", "document", "page"])
    }

    @Test
    func resolvesCommandsByName() throws {
        expectNoDifference(try dictionary.command(.name("launch file"), taking: []).name, "open")
        expectNoDifference(try dictionary.command(nil, taking: ["runtime"]).name, "do script")
    }

    @Test
    func resolvesClassesByPluralAndSynonym() throws {
        let document = AppleEventValue.specifier(want: "documents", form: .index, seld: .number(1), from: .null)
        expectNoDifference(try desiredClass(document, in: dictionary), "docu")
        expectNoDifference(try desiredClass(.specifier(want: "page list", form: .index, seld: .number(1), from: document), in: dictionary), "Fpag")
        expectNoDifference(try desiredClass(.specifier(want: "spread", form: .index, seld: .number(1), from: document), in: dictionary), "Fspr")
        expectNoDifference(try AppleEventValue.type("file document").descriptor(as: [], in: dictionary).typeCodeValue.sdef, "docu")
    }

    @Test
    func listsContainerElementsFirstAmongCandidates() {
        let document = AppleEventValue.specifier(want: "documents", form: .index, seld: .number(1), from: .null)
        let listed = candidates(.specifier(want: "pages", form: .index, seld: .number(1), from: document), in: dictionary)
        expectNoDifference(listed.first, "page")
        expectNoDifference(listed.contains("spread"), false)
    }

    @Test
    func readsPropertiesBySynonym() throws {
        expectNoDifference(try AppleEventValue.type("pages total").descriptor(as: [], in: dictionary).typeCodeValue.sdef, "Fpgc")
        let decoded = AppleEventValue(NSAppleEventDescriptor(typeCode: try #require(FourCharCode(sdef: "Fmod"))), in: dictionary)
        expectNoDifference(try JSONDecoder().decode(JSONValue.self, from: JSONEncoder().encode(decoded)), .object(["_tag": .string("type"), "name": .string("modified")]))
    }
}
