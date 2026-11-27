import CustomDump
import Foundation
import Processes
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ProcessFailureTests {
    @Test
    func bridgesAMachStatusAsItsUnderlyingError() {
        let bridged = ProcessFailure(subject: "host_statistics64", reason: .mach(KERN_INVALID_ARGUMENT)) as NSError
        let cause = bridged.underlyingErrors.first as NSError?
        expectNoDifference(cause.map { cause in [cause.domain, String(cause.code)] }, [NSMachErrorDomain, String(KERN_INVALID_ARGUMENT)])
    }
}
