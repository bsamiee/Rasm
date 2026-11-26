import CustomDump
import Foundation
import Testing

@testable import Processes

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ProcessFailureTests {
    @Test
    func classifiesADecodeFailureAsCorruptContent() throws {
        let error = try #require(throws: DecodingError.self) { try JSONDecoder().decode([String: Int].self, from: Data("{".utf8)) }
        let cause: NSError? = if case .corrupt(let cause) = ProcessFailure(subject: "{", error).reason { cause } else { nil }
        expectNoDifference(cause?.code, CocoaError.Code.coderReadCorrupt.rawValue)
    }

    @Test
    func bridgesAMachStatusAsItsUnderlyingError() {
        let bridged = ProcessFailure(subject: "host_statistics64", reason: .mach(KERN_INVALID_ARGUMENT)) as NSError
        let cause = bridged.underlyingErrors.first as NSError?
        expectNoDifference(cause.map { cause in [cause.domain, String(cause.code)] }, [NSMachErrorDomain, String(KERN_INVALID_ARGUMENT)])
    }
}
