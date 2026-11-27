package import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

package struct AggregateError<Element: Sendable>: Error {
    package let first: Element
    let remaining: [Element]

    package init(first: Element, remaining: [Element] = []) {
        self.first = first
        self.remaining = remaining
    }

    package init?(_ errors: some Collection<Element>) {
        guard let first = errors.first else { return nil }
        self.init(first: first, remaining: Array(errors.dropFirst()))
    }

    package var errors: [Element] { [first] + remaining }
}

extension AggregateError: CustomNSError, LocalizedError where Element: Error {
    package var errorDescription: String? { errors.map(\.localizedDescription).joined(separator: "\n") }

    package var errorUserInfo: [String: Any] { [NSMultipleUnderlyingErrorsKey: errors] }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Result {
    package func aggregated<Next: Sendable>(_ transform: (Failure) -> Next) -> Result<Success, AggregateError<Next>> {
        mapError { error in AggregateError(first: transform(error)) }
    }
}

package func combine<each Value, Failure>(
    _ results: repeat Result<each Value, AggregateError<Failure>>
) -> Result<(repeat each Value), AggregateError<Failure>> {
    var errors: [Failure] = []
    for case .failure(let error) in repeat each results { errors += error.errors }
    return AggregateError(errors).map(Result.failure) ?? Result { () throws(AggregateError<Failure>) in (repeat try (each results).get()) }
}

package func traverse<Element, Value, Failure>(
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
