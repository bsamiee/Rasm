import CustomDump
import Foundation
import PropertyBased
import Testing

@testable import Files

// --- [MODELS] --------------------------------------------------------------------------

private struct Schedule: Sendable, CustomTestStringConvertible {
    let count: Int
    let base: Duration
    let growth: Double
    let cap: Duration?
    let jitter: Double

    var testDescription: String { "count \(count) base \(base) growth \(growth) cap \(String(describing: cap)) jitter \(jitter)" }

    var backoff: Backoff { Backoff(count: count, base: base, growth: growth, cap: cap, jitter: jitter) }

    func bounds(_ attempt: Int) -> ClosedRange<Duration> {
        let grown: Double = base / .seconds(1) * pow(growth, Double(attempt))
        let ceiling: Duration = cap ?? .seconds(Int64.max / 4)
        return min(.seconds(grown), ceiling)...min(.seconds(grown * (1 + jitter)), ceiling)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func delays(_ backoff: Backoff, seed: UInt64 = 1) -> [Duration] {
    var generator: Xoshiro = Xoshiro(seed: (seed, seed &+ 1, seed &+ 2, seed &+ 3))
    return backoff.delays(using: &generator).map { delay in .nanoseconds(Int64((delay / .nanoseconds(1)).rounded())) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Backoffs {
    @Test
    func delaysGrowGeometricallyFromTheBase() {
        expectNoDifference(delays(Backoff(count: 5, base: .milliseconds(100))), [.milliseconds(100), .milliseconds(200), .milliseconds(400), .milliseconds(800), .milliseconds(1600)])
        expectNoDifference(delays(Backoff(count: 4, base: .seconds(1), growth: 1)), Array(repeating: .seconds(1), count: 4))
    }

    @Test
    func capBoundsEveryDelay() {
        expectNoDifference(
            delays(Backoff(count: 10, base: .milliseconds(100), cap: .seconds(1))),
            [.milliseconds(100), .milliseconds(200), .milliseconds(400), .milliseconds(800)] + Array(repeating: .seconds(1), count: 6),
        )
    }

    @Test(arguments: [0, -1, -50])
    func countBelowOneSchedulesOneDelay(count: Int) {
        expectNoDifference(delays(Backoff(count: count, base: .milliseconds(250))), [.milliseconds(250)])
    }

    @Test
    func jitterStretchesEachDelayWithinItsBoundsAndStaysUnderTheCap() async {
        let schedules: Generator<Schedule, some Sequence & SendableMetatype> = zip(
            Gen.int(in: 1...16),
            Gen.int64(in: 1...2_000),
            Gen.double(in: 1...4),
            Gen.int64(in: 1...20_000).optional,
            Gen.double(in: 0...1),
        )
        .map { parts in
            Schedule(count: parts.0, base: .milliseconds(parts.1), growth: parts.2, cap: parts.3.map(Duration.milliseconds), jitter: parts.4)
        }
        await propertyCheck(input: schedules, Gen.uint64()) { schedule, seed in
            let scheduled: [Duration] = delays(schedule.backoff, seed: seed)
            expectNoDifference(scheduled.count, schedule.count)
            let outside: [Int] = scheduled.indices.filter { attempt in
                let bounds: ClosedRange<Duration> = schedule.bounds(attempt)
                return scheduled[attempt] < bounds.lowerBound - .nanoseconds(1) || scheduled[attempt] > bounds.upperBound + .nanoseconds(1)
            }
            expectNoDifference(outside, [], "\(scheduled)")
        }
    }

    @Test
    func jitterSpreadsAcrossItsRangeAndOneSeedRepeatsIt() {
        let backoff: Backoff = Backoff(count: 64, base: .seconds(1), growth: 1, jitter: 1)
        let stretches: [Double] = delays(backoff, seed: 7).map { delay in delay / .seconds(1) - 1 }
        let quartiles: [Int] = (0..<4).map { quartile in stretches.count { stretch in Int(stretch * 4) == quartile } }
        expectNoDifference(delays(backoff, seed: 7), delays(backoff, seed: 7))
        expectNoDifference(quartiles.filter { count in count == 0 }, [], "stretches per quartile \(quartiles)")
        expectNoDifference(stretches.filter { stretch in !(0..<1).contains(stretch) }, [])
        let reseeded: Bool = delays(backoff, seed: 7) != delays(backoff, seed: 8)
        #expect(reseeded)
    }
}
