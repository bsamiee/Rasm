# [BCL]

BCL APIs do not replace LanguageExt rails, Thinktecture shape, MathNet algorithms, Rhino geometry, or GH2 data semantics. C# 14 language features live in `../language/language.md`; this file owns BCL/shared-framework API surfaces. SDK implicit usings and workspace globals — see `meta.md` §6.

## [1][TEXT]

| [INDEX] | [NAMESPACE_OR_TYPE]              | [SURFACE]                                                                      |
| :-----: | -------------------------------- | ------------------------------------------------------------------------------ |
|   [1]   | `System.Text.RegularExpressions` | `[GeneratedRegex]`                                                             |
|   [2]   | `System.Text.RegularExpressions` | `RegexOptions.NonBacktracking`                                                 |
|   [3]   | `System.Text.RegularExpressions` | `Regex.Count`, `Regex.IsMatch`                                                 |
|   [4]   | `System.Buffers`                 | `SearchValues<T>`                                                              |
|   [5]   | `System`                         | `MemoryExtensions`                                                             |
|   [6]   | `System.Text`                    | `CompositeFormat`                                                              |
|   [7]   | `System.Buffers.Text`            | `Utf8Parser`, `Utf8Formatter`                                                  |
|   [8]   | `System`                         | `StringComparison`, `StringComparer`, `string.Create`                          |
|   [9]   | `System.Globalization`           | `CultureInfo`, `NumberFormatInfo`                                              |
|  [10]   | `System.Text`                    | `Encoding`, `UTF8Encoding`                                                     |
|  [11]   | `System.Text.Json.Serialization` | `[JsonSerializable]`, `JsonSerializerContext`, `[JsonSourceGenerationOptions]` |
|  [12]   | `System.Text.Json`               | `SerializeToUtf8Bytes`, `Deserialize(ReadOnlySpan<byte>, JsonTypeInfo<T>)`     |
|  [13]   | `System.Text`                    | `Rune`, `Ascii`                                                                |
|  [14]   | `System.Buffers.Text`            | `Base64Url`                                                                    |
|  [15]   | `System`                         | `Convert.ToHexString`, `ToHexStringLower`                                      |
|  [16]   | `System`                         | `IUtf8SpanParsable<T>`, `IUtf8SpanFormattable`                                 |
|  [17]   | `System.Net`                     | `IPAddress.TryParse(ReadOnlySpan<char>)`, `IsValidUtf8`                        |
|  [18]   | `System.Text.Unicode`            | `UnicodeRanges`                                                                |

Use:
- [1] Stable structural grammar; partial method or property; optional `MatchTimeoutMilliseconds` and `CultureName`; `RegexOptions.Compiled` ignored by generator.
- [2] Linear-time/ReDoS policy on plain `Regex`; source generator falls back to cached runtime `Regex`, not custom IL.
- [3] Match count or boolean check without `MatchCollection` allocation; CA1875 / CA1874.
- [4] Cached allow/deny sets; pair with `MemoryExtensions.ContainsAny`, `ContainsAnyExcept`, `IndexOfAny`, `IndexOfAnyExcept` on spans; CA1870.
- [5] Span tokenization: `Split`, `SplitAny`, `EnumerateLines`, `Contains`, `Trim` on `ReadOnlySpan<char>`.
- [6] Parse repeated format strings once; pass instance to `string.Format` / `StringBuilder.AppendFormat`; CA1863.
- [7] Try-parse/try-format primitives to `Span<byte>` on UTF-8 wire paths.
- [8] Scalar equality vs collection key policy; invariant formatting via `string.Create(CultureInfo.InvariantCulture, …)`.
- [9] Invariant for round-trip persistence and wire formats; `CurrentCulture` only at human-facing UI boundaries.
- [10] Bytes ↔ text at boundaries; use span overloads.
- [11] Compile-time metadata; `partial` context; `Default` + `JsonTypeInfo<T>` resolver.
- [12] Avoid UTF-16 `string` round-trip on wire/config JSON.
- [13] Scalar Unicode validation; strict ASCII wire checks via `Ascii.IsValid` / case transforms.
- [14] URL-safe base64 encode/decode to spans (tokens, query params).
- [15] Hex wire encoding; CA1872.
- [16] Uniform UTF-8 try-parse/format at wire boundaries (`Guid`, `IPAddress`, numerics).
- [17] Span/UTF-8 IP parsing without allocating `string`.
- [18] Composed allowed-code-point policy beside `Rune` and `SearchValues<char>`.


## [2][COLLECTIONS]

