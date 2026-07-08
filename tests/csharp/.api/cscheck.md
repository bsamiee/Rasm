# [cscheck] — the property engine under the kit's Spec gates and Gens bands

`CsCheck` is the random-testing engine the TestKit composes: PCG-seeded generation with size-ordered shrinking, property sampling, model-based and metamorphic state testing, linearizability checking, and chi-squared distribution gates. It is zero-dependency pure BCL; `Directory.Build.props` injects it into test and kit projects with `PrivateAssets="all"` and a global `Using Include="CsCheck"`. The kit's `Gens` bands build on the `Gen` factories and the `Spec` owner routes every sample family, so specs compose kit gates instead of calling `Check` raw.

## [01]-[PACKAGE_SURFACE]

- package: `CsCheck` `4.7.0`
- license: `Apache-2.0`
- namespace: `CsCheck`
- asset: `lib/net8.0/CsCheck.dll` (net10 consumers bind via compat); zero dependencies
- rail: evidence — generation, shrinking, and stateful/parallel sampling behind the kit `Spec`/`Gens`/`Laws` owners

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                          | [KIND]          | [CAPABILITY]                                                                          |
| :-----: | :------------------------------------------------ | :-------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Gen` / `Gen<T> : IGen<T>`                        | factory + owner | typed generator algebra; every primitive, collection, and combinator                  |
|  [02]   | `Check`                                           | static          | the sample surface: property, model-based, metamorphic, parallel, chi-squared, Faster |
|  [03]   | `GenOperation<T>` / `GenOperation<Actual, Model>` | op carrier      | named state operations for model-based and parallel sampling                          |
|  [04]   | `GenMetamorphic<T>`                               | op carrier      | paired-path operations for metamorphic sampling                                       |
|  [05]   | `Size` / `PCG`                                    | engine          | size-ordered shrink metric and the seeded RNG                                         |
|  [06]   | `Classifier` / `MedianEstimator`                  | statistics      | sample classification buckets and streaming median estimates                          |
|  [07]   | `Dbg` / `Causal` / `Hash : IRegression`           | diagnostics     | debug regions, causal profiling, regression hashing                                   |
|  [08]   | `CsCheckException`                                | exception       | the property-failure carrier xunit surfaces                                           |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                                     | [KIND]          | [CAPABILITY]                                                          |
| :-----: | :-------------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `gen.Sample(Action<T> assert, ..., string? seed, long iter, int time, int threads)`           | property        | sample with shrink; predicate, classify, and 2-8 tuple forms mirror   |
|  [02]   | `gen.SampleModelBased(operations, equal, ...)`                                                | stateful        | actual-vs-model equivalence under generated operation sequences       |
|  [03]   | `gen.SampleMetamorphic(operations, equal, ...)`                                               | metamorphic     | two paths through one system converge                                 |
|  [04]   | `gen.SampleParallel(operations, ..., maxSequentialOperations, maxParallelOperations, replay)` | linearizability | concurrent op interleavings check against sequential orders           |
|  [05]   | `Check.ChiSquared(int[] expected, int[] actual, double sigma)`                                | distribution    | frequency-bucket conformance gate                                     |
|  [06]   | `Gen.Frequency` / `OneOf` / `OneOfConst` / `Const` / `Shuffle` / `Select` / `SelectMany`      | combinator      | weighted, alternative, constant, permutation, and monadic composition |
|  [07]   | `Gen.Double[start, finish]` / `Gen.Int[...]` / `Gen.Char[...]` / `.Array[...]` / `.List[...]` | indexer         | ranged scalar and sized collection generation                         |
|  [08]   | `GenOperation.Create` / `GenOperationAsync.Create` / `GenMetamorphic.Create`                  | builder         | named operation construction for the stateful families                |

```csharp contract
public static void Sample<T>(this Gen<T> gen, Action<T> assert, Action<string>? writeLine = null,
    string? seed = null, long iter = -1, int time = -1, int threads = -1,
    Func<T, string>? print = null, ILogger? logger = null);
public static void SampleParallel<T>(this Gen<T> initial, GenOperation<T>[] operations,
    Func<T, T, bool>? equal = null, string? seed = null,
    int maxSequentialOperations = 10, int maxParallelOperations = 5,
    long iter = -1, int time = -1, int threads = -1,
    Func<T, string>? print = null, int replay = -1, Action<string>? writeLine = null);
public static void ChiSquared(int[] expected, int[] actual, double sigma = 6.0);
```

## [04]-[IMPLEMENTATION_LAW]

[ENGINE]: generation and shrinking both ride PCG + `Size` — every draw carries an ordered size, and shrinking re-draws smaller-size candidates below the failing size. Shrinks are seed-reproducible, continuable across runs, and parallelized with sampling; there are no `Arb` classes — composition is `Select`/`SelectMany`/`Where` over `Gen<T>`. Concurrency testing IS `SampleParallel`; no other concurrent sampler exists.

[CONFIG]: `Check` statics carry the run defaults, each overridable per call (`-1`/null defers to the static) and by environment: `Check.Iter` (100, `CsCheck_Iter`), `Check.Time` (off, `CsCheck_Time` — seconds, overrides iter), `Check.Threads` (processor count, `CsCheck_Threads`), `Check.Seed` (`CsCheck_Seed` — failures print the reproducing seed), `Check.Sigma` (6.0, `CsCheck_Sigma`), `Check.Replay` (100, `CsCheck_Replay`), `Check.Timeout` (60, `CsCheck_Timeout`), `Check.Ulps` (4, `CsCheck_Ulps`).

[STACKING]:
- `xunit.v3` (`xunit-v3.md`): no dependency edge — a failed property throws `CsCheckException` inside a `[Fact]` body; the kit `Spec` owner is the composition point.
- `Rasm.TestKit`: `Gens` owns the magnitude-stratified and geometry band vocabulary over the `Gen` factories; `Spec.ForAll`/`ModelBased`/`Metamorphic`/`Parallel`/`Classified`/`Distributed` route the sample families; `Laws` composes `Shuffle`/`Select`/`SelectMany` for algebraic law rows.
- `coverlet.MTP` (`coverlet-mtp.md`): no hook — coverage instruments the SUT the sample bodies exercise.

[LOCAL_ADMISSION]:
- Specs reach the engine through kit gates; a spec calling `Check.Sample` directly instead of `Spec.ForAll`/`Hold` re-derives seed, print, and rail policy the kit owns.
- `seed`/`iter`/`time`/`threads` pass as named arguments with the `-1` sentinel deferring to environment policy.

[RAIL_LAW]:
- Package: `CsCheck`
- Owns: random generation, shrinking, stateful/metamorphic/parallel sampling, and distribution gates for every C# property spec.
- Accept: kit-routed sampling; env-var run scaling in CI; seed replay from failure output.
- Reject: a second property-testing engine, hand-rolled RNG loops in specs, or per-spec shrink logic.
