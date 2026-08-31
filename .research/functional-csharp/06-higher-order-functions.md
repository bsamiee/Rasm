# Higher-Order Functions

## Functions as values

A higher-order function accepts a function, returns a function, or both. C# usually represents the passed behavior with delegates:
- `Func<T, TResult>` accepts a value and returns a value.
- `Action<T>` accepts a value and returns nothing.
- A lambda supplies an inline delegate, as in `items.Where(x => x.IsActive)`.
- A function can create a new function: `Func<int, int> AddBy(int amount) => value => amount + value;`.

This makes behavior configurable. A workflow can keep its fixed sequence while callers provide only the calculation, formatting, or effect that varies. This is the main capability unlocked by first-class functions.

## Delegate part of an algorithm

A higher-order function can own stable control flow while the caller supplies the varying rule. `Seq<A>.Filter(Func<A, bool>)` is that shape: the function owns iteration, and the caller owns the inclusion criterion. This separates concerns that would otherwise be interleaved. The same shape supports:
- **Iterated execution:** invoke a selector, predicate, or comparison for each relevant element.
- **Conditional execution:** invoke a callback only when needed, such as computing a value after a cache miss.
- **Inversion of control:** the caller chooses what behavior to supply; the higher-order function chooses when to run it.

When optional work may be expensive, accept it as a function so it is evaluated only when needed:

```csharp
internal sealed record Cache<T>(HashMap<Guid, T> Entries) {
    public T Get(Guid id, Func<T> onMiss) => Entries.Find(id).IfNone(onMiss);
}
```

`HashMap.Find` returns an `Option<T>`, and `IfNone(Func<T>)` runs the function only on `None`.

### Thunks: defer the variable calculation

A thunk bundles a calculation so it can be executed later. In C#, a `Func` delegate can fill a deliberate hole in a larger function. A reporting workflow, for example, can accept the property used to group its input:

```csharp
internal static class Reports {
    public static Report BuildSummary(Seq<EnemyShip> ships, Func<EnemyShip, string> summarizeBy, string title) =>
        new(title, toSeq(ships.GroupBy(summarizeBy, StringComparer.Ordinal)).Map(static g => new ReportItem(g.Key, string.Create(CultureInfo.InvariantCulture, $"{g.Count()}"))));
}
```

The summary construction is written once. A new report supplies a selector and title rather than copying the grouping and row construction. The same higher-order boundary can also centralize retrieval, empty-result routing, transmission, and error handling while leaving only the selector and report name variable.

The same opening can be widened deliberately: another `Func` can control formatting, while an `Action` can supply logging or event handling. Exposing the configurable operation lets callers extend the report set without modifying the implementation; keeping small named wrappers preserves specific intent where that is useful.

## Adapt an existing function

An adapter returns a new function with a more useful interface while delegating to the original. `flip` from the Prelude swaps the two parameters of a `Func<A, B, R>`: for `Func<decimal, decimal, decimal> Subtract`, `flip(Subtract)` receives the right operand first.

Function interfaces are therefore not fixed at the call site. Small adapters can reshape them without modifying the underlying implementation.

## Create specialized functions

A function factory converts configuration data into behavior:

```csharp
internal static class Factories {
    public static Func<int, bool> IsMod(int divisor) => value => value % divisor == 0;
    public static Seq<int> MultiplesOfThree => toSeq(Range(1, 20)).Filter(IsMod(3));
}
```

The factory centralizes a general rule and produces readable, reusable specializations.

## Encapsulate resource lifecycles

Setup, body, and teardown form another useful higher-order pattern. Parameterize the changing body while keeping resource management in one place:

```csharp
internal static class Lifecycles {
    public static IO<int> Scoped =>
        from connection in use(static () => new Connection())
        select connection.Query();
    public static IO<int> Bracketed =>
        IO.lift(static () => new Connection()).Bracket(Use: static c => IO.pure(c.Query()), Fin: static c => IO.lift(fun(c.Dispose)));
}
```

Database operations can now state only their domain-specific work. Connection acquisition, opening, and disposal remain centralized. `use` acquires the `IDisposable` `Connection` inside an `IO` query and disposes it when the scope ends. `Bracket(Use:, Fin:)` names the release as its own `IO` step. The host runs the `IO` through `RunSafe`, and the domain code never runs it. An asynchronous body enters the same `IO` through `IO.liftAsync`.

