# [REPLACEMENTS]

[IMPORTANT] Replacement means moving behavior to the owning API, not adding wrappers. BCL surface catalog: `bcl.md`. Language features: `../external-libs/csharp/language.md`.

## [1][TEXT_AND_LOOKUP]

| [INDEX] | [LOCAL_PATTERN]                                                  |
| :-----: | ---------------------------------------------------------------- |
|   [1]   | Hand-built static regex.                                         |
|   [2]   | Regex for character allow/deny checks.                           |
|   [3]   | Rebuilt static dictionaries.                                     |
|   [4]   | `Regex.Matches(…).Count` or `.Count()`.                          |
|   [5]   | `new Regex(…, NonBacktracking)` expecting compile-time codegen.  |
|   [6]   | `string.Format` / `AppendFormat` with same format in a hot loop. |
|   [7]   | `int.Parse` / `double.ToString` on UTF-8 protocol buffers.       |
|   [8]   | `string.Split` / manual `IndexOf` token loops.                   |
|   [9]   | `string.Compare(a, b)` / `==` on identifiers.                    |
|  [10]   | `double.Parse(s)` / `ToString()` on persisted or wire values.    |
|  [11]   | Hand-rolled JSON with reflection on known types.                 |
|  [12]   | `Encoding.UTF8.GetBytes(string)` on hot paths.                   |
|  [13]   | `Regex.IsMatch` only for boolean presence.                       |
|  [14]   | Manual hex encode loops.                                         |
|  [15]   | Line iteration via `string.Split('\n')`.                         |

[OWNER]
- [1] `[GeneratedRegex]` with optional timeout and culture; omit `Compiled`.
- [2] Cached `SearchValues<char>` + `MemoryExtensions.ContainsAnyExcept` / `IndexOfAnyExcept`.
- [3] `FrozenDictionary`/`FrozenSet` with explicit `comparer:` or Thinktecture smart-enum `Items`.
- [4] `Regex.Count` (instance or static overload).
- [5] Plain `Regex` with `NonBacktracking`, or `[GeneratedRegex]` — generator emits cached runtime `Regex`, not custom IL.
- [6] `CompositeFormat.Parse` + cached `static readonly CompositeFormat` (CA1863).
- [7] `Utf8Parser.TryParse` / `Utf8Formatter.TryFormat` into `Span<byte>`.
- [8] `ReadOnlySpan<char>.Split`/`SplitAny` + `Range`, or `SearchValues` + span scans.
- [9] `string.Equals(a, b, StringComparison.Ordinal)` or `OrdinalIgnoreCase` (CA1862).
- [10] `Parse`/`TryParse` with `CultureInfo.InvariantCulture` or `string.Create(InvariantCulture, …)`.
- [11] STJ `partial JsonSerializerContext` + `[JsonSerializable]` + `TypeInfoResolver = Context.Default`.
- [12] Span overloads; avoid interim `string` when input is already span/bytes.
- [13] `Regex.IsMatch` or span `Contains` — CA1874 when counting would allocate.
- [14] `Convert.ToHexString` / `ToHexStringLower` (CA1872).
- [15] `ReadOnlySpan<char>.EnumerateLines` — not for length-prefixed wire newlines.

## [2][COLLECTIONS_AND_EQUALITY]

| [INDEX] | [LOCAL_PATTERN]                                                                  |
| :-----: | -------------------------------------------------------------------------------- |
|   [1]   | Public `Dictionary<K,V>` / `List<T>` / `ImmutableDictionary<K,V>` in domain API. |
|   [2]   | Domain `ConcurrentDictionary`.                                                   |
|   [3]   | Repeated `GroupBy` on same key.                                                  |
|   [4]   | Manual binary heap / sort-then-pop.                                              |
|   [5]   | Capacity-bound eviction cache (LRU).                                             |
|   [6]   | Insertion-ordered map at boundary.                                               |
|   [7]   | Loop `.Add` accumulation.                                                        |
|   [8]   | Hand-written `IEqualityComparer<T>` subclass for domain types.                   |
|   [9]   | Manual XOR / prime hash combining.                                               |
|  [10]   | Hash ignoring comparer policy.                                                   |
|  [11]   | Class with field-wise `Equals`/`GetHashCode`.                                    |
|  [12]   | Branded string/scalar domain identity.                                           |
|  [13]   | `GetHashCode()` on host object for correlation key.                              |
|  [14]   | `CompareExchange` for deduplication.                                             |
|  [15]   | String-key frozen lookup from `ReadOnlySpan<char>` wire tokens.                  |
|  [16]   | Static frozen catalog plus runtime registration.                                 |

