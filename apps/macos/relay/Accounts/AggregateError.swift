// --- [ERRORS] --------------------------------------------------------------------------

nonisolated struct AggregateError<Element: Sendable>: Error {
    let first: Element
    let remaining: [Element]

    var errors: [Element] { [first] + remaining }

    func appending(_ other: Self) -> Self {
        Self(first: first, remaining: remaining + other.errors)
    }

    func map<Next: Sendable>(_ transform: (Element) -> Next) -> AggregateError<Next> {
        AggregateError<Next>(first: transform(first), remaining: remaining.map(transform))
    }
}

nonisolated extension AggregateError {
    init?(collecting errors: [Element]) {
        guard let first: Element = errors.first else { return nil }
        self.init(first: first, remaining: Array(errors.dropFirst()))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated extension Result {
    var failure: Failure? {
        if case .failure(let error) = self { error } else { nil }
    }

    func bind<Next>(
        _ next: (Success) async -> Result<Next, Failure>
    ) async -> Result<Next, Failure> {
        switch self {
            case .success(let value): await next(value)
            case .failure(let error): .failure(error)
        }
    }
}

nonisolated func combine<First, Second, Failure>(
    _ first: Result<First, AggregateError<Failure>>,
    _ second: Result<Second, AggregateError<Failure>>,
) -> Result<(First, Second), AggregateError<Failure>> {
    switch (first, second) {
        case (.success(let first), .success(let second)): .success((first, second))
        case (.failure(let first), .failure(let second)): .failure(first.appending(second))
        case (.failure(let first), .success): .failure(first)
        case (.success, .failure(let second)): .failure(second)
    }
}

nonisolated func combine<First, Second, Third, Failure>(
    _ first: Result<First, AggregateError<Failure>>,
    _ second: Result<Second, AggregateError<Failure>>,
    _ third: Result<Third, AggregateError<Failure>>,
) -> Result<(First, Second, Third), AggregateError<Failure>> {
    combine(combine(first, second), third).map { pair in (pair.0.0, pair.0.1, pair.1) }
}

nonisolated func traverse<Element, Value, Failure>(
    _ elements: some Sequence<Element>,
    _ transform: (Element) -> Result<Value, AggregateError<Failure>>,
) -> Result<[Value], AggregateError<Failure>> {
    elements.reduce(.success([])) { outcome, element in
        combine(outcome, transform(element)).map { values, value in values + [value] }
    }
}
