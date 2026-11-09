---
name: dotnet-coding
description: "Use when writing or reviewing C#, covering functional design with result and effect types"
---

# [DOTNET_CODING]

Covers C# design from signature shape to host boundary, with LanguageExt and Thinktecture types as vocabulary.

[PRINCIPLES]:
- Facts are stated once where they belong, and everything else derives from that source
- Packages and hosts are used directly through what they provide, with no layer that restates or wraps them
- Every type, export, and helper states a fact nothing else does, a helper with one reader, forwarder, or alias merges into its owner
- Values that vary are parameters, a value every caller passes alike folds into its owner
- Error cases exist where a caller branches on them, every other failure joins its owner's error
- Checkers disagreeing on a shape resolve by a stronger shape, never by a suppression, a lowered level, or a narrower rule
- Hot paths get faster by fewer passes, allocations, and parses over one input, never by a dropped case, a lock, or a wait
- Changes leave code smaller or more capable, a move, split, rename, or extracted helper changes nothing

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Behavior held as function values and programs composed from them
- [02]-[SEQUENCES](references/sequences.md): Collection work as deferred stages and folds in place of loops and accumulators
- [03]-[RESULTS](references/results.md): Outcomes as values that workflows compose under result equations and hosts translate
- [04]-[IMMUTABLE_DATA](references/immutable-data.md): Change modeled as successive snapshots, with limits and costs of immutability
- [05]-[EFFECTS](references/effects.md): Side effects described as values around a pure core and run once by host
- [06]-[STATE](references/state.md): Behavior depending on earlier inputs, with state as explicit input and its successor as output
- [07]-[STREAMS](references/streams.md): Values that arrive over time and processes that serialize changes to one shared value
- [08]-[EVENT_SOURCING](references/event-sourcing.md): Entity transitions stored as source of truth, with every snapshot and view derived from them
- [09]-[LANGUAGE_FEATURES](references/language-features.md): Features that replace a family of members or hand-written form with one declaration
- [10]-[ANALYZERS](references/analyzers.md): Roslyn analyzers and fixes, with findings established by resolved semantics and edits preserving meaning

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
- `fun` gives an inline lambda its delegate type
- Signatures leave semantic detail unstated (`Where` and `TakeWhile` share one), names state it
- Records hold inputs and outputs, functions hold behavior
- Members differing only in a number, sign, flag, or applied function are one member taking it as a parameter
- Paths a caller names arrive as arguments, and sibling paths derive from them (`Path.ChangeExtension(file, ".tmp")`)
- Constrained types exist only for a rule the domain owns, comparison stands where a consumer orders values, a type with no rule goes

### [01.1]-[HONEST_SIGNATURES]

Functions honor their signature when each declared input produces a declared output, they return no `null` and throw no exception as an outcome the signature omits. Correct a dishonest contract by narrowing the input to a validated type or widening the output to `Option<A>` or `Fin<A>`:
- `int -> Tier` is incomplete when some integers fail validation, and `Quantity -> Tier` is accurate for every constructible `Quantity`
- `string -> int` hides the undefined parses, `string -> Option<int>` describes every outcome, and `parseInt` has that signature
- One-value `Unit` removes the `void` split between `Func` and `Action` and between `Task` and `Task<T>`, `void` stays on an imperative API
- `fun` converts an `Action` into a `Func<Unit>`, and an `Action` overload delegates to the `Func<T>` implementation through it
- `Unit` does not make an effectful function pure
- Statements stay statements in a block body in place of `Unit` arithmetic or a discard ternary spelling them as a value
- Nullable reference types stay enabled, `?` appears only where an external representation requires it, `Optional` converts it to `Option<A>` there
- Absence stays absence: an empty string, zero, or other sentinel never stands for a missing value, and a host that reads `""` as absence receives it from the one member converting `Option<string>` for that host
- `!`, a cast of an `object?` result, and `ValueUnsafe` on a value element are unchecked escapes, `Optional` lifts the nullable result into the outcome that names its failure

### [01.2]-[PURITY]

