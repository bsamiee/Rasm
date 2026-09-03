---
name: dotnet-coding
description: "Use when writing or reviewing C# domain code: signatures, purity, immutability, result and effect types, pattern matching, and composition under the workspace standards."
---

# [DOTNET_CODING]

Covers writing C# under the workspace standards (TOTALITY, FLOW, INDEPENDENCE, PURITY, BOUNDARY): how a signature is shaped, what stays pure, which result or effect type a function returns, which operator joins two steps, and where the host boundary sits. LanguageExt, Thinktecture, and Mapperly are part of the language here, every example uses their types as vocabulary, and each package has its own skill:

[SKILLS]:
- `dotnet-coding-languageext`: The library's types and operations, conversions, recovery, `IO` execution, traits, transformers, collections, streams
- `dotnet-coding-thinktecture`: Declaring value objects, smart enums, and unions, their generated API, settings, factories, and integrations
- `dotnet-coding-mapperly`: Mapping between domain types and host contracts, the conversion allowlist, attributes, options, projections

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Combinators, functions as data, specialization, delegate adapters, the composition root, end-to-end flows
- [02]-[SEQUENCES](references/sequences.md): Deferral and materialization, named stages, `Fold` reductions, indexed replacement, adjacent pairs, a pipeline
- [03]-[RESULTS](references/results.md): Validator folds, fail-fast workflows, lifting into an effect, host translation, union folds, law tests
- [04]-[IMMUTABLE_DATA](references/immutable-data.md): Snapshots and transitions, shared mutation, `With`, cost model, persistent structures
- [05]-[EFFECTS](references/effects.md): I/O around a pure core, injection, `IO` and `Try`, `Reader`, resource scopes, failure policies, stacked effects
- [06]-[STATE](references/state.md): The transition shape, a cache as `State` and `StateT`, seeded generators, tree numbering, the loop forms
- [07]-[STREAMS](references/streams.md): The observable model, operators, per-item failure, backpressure, agents, entity processes
- [08]-[EVENT_SOURCING](references/event-sourcing.md): Append-only storage, events as a union, transitions, reconstruction, command and query sides

Examples assume `using static LanguageExt.Prelude`, which supplies `Some`, `None`, `Seq`, `toSeq`, `Range`, `parseInt`, `guard`, `use`, `par`, `curry`, `compose`, and `fun` as bare names. `Seq<A>` is the default collection in domain code, `Option<A>`, `Fin<A>`, `Validation<Error, A>`, and `IO<A>` are the result and effect types, NodaTime's `Instant`, `LocalDate`, and `Duration` are the time types with the clock injected as `IClock` or `Func<Instant>`, and value objects, smart enums, and unions come from the generator.

## [01]-[FUNCTIONS]

Function signatures are contracts: the input types describe every value the function accepts, and the return type describes every outcome the caller must handle. Arrow notation keeps a signature compact, `()` stands for no input or no returned information, grouped inputs form a tuple, and the delegate carries the signature in code:

| [INDEX] | [PURPOSE]                   | [ARROW_NOTATION]                      | [DELEGATE]                                |
| :-----: | :-------------------------- | :------------------------------------ | :---------------------------------------- |
|  [01]   | Transform a value           | `int -> string`                       | `Func<int, string>`                       |
|  [02]   | Produce a value             | `() -> string`                        | `Func<string>`                            |
|  [03]   | Consume a value for effects | `int -> ()`                           | `Action<int>`                             |
|  [04]   | Perform an effect           | `() -> ()`                            | `Action`                                  |
|  [05]   | Combine two inputs          | `(int, int) -> int`                   | `Func<int, int, int>`                     |
|  [06]   | Accept a function           | `(string, (IDbConnection -> R)) -> R` | `Func<string, Func<IDbConnection, R>, R>` |

- Use `Func` and `Action` when only the signature matters, and a custom delegate when its name conveys domain intent that `Func<T, bool>` does not
- Expose a `Func` field, property, or factory for a function used in partial application, because generic higher-order operations over multi-argument method groups defeat type inference, and `fun` gives an inline lambda its delegate type
- Keep names precise, because a signature cannot express every semantic detail (`Where` and `TakeWhile` share one)
- Keep data and behavior apart: records carry inputs and outputs, functions carry behavior, and a constrained type owns only the validation that constructs it and the operations (comparison) that protect its representation

### [01.1]-[HONEST_SIGNATURES]

