import AppKit
import Applications
import CasePaths
import CoreMedia
import Encoding
import Files
import Processes
import ScreenCapture
import ScreenCaptureKit
import Subprocess
import Synchronization
import System
import UniformTypeIdentifiers

// --- [CONSTANTS] -----------------------------------------------------------------------

private let framesPerSecond: CMTimeScale = 10

// --- [MODELS] --------------------------------------------------------------------------

struct Moment: Encodable {
    enum Event: CaseKeyed {
        case line(any Encodable)
        case log(LogEvent)
        case host(LogLine)
        case movie(Footage)
        case clip(Footage)
        case frame(Footage)
        case fail(ApplicationFailure)
    }

    private enum CodingKeys: CodingKey {
        case at
    }

    let at: Date
    let event: Event

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(at, forKey: .at)
        try event.encode(to: encoder)
    }
}

struct Footage: Encodable {
    let file: String
    let pid: pid_t
    let window: CGWindowID
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Recorder {
    private struct Held {
        var recordings: [CGWindowID: Result<Recording, ApplicationFailure>] = [:]
        var windows: Set<CGWindowID> = []
        var hung: Bool = false
        let logging: Task<Void, Never>
    }

    private let folder: URL
    private let hostLogs: LogFolder
    private let longestSide: Int?
    private let timeline: Mutex<FileDescriptor>
    private let failed: @Sendable (ApplicationFailure) -> Void
    private var held: [Instance: Held] = [:]
    private var tailing: Task<Void, Never>?
    private var observing: Task<Void, Never>?
    private var open: Bool = true

    init(folder: URL, longestSide: Int?, failed: @escaping @Sendable (ApplicationFailure) -> Void) throws(ApplicationFailure) {
        try checkCaptureAccess(prompting: true)
        let hostLogs = LogFolder(url: folder.appending(component: "host", directoryHint: .isDirectory), glob: "*.log")
        (self.folder, self.hostLogs, self.longestSide, self.failed) = (folder, hostLogs, longestSide, failed)
        do {
            try FileManager.default.createDirectory(at: hostLogs.url, withIntermediateDirectories: true)
            timeline = Mutex(
                try FileDescriptor.open(
                    FilePath(folder.appending(component: "timeline.jsonl").path(percentEncoded: false)),
                    .writeOnly,
                    options: [.append, .create, .closeOnExec],
                    permissions: [.ownerReadWrite, .groupRead, .otherRead],
                )
            )
        } catch {
            throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .host(error as NSError))
        }
    }

    nonisolated func append(_ event: Moment.Event, at: Date = .now) {
        do {
            let line = try JSONEncoder.lines.encodeLine(Moment(at: at, event: event))
            try timeline.withLock { descriptor in _ = try descriptor.writeAll(line) }
        } catch {
            failed(ApplicationFailure(subject: folder.appending(component: "timeline.jsonl").path(percentEncoded: false), reason: .host(error as NSError)))
        }
    }

    func observe(_ events: [InstanceEvent]) {
        observing = Task { [observing] in
            _ = await observing?.value
            await record(events)
        }
    }

    private func record(_ events: [InstanceEvent]) async {
        guard open else { return }
        tailing = tailing ?? Task { await tail() }
        for event in events {
            let instance = event.instance
            switch event.change {
                case .started: held[instance] = held[instance] ?? Held(logging: Task { await logging(instance.process.pid) })
                case .opened(let window):
                    held[instance]?.windows.insert(window.number)
                    if window.subrole == kAXStandardWindowSubrole { await start(window.number, of: instance) }
                case .closed(let window):
                    held[instance]?.windows.remove(window.number)
                    await stop(window.number, of: instance)
                case .asking(let dialog):
                    await clip(instance)
                    await frame([dialog.window], of: instance)
                    if held[instance]?.windows.contains(dialog.window) == true { await start(dialog.window, of: instance) }
                case .status(let status):
                    let hung = status.is(\.unresponsive)
                    if hung, held[instance]?.hung == false {
                        await clip(instance)
                        await frame(held[instance]?.recordings.keys.sorted() ?? [], of: instance)
                    }
                    held[instance]?.hung = hung
                case .exited(let ended):
                    let crashed: Bool =
                        switch ended.exit.map(\.status) {
                            case .success(.signaled): true
                            case .success(.exited), .failure: ended.exit[case: \.success]?.reports == true
                        }
                    if crashed { await clip(instance) }
                    await release(instance)
                case .answered: break
            }
        }
    }

