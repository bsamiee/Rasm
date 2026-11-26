import AVFoundation
import AsyncAlgorithms
import CoreImage
import CoreMedia
public import Foundation
import Processes
@unsafe @preconcurrency import ScreenCaptureKit
import Synchronization
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct Recording: Encodable, Sendable {
    public struct KeyFrame: Encodable, Sendable {
        public let at: Date
        public let changed: CGRect?
        public let screenshot: Screenshot
    }

    public let keyFrames: [KeyFrame]
    public let presentation: Watch.Presentation?

    static func keyFrames(
        of samples: some AsyncSequence<KeyFrame, Never> & Sendable,
        from start: KeyFrame,
        keeping limit: Int,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async -> [KeyFrame] {
        var kept: [KeyFrame] = [start]
        let settled = samples.removeDuplicates { earlier, later in later.screenshot.changed(from: earlier.screenshot) == nil }.debounce(for: quiet, clock: clock)
        for await sample in settled {
            guard let changed = kept.last.flatMap({ last in sample.screenshot.changed(from: last.screenshot) }) else { continue }
            kept.append(KeyFrame(at: sample.at, changed: changed, screenshot: sample.screenshot))
            guard kept.count > limit else { continue }
            let areas: [CGFloat] = kept.map { frame in frame.changed.map { rect in rect.width * rect.height } ?? 0 }
            guard let dropped = kept.indices.dropFirst().dropLast().min(by: { first, second in areas[first] < areas[second] }) else { continue }
            let next: KeyFrame = kept[dropped + 1]
            kept[dropped + 1] = KeyFrame(at: next.at, changed: next.screenshot.changed(from: kept[dropped - 1].screenshot), screenshot: next.screenshot)
            kept.remove(at: dropped)
        }
        return kept
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private final class Output: NSObject, SCStreamOutput, SCStreamDelegate, SCRecordingOutputDelegate, Sendable {
    let samples = AsyncStream<Recording.KeyFrame>.makeStream()
    let first = AsyncStream<Result<Recording.KeyFrame, ApplicationFailure>>.makeStream()
    let outcomes = AsyncStream<Result<Void, ApplicationFailure>>.makeStream()
    private let started = Atomic<Bool>(false)
    private let framing: (frame: CGRect, windows: [CGWindowID], document: URL?)
    private let writing: Bool
    private let subject: String

    init(framing: (frame: CGRect, windows: [CGWindowID], document: URL?), writing: Bool, subject: String) { (self.framing, self.writing, self.subject) = (framing, writing, subject) }

    func stream(_: SCStream, didOutputSampleBuffer sampleBuffer: CMSampleBuffer, of _: SCStreamOutputType) {
        guard let image = sampleBuffer.imageBuffer.map({ pixels in CIImage(cvPixelBuffer: pixels) }) else { return }
        guard let copied = rendering.createCGImage(image, from: image.extent) else {
            outcomes.continuation.yield(.failure(ApplicationFailure(subject: subject, reason: .noImage)))
            return
        }
        let screenshot = Screenshot(image: copied, frame: framing.frame, windows: framing.windows, backingStore: false, document: framing.document, presentation: nil)
        let sample = Recording.KeyFrame(at: .now, changed: nil, screenshot: screenshot)
        if !started.exchange(true, ordering: .relaxed) { first.continuation.yield(.success(sample)) }
        samples.continuation.yield(sample)
    }

    func stream(_: SCStream, didStopWithError error: any Error) { end(error) }

    func recordingOutputDidFinishRecording(_: SCRecordingOutput) { outcomes.continuation.yield(.success(())) }

    func recordingOutput(_: SCRecordingOutput, didFailWithError error: any Error) { end(error) }

    func start(_ stream: SCStream) {
        stream.startCapture { (error: (any Error)?) in
            guard let error else { return }
            self.first.continuation.yield(.failure(ApplicationFailure(subject: self.subject, reason: .foundation(error as NSError))))
        }
    }

    func stop(_ stream: SCStream) {
        stream.stopCapture { (error: (any Error)?) in
            switch (error, self.writing) {
                case (.some(let error), _): self.end(error)
                case (.none, false): self.outcomes.continuation.yield(.success(()))
                case (.none, true): break
            }
        }
    }

    private func end(_ error: any Error) { outcomes.continuation.yield(.failure(ApplicationFailure(subject: subject, reason: .foundation(error as NSError)))) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Watch {
    public nonisolated func record<Value>(
        _ source: Source,
        keyFrames limit: Int,
        longestSide: LongestSide? = nil,
        movie: URL? = nil,
        while body: () async throws(ApplicationFailure) -> Value,
    ) async -> (value: Result<Value, ApplicationFailure>, recording: Result<Recording, ApplicationFailure>) {
        let process: pid_t = instance.process.pid
        let subject = "recording of \(source) in process \(process)"
        let (expired, cancelled) = (ApplicationFailure(subject: subject, reason: .unanswered(nil)), ApplicationFailure(subject: subject, reason: .cancelled))
        let deadline = AsyncTimerSequence(interval: messagingTimeout * 2, clock: ContinuousClock())
        var ran: Result<Value, ApplicationFailure>?
        let recording: Result<Recording, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Recording in
            try Grant.screenCapture.check()
            let documents: [CGWindowID: URL] = await self.documents()
            let (keyFrames, presentation) = try await presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> [Recording.KeyFrame] in
                let framed: [Framing] = try await framings(source, showing: menus, longestSide: longestSide).map { (framing: Result<Framing, ApplicationFailure>) throws(ApplicationFailure) in
                    try framing.get()
                }
                let area: CGRect = framed.map(\.frame).reduce(CGRect.null) { area, next in area.union(next) }
                let joint: Framing? = try await framed.count > 1 ? framings(.region(frame: area), showing: [], longestSide: longestSide).first?.get() : framed.first
                guard let joint, let display = joint.display, let application = joint.windows.first?.owningApplication else { throw ApplicationFailure(subject: subject, reason: .noWindow) }
                let configuration = SCStreamConfiguration()
                (configuration.showsCursor, configuration.showMouseClicks, configuration.pixelFormat) = (false, true, kCVPixelFormatType_32BGRA)
                (configuration.shouldBeOpaque, configuration.ignoreShadowsDisplay, configuration.width, configuration.height) = (true, true, joint.width, joint.height)
                configuration.minimumFrameInterval = CMTime(seconds: quiet / 2 / .seconds(1), preferredTimescale: CMTimeScale(NSEC_PER_SEC))
                configuration.sourceRect = joint.frame.offsetBy(dx: -display.frame.minX, dy: -display.frame.minY).intersection(CGRect(origin: .zero, size: display.frame.size))
                let numbers: [CGWindowID] = joint.windows.map(\.windowID)
                let output = Output(framing: (joint.frame, numbers, numbers.lazy.compactMap { number in documents[number] }.first), writing: movie != nil, subject: subject)
                let stream = SCStream(filter: SCContentFilter(display: display, including: [application], exceptingWindows: []), configuration: configuration, delegate: output)
                let movieOutput = SCRecordingOutputConfiguration()
                let type: AVFileType? = movie.flatMap { movie in UTType(filenameExtension: movie.pathExtension) }.map { type in AVFileType(type.identifier) }
                guard movie == nil || type.map({ type in movieOutput.availableOutputFileTypes.contains(type) }) == true else {
                    throw ApplicationFailure(subject: movie?.path(percentEncoded: false) ?? subject, reason: .noEncoder)
                }
                if let movie, let type { (movieOutput.outputURL, movieOutput.outputFileType) = (movie, type) }
                let recorder: SCRecordingOutput? = movie.map { _ in SCRecordingOutput(configuration: movieOutput, delegate: output) }
                do {
                    try stream.addStreamOutput(output, type: .screen, sampleHandlerQueue: nil)
                    _ = try recorder.map(stream.addRecordingOutput)
                } catch {
                    throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
                }
                let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: "Recording process \(process)")
                defer { ProcessInfo.processInfo.endActivity(activity) }
                output.start(stream)
                let captured: Result<[Recording.KeyFrame], ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> [Recording.KeyFrame] in
                    let began = merge(output.first.stream, deadline.map { _ in Result<Recording.KeyFrame, ApplicationFailure>.failure(expired) })
                    let start: Recording.KeyFrame = try await (began.first { _ in true } ?? .failure(cancelled)).get()
                    async let keyFrames: [Recording.KeyFrame] = Recording.keyFrames(of: output.samples.stream, from: start, keeping: limit)
                    ran = await Result(catching: body)
                    output.samples.continuation.finish()
                    return await keyFrames
                }
                output.stop(stream)
                let outcomes = merge(output.outcomes.stream, deadline.map { _ in Result<Void, ApplicationFailure>.failure(expired) })
                let ended: Result<Void, ApplicationFailure> = await outcomes.first { _ in true } ?? .failure(cancelled)
                let keyFrames: [Recording.KeyFrame] = try captured.get()
                try ended.get()
                return keyFrames
            }
            return Recording(keyFrames: keyFrames, presentation: presentation)
        }
        let value: Result<Value, ApplicationFailure> = if let ran { ran } else { await Result(catching: body) }
        return (value, recording)
    }
}
