import CasePaths
import CustomDump
import Darwin
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [MODELS] --------------------------------------------------------------------------

private struct Reaction {
    let stop: Duration
    let resume: Duration
    let window: Duration
    let hang: Duration
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func reacting(
    to fixture: Fixture,
    pid: Int32,
    cycles: some Collection<Int>,
    lines: inout AsyncStream<TimedLine<WatchAppsLine>>.Iterator,
) async throws -> [Reaction] {
    guard let cycle: Int = cycles.first else { return [] }
    let stopping: ContinuousClock.Instant = .now
    expectNoDifference(kill(pid, SIGSTOP), 0)
    let stopped: TimedLine<WatchAppsLine> = try await lines.next { line in line.entry.instances?[pid]?.phase == .unresponsive(.stopped) }
    let resuming: ContinuousClock.Instant = .now
    expectNoDifference(kill(pid, SIGCONT), 0)
    let resumed: TimedLine<WatchAppsLine> = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
    let item: WatchAppsLine.Control = try #require(try await read(fixture, pid: pid).menuBar?.item(.window))
    let opening: ContinuousClock.Instant = .now
    try await perform(item)
    let opened: TimedLine<WatchAppsLine> = try await lines.next { line in line.elements(of: pid)?.titled("Fixture \(cycle)") != nil }
    try await press(.hangBusy, in: fixture, pid: pid)
    let hanging: ContinuousClock.Instant = .now
    let hung: TimedLine<WatchAppsLine> = try await lines.next { line in line.entry.instances?[pid]?.phase == .unresponsive(.running) }
    expectNoDifference(kill(pid, SIGUSR1), 0)
    _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
    let reaction: Reaction = Reaction(stop: stopped.instant - stopping, resume: resumed.instant - resuming, window: opened.instant - opening, hang: hung.instant - hanging)
    let later: [Reaction] = try await reacting(to: fixture, pid: pid, cycles: Array(cycles.dropFirst()), lines: &lines)
    return [reaction] + later
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Reactions {
        @Test(.application(FixtureApplication.self, launches: false))
        func watchPrintsEachChangeWithinItsHeartbeatBound() async throws {
            let heartbeat: Duration = .seconds(1)
            let fixture: Fixture = try Fixture()
            let reactions: [Reaction] = try await watching([fixture.identifier], within: .seconds(120)) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                return try await reacting(to: fixture, pid: pid, cycles: 1...5, lines: &lines)
            }
            .outcome
            let bounds: [(change: String, latency: KeyPath<Reaction, Duration>, bound: Duration)] = [
                ("stop", \.stop, heartbeat * 3), ("resume", \.resume, heartbeat * 3), ("hang", \.hang, heartbeat * 4), ("window", \.window, heartbeat * 2),
            ]
            for (change, latency, bound): (String, KeyPath<Reaction, Duration>, Duration) in bounds {
                let samples: [Duration] = reactions.map { reaction in reaction[keyPath: latency] }.sorted()
                let prompt: Bool = samples[samples.count / 2] < bound
                #expect(prompt, "\(change) median of \(samples) reaches \(bound)")
            }
        }
    }
}
