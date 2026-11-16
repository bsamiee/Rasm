import AVFoundation
public import Applications
import Processes
public import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [SERVICES] ------------------------------------------------------------------------

public final class Recording {
    private final class Events: NSObject, SCRecordingOutputDelegate, Sendable {
        let outcomes: (stream: AsyncStream<Result<Void, ApplicationFailure>>, continuation: AsyncStream<Result<Void, ApplicationFailure>>.Continuation) = AsyncStream.makeStream()
        private let path: String

        init(path: String) { self.path = path }

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
    private let buffer = SCClipBufferingOutput(delegate: nil)
    private let written: Task<Result<Void, ApplicationFailure>, Never>

    init(_ filter: SCContentFilter, _ configuration: SCStreamConfiguration, of owner: ProcessInstance, to file: URL) async throws(ApplicationFailure) {
        let path = file.path(percentEncoded: false)
        guard let type = UTType(filenameExtension: file.pathExtension).map({ type in AVFileType(type.identifier) }) else {
            throw ApplicationFailure(subject: path, reason: .noExtension)
        }
        let movie = SCRecordingOutputConfiguration()
        guard movie.availableOutputFileTypes.contains(type) else { throw ApplicationFailure(subject: type.rawValue, reason: .noEncoder) }
        (movie.outputURL, movie.outputFileType) = (file, type)
        let events = Events(path: path)
        (self.file, self.type, self.owner) = (file, type, owner)
        stream = SCStream(filter: filter, configuration: configuration, delegate: nil)
        written = Task { await events.outcomes.stream.first { _ in true } ?? .failure(ApplicationFailure(subject: path, reason: .cancelled)) }
        do {
            try stream.addRecordingOutput(SCRecordingOutput(configuration: movie, delegate: events))
            try await stream.startCapture()
            try stream.addClipBufferingOutput(buffer)
        } catch {
            events.outcomes.continuation.finish()
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
    }

    deinit {
        if stream.isCapturing { stream.stopCapture() }
    }

    public func clip(to file: URL) async throws(ApplicationFailure) {
        let (path, seconds) = (file.path(percentEncoded: false), Self.buffered / .seconds(1))
        if !owner.hasExited, stream.isCapturing {
            do { try await buffer.exportClip(to: file, duration: seconds) } catch { throw ApplicationFailure(subject: path, reason: .host(error as NSError)) }
            return
        }
        try await finish()
        let asset = AVURLAsset(url: self.file)
        guard let export = AVAssetExportSession(asset: asset, presetName: AVAssetExportPresetPassthrough) else { throw ApplicationFailure(subject: type.rawValue, reason: .noEncoder) }
        do {
            let duration = try await asset.load(.duration)
            export.timeRange = CMTimeRange(start: max(.zero, duration - CMTime(seconds: seconds, preferredTimescale: duration.timescale)), end: duration)
            try await export.export(to: file, as: type)
        } catch {
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
    }

    public func finish() async throws(ApplicationFailure) {
        do { if stream.isCapturing { try await stream.stopCapture() } } catch SCStreamError.attemptToStopStreamState {} catch {
            throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error as NSError))
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
    public func recording(_ source: Source, to file: URL, longestSide: Int? = nil, configuration: SCStreamConfiguration = .background) async throws(ApplicationFailure) -> Recording {
        guard let target = try await targets(source, longestSide: longestSide).values.first else { throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .noWindow) }
        let (filter, region, factor) = try target.get()
        let size = region?.size ?? filter.contentRect.size
        (configuration.width, configuration.height) = (Int((size.width * factor).rounded()), Int((size.height * factor).rounded()))
        if let region { configuration.sourceRect = region }
        return try await Recording(filter, configuration, of: process, to: file)
    }
}
