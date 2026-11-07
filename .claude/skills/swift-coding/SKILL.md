---
name: swift-coding
description: "Use when writing or reviewing Swift, covering functional design with typed throws, value types, and structured concurrency"
---

# [SWIFT_CODING]

Code uses each module's types wherever they state a value:
- Toolchain: standard library, Swift Concurrency, `Observation`, `Synchronization`, `System`
- Packages: `Collections`, `AsyncAlgorithms`, `CasePaths` (`CasePathsCore` for code that reads case paths alone), `Subprocess`, `ArgumentParser`

[PRINCIPLES]:
- Facts are stated once where they belong, and everything else derives from that source
- Packages and hosts are used directly through what they provide
- Every type, export, and helper states something nothing else does, and a helper with one reader or a forwarder merges into its caller
- Values that vary are parameters, a value every caller passes alike folds into its owner, and a field no reader acts on goes
- Error cases exist where a caller branches on them, every other failure joins its owner's error
- Checkers disagreeing on a shape resolve by a stronger shape, never by a suppression, a lowered level, or a narrower rule
- Hot paths get faster by fewer passes, allocations, and parses over one input, never by a dropped case, a lock, or a wait
- Changes leave code smaller or more capable, a move, split, rename, or extracted helper changes nothing

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Closure effects, function references, predicates and comparators as values, and dependencies
- [02]-[RESULTS](references/results.md): Accumulation, validation, workflows, task results, and entry points
- [03]-[DATA](references/data.md): State machines, graph walks, and recursive folds
- [04]-[OWNERSHIP](references/ownership.md): Noncopyable resources and storage, typestate, spans, and synchronized state
- [05]-[CONCURRENCY](references/concurrency.md): Isolation, regions, task hierarchy, deadlines, actors, cancellation, and continuations
- [06]-[STREAMS](references/streams.md): Async sequence sources, typed failure, operators, observation, back pressure, and sharing
- [07]-[BOUNDARIES](references/boundaries.md): Decoding, paths, child processes, command lines, and logging
- [08]-[GENERICS](references/generics.md): Constraints, opaque types, conditional conformances, packs, integer generic parameters, and macros

## [01]-[FUNCTIONS]

Function types state each effect of a call on the function that performs it:

| [INDEX] | [ANNOTATION]             | [STATES]                                                               |
| :-----: | :----------------------- | :--------------------------------------------------------------------- |
|  [01]   | `throws(E)`              | Fails with `E`, `throws(Never)` and no annotation never fail           |
|  [02]   | `async`                  | Suspends on its caller's executor                                      |
|  [03]   | `@concurrent`            | Runs on the global executor, off its caller                            |
|  [04]   | `@Sendable`              | Crosses isolation, captures only `Sendable` values                     |
|  [05]   | `sending`                | Moves the value to another isolation region, the sender stops using it |
|  [06]   | `@isolated(any)`         | Keeps the isolation the closure was formed in                          |
|  [07]   | `borrowing`, `consuming` | Reads the argument, or takes ownership of it                           |

- Members differing only in a number, sign, flag, or applied function are one member taking it as a parameter, and overloads serve other input types
- Declarations stay `internal` until another module calls them, `package` within one package, `public` for a product's API
- `public import` names a module whose types a public signature states, every other import stays internal

### [01.1]-[TOTAL_FUNCTIONS]

Functions are total when every input they accept reaches a declared outcome. Narrow input to a validated type or widen outcome to an `Optional` or typed throw:
- Failable `init?` constructs a value whose only failure is absence, `init(_:) throws(E)` constructs one whose failure has a reason
- Higher-order functions are generic over their closure's error, taking `(T) throws(E) -> U` and throwing `E` themselves
- Closure literals that throw state `throws(E)`, an unannotated one infers `any Error`, and one that never throws binds `E` to `Never`

```swift
func total<E: Error>(of lines: [Line], pricedBy price: (Line) throws(E) -> Decimal) throws(E) -> Decimal {
    try lines.map(price).reduce(0, +)
}
```

