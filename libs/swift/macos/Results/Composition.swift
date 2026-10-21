// --- [OPERATIONS] ----------------------------------------------------------------------

extension Result {
    public func aggregated<Next: Sendable>(_ transform: (Failure) -> Next) -> Result<Success, AggregateError<Next>> {
        mapError { error in AggregateError(first: transform(error)) }
    }
}

public func combine<each Value, Failure>(
    _ results: repeat Result<each Value, AggregateError<Failure>>
) -> Result<(repeat each Value), AggregateError<Failure>> {
    var errors: [Failure] = []
    for case .failure(let error) in repeat each results {
        errors += error.errors
    }
    return AggregateError(errors).map(Result.failure)
        ?? Result { () throws(AggregateError<Failure>) in (repeat try (each results).get()) }
}

public func traverse<Element, Value, Failure>(
    _ elements: some Sequence<Element>,
    _ transform: (Element) -> Result<Value, AggregateError<Failure>>,
) -> Result<[Value], AggregateError<Failure>> {
    let split: (values: [Value], errors: [Failure]) = elements.reduce(into: ([], [])) { split, element in
        switch transform(element) {
            case .success(let value): split.values.append(value)
            case .failure(let error): split.errors.append(contentsOf: error.errors)
        }
    }
    return AggregateError(split.errors).map(Result.failure) ?? .success(split.values)
}

public func withTimeout<Value: Sendable, Failure: DeadlineFailure>(
    _ timeout: Duration,
    _ work: @escaping @Sendable () async -> Result<Value, Failure>,
) async -> Result<Value, Failure> {
    await withTaskGroup { group in
        group.addTask(name: "work") { await work() }
        group.addTask(name: "timeout") {
            await Result { try await Task.sleep(for: timeout) }
                .mapError { _ in Failure.cancelled }
                .flatMap { _ in .failure(.timedOut(timeout)) }
        }
        let first: Result<Value, Failure> = await group.next() ?? .failure(.cancelled)
        group.cancelAll()
        return first
    }
}