Functions honor their signature when each declared input produces a declared output, so they return no `null` and throw no exception as an outcome the signature omits. Repair a dishonest contract by narrowing the input to a validated type or widening the output to `Option<A>` or `Fin<A>`:
- `int -> Tier` is incomplete when some integers fail validation, and `Quantity -> Tier` is accurate for every constructible `Quantity`
- `string -> int` hides the undefined parses, `string -> Option<int>` describes every outcome, and `parseInt` has that signature
- `void` splits delegates into `Func` and `Action` families and tasks into `Task` and `Task<T>`, `Unit` is the one-value type that removes the split, `fun` converts an `Action` into a `Func<Unit>` so an `Action` overload delegates to the `Func<T>` implementation, `void` stays on an imperative API that returns nothing, and `Unit` does not make an effectful function pure
- Nullable reference types stay enabled, `?` appears only where an external representation requires it, and `Optional` converts that value to `Option<A>` at the parse boundary

### [01.2]-[PURITY]

Functions are pure when their return value depends only on their inputs, including immutable values fixed at construction, and evaluating them causes no side effect, so a call can be replaced by its result (referential transparency). Side effects are mutating state visible outside the function (instance fields included), mutating an argument, throwing, and I/O (the clock, console, filesystem, database, network, or another process). Instance methods that read mutable fields and lambdas that close over mutable variables are impure, and mutation local to a function that never escapes is not a side effect. Pure functions are safe for parallel evaluation, lazy evaluation, and memoization, and the same transformations can change the behavior of an impure function. Expose a variable dependency as input data:

```csharp
internal static class Stamps {
    // The clock enters as an argument, and invariant formatting keeps the result independent of the ambient culture
    public static string Format(Instant now, string? label) => string.Create(CultureInfo.InvariantCulture, $"{now} - {label ?? "unlabeled"}");
}
```

Pure methods can be static because every value they need is explicit or immutable, and a static method reads no mutable static field and performs no I/O that a caller cannot replace.

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions accept a function, return one, or both, and a delegate is the function value. The higher-order function owns the stable control flow and the caller supplies the varying rule: `Filter` owns iteration and the caller owns the inclusion criterion, `IfNone(Func<T>)` owns the cache miss and the caller owns the fallback work. Closures combine a lambda with its declaring context, the declared signature stays unary and the computation reads the captured values beside its argument:

```csharp
Seq<DayOfWeek> days = toSeq(Enum.GetValues<DayOfWeek>());
Seq<DayOfWeek> StartingWith(string prefix) => days.Filter(day => day.ToString().StartsWith(prefix, StringComparison.Ordinal));
Seq<DayOfWeek> matched = StartingWith("S"); // Sunday, Saturday
```

Function factories turn configuration into behavior, and `compose(f, g)` joins functions into one reusable function that applies `f` first (method groups need its explicit type arguments), where method chaining expresses the same flow inline:

```csharp
internal static class Factories {
    public static Func<int, bool> IsMod(int divisor) => value => value % divisor == 0;
    public static Seq<int> MultiplesOfThree => toSeq(Range(1, 20)).Filter(IsMod(3));
}
```

Currying turns a function of `N` arguments into `N` unary functions (`curry`), and partial application (`par`) fixes a leading group of arguments and returns a function of the rest. Order parameters so left-to-right application is useful: dependencies and configuration known at the composition root, then policies that select behavior, then the runtime value. Dependencies are functions that describe the behavior the consumer needs: a clock is `Func<Instant>`, a validator is `T -> Validation<Error, T>`, a lookup is `Guid -> Eff<RT, Option<T>>`, and persistence is `T -> IO<Unit>`. The composition root reads configuration, adapts infrastructure into such functions, partially applies dependencies and policies, and injects only specialized functions into handlers. Top-level entry points compose functions from lower-level components while dependencies point downward, and no rule requires each layer to call only its neighbor, because a low-level I/O call makes every delegating layer impure.

Use these techniques when specialized functions simplify call sites, function collections when behaviors share a signature and vary as data, `ForAll` or `Exists` when only a short-circuiting boolean is required, an ordered rule table with an explicit fallback for a first-match decision over values, a returned closure to narrow a noisy API to one operation, and ordinary functions when the helper is larger than the duplication it removes or hides ordering, effects, missing-value behavior, or termination risk.

## [02]-[EXPRESSIONS]

Expressions evaluate to values, and statements perform an action or control execution. Loops, calls made only for side effects, and branches that direct mutation are statements, and a conditional with alternatives that both return a value is an expression. Build a returned object in one expression, defining each property where the object is created rather than assigning properties across branches, and extract a calculation into a function when it obscures the construction:

