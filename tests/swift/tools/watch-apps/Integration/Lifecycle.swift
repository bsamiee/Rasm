import CasePaths
import CustomDump
import Darwin
import Foundation
import Subprocess
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [MODELS] --------------------------------------------------------------------------

private struct Ended: Equatable {
    let status: Int32?
    let signal: Int32?
    let reported: Bool
    let reportFailure: String?
    let statusFailure: String?

    init(status: Int32? = nil, signal: Int32? = nil) {
        self.status = status
        self.signal = signal
        reported = false
        reportFailure = nil
        statusFailure = nil
    }

    init(_ exit: WatchAppsLine.Exit) {
        status = exit.status
        signal = exit.signal
        reported = exit.report != nil
        reportFailure = exit.reportFailure
        statusFailure = exit.statusFailure
    }
}

private struct Outline: Equatable {
    let role: String?
    let subrole: String?
    let title: String?
    let requiresAnswer: Bool?
    let numbered: Bool
    let detailed: Bool

    init(role: String?, subrole: String?, title: String?, requiresAnswer: Bool?, numbered: Bool, detailed: Bool) {
        self.role = role
        self.subrole = subrole
        self.title = title
        self.requiresAnswer = requiresAnswer
        self.numbered = numbered
        self.detailed = detailed
    }

