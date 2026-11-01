---
name: dotnet-coding
description: "Use when writing or reviewing C#, covering functional design with result and effect types"
---

# [DOTNET_CODING]

Covers C# design decisions from signature shape to host boundary. LanguageExt and Thinktecture are part of the language, every example uses their types as vocabulary.

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): `>>` pipelines, operation records and rule rows as data, and the composition root wiring a request's effects
- [02]-[SEQUENCES](references/sequences.md): Deferral and caching, keyed and one-pass reductions, and adjacent-pair comparison
- [03]-[RESULTS](references/results.md): `Fin` workflows joining independent checks and binding dependent steps, and recursive union folds
- [04]-[IMMUTABLE_DATA](references/immutable-data.md): Snapshot transitions, entity registries, equality through a snapshot graph, and persistent collection costs
- [05]-[EFFECTS](references/effects.md): A pure core between injected reads and writes, lifting host calls, and the lease scopes they acquire
- [06]-[STATE](references/state.md): Running state through `Fold`, `Scan`, and zip alignment, and unfolding from a seed
- [07]-[STREAMS](references/streams.md): Sources with per-item results and one boundary reducer, and agents serializing commands over one owned state
- [08]-[EVENT_SOURCING](references/event-sourcing.md): Events, snapshots rebuilt from history, commands appending at an expected position, and projections
- [09]-[LANGUAGE_FEATURES](references/language-features.md): Declarations that replace a family of members with one, from type parameters to caller information

## [01]-[FUNCTIONS]

Function signatures are contracts, input types describe every value the function accepts and the return type every outcome the caller must handle. Arrow notation writes `()` for no input or no returned information and a tuple for grouped inputs, the delegate states the signature in code:

| [INDEX] | [PURPOSE]                   | [ARROW_NOTATION]                      | [DELEGATE]                                |
| :-----: | :-------------------------- | :------------------------------------ | :---------------------------------------- |
|  [01]   | Transform a value           | `int -> string`                       | `Func<int, string>`                       |
|  [02]   | Produce a value             | `() -> string`                        | `Func<string>`                            |
|  [03]   | Consume a value for effects | `int -> ()`                           | `Action<int>`                             |
|  [04]   | Perform an effect           | `() -> ()`                            | `Action`                                  |
|  [05]   | Combine inputs              | `(int, int) -> int`                   | `Func<int, int, int>`                     |
|  [06]   | Accept a function           | `(string, (IDbConnection -> R)) -> R` | `Func<string, Func<IDbConnection, R>, R>` |

- Use `Func` and `Action` for delegate values
- Declared delegates stay for `ref`, `out`, `in`, `params`, and pointer parameters, one per signature, or names telling same-typed parameters apart
- Extension members sit in one `extension(<receiver>)` block per receiver with its type parameters, constraints, and `ref` or `in` in place of `this`
- Multi-argument method groups defeat generic type inference, a partially applied function is a `Func` field, property, or factory
- Signatures leave semantic detail unstated (`Where` and `TakeWhile` share one), names state it
- Records hold inputs and outputs, functions hold behavior
- Members differing only in a number, sign, flag, or applied function are one member taking it as a parameter
- Constrained types exist one per rule the domain owns, never one per quantity name or UI bound, and declare comparison only where a consumer orders values
- Strings hold text a person reads or a wire's fixed token, a kind, key, or member name derives through `nameof` or a caller attribute
- Identity derives from a typed key and never from a label a person reads

### [01.1]-[HONEST_SIGNATURES]

Functions honor their signature when each declared input produces a declared output, they return no `null` and throw no exception as an outcome the signature omits. Correct a dishonest contract by narrowing the input to a validated type or widening the output to `Option<A>` or `Fin<A>`:
- `int -> Tier` is incomplete when some integers fail validation, and `Quantity -> Tier` is accurate for every constructible `Quantity`
- `string -> int` hides the undefined parses, `string -> Option<int>` describes every outcome, and `parseInt` has that signature
- One-value `Unit` removes the `void` split between `Func` and `Action` and between `Task` and `Task<T>`, `void` stays on an imperative API
- `fun` converts an `Action` into a `Func<Unit>`, and an `Action` overload delegates to the `Func<T>` implementation through it
- `Unit` does not make an effectful function pure
- Statements stay statements in a block body in place of `Unit` arithmetic or a discard ternary spelling them as a value
- Nullable reference types stay enabled, `?` appears only where an external representation requires it, `Optional` converts it to `Option<A>` there