`map`, `filter`, `count(where:)`, `Optional.map`, and `Optional.flatMap` keep `E`. Rethrowing members (`reduce`, `compactMap`, `first(where:)`, `contains(where:)`, `allSatisfy`, `sorted(by:)`) widen `E` to `any Error`. Typed steps run through a typed member, and the rethrowing member takes the non-throwing remainder.

### [01.2]-[PURITY]

Current instant, user locale, and random source (`inout some RandomNumberGenerator`) enter as arguments, and a function that waits or measures takes `some Clock<Duration>`.

## [02]-[EXPRESSIONS]

`if` and `switch` are expressions when each branch is one expression, and a binding, `return`, or property takes it whole. Branches needing statements make the `if` or `switch` a statement. Closures nest one level, and a decision over a discriminant becomes a tuple `switch` or a table:
- `let <name>: <Type> = switch <subject> { ... }` binds a value each arm selects, a `throw` arm stays in place
- `count(where:)` counts, `indices(where:)` returns a `RangeSet` that `removeSubranges` and the `subranges` subscript take whole

### [02.1]-[PATTERN_MATCHING]

`switch` keeps recognition, extraction, guards, and results together. A tuple subject makes one table where each arm names a combination of its values:

```swift
func access(of role: Role, owning: Bool, to state: Document.State) -> Access? {
    switch (role, owning, state) {
        case (.admin, _, _), (_, true, .draft): .write
        case (.editor, _, .review), (_, _, .published): .read
        default: nil
    }
}
```

`Role`, `Document.State`, and `Access` are module enums:
- Switches over a module's own enum list every case, and a new case fails to compile until each switch handles it
- Values every arm reads join the tuple subject, a `where` clause holds a test one pattern alone reads

### [02.2]-[RECURSION]

Swift guarantees no tail calls, and depth bound and successor sharing select the form:

| [INDEX] | [FORM]                  | [USE_WHEN]                                             |
| :-----: | :---------------------- | :----------------------------------------------------- |
|  [01]   | Direct recursion        | Depth is small and bounded by the input's format       |
|  [02]   | `indirect enum` fold    | Each child belongs to one parent                       |
|  [03]   | `Deque` worklist        | Depth is unbounded, or order is breadth-first          |
|  [04]   | `sequence(state:next:)` | Intermediate states are values a consumer reads lazily |
|  [05]   | `OrderedSet` worklist   | Successors are shared or cyclic                        |

## [03]-[DATA]

Values take the type that the standard library, Foundation, or a package in use declares for their fact. Declarations add only a rule no such type states:

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

- `RawRepresentable` structs (`Bounded<low, high>`) hold a value's rule in `init?(rawValue:)`
- Records of `RawRepresentable` fields synthesize each initializer and coder
- Enums declare a closed set, and each per-case fact is a computed property over the case
- Kinds, keys, and states are enum cases and key paths, a string holds text a person reads or a token a wire fixes
- Wire tokens come from their owner (`kCFBundleIdentifierKey`, `UTType`, `String(describing: T.self)`, a `rawValue`) in place of a literal
- `Comparable` synthesis orders the cases of an enum with no raw type, a `String` raw value holds an external spelling
- Per-case values that follow a rule compute from case order and parameters the caller passes:

```swift
enum Plan: CaseIterable, Comparable, Sendable {
    case basic
    case team
    case enterprise

    func seats(base: Int, factor: Int) -> Int {
        repeatElement(factor, count: Self.allCases.count(where: { plan in plan < self })).reduce(base, *)
    }
}
```

- Feature gates read `plan >= required`, and a new case takes its seats from its position
- Optional properties stand where absence is a state consumers read, and a default stands where a standard, the host, or a package states its value

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

### [03.1]-[ENUMS]

Each state a consumer inspects is a distinct case. Counting a type's values (struct fields multiply, enum cases add, `Optional` adds one) exposes states no consumer reaches:
- Fields every case holds sit once in the struct that stores the enum, and the enum holds what differs between cases
- Flags whose companion fields mean something for one value become one case per state
- Outcomes one case holds side by side are one `Result` each, an optional pair or a reason string loses which outcome failed
- `@CasePathable` with `@dynamicMemberLookup` reads one case (`\.inTransit`, `[case:]`), and `switch` decides every case or tuple associated value
- Absence and failure stay separate when the consumer responds differently to each

