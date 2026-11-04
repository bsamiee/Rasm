import ApplicationServices
import CasePaths
import CustomDump
import Darwin
import Foundation
import Subprocess
import System
import TestSupport
import Testing
import WatchAppsWire

@testable import AppKitFixture

// --- [MODELS] --------------------------------------------------------------------------

private struct Ended: Equatable {
    let status: Int32?
    let signal: Int32?
    let detail: [String]?
    let reported: Bool
    let reportFailure: String?
    let statusFailure: String?
    let next: WatchAppsLine.Next?

    init(status: Int32? = nil, signal: Int32? = nil) {
        self.status = status
        self.signal = signal
        detail = nil
        reported = false
        reportFailure = nil
        statusFailure = nil
        next = nil
    }

    init(_ exit: WatchAppsLine.Exit) {
        status = exit.status
        signal = exit.signal
        detail = exit.detail
        reported = exit.report != nil
        reportFailure = exit.reportFailure
        statusFailure = exit.statusFailure
        next = exit.next
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
            let (launched, transcript): ((pid: Int32, lines: [WatchAppsLine]), [WatchAppsLine]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let launched: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture)
                _ = try await lines.next { line in line.entry.instances?[launched.pid]?.phase == .responding }
                return launched
            }
            let start: [WatchAppsLine] = Array(transcript.prefix(2))
            let launchPhases: [Phase] = phases(of: launched.pid, in: launched.lines)
            let launching: [Phase] = Array(repeating: .launching, count: max(launched.lines.count - 1, 0))
            let outline: [Outline]? = transcript.last?.elements(of: launched.pid)?.map(Outline.init)
            let running: [WatchAppsLine.Running?] = launched.lines.map { line in line.entry.instances?[launched.pid]?.running }
            expectNoDifference(start.map(\.key), ["system", fixture.identifier])
            expectNoDifference(start.last?.entry, .instances([:]))
            expectNoDifference(Set(launched.lines.map(\.key)), [fixture.identifier])
            expectNoDifference(Set(pids(in: launched.lines)), [[launched.pid]])
            expectNoDifference(launchPhases, launching + [.responding])
            expectNoDifference(running.dropLast().map { line in line?.next }, Array(repeating: WatchAppsLine.Next.wait, count: launching.count))
            expectNoDifference(Set(running.map { line in line?.created }), [true])
            expectNoDifference(Array(phases(of: launched.pid, in: transcript).drop { phase in phase == .launching }), [.responding])
            expectNoDifference(outline, [Outline(role: kAXWindowRole, subrole: kAXStandardWindowSubrole, title: "Fixture 0", requiresAnswer: false, numbered: true, detailed: false)])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchPrintsLaunchingUntilSlowLaunchFinishesThenExitsLeavingInstanceRunning() async throws {
            let fixture: Fixture = try Fixture()
            let settings: Settings = Settings(scenario: .slowLaunch)
            let launched: (outcome: Int32, lines: [WatchAppsLine], status: TerminationStatus) = try await streamingJSONLines(
                of: .product(named: "watch-apps"),
                arguments: ["launch", fixture.identifier] + settings.arguments.flatMap { argument in ["--arguments", argument] },
                as: WatchAppsLine.self,
            ) { lines in
                var iterator: AsyncStream<WatchAppsLine>.Iterator = lines.makeAsyncIterator()
                let first: WatchAppsLine = try await iterator.next { line in line.entry.instances?.isEmpty == false }
                let pid: Int32 = try #require(first.entry.instances?.keys.first)
                expectNoDifference(kill(pid, SIGUSR1), 0)
                while await iterator.next() != nil {}
                return pid
            }
            let launchPhases: [Phase] = phases(of: launched.outcome, in: launched.lines)
            expectNoDifference(launched.status, .exited(0))
            expectNoDifference([launchPhases.first, launchPhases.last], [.launching, .responding])
            let titles: [String] = try await read(fixture, pid: launched.outcome).compactMap(\.title)
            expectNoDifference(titles.contains("Fixture 0"), true, "\(titles)")
        }

