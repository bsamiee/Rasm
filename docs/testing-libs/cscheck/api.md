# [H1][CSCHECK_API]
>**Dictum:** *CsCheck searches state space; Rasm supplies independent oracles.*

<br>

[IMPORTANT] Rasm pins `CsCheck 4.7.0`. Specs use `Rasm.TestKit.Spec` first; raw `Check.*` calls move into `_testkit` only after at least two specs need the same rail.

---
## [1][PACKAGE_TRUTH]
>**Dictum:** *The wrapper exposes a smaller contract than the package.*

<br>

| [INDEX] | [FACT]                   | [VALUE]                                             |
| :-----: | ------------------------ | --------------------------------------------------- |
|   [1]   | Current pin              | `CsCheck 4.7.0`                                     |
|   [2]   | Package TFM              | `net8.0`, compatible with Rasm `net10.0` projects   |
|   [3]   | Notable current addition | `SampleModelBasedAsync`                             |
|   [4]   | Rasm policy              | `Spec.ForAll` owns seed/iter/time/thread precedence |

[SOURCE] NuGet package page: https://www.nuget.org/packages/CsCheck/4.7.0

---
## [2][CHECK_SURFACE]
>**Dictum:** *Promote package APIs through one Rasm contract.*

<br>

| [INDEX] | [API]                            | [USE]                                     | [RASM]                                          |
| :-----: | -------------------------------- | ----------------------------------------- | ----------------------------------------------- |
|   [1]   | `Check.Sample`(+Async)           | Action/predicate/classifier checks        | `Spec.ForAll`; async after consumer             |
|   [2]   | `Check.SampleModelBased`(+Async) | Actual vs smaller model                   | Two stateful specs min.                         |
|   [3]   | `Check.SampleMetamorphic`        | `GenMetamorphic<T>` ops                   | Not `Spec.Metamorphic`                          |
|   [4]   | `Check.SampleParallel`           | Parallel ops + shrinking                  | `Spec.ConcurrentProfiled`; policy cleanup first |
|   [5]   | `Check.Hash`                     | Hash regression on stable large artifacts | Source proof before cache/path claims           |
|   [6]   | `Check.ChiSquared`               | Generator distribution audit              | Testkit gen validation only                     |
|   [7]   | `Check.Faster`(+Async)           | Statistical perf comparison               | BenchmarkDotNet on benchmark rail               |

---
## [3][GEN_SURFACE]
>**Dictum:** *Generators describe domains, not branches.*

<br>

| [INDEX] | [API]                                                       | [RASM_USE]                                                |
| :-----: | ----------------------------------------------------------- | --------------------------------------------------------- |
|   [1]   | `Gen.Select`, `SelectMany`                                  | Product axes and dependent generation.                    |
|   [2]   | `Gen.OneOfConst`, `OneOf`, `Frequency`                      | Case tables and edge bias.                                |
|   [3]   | `Gen.Array`, `ArrayUnique`, `Array2D`, `List`, `Dictionary` | Collection domains before `Seq<T>` projection.            |
|   [4]   | `Gen.Shuffle`, `ShuffleSelect`                              | Permutation and selection metamorphic laws.               |
|   [5]   | `Gen.Recursive`                                             | Recursive structures only with explicit depth discipline. |
|   [6]   | `Gen.Clone`                                                 | Identical streams for two-path comparisons.               |

---
## [4][RASM_POLICY]
>**Dictum:** *Every failing sample must reproduce the law.*

<br>

- `Spec.ForAll` precedence: explicit args, then `CsCheck_Seed`, `CsCheck_Iter`, `CsCheck_Time`, `CsCheck_Threads`, then package defaults.
- `Spec.Metamorphic` currently means one generated input checked by path/oracle equality; it is not CsCheck `SampleMetamorphic`.
- `Spec.Regression` records a durable shrunk seed only after product behavior is classified.
- Use model-based APIs only when the model is smaller than production: list log, scalar receipt, set/map reference, or finite state machine.
- Do not promote spec-local generators until two specs share the same domain shape.

---
## [5][SHRINKING_DISCIPLINE]
>**Dictum:** *Generators must preserve shrinking — `throw` breaks it.*

<br>

CsCheck shrinking finds the minimal counterexample by repeatedly narrowing failed inputs. Two generator patterns break shrinking:

| [INDEX] | [ANTI_PATTERN]                   | [PATTERN]                                             |
| :-----: | -------------------------------- | ----------------------------------------------------- |
|   [1]   | `throw` inside `Select`          | Use `Where` then `Select` — see code block below      |
|   [2]   | `throw` inside optional `Select` | Use `Where(opt => opt.IsSome)` — see code block below |

[DETAIL]
- [1] `Gen.Int.Select(i => i > 0 ? new T(i) : throw …)` → `Gen.Int.Where(i => i > 0).Select(i => new T(i))`
- [2] `Gen.Select(Factory).Select(opt => opt.IfNone(() => throw …))` → `Gen.Select(Factory).Where(opt => opt.IsSome).Select(opt => opt.IfNone(default!))`

The `throw` form fires CsCheck's `WhereLimit` (default 100); when exhausted CsCheck gives up with a generic "could not satisfy" message and no minimal counterexample. The `Where` form keeps shrinking on the satisfying subset.

`Try.lift` pattern for factories returning `Fin`:

```csharp
public static readonly Gen<Dimension> Dimension =
    SmallDimension
        .Select(value => Vectors.Dimension.TryCreate(value: value, obj: out Vectors.Dimension d) ? Some(d) : None)
        .Where(opt => opt.IsSome)
        .Select(opt => opt.IfNone(default(Vectors.Dimension)));
```

[SOURCE] CsCheck README on `Where` shrinking: https://github.com/AnthonyLloyd/CsCheck

---
## [6][ENV_KNOBS]
>**Dictum:** *Env policy flows through `Spec.ForAll` precedence; never set globals.*

<br>

| [INDEX] | [VAR]                | [DEFAULT]    | [USE]                                                                                            |
| :-----: | -------------------- | ------------ | ------------------------------------------------------------------------------------------------ |
|   [1]   | `CsCheck_Iter`       | 100          | Per-property iteration count.                                                                    |
|   [2]   | `CsCheck_Time`       | 0 (disabled) | Wall-clock budget in seconds (overrides Iter when set).                                          |
|   [3]   | `CsCheck_Threads`    | 1            | Parallel sample workers (for `SampleParallel`).                                                  |
|   [4]   | `CsCheck_Seed`       | unset        | Fixed seed for reproducible runs.                                                                |
|   [5]   | `CsCheck_Replay`     | 100          | Number of times to replay a failing seed for parallel reproduction.                              |
|   [6]   | `CsCheck_Sigma`      | 6.0          | Statistical significance for `Check.Faster`.                                                     |
|   [7]   | `CsCheck_Timeout`    | 30           | Per-sample timeout (seconds).                                                                    |
|   [8]   | `CsCheck_Ulps`       | 0            | Allowed ULP slack for floating equality (off by default; tolerance lives in `Spec.EqualWithin`). |
|   [9]   | `CsCheck_WhereLimit` | 100          | Filter rejection cap; lower → fail faster, higher → tolerate sparse-acceptance generators.       |

`Spec.ForAll` precedence: explicit args > env vars > package defaults. CI policy: tune `CsCheck_Iter=1000` and `CsCheck_Time=60` for nightly extended runs; keep PR validation at defaults. `CsCheck_Replay=10` for CI, default 100 for local repro.

---
## [7][MODEL_BASED]
>**Dictum:** *Model is smaller than actual; both are observed at every step.*

<br>

`Check.SampleModelBased(initial, model_initial, ops...)` traces `(actual, model)` pairs through a sequence of `GenOperation<TActual, TModel>` steps and asserts equivalence at every step. Use when:

- Actual implementation is `Atom<T>` or `ConcurrentDictionary` — model is `Dictionary<T>` or `Seq<T>`.
- Actual is a state-threaded `[Union].Switch` dispatcher — model is a `Map<Key, T>`.
- Actual is `Validation` accumulation — model is `List<Error>`.

Avoid when no smaller model exists (model = actual implementation defeats the purpose).

```csharp
public static GenOperation<Atom<HashMap<int, int>>, Dictionary<int, int>> SetOp(int key, int value) =>
    Gen.Operation(
        $"Set({key}, {value})",
        atom => atom.Swap(m => m.AddOrUpdate(key, value)),
        dict => dict[key] = value);
```

---
## [8][DISTRIBUTION_AUDIT]
>**Dictum:** *Generated distributions must be audited — assumptions about uniformity rot.*

<br>

`Check.ChiSquared(observed, expected, sigma)` audits a generator's distribution against expected frequencies. Use when:

- An edge-biased `Gen.Frequency` ships with 90/10 weights — verify the tail actually fires 10% of the time across 10k samples.
- A factory-routed generator filters >20% of inputs — verify the surviving distribution isn't skewed away from the boundary cases the test depends on.
- A SmartEnum case sweep with case-specific frequencies — verify rarely-fired cases still fire often enough to exercise their per-case oracle in `Spec.Cases`.

Audit lives in `tests/csharp/_testkit/Gens.spec.cs` (a generator self-test), not in spec files.

---
## [9][PARALLEL_CHECKS]
>**Dictum:** *`Check.SampleParallel` linearizes; `Causal.Profile` explains.*

<br>

`Check.SampleParallel` runs the generated operations concurrently and asserts a linearizable history exists. `Spec.ConcurrentProfiled` wraps this and emits `Causal.Profile` output to `ITestOutputHelper`. Use for:

- `Atom<T>` swap contention (closure-free `applyScratch` patterns).
- Process-static cache races (`ConcurrentDictionary` + `[ThreadStatic]`).
- `SmartEnum.Items` lazy initialization under first-access concurrency.

Pair with `Check.Sample(seed: ...)` after the first parallel failure to reproduce deterministically.

[SOURCE] CsCheck causal profiling: https://github.com/AnthonyLloyd/CsCheck#parallel-sampling

---
## [10][PERFORMANCE_ASSERTIONS]
>**Dictum:** *`Check.Faster` is statistical; BenchmarkDotNet is for shipping numbers.*

<br>

`Check.Faster(slow, fast, sigma)` compares two implementations across generated inputs and reports a sigma confidence that `fast < slow`. Use only for:

- Regression detection (e.g., Yuksel WSE O(n²) allocation regression on Sample.cs at `n ≥ candidate_count × MeshScale`).
- A/B-style algorithmic choice during refactor (e.g., Cholesky vs LU on the same SPD generator).

Never use for absolute performance claims — those belong in `tests/csharp/_benchmarks` with BenchmarkDotNet (see `docs/testing-libs/benchmarkdotnet/api.md`).