```csharp
internal static class Projections {
    public static Output Make(Input input) =>
        new() {
            Total = input.Unit * input.Count,
            Label = input.Alternate ? input.FirstChoice : input.SecondChoice,
        };
}
```

Prefer a query over a mutable accumulator, use `Fold` with a seed for a reduction, and use the indexed `Map` overload in place of a declared counter.

### [02.1]-[PATTERN_MATCHING]

Pattern matching selects a result from a value's type, shape, or properties, and keeps recognition, extraction, guards, and results together, where procedural type checks need casts and nested branches and virtual methods spread one rule across classes:

```csharp
internal abstract record Entry(decimal Amount, decimal Rate);
internal sealed record TieredEntry(decimal Amount, decimal Rate, decimal Bonus) : Entry(Amount, Rate);
internal sealed record ClosedEntry(decimal Amount, decimal Rate) : Entry(Amount, Rate);

internal static class Rates {
    public static decimal Apply(Entry entry) =>
        entry switch {
            TieredEntry { Amount: > 20_000m } tiered => tiered.Amount * (tiered.Rate + (tiered.Bonus * 1.25m)),
            TieredEntry { Amount: > 10_000m and <= 20_000m } tiered => tiered.Amount * (tiered.Rate + tiered.Bonus),
            ClosedEntry => 0m,
            _ => entry.Amount * entry.Rate,
        };
}
```

Each arm states an input category beside its outcome, and the switch expression forms a function body or a `Func` passed to a higher-order function. The patterns:
- Type patterns test a runtime type and bind the typed value only when the arm needs its properties
- Property patterns inspect the shape of an object, and extended property patterns (`{ Parent.Name: "root" }`) inspect nested data without extracting each intermediate object
- Relational and logical patterns (`>`, `and`, `not`) express ranges and exclusions inside a pattern
- `when` adds a guard, `_` is the discard for the fallback arm, and list patterns recognize values at the beginning, middle, or end of an array with known headers or footers

C# has no active patterns, `Deconstruct` is the only user code that runs inside a pattern and it cannot fail, so interpret an external value once near its source into `Option<A>` or a closed union (`parseInt(text)`, a classifier that returns a `[Union]` case) and match the typed result afterward.

### [02.2]-[TUPLES]

Tuples group related values without inventing a domain type that has no independent meaning, and named elements carry lookup results together between `Map` operations on a `Seq<A>` until the next projection consumes them:

```csharp
internal static class Report {
    public static Seq<string> Render(Seq<int> ids, Func<int, Item> getItem, Func<int, Seq<string>> getTags) =>
        ids
            .Map(id => (Item: getItem(id), Tags: getTags(id)))
            .Map(static x => $"{x.Item.Name}: {string.Join(", ", x.Tags)}");
}
```

### [02.3]-[RECURSION]

Recursive functions need a base case that returns the final value, a recursive case that calls the same function with values closer to that condition, and a returned value on every path. C# does not optimize tail calls, so the depth bound selects the form:

| [INDEX] | [APPROACH]                | [USE_WHEN]                                                     | [COST]                                   |
| :-----: | :------------------------ | :------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Direct recursion          | The maximum depth is small and bounded                         | Unbounded calls grow the stack           |
|  [02]   | `Trampoline<A>`           | The transition is pure and the depth is unbounded              | Every step is a deferred call            |
|  [03]   | `Monad.recur`             | The transition is an effect and only the final value is needed | Intermediate states are lost             |
|  [04]   | `LanguageExt.List.unfold` | Intermediate states are meaningful and compose as a `Seq<A>`   | Each `unfold` call reruns the transition |

## [03]-[IMMUTABILITY]

Immutable values are fixed once created, a needed change derives a new value from values already available, and a state transition is a function that receives the old state and the information the change needs and returns the replacement. Language features constrain reassignment:
- `readonly struct` prevents reassignment of its fields, a struct is copied when passed, and `readonly` protects the fields and not the objects reached through them
- `init` accessors permit object-initializer syntax and prevent reassignment after initialization, and `required` marks a property that cannot be omitted
- `record` supports nondestructive mutation through `with`, which copies the unchanged properties and leaves the original unchanged, and a state machine pattern matches the next interaction to select the transition
- Getter-only auto-properties get a compiler-generated readonly backing field, assigned inline or in the constructor