| [INDEX] | [NAMESPACE_OR_TYPE]              | [SURFACE]                                                                                                      |
| :-----: | -------------------------------- | -------------------------------------------------------------------------------------------------------------- |
|   [1]   | `System.Collections.Frozen`      | `FrozenDictionary`, `FrozenSet`, `ToFrozenDictionary`, `ToFrozenSet`, `GetAlternateLookup<ReadOnlySpan<char>>` |
|   [2]   | `System.Collections.Immutable`   | `ImmutableDictionary`, `ImmutableArray`, `ImmutableHashSet`, builders                                          |
|   [3]   | `System.Collections.Generic`     | `OrderedDictionary<K,V>` (.NET 9; index APIs .NET 10)                                                          |
|   [4]   | `System.Collections.Generic`     | `PriorityQueue<TElement,TPriority>`                                                                            |
|   [5]   | `System.Linq`                    | `ToLookup`, `ILookup<TKey,TElement>`                                                                           |
|   [6]   | `System.Linq`                    | `DistinctBy`, `CountBy`, `AggregateBy`, `ExceptBy`, `IntersectBy`, `UnionBy`                                   |
|   [7]   | `System.Runtime.InteropServices` | `CollectionsMarshal.GetValueRefOrAddDefault`                                                                   |
|   [8]   | LanguageExt                      | `HashMap`, `Seq`, `HashSet`                                                                                    |

Use:
- [1] Process-static read-mostly lookup; explicit `comparer:`; span alternate keys for string catalogs.
- [2] Snapshot or boundary payload outside LanguageExt rails; not default domain public identity.
- [3] Insertion-ordered boundary map — not LRU eviction.
- [4] Best-first scheduling heap at algorithm/boundary.
- [5] One-shot grouped index; avoid repeated `GroupBy` materialization.
- [6] Cold-path dedup and keyed set algebra; explicit `IEqualityComparer` when key policy ≠ default.
- [7] Measured hot-path dictionary builds; `AsSpan` for trusted `List<T>` only.
- [8] Default domain collections — see `../product-libs/languageext/collections.md`.

Collection expressions (`[]`, spread) are a C# owner (`../language/language.md`); BCL targets may be expression-built then frozen via `ToFrozenDictionary`/`ToFrozenSet`.

## [3][EQUALITY]

| [INDEX] | [NAMESPACE_OR_TYPE]               | [SURFACE]                                            |
| :-----: | --------------------------------- | ---------------------------------------------------- |
|   [1]   | `System.Collections.Generic`      | `EqualityComparer<T>.Default`                        |
|   [2]   | `System.Collections.Generic`      | `EqualityComparer<T>.Create`                         |
|   [3]   | `System`                          | `HashCode.Combine`, `HashCode.Add`, `ToHashCode()`   |
|   [4]   | C# / `System`                     | `record`, `record struct`                            |
|   [5]   | `System.Runtime.CompilerServices` | `RuntimeHelpers.GetHashCode(object)`                 |
|   [6]   | Thinktecture                      | `[KeyMemberEqualityComparer]`, `[KeyMemberComparer]` |

Use:
- [1] Default typed equality for non-branded types.
- [2] Ad-hoc comparer without subclassing — non-branded tooling only.
- [3] Custom `GetHashCode`; `Add(value, IEqualityComparer<T>)` when hash must match comparer.
- [4] Structural data carriers; compiler delegates per-field equality.
- [5] Identity keys for live host objects; not structural equality.
- [6] Branded domain admission — see `../product-libs/thinktecture/objects.md`.

`HashCode` output is process-randomized — never persist or use as stable file keys. Records use compiler-synthesized equality; Thinktecture overrides synthesis on branded members.

## [4][NUMERICS]

| [INDEX] | [NAMESPACE_OR_TYPE]         | [SURFACE]                                                                                              |
| :-----: | --------------------------- | ------------------------------------------------------------------------------------------------------ |
|   [1]   | RhinoCommon                 | Geometry, units, tolerances, transforms, topology                                                      |
|   [2]   | `System`                    | `Math`, `MathF`, `Half`                                                                                |
|   [3]   | `System.Numerics`           | `Complex`                                                                                              |
|   [4]   | `System.Numerics`           | `INumberBase<T>`, `INumber<T>`, `IBinaryInteger<T>`, `IFloatingPointIeee754<T>`, `IShiftOperators<,,>` |
|   [5]   | `System.Numerics`           | `Vector2/3/4`, `Matrix3x2`, `Matrix4x4`, `Quaternion`                                                  |
|   [6]   | `System.Numerics`           | `Vector<T>`, static `Vector.*`, `Vector.IsHardwareAccelerated`                                         |
|   [7]   | `System.Runtime.Intrinsics` | `Vector128/256/512<T>`, `Shuffle`, `ShuffleNative`, ISA subnamespaces                                  |
|   [8]   | `System.Numerics.Tensors`   | `TensorPrimitives`, `Tensor<T>`, `TensorSpan<T>`                                                       |
|   [9]   | MathNet                     | Matrices, solvers, statistics, symbolic math                                                           |

