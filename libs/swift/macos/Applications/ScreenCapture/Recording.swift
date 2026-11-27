import AVFoundation
import AsyncAlgorithms
import CasePaths
import CoreImage
import CoreMedia
import Encoding
package import Foundation
@unsafe @preconcurrency import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

package struct KeyFrameLimit: RawRepresentable, Decodable, Sendable {
    package let rawValue: Int

    package init?(rawValue: Int) {
        guard rawValue > 0 else { return nil }
        self.rawValue = rawValue
    }
}

package struct Recording: Encodable, Sendable {
    enum Marker: CaseTagged, Sendable {
        case step(index: Int, at: Date)
        case event(InstanceEvent)

        var at: Date {
            switch self {
                case .step(_, let at): at
                case .event(let event): event.at
            }
        }
    }

    struct KeyFrame: Encodable, Sendable {
        let at: Date
        var changed: CGRect?
        let screenshot: Screenshot
        var markers: [Marker] = []
    }

    private enum Arrival: Sendable {
        case settled(KeyFrame)
        case marked(Marker)
    }

    let keyFrames: [KeyFrame]
    let presentation: Watch.Presentation?

    static func keyFrames(
        of samples: some AsyncSequence<KeyFrame, Never> & Sendable,
        marking markers: some AsyncSequence<Marker, Never> & Sendable,
        from start: KeyFrame,
        keeping limit: KeyFrameLimit,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async -> [KeyFrame] {
        let settled = samples.removeDuplicates { (earlier: KeyFrame, later: KeyFrame) -> Bool in later.screenshot.changed(from: earlier.screenshot)?.isEmpty == true }.debounce(
            for: quiet,
            clock: clock,
        )
        let arrivals = merge(chain(settled.map { (frame: KeyFrame) -> Arrival? in .settled(frame) }, [Arrival?.none].async), markers.map { (marker: Marker) -> Arrival? in .marked(marker) })
        var kept: [KeyFrame] = [start]
        for await arrival in arrivals.prefix(while: { (arrival: Arrival?) -> Bool in arrival != nil }).compactMap(\.self) {
            let sample: KeyFrame
            switch arrival {
                case .marked(let marker):
                    kept[kept.count - 1].markers.append(marker)
                    continue
                case .settled(let frame): sample = frame
            }
            let last: Int = kept.count - 1
            guard let regions = sample.screenshot.changed(from: kept[last].screenshot), let lead = regions.first else { continue }
            let changed: CGRect = regions.dropFirst().reduce(lead) { (joint: CGRect, region: CGRect) -> CGRect in joint.union(region) }
            let split: Int = kept[last].markers.firstIndex { (marker: Marker) -> Bool in marker.at >= sample.at } ?? kept[last].markers.endIndex
            let following: [Marker] = Array(kept[last].markers[split...])
            kept[last].markers.removeSubrange(split...)
            let reverted: Bool = last > 0 && kept[last].markers.isEmpty && kept[last - 1].markers.isEmpty && sample.screenshot.changed(from: kept[last - 1].screenshot)?.isEmpty == true
            guard !reverted else {
                kept.removeLast()
                kept[last - 1].markers = following
                continue
            }
            kept.append(KeyFrame(at: sample.at, changed: changed, screenshot: sample.screenshot, markers: following))
            guard kept.count > limit.rawValue else { continue }
            let areas: [CGFloat] = kept.map { (frame: KeyFrame) -> CGFloat in frame.changed.map { rect in rect.width * rect.height } ?? 0 }
            guard let dropped = kept.indices.dropFirst().dropLast().min(by: { first, second in areas[first] < areas[second] }) else { continue }
            let merged: KeyFrame = kept.remove(at: dropped)
            kept[dropped - 1].markers += merged.markers
            while dropped < kept.count {
                let regions: [CGRect]? = kept[dropped].screenshot.changed(from: kept[dropped - 1].screenshot)
                guard regions?.isEmpty != true else {
                    let same: KeyFrame = kept.remove(at: dropped)
                    kept[dropped - 1].markers += same.markers
                    continue
                }
                kept[dropped].changed = regions.flatMap { (regions: [CGRect]) -> CGRect? in
                    regions.first.map { (lead: CGRect) -> CGRect in regions.dropFirst().reduce(lead) { (joint: CGRect, region: CGRect) -> CGRect in joint.union(region) } }
                }
                break
            }
        }
        let shown: ArraySlice<KeyFrame> = kept.suffix(limit.rawValue)
        return shown.indices.map { (index: Int) -> KeyFrame in
            let frame: KeyFrame = shown[index]
            let changed: CGRect? = index == shown.startIndex ? nil : frame.changed
            let shot: Screenshot = frame.screenshot
            let scale = CGAffineTransform(scaleX: CGFloat(shot.image.width) / shot.frame.width, y: CGFloat(shot.image.height) / shot.frame.height)
            let whole = CGRect(x: 0, y: 0, width: shot.image.width, height: shot.image.height)
            let cropped: Screenshot? = (index < shown.endIndex - 1 ? changed : nil).flatMap { (change: CGRect) -> Screenshot? in
                let pixels: CGRect = change.offsetBy(dx: -shot.frame.minX, dy: -shot.frame.minY).applying(scale).integral.intersection(whole)
                return shot.image.cropping(to: pixels).map { (image: CGImage) -> Screenshot in
                    let area: CGRect = pixels.applying(scale.inverted()).offsetBy(dx: shot.frame.minX, dy: shot.frame.minY)
                    return Screenshot(image: image, frame: area, windows: shot.windows, backingStore: shot.backingStore, document: shot.document, presentation: shot.presentation)
                }
            }
            return KeyFrame(at: frame.at, changed: changed, screenshot: cropped ?? shot, markers: frame.markers)
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

private final class Output: NSObject, SCStreamOutput, SCStreamDelegate, SCRecordingOutputDelegate, Sendable {
    let first = AsyncStream<Result<Recording.KeyFrame, ApplicationFailure>>.makeStream(bufferingPolicy: .bufferingOldest(1))
    let samples = AsyncStream<Recording.KeyFrame>.makeStream(bufferingPolicy: .bufferingNewest(1))
    let outcomes = AsyncStream<Result<Void, ApplicationFailure>>.makeStream(bufferingPolicy: .bufferingOldest(1))
    private let framing: (frame: CGRect, windows: [CGWindowID], document: URL?)
    private let subject: String

    init(framing: (frame: CGRect, windows: [CGWindowID], document: URL?), subject: String) { (self.framing, self.subject) = (framing, subject) }

    func stream(_: SCStream, didOutputSampleBuffer sampleBuffer: CMSampleBuffer, of _: SCStreamOutputType) {
        let info: [SCStreamFrameInfo: Any]? = (CMSampleBufferGetSampleAttachmentsArray(sampleBuffer, createIfNecessary: false) as? [[SCStreamFrameInfo: Any]])?.first
        let status: SCFrameStatus? = (info?[.status] as? Int).flatMap(SCFrameStatus.init(rawValue:))
        guard status == .complete, let displayed = info?[.displayTime] as? UInt64, let pixels = sampleBuffer.imageBuffer else { return }
        let image = CIImage(cvPixelBuffer: pixels)
        guard let copied = rendering.createCGImage(image, from: image.extent) else {
            let failure = ApplicationFailure(subject: subject, reason: .noImage)
            first.continuation.yield(.failure(failure))
            outcomes.continuation.yield(.failure(failure))
            return
        }
        let elapsed: CMTime = CMClock.hostTimeClock.time - CMClock.convertSystemUnitsToHostTime(displayed)
        let screenshot = Screenshot(image: copied, frame: framing.frame, windows: framing.windows, backingStore: false, document: framing.document, presentation: nil)
        let sample = Recording.KeyFrame(at: Date(timeIntervalSinceNow: -elapsed.seconds), changed: nil, screenshot: screenshot)
        first.continuation.yield(.success(sample))
        samples.continuation.yield(sample)
    }

    func stream(_: SCStream, didStopWithError error: any Error) {
        let failure = ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
        first.continuation.yield(.failure(failure))
        outcomes.continuation.yield(.failure(failure))
    }

    func recordingOutputDidFinishRecording(_: SCRecordingOutput) { outcomes.continuation.yield(.success(())) }

    func recordingOutput(_: SCRecordingOutput, didFailWithError error: any Error) {
        let failure = ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
        first.continuation.yield(.failure(failure))
        outcomes.continuation.yield(.failure(failure))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Watch {
    package nonisolated func record<Value>(
        _ source: Source,
        keyFrames limit: KeyFrameLimit,
        longestSide: LongestSide? = nil,
        movie: URL? = nil,
        while body: (_ began: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) -> Value,
    ) async -> (value: Result<Value, ApplicationFailure>, recording: Result<Recording, ApplicationFailure>) {
        var ran: Result<Value, ApplicationFailure>?
        let recording: Result<Recording, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Recording in
            try Grant.screenCapture.check()
            let documents: [CGWindowID: URL] = await self.documents()
            let (keyFrames, presentation) = try await presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> [Recording.KeyFrame] in
                let framed: [Framing] = try await framings(source, showing: menus).map { (framing: Result<Framing, ApplicationFailure>) throws(ApplicationFailure) in
                    try framing.get()
                }
                let area: CGRect = framed.map(\.frame).reduce(CGRect.null) { area, next in area.union(next) }
                guard let joint = try await framed.count > 1 ? framings(.region(frame: area), showing: []).first?.get() : framed.first else {
                    throw ApplicationFailure(subject: "Recording", reason: .noWindow)
                }
                let subject = "Recording of \(joint.subject)"
                let unshown = ApplicationFailure(subject: "\(subject), which no on-screen window of the application shows,", reason: .noImage)
                let (windows, display): ([SCWindow], SCDisplay) =
                    switch joint.route {
                        case .composite(_, let windows, .some(let display)): (windows, display)
                        case .composite(_, _, .none), .displays: throw ApplicationFailure(subject: subject, reason: .spansDisplays)
                        case .backingStore: throw unshown
                    }
                guard let application = windows.first?.owningApplication else { throw unshown }
                let deadline = AsyncTimerSequence(interval: messagingTimeout * 2, clock: SuspendingClock())
                let (expired, cancelled) = (ApplicationFailure(subject: subject, reason: .unanswered(nil)), ApplicationFailure(subject: subject, reason: .cancelled))
                let content = SCContentFilter(display: display, including: [application], exceptingWindows: [])
                let shown: CGRect = joint.frame.intersection(display.frame)
                let size: (width: Int, height: Int) = Framing(route: joint.route, frame: shown, subject: joint.subject).pixels(at: CGFloat(content.pointPixelScale), longestSide: longestSide)
                let configuration = SCStreamConfiguration()
                (configuration.showsCursor, configuration.showMouseClicks, configuration.pixelFormat) = (false, movie != nil, kCVPixelFormatType_32BGRA)
                (configuration.shouldBeOpaque, configuration.ignoreShadowsDisplay, configuration.width, configuration.height) = (true, true, size.width, size.height)
                configuration.minimumFrameInterval = CMTime(seconds: quiet / 2 / .seconds(1), preferredTimescale: CMTimeScale(NSEC_PER_SEC))
                configuration.sourceRect = shown.offsetBy(dx: -display.frame.minX, dy: -display.frame.minY)
                let output = Output(framing: (shown, joint.windows, joint.windows.lazy.compactMap { number in documents[number] }.first), subject: subject)
                let stream = SCStream(filter: content, configuration: configuration, delegate: output)
                let file = SCRecordingOutputConfiguration()
                let type: AVFileType? = movie.flatMap { movie in UTType(filenameExtension: movie.pathExtension) }.map { type in AVFileType(type.identifier) }
                switch (movie, type) {
                    case (.none, _): break
                    case (.some(let movie), .some(let type)) where file.availableOutputFileTypes.contains(type): (file.outputURL, file.outputFileType) = (movie, type)
                    case (.some(let movie), _):
                        let written: [String] = file.availableOutputFileTypes.compactMap { (available: AVFileType) -> String? in UTType(available.rawValue)?.preferredFilenameExtension }
                        throw ApplicationFailure(subject: movie.path(percentEncoded: false), reason: .noEncoder(extensions: written))
                }
                let recorder: SCRecordingOutput? = movie.map { _ in SCRecordingOutput(configuration: file, delegate: output) }
                let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: subject)
                defer { ProcessInfo.processInfo.endActivity(activity) }
                do {
                    try stream.addStreamOutput(output, type: .screen, sampleHandlerQueue: nil)
                    _ = try recorder.map(stream.addRecordingOutput)
                    try await stream.startCapture()
                } catch {
                    throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
                }
                let captured: Result<[Recording.KeyFrame], ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> [Recording.KeyFrame] in
                    let lapse = deadline.map { _ in Result<Recording.KeyFrame, ApplicationFailure>.failure(expired) }
                    let start: Recording.KeyFrame = try await (merge(output.first.stream, lapse).first { _ in true } ?? .failure(cancelled)).get()
                    let (steps, stepping) = AsyncStream<Recording.Marker>.makeStream()
                    let events = await messages().compactMap { (message: Message) -> InstanceEvent? in message[case: \.event] }
                    let markers = merge(events.filter { event in !event.change.is(\.started) }.map(Recording.Marker.event), steps)
                    async let keyFrames: [Recording.KeyFrame] = Recording.keyFrames(of: output.samples.stream, marking: markers, from: start, keeping: limit)
                    ran = await Result { () async throws(ApplicationFailure) -> Value in try await body { index in stepping.yield(.step(index: index, at: .now)) } }
                    stepping.finish()
                    output.samples.continuation.finish()
                    return await keyFrames
                }
                let stopped: Result<Void, any Error> = await withTaskCancellationShield { await Result { () async throws in try await stream.stopCapture() } }
                let stopping: Result<Void, ApplicationFailure> = stopped.mapError { error in ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
                switch (stopping, recorder) {
                    case (.success, .some): break
                    case (.success, .none), (.failure, _): output.outcomes.continuation.yield(stopping)
                }
                let lapse = deadline.map { _ in Result<Void, ApplicationFailure>.failure(expired) }
                let ended: Result<Void, ApplicationFailure> = await merge(output.outcomes.stream, lapse).first { _ in true } ?? .failure(cancelled)
                let keyFrames: [Recording.KeyFrame] = try captured.get()
                try ended.get()
                return keyFrames
            }
            return Recording(keyFrames: keyFrames, presentation: presentation)
        }
        let value: Result<Value, ApplicationFailure> = if let ran { ran } else { await Result { () async throws(ApplicationFailure) -> Value in try await body { _ in } } }
        return (value, recording)
    }
}
