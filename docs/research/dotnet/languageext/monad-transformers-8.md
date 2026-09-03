<!-- Ideas integrated into .claude/skills/dotnet-languageext/references/traits-and-transformers.md on the real types, the toy Maybe, List, and MaybeT code enters no skill
# [MONAD_TRANSFORMERS]

## [01]-[MOTIVATION]

`Bind` continues only in the same higher-kinded type:

```csharp
K<M, B> Bind<A, B>(K<M, A> ma, Func<A, K<M, B>> f);
```

`Option<A>` computations cannot bind `IO<B>` computations. Nesting ordinary monads (`IO<Option<A>>`) compiles, but the caller inspects the inner `Option` by hand and reproduces its branching inside `IO`. Monad transformers package that nested behavior. `OptionT<IO, A>` combines optional and IO behavior in one monad, and ordinary functions lift into one expression:

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

`readAllLines` needs no explicit lift, because `OptionT` lifts IO through its LINQ `Bind` and `SelectMany` extensions. The explicit form:

```csharp
MonadIO.liftIO<OptionT<IO>, Seq<string>>(readAllLines(path));
```

The same composition serves other pairings: `ValidationT<IO, A>` combines validation with IO.

## [02]-[TRANSFORMER_CONTRACT]

```csharp
public interface MonadT<T, out M> : Monad<T>
    where T : MonadT<T, M>
    where M : Monad<M>
{
    static abstract K<T, A> Lift<A>(K<M, A> ma);
}
```

`T` is a transformer and a monad, it stacks inside another transformer. `M` is the lifted monad, and `Lift` embeds an `M<A>` action in `T`. With first-class higher kinds the relationship reads `T<M<A>>`. LanguageExt expresses it with the `K<T, K<M, A>>` encoding and a concrete transformer representation.

Regular monads become transformers only through a bespoke transformer implementation. The reverse holds through `Identity`: `OptionT<Identity, A>` corresponds to `Option<A>`. Dedicated regular types remain preferable for performance. Transformer types carry a `T` suffix.

## [03]-[BUILDING_MAYBE_T]

Optional transformers store an arbitrary monad containing a known `Maybe` value:

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

The implementation knows the cases of `Maybe<A>`. It does not know the concrete `M`, it uses only the operations `Monad<M>` guarantees. The known and lifted monads occupy no universal nesting order: here `M` is outside `Maybe`, and other transformers arrange their representation differently. The concrete wrapped type, not the `T` suffix, determines how the layers unwrap.

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
- `Map` maps through `M`, then through `Maybe`
- `Pure` makes a `Just` and lifts it with `M.Pure`
- `Bind` sequences `M`, continues with `f` for `Just`, and lifts `Nothing` back into `M`
- `Lift` maps the result of `M` into `Just` to create the required `K<M, Maybe<A>>`
- `Apply` uses `Bind` and `Map`. Reaching the inner `Maybe` requires sequencing the outer `M`, only the inner type's applicative operation is directly available in an expanded implementation

Convenience functions lift either layer and construct the optional states:

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

The generic `MonadT.lift` performs the inner-monad lift as well, bespoke functions need fewer generic arguments.

## [04]-[LIFTING_IO]

No `IOT` exists. In an IO-based stack, `IO<A>` is the innermost monad. Repeated `lift(lift(lift(io)))` calls expose the stack depth, `liftIO` forwards the action through every transformer to `IO`.

C# cannot make a transformer implement a trait only for the choices of `M` that contain IO. LanguageExt has related interfaces: the `MonadIO` operations inherited by monads carry default unsupported behavior, and `MonadIO<M>` declares that a type supports IO and serves as a generic constraint. Transformer types implement `MonadIO<T>` and pass IO operations to the lifted `M`, a transformer omits it only when IO is deliberately barred from every stack that uses it.

For `MaybeT`, lifting maps the IO result into `Maybe` and passes the action inward, at the `IO` layer, lifting is identity:

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

Abstractions that perform IO use `IO<A>` internally.

## [05]-[EXECUTION]

`Run` extensions expose one layer of the stack:

```csharp
public static K<M, Maybe<A>> Run<M, A>(this K<MaybeT<M>, A> ma)
    where M : Monad<M> =>
    ma.As().runMaybeT;
```

