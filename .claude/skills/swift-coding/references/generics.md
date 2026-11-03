# [GENERICS]

## [01]-[CONSTRAINTS]

Constraints name the narrowest protocol a body calls, a standard protocol before a declared one. An `any P<A>` argument opens to its concrete type at a `some P<A>` parameter, and one function serves a single value and a stored existential:

```swift
func median<Element: Comparable>(_ values: some Collection<Element>) -> Element? {
    values.sorted().dropFirst(values.count / 2).first
}

let sources: [any Collection<Decimal>] = [prices, fees.values]
let medians: [Decimal?] = sources.map { source in median(source) }
```

`prices` is a `[Decimal]` and `fees` a `[String: Decimal]`. `median` counts before a second pass and takes `Collection` where one pass takes `Sequence`. Members of `any P<A>` with types reading only constrained associated types (`first`, `count`) run on the existential directly.

## [02]-[OPAQUE_TYPES]

Lazy chains over a `some` parameter have a type no signature names and return as `some Collection<Element>` with no box per element access. Every return path yields one concrete type, and a choice between chains becomes a parameter of one chain:

```swift
func overdrawn(_ accounts: some Collection<Account>, below limit: Decimal = 0) -> some Collection<Account.ID> {
    accounts.lazy.filter { account in account.balance < limit }.map(\.id)
}
```

## [03]-[CONDITIONAL_CONFORMANCES]

Requirements dispatch through the conformance, and other extension members bind statically. Operations a conformer replaces are requirements with a default in an extension. A conditional conformance derives a container's capability from its elements:

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

`Line` conforms with a `Decimal` cost, and `[[Line]?]` costs through both conformances with no member of its own. Generic types state `Equatable`, `Hashable`, and `Codable` where their parameters conform in an empty extension, and synthesis writes the members. `@MainActor` types mark a conformance read off the main actor `nonisolated` (`final class Draft: nonisolated Decodable`). Nonisolated witnesses only initialize the type or read a struct's stored properties.

## [04]-[PARAMETER_PACKS]

Key paths as pack arguments project rows into typed tuples, each column type inferred from its key path:

```swift
func projected<Row, each Value>(_ rows: [Row], _ fields: repeat KeyPath<Row, each Value>) -> [(repeat each Value)] {
    rows.map { row in (repeat row[keyPath: each fields]) }
}

let priced: [(String, Int, Decimal)] = projected(lines, \.sku, \.quantity.rawValue, \.unitPrice)
```

`lines` is a `[Line]`.

## [05]-[INTEGER_GENERIC_PARAMETERS]

Integer generic parameters state a length shared across positions: the compiler infers `N` from argument types and rejects mismatched lengths. `[N of T]` holds its elements inline with no allocation. A result of the same length builds each element from its index through `InlineArray.init(_:)`:

```swift
func blended<let N: Int>(_ from: [N of Double], _ to: [N of Double], at fraction: Double) -> [N of Double] {
    InlineArray { index in from[index] + (to[index] - from[index]) * fraction }
}
```

`N` reads as an `Int` in the body. `init(first:next:)` unfolds each element from its predecessor (`InlineArray(first: 1) { previous in previous * 2 }`).

## [06]-[MACROS]

`@CasePathable` gives each case a `Hashable` key path, and `allCasePaths` lists them in declaration order. Reports over every case take rows from `allCasePaths`:

```swift
let report: [(path: PartialCaseKeyPath<Delivery>, count: Int)] = Delivery.allCasePaths.map { path in (path, deliveries.count { delivery in delivery.is(path) }) }
```

`deliveries` is a `[Delivery]` of a `@CasePathable` enum, and a new case enters the report with no edit. `case` returns a value's key path, the key of a table or a grouping by case.