```csharp
internal sealed record Document {
    public required string Title { get; init; }
    public required string Author { get; init; }
    public Seq<string> Tags { get; init; }
}

internal static class Editions {
    public static Document Retitled(Document document, string suffix) => document with { Title = $"{document.Title} {suffix}" };
}
```

None of `readonly`, `init`, and `with` makes a referenced child object immutable, so immutability extends through every referenced value:
- Remove setters and construct through a constructor or factory that enforces required values and invariants, so an invalid snapshot cannot exist
- Seal the type, store collections as `Seq<A>`, copy a mutable input collection at the boundary with `toSeq`, and keep element types immutable
- Expose only meaningful transitions, one `With` method with `Option` parameters updates the permitted fields in one allocation, and `Lens<A, B>` with `lens(outer, inner)` updates a nested field
- Confine local mutation to a scope that owns it, a `List<T>` or `Dictionary<K, V>` stays inside a scope that publishes an immutable value

In-place updates destroy the prior value, and transformations preserve it:

```csharp
// BAD: In-place updates destroy the prior value
List<int> values = [7, 6, 1];
values.Sort(); // values is now 1, 6, 7

// GOOD: Functional alternatives preserve it
Seq<int> values = Seq(7, 6, 1);
Seq<int> sorted = toSeq(values.Order());              // values remains 7, 6, 1
Seq<int> odd = values.Filter(static x => x % 2 == 1); // 7, 1
```

Two readers can observe the same stable value safely, while a concurrent reorder of a shared list during a sum gives the reader an inconsistent traversal, and a separate ordered view removes the interference. Choose the collection by the operation the domain performs most: `Seq<A>` for ordered reads, `Lst<A>` for indexed edits, `Map<K, V>` or `HashMap<K, V>` for keyed lookups, `Set<A>` or `HashSet<A>` for uniqueness, and `Iterable<A>` for a lazy source.
- See `dotnet-coding-languageext` for the construction, memoization, folds, sequence operations, and compiler pitfalls of each collection type

## [04]-[RESULTS]

Every function returns an explicit result type, and one type serves one concern:

| [INDEX] | [TYPE]                 | [CONCERN]                                       |
| :-----: | :--------------------- | :---------------------------------------------- |
|  [01]   | `Option<A>`            | Absence without an `Error`                      |
|  [02]   | `Fin<A>`               | Expected failure with an `Error`, short-circuit |
|  [03]   | `Either<L, R>`         | Two value types, neither an error               |
|  [04]   | `Validation<Error, A>` | Independent failures, accumulate                |
|  [05]   | `Try<A>`               | Synchronous exception capture, deferred         |
|  [06]   | `IO<A>`                | Side effects with a failure channel             |
|  [07]   | `Eff<RT, A>`           | Effects that read a capability                  |

The input boundary selects the result type, domain functions preserve it, and conversion between types happens at one named boundary through the method named for the target (`ToFin`, `ToValidation`, `ToOption`, `ToSeq`). `Match`, `Run`, `RunSafe`, `IfNone`, and `IfFail` are host operations, and a domain function never runs an effect. Values of `A` and `Error` lift into `Fin<A>` and `Validation<Error, A>` by implicit conversion, and `Pure(x)` and `Fail<Error>(e)` make the lift explicit when the branches of a conditional differ in type. The empty `Seq<A>` is a result and not absence, `Option<Seq<A>>` exists only where the consumer responds differently to no collection and to an empty one, and a producer that maps an operational failure to `None` hides the failure from the consumer.

### [04.1]-[COMPOSITION]

Give each step a function and select the operator by the step's signature and by whether the steps depend on each other:

| [INDEX] | [SIGNATURE]                     | [OPERATOR]                             | [BEHAVIOR]                                                  |
| :-----: | :------------------------------ | :------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `T -> R`                        | `Map`                                  | Transforms a present value and preserves the result type    |
|  [02]   | `T -> bool`                     | `Filter` on `Option`, `guard` on `Fin` | Keeps a value only when it passes, `guard` names the error  |
|  [03]   | `T -> F<R>`                     | `Bind`, one `from` per step in LINQ    | Continues with a dependent step, the first failure stops it |
|  [04]   | `(F<A>, F<B>)` independent      | Tuple `Apply`                          | Accumulates errors under `Validation`, overlaps under `IO`  |
|  [05]   | `Seq<A>` with `A -> F<B>`       | `Traverse`, `TraverseM`                | `Traverse` accumulates or overlaps, `TraverseM` stops first |
|  [06]   | `T -> void`                     | `Iter`                                 | Performs the terminal effect only for a present value       |

