import AppKit
import Applications
import ArgumentParser
import AsyncAlgorithms
import CasePaths
import CurrentProcess
import Encoding
import Files
import OrderedCollections
import Processes
import Synchronization
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let sampling: Duration = .seconds(5)
private let busyLoad = 0.9
private let probeSpan: TimeInterval = 60
private let probeRuns = 3
private let probes: Set<String> = [Read._commandName, Capture._commandName, Find._commandName, ProcessTable._commandName]

// --- [SERVICES] ------------------------------------------------------------------------

final class Session: Sendable {
    private let run: Run
    private let named: @Sendable (NSRunningApplication) -> Bool
    private let adoptions: (stream: AsyncStream<Instance>, continuation: AsyncStream<Instance>.Continuation) = AsyncStream.makeStream()
    private let held =
        Mutex<
            (
                states: OrderedDictionary<Instance, InstanceState>,
                adopted: Set<ProcessInstance>,
                sensing: [Instance: (logs: Task<Void, Never>, logged: Set<[String?]>, load: (usage: ResourceUsage, at: ContinuousClock.Instant, high: Int)?)],
                probed: [[String]: [Date]]
            )
        >(([:], [], [:], [:]))

    init(_ run: Run, named: @escaping @Sendable (NSRunningApplication) -> Bool) { (self.run, self.named) = (run, named) }

    var names: [String] { held.withLock { held in held.states.keys.compactMap(\.application.localizedName) } }

    private func emit(_ instance: Instance, _ event: Event) { run.emit(instance.key, [String(instance.process.pid): Tagged(instance: instance.process, value: event)]) }

    // --- [STATES]

    func observe(_ instance: Instance, _ state: InstanceState?) async {
        let sensed = state?.is(\.running) == true && !instance.application.isDialogHelper
        let (last, next, stopped) = held.withLock { held in
            let last = held.states
            held.states[instance] = state
            let stopped = sensed ? nil : held.sensing.removeValue(forKey: instance)?.logs
            if sensed, held.sensing[instance] == nil {
                held.sensing[instance] = (
                    Task { for await entry in instance.process.logEntries(matching: NSPredicate(format: "messageType == error OR messageType == fault")) { self.logged(instance, entry) } }, [], nil,
                )
            }
            return (last, held.states, stopped)
        }
        stopped?.cancel()
        for change in InstanceState.changes(from: last[instance], to: state) { emit(instance, .change(change)) }
        if let state { run.settle(instance, state) }
        await run.recorder?.observe(next, after: last)
    }

    func follow(_ feed: some AsyncSequence<OrderedDictionary<Instance, InstanceState>, Never> & Sendable) async {
        _ = await feed.reduce(OrderedDictionary<Instance, InstanceState>()) { last, next in
            for (instance, state) in next where last[instance] != state { await observe(instance, state) }
            for instance in last.keys where next[instance] == nil { await observe(instance, nil) }
            return next
        }
    }

    func follow(_ instance: Instance) async {
        for await state in instance.states() { await observe(instance, state) }
        await observe(instance, nil)
    }

    func followAdopted() async {
        await withDiscardingTaskGroup { group in
            for await instance in adoptions.stream { group.addTask { await self.follow(instance) } }
        }
    }

    // --- [JOURNAL]

    func readJournal() async {
        let touched = FileEvent.events(of: journal.deletingLastPathComponent()).map { batch in batch.map { events in events.map(\.url.lastPathComponent).contains(journal.lastPathComponent) } }
        var (offset, probing): (UInt64, Bool) = (0, false)
        for await batch in chain([Result<Bool, FileFailure>.success(true)].async, touched) where batch != .success(false) {
            do {
                _ = try batch.mapError { failure in ApplicationFailure(subject: journal.path(percentEncoded: false), reason: .unwatched(failure)) }.get()
                let handle = try FileHandle(forReadingFrom: journal)
                let start = try handle.seekToEnd() < offset ? 0 : offset
                try handle.seek(toOffset: start)
                let fresh = try handle.readToEnd() ?? Data()
                try handle.close()
                let whole = fresh.dropLast(Array(fresh.reversed()).firstIndex(of: UInt8(ascii: "\n")) ?? fresh.count)
                offset = start + UInt64(whole.count)
                adopt(try Record.journaled(Data(whole)).map(\.record), probing: probing)
            } catch let failure as ApplicationFailure {
                run.fail(Program.current.name, failure)
            } catch {
                run.fail(Program.current.name, ApplicationFailure(subject: journal.path(percentEncoded: false), reason: .host(error as NSError)))
            }
            probing = true
        }
    }

    private func adopt(_ records: [Record<[String: JSONValue]>], probing: Bool) {
        let foreign = records.filter { record in record.pid != getpid() }
        if probing { foreign.forEach(probed) }
        for seen in foreign.flatMap(\.instances) {
            guard let application = NSRunningApplication(processIdentifier: seen.instance.pid), !application.isDialogHelper,
                case .success(let instance) = Result(catching: { () throws(ApplicationFailure) in try Instance(application) }), instance.process == seen.instance
            else { continue }
            if seen.launched?.created == true { run.own(instance) }
            if !named(application), held.withLock({ held in held.adopted.insert(seen.instance).inserted }) { adoptions.continuation.yield(instance) }
        }
    }

    private func probed(_ record: Record<[String: JSONValue]>) {
        guard let argv = record.argv.map({ argv in Array(argv.dropFirst()) }), let command = argv.first, probes.contains(command) else { return }
        let runs = held.withLock { held in
            held.probed[argv] = (held.probed[argv] ?? []).filter { at in record.at.timeIntervalSince(at) < probeSpan } + [record.at]
            return held.probed[argv] ?? []
        }
        if runs.count == probeRuns, let first = runs.first {
            run.emit(Program.current.name, Event.polled(Probe(argv: argv, runs: runs.count, seconds: (record.at.timeIntervalSince(first) * 10).rounded() / 10)))
        }
    }

    // --- [SENSING]

    private func logged(_ instance: Instance, _ entry: Result<LogEntry, ProcessFailure>) {
        switch entry {
            case .success(let entry):
                let fresh = held.withLock { held in held.sensing[instance]?.logged.insert([entry.subsystem, entry.category, entry.eventMessage]).inserted == true }
                if fresh { emit(instance, .logged(entry)) }
            case .failure(let failure): run.fail(instance.key, ApplicationFailure(subject: failure.subject, reason: .host(failure as NSError)))
        }
    }

    func sample(clock: ContinuousClock = ContinuousClock()) async {
        while true {
            do { try await clock.sleep(for: sampling) } catch { return }
            let now = clock.now
            for instance in held.withLock({ held in Array(held.sensing.keys) }) {
                guard case .success(let usage) = Result(catching: { () throws(Errno) in try ResourceUsage(of: instance.process) }) else { continue }
                let last = held.withLock { held in held.sensing[instance]?.load }
                let load = last.map { last in (usage.cpu - last.usage.cpu) / last.at.duration(to: now) }
                let high = (load ?? 0) >= busyLoad ? (last?.high ?? 0) + 1 : 0
                held.withLock { held in held.sensing[instance]?.load = (usage, now, high) }
                switch (load, last?.high ?? 0, high) {
                    case (.some(let load), 1, 2): emit(instance, .busy(usage, load: load))
                    case (.some(let load), 2..., 0): emit(instance, .idle(load: load))
                    case (.some, _, _), (.none, _, _): break
                }
            }
        }
    }
}
