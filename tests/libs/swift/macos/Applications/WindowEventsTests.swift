import AppKit
@unsafe @preconcurrency import ApplicationServices
import Applications
import CasePathsCore
import CustomDump
import Fixture
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    struct WindowEventsTests {
        @Test(.enabled(if: AXIsProcessTrusted(), "Watching the fixture from outside needs this test process to hold the Accessibility grant"))
        func statesEachChildWindowUnderTheWindowItsApplicationAttachedItTo() async throws {
            let (reported, rows): ([Int: Int], [Int: Int]) = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.show(surface: .sheet))
                try await fixture.send(.show(surface: .popover))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let states = await watch.messages()
                let children: Set<Int> = Set(report.windows.compactMap { window in window.parent.map { _ in window.number } })
                let state: InstanceState.Activity? = await states.compactMap { (message: Watch.Message) -> InstanceState.Activity? in message[case: \.state]?[case: \.running] }
                    .first { activity in children.isSubset(of: Set(activity.windows.map { row in Int(row.number) })) }
                await Captures.current?.take(in: watch)
                let windows: [InstanceState.Window] = try #require(state, "no state listed the windows \(children)").windows
                return (
                    Dictionary(report.windows.compactMap { window in window.parent.map { parent in (window.number, parent) } }) { first, _ in first },
                    Dictionary(windows.compactMap { row in row.parent.map { parent in (Int(row.number), Int(parent)) } }) { first, _ in first },
                )
            }
            #expect(!reported.isEmpty, "the fixture reported no child window")
            expectNoDifference(rows.filter { child, _ in reported[child] != nil }, reported)
        }

        @Test(.enabled(if: AXIsProcessTrusted(), "Watching the fixture from outside needs this test process to hold the Accessibility grant"))
        func reportsAWindowNoContainerDescribesAsItHidesAndShowsWithinAFractionOfAHeartbeat() async throws {
            let clock = ContinuousClock()
            let seen: [(change: WindowNotification, hidden: Bool, after: Duration)] = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                try await fixture.send(.report)
                let report: Report = try await fixture.next(\.report).value
                let found: Int? = report.windows.first { (window: Report.Window) -> Bool in window.surface == .unlisted }?.number
                let unlisted: Int = try #require(found)
                var seen: [(change: WindowNotification, hidden: Bool, after: Duration)] = []
                for (command, change): (Command, WindowNotification) in [(.dismiss(surface: .unlisted), .hidden), (.show(surface: .unlisted), .shown)] {
                    let events = await watch.messages()
                    let sent: ContinuousClock.Instant = clock.now
                    try await fixture.send(command)
                    let row: InstanceState.Window? = await events.compactMap { (message: Watch.Message) -> InstanceState.Window? in
                        guard case .event(let event) = message, case .window(change, let row) = event.change, Int(row.number) == unlisted else { return nil }
                        return row
                    }
                    .first { _ in true }
                    let shown: InstanceState.Window = try #require(row, "no \(change) event named window \(unlisted)")
                    seen.append((change, shown.hidden, sent.duration(to: clock.now)))
                }
                await Captures.current?.take(in: watch)
                return seen
            }
            expectNoDifference(seen.map { seen in "\(seen.change) hidden \(seen.hidden)" }, ["hidden hidden true", "shown hidden false"])
            let paced: [String] = seen.filter { event in event.after >= Duration.milliseconds(250) }.map { event in "\(event.change) after \(event.after)" }
            #expect(paced.isEmpty, "events arrived at a window-list poll's pace: \(paced)")
        }
    }
}
