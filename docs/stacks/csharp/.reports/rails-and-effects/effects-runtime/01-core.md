# Effects-Runtime: Core

[CARRIER_HIERARCHY]:
- `IO<A>` is the bottom carrier — `abstract record` implementing `K<IO, A>`, `Monoid<IO<A>>`, `Semigroup<IO<A>>`. Every execution path bottoms out here; `Eff<A>` and `Eff<RT, A>` are readers over `IO`.
- `Eff<A>` is `record Eff<A>(Eff<MinRT, A> effect)` where `MinRT` is a zero-byte `readonly struct`. It implements `Readable<Eff<A>, A>` via `Deriving.Readable<Eff<A>, A, ReaderT<A, IO>>` — the type parameter `A` itself is the reader environment, so `ask<A>()` projects the bound type, not a capability record. Use `Eff<A>` only when the computation needs no capability beyond cancellation and resource tracking.
- `Eff<RT, A>` is `record Eff<RT, A>(ReaderT<RT, IO, A> effect)` — a `ReaderT` over `IO` named distinctly for ergonomics. The `RT` type carries capability slots via `Has<Eff<RT>, Cap>` implementations; the computation reads them via `asks<RT, Cap>()` or `Has<Eff<RT>, RT, Cap>.ask`.
- `Eff` (non-generic) is the higher-kinded type-class implementing `Readable<Eff, MinRT>`, `Natural<Eff, Eff<MinRT>>`, and all monad/applicative/fallible traits. It is the constraint surface for carrier-polymorphic arrows: `K<Eff, A>` abstracts over `Eff<A>` and `Eff<MinRT, A>` simultaneously via `Natural` conversions.

[LIFT_FORMS]:
- `Eff.lift<A>(Func<A>)`, `Eff.lift<A>(Func<Fin<A>>)`, `Eff.lift<A>(Func<Either<Error,A>>)` — pure synchronous lifts; the `Fin`/`Either` overloads admit pre-computed failure.
- `Eff.lift<A>(Func<EnvIO, A>)` and `Eff.lift<A>(Func<EnvIO, Fin<A>>)` — synchronous lifts with `EnvIO` access; implemented as `+Prelude.envIO.Bind(e => Eff<A>.Lift(() => f(e)))`. The unary `+` on `K<Eff, A>` is the downcast operator, not a custom combinator.
- `Eff.lift<A>(Func<Task<A>>)`, `Eff.lift<A>(Func<Task<Fin<A>>>)`, `Eff.lift<A>(Func<EnvIO, Task<A>>)` — async lifts; `Task<Fin<A>>` unwraps via `.ThrowIfFail()` before storage, so failures surface at run-time, not at lift time.
- `Eff.lift<A>(IO<A>)` — admits an already-constructed `IO` monad directly into the `Eff` carrier; the canonical bridge between the two layers.
- `Prelude.liftEff` mirrors all static `Eff.lift` overloads; prefer the static form on `Eff<A>` for explicitness at call sites.
- `IO.lift<A>(Func<EnvIO, A>)` carries the `EnvIO` token for cancellation polling; `IO.liftAsync<A>(Func<EnvIO, Task<A>>)` and `IO.liftVAsync<A>(Func<EnvIO, ValueTask<A>>)` are the async equivalents. `IO.env` is the built-in effect `IO<EnvIO>` for accessing the context without manual lift construction.

