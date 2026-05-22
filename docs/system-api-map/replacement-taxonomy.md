# [H1][REPLACEMENT_TAXONOMY]
>**Dictum:** *Refactoring starts by choosing the API owner for the repeated behavior.*

<br>

[IMPORTANT] Replacement map for owning edits. Apply replacements only where Rasm semantics remain intact.

---
## [1][TEXT_AND_PATHS]
>**Dictum:** *Parsing becomes durable when grammar, character policy, and path kind are separate.*

<br>

| [INDEX] | [CURRENT_SMELL] | [APPROVED_REPLACEMENT] | [BOUNDARY] |
| :-----: | --------------- | ---------------------- | ---------- |
| [1] | Hand-rolled structural pattern checks. | `[GeneratedRegex]` partial property or method. | Static known grammar. |
| [2] | Regex for simple allowed/disallowed character sets. | `SearchValues<char>` plus span search. | Hot or repeated text scans. |
| [3] | `Path.Exists` before file IO. | `File.Exists`, `Directory.Exists`, or typed endpoint policy. | Exchange and tooling paths. |
| [4] | Culture-dependent number/date parse. | Invariant parse/format. | Commands, files, GH2 parameters. |
| [5] | `DateTime.MinValue` sentinel. | `Option<T>`, NodaTime, or explicit timestamp policy. | Metadata and runtime events. |

---
## [2][VALIDATION_AND_DISPATCH]
>**Dictum:** *Closed vocabularies carry their lookup, validation, and behavior.*

<br>

| [INDEX] | [CURRENT_SMELL] | [APPROVED_REPLACEMENT] | [BOUNDARY] |
| :-----: | --------------- | ---------------------- | ---------- |
| [1] | Parallel dictionaries beside a smart enum. | Constructor metadata and `Items`-derived lookup. | Thinktecture SmartEnum owners. |
| [2] | Switch tables over known variants. | Thinktecture `Switch`/`Map` or item delegates. | Closed domain or host vocabularies. |
| [3] | Primitive option bags with impossible states. | Thinktecture union or value object. | UI, publish, command, progress, input policy. |
| [4] | Throwing `Of`, `From`, `Get` construction. | `Fin<T>` / `Validation<Error,T>` factory rail. | Public or boundary input. |
| [5] | Expression-tree validation for simple predicates. | Static delegates, smart-enum metadata, or value-object validation hooks. | Domain validation maps. |

---
## [3][NUMERICS]
>**Dictum:** *Numerical code exposes policy and delegates execution to proven kernels.*

<br>

| [INDEX] | [CURRENT_SMELL] | [APPROVED_REPLACEMENT] | [BOUNDARY] |
| :-----: | --------------- | ---------------------- | ---------- |
| [1] | Manual dense matrix solve or factorization. | MathNet factorization and `Solve` APIs. | `Rasm.Vectors`. |
| [2] | LINQ reductions in hot numeric loops. | `TensorPrimitives`, spans, or MathNet vector operations. | Measured hot path only. |
| [3] | Hand-rolled statistics without named estimator policy. | MathNet statistics when semantics match. | Domain stats and analysis outputs. |
| [4] | Quaternion or interpolation names that exceed implementation truth. | `System.Numerics.Quaternion` or renamed/validated operation. | Motion/vector intent. |
| [5] | Densifying sparse algorithms without policy. | Named dense fallback or true sparse solver path. | Sparse vector kernels. |

---
## [4][EFFECTS_AND_RUNTIME]
>**Dictum:** *Runtime work stays effectful until the boundary collapses it.*

<br>

| [INDEX] | [CURRENT_SMELL] | [APPROVED_REPLACEMENT] | [BOUNDARY] |
| :-----: | --------------- | ---------------------- | ---------- |
| [1] | Nested `Fin<Fin<T>>` or manual rail unwrap. | LanguageExt `Flatten()` when available. | Rail composition. |
| [2] | Resource cleanup around native projections. | `Bracket`, `Resources`, scoped projection, or owning callback. | Rhino/GH resources. |
| [3] | Shared mutable boundary state. | `Atom<T>`, `StrongBox<T>`, or closure-owned mutation with boundary comment. | UI/event adapters. |
| [4] | Direct `DateTime.UtcNow` for elapsed behavior. | `Stopwatch.GetTimestamp`, `TimeProvider`, or NodaTime clock. | UI timing and tools. |
| [5] | Retry/timeout loops around host/process work. | Polly policy wrapped back into `Fin`/`Eff`. | Bridge, IO, process, network. |

---
## [5][OBSERVABILITY_AND_CONFIG]
>**Dictum:** *Operational concerns belong at composition and tool boundaries.*

<br>

| [INDEX] | [CURRENT_SMELL] | [APPROVED_REPLACEMENT] | [BOUNDARY] |
| :-----: | --------------- | ---------------------- | ---------- |
| [1] | Ad hoc process logs. | Serilog or `ILogger` with OpenTelemetry export. | Tools, bridge, app hosts. |
| [2] | Untyped JSON settings. | Microsoft.Extensions.Options with validation before domain entry. | CLI and bridge config. |
| [3] | Unmeasured performance rewrites. | BenchmarkDotNet proof before adopting SIMD/pooling. | Numeric and geometry hot paths. |
| [4] | Cross-cutting service registration lists. | Scrutor scan/decorator composition. | Composition roots only. |
| [5] | Mock-heavy tests for pure domain logic. | CsCheck/xUnit property and law checks. | Existing test stack. |