This technique becomes more valuable as lifecycle logic grows more intricate or is reused more widely. It provides:
- less duplication;
- a clear boundary between resource management and domain behavior;
- concise callers that expose their actual intent;
- guaranteed disposal on every exit of the `IO`, including the failure path.

## Combinators

A combinator applies or combines functions to build richer behavior. A small vocabulary covers common transformation shapes.

### Pipe: transform one whole value

This value-level operation, also called Chain or Pipe, applies one function to one value. `Map` keeps its structure-preserving meaning over a sequence. LINQ `Select` and the functor `Map` apply a function to each element of a sequence. Piping a sequence with this extension treats the sequence itself as the value. LanguageExt has no value-level pipe, so `Pipe` stays hand-rolled.

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

The generic input and output types allow each step to change type. The chain expresses a multi-stage calculation without throwaway variables.

### Fork: derive several values, then join

`Fork`, also called Converge, gives the same input to multiple prong functions and passes their outputs to a join function:

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

Separate generic result types let fixed prongs produce different kinds of value. More fixed prongs require more overloads. An arbitrary number of prongs is possible when every prong returns the same intermediate type, held in a `Seq`.

The shown implementations invoke their prongs through ordinary calls; the important relationship is that every prong receives the original input before their results are joined.

### Compose: build a reusable function

`Pipe` produces a transformed value. `compose` from the Prelude joins functions and produces another function:

```csharp
internal static class Conversions {
    public static readonly Func<decimal, decimal> FahrenheitToCelsius = static x => (x - 32) * 5 / 9;
    public static readonly Func<decimal, string> Format = static x => string.Create(CultureInfo.InvariantCulture, $"{Math.Round(x, 2, MidpointRounding.ToEven)} degrees");
    public static readonly Func<decimal, string> FormattedConversion = compose(FahrenheitToCelsius, Format);
}
```

The reusable formatting suffix can now be composed with conversions in either direction. Improvements to that suffix are made once. `compose(f, g)` applies `f` first and then `g`. In C#, first give a lambda a delegate type so `compose` can infer its type arguments.

C# has no dedicated syntax for function composition, and a composition function does not improve the readability of nested right-to-left calls such as `f(g(x))`. `compose` reads left to right, so that objection does not apply to it. Method chaining is the C# form for value flow. `compose` is used when a reusable function value is the required output.

### Do: observe a chain without changing its value

`Do` passes the current value to an `Action`, then returns that same value so the chain can continue:

```csharp
internal static class Observers {
    public static Option<int> Logged(Option<int> value, Action<int> log) => value.Do(log);
    public static Seq<int> Traced(Seq<int> values, Action<int> log) => values.Do(log);
}
```

It can log or otherwise inspect an intermediate result between transformations. The action is an effect, but it does not replace the value moving through the chain. `Do` on an `Option` runs the action on `Some`, and `Do` on a `Seq` runs it for each element before the `Seq` returns.

### Unless: conditionally perform an effect

`unless` runs an effect only when its flag is false, and `when` runs it only when the flag is true:

```csharp
internal static class Guards {
    public static IO<Unit> WarnWhenEmpty(int stock, Action<string> notify) =>
        unless(stock > 0, IO.lift(() => notify("out of stock"))).As();
    public static IO<Unit> WarnWhenFull(int stock, Action<string> notify) =>
        when(stock > 100, IO.lift(() => notify("overstocked"))).As();
}
```

The skipped branch has no computed result, so both return `IO<Unit>` for the host to run.

## Treat functions as data

A `Func` is a value that describes behavior. Once functions are stored in collections, passed into adapters, or returned from other functions, control flow can be expressed as data:

In `Func<T1, ..., TResult>`, every type except the last is a parameter type; the final type is the return type. Functions stored together therefore need compatible signatures.

- A collection of transformations applies several views to one input.
- A collection of predicates becomes a validation policy.
- An ordered collection of predicate-transform pairs becomes a decision table.
- A returned function can narrow access to one guarded operation; ordinary adapter functions can hide repetitive conversion branches.

This style removes repeated branching while leaving the variable part - the rule or transformation - visible.

### Collections of transformations

Store functions with the same input and output types in a `Seq`, then apply all of them to one value:

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

The behavior set can be assembled dynamically, extended by adding one element, and kept separate from aggregation. `Seq.Map` is deferred: the descriptor functions run when the result is enumerated. This can postpone unnecessary work, while repeated enumeration repeats the projection. The indexed form `Map((item, index) => ...)` rewrites one position and returns a new `Seq`, leaving the source unchanged.

