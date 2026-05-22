# [H1][SYSTEM_API]
>**Dictum:** *Use the strongest built-in API before creating local string, collection, numeric, or runtime machinery.*

<br>

[IMPORTANT] Baseline: .NET 10 SDK `10.0.103`, Microsoft Learn, NuGet V3 metadata, local package XML, and RhinoWIP app-bundle artifacts.

---
## [1][TEXT_AND_REGEX]
>**Dictum:** *Text APIs split between structural grammar and character-set search.*

<br>

| [INDEX] | [USING] | [SURFACE] | [USE] | [RASM_RULE] |
| :-----: | ------- | --------- | ----- | ----------- |
| [1] | `System.Text.RegularExpressions` | `[GeneratedRegex]` | Static structural grammar with known pattern. | Prefer over `new Regex(...)` and `RegexOptions.Compiled`. |
| [2] | `System.Text.RegularExpressions` | `Regex` static methods | Dynamic or one-off patterns. | Keep at boundary; avoid per-call construction. |
| [3] | `System.Buffers` | `SearchValues<char>` | Fast allow/deny character sets. | Prefer over regex for simple symbol, path, and key scanning. |
| [4] | `System.StringComparison` | Ordinal/invariant comparisons | Keys, protocol tokens, paths, and command flags. | Always choose comparison explicitly. |
| [5] | `System.Globalization` | `CultureInfo.InvariantCulture` | Stable parse/format. | Required for persisted, command, GH2, and file formats. |

---
## [2][COLLECTIONS]
>**Dictum:** *Immutable, frozen, and pooled collections serve different lifetimes.*

<br>

| [INDEX] | [USING] | [SURFACE] | [USE] | [RASM_RULE] |
| :-----: | ------- | --------- | ----- | ----------- |
| [1] | `System.Collections.Frozen` | `FrozenDictionary`, `FrozenSet` | Static read-mostly lookup built once. | Use for catalogs and derived native-type maps. |
| [2] | `System.Collections.Immutable` | `ImmutableArray`, `ImmutableDictionary` | Evolving immutable state. | Shared framework use needs no package; LanguageExt remains default rail collection. |
| [3] | `System.Buffers` | `ArrayPool<T>` | Temporary hot-path buffers. | Use only with measured allocation pressure and clear return ownership. |
| [4] | `System.Runtime.InteropServices` | `CollectionsMarshal` | Ref access to dictionary/list internals. | Hot-path boundary only; never expose returned refs. |
| [5] | `System.Linq` | `Enumerable` | Readable outer transformations. | Avoid in hot numeric loops; use spans, MathNet, or TensorPrimitives. |

---
## [3][NUMERICS]
>**Dictum:** *BCL numerics own primitives; MathNet owns algorithms.*

<br>

| [INDEX] | [USING] | [SURFACE] | [USE] | [RASM_RULE] |
| :-----: | ------- | --------- | ----- | ----------- |
| [1] | `System.Numerics` | `Vector2`, `Vector3`, `Quaternion`, `Matrix4x4` | Lightweight primitive math and interpolation support. | Use for local numeric primitives when RhinoCommon is not semantic owner. |
| [2] | `System.Numerics` | Generic math interfaces | Algorithmic constraints over numeric types. | Use when scalar-polymorphic kernels exist. |
| [3] | `System.Numerics.Tensors` | `TensorPrimitives` | Span reductions, distances, dot products, norms. | Add package only after `package-adoption.md` consumer gate passes. |
| [4] | `System.Runtime.Intrinsics` | `Vector128/256/512` | Measured SIMD kernels. | Benchmark first; prefer `TensorPrimitives` when available. |
| [5] | `MathNet.Numerics` | Matrix, solver, stats, interpolation, optimization. | Managed numerical algorithms. | Prefer over hand-rolled algorithm code. |

---
## [4][SERIALIZATION_IO_PATHS]
>**Dictum:** *Path and wire APIs must encode file-kind and culture policy explicitly.*

<br>

| [INDEX] | [USING] | [SURFACE] | [USE] | [RASM_RULE] |
| :-----: | ------- | --------- | ----- | ----------- |
| [1] | `System.Text.Json` | Source generation, converters, `JsonSerializerOptions` | Stable command/tool wire format. | Use source generation for stable tool contracts. |
| [2] | `System.IO` | `File`, `Directory`, `FileInfo`, `DirectoryInfo` | File and directory boundaries. | Do not use `Path.Exists` where file-vs-directory matters. |
| [3] | `System.IO.Enumeration` | `FileSystemEnumerable<T>` | Allocation-sensitive directory scans. | Boundary tooling only. |
| [4] | `System.IO.Hashing` | `XxHash3`, CRC families | Non-cryptographic content or cache keys. | Add package only after `package-adoption.md` consumer gate passes. |
| [5] | `System.Security.Cryptography` | SHA/HMAC APIs | Cryptographic identity. | Use for security or integrity, not fast cache checks. |

---
## [5][TIME_DIAGNOSTICS_CONCURRENCY]
>**Dictum:** *Runtime state must be injectable, monotonic, and observable.*

<br>

| [INDEX] | [USING] | [SURFACE] | [USE] | [RASM_RULE] |
| :-----: | ------- | --------- | ----- | ----------- |
| [1] | `System.TimeProvider` | `TimeProvider` | Testable wall-clock and timers. | Prefer at boundary; NodaTime owns domain time model when adopted. |
| [2] | `System.Diagnostics` | `Stopwatch.GetTimestamp` | Monotonic elapsed time. | Use for UI springs, throttles, benchmarks, and timeouts. |
| [3] | `System.Diagnostics` | `ActivitySource`, `Meter` | Tracing and metrics. | Use at bridge/tool boundaries; keep domain pure. |
| [4] | `System.Threading` | `CancellationToken` | Host cancellation. | Thread through `Eff` runtime records and boundary operations. |
| [5] | `System.Threading.Channels` | `Channel<T>` | Async producer/consumer pipelines. | Boundary infrastructure only, not domain transforms. |

---
## [6][REFLECTION_RUNTIME_INTEROP]
>**Dictum:** *Reflection and interop stay at host edges with typed collapse immediately after.*

<br>

| [INDEX] | [USING] | [SURFACE] | [USE] | [RASM_RULE] |
| :-----: | ------- | --------- | ----- | ----------- |
| [1] | `System.Reflection` | `GetCustomAttribute<T>` | Metadata lookup. | Prefer generic API over manual attribute arrays. |
| [2] | `System.Runtime.InteropServices` | `GuidAttribute`, `StructLayout` | Plugin identity and layout metadata. | Keep in project/host boundary files. |
| [3] | `System.Runtime.CompilerServices` | `CallerArgumentExpression`, `CallerMemberName` | Diagnostics and validation context. | Use only when it improves typed error messages. |
| [4] | `System.Runtime.Versioning` | `SupportedOSPlatform` | macOS/Rhino UI target annotations. | Keep centralized in build props where possible. |
| [5] | `System.Drawing.*` | Bitmap, codec, image formats. | RhinoWIP UI/raster boundary only. | Use central Rhino UI-aware compile package plus host assembly reference; never universal NuGet. |
