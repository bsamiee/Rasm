import AVFoundation
import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import CoreMedia
import Fixture
import Testing
import UniformTypeIdentifiers

@testable import ScreenCapture

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct RecordingTests {
    @Test(.enabled(if: CGPreflightScreenCaptureAccess() && AXIsProcessTrusted(), "Recording the fixture from outside needs this test process to hold the Screen Recording and Accessibility grants"))
    func recordsAnMPEG4MovieOfTheWindowAtItsFrameTimesItsScreenScaleNoLongerThanTheSpan() async throws {
        let span: Duration = .seconds(1)
        let (window, recording): (Report.Window, Recording) = try await FixtureSession.launched { fixture in
            let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
            let watch: Watch = await Watches(recognize: { watched, window throws(ApplicationFailure) in try await watched.recognizeText(in: window) }).watch(try Instance(application))
            try await fixture.send(.report)
            let window: Report.Window = try #require(try await fixture.next(\.report).value.windows.first(where: \.visible), "The fixture reports no visible window")
            return (window, try await watch.instance.recording(.windows(numbers: [CGWindowID(window.number)]), in: watch, for: span))
        }
        let folder: URL = try FileManager.default.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: FileManager.default.temporaryDirectory, create: true)
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        let movie: URL = folder.appending(component: "recording").appendingPathExtension(for: try #require(UTType(recording.type)))
        try recording.movie.write(to: movie)
        let asset = AVURLAsset(url: movie)
        let track: AVAssetTrack = try #require(try await asset.loadTracks(withMediaType: .video).first)
        let (size, duration): (CGSize, CMTime) = (try await track.load(.naturalSize), try await asset.load(.duration))
        let scale: CGFloat = try #require(NSScreen.screens.first { screen in screen.frame.contains(CGPoint(x: window.frame.midX, y: window.frame.midY)) }?.backingScaleFactor)
        #expect(recording.type == UTType.mpeg4Movie.identifier)
        #expect(size == CGSize(width: (window.frame.width * scale).rounded(), height: (window.frame.height * scale).rounded()), "window \(window.number) at scale \(scale): got \(size)")
        #expect(duration.seconds > 0 && duration.seconds <= span / .seconds(1) + 0.5, "recorded \(duration.seconds) s for a \(span) span")
    }
}
