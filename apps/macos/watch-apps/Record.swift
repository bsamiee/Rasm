import AppKit
import Applications
import ArgumentParser
import CasePaths
import CoreMedia
import Encoding
import Files
import OrderedCollections
import Processes
import ScreenCaptureKit
import Subprocess
import System
import UniformTypeIdentifiers

// --- [CONSTANTS] -----------------------------------------------------------------------

private let framesPerSecond: CMTimeScale = 10
private let logFields: Set<String> = ["processID", "messageType", "subsystem", "category", "eventMessage", "senderImagePath"]
private let logTime: Date.ISO8601FormatStyle = .init(includingFractionalSeconds: true).year().month().day().dateTimeSeparator(.space).time(includingFractionalSeconds: true)
    .timeZone(separator: .omitted)
private let fileTime: Date.ISO8601FormatStyle = .init(includingFractionalSeconds: true).dateSeparator(.omitted).timeSeparator(.omitted)

// --- [MODELS] --------------------------------------------------------------------------

struct Moment: Encodable {
    enum Key: CodingKey {
        case at, line, log, host, movie, clip, frame, fail
    }

    let at: Date
    let key: Key
    let value: any Encodable

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(at, forKey: .at)
        try fields.encode(value, forKey: key)
    }
}

struct Footage: Encodable {
    let file: String
    let pid: pid_t
    let window: CGWindowID
}

struct HostLine: Encodable {
    let file: String
    let text: String
}

// --- [SERVICES] ------------------------------------------------------------------------