    private func start(_ window: CGWindowID, of instance: Instance) async {
        guard held[instance] != nil, held[instance]?.recordings[window] == nil else { return }
        let movie = footageFile(in: folder, window: window, .quickTimeMovie)
        let configuration = SCStreamConfiguration.background
        configuration.minimumFrameInterval = CMTime(value: 1, timescale: framesPerSecond)
        let started = await Result { () async throws(ApplicationFailure) in
            try await instance.recording(.windows(numbers: [window]), to: movie, longestSide: longestSide, configuration: configuration)
        }
        held[instance]?.recordings[window] = started
        switch started {
            case .success: append(.movie(Footage(file: movie.lastPathComponent, pid: instance.process.pid, window: window)))
            case .failure(let failure): append(.fail(failure))
        }
    }

    private func stop(_ window: CGWindowID, of instance: Instance) async {
        guard case .success(let recording)? = held[instance]?.recordings.removeValue(forKey: window) else { return }
        do throws(ApplicationFailure) { try await recording.finish() } catch { append(.fail(error)) }
    }

    private func clip(_ instance: Instance) async {
        for case (let window, .success(let recording)) in held[instance]?.recordings ?? [:] {
            do throws(ApplicationFailure) {
                let clip = footageFile(in: folder, window: window, .quickTimeMovie)
                try await recording.clip(to: clip)
                append(.clip(Footage(file: clip.lastPathComponent, pid: instance.process.pid, window: window)))
            } catch {
                append(.fail(error))
            }
        }
    }

    private func frame(_ windows: [CGWindowID], of instance: Instance) async {
        guard !windows.isEmpty else { return }
        let frames = await Result { () async throws(ApplicationFailure) in try await instance.capture(.windows(numbers: windows), to: folder, longestSide: longestSide) }
        if case .failure(let failure) = frames { append(.fail(failure)) }
        for (window, shot) in frames[case: \.success] ?? [:] {
            switch shot {
                case .success(let shot): append(.frame(Footage(file: shot.file.lastPathComponent, pid: instance.process.pid, window: window)))
                case .failure(let failure): append(.fail(failure))
            }
        }
    }

    private func release(_ instance: Instance) async {
        guard let left = held.removeValue(forKey: instance) else { return }
        left.logging.cancel()
        for case .success(let recording) in left.recordings.values {
            do throws(ApplicationFailure) { try await recording.finish() } catch { append(.fail(error)) }
        }
        _ = await left.logging.value
    }

    func finish() async {
        open = false
        _ = await observing?.value
        tailing?.cancel()
        for instance in held.keys { await release(instance) }
        _ = await tailing?.value
    }

    private nonisolated func logging(_ pid: pid_t) async {
        for await outcome in LogEvent.stream(of: .process(pid)) {
            switch outcome {
                case .success(let event): append(.log(event), at: event.timestamp)
                case .failure(let failure): append(.fail(ApplicationFailure(subject: failure.subject, reason: .host(failure as NSError))))
            }
        }
    }

    private nonisolated func tail() async {
        for await batch in LogLine.appended(in: hostLogs) {
            for line in batch.lines { append(.host(line)) }
            for failure in batch.failures { append(.fail(ApplicationFailure(subject: failure.subject, reason: .unwatched(failure)))) }
        }
    }
}
