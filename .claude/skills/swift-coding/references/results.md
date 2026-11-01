# [RESULTS]

Covers typed outcome flows, from a module's error type through validation and workflows to translation at the host.

## [01]-[ERROR_TYPE]

Each throwing module declares one error. The subject names the rejected value, the reason enum holds every fact a consumer decides on, and the text derives from both:

```swift
struct OrderError: Error, CustomStringConvertible {
    @CasePathable
    enum Reason: Sendable {
        case missing
        case outOfRange
        case malformed
        case insufficientFunds(shortBy: Decimal)
        case unreadable(any Error)
    }

    let subject: String
    let reason: Reason

    var description: String { "\(subject): \(reason)" }

    static func checked<Value>(_ value: Value, _ reason: Reason, where valid: (Value) -> Bool) -> Result<Value, Self> {
        valid(value) ? .success(value) : .failure(Self(subject: String(reflecting: value), reason: reason))
    }
}
```

- Reason cases hold a fact only where some failures carry one (`shortBy`, a host cause), a case name states the rule
- Causes keep the type their source throws: `Errno` from `FileDescriptor.open`, another module's error whole, `any Error` from an untyped host member
- `Error` refines `Sendable`, so every reason fact is `Sendable` and a cause crosses tasks whole
- Text renders the case name and its facts, and consumers classify through `reason`, with a `switch` or a case path
- `checked` turns one rule over one value into a `Result`, the subject rendered from the rejected value

Untyped host members convert at the call:

```swift
func prices(at url: URL) throws(OrderError) -> [String: Decimal] {
    do {
        return try JSONDecoder().decode([String: Decimal].self, from: Data(contentsOf: url))
    } catch {
        throw OrderError(subject: url.path(percentEncoded: false), reason: .unreadable(error))
    }
}
```

## [02]-[ACCUMULATION]

Independent steps read their own input alone, and a caller wants every rejection at once. `Failures` and both `combine` overloads sit once in a module every throwing module imports:

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
            failures += [failure]
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

Tuples of results with different value types build through `get()`, and the first failure sends `combine` through one pack iteration collecting every failure. CasePaths makes `Result` case-pathable, so `[case: \.failure]` reads one case of an array element without a `switch`.

## [03]-[VALIDATION]

Construction is validation: a record's throwing initializer states each field rule once, and every consumer reads the record as proof:

```swift
struct Order: Sendable {
    let name: String
    let quantity: Int
    let sku: String

    init(name: String, quantity: Int, sku: String) throws(Failures<OrderError>) {
        (self.name, self.quantity, self.sku) = try combine(
            OrderError.checked(name, .missing) { text in !text.isEmpty },
            OrderError.checked(quantity, .outOfRange, where: (1...999).contains),
            OrderError.checked(sku, .malformed) { code in code.wholeMatch(of: /[A-Z0-9]{8}/) != nil },
        )
    }
}
```

- Initializers in the struct body replace the memberwise one, so every `Order` value passed each rule
- Fields keep their standard types, and each rule is a value the call states once (a range's `contains`, a regex literal)
- Shared rules go in a value type of their own, whose `init(_:) throws(OrderError)` runs the rule
- Cross-field rules run on the values `combine` returns, each field already valid

## [04]-[WORKFLOWS]

Dependent steps run in sequence. Each binds its value with `try` or `guard let`, and the first throw ends the workflow:

```swift
func charge(_ order: Order, prices: [String: Decimal], balance: Decimal) throws(OrderError) -> Decimal {
    guard let price: Decimal = prices[order.sku] else { throw OrderError(subject: order.sku, reason: .missing) }
    let total: Decimal = price * Decimal(order.quantity)
    guard total <= balance else { throw OrderError(subject: "\(total)", reason: .insufficientFunds(shortBy: total - balance)) }
    return balance - total
}
```

Workflows composing validation with dependent steps throw `Failures<OrderError>`, and a `catch` around the dependent steps throws `Failures(all: [error])`.

## [05]-[STORED_OUTCOMES]

`Result` holds an outcome as a value: a child task's return, a collection element, a cache entry, a field whose fact fails apart from its neighbors. Tasks and task groups fail with `any Error` alone, so each child returns its typed outcome and the parent decides once:

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

Concurrency declares the async `Result(catching:)`, and AsyncAlgorithms builds an `Array` from the group in completion order. Every fetch runs, and `combine` keeps each key beside its price. `.get()` resumes the throwing form, and `map`, `flatMap`, and `mapError` transform a stored outcome without resuming it.

## [06]-[HOST_TRANSLATION]

Hosts translate an error into their own terms once (an exit code, a response status, rendered text):

```swift
func exitCode(for failures: Failures<OrderError>) -> ExitCode {
    ExitCode(failures.all.contains { failure in failure.reason.is(\.unreadable) } ? EX_IOERR : EX_DATAERR)
}
```

`EX_IOERR` and `EX_DATAERR` come from `sysexits.h` through the platform module. Layers below the host return their typed error unchanged and log nothing about it, and the host logs a failure once where it translates it.
