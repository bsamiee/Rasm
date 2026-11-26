@unsafe @preconcurrency import ApplicationServices
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct GrantTests {
    @Test(arguments: [(Grant.accessibility, AXIsProcessTrusted()), (.screenCapture, CGPreflightScreenCaptureAccess()), (.postEvent, CGPreflightPostEventAccess())])
    func refusesEachGrantTheSystemPreflightFindsMissingAndNamesIt(grant: Grant, held: Bool) {
        var refused: ApplicationFailure.Reason?
        do throws(ApplicationFailure) { try grant.check() } catch { refused = error.reason }
        let named: Grant? =
            switch refused {
                case .noGrant(let named), .notResponsible(let named, _): named
                default: nil
            }
        let expected: Grant? = held ? nil : grant
        let namesTheMissingGrant: Bool = named == expected
        let refusesOnlyWhenMissing: Bool = (refused == nil) == held
        #expect(namesTheMissingGrant)
        #expect(refusesOnlyWhenMissing)
    }

    @Test
    func readsADisabledAccessibilityAnswerAsTheMissingGrantOnlyWhileTheSystemFindsItMissing() {
        let otherwise = ApplicationFailure(subject: "probe", reason: .refused)
        let read = ApplicationFailure(.apiDisabled, subject: "probe", process: nil) { otherwise }
        let deferredOnlyWhileTrusted: Bool = (read == otherwise) == AXIsProcessTrusted()
        #expect(deferredOnlyWhileTrusted)
    }
}
