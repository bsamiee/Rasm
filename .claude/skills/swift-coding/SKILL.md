---
name: swift-coding
description: "Use when writing or reviewing Swift, covering functional design with typed throws, value types, and structured concurrency"
---

# [SWIFT_CODING]

Covers Swift design decisions from signature shape to host boundary. The standard library, Swift Concurrency, Observation, Synchronization, System, swift-collections, swift-async-algorithms, swift-case-paths, swift-subprocess, and swift-argument-parser are part of the language, and code reaches for their types wherever they state a value.

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Worked flows for closure effects, function references, rules as values, and capabilities
- [02]-[RESULTS](references/results.md): Worked flows for the error type, accumulation, validation, workflows, stored outcomes, and host translation
- [03]-[DATA](references/data.md): Worked flows for state machines, transitions, collections, and recursive folds
- [04]-[OWNERSHIP](references/ownership.md): Noncopyable resources, typestate, borrowed views, inline storage, and synchronized state
- [05]-[CONCURRENCY](references/concurrency.md): Isolation, regions, task trees, deadlines, actors, cancellation, and callback bridges
- [06]-[STREAMS](references/streams.md): Async sequence sources, typed failure, operators, observation, pacing, and sharing
- [07]-[BOUNDARIES](references/boundaries.md): Decoding at the edge, child processes, command lines, logging, and the entry point
- [08]-[GENERICS](references/generics.md): Primary associated types, opaque returns, derived operations, packs, value generics, builders, and macros

## [01]-[FUNCTIONS]

Swift function types state every effect of a call, each annotation exactly where the function performs it:

| [INDEX] | [ANNOTATION]             | [STATES]                                                               |
| :-----: | :----------------------- | :--------------------------------------------------------------------- |
|  [01]   | `throws(E)`              | Fails with `E`, `throws(Never)` and no annotation never fail           |
|  [02]   | `async`                  | Suspends on its caller's executor                                      |
|  [03]   | `@concurrent`            | Runs on the global executor, off its caller                            |
|  [04]   | `@Sendable`              | Crosses isolation, captures only `Sendable` values                     |
|  [05]   | `sending`                | Moves the value to another isolation region, the sender stops using it |
|  [06]   | `@isolated(any)`         | Keeps the isolation the closure was formed in                          |
|  [07]   | `borrowing`, `consuming` | Reads the argument, or takes ownership of it                           |
|  [08]   | `inout`                  | Replaces the caller's value when the callee returns                    |

- Members differing only in a number, sign, flag, or applied function are one member taking it as a parameter
- Defaulted parameters hold a caller's choice with its usual value, overloads exist only for inputs of different types
- Operations with a natural receiver are methods or computed properties in an extension of that type, other operations are module functions
- Declarations stay `internal` until another module calls them, `package` within one package, `public` for a product's API
- `public import` names a module whose types a public signature states, every other import stays internal

### [01.1]-[HONEST_SIGNATURES]

Signatures are honest when every input they accept reaches a declared outcome. Narrow the input to a validated type or widen the outcome to an `Optional` or a typed throw:
- Failable `init?` constructs a value whose only failure is absence, `init(_:) throws(E)` constructs one whose failure has a reason
- Higher-order functions are generic over their closure's error, taking `(T) throws(E) -> U` and throwing `E` themselves
- Traps (`precondition`, `fatalError`) mark a developer defect alone, an input from outside the module reaches a typed throw

```swift
func total<E: Error>(of lines: [Line], pricedBy price: (Line) throws(E) -> Decimal) throws(E) -> Decimal {
    try lines.map(price).reduce(0, +)
}
```

`map`, `filter`, `count(where:)`, `Optional.map`, and `Optional.flatMap` keep `E`. Rethrowing members (`reduce`, `compactMap`, `first(where:)`, `contains(where:)`, `allSatisfy`, `sorted(by:)`) widen `E` to `any Error`, so the typed step runs through a typed member and the rethrowing member takes the non-throwing remainder.

### [01.2]-[PURITY]

