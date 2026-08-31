# [READER_TRANSFORMER]

`ReaderT<Env, M, A>` adds an environment `Env` to a monad `M`. A computation reads configuration or request context without global state and without the same parameter on every function. The environment stays explicit: one value is supplied when the computation runs and is threaded through its stages. Until then, `ReaderT` composes functions that wait for that value.

## [01]-[REPRESENTATION_AND_EXECUTION]

```csharp
public record ReaderT<Env, M, A>(Func<Env, K<M, A>> runReader) : K<ReaderT<Env, M>, A>
    where M : Monad<M>
{
    public K<M, A> Run(Env env) =>
        runReader(env);
}
```

Given an `Env`, the wrapped function returns `K<M, A>`. Compare `OptionT`, whose shape is `K<M, Option<A>>`: the lifted monad wraps an optional result, while an environment-carrying computation wraps the lifted monad, so the inner computation runs with the environment.

Configuration threads through file-reading code without becoming global:

```csharp
public record MyConfig(string SourceFolder);

public static ReaderT<MyConfig, IO, string> readSourceText(string file) =>
    from config in ReaderT.ask<IO, MyConfig>()
    let path = Path.Combine(config.SourceFolder, file)
    from text in IO.liftAsync(env =>
        File.ReadAllTextAsync(path, env.Token))
    select text;
```

The configuration is supplied once when the transformer runs:

```csharp
var config = new MyConfig(@"c:\folder");
var textIO = readSourceText("test.cs").Run(config);
var text = textIO.Run();
```

The first `Run` supplies the environment and returns the lifted `IO<string>`; the second runs that effect. Run the `IO` at the edge of the application.

## [02]-[ASK_AND_BIND]

`ask` retrieves no global state. It builds a function that lifts its eventual input into `M`:

```csharp
public static ReaderT<Env, M, Env> ask<Env, M>()
    where M : Monad<M> =>
    new(M.Pure);
```

Everything before `Run` stays lazy, so the operations are expressed through function composition. `Bind` runs both dependent stages with the same environment and lets `M.Bind` sequence their inner computations:

```csharp
public ReaderT<Env, M, B> Bind<B>(
    Func<A, ReaderT<Env, M, B>> f) =>
    new(env => M.Bind(
        runReader(env),
        value => f(value).runReader(env)));
```

The environment reaches each `ReaderT` stage because it is passed whenever the wrapped function is invoked.

## [03]-[STACK_ENCAPSULATION]

A domain type hides the stack. A simplified `Eff<RT, A>` is a wrapper around `ReaderT<RT, IO, A>`:

```csharp
public record Eff<RT, A>(ReaderT<RT, IO, A> runEff) : K<Eff<RT>, A>;

public class Eff<RT> : Monad<Eff<RT>>
{
    public static K<Eff<RT>, B> Bind<A, B>(
        K<Eff<RT>, A> ma,
        Func<A, K<Eff<RT>, B>> f) =>
        new Eff<RT, B>(ma.As().runEff.Bind(x => f(x).As().runEff));
    public static K<Eff<RT>, B> Map<A, B>(
        Func<A, B> f,
        K<Eff<RT>, A> ma) =>
        new Eff<RT, B>(ma.As().runEff.Map(f));
    public static K<Eff<RT>, A> Pure<A>(A value) =>
        new Eff<RT, A>(ReaderT.Pure<RT, IO, A>(value));
    public static K<Eff<RT>, B> Apply<A, B>(
        K<Eff<RT>, Func<A, B>> mf,
        K<Eff<RT>, A> ma) =>
        new Eff<RT, B>(mf.As().runEff.Apply(ma.As().runEff).As());
    public static K<Eff<RT>, A> LiftIO<A>(IO<A> ma) =>
        new Eff<RT, A>(ReaderT.liftIO<RT, IO, A>(ma));
}
```

Every operation delegates to the corresponding `ReaderT` operation. `LiftIO` lets `IO` operations appear directly in LINQ expressions over the wrapper. The companion operations unwrap the higher-kinded value, run it, or construct common values:

```csharp
public static class Eff
{
    public static Eff<RT, A> As<RT, A>(this K<Eff<RT>, A> ma) => (Eff<RT, A>)ma;
    public static IO<A> Run<RT, A>(
        this K<Eff<RT>, A> ma,
        RT runtime) =>
        ma.As().runEff.Run(runtime).As();
    public static Eff<RT, A> Pure<RT, A>(A value) =>
        new(ReaderT.Pure<RT, IO, A>(value));
    public static Eff<RT, A> Fail<RT, A>(Error error) =>
        new(ReaderT.liftIO<RT, IO, A>(IO.Fail<A>(error)));
    public static Eff<RT, RT> runtime<RT>() =>
        new(ReaderT.ask<IO, RT>());
}
```

The wrapper gains more capabilities by enclosing a larger transformer stack.

## [04]-[READABLE_CAPABILITY]

