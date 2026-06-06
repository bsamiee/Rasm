# [CSCHECK_API]

Specs use the project testkit first; raw `Check.*` calls move into the shared testkit only after at least two specs need the same rail.

## [1][CHECK_SURFACE]

| [INDEX] | [API]                            | [USE]                                     | [PROJECT_USE]                                   |
| :-----: | -------------------------------- | ----------------------------------------- | ----------------------------------------------- |
|   [1]   | `Check.Sample`(+Async)           | Action/predicate/classifier checks        | `Spec.ForAll`; async after consumer             |
|   [2]   | `Check.SampleModelBased`(+Async) | Actual vs smaller model                   | Two stateful specs min.                         |
|   [3]   | `Check.SampleMetamorphic`        | `GenMetamorphic<T>` ops                   | Not `Spec.Metamorphic`                          |
|   [4]   | `Gen<T>.SampleParallel`          | Parallel ops + shrinking                  | `Spec.ConcurrentProfiled`; policy cleanup first |
|   [5]   | `Check.Hash`                     | Hash regression on stable large artifacts | Stable tool artifacts only                      |
|   [6]   | `Check.ChiSquared`               | Generator distribution audit              | Testkit gen validation only                     |
|   [7]   | `Check.Faster`(+Async)           | Statistical perf comparison               | BenchmarkDotNet on benchmark rail               |

## [2][GEN_SURFACE]

| [INDEX] | [API]                                                     | [PROJECT_USE]                                             |
| :-----: | --------------------------------------------------------- | --------------------------------------------------------- |
|   [1]   | `Gen.Select`, `SelectMany`                                | Product axes and dependent generation.                    |
|   [2]   | `Gen.OneOfConst`, `OneOf`, `Frequency`                    | Case tables and edge bias.                                |
|   [3]   | `Gen.Shuffle`, `ShuffleSelect`                            | Permutation and selection metamorphic laws.               |
|   [4]   | `Gen.Recursive`                                           | Recursive structures only with explicit depth discipline. |
|   [5]   | `Gen.Clone`                                               | Identical streams for two-path comparisons.               |
|   [6]   | `Gen.Enum<T>`, `FrequencyConst`, nullable/null generators | Closed-set and edge-bias domains.                         |
|   [7]   | `Gen.Operation`, `GenOperationAsync`, `GenMetamorphic`    | Stateful, async, and paired-operation laws.               |

Note: Collection domains before `Seq<T>` projection
- `HashSet`
- `List` 
- `Gen.Array`, `ArrayUnique`, `Array2D`
- `Dictionary`, `SortedDictionary`

## [3][PROJECT_POLICY]

- `Spec.ForAll` precedence: explicit args, then `CsCheck_Seed`, `CsCheck_Iter`, `CsCheck_Time`, `CsCheck_Threads`, then package defaults.
- `Spec.Metamorphic` currently means one generated input checked by path/oracle equality; it is not CsCheck `SampleMetamorphic`.
- `Spec.Regression` records a durable shrunk seed only after product behavior is classified.
- Use model-based APIs only when the model is smaller than production: list log, scalar receipt, set/map reference, or finite state machine.
- Do not promote spec-local generators until two specs share the same domain shape.
- Use `Check.Hash` only for stable tool artifacts. Treat cache location and key shape as package-owned details unless current source inspection proves them.

## [4][SHRINKING_DISCIPLINE]

CsCheck shrinking finds the minimal counterexample by repeatedly narrowing failed inputs. Two generator patterns break shrinking:

| [INDEX] | [ANTI_PATTERN]                   | [PATTERN]                                             |
| :-----: | -------------------------------- | ----------------------------------------------------- |
|   [1]   | `throw` inside `Select`          | Use `Where` then `Select` — see code block below      |
|   [2]   | `throw` inside optional `Select` | Use `Where(opt => opt.IsSome)` — see code block below |

Generator repair patterns:
- [1] `Gen.Int.Select(i => i > 0 ? new T(i) : throw …)` → `Gen.Int.Where(i => i > 0).Select(i => new T(i))`
- [2] `Gen.Select(Factory).Select(opt => opt.IfNone(() => throw …))` → `Gen.Select(Factory).Where(opt => opt.IsSome).Select(opt => opt.IfNone(default!))`

The `throw` form converts rejected generated values into property failures instead of filtered generation. `WhereLimit` applies to `Gen.Where`; keep `Where` predicates broad enough to preserve shrinkable satisfying values.

`Try.lift` pattern for factories returning `Fin`:

```csharp
public static readonly Gen<Dimension> Dimension =
    SmallDimension
        .Select(value => Vectors.Dimension.TryCreate(value: value, obj: out Vectors.Dimension d) ? Some(d) : None)
        .Where(opt => opt.IsSome)
        .Select(opt => opt.IfNone(default(Vectors.Dimension)));
```

CsCheck `Where` semantics are package API behavior; project generators keep predicates broad enough to preserve shrinkable satisfying values.

## [5][ENV_KNOBS]

| [INDEX] | [VAR]                | [DEFAULT]       | [USE]                                                                                      |
| :-----: | -------------------- | --------------- | ------------------------------------------------------------------------------------------ |
|   [1]   | `CsCheck_Iter`       | 100             | Per-property iteration count.                                                              |
|   [2]   | `CsCheck_Time`       | -1              | Wall-clock budget in seconds; package default disables time override.                      |
|   [3]   | `CsCheck_Threads`    | processor count | Parallel sample workers.                                                                   |
|   [4]   | `CsCheck_Seed`       | unset           | Fixed seed for reproducible runs.                                                          |
|   [5]   | `CsCheck_Replay`     | 100             | Number of times to replay a failing seed for parallel reproduction.                        |
|   [6]   | `CsCheck_Sigma`      | 6.0             | Statistical significance for `Check.Faster`.                                               |
|   [7]   | `CsCheck_Timeout`    | 60              | Per-sample timeout (seconds).                                                              |
|   [8]   | `CsCheck_Ulps`       | 4               | Package floating equality slack; project tolerance lives in project assertion wrappers.    |
|   [9]   | `CsCheck_WhereLimit` | 100             | Filter rejection cap; lower → fail faster, higher → tolerate sparse-acceptance generators. |

`Spec.ForAll` precedence: explicit args > env vars > package defaults. CI policy: tune `CsCheck_Iter=1000` and `CsCheck_Time=60` for nightly extended runs; keep PR validation at defaults. `CsCheck_Replay=10` for CI, default 100 for local repro.

## [6][MODEL_BASED]

`Check.SampleModelBased` takes `Gen<(Actual, Model)>` plus `GenOperation<Actual, Model>` operations, applies a generated operation sequence, and asserts actual/model equivalence after transitions. `SampleModelBasedAsync` uses `Gen<Task<(Actual, Model)>>` plus `GenOperationAsync<Actual, Model>`. Use when:

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

## [7][DISTRIBUTION_AUDIT]

`Check.ChiSquared(expected, actual, sigma)` audits a generator's distribution against expected frequencies. Expected bucket counts must all be greater than `5`; expected and actual arrays must have the same length. Sigma defaults to `6`. Use when:

- An edge-biased `Gen.Frequency` ships with 90/10 weights — verify the tail actually fires 10% of the time across 10k samples.
- A factory-routed generator filters >20% of inputs — verify the surviving distribution isn't skewed away from the boundary cases the test depends on.
- A SmartEnum case sweep with case-specific frequencies — verify rarely-fired cases still fire often enough to exercise their per-case oracle in `Spec.Cases`.

Audit lives in the project testkit generator self-test, not in product spec files.

## [8][PARALLEL_CHECKS]

`Gen<T>.SampleParallel` runs generated operations concurrently and asserts a linearizable history exists. `Spec.ConcurrentProfiled` wraps this and emits `Causal.Profile` output to `ITestOutputHelper`. Use for:

- `Atom<T>` swap contention (closure-free `applyScratch` patterns).
- Process-static cache races (`ConcurrentDictionary` + `[ThreadStatic]`).
- `SmartEnum.Items` lazy initialization under first-access concurrency.

Replay `SampleParallel` failures with the emitted parallel seed, including any thread-id bracket suffix. Do not translate a parallel failure into a plain `Check.Sample` seed unless the package output says the sequential sample also fails.

CsCheck causal profiling output is replay material for the parallel check that emitted it.

## [9][PERFORMANCE_ASSERTIONS]

`Check.Faster(faster, slower, sigma)` compares two implementations across generated inputs and reports confidence that the first implementation is faster. Use only for:

- Regression detection for an allocation or complexity path where the input size is part of the oracle.
- A/B-style algorithmic choice during refactor on the same generator and project-owned tolerance policy.

Never use for absolute performance claims — those belong in `tests/csharp/_benchmarks` with BenchmarkDotNet (see `docs/testing-libs/benchmarkdotnet/api.md`).