Functions are pure when their return value depends only on their inputs (immutable values fixed at construction included) and evaluation causes no side effect, a pure call's result replaces the call (referential transparency). Side effects are mutating state visible outside the function (instance fields included), mutating an argument, throwing, and I/O (clock, console, filesystem, database, network, or another process). Instance methods that read mutable fields and lambdas that close over mutable variables are impure, mutation local to a function that never escapes is no side effect. Pure functions are safe for parallel evaluation, lazy evaluation, and memoization. Expose a variable dependency as input data, clock and reader's culture enter as arguments in place of ambient clock and culture:

```csharp
internal static class Stamps {
    public static string Format(Instant now, CultureInfo culture, string label) => string.Create(culture, $"{now} - {label}");
}
```

Pure methods can be static, every value they need is an argument or immutable, and a static method reads no mutable static field and performs no I/O a caller cannot replace.

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Higher-order functions accept a function, return one, or both, and a delegate is the function value. The higher-order function owns the stable control flow and the caller supplies the varying rule: `Filter` owns iteration and the caller owns the inclusion criterion, `IfNone(Func<T>)` owns the cache miss and the caller owns the fallback work. Closures combine a lambda with its declaring context, the declared signature stays unary and the computation reads the captured values beside its argument:

```csharp
Seq<DayOfWeek> days = toSeq(Enum.GetValues<DayOfWeek>());
Seq<DayOfWeek> StartingWith(string prefix) => days.Filter(day => day.ToString().StartsWith(prefix, StringComparison.Ordinal));
Seq<DayOfWeek> matched = StartingWith("S");
```

Function factories turn configuration into behavior. `compose(f, g)` joins functions into one function that applies `f` first (method groups need its explicit type arguments), method chaining expresses the same flow inline:

```csharp
internal static class Factories {
    public static Func<int, bool> IsMod(int divisor) => value => value % divisor is 0;
    public static Seq<int> MultiplesOfThree => toSeq(Range(1, 20)).Filter(IsMod(3));
}
```

Currying turns a function of `N` arguments into `N` unary functions (`curry`), partial application (`par`) fixes leading arguments and returns a function of the rest. Order parameters for left-to-right application: dependencies and configuration known at composition root, then policies that select behavior, then the runtime value. Dependencies are functions describing behavior a consumer needs: a clock is a `TimeProvider`, a validator `T -> Fin<T>`, a lookup `Guid -> Eff<RT, Option<T>>`, and persistence `T -> IO<Unit>`. Composition roots read configuration, adapt infrastructure into dependency functions, partially apply dependencies and policies, and inject only specialized functions into handlers. I/O calls make every delegating layer above them impure.

Select the form by the call site:
- Specialization when it simplifies call sites
- Function collections when behaviors share a signature and vary as data
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

Prefer a query over a mutable accumulator, a named reduction (`Min(seed)`, `Max(seed)`) over a `Fold` that restates it, `Fold` with a seed for any other reduction, and indexed `Map` over a declared counter. One-arm `x switch { var y => … }` bindings stay for a value an expression reads more than once or inside a lambda, and a binding read once or never is inlined.

A declaration states its type once, where it adds a fact: a local names its type and the value takes it from the target (`new()`, `[]`, `default`), and `var` stays in patterns. Text that differs by case is one expression choosing between complete values (`flag ? "Saved" : "Unsaved"`, or a switch expression), never a fragment spliced in or a statement per branch.

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

Each arm states an input category beside its outcome, and the switch expression forms a function body or a `Func` passed to a higher-order function. Pattern forms:
- Type patterns test a runtime type and bind the typed value only when the arm needs its properties
- Property patterns inspect the shape of an object
- Extended property patterns (`{ Parent.Name: "root" }`) inspect nested data without extracting each intermediate object
- Relational and logical patterns (`>`, `and`, `not`) express ranges and exclusions inside a pattern
- `when` adds a guard, `_` is the discard for the fallback arm
- List patterns recognize values at the beginning, middle, or end of an array with known headers or footers

Patterns can invoke getters, indexers, and `Deconstruct` with unspecified evaluation order. Keep those members pure, and decode fallible input into `Option<A>` or a closed union before matching.

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

### [02.3]-[RECURSION]

