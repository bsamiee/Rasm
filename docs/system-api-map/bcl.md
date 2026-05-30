# [H1][BCL]
>**Dictum:** *Use built-in primitives where they own the concern completely.*

<br>

[IMPORTANT] BCL APIs do not replace LanguageExt rails, Thinktecture shape, MathNet algorithms, Rhino geometry, or GH2 data semantics. C# 14 language features live in `../external-libs/csharp/language.md`; this file owns BCL/shared-framework API surfaces. SDK implicit usings and workspace globals — see `meta.md` §6.

---
## [1][TEXT]
>**Dictum:** *Grammar, character-set policy, culture, and encoding are separate concerns — each gets one BCL owner.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Text.RegularExpressions` | `[GeneratedRegex]` | Stable structural grammar; partial method or property; optional `MatchTimeoutMilliseconds` and `CultureName`; `RegexOptions.Compiled` ignored by generator. |
| [2] | `System.Text.RegularExpressions` | `RegexOptions.NonBacktracking` | Linear-time/ReDoS policy on plain `Regex`; source generator falls back to cached runtime `Regex`, not custom IL. |
| [3] | `System.Text.RegularExpressions` | `Regex.Count`, `Regex.IsMatch` | Match count or boolean check without `MatchCollection` allocation; CA1875 / CA1874. |
| [4] | `System.Buffers` | `SearchValues<T>` | Cached allow/deny sets; pair with `MemoryExtensions.ContainsAny`, `ContainsAnyExcept`, `IndexOfAny`, `IndexOfAnyExcept` on spans; CA1870. |
| [5] | `System` | `MemoryExtensions` | Span tokenization: `Split`, `SplitAny`, `EnumerateLines`, `Contains`, `Trim` on `ReadOnlySpan<char>`. |
| [6] | `System.Text` | `CompositeFormat` | Parse repeated format strings once; pass instance to `string.Format` / `StringBuilder.AppendFormat`; CA1863. |
| [7] | `System.Buffers.Text` | `Utf8Parser`, `Utf8Formatter` | Try-parse/try-format primitives to `Span<byte>` on UTF-8 wire paths. |
| [8] | `System` | `StringComparison`, `StringComparer`, `string.Create` | Scalar equality vs collection key policy; invariant formatting via `string.Create(CultureInfo.InvariantCulture, …)`. |
| [9] | `System.Globalization` | `CultureInfo`, `NumberFormatInfo` | Invariant for round-trip persistence and wire formats; `CurrentCulture` only at human-facing UI boundaries. |
| [10] | `System.Text` | `Encoding`, `UTF8Encoding` | Bytes ↔ text at boundaries; use span overloads. |
| [11] | `System.Text.Json.Serialization` | `[JsonSerializable]`, `JsonSerializerContext`, `[JsonSourceGenerationOptions]` | Compile-time metadata; `partial` context; `Default` + `JsonTypeInfo<T>` resolver. |
| [12] | `System.Text.Json` | `SerializeToUtf8Bytes`, `Deserialize(ReadOnlySpan<byte>, JsonTypeInfo<T>)` | Avoid UTF-16 `string` round-trip on wire/config JSON. |
| [13] | `System.Text` | `Rune`, `Ascii` | Scalar Unicode validation; strict ASCII wire checks via `Ascii.IsValid` / case transforms. |
| [14] | `System.Buffers.Text` | `Base64Url` | URL-safe base64 encode/decode to spans (tokens, query params). |
| [15] | `System` | `Convert.ToHexString`, `ToHexStringLower` | Hex wire encoding; CA1872. |
| [16] | `System` | `IUtf8SpanParsable<T>`, `IUtf8SpanFormattable` | Uniform UTF-8 try-parse/format at wire boundaries (`Guid`, `IPAddress`, numerics). |
| [17] | `System.Net` | `IPAddress.TryParse(ReadOnlySpan<char>)`, `IsValidUtf8` | Span/UTF-8 IP parsing without allocating `string`. |
| [18] | `System.Text.Unicode` | `UnicodeRanges` | Composed allowed-code-point policy beside `Rune` and `SearchValues<char>`. |

Charset scans → `SearchValues` + span extensions. Structural grammar → `[GeneratedRegex]`.

---
## [2][COLLECTIONS]
>**Dictum:** *Collection lifetime and comparer policy determine the owner.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Collections.Frozen` | `FrozenDictionary`, `FrozenSet`, `ToFrozenDictionary`, `ToFrozenSet`, `GetAlternateLookup<ReadOnlySpan<char>>` | Process-static read-mostly lookup; explicit `comparer:`; span alternate keys for string catalogs. |
| [2] | `System.Collections.Immutable` | `ImmutableDictionary`, `ImmutableArray`, `ImmutableHashSet`, builders | Snapshot or boundary payload outside LanguageExt rails; not default domain public identity. |
| [3] | `System.Collections.Generic` | `OrderedDictionary<K,V>` (.NET 9; index APIs .NET 10) | Insertion-ordered boundary map — not LRU eviction. |
| [4] | `System.Collections.Generic` | `PriorityQueue<TElement,TPriority>` | Best-first scheduling heap at algorithm/boundary. |
| [5] | `System.Linq` | `ToLookup`, `ILookup<TKey,TElement>` | One-shot grouped index; avoid repeated `GroupBy` materialization. |
| [6] | `System.Linq` | `DistinctBy`, `CountBy`, `AggregateBy`, `ExceptBy`, `IntersectBy`, `UnionBy` | Cold-path dedup and keyed set algebra; explicit `IEqualityComparer` when key policy ≠ default. |
| [7] | `System.Runtime.InteropServices` | `CollectionsMarshal.GetValueRefOrAddDefault` | Measured hot-path dictionary builds; `AsSpan` for trusted `List<T>` only. |
| [8] | LanguageExt | `HashMap`, `Seq`, `HashSet` | Default domain collections — see `../external-libs/languageext/collections.md`. |

