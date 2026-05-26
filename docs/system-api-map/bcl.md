# [H1][BCL]
>**Dictum:** *Use built-in primitives where they own the concern completely.*

<br>

[IMPORTANT] BCL APIs do not replace LanguageExt rails, Thinktecture shape, MathNet algorithms, Rhino geometry, or GH2 data semantics.

---
## [1][TEXT]
>**Dictum:** *Parsing separates grammar, character policy, and culture.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Text.RegularExpressions` | `[GeneratedRegex]` | Stable structural grammar with explicit timeout and culture. |
| [2] | `System.Buffers` | `SearchValues<char>` | Fast allow/deny character sets. |
| [3] | `System` | `StringComparison` | Explicit key, token, path, and command comparison. |
| [4] | `System.Globalization` | `CultureInfo.InvariantCulture` | Persisted, command, GH2, and file formats. |

---
## [2][COLLECTIONS]
>**Dictum:** *Collection lifetime determines the owner.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Collections.Frozen` | `FrozenDictionary`, `FrozenSet` | Static read-mostly lookup. |
| [2] | `System.Collections.Immutable` | Immutable collections | Evolving immutable state outside LanguageExt rails. |
| [3] | `System.Collections.Generic` | `OrderedDictionary<K,V>` (.NET 10) | Insertion-ordered key/value store at boundary; never inside LanguageExt rails. |
| [4] | `System.Buffers` | `ArrayPool<T>` | Temporary buffers with explicit return ownership. |
| [5] | `System.Runtime.InteropServices` | `CollectionsMarshal` | Measured hot-path internal access only. |

---
## [3][NUMERICS]
>**Dictum:** *Primitive numeric kernels do not own geometry or algorithms.*

<br>

| [INDEX] | [OWNER] | [USE] |
| :-----: | ------- | ----- |
| [1] | RhinoCommon | Model geometry, units, tolerances, transforms, topology. |
| [2] | `System.Numerics` | Pure scalar/vector interop and generic math constraints. |
| [3] | `System.Numerics.Tensors` | Package-gated measured span reductions and distances. |
| [4] | MathNet | Matrices, solvers, statistics, optimization, symbolic math. |

Do not add tensor package references until `packages.md` records a concrete measured consumer.

---
## [4][RUNTIME]
>**Dictum:** *Runtime state stays injectable, monotonic, and observable.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System` | `TimeProvider` | Boundary wall-clock and timers. |
| [2] | `System.Diagnostics` | `Stopwatch.GetTimestamp` | Monotonic elapsed time. |
| [3] | `System.Diagnostics` | `ActivitySource`, `Meter` | Tool, bridge, and host observability. |
| [4] | `System.Threading` | `CancellationToken` | Host cancellation through runtime records. |
| [5] | `System.Threading.Channels` | `Channel<T>` | Boundary producer/consumer infrastructure. |

---
## [5][DRAWING]
>**Dictum:** *Drawing is a RhinoWIP host-boundary exception on macOS.*

<br>

`System.Drawing.Common` is not a universal dependency. Rasm resolves Rhino UI drawing through RhinoWIP app-bundle assemblies and uses compile-only package metadata only where forwarded `System.Drawing.*` types require it. Runtime drawing claims need RhinoWIP host proof, not NuGet claims alone.
