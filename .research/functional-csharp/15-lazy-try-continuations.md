# Lazy Computations, Failure-Aware Execution, and Continuations

## 1. Laziness is an execution decision

C# evaluates arguments eagerly. If a function may not need an argument, accepting the value directly can perform unnecessary work:

```csharp
internal static partial class Laziness {
    public static A Pick<A>(bool takeLeft, A left, A right) => takeLeft ? left : right;
}
```

Both arguments are evaluated before `Pick` chooses one. Accept computations instead when an expensive branch might not be used:

```csharp
internal static partial class Laziness {
    public static IO<A> Pick<A>(bool takeLeft, IO<A> left, IO<A> right) => takeLeft ? left : right;
}
```

Wrapping an expression in `IO<A>` changes it from a value available now into an effect that produces a value when run. `IO.lift(Func<A>)` wraps the work as `IO<A>`. The receiving function owns the decision to return it.

A pure value that is reused is memoized instead. `memo(Func<A>)` returns a `Memo<A>` whose `Value` runs the function once and keeps the result:

```csharp
internal static partial class Laziness {
    public static int Twice(Func<int> compute) {
        Memo<int> total = memo(compute);
        return total.Value + total.Value;
    }
}
```

### Lazy fallback APIs

Fallbacks are a common place where this distinction matters. An eager fallback defeats a cache because the database lookup runs even when the cache contains the value:

```csharp
internal static partial class Laziness {
    public static Option<string> Eager(Cache cache, Database database, int id) => cache.Find(id) | database.Find(id);
}
```

The fallback must stay unevaluated until it is needed. `Option<A>` defines `operator true`, so `||` evaluates the right operand only when the left operand is `None`:

```csharp
internal static partial class Laziness {
    public static Option<string> Deferred(Cache cache, Database database, int id) => cache.Find(id) || database.Find(id);
}
```

The same design applies when extracting an `Option<A>` with a default. `IfNone(A)` takes a value, `IfNone(Func<A>)` takes a computation, and both are host operations that leave the `Option`:

```csharp
internal static partial class Laziness {
    public static string Named(Cache cache, int id) => cache.Find(id).IfNone("unknown");
    public static string Loaded(Cache cache, Database database, int id) => cache.Find(id).IfNone(() => database.Load(id));
}
```

`Option<A>` offers both overloads:
- A direct value is clearer when its construction is negligible.
- A `Func<A>` avoids work when the value is expensive and may not be needed.
- If a function may ignore an argument, that argument is a candidate for lazy input.

## 2. Compose first, execute later

An `IO<A>` contains the potential to produce an `A`. `Map` transforms that future result without running the source effect:

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

Neither operator performs the work while the pipeline is being assembled. `RunSafe()` remains the explicit execution boundary and returns `Fin<A>` to the host. `Map` is ordinary function composition under a deferred wrapper. `Bind` additionally flattens the nested `IO<IO<B>>` introduced by a dependent next step.

## 3. `Try<T>` describes deferred work that may throw

Repeated `try/catch` blocks obscure the computation being performed. Give exception-prone lazy work a semantic type and centralize execution. `Try<A>` wraps a `Func<Fin<A>>`: `Try.lift(Func<A>)` captures a thrown exception as an `Error` whose `IsExceptional` is true, and `Run()` returns `Fin<A>`.

An operation can now describe unsafe work without executing it:

```csharp
internal static partial class Parsing {
    public static Try<System.Text.Json.JsonDocument> Parse(string json) => Try.lift(() => System.Text.Json.JsonDocument.Parse(json));
    public static Try<Uri> CreateUri(string value) => Try.lift(() => new Uri(value));
}
```

For one-off work, `Try.lift(() => new Uri(value)).Run()` captures and runs in one expression.

Composing the operations still performs no work:

```csharp
internal static partial class Parsing {
    public static Try<Uri> ExtractUri(string json) =>
        from document in Parse(json)
        let uriString = document.RootElement.GetProperty("Uri").ToString()
        from uri in CreateUri(uriString)
        select uri;
}
```