[RUN_COLLAPSE]:
- `Eff<A>.Run()` (via `EffExtensions`) — synchronous; returns `Fin<A>`; creates a fresh `EnvIO.New()`. Exceptions from unmanaged boundaries are caught and converted to `Error`.
- `Eff<A>.Run(EnvIO envIO)` — synchronous with caller-supplied environment; allows sharing a cancellation token across composed operations.
- `Eff<A>.RunAsync()` — returns `Task<Fin<A>>`; creates `EnvIO.New()` internally.
- `Eff<A>.RunAsync(EnvIO envIO)` — `Task<Fin<A>>` with explicit env.
- `Eff<A>.RunUnsafe()` / `RunUnsafeAsync()` — omit the `Fin` wrapper; throw on failure. Reserve for composition roots that have already exhausted recovery.
- `Eff<A>.RunIO()` — returns `IO<A>` without executing it; the escape hatch for passing an `Eff` computation into an `IO`-shaped boundary.
- `Eff<RT, A>.Run(RT env)` / `RunAsync(RT env)` — the RT form passes the runtime record; the `Fin<A>` wrapping and exception capture are identical to the no-RT form.
- `IO<A>.Run()` / `Run(EnvIO)` — synchronous; throws on failure because `IO` does not return `Fin`. When `ValueTask.IsCompleted` is false on the synchronous path, the runtime blocks. Use only when the call site is guaranteed synchronous (e.g., Rhino plug-in `OnLoad`).
- `IO<A>.RunAsync()` / `RunAsync(EnvIO)` / `RunAsync(CancellationToken)` — `ValueTask<A>` return; the `CancellationToken` overload allocates a new `EnvIO.New(null, token)` internally.

[ENVIO_AND_CONTEXT]:
- `EnvIO.New(resources, token, source, syncContext, timeout)` — all parameters optional. The `source` and `token` are kept separate: `source` is disposable and owns cancellation; `token` is the read-only observable. Passing `source: null` creates a fresh `CancellationTokenSource` internally.
- `EnvIO.Local` — `New(null, Token, null, SynchronizationContext.Current)`; creates fresh `Resources` and a fresh `CancellationTokenSource` linked to the parent token. Both are owned (bits 1 and 2 of `Own`). The resulting scope is fully isolated.
- `EnvIO.LocalResources` — `New(null, Token, Source, SyncContext)`; creates fresh `Resources` (owned) but reuses the parent source and sync context. Used by `IO.BracketFail()` — resource scope is per-attempt, cancellation context is shared across attempts.
- `EnvIO.LocalCancel` — inherits `Resources` and `Token`, clears `Source`. Used for cancel-scope isolation without resource isolation.
- `IO.Post()` — marshals execution onto `env.SyncContext` when non-null; no-op when `SyncContext` is null. The SyncContext is captured from `SynchronizationContext.Current` at `EnvIO.New()` time, not at `Post()` call time.

[RESOURCE_SAFETY]:
- `IO<A>.Bracket()` — `WithEnv(env => env.LocalResources)`; creates a fresh `Resources` scope that disposes on both success and failure. Every `use<A>()` call and `use<A>(acquire, release)` call registers an `IDisposable` or custom release function in `env.Resources`; disposal runs at scope boundary.
- `IO<A>.BracketFail()` — `WithEnvFail(env => env.LocalResources)`; disposes the fresh resource scope only on failure. Used as the atom inside `Retry`: each failed attempt releases its acquired handles while the successful attempt's resources propagate to the enclosing scope.
- `IO<C>.Bracket<B,C>(Func<A, IO<C>> Use, Func<A, IO<B>> Fin)` — the three-argument form is sugar for `Bind(x => Use(x).Finally(Fin(x)))`.
- `IO<C>.Bracket<B,C>(Use, Catch, Fin)` — adds an error recovery path: `Bind(x => Use(x).Catch(Catch).Finally(Fin(x)))`. `Fin` always runs; `Catch` fires only on error.
- `Prelude.use<A>(IO<A> acquire) where A : IDisposable` — lifts `acquire` into the current `Resources` scope; the value is released when the enclosing `Bracket()` scope exits. `use<A>(Func<A> acquire, Func<A, IO<Unit>> release)` accepts a custom release function for non-`IDisposable` handles.
- `Prelude.useAsync<A>` — for `IAsyncDisposable`.
- `IO<A>.Finally<X>(K<IO, X> finally)` — runs `finally` unconditionally after `fa`, regardless of success or failure; result of `finally` is discarded. The `Final<IO>` trait static abstract exposes `Finally` generically, enabling use on `K<IO, A>`.