`Readable<M, Env>` abstracts environment access away from `ReaderT`, `Reader`, `Eff`, or any other concrete implementation:

```csharp
public interface Readable<M, Env>
    where M : Readable<M, Env>
{
    public static abstract K<M, A> Asks<A>(Func<Env, A> f);

    public static virtual K<M, Env> Ask =>
        M.Asks(Prelude.identity);

    public static abstract K<M, A> Local<A>(
        Func<Env, Env> f,
        K<M, A> ma);
}
```

`Readable` does not require `Monad`. Its helper functions provide four operations:

```csharp
public static class Readable
{
    public static K<M, Env> ask<M, Env>()
        where M : Readable<M, Env> =>
        M.Ask;
    public static K<M, A> asks<M, Env, A>(Func<Env, A> f)
        where M : Readable<M, Env> =>
        M.Asks(f);
    public static K<M, A> asksM<M, Env, A>(Func<Env, K<M, A>> f)
        where M : Readable<M, Env>, Monad<M> =>
        M.Flatten(M.Asks(f));
    public static K<M, A> local<M, Env, A>(
        Func<Env, Env> f,
        K<M, A> ma)
        where M : Readable<M, Env> =>
        M.Local(f, ma);
}
```

The simplified `Eff` gains the capability by delegating again:

```csharp
public class Eff<RT> : Monad<Eff<RT>>, Readable<Eff<RT>, RT>
{
    // Monad members omitted

    public static K<Eff<RT>, A> Asks<A>(Func<RT, A> f) =>
        new Eff<RT, A>(ReaderT<RT, IO, A>.Asks(f));
    public static K<Eff<RT>, A> Local<A>(
        Func<RT, RT> f,
        K<Eff<RT>, A> ma) =>
        new Eff<RT, A>(ma.As().runEff.Local(f));
}
```

Code then requires only the capabilities it uses:

```csharp
public static K<M, int> addRdr<M>(K<M, int> mx, K<M, int> my)
    where M : Readable<M, int>, Monad<M> =>
    from x in mx
    from y in my
    from z in Readable.ask<M, int>()
    select x + y + z;

var compute = addRdr(
    Eff.Pure<int, int>(100),
    Eff.Pure<int, int>(200));

var result = compute.Run(300).Run(); // 600
```

`addRdr` knows neither the concrete monad nor how it stores the environment. It requires values that are bindable and readable. Its arguments can still represent computations with IO, parallelism, resource cleanup, errors, or retries — capabilities this function does not name. Shared support functions operate across multiple domain monads by reading their configuration through `Readable`.

## [05]-[REQUEST_CONTEXT]

A session is supplied once per API request and read anywhere in a compatible monad:

```csharp
public record User(string Id, Seq<Role> Memberships);
public record Session(User LoggedIn, DateTime LastAccess);

public abstract record Right;

public record CanViewDemographic : Right;
public record CanSendInvoice : Right;
public record Role(Seq<Right> Rights);

public static K<M, User> getLoggedInUser<M>()
    where M : Readable<M, Session> =>
    Readable.asks<M, Session, User>(session => session.LoggedIn);
public static K<M, Unit> assertHasRight<M, R>()
    where R : Right
    where M : Readable<M, Session>, Functor<M> =>
    getLoggedInUser<M>().Map(user =>
        user.Memberships.Exists(role =>
            role.Rights.Exists(right => right is R))
                ? Prelude.unit
                : throw new SecurityException("Access denied"));
```

Running the request computation with `Run(session)` threads the read-only user, roles, and rights through the stack. Authorization logic is written once and used by every monad that exposes the session through `Readable`:

```csharp
public static Eff<Session, Unit> sendInvoice() =>
    from _ in assertHasRight<Eff<Session>, CanSendInvoice>()
    from result in doSendInvoice()
    select result;
```

## [06]-[LOCAL_ENVIRONMENTS]

Two operations limit the environment a computation sees:
- `local(f, ma)` maps `Env` to another `Env` of the same type and runs `ma` with that temporary value. The change is scoped to `ma`; later computation sees the original environment.
- `with(f, ma)` maps an outer environment to a different environment type, such as `AppConfig -> DbConfig`, so a data layer receives only its database configuration.

`with` is not part of `Readable`, because `Env` is fixed in the trait implementation. Use `Reader.with` or `ReaderT.with`, and expose an equivalent operation on a wrapper where environment-type mapping is useful.

## [07]-[TRANSFORMER_POSITION]

Any monad lifts into `ReaderT`; `IO` is one concrete choice. Lifting `Validation<F, A>` gives validators access to an environment. `ReaderT` sits outermost in most stacks, so the inner monads reach the environment:

```text
ReaderT<Env, Inner, A>
```

The placement is effective, not mandatory. Unlike a standalone `Reader`, `ReaderT` composes environment access with another monad, and domain wrappers expose that capability beside other transformer-provided capabilities.
