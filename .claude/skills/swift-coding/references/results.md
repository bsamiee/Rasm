# [RESULTS]

## [01]-[ACCUMULATION]

Independent steps read their own input alone, and a caller receives every rejection at once:

```swift
struct Failures<Failure: Error>: Error {
    let all: [Failure]
}

func combine<each Value, Failure: Error>(_ results: repeat Result<each Value, Failure>) throws(Failures<Failure>) -> (repeat each Value) {
    do {
        return try (repeat (each results).get())
    } catch {
        var failures: [Failure] = []
        for case .failure(let failure) in repeat each results {
            failures.append(failure)
        }
        throw Failures(all: failures)
    }
}

func combine<Value, Failure: Error>(_ results: [Result<Value, Failure>]) throws(Failures<Failure>) -> [Value] {
    let failures: [Failure] = results.compactMap { result in result[case: \.failure] }
    guard failures.isEmpty else { throw Failures(all: failures) }
    return results.compactMap { result in result[case: \.success] }
}
```

Results with different value types build a tuple through `get()`, and the first failure makes `combine` collect every failure in one pack iteration. CasePaths makes `Result` case-pathable, and `[case: \.failure]` reads one case of an array element without a `switch`.

## [02]-[VALIDATION]

Throwing initializers build a record from raw input with every field rejection at once, each field's rule held by its type. `map(Result.success)` lifts a failable constructor into a `Result` with its rejection beside it, and `Result(catching:)` lifts a throwing one:

```swift
struct Order: Sendable {
    let quantity: Bounded<1, 999>
    let sku: SKU

    init(quantity: Int, sku: String) throws(Failures<OrderError>) {
        (self.quantity, self.sku) = try combine(
            Bounded(rawValue: quantity).map(Result.success) ?? .failure(OrderError(subject: "quantity \(quantity)", reason: .outOfRange)),
            Result { () throws(OrderError) in try SKU(sku) },
        )
    }
}
```

`OrderError.Reason` holds `outOfRange` beside its other cases. Cross-field rules run on the values `combine` returns.

## [03]-[WORKFLOWS]

Dependent steps run in sequence, and the first throw ends the workflow. Workflows behind validation lift the dependent steps' error into `Failures`:

```swift
func checkout(quantity: Int, sku: String, in catalog: Catalog, balance: Decimal) throws(Failures<OrderError>) -> Decimal {
    let order: Order = try Order(quantity: quantity, sku: sku)
    do throws(OrderError) {
        let total: Decimal = try catalog.price(of: order.sku) * Decimal(order.quantity.rawValue)
        guard total <= balance else { throw OrderError(subject: "\(total) against \(balance)", reason: .insufficientFunds(shortBy: total - balance)) }
        return balance - total
    } catch {
        throw Failures(all: [error])
    }
}
```

`do throws(OrderError)` types `error` in its `catch`. Untyped `do` bodies infer their error type from `try` sites alone, and a `throw` statement inside widens it to `any Error`.

## [04]-[TASK_RESULTS]

Tasks, task groups, and `async let` fail with `any Error` alone:

```swift
func prices(of skus: Set<String>, fetch: @escaping @Sendable (String) async throws(OrderError) -> Decimal) async throws(Failures<OrderError>) -> [String: Decimal] {
    let results: [Result<(String, Decimal), OrderError>] = await withTaskGroup { group in
        for sku: String in skus {
            group.addTask { await Result { () async throws(OrderError) in try await (sku, fetch(sku)) } }
        }
        return await Array(group)
    }
    return try Dictionary(uniqueKeysWithValues: combine(results))
}
```

Concurrency declares the async `Result(catching:)`, and AsyncAlgorithms builds an `Array` from the group in completion order. Every fetch runs, and `combine` keeps each key beside its price. A fixed count of steps binds each `async let` to a `Result` and reads every binding in one `try await combine(price, stock)`.

## [05]-[ENTRY_POINTS]

`@main` types state application values, adapt host APIs, then run the work and map its error to host terms once (exit code, response status, text):

```swift
func exitCode(for failures: Failures<OrderError>) -> ExitCode {
    ExitCode(failures.all.contains { failure in failure.reason.is(\.unreadable) } ? EX_IOERR : EX_DATAERR)
}
```

`EX_IOERR` and `EX_DATAERR` come from `sysexits.h` through the platform module. Layers below the host return their typed error unchanged.