### [01.2]-[PURITY]

Functions are pure when their return value depends only on their inputs (immutable values fixed at construction included) and evaluation causes no side effect, a pure call's result replaces the call (referential transparency). Side effects are mutating state visible outside the function (instance fields included), mutating an argument, throwing, and I/O (clock, console, filesystem, database, network, or another process). Instance methods that read mutable fields and lambdas that close over mutable variables are impure, mutation local to a function that never escapes is no side effect. Pure functions are safe for parallel evaluation, lazy evaluation, and memoization. Expose a variable dependency as input data, clock and reader's culture enter as arguments in place of ambient clock and culture:

```csharp
internal static class Stamps {
    public static string Format(Instant now, CultureInfo culture, string label) => string.Create(culture, $"{now} - {label}");
}
```

Pure methods can be static, every value they need is an argument or immutable, and a static method reads no mutable static field and performs no I/O a caller cannot replace.

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions accept a function, return one, or both, and a delegate is the function value. The higher-order function owns the stable control flow and the caller supplies the varying rule: `Filter` owns iteration and the caller owns the inclusion criterion, `IfNone(Func<T>)` owns the cache miss and the caller owns the fallback work. Function factories close over configuration and return a unary function, and `f >> g` joins `Func` values into one function that applies `f` first:

```csharp
internal static class Factories {
    public static Func<int, bool> IsMod(int divisor) => value => value % divisor is 0;
    public static Func<int, int> Scale(int factor) => value => value * factor;
    public static Seq<int> Matched(Seq<int> values, int factor = 2, int divisor = 3) => values.Filter(Scale(factor) >> IsMod(divisor));
}
```

Both `>>` operands are typed `Func` values (a field, a factory result, a parameter), a method group or lambda has no type in operator resolution (CS0019) and takes one through `fun`. Order parameters for partial application: dependencies and configuration known at composition root, then policies that select behavior, then the runtime value. Dependencies are functions describing behavior a consumer needs: a clock is a `TimeProvider`, a validator `T -> Fin<T>`, a lookup `Guid -> IO<Option<T>>`, and persistence `T -> IO<Unit>`. Composition roots read configuration, adapt infrastructure into dependency functions, partially apply dependencies and policies, and inject only specialized functions into handlers. I/O calls make every delegating layer above them impure.

Variants that differ in the functions they supply are rows of one record of those functions, defaults as `init` members, and one generic operation reads every row:

```csharp
internal sealed record Table<T>(Func<Seq<T>> Rows, Func<T, Guid> Id) {
    public Func<T, bool> Live { get; init; } = static _ => true;

    public Option<T> Find(Guid id) => Rows().Find(row => Live(row) && Id(row) == id);
}
```

Select the form by the call site:
- Specialization when it simplifies call sites
- `ForAll` or `Exists` when a short-circuiting boolean suffices
- Ordered rule table with an explicit fallback for a first-match decision over values
- Returned closure to narrow a noisy API to one operation
- Ordinary functions when the helper is larger than the duplication it removes or hides ordering, effects, missing-value behavior, or termination risk

## [02]-[EXPRESSIONS]

Expressions evaluate to values, and statements perform an action or control execution. Loops, calls made only for side effects, and branches that direct mutation are statements, and a conditional with alternatives that both return a value is an expression. Build a returned object in one expression that defines each property at creation, and extract a calculation into a function when it obscures the construction:

```csharp
internal static class Projections {
    public static Output Make(Input input) =>
        new() {
            Total = input.Unit * input.Count,
            Label = input.Alternate ? input.FirstChoice : input.SecondChoice,
        };
}
```

Prefer a query over a mutable accumulator, a named reduction (`Min(seed)`, `Max(seed)`) over a `Fold` that restates it, `Fold` with a seed for any other reduction, and indexed `Map` over a declared counter. Grouped reductions are `CountBy` and `AggregateBy`. One-arm `x switch { var y => … }` bindings stay for a value an expression reads more than once or inside a lambda, and a binding read once or never is inlined.

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

Each arm states an input category beside its outcome, and the switch expression forms a function body or a `Func` passed to a higher-order function. Type patterns bind the typed value only when the arm reads its properties, and extended property patterns (`{ Parent.Depth: 0 }`) reach nested data without extracting each intermediate object.