Pure functions return results that depend on their arguments alone and change nothing outside them. The compiler rejects unguarded global mutable state, so impurity enters through I/O, isolated state, and clocks. Nonisolated synchronous functions over `Sendable` values that perform no I/O are pure and run on any task. Mutation of a local `var` that never escapes is no side effect, and a `mutating` method or `inout` parameter updates the caller's own copy. The current instant and the user's locale enter as arguments, and a function that waits or measures takes `some Clock<Duration>`.

### [01.3]-[CAPABILITIES]

Dependencies are the operations a consumer reads, and the count of operations over one resource selects the form:
- One operation is a function value: a lookup `(Key) async throws(E) -> Item?`, a persistence step `(Item) async throws(E) -> Void`
- Operations that share one resource are a protocol with a primary associated type, consumed as `some Store<Item>`
- Libraries take delays, limits, paths, and vocabularies as parameters, and the composition states each value one application owns

## [02]-[EXPRESSIONS]

`if` and `switch` are expressions when every branch is one expression, and a binding, `return`, or property takes the expression whole. A branch that needs statements calls a function:
- `let <name>: <Type> = switch <subject> { ... }` binds a value each arm selects, a `throw` arm stays in place
- `count(where:)` counts, `indices(where:)` returns a `RangeSet` that `removeSubranges` and the `subranges` subscript take whole
- `lazy` precedes a transform whose consumer stops early (`first(where:)`, `prefix`, `contains(where:)`)
- `for <item>: <Type> in <sequence> where <test>` selects in the header, `for case <pattern> in` matches a case
- Key paths serve as functions (`map(\.price)`, `sorted(using: KeyPathComparator(\.date))`)
- `reduce(into:)` builds a collection, `reduce(_:_:)` folds a value, named reductions (`min()`, `max(by:)`, `allSatisfy`) state their own fold

### [02.1]-[PATTERN_MATCHING]

`switch` keeps recognition, extraction, guards, and results together, and a tuple subject decides over every value it holds in one table where each arm names its combination:

```swift
func retryDelay(status: Int, retryAfter: Duration?, attempt: Int, limit: Int = 5, base: Duration = .seconds(1)) -> Duration? {
    switch (status, retryAfter, attempt < limit) {
        case (429, let wait?, true): wait
        case (429, nil, true), (500..<600, _, true): base * (1 << attempt)
        default: nil
    }
}
```

- Switches over a module's own enum list every case, so the next case fails to compile until each switch handles it
- `@unknown default` closes a switch over a system enum that grows
- Conditions every arm reads join the tuple subject, a `where` clause holds a test one pattern alone reads
- `if case`, `guard case`, and `for case` match one case where the remaining cases continue unchanged

Interpret an external value once near its source into an `Optional` or an enum, then match the typed result everywhere else.

### [02.2]-[RECURSION]

Swift does not guarantee tail calls, the depth bound and successor sharing select the form:

| [INDEX] | [FORM]                  | [USE_WHEN]                                             |
| :-----: | :---------------------- | :----------------------------------------------------- |
|  [01]   | Direct recursion        | Depth is small and bounded by the input's format       |
|  [02]   | `indirect enum` fold    | Each child belongs to one parent                       |
|  [03]   | `Deque` worklist        | Depth is unbounded, or order is breadth-first          |
|  [04]   | `sequence(state:next:)` | Intermediate states are values a consumer reads lazily |
|  [05]   | `OrderedSet` worklist   | Successors are shared or cyclic                        |

## [03]-[DATA]

Values take the type the standard library, Foundation, or a core package declares for their fact, and a declaration adds only a rule no such type states:

| [INDEX] | [FACT]                   | [TYPE]                                |
| :-----: | :----------------------- | :------------------------------------ |
|  [01]   | Values one reader groups | Labeled tuple                         |
|  [02]   | Interval                 | `Range`, `ClosedRange`                |
|  [03]   | Positions a test selects | `RangeSet`                            |
|  [04]   | Elapsed time, instant    | `Duration`, `ContinuousClock.Instant` |
|  [05]   | Exact amount             | `Decimal`                             |
|  [06]   | Physical quantity        | `Measurement<UnitLength>`             |
|  [07]   | Identity                 | `UUID`, `Identifiable.ID`             |
|  [08]   | File location            | `FilePath`                            |
|  [09]   | Sort order               | `[KeyPathComparator<T>]`              |
|  [10]   | Vector                   | `SIMD2<Double>`                       |
|  [11]   | Rendered text            | `FormatStyle` of the value's type     |