Use:
- [1] Model-space truth — see `../../../usage/README.md` §1 and local RhinoWIP XML.
- [2] Scalar math; `Half` for compact wire/GPU interchange at boundary.
- [3] Double complex scalar; bridge to MathNet at algorithm boundary.
- [4] Narrowest generic-math constraint that admits required static members.
- [5] Lightweight structs for 2D layout and wire projection at boundaries.
- [6] Portable SIMD over contiguous spans; gate explicit intrinsics on hardware acceleration.
- [7] Explicit width when portable SIMD is insufficient; never global-import.
- [8] Platform package — explicit `PackageReference` on adoption; BenchmarkDotNet proof; see `packages.md` row [10].
- [9] See `../product-libs/mathnet/`.

**Name collisions:** BCL `Vector<T>` (SIMD) ≠ MathNet `LinearAlgebra.Vector<T>`; BCL `Complex` (double) ≠ MathNet `Complex32`.

Do not adopt `System.Numerics.Tensors` in production until `packages.md` records a measured consumer.

## [5][RUNTIME]

| [INDEX] | [NAMESPACE_OR_TYPE]             | [SURFACE]                                                          |
| :-----: | ------------------------------- | ------------------------------------------------------------------ |
|   [1]   | `System`                        | `TimeProvider`, `PeriodicTimer`                                    |
|   [2]   | `System.Diagnostics`            | `Stopwatch.GetTimestamp`, `Stopwatch.GetElapsedTime`               |
|   [3]   | `System.Diagnostics`            | `ActivitySource`, `Activity`                                       |
|   [4]   | `System.Diagnostics.Metrics`    | `Meter`, counters/histograms/gauges, `TagList`, `InstrumentAdvice` |
|   [5]   | `Microsoft.Extensions.Logging`  | `[LoggerMessage]`, `ILogger`                                       |
|   [6]   | `System.Threading`              | `CancellationToken`, `CreateLinkedTokenSource`, `CancelAfter`      |
|   [7]   | `System.Threading`              | `Lock`, `Lock.EnterScope()`                                        |
|   [8]   | `System.Threading`              | `Interlocked`, `Volatile`                                          |
|   [9]   | `System.Threading.Channels`     | `Channel<T>`, `BoundedChannelOptions`                              |
|  [10]   | `System.Threading.Tasks`        | `TaskCompletionSource<T>`, `ConfigureAwait(false)`, `Task.WhenAll` |
|  [11]   | `System.Collections.Generic`    | `IAsyncEnumerable<T>` + `[EnumeratorCancellation]`                 |
|  [12]   | `System.Threading.RateLimiting` | `TokenBucketRateLimiter`, `PartitionedRateLimiter`                 |

Use:
- [1] Injectable clock and async tick loops at boundaries; `PeriodicTimer` not in domain (CSP0401).
- [2] Allocation-free monotonic timing when injection is unnecessary.
- [3] Distributed spans; W3C trace context; bracket-disposed lifecycle; CSP0604 centralization.
- [4] Process metrics; no standalone synchronous `Gauge<T>`; histogram buckets via `InstrumentAdvice`.
- [5] Structured logs — requires `Microsoft.Extensions.Logging.Abstractions` package on net10.0; first-consumer candidate in `packages.md`.
- [6] Host cancellation and deadline-scoped operations.
- [7] Sync boundary gates; never hold across `await`.
- [8] Lifecycle gates and ref-count — not value deduplication.
- [9] Bounded producer/consumer; explicit `using System.Threading.Channels`; CSP0404/0405.
- [10] Async handoff with `RunContinuationsAsynchronously`; library continuation policy.
- [11] Streaming boundary APIs; CSP0608.
- [12] Throttle bridge/operator traffic — not domain timers.

Do not use `DiagnosticSource`, `TraceSource`, or `Debug.Assert` for new domain invariants. Use `ActivitySource`, typed rails, or `UnreachableException` according to the owning boundary.

## [6][IO_AND_BUFFERS]

