# [RESULTS]

## [01]-[ACCUMULATION]

Independent steps read their own input alone, and a caller receives every rejection at once. Aggregates hold a first element beside the rest, and a failable init over a collection is the one path to no aggregate:

```swift
struct AggregateError<Element: Sendable>: Error {
    let first: Element
    let remaining: [Element]

    var errors: [Element] { [first] + remaining }
}

extension AggregateError {
    init?(_ elements: some Collection<Element>) {
        guard let first = elements.first else { return nil }
        self.init(first: first, remaining: Array(elements.dropFirst()))
    }
}

extension AggregateError: LocalizedError, CustomNSError where Element: Error {
    var errorDescription: String? { errors.map(\.localizedDescription).joined(separator: "\n") }

    var errorUserInfo: [String: Any] { [NSMultipleUnderlyingErrorsKey: errors] }
}

extension Result {
    func aggregated<Element: Sendable>(_ transform: (Failure) -> Element) -> Result<Success, AggregateError<Element>> {
        mapError { failure in AggregateError(first: transform(failure), remaining: []) }
    }
}

func combine<each Value, Element>(_ results: repeat Result<each Value, AggregateError<Element>>) -> Result<(repeat each Value), AggregateError<Element>> {
    var elements: [Element] = []
    for case .failure(let aggregate) in repeat each results { elements += aggregate.errors }
    return AggregateError(elements).map(Result.failure) ?? Result { () throws(AggregateError<Element>) in (repeat try (each results).get()) }
}

func traverse<Input, Value, Element>(_ inputs: some Sequence<Input>, _ transform: (Input) -> Result<Value, AggregateError<Element>>) -> Result<[Value], AggregateError<Element>> {
    let results = inputs.map(transform)
    return AggregateError(results.compactMap { result in result[case: \.failure] }.flatMap(\.errors)).map(Result.failure)
        ?? .success(results.compactMap { result in result[case: \.success] })
}
```

- `init?` in an extension keeps the memberwise `init(first:remaining:)`
- `Element` needs `Sendable` alone, and `aggregated { failure in (field: <name>, failure) }` keys each rejection to its input
- Callers convert a `combine` result once through `get()`
- Pack iteration collects every failure in one pass, and CasePaths makes `Result` case-pathable for `[case: \.failure]`
- IRGen crashes on `[case:]` over a `Result` with a tuple `Success`, and a `switch` reads such a case until a release compiles it

## [02]-[VALIDATION]

Throwing initializers build a record from raw input with every field rejection at once, each field's rule held by its type. `map(Result.success)` lifts a failable constructor into a `Result` with its rejection beside it, `Result(catching:)` lifts a throwing one, and `traverse` validates a list field inside the same `combine`:

```swift
struct Spread: Sendable {
    let lots: Bounded<1, 999>
    let legs: [Contract]

    init(lots: Int, legs: [String]) throws(AggregateError<OrderError>) {
        (self.lots, self.legs) = try combine(
            (Bounded(rawValue: lots).map(Result.success) ?? .failure(OrderError(subject: "lots \(lots)", reason: .outOfRange))).aggregated(\.self),
            traverse(legs) { leg in Result { () throws(OrderError) in try Contract(leg) }.aggregated(\.self) },
        )
        .get()
    }
}
```

`Contract(_:)` parses an exchange contract code and throws `OrderError`, and `OrderError.Reason` holds `outOfRange` beside its other cases. A rejected lot count and every unparsable leg arrive in one aggregate, and cross-field rules run on the values `combine` returns.

## [03]-[WORKFLOWS]

Workflows behind validation lift the dependent steps' error into a one-element aggregate:

```swift
func freeCollateral(lots: Int, legs: [String], in book: Book, posted: Decimal) throws(AggregateError<OrderError>) -> Decimal {
    let spread = try Spread(lots: lots, legs: legs)
    do throws(OrderError) {
        let required = try book.margin(of: spread.legs) * Decimal(spread.lots.rawValue)
        guard required <= posted else { throw OrderError(subject: "\(required) against \(posted)", reason: .insufficientFunds(shortBy: required - posted)) }
        return posted - required
    } catch {
        throw AggregateError(first: error, remaining: [])
    }
}
```

`book.margin(of:)` throws `OrderError`. `do throws(OrderError)` types `error` in its `catch`. Untyped `do` bodies infer their error type from `try` sites alone, and a `throw` statement inside widens it to `any Error`.

## [04]-[TASK_RESULTS]

Tasks, task groups, and `async let` fail with `any Error` alone:

```swift
func quotes(of contracts: Set<Contract>, fetch: @escaping @Sendable (Contract) async throws(OrderError) -> Decimal) async throws(AggregateError<OrderError>) -> [Contract: Decimal] {
    let results: [Result<(Contract, Decimal), OrderError>] = await withTaskGroup { group in
        for contract in contracts {
            group.addTask { await Result { () async throws(OrderError) in try await (contract, fetch(contract)) } }
        }
        return await Array(group)
    }
    return try traverse(results) { result in result.aggregated(\.self) }.map { quotes in Dictionary(uniqueKeysWithValues: quotes) }.get()
}
```

`Contract` is `Hashable` and `Sendable`. Concurrency declares the async `Result(catching:)`, and AsyncAlgorithms builds an `Array` from the group in completion order. Every fetch runs, and `traverse` keeps each contract beside its quote. A fixed count of steps binds each `async let` to a `Result` and reads every binding in one `combine`.

## [05]-[ENTRY_POINTS]

`@main` types state application values, adapt host APIs, then run the work and map its error to host terms once (exit code, response status, text):

```swift
func exitCode(for failures: AggregateError<OrderError>) -> ExitCode {
    ExitCode(failures.errors.contains { failure in failure.reason.is(\.unreadable) } ? EX_IOERR : EX_DATAERR)
}
```

`EX_IOERR` and `EX_DATAERR` come from `sysexits.h` through the platform module. Layers below the host return their typed error unchanged.
