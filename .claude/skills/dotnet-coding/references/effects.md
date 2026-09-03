# [EFFECTS]

Worked flows for keeping effects at the boundary: isolating I/O around a pure core, injecting values and effects, deferring work as `IO<A>` and `Try<A>`, reading an environment through `Reader`, scoping resources, and running the composed effect at the host.

## [01]-[ISOLATION]

Useful programs require I/O, so the goal is a small impure boundary rather than universal purity, and each kind of effect gets its own treatment: I/O is isolated, argument mutation is replaced by returned data, errors are results, and non-local state is designed away. List every non-local value a function reads and every externally visible change it makes, then extract the deterministic computation. The effectful function describes the reads and writes as an `Eff<RT, Unit>` that the host performs at `Run(rt)`, and the deterministic logic sits in a pure function it calls:

```csharp
internal static class Prompting {
    public static Eff<RT, Unit> Prompt<RT>() where RT : Has<Eff<RT>, ConsoleIO> =>
        from _ in Console<RT>.writeLine("Enter a value:")
        from value in Console<RT>.readLine
        from __ in Console<RT>.writeLine(Render(value))
        select unit;
    public static string Render(string value) => $"Received {value}";
}
```

An output parameter represented by a mutable collection hides part of a function's result and couples caller and callee through initialization rules and mutation order, so every computed value returns explicitly, and an operation that both mutates an object and calculates a result splits into the calculation, which stays pure, and the mutation:

```csharp
internal sealed record Item(string Name, decimal Price);
internal sealed record Line(Item Item, int Quantity);
internal sealed record Order(Seq<Line> Lines);

internal static class Totals {
    public static (decimal Total, Seq<Line> LinesToRemove) Compute(Order order) =>
        (
            order.Lines.Fold(0m, static (total, line) => total + (line.Item.Price * line.Quantity)),
            order.Lines.Filter(static line => line.Quantity == 0)
        );
}
```

Concurrency does not guarantee evaluation order, so shared mutable state turns a read-modify-write into a race: a formatter that numbers items through an instance counter loses increments under parallel `Map`, because `++` is not atomic. Locks or atomic operations protect the counter, and a design without shared state removes the race by generating the indices as values and combining the independent sequences:

```csharp
internal static class Formatting {
    public static Seq<string> Numbered(Seq<string> items) =>
        items
            .Map(static item => char.ToUpperInvariant(item[0]) + string.Concat(item[1..].Select(char.ToLowerInvariant)))
            .Zip(toSeq(Range(1, items.Count)), static (item, index) => string.Create(CultureInfo.InvariantCulture, $"{index}. {item}"));
}
```

`Range(1, items.Count)` generates the indices, `Zip` pairs each item with one, and state is input data rather than shared mutable state, so parallel evaluation preserves behavior. Asynchronous code begins another task before an outstanding operation completes, parallel code runs work across cores, and multithreading schedules threads the hardware cannot all run at once, and each makes a hidden dependency on mutable state harder to control. `Map` accepts an impure delegate, so its function stays pure, parallel execution is requested explicitly because the compiler cannot infer purity, and its overhead is justified only by sufficient work and input size.

A unit test for a pure function supplies inputs and asserts the output, and an impure function has hidden inputs (the current time, database contents, the environment), hidden outputs (emails sent, files written, fields changed), or both, so it behaves like a larger pure transformation:

```text
(arguments, current program state, current world state)
    -> (return value, new program state, new world state)
```

Arrange must construct substitute external and program state, assert must inspect both the explicit result and the externally visible changes, mocks model the external state, and assertions over internal mutation are brittle and break encapsulation. Parameterized tests make inputs and expected outputs explicit across boundary cases. Distributed systems delegate computation to other processes and raise the share of I/O, and performance now comes from more cores, which pure computations use safely, so both trends raise the value of the small explicit boundary.

## [02]-[INJECTION]

Reading `DateTime.UtcNow` inside a validator makes the result depend on the system clock, so the code that constructs the validator reads the date once and injects the value, which makes the check deterministic and applies to configuration, environment settings, and request-scoped values, with the tradeoff that the object must not outlive the validity of the captured snapshot:

