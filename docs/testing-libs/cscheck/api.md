# [H1][CSCHECK_API]
>**Dictum:** *CsCheck searches state space; Rasm supplies independent oracles.*

<br>

[IMPORTANT] Rasm pins `CsCheck 4.7.0`. Specs use `Rasm.TestKit.Spec` first; raw `Check.*` calls move into `_testkit` only after at least two specs need the same rail.

---
## [1][PACKAGE_TRUTH]
>**Dictum:** *The wrapper exposes a smaller contract than the package.*

<br>

| [INDEX] | [FACT] | [VALUE] |
| :-----: | ------ | ------- |
| [1] | Current pin | `CsCheck 4.7.0` |
| [2] | Package TFM | `net8.0`, compatible with Rasm `net10.0` projects |
| [3] | Notable current addition | `SampleModelBasedAsync` |
| [4] | Rasm policy | `Spec.ForAll` owns seed/iter/time/thread precedence |

[SOURCE] NuGet package page: https://www.nuget.org/packages/CsCheck/4.7.0

---
## [2][CHECK_SURFACE]
>**Dictum:** *Promote package APIs through one Rasm contract.*

<br>

| [INDEX] | [API] | [USE] | [RASM_EXPOSURE] |
| :-----: | --- | --- | --- |
| [1] | `Check.Sample` / `SampleAsync` | Generated action, predicate, classifier checks. | `Spec.ForAll`; async only after real consumer. |
| [2] | `Check.SampleModelBased` / `SampleModelBasedAsync` | Actual-vs-smaller-model stateful checks. | Add only with two stateful specs. |
| [3] | `Check.SampleMetamorphic` | `GenMetamorphic<T>` operation rail. | Distinct from current `Spec.Metamorphic`. |
| [4] | `Check.SampleParallel` | Parallel operations and shrinking. | `Spec.ConcurrentProfiled`; needs policy cleanup before broad use. |
| [5] | `Check.Hash` | Hash regression for large stable artifacts. | No cache/path claim without current source proof. |
| [6] | `Check.ChiSquared` | Generator distribution audit. | Testkit generator validation only. |
| [7] | `Check.Faster` / `FasterAsync` | Statistical performance comparison. | Prefer BenchmarkDotNet for benchmark rail. |

---
## [3][GEN_SURFACE]
>**Dictum:** *Generators describe domains, not branches.*

<br>

| [INDEX] | [API] | [RASM_USE] |
| :-----: | --- | ---------- |
| [1] | `Gen.Select`, `SelectMany` | Product axes and dependent generation. |
| [2] | `Gen.OneOfConst`, `OneOf`, `Frequency` | Case tables and edge bias. |
| [3] | `Gen.Array`, `ArrayUnique`, `Array2D`, `List`, `Dictionary` | Collection domains before projection to `Seq<T>`. |
| [4] | `Gen.Shuffle`, `ShuffleSelect` | Permutation and selection metamorphic laws. |
| [5] | `Gen.Recursive` | Recursive structures only with explicit depth discipline. |
| [6] | `Gen.Clone` | Identical streams for two-path comparisons. |

---
## [4][RASM_POLICY]
>**Dictum:** *Every failing sample must reproduce the law.*

<br>

- `Spec.ForAll` precedence: explicit args, then `CsCheck_Seed`, `CsCheck_Iter`, `CsCheck_Time`, `CsCheck_Threads`, then package defaults.
- `Spec.Metamorphic` currently means one generated input checked by path/oracle equality; it is not CsCheck `SampleMetamorphic`.
- `Spec.Regression` records a durable shrunk seed only after product behavior is classified.
- Use model-based APIs only when the model is smaller than production: list log, scalar receipt, set/map reference, or finite state machine.
- Do not promote spec-local generators until two specs share the same domain shape.