## [04]-[RESULTS]

One form serves one concern:

| [INDEX] | [FORM]                         | [CONCERN]                                                     |
| :-----: | :----------------------------- | :------------------------------------------------------------ |
|  [01]   | `T?`                           | Absence with no reason                                        |
|  [02]   | `throws(E) -> T`               | Expected failure of a step                                    |
|  [03]   | `async throws(E) -> T`         | Expected failure of a step that suspends                      |
|  [04]   | `Result<T, E>`                 | Outcome stored, collected, or handed to a callback as a value |
|  [05]   | `Result<T, AggregateError<E>>` | Every failure of independent steps, at least one              |
|  [06]   | `some AsyncSequence<T, E>`     | Values over time with one failure that ends the sequence      |
|  [07]   | `Task<Result<T, E>, Never>`    | Work started now and joined later, its typed outcome a value  |

### [04.1]-[COMPOSITION]

Step signatures and the dependence between steps select the operator:

| [INDEX] | [SIGNATURE]                     | [OPERATOR]                    | [BEHAVIOR]                                           |
| :-----: | :------------------------------ | :---------------------------- | :--------------------------------------------------- |
|  [01]   | `T -> U`                        | `map`                         | Transforms a present value, keeps the form           |
|  [02]   | `T -> U?`                       | `flatMap`, `compactMap`       | Continues with a step that can find nothing          |
|  [03]   | `T throws(E) -> U`              | One `try` over the expression | Continues with a dependent step, first throw ends it |
|  [04]   | Independent aggregate `Result`s | `combine` over a pack         | Runs every step, keeps every failure together        |
|  [05]   | `[T]` with `T throws(E) -> U`   | `map` with a throwing closure | Stops at the first failure                           |
|  [06]   | `[T]` with an aggregate step    | `traverse`                    | Runs every step, keeps every success or failure      |

`AggregateError`, `combine`, and `traverse` are declared once for every throwing module, and `aggregated` lifts one step's failure into an aggregate.

- Steps stay in one outcome form through the pipeline
- Nested optionals and nested results stay nested, and consumers match the inner layer once

### [04.2]-[ERRORS]

Each throwing module declares one error type: a struct holding the rejected subject and a `@CasePathable` reason enum, its text derived from both:

```swift
struct OrderError: LocalizedError, CustomNSError {
    @CasePathable
    enum Reason: Sendable {
        case missing
        case insufficientFunds(shortBy: Decimal)
        case unreadable(Errno)
    }

    let subject: String
    let reason: Reason

    var errorDescription: String? { "\(reason): \(subject)" }

    var errorUserInfo: [String: Any] {
        switch reason {
            case .unreadable(let cause): [NSUnderlyingErrorKey: cause]
            case .missing, .insufficientFunds: [:]
        }
    }
}
```

- `localizedDescription` and a bridged `NSError` read `errorDescription`, and `errorUserInfo` holds the host cause under `NSUnderlyingErrorKey`
- `Error` refines `Sendable`, every reason fact is `Sendable`, and a cause crosses tasks whole
- Reason cases name the rule, and a case holds a fact only where some of its failures have one (a shortfall, a host cause)
- Guards checking separate rules each throw their own reason, and a shared reason loses which rule failed
- Subjects name the rejected value as the caller wrote it with the facts its reason reads (`"debit 120 against 100"`)
- Host causes stay whole in a reason case of the host's thrown type (`Errno`, `DecodingError`, `AXError`), an untyped `throws` in an `any Error` case
- Host calls convert at the call, `do` holds the host call alone and its `catch` throws the module error
- Fallbacks answering one failure catch its pattern in one `do throws(E)` (`catch let error where error.reason.is(\.missing)`), and the rest propagate
- Guards, defaults, retries, and `try?` hiding a cause go, and a `Result` field keeps a cause a reader reports
- `CancellationError` passes through as the cancelled outcome
- Consumers classify by `reason`

## [05]-[CONCURRENCY]

Isolation decides where code runs and what it touches, and the state a value shares selects the form:

| [INDEX] | [STATE]                                    | [FORM]                                                    |
| :-----: | :----------------------------------------- | :-------------------------------------------------------- |
|  [01]   | No mutable state shared                    | `Sendable` values and nonisolated functions               |
|  [02]   | State the user interface reads             | `@MainActor` types and functions                          |
|  [03]   | Shared mutable state whose updates suspend | `actor`                                                   |
|  [04]   | Shared mutable state updated synchronously | `Mutex` or `Atomic` stored in a `final class: Sendable`   |
|  [05]   | CPU-bound work off the caller's executor   | `@concurrent` function                                    |
|  [06]   | Blocking host call                         | `@concurrent` function under `withTaskExecutorPreference` |

Child tasks follow the shape of the work, and each child returns its typed `Result` for its parent to decide once:

| [INDEX] | [WORK]                               | [FORM]                              |
| :-----: | :----------------------------------- | :---------------------------------- |
|  [01]   | Fixed number of independent steps    | `async let`                         |
|  [02]   | Dynamic number of steps with results | `withTaskGroup` over `Result<T, E>` |
|  [03]   | Dynamic number of steps, no results  | `withDiscardingTaskGroup`           |
|  [04]   | Work that outlives the calling scope | `Task` its owner stores and cancels |

## [06]-[STREAMS]

Delivery shape selects source and operator:

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
|  [10]   | State with no event                   | `AsyncTimerSequence(interval:clock:).first(where:)`  |

Request-response work and one-shot waits stay an `async throws(E)` function. One-shot waits whose typed failure has no cancelled case (`Errno`) are a one-element stream, and a cancelled consumer ends it with no element.

## [07]-[BOUNDARIES]

Values entering a module decode once into their model type, and values leaving encode from it. Wire enums that grow use one generic field type with a `RawRepresentable` conformance that supplies `Codable` and writes unknown spellings back:

```swift
enum Lenient<Known: RawRepresentable<String> & Sendable>: RawRepresentable, Codable, Sendable {
    case known(Known)
    case unknown(String)

    init(rawValue: String) {
        self = Known(rawValue: rawValue).map(Self.known) ?? .unknown(rawValue)
    }

    var rawValue: String {
        switch self {
            case .known(let value): value.rawValue
            case .unknown(let text): text
        }
    }
}
```

- Synthesized `Codable` holds every wire mapping, and `init(from:)` is hand-written only where a stored type lacks `Codable` or a field names the case
- Hand-written `encode(to:)` writes through a keyed container over `CodingKeys`, and a dictionary literal drops fields it omits
- Untyped host values (`Any`, `CFTypeRef`) decode through one generic read that checks the type
- Host types stand where they state the value: a caller passes the host's configuration and receives its output whole
- Host enums stay the host's type, and a model holding one writes it in its own `encode(to:)`, `@unknown default` writing the raw value
- Names a host enumerates (attribute names, actions, settability) derive from its query at the read
- One host query reads every attribute a pass needs, and consumers take the read record
- Formats derive from their target (`UTType(filenameExtension: file.pathExtension)`) in place of a format parameter restating it
- `FileDescriptor` and `Errno` from System state the host's file API and its failures

## [08]-[GENERICS]

Generic code states the capability it reads, and the call site's knowledge of the concrete type selects the form:

| [INDEX] | [KNOWLEDGE]                                         | [FORM]                                |
| :-----: | :-------------------------------------------------- | :------------------------------------ |
|  [01]   | One concrete type per call, fixed by the caller     | `some Protocol<Assoc>` parameter      |
|  [02]   | One concrete type returned, hidden from the caller  | `some Protocol<Assoc>` return         |
|  [03]   | Different concrete types in one collection or field | `any Protocol<Assoc>`                 |
|  [04]   | Relation between types across parameters            | Named generic parameters with `where` |
|  [05]   | Any number of types, each kept distinct             | Parameter pack (`each T`)             |
|  [06]   | Count or bound fixed per use                        | Integer generic parameter (`let N`)   |

Protocols state primary associated types (`protocol Store<Item>`), and signatures constrain them without a `where` clause.