[OWNER]
- [1] LanguageExt `HashMap` / `Seq`.
- [2] `Atom<HashMap<K,V>>` at boundary; `ConcurrentDictionary` only for measured boundary caches.
- [3] `ToLookup` once, pre-index `FrozenDictionary`, or `DistinctBy` with explicit comparer.
- [4] `PriorityQueue<TElement,TPriority>`.
- [5] `Dictionary` + `LinkedList` with measured policy, or bounded custom cache — not `OrderedDictionary` (insertion order only).
- [6] `OrderedDictionary<K,V>` (.NET 9+; index APIs .NET 10).
- [7] `Seq.Fold`/`Choose` or boundary `[..]` materialization.
- [8] Thinktecture `[KeyMemberEqualityComparer]`; reserve `EqualityComparer<T>.Create` for non-branded ad-hoc comparers.
- [9] `HashCode.Combine` or `new HashCode()` + `Add` + `ToHashCode()`.
- [10] `hash.Add(value, StringComparer.OrdinalIgnoreCase)` or matching frozen comparer.
- [11] `record`/`record struct` or Thinktecture value object.
- [12] Thinktecture `[KeyMemberEqualityComparer]`.
- [13] `RuntimeHelpers.GetHashCode(hostRef)`.
- [14] `HashSet`/`FrozenSet` with explicit comparer, or `HashMap` key policy.
- [15] `FrozenDictionary.GetAlternateLookup<ReadOnlySpan<char>>()` when comparer supports alternate keys.
- [16] `FrozenDictionary` slice + `Atom<HashMap<…>>` overlay; rebuild frozen view on registration.

## [3][VALIDATION_AND_DISPATCH]

| [INDEX] | [LOCAL_PATTERN]                                           |
| :-----: | --------------------------------------------------------- |
|   [1]   | Primitive validation repeated across files.               |
|   [2]   | Parallel dictionaries beside enum-like values.            |
|   [3]   | Repeated switch or visitor arms.                          |
|   [4]   | Nested fallible unwrapping.                               |
|   [5]   | Manual `paramName` in throws.                             |
|   [6]   | Manual field guard chains on wire DTOs.                   |
|   [7]   | `GroupJoin` + `SelectMany` + `DefaultIfEmpty` outer join. |

[OWNER]
- [1] Thinktecture value object plus LanguageExt rail.
- [2] Thinktecture smart enum with item-owned behavior.
- [3] Thinktecture union `Switch`/`Map`.
- [4] LanguageExt `Flatten`, `Bind`, or LINQ comprehension.
- [5] `[CallerArgumentExpression]`.
- [6] `Validator.TryValidateObject` + data annotations at boundary only — not domain rails.
- [7] `Enumerable.LeftJoin` / `RightJoin`.

## [4][NUMERICS]

| [INDEX] | [LOCAL_PATTERN]                             |
| :-----: | ------------------------------------------- |
|   [1]   | Manual matrix solve or decomposition.       |
|   [2]   | Sparse SPD direct factorization.            |
|   [3]   | Geometry algorithm already native to Rhino. |
|   [4]   | Flat hot numeric reduction.                 |
|   [5]   | Branchless conditional numeric map.         |
|   [6]   | Symbolic formula parsing/evaluation.        |
|   [7]   | Hand-rolled per-type scalar math.           |
|   [8]   | MathNet `Vector<double>` on public API.     |
|   [9]   | Manual SIMD lane permute.                   |
|  [10]   | `OrderBy(_ => Guid.NewGuid())` shuffle.     |
|  [11]   | Non-cryptographic content fingerprint.      |

[OWNER]
- [1] MathNet factorization/solver API.
- [2] CSparse Cholesky + MathNet residual check — see `../external-libs/mathnet/sparse.md`.
- [3] RhinoCommon.
- [4] `TensorPrimitives` or `Vector<T>` after `System.Numerics.Tensors` adoption gate.
- [5] `Vector.ConditionalSelect` / `Vector<T>` masks.
- [6] MathNet.Symbolics plus LanguageExt admission rail.
- [7] Narrowest `INumberBase<T>` / `IBinaryInteger<T>` / `IFloatingPointIeee754<T>` constraint.
- [8] Domain types; MathNet vectors internal to algorithms only.
- [9] `Vector128.Shuffle` → `ShuffleNative` → ISA intrinsics after portable path fails.
- [10] `Enumerable.Shuffle` on materialized sequences.
- [11] `System.IO.Hashing.XxHash64` — reserve `SHA256` for integrity-oriented boundaries.

