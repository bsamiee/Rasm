import AppleEvents
import CasePathsCore
import CoreServices
import CustomDump
import Foundation
import Results
import Testing

// --- [CONSTANTS] -----------------------------------------------------------------------

private let definition = Data(
    """
    <dictionary title="Fixture">
        <suite name="Standard Suite" code="core">
            <command name="activate" code="miscactv"/>
            <class name="application" code="capp">
                <property name="name" code="pnam" type="text"/>
                <property name="frontmost" code="pisf" type="boolean"/>
                <property name="properties" code="pALL" type="record"/>
            </class>
        </suite>
    </dictionary>
    """.utf8
)

private let frontmost = AppleEventValue.specifier(want: "prop", form: .property, seld: .type("frontmost"), from: .null)
private let properties = AppleEventValue.specifier(want: "prop", form: .property, seld: .type("properties"), from: .null)
private let name = AppleEventValue.specifier(want: "prop", form: .property, seld: .type("name"), from: .null)
private let raised: [String: AppleEventValue] = ["frontmost": .bool(true), "name": .text("Front")]
private let renamed: [String: AppleEventValue] = ["name": .text("Front")]
private let events: [(String, [String: AppleEventValue], [String], [String])] = [
    ("set", ["direct": frontmost, "to": .bool(true)], [], ["activating"]),
    ("set", ["direct": frontmost, "to": .bool(false)], [], ["activating"]),
    ("set", ["direct": properties, "to": .record(type: DescType(typeAERecord), fields: raised)], [], ["activating"]),
    ("set", ["direct": properties, "to": .record(type: DescType(typeAERecord), fields: renamed)], [], ["coresetd", "pALL"]),
    ("set", ["direct": name, "to": .text("Front")], [], ["coresetd", "pnam"]),
    ("set", ["direct": name, "to": .text("Front")], ["coresetd"], ["activating"]),
    ("activate", [:], [], ["activating"]),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct EventTests {
    let dictionary: ScriptingDictionary

    init() throws { dictionary = try ScriptingDictionary(definition: definition) }

    @Test(arguments: events)
    func refusesEveryEventThatActivatesWritesTheFrontmostStateOrIsListedAsActivating(command: String, arguments: [String: AppleEventValue], refused: [String], outcome: [String]) throws {
        let codes = Set(try refused.map { code in try #require(UInt64(sdef: code)) })
        let resolved = try dictionary.command(command, taking: Set(arguments.keys))
        let built = Result { () throws(AggregateError<AppleEventFailure>) in
            try dictionary.event(resolved, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: getpid()), refusing: codes)
        }
        let observed: [String] =
            switch built {
                case .success(let event):
                    [(UInt64(event.eventClass) << 32 | UInt64(event.eventID)).sdef]
                        + [event.paramDescriptor(forKeyword: AEKeyword(keyDirectObject))?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue.sdef].compactMap(\.self)
                case .failure(let failure): failure.errors.map { failure in failure.reason.is(\.activating) ? "activating" : failure.localizedDescription }
            }
        expectNoDifference(observed, outcome)
    }
}