Recursive functions need a base case that returns the final value, a recursive case that calls the same function with values closer to that condition, and a returned value on every path. C# does not optimize tail calls, the depth bound and successor sharing select the form:

| [INDEX] | [APPROACH]                | [USE_WHEN]                                                   | [COST]                                   |
| :-----: | :------------------------ | :----------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Direct recursion          | Maximum depth small and bounded                              | Unbounded calls grow the stack           |
|  [02]   | `Trampoline<A>`           | Pure transition, unbounded depth                             | Every step is a deferred call            |
|  [03]   | `Monad.recur`             | Effectful transition, only the final value needed            | Intermediate states are lost             |
|  [04]   | `LanguageExt.List.unfold` | Intermediate states are meaningful and compose as a `Seq<A>` | Each `unfold` call reruns the transition |
|  [05]   | Graph traversal           | Shared or cyclic successors (dependencies, references)       | Graph built before the walk              |

Graphs build once into a read-only graph a record holds, successor functions walk as implicit graphs, and cyclic graphs order through their condensation with each cycle as one component. Recursive unions with children no other parent holds keep the union fold.

## [03]-[IMMUTABILITY]

Immutable values are fixed once created, a needed change derives a new value from existing values, and a state transition is a function that receives the old state and the information the change needs and returns the replacement. Language features constrain reassignment:
- `readonly` protects the fields and not the objects reached through them
- `init` accessors permit object-initializer syntax and prevent reassignment after initialization
- `required` marks a property that cannot be omitted, optional parts are `init` members or defaulted parameters in place of a positional `None`
- `record` supports nondestructive mutation through `with`, which copies the unchanged properties and leaves the original unchanged
- State machines pattern match the next interaction to select the transition
- Getter-only auto-properties get a compiler-generated readonly backing field, assigned inline or in the constructor
- Types are records, classes, and unions, a `struct` only for a native call, package API (parallel action), or JIT-specialized generic kernel

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
- `Lens<A, B>` with `lens(outer, inner)` updates a nested field without a chain of `with` expressions
- Confine local mutation to a scope that owns it, a `List<T>` or `Dictionary<K, V>` stays inside a scope that publishes an immutable value
- Mutable host options extending another copy it through the copy constructor and set the difference in an initializer (`new(source) { ... }`)

In-place updates (`List<T>.Sort()`) destroy the prior value, transformations preserve it and `values` keeps its order:

```csharp
Seq<int> values = Seq(7, 6, 1);
Seq<int> sorted = toSeq(values.Order());
```

Concurrent reorders of a shared list during a sum give readers an inconsistent traversal, a separate ordered view removes interference. Choose collections by their most frequent operation: `Seq<A>` for ordered reads, `Lst<A>` for indexed edits, `Map<K, V>` or `HashMap<K, V>` for keyed lookups successors update, `FrozenDictionary<K, V>` for a table built once and only read, `Set<A>` or `HashSet<A>` for uniqueness, and `Iterable<A>` for a lazy source.

## [04]-[RESULTS]

Every function returns an explicit result type, and one type serves one concern:

| [INDEX] | [TYPE]             | [CONCERN]                                     |
| :-----: | :----------------- | :-------------------------------------------- |
|  [01]   | `Option<A>`        | Absence without an `Error`                    |
|  [02]   | `Fin<A>`           | Expected failure, `Bind` stops, `Apply` joins |
|  [03]   | `Either<L, R>`     | Value of either type, neither an error        |
|  [04]   | `Validation<F, A>` | Failures of a monoid `F` other than `Error`   |
|  [05]   | `Try<A>`           | Synchronous exception capture, deferred       |
|  [06]   | `IO<A>`            | Side effects with a failure channel           |
|  [07]   | `Eff<RT, A>`       | Effects that read a capability                |

Input boundaries select the result type, functions preserve it, and conversions happen at one named boundary through the method named for the target (`ToFin`, `ToOption`, `ToSeq`). `Match`, `Run`, `RunSafe`, `IfNone`, and `IfFail` are host operations. Values of `A` and `Error` lift into `Fin<A>` by implicit conversion, `Pure(x)` and `Fail<Error>(e)` make the lift explicit where no target type fixes the result (a `var` local, an inferred type argument), and a `Unit` or `Error` in a position that fixes `Fin` converts alone. An empty `Seq<A>` is a result and not absence, `Option<Seq<A>>` exists only where the consumer responds differently to no collection and to an empty one. A producer that maps an operational failure to `None` hides the failure from the consumer.

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

