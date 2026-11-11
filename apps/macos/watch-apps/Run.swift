import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import CasePaths
import CurrentProcess
import Encoding
import Files
import Logging
import OrderedCollections
import Processes
import ServiceLifecycle
import Subprocess
import Synchronization
import System
import Telemetry
import Tracing
import UnixSignals

// --- [CONSTANTS] -----------------------------------------------------------------------

let runLimit: (bytes: Int, files: Int) = (1 << 20, 64)

// --- [SERVICES] ------------------------------------------------------------------------

struct Journal: Sendable {
    static let folder: URL = Program.current.directory(.log).appending(path: Program.current.name, directoryHint: .isDirectory)

    private let file: URL
    private let start: Data
    private var descriptor: FileDescriptor
    private var bytes: Int
    private var threshold: Int = runLimit.bytes
    private var latest: OrderedDictionary<String, Data> = [:]

    init(start: Data) throws {
        file = Self.folder.appending(component: [callerSession, String(getpid())].compactMap(\.self).joined(separator: ".")).appendingPathExtension("jsonl")
        try FileManager.default.createDirectory(at: Self.folder, withIntermediateDirectories: true, attributes: [.posixPermissions: 0o700])
        for old in try Self.files().dropLast(runLimit.files - 1) { _ = try ifPresent { try FileManager.default.removeItem(at: old) } }
        descriptor = try FileDescriptor.open(FilePath(file.path(percentEncoded: false)), .writeOnly, options: [.append, .create, .truncate], permissions: .ownerReadWrite)
        try descriptor.writeAll(start)
        (self.start, bytes) = (start, start.count)
    }

    static func files() throws -> [URL] {
        let listed = try ifPresent { try FileManager.default.contentsOfDirectory(at: folder, includingPropertiesForKeys: [.contentModificationDateKey]) } ?? []
        let dated: [(file: URL, modified: Date)] = try listed.map { file in
            let values = try ifPresent { try file.resourceValues(forKeys: [.contentModificationDateKey]) }
            return (file, values?.contentModificationDate ?? .distantPast)
        }
        return dated.sorted(using: KeyPathComparator(\.modified)).map(\.file)
    }

    mutating func append(_ record: Data, replacing key: String? = nil) throws {
        if let key { latest[key] = record }
        guard bytes + record.count > threshold else {
            try descriptor.writeAll(record)
            bytes += record.count
            return
        }
        let compacted = latest.values.reduce(into: start) { content, line in content += line } + (key == nil ? record : Data())
        try compacted.write(to: file, options: .atomic)
        try descriptor.close()
        descriptor = try FileDescriptor.open(FilePath(file.path(percentEncoded: false)), .writeOnly, options: .append)
        (bytes, threshold) = (compacted.count, max(runLimit.bytes, 2 * compacted.count))
    }
}

final class Run: Sendable {
    private let output: FileDescriptor?
    private let lineLength: Int?
    private let telemetry: ServiceGroup = Program.current.telemetry(scopes: [Run.self])
    private let services: Task<Void, any Error>
    private let logger: Logger
    private let held: Mutex<(status: ExitCode, shown: [pid_t: Data], journal: Journal?)>
    private let closed: (stream: AsyncStream<Int32?>, continuation: AsyncStream<Int32?>.Continuation) = AsyncStream.makeStream()
    private let claims: (stream: AsyncStream<NSRunningApplication?>, continuation: AsyncStream<NSRunningApplication?>.Continuation)?

