import AVFoundation
import Processes
public import ScreenCaptureKit
import Synchronization
import UniformTypeIdentifiers

// --- [SERVICES] ------------------------------------------------------------------------

public final class WindowRecording {
    private final class Events: NSObject, SCStreamDelegate, SCRecordingOutputDelegate, Sendable {
        let stopped: Atomic<Bool> = .init(false)
        let outcomes: (stream: AsyncStream<Result<Void, ApplicationFailure>>, continuation: AsyncStream<Result<Void, ApplicationFailure>>.Continuation) = AsyncStream.makeStream()
        private let path: String

        init(path: String) { self.path = path }

        func stream(_: SCStream, didStopWithError _: any Error) { stopped.store(true, ordering: .relaxed) }

        func recordingOutputDidFinishRecording(_: SCRecordingOutput) {
            outcomes.continuation.yield(.success(()))
            outcomes.continuation.finish()
        }

        func recordingOutput(_: SCRecordingOutput, didFailWithError error: any Error) {
            outcomes.continuation.yield(.failure(ApplicationFailure(subject: path, reason: .host(error as NSError))))
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
            try stream.addClipBufferingOutput(buffer)
            try await stream.startCapture()
        } catch {
            events.outcomes.continuation.finish()
            throw ApplicationFailure(subject: path, reason: .host(error as NSError))
        }
    }

    public func clip(to file: URL) async throws(ApplicationFailure) {
        let (path, seconds) = (file.path(percentEncoded: false), Self.buffered / .seconds(1))
        if live {
            let failed: any Error? = await withCheckedContinuation { exported in
                buffer.exportClip(to: file, duration: seconds) { error in exported.resume(returning: error) }
            }
            guard let failed else { return }
            if live { throw ApplicationFailure(subject: path, reason: .host(failed as NSError)) }
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
        do {
            if live {
                try stream.removeRecordingOutput(output)
                _ = await written.value
                try await stream.stopCapture()
            }
        } catch  where live {
            throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error as NSError))
        } catch {}
        try await written.value.get()
    }
}

@available(*, unavailable)
extension WindowRecording: Sendable {}

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
        of window: CGWindowID,
        to file: URL,
        longestSide: Int? = nil,
        configuration: () -> SCStreamConfiguration = { .background },
    ) async throws(ApplicationFailure) -> WindowRecording {
        guard let target = try await targets(of: [window], longestSide: longestSide)[window] else { throw ApplicationFailure(subject: String(window), reason: .noWindow) }
        let (filter, width, height) = try target.get()
        let capture = configuration()
        (capture.width, capture.height) = (width, height)
        return try await WindowRecording(filter, capture, of: process, to: file)
    }
}
