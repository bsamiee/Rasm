import CoreVideo
public import Foundation
import Processes
import Results
@unsafe @preconcurrency import ScreenCaptureKit
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

extension Watch {
    public nonisolated func record(_ source: Source, for span: Duration, longestSide: LongestSide? = nil) async throws(ApplicationFailure) -> Recording {
        try Grant.screenCapture.check()
        let process: pid_t = instance.process.pid
        return try await presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> Recording in
            guard let framing = try await framings(source, showing: menus, longestSide: longestSide).first else {
                throw ApplicationFailure(subject: String(process), reason: .noWindow)
            }
            let framed: Framing = try framing.get()
            guard let display = framed.display, let application = framed.windows.first?.owningApplication else { throw ApplicationFailure(subject: String(process), reason: .noWindow) }
            let configuration = SCStreamConfiguration()
            (configuration.showsCursor, configuration.showMouseClicks, configuration.pixelFormat) = (false, true, kCVPixelFormatType_32BGRA)
            (configuration.ignoreShadowsDisplay, configuration.width, configuration.height) = (true, framed.width, framed.height)
            configuration.sourceRect = framed.frame.offsetBy(dx: -display.frame.minX, dy: -display.frame.minY).intersection(CGRect(origin: .zero, size: display.frame.size))
            let output = SCRecordingOutputConfiguration()
            guard let type = UTType(output.outputFileType.rawValue) else { throw ApplicationFailure(subject: output.outputFileType.rawValue, reason: .noEncoder) }
            let folder: URL
            do { folder = try FileManager.default.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: FileManager.default.temporaryDirectory, create: true) } catch {
                throw ApplicationFailure(subject: FileManager.default.temporaryDirectory.path(percentEncoded: false), reason: .foundation(error as NSError))
            }
            output.outputURL = folder.appending(component: String(process)).appendingPathExtension(for: type)
            let path = output.outputURL.path(percentEncoded: false)
            let events = Events(path: path)
            let stream = SCStream(filter: SCContentFilter(display: display, including: [application], exceptingWindows: []), configuration: configuration, delegate: events)
            let activity = ProcessInfo.processInfo.beginActivity(options: [.idleDisplaySleepDisabled, .userInitiated], reason: "Recording process \(process)")
            defer { ProcessInfo.processInfo.endActivity(activity) }
            let recorded: Result<Data, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Data in
                let (expired, cancelled) = (ApplicationFailure(subject: path, reason: .unanswered(nil)), ApplicationFailure(subject: path, reason: .cancelled))
                do { try stream.addRecordingOutput(SCRecordingOutput(configuration: output, delegate: events)) } catch {
                    throw ApplicationFailure(subject: path, reason: .foundation(error as NSError))
                }
                try await withTimeout(messagingTimeout * 2, expired: expired, cancelled: cancelled) { answer in
                    stream.startCapture { (error: (any Error)?) in
                        answer(Result { () throws(ApplicationFailure) in if let error { throw ApplicationFailure(subject: path, reason: .foundation(error as NSError)) } })
                    }
                }
                let elapsed: Result<Void, ApplicationFailure> = await Result { () async throws(ApplicationFailure) in
                    do { try await Task.sleep(for: span) } catch { throw cancelled }
                }
                let stopped: Result<Void, ApplicationFailure> = await Result { () async throws(ApplicationFailure) in
                    try await withTimeout(messagingTimeout * 2, expired: expired, cancelled: cancelled) { answer in
                        stream.stopCapture { (error: (any Error)?) in
                            answer(Result { () throws(ApplicationFailure) in if let error { throw ApplicationFailure(subject: path, reason: .foundation(error as NSError)) } })
                        }
                    }
                }
                try elapsed.get()
                try stopped.get()
                try await withTimeout(messagingTimeout * 2, expired: expired) { () async throws(ApplicationFailure) in
                    try await (events.outcomes.stream.first { _ in true } ?? .failure(cancelled)).get()
                }
                do { return try Data(contentsOf: output.outputURL) } catch { throw ApplicationFailure(subject: path, reason: .foundation(error as NSError)) }
            }
            do { try FileManager.default.removeItem(at: folder) } catch { throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .foundation(error as NSError)) }
            return Recording(movie: try recorded.get(), type: type.identifier)
        }.value
    }
}
