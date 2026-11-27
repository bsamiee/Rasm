@unsafe @preconcurrency import ApplicationServices
import CustomDump
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct GrantTests {
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
