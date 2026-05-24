# [H1][CSCHECK_API]
>**Dictum:** *CsCheck owns search, shrinking, replay, and stateful pressure.*

<br>

[IMPORTANT] Rasm pins `CsCheck 4.7.0`. This page maps the package API surface that specs may use through `Rasm.TestKit`; module specs should prefer `Spec.ForAll`, `Spec.Metamorphic`, and shared generators over raw `Check.Sample` calls.

---
## [1][PACKAGE_TRUTH]
>**Dictum:** *The version matters because CsCheck evolves quickly.*

<br>

| [INDEX] | [FACT] | [VALUE] |
| :-----: | ------ | ------- |
| [1] | Current pin | `CsCheck 4.7.0` |
| [2] | Target | `net8.0` package, compatible with `net10.0` projects |
| [3] | New 4.7.0 note | `SampleModelBasedAsync` added |
| [4] | Core model | PCG-based generation and automatic shrinking |

[SOURCE] NuGet package page and README: https://www.nuget.org/packages/CsCheck/4.7.0

---
## [2][CHECK]
>**Dictum:** *Sampling is configurable; wrappers make it consistent.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `Check.Iter`, `Time`, `Replay`, `Threads`, `Seed`, `Sigma`, `Timeout`, `Ulps`, `WhereLimit` | Global defaults. Rasm wrappers honor explicit args first, then `CsCheck_*` env vars, then these defaults. |
| [2] | `Check.Sample` | Action, predicate, and classifier overloads for tuple arities through eight. |
| [3] | `Check.SampleAsync` | Async action/predicate/classifier overloads for tuple arities through eight. |
| [4] | `Check.SampleModelBased` | Stateful actual-vs-model operations. |
| [5] | `Check.SampleModelBasedAsync` | Async stateful actual-vs-model operations added in 4.7.0. |
| [6] | `Check.SampleMetamorphic` | Two-path metamorphic testing over generated `GenMetamorphic<T>`. |
| [7] | `Check.SampleParallel` | Concurrent operation shrinking for actual-only or actual/model operations. |
| [8] | `Check.ChiSquared` | Distribution checks for generator quality. |
| [9] | `Check.Faster` / `FasterAsync` | Statistical performance comparison. Keep opt-in and local. |
| [10] | `Check.Single` | Generate one satisfying sample. |
| [11] | `Check.Equality`, `Equal`, `EqualUnordered`, `EqualSkip`, `ModelEqual`, `AreClose`, `UlpsBetween` | Built-in oracle helpers. Prefer `Spec.EqualWithin` when test labels matter. |
| [12] | `Check.Hash` | Hash-regression API. Do not assume a cache file location unless verified in the current package/source. |
| [13] | `Check.Print` | Stable rendering for primitive and generic values. |

---
## [3][GEN_CORE]
>**Dictum:** *Generators should describe domains, not implementation branches.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `Gen<T>` | Generator instance; exposes `Array`, `Array2D`, `List`, `HashSet`, `ArrayUnique`. |
| [2] | `Gen.Const`, `Const(Func<T>)` | Fixed values and lazily-created values. |
| [3] | `Gen.Select` | Map and zip generators; overloads support tuple/value arities through eight. |
| [4] | `Gen.SelectMany` | Dependent generation, LINQ query syntax, generator fan-out. |
| [5] | `Gen.Where` | Domain filtering; keep predicates cheap and high-yield. |
| [6] | `Gen.OneOfConst`, `OneOf`, `FrequencyConst`, `Frequency` | Exhaustive cases and weighted case bias. |
| [7] | `Gen.Enum<T>` | CLR enum generation; Rasm SmartEnums usually need `OneOfConst`. |
| [8] | `Gen.Recursive` | Recursive structures with optional depth and `GenMap<T>`. |
| [9] | `Gen.Clone` | Duplicate generator stream when a metamorphic law needs identical input. |
| [10] | `Gen.Shuffle`, `ShuffleSelect` | Permutation and selection laws. |
| [11] | `Gen.Nullable`, `Null` | Null injection for boundary adapters. |

---
## [4][PRIMITIVES_COLLECTIONS]
>**Dictum:** *Choose bounded primitives by default.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `Gen.Bool`, integer families, `UInt4` through `UInt2048` | Boolean and numeric primitive domains. |
| [2] | `Gen.Int[start, finish]`, `Long[...]`, `Double[...]`, `Decimal[...]` | Bounded domains. |
| [3] | `Gen.Int.Positive`, `NonNegative`, `Uniform`; `Gen.Double.Unit`, `OneTwo`, `Special`; `Gen.Float.*` | Distribution and edge-focused primitive domains. |
| [4] | `Gen.Date`, `DateOnly`, `DateTime`, `DateTimeOffset`, `TimeOnly`, `TimeSpan`, `Guid`, `Char`, `String`, `Seed` | BCL values. |
| [5] | `gen.Array[min,max]`, `Array[n]`, `Array.Nonempty`, `ArrayUnique`, `Array2D.Nonempty` | Collection domains. |
| [6] | `gen.List`, `HashSet`, `Dictionary`, `SortedDictionary` | CLR collection domains. Rasm specs usually project into `Seq<T>`/`Arr<T>` after generation. |

---
## [5][OPERATIONS_AND_PROFILING]
>**Dictum:** *Stateful tests should compare against a smaller model.*

<br>

- `GenOperation<TActual>` and `GenOperation<TActual,TModel>` drive parallel/model-based operations.
- `GenOperationAsync<TActual,TModel>` drives async model-based tests.
- `GenMetamorphic<T>` drives metamorphic transformations.
- `Causal.RegionStart`, `RegionEnd`, and `Causal.Profile` annotate action cost and contention. Use this for shared mutable rails or future cache/concurrent APIs, not for routine unit specs.
- `Hash` and `HashStream` support hash-regression work when a value is too large to assert directly; keep hash use opt-in and documented with the source of the expected value.

---
## [6][RASM_POLICY]
>**Dictum:** *The small wrapper is the contract.*

<br>

- Use `Spec.ForAll` so explicit seed/iter/time/thread args beat env vars and env vars beat CsCheck defaults.
- Use `Spec.Regression` for shrunk seed replay.
- Use `Spec.Metamorphic` for independent oracle paths.
- Use `Spec.ConcurrentProfiled` only when the API is intended to be concurrent.
- Put reusable domain generators in `tests/csharp/_testkit/Gens.cs` only after at least two specs consume them.
