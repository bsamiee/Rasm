# [LAZY_TRY_CONTINUATIONS]

## [01]-[LAZINESS]

C# uses call-by-value evaluation: every argument is evaluated before the call, including an argument the body never uses. Call-by-name evaluation substitutes the unevaluated expression instead: an unused argument never runs, even one that throws, and an argument used twice runs twice. Thunks, `Func<A>` or `IO<A>`, recover call-by-name for one argument. The cost of call by value appears when a function needs only one of its arguments:

```csharp
internal static partial class Laziness {
    public static A Pick<A>(bool takeLeft, A left, A right) => takeLeft ? left : right;
}
```

Both arguments are evaluated before `Pick` chooses one. Accept computations instead when an expensive branch can go unused:

```csharp
internal static partial class Laziness {
    public static IO<A> Pick<A>(bool takeLeft, IO<A> left, IO<A> right) => takeLeft ? left : right;
}
```

Wrapping an expression in `IO<A>` changes an eager value into an effect that produces a value when run. `IO.lift(Func<A>)` wraps the work as `IO<A>`. The receiving function decides which effect to return.

Memoize a reused pure value instead. Memoizing a thunk gives call-by-need evaluation: `memo(Func<A>)` returns a `Memo<A>` whose `Value` runs the function once and keeps the result:

```csharp
internal static partial class Laziness {
    public static int Twice(Func<int> compute) {
        Memo<int> total = memo(compute);
        return total.Value + total.Value;
    }
}
```

### [01.1]-[LAZY_FALLBACKS]

An eager fallback defeats a cache because the database lookup runs even when the cache contains the value:

```csharp
internal static partial class Laziness {
    public static Option<string> Eager(Cache cache, Database database, int id) => cache.Find(id) | database.Find(id);
}
```

The fallback must stay unevaluated until it is needed. `Option<A>` defines `operator true`, `||` evaluates the right operand only when the left operand is `None`:

```csharp
internal static partial class Laziness {
    public static Option<string> Deferred(Cache cache, Database database, int id) => cache.Find(id) || database.Find(id);
}
```

`IfNone(A)` takes a value, `IfNone(Func<A>)` takes a computation, and both return `A`, not `Option<A>`:

```csharp
internal static partial class Laziness {
    public static string Named(Cache cache, int id) => cache.Find(id).IfNone("unknown");
    public static string Loaded(Cache cache, Database database, int id) => cache.Find(id).IfNone(() => database.Load(id));
}
```

`Option<A>` offers both overloads:
- Use a direct value when its construction is negligible
- `Func<A>` avoids work when the value is expensive and can go unused

## [02]-[COMPOSE_THEN_EXECUTE]

`Map` transforms an `IO<A>` result without running the source effect:

```csharp
internal static partial class Composition {
    public static IO<int> Count(Atom<int> reads) => IO.lift(() => reads.Swap(static n => n + 1));
    public static IO<int> Doubled(Atom<int> reads) => Count(reads).Map(static n => n * 2);
}
```

`Bind` sequences an effect whose next step also returns an effect:

```csharp
internal static partial class Composition {
    public static IO<int> Summed(Atom<int> reads) => Count(reads).Bind(first => Count(reads).Map(second => first + second));
}
```

`RunSafe()` remains the explicit execution boundary and returns `Fin<A>` to the host. `Map` applies a function to the deferred result. `Bind` flattens the nested `IO<IO<B>>` introduced by a dependent next step.

## [03]-[TRY]

Repeated `try/catch` blocks obscure the computation. Represent exception-prone lazy work with `Try<A>` and centralize execution. `Try<A>` wraps a `Func<Fin<A>>`: `Try.lift(Func<A>)` captures a thrown exception as an `Error` whose `IsExceptional` is true, and `Run()` returns `Fin<A>`.

```csharp
internal static partial class Parsing {
    public static Try<System.Text.Json.JsonDocument> Parse(string json) => Try.lift(() => System.Text.Json.JsonDocument.Parse(json));
    public static Try<Uri> CreateUri(string value) => Try.lift(() => new Uri(value));
}
```

For one-off work, `Try.lift(() => new Uri(value)).Run()` captures and runs in one expression.

The operations compose with query syntax:

```csharp
internal static partial class Parsing {
    public static Try<Uri> ExtractUri(string json) =>
        from document in Parse(json)
        let uriString = document.RootElement.GetProperty("Uri").ToString()
        from uri in CreateUri(uriString)
        select uri;
}
```

The `Try<B>` returned by `Bind` remains deferred. When run, it runs the first stage, propagates its error unchanged, or runs the dependent stage with the successful value. `Run()` captures a property lookup that throws inside the `let` clause.

For query syntax, a monadic type supplies:
- `Select` as an alias for `Map`
- `SelectMany` as an alias for `Bind`
- Projection overload of `SelectMany` for multiple `from` clauses

## [04]-[MONADIC_COMPOSITION]

Ordinary functions compose when their shapes line up:

```text
A -> B
B -> C
```

Effectful functions do not line up directly:

```text
A -> Try<B>
B -> Try<C>
```

`Bind` defines how to connect them while preserving `Try` semantics: remain lazy, stop on failure, and carry the failure to the final result. Monadic composition connects functions that return a computational context; each context's `Bind` captures its sequencing rules.

