# [H1][REPLACEMENTS]
>**Dictum:** *Refactoring starts by choosing the canonical owner.*

<br>

[IMPORTANT] Replacement means moving behavior to the owning API, not adding wrappers. BCL surface catalog: `bcl.md`. Language features: `../external-libs/csharp/language.md`.

---
## [1][TEXT_AND_LOOKUP]
>**Dictum:** *Grammar, character-set scans, culture/encoding, and lookup lifetime are separate.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Hand-built static regex. | `[GeneratedRegex]` with optional timeout and culture; omit `Compiled`. |
| [2] | Regex for character allow/deny checks. | Cached `SearchValues<char>` + `MemoryExtensions.ContainsAnyExcept` / `IndexOfAnyExcept`. |
| [3] | Rebuilt static dictionaries. | `FrozenDictionary`/`FrozenSet` with explicit `comparer:` or Thinktecture smart-enum `Items`. |
| [4] | `Regex.Matches(…).Count` or `.Count()`. | `Regex.Count` (instance or static overload). |
| [5] | `new Regex(…, NonBacktracking)` expecting compile-time codegen. | Plain `Regex` with `NonBacktracking`, or `[GeneratedRegex]` — generator emits cached runtime `Regex`, not custom IL. |
| [6] | `string.Format` / `AppendFormat` with same format in a hot loop. | `CompositeFormat.Parse` + cached `static readonly CompositeFormat` (CA1863). |
| [7] | `int.Parse` / `double.ToString` on UTF-8 protocol buffers. | `Utf8Parser.TryParse` / `Utf8Formatter.TryFormat` into `Span<byte>`. |
| [8] | `string.Split` / manual `IndexOf` token loops. | `ReadOnlySpan<char>.Split`/`SplitAny` + `Range`, or `SearchValues` + span scans. |
| [9] | `string.Compare(a, b)` / `==` on identifiers. | `string.Equals(a, b, StringComparison.Ordinal)` or `OrdinalIgnoreCase` (CA1862). |
| [10] | `double.Parse(s)` / `ToString()` on persisted or wire values. | `Parse`/`TryParse` with `CultureInfo.InvariantCulture` or `string.Create(InvariantCulture, …)`. |
| [11] | Hand-rolled JSON with reflection on known types. | STJ `partial JsonSerializerContext` + `[JsonSerializable]` + `TypeInfoResolver = Context.Default`. |
| [12] | `Encoding.UTF8.GetBytes(string)` on hot paths. | Span overloads; avoid interim `string` when input is already span/bytes. |
| [13] | `Regex.IsMatch` only for boolean presence. | `Regex.IsMatch` or span `Contains` — CA1874 when counting would allocate. |
| [14] | Manual hex encode loops. | `Convert.ToHexString` / `ToHexStringLower` (CA1872). |
| [15] | Line iteration via `string.Split('\n')`. | `ReadOnlySpan<char>.EnumerateLines` — not for length-prefixed wire newlines. |

