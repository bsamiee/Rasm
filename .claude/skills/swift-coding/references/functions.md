# [FUNCTIONS]

Covers functions as values, from the effects a closure type states to the capabilities an entry point hands each consumer.

## [01]-[EFFECT_ANNOTATIONS]

Closure parameters take the fewest annotations their use requires, so callers pass the most closures. A closure called before return runs on its caller's executor, reads state isolated to its caller, and takes no `@Sendable`:

| [INDEX] | [CLOSURE_RUNS]               | [PARAMETER_TYPE]                                       |
| :-----: | :--------------------------- | :----------------------------------------------------- |
|  [01]   | Before return, on caller     | `(T) async throws(E) -> U`                             |
|  [02]   | Once, in another isolation   | `sending (T) throws(E) -> U`                           |
|  [03]   | Concurrently, in child tasks | `@escaping @Sendable (T) async throws(E) -> U`         |
|  [04]   | Stored, where formed         | `@escaping @isolated(any) @Sendable (T) async -> Void` |
|  [05]   | Stored, on main actor        | `@escaping @MainActor (T) -> Void`                     |

```swift
func measured<Value, Failure: Error, C: Clock<Duration>>(on clock: C, _ work: () async throws(Failure) -> Value) async throws(Failure) -> (value: Value, elapsed: Duration) {
    let start: C.Instant = clock.now
    return (try await work(), start.duration(to: clock.now))
}
```

- `measured` keeps the value and error type that `Clock.measure` drops, its rethrowing closure returns `Void` and widens to `any Error`
- `sending` closures capture non-`Sendable` locals the caller stops using, and a `Task` the callee starts takes one as `sending @escaping`
- Holders call a stored `@isolated(any)` closure with `await` from any isolation, and its `isolation` property names the actor it runs on

## [02]-[FUNCTION_REFERENCES]

Declarations are function values by name, and a closure passing its arguments unchanged to one declaration is that declaration's reference:

| [INDEX] | [DECLARATION]    | [REFERENCE]                     | [TYPE]                                             |
| :-----: | :--------------- | :------------------------------ | :------------------------------------------------- |
|  [01]   | Key path literal | `\Reading.level`                | `(Reading) -> Int`                                 |
|  [02]   | Initializer      | `SKU.init`                      | `(String) throws(OrderError) -> SKU`               |
|  [03]   | Enum case        | `Delivery.inTransit(tracking:)` | `(String) -> Delivery`                             |
|  [04]   | Bound method     | `catalog.price(of:)`            | `(SKU) throws(OrderError) -> Decimal`              |
|  [05]   | Unbound method   | `Catalog.price(of:)`            | `(Catalog) -> (SKU) throws(OrderError) -> Decimal` |

Swift has no curried declarations, and a bound method is its partial application: the receiver holds the values the entry point fixes, the argument is the per-call value, and `catalog.price(of:)` is the function a consumer receives:

```swift
func prices(of codes: [String], in catalog: Catalog) throws(OrderError) -> [Decimal] {
    try codes.map(SKU.init).map(catalog.price(of:))
}
```

Each reference keeps its typed throw, so `map` keeps `OrderError` through both steps with no closure literal.

## [03]-[RULES_AS_VALUES]

Rules a consumer stores, compares, or shows are package values whose members are the functions: a `ClosedRange` or `Set` tests membership, a `KeyPathComparator` orders. Rules nobody reads back are closures:

```swift
func alarms(in readings: [Reading], outside band: ClosedRange<Int>, by order: [KeyPathComparator<Reading>] = [KeyPathComparator(\Reading.level, order: .reverse)]) -> [Reading] {
    readings.filter { reading in !band.contains(reading.level) }.sorted(using: order)
}
```

`band` decodes from configuration and a settings view shows its bounds, and `order` is `Hashable` and `Sendable`, so a view stores the user's sort and passes it unchanged.

## [04]-[CAPABILITIES]

Operations that read and write one record are one requirement taking the transition as a `sending` closure, so the conformance runs read, transition, and write as one isolated step:

```swift
protocol Store<Record>: Sendable {
    associatedtype Record: Identifiable, Sendable where Record.ID: Sendable

    func load(_ id: Record.ID) async throws(OrderError) -> Record?
    func update(_ id: Record.ID, by transition: sending (Record) throws(OrderError) -> Record) async throws(OrderError) -> Record
}

func debit(_ id: Account.ID, by amount: Decimal, in store: some Store<Account>) async throws(OrderError) -> Account {
    try await store.update(id) { account throws(OrderError) in try account.debited(amount) }
}
```

`Account` is `Identifiable` and `Sendable`, and `debited(_:)` throws `OrderError`. Actors conform through synchronous methods with no suspension between the read and the write, and `load`, whose storage cannot fail, drops `throws`. Consumers reading one operation receive the bound requirement, `store.load` typed `(Account.ID) async throws(OrderError) -> Account?`, formed on `some Store<Account>` since an actor's own methods take no partial application. Library functions take each value an app decides (a bundle prefix, a poll interval, a retry count) as a parameter the entry point supplies, a host configuration value whole when the caller sets every field, and the caller's facts alone when the function builds the rest.
