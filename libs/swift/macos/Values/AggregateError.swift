// --- [ERRORS] --------------------------------------------------------------------------

public struct AggregateError<Element: Sendable>: Error {
    public let first: Element
    public let remaining: [Element]

    public init(first: Element, remaining: [Element] = []) {
        self.first = first
        self.remaining = remaining
    }

    public var errors: [Element] { [first] + remaining }

    public static func + (left: Self, right: Self) -> Self {
        Self(first: left.first, remaining: left.remaining + right.errors)
    }

    public func map<Next>(_ transform: (Element) -> Next) -> AggregateError<Next> {
        AggregateError<Next>(first: transform(first), remaining: remaining.map(transform))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Result {
    public var failure: Failure? {
        if case .failure(let error) = self { error } else { nil }
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

public func combine<First, Second, Failure>(
    _ first: Result<First, AggregateError<Failure>>,
    _ second: Result<Second, AggregateError<Failure>>,
) -> Result<(First, Second), AggregateError<Failure>> {
    switch (first, second) {
        case (.success(let first), .success(let second)): .success((first, second))
        case (.failure(let first), .failure(let second)): .failure(first + second)
        case (.failure(let first), .success): .failure(first)
        case (.success, .failure(let second)): .failure(second)
    }
}

public func combine<First, Second, Third, Failure>(
    _ first: Result<First, AggregateError<Failure>>,
    _ second: Result<Second, AggregateError<Failure>>,
    _ third: Result<Third, AggregateError<Failure>>,
) -> Result<(First, Second, Third), AggregateError<Failure>> {
    combine(combine(first, second), third).map { pair in (pair.0.0, pair.0.1, pair.1) }
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
    return split.errors.first.map { first in .failure(AggregateError(first: first, remaining: Array(split.errors.dropFirst()))) }
        ?? .success(split.values)
}