    init(_ element: WatchAppsLine.Element) {
        role = element.role
        subrole = element.subrole
        title = element.title
        requiresAnswer = element.requiresAnswer
        numbered = element.window != nil
        detailed = element.detail != nil
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Lifecycle {
        @Test(.application(FixtureApplication.self, launches: false))
        func watchStartsWithSystemThenEmptyAndFollowsLaunch() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: (pid: Int32, lines: [WatchAppsLine]), lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let launched: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture)
                _ = try await lines.next { line in line.entry.instances?[launched.pid]?.phase == .responding }
                return launched
            }
            let launched: (pid: Int32, lines: [WatchAppsLine]) = watched.outcome
            let transcript: [WatchAppsLine] = watched.lines.map(\.line)
            let start: [WatchAppsLine] = Array(transcript.prefix(2))
            let launchPhases: [Phase] = phases(of: launched.pid, in: launched.lines)
            let launching: [Phase] = Array(repeating: .launching, count: max(launched.lines.count - 1, 0))
            let outline: [Outline]? = transcript.last?.elements(of: launched.pid)?.map(Outline.init)
            expectNoDifference(start.map(\.key), ["system", fixture.identifier])
            expectNoDifference(start.last?.entry, .instances([:]))
            expectNoDifference(Set(launched.lines.map(\.key)), [fixture.identifier])
            expectNoDifference(Set(pids(in: launched.lines)), [[launched.pid]])
            expectNoDifference(launchPhases, launching + [.responding])
            expectNoDifference(Array(phases(of: launched.pid, in: transcript).drop { phase in phase == .launching }), [.responding])
            expectNoDifference(outline, [Outline(role: "AXWindow", subrole: "AXStandardWindow", title: "Fixture 0", requiresAnswer: false, numbered: true, detailed: false)])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchPrintsLaunchingUntilSlowLaunchFinishesThenExitsLeavingInstanceRunning() async throws {
            let fixture: Fixture = try Fixture()
            let duration: Duration = .seconds(2)
            let clock: ContinuousClock = ContinuousClock()
            let start: ContinuousClock.Instant = clock.now
            let launched: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture, Settings(scenario: .slowLaunch, duration: duration))
            let elapsed: Duration = clock.now - start
            let launchPhases: [Phase] = phases(of: launched.pid, in: launched.lines)
            let held: Bool = elapsed >= duration
            #expect(held, "launch returned after \(elapsed)")
            expectNoDifference([launchPhases.first, launchPhases.last], [.launching, .responding])
            let titles: [String] = try await read(fixture, pid: launched.pid).compactMap(\.title)
            expectNoDifference(titles.contains("Fixture 0"), true, "\(titles)")
        }

        @Test(.application(FixtureApplication.self, launches: false), .timeLimit(.minutes(1)))
        func launchTerminatedWhileLaunchingExitsZeroLeavingInstanceRunning() async throws {
            let fixture: Fixture = try Fixture()
            let settings: Settings = Settings(scenario: .slowLaunch, duration: .seconds(3))
            let ended: (outcome: Int32, lines: [TimedLine<WatchAppsLine>], status: TerminationStatus) = try await streamingJSONLines(
                of: .product(named: "watch-apps"),
                within: .seconds(30),
                arguments: ["launch", fixture.identifier] + settings.arguments.flatMap { argument in ["--arguments", argument] },
                as: WatchAppsLine.self,
            ) { lines in
                var iterator: AsyncStream<TimedLine<WatchAppsLine>>.Iterator = lines.makeAsyncIterator()
                let first: TimedLine<WatchAppsLine> = try await iterator.next { line in line.entry.instances?.isEmpty == false }
                return try #require(first.line.entry.instances?.keys.first)
            }
            expectNoDifference(ended.status, .exited(0))
            expectNoDifference(phases(in: ended.lines.map(\.line)), [.launching])
            _ = try await watching([fixture.identifier], within: .seconds(15)) { lines in
                _ = try await lines.next { line in line.entry.instances?[ended.outcome]?.phase == .responding }
            }
            let titles: [String] = try await read(fixture, pid: ended.outcome).compactMap(\.title)
            expectNoDifference(titles.contains("Fixture 0"), true, "\(titles)")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchModalEndsLaunchWithDialogRequiringAnswer() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: (pid: Int32, lines: [WatchAppsLine]), lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let launched: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture, Settings(scenario: .launchModal))
                let answers: [WatchAppsLine.Element]? = launched.lines.last?.elements(of: launched.pid)?.answers
                let dialog: WatchAppsLine.Element = try #require(answers?.first)
                let expected: Answer = Answer(
                    role: "AXWindow",
                    modal: true,
                    description: "alert",
                    identifier: "_NS:87",
                    requiresAnswer: true,
                    buttons: [
                        Button(identifier: "action-button-1", label: "OK", attribute: "AXDefaultButton"), Button(identifier: "action-button-2", label: "Cancel", attribute: "AXCancelButton"),
                    ],
                )
                expectNoDifference(Answer(dialog, modal: true), expected)
                expectNoDifference(dialog.detail?.text, ["Launch modal", Scenario.launchModal.rawValue])
                let ok: WatchAppsLine.Control = try #require(dialog.control("action-button-1"))
                try await perform(ok)
                _ = try await lines.next { line in line.entry.instances?[launched.pid]?.phase == .responding }
                return launched
            }
            let pid: Int32 = watched.outcome.pid
            let watchedPhases: [Phase] = Array(phases(of: pid, in: watched.lines.map(\.line)).suffix(2))
            let window: WatchAppsLine.Element? = watched.lines.last?.line.elements(of: pid)?.titled("Fixture 0")
            expectNoDifference(phases(of: pid, in: watched.outcome.lines).filter { phase in phase != .launching }, [.answering])
            expectNoDifference(watchedPhases, [.answering, .responding])
            expectNoDifference(window?.title, "Fixture 0")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchReusesRunningInstanceUnlessNew() async throws {
            let fixture: Fixture = try Fixture()
            let first: Int32 = try await launch(fixture).pid
            let watched: (outcome: (again: [WatchAppsLine], second: Int32), lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let again: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture)
                let second: Int32 = try await launch(fixture, flags: ["--new"], running: [first]).pid
                _ = try await lines.next { line in line.entry.instances?[second]?.phase == .responding }
                return (again.lines, second)
            }
            let transcript: [WatchAppsLine] = watched.lines.map(\.line)
            let start: [String: Phase]? = transcript.dropFirst().first?.entry.instances?.mapValues(\.phase).reduce(into: [:]) { all, entry in all[String(entry.key)] = entry.value }
            let end: [String: Phase]? = transcript.last?.entry.instances?.mapValues(\.phase).reduce(into: [:]) { all, entry in all[String(entry.key)] = entry.value }
            expectNoDifference(Set(pids(in: watched.outcome.again)), [[first]])
            expectNoDifference(transcript.prefix(2).map(\.key), ["system", fixture.identifier])
            expectNoDifference(start, [String(first): .responding])
            expectNoDifference(end, [String(first): .responding, String(watched.outcome.second): .responding])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func hangReadsUnresponsiveWithUsageAndEveryWindowThenRecovers() async throws {
            let fixture: Fixture = try Fixture()
            _ = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                let described: Int = try #require(try await read(fixture, pid: pid).titled("Fixture 0")?.window)
                try await press(.hangBusy, in: fixture, pid: pid)
                let hung: TimedLine<WatchAppsLine> = try await lines.next { line in line.entry.instances?[pid]?.phase == .unresponsive(.running) }
                _ = try #require(hung.line.entry.instances?[pid]?.running?.usage, "unresponsive line holds usage")
                expectNoDifference(hung.line.windows(of: pid)?.contains { window in window.number == described }, true)
                let released: Int32 = kill(pid, SIGUSR1)
                expectNoDifference(released, 0)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
            }
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func stoppedProcessReadsStoppedThenRespondsOnContinue() async throws {
            let fixture: Fixture = try Fixture()
            _ = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.stop, in: fixture, pid: pid)
                let stopped: TimedLine<WatchAppsLine> = try await lines.next { line in line.entry.instances?[pid]?.phase == .unresponsive(.stopped) }
                let measured: Bool = stopped.line.entry.instances?[pid]?.running?.usage != nil
                let resumed: Int32 = kill(pid, SIGCONT)
                #expect(measured)
                expectNoDifference(resumed, 0)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
            }
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func exitPrintsStatusThenInstanceLeaves() async throws {
            let fixture: Fixture = try Fixture()
            let status: Int32 = 7
            let watched: (outcome: Int32, lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture, Settings(status: status)).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.exit, in: fixture, pid: pid)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .exited }
                return pid
            }
            let transcript: [WatchAppsLine] = watched.lines.map(\.line)
            let ends: [Ended] = transcript.compactMap { line in line.entry.instances?[watched.outcome]?.exited.map(Ended.init) }
            let after: (outcome: Void, lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
            }
            let read: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["read", fixture.identifier],
                as: WatchAppsLine.self,
            )
            expectNoDifference(ends, [Ended(status: status)])
            expectNoDifference(after.lines.map(\.line.key), ["system", fixture.identifier])
            expectNoDifference(after.lines.last?.line.entry, .instances([:]))
            expectNoDifference(read.lines.map(\.entry), [.instances([:])])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func killPrintsSignalWithoutReport() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: Int32, lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                let killed: Int32 = kill(pid, SIGKILL)
                expectNoDifference(killed, 0)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .exited }
                return pid
            }
            let ends: [Ended] = watched.lines.compactMap { timed in timed.line.entry.instances?[watched.outcome]?.exited.map(Ended.init) }
            expectNoDifference(ends, [Ended(signal: SIGKILL)])
        }

        @Test(.application(FixtureApplication.self, launches: false), .timeLimit(.minutes(1)))
        func watcherExitsWhenItsParentExits() async throws {
            let fixture: Fixture = try Fixture()
            let lifetime: Duration = .seconds(2)
            let budget: Duration = .seconds(30)
            let watchApps: String = try builtProduct(named: "watch-apps").path(percentEncoded: false)
            let clock: ContinuousClock = ContinuousClock()
            let start: ContinuousClock.Instant = clock.now
            let parent: (outcome: ContinuousClock.Instant, lines: [TimedLine<WatchAppsLine>], status: TerminationStatus) = try await streamingJSONLines(
                of: .name("sh"),
                within: budget,
                arguments: ["-c", "\"$0\" watch \"$1\" & sleep \(lifetime.components.seconds)", watchApps, fixture.identifier],
                as: WatchAppsLine.self,
            ) { lines in
                for await _: TimedLine<WatchAppsLine> in lines {}
                return clock.now
            }
            let ended: Duration = parent.outcome - start
            let bounded: Bool = ended >= lifetime && ended < budget
            expectNoDifference(parent.status, .exited(0))
            expectNoDifference(parent.lines.map(\.line.key), ["system", fixture.identifier])
            expectNoDifference(parent.lines.last?.line.entry, .instances([:]))
            #expect(bounded, "watch-apps closed its output \(ended) after its parent started")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func concurrentWatchersPrintIdenticalLines() async throws {
            let fixture: Fixture = try Fixture()
            let watchers: Int = 4
            let (ready, readiness): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream()
            let watched: (pid: Int32, transcripts: [[WatchAppsLine]]) = try await withThrowingTaskGroup(of: [WatchAppsLine].self) { group in
                for _: Int in 0..<watchers {
                    group.addTask {
                        let session: (outcome: Void, lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                            _ = try await lines.next { line in line.key == fixture.identifier }
                            readiness.yield()
                            _ = try await lines.next { line in line.entry.instances?.values.contains { instance in instance.phase == .exited } == true }
                        }
                        return session.lines.map(\.line)
                    }
                }
                var signals: AsyncStream<Void>.Iterator = ready.makeAsyncIterator()
                for _: Int in 0..<watchers { await signals.next() }
                let pid: Int32 = try await launch(fixture, Settings(status: 5)).pid
                try await press(.window, in: fixture, pid: pid)
                try await press(.exit, in: fixture, pid: pid)
                let transcripts: [[WatchAppsLine]] = try await group.reduce(into: []) { all, transcript in all.append(transcript) }
                return (pid, transcripts)
            }
            let reference: [WatchAppsLine] = try #require(watched.transcripts.first)
            let ends: [Ended] = reference.compactMap { line in line.entry.instances?[watched.pid]?.exited.map(Ended.init) }
            expectNoDifference(watched.transcripts.count, watchers)
            expectNoDifference(ends, [Ended(status: 5)])
            for transcript: [WatchAppsLine] in watched.transcripts.dropFirst() { expectNoDifference(transcript, reference) }
        }
    }
}