- Structs declare a value with a rule, its throwing initializer enforces the rule for construction and decoding alike
- Enums declare a closed set, and each per-case fact is a computed property over the case
- Kinds, keys, and states are enum cases and key paths, a string holds text a person reads or a token a wire fixes
- Synthesis supplies `Equatable`, `Hashable`, `Comparable`, `CaseIterable`, `Codable`, `RawRepresentable`, and `Sendable`
- `Comparable` synthesis orders the cases of an enum with no raw type, a `String` raw value holds an external spelling
- Per-case values that follow a rule compute from case order and defaulted parameters:

```swift
enum Plan: CaseIterable, Comparable, Sendable {
    case basic
    case team
    case enterprise

    func seats(base: Int = 1, factor: Int = 10) -> Int {
        repeatElement(factor, count: Self.allCases.count(where: { $0 < self })).reduce(base, *)
    }
}
```

Higher plans hold every lower plan's features through `<=`, and a feature gate reads `plan >= required`. A new case takes its seats from its position.

- Stored properties a transition sets are `private(set) var`, every other stored property is `let`, a derived property is computed
- Transitions are `consuming` methods returning the next value, and value semantics keep every other copy unchanged
- Value types holding standard collections copy on write through them, `isKnownUniquelyReferenced` serves storage no collection provides
- Optional properties stand where absence is a state consumers read, a value with a usual default holds that default

Choose collections by their most frequent operation:

| [INDEX] | [OPERATION]                             | [COLLECTION]                      |
| :-----: | :-------------------------------------- | :-------------------------------- |
|  [01]   | Ordered reads, appends                  | `Array`                           |
|  [02]   | Keyed lookups with insertion order read | `OrderedDictionary`, `OrderedSet` |
|  [03]   | Keyed lookups, membership               | `Dictionary`, `Set`               |
|  [04]   | Insertion and removal at both ends      | `Deque`                           |
|  [05]   | Repeated minimum or maximum             | `Heap`                            |
|  [06]   | Many versions sharing structure         | `TreeDictionary`, `TreeSet`       |
|  [07]   | Dense small non-negative integers       | `BitSet`                          |
|  [08]   | Fixed count known at compile time       | `[N of T]`                        |

### [03.1]-[UNIONS]

Each state a consumer inspects is a distinct case. Counting the values a type has (a struct multiplies its fields' counts, an enum adds its cases' counts, `Optional` adds one) exposes states no consumer reaches:
- Fields every case holds sit once in the struct that stores the enum, and the enum holds what differs between cases
- Flags whose companion fields mean something for one value become one case per state
- Outcomes one case holds side by side are one `Result` each, an optional pair or a reason string loses which outcome failed
- `@CasePathable` with `@dynamicMemberLookup` projects one case for a reader (`\.inTransit`, `is(\.pending)`, `[case:]`)
- Behavior that differs per case is a `switch`
- Absence and failure stay separate when the consumer responds differently to each

## [04]-[RESULTS]

One form serves one concern:

| [INDEX] | [FORM]                      | [CONCERN]                                                         |
| :-----: | :-------------------------- | :---------------------------------------------------------------- |
|  [01]   | `T?`                        | Absence with no reason                                            |
|  [02]   | `throws(E) -> T`            | Expected failure of a step, `try` binds and the first throw stops |
|  [03]   | `async throws(E) -> T`      | Expected failure of a step that suspends                          |
|  [04]   | `Result<T, E>`              | Outcome stored, collected, or handed to a callback as a value     |
|  [05]   | `throws(Failures<E>)`       | Every failure of independent steps, accumulated                   |
|  [06]   | `some AsyncSequence<T, E>`  | Values over time with one failure that ends the sequence          |
|  [07]   | `Task<Result<T, E>, Never>` | Work started now and joined later, its typed outcome a value      |

### [04.1]-[COMPOSITION]

Step signatures and the dependence between steps select the operator:

| [INDEX] | [SIGNATURE]                   | [OPERATOR]                    | [BEHAVIOR]                                           |
| :-----: | :---------------------------- | :---------------------------- | :--------------------------------------------------- |
|  [01]   | `T -> U`                      | `map`                         | Transforms a present value, keeps the form           |
|  [02]   | `T -> U?`                     | `flatMap`, `compactMap`       | Continues with a step that can find nothing          |
|  [03]   | `T throws(E) -> U`            | One `try` over the expression | Continues with a dependent step, first throw ends it |
|  [04]   | Independent `Result` values   | `combine` over a pack         | Runs every step, throws every failure together       |
|  [05]   | `[T]` with `T throws(E) -> U` | `map` with a throwing closure | Stops at the first failure                           |
|  [06]   | `[Result<U, E>]`              | `combine` over an array       | Keeps every success or throws every failure          |

- Steps stay in one outcome form through the pipeline, a `do`/`catch` exists only where it converts the error
- `guard let` and `guard case` bind a present value and exit with the named failure in one line
- Nested optionals and nested results stay nested, the consumer matches the inner layer once

### [04.2]-[ERRORS]

Each throwing module declares one error type: a struct holding the rejected subject and a `@CasePathable` reason enum, its text derived from both:
- Reason cases name the rule, and a case holds a fact only where some of its failures carry one (a shortfall, a host cause)
- Subjects name the rejected value as the caller wrote it with the facts its reason reads (`"debit 120 against 100"`)
- Host causes stay whole in a reason case of the type the host throws (`Errno`, `SubprocessError`), rendered only where text is written
- Host calls convert at the call, the `do` block holds the host call alone and its `catch` throws the module error
- `CancellationError` stays the cancelled outcome, no reason restates it
- Findings a check collects without failing are module error values the check returns in an array
- Consumers classify by `reason`, one case through `is(\.<case>)` and every case through an exhaustive `switch`

## [05]-[OWNERSHIP]

Copyability is a fact about a value: a value with one owner at a time is `~Copyable`, the compiler enforces every hand-off, and `deinit` releases what the value owns on every path its owner abandons:
- `consuming` methods end the value, `discard self` ends one whose method released the resource already
- `FileDescriptor.closeAfter` scopes a descriptor one call reads, a noncopyable owner holds one that outlives the call
- `borrowing` parameters read a noncopyable value without taking it, `inout` updates it in place
- `Span` and `RawSpan` lend contiguous storage to a reader without a copy or an escaping pointer, `MutableSpan` to a writer
- `OutputSpan` fills uninitialized storage, from `withTemporaryAllocation(of:capacity:)` for scratch space

## [06]-[CONCURRENCY]

Isolation decides where code runs and what it touches, and the state a value shares selects the form:

| [INDEX] | [STATE]                                    | [FORM]                                                    |
| :-----: | :----------------------------------------- | :-------------------------------------------------------- |
|  [01]   | No mutable state shared                    | `Sendable` values and nonisolated functions               |
|  [02]   | State the user interface reads             | `@MainActor` types and functions                          |
|  [03]   | Shared mutable state whose updates suspend | `actor`                                                   |
|  [04]   | Shared mutable state updated synchronously | `Mutex` or `Atomic` stored in a `final class: Sendable`   |
|  [05]   | CPU-bound work off the caller's executor   | `@concurrent` function                                    |
|  [06]   | Blocking host call                         | `@concurrent` function under `withTaskExecutorPreference` |

Child tasks follow the shape of the work, and tasks fail with `any Error` alone, so a child returns its typed `Result`:

| [INDEX] | [WORK]                               | [FORM]                              |
| :-----: | :----------------------------------- | :---------------------------------- |
|  [01]   | Fixed number of independent steps    | `async let`                         |
|  [02]   | Dynamic number of steps with results | `withTaskGroup` over `Result<T, E>` |
|  [03]   | Dynamic number of steps, no results  | `withDiscardingTaskGroup`           |
|  [04]   | Work that outlives the calling scope | `Task` its owner stores and cancels |
|  [05]   | Cleanup that finishes after cancel   | `withTaskCancellationShield`        |