C# has no active patterns, `Deconstruct` is the only user code that runs inside a pattern and cannot fail. Interpret an external value once near its source into `Option<A>` or a closed union (`parseInt(text)`, a classifier returning a `[Union]` case), then match the typed result.

### [02.2]-[TUPLES]

Tuples group related values without inventing a type that has no independent meaning, and named elements hold lookup results together between `Map` operations on a `Seq<A>` until the next projection consumes them:

```csharp
internal static class Report {
    public static Seq<string> Render(Seq<int> ids, Func<int, Item> getItem, Func<int, Seq<string>> getTags) =>
        ids
            .Map(id => (Item: getItem(id), Tags: getTags(id)))
            .Map(static x => $"{x.Item.Name}: {string.Join(", ", x.Tags)}");
}
```

Tuple shapes repeated across signatures are a record.

### [02.3]-[RECURSION]

Recursive functions need a base case that returns the final value, a recursive case that calls the same function with values closer to that condition, and a returned value on every path. C# does not optimize tail calls, the depth bound and successor sharing select the form:

| [INDEX] | [APPROACH]                | [USE_WHEN]                                                   | [COST]                                   |
| :-----: | :------------------------ | :----------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Direct recursion          | Maximum depth small and bounded                              | Unbounded calls grow the stack           |
|  [02]   | `Monad.recur`             | Unbounded depth, only the final value needed                 | Intermediate states are lost             |
|  [03]   | `LanguageExt.List.unfold` | Intermediate states are meaningful and compose as a `Seq<A>` | Each `unfold` call reruns the transition |
|  [04]   | Graph traversal           | Shared or cyclic successors (dependencies, references)       | Graph built before the walk              |

Graphs build once into an `ArrayBidirectionalGraph<V, SEquatableEdge<V>>` a record holds, a successor function walks as an implicit graph through `ToDelegateIncidenceGraph`, and a cyclic graph orders through `SourceFirstBidirectionalTopologicalSort` over `CondensateStronglyConnected` with each cycle as one component. Recursive unions with children no other parent holds keep the union fold.

## [03]-[IMMUTABILITY]

Immutable values are fixed once created, a needed change derives a new value from existing values through `with`, and a state transition is a function that receives the old state and the information the change needs and returns the replacement:
- `required` marks a property that cannot be omitted, optional parts are `init` members or defaulted parameters in place of a positional `None`
- Declared types are records, classes, and unions, a `struct` only for a native layout or a struct-constrained type argument of a package or kernel API

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

None of `readonly`, `init`, and `with` makes a referenced child object immutable, immutability extends through every referenced value:
- Remove setters and construct through a constructor or factory that enforces required values and invariants, an invalid snapshot cannot exist
- Seal the type, store collections as `Seq<A>`, copy a mutable input collection at the boundary with `toSeq`, and keep element types immutable
- Confine local mutation to a scope that owns it, a `List<T>` or `Dictionary<K, V>` stays inside a scope that publishes an immutable value
- Tables built once and only read publish as `FrozenDictionary<K, V>` or `FrozenSet<T>`

Partial overrides are a record of `Option` fields implementing `Monoid<T>`: `|` keeps the override's present value, `HashMap.Union(Merge:)` combines nested maps, and `Monoid.combine` folds a `Seq` of layers into one value:

```csharp
internal sealed record Style : Monoid<Style> {
    public static Style Empty { get; } = new();

    public Option<Color> Fill { get; init; }
    public Option<float> Weight { get; init; }
    public HashMap<Part, Style> Parts { get; init; }

    public Style Combine(Style rhs) =>
        new() { Fill = rhs.Fill | Fill, Weight = rhs.Weight | Weight, Parts = Parts.Union(rhs.Parts, Merge: static (_, under, over) => under.Combine(over)) };
}
```

## [04]-[RESULTS]

Every function returns an explicit result type, and one type serves one concern:

| [INDEX] | [TYPE]         | [CONCERN]                                     |
| :-----: | :------------- | :-------------------------------------------- |
|  [01]   | `Option<A>`    | Absence without an `Error`                    |
|  [02]   | `Fin<A>`       | Expected failure, `Bind` stops, `Apply` joins |
|  [03]   | `Either<L, R>` | Value of either type, neither an error        |
|  [04]   | `Try<A>`       | Synchronous exception capture, deferred       |
|  [05]   | `IO<A>`        | Side effects with a failure channel           |

