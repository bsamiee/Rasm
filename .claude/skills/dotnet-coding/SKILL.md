---
name: dotnet-coding
description: "Use when writing or reviewing C#, covering functional design with result and effect types"
---

# [DOTNET_CODING]

Covers C# design decisions under workspace standards (TOTALITY, PURITY, BOUNDARY), from signature shape to host boundary. LanguageExt, Thinktecture, and Mapperly are part of the language, every example uses their types as vocabulary.

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Worked flows for functions as values, from combinators to the composition root and a request's effects
- [02]-[SEQUENCES](references/sequences.md): Worked flows behind the expression rules, from deferral to a text-to-report pipeline
- [03]-[RESULTS](references/results.md): Worked flows for validators, `Fin` workflows, host translation, union folds, and the law tests
- [04]-[IMMUTABLE_DATA](references/immutable-data.md): Snapshot and transition model behind the immutability rules, with persistent structure costs
- [05]-[EFFECTS](references/effects.md): Worked flows for effects at the boundary, from isolating I/O and injection to running the composed effect
- [06]-[STATE](references/state.md): Worked flows for state passed through `State` and `StateT`, with the generators and the loop forms
- [07]-[STREAMS](references/streams.md): Observable model and agent model, with per-item failure, backpressure, and entity processes
- [08]-[EVENT_SOURCING](references/event-sourcing.md): Event history as the source of truth, from storage to the command and query sides

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
- Constrained types own only the validation that constructs them and the operations (comparison) that protect their representation

### [01.1]-[HONEST_SIGNATURES]