- Errors from one traversed element hold the element index
- Values stay in one abstraction through the pipeline, an unwrap followed by a rewrap duplicates effect handling
- Nested `Bind` calls become a query when an inner step reads the callback value, an inner step reading nothing bound moves to the outer chain
- Final `from` clauses that `select` returns unchanged become `select <value>` over the earlier steps
- Nested contexts (`IO<Option<A>>`) stay nested, the consumer matches the inner `Option` once and no chain binds through it with a transformer
- Stacks that appear throughout a workflow become a dedicated type

```csharp
internal static class Workflow {
    public static Fin<Account> Handle(Command command, Account state) =>
        from normalized in Pure(Normalize(command)).ToFin()
        from _ in Fault.Unless(IsValid(normalized), Reason.Invalid, normalized)
        from next in state.Transition(normalized.Amount)
        select next;
}
```

### [04.2]-[ERRORS]

Expected failures are data in return types, and exceptions signal developer defects, initialization failures, or third-party calls a boundary converts with `IO.lift` or `Try.lift`. Each raising package declares an error record retaining rejected values and a typed reason. Optional facts take `Option` fields, findings collected without failing take union cases. Records extend `Expected` with code 0 and derive `Message` from current fields. `Unless` returns `Fin`, and `Create(string)` implements `IValidationError`. Rejected values keep their call text through `[CallerArgumentExpression]`:

```csharp
[SmartEnum]
public sealed partial class Reason {
    public static readonly Reason Stated = new("rejected input");
    public static readonly Reason Invalid = new("rule violated");
    public static readonly Reason Missing = new("missing");

    public string Message { get; }
}

public sealed record Fault(object? Subject, Reason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(Reason.Message, 0, Inner), IValidationError<Fault> {
    public object? Subject { get; } = Subject;
    public Reason Reason { get; } = Reason;
    public Option<int> Index { get; } = Index;

    public override string Message => Index.Match(
        at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject} at index {at}"),
        () => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject}"));

    public static Fault Create(string message) => new(message, Reason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, Reason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new Fault((names, rejected), reason, index);
}
```

Consumers classify an error by type with `IsType<E>` and `Filter<E>`, and a captured exception with `HasException<E>`. LanguageExt's `Errors.Cancelled`, `Errors.TimedOut`, `Errors.Closed`, and `Errors.ParseError` stand for those failures. A thrown exception stays the `Exceptional` the lift captured, and a boundary wraps it only to add a subject it lacks (a path), through `Catch` on its `HasException<E>` into an error that keeps the exception as its cause. `Fin<A>` holds violated business rules, `IO<A>` holds technical work and captures a thrown exception as `Exceptional`. Hosts separate both in one `Match`, `IsExceptional` logs detail and renders a generic failure, every other error renders its business message.

Host calls signaling failure with `bool`, `-1`, or `null` become `Fin` at the call (`store.Write(file) ? unit : Error.New(...)`), and a failed step the caller continues past keeps its error message in the outcome.

Use `Skill(dotnet-coding-thinktecture)` for value type validation.

### [04.3]-[UNIONS]

