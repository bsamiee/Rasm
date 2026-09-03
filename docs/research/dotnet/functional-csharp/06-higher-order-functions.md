# [HIGHER_ORDER_FUNCTIONS]

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [01]-[FUNCTIONS_AS_VALUES]

Higher-order functions accept a function, return one, or both. C# represents the passed behavior with delegates:
- `Func<T, TResult>` accepts a value and returns a value
- `Action<T>` accepts a value and returns nothing
- Lambdas supply an inline delegate: `items.Where(x => x.IsActive)`
- Functions can create a new function: `Func<int, int> AddBy(int amount) => value => amount + value;`

Delegates treat behavior as a first-class value.
-->

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [02]-[BEHAVIOR_PARAMETERIZATION]

Higher-order functions can own stable control flow while the caller supplies the varying rule. For example, `Seq<A>.Filter(Func<A, bool>)` owns iteration while the caller owns the inclusion criterion. This separates concerns that would otherwise be interleaved. The pattern supports:
- Iteration: invoke a selector, predicate, or comparison for each relevant element
- Conditional execution: invoke a callback only when needed, for example after a cache miss
- Inversion of control: the caller chooses what behavior to supply, and the higher-order function chooses when to run it

When optional work can be expensive, accept it as a function to evaluate it only when needed:

```csharp
internal sealed record Cache<T>(HashMap<Guid, T> Entries) {
    public T Get(Guid id, Func<T> onMiss) => Entries.Find(id).IfNone(onMiss);
}
```

`HashMap.Find` returns an `Option<T>`, and `IfNone(Func<T>)` runs the function only on `None`.
-->

### [02.1]-[SELECTORS]

Selectors derive a value from each input. `Func` delegates can supply that calculation to a larger function. For example, a reporting workflow can accept a selector for its grouping key:

```csharp
internal static class Reports {
    public static Report BuildSummary(Seq<EnemyShip> ships, Func<EnemyShip, string> summarizeBy, string title) =>
        new(title, toSeq(ships.GroupBy(summarizeBy, StringComparer.Ordinal)).Map(static g => new ReportItem(g.Key, string.Create(CultureInfo.InvariantCulture, $"{g.Count()}"))));
}
```

The summary construction is written once. New reports supply a selector and title rather than copying the grouping and row construction. Broader higher-order functions can centralize retrieval, empty-result handling, transmission, and error handling while leaving only the selector and report name variable.

Additional parameters let a `Func` control formatting and an `Action` supply logging or event handling. Small named wrappers preserve intent.

## [03]-[FUNCTION_ADAPTERS]

Adapters return a new function with a different signature while delegating to the original. `flip` from the Prelude swaps the two parameters of a `Func<A, B, R>`: for `Func<decimal, decimal, decimal> Subtract`, `flip(Subtract)` receives the right operand first.

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [04]-[SPECIALIZATION]

Function factories convert configuration data into behavior:

```csharp
internal static class Factories {
    public static Func<int, bool> IsMod(int divisor) => value => value % divisor == 0;
    public static Seq<int> MultiplesOfThree => toSeq(Range(1, 20)).Filter(IsMod(3));
}
```

The factory centralizes a general rule and produces reusable specializations.
-->

## [05]-[RESOURCE_LIFECYCLES]

Setup, body, and teardown form a higher-order pattern. Parameterize the changing body while keeping resource management in one place:

```csharp
internal static class Lifecycles {
    public static IO<int> Scoped =>
        from connection in use(static () => new Connection())
        select connection.Query();
    public static IO<int> Bracketed =>
        IO.lift(static () => new Connection()).Bracket(Use: static c => IO.pure(c.Query()), Fin: static c => IO.lift(fun(c.Dispose)));
}
```

Database operations can state only their domain-specific work. Connection acquisition, opening, and disposal remain centralized. `use` acquires the `IDisposable` `Connection` inside an `IO` query and disposes it when the scope ends. `Bracket(Use:, Fin:)` represents release as a separate `IO` action. The host runs the `IO` through `RunSafe`, and the domain code never runs it. Asynchronous bodies use `IO.liftAsync`. The pattern guarantees disposal on every `IO` exit, including failure.

## [06]-[COMBINATORS]

Combinators apply or combine functions.

### [06.1]-[PIPE]

`Pipe` applies one function to a whole value. `Map` keeps its structure-preserving meaning over a sequence. LINQ `Select` and the functor `Map` apply a function to each sequence element. With this extension, piping a sequence treats the sequence as the input value. LanguageExt has no equivalent `Pipe` operation. This implementation is custom.

