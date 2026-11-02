import CustomDump
import Foundation
import PropertyBased
import Results
import Synchronization
import Testing

// --- [MODELS] --------------------------------------------------------------------------

private enum Outcome: Equatable {
    case values([Int])
    case errors([String])
}

private struct Rejection: LocalizedError {
    let field: String

    var errorDescription: String? { "rejected \(field)" }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func failures(_ result: Result<Int, AggregateError<String>>) -> [String] {
    switch result {
        case .success: []
        case .failure(let error): error.errors
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct AggregateErrors {
    @Test
    func emptyCollectionBuildsNoAggregate() {
        expectNoDifference(AggregateError([String]())?.errors, nil)
    }

    @Test
    func collectionKeepsEveryElementInOrder() async {
        await propertyCheck(input: Gen.lowercaseLetter.string(of: 0...4).array(of: 1...12)) { errors in
            let aggregate: AggregateError<String> = try #require(AggregateError(errors))
            expectNoDifference(aggregate.first, errors[0])
            expectNoDifference(aggregate.remaining, Array(errors.dropFirst()))
            expectNoDifference(aggregate.errors, errors)
        }
    }

    @Test
    func mapTransformsEveryElementInOrder() {
        let aggregate: AggregateError<Int> = AggregateError(first: 3, remaining: [1, 2]).map { error in error * 10 }
        expectNoDifference(aggregate.errors, [30, 10, 20])
    }

    @Test
    func descriptionListsEachFailureOnItsOwnLine() {
        let aggregate: AggregateError<Rejection> = AggregateError(first: Rejection(field: "name"), remaining: [Rejection(field: "email"), Rejection(field: "age")])
        expectNoDifference(aggregate.errorDescription, "rejected name\nrejected email\nrejected age")
        expectNoDifference(aggregate.localizedDescription, "rejected name\nrejected email\nrejected age")
    }

    @Test
    func bridgedErrorListsUnderlyingErrorsInOrder() throws {
        let aggregate: AggregateError<Rejection> = AggregateError(first: Rejection(field: "name"), remaining: [Rejection(field: "email")])
        let underlying: [any Error] = try #require(aggregate.errorUserInfo[NSMultipleUnderlyingErrorsKey] as? [any Error])
        expectNoDifference(underlying.map(\.localizedDescription), ["rejected name", "rejected email"])
        let bridged: [any Error] = try #require((aggregate as NSError).userInfo[NSMultipleUnderlyingErrorsKey] as? [any Error])
        expectNoDifference(bridged.map(\.localizedDescription), ["rejected name", "rejected email"])
    }

    @Test
    func aggregatedLiftsOneFailureAndKeepsSuccess() {
        let failed: Result<Int, AggregateError<String>> = Result<Int, Rejection>.failure(Rejection(field: "name")).aggregated { error in error.field }
        expectNoDifference(failures(failed), ["name"])
        let succeeded: Result<Int, AggregateError<String>> = Result<Int, Rejection>.success(7).aggregated { error in error.field }
        expectNoDifference(try succeeded.get(), 7)
    }

    @Test
    func combineKeepsEveryFailureInArgumentOrder() async {
        let outcomes: Generator<Result<Int, AggregateError<String>>, some Sequence & SendableMetatype> = zip(Gen.int(in: -9...9), Gen.lowercaseLetter.string(of: 1...3).array(of: 0...3)).map {
            value,
            errors in
            AggregateError(errors).map(Result.failure) ?? .success(value)
        }
        await propertyCheck(input: outcomes, outcomes, outcomes) { first, second, third in
            let errors: [String] = failures(first) + failures(second) + failures(third)
            let expected: Outcome = errors.isEmpty ? .values([first, second, third].compactMap { result in try? result.get() }) : .errors(errors)
            let actual: Outcome =
                switch combine(first, second, third) {
                    case .success(let values): .values([values.0, values.1, values.2])
                    case .failure(let error): .errors(error.errors)
                }
            expectNoDifference(actual, expected)
        }
    }

    @Test
    func combineJoinsSuccessesOfDifferentTypes() throws {
        let values: (Int, String, Bool) = try combine(
            Result<Int, AggregateError<String>>.success(1),
            Result<String, AggregateError<String>>.success("a"),
            Result<Bool, AggregateError<String>>.success(true),
        ).get()
        expectNoDifference([String(values.0), values.1, String(values.2)], ["1", "a", "true"])
    }

    @Test
    func combineReportsFailuresAcrossDifferentSuccessTypes() {
        let combined: Result<(Int, String, Bool), AggregateError<String>> = combine(
            Result<Int, AggregateError<String>>.failure(AggregateError(first: "count", remaining: ["range"])),
            Result<String, AggregateError<String>>.success("a"),
            Result<Bool, AggregateError<String>>.failure(AggregateError(first: "flag")),
        )
        let errors: [String]? =
            switch combined {
                case .success: nil
                case .failure(let error): error.errors
            }
        expectNoDifference(errors, ["count", "range", "flag"])
    }

    @Test
    func traverseRunsEveryStepAndKeepsEveryFailureInOrder() async {
        await propertyCheck(input: Gen.int(in: -50...50).array(of: 0...16)) { elements in
            let visited: Mutex<[Int]> = Mutex([])
            let traversed: Result<[Int], AggregateError<String>> = traverse(elements) { element in
                visited.withLock { visits in visits.append(element) }
                return element.isMultiple(of: 2) ? .success(element * 3) : .failure(AggregateError(first: "odd \(element)", remaining: ["step \(element)"]))
            }
            let odd: [Int] = elements.filter { element in !element.isMultiple(of: 2) }
            let expected: Outcome = odd.isEmpty ? .values(elements.map { element in element * 3 }) : .errors(odd.flatMap { element in ["odd \(element)", "step \(element)"] })
            let actual: Outcome =
                switch traversed {
                    case .success(let values): .values(values)
                    case .failure(let error): .errors(error.errors)
                }
            expectNoDifference(actual, expected)
            expectNoDifference(visited.withLock(\.self), elements)
        }
    }

    @Test
    func traverseOfNoElementsSucceedsEmpty() throws {
        let values: [Int] = try traverse([Int]()) { element in Result<Int, AggregateError<String>>.success(element) }.get()
        expectNoDifference(values, [])
    }
}