---
## [2][COLLECTIONS_AND_EQUALITY]
>**Dictum:** *Lifetime, comparer policy, and domain rail ownership are decided before collection choice.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Public `Dictionary<K,V>` / `List<T>` / `ImmutableDictionary<K,V>` in domain API. | LanguageExt `HashMap` / `Seq`. |
| [2] | Domain `ConcurrentDictionary`. | `Atom<HashMap<K,V>>` at boundary; `ConcurrentDictionary` only for measured boundary caches. |
| [3] | Repeated `GroupBy` on same key. | `ToLookup` once, pre-index `FrozenDictionary`, or `DistinctBy` with explicit comparer. |
| [4] | Manual binary heap / sort-then-pop. | `PriorityQueue<TElement,TPriority>`. |
| [5] | Capacity-bound eviction cache (LRU). | `Dictionary` + `LinkedList` with measured policy, or bounded custom cache — not `OrderedDictionary` (insertion order only). |
| [6] | Insertion-ordered map at boundary. | `OrderedDictionary<K,V>` (.NET 9+; index APIs .NET 10). |
| [7] | Loop `.Add` accumulation. | `Seq.Fold`/`Choose` or boundary `[..]` materialization. |
| [8] | Hand-written `IEqualityComparer<T>` subclass for domain types. | Thinktecture `[KeyMemberEqualityComparer]`; reserve `EqualityComparer<T>.Create` for non-branded ad-hoc comparers. |
| [9] | Manual XOR / prime hash combining. | `HashCode.Combine` or `new HashCode()` + `Add` + `ToHashCode()`. |
| [10] | Hash ignoring comparer policy. | `hash.Add(value, StringComparer.OrdinalIgnoreCase)` or matching frozen comparer. |
| [11] | Class with field-wise `Equals`/`GetHashCode`. | `record`/`record struct` or Thinktecture value object. |
| [12] | Branded string/scalar domain identity. | Thinktecture `[KeyMemberEqualityComparer]`. |
| [13] | `GetHashCode()` on host object for correlation key. | `RuntimeHelpers.GetHashCode(hostRef)`. |
| [14] | `CompareExchange` for deduplication. | `HashSet`/`FrozenSet` with explicit comparer, or `HashMap` key policy. |
| [15] | String-key frozen lookup from `ReadOnlySpan<char>` wire tokens. | `FrozenDictionary.GetAlternateLookup<ReadOnlySpan<char>>()` when comparer supports alternate keys. |
| [16] | Static frozen catalog plus runtime registration. | `FrozenDictionary` slice + `Atom<HashMap<…>>` overlay; rebuild frozen view on registration. |

---
## [3][VALIDATION_AND_DISPATCH]
>**Dictum:** *Closed vocabularies carry lookup and behavior.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Primitive validation repeated across files. | Thinktecture value object plus LanguageExt rail. |
| [2] | Parallel dictionaries beside enum-like values. | Thinktecture smart enum with item-owned behavior. |
| [3] | Repeated switch or visitor arms. | Thinktecture union `Switch`/`Map`. |
| [4] | Nested fallible unwrapping. | LanguageExt `Flatten`, `Bind`, or LINQ comprehension. |
| [5] | Manual `paramName` in throws. | `[CallerArgumentExpression]`. |
| [6] | Manual field guard chains on wire DTOs. | `Validator.TryValidateObject` + data annotations at boundary only — not domain rails. |
| [7] | `GroupJoin` + `SelectMany` + `DefaultIfEmpty` outer join. | `Enumerable.LeftJoin` / `RightJoin`. |

---
## [4][NUMERICS]
>**Dictum:** *Numerical policy delegates to Rhino, MathNet, or measured BCL kernels.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Manual matrix solve or decomposition. | MathNet factorization/solver API. |
| [2] | Sparse SPD direct factorization. | CSparse Cholesky + MathNet residual check — see `../external-libs/mathnet/sparse.md`. |
| [3] | Geometry algorithm already native to Rhino. | RhinoCommon. |
| [4] | Flat hot numeric reduction. | `TensorPrimitives` or `Vector<T>` after `System.Numerics.Tensors` adoption gate. |
| [5] | Branchless conditional numeric map. | `Vector.ConditionalSelect` / `Vector<T>` masks. |
| [6] | Symbolic formula parsing/evaluation. | MathNet.Symbolics plus LanguageExt admission rail. |
| [7] | Hand-rolled per-type scalar math. | Narrowest `INumberBase<T>` / `IBinaryInteger<T>` / `IFloatingPointIeee754<T>` constraint. |
| [8] | MathNet `Vector<double>` on public API. | Domain types; MathNet vectors internal to algorithms only. |
| [9] | Manual SIMD lane permute. | `Vector128.Shuffle` → `ShuffleNative` → ISA intrinsics after portable path fails. |
| [10] | `OrderBy(_ => Guid.NewGuid())` shuffle. | `Enumerable.Shuffle` on materialized sequences. |
| [11] | Non-cryptographic content fingerprint. | `System.IO.Hashing.XxHash64` — reserve `SHA256` for integrity-oriented boundaries. |

