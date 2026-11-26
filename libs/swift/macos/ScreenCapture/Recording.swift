public import Applications
public import Foundation
import Processes
import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct Recording: Encodable, Sendable {
    public let movie: Data
    public let type: String
}

// --- [SERVICES] ------------------------------------------------------------------------

private final class Events: NSObject, SCRecordingOutputDelegate, SCStreamDelegate, Sendable {
    let outcomes: (stream: AsyncStream<Result<Void, ApplicationFailure>>, continuation: AsyncStream<Result<Void, ApplicationFailure>>.Continuation) = AsyncStream.makeStream()
    private let path: String

    init(path: String) { self.path = path }

    func recordingOutputDidFinishRecording(_: SCRecordingOutput) { end(.success(())) }

    func recordingOutput(_: SCRecordingOutput, didFailWithError error: any Error) { end(.failure(error)) }

    func stream(_: SCStream, didStopWithError error: any Error) { end(.failure(error)) }

    func end(_ outcome: Result<Void, any Error>) {
        outcomes.continuation.yield(outcome.mapError { (error: any Error) -> ApplicationFailure in ApplicationFailure(subject: path, reason: .foundation(error as NSError)) })
        outcomes.continuation.finish()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Instance {
    public func recording(_ source: Source, in watch: Watch, for span: Duration, longestSide: LongestSide? = nil) async throws(ApplicationFailure) -> Recording {
        try Grant.screenCapture.check()
        return try await watch.presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> Recording in
            guard let framing = try await framings(source, showing: menus, in: watch, longestSide: longestSide).first else {
                throw ApplicationFailure(subject: String(process.pid), reason: .noWindow)
            }
            let (_, _, width, height, frame, windows, display) = try framing.get()
            guard let display, let application = windows.first?.owningApplication else { throw ApplicationFailure(subject: String(process.pid), reason: .noWindow) }
            let configuration = SCStreamConfiguration()
            (configuration.showsCursor, configuration.ignoreShadowsDisplay, configuration.width, configuration.height) = (false, true, width, height)
            configuration.sourceRect = frame.offsetBy(dx: -display.frame.minX, dy: -display.frame.minY).intersection(CGRect(origin: .zero, size: display.frame.size))
            let output = SCRecordingOutputConfiguration()
            guard let type = UTType(output.outputFileType.rawValue) else { throw ApplicationFailure(subject: output.outputFileType.rawValue, reason: .noEncoder) }
            let folder: URL
            do { folder = try FileManager.default.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: FileManager.default.temporaryDirectory, create: true) } catch {
                throw ApplicationFailure(subject: FileManager.default.temporaryDirectory.path(percentEncoded: false), reason: .foundation(error as NSError))
            }
            output.outputURL = folder.appending(component: String(process.pid)).appendingPathExtension(for: type)
            let path = output.outputURL.path(percentEncoded: false)
            let events = Events(path: path)
            let stream = SCStream(filter: SCContentFilter(display: display, including: [application], exceptingWindows: []), configuration: configuration, delegate: events)
            let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: "Recording process \(process.pid)")
            defer { ProcessInfo.processInfo.endActivity(activity) }
            let recorded: Result<Data, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Data in
                do {
                    try stream.addRecordingOutput(SCRecordingOutput(configuration: output, delegate: events))
                    try await stream.startCapture()
                } catch {
                    throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
                }
                _ = try? await Task.sleep(for: span)
                stream.stopCapture { (error: (any Error)?) in if let error { events.end(.failure(error)) } }
                try await (events.outcomes.stream.first { _ in true } ?? .failure(ApplicationFailure(subject: path, reason: .cancelled))).get()
                do { return try Data(contentsOf: output.outputURL) } catch { throw ApplicationFailure(subject: path, reason: .foundation(error as NSError)) }
            }
            do { try FileManager.default.removeItem(at: folder) } catch { throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .foundation(error as NSError)) }
            return Recording(movie: try recorded.get(), type: type.identifier)
        }.value
    }
}
