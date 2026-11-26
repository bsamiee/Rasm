---
name: coding
description: "Use when writing or reviewing Swift, covering functional design with typed throws, value types, and structured concurrency"
---

# [SWIFT_CODING]

Code uses each module's types wherever they state a value, and a module below performs its field's job before any hand form:

| [INDEX] | [FIELD]           | [MODULES]                                                                           |
| :-----: | :---------------- | :---------------------------------------------------------------------------------- |
|  [01]   | Language          | Standard library, Swift Concurrency, `Synchronization`, `Observation`               |
|  [02]   | Collections       | `OrderedCollections`, `DequeModule`                                                 |
|  [03]   | Async sequences   | `AsyncAlgorithms`                                                                   |
|  [04]   | Enum cases        | `CasePaths` declaring `@CasePathable`, `CasePathsCore` for code reading paths alone |
|  [05]   | Files, processes  | `System`, `Subprocess`                                                              |
|  [06]   | Settings          | `Configuration`                                                                     |
|  [07]   | Service lifecycle | `ServiceLifecycle`, `UnixSignals`                                                   |
|  [08]   | Observability     | `Logging`, `Tracing`, `Instrumentation`, `OTel`, `OTelSemanticConventions`          |
|  [09]   | Process metrics   | `SystemMetrics`                                                                     |
|  [10]   | App updates       | `Sparkle`                                                                           |
|  [11]   | Command lines     | `ArgumentParser`, in a tool package outside root `Package.swift`                    |

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Closure effects, function references, predicates and comparators as values, and dependencies
- [02]-[RESULTS](references/results.md): Accumulation, validation, workflows, task results, and entry points
- [03]-[DATA](references/data.md): State machines, graph walks, and recursive folds
- [04]-[OWNERSHIP](references/ownership.md): Noncopyable host owners, spans, and synchronized state
- [05]-[CONCURRENCY](references/concurrency.md): Isolation, regions, task hierarchy, deadlines, actors, cancellation, and continuations
- [06]-[STREAMS](references/streams.md): Async sequence sources, typed failure, operators, observation, back pressure, and sharing
- [07]-[BOUNDARIES](references/boundaries.md): Codable, files, settings, child processes, command lines, and telemetry
- [08]-[GENERICS](references/generics.md): Constraints, failure parameters, opaque types, protocol extensions, conformances, and case key paths
- [09]-[INTEROPERABILITY](references/interoperability.md): Unsafe imports, CF values, out-parameters, C callbacks, private symbols, and overlays

Use `swift:testing` for suites.

## [01]-[FUNCTIONS]

Function types state each effect of a call on the function that performs it:

| [INDEX] | [ANNOTATION]             | [STATES]                                                               |
| :-----: | :----------------------- | :--------------------------------------------------------------------- |
|  [01]   | `throws(E)`              | Fails with `E`, `throws(Never)` and no annotation never fail           |
|  [02]   | `async`                  | Suspends, a nonisolated one runs on its caller's actor                 |
|  [03]   | `@concurrent`            | Runs off its caller's actor                                            |
|  [04]   | `@Sendable`              | Crosses isolation, captures only `Sendable` values                     |
|  [05]   | `sending`                | Moves the value to another isolation region, the sender stops using it |
|  [06]   | `borrowing`, `consuming` | Reads the argument, or takes ownership of it                           |

- Members differing only in a number, sign, flag, or applied function are one member taking it as a parameter, and overloads serve other input types
- Parameters every caller fills alike leave the signature for a value the callee derives, a list of every case is `allCases` (`Program.require()` checks `Grant.allCases`)
- Declarations stay `internal` until another module calls them, `package` within one package, `public` for a product's API
- Imports stay internal, `public import` or `package import` names the module declaring a type its signatures state, and members resolve through a module the file imports

### [01.1]-[TOTAL_FUNCTIONS]

Functions are total when every input they accept reaches a declared outcome. Narrow input to a validated type or widen outcome to an `Optional` or typed throw:
- Failable `init?` constructs a value whose only failure is absence, `init(_:) throws(E)` constructs one whose failure has a reason
- Narrower input types delete a throwing path: `ElementAction.set` takes the four cases of `Fact.Scalar` in place of `JSONValue`, and its `malformed` reason left the error
- Higher-order functions are generic over their closure's error, taking `(T) throws(E) -> U` and throwing `E` themselves (`withTimeout(_:expired:clock:_:)` from `Results`)
- Closure literals that throw state `throws(E)`, an unannotated one infers `any Error`, and one that never throws binds `E` to `Never`

