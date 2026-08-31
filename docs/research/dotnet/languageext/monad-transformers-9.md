# [MONAD_TRANSFORMERS]

## [01]-[MOTIVATION]

`Bind` can only continue in the same higher-kinded type:

```csharp
K<M, B> Bind<A, B>(K<M, A> ma, Func<A, K<M, B>> f);
```

An `Option<A>` computation therefore cannot generally bind an `IO<B>` computation. Nesting ordinary monads, such as `IO<Option<A>>`, compiles, but the caller must manually inspect the inner `Option` and reproduce its branching behavior inside `IO`.

A monad transformer packages that nested behavior. `OptionT<IO, A>` combines optional and IO behavior in one monad, so ordinary functions can be lifted into one expression:

```csharp
static IO<Seq<string>> readAllLines(string path) =>
    liftIO(env => File.ReadAllLinesAsync(path, env.Token)).Map(toSeq);

static Option<string> validatePath(string path) =>
    Path.IsPathRooted(path) && Path.IsPathFullyQualified(path)
        ? Some(path)
        : None;

var computation =
    from path  in OptionT.lift(validatePath(input))
    from lines in readAllLines(path)
    select lines;
```

`readAllLines` does not need an explicit lift because `OptionT` supports lifting IO through its LINQ `Bind` and `SelectMany` extensions. The explicit form is:

```csharp
MonadIO.liftIO<OptionT<IO>, Seq<string>>(readAllLines(path));
```

This composition replaces combinations that once required dedicated types. For example, `OptionT<IO, A>` represents the former `OptionAsync<A>`, while `ValidationT<IO, A>` combines validation with IO.

## [02]-[TRANSFORMER_CONTRACT]

```csharp
public interface MonadT<T, out M> : Monad<T>
    where T : MonadT<T, M>
    where M : Monad<M>
{
    static abstract K<T, A> Lift<A>(K<M, A> ma);
}
```

`T` is both a transformer and a monad, so it can itself be stacked inside another transformer. `M` is the monad being lifted. `Lift` embeds an `M<A>` action in `T`.

With first-class higher-kinded types the relationship could be pictured as `T<M<A>>`; LanguageExt expresses it with higher-kinded encodings such as `K<T, K<M, A>>` and a concrete transformer representation.

A regular monad cannot be turned into a transformer without a bespoke transformer implementation. The reverse construction is possible by using `Identity` as the lifted monad: for example, `OptionT<Identity, A>` corresponds to `Option<A>`. Dedicated regular types may still be preferable for performance.

Transformer-related types conventionally carry a `T` suffix.

## [03]-[BUILDING_MAYBE_T]

An optional transformer can store an arbitrary monad containing a known `Maybe` value:

```csharp
public record MaybeT<M, A>(K<M, Maybe<A>> runMaybeT) : K<MaybeT<M>, A>
    where M : Monad<M>;

public static class MaybeTExtensions
{
    public static MaybeT<M, A> As<M, A>(this K<MaybeT<M>, A> ma)
        where M : Monad<M> =>
        (MaybeT<M, A>)ma;
}
```

The implementation knows the cases of `Maybe<A>`. It does not know the concrete type of `M`, so it can use only the operations guaranteed by `Monad<M>`.

The known and lifted monads do not occupy a universal nesting order. Here `M` is outside `Maybe`, but other transformers arrange their representation differently. The concrete wrapped type, not the `T` suffix alone, determines how the layers unwrap.

```csharp
public class MaybeT<M> :
    MonadT<MaybeT<M>, M>,
    MonadIO<MaybeT<M>>
    where M : Monad<M>
{
    static K<MaybeT<M>, B> Functor<MaybeT<M>>.Map<A, B>(
        Func<A, B> f,
        K<MaybeT<M>, A> ma) =>
        new MaybeT<M, B>(
            ma.As().runMaybeT.Map(mx => mx.Map(f).As()));

    static K<MaybeT<M>, A> Applicative<MaybeT<M>>.Pure<A>(A value) =>
        new MaybeT<M, A>(M.Pure(Maybe.Just(value)));

    static K<MaybeT<M>, B> Applicative<MaybeT<M>>.Apply<A, B>(
        K<MaybeT<M>, Func<A, B>> mf,
        K<MaybeT<M>, A> ma) =>
        mf.As().Bind(f => ma.As().Map(f));

    static K<MaybeT<M>, B> Monad<MaybeT<M>>.Bind<A, B>(
        K<MaybeT<M>, A> ma,
        Func<A, K<MaybeT<M>, B>> f) =>
        new MaybeT<M, B>(
            ma.As().runMaybeT.Bind(mx => mx switch
            {
                Just<A>(var x) => f(x).As().runMaybeT,
                Nothing<A> => M.Pure(Maybe.Nothing<B>())
            }));

    static K<MaybeT<M>, A> MonadT<MaybeT<M>, M>.Lift<A>(K<M, A> ma) =>
        new MaybeT<M, A>(ma.Map(Maybe.Just));
}
```