actor Recorder {
    private let folder: URL
    private let hostLogs: URL
    private let longestSide: Int?
    private let timeline: FileDescriptor
    private var held: [Instance: (recordings: [CGWindowID: Result<WindowRecording, ApplicationFailure>], logging: Task<Void, Never>)] = [:]
    private var tailing: Task<Void, Never>?
    private var observing: Task<Void, Never>?
    private var open: Bool = true

    init(folder: URL, longestSide: Int?) throws(ApplicationFailure) {
        guard CGPreflightScreenCaptureAccess() else { throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .noCaptureAccess) }
        (self.folder, hostLogs, self.longestSide) = (folder, folder.appending(component: "host", directoryHint: .isDirectory), longestSide)
        do {
            try FileManager.default.createDirectory(at: hostLogs, withIntermediateDirectories: true)
            timeline = try FileDescriptor.open(
                FilePath(folder.appending(component: "timeline.jsonl").path(percentEncoded: false)),
                .writeOnly,
                options: [.append, .create, .closeOnExec],
                permissions: [.ownerReadWrite, .groupRead, .otherRead],
            )
        } catch {
            throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .host(error as NSError))
        }
    }

    nonisolated func append(_ key: Moment.Key, _ value: any Encodable, at: Date = .now) {
        do { try timeline.writeAll(encoder.encodeLine(Moment(at: at, key: key, value: value))) } catch { WatchApps.exit(withError: error) }
    }

    private func file(_ window: CGWindowID, _ type: UTType) -> URL {
        folder.appending(component: "\(window)-\(fileTime.format(.now))").appendingPathExtension(for: type)
    }

    func observe(_ states: OrderedDictionary<Instance, InstanceState>, after last: OrderedDictionary<Instance, InstanceState>) {
        observing = Task { [observing] in
            _ = await observing?.value
            await record(states, after: last)
        }
    }

    private func record(_ states: OrderedDictionary<Instance, InstanceState>, after last: OrderedDictionary<Instance, InstanceState>) async {
        guard open else { return }
        tailing = tailing ?? Task { await tail() }
        for (instance, state) in states {
            let (pid, was, now) = (instance.process.pid, last[instance]?[case: \.running], state[case: \.running])
            var recordings = held[instance]?.recordings ?? [:]
            let asked = Set(now?.asking.map(\.number) ?? []).subtracting(was?.asking.map(\.number) ?? [])
            let hung = now?.status.is(\.unresponsive) == true && was?.status.is(\.unresponsive) != true
            let exit = last[instance]?.is(\.exited) == true ? nil : state[case: \.exited]?.0[case: \.success]
            let crashed = exit.map { exit in if case .signaled = exit.status { true } else { exit.reports } } ?? false
            let clipped = !asked.isEmpty || hung || crashed ? recordings.compactMapValues { recording in recording[case: \.success] } : [:]
            for (window, recording) in clipped {
                do throws(ApplicationFailure) {
                    let clip = file(window, .quickTimeMovie)
                    try await recording.clip(to: clip)
                    append(.clip, Footage(file: clip.lastPathComponent, pid: pid, window: window))
                } catch {
                    append(.fail, error)
                }
            }
            let shown = (now?.windows ?? []).filter { window in window.asking || window.description[case: \.element]?.subrole == kAXStandardWindowSubrole }
            let windows = Set(shown.map(\.number))
            let framed = hung ? windows : asked.intersection(windows)
            let shots = await Result { () async throws(ApplicationFailure) in
                try await framed.isEmpty ? [:] : instance.screenshots(of: Array(framed), longestSide: longestSide)
            }
            if case .failure(let failure) = shots { append(.fail, failure) }
            for (window, shot) in shots[case: \.success] ?? [:] {
                do throws(ApplicationFailure) {
                    let frame = file(window, .png)
                    try shot.get().write(to: frame)
                    append(.frame, Footage(file: frame.lastPathComponent, pid: pid, window: window))
                } catch {
                    append(.fail, error)
                }
            }
            for window in Set(recordings.keys).subtracting(windows) {
                do throws(ApplicationFailure) { try await recordings.removeValue(forKey: window)?[case: \.success]?.finish() } catch { append(.fail, error) }
            }
            for window in windows.subtracting(recordings.keys) {
                let movie = file(window, .quickTimeMovie)
                let started = await Result { () async throws(ApplicationFailure) in
                    try await instance.recording(of: window, to: movie, longestSide: longestSide) {
                        let configuration = SCStreamConfiguration.background
                        configuration.minimumFrameInterval = CMTime(value: 1, timescale: framesPerSecond)
                        return configuration
                    }
                }
                recordings[window] = started
                switch started {
                    case .success: append(.movie, Footage(file: movie.lastPathComponent, pid: pid, window: window))
                    case .failure(let failure): append(.fail, failure)
                }
            }
            held[instance] = (recordings, held[instance]?.logging ?? Task { await logging(pid) })
        }
        for instance in last.keys where states[instance] == nil { await release(instance) }
    }

    private func release(_ instance: Instance) async {
        guard let left = held.removeValue(forKey: instance) else { return }
        left.logging.cancel()
        for case .success(let recording) in left.recordings.values {
            do throws(ApplicationFailure) { try await recording.finish() } catch { append(.fail, error) }
        }
        _ = await left.logging.value
    }

    func finish() async {
        open = false
        _ = await observing?.value
        tailing?.cancel()
        for instance in held.keys { await release(instance) }
    }

    private nonisolated func logging(_ pid: pid_t) async {
        let decoder = JSONDecoder()
        do {
            let stream = Configuration(executable: .name("log"), arguments: ["stream", "--process", String(pid), "--style", "ndjson"], platformOptions: .session())
            _ = try await Subprocess.run(stream, input: .none, output: .sequence, error: .discarded) { execution in
                for try await line in execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"])) where line.hasPrefix("{") {
                    let event = try decoder.decode([String: JSONValue].self, from: Data(line.utf8))
                    guard case .string(let stamp)? = event["timestamp"] else { continue }
                    append(.log, event.filter { field in logFields.contains(field.key) }, at: try Date(stamp, strategy: logTime))
                }
            }
        } catch  where !Task.isCancelled {
            append(.fail, ApplicationFailure(subject: "log stream --process \(pid)", reason: .host(error as NSError)))
        } catch {}
    }

    private nonisolated func tail() async {
        let present = Result {
            try FileManager.default.contentsOfDirectory(at: hostLogs, includingPropertiesForKeys: [.fileSizeKey]).map { file in
                (file.lastPathComponent, try file.resourceValues(forKeys: [.fileSizeKey]).fileSize ?? 0)
            }
        }
        if case .failure(let error) = present { append(.fail, ApplicationFailure(subject: hostLogs.path(percentEncoded: false), reason: .host(error as NSError))) }
        var read = Dictionary(present[case: \.success] ?? []) { _, size in size }
        for await batch in FileEvent.events(of: hostLogs) {
            if case .failure(let failure) = batch { append(.fail, ApplicationFailure(subject: hostLogs.path(percentEncoded: false), reason: .unwatched(failure))) }
            for file in Set(batch[case: \.success]?.map(\.url) ?? []) where file.pathExtension == "log" {
                let name = file.lastPathComponent
                do {
                    guard let text = try ifPresent({ try Data(contentsOf: file, options: .mappedIfSafe) }) else {
                        read[name] = nil
                        continue
                    }
                    let offset = read[name] ?? 0
                    let start = offset > text.count ? 0 : offset
                    let fresh = text.dropFirst(start)
                    let whole = fresh.lastIndex(of: UInt8(ascii: "\n")).map { end in fresh[...end] } ?? Data()
                    read[name] = start + whole.count
                    guard let lines = String(bytes: whole, encoding: .utf8) else { throw CocoaError(.fileReadInapplicableStringEncoding, userInfo: [NSURLErrorKey: file]) }
                    for line in lines.split(whereSeparator: \.isNewline) { append(.host, HostLine(file: name, text: String(line))) }
                } catch {
                    append(.fail, ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error as NSError)))
                }
            }
        }
    }
}