- Cancellation is cooperative: a loop that never suspends calls `try Task.checkCancellation()` at its head
- `withTaskCancellationHandler` forwards a cancel to the host operation it wraps
- Deadlines are instants a call tree passes down, and the work races `clock.sleep(until:)` in one task group
- Waits that an event ends early race the sleep against the event's `bufferingNewest(1)` stream
- Actor state read after an `await` is read again, an actor caches an in-flight `Task` per key and evicts it on failure
- Callback APIs enter through `withCheckedThrowingContinuation` over `CheckedContinuation<T, E>` for one value, `AsyncStream.makeStream` for many

## [07]-[STREAMS]

Values that arrive over time are an `AsyncSequence` with its element and failure types, `some AsyncSequence<Element, Failure>` in signatures. Delivery shape selects the source and the operator:

| [INDEX] | [DELIVERY]                            | [FORM]                                               |
| :-----: | :------------------------------------ | :--------------------------------------------------- |
|  [01]   | Host callback delivering values       | `AsyncStream.makeStream`, release in `onTermination` |
|  [02]   | Changes of an `@Observable` model     | `Observations`                                       |
|  [03]   | Sources merged into one               | `merge`, `combineLatest`, `zip`, `chain`             |
|  [04]   | Bursts                                | `debounce(for:)`                                     |
|  [05]   | Batches                               | `chunks(ofCount:)`, `chunked(by:)`                   |
|  [06]   | Latest inner sequence per outer value | `flatMapLatest`                                      |
|  [07]   | Producer paced by the consumer        | `AsyncChannel`                                       |
|  [08]   | Consumers sharing one source          | `share(bufferingPolicy:)`                            |
|  [09]   | Lines of a child process              | `strings(separatedBy:bufferingPolicy:)`              |

Request-response work stays a function, and a stream holds values a consumer reads in order.

## [08]-[BOUNDARIES]

Every value entering the module is decoded once into its model type, and every value leaving is encoded from it:
- Synthesized `Codable` holds every wire mapping, a hand coder exists where the wire shape differs from the model
- External enums that grow decode an unknown value into an `unknown(String)` case, the next encode writes the text back
- Untyped host values (`Any`, `CFTypeRef`) decode through one generic read that checks the type
- Host types stand where they state the value: a caller passes the host's configuration and receives its output whole
- Host enums stay the host's type, and the wire spelling is one `switch` in the encoder that writes it
- Vocabularies a host enumerates (attribute names, actions, settability) derive from its query at the read
- `FilePath` names paths, `FileDescriptor` and `Errno` from System state the host's file API and its failures
- Child processes run through `Subprocess.run` in a session of their own with a `teardownSequence`
- Command lines are `ParsableCommand` types, `AsyncParsableCommand` where `run` awaits, and a validated type conforms to `ExpressibleByArgument`
- `Logger` interpolations each state `privacy:`, and the entry point passes one logger per category down
- `@main` types are the composition: they state application values, adapt host APIs, run the work once, and translate its error to an exit code

## [09]-[GENERICS]

Generic code states the capability it reads, and the call site's knowledge of the concrete type selects the form:

| [INDEX] | [KNOWLEDGE]                                         | [FORM]                                |
| :-----: | :-------------------------------------------------- | :------------------------------------ |
|  [01]   | One concrete type per call, fixed by the caller     | `some Protocol<Assoc>` parameter      |
|  [02]   | One concrete type returned, hidden from the caller  | `some Protocol<Assoc>` return         |
|  [03]   | Different concrete types in one collection or field | `any Protocol<Assoc>`                 |
|  [04]   | Relation between types across parameters            | Named generic parameters with `where` |
|  [05]   | Any number of types, each kept distinct             | Parameter pack (`each T`)             |
|  [06]   | Count or bound fixed per use                        | Integer generic parameter (`let N`)   |

- Protocols state primary associated types (`protocol Store<Item>`) so signatures constrain them without a `where` clause
- Protocol requirements hold the operations every conforming type provides, an extension derives the rest
- Conditional conformances (`extension Pair: Equatable where First: Equatable`) derive a capability from the parts
- Result builders state a domain language whose statements build one value
- Macros generate members a type would state by hand (`@CasePathable`, `@Observable`, `@Entry`)
