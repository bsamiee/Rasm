import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import CasePathsCore
import CustomDump
import Fixture
import Testing

@testable import ScreenCapture

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct ScreenshotTests {
    static let frame = CGRect(x: 731, y: 212, width: 260, height: 140)
    static let marks: [(mark: CGRect?, changed: CGRect?)] = [
        (nil, nil),
        (CGRect(x: 40, y: 30, width: 25, height: 10), CGRect(x: 771, y: 242, width: 25, height: 10)),
        (CGRect(x: 259.5, y: 0, width: 0.5, height: 0.5), CGRect(x: 990.5, y: 212, width: 0.5, height: 0.5)),
        (CGRect(x: 0, y: 120, width: 260, height: 20), CGRect(x: 731, y: 332, width: 260, height: 20)),
    ]

    static func drawn(_ mark: CGRect?) throws -> CGImage {
        let scale: CGFloat = 2
        let context: CGContext = try #require(
            unsafe CGContext(
                data: nil,
                width: Int(frame.width * scale),
                height: Int(frame.height * scale),
                bitsPerComponent: 8,
                bytesPerRow: 0,
                space: CGColorSpaceCreateDeviceRGB(),
                bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedLast.rawValue),
            )
        )
        context.translateBy(x: 0, y: CGFloat(context.height))
        context.scaleBy(x: scale, y: -scale)
        context.setFillColor(CGColor(red: 0.2, green: 0.4, blue: 0.9, alpha: 1))
        context.fill(CGRect(origin: .zero, size: frame.size))
        context.setFillColor(CGColor(red: 0.21, green: 0.4, blue: 0.9, alpha: 1))
        context.fill(mark ?? .null)
        return try #require(context.makeImage())
    }

    @Test(arguments: marks)
    func changedNamesTheScreenAreaWhosePixelsDifferAndNothingForEqualImages(mark: CGRect?, changed: CGRect?) throws {
        let before = Screenshot(image: try Self.drawn(nil), frame: Self.frame, windows: [], backingStore: false, document: nil, presentation: nil)
        let after = Screenshot(image: try Self.drawn(mark), frame: Self.frame, windows: [], backingStore: false, document: nil, presentation: nil)
        let found: CGRect? = after.changed(from: before)
        let within: Bool =
            switch (found, changed) {
                case (.none, .none): true
                case (.some(let found), .some(let changed)): changed.insetBy(dx: -0.5, dy: -0.5).contains(found) && found.insetBy(dx: -0.5, dy: -0.5).contains(changed)
                case (.some, .none), (.none, .some): false
            }
        #expect(within, "mark \(String(describing: mark)) changed \(String(describing: found)), want \(String(describing: changed)) within half a point")
    }

    @Test(.enabled(if: CGPreflightScreenCaptureAccess() && AXIsProcessTrusted(), "Capturing the fixture from outside needs this test process to hold the Screen Recording and Accessibility grants"))
    func capturesEachWindowAtItsReportedFrameTimesItsScreenScaleWithNoShadow() async throws {
        let (windows, shots): ([Report.Window], [Result<Screenshot, ApplicationFailure>]) = try await FixtureSession.launched { fixture in
            let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
            let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(try Instance(application))
            try await fixture.send(.report)
            let windows: [Report.Window] = try await fixture.next(\.report).value.windows.filter(\.visible)
            try #require(!windows.isEmpty, "The fixture reports no visible window")
            return (windows, try await watch.instance.capture(.windows(numbers: windows.map { window in CGWindowID(window.number) }), in: watch))
        }
        for (window, shot) in zip(windows, shots) {
            let image: CGImage = try shot.get().image
            let screen: NSScreen? = NSScreen.screens.first { (screen: NSScreen) -> Bool in screen.frame.contains(CGPoint(x: window.frame.midX, y: window.frame.midY)) }
            let scale: CGFloat = try #require(screen?.backingScaleFactor)
            let wanted = CGSize(width: (window.frame.width * scale).rounded(), height: (window.frame.height * scale).rounded())
            expectNoDifference(CGSize(width: image.width, height: image.height), wanted, "window \(window.number) frame \(String(describing: window.frame)) at scale \(scale)")
        }
    }
}