```csharp
internal static class Piping {
    public static TOut Pipe<TIn, TOut>(this TIn value, Func<TIn, TOut> transform) => transform(value);
    public static string Celsius(decimal fahrenheit) =>
        fahrenheit
            .Pipe(static x => x - 32).Pipe(static x => x * 5).Pipe(static x => x / 9)
            .Pipe(static x => Math.Round(x, 2, MidpointRounding.ToEven))
            .Pipe(static x => string.Create(CultureInfo.InvariantCulture, $"{x} degrees C"));
}
```

The generic input and output types allow each step to change type. The chain expresses a multi-stage calculation without temporary variables.

### [06.2]-[FORK]

`Fork`, also called Converge, gives the same input to multiple functions and passes their outputs to a combining function:

```csharp
internal static class Forks {
    public static TOut Fork<TIn, TLeft, TRight, TOut>(this TIn value, Func<TIn, TLeft> left, Func<TIn, TRight> right, Func<TLeft, TRight, TOut> join) =>
        join(left(value), right(value));
    public static double Average(Seq<double> values) =>
        values.Fork(static s => s.Fold(0.0, static (total, x) => total + x), static s => s.Count, static (sum, count) => sum / count);
    public static TOut Fork<TIn, TPart, TOut>(this TIn value, Func<Seq<TPart>, TOut> join, Seq<Func<TIn, TPart>> prongs) =>
        join(prongs.Map(prong => prong(value)));
}
```

Separate generic result types let a fixed set of functions produce different kinds of value. Supporting more fixed functions requires more overloads. `Seq` can hold any number when each function returns the same intermediate type.

These implementations call each function directly.

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
### [06.3]-[COMPOSE]

`Pipe` produces a transformed value. `compose` from the Prelude joins functions and produces another function:

```csharp
internal static class Conversions {
    public static readonly Func<decimal, decimal> FahrenheitToCelsius = static x => (x - 32) * 5 / 9;
    public static readonly Func<decimal, string> Format = static x => string.Create(CultureInfo.InvariantCulture, $"{Math.Round(x, 2, MidpointRounding.ToEven)} degrees");
    public static readonly Func<decimal, string> FormattedConversion = compose(FahrenheitToCelsius, Format);
}
```

The reusable formatting function can be composed with conversions in either direction. Improvements to that function are made once. `compose(f, g)` applies `f` first and then `g`. First give a lambda a delegate type, `compose` can infer its type arguments.

C# has no dedicated syntax for function composition. Use method chaining for value flow and `compose` when the output must be a reusable function.
-->

### [06.4]-[DO]

`Do` passes the current value to an `Action`, then returns that same value, the chain can continue:

```csharp
internal static class Observers {
    public static Option<int> Logged(Option<int> value, Action<int> log) => value.Do(log);
    public static Seq<int> Traced(Seq<int> values, Action<int> log) => values.Do(log);
}
```

It can log or inspect an intermediate result between transformations. `Do` on an `Option` runs the action on `Some`, and `Do` on a `Seq` runs it for each element before the `Seq` returns.

### [06.5]-[UNLESS]

`unless` runs an effect only when its flag is false, and `when` runs it only when the flag is true:

```csharp
internal static class Guards {
    public static IO<Unit> WarnWhenEmpty(int stock, Action<string> notify) =>
        unless(stock > 0, IO.lift(() => notify("out of stock"))).As();
    public static IO<Unit> WarnWhenFull(int stock, Action<string> notify) =>
        when(stock > 100, IO.lift(() => notify("overstocked"))).As();
}
```

The skipped branch has no computed result. Both return `IO<Unit>` for the host to run.

## [07]-[FUNCTIONS_AS_DATA]

Storing functions in collections, passing them into adapters, or returning them can express control flow as data:
In `Func<T1, ..., TResult>`, every type except the last is a parameter type, and the final type is the return type. Functions stored together need compatible signatures.

- Collections of transformations apply several views to one input
- Collections of predicates become validation policies
- Ordered collections of predicate-transform pairs become decision tables
- Returned functions can narrow access to one guarded operation, and ordinary adapter functions can hide repetitive conversion branches

### [07.1]-[TRANSFORMATION_COLLECTIONS]

Apply all functions in a `Seq` to one value:

```csharp
internal static class Descriptions {
    private static readonly Seq<Func<Employee, string>> Descriptors = [
        static employee => $"First name: {employee.FirstName}",
        static employee => $"Last name: {employee.LastName}",
        static employee => $"Role: {employee.Role}",
    ];
    public static string Describe(Employee employee) => string.Join(Environment.NewLine, Descriptors.Map(describe => describe(employee)));
}
```

The function collection can be assembled at runtime, extended by adding one element, and kept separate from aggregation. `Seq.Map` is deferred: the descriptor functions run when the result is enumerated. This can postpone unnecessary work.

### [07.2]-[PREDICATE_SETS]