Functions honor their signature when each declared input produces a declared output, they return no `null` and throw no exception as an outcome the signature omits. Correct a dishonest contract by narrowing the input to a validated type or widening the output to `Option<A>` or `Fin<A>`:
- `int -> Tier` is incomplete when some integers fail validation, and `Quantity -> Tier` is accurate for every constructible `Quantity`
- `string -> int` hides the undefined parses, `string -> Option<int>` describes every outcome, and `parseInt` has that signature
- `Unit` is the one-value type that removes the split `void` makes between `Func` and `Action` and between `Task` and `Task<T>`
- `void` stays on an imperative API that returns nothing
- `fun` converts an `Action` into a `Func<Unit>`, and an `Action` overload delegates to the `Func<T>` implementation through it
- `Unit` does not make an effectful function pure
- Statements stay statements in a block body in place of `Unit` arithmetic or a discard ternary spelling them as a value
- Nullable reference types stay enabled, `?` appears only where an external representation requires it
- `Optional` converts a nullable value to `Option<A>` at the parse boundary

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
    public static Func<int, bool> IsMod(int divisor) => value => value % divisor == 0;
    public static Seq<int> MultiplesOfThree => toSeq(Range(1, 20)).Filter(IsMod(3));
}
```

Currying turns a function of `N` arguments into `N` unary functions (`curry`), and partial application (`par`) fixes a leading group of arguments and returns a function of the rest. Order parameters for useful left-to-right application: dependencies and configuration known at the composition root, then policies that select behavior, then the runtime value. Dependencies are functions that describe the behavior the consumer needs: a clock is a `TimeProvider`, a validator is `T -> Validation<Error, T>`, a lookup is `Guid -> Eff<RT, Option<T>>`, and persistence is `T -> IO<Unit>`. The composition root reads configuration, adapts infrastructure into functions of that shape, partially applies dependencies and policies, and injects only specialized functions into handlers. I/O calls make every delegating layer above them impure.

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

Each arm states an input category beside its outcome, and the switch expression forms a function body or a `Func` passed to a higher-order function. Pattern forms:
- Type patterns test a runtime type and bind the typed value only when the arm needs its properties
- Property patterns inspect the shape of an object
- Extended property patterns (`{ Parent.Name: "root" }`) inspect nested data without extracting each intermediate object
- Relational and logical patterns (`>`, `and`, `not`) express ranges and exclusions inside a pattern
- `when` adds a guard, `_` is the discard for the fallback arm
- List patterns recognize values at the beginning, middle, or end of an array with known headers or footers

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

### [02.3]-[RECURSION]

Recursive functions need a base case that returns the final value, a recursive case that calls the same function with values closer to that condition, and a returned value on every path. C# does not optimize tail calls, the depth bound and successor sharing select the form:

| [INDEX] | [APPROACH]                | [USE_WHEN]                                                   | [COST]                                   |
| :-----: | :------------------------ | :----------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Direct recursion          | Maximum depth small and bounded                              | Unbounded calls grow the stack           |
|  [02]   | `Trampoline<A>`           | Pure transition, unbounded depth                             | Every step is a deferred call            |
|  [03]   | `Monad.recur`             | Effectful transition, only the final value needed            | Intermediate states are lost             |
|  [04]   | `LanguageExt.List.unfold` | Intermediate states are meaningful and compose as a `Seq<A>` | Each `unfold` call reruns the transition |
|  [05]   | QuikGraph traversal       | Shared or cyclic successors (dependencies, references)       | Graph built before the walk              |

QuikGraph `DepthFirstSearchAlgorithm` and `BreadthFirstSearchAlgorithm` visit each vertex of an `AdjacencyGraph<V, E>` once. Recursive unions with children no other parent holds keep the union fold.

## [03]-[IMMUTABILITY]

Immutable values are fixed once created, a needed change derives a new value from existing values, and a state transition is a function that receives the old state and the information the change needs and returns the replacement. Language features constrain reassignment:
- `readonly` protects the fields and not the objects reached through them
- `init` accessors permit object-initializer syntax and prevent reassignment after initialization
- `required` marks a property that cannot be omitted
- `record` supports nondestructive mutation through `with`, which copies the unchanged properties and leaves the original unchanged
- State machines pattern match the next interaction to select the transition
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

None of `readonly`, `init`, and `with` makes a referenced child object immutable, immutability extends through every referenced value:
- Remove setters and construct through a constructor or factory that enforces required values and invariants, an invalid snapshot cannot exist
- Seal the type, store collections as `Seq<A>`, copy a mutable input collection at the boundary with `toSeq`, and keep element types immutable
- `Lens<A, B>` with `lens(outer, inner)` updates a nested field
- Confine local mutation to a scope that owns it, a `List<T>` or `Dictionary<K, V>` stays inside a scope that publishes an immutable value

In-place updates destroy the prior value:

```csharp
List<int> values = [7, 6, 1];
values.Sort();
```

Transformations preserve it, `values` keeps its order:

```csharp
Seq<int> values = Seq(7, 6, 1);
Seq<int> sorted = toSeq(values.Order());
Seq<int> odd = values.Filter(static x => x % 2 == 1);
```

Concurrent reorders of a shared list during a sum give the reader an inconsistent traversal, a separate ordered view removes the interference. Choose the collection by the operation performed most: `Seq<A>` for ordered reads, `Lst<A>` for indexed edits, `Map<K, V>` or `HashMap<K, V>` for keyed lookups, `Set<A>` or `HashSet<A>` for uniqueness, and `Iterable<A>` for a lazy source.

## [04]-[RESULTS]

Every function returns an explicit result type, and one type serves one concern:

| [INDEX] | [TYPE]                 | [CONCERN]                                       |
| :-----: | :--------------------- | :---------------------------------------------- |
|  [01]   | `Option<A>`            | Absence without an `Error`                      |
|  [02]   | `Fin<A>`               | Expected failure with an `Error`, short-circuit |
|  [03]   | `Either<L, R>`         | Value of either type, neither an error          |
|  [04]   | `Validation<Error, A>` | Independent failures, accumulate                |
|  [05]   | `Try<A>`               | Synchronous exception capture, deferred         |
|  [06]   | `IO<A>`                | Side effects with a failure channel             |
|  [07]   | `Eff<RT, A>`           | Effects that read a capability                  |

Input boundary selects the result type, functions preserve it, and conversion between types happens at one named boundary through the method named for the target (`ToFin`, `ToValidation`, `ToOption`, `ToSeq`). `Match`, `Run`, `RunSafe`, `IfNone`, and `IfFail` are host operations. Values of `A` and `Error` lift into `Fin<A>` and `Validation<Error, A>` by implicit conversion, `Pure(x)` and `Fail<Error>(e)` make the lift explicit where no target type fixes the result (a `var` local, an inferred type argument), and a `Unit` or `Error` in a position that fixes `Fin` or `Validation` converts alone. An empty `Seq<A>` is a result and not absence, `Option<Seq<A>>` exists only where the consumer responds differently to no collection and to an empty one. A producer that maps an operational failure to `None` hides the failure from the consumer.

### [04.1]-[COMPOSITION]

Give each step a function and select the operator by the step's signature and by whether the steps depend on each other:

| [INDEX] | [SIGNATURE]                | [OPERATOR]                             | [BEHAVIOR]                                                  |
| :-----: | :------------------------- | :------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `T -> R`                   | `Map`                                  | Transforms a present value and preserves the result type    |
|  [02]   | `T -> bool`                | `Filter` on `Option`, `guard` on `Fin` | Keeps a value only when it passes, `guard` names the error  |
|  [03]   | `T -> F<R>`                | `Bind`, one `from` per step in LINQ    | Continues with a dependent step, the first failure stops it |
|  [04]   | `(F<A>, F<B>)` independent | Tuple `Apply`                          | Accumulates errors under `Validation`, overlaps under `IO`  |
|  [05]   | `Seq<A>` with `A -> F<B>`  | `Traverse`, `TraverseM`                | `Traverse` accumulates or overlaps, `TraverseM` stops first |
|  [06]   | `T -> void`                | `Iter`                                 | Performs the terminal effect only for a present value       |

- Errors from one traversed element hold the element index
- Values stay in one abstraction through the pipeline, an unwrap followed by a rewrap duplicates effect handling
- Nested `Bind` calls with an inner step reading the callback value become a query
- Inner steps reading nothing bound move up to the outer chain
- Final `from` clauses that `select` returns unchanged become `select <value>` over the earlier steps
- Nested contexts (`IO<Option<A>>`) stay nested, the consumer matches the inner `Option` once and no chain binds through it with a transformer
- Stacks that appear throughout a workflow become a dedicated type

```csharp
internal static class Workflow {
    private static readonly Error InvalidCommand = Error.New(ErrorCodes.ValidationFailed, "command is invalid");