Running `MaybeT<IO, A>` once yields `K<IO, Maybe<A>>`, running that IO yields `Maybe<A>`. Consumers stop after any layer when the partially unwrapped result is useful in another monadic expression. Stack order matters because `Run` unwraps one layer at a time, and transformers differ in where they place the lifted monad: `MaybeT<M, A>` stores `K<M, Maybe<A>>`, and `ReaderT<Env, M, A>` stores `Func<Env, K<M, A>>`. Inspect the concrete wrapped type to know what each `Run` produces.

## [06]-[STACK_ENCAPSULATION]

LanguageExt supplies the common transformers. Application code composes them and hides the generic stack behind a stable type with a domain-focused API:

```csharp
public record AppConfig;
public record App<A>(ReaderT<AppConfig, IO, A> runService) : K<App, A>;

public static class AppExtensions
{
    public static App<A> As<A>(this K<App, A> ma) => (App<A>)ma;
    public static IO<A> Run<A>(this K<App, A> ma, AppConfig config) => ma.As().runService.Run(config).As();
}
```

Without derivation, the `App` trait implementations forward `Map`, `Pure`, `Apply`, `Bind`, IO operations, and readable-environment operations to `ReaderT<AppConfig, IO>`. `Deriving` removes that forwarding: each exposed capability uses its deriving trait, and the wrapper supplies one natural transformation in each direction:

```csharp
public class App : Deriving.MonadIO<App, ReaderT<AppConfig, IO>>
{
    public static K<ReaderT<AppConfig, IO>, A> Transform<A>(K<App, A> fa) => fa.As().runService;
    public static K<App, A> CoTransform<A>(
        K<ReaderT<AppConfig, IO>, A> fa) =>
        new App<A>(fa.As());
}
```

`Transform` unwraps the implementation, `CoTransform` wraps it again. The deriving traits delegate to the hidden stack. The declaration above covers the monad-and-IO capability, readable-environment behavior requires its own deriving trait.

The supplied transformer set covers failure and alternatives (`EitherT`, `OptionT`, `TryT`, `ValidationT`), continuations and identity (`ContT`, `IdentityT`), environment, output, and state (`ReaderT`, `WriterT`, `StateT`, `RWST`), and streaming roles (`ProducerT`, `PipeT`, `ConsumerT`, `SinkT`, `SourceT`, `ConduitT`).

## [07]-[DOMAIN_MONADS]

Domain wrappers expose only the operations meaningful to one architectural layer:

```csharp
public record Db<A>(StateT<DbEnv, IO, A> runDB) : K<Db, A>;
public record Service<A>(ReaderT<ServiceEnv, IO, A> runService) : K<Service, A>;
```

`Db<A>` fixes the database state and IO stack and exposes connection, transaction, subspace, read, and write operations. `Service<A>` fixes a configuration environment and IO stack for external-service calls. Their trait implementations derive general behavior from the hidden transformers and implement only domain-specific compromises: converting a stateful database computation to plain IO discards returned state, a choice valid only where the domain accepts it.

Higher-level abstractions join separate domain monads. `Api<A>` wraps a `Free<ApiDsl, A>` with DSL cases carrying a failure, a `Db<A>` action, or a `Service<A>` action:

```csharp
public abstract record ApiDsl<A> : K<ApiDsl, A>;

public record ApiFail<A>(Error Error) : ApiDsl<A>;
public record ApiDb<A>(Db<A> Action) : ApiDsl<A>;
public record ApiService<A>(Service<A> Action) : ApiDsl<A>;
public record Api<A>(Free<ApiDsl, A> runApi) : K<Api, A>;
```

The API lifts database actions through explicit read-only or read-write operations and lifts service actions separately, a registration workflow keeps transactional database work distinct from sending an external email. Running `Api<A>` requires an interpreter: `Pure` completes, `Bind` continues, failures become failed IO, and the database and service cases run their hidden stacks with the supplied environments. Both domain stacks end in `IO`, the interpreter returns `IO<A>`. The architecture hides representation until `Run`, centralizes environment and transaction handling, and lets the private stack change without changing consumers.

## [08]-[PERFORMANCE_AND_DESIGN]

Nested transformers add lambdas, allocations, and CPU cost. Build from the compositional pieces, hide the stack, and prioritize correctness. When profiling shows the stack is a bottleneck, replace its private implementation with one bespoke monad behind the same domain surface, consumers do not change. Choose the stack from the capabilities the domain needs, wrap it, and make the domain API the public feature. Constructing stacks is occasional infrastructure work, application code consumes the focused API.
-->
