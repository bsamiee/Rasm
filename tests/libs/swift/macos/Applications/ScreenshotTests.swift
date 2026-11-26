import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
import CustomDump
import Fixture
import ImageIO
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    struct ScreenshotTests {
        static let frame = CGRect(x: 731, y: 212, width: 260, height: 140)
        static let marks: [(mark: CGRect?, changed: CGRect?)] = [
            (nil, nil),
            (CGRect(x: 40, y: 30, width: 25, height: 10), CGRect(x: 771, y: 242, width: 25, height: 10)),
            (CGRect(x: 259.5, y: 0, width: 0.5, height: 0.5), CGRect(x: 990.5, y: 212, width: 0.5, height: 0.5)),
            (CGRect(x: 0, y: 120, width: 260, height: 20), CGRect(x: 731, y: 332, width: 260, height: 20)),
        ]
        static let placed: [(mark: CGRect, changed: CGRect?)] = [
            (CGRect(x: 771, y: 242, width: 60, height: 20), CGRect(x: 771, y: 229, width: 60, height: 33)),
            (CGRect(x: 960, y: 330, width: 60, height: 40), CGRect(x: 923.5, y: 330, width: 67.5, height: 22)),
            (CGRect(x: 700, y: 180, width: 60, height: 40), CGRect(x: 731, y: 212, width: 36.5, height: 13)),
            (CGRect(x: 0, y: 0, width: 40, height: 40), nil),
        ]

        static func cropped(_ image: CGImage, to frame: CGRect) throws -> Screenshot {
            let pixels = CGRect(x: (frame.minX - Self.frame.minX) * 2, y: (frame.minY - Self.frame.minY) * 2, width: frame.width * 2, height: frame.height * 2)
            return Screenshot(image: try #require(image.cropping(to: pixels)), frame: frame, windows: [], backingStore: false, document: nil, presentation: nil)
        }

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

        @Test(arguments: placed)
        func marksDrawTheirFrameWithTheLabelBesideItInsideTheImageAndNowhereElse(mark: CGRect, changed: CGRect?) throws {
            let before = Screenshot(image: try Self.drawn(nil), frame: Self.frame, windows: [], backingStore: false, document: nil, presentation: nil)
            let marked: CGImage = try before.image.marking([Mark(frame: mark, label: "@a12")], in: Self.frame)
            let found: CGRect? = Screenshot(image: marked, frame: Self.frame, windows: [], backingStore: false, document: nil, presentation: nil).changed(from: before)
            let within: Bool =
                switch (found, changed) {
                    case (.none, .none): true
                    case (.some(let found), .some(let changed)): changed.insetBy(dx: -0.5, dy: -0.5).contains(found) && found.insetBy(dx: -0.5, dy: -0.5).contains(changed)
                    case (.some, .none), (.none, .some): false
                }
            #expect(within, "mark \(String(describing: mark)) changed \(String(describing: found)), want \(String(describing: changed)) within half a point")
        }

        @Test
        func changedFindsNothingBetweenAWindowImageAndItselfWhereItsRoundedCornersArePartlyTransparent() throws {
            let context: CGContext = try #require(
                unsafe CGContext(
                    data: nil,
                    width: Int(Self.frame.width * 2),
                    height: Int(Self.frame.height * 2),
                    bitsPerComponent: 8,
                    bytesPerRow: 0,
                    space: CGColorSpaceCreateDeviceRGB(),
                    bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedFirst.rawValue),
                )
            )
            context.setFillColor(CGColor(red: 0.2, green: 0.4, blue: 0.9, alpha: 1))
            context.addPath(unsafe CGPath(roundedRect: CGRect(x: 0, y: 0, width: context.width, height: context.height), cornerWidth: 21, cornerHeight: 21, transform: nil))
            context.fillPath()
            let window = Screenshot(image: try #require(context.makeImage()), frame: Self.frame, windows: [], backingStore: false, document: nil, presentation: nil)
            expectNoDifference(window.changed(from: window), nil)
        }

        @Test
        func labelsCoverNoOtherMarkedControlWhereTheImageHasRoomBesideTheirFrame() throws {
            let (pressed, above, beside): (Mark, Mark, Mark) = (
                Mark(frame: CGRect(x: 771, y: 242, width: 20, height: 20), label: "@a12"),
                Mark(frame: CGRect(x: 771, y: 222, width: 20, height: 20), label: "@a13"),
                Mark(frame: CGRect(x: 791, y: 242, width: 20, height: 20), label: "@a14"),
            )
            let every: CGImage = try Self.drawn(nil).marking([pressed, above, beside], in: Self.frame)
            let others: CGImage = try Self.drawn(nil).marking([above, beside], in: Self.frame)
            for interior in [above.frame.insetBy(dx: 1, dy: 1), beside.frame.insetBy(dx: 1, dy: 1)] {
                expectNoDifference(try Self.cropped(every, to: interior).changed(from: try Self.cropped(others, to: interior)), nil, "label \(pressed.label) covers \(interior)")
            }
        }

        @Test
        func pngCarriesSRGBColourWhateverSpaceTheImageWasDrawnIn() throws {
            let space: CGColorSpace = try #require(CGColorSpace(name: CGColorSpace.displayP3))
            let context: CGContext = try #require(
                unsafe CGContext(data: nil, width: 40, height: 20, bitsPerComponent: 8, bytesPerRow: 0, space: space, bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedLast.rawValue))
            )
            context.setFillColor(CGColor(red: 0.2, green: 0.4, blue: 0.9, alpha: 1))
            context.fill(CGRect(x: 0, y: 0, width: 40, height: 20))
            let encoded: Data = try #require(context.makeImage()).png()
            let decoded: CGImage? = CGImageSourceCreateWithData(encoded as CFData, nil).flatMap { source in CGImageSourceCreateImageAtIndex(source, 0, nil) }
            expectNoDifference(decoded?.colorSpace?.name, CGColorSpace.sRGB)
        }

        @Test(
            .enabled(if: CGPreflightScreenCaptureAccess() && AXIsProcessTrusted(), "Capturing the fixture from outside needs this test process to hold the Screen Recording and Accessibility grants")
        )
        func capturesARegionFromTheWindowsShownThereAndNoneOffScreen() async throws {
            let (rows, shots): ([InstanceState.Window], [Result<Screenshot, ApplicationFailure>]) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                let rows: [InstanceState.Window] = InstanceState.Window.all(.optionAll)[fixture.process, default: []]
                return (rows, try await watch.capture(.region(frame: rows.map(\.frame).reduce(CGRect.null) { joint, frame in joint.union(frame) })))
            }
            let hidden: Set<CGWindowID> = Set(rows.filter { row in !row.onScreen }.map(\.number))
            try #require(!hidden.isEmpty, "The fixture holds no window off screen inside the region")
            let shot: Screenshot = try #require(shots.first).get()
            expectNoDifference(hidden.intersection(shot.windows), [])
        }

        @Test(
            .enabled(if: CGPreflightScreenCaptureAccess() && AXIsProcessTrusted(), "Capturing the fixture from outside needs this test process to hold the Screen Recording and Accessibility grants")
        )
        func capturesEachWindowAtItsReportedFrameTimesItsScreenScaleWithNoShadow() async throws {
            let (windows, shots): ([Report.Window], [Result<Screenshot, ApplicationFailure>]) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let windows: [Report.Window] = try await fixture.next(\.report).value.windows.filter(\.visible)
                try #require(!windows.isEmpty, "The fixture reports no visible window")
                return (windows, try await watch.capture(.windows(numbers: windows.map { window in CGWindowID(window.number) })))
            }
            for (window, shot) in zip(windows, shots) {
                let image: CGImage = try shot.get().image
                let screen: NSScreen? = NSScreen.screens.first { (screen: NSScreen) -> Bool in screen.frame.contains(CGPoint(x: window.frame.midX, y: window.frame.midY)) }
                let scale: CGFloat = try #require(screen?.backingScaleFactor)
                let wanted = CGSize(width: (window.frame.width * scale).rounded(), height: (window.frame.height * scale).rounded())
                expectNoDifference(CGSize(width: image.width, height: image.height), wanted, "window \(window.number) frame \(String(describing: window.frame)) at scale \(scale)")
            }
        }

        @Test(
            .enabled(if: CGPreflightScreenCaptureAccess() && AXIsProcessTrusted(), "Capturing the fixture from outside needs this test process to hold the Screen Recording and Accessibility grants")
        )
        func capturesAWindowWithItsChildWindowsAcrossTheirJointFrame() async throws {
            let (parent, child, shots): (Report.Window, Report.Window, [Result<Screenshot, ApplicationFailure>]) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.show(surface: .popover))
                try await fixture.send(.report)
                let windows: [Report.Window] = try await fixture.next(\.report).value.windows.filter(\.visible)
                let parent: Report.Window = try #require(windows.first { window in window.surface == .controls }, "The fixture reports no visible controls window")
                let child: Report.Window = try #require(windows.first { window in window.surface == .popover }, "The fixture reports no visible popover")
                return (parent, child, try await watch.capture(.windows(numbers: [CGWindowID(parent.number)])))
            }
            let shot: Screenshot = try #require(shots.first).get()
            let joint: CGRect = parent.frame.union(child.frame)
            expectNoDifference(Set(shot.windows), [CGWindowID(parent.number), CGWindowID(child.number)])
            expectNoDifference(CGSize(width: shot.frame.width.rounded(), height: shot.frame.height.rounded()), CGSize(width: joint.width.rounded(), height: joint.height.rounded()))
        }
    }
}
