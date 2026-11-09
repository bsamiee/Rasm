import CustomDump
import Foundation
import PropertyBased
import Results
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private struct Refusal: Error, Equatable, LocalizedError {
    let field: String

    var errorDescription: String? { "Refused \(field)" }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Result where Failure == AggregateError<Refusal> {
    fileprivate var succeeded: Success? {
        switch self {
            case .success(let value): value
            case .failure: nil
        }
    }

    fileprivate var failureFields: [String] {
        switch self {
            case .success: []
            case .failure(let errors): errors.errors.map(\.field)
        }
    }
}

@Suite
struct AccumulationTests {
    @Test
    func combinesEverySuccess() throws {
        let combined: Result<(Int, String), AggregateError<Refusal>> = combine(Result.success(1), Result.success("a"))
        let (number, text): (Int, String) = try combined.get()
        expectNoDifference([String(number), text], ["1", "a"])
    }

    @Test
    func combinesEveryFailureInOrder() {
        let combined: Result<(Int, String, Bool), AggregateError<Refusal>> = combine(
            Result<Int, Refusal>.failure(Refusal(field: "width")).aggregated { $0 },
            Result<String, AggregateError<Refusal>>.success("kept"),
            Result<Bool, AggregateError<Refusal>>.failure(AggregateError(first: Refusal(field: "height"), remaining: [Refusal(field: "depth")])),
        )
        expectNoDifference(combined.failureFields, ["width", "height", "depth"])
    }

    @Test
    func traversesEveryElement() async {
        await propertyCheck(input: Gen.int(in: -50...50).array(of: 0...20)) { numbers in
            let traversed: Result<[Int], AggregateError<Refusal>> = traverse(numbers) { number in number < 0 ? .failure(AggregateError(first: Refusal(field: String(number)))) : .success(number * 2) }
            let refused: [String] = numbers.filter { number in number < 0 }.map(String.init)
            let doubled: [Int]? = refused.isEmpty ? numbers.map { number in number * 2 } : nil
            expectNoDifference(traversed.succeeded, doubled)
            expectNoDifference(traversed.failureFields, refused)
        }
    }

    @Test
    func holdsAtLeastOneError() {
        let empty = AggregateError([Refusal]())
        let errors = AggregateError([Refusal(field: "a"), Refusal(field: "b")])
        let underlying = errors?.errorUserInfo[NSMultipleUnderlyingErrorsKey] as? [Refusal]
        let description: String? = errors?.errorDescription
        expectNoDifference(empty?.errors, nil)
        expectNoDifference(description, "Refused a\nRefused b")
        expectNoDifference(underlying, [Refusal(field: "a"), Refusal(field: "b")])
    }

    @Test
    func singlesOutOneMatch() {
        let named: (String) -> String = \.self
        let single = Unmatched.single(["a"], subject: "window", query: "a", candidates: [], named: named)
        let none = Unmatched.single([], subject: "window", query: "z", candidates: ["a", "b"], named: named)
        let many = Unmatched.single(["a", "b"], subject: "window", query: nil, candidates: [], named: named)
        expectNoDifference(single, .success("a"))
        expectNoDifference(none, .failure(Unmatched(subject: "window", query: "z", candidates: ["a", "b"])))
        expectNoDifference(many, .failure(Unmatched(subject: "single window", query: nil, candidates: ["a", "b"])))
    }
}

@Suite
struct RaceTests {
    @Test
    func answersBeforeTheTimeout() async throws {
        let value: Int = try await withTimeout(.seconds(60), expired: Refusal(field: "timeout")) { () async throws(Refusal) -> Int in 1 }
        expectNoDifference(value, 1)
    }

    @Test
    func expiresSlowWork() async {
        let outcome = await Result { () async throws(Refusal) -> Int in
            try await withTimeout(.milliseconds(10), expired: Refusal(field: "timeout")) { () async throws(Refusal) -> Int in
                try? await Task.sleep(for: .seconds(60))
                return 1
            }
        }
        expectNoDifference(outcome, .failure(Refusal(field: "timeout")))
    }

    @Test
    func takesTheFirstOutcome() async throws {
        let value: Int = try await race(
            { () async throws(Refusal) -> Int in 1 },
            against: { () async throws(Refusal) -> Int in
                try? await Task.sleep(for: .seconds(60))
                return 2
            },
        )
        expectNoDifference(value, 1)
    }
}
