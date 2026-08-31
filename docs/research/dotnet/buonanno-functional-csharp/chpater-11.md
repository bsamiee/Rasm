# Lazy Computations, Failure-Aware Execution, and Continuations

## 1. Laziness is an execution decision

C# evaluates arguments eagerly. If a function may not need an argument, accepting the value directly can perform unnecessary work:

```csharp
var random = new Random();

T Pick<T>(T left, T right) =>
    random.NextDouble() < 0.5 ? left : right;
```

Both arguments are evaluated before `Pick` chooses one. Accept computations instead when an expensive branch might not be used:

```csharp
T Pick<T>(Func<T> left, Func<T> right) =>
    (random.NextDouble() < 0.5 ? left : right)();
```

Wrapping an expression in `Func<T>` changes it from a value available now into a computation that can produce a value later. The receiving function owns the decision to invoke it.

### Lazy fallback APIs

Fallbacks are a common place where this distinction matters. An eager fallback defeats a cache because the database lookup runs even when the cache contains the value:

```csharp
cache.Lookup(id).OrElse(db.Lookup(id));
```

The fallback should be represented as a computation:

```csharp
public static Option<T> OrElse<T>(
    this Option<T> option,
    Func<Option<T>> fallback) =>
    option.Match(
        () => fallback(),
        _ => option);

cache.Lookup(id).OrElse(() => db.Lookup(id));
```

The same design applies when extracting an `Option<T>` with a default:

```csharp
public static T GetOrElse<T>(this Option<T> option, T defaultValue) =>
    option.Match(() => defaultValue, value => value);

public static T GetOrElse<T>(this Option<T> option, Func<T> fallback) =>
    option.Match(fallback, value => value);
```

Offer both overloads when useful:
- A direct value is clearer when its construction is negligible.
- A `Func<T>` avoids work when the value is expensive and may not be needed.
- If a function may ignore an argument, that argument is a candidate for lazy input.

## 2. Compose first, execute later

A `Func<T>` contains the potential to produce a `T`. `Map` transforms that future result without invoking the source computation:

```csharp
public static Func<R> Map<T, R>(
    this Func<T> source,
    Func<T, R> transform) =>
    () => transform(source());
```

`Bind` sequences a computation whose next step also returns a lazy computation:

```csharp
public static Func<R> Bind<T, R>(
    this Func<T> source,
    Func<T, Func<R>> next) =>
    () => next(source())();
```

Neither operator performs the work while the pipeline is being assembled. Invocation remains the explicit execution boundary. `Map` is ordinary function composition under a deferred wrapper; `Bind` additionally flattens the nested `Func<Func<R>>` introduced by a dependent next step.

## 3. `Try<T>` describes deferred work that may throw

Repeated `try/catch` blocks obscure the computation being performed. Give exception-prone lazy work a semantic type and centralize execution:

```csharp
public delegate Exceptional<T> Try<T>();

public static Exceptional<T> Run<T>(this Try<T> attempt)
{
    try { return attempt(); }
    catch (Exception exception) { return exception; }
}

public static Try<R> Bind<T, R>(
    this Try<T> attempt,
    Func<T, Try<R>> next) =>
    () => attempt.Run().Match<Exceptional<R>>(
        Exception: error => error,
        Success: value => next(value).Run());
```

An operation can now describe unsafe work without executing it:

```csharp
Try<JObject> Parse(string json) =>
    () => JObject.Parse(json);

Try<Uri> CreateUri(string value) =>
    () => new Uri(value);
```

For one-off work, the `Try` helper converts a `Func<T>` into a `Try<T>`, as in `Try(() => new Uri(value)).Run()`.

Composing the operations still performs no work:

```csharp
Try<Uri> ExtractUri(string json) =>
    from document in Parse(json)
    let uriString = (string)document["Uri"]
    from uri in CreateUri(uriString)
    select uri;

Exceptional<Uri> result = ExtractUri(input).Run();
```