| [INDEX] | [NAMESPACE_OR_TYPE]           | [SURFACE]                                                      |
| :-----: | ----------------------------- | -------------------------------------------------------------- |
|   [1]   | `System.IO`                   | `Path.Combine`, `Path.GetFullPath`, `Path.GetRelativePath`     |
|   [2]   | `System.IO`                   | `File.OpenHandle`, `RandomAccess`                              |
|   [3]   | `System.IO`                   | `FileStreamOptions`, `FileOptions`                             |
|   [4]   | `System.IO`                   | `Stream.ReadAtLeastAsync`, `ReadExactlyAsync`                  |
|   [5]   | `System.IO`                   | `BinaryReader.ReadExactly(Span<byte>)`                         |
|   [6]   | `System.Buffers`              | `ArrayPool<T>.Shared`, `MemoryPool<T>.Shared`                  |
|   [7]   | `System.Memory`               | `ReadOnlySpan<T>`, `Span<T>`, `ReadOnlyMemory<T>`, `Memory<T>` |
|   [8]   | `System.Buffers`              | `ReadOnlySequence<T>`, `SequenceReader<T>`                     |
|   [9]   | `System.Buffers.Binary`       | `BinaryPrimitives`                                             |
|  [10]   | `System.IO.Pipelines`         | `PipeReader`, `PipeWriter`, `PipeOptions`                      |
|  [11]   | `System.IO`                   | `FileSystemWatcher`                                            |
|  [12]   | `System.IO.MemoryMappedFiles` | `MemoryMappedFile`                                             |

Use:
- [1] Canonical path algebra; explicit `StringComparison` on macOS.
- [2] Offset-based binary I/O via `Read`/`ReadAsync`/`Write`/`WriteAsync` — not `ReadAtOffsetAsync`.
- [3] Explicit buffer size, async, sequential/random scan, preallocation.
- [4] Length-prefixed framing without manual read-until-N loops.
- [5] Structured binary parse into caller workspace.
- [6] Temporary workspaces; `Return(clearArray:)` in `finally`; `IMemoryOwner<T>` across `await`.
- [7] Internal hot-path contract; C# 14 implicit span conversions per language doc.
- [8] Multi-segment parse without flattening to `byte[]`.
- [9] Endian wire integers; frame headers.
- [10] Streaming protocol with backpressure and buffer pooling.
- [11] Directory change notifications at boundary; debounce with `TimeProvider`.
- [12] Large read-mostly binary maps — not length-prefixed wire framing.

Validate length-from-wire against a bounded cap before any pool rent or allocation.

## [7][CRYPTO_AND_INTEGRITY]

| [INDEX] | [NAMESPACE_OR_TYPE]            | [SURFACE]                                                                       |
| :-----: | ------------------------------ | ------------------------------------------------------------------------------- |
|   [1]   | `System.Security.Cryptography` | `SHA256.HashData`, `IncrementalHash`, `CryptographicOperations.FixedTimeEquals` |
|   [2]   | `System.Security.Cryptography` | `RandomNumberGenerator.Fill`                                                    |
|   [3]   | `System.IO.Hashing`            | `XxHash3`, `XxHash64`, `Crc32`                                                  |
|   [4]   | `System.Formats.Asn1`          | `AsnReader`, `AsnWriter`                                                        |

Use:
- [1] Content fingerprints and tamper-evident bundles; bridge `ContentFingerprint` pattern.
- [2] Cryptographic random bytes — not `System.Random`.
- [3] Non-cryptographic cache keys and content IDs — not `SHA256`.
- [4] Structured DER when PKI/signing needs ASN.1 — defer until required.

## [8][BOUNDARY_CONTRACTS]

| [INDEX] | [NAMESPACE_OR_TYPE]                     | [SURFACE]                                                                   |
| :-----: | --------------------------------------- | --------------------------------------------------------------------------- |
|   [1]   | `System.Diagnostics.CodeAnalysis`       | `NotNullWhen`, `NotNullIfNotNull`, `MemberNotNull`, `DoesNotReturn`         |
|   [2]   | `System.Diagnostics.CodeAnalysis`       | `StringSyntax`, `DynamicallyAccessedMembers`                                |
|   [3]   | `System.ComponentModel.DataAnnotations` | `ValidationContext`, `Validator.TryValidateObject`, `[Required]`, `[Range]` |

Use:
- [1] NRT flow after `Try*` / guard methods on GH/Rhino boundaries.
- [2] Regex/JSON syntax hints; trim annotations only when pursuing NativeAOT.
- [3] Sync DTO validation at boundary — not Thinktecture/LanguageExt domain admission.

## [9][LINQ_EXTENDED]