[RECOVERY_COMBINATORS]:
- `@catch<M,A>(Func<Error, K<M,A>> Fail)` — unconditional catch; the `@` prefix is mandatory because `catch` is a C# keyword. Returns `CatchM<Error, M, A>`.
- `@catch<M,A>(Func<Error, bool> predicate, Func<Error, K<M,A>> Fail)` — guarded catch; only fires when `predicate` returns true.
- `@catch<M,A>(Error error, Func<Error,K<M,A>> Fail)` / `@catch<M,A>(int errorCode, ...)` — match by equality or numeric code.
- `@catch<M,A>(K<M,A> Fail)` — recovery value, no inspection of the error.
- `catchOf<E, M, A>(Func<E, K<M,A>> Fail) where E : Error` — typed catch; automatically narrows to `E` subtype errors and dispatches, ignoring the rest.
- `expected<M,A>` / `exceptional<M,A>` / `expectedOf<E,M,A>` / `exceptionalOf<E,M,A>` — `expected` targets `Expected` (user-defined, recoverable) errors; `exceptional` targets `Exceptional` (system exception wrappers). Both forward to `catchOf`.
- `CatchM<E, M, A>` is a value — `(Match predicate, Action handler)`. Multiple `CatchM` values compose via the `|` operator on `K<F, A>`: `lhs | @catch(...) | @catch(...)` builds a left-to-right guarded chain; first matching predicate wins.
- `Catch(fa, CatchM)` extension on `K<F, A>` is the method form; `FallibleExtensions` provides all arity variants.
- `IfFail(Func<Error, A>)` / `IfFailEff(Func<Error, Eff<A>>)` — instance members on `Eff<A>` and `Eff<RT,A>`; equivalent to `.Catch(Fail)` and `.Catch(Fail).As()`.

[SCHEDULE_BUILDERS]:
- `Schedule.spaced(Duration)` — constant delay between every step; produces an infinite series.
- `Schedule.linear(Duration seed, double factor = 1.0)` — delay grows linearly: `seed * (1 + factor * n)`.
- `Schedule.exponential(Duration seed, double factor = 2.0)` — delay doubles (or multiplies by `factor`) each step.
- `Schedule.fibonacci(Duration seed)` — Fibonacci-shaped delay series.
- `Schedule.upto(Duration max)` — emits until cumulative wall-clock time exceeds `max`; accepts an optional `currentTimeFn` for deterministic testing.
- `Schedule.fixedInterval(Duration)` — fires at absolute wall-clock boundaries regardless of execution time.
- `Schedule.windowed(Duration)` — subdivides a window into slots; restarts from the window boundary each cycle.
- `Schedule.recurs(int times)` — returns `ScheduleTransformer`, not `Schedule`; it applies `.Take(times)` to the base schedule it modifies. Composed as `Schedule.recurs(5) | Schedule.exponential(10.Milliseconds())`.
- `Schedule.maxDelay(Duration max)` — `ScheduleTransformer`; caps each individual delay at `max`.
- `Schedule.maxCumulativeDelay(Duration max)` — `ScheduleTransformer`; terminates the schedule once total accumulated delay meets or exceeds `max`. Terminates on the step *before* the threshold is crossed.
- `Schedule.jitter(Duration min, Duration max)` — `ScheduleTransformer`; adds absolute random jitter in `[min, max]`.
- `Schedule.jitter(double factor = 0.5)` — `ScheduleTransformer`; adds proportional jitter: `delay * factor * rand`.
- `Schedule.decorrelate(double factor = 0.1, Option<int> seed = default)` — `ScheduleTransformer`; emits two durations per base step — `current + jitter` then `current - jitter` — doubling the schedule's length. Accepts an optional `seed` for reproducible jitter.
- `Schedule.NoDelayOnFirst` — `ScheduleTransformer`; strips the first step delay to zero then restores it. Equivalent to `s => s.Tail.Prepend(Duration.Zero)`.
- `Schedule.RepeatForever` — `ScheduleTransformer`; wraps the schedule in a repeat loop; use `Schedule.Forever` (the bare Schedule) for an infinite constant-zero-delay sequence.

