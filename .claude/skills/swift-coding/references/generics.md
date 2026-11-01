# [GENERICS]

Covers code generic over the types it serves, from constraints and opaque returns to derived conformances, packs, and macros.

## [01]-[CONSTRAINTS]

Constraints name the narrowest protocol a body calls, a standard protocol before a declared one. Named generic parameters state a type or `let` length shared across positions, an element type relating values to a result or a length making `[N of T]` arguments of different lengths a compile error. An `any P<A>` argument opens to its concrete type at a `some P<A>` parameter, so one function serves a single value and a stored existential:

```swift
func percentile<Element: Comparable>(_ values: some Collection<Element>, at fraction: Double = 0.5) -> Element? {
    values.sorted().dropFirst(Int(Double(values.count) * fraction)).first
}

let sources: [any Collection<Decimal>] = [prices, fees.values]
let medians: [Decimal?] = sources.map { source in percentile(source) }
```

`prices` is a `[Decimal]` and `fees` a `[String: Decimal]`. `percentile` counts before a second pass and takes `Collection` where one pass takes `Sequence`. Members of `any P<A>` with types reading only constrained associated types (`first`, `count`) run on the existential directly.

## [02]-[OPAQUE_RETURNS]

Opaque returns hide a concrete type from callers while the compiler keeps it, so a lazy chain returns as `some Collection<Element>` with no box per element access. Every return path yields one concrete type, so a choice between chains becomes a parameter of one chain:

```swift
func pending(_ orders: some Collection<Order>, before cutoff: Date = .distantFuture) -> some Collection<Order.ID> {
    orders.lazy.filter { order in order.placed < cutoff }.map(\.id)
}
```

`Order` is `Identifiable` with a `placed` date, and the default cutoff keeps every order. Lazy results recompute on each pass, a caller reading one twice takes `Array(_:)` once.

## [03]-[DERIVED_CONFORMANCES]

Requirements state what a conformer supplies, and a constrained extension derives every other operation once. Requirements dispatch through the conformance and extension members bind statically, so an operation a conformer replaces is a requirement with its default in an extension. Conformance to a standard protocol (`Collection`, `AsyncSequence`) derives every standard algorithm, and a conditional conformance derives a container's capability from its elements:

```swift
protocol Costed<Cost> {
    associatedtype Cost: AdditiveArithmetic

    var cost: Cost { get }
}

extension Array: Costed where Element: Costed {
    var cost: Element.Cost { lazy.map(\.cost).reduce(.zero, +) }
}

extension Optional: Costed where Wrapped: Costed {
    var cost: Wrapped.Cost { self?.cost ?? .zero }
}
```

`Line` conforms with a `Decimal` cost, and `[[Line]?]` costs through both conformances with no member of its own. `.zero` comes from `AdditiveArithmetic`, so an empty array and an absent value cost nothing in every cost type. Generic types state `Equatable`, `Hashable`, and `Codable` where their parameters conform in an empty extension, and synthesis writes the members.

## [04]-[PARAMETER_PACKS]

Parameter packs state a list of types of any length with each element's type kept, so one declaration serves every arity. Key paths as pack arguments project rows into typed tuples, each column type inferred from its key path:

```swift
func projected<Row, each Value>(_ rows: [Row], _ fields: repeat KeyPath<Row, each Value>) -> [(repeat each Value)] {
    rows.map { row in (repeat row[keyPath: each fields]) }
}

let priced: [(String, Int, Decimal)] = projected(lines, \.sku, \.quantity, \.price)
```

`lines` is a `[Line]` with `sku`, `quantity`, and `price`. `repeat` expands its pattern once per pack element inside a tuple, an argument list, or a generic argument list. Packs expanded in one pattern (`repeat (each Key, each Value)`) share a length, and the compiler rejects a call with packs of different lengths.

## [05]-[MACROS]

Macros derive members from a declaration's own syntax, so a declaration states each case or stored property once. `@CasePathable` gives each case a `Hashable` key path, and a table keyed by case takes its keys from the enum:

```swift
let counts: [PartialCaseKeyPath<Delivery>: Int] = Dictionary(grouping: deliveries, by: \.case).mapValues(\.count)
let report: [(path: PartialCaseKeyPath<Delivery>, count: Int)] = Delivery.allCasePaths.map { path in (path, counts[path, default: 0]) }
```

`deliveries` is a `[Delivery]` of a `@CasePathable` enum. `case` returns a value's case key path and `allCasePaths` lists every case in declaration order, so a new case enters the report with no edit.