- Errors from one traversed element carry the element index as a typed field
- Values stay in one abstraction through the pipeline, an unwrap followed by a rewrap duplicates effect handling, and a nested `Bind` becomes a query
- Nested contexts (`IO<Option<A>>`) compose through a transformer (`OptionT<IO, A>`), and a stack that appears throughout a workflow becomes a dedicated type

```csharp
internal sealed record InvalidCommand() : Expected("command is invalid", 901);

internal static class Workflow {
    public static Fin<State> Handle(Command command, State state) =>
        from normalized in Pure(Normalize(command)).ToFin()
        from _ in guard<Error>(IsValid(normalized), new InvalidCommand())
        from next in state.Transition(normalized.Amount)
        select next;
}
```

The workflow runs in domain order (normalize, validate, transition), `Fin` handles the control flow, and adding a step means defining one function and inserting one clause.

### [04.2]-[ERRORS]

Expected failures are data in the return type, and exceptions are reserved for developer defects that violate a precondition, configuration failures during initialization, and exception-based third-party calls that a boundary converts immediately with `IO.lift` or `Try.lift`. Each package declares its errors as `sealed record`s extending `Expected` with a message and a code from the package's `Codes` class, beside the function that returns them or the value object they protect:

```csharp
internal sealed record InvalidQuantity() : Expected("quantity out of range", Codes.InvalidQuantity), IValidationError<InvalidQuantity> {
    public static InvalidQuantity Create(string message) => new();
}

[ValueObject<int>]
[ValidationError<InvalidQuantity>]
internal readonly partial struct Quantity {
    public static Fin<Quantity> From(int value) => Validate(value, provider: null, out Quantity item) is { } error ? error : item;

    static partial void ValidateFactoryArguments(ref InvalidQuantity? validationError, ref int value) {
        if (value is < 0 or > 1_000)
            validationError = new InvalidQuantity();
    }
}
```

`Validate` is the generated hook, and the `From` factory maps it to `Fin<Quantity>`, so every consumer receives a validated value and none re-validates. Consumers classify an error with `Is`, `HasCode`, `IsType<E>`, and `Filter<E>`, never with the message text, and a package translates a dependency error it reacts to with `MapFail` into its own `Expected` that keeps the original as `Inner`. `Validation<Error, A>` holds violated business rules and accumulates through `+` into `ManyErrors`, `IO<A>` holds technical work and captures a thrown exception as `Exceptional`, and the host separates them in one `Match`: `Expected` or `ManyErrors` renders the business errors, and `Exceptional` logs the detail and renders a generic failure.

### [04.3]-[UNIONS]

Model each valid state as a distinct case, not as a flag with fields that are meaningful for one flag value, and count the values a type admits (a product multiplies the counts of its fields, `Option<A>` adds one) to expose states the domain does not need:

```csharp
[Union]
internal abstract partial record Identity {
    internal sealed record Registered(string Name, string Key, bool Enabled) : Identity;
    internal sealed record Anonymous(string Key) : Identity;
}
```

- Put shared data on the abstract base and case-specific data in its case, call `Switch` only where behavior depends on the case, and pass the union onward elsewhere
- Treat a case as an alternative to its siblings and not as an extension of one, name each case's data in its own terms, and hold mixed cases in one `Seq` of the base type
- Use a union for a growing set of operations and abstract members for a growing set of cases, because a new union case is a compile error at every `Switch` until it gains an arm
- Use a domain union when the consumer needs domain outcome names, and `Option`, `Fin`, or `Either` otherwise, keeping absence and failure separate when the consumer responds differently
- Fold a recursive union with one replacement per constructor, and fold an unbounded depth through `Trampoline<A>`

### [04.4]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                              | [CORRECT_FORM]                                     |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `Match` in the middle of a pipeline unwraps a value the next step relifts | `Bind` the next step                               |
|  [02]   | `IfNone` with an arbitrary default hides absence                          | `ToFin` with an `Error`                            |
|  [03]   | Matching on message text couples the consumer to prose                    | `HasCode` or `IsType<E>`                           |
|  [04]   | `Option` nested inside an effect forces two unwraps                       | `OptionT<IO, A>`                                   |
|  [05]   | `Fin` nested inside an effect duplicates the failure channel              | Typed `Expected` on the `IO` error channel         |
|  [06]   | `Run` inside the domain performs the effect before the host runs it       | Keep the `IO` and `Bind` the next step             |
|  [07]   | `Some` as a null guard                                                    | `Optional` at the null boundary                    |
|  [08]   | Separate result and error fields, or `default` on failure                 | One result type with mutually exclusive cases      |