The nested operations are the implementation:
- `Map` maps through `M`, then through `Maybe`.
- `Pure` makes a `Just` and lifts it with `M.Pure`.
- `Bind` sequences `M`, continues with `f` for `Just`, and lifts `Nothing` back into `M`.
- `Lift` maps the result of `M` into `Just` to create the required `K<M, Maybe<A>>`.
- `Apply` uses `Bind` and `Map`. Reaching the inner `Maybe` requires sequencing the outer `M`, so only the inner type's applicative operation would be directly available in a more expanded implementation.

Convenience functions can lift either layer and construct the optional states:

```csharp
public partial class MaybeT<M>
{
    public static MaybeT<M, A> lift<A>(Maybe<A> ma) =>
        new(M.Pure(ma));
    public static MaybeT<M, A> Just<A>(A value) =>
        lift(Maybe.Just(value));
    public static MaybeT<M, A> Nothing<A>() =>
        lift(Maybe.Nothing<A>());
}

public class MaybeT
{
    public static MaybeT<M, A> lift<M, A>(K<M, A> ma)
        where M : Monad<M> =>
        new(ma.Map(Maybe.Just));
}
```

The generic `MonadT.lift` can also perform the inner-monad lift; bespoke functions often need fewer generic arguments.

## [04]-[LIFTING_IO]

There is no `IOT`. In an IO-based transformer stack, `IO<A>` is expected to be the innermost monad. Repeated `lift(lift(lift(io)))` calls would expose the stack depth, so `liftIO` forwards the action through every transformer until it reaches `IO`.

C# cannot make a transformer implement a trait only for those choices of `M` that contain IO. LanguageExt therefore has two related interfaces:
- The partial `Maybe.MonadIO<M>` operations inherited by monads have default unsupported behavior.
- `MonadIO<M>` declares that a type supports IO and can be used as a generic constraint.

Transformer types normally implement `MonadIO<T>` so they can pass IO operations to the lifted `M`. A transformer should omit it only when IO is deliberately barred from every stack using that transformer.

For `MaybeT`, the core lifting behavior maps the IO result into `Maybe` and passes the action inward. At the `IO` layer, lifting is the identity operation:

```csharp
static K<MaybeT<M>, A> LiftIO<A>(IO<A> ma) =>
    new MaybeT<M, A>(M.LiftIO(ma.Map(Maybe.Just)));

static K<IO, A> LiftIO<A>(IO<A> ma) => ma;
```

Specialized `Bind` and `SelectMany` extensions make an `IO<B>` continuation work directly in a `MonadIO<M>` expression:

```csharp
public static K<M, B> Bind<M, A, B>(
    this K<M, A> ma,
    Func<A, K<IO, B>> f)
    where M : MonadIO<M> =>
    M.Bind(ma, x => M.LiftIO(f(x).As()));
```

Any abstraction that performs IO should use `IO<A>` internally.

## [05]-[EXECUTION]

A `Run` extension exposes one layer of the stack:

```csharp
public static K<M, Maybe<A>> Run<M, A>(this K<MaybeT<M>, A> ma)
    where M : Monad<M> =>
    ma.As().runMaybeT;
```

Running `MaybeT<IO, A>` once yields `K<IO, Maybe<A>>`; running that IO yields `Maybe<A>`. A consumer can stop after any layer when the partially unwrapped result is useful in another monadic expression.

Stack order is significant because `Run` unwraps layers one at a time, and some transformers place the lifted monad inside while others place it outside. `MaybeT<M, A>` stores `K<M, Maybe<A>>`, whereas `ReaderT<Env, M, A>` stores `Func<Env, K<M, A>>`. Inspect the concrete wrapped type to understand what each `Run` will produce.

## [06]-[STACK_ENCAPSULATION]

Creating a transformer is substantial work, but LanguageExt supplies common transformers. Application code usually needs to compose them and hide the resulting generic stack behind a stable type and a domain-focused API.