`map`, `filter`, `count(where:)`, `Optional.map`, and `Optional.flatMap` keep `E`. Rethrowing members (`reduce`, `compactMap`, `first(where:)`, `contains(where:)`, `allSatisfy`, `sorted(by:)`) widen `E` to `any Error`. Typed steps run through a typed member, and the rethrowing member takes the non-throwing remainder.

### [01.2]-[PURITY]

Current instant, user locale, host settings, and random source (`inout some RandomNumberGenerator`) enter as arguments, and a function that waits or measures takes `some Clock<Duration>`. Boundaries read the host once and pass values: `ElementAction` reads `NSEvent.doubleClickInterval` and key repeat timing, and `Input.timeline(doubleClick:keyRepeat:frames:layout:numbering:using:)` turns them into a replayable event course with jitter from a seeded generator.

## [02]-[EXPRESSIONS]

`if` and `switch` are expressions when each branch is one expression, and a binding, `return`, or property takes it whole. Branches needing statements make the `if` or `switch` a statement. Closures nest one level, and a decision over a discriminant becomes a tuple `switch` or a table:
- `let <name>: <Type> = switch <subject> { ... }` binds a value each arm selects, a `throw` arm stays in place
- Arms select one complete tuple of what differs per case, and one statement sequence after the `switch` does the shared work (`let (tag, key, error): (String, Key, any Error) = switch self` in `Reason.encode(to:)`)
- `count(where:)` counts, `indices(where:)` returns a `RangeSet` that `removeSubranges` and the `subranges` subscript take whole
- `zip(items, items.dropFirst())` reads neighbors, `Dictionary(_:uniquingKeysWith: +)` counts by key, `Dictionary(grouping:by:)` buckets
- Condition lists bind a derived value with `case let` (`while let last = menuRead, case let remaining = menuGap - last.duration(to: .now), remaining > .zero`)
- Signatures state their type, and stored properties and statics take the type their initializer names (`let <name> = <Type>(...)`)
- Locals and bindings state a type where inference picks another (empty literal, implicit member, generic result)
- Closures in a chain over `Result`, optionals, and `??` state parameter and result types, as root `Package.swift` fails an expression past 200 type-check scopes

### [02.1]-[PATTERN_MATCHING]

`switch` keeps recognition, extraction, guards, and results together. Host tokens (`kAX…Role` and subrole constants, flags) classify once at the boundary into the module's enum, a tuple subject making one table where each arm names a combination of its values:

```swift
enum Kind: Sendable {
    case window
    case dialog
    case sheet
    case menu

    init?(role: String, subrole: String?, root: Bool, modal: Bool) {
        switch (root, role, subrole, modal) {
            case (true, kAXMenuRole, _, _): self = .menu
            case (_, kAXSheetRole, _, _): self = .sheet
            case (true, _, kAXDialogSubrole, _), (true, _, _, true): self = .dialog
            case (true, _, _, _): self = .window
            case (false, _, _, _): return nil
        }
    }
}
```

`Container.Kind.init?(_:root:in:)` holds this shape, and a new Accessibility role changes one arm:
- Consumers switch over the module's enum and never see a host string, and each per-case fact is an exhaustive `switch` beside the classification
- Switches over a module's own enum list every case, and a new case fails to compile until each switch handles it
- Values every arm reads join the tuple subject, a `where` clause holds a test one pattern alone reads
- Range patterns match counts inside a tuple arm, as `metadataForm(of:)` drops a predicate in `case (.or, ..<compound.subpredicates.count), (_, 0): nil`

### [02.2]-[RECURSION]

Swift guarantees no tail calls, and depth bound and successor sharing select the form:

| [INDEX] | [FORM]                                          | [USE_WHEN]                                             |
| :-----: | :---------------------------------------------- | :----------------------------------------------------- |
|  [01]   | Direct recursion                                | Depth is small and bounded by the input's format       |
|  [02]   | `indirect case` fold                            | Each child belongs to one parent                       |
|  [03]   | `sequence(state:next:)`                         | Intermediate states are values a consumer reads lazily |
|  [04]   | Array stack beside a `Set` of visits            | Depth-first walk over links that repeat or cycle       |
|  [05]   | `OrderedSet` worklist                           | Breadth-first walk over shared or cyclic successors    |
|  [06]   | `sequence(first:next:)`, `prefix(<node count>)` | Parent chain over links another process reports        |

## [03]-[DATA]

Values take the type that the standard library, an SDK framework, or a package in use declares for their fact. Declarations add only a rule no such type states:

| [INDEX] | [FACT]                   | [TYPE]                                      |
| :-----: | :----------------------- | :------------------------------------------ |
|  [01]   | Values one reader groups | Labeled tuple                               |
|  [02]   | Interval                 | `Range`, `ClosedRange`                      |
|  [03]   | Positions a test selects | `RangeSet`                                  |
|  [04]   | Elapsed time, instant    | `Duration`, `ContinuousClock.Instant`       |
|  [05]   | Identity                 | `UUID`, `Identifiable.ID`                   |
|  [06]   | File location            | `FilePath`, `URL`                           |
|  [07]   | File format              | `UTType`                                    |
|  [08]   | Host failure code        | `Errno`, `AXError`, `OSStatus`              |
|  [09]   | Sort order               | `KeyPathComparator<T>`                      |
|  [10]   | Vector                   | `SIMD2<Double>`                             |
|  [11]   | Rendered text            | `formatted()` with the value's format style |

- `RawRepresentable` structs hold a value's rule in `init?(rawValue:)`, and the standard library's coder for their raw type refuses a rejected value at decode (`LongestSide`)
- Records of `RawRepresentable` fields synthesize each initializer and coder, a field computed from stored fields is a computed property (`LogLine.timestamp`)
- Custom initializers sit in an extension, and the memberwise initializer and synthesized coders stay (`DiagnosticReport.init?(_:written:)`)
- Kinds, keys, and states are enum cases and key paths, a string holds text a person reads or a token a wire fixes
- Wire tokens come from their owner (`kCFBundleIdentifierKey`, `UTType`, `String(describing: T.self)`, a `rawValue`) in place of a literal
- Absence is `nil` of an `Optional`, never an empty string, a sentinel, a force cast, or a force unwrap
- Defaults stand where a standard, the host, or a package states the value, `""` included where an API reads it as unset

Enums declare a closed set, and one `switch` returns every host constant a case maps to as a labeled tuple. Reverse lookups build once from `allCases` into a static dictionary:

```swift
enum Special: CaseIterable, Sendable {
    case `return`
    case tab
    case escape

    private static let glyphs: [Int: Self] = Dictionary(uniqueKeysWithValues: allCases.map { special in (special.codes.glyph, special) })

    init?(glyph: Int) {
        guard let special = Self.glyphs[glyph] else { return nil }
        self = special
    }

    var codes: (key: CGKeyCode, glyph: Int) {
        let (key, glyph): (Int, Int) =
            switch self {
                case .return: (kVK_Return, kMenuReturnGlyph)
                case .tab: (kVK_Tab, kMenuTabRightGlyph)
                case .escape: (kVK_Escape, kMenuEscapeGlyph)
            }
        return (CGKeyCode(key), glyph)
    }
}
```

`Key.Special` holds this shape over every special key, `kVK_*` and `kMenu*Glyph` from Carbon's HIToolbox. Menu shortcuts read back as keys through `init?(glyph:)`, and a new case cannot miss a column.

Choose collections by their most frequent operation:

| [INDEX] | [OPERATION]                             | [COLLECTION]                                 |
| :-----: | :-------------------------------------- | :------------------------------------------- |
|  [01]   | Ordered reads, appends                  | `Array`                                      |
|  [02]   | Keyed lookups with insertion order read | `OrderedDictionary`, `OrderedSet`            |
|  [03]   | Values interned to dense ids            | `OrderedSet`, `append(_:)` returns the index |
|  [04]   | Keyed lookups, membership               | `Dictionary`, `Set`                          |
|  [05]   | Insertion and removal at both ends      | `Deque`                                      |

### [03.1]-[ENUMS]

Each state a consumer inspects is a distinct case. Counting a type's values (struct fields multiply, enum cases add, `Optional` adds one) exposes states no consumer reaches:
- Fields every case holds sit once in the struct that stores the enum, and the enum holds what differs between cases
- Facts one kind alone holds are associated values of its case, an optional field on every value admits combinations no kind reaches
- Flags whose companion fields mean something for one value become one case per state, as `InstanceState` cases replaced the flag fields of `Poll`
- Outcomes one case holds side by side are one `Result` each, an optional pair or a reason string loses which outcome failed
- `@CasePathable` with `@dynamicMemberLookup` reads one case (`fact.integer`, `message[case: \.state]`, `state.is(\.exited)`), and `switch` decides every case
- Absence and failure stay separate when the consumer responds differently to each

## [04]-[RESULTS]

One form serves one concern:

| [INDEX] | [FORM]                         | [CONCERN]                                                     |
| :-----: | :----------------------------- | :------------------------------------------------------------ |
|  [01]   | `T?`                           | Absence with no reason                                        |
|  [02]   | `throws(E) -> T`               | Expected failure of a step                                    |
|  [03]   | `async throws(E) -> T`         | Expected failure of a step that suspends                      |
|  [04]   | `Result<T, E>`                 | Outcome stored, collected, or handed to a callback as a value |
|  [05]   | `Result<T, AggregateError<E>>` | Independent steps the whole needs, every failure kept         |
|  [06]   | Record of successes, failures  | Items read independently, each kept (`Installation.Listing`)  |
|  [07]   | `some AsyncSequence<T, E>`     | Values over time with one failure that ends the sequence      |
|  [08]   | `Task<Result<T, E>, Never>`    | Work started now and joined later, its typed outcome a value  |

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

`Results` declares `AggregateError`, `combine`, `traverse`, and `aggregated`, which lifts one step's failure into an aggregate, once for every module:
- Steps stay in one outcome form through the pipeline
- Nested optionals and nested results stay nested, and consumers match the inner layer once

### [04.2]-[ERRORS]

Each throwing module declares one error type: a struct holding the rejected subject and a `@CasePathable` reason enum, its text derived from both:

```swift
public struct FileFailure: CustomNSError, Equatable, LocalizedError {
    @CasePathable
    public enum Reason: Equatable, Sendable {
        case held
        case compromised
        case filesystem(NSError)
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? {
        switch reason {
            case .held: "\(subject) is held"
            case .compromised: "\(subject) changed while held"
            case .filesystem(let error): "\(subject): \(error.localizedDescription)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch reason {
            case .filesystem(let error): [NSUnderlyingErrorKey: error]
            case .held, .compromised: [:]
        }
    }
}
```

- `localizedDescription` and a bridged `NSError` read `errorDescription`, and `errorUserInfo` holds the host cause under `NSUnderlyingErrorKey`
- `Error` refines `Sendable`, every reason fact is `Sendable`, and a cause crosses tasks whole
- Reason cases name the rule and their own cause, and a case holds a fact only where some of its failures have one
- Guards checking separate rules each throw their own reason, and a reason holding a sentence or a `nil` shared across rules loses which rule failed
- Lookup refusals state how far the lookup matched and what stood there (`.stale(reached:candidates:)`)
- Subjects name the rejected value with the facts its reason reads (`"\(attribute) of process \(pid)"`)
- Host causes stay whole in the host's thrown type: `Errno` from System, `AXError`, `DecodingError`, and `NSError` for an untyped Foundation throw
- `NSError` is `Sendable` and `Hashable`, and a reason holding one synthesizes `Equatable` where `any Error` forces a hand `==`
- Host calls convert at the call, `do` holds the host call alone and its `catch` throws the module error
- Host calls keep the status they return, as a discarded `AXObserverAddNotification` result leaves a refused registration silent
- Host statuses classify in one initializer, and its `otherwise: () -> Self` closure makes the extra host read a fall-through case alone needs
- Host writes into another process prove their effect by reading the value back, the returned status is a fact the read-back weighs (`.unchanged`)
- Fallbacks answering one failure catch its pattern in one `do throws(E)` (`catch let error where error.reason.is(\.held)`), and the rest propagate
- Guards, defaults, retries, and `try?` hiding a cause go, and a `Result` field keeps a cause a reader reports
- `CancellationError` passes through as the cancelled outcome, consumers classify by `reason`, and cleanup a cancelled task must finish runs under `withTaskCancellationShield`

## [05]-[CONCURRENCY]

Isolation decides where code runs and what it touches, and the state a value shares selects the form:

| [INDEX] | [STATE]                                    | [FORM]                                                             |
| :-----: | :----------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | No mutable state shared                    | `Sendable` values and nonisolated functions                        |
|  [02]   | State the user interface reads             | `@MainActor` types and functions                                   |
|  [03]   | Shared mutable state whose updates suspend | `actor`                                                            |
|  [04]   | Non-`Sendable` host handles                | `actor` holding the handles, `Sendable` records leaving it         |
|  [05]   | Blocking host calls over actor-held state  | `actor` whose `unownedExecutor` is its own `DispatchSerialQueue`   |
|  [06]   | Shared mutable state updated synchronously | `Mutex` in a `final class: Sendable`                               |
|  [07]   | Same state held by a copyable value        | `OSAllocatedUnfairLock`                                            |
|  [08]   | CPU-bound work off the caller's actor      | `@concurrent` function                                             |
|  [09]   | Nonisolated blocking host call             | `@concurrent` function under `withTaskExecutorPreference(<queue>)` |

Child tasks follow the shape of the work, and each child returns its typed `Result` for its parent to decide once:

| [INDEX] | [WORK]                               | [FORM]                              |
| :-----: | :----------------------------------- | :---------------------------------- |
|  [01]   | Fixed number of independent steps    | `async let`                         |
|  [02]   | Dynamic number of steps with results | `withTaskGroup` over `Result<T, E>` |
|  [03]   | Dynamic number of steps, no results  | `withDiscardingTaskGroup`           |
|  [04]   | Work that outlives the calling scope | `Task` its owner stores and cancels |

## [06]-[STREAMS]

Delivery shape selects source and operator:

| [INDEX] | [DELIVERY]                                 | [FORM]                                                                 |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | Host callback delivering values            | `AsyncStream.makeStream`, release in `onTermination`                   |
|  [02]   | Host notifications                         | `NotificationCenter.messages(of:for:)`                                 |
|  [03]   | Changes of an `@Observable` model          | `Observations`                                                         |
|  [04]   | Sources merged into one                    | `merge`, `combineLatest`, `zip`, `chain`                               |
|  [05]   | Bursts                                     | `debounce(for:)`                                                       |
|  [06]   | Batches                                    | `chunks(ofCount:)`, `chunked(by:)`                                     |
|  [07]   | Producer paced by the consumer             | `AsyncChannel`                                                         |
|  [08]   | Consumers sharing one source from now on   | `share(bufferingPolicy:)`, no replay                                   |
|  [09]   | Subscribers taking the current value first | Owning actor yields its state and stores the continuation in one step  |
|  [10]   | Lines of a child process                   | `strings(separatedBy:bufferingPolicy:)`                                |
|  [11]   | State with no event                        | `AsyncTimerSequence(interval:clock:).first(where:)`                    |
|  [12]   | Elements until an event                    | `merge` with the event as `nil`, `prefix(while:)`                      |
|  [13]   | One event an act causes, or a timeout      | `first { _ in true }` of a stream opened before the act, `withTimeout` |

`flatMapLatest` traps at a state precondition when an outer element races its inner sequence's end, and row [12] replaces it. Request-response work and one-shot waits stay an `async throws(E)` function. One-shot waits whose typed failure has no cancelled case (`Errno`) are a one-element stream, and a cancelled consumer ends it with no element.

## [07]-[BOUNDARIES]

Values entering a module decode once into their model type, and values leaving encode from it:
- Enums a producer or host extends are `Lenient<Known>` fields from `Encoding` over any raw value, as `Lenient<AXError>` keeps a code the SDK names no case for
- Synthesized `Codable` holds every wire mapping, and `init(from:)` is hand-written only where a stored type lacks `Codable` or a field names the case
- Hand-written `encode(to:)` writes through a keyed container over `CodingKeys`, and a dictionary literal drops fields it omits
- Untyped host values (`CFTypeRef`) decode in one failable init switching on `CFGetTypeID` into a closed enum with one case per host type (`Fact.init?(_:interning:)`)
- Host types stand where they state the value: a caller passes the host's configuration and receives its output whole
- Names a host enumerates (attribute names, actions, settability) derive from its query at the read, a hand list beside `AXUIElementCopyAttributeNames` leaves the rest unread
- One host query reads every attribute a pass needs (`AXUIElementCopyMultipleAttributeValues`), and consumers take the read record
- Host members perform their job whole: `CGImageSourceCreateThumbnailAtIndex` under `kCGImageSourceThumbnailMaxPixelSize` scales an image, and `RecognizeTextRequest` boxes map through `toImageCoordinates`
- Formats derive from their target (`UTType(filenameExtension: file.pathExtension)`) in place of a format parameter restating it
- Intermediate files a library writes take the host's default format (`SCRecordingOutputConfiguration().outputFileType`) in a system temporary folder, and the caller receives `Data`
- `FileDescriptor` and `Errno` from System state the host's file API and its failures

## [08]-[GENERICS]

Generic code states the capability it reads, and the call site's knowledge of the concrete type selects the form:

| [INDEX] | [KNOWLEDGE]                                         | [FORM]                                |
| :-----: | :-------------------------------------------------- | :------------------------------------ |
|  [01]   | One concrete type per call, fixed by the caller     | `some Protocol<Assoc>` parameter      |
|  [02]   | One concrete type returned, hidden from the caller  | `some Protocol<Assoc>` return         |
|  [03]   | Different concrete types in one collection or field | `any Protocol<Assoc>`                 |
|  [04]   | Relation between types across parameters            | Named generic parameters with `where` |
|  [05]   | Any number of types, each kept distinct             | Parameter pack (`each T`), `combine`  |

- Protocols state primary associated types (`protocol Store<Item>`), and signatures constrain them without a `where` clause
