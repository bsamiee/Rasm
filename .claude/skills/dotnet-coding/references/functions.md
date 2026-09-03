# [FUNCTIONS]

Covers functions as values: the combinators that apply and join functions, functions stored as data, specialization through partial application and currying, adapters that expose delegates, the composition root, and the end-to-end flow from a request to its effects. Which delegate a signature takes, how arguments are ordered, and which dependency shape a consumer receives are decisions in `dotnet-coding`.

## [01]-[COMBINATORS]

Combinators apply or join functions. `Pipe` applies one function to a whole value, where `Map` applies a function to each element of a sequence, so piping a sequence treats the sequence as the input value, and the generic input and output types let each step change type. LanguageExt has no `Pipe`, and this implementation is custom:

```csharp
internal static class Piping {
    public static TOut Pipe<TIn, TOut>(this TIn value, Func<TIn, TOut> transform) => transform(value);
    public static string Scaled(decimal input) =>
        input
            .Pipe(static x => x - 32).Pipe(static x => x * 5).Pipe(static x => x / 9)
            .Pipe(static x => Math.Round(x, 2, MidpointRounding.ToEven))
            .Pipe(static x => string.Create(CultureInfo.InvariantCulture, $"{x} units"));
}
```

`Fork` gives the same input to more than one function and passes their outputs to a joining function, separate generic result types let a fixed set of functions produce different kinds of value, each further fixed function needs another overload, and a `Seq` of prongs holds any number when each returns the same intermediate type:

```csharp
internal static class Forks {
    public static TOut Fork<TIn, TLeft, TRight, TOut>(this TIn value, Func<TIn, TLeft> left, Func<TIn, TRight> right, Func<TLeft, TRight, TOut> join) =>
        join(left(value), right(value));
    public static TOut Fork<TIn, TPart, TOut>(this TIn value, Func<Seq<TPart>, TOut> join, Seq<Func<TIn, TPart>> prongs) =>
        join(prongs.Map(prong => prong(value)));
    public static double Average(Seq<double> values) =>
        values.Fork(static s => s.Fold(0.0, static (total, x) => total + x), static s => s.Count, static (sum, count) => sum / count);
}
```

`flip` from the Prelude swaps the two parameters of a `Func<A, B, R>`, so `flip(Subtract)` receives the right operand first, and an adapter of that kind returns a new function with a different signature while delegating to the original.
- See `dotnet-languageext` for `Do`, `when`, and `unless`, the observing and guarding steps that sit between transformations

## [02]-[FUNCTIONS_AS_DATA]

Functions stored in collections, passed into adapters, or returned express control flow as data, and functions stored together need compatible signatures, where every type in `Func<T1, ..., TResult>` except the last is a parameter type. Selectors parameterize a larger function: a report builder written once accepts its grouping key, and each new report supplies a selector and a title in place of copying the grouping and row construction:

```csharp
internal static class Reports {
    public static Report Summary(Seq<Item> items, Func<Item, string> groupBy, string title) =>
        new(title, toSeq(items.GroupBy(groupBy, StringComparer.Ordinal)).Map(static g => new Row(g.Key, string.Create(CultureInfo.InvariantCulture, $"{g.Count()}"))));
}
```

Broader higher-order functions centralize retrieval, empty-result handling, transmission, and error handling while only the selector and the name vary, and small named wrappers preserve intent. Collections of transformations apply many views to one input, assemble at runtime, extend by one element, and stay separate from the aggregation, where `Seq.Map` is deferred so the functions run when the result is enumerated:

```csharp
internal static class Descriptions {
    private static readonly Seq<Func<Item, string>> Descriptors = [
        static item => $"Name: {item.Name}",
        static item => $"Kind: {item.Kind}",
    ];
    public static string Describe(Item item) => string.Join(Environment.NewLine, Descriptors.Map(describe => describe(item)));
}
```

