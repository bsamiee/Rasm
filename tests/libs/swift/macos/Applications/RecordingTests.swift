import AVFoundation
import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
import CoreMedia
import CustomDump
import Fixture
import Testing
import UniformTypeIdentifiers

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    struct RecordingTests {
        static let frame = CGRect(x: 300, y: 200, width: 200, height: 120)

        static func sample(_ marks: [CGRect]) throws -> Recording.KeyFrame {
            let context: CGContext = try #require(
                unsafe CGContext(
                    data: nil,
                    width: Int(frame.width),
                    height: Int(frame.height),
                    bitsPerComponent: 8,
                    bytesPerRow: 0,
                    space: CGColorSpaceCreateDeviceRGB(),
                    bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedLast.rawValue),
                )
            )
            context.translateBy(x: 0, y: frame.height)
            context.scaleBy(x: 1, y: -1)
            context.setFillColor(CGColor(red: 0.2, green: 0.4, blue: 0.9, alpha: 1))
            context.fill(CGRect(origin: .zero, size: frame.size))
            context.setFillColor(CGColor(red: 0.9, green: 0.4, blue: 0.2, alpha: 1))
            context.fill(marks)
            let screenshot = Screenshot(image: try #require(context.makeImage()), frame: frame, windows: [], backingStore: false, document: nil, presentation: nil)
            return Recording.KeyFrame(at: .now, changed: nil, screenshot: screenshot)
        }

        static func settle(_ samples: [(sample: Recording.KeyFrame, after: Duration)], keeping limit: Int) async throws -> [Recording.KeyFrame] {
            let (stream, continuation) = AsyncStream<Recording.KeyFrame>.makeStream()
            let feeding = Task {
                for (sample, after) in samples {
                    try await Task.sleep(for: after)
                    continuation.yield(sample)
                }
                continuation.finish()
            }
            let kept: [Recording.KeyFrame] = await Recording.keyFrames(of: stream, from: try #require(samples.first?.sample), keeping: limit)
            try await feeding.value
            return kept
        }

        @Test
        func keepsTheStartAndTheLastStateOfEachBurstOnceItSettledForAQuietInterval() async throws {
            let (start, small) = (try Self.sample([]), try Self.sample([CGRect(x: 10, y: 10, width: 20, height: 10)]))
            let (large, apart) = (try Self.sample([CGRect(x: 10, y: 10, width: 60, height: 10)]), try Self.sample([CGRect(x: 120, y: 70, width: 30, height: 30)]))
            let kept = try await Self.settle([(start, .zero), (small, .milliseconds(10)), (large, .milliseconds(10)), (apart, .milliseconds(300))], keeping: 6)
            let images: [CGImage] = kept.map(\.screenshot.image)
            let wanted: [CGImage] = [start, large, apart].map(\.screenshot.image)
            let starts: Bool = kept.first?.changed == nil
            let marked: Bool = kept.dropFirst().allSatisfy { frame in frame.changed != nil }
            let same: Bool = images.elementsEqual(wanted) { (first: CGImage, second: CGImage) -> Bool in CFEqual(first, second) }
            #expect(same, "kept \(kept.count) frames, want the start, the settled end of the burst, and the later state")
            #expect(starts && marked, "changed \(kept.map(\.changed))")
        }

        @Test
        func mergesTheSmallestChangeIntoTheFrameAfterItOncePastTheLimit() async throws {
            let marks = [CGRect(x: 10, y: 10, width: 4, height: 4), CGRect(x: 60, y: 30, width: 80, height: 60), CGRect(x: 150, y: 90, width: 40, height: 20)]
            let (start, small, large, last) = (try Self.sample([]), try Self.sample(Array(marks.prefix(1))), try Self.sample(Array(marks.prefix(2))), try Self.sample(marks))
            let kept = try await Self.settle([(start, .zero), (small, .milliseconds(200)), (large, .milliseconds(200)), (last, .milliseconds(200))], keeping: 3)
            let images: [CGImage] = kept.map(\.screenshot.image)
            let wanted: [CGImage] = [start, large, last].map(\.screenshot.image)
            let same: Bool = images.elementsEqual(wanted) { (first: CGImage, second: CGImage) -> Bool in CFEqual(first, second) }
            #expect(same, "kept \(kept.count) frames, want the small change dropped")
            let second: CGRect? = kept.dropFirst().first?.changed
            let merged: CGRect = try #require(second)
            let screen: [CGRect] = marks.prefix(2).map { mark in mark.offsetBy(dx: Self.frame.minX, dy: Self.frame.minY) }
            #expect(screen.allSatisfy { mark in merged.insetBy(dx: -1, dy: -1).contains(mark) }, "merged frame changed \(merged), want it to hold \(screen)")
        }

        @Test(
            .enabled(if: CGPreflightScreenCaptureAccess() && AXIsProcessTrusted(), "Recording the fixture from outside needs this test process to hold the Screen Recording and Accessibility grants")
        )
        func recordsTheStartAndTheRetitledStateAtTheWindowFrameTimesItsScreenScaleWithItsMovie() async throws {
            let folder: URL = try FileManager.default.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: FileManager.default.temporaryDirectory, create: true)
            defer {
                do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
            }
            let movie: URL = folder.appending(component: "recording").appendingPathExtension(for: .mpeg4Movie)
            let span: Duration = .seconds(1)
            let (window, recorded): (Report.Window, Result<Recording, ApplicationFailure>) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let window: Report.Window = try #require(try await fixture.next(\.report).value.windows.first(where: \.visible), "The fixture reports no visible window")
                let outcome = await watch.record(.windows(numbers: [CGWindowID(window.number)]), keyFrames: 6, movie: movie) { () async throws(ApplicationFailure) in
                    do {
                        try await Task.sleep(for: .milliseconds(300))
                        try await fixture.send(.retitle(entry: .action, member: 0, to: "Recorded"))
                        try await Task.sleep(for: span)
                    } catch {
                        throw ApplicationFailure(subject: "fixture retitle", reason: .foundation(error as NSError))
                    }
                }
                try outcome.value.get()
                return (window, outcome.recording)
            }
            let recording: Recording = try recorded.get()
            let screen: NSScreen? = NSScreen.screens.first { (screen: NSScreen) -> Bool in screen.frame.contains(CGPoint(x: window.frame.midX, y: window.frame.midY)) }
            let scale: CGFloat = try #require(screen?.backingScaleFactor)
            let size = CGSize(width: (window.frame.width * scale).rounded(), height: (window.frame.height * scale).rounded())
            let first: Screenshot = try #require(recording.keyFrames.first?.screenshot)
            expectNoDifference(CGSize(width: first.image.width, height: first.image.height), size, "window \(window.number) at scale \(scale)")
            #expect(recording.keyFrames.count >= 2 && recording.keyFrames.dropFirst().allSatisfy { frame in frame.changed != nil }, "key frames changed \(recording.keyFrames.map(\.changed))")
            let asset = AVURLAsset(url: movie)
            let track: AVAssetTrack = try #require(try await asset.loadTracks(withMediaType: .video).first)
            let (natural, duration): (CGSize, CMTime) = (try await track.load(.naturalSize), try await asset.load(.duration))
            expectNoDifference(natural, size)
            #expect(duration.seconds > 0 && duration.seconds <= (span + .milliseconds(300)) / .seconds(1) + 0.5, "recorded \(duration.seconds) s")
        }

        @Test(
            .enabled(if: CGPreflightScreenCaptureAccess() && AXIsProcessTrusted(), "Recording the fixture from outside needs this test process to hold the Screen Recording and Accessibility grants")
        )
        func endsACancelledRecordingAsCancelledLongBeforeItsSpan() async throws {
            let clock = ContinuousClock()
            let (ended, took): (Result<Void, ApplicationFailure>, Duration) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let window: Report.Window = try #require(try await fixture.next(\.report).value.windows.first(where: \.visible), "The fixture reports no visible window")
                let started = clock.now
                let recording = Task { () async -> Result<Void, ApplicationFailure> in
                    await watch.record(.windows(numbers: [CGWindowID(window.number)]), keyFrames: 6) { () async throws(ApplicationFailure) in
                        do { try await Task.sleep(for: .seconds(30)) } catch { throw ApplicationFailure(subject: "recording", reason: .cancelled) }
                    }
                    .value
                }
                try await clock.sleep(for: .seconds(1))
                recording.cancel()
                return (await recording.value, clock.now - started)
            }
            let reason: ApplicationFailure.Reason? =
                switch ended {
                    case .failure(let failure): failure.reason
                    case .success: nil
                }
            expectNoDifference(reason, .cancelled)
            let prompt: Bool = took < Duration.seconds(10)
            #expect(prompt, "the cancelled recording ended after \(took)")
        }
    }
}
