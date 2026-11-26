import AppKit
import ApplicationServices
import AsyncAlgorithms
import CustomDump
import Darwin
import Fixture
import Processes
import System
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

private func logChildren() -> Int {
    let size: Int32 = proc_listpids(UInt32(PROC_PPID_ONLY), UInt32(getpid()), nil, 0)
    let children: [pid_t] = unsafe [pid_t](unsafeUninitializedCapacity: Int(max(size, 0)) / MemoryLayout<pid_t>.stride + 8) { buffer, count in
        let read: Int32 = unsafe proc_listpids(UInt32(PROC_PPID_ONLY), UInt32(getpid()), buffer.baseAddress, Int32(buffer.count * MemoryLayout<pid_t>.stride))
        count = Int(max(read, 0)) / MemoryLayout<pid_t>.stride
    }
    return children.count { child in Result(catching: { () throws(Errno) in try ProcessInstance(of: child).executable() }) == .success(FilePath("/usr/bin/log")) }
}

private func settled(on count: Int, within ticks: Int) async -> Bool {
    await AsyncTimerSequence(interval: .milliseconds(50), clock: ContinuousClock()).prefix(ticks).map { _ in logChildren() }.contains(count)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension WindowServerTests {
    @Suite(.enabled(if: AXIsProcessTrusted(), "Watching the fixture from outside needs this test process to hold the Accessibility grant"))
    struct WatchTests {
        @Test
        func runsTheFaultLogChildOnlyWhileAnActCanDriveTheWatchAndEndsItWhenWatchesFinish() async throws {
            let counts: [Bool] = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watches = Watches()
                _ = await watches.watch(try Instance(application), driven: false)
                let undriven: Bool = await settled(on: 1, within: 10)
                _ = await watches.watch(try Instance(application))
                let driven: Bool = await settled(on: 1, within: 40)
                await watches.finish()
                let finished: Bool = await settled(on: 0, within: 40)
                return [undriven, driven, finished]
            }
            expectNoDifference(counts, [false, true, true], "log children seen: none while undriven, one once driven, none after finish")
        }

        @Test
        func publishesAStateWithinAQuietIntervalOfAnAccessibilityChangeInsteadOfTheNextBeat() async throws {
            let clock = ContinuousClock()
            let waits: [Duration] = try await FixtureSession.launched { fixture in
                let application = try #require(NSRunningApplication(processIdentifier: fixture.process))
                let watch: Watch = await Watches().watch(try Instance(application))
                var waits: [Duration] = []
                for trial in 0..<5 {
                    let states = await watch.states().dropFirst()
                    let sent: ContinuousClock.Instant = clock.now
                    try await fixture.send(.retitle(entry: .action, member: 0, to: "Retitled \(trial)"))
                    _ = await states.first { _ in true }
                    waits.append(sent.duration(to: clock.now))
                }
                await Captures.current?.take(in: watch)
                return waits
            }
            let paced: [Duration] = waits.filter { wait in wait >= quiet * 5 }
            #expect(paced.isEmpty, "states arrived at the beat's pace after a retitle: \(waits)")
        }
    }
}