## [5][RUNTIME_AND_IO]

| [INDEX] | [LOCAL_PATTERN]                                           |
| :-----: | --------------------------------------------------------- |
|   [1]   | `DateTime.Now`/`UtcNow` deltas for duration.              |
|   [2]   | `Stopwatch.StartNew()` + `.Elapsed` in testable paths.    |
|   [3]   | Manual tick math `(end - start) / frequency`.             |
|   [4]   | Hand-rolled queue + lock + event.                         |
|   [5]   | `ConcurrentQueue` in domain.                              |
|   [6]   | Timeout token ad hoc.                                     |
|   [7]   | `lock(object)` at boundary sites.                         |
|   [8]   | `Console.WriteLine` / string-interpolated logs.           |
|   [9]   | Ad-hoc `ActivitySource` / `Meter` in handlers.            |
|  [10]   | Hand-rolled counters.                                     |
|  [11]   | Rolling averages / latency distributions.                 |
|  [12]   | Point-in-time level metrics.                              |
|  [13]   | `TraceSource` / `DiagnosticSource.Write`.                 |
|  [14]   | `Debug.Assert` for exhaustiveness.                        |
|  [15]   | `new byte[n]` workspace for one operation.                |
|  [16]   | `FileStream` + `Seek` + `Read` loop.                      |
|  [17]   | Manual read-until-N-bytes loop.                           |
|  [18]   | `BitConverter` on endian wire frames.                     |
|  [19]   | `MemoryStream` accumulating unbounded wire data.          |
|  [20]   | Ad-hoc `FileStream` construction.                         |
|  [21]   | `new PeriodicTimer` / `System.Threading.Timer` in domain. |
|  [22]   | `TaskCompletionSource` without async continuations.       |
|  [23]   | `IAsyncEnumerable` without cancellation flow.             |
|  [24]   | Polling for filesystem changes.                           |
|  [25]   | `new HttpClient()` in domain/application.                 |

[OWNER]
- [1] `TimeProvider.GetTimestamp` + `GetElapsedTime`.
- [2] Injected `TimeProvider` monotonic pair; static `Stopwatch.GetElapsedTime` at tool boundary only.
- [3] `TimeProvider.GetElapsedTime(start)`.
- [4] Bounded `System.Threading.Channels.Channel<T>`.
- [5] `Atom<Seq<T>>` or boundary-only adapter.
- [6] `CreateLinkedTokenSource` + `CancelAfter` inside `Bracket`.
- [7] `System.Threading.Lock` + `EnterScope()`.
- [8] `[LoggerMessage]` after `Microsoft.Extensions.Logging.Abstractions` adoption at host/runtime boundaries.
- [9] Centralized observability module (CSP0604).
- [10] `Meter.CreateCounter` / `CreateUpDownCounter`.
- [11] `Meter.CreateHistogram` + `InstrumentAdvice` bucket boundaries.
- [12] `CreateObservableGauge` or `CreateUpDownCounter`.
- [13] `ActivitySource.StartActivity` + tag policy.
- [14] Typed error rails; `_ => throw new UnreachableException()`.
- [15] `stackalloc` (<256 B) or `ArrayPool<T>.Rent` + `Return` in `finally`.
- [16] `RandomAccess.ReadAsync` / `Read`.
- [17] `Stream.ReadExactlyAsync` / `ReadAtLeastAsync`.
- [18] `BinaryPrimitives` / `Utf8Parser`; `BitConverter.DoubleToUInt64Bits` acceptable for hash material only.
- [19] `PipeReader`/`PipeWriter` or bounded pool with frame cap.
- [20] `FileStreamOptions` with explicit `BufferSize` and `FileOptions`.
- [21] LanguageExt `Schedule` in domain; `PeriodicTimer` at boundary adapters only (CSP0401).
- [22] `new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously)`.
- [23] `[EnumeratorCancellation]` on token parameter (CSP0608).
- [24] `FileSystemWatcher` + debounce via injectable `TimeProvider`.
- [25] Centralized handler at composition root — CSP0008; product Rhino code is not an HTTP service.

## [6][HOST_BOUNDARIES]

| [INDEX] | [LOCAL_PATTERN]                           |
| :-----: | ----------------------------------------- |
|   [1]   | Hand-built GH2 tree paths.                |
|   [2]   | Rhino tolerance constants copied locally. |
|   [3]   | Runtime package assumptions.              |

[OWNER]
- [1] GH2 `Garden`, `Tree`, `Coverage`, `WithPathPrefix`.
- [2] `RhinoMath` and document context.
- [3] Host load evidence and `meta.md` host-reference policy.