Validation rules have shape `T -> bool`. Collecting rules makes the policy explicit:

```csharp
internal static class Policies {
    public static bool IsValid<T>(this T value, Seq<Func<T, bool>> rules) => rules.ForAll(rule => rule(value));
    public static bool IsInvalid<T>(this T value, Seq<Func<T, bool>> violations) => violations.Exists(rule => rule(value));
}
```

Use `ForAll` when each predicate states what valid input must satisfy. Use `Exists` when each predicate describes a violation:
- `ForAll` stops at the first failed rule
- `Exists` stops at the first detected violation
- Empty validity rule sets return `true`, empty violation sets return `false`

Short-circuiting is appropriate for a boolean answer. It is not suitable when every failure must be reported, because later rules do not run. Validators that return typed errors accumulate every failure instead.

Keep each rule focused on one condition.

### [07.3]-[RULE_TABLES]

`if`/`else if` ladders can be represented as ordered pairs:

```csharp
internal static class RuleTables {
    public static TOutput Match<TInput, TOutput>(
        this TInput value,
        Func<TInput, TOutput> fallback,
        Seq<(Func<TInput, bool> When, Func<TInput, TOutput> Then)> cases) =>
        cases.Find(c => c.When(value)).Match(Some: c => c.Then(value), None: () => fallback(value));

    public static decimal NetIncome(decimal income) =>
        income.Match(static x => x * 0.55m, [
            (static x => x <= 12_570m, static x => x),
            (static x => x <= 50_270m, static x => x * 0.80m),
            (static x => x <= 150_000m, static x => x * 0.60m),
        ]);
}
```

The first matching predicate wins. Ordering is part of the meaning. Each predicate can contain detailed criteria or delegate that decision to a named function. Fallbacks make the operation defined for every input. `Seq.Find` returns an `Option`, the missing case is `None` and the `Match` on that `Option` selects the fallback without a null check. Staged `.Match(...).DefaultMatch(...)` designs must track whether a predicate matched. Inferring "no match" by comparing the transformed value with `default(TOutput)` fails, because a matching transformation can return `0`, `false`, or `null`. Passing the fallback directly avoids that ambiguity.

This matches values with predicates, not object types. For a fixed decision, use a native switch expression. Using `KeyValuePair` instead of tuples adds syntax without changing the mechanism.

The custom `Match<T>` accepts any `T`. `Option<T>.Match` requires a handler for every option case.

### [07.4]-[RETURNED_FUNCTIONS]

Returned functions can capture the original value in a closure while exposing only one operation.

#### [07.4.1]-[DICTIONARY_LOOKUP]

Closures over a `HashMap<int, string>` narrow it to one lookup: `number => actors.Find(number).IfNone("Unknown")`.

The returned function keeps the map in scope and converts an absent-key lookup into a fallback. The restricted interface prevents callers from enumerating or modifying the map, or performing other queries against it. For this reference type, `default` is `null`. Explicit domain fallbacks avoid that `null`.

#### [07.4.2]-[PARSING]

Move repeated parsing branches into focused conversion functions:

```csharp
internal static class Parsing {
    public static int ToInt(string text, int fallback) => parseInt(text).IfNone(fallback);
}
```

Call sites can construct a settings value directly, and every default is visible beside its setting. This technique collapses missing and invalid input into the same fallback. Use it only when callers do not need to distinguish those cases.

The `Option`-returning forms, `parseInt` and `HashMap.Find`, preserve every outcome. `IfNone` extracts a value from the `Option` by applying a fallback. Call it at the boundary that selects that fallback.

## [08]-[EXCEPTIONS_AT_THE_BOUNDARY]

Boundary calls to databases, web APIs, and network files can fail. Higher-order wrappers centralize `try/catch` and keep exception control flow out of the call layers. `Try.lift(f).Run()` captures a throwing synchronous dependency as a `Fin<A>`. `IO.lift(f)` defers the same call and carries the failure on the `IO` error channel for the host to run.

<!-- Integrated into .claude/skills/dotnet-coding/SKILL.md
## [09]-[TECHNIQUE_SELECTION]

- Use function collections when behaviors share a signature and vary as data
- Use `ForAll` or `Exists` when only a short-circuiting boolean result is required
- Use an ordered rule table for first-match decisions, always with an explicit fallback
- Return a closure when intentionally narrowing an unsafe or noisy API to one operation
- Pair each element with its neighbor through `Zip` against `Tail` when the condition depends on adjacent elements
- Use recursive state transitions only when termination is bounded, with `Trampoline` in pure code and `Monad.recur` in effectful code for deep recursion

Higher-order functions add callback frames. Debuggers show less direct control flow. Do not remove boilerplate at the cost of hiding ordering, effects, missing-value behavior, or termination risk.
-->