    public static Fin<State> Handle(Command command, State state) =>
        from normalized in Pure(Normalize(command)).ToFin()
        from _ in guard(IsValid(normalized), InvalidCommand)
        from next in state.Transition(normalized.Amount)
        select next;
}
```

Workflow runs in order (normalize, validate, transition), `Fin` handles the control flow, and adding a step means defining one function and inserting one clause.

### [04.2]-[ERRORS]

Expected failures are data in return types, and exceptions are reserved for developer defects that violate a precondition, configuration failures during initialization, and exception-based third-party calls a boundary converts with `IO.lift` or `Try.lift` around the throwing call alone. Each raising package declares one error record holding the subject and a reason as fields, the reason a smart enum whose items carry their message text, a fact some failures lack is an `Option` field. Findings a check collects into a report without failing are a data union, one case per finding kind rendering its message. Records extend `Expected` with code 0 and an interpolated message. Records hold fields and one `Unless` factory returning `Fin` (`flag ? unit : new <Record>(…)`) alone, a limit or other operation sits in the raising class. `Unless` takes its subject from call text through `[CallerArgumentExpression]`:

```csharp
public sealed record Failure(string Subject, Reason Reason, Option<int> Index) : Expected($"{Subject} failed: {Reason}", 0);
```

Consumers classify an error by type with `IsType<E>` and `Filter<E>`, a validation failure with `HasCode(ErrorCodes.ValidationFailed)`, and a captured exception with `HasException<E>`. LanguageExt's `Errors.Cancelled`, `Errors.TimedOut`, `Errors.Closed`, and `Errors.ParseError` stand for those failures. A thrown exception stays the `Exceptional` the lift captured, and a boundary wraps it only to add a subject it lacks (a path), through `Catch` on its `HasException<E>` into an error that keeps the exception as its cause. `Validation<Error, A>` holds violated business rules and accumulates, `IO<A>` holds technical work and captures a thrown exception as `Exceptional`. The host separates them in one `Match`, `IsExceptional` logs the detail and renders a generic failure, and every other error renders its business message.

Use `Skill(dotnet-coding-thinktecture)` for value type validation.

### [04.3]-[UNIONS]

Model each valid state a consumer inspects as a distinct case, and states differing only by a tag as one record of tag and value. Counting values a type has (a product multiplies its fields' counts, `Option<A>` adds one) exposes unneeded states:

```csharp
[Union]
internal abstract partial record Identity {
    internal sealed record Registered(string Name, string Key, bool Enabled) : Identity;
    internal sealed record Anonymous(string Key) : Identity;
}
```

- Put shared data on the abstract base and case-specific data in its case
- Call `Switch` only where behavior depends on the case, and pass the union onward elsewhere
- Treat a case as an alternative to its siblings, name each case's data in its own terms, and hold mixed cases in one `Seq` of the base type
- Every `Switch` lacking an arm for a new union case fails to compile
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
|  [09]   | Flag with fields that are meaningful for one flag value                   | One union case per valid state                              |
|  [10]   | Union case that extends a sibling case                                    | Sibling cases with their own data                           |

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
    private static readonly Error DateIsPast = Error.New(ErrorCodes.ValidationFailed, "date is in the past");

    public static Func<Command, Validation<Error, Command>> NotPast(LocalDate today) =>
        command => command.Date < today ? DateIsPast : command;
}
```