Collection expressions (`[]`, spread) are a C# owner (`../external-libs/csharp/language.md`); BCL targets may be expression-built then frozen via `ToFrozenDictionary`/`ToFrozenSet`.

---
## [3][EQUALITY]
>**Dictum:** *Equality policy is chosen at construction; hash follows comparer contract.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Collections.Generic` | `EqualityComparer<T>.Default` | Default typed equality for non-branded types. |
| [2] | `System.Collections.Generic` | `EqualityComparer<T>.Create` | Ad-hoc comparer without subclassing — non-branded tooling only. |
| [3] | `System` | `HashCode.Combine`, `HashCode.Add`, `ToHashCode()` | Custom `GetHashCode`; `Add(value, IEqualityComparer<T>)` when hash must match comparer. |
| [4] | C# / `System` | `record`, `record struct` | Structural data carriers; compiler delegates per-field equality. |
| [5] | `System.Runtime.CompilerServices` | `RuntimeHelpers.GetHashCode(object)` | Identity keys for live host objects; not structural equality. |
| [6] | Thinktecture | `[KeyMemberEqualityComparer]`, `[KeyMemberComparer]` | Branded domain admission — see `../external-libs/thinktecture/objects.md`. |

`HashCode` output is process-randomized — never persist or use as stable file keys. Records use compiler-synthesized equality; Thinktecture overrides synthesis on branded members.

---
## [4][NUMERICS]
>**Dictum:** *Primitive numeric kernels do not own geometry or algorithms.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | RhinoCommon | Geometry, units, tolerances, transforms, topology | Model-space truth — see `../external-libs/mathnet/rhino.md`. |
| [2] | `System` | `Math`, `MathF`, `Half` | Scalar math; `Half` for compact wire/GPU interchange at boundary. |
| [3] | `System.Numerics` | `Complex` | Double complex scalar; bridge to MathNet at algorithm boundary. |
| [4] | `System.Numerics` | `INumberBase<T>`, `INumber<T>`, `IBinaryInteger<T>`, `IFloatingPointIeee754<T>`, `IShiftOperators<,,>` | Narrowest generic-math constraint that admits required static members. |
| [5] | `System.Numerics` | `Vector2/3/4`, `Matrix3x2`, `Matrix4x4`, `Quaternion` | Lightweight structs for 2D layout and wire projection at boundaries. |
| [6] | `System.Numerics` | `Vector<T>`, static `Vector.*`, `Vector.IsHardwareAccelerated` | Portable SIMD over contiguous spans; gate explicit intrinsics on hardware acceleration. |
| [7] | `System.Runtime.Intrinsics` | `Vector128/256/512<T>`, `Shuffle`, `ShuffleNative`, ISA subnamespaces | Explicit width when portable SIMD is insufficient; never global-import. |
| [8] | `System.Numerics.Tensors` | `TensorPrimitives`, `Tensor<T>`, `TensorSpan<T>` | Platform package — explicit `PackageReference` on adoption; BenchmarkDotNet proof; see `packages.md` row [10]. |
| [9] | MathNet | Matrices, solvers, statistics, symbolic math | See `../external-libs/mathnet/`. |

**Name collisions:** BCL `Vector<T>` (SIMD) ≠ MathNet `LinearAlgebra.Vector<T>`; BCL `Complex` (double) ≠ MathNet `Complex32`.

Do not adopt `System.Numerics.Tensors` in production until `packages.md` records a measured consumer.

---
## [5][RUNTIME]
>**Dictum:** *Runtime state stays injectable, monotonic, and observable.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System` | `TimeProvider`, `PeriodicTimer` | Injectable clock and async tick loops at boundaries; `PeriodicTimer` not in domain (CSP0401). |
| [2] | `System.Diagnostics` | `Stopwatch.GetTimestamp`, `Stopwatch.GetElapsedTime` | Allocation-free monotonic timing when injection is unnecessary. |
| [3] | `System.Diagnostics` | `ActivitySource`, `Activity` | Distributed spans; W3C trace context; bracket-disposed lifecycle; CSP0604 centralization. |
| [4] | `System.Diagnostics.Metrics` | `Meter`, `Counter<T>`, `Histogram<T>`, `UpDownCounter<T>`, `ObservableGauge<T>`, `TagList`, `InstrumentAdvice` | Process metrics; no standalone synchronous `Gauge<T>`; histogram buckets via `InstrumentAdvice`. |
| [5] | `Microsoft.Extensions.Logging` | `[LoggerMessage]`, `ILogger` | Structured logs — requires `Microsoft.Extensions.Logging.Abstractions` package on net10.0; first-consumer candidate in `packages.md`. |
| [6] | `System.Threading` | `CancellationToken`, `CreateLinkedTokenSource`, `CancelAfter` | Host cancellation and deadline-scoped operations. |
| [7] | `System.Threading` | `Lock`, `Lock.EnterScope()` | Sync boundary gates; never hold across `await`. |
| [8] | `System.Threading` | `Interlocked`, `Volatile` | Lifecycle gates and ref-count — not value deduplication. |
| [9] | `System.Threading.Channels` | `Channel<T>`, `BoundedChannelOptions` | Bounded producer/consumer; explicit `using System.Threading.Channels`; CSP0404/0405. |
| [10] | `System.Threading.Tasks` | `TaskCompletionSource<T>`, `ConfigureAwait(false)`, `Task.WhenAll` | Async handoff with `RunContinuationsAsynchronously`; library continuation policy. |
| [11] | `System.Collections.Generic` | `IAsyncEnumerable<T>` + `[EnumeratorCancellation]` | Streaming boundary APIs; CSP0608. |
| [12] | `System.Threading.RateLimiting` | `TokenBucketRateLimiter`, `PartitionedRateLimiter` | Throttle bridge/operator traffic — not domain timers. |