```csharp
internal sealed record Command(DateTime Date, string Code);

internal sealed class DateValidator(DateTime today) {
    public bool IsValid(Command command) => today <= command.Date.Date;
}
```

Where a consumer reads many capabilities, a runtime record carries them with one `Has<Eff<RT>, T>` trait per capability, the consumer is an `Eff<RT, A>` generic over `RT` that reads the capability through `RT.Ask` and passes the snapshot to the validator, and a test runtime carries a fixed value:

```csharp
internal sealed record Clock(DateTime Today);
internal sealed record Runtime(Clock Clock, ConsoleIO Console) : Has<Eff<Runtime>, Clock>, Has<Eff<Runtime>, ConsoleIO> {
    static K<Eff<Runtime>, Clock> Has<Eff<Runtime>, Clock>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Clock);
    static K<Eff<Runtime>, ConsoleIO> Has<Eff<Runtime>, ConsoleIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Console);
}

internal static class Capabilities {
    public static Eff<RT, bool> DateNotPast<RT>(Command command) where RT : Has<Eff<RT>, Clock> =>
        RT.Ask.Map(clock => new DateValidator(clock.Today).IsValid(command)).As();
}
```

A validator that needs a list of valid codes receives the codes as a `Seq<string>` that the caller loads as an effect, production composition supplies the `IO<Seq<string>>` that queries them, tests supply `IO.pure`, and the query runs only when the check runs because the `IO` defers it until the bind:

```csharp
internal sealed class CodeValidator(Seq<string> validCodes) {
    public bool IsValid(Command command) =>
        validCodes.Exists(code => string.Equals(code, command.Code, StringComparison.Ordinal));
}

internal static class Checks {
    public static IO<bool> CodeExists(IO<Seq<string>> loadCodes, Command command) =>
        loadCodes.Map(codes => new CodeValidator(codes).IsValid(command));
}
```

The validator stays pure and the effect is explicit and replaceable, the host runs `CodeExists` with `RunSafe()` and matches the `Fin<bool>`, and because a function signature is a narrow interface, injecting an effect value replaces a one-method interface, its implementation, constructor wiring, dependency-injection registration, and test fake. Interfaces remain appropriate as a common contract for distinct implementations, and one-method interfaces for every effect add infrastructure without benefit.

## [03]-[DEFERRAL]

C# evaluates every argument before the call, so a function that needs only one of its arguments pays for both, and a thunk (`Func<A>` or `IO<A>`) recovers call-by-name for that argument: an unused argument never runs, even one that throws, and an argument used twice runs twice. Memoizing a thunk gives call-by-need, where `memo(Func<A>)` runs the function once and keeps the result:

```csharp
internal static class Selection {
    public static IO<A> Pick<A>(bool takeLeft, IO<A> left, IO<A> right) => takeLeft ? left : right;
}
```

Wrapping the expression in `IO<A>` changes an eager value into an effect that produces a value when run, and the receiving function decides which effect to return. Three shapes differ in how work starts and how the consumer receives the result: `Option<T>` can contain a `T`, `Func<T>` does no work until the consumer invokes it, and `Task<T>` starts its work when the operation is called and produces a `T` later or faults, so the consumer controls neither. `IO<A>` holds the `Func<Task<A>>` that `IO.liftAsync` receives and calls it again on each run, which is why fallback and retry operate on an `IO<A>` and not on a started task, and an operation that returns a non-generic `Task` adapts to `IO<Unit>` to stay composable. `Map` transforms the deferred result without running the source, `Bind` sequences an effect with a next step that returns an effect and flattens the `IO<IO<B>>` a dependent step introduces, and the LINQ query is one bind per dependent `from`:

```csharp
internal sealed record Quote(string Provider, decimal Price);

internal static class Quotes {
    public static IO<Quote> Known(Quote quote) => IO.pure(quote);
    public static IO<Quote> Fetch(Func<Task<Quote>> request) => IO.liftAsync(request);
    public static IO<Quote> FetchWithToken(Func<CancellationToken, Task<Quote>> request) => IO.liftAsync(env => request(env.Token));
    public static IO<decimal> Price(IO<Quote> quote) => quote.Map(static q => q.Price);
    public static IO<int> Seats(IO<Quote> quote, Func<Quote, IO<int>> availability) => quote.Bind(availability);
    public static IO<decimal> Total(IO<Quote> outbound, IO<Quote> inbound) =>
        from o in outbound
        from i in inbound
        select o.Price + i.Price;
}
```

