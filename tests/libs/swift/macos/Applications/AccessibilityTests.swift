@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
import CustomDump
import Encoding
import Fixture
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

extension WindowServerTests {
    @Suite(.enabled(if: AXIsProcessTrusted(), "Reading the fixture from outside needs this test process to hold the Accessibility grant"))
    struct ElementReadTests {
        @Test
        func answersTheElementReadOfAHungInstanceWithinOneMessagingTimeout() async throws {
            let (reason, waited): (ApplicationFailure.Reason?, Duration) = try await FixtureSession.launched { fixture in
                AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
                let window: AXUIElement = try #require(try AXUIElementCreateApplication(fixture.process).elements(kAXWindowsAttribute).first)
                try await fixture.send(.hang(kind: .sleeping(.seconds(5))))
                _ = try await fixture.next(\.hanging)
                let started = ContinuousClock.now
                let reason: ApplicationFailure.Reason?
                do throws(ApplicationFailure) {
                    _ = try window.element(reading: windowAttributes)
                    reason = nil
                } catch {
                    reason = error.reason
                }
                return (reason, ContinuousClock.now - started)
            }
            expectNoDifference(reason, .unanswered(nil))
            #expect(waited < messagingTimeout * 1.5, "an element read of a hung window waited \(waited) under a \(messagingTimeout) messaging timeout")
        }
    }
}
