@unsafe @preconcurrency import ApplicationServices
import CustomDump
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
        expectNoDifference(named, held ? nil : grant)
        expectNoDifference(held ? refused : nil, nil)
    }

    @Test
    func readsADisabledAccessibilityAnswerAsTheMissingGrantOnlyWhileTheSystemFindsItMissing() {
        let otherwise = ApplicationFailure(subject: "probe", reason: .refused)
        let read = ApplicationFailure(.apiDisabled, subject: "probe", process: nil) { otherwise }
        switch (AXIsProcessTrusted(), read.reason) {
            case (true, _): expectNoDifference(read, otherwise)
            case (false, .noGrant(let grant)), (false, .notResponsible(let grant, _)): expectNoDifference(grant, .accessibility)
            case (false, let reason): Issue.record("disabled Accessibility answer read as \(reason) while the system finds the grant missing")
        }
    }
}