Validation rules of shape `T -> bool` collect into a policy: `ForAll` fits rules that state what valid input must satisfy and stops at the first failed rule, `Exists` fits rules that describe a violation and stops at the first one detected, an empty validity set returns `true` and an empty violation set returns `false`, and each rule states one condition:

```csharp
internal static class Policies {
    public static bool IsValid<T>(this T value, Seq<Func<T, bool>> rules) => rules.ForAll(rule => rule(value));
    public static bool IsInvalid<T>(this T value, Seq<Func<T, bool>> violations) => violations.Exists(rule => rule(value));
}
```

Short-circuiting suits a boolean answer and not a report of every failure, because later rules do not run, and validators that return typed errors accumulate instead. The `if` and `else if` chain becomes an ordered table of predicate and transform pairs, where the first matching predicate wins, the order is part of the meaning, each predicate holds its criteria or delegates to a named function, and the explicit fallback makes the operation total:

```csharp
internal static class RuleTables {
    public static TOutput Match<TInput, TOutput>(this TInput value, Func<TInput, TOutput> fallback, Seq<(Func<TInput, bool> When, Func<TInput, TOutput> Then)> cases) =>
        cases.Find(c => c.When(value)).Match(Some: c => c.Then(value), None: () => fallback(value));
    public static decimal Net(decimal amount) =>
        amount.Match(static x => x * 0.55m, [
            (static x => x <= 12_570m, static x => x),
            (static x => x <= 50_270m, static x => x * 0.80m),
            (static x => x <= 150_000m, static x => x * 0.60m),
        ]);
}
```

`Seq.Find` returns an `Option`, the missing case is `None`, and its `Match` selects the fallback without a null check, where a staged design that infers no match by comparing the transformed value with `default(TOutput)` fails because a matching transform can return `0`, `false`, or `null`. The table matches values with predicates and not object types, a fixed decision over types is a native switch expression, and `KeyValuePair` in place of tuples adds syntax without changing the mechanism.

Returned functions capture the original value in a closure and expose one operation: `number => names.Find(number).IfNone("unknown")` narrows a `HashMap<int, string>` to one lookup, keeps the map in scope, converts an absent key into a fallback, and prevents the caller from enumerating, modifying, or otherwise querying the map. Repeated parsing branches move into focused conversion functions, where `parseInt(text).IfNone(fallback)` collapses missing and invalid input into one fallback, so it suits only a caller that need not distinguish them, and `IfNone` applies at the boundary that selects the fallback because `parseInt` and `HashMap.Find` preserve every outcome.

## [03]-[SPECIALIZATION]

`par` exists for each arity and number of supplied arguments, `lpar` fixes the second argument of a two-argument function, and partial application avoids unary stages when a group of arguments is always fixed together:

```csharp
internal sealed record Record(string Name, string Group, string Stamp);

internal static class Parsing {
    public const bool SkipHeader = true;
    public const bool KeepHeader = false;
    public static readonly Func<bool, string, string, string, Seq<Record>> ParseRecords =
        static (skipHeader, lineBreak, fieldDelimiter, content) =>
            toSeq(content.Split(lineBreak))
                .Skip(skipHeader ? 1 : 0)
                .Map(line => line.Split(fieldDelimiter))
                .Map(static fields => new Record(fields[0], fields[1], fields[2]));
}

internal static class Partials {
    public static readonly Func<string, Seq<Record>> ParseUnixComma = par(Parsing.ParseRecords, Parsing.KeepHeader, "\n", ",");
    public static readonly Func<string, string, Seq<Record>> ParseWindows = par(Parsing.ParseRecords, Parsing.SkipHeader, Environment.NewLine);
    public static readonly Func<string, Seq<Record>> ParseWindowsComma = par(ParseWindows, ",");
}
```

`curry` turns the same four-argument delegate into `bool -> string -> string -> string -> Seq<Record>`, each stage can be saved and reused for many specializations, and a function written in curried form suits consumers that always supply one argument at a time:

```csharp
internal static class Families {
    public static readonly Func<bool, Func<string, Func<string, Func<string, Seq<Record>>>>> Curried = curry(Parsing.ParseRecords);
    public static readonly Func<string, Func<string, Func<string, Seq<Record>>>> WithHeader = Curried(true);
    public static readonly Func<string, Func<string, Seq<Record>>> WindowsWithHeader = WithHeader(Environment.NewLine);
    public static readonly Func<string, Seq<Record>> WindowsComma = WindowsWithHeader(",");
    public static readonly Func<string, Seq<Record>> WindowsPipe = WindowsWithHeader("|");
    public static readonly Func<decimal, Func<decimal, decimal>> Add = curry(static (decimal x, decimal y) => x + y);
    public static readonly Func<decimal, decimal> Add10 = Add(10m);
}
```

Explicit lambda parameter types are needed where the compiler does not infer the delegate's generic arguments at the `curry` call, a delegate value with a declared `Func` type needs no annotation, the returned function retains each supplied value and invokes the original only when the remaining arguments arrive, and currying adds nothing when every argument arrives together. Fixing a logger's level the same way creates one function per level that needs only a message. Partial application turns a general operation into a unary function for mapping or composition, where a noncommutative operation fixes its first parameter first and takes the pipeline value last:

```csharp
internal static class Scaling {
    private static readonly Func<decimal, decimal, decimal> SubtractBase = static (fixedValue, input) => input - fixedValue;
    private static readonly Func<decimal, decimal, decimal> MultiplyBase = static (fixedValue, input) => input * fixedValue;
    private static readonly Func<decimal, decimal, decimal> DivideBase = static (fixedValue, input) => input / fixedValue;
    public static decimal Scaled(decimal value) => value.Pipe(par(SubtractBase, 32m)).Pipe(par(MultiplyBase, 5m)).Pipe(par(DivideBase, 9m));
}
```

The costs are `Func` conversion with occasional explicit type annotations and nested delegate types that read poorly at higher arities, so ordinary functions stay when the helper is larger or less readable than the duplication it removes.

## [04]-[ADAPTERS]

Unary methods convert where a `Func<T, R>` is expected, generic higher-order operations over multi-argument method groups defeat type inference, local functions share that limit, and explicit generic arguments or delegate casts add syntax, so a function used in partial application is exposed as a delegate value, where each form has one reach:

```csharp
internal sealed class Greeter(string separator) {
    public static readonly Func<string, string, string> Greet = static (greeting, name) => $"{greeting}, {name}";
    public Func<string, string, string> GreetProperty => (greeting, name) => $"{greeting}{separator}{name}";
    public static Func<string, TName, string> CreateGreeter<TName>() => static (greeting, name) => $"{greeting}: {name}";
    public static string GreetInformally(string name) => fun(static (string greeting, string who) => $"{greeting} {who}")("Hey", name);
}
```

- Field initializers cannot depend on instance state
- Getter-only properties create delegates that close over instance state
- Factory methods introduce generic type parameters, which fields and properties cannot
- `fun` gives a lambda its `Func` type at the call site, so it is invoked or passed without a declared local

Existing APIs can expose arguments in an order that works poorly for partial application, and an adapter exposes domain-specific types in place of ambiguous primitives, acquires a short-lived resource only when the operation runs, and returns a `Func` so later specialization benefits from delegate inference:

```csharp
internal sealed record Template(string Text);
internal sealed record Entry(Guid Id, string Name, decimal Amount);

internal interface ConnectionIO {
    public Seq<T> Query<T>(Template template, object parameters);
}

internal static class Queries {
    public static readonly Template EntryById = new("select * from entry where id = @Id");
    public static Func<Template, object, Eff<RT, Seq<T>>> Query<RT, T>() where RT : Has<Eff<RT>, ConnectionIO> =>
        static (template, parameters) => Has<Eff<RT>, RT, ConnectionIO>.ask.As().Map(connection => connection.Query<T>(template, parameters));
}

internal static class Lookups<RT> where RT : Has<Eff<RT>, ConnectionIO> {
    private static readonly Func<Template, object, Eff<RT, Seq<Entry>>> QueryEntries = Queries.Query<RT, Entry>();
    private static readonly Func<object, Eff<RT, Seq<Entry>>> QueryById = par(QueryEntries, Queries.EntryById);
    public static readonly Func<Guid, Eff<RT, Option<Entry>>> Lookup = static id => QueryById(new { Id = id }).Map(static rows => rows.Head);
}
```