`Run` is the boundary that executes a pipeline and converts a thrown exception into `Exceptional<T>`. The `Try<R>` returned by `Bind` remains deferred; when run, it safely runs the first stage, propagates its exception unchanged, or safely runs the dependent stage with the successful value.

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

## 5. Continuations model setup and teardown scopes

Resource and instrumentation helpers often take a callback so they can act before and after it:

```csharp
R Connect<R>(ConnectionString connectionString, Func<SqlConnection, R> use);
T Trace<T>(ILogger logger, string operation, Func<T> run);
T Time<T>(ILogger logger, string operation, Func<T> run);
R Transact<R>(SqlConnection connection, Func<SqlTransaction, R> use);
```

After partially applying configuration such as a connection string, logger, or operation name, they share the continuation shape:

```text
(T -> R) -> R
```

The computation produces a `T`, supplies it to a continuation, and returns the continuation's result. It can therefore create a resource before the continuation and release, commit, time, or trace after the continuation returns.

Naively combining several helpers nests callbacks into a pyramid. Capture the shared shape as middleware and make it composable:

```csharp
public delegate dynamic Middleware<T>(Func<T, dynamic> continuation);

public static Middleware<R> Bind<T, R>(
    this Middleware<T> middleware,
    Func<T, Middleware<R>> next) =>
    continuation =>
        middleware(value => next(value)(continuation));

public static Middleware<R> Map<T, R>(
    this Middleware<T> middleware,
    Func<T, R> transform) =>
    continuation =>
        middleware(value => continuation(transform(value)));

public static T Run<T>(this Middleware<T> middleware) =>
    (T)middleware(value => value);
```

C# cannot leave the continuation result type unresolved while fixing `T`, so the delegate uses `dynamic` internally. Client code regains a typed boundary by mapping the final operation and calling `Run`, which supplies the identity continuation and casts to the known result type.

`Middleware<T>` is a monad over the continuation's input `T`: although it does not visibly store a `T`, it must be able to produce one in order to call a continuation of type `T -> R`.

## 6. Flat resource pipelines

Once helpers are adapted to `Middleware<T>`, query syntax exposes scoped behavior without callback nesting:

```csharp
public static Func<T> ToNullary<T>(this Func<Unit, T> function) =>
    () => function(Unit());

Middleware<SqlConnection> connect =
    continuation => Connect(connectionString, continuation);

Func<string, Middleware<Unit>> time =
    operation => continuation =>
        Time(logger, operation, continuation.ToNullary());

int affected = (
    from _ in time("DeleteOldLogs")
    from connection in connect
    select connection.Execute(
        "DELETE [Logs] WHERE [Timestamp] < @upTo",
        new { upTo = 7.Days().Ago() })
).Run();

Middleware<SqlTransaction> transact(SqlConnection connection) =>
    continuation => Transact(connection, continuation);

var parameters = new { Id = id };
int deleted = (
    from connection in connect
    from transaction in transact(connection)
    select connection.Execute("DELETE OrderLines WHERE OrderId = @Id", parameters, transaction)
         + connection.Execute("DELETE Orders WHERE OrderId = @Id", parameters, transaction)
).Run();
```

`Unit` adapts a parameterless timing operation to the continuation shape. Transaction middleware instead depends on the connection and supplies a transaction downstream. Normal completion commits; if the continuation throws, commit is skipped and disposal rolls the transaction back.

## 7. Ordering determines scope

Middleware pipelines are U-shaped: each block can act before calling its continuation and again after downstream work returns. Reordering clauses therefore changes behavior, not just presentation:
- Timing outside connection acquisition measures acquisition and database work.
- Timing inside the connection scope measures only downstream work.
- Transaction middleware must follow connection middleware because it depends on the connection.
- Operations that must be atomic belong inside the transaction continuation.

The flat query makes these scopes visible and turns adding, removing, or reordering cross-cutting behavior into a local change. The named `Middleware<T>` delegate is optional: the same continuation computation can be written as `Func<Func<T, dynamic>, dynamic>`, and its `Map`, `Bind`, and `Run` are not database-specific.
