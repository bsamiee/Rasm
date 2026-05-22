# [H1][LANGUAGEEXT_ROP_EFFECTS]
>**Dictum:** *Rails preserve failure meaning until a boundary consumes it.*

<br>

[IMPORTANT] Rasm uses LanguageExt effects as architecture, not syntax preference. Keep rail choice visible in the return type and keep `.Match` at final boundaries.

---
## [1][RAIL_SELECTION]
>**Dictum:** *Each rail answers a different failure question.*

<br>

| [INDEX] | [RAIL] | [QUESTION] | [CANONICAL_USE] |
| :-----: | ------ | ---------- | --------------- |
| [1] | `Option<T>` | Is a value present? | Optional GH2 inputs, probes, fallback values. |
| [2] | `Fin<T>` | Did a fallible operation succeed? | Smart constructors, dispatch, native API projection, shape normalization. |
| [3] | `Validation<Error,T>` | Which independent constraints failed? | Context and geometry validation before analysis. |
| [4] | `Eff<RT,T>` | What runtime-dependent effect produces the value? | Analysis pipelines requiring `Env`, progress, cancellation, or context. |
| [5] | `IO<T>` | Which boundary effect is deferred? | Resource and async descriptions before execution. |

---
## [2][FIN]
>**Dictum:** *`Fin<T>` is the default synchronous error rail.*

<br>

Use `Fin<T>` when:
- A RhinoCommon call can return invalid, empty, unsupported, or cancelled output.
- A Thinktecture value object needs custom error mapping.
- A dispatch lookup can fail with a typed `Error`.
- A `Validation<Error,T>` result must enter an effect pipeline through `.ToFin()` and `liftEff` or `Eff.lift`.

[CRITICAL] Do not unwrap `Fin<T>` mid-pipeline. Use `Map`, `Bind`, `BiMap`, `BindFail`, and LINQ comprehension until the adapter writes a warning, output, or test assertion.

---
## [3][VALIDATION]
>**Dictum:** *`Validation<Error,T>` accumulates independent failures.*

<br>

Use `Validation<Error,T>` when:
- Multiple geometry requirements must report together.
- Context construction validates absolute tolerance, relative tolerance, angle tolerance, and units.
- A public API should return all expected invalid inputs instead of the first failure.

`Validation<Error,T>` composes through tuple `.Apply()` and traversal. Convert `Validation<Error,T>` to `Fin<T>` with `.ToFin()` when accumulation ends. Convert `Fin<T>` to `Validation<Error,T>` only at accumulation boundaries through explicit success/failure projection or repo-owned extensions that are proven in the pinned API.

---
## [4][EFF]
>**Dictum:** *`Eff<RT,T>` is runtime-record dependency injection plus typed failure.*

<br>

Use `Eff<RT,T>` when:
- The computation needs runtime context through `Eff.runtime<RT>().Map(...).As()` or a module-level accessor such as `Env.Asks`.
- The computation must remain testable without service location.
- IO, cancellation, progress, or host context participates.
- A native operation must remain deferred until `Run(runtime)` at the adapter boundary.

Rasm shape:
- `Env` is the runtime record for analysis effects.
- `Env.Context` carries model tolerance and unit policy.
- `Env.Progress` and `Env.Cancellation` carry host feedback and cancellation.
- `Env.Asks` and `Env.EnvAsks` are the repo-owned runtime accessors over `Eff.runtime<Env>()`.
- `Operation<TGeometry,TOut>.Apply` returns `Eff<Env, Seq<TOut>>`.

[CRITICAL] Do not resurrect v4 `Has<RT, Trait>` patterns or invent generic `Eff<RT,T>.Asks` calls. Runtime records plus `Eff.runtime<RT>()` are the pinned v5.0.0-beta-77 baseline.

---
## [5][IO]
>**Dictum:** *`IO<T>` describes boundary work before execution.*

<br>

Use `IO<T>` for:
- Async APIs lifted with `liftIO(() => task)`, `liftEff(() => task)`, `Eff.lift`, or `Eff<RT,T>.LiftIO`.
- Resource lifecycles through `Bracket`.
- Boundary operations that must run exactly once after composition.
- Repeated or retried effects via `Schedule`.

Collapse surfaces:
- `Eff<RT,T>.Run(runtime)` returns `Fin<T>` for normal boundary execution.
- `Eff<RT,T>.RunIO(runtime)` preserves the resulting `IO<T>` when the host boundary must compose or delay IO further.
- `Eff<RT,T>.RunAsync(runtime)` returns asynchronous boundary execution.
- `Eff<RT,T>.RunUnsafe*` converts failures to exceptions only at host-required boundaries.
- `IO<T>.Run(...)` collapses a raw IO description when no runtime record participates.

---
## [6][IO_POLICY]
>**Dictum:** *Boundary policy is a value, not control flow.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `IO<T>.Bracket`, `BracketFail`, `Finally` | Resource-safe native handles and cleanup paths. |
| [2] | `IO<T>.WithEnv`, `WithEnvFail` | Local `EnvIO` adjustment without global mutation. |
| [3] | `IO<T>.Fork`, `ForkIO<T>`, `awaitAll`, `awaitAny` | Parallel boundary work with explicit cancellation and awaiting. |
| [4] | `IO<T>.Timeout` | Time-bounded boundary work. |
| [5] | `IO<T>.Retry`, `RetryWhile`, `RetryUntil` | Retry failed IO using optional `Schedule` policy. |
| [6] | `IO<T>.Repeat`, `RepeatWhile`, `RepeatUntil` | Repeat successful IO using optional `Schedule` policy. |
| [7] | `Schedule.recurs`, `spaced`, `linear`, `exponential`, `fibonacci` | Attempt count and backoff families. |
| [8] | `Schedule.upto`, `maxDelay`, `maxCumulativeDelay`, `jitter`, `decorrelate`, `fixedInterval`, `windowed` | Runtime budgets, delay ceilings, de-correlation, and clock-window policy. |

Use `Run` for ordinary `Eff` boundaries. Use `RunIO` only when the next boundary still speaks `IO<T>`. Use unsafe collapse only where a host API requires exceptions.

---
## [7][RECOVERY]
>**Dictum:** *Recovery is an algebraic projection over the failure channel.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `MapFail` | Annotate or normalize errors while preserving the rail. |
| [2] | `BindFail` | Recover with another fallible computation. |
| [3] | `@catch` | Selective recovery in `Eff` and `IO` pipelines. |
| [4] | `Schedule` | Retry, repeat, timeout, and backoff as values. |
| [5] | `|` alternative | Fallback path after failure. |
| [6] | `expected`, `exceptional`, `expectedOf`, `exceptionalOf` | Route expected and exceptional `Error` classes without collapsing the rail. |
| [7] | `RetryIO`, `RepeatIO`, `TimeoutIO`, `BracketIO` | Advanced IO operations available through `MonadUnliftIO` style carriers. |

---
## [8][RULES]
>**Dictum:** *Effect code stays dense by never leaving the rail early.*

<br>

- Use LINQ comprehension for multi-step rail composition.
- Use `let` for pure projections inside a comprehension.
- Use `Map` for pure success projection and `Bind` for fallible success projection.
- Use `BiMap` when both channels must be observed without collapse.
- Collapse effects only in GH2 adapters, host lifecycle, CLI entrypoints, or tests.
