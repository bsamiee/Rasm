# [CSHARP_TESTING_API_CSCHECK]

`CsCheck` is the random-testing engine the TestKit composes: PCG-seeded generation with size-ordered shrinking, property sampling, model-based and metamorphic state testing, linearizability checking, and chi-squared distribution gates. It is zero-dependency pure BCL; `Directory.Build.props` injects it into test and kit projects with `PrivateAssets="all"` and a global `Using Include="CsCheck"`, and the kit's `Gens` bands build on the `Gen` factories while the `Spec` owner routes every sample family, so specs compose kit gates instead of calling `Check` raw.

## [01]-[PACKAGE_SURFACE]

- package: `CsCheck` `4.7.0`
- license: `Apache-2.0`
- namespace: `CsCheck`
- asset: `lib/net8.0/CsCheck.dll` (net10 consumers bind via compat); zero dependencies
- rail: evidence — generation, shrinking, and stateful/parallel sampling behind the kit `Spec`/`Gens`/`Laws` owners

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                | [KIND]          | [CAPABILITY]                                                               |
| :-----: | :-------------------------------------- | :-------------- | :------------------------------------------------------------------------- |
|  [01]   | `Gen` / `Gen<T> : IGen<T>`              | factory + owner | typed generator algebra: primitives, collections, combinators              |
|  [02]   | `Check`                                 | static          | samples: property, model-based, metamorphic, parallel, chi-squared, Faster |
|  [03]   | `GenOperation<T>`                       | op carrier      | named single-type state ops for parallel sampling                          |
|  [04]   | `GenOperation<Actual, Model>`           | op carrier      | named actual-vs-model ops for model-based sampling                         |
|  [05]   | `GenMetamorphic<T>`                     | op carrier      | paired-path operations for metamorphic sampling                            |
|  [06]   | `Size` / `PCG`                          | engine          | size-ordered shrink metric and the seeded RNG                              |
|  [07]   | `Classifier` / `MedianEstimator`        | statistics      | sample classification buckets and streaming median estimates               |
|  [08]   | `Dbg` / `Causal` / `Hash : IRegression` | diagnostics     | debug regions, causal profiling, regression hashing                        |
|  [09]   | `CsCheckException`                      | exception       | the property-failure carrier xunit surfaces                                |

## [03]-[ENTRYPOINTS]

Every `Sample*` method takes the shared run tail `string? seed = null, long iter = -1, int time = -1, int threads = -1`; rows carry only the distinguishing arguments, and the fence gives the full `Sample`/`SampleParallel`/`ChiSquared` signatures.

| [INDEX] | [SURFACE]                                          | [KIND]          | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `gen.Sample(Action<T> assert, ...)`                | property        | sample with shrink; predicate, classify, tuple forms mirror  |
|  [02]   | `gen.SampleModelBased(operations, equal, ...)`     | stateful        | actual-vs-model equivalence over generated op sequences      |
|  [03]   | `gen.SampleMetamorphic(operations, equal, ...)`    | metamorphic     | two paths through one system converge                        |
|  [04]   | `gen.SampleParallel(operations, ...)`              | linearizability | concurrent op interleavings vs sequential orders             |
|  [05]   | `Check.ChiSquared(...)`                            | distribution    | frequency-bucket conformance gate                            |
|  [06]   | `Gen.Frequency` / `Gen.OneOf` / `Gen.OneOfConst`   | combinator      | weighted and alternative selection over generators/constants |
|  [07]   | `Gen.Const` / `Gen.Shuffle`                        | combinator      | constant generator and permutation                           |
|  [08]   | `Gen.Select` / `Gen.SelectMany`                    | combinator      | map and monadic bind                                         |
|  [09]   | `Gen.Double[start, finish]` / `Gen.Int[...]`       | indexer         | ranged double and int generation                             |
|  [10]   | `Gen.Char[...]` / `.Array[...]` / `.List[...]`     | indexer         | ranged char and sized collection generation                  |
|  [11]   | `GenOperation.Create` / `GenOperationAsync.Create` | builder         | named sync/async operation construction                      |
|  [12]   | `GenMetamorphic.Create`                            | builder         | named metamorphic operation construction                     |

```csharp signature
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
