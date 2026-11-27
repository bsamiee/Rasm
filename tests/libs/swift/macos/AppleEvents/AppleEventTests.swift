import CasePathsCore
import CoreServices
import CustomDump
import Fixture
import Foundation
import Results
import Testing

@testable import AppleEvents

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
private let activating: [(String, [String: AppleEventValue])] = [
    ("set", ["direct": frontmost, "to": .bool(true)]),
    ("set", ["direct": frontmost, "to": .bool(false)]),
    ("set", ["direct": properties, "to": .record(type: DescType(typeAERecord), fields: raised)]),
    ("activate", [:]),
]
private let built: [(String, [String: AppleEventValue], [String])] = [
    ("set", ["direct": properties, "to": .record(type: DescType(typeAERecord), fields: renamed)], ["coresetd", "pALL"]),
    ("set", ["direct": name, "to": .text("Front")], ["coresetd", "pnam"]),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct AppleEventTests {
    let dictionary: ScriptingDictionary

    init() throws { dictionary = try ScriptingDictionary(definition: definition) }

    @Test(arguments: activating)
    func refusesEveryEventThatActivatesOrWritesTheFrontmostState(command: String, arguments: [String: AppleEventValue]) throws {
        let resolved = try dictionary.command(.name(command), taking: Set(arguments.keys))
        let refusal = #expect(throws: AggregateError<AppleEventFailure>.self) {
            try dictionary.event(resolved, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: getpid()))
        }
        expectNoDifference(refusal?.errors.map(\.reason), [.activating])
    }

    @Test(arguments: built)
    func buildsEveryEventThatLeavesTheFrontmostStateAlone(command: String, arguments: [String: AppleEventValue], outcome: [String]) throws {
        let resolved = try dictionary.command(.name(command), taking: Set(arguments.keys))
        let event = try dictionary.event(resolved, arguments: arguments, target: NSAppleEventDescriptor(processIdentifier: getpid()))
        let observed: [String] =
            [(UInt64(event.eventClass) << 32 | UInt64(event.eventID)).sdef]
            + [event.paramDescriptor(forKeyword: AEKeyword(keyDirectObject))?.forKeyword(AEKeyword(keyAEKeyData))?.typeCodeValue.sdef].compactMap(\.self)
        expectNoDifference(observed, outcome)
    }

    @Test(.timeLimit(.minutes(1)))
    func endsTheWaitForAReplyFromAHungTargetAtTheDeadlineTheCallerPasses() async throws {
        let (reason, waited) = try await FixtureSession.launched { fixture in
            try await fixture.send(.hang(kind: .sleeping(.seconds(20))))
            _ = try await fixture.next(\.hanging)
            let started = ContinuousClock.now
            let sent = await Result { () async throws(AppleEventFailure) in
                let event = NSAppleEventDescriptor(
                    eventClass: AEEventClass(kAECoreSuite),
                    eventID: AEEventID(kAEGetData),
                    targetDescriptor: NSAppleEventDescriptor(processIdentifier: fixture.process),
                    returnID: AEReturnID(kAutoGenerateReturnID),
                    transactionID: AETransactionID(kAnyTransactionID),
                )
                try await NSAppleEventDescriptor.send(event, within: .seconds(2)) { _ in }
            }
            let reason: AppleEventFailure.Reason? =
                switch sent {
                    case .success: nil
                    case .failure(let failure): failure.reason
                }
            return (reason, ContinuousClock.now - started)
        }
        expectNoDifference(reason, .unanswered(OSStatus(errAETimeout)))
        #expect(waited < Duration.seconds(8), "reply wait \(waited) passed the 2 s deadline of a target hung for 20 s")
    }
}