Model each valid state a consumer inspects as a distinct case. States differing only by a tag are one record keyed by a smart enum item holding the data consumers read. Special values of one formula are one record of its parameters. Counting values a type has (a product multiplies its fields' counts, `Option<A>` adds one) exposes unneeded states:

```csharp
[Union]
internal abstract partial record Shape {
    public double Area => Switch(
        circle: static circle => double.Pi * circle.Radius * circle.Radius,
        rectangle: static rectangle => rectangle.Width * rectangle.Height);

    internal sealed record Circle(double Radius) : Shape;
    internal sealed record Rectangle(double Width, double Height) : Shape;
}
```

- Shared derived members read current case data, record copies preserve stored base-constructor results unchanged
- Call `Switch` only where behavior depends on the case, and pass the union onward elsewhere
- Treat a case as an alternative to its siblings, name each case's data in its own terms, and hold mixed cases in one `Seq` of the base type
- Unions fit a growing set of operations, abstract members fit a growing set of cases
- Use a union of named outcomes when the consumer needs the names, and `Option`, `Fin`, or `Either` otherwise
- Keep absence and failure separate when the consumer responds differently
- Fold a recursive union with one replacement per constructor, and fold an unbounded depth through `Trampoline<A>`

### [04.4]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                              | [CORRECT_FORM]                                              |
| :-----: | :------------------------------------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | `Match` in the middle of a pipeline unwraps a value the next step relifts | `Bind` the next step                                        |
|  [02]   | `IfNone` with an arbitrary default hides absence                          | `ToFin` with an `Error`                                     |
|  [03]   | Matching on message text couples the consumer to prose                    | `IsType<E>` or `Filter<E>`                                  |
|  [04]   | `OptionT<IO, A>` or a match per step over an `Option` inside an effect    | `IO<Option<A>>` matched once by its consumer                |
|  [05]   | `Fin`, `Either<Error, A>`, `Validation`, or `Try` inside an effect        | `Expected` on `IO`, `ValidationT<Error, IO, A>` accumulates |
|  [06]   | `Run` inside a pipeline performs the effect before the host runs it       | Keep the `IO` and `Bind` the next step                      |
|  [07]   | `Some` as a null guard                                                    | `Optional` at the null boundary                             |
|  [08]   | Separate result and error fields, or `default` on failure                 | One result type with mutually exclusive cases               |
|  [09]   | Fields present together or meaningful under one flag                      | One `Option` of a record or one union case                  |
|  [10]   | Union case that extends a sibling case                                    | Sibling cases with their own data                           |
|  [11]   | `from` chain or `ToValidation` round trip over independent checks         | Tuple `Apply` or `Traverse` over `Fin` checks               |

## [05]-[EFFECTS]

`IO<A>` describes a side effect with a failure channel, performs nothing until the host runs it, and holds a rejection as an `Expected` on that channel in place of a nested result:
- `IO.lift` defers a thunk, `IO.lift(Fin<A>)` puts an evaluated rejection on the channel
- `IO.liftAsync` adapts a task thunk, its `EnvIO` overload passes `env.Token`
- Operations that wait on I/O enter through `IO.liftAsync` and have no synchronous counterpart that blocks a thread
- `IO.pure` and `IO.fail` build the plain cases
- `Eff<RT, A>` reads a capability from a runtime `RT` through `Has<Eff<RT>, T>`, `IO<A>` converts to it implicitly

Host runs the effect once, `RunSafe()` returns the `Fin<A>` for translation into the host's vocabulary, and `Run` throws and belongs to `Main`:

```csharp
internal static class Host {
    public static int Exit(IO<Unit> handler) =>
        handler.RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => error.IsExpected ? 4 : 1);
}
```

### [05.1]-[DEPENDENCIES]

Inject the narrowest dependency that represents what consumers need: a value for a stable snapshot, an `IO<A>` for an operation that must run on demand, a runtime `RT` for a consumer that reads many capabilities, and one interface resolved once for a seam each platform implements in full. Clocks behind an interface leave consumers impure, an injected date makes a consumer deterministic:

```csharp
internal static class Validators {
    public static Func<Command, Fin<Command>> NotPast(LocalDate today) =>
        command => Fault.Unless(command.Date >= today, Reason.Invalid, command.Date).Map(_ => command);
}
```

Hosts read the date once from a `TimeProvider` argument (`time.ToZonedClock().GetCurrentDate()`). Request handling supplies each command's date later. `TimeProvider` supplies elapsed time through `GetTimestamp()` and `GetElapsedTime(start).ToDuration()`, each read inside its consuming `IO`, and the composition root alone names `TimeProvider.System`. `Duration` compares and scales as a value (`a - b < step * n`) and is the one time type model fields hold, a `TimeSpan`, tick, or second count appears only at an API taking one. Hosts log through `ILogger` with `[LoggerMessage]` members from one `LoggerFactory` per session, a sink interface over `(Error, Type, string)` restates it. Pair acquisition and release through `use`, closing its resource scope with `Bracket()`. Release functions return `void` or `IO<Unit>`, a commit follows successful work outside release.

### [05.2]-[POLICIES]

Failure policies are distinct operations:
- Fallback runs a lower-priority effect when the preferred one fails, `primary | secondary`, or `primary | @catch(predicate, f)` for one error class
- Retry reruns a transient failure on a schedule, `Retry(Schedule.exponential(delay) | Schedule.recurs(attempts))`
- Recovery maps a final error to a value at the host, `IfFail` on the `Fin<A>`

Libraries return their result type with their own errors, the application composes the retry schedule, the fallback order, and the cache around it, and the host logs only a failure that reaches its translation. Traversals beyond the composition operators take a concurrency bound or keep every failure:

| [INDEX] | [SCENARIO]                  | [TRAVERSAL]                             | [BEHAVIOR]                                            |
| :-----: | :-------------------------- | :-------------------------------------- | :---------------------------------------------------- |
|  [01]   | Bounded concurrency         | Chunk, then `TraverseM` over the chunks | One chunk at a time, the chunk width sets the bound   |
|  [02]   | Every effect, failures kept | `PartitionFallible`                     | Serial, no short-circuit, returns `Fails` and `Succs` |

Keep state unshared, and when one logical value must be shared, serialize its changes:
- `Atom<A>` replaces the value with compare-and-swap
- `AtomHashMap<K, V>` holds a registry with `FindOrAdd`
- `Ref<A>` under `atomic` commits coordinated updates
- `Conduit` reduced under `Fork` serializes commands as an agent
- Conflicts rerun the update function, the function stays free of effects

Delivery shape selects the construct:
- `Source<A>` fits values that arrive over time, logic across events or sources (sequences, transitions, windows), and one-way dataflow
- Expected per-item failures stay `Fin<A>` values inside the stream
- Independent events take a callback or an `IO<A>`, request-response work stays out of a stream
- Coordination that needs explicit queues and exact sequencing takes a `Conduit` as the queue or `Pipes` as the pipeline

## [06]-[NUMERICS]

Numeric facts come from the package that states them. Sample loops read only spans and SIMD lanes, with `Matrix<double>`, quantities, and `BigInteger` outside the loop:
- Roots, quadrature, splines, fits, quantiles, factorizations, antiderivatives, and gradients are members of the referenced numerics packages
- Hand loops that converge, bisect, or sum a series are a defect
- `TryFindRoot` receives a signed margin, bracket, caller accuracy, and iteration limit, a missed root stays a typed failure
- Per-sample passes prepare native transforms once, bulk kernels or runtime-supported vectors consume allocation-free samples
- Conversions every pass needs (a host buffer's 2D view, a matrix to lanes) sit once at the type that owns them
- Exact integer-plus-fraction arithmetic on a hot path is fixed-point on `long` or `Int128`, a shift for the cell and a mask for the local part
- `Length` and `Angle` name parameters, bounds, and presentation units, a `float` holds samples and a host unit scale enters once at the boundary
- Pure geometry takes package value types, a host frame enters where a result lowers to a host type, one extension family converts between them

| [INDEX] | [FACT]         | [MEMBER]                             |
| :-----: | :------------- | :----------------------------------- |
|  [01]   | Scalar root    | `Bisection.TryFindRoot`              |
|  [02]   | Quadrature     | `Integrate.GaussKronrod`             |
|  [03]   | Spline         | `CubicSpline.InterpolatePchipSorted` |
|  [04]   | Fit            | `Fit.Polynomial`                     |
|  [05]   | Quantile       | `SortedArrayStatistics.Quantile`     |
|  [06]   | Factorization  | `Matrix<double>.Svd`                 |
|  [07]   | Antiderivative | `Polynomial.Integrate`               |

- Use `Skill(dotnet-coding-memory)` for spans, pooled buffers, 2D views, parallel passes, and span kernels
- Use `Skill(dotnet-coding-ui)` for drawn surfaces, shaped text, vector art, color, and host controls