## [05]-[EFFECTS]

`IO<A>` is the effect type: it describes a side effect with a failure channel, performs nothing until the host runs it, and carries a domain rejection as a typed `Expected` on that channel rather than as a nested result. `IO.lift` defers a thunk, `IO.lift(Fin<A>)` puts an evaluated rejection on the channel, `IO.liftAsync` adapts a task thunk and its `EnvIO` overload passes `env.Token`, an operation that waits on I/O enters that way and has no synchronous counterpart that blocks a thread, and `IO.pure` and `IO.fail` build the plain cases. `Eff<RT, A>` reads a capability from a runtime `RT` through `Has<Eff<RT>, T>`, and `IO<A>` converts to it implicitly. The host runs the effect once, `RunSafe()` returns the `Fin<A>` for translation into the host's vocabulary, and `Run` throws and belongs to `Main`:

```csharp
internal static class Host {
    public static int Exit(IO<Unit> handler) =>
        handler.RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => error.IsExpected ? 4 : 1);
}
```

### [05.1]-[DEPENDENCIES]

Inject the narrowest dependency that represents what the consumer needs: a value for a stable snapshot, an `IO<A>` for an operation that must run on demand, and a runtime `RT` for a consumer that reads many capabilities. Wrapping the clock behind an interface does not make the consumer pure, and injecting the date makes it deterministic:

```csharp
internal sealed record DateIsPast() : Expected("the date is in the past", 100);

internal static class Validators {
    public static Func<Command, Validation<Error, Command>> NotPast(Func<Instant> clock) =>
        command => command.Date < clock().InUtc().Date ? new DateIsPast() : command;
}
```

The composition root supplies the clock once, request handling supplies the command later, and tests supply a deterministic clock without a fake service. Pair acquisition and release in one scope with `use` for an `IDisposable` or `Bracket(Use:, Fin:)` for a named release action, and a commit belongs to a step after the work, not to the release action.

### [05.2]-[POLICIES]

Keep the failure policies distinct: fallback runs a lower-priority effect after the preferred one fails (`primary | secondary`, or `Catch(code, f)` for one classified error), retry reruns a transient failure on a schedule (`Retry(Schedule.exponential(delay) | Schedule.recurs(3))`), and recovery maps a final error to a value at the host (`IfFail` on the `Fin<A>`). Libraries return their result type with their own errors, the application composes the retry schedule, the fallback order, and the cache around it, and the host logs only a failure that reaches its translation. The dependency structure and the concurrency bound select the traversal:

| [INDEX] | [SCENARIO]                   | [TRAVERSAL]                             | [BEHAVIOR]                                                  |
| :-----: | :--------------------------- | :-------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Independent checks           | `Traverse` under `Validation`           | Accumulates every error                                     |
|  [02]   | Independent effects          | `Traverse` under `IO`                   | Overlaps without a bound, fails if one effect fails         |
|  [03]   | Dependent or ordered effects | `TraverseM`                             | Serial, stops at the first failure                          |
|  [04]   | Bounded concurrency          | Chunk, then `TraverseM` over the chunks | One chunk at a time, the chunk width sets the bound         |
|  [05]   | Best effort                  | `PartitionFallible`                     | Serial, no short-circuit, returns `Fails` and `Succs`       |

Independent effects combine with the tuple `Apply` or with `Fork` and `Await`, and `awaitAll` runs a `Seq<IO<A>>`. Avoid shared state by default, and when one logical value must be shared, `Atom<A>` replaces it with compare-and-swap, `AtomHashMap<K, V>` holds a registry with `FindOrAdd`, `Ref<A>` under `atomic` commits coordinated updates, a `Conduit` reduced under `Fork` serializes commands as an agent, and every update function stays free of effects because a conflict reruns it. The delivery shape selects the construct:
- `Source<A>` fits values that arrive over time, logic across events or sources (sequences, transitions, windows), and one-way dataflow, and an expected per-item failure stays a `Fin<A>` value inside the stream
- Independent events take a callback or an `IO<A>`, request-response work stays out of a stream, and coordination that needs explicit queues and exact sequencing takes a `Conduit` as the queue or `Pipes` as the pipeline