Hosts read the date once from a `TimeProvider` argument (`time.ToZonedClock().GetCurrentDate()`), request handling supplies each command later, and tests supply a date without a fake clock. `TimeProvider` supplies elapsed time through `GetTimestamp()` and `GetElapsedTime(start).ToDuration()`, each read inside its consuming `IO`, and the composition root alone names `TimeProvider.System`. `Duration` compares and scales as a value (`a - b < step * n`), a tick or second count appears at a host API taking a number alone. Hosts log through `ILogger` with `[LoggerMessage]` members from one `LoggerFactory` per session, a sink interface over `(Error, Type, string)` restates it. Pair acquisition and release in one scope with `use` for an `IDisposable` or `Bracket(Use:, Fin:)` for a named release action, a commit is a step after the work, outside its release action.

### [05.2]-[POLICIES]

Failure policies are distinct operations:
- Fallback runs a lower-priority effect after the preferred one fails, `primary | secondary`, or `primary | @catch(predicate, f)` for one classified error
- Retry reruns a transient failure on a schedule, `Retry(Schedule.exponential(delay) | Schedule.recurs(3))`
- Recovery maps a final error to a value at the host, `IfFail` on the `Fin<A>`

Libraries return their result type with their own errors, the application composes the retry schedule, the fallback order, and the cache around it, and the host logs only a failure that reaches its translation. The dependency structure and the concurrency bound select the traversal:

| [INDEX] | [SCENARIO]                   | [TRAVERSAL]                             | [BEHAVIOR]                                            |
| :-----: | :--------------------------- | :-------------------------------------- | :---------------------------------------------------- |
|  [01]   | Independent checks           | `Traverse` under `Validation`           | Accumulates every error                               |
|  [02]   | Independent effects          | `Traverse` under `IO`                   | Overlaps without a bound, fails if one effect fails   |
|  [03]   | Dependent or ordered effects | `TraverseM`                             | Serial, stops at the first failure                    |
|  [04]   | Bounded concurrency          | Chunk, then `TraverseM` over the chunks | One chunk at a time, the chunk width sets the bound   |
|  [05]   | Every effect, failures kept  | `PartitionFallible`                     | Serial, no short-circuit, returns `Fails` and `Succs` |

Independent effects combine with the tuple `Apply` or with `Fork` and `Await`, `awaitAll` runs a `Seq<IO<A>>`. Keep state unshared, and when one logical value must be shared, serialize its changes:
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

Numeric facts come from the package that states them, and a sample loop reads only spans and SIMD lanes:
- Scalar roots, quadrature, splines, fits, quantiles, and factorizations are MathNet members
- Hand loops that converge, bisect, or sum a series are a defect
- Elementwise and reduction loops over a float span are one `TensorPrimitives` call, a loop stays where each element is a `Vector4` pixel or reads neighbors
- Flat pixel buffers are read through `Span2D<T>` and `GetRowSpan` with the source pitch, a hand `[(y * width) + x]` is a defect
- Row-parallel passes are `ParallelHelper.For<TAction>` over a `readonly struct : IAction`, a `Parallel.For` closure is a defect
- Per-sample passes lift a matrix or curve once into `Vector256<double>` or `Vector4` lanes and allocate nothing, `Matrix<double>`, `Unicolour`, UnitsNet quantities, and `BigInteger` stay outside the loop
- Exact integer-plus-fraction arithmetic on a hot path is fixed-point on `long` or `Int128`, a shift for the cell and a mask for the local part
- `Length` and `Angle` name parameters, bounds, and presentation units, a `float` holds samples and a host unit scale enters once at the boundary
- Host decompositions and projections (`Transform.DecomposeAffine`, `Quaternion.Slerp`, `Plane.FitPlaneToPoints`) have no package member beside them

| [INDEX] | [FACT]        | [MEMBER]                             |
| :-----: | :------------ | :----------------------------------- |
|  [01]   | Scalar root   | `Brent.FindRoot`                     |
|  [02]   | Quadrature    | `Integrate.GaussKronrod`             |
|  [03]   | Spline        | `CubicSpline.InterpolatePchipSorted` |
|  [04]   | Fit           | `Fit.Polynomial`                     |
|  [05]   | Quantile      | `SortedArrayStatistics.Quantile`     |
|  [06]   | Factorization | `Matrix<double>.Svd`                 |

Use `Skill(dotnet-coding-ui)` for drawn surfaces, shaped text, vector art, color, and host controls.

```csharp
internal static class Rows {
    public static void Scaled(ReadOnlySpan2D<float> source, float gain, Span2D<float> target) {
        for (int row = 0; row < source.Height; row++) {
            TensorPrimitives.Multiply(source.GetRowSpan(row), gain, target.GetRowSpan(row));
        }
    }
}
```