Input boundaries select the result type, functions preserve it, and conversions happen at one named boundary through the method named for the target (`ToFin`, `ToOption`, `ToSeq`). `Match`, `Run`, `RunSafe`, `IfNone`, and `IfFail` are host operations, and `IfNone` with a value stands only where the domain defines that value as the answer to absence. Values of `A` and `Error` lift into `Fin<A>` by implicit conversion, `Pure(x)` and `Fail<Error>(e)` make the lift explicit where no target type fixes the result (a `var` local, an inferred type argument), and a `Unit` or `Error` in a position that fixes `Fin` converts alone. An empty `Seq<A>` is a result and not absence, `Option<Seq<A>>` exists only where the consumer responds differently to no collection and to an empty one. A producer that maps an operational failure to `None` hides the failure from the consumer.

### [04.1]-[COMPOSITION]

Give each step a function and select the operator by the step's signature and by whether the steps depend on each other:

| [INDEX] | [SIGNATURE]                | [OPERATOR]                             | [BEHAVIOR]                                                  |
| :-----: | :------------------------- | :------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `T -> R`                   | `Map`                                  | Transforms a present value and preserves the result type    |
|  [02]   | `T -> bool`                | `Filter` on `Option`, `guard` on `Fin` | Keeps a value only when it passes, `guard` names the error  |
|  [03]   | `T -> F<R>`                | `Bind`, one `from` per step in LINQ    | Continues with a dependent step, the first failure stops it |
|  [04]   | `(F<A>, F<B>)` independent | Tuple `Apply`                          | Joins every error under `Fin`, overlaps under `IO`          |
|  [05]   | `Seq<A>` with `A -> F<B>`  | `Traverse`, `TraverseM`                | `Traverse` accumulates or overlaps, `TraverseM` stops first |
|  [06]   | `T -> void`                | `Iter`                                 | Performs the terminal effect only for a present value       |

- Values stay in one abstraction through the pipeline, an unwrap followed by a rewrap duplicates effect handling
- Nested `Bind` calls become a query when an inner step reads the callback value, an inner step reading nothing bound moves to the outer chain
- Final `from` clauses that `select` returns unchanged become `select <value>` over the earlier steps
- Nested contexts (`IO<Option<A>>`) stay nested, the consumer matches the inner `Option` once and no chain binds through it with a transformer
- Stacks that appear throughout a workflow become a dedicated type

```csharp
internal static class Workflow {
    public static Fin<Account> Handle(Command command, HashMap<Guid, Account> accounts) =>
        from account in accounts.Find(command.Account).ToFin(Fault.Of(Reason.Missing, command.Account))
        from _ in (Fault.Unless(command.Amount > 0m, Reason.Range, command.Amount), Fault.Unless(command.Date >= account.Opened, Reason.Order, (command.Date, account.Opened)))
            .Apply(static (_, _) => unit).As()
        from next in account.Transition(command.Amount)
        select next;
}
```

### [04.2]-[ERRORS]

Expected failures are data in return types, and exceptions are reserved for developer defects that violate a precondition, configuration failures during initialization, and exception-based third-party calls a boundary converts with `IO.lift` or `Try.lift` around the throwing call alone. Each raising package declares one error record extending `Expected` with code 0 and a `<reason>: <subject>` message. Its reason is a smart enum of rule categories a consumer tells apart (range, order, missing, malformed, refused), each item holding its message text and no host value, and its subject names the rejected values from their call text through `[CallerArgumentExpression]`. `Of` builds the error from one value or a tuple, `Unless` returns `Fin<Unit>` through it, and an `Option` converts with `.ToFin(Fault.Of(…))`:

```csharp
[SmartEnum]
public sealed partial class Reason {
    public static readonly Reason Range = new("out of range");
    public static readonly Reason Order = new("out of order");
    public static readonly Reason Missing = new("missing");
    public static readonly Reason Malformed = new("malformed");
    public static readonly Reason Refused = new("refused");

    public string Message { get; }
}

public sealed record Fault(string Subject, Reason Reason, Option<Error> Inner = default) : Expected($"{Reason.Message}: {Subject}", 0, Inner) {
    public static Fault Of<T>(Reason reason, T rejected, Option<Error> inner = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        new(string.Create(CultureInfo.InvariantCulture, $"{names} = {rejected}"), reason, inner);

    public static Fin<Unit> Unless<T>(bool flag, Reason reason, T rejected, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : Of(reason, rejected, names: names);
}
```