[SCHEDULE_ALGEBRA]:
- `|` on two `Schedule` values is `Union`: min-of-two semantics at each step, runs to the *longer* schedule. The `SchUnion` source: when one is exhausted, yields from the other's remaining steps.
- `&` on two `Schedule` values is `Intersect`: max-of-two semantics, runs to the *shorter* schedule. `SchIntersect`: `Math.Max` at each zipped step.
- `+` on two `Schedule` values is `Combine` (sequential append): appends one series after the other exhausts.
- `|` on `Schedule | ScheduleTransformer` and `ScheduleTransformer | Schedule` applies the transformer to the schedule — it is *not* a union. Same for `&` with a transformer operand. This asymmetry means `|` means different things depending on operand types.
- `ScheduleTransformer + ScheduleTransformer` composes transformers left-to-right: `f + g` produces `g(f(s))` for any schedule `s`.
- `ScheduleTransformer` is implicitly convertible to `Schedule` via `static implicit operator Schedule(ScheduleTransformer t) => Schedule.Forever | t`, making a bare transformer in an operator chain produce a `Schedule`.
- `Schedule.Interleave(b)` — alternates durations from two schedules element-by-element; both must run for the result to continue.

[RETRY_AND_REPEAT]:
- Retry uses `BracketFail()` internally: each attempt runs in a scoped resource environment that disposes on failure but not on success.
- Repeat uses `Bracket()` internally: each iteration disposes resources on completion regardless of outcome. The `RepeatUntil` source confirms: `IO.yieldFor(head).Bind(_ => Bracket().Bind(v => ...))`.
- Both `RetryUntil(Schedule, predicate)` and `RepeatUntil(Schedule, predicate)` call `schedule.PrependZero.Run()` — the zero prepended means the first attempt fires immediately (zero delay), with scheduled delays between subsequent attempts.
- `RetryUntil(Func<Error, bool> predicate)` — no-schedule form; recursively calls itself without `yieldFor`, so there is no delay between attempts and no schedule exhaustion. Use with a maximum-iteration guard via a `recurs`-capped schedule.
- `Fold<S>(Schedule, S, Func<S,A,S>)` — runs `ma` repeatedly, accumulating state; terminates when the schedule is exhausted.
- `FoldWhile<S>(Schedule, S, folder, Func<(S State, A Value), bool>)` — the predicate receives a named tuple `(State, Value)`, not two separate parameters. The overloads accepting `Func<S, bool>` and `Func<A, bool>` project to this form internally.

[TRAVERSAL_AND_SEQUENCING]:
- `seq.TraverseM<M, B>(Func<A, K<M, B>>)` on `Seq<A>`, `Arr<A>`, `Lst<A>`, `Set<A>`, `HashSet<A>`, `Iterable<A>` — monadic traversal; returns `K<M, Seq<B>>` (or the appropriate container type). Call `.As()` immediately to recover the concrete `Eff<Seq<B>>`.
- `Traversable.traverseM<T, M, A, B>(f, ta)` — the free-standing static form; `T` must satisfy `Traversable<T>`. Prefer the instance `.TraverseM` on concrete collection types to avoid type inference failures on `T`.
- `Seq<K<Eff, A>>.Actions()` (via `Applicative<Eff>`) — sequences effects, discards values; used when only side-effects matter. The `Eff` trait class implements `Actions` by running each effect's `IO` as a left-fold via `io.Actions()`.
- `fork<M, A>(K<M, A> ma)` — `where M : MonadUnliftIO<M>`; lifts `ma` into a background task returning `ForkIO<A>(Cancel: IO<Unit>, Await: IO<A>)`. The forked computation runs in a child `EnvIO` with its own `CancellationTokenSource` linked to the parent token.
- `awaitAll` / `awaitAny` — overloads accepting `params K<M,A>[]` and `Seq<K<M,A>>` or `Seq<ForkIO<A>>`; the `awaitAll(Seq<K<M,A>>)` form forks all internally via `TraverseM`.

