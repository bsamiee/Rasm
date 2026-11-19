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

// --- [SERVICES] ------------------------------------------------------------------------

final class Run: Sendable {
    private let lineLength: Int?
    private let telemetry: ServiceGroup = Program.current.telemetry(scopes: [Run.self])
    private let services: Task<Void, any Error>
    private let logger: Logger
    private let records = RotatingFile(journal)
    private let held: Mutex<(status: ExitCode, shown: [ProcessInstance: Data], journaling: Bool)>
    private let closed: (stream: AsyncStream<Int32?>, continuation: AsyncStream<Int32?>.Continuation) = AsyncStream.makeStream()
    private let failures: (stream: AsyncStream<ApplicationFailure>, continuation: AsyncStream<ApplicationFailure>.Continuation)
    private let claims: (stream: AsyncStream<Instance?>, continuation: AsyncStream<Instance?>.Continuation)?
    private let recorder: Recorder?

    init(lineLength: Int? = nil, waiting: Bool = false, recording: (folder: URL, longestSide: Int?)? = nil) {
        let failures = AsyncStream<ApplicationFailure>.makeStream()
        let failed = failures.continuation
        let recorded = Result { () throws(ApplicationFailure) in
            try recording.map { target throws(ApplicationFailure) in try Recorder(folder: target.folder, longestSide: target.longestSide) { failure in failed.yield(failure) } }
        }
        (self.lineLength, self.failures) = (lineLength, failures)
        services = Task(name: "Telemetry") { [telemetry] in try await telemetry.run() }
        logger = Logger(label: String(reflecting: Run.self))
        held = Mutex((waiting ? .failure : .success, [:], true))
        claims = waiting ? AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1)) : nil
        recorder = recorded[case: \.success]
        journaled(encoded(Record<JSONValue>(argv: CommandLine.arguments)))
        if case .failure(let failure) = recorded { fail(Program.current.name, failure) }
    }

    private func encoded(_ value: some Encodable) -> Data {
        do { return try JSONEncoder.lines.encodeLine(value) } catch { WatchApp.exit(withError: error) }
    }

    private func journaled(_ record: Data) {
        do throws(FileFailure) { try records.append(record) } catch {
            let first = held.withLock { held in
                defer { held.journaling = false }
                return held.journaling
            }
            if first { fail(Program.current.name, ApplicationFailure(subject: records.url.path(percentEncoded: false), reason: .host(error as NSError))) }
        }
    }

    private func emit(_ line: Data, logging shown: some Encodable) {
        recorder?.append(.line(shown))
        journaled(encoded(Record(line: shown)))
        let open = held.withLock { _ in
            do {
                try FileDescriptor.standardOutput.writeAll(line)
                return true
            } catch Errno.brokenPipe {
                return false
            } catch {
                WatchApp.exit(withError: error)
            }
        }
        if !open { closed.continuation.yield(nil) }
    }

    func emit(_ key: String, _ value: some Encodable) { emit(encoded([key: value]), logging: [key: value]) }

    func fail(_ key: String, _ failure: ApplicationFailure) {
        fail()
        switch failure.reason {
            case .event, .system, .host, .unwatched, .unreadable, .noEncoder, .noImage:
                logger.error("Command failed in host", error: failure, metadata: ["key": "\(key, privacy: .public)"])
            case .noGrant, .unanswered, .noProcess, .unresolved, .foreign, .disabled, .unchanged, .refused, .noIdentifier, .noPath, .noHandler, .noWindow, .noExtension,
                .cancelled, .unmatched, .unnamed, .malformed:
                logger.warning("Command refused input", error: failure, metadata: ["key": "\(key, privacy: .public)"])
        }
        emit(key, Event.fail(failure))
    }

    func fail() { held.withLock { held in if held.status.isSuccess { held.status = .failure } } }

    func set(_ status: ExitCode) { held.withLock { held in held.status = status } }

    func show(_ instance: Instance, _ state: InstanceState, whole: Bool = false) {
        let (key, pid, process) = (instance.key, String(instance.process.pid), instance.process)
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
            let fitted = lineLength.map { limit in outline.fitted(within: limit) { outline in encoded([key: [pid: Tagged(instance: process, value: Event.running(outline))]]).count } } ?? outline
            shown = [key: [pid: Tagged(instance: process, value: Event.running(fitted))]]
        } else {
            shown = [key: [pid: Tagged(instance: process, value: state)]]
        }
        let line = encoded(shown)
        guard held.withLock({ held in held.shown.updateValue(line, forKey: process) }) != line else { return }
        emit(line, logging: shown)
    }

    func show(_ step: Launching, as key: String) {
        switch step {
            case .launched(let launched): emit(key, [String(launched.instance.process.pid): Tagged(instance: launched.instance.process, value: Event.launched(launched))])
            case .state(let row): show(row.instance, row.state)
            case .prompts(let rows): for row: RunningInstance in rows { show(row.instance, row.state) }
            case .failed(let failure): fail(key, failure)
        }
    }

    func follow(_ feed: some AsyncSequence<OrderedDictionary<Instance, InstanceState>, Never> & Sendable) async {
        _ = await feed.reduce(OrderedDictionary<Instance, InstanceState>()) { last, next in
            for (instance, state) in next { show(instance, state) }
            for (instance, state) in last where next[instance] == nil {
                held.withLock { held in held.shown[instance.process] = nil }
                if !state.is(\.exited) { emit(instance.key, [String: String]()) }
            }
            await recorder?.observe(InstanceEvent.changes(from: last, to: next, at: .now))
            return next
        }
    }

    func follow(launching steps: some AsyncSequence<Launching, Never> & Sendable, as key: String) async {
        for await step in steps {
            show(step, as: key)
            switch step {
                case .launched(let launched): claims?.continuation.yield(launched.created ? launched.instance : nil)
                case .failed: claims?.continuation.yield(nil)
                case .state(let row) where claims != nil: settle(row.state)
                case .state, .prompts: break
            }
        }
    }

    func settle(_ state: InstanceState) {
        guard case .exited(let ended) = state else { return }
        claims?.continuation.yield(nil)
        switch ended.exit.map(\.status) {
            case .success(.exited(let code)): set(ExitCode(code))
            case .success(.signaled(let signal)): set(ExitCode(128 + signal))
            case .failure: break
        }
    }

    func quit(_ instance: Instance, _ steps: some AsyncSequence<Result<InstanceState, ApplicationFailure>, Never> & Sendable) async -> InstanceState? {
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
        let saved = await quit(instance, instance.quit(.saving))
        let ended = saved?.is(\.exited) == true ? saved : await quit(instance, instance.quit(.discarding))
        if let ended { settle(ended) }
    }

    func restart(into executable: URL) async -> Never {
        await recorder?.finish()
        await telemetry.triggerGracefulShutdown()
        _ = await services.result
        _ = unsafe execv(executable.path(percentEncoded: false), CommandLine.unsafeArgv)
        fail(Program.current.name, ApplicationFailure(subject: executable.path(percentEncoded: false), reason: .system(Errno(rawValue: errno))))
        await finish()
    }

    func finish(signal: Int32? = nil, draining: Bool = true) async -> Never {
        if draining { await recorder?.finish() }
        let status = held.withLock { held in held.status }
        journaled(encoded(Record<JSONValue>(exit: Exit(status: status.rawValue, signal: signal))))
        if draining {
            await telemetry.triggerGracefulShutdown()
            _ = await services.result
        }
        WatchApp.exit(withError: status)
    }

    static func execute(
        _ command: (some ParsableCommand).Type,
        lineLength: Int? = nil,
        grants: [Grant] = [.accessibility],
        waiting: Bool = false,
        recording: (folder: URL, longestSide: Int?)? = nil,
        _ work: @escaping @Sendable (Run) async throws(ApplicationFailure) -> Void,
    ) async {
        let run = Run(lineLength: lineLength, waiting: waiting, recording: recording)
        let operation = "\(Program.current.name) \(command._commandName)"
        let (signals, signaled) = AsyncStream<Int32?>.makeStream()
        Task {
            var first = true
            for await signal in await UnixSignalsSequence(trapping: .sigterm, .sigint, .sighup) {
                if first { signaled.yield(signal.rawValue) } else { await run.finish(signal: signal.rawValue, draining: false) }
                first = false
            }
        }
        let parent = Result { () throws(Errno) in try ProcessInstance(of: getppid()) }
        let orphaned = (parent[case: \.success]?.termination() ?? AsyncStream { continuation in continuation.finish() }).compactMap { exit in exit[case: \.success].map { _ in Int32?.none } }
        let endings = merge(signals, orphaned, run.closed.stream)
        await withTaskGroup(of: Int32??.self) { group in
            group.addTask {
                var context = InstrumentationContext.topLevel
                InstrumentationSystem.instrument.extract(ProcessInfo.processInfo.environment, into: &context, using: EnvironmentCarrier())
                await withSpan(operation, context: context) { span in
                    do throws(ApplicationFailure) {
                        try Program.current.require(grants)
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
            group.addTask {
                for await failure in run.failures.stream { run.fail(Program.current.name, failure) }
                return nil
            }
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

extension Instance {
    var key: String { application.bundleIdentifier ?? String(process.pid) }
}
