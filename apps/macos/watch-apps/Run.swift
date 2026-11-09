import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import CasePaths
import CurrentProcess
import Encoding
import OrderedCollections
import Processes
import Subprocess
import Synchronization
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

let logVariable: String = "WATCH_APPS_LOG"
let logFile: URL =
    ProcessInfo.processInfo.environment[logVariable].map { path in URL(filePath: path) }
    ?? .libraryDirectory.appending(components: "Logs", program, "\(program).log", directoryHint: .notDirectory)

// --- [SERVICES] ------------------------------------------------------------------------

final class Run: Sendable {
    private let output: FileDescriptor?
    private let log: FileDescriptor?
    private let lineLength: Int?
    private let held: Mutex<(status: ExitCode, shown: [pid_t: Data])>
    private let closed: (stream: AsyncStream<Int32?>, continuation: AsyncStream<Int32?>.Continuation) = AsyncStream.makeStream()
    private let claims: (stream: AsyncStream<NSRunningApplication?>, continuation: AsyncStream<NSRunningApplication?>.Continuation)?

    init(output: FileDescriptor? = .standardOutput, lineLength: Int? = nil, waiting: Bool = false) {
        let opening = Result { () throws(ApplicationFailure) in
            do {
                try FileManager.default.createDirectory(at: logFile.deletingLastPathComponent(), withIntermediateDirectories: true)
                return try FileDescriptor.open(FilePath(logFile.path(percentEncoded: false)), .writeOnly, options: [.append, .create], permissions: [.ownerReadWrite, .groupRead, .otherRead])
            } catch {
                throw ApplicationFailure(subject: logFile.path(percentEncoded: false), reason: .host(error as NSError))
            }
        }
        self.output = output
        self.lineLength = lineLength
        log = opening[case: \.success]
        held = Mutex((waiting ? .failure : .success, [:]))
        claims = waiting ? AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1)) : nil
        append(Record<JSONValue>(argv: CommandLine.arguments))
        if case .failure(let failure) = opening { fail(program, failure) }
    }

    private func encoded(_ value: some Encodable) -> Data {
        do { return try encoder.encodeLine(value) } catch { WatchApps.exit(withError: error) }
    }

    private func append(_ record: Record<some Encodable>) {
        do { try log?.writeAll(encoded(record)) } catch { WatchApps.exit(withError: error) }
    }

    private func write(_ line: Data, logging whole: some Encodable) {
        let open: Bool = held.withLock { _ in
            append(Record(line: whole))
            do {
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

    func emit(_ key: String, _ value: some Encodable) { write(encoded([key: value]), logging: [key: value]) }

    func fail(_ key: String, _ failure: ApplicationFailure) {
        fail()
        emit(key, Event.fail(failure))
    }

    func fail() { held.withLock { held in if held.status.isSuccess { held.status = .failure } } }

    func set(_ status: ExitCode) { held.withLock { held in held.status = status } }

    func show(_ instance: NSRunningApplication, _ state: InstanceState, whole: Bool = false) {
        let (key, pid): (String, String) = (instance.key, String(instance.processIdentifier))
        let full: [String: [String: InstanceState]] = [key: [pid: state]]
        let line: Data
        if case .running(let running) = state, !whole {
            let outline = Outline(
                active: running.active,
                asking: running.asking.map { window in
                    let element: Element? = window.description[case: \.element]
                    return Outline.Row(
                        kind: window.description,
                        number: window.number,
                        options: element?.answers?.buttons.compactMap(\.label) ?? [],
                        text: element?.detail?.text ?? [],
                        title: element?.title ?? window.title,
                    )
                },
                menus: running.menus.filter { menu in menu.role == kAXMenuRole }.compactMap(\.title),
                process: running.status[case: \.unresponsive] == .stopped ? .stopped : nil,
                status: running.status,
                windows: running.windows.filter { window in window.presented && !window.asking }.map(\.number),
            )
            let fitted: Outline = lineLength.map { limit in outline.fitted(within: limit) { outline in encoded([key: [pid: Event.running(outline)]]).count } } ?? outline
            line = encoded([key: [pid: Event.running(fitted)]])
        } else {
            line = encoded(full)
        }
        guard held.withLock({ held in held.shown.updateValue(line, forKey: instance.processIdentifier) }) != line else { return }
        write(line, logging: full)
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
            for (instance, state): (NSRunningApplication, InstanceState) in next { show(instance, state) }
            for (instance, state): (NSRunningApplication, InstanceState) in last where next[instance] == nil && !state.is(\.exited) { emit(instance.key, [String: String]()) }
            return next
        }
    }

    func follow(launching steps: some AsyncSequence<Launching, Never> & Sendable, as key: String) async {
        for await step: Launching in steps {
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
        let saved: InstanceState? = await quit(instance, instance.quit(saving: true))
        let ended: InstanceState? = saved?.is(\.exited) == true ? saved : await quit(instance, instance.quitting(force: true))
        if let ended { settle(ended) }
    }

    func finish(signal: Int32? = nil) -> Never {
        let status: ExitCode = held.withLock { held in held.status }
        append(Record<JSONValue>(exit: Exit(status: status.rawValue, signal: signal)))
        WatchApps.exit(withError: status)
    }

    static func execute(
        output: FileDescriptor? = .standardOutput,
        lineLength: Int? = nil,
        accessibility: Bool = true,
        waiting: Bool = false,
        _ work: @escaping @Sendable (Run) async throws(ApplicationFailure) -> Void,
    ) async {
        let run = Run(output: output, lineLength: lineLength, waiting: waiting)
        let endings: AsyncMerge2Sequence<AsyncStream<Int32?>, AsyncStream<Int32?>> = merge(processEndings(), run.closed.stream)
        await withTaskGroup(of: Int32??.self) { group in
            group.addTask {
                do throws(ApplicationFailure) {
                    if accessibility { try checkAccessibilityTrust() }
                    try await work(run)
                } catch {
                    run.fail(program, error)
                }
                return nil
            }
            group.addTask { await endings.first { _ in true } }
            switch await group.next() {
                case .some(.some(let ending)):
                    await run.release()
                    run.finish(signal: ending)
                case .some(.none), .none: run.finish()
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    var key: String { bundleIdentifier ?? String(processIdentifier) }
}