Reject for new code: `DiagnosticSource`, `TraceSource` (legacy spans), `Debug.Assert` for domain invariants (use typed rails or `UnreachableException`).

---
## [6][IO_AND_BUFFERS]
>**Dictum:** *File and wire I/O stay at boundaries; buffer lifetime is explicit; spans are the internal contract.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.IO` | `Path.Combine`, `Path.GetFullPath`, `Path.GetRelativePath` | Canonical path algebra; explicit `StringComparison` on macOS. |
| [2] | `System.IO` | `File.OpenHandle`, `RandomAccess` | Offset-based binary I/O via `Read`/`ReadAsync`/`Write`/`WriteAsync` — not `ReadAtOffsetAsync`. |
| [3] | `System.IO` | `FileStreamOptions`, `FileOptions` | Explicit buffer size, async, sequential/random scan, preallocation. |
| [4] | `System.IO` | `Stream.ReadAtLeastAsync`, `ReadExactlyAsync` | Length-prefixed framing without manual read-until-N loops. |
| [5] | `System.IO` | `BinaryReader.ReadExactly(Span<byte>)` | Structured binary parse into caller workspace. |
| [6] | `System.Buffers` | `ArrayPool<T>.Shared`, `MemoryPool<T>.Shared` | Temporary workspaces; `Return(clearArray:)` in `finally`; `IMemoryOwner<T>` across `await`. |
| [7] | `System.Memory` | `ReadOnlySpan<T>`, `Span<T>`, `ReadOnlyMemory<T>`, `Memory<T>` | Internal hot-path contract; C# 14 implicit span conversions per language doc. |
| [8] | `System.Buffers` | `ReadOnlySequence<T>`, `SequenceReader<T>` | Multi-segment parse without flattening to `byte[]`. |
| [9] | `System.Buffers.Binary` | `BinaryPrimitives` | Endian wire integers; frame headers. |
| [10] | `System.IO.Pipelines` | `PipeReader`, `PipeWriter`, `PipeOptions` | Streaming protocol with backpressure and buffer pooling. |
| [11] | `System.IO` | `FileSystemWatcher` | Directory change notifications at boundary; debounce with `TimeProvider`. |
| [12] | `System.IO.MemoryMappedFiles` | `MemoryMappedFile` | Large read-mostly binary maps — not length-prefixed wire framing. |

Validate length-from-wire against a bounded cap before any pool rent or allocation.

---
## [7][CRYPTO_AND_INTEGRITY]
>**Dictum:** *Cryptographic integrity stays at tooling and exchange boundaries.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Security.Cryptography` | `SHA256.HashData`, `IncrementalHash`, `CryptographicOperations.FixedTimeEquals` | Content fingerprints and tamper-evident bundles; bridge `ContentFingerprint` pattern. |
| [2] | `System.Security.Cryptography` | `RandomNumberGenerator.Fill` | Cryptographic random bytes — not `System.Random`. |
| [3] | `System.IO.Hashing` | `XxHash3`, `XxHash64`, `Crc32` | Non-cryptographic cache keys and content IDs — not `SHA256`. |
| [4] | `System.Formats.Asn1` | `AsnReader`, `AsnWriter` | Structured DER when PKI/signing needs ASN.1 — defer until required. |

