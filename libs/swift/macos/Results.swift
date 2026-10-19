// --- [ERRORS] --------------------------------------------------------------------------

public struct AggregateError<Element: Sendable>: Error {
    public let first: Element
    public let remaining: [Element]

    public init(first: Element, remaining: [Element] = []) {
        self.first = first
        self.remaining = remaining
    }

    public init?(_ errors: [Element]) {
        guard let first: Element = errors.first else { return nil }
        self.init(first: first, remaining: Array(errors.dropFirst()))
    }

    public var errors: [Element] { [first] + remaining }

    public func map<Next>(_ transform: (Element) -> Next) -> AggregateError<Next> {
        AggregateError<Next>(first: transform(first), remaining: remaining.map(transform))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Result {
    public var failure: Failure? {
        if case .failure(let error) = self { error } else { nil }
    }

    public func aggregated<Next: Sendable>(_ transform: (Failure) -> Next) -> Result<Success, AggregateError<Next>> {
        mapError { error in AggregateError(first: transform(error)) }
    }

    public func bind<Next>(
        _ next: (Success) async -> Result<Next, Failure>
    ) async -> Result<Next, Failure> {
        switch self {
            case .success(let value): await next(value)
            case .failure(let error): .failure(error)
        }
    }
}

public func combine<each Value, Failure>(
    _ results: repeat Result<each Value, AggregateError<Failure>>
) -> Result<(repeat each Value), AggregateError<Failure>> {
    var errors: [Failure] = []
    for failures: [Failure] in repeat (each results).failure?.errors ?? [] {
        errors += failures
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