### Validation as a predicate set

A validation rule has the shape `T -> bool`. Collecting rules makes the policy explicit:

```csharp
internal static class Policies {
    public static bool IsValid<T>(this T value, Seq<Func<T, bool>> rules) => rules.ForAll(rule => rule(value));
    public static bool IsInvalid<T>(this T value, Seq<Func<T, bool>> violations) => violations.Exists(rule => rule(value));
}
```

Use `ForAll` when each predicate states what valid input must satisfy. Use `Exists` when each predicate describes a violation. Both short-circuit:
- `ForAll` stops at the first failed rule.
- `Exists` stops at the first detected violation.
- An empty validity rule set returns `true`; an empty violation set returns `false`.

Short-circuiting is appropriate for a boolean answer. It is not suitable when every failure must be reported, because later rules may never run. A validator that returns typed errors accumulates every failure instead.

Keep each rule focused on one condition. The compact form works because the rules carry the variable logic while `ForAll` or `Exists` supplies the repeated branching and early return.

### Ordered rule tables

An `if`/`else if` ladder can be represented as ordered pairs:

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

The first matching predicate wins, so ordering is part of the meaning. Each predicate can contain detailed criteria or delegate that decision to a named function. A fallback makes the operation defined for every input. `Seq.Find` returns an `Option`, so the missing case is `None` and the `Match` on that `Option` selects the fallback without a null check. A staged `.Match(...).DefaultMatch(...)` design must track whether a predicate matched; it cannot infer "no match" by comparing the transformed value with `default(TOutput)`, because a matching transformation may legitimately return `0`, `false`, or `null`. Passing the fallback directly, as above, avoids that ambiguity.

This is value-based predicate matching, not object-type matching. Its distinctive value is that the cases can be assembled as data; when the decision is fixed, a native switch expression is usually clearer. Tuple-free encodings such as `KeyValuePair` preserve the mechanism but add enough syntax to erase much of the benefit.

This `Match` is an ordered rule table on any `T`. The `Match` on `Option<T>` requires a handler for every case. The two share a name and nothing else.

### Functions as protective filters

A returned function can sit between callers and an awkward API, capturing the original value in a closure while exposing only the safe operation.

#### Dictionary lookup with a fallback

A closure over a `HashMap<int, string>` narrows it to one lookup: `number => actors.Find(number).IfNone("Unknown")`.

The returned function keeps the map in scope and converts an absent-key lookup into a fallback. The tradeoff is intentional interface narrowing: callers can no longer enumerate, query, or modify the value through the map. A type default is often `null`, so an explicit domain fallback is usually clearer.

#### Parse-or-default adapters

Move repeated parsing branches into focused conversion functions:

```csharp
internal static class Parsing {
    public static int ToInt(string text, int fallback) => parseInt(text).IfNone(fallback);
}
```

Call sites can then construct a settings value directly, making every default visible beside its setting. This technique deliberately collapses missing and invalid input into the same fallback; use it only when callers do not need to distinguish those cases.

The default for parsing and lookup is the `Option`-returning form: `parseInt` and `HashMap.Find` describe every outcome. `IfNone` closes the `Option` and belongs to the boundary that owns the fallback.

## Exceptions at the edge

Pure transformations should not fail because of external conditions, but calls at the edge - databases, web APIs, network files - can. A higher-order wrapper can centralize `try/catch`, reduce repeated boilerplate, and keep exception-driven jumps from being scattered across call layers. `Try.lift(f).Run()` captures a throwing synchronous dependency as a `Fin<A>`. `IO.lift(f)` defers the same call and carries the failure on the `IO` error channel for the host to run.

## Choosing the technique

- Use function collections when behaviors share a signature and vary as data.
- Use `ForAll` or `Exists` when only a short-circuiting boolean result is required.
- Use an ordered rule table for first-match decisions, always with an explicit fallback.
- Return a closure when intentionally narrowing an unsafe or noisy API to one operation.
- Pair each element with its neighbor through `Zip` against `Tail` when the condition depends on adjacent elements.
- Use recursive state transitions only when termination is bounded, run a deep pure loop through `Trampoline`, and run a deep effect loop through `Monad.recur`.

Conciseness is valuable when it exposes intent. Higher-order functions add callback frames, so a debugger shows less direct control flow. If an abstraction hides ordering, effects, missing-value semantics, or termination risk, the removed boilerplate has merely been displaced.
