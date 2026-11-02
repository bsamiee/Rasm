import CustomDump
import Results
import Synchronization
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private enum Probe: Error, Equatable {
    case work
    case rival
    case expired
}

private final class Cancellation: Sendable {
    let observed: Atomic<Bool> = Atomic(false)

    var canceled: Bool { observed.load(ordering: .sequentiallyConsistent) }

    func park() async {
        await withTaskCancellationHandler {
            let (stream, continuation): (AsyncStream<Never>, AsyncStream<Never>.Continuation) = AsyncStream<Never>.makeStream()
            for await _: Never in stream {}
            continuation.finish()
        } onCancel: {
            observed.store(true, ordering: .sequentiallyConsistent)
        }
    }
}

private struct ImmediateClock: Clock {
    let now: ContinuousClock.Instant
    let deadlines: Deadlines

    var minimumResolution: Duration { .zero }

    func sleep(until deadline: ContinuousClock.Instant, tolerance: Duration?) async throws {
        deadlines.requested.withLock { requested in requested.append(deadline) }
        await Task.yield()
    }
}

private final class Deadlines: Sendable {
    let requested: Mutex<[ContinuousClock.Instant]> = Mutex([])
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Races {
    @Test
    func firstSuccessWinsAndCancelsTheRival() async throws {
        let rival: Cancellation = Cancellation()
        let value: Int = try await race(
            { () async throws(Probe) -> Int in 1 },
            against: { () async throws(Probe) -> Int in
                await rival.park()
                return 2
            },
        )
        expectNoDifference(value, 1)
        #expect(rival.canceled)
    }

    @Test
    func rivalSuccessWinsAndCancelsTheWork() async throws {
        let work: Cancellation = Cancellation()
        let value: Int = try await race(
            { () async throws(Probe) -> Int in
                await work.park()
                return 1
            },
            against: { () async throws(Probe) -> Int in 2 },
        )
        expectNoDifference(value, 2)
        #expect(work.canceled)
    }

    @Test
    func firstFailureWinsAndCancelsTheRival() async {
        let rival: Cancellation = Cancellation()
        let failure: Probe? = await #expect(throws: Probe.self) {
            try await race(
                { () async throws(Probe) -> Int in throw Probe.work },
                against: { () async throws(Probe) -> Int in
                    await rival.park()
                    return 2
                },
            )
        }
        expectNoDifference(failure, .work)
        #expect(rival.canceled)
    }

    @Test
    func rivalFailureWinsOverParkedWork() async {
        let work: Cancellation = Cancellation()
        let failure: Probe? = await #expect(throws: Probe.self) {
            try await race(
                { () async throws(Probe) -> Int in
                    await work.park()
                    return 1
                },
                against: { () async throws(Probe) -> Int in throw Probe.rival },
            )
        }
        expectNoDifference(failure, .rival)
        #expect(work.canceled)
    }
}

struct Timeouts {
    @Test
    func workFinishingFirstReturnsItsValue() async throws {
        let value: String = try await withTimeout(.seconds(3600), expired: Probe.expired) { () async throws(Probe) -> String in "done" }
        expectNoDifference(value, "done")
    }

    @Test
    func workFailureFinishingFirstPropagatesUnreplaced() async {
        let failure: Probe? = await #expect(throws: Probe.self) {
            try await withTimeout(.seconds(3600), expired: Probe.expired) { () async throws(Probe) -> String in throw Probe.work }
        }
        expectNoDifference(failure, .work)
    }

    @Test
    func expiredDeadlineThrowsTheGivenFailureAndCancelsTheWork() async {
        let work: Cancellation = Cancellation()
        let clock: ImmediateClock = ImmediateClock(now: ContinuousClock.now, deadlines: Deadlines())
        let failure: Probe? = await #expect(throws: Probe.self) {
            try await withTimeout(.milliseconds(250), expired: Probe.expired, clock: clock) { () async throws(Probe) -> String in
                await work.park()
                return "late"
            }
        }
        expectNoDifference(failure, .expired)
        #expect(work.canceled)
        expectNoDifference(clock.deadlines.requested.withLock(\.self), [clock.now.advanced(by: .milliseconds(250))])
    }
}
