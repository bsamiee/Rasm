import AppKit
import AsyncAlgorithms
import CasePathsCore
import CustomDump
import Fixture
import Foundation
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

extension WindowServerTests {
    struct LaunchTests {
        static func normalOwners() -> [pid_t] {
            let rows: [[String: Any]] = CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]] ?? []
            return rows.filter { row in row[kCGWindowLayer as String] as? Int32 == CGWindowLevelForKey(.normalWindow) }.compactMap { row in row[kCGWindowOwnerPID as String] as? pid_t }
        }

        @Test(
            .enabled(
                if: NSWorkspace.shared.frontmostApplication.map { front in Self.normalOwners().contains(front.processIdentifier) } == true,
                "Ordering behind the frontmost application needs one showing a normal window",
            )
        )
        func neverRanksAWindowOfAnInstanceOrderingItselfFrontAboveTheFrontmostApplicationFromOpenToSettle() async throws {
            let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)
            let contents = folder.appending(components: "Fixture.app", "Contents")
            try FileManager.default.createDirectory(at: contents.appending(component: "MacOS"), withIntermediateDirectories: true)
            try FileManager.default.copyItem(at: FixtureSession.executable, to: contents.appending(components: "MacOS", FixtureSession.executable.lastPathComponent))
            try PropertyListSerialization.data(
                fromPropertyList: [
                    kCFBundleIdentifierKey as String: "app.rasm.fixture.\(UUID().uuidString)", kCFBundleExecutableKey as String: FixtureSession.executable.lastPathComponent,
                ],
                format: .xml,
                options: 0,
            ).write(to: contents.appending(component: "Info.plist"))
            let front = try #require(NSWorkspace.shared.frontmostApplication)
            let sampling = Task {
                await AsyncTimerSequence(interval: .milliseconds(5), clock: ContinuousClock()).map { _ in (front: focusedProcesses().front, owners: Self.normalOwners()) }
                    .reduce(into: [(front: pid_t?, owners: [pid_t])]()) { held, sample in held.append(sample) }
            }
            let events = NSWorkspace.shared.launchEvents(
                try Installation(at: folder.appending(component: "Fixture.app")),
                in: ScriptingDictionaries { _ in nil },
                watches: Watches { _, _ throws(ApplicationFailure) in [] },
                within: .seconds(5),
                arguments: ["-\(FixtureSession.ordersFrontAtLaunch)", "YES"],
            )
            var launched: Instance?
            for await case .launched(let event) in events { launched = event.instance }
            sampling.cancel()
            let samples: [(front: pid_t?, owners: [pid_t])] = await sampling.value
            let entries: Int = PermittedFrontList.entries
            let instance = try #require(launched)
            _ = instance.application.forceTerminate()
            try FileManager.default.removeItem(at: folder)
            let fronts: Set<pid_t?> = Set(samples.map(\.front))
            expectNoDifference(fronts, [front.processIdentifier], "Front processes sampled from open to settle")
            let covering: Int = samples.count { sample in
                sample.owners.firstIndex(of: instance.application.processIdentifier).map { fixture in
                    sample.owners.firstIndex(of: front.processIdentifier).map { frontmost in fixture < frontmost } ?? false
                } ?? false
            }
            expectNoDifference(covering, 0, "Samples of \(samples.count) every 5 ms where a fixture window ranked above the frontmost application's")
            let behind: Bool =
                samples.last.map { sample in (sample.owners.firstIndex(of: instance.application.processIdentifier) ?? -1) > (sample.owners.firstIndex(of: front.processIdentifier) ?? .max) } ?? false
            #expect(behind, "The settled fixture shows a normal window below the frontmost application's")
            expectNoDifference(entries, 0, "Permitted front entries left after settle")
        }

        @Test(
            .enabled(
                if: NSWorkspace.shared.frontmostApplication.map { front in Self.normalOwners().contains(front.processIdentifier) } == true,
                "Ordering behind the frontmost application needs one showing a normal window",
            )
        )
        func ordersAWindowADrivenInactiveInstanceRaisesBelowTheFrontmostApplication() async throws {
            let front = try #require(NSWorkspace.shared.frontmostApplication)
            let (samples, lines, fixture): ([[pid_t]], [Line], pid_t) = try await FixtureSession.launched { fixture in
                let instance = try Instance(try #require(NSRunningApplication(processIdentifier: fixture.process)))
                let watch: Watch = await Watches { _, _ throws(ApplicationFailure) in [] }.watch(instance)
                let sampling = Task {
                    await AsyncTimerSequence(interval: .milliseconds(5), clock: ContinuousClock()).prefix(200).map { _ in Self.normalOwners() }
                        .reduce(into: [[pid_t]]()) { held, owners in held.append(owners) }
                }
                try await fixture.send(.raise(surface: .controls))
                try await Task.sleep(for: .milliseconds(400))
                try await fixture.send(.raise(surface: .controls))
                let samples: [[pid_t]] = await sampling.value
                await Captures.current?.take(in: watch)
                try await fixture.send(.report)
                return (samples, try await fixture.next(\.report).before, fixture.process)
            }
            let ranks: [Bool] = samples.map { owners in
                owners.firstIndex(of: fixture).map { fixture in owners.firstIndex(of: front.processIdentifier).map { frontmost in fixture < frontmost } ?? false } ?? false
            }
            let covered: Int = ranks.count(where: \.self)
            #expect(covered <= 20, "Two raises of one fixture window ranked it above the frontmost application's in \(covered) of \(ranks.count) samples 5 ms apart")
            expectNoDifference(ranks.suffix(1), [false], "The last sample keeps the raised window above the frontmost application's")
            #expect(!lines.contains { line in line[case: \.active] == true }, "The fixture became active")
        }
    }
}
