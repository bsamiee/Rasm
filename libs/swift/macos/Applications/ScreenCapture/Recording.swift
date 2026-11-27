import AVFoundation
import AsyncAlgorithms
import CasePaths
import CasePathsCore
import CoreImage
import CoreMedia
import Encoding
package import Foundation
@unsafe @preconcurrency import ScreenCaptureKit
import Synchronization
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

package struct KeyFrameLimit: RawRepresentable, Decodable, Sendable {
    package let rawValue: Int

    package init?(rawValue: Int) {
        guard rawValue > 0 else { return nil }
        self.rawValue = rawValue
    }
}

package struct KeyFrame: Encodable, Sendable {
    // --- [ENCODING]

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

    private enum CodingKeys: CodingKey {
        case at, changed, markers
    }

    let at: Date
    var changed: CGRect?
    var screenshot: Screenshot
    var markers: [Marker] = []

    package func encode(to encoder: any Encoder) throws {
        try screenshot.encode(to: encoder)
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(at, forKey: .at)
        try fields.encodeIfPresent(changed, forKey: .changed)
        try fields.encode(markers, forKey: .markers)
    }

    // --- [SELECTION]

    private enum Arrival: Sendable {
        case drawn(StreamOutput.Frame)
        case marked(Marker)
    }

    static func select(
        from frames: some AsyncSequence<StreamOutput.Frame, Never> & Sendable,
        marking markers: some AsyncSequence<Marker, Never> & Sendable,
        after start: Self,
        keeping limit: KeyFrameLimit,
    ) async -> [Self] {
        let settling = StreamOutput.Frame(status: .idle, displayed: nil, dirty: [], image: nil)
        let drawn = chain(frames.map { (frame: StreamOutput.Frame) -> Arrival? in .drawn(frame) }, [Arrival?.some(.drawn(settling)), nil].async)
        let arrivals = merge(drawn, markers.map { (marker: Marker) -> Arrival? in .marked(marker) })
        var kept: [Self] = [start]
        var pending: (image: CGImage, at: Date, changed: CGRect)?
        for await arrival in arrivals.prefix(while: { (arrival: Arrival?) -> Bool in arrival != nil }).compactMap(\.self) {
            let frame: StreamOutput.Frame
            switch arrival {
                case .marked(let marker):
                    kept[kept.count - 1].markers.append(marker)
                    continue
                case .drawn(let drawn): frame = drawn
            }
            let changed: CGRect = frame.dirty.reduce(pending?.changed ?? CGRect.null) { (joint: CGRect, region: CGRect) -> CGRect in joint.union(region) }
            if frame.status == .complete, let image = frame.image { pending = (image, frame.at, changed) }
            guard frame.status == .idle, let settled = exchange(&pending, with: nil), !settled.changed.isNull else { continue }
            let last: Int = kept.count - 1
            let split: Int = kept[last].markers.firstIndex { (marker: Marker) -> Bool in marker.at >= settled.at } ?? kept[last].markers.endIndex
            let following: [Marker] = Array(kept[last].markers[split...])
            kept[last].markers.removeSubrange(split...)
            let shot: Screenshot = start.screenshot
            let screenshot = Screenshot(image: settled.image, frame: shot.frame, windows: shot.windows, backingStore: false, document: shot.document, presentation: nil)
            kept.append(Self(at: settled.at, changed: settled.changed, screenshot: screenshot, markers: following))
            guard kept.count > limit.rawValue else { continue }
            let areas: [CGFloat] = kept.map { (frame: Self) -> CGFloat in frame.changed.map { rect in rect.width * rect.height } ?? 0 }
            guard let dropped = kept.indices.dropFirst().dropLast().min(by: { first, second in areas[first] < areas[second] }) else { continue }
            let merged: Self = kept.remove(at: dropped)
            kept[dropped - 1].markers += merged.markers
            kept[dropped].changed = [merged.changed, kept[dropped].changed].compactMap(\.self).reduce(CGRect.null) { (joint: CGRect, region: CGRect) -> CGRect in joint.union(region) }
        }
        let shown: ArraySlice<Self> = kept.suffix(limit.rawValue)
        return shown.indices.map { (index: Int) -> Self in
            let frame: Self = shown[index]
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
            return Self(at: frame.at, changed: changed, screenshot: cropped ?? shot, markers: frame.markers)
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

final class StreamOutput: NSObject, SCStreamOutput, SCStreamDelegate, SCRecordingOutputDelegate, Sendable {
    // --- [FRAMES]

    struct Frame: Sendable {
        let status: SCFrameStatus
        let displayed: UInt64?
        let dirty: [CGRect]
        let image: CGImage?

        var drew: Bool { status == .complete }

        var at: Date {
            let elapsed: CMTime? = displayed.map { (displayed: UInt64) -> CMTime in CMClock.hostTimeClock.time - CMClock.convertSystemUnitsToHostTime(displayed) }
            return Date(timeIntervalSinceNow: -(elapsed?.seconds ?? 0))
        }
    }

    private struct Held {
        var waiting: [UUID: AsyncStream<Frame>.Continuation] = [:]
        var began: Bool = false
        var resting: Bool = true
        var released: Bool = false
        var drawn: UInt64 = 0
    }

    let first = AsyncStream<Result<Frame, ApplicationFailure>>.makeStream(bufferingPolicy: .bufferingOldest(1))
    let outcomes = AsyncStream<Result<Void, ApplicationFailure>>.makeStream(bufferingPolicy: .bufferingOldest(1))
    let ended = AsyncStream<Void>.makeStream()
    let subject: String
    private let area: CGRect
    private let resting: (@Sendable ([CGRect]) -> Void)?
    private let held = Mutex(Held())

    init(showing area: CGRect, subject: String, resting: (@Sendable ([CGRect]) -> Void)?) { (self.area, self.subject, self.resting) = (area, subject, resting) }

    func frames() -> AsyncStream<Frame> {
        let (stream, continuation) = AsyncStream<Frame>.makeStream()
        let id = UUID()
        continuation.onTermination = { _ in self.release(id) }
        held.withLock { held in
            held.waiting[id] = continuation
            held.resting = false
        }
        return stream
    }

    func disturb() { held.withLock { held in held.resting = false } }

    func drew(after instant: UInt64) -> Bool { held.withLock { held in held.drawn > instant } }

    func release(_ waiter: UUID? = nil) {
        let over: Bool = held.withLock { held in
            if let waiter { held.waiting[waiter] = nil }
            held.released = held.released || waiter == nil
            return held.released && held.waiting.isEmpty
        }
        if over { ended.continuation.finish() }
    }

    func finish() {
        let waiting: [UUID: AsyncStream<Frame>.Continuation] = held.withLock { held in exchange(&held.waiting, with: [:]) }
        for waiter in waiting.values { waiter.finish() }
    }

    // --- [DELEGATE]

    func stream(_: SCStream, didOutputSampleBuffer sampleBuffer: CMSampleBuffer, of _: SCStreamOutputType) {
        let info: [SCStreamFrameInfo: Any]? = (CMSampleBufferGetSampleAttachmentsArray(sampleBuffer, createIfNecessary: false) as? [[SCStreamFrameInfo: Any]])?.first
        guard let status = (info?[.status] as? Int).flatMap(SCFrameStatus.init(rawValue:)) else { return }
        let content: CGRect? = (info?[.contentRect] as? [String: Any]).flatMap { (fields: [String: Any]) -> CGRect? in CGRect(dictionaryRepresentation: fields as CFDictionary) }
        let (scale, shrink): (CGFloat?, CGFloat?) = (info?[.scaleFactor] as? CGFloat, info?[.contentScale] as? CGFloat)
        let placing: CGAffineTransform? =
            if let content, let scale, let shrink {
                CGAffineTransform(translationX: area.minX - content.minX / shrink, y: area.minY - content.minY / shrink).scaledBy(x: 1 / (scale * shrink), y: 1 / (scale * shrink))
            } else {
                nil
            }
        let listed: [Any] = info?[.dirtyRects] as? [Any] ?? []
        let rects: [CGRect] = listed.compactMap { (entry: Any) -> CGRect? in
            (entry as? NSValue)?.rectValue ?? (entry as? [String: Any]).flatMap { (fields: [String: Any]) -> CGRect? in CGRect(dictionaryRepresentation: fields as CFDictionary) }
        }
        let drawn: [CGRect] = rects.filter { (rect: CGRect) -> Bool in !rect.isEmpty }
        let dirty: [CGRect] = placing.map { (placing: CGAffineTransform) -> [CGRect] in drawn.map { (rect: CGRect) -> CGRect in rect.applying(placing) } } ?? []
        let pixels: CVPixelBuffer? = status == .complete && resting == nil ? sampleBuffer.imageBuffer : nil
        let image: CGImage? = pixels.flatMap { (pixels: CVPixelBuffer) -> CGImage? in
            let picture = CIImage(cvPixelBuffer: pixels)
            return rendering.createCGImage(picture, from: picture.extent)
        }
        guard pixels == nil || image != nil else {
            fail(.noImage)
            return
        }
        let frame = Frame(status: status, displayed: info?[.displayTime] as? UInt64, dirty: dirty, image: image)
        let drew: Bool = status == .complete && (resting != nil || image != nil)
        if drew { first.continuation.yield(.success(frame)) }
        let (waiting, rested): ([AsyncStream<Frame>.Continuation], Bool) = held.withLock { held in
            let unwatched: Bool = drew && held.began && held.resting && held.waiting.isEmpty
            held.began = held.began || drew
            held.resting = held.resting || (status == .idle && held.waiting.isEmpty)
            held.drawn = drew && !dirty.isEmpty ? frame.displayed ?? held.drawn : held.drawn
            return (Array(held.waiting.values), unwatched)
        }
        for waiter in waiting { waiter.yield(frame) }
        if rested, !dirty.isEmpty { resting?(dirty) }
    }

    func stream(_: SCStream, didStopWithError error: any Error) { fail(.foundation(error as NSError)) }

    func recordingOutputDidFinishRecording(_: SCRecordingOutput) { outcomes.continuation.yield(.success(())) }

    func recordingOutput(_: SCRecordingOutput, didFailWithError error: any Error) { fail(.foundation(error as NSError)) }

    private func fail(_ reason: ApplicationFailure.Reason) {
        let failure = ApplicationFailure(subject: subject, reason: reason)
        first.continuation.yield(.failure(failure))
        outcomes.continuation.yield(.failure(failure))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Watch {
    nonisolated func stream<Value>(
        _ source: Source,
        longestSide: LongestSide? = nil,
        movie: URL? = nil,
        resting: (@Sendable ([CGRect]) -> Void)? = nil,
        while body: (_ output: StreamOutput, _ start: StreamOutput.Frame, _ shown: (frame: CGRect, windows: [CGWindowID])) async throws(ApplicationFailure) -> Value,
    ) async throws(ApplicationFailure) -> (value: Value, presentation: Presentation?) {
        try Grant.screenCapture.check()
        return try await presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> Value in
            let framed: [Framing] = try await frame(source, showing: menus).map { (framing: Result<Framing, ApplicationFailure>) throws(ApplicationFailure) in
                try framing.get()
            }
            let area: CGRect = framed.map(\.frame).reduce(CGRect.null) { area, next in area.union(next) }
            guard let joint = try await framed.count > 1 ? frame(.region(frame: area), showing: []).first?.get() : framed.first else {
                throw ApplicationFailure(subject: "\(SCStream.self)", reason: .noWindow)
            }
            let subject = "\(SCStream.self) of \(joint.subject)"
            let unshown = ApplicationFailure(subject: subject, reason: .noWindow)
            let (windows, display): ([SCWindow], SCDisplay) =
                switch joint.content {
                    case .composite(_, let windows, let displays) where displays.count == 1: (windows, displays[0])
                    case .composite, .displays: throw ApplicationFailure(subject: subject, reason: .spansDisplays)
                    case .backingStore: throw unshown
                }
            guard let application = windows.first?.owningApplication else { throw unshown }
            let deadline = AsyncTimerSequence(interval: messagingTimeout * 2, clock: SuspendingClock())
            let (expired, cancelled) = (ApplicationFailure(subject: subject, reason: .unanswered), ApplicationFailure(subject: subject, reason: .cancelled))
            let filter = SCContentFilter(display: display, including: [application], exceptingWindows: [])
            let shown: CGRect = joint.frame.intersection(display.frame)
            let size: (width: Int, height: Int) = Framing(content: joint.content, frame: shown, subject: joint.subject).fit(at: CGFloat(filter.pointPixelScale), longestSide: longestSide)
            let configuration = SCStreamConfiguration()
            (configuration.showsCursor, configuration.showMouseClicks, configuration.pixelFormat) = (false, movie != nil, kCVPixelFormatType_32BGRA)
            (configuration.shouldBeOpaque, configuration.ignoreShadowsDisplay, configuration.width, configuration.height) = (true, true, size.width, size.height)
            configuration.sourceRect = shown.offsetBy(dx: -display.frame.minX, dy: -display.frame.minY)
            let output = StreamOutput(showing: shown, subject: subject, resting: resting)
            let stream = SCStream(filter: filter, configuration: configuration, delegate: output)
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
            let ran: Result<Value, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Value in
                let lapse = deadline.map { _ in Result<StreamOutput.Frame, ApplicationFailure>.failure(expired) }
                let start: StreamOutput.Frame = try await (merge(output.first.stream, lapse).first { _ in true } ?? .failure(cancelled)).get()
                return try await body(output, start, (shown, joint.windows))
            }
            let stopped: Result<Void, any Error> = await withTaskCancellationShield { await Result { () async throws in try await stream.stopCapture() } }
            let stopping: Result<Void, ApplicationFailure> = stopped.mapError { error in ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
            switch (stopping, recorder) {
                case (.success, .some): break
                case (.success, .none), (.failure, _): output.outcomes.continuation.yield(stopping)
            }
            let lapse = deadline.map { _ in Result<Void, ApplicationFailure>.failure(expired) }
            let ended: Result<Void, ApplicationFailure> = await merge(output.outcomes.stream, lapse).first { _ in true } ?? .failure(cancelled)
            let value: Value = try ran.get()
            try ended.get()
            return value
        }
    }

    nonisolated func proving(_ number: CGWindowID, during call: AsyncStream<Void>, into opening: AsyncStream<Result<StreamOutput, ApplicationFailure>>.Continuation) async {
        let resting: @Sendable ([CGRect]) -> Void = { (dirty: [CGRect]) in self.rest(dirty, in: number) }
        do throws(ApplicationFailure) {
            _ = try await stream(.windows(numbers: [number]), resting: resting) {
                (output: StreamOutput, _: StreamOutput.Frame, _: (frame: CGRect, windows: [CGWindowID])) async throws(ApplicationFailure) in
                opening.yield(.success(output))
                for await _ in call {}
                output.release()
                for await _ in output.ended.stream {}
            }
        } catch {
            opening.yield(.failure(error))
        }
        opening.finish()
    }

    package nonisolated func record<Value>(
        _ source: Source,
        keyFrames limit: KeyFrameLimit,
        longestSide: LongestSide? = nil,
        movie: URL? = nil,
        while body: (_ began: @escaping @Sendable (Int) -> Void) async throws(ApplicationFailure) -> Value,
    ) async -> (value: Result<Value, ApplicationFailure>, frames: Result<[KeyFrame], ApplicationFailure>) {
        var ran: Result<Value, ApplicationFailure>?
        let frames: Result<[KeyFrame], ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> [KeyFrame] in
            let documents: [CGWindowID: URL] = await self.documents()
            let (keyFrames, presentation) = try await stream(source, longestSide: longestSide, movie: movie) {
                (output: StreamOutput, start: StreamOutput.Frame, shown: (frame: CGRect, windows: [CGWindowID])) async throws(ApplicationFailure) -> [KeyFrame] in
                guard let image = start.image else { throw ApplicationFailure(subject: output.subject, reason: .noImage) }
                let document: URL? = shown.windows.lazy.compactMap { number in documents[number] }.first
                let screenshot = Screenshot(image: image, frame: shown.frame, windows: shown.windows, backingStore: false, document: document, presentation: nil)
                let (steps, stepping) = AsyncStream<KeyFrame.Marker>.makeStream()
                let events = await messages().compactMap { (message: Message) -> InstanceEvent? in message[case: \.event] }
                let markers = merge(events.filter { event in event.isPushed && !event.change.is(\.started) }.map(KeyFrame.Marker.event), steps)
                let drawn: AsyncStream<StreamOutput.Frame> = output.frames()
                async let keyFrames: [KeyFrame] = KeyFrame.select(from: drawn, marking: markers, after: KeyFrame(at: start.at, changed: nil, screenshot: screenshot), keeping: limit)
                ran = await Result { () async throws(ApplicationFailure) -> Value in try await body { index in stepping.yield(.step(index: index, at: .now)) } }
                stepping.finish()
                output.finish()
                return await keyFrames
            }
            return keyFrames.map { (keyFrame: KeyFrame) -> KeyFrame in
                var presented: KeyFrame = keyFrame
                presented.screenshot.presentation = presentation
                return presented
            }
        }
        let value: Result<Value, ApplicationFailure> = if let ran { ran } else { await Result { () async throws(ApplicationFailure) -> Value in try await body { _ in } } }
        return (value, frames)
    }
}
