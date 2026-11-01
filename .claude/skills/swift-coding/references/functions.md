# [FUNCTIONS]

Functions as values and the effects, errors, and dependencies their types state.

## [01]-[EFFECT_ANNOTATIONS]

Closure parameters take the fewest annotations their use requires, and callers pass the most closures:

```swift
func measured<Value, Failure: Error, C: Clock<Duration>>(on clock: C, _ work: () async throws(Failure) -> Value) async throws(Failure) -> (value: Value, elapsed: Duration) {
    let start: C.Instant = clock.now
    return (try await work(), start.duration(to: clock.now))
}
```

- `Clock.measure` takes a `Void` closure and rethrows `any Error`, `measured` keeps the closure's value and error type
- Closures run once in another isolation take `sending (T) throws(E) -> U` and capture non-`Sendable` locals the caller stops using
- Closures stored to run where they were formed take `@escaping @isolated(any) @Sendable (T) async -> Void`, holders call them with `await` from any isolation, and the `isolation` property names the actor each runs on

## [02]-[FUNCTION_REFERENCES]

Swift has no curried declarations, and a bound method is their partial application: the receiver holds values the entry point fixes, the argument is the per-call value, and `catalog.price(of:)` is the function a consumer receives:

```swift
func prices(of codes: [String], in catalog: Catalog) throws(OrderError) -> [Decimal] {
    try codes.map(SKU.init).map(catalog.price(of:))
}
```

References keep their typed throw, and `map` keeps `OrderError` through both steps with no closure literal.

## [03]-[PREDICATE_VALUES]

Rules a consumer stores, compares, or shows are package values with the functions as members: a `ClosedRange` or `Set` tests membership, a `KeyPathComparator` orders. Rules nobody reads back are closures:

```swift
func alarms(in readings: [Reading], outside band: ClosedRange<Int>, by order: [KeyPathComparator<Reading>] = [KeyPathComparator(\Reading.level, order: .reverse)]) -> [Reading] {
    readings.filter { reading in !band.contains(reading.level) }.sorted(using: order)
}
```

`band` decodes from configuration, where an inverted pair fails with `DecodingError.dataCorrupted`, and a settings view shows its bounds. `order` is `Hashable` and `Sendable`, and a view stores the user's sort and passes it unchanged.

## [04]-[DEPENDENCIES]

Libraries own their host calls, take each value an application decides as a defaulted parameter, and build host configuration inside. Operations that read and write one record are one requirement taking the transition as a `sending` closure, and the conformance runs read, transition, and write as one isolated step:

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

`Account` is `Identifiable` and `Sendable`, and `debited(_:)` throws `OrderError`. Actors conform through synchronous methods with no suspension between read and write, and a witness over storage that cannot fail drops `throws`. Actor-isolated methods take no partial application, and a consumer reading one operation receives the requirement bound on `some Store<Account>`: `store.load`, typed `(Account.ID) async throws(OrderError) -> Account?`.
