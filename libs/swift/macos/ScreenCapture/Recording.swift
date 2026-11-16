import AVFoundation
public import Applications
import Processes
public import ScreenCaptureKit
import Synchronization
import UniformTypeIdentifiers

// --- [SERVICES] ------------------------------------------------------------------------

public final class Recording {
    private final class Events: NSObject, SCStreamDelegate, SCRecordingOutputDelegate, Sendable {
        let stopped: Atomic<Bool> = .init(false)
        let outcomes: (stream: AsyncStream<Result<Void, ApplicationFailure>>, continuation: AsyncStream<Result<Void, ApplicationFailure>>.Continuation) = AsyncStream.makeStream()
        private let path: String

        init(path: String) { self.path = path }

        func stream(_: SCStream, didStopWithError error: any Error) {
            stopped.store(true, ordering: .relaxed)
            ended(.failure(ApplicationFailure(subject: path, reason: .host(error as NSError))))
        }

        func recordingOutputDidFinishRecording(_: SCRecordingOutput) { ended(.success(())) }

        func recordingOutput(_: SCRecordingOutput, didFailWithError error: any Error) { ended(.failure(ApplicationFailure(subject: path, reason: .host(error as NSError)))) }

        private func ended(_ outcome: Result<Void, ApplicationFailure>) {
            outcomes.continuation.yield(outcome)
            outcomes.continuation.finish()
        }
    }

    public static let buffered: Duration = .seconds(15)

    public let file: URL
    private let type: AVFileType
    private let owner: ProcessInstance
    private let stream: SCStream
    private let output: SCRecordingOutput
    private let buffer: SCClipBufferingOutput = .init(delegate: nil)
    private let events: Events
    private let written: Task<Result<Void, ApplicationFailure>, Never>

    private var live: Bool { !(owner.hasExited || events.stopped.load(ordering: .relaxed)) }

    init(_ filter: SCContentFilter, _ configuration: SCStreamConfiguration, of owner: ProcessInstance, to file: URL) async throws(ApplicationFailure) {
        let path = file.path(percentEncoded: false)
        guard let type = UTType(filenameExtension: file.pathExtension).map({ type in AVFileType(type.identifier) }) else {
            throw ApplicationFailure(subject: path, reason: .noExtension)
        }
        let movie = SCRecordingOutputConfiguration()
        guard movie.availableOutputFileTypes.contains(type) else { throw ApplicationFailure(subject: type.rawValue, reason: .noEncoder) }
        (movie.outputURL, movie.outputFileType) = (file, type)
        let events = Events(path: path)
        (self.file, self.type, self.owner, self.events) = (file, type, owner, events)
        stream = SCStream(filter: filter, configuration: configuration, delegate: events)
        output = SCRecordingOutput(configuration: movie, delegate: events)
        written = Task { await events.outcomes.stream.first { _ in true } ?? .failure(ApplicationFailure(subject: path, reason: .cancelled)) }
        do {
            try stream.addRecordingOutput(output)
            try await stream.startCapture()
        } catch {
            events.outcomes.continuation.finish()
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
        do { try stream.addClipBufferingOutput(buffer) } catch {
            try? await stream.stopCapture()
            events.outcomes.continuation.finish()
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
    }

    deinit {
        if !events.stopped.load(ordering: .relaxed) { stream.stopCapture(completionHandler: nil) }
    }

    public func clip(to file: URL) async throws(ApplicationFailure) {
        let (path, seconds) = (file.path(percentEncoded: false), Self.buffered / .seconds(1))
        if live {
            let failed: any Error? = await withCheckedContinuation { exported in
                buffer.exportClip(to: file, duration: seconds) { error in exported.resume(returning: error) }
            }
            guard let failed else { return }
            throw ApplicationFailure(subject: path, reason: .host(failed as NSError))
        }
        try await written.value.get()
        let asset = AVURLAsset(url: self.file)
        guard let export = AVAssetExportSession(asset: asset, presetName: AVAssetExportPresetPassthrough) else {
            throw ApplicationFailure(subject: type.rawValue, reason: .noEncoder)
        }
        do {
            let duration = try await asset.load(.duration)
            export.timeRange = CMTimeRange(start: max(.zero, duration - CMTime(seconds: seconds, preferredTimescale: duration.timescale)), end: duration)
            try await export.export(to: file, as: type)
        } catch {
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
    }

    public func finish() async throws(ApplicationFailure) {
        if !events.stopped.load(ordering: .relaxed) {
            var failure: (any Error)?
            do {
                try stream.removeClipBufferingOutput(buffer)
                try stream.removeRecordingOutput(output)
                _ = await written.value
            } catch {
                failure = error
            }
            do { try await stream.stopCapture() } catch { failure = failure ?? error }
            events.stopped.store(true, ordering: .relaxed)
            if let failure { throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(failure as NSError)) }
        }
        try await written.value.get()
    }
}

@available(*, unavailable)
extension Recording: Sendable {}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension SCStreamConfiguration {
    public static var background: SCStreamConfiguration {
        let configuration = SCStreamConfiguration()
        configuration.showsCursor = false
        return configuration
    }
}

extension Instance {
    public func recording(
        _ source: Source,
        to file: URL,
        longestSide: Int? = nil,
        configuration: () -> SCStreamConfiguration = { .background },
    ) async throws(ApplicationFailure) -> Recording {
        guard let target = try await targets(source, longestSide: longestSide).values.first else { throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .noWindow) }
        let (filter, region, factor) = try target.get()
        let size = region?.size ?? filter.contentRect.size
        let capture = configuration()
        (capture.width, capture.height) = (Int((size.width * factor).rounded()), Int((size.height * factor).rounded()))
        if let region { capture.sourceRect = region }
        return try await Recording(filter, capture, of: process, to: file)
    }
}