    init(output: FileDescriptor? = .standardOutput, lineLength: Int? = nil, waiting: Bool = false) {
        let opening = Result { () throws(ApplicationFailure) in
            do { return try Journal(start: encoder.encodeLine(Record<JSONValue>(argv: CommandLine.arguments))) } catch {
                throw ApplicationFailure(subject: Journal.folder.path(percentEncoded: false), reason: .host(error as NSError))
            }
        }
        self.output = output
        self.lineLength = lineLength
        services = Task(name: "Telemetry") { [telemetry] in try await telemetry.run() }
        logger = Logger(label: String(reflecting: Run.self))
        held = Mutex((waiting ? .failure : .success, [:], opening[case: \.success]))
        claims = waiting ? AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1)) : nil
        if case .failure(let failure) = opening { fail(Program.current.name, failure) }
    }

    private func encoded(_ value: some Encodable) -> Data {
        do { return try encoder.encodeLine(value) } catch { WatchApps.exit(withError: error) }
    }

    private func emit(_ line: Data, logging shown: some Encodable, as key: String) {
        let record = encoded(Record(line: shown))
        let open = held.withLock { held in
            do {
                try held.journal?.append(record, replacing: key)
                try output?.writeAll(line)
                return true
            } catch Errno.brokenPipe {
                return false
            } catch {
                WatchApps.exit(withError: error)
            }
        }
        if !open { closed.continuation.yield(nil) }
    }

    func emit(_ key: String, _ value: some Encodable) { emit(encoded([key: value]), logging: [key: value], as: key) }

    func fail(_ key: String, _ failure: ApplicationFailure) {
        fail()
        switch failure.reason {
            case .event, .system, .host, .unwatched, .unreadable, .noEncoder, .noImage:
                logger.error("Command failed in host", error: failure, metadata: ["key": "\(key, privacy: .public)"])
            case .untrusted, .noCaptureAccess, .unanswered, .noProcess, .unresolved, .foreign, .disabled, .notSettable, .refused, .noIdentifier, .noPath, .noHandler, .noWindow, .noExtension,
                .noReport,
                .cancelled, .unmatched, .unnamed, .malformed:
                logger.warning("Command refused input", error: failure, metadata: ["key": "\(key, privacy: .public)"])
        }
        emit(key, Event.fail(failure))
    }

    func fail() { held.withLock { held in if held.status.isSuccess { held.status = .failure } } }

    func set(_ status: ExitCode) { held.withLock { held in held.status = status } }

    func show(_ instance: NSRunningApplication, _ state: InstanceState, whole: Bool = false) {
        let (key, pid) = (instance.key, String(instance.processIdentifier))
        let shown: any Encodable
        if case .running(let running) = state, !whole {
            let outline = Outline(
                active: running.active,
                asking: running.asking.map { window in
                    let element = window.description[case: \.element]
                    return Outline.Row(
                        kind: window.description,
                        number: window.number,
                        options: element?.answers?.buttons.compactMap(\.label) ?? [],
                        text: element?.detail?.text ?? [],
                        title: element?.title ?? window.title,
                    )
                },
                menus: running.menus.filter { menu in menu.role == kAXMenuRole }.compactMap(\.title),
                process: running.status[case: \.unresponsive],
                status: running.status,
                windows: running.windows.filter { window in window.presented && !window.asking }.map(\.number),
            )
            let fitted = lineLength.map { limit in outline.fitted(within: limit) { outline in encoded([key: [pid: Event.running(outline)]]).count } } ?? outline
            shown = [key: [pid: Event.running(fitted)]]
        } else {
            shown = [key: [pid: state]]
        }
        let line = encoded(shown)
        guard held.withLock({ held in held.shown.updateValue(line, forKey: instance.processIdentifier) }) != line else { return }
        emit(line, logging: shown, as: key)
    }

    func show(_ step: Launching, as key: String) {
        switch step {
            case .launched(let launched): emit(key, [String(launched.pid): Event.launched(created: launched.created)])
            case .state(let row): show(row.instance, row.state)
            case .prompts(let rows): for row: RunningInstance in rows { show(row.instance, row.state) }
            case .failed(let failure): fail(key, failure)
        }
    }

    func follow(_ feed: some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable) async {
        _ = await feed.reduce(OrderedDictionary<NSRunningApplication, InstanceState>()) { last, next in
            for (instance, state) in next { show(instance, state) }
            for (instance, state) in last where next[instance] == nil && !state.is(\.exited) { emit(instance.key, [String: String]()) }
            return next
        }
    }

    func follow(launching steps: some AsyncSequence<Launching, Never> & Sendable, as key: String) async {
        for await step in steps {
            show(step, as: key)
            switch step {
                case .launched(let launched): claims?.continuation.yield(launched.created ? NSRunningApplication(processIdentifier: launched.pid) : nil)
                case .failed: claims?.continuation.yield(nil)
                case .state(let row) where claims != nil: settle(row.state)
                case .state, .prompts: break
            }
        }
    }

    func recording(_ steps: some AsyncSequence<any Encodable & Sendable, Never> & Sendable) -> some AsyncSequence<any Encodable & Sendable, Never> & Sendable {
        steps.map { step in
            if case .launched(let launched)? = step as? Launching {
                self.show(.launched(launched), as: NSRunningApplication(processIdentifier: launched.pid)?.key ?? String(launched.pid))
            }
            return step
        }
    }

    func settle(_ state: InstanceState) {
        guard case .exited(let ended, _) = state else { return }
        claims?.continuation.yield(nil)
        switch ended.map(\.status) {
            case .success(.exited(let code)): set(ExitCode(code))
            case .success(.signaled(let signal)): set(ExitCode(128 + signal))
            case .failure: break
        }
    }

    func quit(_ instance: NSRunningApplication, _ steps: some AsyncSequence<Result<InstanceState, ApplicationFailure>, Never> & Sendable) async -> InstanceState? {
        await steps.reduce(InstanceState?.none) { last, step in
            switch step {
                case .success(let state):
                    show(instance, state)
                    return state
                case .failure(let failure):
                    fail(instance.key, failure)
                    return last
            }
        }
    }

    func release() async {
        guard case .some(.some(let instance)) = await claims?.stream.first(where: { _ in true }) else { return }
        let saved = await quit(instance, instance.quit(saving: true))
        let ended = saved?.is(\.exited) == true ? saved : await quit(instance, instance.quit(force: true))
        if let ended { settle(ended) }
    }

    func finish(signal: Int32? = nil) async -> Never {
        let status = held.withLock { held in
            let ended = encoded(Record<JSONValue>(exit: Exit(status: held.status.rawValue, signal: signal)))
            do { try held.journal?.append(ended) } catch { WatchApps.exit(withError: error) }
            return held.status
        }
        await telemetry.triggerGracefulShutdown()
        _ = await services.result
        WatchApps.exit(withError: status)
    }

    static func execute(
        _ command: (some ParsableCommand).Type,
        output: FileDescriptor? = .standardOutput,
        lineLength: Int? = nil,
        accessibility: Bool = true,
        waiting: Bool = false,
        _ work: @escaping @Sendable (Run) async throws(ApplicationFailure) -> Void,
    ) async {
        let run = Run(output: output, lineLength: lineLength, waiting: waiting)
        let operation = "\(Program.current.name) \(command._commandName)"
        let signals = await UnixSignalsSequence(trapping: .sigterm, .sigint, .sighup).map { signal in Int32?.some(signal.rawValue) }
        let parent = termination(of: getppid()).compactMap { exit in exit[case: \.success].map { _ in Int32?.none } }
        let endings = merge(signals, parent, run.closed.stream)
        await withTaskGroup(of: Int32??.self) { group in
            group.addTask {
                var context = InstrumentationContext.topLevel
                InstrumentationSystem.instrument.extract(ProcessInfo.processInfo.environment, into: &context, using: EnvironmentCarrier())
                await withSpan(operation, context: context) { span in
                    do throws(ApplicationFailure) {
                        if accessibility { try checkAccessibilityTrust() }
                        try await work(run)
                    } catch {
                        span.recordError(error)
                        span.setStatus(SpanStatus(code: .error))
                        run.fail(Program.current.name, error)
                    }
                }
                return nil
            }
            group.addTask { await endings.first { _ in true } }
            switch await group.next() {
                case .some(.some(let ending)):
                    await run.release()
                    await run.finish(signal: ending)
                case .some(.none), .none: await run.finish()
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    var key: String { bundleIdentifier ?? String(processIdentifier) }
}