The sequencing rule changes with the return type: a `(value, K)` pair combines the `K` values, a stateful shape threads a seed forward, and a continuation shape surrounds downstream work.

## [05]-[READER]

`Reader<Env, A>` stores a function that cannot run until `Run(env)` supplies its environment:

```text
Reader<E, A> = E -> A
Run: Reader<E, A> + E -> A
```

`Reader.ask<Env>()` reads the whole environment, `Reader.asks` reads a projection, and `Reader.local` runs one Reader under a changed environment of the same type. The instance `With<Env1>(Func<Env1, Env>)` adapts a Reader to a larger environment:

```csharp
internal sealed record Settings(string Target, int Timeout);
internal sealed record AppSettings(string Name, Settings Database);
```

```csharp
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

Each bind wraps another deferred transformation. The environment type remains fixed while the value type can change. Build a complete workflow before a database connection or other shared input exists, then run it once that input arrives.

An effectful workflow uses `ReaderT<Env, IO, A>`. `ReaderT.ask<IO, Env>()` reads the environment, an `IO<A>` binds directly in the query, `ReaderT.with` maps a larger environment, and `Run(env)` returns `K<IO, A>` that `.As()` narrows to the `IO<A>` the host runs with `RunSafe()`:

```csharp
internal static partial class Environments {
    public static ReaderT<Settings, IO, int> Queried(Atom<int> queries) =>
        from settings in ReaderT.ask<IO, Settings>()
        from count in IO.lift(() => queries.Swap(static n => n + 1))
        select count * settings.Timeout;
    public static ReaderT<AppSettings, IO, int> QueriedFromApp(Atom<int> queries) => ReaderT.with(static (AppSettings app) => app.Database, Queried(queries));
}
```

This avoids carrying the environment through every step in tuples. `Reader` passes dependencies as an explicit environment, which can contain a connection, identifier, configuration value, or another required input.

The environment type is the smallest structure the workflow reads.

## [06]-[CONTINUATIONS]

Resource and instrumentation helpers take a callback to act before and after it:

```text
R Connect<R>(Func<Connection, R> use)
A Time<A>(string operation, Func<A> run)
R Transact<R>(Connection connection, Func<Transaction, R> use)
```

After partially applying configuration, such as the operation name, they share the continuation type:

```text
(T -> R) -> R
```

The computation produces a `T`, supplies it to a continuation, and returns the continuation's result. It can create a resource before the continuation and release, commit, or time after the continuation returns.

Combining these helpers directly creates nested callbacks. `IO<A>` provides `Bracket(Use:, Fin:)` for this pattern: it acts before and after a continuation. `Connection : IDisposable` opens a `Transaction` whose `Dispose` rolls back unless `Commit` has run:

```csharp
internal sealed class Transaction(Connection connection) : IDisposable {
    public static int Committed { get; private set; }
    public static int RolledBack { get; private set; }

    public bool Open { get; private set; } = !connection.Disposed;

    public void Commit() {
        Open = false;
        Committed++;
    }
    public void Dispose() {
        if (Open) RolledBack++;
        Open = false;
    }
}
```

```csharp
internal static partial class Scopes {
    public static IO<A> Time<A>(Atom<TimeSpan> elapsed, IO<A> work) =>
        IO.lift(System.Diagnostics.Stopwatch.StartNew).Bracket(
            Use: _ => work,
            Fin: watch => IO.lift(() => elapsed.Swap(_ => watch.Elapsed)));
}
```

`Time` transforms `IO<A>` into `IO<A>`. `Fin` runs after the continuation succeeds and after a failure occurs while it runs. `Fin` does not run when `Time` receives a pre-built `IO.fail`; the supplied work remains deferred.

## [07]-[RESOURCE_SCOPES]

Once helpers return `IO<A>`, query syntax exposes scoped behavior without callback nesting:

```csharp
internal static partial class Scopes {
    public static IO<int> DeleteOldLogs(Atom<TimeSpan> elapsed) =>
        Time(
            elapsed,
            from connection in use(static () => new Connection())
            select connection.Execute("DELETE Logs WHERE Timestamp < @upTo"));
    public static IO<int> DeleteOrder =>
        from connection in use(static () => new Connection())
        from transaction in use(connection.BeginTransaction)
        from affected in IO.lift(() =>
            connection.Execute("DELETE OrderLines WHERE OrderId = @Id", transaction)
            + connection.Execute("DELETE Orders WHERE OrderId = @Id", transaction))
        from _ in IO.lift(fun(transaction.Commit))
        select affected;
}
```

`use(Func<A>)` accepts an `IDisposable` and disposes it when the effect succeeds or fails. The transaction scope depends on the connection and supplies a transaction downstream. The commit step runs only after both statements succeed. Failures skip it, and `Dispose` rolls the open transaction back. `use(Func<A>, Action<A>)` runs its release action on every exit, a commit does not belong in a release action.

## [08]-[SCOPE_ORDERING]

The order of bracketed effects determines which downstream work each scope surrounds:
- Timing outside connection acquisition measures acquisition and database work
- Timing inside the connection scope measures only downstream work
- The transaction scope must follow the connection scope because it depends on the connection
- Operations that must be atomic belong inside the transaction scope, before the commit step

Adding, removing, or reordering cross-cutting behavior changes the corresponding query clauses. `Bracket`, `use`, `Map`, and `Bind` are not database-specific.
