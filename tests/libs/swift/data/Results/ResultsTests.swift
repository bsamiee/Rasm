import Clocks
import CustomDump
import Foundation
import PropertyBased
import Results
import Testing

// --- [MODELS] --------------------------------------------------------------------------

struct Refusal: Error, Equatable, LocalizedError {
    let field: String

    var errorDescription: String? { "Refused \(field)" }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private let rows = zip(Gen.frequency((2, Gen.always([Int]())), (1, Gen.int(in: 0...9).array(of: 1...3))), Gen.int(in: -99...99))

private func outcome(_ row: (refused: [Int], value: Int)) -> Result<Int, AggregateError<Refusal>> {
    row.refused.first.map { first in
        .failure(AggregateError(first: Refusal(field: String(first)), remaining: row.refused.dropFirst().map { field in Refusal(field: String(field)) }))
    } ?? .success(row.value)
}

private func refusals<Value>(_ result: Result<Value, AggregateError<Refusal>>) -> [String] {
    switch result {
        case .success: []
        case .failure(let error): error.errors.map(\.field)
    }
}

@Suite
struct AccumulationTests {
    @Test
    func combinesEveryFailureInArgumentOrderElseEverySuccess() async {
        await propertyCheck(input: zip(rows, rows, rows)) { first, second, third in
            let combined = combine(outcome(first), outcome(second), outcome(third)).map { first, second, third in [first, second, third] }
            let refused = (first.0 + second.0 + third.0).map(String.init)
            expectNoDifference(refusals(combined), refused)
            expectNoDifference(try? combined.get(), refused.isEmpty ? [first.1, second.1, third.1] : nil)
        }
    }

    @Test
    func traversesEveryElementKeepingEveryFailureInOrder() async {
        await propertyCheck(input: Gen.int(in: -50...50).array(of: 0...20)) { numbers in
            let traversed = traverse(numbers) { number in
                number < 0 ? .failure(AggregateError(first: Refusal(field: String(number)), remaining: [Refusal(field: "again \(number)")])) : .success(-number)
            }
            let refused = numbers.filter { number in number < 0 }.flatMap { number in [String(number), "again \(number)"] }
            expectNoDifference(refusals(traversed), refused)
            expectNoDifference(try? traversed.get(), refused.isEmpty ? numbers.map { number in -number } : nil)
        }
    }

    @Test
    func holdsAtLeastOneErrorAndBridgesEachAsUnderlying() throws {
        let errors = try #require(AggregateError([Refusal(field: "a"), Refusal(field: "b")]))
        expectNoDifference(AggregateError([Refusal]())?.errors, nil)
        expectNoDifference((errors as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [Refusal], [Refusal(field: "a"), Refusal(field: "b")])
    }

    @Test(
        arguments: [
            ([7], "7", ["c"], .success(7)),
            ([], "9", ["a", "b"], .failure(Unmatched(subject: "window", query: "9", candidates: ["a", "b"]))),
            ([7, 8], nil, ["c"], .failure(Unmatched(subject: "single window", query: nil, candidates: ["#7", "#8"]))),
        ] as [([Int], String?, [String], Result<Int, Unmatched>)]
    )
    func singlesOutOneMatch(matches: [Int], query: String?, candidates: [String], expected: Result<Int, Unmatched>) {
        expectNoDifference(Unmatched.single(matches, subject: "window", query: query, candidates: candidates, named: { match in "#\(match)" }), expected)
    }

    @Test(
        arguments: [
            ("9", [], "No window matching 9"),
            ("9", ["a"], "No window matching 9, candidates: a"),
            (nil, [], "No window"),
            (nil, ["a"], "No window, candidates: a"),
        ] as [(String?, [String], String)]
    )
    func describesTheMissFromItsQueryAndCandidates(query: String?, candidates: [String], expected: String) {
        expectNoDifference(Unmatched(subject: "window", query: query, candidates: candidates).localizedDescription, expected)
    }
}

private func pending(_ value: Int, cancelled: AsyncStream<Int>.Continuation) -> @Sendable () async throws(Refusal) -> Int {
    { () async throws(Refusal) -> Int in
        do { try await Task.sleep(for: .seconds(3600)) } catch { cancelled.yield(value) }
        return value
    }
}

@Suite(.timeLimit(.minutes(1)))
struct RaceTests {
    @Test
    func answersWithTheWorkFinishingBeforeTheTimeout() async throws {
        let value = try await withTimeout(.seconds(5), expired: Refusal(field: "timeout"), clock: TestClock<Duration>()) { () async throws(Refusal) -> Int in 1 }
        expectNoDifference(value, 1)
    }

    @Test
    func expiresAtTheTimeoutAndCancelsTheWork() async {
        let clock = ImmediateClock<Duration>()
        let (interruptions, interrupted) = AsyncStream<Int>.makeStream()
        let ended = await Result { () async throws(Refusal) -> Int in
            try await withTimeout(.seconds(5), expired: Refusal(field: "timeout"), clock: clock, pending(1, cancelled: interrupted))
        }
        interrupted.finish()
        let cancelled = await interruptions.reduce(into: [Int]()) { cancelled, value in cancelled.append(value) }
        expectNoDifference(ended, .failure(Refusal(field: "timeout")))
        expectNoDifference(clock.now, ImmediateClock<Duration>.Instant(offset: .seconds(5)))
        expectNoDifference(cancelled, [1])
    }

    @Test
    func expiresACallbackThatNeverAnswersAtTheTimeout() async {
        let clock = ImmediateClock<Duration>()
        let outcome = await Result { () async throws(Refusal) -> Int in
            try await withTimeout(.seconds(5), expired: Refusal(field: "timeout"), cancelled: Refusal(field: "cancelled"), clock: clock) { _ in }
        }
        expectNoDifference(outcome, .failure(Refusal(field: "timeout")))
        expectNoDifference(clock.now, ImmediateClock<Duration>.Instant(offset: .seconds(5)))
    }

    @Test(arguments: [Result<Int, Refusal>.success(1), .failure(Refusal(field: "host"))])
    func answersWithTheCallbackOutcomeBeforeTheTimeout(answered: Result<Int, Refusal>) async {
        let outcome = await Result { () async throws(Refusal) -> Int in
            try await withTimeout(.seconds(5), expired: Refusal(field: "timeout"), cancelled: Refusal(field: "cancelled"), clock: TestClock<Duration>()) { answer in answer(answered) }
        }
        expectNoDifference(outcome, answered)
    }

    @Test(arguments: [(true, 1, 2), (false, 2, 1)])
    func takesTheFirstOutcomeAndCancelsTheOther(workFinishesFirst: Bool, winner: Int, loser: Int) async throws {
        let (interruptions, interrupted) = AsyncStream<Int>.makeStream()
        let finished: @Sendable (Int) -> @Sendable () async throws(Refusal) -> Int = { value in { () async throws(Refusal) -> Int in value } }
        let won = try await race(
            workFinishesFirst ? finished(1) : pending(1, cancelled: interrupted),
            against: workFinishesFirst ? pending(2, cancelled: interrupted) : finished(2),
        )
        interrupted.finish()
        let cancelled = await interruptions.reduce(into: [Int]()) { cancelled, value in cancelled.append(value) }
        expectNoDifference(won, winner)
        expectNoDifference(cancelled, [loser])
    }
}