The runtime `RT` supplies the connection through `Has<Eff<RT>, ConnectionIO>`, the runtime record holds the capability from startup while the implementation opens the short-lived connection when the query runs, the effect reads the trait only when the host runs it and not when the function is built, the template specializes the query to one operation and leaves only the invocation parameters, and `Seq<A>.Head` is an `Option<A>` so lookup absence stays explicit. Custom types (`ConnectionIO`, `Template`) make the signature intention-revealing and own extension methods that do not belong on `string`.

## [05]-[COMPOSITION_ROOT]

Function dependencies decouple the consumer from the implementation, let tests inject deterministic functions, need no single-method interface or mock setup, and enforce interface segregation: a consumer that only saves receives `T -> IO<Unit>` and not a repository abstraction that exposes lookup beside saving, and independent behaviors stay separate functions. Objects and interfaces remain compatible, and functional behavior can sit behind a framework controller that handles requests and responses.

The runtime record holds the configuration and implements one `Has` trait per capability, the workflow is generic over `RT` and builds an `Eff<RT, A>` from its function dependencies, `Eff<RT, A>.Lift(Func<Fin<A>>)` carries a `Fin` into the query, a `from` clause binds the `IO<Unit>` dependency, and the host runs the effect once with `Run(rt)`, which returns `Fin<A>`:

```csharp
internal sealed record OwnerUnknown() : Expected("the owner is unknown", 101);
internal sealed record Runtime(ConnectionIO Connection, Func<DateTime> Clock) : Has<Eff<Runtime>, ConnectionIO> {
    static K<Eff<Runtime>, ConnectionIO> Has<Eff<Runtime>, ConnectionIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Connection);
}

internal static class Workflow {
    public static Eff<RT, Entry> Book<RT>(
        Func<Command, Validation<Error, Command>> validate, Func<Guid, Eff<RT, Option<Entry>>> lookup,
        Func<Command, IO<Unit>> save, Command command) =>
        from valid in Eff<RT, Command>.Lift(() => validate(command).ToFin())
        from owner in lookup(valid.OwnerId)
        from found in Eff<RT, Entry>.Lift(() => owner.ToFin(new OwnerUnknown()))
        from _ in save(valid)
        select found;
}

internal static class Host {
    public static Fin<Entry> Book(Runtime runtime, Func<Command, IO<Unit>> save, Command command) =>
        Workflow.Book(Validators.NotPast(runtime.Clock), Lookups<Runtime>.Lookup, save, command).Run(runtime);
}
```

The framework entry point stays thin while the behavior it invokes arrives as narrow functions, composition is ordinary function application without an inversion-of-control container, and `Validators.NotPast` is the clock-taking validator factory that composition specializes once.

## [06]-[END_TO_END_FLOW]

Compositional programs are sequences of typed transformations, and each step's effect on the value and its enclosing structure stays visible, where reusable operations hold the iteration, branching, and enumeration mechanics and a terminal operation evaluates the preceding lazy sequence:

```csharp
internal static class DataFlow {
    public static decimal AverageOfTopQuartile(Seq<Entry> population) =>
        population
            .OrderByDescending(static e => e.Amount) // reorder, preserve elements
            .Take(population.Count / 4)              // reduce cardinality
            .Average(static e => e.Amount);          // project and collapse to a scalar
}
```

Small general building blocks beat one specific aggregate operation, and a function composes when it is pure, chainable through an instance or extension receiver, general in performing one operation for many uses, structure-preserving in returning the outer structure it accepts where possible, and non-`void` in returning data for the next function, while a terminal operation reduces, materializes, or performs an effect and ends the chain:

```csharp
internal static class Quartiles {
    public static Seq<Entry> TopQuartile(this Seq<Entry> population) =>
        toSeq(population.OrderByDescending(static e => e.Amount)).Take(population.Count / 4);
    public static decimal AverageAmount(this IEnumerable<Entry> population) => population.Average(static e => e.Amount);
    public static decimal Result(Seq<Entry> population) => population.TopQuartile().AverageAmount();
}
```

Nested lambdas in a fluent chain hide the first incorrect transformation, and a query with a `from` clause per stage keeps every stage bound once and inspectable, at the cost that a large intermediate value stays in scope until the containing function ends, so stages combine when a large value must be released sooner:

```csharp
internal static class Stages {
    public static Option<int> Inspect(int id) =>
        from initial in Some(id)
        from first in StepOne(initial)
        from second in StepTwo(first)
        from result in StepThree(second)
        select result;
}
```

Immutable state stays separate from behavior, and a pure transition returns its expected failure as data in the return type, produces a value that later steps consume, and leaves the original state unchanged:

```csharp
internal sealed record State(decimal Balance);
internal sealed record Insufficient() : Expected("insufficient balance", 902);

internal static class Transitions {
    public static Fin<State> Debit(this State state, decimal amount) =>
        state.Balance < amount ? new Insufficient() : new State(state.Balance - amount);
}
```

Boundary services expose reads as `OptionT<IO, A>` and writes as `IO<Unit>` while the transition stays pure: the repository lifts its `Option` read with `OptionT.lift`, `Run` unwraps the `OptionT` layer, `ToFin` with a typed `Expected` puts absence on the `IO` error channel, and `IO.lift(Fin<A>)` carries the transition's rejection onto the same channel:

```csharp
internal sealed record NotFound() : Expected("state not found", 903);

internal interface IRepository<T> {
    public OptionT<IO, T> Get(Guid id);
    public IO<Unit> Save(Guid id, T value);
}
internal interface INotifier {
    public IO<Unit> Send(Command command, State state);
}

internal sealed class MemoryStates : IRepository<State> {
    private readonly AtomHashMap<Guid, State> store = AtomHashMap<Guid, State>();

    public OptionT<IO, State> Get(Guid id) => OptionT.lift<IO, State>(IO.lift(() => store.Find(id)));
    public IO<Unit> Save(Guid id, State value) => IO.lift(() => store.AddOrUpdate(id, value));
}
internal sealed class Handler(IRepository<State> states, INotifier notifier) {
    public IO<Unit> Handle(Command command) =>
        from state in Require(states.Get(command.TargetId))
        from next in IO.lift(Workflow.Handle(command, state))
        from _ in states.Save(command.TargetId, next)
        from __ in notifier.Send(command, next)
        select unit;

    private static IO<State> Require(OptionT<IO, State> lookup) =>
        lookup.Run().As().Bind(static option => IO.lift(option.ToFin(new NotFound())));
}
```

`Get` can find no state and `Workflow.Handle` can reject the command, `Handle` binds both on one `IO` error channel instead of nesting result types, `Save` and `Send` run only when both succeed as visible steps of the query, and the host runs `Handle` with `RunSafe`, which returns `Fin<Unit>` with the typed error. Expressions move effects to explicit boundaries in this order: receive external input, transform and validate through expressions, compute the new domain state with pure functions, then persist or communicate at the effect boundary, and a terminal step with more than one effect keeps each one visible. Composition has limits:
- `Option` discards the reason for a failure and `Fin` keeps it, so `Option` short-circuits but cannot distinguish a missing state from an insufficient balance
- Composition does not make distributed effects atomic, saving a state and sending a notification can fail between the operations, and a database transaction cannot protect an external call from a process failure after the call and before the commit
- One multi-system pattern persists a representation of the combined work atomically, processes it until every effect completes, and makes repeat execution safe through idempotency
- Confidence comes from tests, not from inspecting the implementation and assuming its abstracted operations are correct