[OPERATOR_ALGEBRA]:
- `>>` on `K<Eff, A>` with `Func<A, K<Eff, B>>` — Kleisli bind; desugars to `+ma.Bind(f)`. The unary `+` downcast is emitted automatically by the operator implementation, not by user code.
- `>>>` on `K<Eff, A>` with `K<Eff, B>` — `Action`; runs `ma`, discards value, runs `mb`, returns `mb`'s value.
- `*` on `K<Eff, Func<A,B>>` with `K<Eff, A>` — applicative `Apply`; both sides run, function is applied to value.
- `*` on `Func<A,B>` with `K<Eff, A>` — lifts the pure function and applies; `map` without explicit `.Map()`.
- `|` on two `K<Eff, A>` — `Choose`; runs left, falls back to right on failure.
- `|` on `K<Eff, A>` with `CatchM<Error, Eff, A>` — applies the catch combinator.
- `|` on `K<Eff, A>` with `Pure<A>` — fallback to a pure value on failure.
- Downcast via `+ma` (`operator+(K<Eff,A>)` → `(Eff<A>)ma`) and `.As()` (`this K<Eff,A>` → `(Eff<A>)ma`, plus `this Eff<MinRT,A>` → `Eff<A>`) are semantically identical casts, not copies. The `>> Lower` form is also a downcast alias.
- `IO<A>` carries the same operator set: `>>`, `>>>`, `*`, `|`.

[RUNTIME_CAPABILITY_PROJECTION]:
- `Eff<RT, A>` reads the runtime via `asks<RT, Cap>(Func<RT, Cap>)` — returns `Ask<RT, Cap>`, which `Eff<RT,A>.Bind` accepts directly. The `Ask<RT,A>` record resolves via the `Readable<M, Env>` trait without allocating a new monad wrapper.
- Runtime capability slots use `Has<Eff<RT>, Cap>` — a static abstract interface with a single `static abstract K<M, Cap> Ask { get; }`. Implementing `Has<Eff<RT>, MyService>` on the runtime record enables `Has<Eff<RT>, MyService>.Ask` to inject the service without any explicit environment read.
- `Eff<A>` (no-RT) is `Readable<Eff<A>, A>` where the env is the type parameter itself — useful for dependency-injection patterns where the outer type binds all capabilities before running, but this form cannot extend capability slots at composition time.
- `WithRuntime<RT>()` on `Eff<A>` converts to `Eff<RT, A>` via `MonadIO.liftIO<Eff<RT>, A>(effect.RunIO(default(MinRT))).As()` — it runs the inner `ReaderT<MinRT, IO, A>` against the zero-byte `MinRT` value to produce `IO<A>`, then re-lifts that `IO<A>` under the `RT` carrier. This is a carrier upgrade, not a capability injection; the `RT` value is still required at the final `Run(RT env)` call.

[ASYNC_BOUNDARY_DISCIPLINE]:
- `IO<A>.Run()` internally calls `RunAsync()` and then blocks if `ValueTask.IsCompleted` is false. There is no dedicated synchronous execution path in the DSL — synchronous appearance is an optimisation for already-completed `ValueTask`.
- `IO<A>.RunAsync(CancellationToken token)` creates `EnvIO.New(null, token)` — fresh resources and a fresh `CancellationTokenSource` linked to the supplied token. Prefer passing an existing `EnvIO` to preserve resource scope and avoid the allocation; the `CancellationToken` overload is for integration points that hand tokens from external callers.
- `IO.yieldFor(Duration)` emits a `Task.Delay`-based pause; used inside schedule-driven retry/repeat to honour the inter-attempt delay without blocking the thread.
- `Eff<A>.RunIO()` returns `IO<A>` without executing; enables deferred execution, composition into larger `IO` pipelines, or passing to test harnesses that run the `IO` layer directly.