| [INDEX] | [NAMESPACE_OR_TYPE] | [SURFACE]                                    |
| :-----: | ------------------- | -------------------------------------------- |
|   [1]   | `System.Linq`       | `LeftJoin`, `RightJoin`                      |
|   [2]   | `System.Linq`       | `MinBy`, `MaxBy`                             |
|   [3]   | `System.Linq`       | `Chunk`, `Index`, `TryGetNonEnumeratedCount` |
|   [4]   | `System.Linq`       | `Shuffle`                                    |

Use:
- [1] Outer joins without `GroupJoin` ceremony.
- [2] Argmin/argmax without full sort.
- [3] Batching, indexed enumeration, avoid forced full counts.
- [4] In-memory random order — tests use CsCheck `Gen.Shuffle` instead.

## [10][COMPILER_AND_INTEROP]

| [INDEX] | [NAMESPACE_OR_TYPE]               | [SURFACE]                                        |
| :-----: | --------------------------------- | ------------------------------------------------ |
|   [1]   | `System.Runtime.CompilerServices` | `CallerArgumentExpression`                       |
|   [2]   | `System.Runtime.CompilerServices` | `MethodImpl`, `CallerMemberName`                 |
|   [3]   | `System.Runtime.CompilerServices` | `InterpolatedStringHandler`, `CollectionBuilder` |
|   [4]   | `System.Runtime.InteropServices`  | `StructLayout`, `LayoutKind`                     |
|   [5]   | `System.Runtime.InteropServices`  | `MemoryMarshal`                                  |
|   [6]   | `System.Runtime.InteropServices`  | `NativeMemory`, `SuppressGCTransition`           |

[USE]
- [1] Validation messages without stringly `paramName`.
- [2] Hot-path inlining; boundary telemetry keys.
- [3] Custom formatters and collection-expression targets at boundary only.
- [4] Dense boundary record layout.
- [5] Unmanaged reinterpret; never on managed-reference spans.
- [6] Native alloc and tight P/Invoke; `unsafe` + measured proof only.

`required` members need no manual `[RequiredMember]` import when using the keyword. ISA intrinsics require explicit subnamespace usings — never workspace-global.

## [11][EXPLICIT_USINGS]

| [INDEX] | [NAMESPACE]                                                                                                                         | [SDK_IMPLICIT] | [TYPICAL_POLICY]                                                   |
| :-----: | ----------------------------------------------------------------------------------------------------------------------------------- | -------------- | ------------------------------------------------------------------ |
|   [1]   | `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Net.Http`, `System.Threading`, `System.Threading.Tasks` | Yes            | Default SDK set.                                                   |
|   [2]   | `System.Text`, `System.Text.RegularExpressions`, `System.Buffers`, `System.Buffers.Text`, `System.Buffers.Binary`                   | No             | File-scoped at text/wire boundaries.                               |
|   [3]   | `System.Collections.Frozen`, `System.Collections.Immutable`                                                                         | No             | Project-global or file-scoped on catalog libs.                     |
|   [4]   | `System.Globalization`                                                                                                              | No             | File-scoped or project-global when invariant culture is pervasive. |
|   [5]   | `System.Text.Json`, `System.Text.Json.Serialization`                                                                                | No             | Boundary serialization modules.                                    |
|   [6]   | `System.Threading.Channels`, `System.IO.Pipelines`, `System.Threading.RateLimiting`                                                 | No             | Protocol/framing modules only.                                     |
|   [7]   | `System.Diagnostics.Metrics`, `System.Diagnostics.CodeAnalysis`                                                                     | No             | Observability and contract modules.                                |
|   [8]   | `Microsoft.Extensions.Logging`                                                                                                      | No             | Package-required on net10.0 until adopted.                         |
|   [9]   | `System.Numerics`, `System.Numerics.Tensors`, `System.Runtime.Intrinsics.*`                                                         | No             | Hot-path numerics; ISA NS never global.                            |
|  [10]   | `System.IO.Hashing`, `System.Security.Cryptography`, `System.Formats.Asn1`, `System.Text.Unicode`                                   | No             | Integrity/crypto/ASN at boundaries.                                |
|  [11]   | `System.Runtime.CompilerServices`, `System.Runtime.InteropServices`                                                                 | No             | Per-project globals when dense; not transitive.                    |

Workspace LanguageExt/Thinktecture globals — see `meta.md` §6.

## [12][DRAWING]

`System.Drawing.Common` is not a universal dependency. Rasm resolves Rhino UI drawing through RhinoWIP app-bundle assemblies and uses compile-only package metadata only where forwarded `System.Drawing.*` types require it. Runtime drawing claims need RhinoWIP host proof, not NuGet claims alone.