---
## [8][BOUNDARY_CONTRACTS]
>**Dictum:** *Nullable flow and validation attributes clarify boundary APIs without replacing domain rails.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Diagnostics.CodeAnalysis` | `NotNullWhen`, `NotNullIfNotNull`, `MemberNotNull`, `DoesNotReturn` | NRT flow after `Try*` / guard methods on GH/Rhino boundaries. |
| [2] | `System.Diagnostics.CodeAnalysis` | `StringSyntax`, `DynamicallyAccessedMembers` | Regex/JSON syntax hints; trim annotations only when pursuing NativeAOT. |
| [3] | `System.ComponentModel.DataAnnotations` | `ValidationContext`, `Validator.TryValidateObject`, `[Required]`, `[Range]` | Sync DTO validation at boundary — not Thinktecture/LanguageExt domain admission. |

---
## [9][LINQ_EXTENDED]
>**Dictum:** *LINQ extended operators serve cold in-memory transforms at boundaries — domain traversal stays LanguageExt.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Linq` | `LeftJoin`, `RightJoin` | Outer joins without `GroupJoin` ceremony. |
| [2] | `System.Linq` | `MinBy`, `MaxBy` | Argmin/argmax without full sort. |
| [3] | `System.Linq` | `Chunk`, `Index`, `TryGetNonEnumeratedCount` | Batching, indexed enumeration, avoid forced full counts. |
| [4] | `System.Linq` | `Shuffle` | In-memory random order — tests use CsCheck `Gen.Shuffle` instead. |