`Run()` is the boundary that executes a pipeline and returns `Fin<Uri>`, with a thrown exception converted into an `Error`. The `Try<B>` returned by `Bind` remains deferred. When run, it runs the first stage, propagates its error unchanged, or runs the dependent stage with the successful value. A property lookup that throws inside the `let` clause is captured by `Run()` as well.

For query syntax, a monadic type supplies:
- `Select` as an alias for `Map`.
- `SelectMany` as an alias for `Bind`.
- A projection overload of `SelectMany` for multiple `from` clauses.

## 4. Monadic composition preserves the computation's rules

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

`Bind` defines how to connect them while preserving `Try` semantics: remain lazy, stop on failure, and carry the failure to the final result. More generally, monadic composition connects functions that return a computational context; each context's `Bind` captures its sequencing rules.

The rule changes with the computation's shape. For functions returning `(value, K)`, sequencing can feed the value forward and combine both `K` values when two `K` values can be combined into one. A list works because the two lists can be concatenated. Other useful shapes thread a seed or state through successive computations, or accept a continuation that surrounds downstream work.

## 5. Reader defers a shared environment

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

Each bind wraps another deferred transformation. The environment type remains fixed while the value type can change. A complete workflow can therefore be constructed before a database connection or other shared input exists, then run once that input is supplied.

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

This avoids carrying the environment through every step in tuples and moves acquisition of an external dependency outside workflow construction. Reader is function-level dependency injection: the environment can be a connection, identifier, configuration value, or any other input needed when the deferred computation runs. A returned Reader accepts further transformations before `Run`.

The environment type is the smallest structure the workflow reads.

## 6. Continuations model setup and teardown scopes

Resource and instrumentation helpers often take a callback so they can act before and after it:

```text
R Connect<R>(Func<Connection, R> use)
A Time<A>(string operation, Func<A> run)
R Transact<R>(Connection connection, Func<Transaction, R> use)
```

After partially applying configuration such as the operation name, they share the continuation shape:

```text
(T -> R) -> R
```

The computation produces a `T`, supplies it to a continuation, and returns the continuation's result. It can therefore create a resource before the continuation and release, commit, or time after the continuation returns.

Naively combining several helpers nests callbacks into a pyramid. `IO<A>` already has the continuation shape: `Bracket(Use:, Fin:)` acts before and after a continuation, and `use` registers a release that runs when the effect exits. `Connection : IDisposable` opens a `Transaction` whose `Dispose` rolls back unless `Commit` has run:

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

`Time` is a function from `IO<A>` to `IO<A>`: the continuation is the effect it receives, and the bracket acts before and after that effect. `Fin` runs after a successful continuation and after a failure raised while the continuation runs. A pre-built `IO.fail` passed as the continuation drops `Fin`, so the work handed to `Time` stays a deferred effect.

## 7. Flat resource pipelines

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

`use(Func<A>)` accepts an `IDisposable` and disposes it when the effect exits, on success and on failure. The transaction scope depends on the connection and supplies a transaction downstream. The commit step runs only after both statements succeed. A failure skips it, and `Dispose` rolls the open transaction back. `use(Func<A>, Action<A>)` runs its release action on every exit, so a commit does not belong in a release action.

## 8. Ordering determines scope

Bracketed effects are U-shaped: each scope can act before its continuation and again after downstream work returns. Reordering clauses therefore changes behavior, not just presentation:
- Timing outside connection acquisition measures acquisition and database work.
- Timing inside the connection scope measures only downstream work.
- The transaction scope must follow the connection scope because it depends on the connection.
- Operations that must be atomic belong inside the transaction scope, before the commit step.

The flat query makes these scopes visible and turns adding, removing, or reordering cross-cutting behavior into a local change. `Bracket`, `use`, `Map`, and `Bind` are not database-specific.