```csharp
public record AppConfig;
public record App<A>(ReaderT<AppConfig, IO, A> runService) : K<App, A>;

public static class AppExtensions
{
    public static App<A> As<A>(this K<App, A> ma) => (App<A>)ma;
    public static IO<A> Run<A>(this K<App, A> ma, AppConfig config) => ma.As().runService.Run(config).As();
}
```

Without derivation, the `App` trait implementations merely forward `Map`, `Pure`, `Apply`, `Bind`, IO operations, and readable-environment operations to `ReaderT<AppConfig, IO>`. `Deriving` removes that forwarding boilerplate. Each exposed capability uses its corresponding deriving trait, while the wrapper supplies only a natural transformation in each direction:

```csharp
public class App : Deriving.MonadIO<App, ReaderT<AppConfig, IO>>
{
    public static K<ReaderT<AppConfig, IO>, A> Transform<A>(K<App, A> fa) => fa.As().runService;
    public static K<App, A> CoTransform<A>(
        K<ReaderT<AppConfig, IO>, A> fa) =>
        new App<A>(fa.As());
}
```

`Transform` unwraps the implementation; `CoTransform` wraps it again. The deriving traits delegate their behavior to the hidden stack. The abbreviated declaration above shows the monad-and-IO capability; readable-environment behavior requires its deriving trait as well.

The supplied transformer set covers failure and alternatives (`EitherT`, `OptionT`, `TryT`, `ValidationT`), continuations and identity (`ContT`, `IdentityT`), environment, output, and state (`ReaderT`, `WriterT`, `StateT`, `RWST`), plus streaming roles (`ProducerT`, `PipeT`, `ConsumerT`, `SinkT`, `SourceT`, `ConduitT`). `ContT` is unfinished.

This mechanism makes many old combination-specific types unnecessary:
- `TryAsync<A>` becomes `TryT<IO, A>`.
- `TryOption<A>` becomes `OptionT<Try, A>`.
- `TryOptionAsync<A>` becomes `OptionT<TryT<IO>, A>`.
- `OptionAsync<A>` becomes `OptionT<IO, A>`.
- `EitherAsync<L, R>` becomes `EitherT<L, IO, R>`.
- `ValidationAsync<F, S>` becomes `ValidationT<F, IO, S>`.

## [07]-[DOMAIN_MONADS]

A domain wrapper can expose only the operations meaningful to one architectural layer:

```csharp
public record Db<A>(StateT<DbEnv, IO, A> runDB) : K<Db, A>;
public record Service<A>(ReaderT<ServiceEnv, IO, A> runService) : K<Service, A>;
```

`Db<A>` fixes the database state and IO stack and can expose connection, transaction, subspace, read, and write operations. `Service<A>` fixes a configuration environment and IO stack for calls to external services. Their trait implementations can derive general behavior from the hidden transformers and implement only domain-specific compromises. For example, converting a stateful database computation to plain IO discards returned state, so that choice is valid only where the domain explicitly accepts it.

Separate domain monads can be joined by a higher-level abstraction. `Api<A>` wraps a `Free<ApiDsl, A>` whose DSL cases contain a failure, a `Db<A>` action, or a `Service<A>` action:

```csharp
public abstract record ApiDsl<A> : K<ApiDsl, A>;

public record ApiFail<A>(Error Error) : ApiDsl<A>;
public record ApiDb<A>(Db<A> Action) : ApiDsl<A>;
public record ApiService<A>(Service<A> Action) : ApiDsl<A>;
public record Api<A>(Free<ApiDsl, A> runApi) : K<Api, A>;
```

The API lifts database actions through explicit read-only or read-write operations and lifts service actions separately. A registration workflow can therefore keep transactional database work distinct from sending an external email.

Running `Api<A>` requires an interpreter. It recursively handles `Pure` as completion, `Bind` as continuation, failures as failed IO, and the database and service cases by running their hidden stacks with the supplied environments. Because both domain stacks end in `IO`, the interpreter can return `IO<A>`.

This architecture hides representation until `Run`, centralizes environment and transaction handling, and allows the private transformer stack to change without changing its consumers.

## [08]-[PERFORMANCE_AND_DESIGN]

Nested transformers add lambdas, allocations, and CPU cost. Build from the existing compositional pieces, hide the stack, and prioritize correctness. If profiling later shows that the stack is a bottleneck, replace its private implementation with one bespoke monad that offers the same domain surface; consumers need not change.

Choose the stack from the capabilities the domain needs. Then wrap it and make the domain API the public feature. Constructing transformer stacks is occasional infrastructure work; ordinary application code should consume the focused API rather than manipulate the stack directly.