---
## [10][COMPILER_AND_INTEROP]
>**Dictum:** *Compiler attributes and interop surfaces are opt-in per module, never workspace-global.*

<br>

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE] | [USE] |
| :-----: | ------------------- | --------- | ----- |
| [1] | `System.Runtime.CompilerServices` | `CallerArgumentExpression` | Validation messages without stringly `paramName`. |
| [2] | `System.Runtime.CompilerServices` | `MethodImpl`, `CallerMemberName` | Hot-path inlining; boundary telemetry keys. |
| [3] | `System.Runtime.CompilerServices` | `InterpolatedStringHandler`, `CollectionBuilder` | Custom formatters and collection-expression targets at boundary only. |
| [4] | `System.Runtime.InteropServices` | `StructLayout`, `LayoutKind` | Dense boundary record layout. |
| [5] | `System.Runtime.InteropServices` | `MemoryMarshal` | Unmanaged reinterpret; never on managed-reference spans. |
| [6] | `System.Runtime.InteropServices` | `NativeMemory`, `SuppressGCTransition` | Native alloc and tight P/Invoke; `unsafe` + measured proof only. |

`required` members need no manual `[RequiredMember]` import when using the keyword. ISA intrinsics require explicit subnamespace usings — never workspace-global.

---
## [11][EXPLICIT_USINGS]
>**Dictum:** *SDK implicit usings cover a narrow subset; most BCL owners need file-level or project-global `using`.*

<br>

| [INDEX] | [NAMESPACE] | [SDK_IMPLICIT] | [TYPICAL_POLICY] |
| :-----: | ----------- | -------------- | ---------------- |
| [1] | `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Net.Http`, `System.Threading`, `System.Threading.Tasks` | Yes | Default SDK set for class libraries. |
| [2] | `System.Text`, `System.Text.RegularExpressions`, `System.Buffers`, `System.Buffers.Text`, `System.Buffers.Binary` | No | File-scoped at text/wire boundaries. |
| [3] | `System.Collections.Frozen`, `System.Collections.Immutable` | No | Project-global on catalog-owning libs or file-scoped. |
| [4] | `System.Globalization` | No | File-scoped or project-global when invariant culture is pervasive. |
| [5] | `System.Text.Json`, `System.Text.Json.Serialization` | No | Boundary serialization modules. |
| [6] | `System.Threading.Channels`, `System.IO.Pipelines`, `System.Threading.RateLimiting` | No | Protocol/framing modules only. |
| [7] | `System.Diagnostics.Metrics`, `System.Diagnostics.CodeAnalysis` | No | Observability and contract modules. |
| [8] | `Microsoft.Extensions.Logging` | No | Package-required on net10.0 until adopted. |
| [9] | `System.Numerics`, `System.Numerics.Tensors`, `System.Runtime.Intrinsics.*` | No | Hot-path numeric modules; ISA namespaces never global. |
| [10] | `System.IO.Hashing`, `System.Security.Cryptography`, `System.Formats.Asn1`, `System.Text.Unicode` | No | Integrity/crypto/ASN modules at boundaries. |
| [11] | `System.Runtime.CompilerServices`, `System.Runtime.InteropServices` | No | Per-project globals where density warrants; not transitive across references. |

Workspace LanguageExt/Thinktecture globals — see `meta.md` §6.

---
## [12][DRAWING]
>**Dictum:** *Drawing is a RhinoWIP host-boundary exception on macOS.*

<br>

`System.Drawing.Common` is not a universal dependency. Rasm resolves Rhino UI drawing through RhinoWIP app-bundle assemblies and uses compile-only package metadata only where forwarded `System.Drawing.*` types require it. Runtime drawing claims need RhinoWIP host proof, not NuGet claims alone.
