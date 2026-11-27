import AppKit
import AppleEvents
import AsyncAlgorithms
import CasePathsCore
import CustomDump
import Fixture
import Foundation
import Processes
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
            let front = try #require(NSWorkspace.shared.frontmostApplication)
            let (samples, entries, launched): ([(front: pid_t?, owners: [pid_t])], Int, Instance?) = try await FixtureBundle.installed { bundle in
                let sampling = Task {
                    await AsyncTimerSequence(interval: .milliseconds(5), clock: ContinuousClock()).map { _ in (front: focusedProcesses().front, owners: Self.normalOwners()) }
                        .reduce(into: [(front: pid_t?, owners: [pid_t])]()) { held, sample in held.append(sample) }
                }
                let events = NSWorkspace.shared.launchEvents(
                    try Installation(at: bundle.url),
                    in: ScriptingDictionaries { _ in nil },
                    watches: Watches(),
                    within: .seconds(5),
                    arguments: bundle.arguments(orderingFront: true),
                )
                var launched: Instance?
                for await case .launched(let event) in events { launched = event.instance }
                sampling.cancel()
                return (await sampling.value, PermittedFrontList.entries, launched)
            }
            let instance = try #require(launched)
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

        @Test
        func leavesAutomationConsentTowardTheLaunchedInstanceUnaskedAsLaunchServicesSendsItsFirstAppleEvent() async throws {
            let consent: OSStatus = try await FixtureBundle.installed { bundle in
                let events = NSWorkspace.shared.launchEvents(
                    try Installation(at: bundle.url),
                    in: ScriptingDictionaries { _ in nil },
                    watches: Watches(),
                    within: .seconds(5),
                    arguments: bundle.arguments(orderingFront: false),
                )
                var launched: Instance?
                for await case .launched(let event) in events { launched = event.instance }
                let instance = try #require(launched)
                return await NSAppleEventDescriptor(processIdentifier: instance.process.pid).consent(to: AEEventClass(kCoreEventClass), AEEventID(kAEOpenApplication), asking: false)
            }
            expectNoDifference(consent, OSStatus(errAEEventWouldRequireUserConsent), "Automation consent of this process toward the settled instance")
        }

        @Test
        func reportsAnInstanceAtTheURLOfItsInstallationReadThroughASymlinkedFolderWithoutATrailingSlash() async throws {
            let (reported, installed): (URL?, URL) = try await FixtureBundle.installed { bundle in
                let installation = try Installation(at: bundle.url)
                let events = NSWorkspace.shared.launchEvents(
                    installation,
                    in: ScriptingDictionaries { _ in nil },
                    watches: Watches(),
                    within: .seconds(5),
                    arguments: bundle.arguments(orderingFront: false),
                )
                var launched: Instance?
                for await case .launched(let event) in events { launched = event.instance }
                let instance = try #require(launched)
                return (InstanceSnapshot(instance, state: .failed(ApplicationFailure(subject: "probe", reason: .noWindow)))?.url, installation.url)
            }
            expectNoDifference(reported?.absoluteString, installed.absoluteString, "Instance URL against the installation URL the listing matches it by")
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
                let watch: Watch = await Watches().watch(instance)
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