---
## [5][RUNTIME_AND_IO]
>**Dictum:** *Concurrency, timing, observability, and wire framing each have one BCL owner.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | `DateTime.Now`/`UtcNow` deltas for duration. | `TimeProvider.GetTimestamp` + `GetElapsedTime`. |
| [2] | `Stopwatch.StartNew()` + `.Elapsed` in testable paths. | Injected `TimeProvider` monotonic pair; static `Stopwatch.GetElapsedTime` at tool boundary only. |
| [3] | Manual tick math `(end - start) / frequency`. | `TimeProvider.GetElapsedTime(start)`. |
| [4] | Hand-rolled queue + lock + event. | Bounded `System.Threading.Channels.Channel<T>`. |
| [5] | `ConcurrentQueue` in domain. | `Atom<Seq<T>>` or boundary-only adapter. |
| [6] | Timeout token ad hoc. | `CreateLinkedTokenSource` + `CancelAfter` inside `Bracket`. |
| [7] | `lock(object)` at boundary sites. | `System.Threading.Lock` + `EnterScope()`. |
| [8] | `Console.WriteLine` / string-interpolated logs. | `[LoggerMessage]` after `Microsoft.Extensions.Logging.Abstractions` adoption — not in graph today. |
| [9] | Ad-hoc `ActivitySource` / `Meter` in handlers. | Centralized observability module (CSP0604). |
| [10] | Hand-rolled counters. | `Meter.CreateCounter` / `CreateUpDownCounter`. |
| [11] | Rolling averages / latency distributions. | `Meter.CreateHistogram` + `InstrumentAdvice` bucket boundaries. |
| [12] | Point-in-time level metrics. | `CreateObservableGauge` or `CreateUpDownCounter`. |
| [13] | `TraceSource` / `DiagnosticSource.Write`. | `ActivitySource.StartActivity` + tag policy. |
| [14] | `Debug.Assert` for exhaustiveness. | Typed error rails; `_ => throw new UnreachableException()`. |
| [15] | `new byte[n]` workspace for one operation. | `stackalloc` (<256 B) or `ArrayPool<T>.Rent` + `Return` in `finally`. |
| [16] | `FileStream` + `Seek` + `Read` loop. | `RandomAccess.ReadAsync` / `Read`. |
| [17] | Manual read-until-N-bytes loop. | `Stream.ReadExactlyAsync` / `ReadAtLeastAsync`. |
| [18] | `BitConverter` on endian wire frames. | `BinaryPrimitives` / `Utf8Parser`; `BitConverter.DoubleToUInt64Bits` acceptable for hash material only. |
| [19] | `MemoryStream` accumulating unbounded wire data. | `PipeReader`/`PipeWriter` or bounded pool with frame cap. |
| [20] | Ad-hoc `FileStream` construction. | `FileStreamOptions` with explicit `BufferSize` and `FileOptions`. |
| [21] | `new PeriodicTimer` / `System.Threading.Timer` in domain. | LanguageExt `Schedule` in domain; `PeriodicTimer` at boundary adapters only (CSP0401). |
| [22] | `TaskCompletionSource` without async continuations. | `new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously)`. |
| [23] | `IAsyncEnumerable` without cancellation flow. | `[EnumeratorCancellation]` on token parameter (CSP0608). |
| [24] | Polling for filesystem changes. | `FileSystemWatcher` + debounce via injectable `TimeProvider`. |
| [25] | `new HttpClient()` in domain/application. | Centralized handler at composition root — CSP0008; product Rhino code is not an HTTP service. |

---
## [6][HOST_BOUNDARIES]
>**Dictum:** *Native host structures remain native until projected.*

<br>

| [INDEX] | [LOCAL_PATTERN] | [OWNER] |
| :-----: | --------------- | ------- |
| [1] | Hand-built GH2 tree paths. | GH2 `Garden`, `Tree`, `Coverage`, `WithPathPrefix`. |
| [2] | Rhino tolerance constants copied locally. | `RhinoMath` and document context. |
| [3] | Runtime package assumptions. | Host load evidence and `meta.md` host-reference policy. |
