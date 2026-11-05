import ApplicationServices
import CustomDump
import Foundation
import Results
import System
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let subject: String = "perform on AQID"
private let phrasings: [(reason: ApplicationFailure.Reason, phrases: [String])] = [
    (.refused(.actionUnsupported), [subject, String(AXError.actionUnsupported.rawValue)]),
    (.unresolved, [subject, "no longer exists", "read the app again"]),
    (.foreign(4242), [subject, "other than 4242"]),
    (.unanswered(.perform(element: Element.ID(token: Data([1, 2, 3])), action: kAXPressAction)), [subject, "can have run"]),
    (.untrusted, ["Accessibility"]),
    (.noCaptureAccess, [subject, "Screen Recording"]),
]
private let causes: [(reason: ApplicationFailure.Reason, cause: any Error)] = [
    (.system(.permissionDenied), Errno.permissionDenied),
    (.noProcess, Errno.noSuchProcess),
    (.unmatched(Unmatched(subject: "Probe", query: nil, candidates: ["a", "b"])), Unmatched(subject: "Probe", query: nil, candidates: ["a", "b"])),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct ApplicationFailures {
    @Test(arguments: phrasings)
    func failureTextNamesItsSubjectAndRule(reason: ApplicationFailure.Reason, phrases: [String]) throws {
        let text: String = try #require(ApplicationFailure(subject: subject, reason: reason).errorDescription)
        expectNoDifference(phrases.filter { phrase in !text.contains(phrase) }, [], "\(text)")
    }

    @Test
    func unansweredReadClaimsNoActionRan() throws {
        let read: String = try #require(ApplicationFailure(subject: "AXRole of 4242", reason: .unanswered(nil)).errorDescription)
        #expect(!read.contains("can have run"), "\(read) claims a read ran")
    }

    @Test(arguments: causes)
    func hostCauseSitsUnderUnderlyingErrorKey(reason: ApplicationFailure.Reason, cause: any Error) {
        let underlying: String? = (ApplicationFailure(subject: "4242", reason: reason).errorUserInfo[NSUnderlyingErrorKey] as? any Error).map { error in String(reflecting: error) }
        expectNoDifference(underlying, String(reflecting: cause))
    }
}