        @Test(.application(FixtureApplication.self, launches: false), .timeLimit(.minutes(1)), arguments: [false, true])
        func interruptedLaunchLeavesApplicationRunning(wait: Bool) async throws {
            let fixture: Fixture = try Fixture()
            let settings: Settings = Settings(scenario: .slowLaunch)
            let ended: (outcome: WatchAppsLine, lines: [WatchAppsLine], status: TerminationStatus) = try await streamingJSONLines(
                of: .product(named: "watch-apps"),
                arguments: Array([["launch", fixture.identifier], wait ? ["--wait"] : [], settings.arguments.flatMap { argument in ["--arguments", argument] }].joined()),
                as: WatchAppsLine.self,
            ) { lines in
                var iterator: AsyncStream<WatchAppsLine>.Iterator = lines.makeAsyncIterator()
                return try await iterator.next { line in line.key == fixture.identifier && line.entry.instances?.isEmpty == false }
            }
            let pid: Int32 = try #require(ended.outcome.entry.instances?.keys.first)
            expectNoDifference(ended.status.isSuccess, !wait)
            expectNoDifference(phases(in: ended.lines), [.launching])
            _ = try await watching([fixture.identifier]) { lines in
                expectNoDifference(kill(pid, SIGUSR1), 0)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
            }
            let titles: [String] = try await read(fixture, pid: pid).compactMap(\.title)
            expectNoDifference(titles.contains("Fixture 0"), true, "\(titles)")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchModalEndsLaunchWithDialogRequiringAnswer() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: (pid: Int32, lines: [WatchAppsLine]), lines: [WatchAppsLine]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let launched: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture, Settings(scenario: .launchModal))
                let answers: [WatchAppsLine.Element]? = launched.lines.last?.elements(of: launched.pid)?.answers
                let dialog: WatchAppsLine.Element = try #require(answers?.first)
                let expected: Answer = Answer(
                    role: kAXWindowRole,
                    modal: true,
                    description: "alert",
                    identifier: "_NS:87",
                    requiresAnswer: true,
                    buttons: Button.alert,
                )
                expectNoDifference(Answer(dialog, modal: true), expected)
                expectNoDifference(dialog.detail?.text, ["Launch modal", Scenario.launchModal.rawValue])
                let ok: WatchAppsLine.Control = try #require(dialog.control("action-button-1"))
                try await perform(ok)
                _ = try await lines.next { line in line.entry.instances?[launched.pid]?.phase == .responding }
                return launched
            }
            let pid: Int32 = watched.outcome.pid
            let watchedPhases: [Phase] = Array(phases(of: pid, in: watched.lines).suffix(2))
            let window: WatchAppsLine.Element? = watched.lines.last?.elements(of: pid)?.titled("Fixture 0")
            expectNoDifference(phases(of: pid, in: watched.outcome.lines).filter { phase in phase != .launching }, [.answering])
            expectNoDifference(watchedPhases, [.answering, .responding])
            expectNoDifference(window?.title, "Fixture 0")
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchWaitObservesDialogAndReturnsApplicationExitStatus() async throws {
            let fixture: Fixture = try Fixture()
            let settings: Settings = Settings(scenario: .launchModal, status: 7)
            let ended: (outcome: Int32, lines: [WatchAppsLine], status: TerminationStatus) = try await streamingJSONLines(
                of: .product(named: "watch-apps"),
                arguments: ["launch", fixture.identifier, "--wait", "--new"] + settings.arguments.flatMap { argument in ["--arguments", argument] },
                as: WatchAppsLine.self,
            ) { lines in
                var iterator: AsyncStream<WatchAppsLine>.Iterator = lines.makeAsyncIterator()
                let asking: WatchAppsLine = try await iterator.next { line in line.entry.instances?.values.contains { instance in instance.phase == .answering } == true }
                let pid: Int32 = try #require(asking.entry.instances?.keys.first)
                let dialog: WatchAppsLine.Element = try #require(asking.elements(of: pid)?.answers.first)
                let button: WatchAppsLine.Control = try #require(dialog.control("action-button-1"))
                try await perform(button)
                _ = try await iterator.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.exit, in: fixture, pid: pid)
                while await iterator.next() != nil {}
                return pid
            }
            expectNoDifference(ended.status, .exited(settings.status))
            expectNoDifference(ended.lines.last?.entry.instances?[ended.outcome]?.exited.map(Ended.init), Ended(status: settings.status))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchWaitReportsNativeSignalAndReturnsShellSignalStatus() async throws {
            let fixture: Fixture = try Fixture()
            let ended: (outcome: Int32, lines: [WatchAppsLine], status: TerminationStatus) = try await streamingJSONLines(
                of: .product(named: "watch-apps"),
                arguments: ["launch", fixture.identifier, "--wait", "--new"],
                as: WatchAppsLine.self,
            ) { lines in
                var iterator: AsyncStream<WatchAppsLine>.Iterator = lines.makeAsyncIterator()
                let ready: WatchAppsLine = try await iterator.next { line in line.entry.instances?.values.contains { instance in instance.phase == .responding } == true }
                let pid: Int32 = try #require(ready.entry.instances?.keys.first)
                expectNoDifference(kill(pid, SIGKILL), 0)
                while await iterator.next() != nil {}
                return pid
            }
            expectNoDifference(ended.status, .exited(128 + SIGKILL))
            expectNoDifference(ended.lines.last?.entry.instances?[ended.outcome]?.exited.map(Ended.init), Ended(signal: SIGKILL))
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: [Int32(0), 7])
        func launchWaitCapturesExitDuringApplicationStartup(status: Int32) async throws {
            let fixture: Fixture = try Fixture()
            let settings: Settings = Settings(scenario: .exit, status: status)
            let ended: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["launch", fixture.identifier, "--wait", "--new"] + settings.arguments.flatMap { argument in ["--arguments", argument] },
                as: WatchAppsLine.self,
            )
            let exits: [Ended] = ended.lines.compactMap(\.entry.instances).flatMap { instances in instances.values.compactMap { instance in instance.exited.map(Ended.init) } }
            expectNoDifference(ended.status, .exited(settings.status))
            expectNoDifference(exits, [Ended(status: settings.status)])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchWaitReusesInstanceAndContinuesPastSystemPrompt() async throws {
            let fixture: Fixture = try Fixture()
            let settings: Settings = Settings(status: 7)
            let pid: Int32 = try await launch(fixture, settings).pid
            let ended: (outcome: Void, lines: [WatchAppsLine], status: TerminationStatus) = try await streamingJSONLines(
                of: .product(named: "watch-apps"),
                arguments: ["launch", fixture.identifier, "--wait"],
                as: WatchAppsLine.self,
            ) { lines in
                var iterator: AsyncStream<WatchAppsLine>.Iterator = lines.makeAsyncIterator()
                let ready: WatchAppsLine = try await iterator.next { line in line.entry.instances?[pid]?.phase == .responding }
                expectNoDifference(ready.entry.instances?[pid]?.running?.created, false)
                let header: [CFString: String] = [kCFUserNotificationAlertHeaderKey: fixture.name]
                let created: CFUserNotification? = CFUserNotificationCreate(nil, 0, kCFUserNotificationPlainAlertLevel, nil, header as CFDictionary)
                let alert: CFUserNotification = try #require(created)
                defer { CFUserNotificationCancel(alert) }
                let prompt: WatchAppsLine = try await iterator.next { line in line.prompts(besides: fixture.identifier) }
                CFUserNotificationCancel(alert)
                _ = try await iterator.next { line in line.key == prompt.key && line.entry == .instances([:]) }
                try await press(.exit, in: fixture, pid: pid)
                while await iterator.next() != nil {}
            }
            expectNoDifference(ended.status, .exited(settings.status))
            expectNoDifference(ended.lines.last?.entry.instances?[pid]?.exited.map(Ended.init), Ended(status: settings.status))
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func launchReusesRunningInstanceUnlessNew() async throws {
            let fixture: Fixture = try Fixture()
            let first: Int32 = try await launch(fixture).pid
            let (outcome, transcript): ((again: [WatchAppsLine], second: (pid: Int32, lines: [WatchAppsLine])), [WatchAppsLine]) = try await watching([fixture.identifier]) {
                lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                let again: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture)
                let second: (pid: Int32, lines: [WatchAppsLine]) = try await launch(fixture, flags: ["--new"], running: [first])
                _ = try await lines.next { line in line.entry.instances?[second.pid]?.phase == .responding }
                return (again.lines, second)
            }
            let created: [Bool?] = [
                outcome.again.last?.entry.instances?[first]?.running?.created, outcome.second.lines.last?.entry.instances?[outcome.second.pid]?.running?.created,
            ]
            let start: [Int32: Phase]? = transcript.dropFirst().first?.entry.instances?.mapValues(\.phase)
            let end: [Int32: Phase]? = transcript.last?.entry.instances?.mapValues(\.phase)
            expectNoDifference(Set(pids(in: outcome.again)), [[first]])
            expectNoDifference(transcript.prefix(2).map(\.key), ["system", fixture.identifier])
            expectNoDifference(start, [first: .responding])
            expectNoDifference(end, [first: .responding, outcome.second.pid: .responding])
            expectNoDifference(created, [false, true])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func quitPrintsStatesUntilExitThenNothingRuns() async throws {
            let fixture: Fixture = try Fixture()
            let pid: Int32 = try await launch(fixture).pid
            let quit: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["quit", "--pid", String(pid)],
                as: WatchAppsLine.self,
            )
            let again: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["quit", fixture.identifier],
                as: WatchAppsLine.self,
            )
            expectNoDifference(quit.status, .exited(0))
            expectNoDifference(quit.lines.last?.entry.instances?[pid]?.exited.map(Ended.init), Ended(status: 0))
            expectNoDifference(again.lines.map(\.entry), [.instances([:])])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func hangReadsUnresponsiveWithUsageAndEveryWindowThenRecovers() async throws {
            let fixture: Fixture = try Fixture()
            _ = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                let described: Int = try #require(try await read(fixture, pid: pid).titled("Fixture 0")?.window)
                try await press(.hangBusy, in: fixture, pid: pid)
                let hung: WatchAppsLine = try await lines.next { line in line.entry.instances?[pid]?.phase == .unresponsive(.running) }
                _ = try #require(hung.entry.instances?[pid]?.running?.usage, "unresponsive line holds usage")
                expectNoDifference(hung.windows(of: pid)?.contains { window in window.number == described }, true)
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
                let stopped: WatchAppsLine = try await lines.next { line in line.entry.instances?[pid]?.phase == .unresponsive(.stopped) }
                let measured: Bool = stopped.entry.instances?[pid]?.running?.usage != nil
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
            let watched: (outcome: Int32, lines: [WatchAppsLine]) = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture, Settings(status: status)).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.exit, in: fixture, pid: pid)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .exited }
                return pid
            }
            let ends: [Ended] = watched.lines.compactMap { line in line.entry.instances?[watched.outcome]?.exited.map(Ended.init) }
            let after: (outcome: Void, lines: [WatchAppsLine]) = try await watching([fixture.identifier]) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
            }
            let read: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
                of: .product(named: "watch-apps"),
                arguments: ["read", fixture.identifier],
                as: WatchAppsLine.self,
            )
            expectNoDifference(ends, [Ended(status: status)])
            expectNoDifference(after.lines.map(\.key), ["system", fixture.identifier])
            expectNoDifference(after.lines.last?.entry, .instances([:]))
            expectNoDifference(read.lines.map(\.entry), [.instances([:])])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func killPrintsSignalWithoutReport() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: Int32, lines: [WatchAppsLine]) = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                let killed: Int32 = kill(pid, SIGKILL)
                expectNoDifference(killed, 0)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .exited }
                return pid
            }
            let ends: [Ended] = watched.lines.compactMap { line in line.entry.instances?[watched.outcome]?.exited.map(Ended.init) }
            expectNoDifference(ends, [Ended(signal: SIGKILL)])
        }

        @Test(.application(FixtureApplication.self, launches: false), .testDirectory, .timeLimit(.minutes(1)))
        func watcherExitsWhenItsParentExits() async throws {
            let fixture: Fixture = try Fixture()
            let release: String = try #require(TestDirectory.current).appending(path: "release").path(percentEncoded: false)
            try #require(unsafe mkfifo(release, 0o600) == 0)
            let parent: (outcome: Void, lines: [WatchAppsLine], status: TerminationStatus) = try await streamingJSONLines(
                of: .name("sh"),
                arguments: ["-c", "\"$0\" watch \"$1\" & read -r _ < \"$2\"", try builtProduct(named: "watch-apps").path(percentEncoded: false), fixture.identifier, release],
                as: WatchAppsLine.self,
            ) { lines in
                var iterator: AsyncStream<WatchAppsLine>.Iterator = lines.makeAsyncIterator()
                _ = try await iterator.next { line in line.key == fixture.identifier }
                let writer: FileDescriptor = try FileDescriptor.open(FilePath(release), .writeOnly)
                _ = try writer.closeAfter { try writer.writeAll("\n".utf8) }
                while await iterator.next() != nil {}
            }
            expectNoDifference(parent.status, .exited(0))
            expectNoDifference(parent.lines.filter { line in line.key == fixture.identifier }.map(\.entry), [.instances([:])])
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func concurrentWatchersPrintIdenticalLinesForEachSettledState() async throws {
            let fixture: Fixture = try Fixture()
            let watchers: Int = 4
            let (ready, readiness): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream()
            let opened: @Sendable (WatchAppsLine) -> Bool = { line in
                line.entry.instances?.values.contains { instance in instance.running?.status.responding?.elements.titled("Fixture 1") != nil } == true
            }
            let watched: (pid: Int32, settled: [[WatchAppsLine]]) = try await withThrowingTaskGroup(of: [WatchAppsLine].self) { group in
                for _: Int in 0..<watchers {
                    group.addTask {
                        try await watching([fixture.identifier]) { lines in
                            let start: WatchAppsLine = try await lines.next { line in line.key == fixture.identifier }
                            readiness.yield()
                            let shown: WatchAppsLine = try await lines.next(where: opened)
                            readiness.yield()
                            let ended: WatchAppsLine = try await lines.next { line in line.entry.instances?.values.contains { instance in instance.phase == .exited } == true }
                            return [start, shown, ended]
                        }
                        .outcome
                    }
                }
                var signals: AsyncStream<Void>.Iterator = ready.makeAsyncIterator()
                for _: Int in 0..<watchers { await signals.next() }
                let pid: Int32 = try await launch(fixture, Settings(status: 5)).pid
                try await press(.window, in: fixture, pid: pid)
                for _: Int in 0..<watchers { await signals.next() }
                try await press(.exit, in: fixture, pid: pid)
                return (pid, try await group.reduce(into: []) { all, settled in all.append(settled) })
            }
            let reference: [WatchAppsLine] = try #require(watched.settled.first)
            expectNoDifference(watched.settled.count, watchers)
            expectNoDifference(reference.last?.entry.instances?[watched.pid]?.exited.map(Ended.init), Ended(status: 5))
            for settled: [WatchAppsLine] in watched.settled.dropFirst() { expectNoDifference(settled, reference) }
        }
    }
}