- Traversals pass the element index inside the rejected tuple, `Inner` holds a wrapped cause, and a limit or other operation sits in the raising class
- Tables from a reason to a host answer derive from the function that raises it
- Findings a check collects into a report without failing are a data union, one case per finding kind rendering its message

Consumers branch on the package error's `Reason` through `Filter<E>`, recover through a `Catch` predicate, and test a captured exception with `HasException<E>`. A thrown exception stays the `Exceptional` the lift captured, and a boundary wraps it only to add a subject it lacks (a path), through `Catch` on its `HasException<E>` into an error that keeps the exception as its `Inner`. `Fin<A>` holds violated business rules, `IO<A>` holds technical work and captures a thrown exception as `Exceptional`. Hosts separate both in one `Match`, `IsExceptional` logs detail and renders a generic failure, every other error renders its business message.

Use `Skill(dotnet-coding-thinktecture)` for value type validation.

### [04.3]-[UNIONS]

Model each valid state a consumer inspects as a distinct case. States differing only by a tag are one record keyed by a smart enum item holding the data consumers read. Special values of one formula are one record of its parameters. Counting values a type has (a product multiplies its fields' counts, `Option<A>` adds one) exposes unneeded states:

```csharp
[Union]
internal abstract partial record Shape {
    private Shape(double area) => Area = area;

    public double Area { get; }

    internal sealed record Circle(double Radius) : Shape(double.Pi * Radius * Radius);
    internal sealed record Rectangle(double Width, double Height) : Shape(Width * Height);
}
```

- Cases pass shared data and each fact they derive (a function, bounds, a linear part) to the base constructor, consumers read one base member
- Call `Switch` only where behavior depends on the case, and pass the union onward elsewhere
- Treat a case as an alternative to its siblings, name each case's data in its own terms, and hold mixed cases in one `Seq` of the base type
- Unions fit a growing set of operations, abstract members fit a growing set of cases
- Use a union of named outcomes when the consumer needs the names, and `Option`, `Fin`, or `Either` otherwise
- Keep absence and failure separate when the consumer responds differently
- Operations are functions and not cases: one record holds the call as `Func<T, IO<T>>` and its name from `[CallerArgumentExpression]`
- Records pairing a tag with a value implement `K<F, A>` under a witness implementing `Traversable<F>`, and `Map` and `Traverse` come from LanguageExt
- Recursive cases pass their children to the base constructor, and one `Fold` stepping over a node and its folded children serves every reduction

## [05]-[EFFECTS]

`IO<A>` describes a side effect with a failure channel, performs nothing until the host runs it, and holds a rejection as an `Expected` on that channel in place of a nested result. Operations that wait on I/O enter through `IO.liftAsync` and have no synchronous counterpart that blocks a thread. Passes, caches, and memoized derivations take the `IO` and run nothing, the host runs the effect once, `RunSafe()` returns the `Fin<A>` for translation into the host's vocabulary, and `Run` throws and belongs to `Main`:

```csharp
internal static class Host {
    public static int Exit(IO<Unit> handler) =>
        handler.RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => error.IsExpected ? 4 : 1);
}
```

Host APIs enter the domain in their own types and answers:
- Host types, presets, and argument lists pass as the host type or a function over it, a fact the host hides is an extension property on that type
- Package and host enums stay those enums
- Documented unset values become `Option` at the call
- Bulk host calls that answer once for partial success run per element under `Traverse`, one error per element
- Host outputs with a validity flag pass after the flag is tested, a typed failure on false
- Host rules reach the domain as the host's answer or exception mapped to a typed case
- Host members lift at the caller where they compose
- Events are a function attaching a `Func<TArgs, IO<Unit>>` handler, `use(add, remove)` pairs the attach with its detach at the consumer

### [05.1]-[DEPENDENCIES]

Inject the narrowest dependency that represents what consumers need: a value for a stable snapshot, an `IO<A>` for an operation that must run on demand, a function for an operation over the consumer's input, and one interface resolved once for a seam each platform implements in full. Clocks behind an interface leave consumers impure, an injected date makes a consumer deterministic:

```csharp
internal static class Validators {
    public static Func<Command, Fin<Command>> NotPast(LocalDate today) =>
        command => Fault.Unless(command.Date >= today, Reason.Order, (command.Date, today)).Map(_ => command);
}
```

Hosts read the date once from a `TimeProvider` argument (`time.ToZonedClock().GetCurrentDate()`), request handling supplies each command later, and tests supply a date without a fake clock. `TimeProvider` supplies elapsed time through `GetTimestamp()` and `GetElapsedTime(start).ToDuration()`, each read inside its consuming `IO`, and the composition root alone names `TimeProvider.System`. `Duration` compares and scales as a value (`a - b < step * n`) and is the one time type model fields hold, a `TimeSpan`, tick, or second count appears only at an API taking one. Hosts log through `ILogger` with `[LoggerMessage]` members from one `LoggerFactory` per session, a sink interface over `(Error, Type, string)` restates it. Generators derive wire names, patterns, and option rules through a `[JsonSerializable]` `JsonSerializerContext`, `[GeneratedRegex]`, and `[OptionsValidator]`, and a format reused at runtime parses once into `CompositeFormat`.

Leases are `use` scopes, `use(acquire, release)` names a release action, and `.Bracket()` on the query opening with the first `use` releases every resource on both paths. A commit is a step after the work, outside its release action:

```csharp
internal static class Exports {
    public static IO<Unit> Written(FileInfo target, Func<Stream, IO<Unit>> write) =>
        (from stream in use(target.Create)
         from written in write(stream)
         select written).Bracket();
}
```

### [05.2]-[POLICIES]

Failure policies are distinct operations:
- Fallback runs a lower-priority effect after the preferred one fails, `primary | secondary`, or `primary | @catch(predicate, f)` for one classified error
- Retry reruns a transient failure on a `Schedule`
- Recovery maps a final error to a value at the host, `IfFail` on the `Fin<A>`

Libraries return their result type with their own errors, the application composes the retry schedule, the fallback order, and the cache around it, and the host logs only a failure that reaches its translation. Dependency between effects selects the traversal (`Traverse` overlaps independent effects, `TraverseM` runs dependent ones in order), a concurrency bound chunks the input, and a caller keeping successes beside failures partitions them. State stays unshared, and one value shared across effects serializes its changes through `Atom<A>` or `AtomHashMap<K, V>` with an update function free of effects. Values arriving over time are a `Source<A>` with each expected failure a `Fin<A>` item, request-response work stays an `IO<A>`.

Use `Skill(dotnet-coding-languageext)` for traversal, concurrency, shared state, and stream members.

## [06]-[NUMERICS]

Numeric facts come from the package that states them, and a sample loop reads only spans and SIMD lanes:
- Roots, quadrature, splines, fits, quantiles, factorizations, antiderivatives, and gradients are members of the referenced numerics packages
- Root searches widen an unbracketed interval first, pass a signed margin to `TryFindRoot`, and return a missed root as `None`
- Per-sample passes lift a matrix or curve once into `Vector256<double>` or `Vector4` lanes and allocate nothing, `Matrix<double>`, quantities, and `BigInteger` stay outside the loop
- Conversions every pass needs (a host buffer's 2D view, a matrix to lanes) sit once at the type that owns them
- Exact integer-plus-fraction arithmetic on a hot path is fixed-point on `long` or `Int128`, a shift for the cell and a mask for the local part
- `Length` and `Angle` name parameters, bounds, and presentation units, a `float` holds samples and a host unit scale enters once at the boundary
- Pure geometry takes package value types, a host frame enters where a result lowers to a host type, one extension family converts between them
- Host decompositions and fits (`Transform.DecomposeAffine`, `Plane.FitPlaneToPoints`, `Quaternion.Slerp`) are called on the host type

| [INDEX] | [FACT]         | [MEMBER]                             |
| :-----: | :------------- | :----------------------------------- |
|  [01]   | Root bracket   | `ZeroCrossingBracketing.Expand`      |
|  [02]   | Scalar root    | `Brent.TryFindRoot`                  |
|  [03]   | Quadrature     | `Integrate.GaussKronrod`             |
|  [04]   | Spline         | `CubicSpline.InterpolatePchipSorted` |
|  [05]   | Antiderivative | `IInterpolation.Integrate`           |
|  [06]   | Fit            | `Fit.Polynomial`                     |
|  [07]   | Quantile       | `SortedArrayStatistics.Quantile`     |
|  [08]   | Factorization  | `Matrix<double>.Svd`                 |
|  [09]   | Gradient       | `DScalar3<T>.Variables`              |

- Use `Skill(dotnet-coding-memory)` for spans, pooled buffers, 2D views, parallel passes, and span kernels
- Use `Skill(dotnet-coding-ui)` for drawn surfaces, shaped text, vector art, color, and host controls