If the source fails, the transformation is skipped and the returned effect carries the error. `await` extracts a task's value and an `async` method wraps its return in a task, and the query pattern gives an effect the same composition for any monad through `Monad<M>`, so the workflow remains inside `IO` and the host runs it once at the boundary, because extracting a value earlier waits for the task to complete.

Repeated `try/catch` blocks obscure a computation, so exception-prone lazy work is a `Try<A>` that wraps a `Func<Fin<A>>`: `Try.lift(Func<A>)` captures a thrown exception as an `Error` with `IsExceptional` true, `Run()` returns `Fin<A>`, and `Try.lift(() => new Uri(value)).Run()` captures and runs one-off work in one expression. The operations compose in query syntax, the `Try<B>` that `Bind` returns stays deferred, and when run it runs the first stage, propagates its error unchanged, or runs the dependent stage with the successful value, where `Run()` also captures a property lookup that throws inside the `let` clause:

```csharp
internal static class Parsing {
    public static Try<System.Text.Json.JsonDocument> Parse(string json) => Try.lift(() => System.Text.Json.JsonDocument.Parse(json));
    public static Try<Uri> MakeUri(string value) => Try.lift(() => new Uri(value));
    public static Try<Uri> ExtractUri(string json) =>
        from document in Parse(json)
        let text = document.RootElement.GetProperty("Uri").ToString()
        from uri in MakeUri(text)
        select uri;
}
```

Ordinary functions compose when their shapes line up (`A -> B` then `B -> C`), and effectful functions (`A -> Try<B>` then `B -> Try<C>`) do not, so `Bind` defines the connection while it preserves the context's semantics: `Try` remains lazy, stops on failure, and carries the failure to the final result. The sequencing rule changes with the return type, a `(value, K)` pair combines the `K` values, a stateful shape threads a seed forward, and a continuation shape surrounds downstream work.

## [04]-[ENVIRONMENT]

`Reader<Env, A>` stores a function that cannot run until `Run(env)` supplies its environment, so a complete workflow is built before a database connection or other shared input exists and runs once that input arrives, and the environment type is the smallest structure the workflow reads. `Reader.ask<Env>()` reads the whole environment, `Reader.asks` reads a projection, `Reader.local` runs one `Reader` under a changed environment of the same type, and the instance `With<Env1>(Func<Env1, Env>)` adapts a `Reader` to a larger environment:

```csharp
internal sealed record Settings(string Target, int Timeout);
internal sealed record AppSettings(string Name, Settings Database);

internal static partial class Environments {
    public static Reader<Settings, string> Target => Reader.asks<Settings, string>(static settings => settings.Target);
    public static Reader<Settings, string> Described =>
        from settings in Reader.ask<Settings>()
        from target in Target
        select string.Create(CultureInfo.InvariantCulture, $"{target} within {settings.Timeout}s");
    public static Reader<Settings, string> Patient => Reader.local(static (Settings settings) => settings with { Timeout = settings.Timeout * 2 }, Described);
    public static Reader<AppSettings, string> FromApp => Described.With(static (AppSettings app) => app.Database);
}
```

Each bind wraps another deferred transformation, the environment type stays fixed while the value type changes, and the environment can hold a connection, an identifier, a configuration value, or another required input, which avoids carrying it through every step in tuples. An effectful workflow uses `ReaderT<Env, IO, A>`: `ReaderT.ask<IO, Env>()` reads the environment, an `IO<A>` binds directly in the query, `ReaderT.with` maps a larger environment, and `Run(env)` returns a `K<IO, A>` that `.As()` narrows to the `IO<A>` the host runs with `RunSafe()`:

```csharp
internal static partial class Environments {
    public static ReaderT<Settings, IO, int> Queried(Atom<int> queries) =>
        from settings in ReaderT.ask<IO, Settings>()
        from count in IO.lift(() => queries.Swap(static n => n + 1))
        select count * settings.Timeout;
    public static ReaderT<AppSettings, IO, int> QueriedFromApp(Atom<int> queries) => ReaderT.with(static (AppSettings app) => app.Database, Queried(queries));
}
```

## [05]-[SCOPES]

Resource and instrumentation helpers take a callback and act before and after it (`R Connect<R>(Func<Connection, R> use)`, `A Time<A>(string operation, Func<A> run)`, `R Transact<R>(Connection connection, Func<Transaction, R> use)`), and after their configuration is partially applied they share the continuation type `(T -> R) -> R`: the computation produces a `T`, supplies it to the continuation, and returns its result, creating a resource before and releasing, committing, or timing after. Combining such helpers directly nests callbacks, and `Bracket(Use:, Fin:)` on `IO<A>` expresses the same before-and-after around a continuation, so a timing helper transforms `IO<A>` into `IO<A>` and its `Fin` runs after the continuation succeeds and after a failure inside it, while a pre-built `IO.fail` received as the work stays deferred and skips `Fin`:

```csharp
internal static class Scopes {
    public static IO<A> Timed<A>(Atom<TimeSpan> elapsed, IO<A> work) =>
        IO.lift(System.Diagnostics.Stopwatch.StartNew).Bracket(
            Use: _ => work,
            Fin: watch => IO.lift(() => elapsed.Swap(_ => watch.Elapsed)));
}
```

Once helpers return `IO<A>`, query syntax exposes scoped behavior without callback nesting: a transaction scope depends on the connection and supplies the transaction downstream, the commit step runs only after every statement succeeds, and a failure skips it so `Dispose` rolls the open transaction back:

```csharp
internal static class Removals {
    public static IO<int> PurgeOld(Atom<TimeSpan> elapsed) =>
        Scopes.Timed(
            elapsed,
            from connection in use(static () => new Connection())
            select connection.Execute("DELETE Entries WHERE Timestamp < @upTo"));
    public static IO<int> RemoveOrder =>
        from connection in use(static () => new Connection())
        from transaction in use(connection.BeginTransaction)
        from affected in IO.lift(() =>
            connection.Execute("DELETE Lines WHERE OrderId = @Id", transaction)
            + connection.Execute("DELETE Orders WHERE OrderId = @Id", transaction))
        from _ in IO.lift(fun(transaction.Commit))
        select affected;
}
```

The order of the bracketed effects determines which downstream work each scope surrounds, and adding, removing, or reordering a cross-cutting behavior changes the corresponding query clauses, where `Bracket`, `use`, `Map`, and `Bind` are not database-specific:
- Timing outside connection acquisition measures acquisition and database work, and timing inside the connection scope measures only downstream work
- The transaction scope follows the connection scope because it depends on the connection
- Operations that must be atomic sit inside the transaction scope, before the commit step

## [06]-[EXECUTION]

Each retry waits for the next delay of the schedule and invokes the effect again, when the schedule expires the last error remains observable, and recovery happens on the `Fin<A>` that `RunSafe()` returns:

```csharp
internal sealed record ProviderDown() : Expected("provider down", 3001);

internal static class Policies {
    public static IO<Quote> Fallback(IO<Quote> primary, IO<Quote> secondary) => primary | secondary;
    public static IO<Quote> FallbackOnOutage(IO<Quote> primary, IO<Quote> secondary) => primary.Catch(3001, _ => secondary).As();
    public static IO<Quote> Retried(IO<Quote> attempt) => attempt.Retry(Schedule.exponential(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(3));
    public static Quote Recovered(IO<Quote> quote, Quote substitute) => quote.RunSafe().IfFail(substitute);
}
```

`Bind` is sequential because the next step needs the first effect's value before it can create the second effect, and independent operations use the tuple `Apply` over already-created effects, which both start before either is awaited so completion time follows the slower call rather than their sum, or `Fork` and `Await` for explicit control, and `awaitAll` for a `Seq<IO<A>>`:

```csharp
internal static class Independence {
    public static IO<Quote> Cheapest(IO<Quote> first, IO<Quote> second) => (first, second).Apply(PickCheaper).As();
    public static IO<Quote> CheapestForked(IO<Quote> first, IO<Quote> second) =>
        from f1 in first.Fork()
        from f2 in second.Fork()
        from a in f1.Await
        from b in f2.Await
        select PickCheaper(a, b);
    public static IO<Seq<Quote>> All(Seq<IO<Quote>> requests) => awaitAll(requests);

    private static Quote PickCheaper(Quote a, Quote b) => a.Price <= b.Price ? a : b;
}
```

`Traverse` applies an effect-returning function and flips the traversable and the effect (`Tr<T> -> (T -> A<R>) -> A<Tr<R>>` where `Map` gives `Tr<A<R>>`), and one signature takes the evaluation policy of its effect, so under `Option` one failed parse makes the whole input `None` instead of silently dropping it:

```csharp
internal sealed record Provider(string Name, IO<Seq<Quote>> Quotes);

internal static class Traversals {
    public static Option<Seq<double>> ParseAll(Seq<string> values) => values.Traverse(static s => parseDouble(s)).As();
    public static Validation<Error, Seq<int>> ValidateAll(Seq<string> values, Func<string, Validation<Error, int>> validate) => values.Traverse(validate).As();
    public static Validation<Error, Seq<int>> ValidateUntilFirstFailure(Seq<string> values, Func<string, Validation<Error, int>> validate) => values.TraverseM(validate).As();
    public static IO<Seq<Quote>> SearchParallel(Seq<Provider> providers) => providers.Traverse(static p => p.Quotes).As().Map(static groups => groups.Flatten());
    public static IO<Seq<Quote>> SearchSerial(Seq<Provider> providers) => providers.TraverseM(static p => p.Quotes).As().Map(static groups => groups.Flatten());
    public static IO<Seq<Quote>> SearchBestEffort(Seq<Provider> providers) =>
        providers.Map<K<IO, Seq<Quote>>>(static p => p.Quotes).PartitionFallible().As().Map(static parts => parts.Succs.Flatten());
}
```

`Option`, `Either`, and `Validation` are traversables with one value on success and none on failure, so the failure branch preserves its existing error without calling the function and the success branch applies it and maps the success constructor back over the result, and traversing with the identity function swaps nested structures:

```csharp
internal static class Layers {
    public static Option<Validation<Error, int>> Parse(Validation<Error, string> validated) => validated.Traverse(static s => parseInt(s)).As();
    public static Option<Validation<Error, int>> Swap(Validation<Error, Option<int>> stacked) => stacked.Traverse(static option => option).As();
}
```

Nested effects cannot compose directly because each `Bind` understands only its outer effect, so a lookup that can return no value stays inside `OptionT<IO, A>` while it consumes an `IO<A>` through `OptionT.liftIO`, an operation that returns a non-generic `Task` adapts through an `async` lambda that returns `unit`, and an `Eff<RT, A>` exits asynchronously through `RunAsync(rt)`, which returns `Task<Fin<A>>`:

```csharp
internal static class Stacks {
    public static OptionT<IO, decimal> Converted(Func<Guid, OptionT<IO, State>> lookup, Guid id, IO<decimal> rate) =>
        from state in lookup(id)
        from factor in OptionT.liftIO<IO, decimal>(rate)
        select state.Balance * factor;
    public static IO<Unit> Publish(Func<Task> publish) =>
        IO.liftAsync(async () => {
            await publish().ConfigureAwait(false);
            return unit;
        });
    public static Task<Fin<State>> ExitAsync(Eff<Runtime, State> workflow, Runtime runtime) => workflow.RunAsync(runtime);
}
```

Reduce unnecessary effects before building the workflow, and adapt every operation to `IO` at its boundary: a `Validation` from the validation boundary enters through `ToFin` and `IO.lift`, a `Fin` from a pure transition enters through `IO.lift`, and an `OptionT<IO, A>` read leaves the stack through `Run()` and `ToFin` with a typed `Expected`, as the repository flow in `references/functions.md` shows.
