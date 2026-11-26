@unsafe @preconcurrency import ApplicationServices
import CustomDump
import Encoding
import Foundation
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct AccessibilityTests {
    @Test
    func readsEachCoreFoundationValueAsTheFactOfItsTypeDroppingAbsentSlots() throws {
        var frame = CGRect(x: 10, y: 20, width: 300, height: 200)
        var (refused, absent) = (AXError.cannotComplete, AXError.noValue)
        let boxed: [AXValue] = try [#require(unsafe AXValueCreate(.cgRect, &frame)), #require(unsafe AXValueCreate(.axError, &refused)), #require(unsafe AXValueCreate(.axError, &absent))]
        let listed: [AnyObject] = [AXUIElementCreateApplication(getpid()), NSNull()]
        let values: [CFTypeRef] = [kCFBooleanTrue, 1 as NSNumber, 0.5 as NSNumber, "Save" as CFString, NSAttributedString(string: "Bold"), listed as CFArray, NSNull()]
        expectNoDifference(
            (values + boxed).map { value in Fact(value, interning: { _ in 7 }) },
            [
                .bool(value: true), .integer(value: 1), .real(value: 0.5), .text(value: "Save"), .text(value: "Bold"), .list(values: [.element(id: 7)]), nil, .rect(value: frame),
                .refused(error: .known(.cannotComplete)), nil,
            ],
        )
    }
}
