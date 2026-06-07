# [SYSTEM_APIS]

System APIs replace local machinery only when they own the concern. They do not replace LanguageExt rails, Thinktecture domain shapes, MathNet algorithms, Rhino geometry, or GH2 data semantics.

Package gates and global using policy route to [build and packages](build-and-packages.md). C# syntax routes to [language](../language.md).

## [1][SMELL_LOOKUP]

This table is a lookup by repeated local smell.

| [INDEX] | [SMELL]                 | [OWNER]                       |
| :-----: | :---------------------- | :---------------------------- |
|   [1]   | manual hex loop         | `Convert.ToHexString`         |
|   [2]   | regex count allocation  | `Regex.Count`                 |
|   [3]   | regex character filter  | `SearchValues<T>`             |
|   [4]   | mutable public payload  | immutable boundary collection |
|   [5]   | primitive branded value | generated domain shape        |
|   [6]   | matrix algorithm        | numeric concept page          |
|   [7]   | `DateTime.Now` delta    | `TimeProvider` or `Stopwatch` |

## [2][TEXT_AND_WIRE]

Regex grammar:
    Owner: `[GeneratedRegex]` for stable structural grammar; plain `Regex` with `RegexOptions.NonBacktracking` for runtime grammar.
    Replace: hand-built static regex, regex count via `Matches().Count`, and ad-hoc boolean match loops.
    Gate: use `Regex.Count` when the count is the result; use timeout and culture on `[GeneratedRegex]` when the grammar needs them.
    Reject: expecting `[GeneratedRegex]` to emit custom IL for `RegexOptions.Compiled`.

Character search:
    Owner: `SearchValues<T>` with `MemoryExtensions.ContainsAny`, `ContainsAnyExcept`, `IndexOfAny`, and `IndexOfAnyExcept`.
    Replace: regex used only for allow/deny character checks.
    Gate: cache the `SearchValues<T>` instance beside the policy.

Formatting and parsing:
    Owner: `CompositeFormat`, `Utf8Parser`, `Utf8Formatter`, invariant `TryParse`, and `string.Create`.
    Replace: repeated format parsing, UTF-16 round trips on UTF-8 paths, and culture-ambient persisted values.
    Gate: use `CultureInfo.InvariantCulture` for wire and persistence.

JSON:
    Owner: `[JsonSerializable]`, `JsonSerializerContext`, `JsonSourceGenerationOptions`, and `JsonTypeInfo<T>` overloads.
    Replace: reflection-heavy serializers for known contract types and hand-rolled JSON.
    Gate: boundary contracts only; domain admission still routes through Thinktecture and LanguageExt.

Encoding:
    Owner: `Encoding`, `UTF8Encoding`, `Rune`, `Ascii`, `Base64Url`, `Convert.ToHexString`, and `ToHexStringLower`.
    Replace: manual hex loops, unsafe ASCII checks, and URL-base64 helpers.
    Gate: validate allowed code points with text policy, not scattered predicates.

## [3][COLLECTIONS_AND_LOOKUP]

Read-mostly lookup:
    Owner: `FrozenDictionary`, `FrozenSet`, and alternate span lookup where the comparer supports it.
    Replace: rebuilt static dictionaries and string-key lookup copied per call.
    Gate: pass an explicit comparer.

Immutable boundary payload:
    Owner: `ImmutableArray`, `ImmutableDictionary`, and `ImmutableHashSet`.
    Replace: mutable public collection payloads outside LanguageExt rails.
    Route-away: domain sequence identity belongs to LanguageExt `Seq<T>` and `HashMap<K,V>`.

Ordering and scheduling:
    Owner: `OrderedDictionary<K,V>` for insertion order and `PriorityQueue<TElement,TPriority>` for heap scheduling.
    Replace: sort-then-pop and manual insertion-order maps.
    Reject: treating `OrderedDictionary<K,V>` as an LRU cache.

Grouping and set algebra:
    Owner: `ToLookup`, `DistinctBy`, `CountBy`, `AggregateBy`, `ExceptBy`, `IntersectBy`, and `UnionBy`.
    Replace: repeated `GroupBy`, loop dedup, and comparer-free keyed set operations.
    Gate: declare key comparer policy when default equality is not the domain rule.

Hot dictionary construction:
    Owner: `CollectionsMarshal.GetValueRefOrAddDefault` at measured boundaries.
    Reject: domain public identity built from mutable dictionary internals.

## [4][EQUALITY_AND_IDENTITY]

Default equality:
    Owner: `EqualityComparer<T>.Default`, `EqualityComparer<T>.Create`, `HashCode`, records, and record structs.
    Replace: hand-written comparer classes and XOR or prime hash combining for non-branded data.
    Gate: hash with the same comparer policy used by lookup.
    Reject: persisting `HashCode` output or using it as stable file identity because the output is process-randomized.

Branded equality:
    Owner: Thinktecture value objects, `[KeyMemberEqualityComparer]`, and `[KeyMemberComparer]`.
    Replace: primitive branded string or scalar equality copied across files.
    Route-away: generated shape details live in [domain shapes](../domain-shapes.md).

Host identity:
    Owner: `RuntimeHelpers.GetHashCode(object)` for live host reference identity.
    Reject: persisting `GetHashCode()` output or using it as a stable file key.

## [5][NUMERICS]

Scalar and generic math:
    Owner: `Math`, `MathF`, `Half`, BCL `Complex`, and the narrowest generic math constraint such as `INumberBase<T>`, `IBinaryInteger<T>`, or `IFloatingPointIeee754<T>`.
    Replace: hand-rolled per-type scalar math.
    Route-away: matrix and solver algorithms live in [numeric algorithms](../numeric-algorithms.md).

Vector and tensor primitives:
    Owner: BCL `Vector<T>`, `Vector128/256/512<T>`, `Shuffle`, and `TensorPrimitives`.
    Replace: flat hot numeric reductions only after measured proof.
    Gate: `System.Numerics.Tensors` requires an explicit package consumer recorded in [build and packages](build-and-packages.md).

Type collision:
    Rule: BCL `Vector<T>` is SIMD, MathNet `Vector<T>` is linear algebra, and BCL `Complex` is not MathNet `Complex32`.
    Gate: qualify namespaces at boundaries where both meanings appear.

Host geometry:
    Owner: RhinoCommon for geometry, units, tolerances, transforms, and topology.
    Reject: replacing native host geometry semantics with generic numeric helpers.

## [6][RUNTIME_AND_OBSERVABILITY]

Time:
    Owner: `TimeProvider`, `PeriodicTimer`, `Stopwatch.GetTimestamp`, and `Stopwatch.GetElapsedTime`.
    Replace: `DateTime.Now` deltas, manual tick math, and timers inside domain logic.
    Gate: `PeriodicTimer` belongs at boundary adapters.

Observability:
    Owner: `ActivitySource`, `Activity`, `Meter`, counters, histograms, gauges, `TagList`, and `InstrumentAdvice`.
    Replace: ad-hoc `DiagnosticSource`, `TraceSource`, and manual counters.
    Gate: centralize source handles; do not create handler-local telemetry owners.

Logging:
Owner: `[LoggerMessage]` and `ILogger` after `Microsoft.Extensions.Logging.Abstractions` has a source-backed owner route.
    Replace: `Console.WriteLine` and string-interpolated logs at runtime boundaries.
    Gate: package adoption belongs in [build and packages](build-and-packages.md); no domain logging rail appears before the boundary owner exists.

Concurrency:
    Owner: `CancellationToken`, linked token sources, `Lock`, `Interlocked`, `Volatile`, `Channel<T>`, `TaskCompletionSource<T>`, `IAsyncEnumerable<T>`, and framework-proven rate limiters.
    Replace: hand-rolled queues, private `object` locks, timeout token scatter, and task completions without asynchronous continuations.
    Gate: never hold `Lock` across `await`; annotate async streams with `[EnumeratorCancellation]`.

## [7][IO_BUFFERS_AND_INTEGRITY]

Path and file IO:
    Owner: `Path`, `File.OpenHandle`, `RandomAccess`, `FileStreamOptions`, `FileOptions`, `ReadExactlyAsync`, and `ReadAtLeastAsync`.
    Replace: seek/read loops, ad-hoc `FileStream` construction, and manual read-until-N loops.
    Gate: validate wire lengths against bounded caps before allocation.

Buffers and pipelines:
    Owner: `ArrayPool<T>`, `MemoryPool<T>`, `ReadOnlySpan<T>`, `ReadOnlyMemory<T>`, `ReadOnlySequence<T>`, `SequenceReader<T>`, `BinaryPrimitives`, `PipeReader`, and `PipeWriter`.
    Replace: unbounded `MemoryStream`, `new byte[n]` scratch buffers, and `BitConverter` for endian wire frames.
    Gate: return rented arrays in `finally`; carry `IMemoryOwner<T>` across `await`.

Integrity:
    Owner: `SHA256.HashData`, `IncrementalHash`, `FixedTimeEquals`, `RandomNumberGenerator`, `XxHash3`, `XxHash64`, `Crc32`, `AsnReader`, and `AsnWriter`.
    Replace: `System.Random` for cryptographic bytes and `SHA256` for non-cryptographic cache keys.
    Gate: reserve ASN.1 for PKI or signing requirements.

## [8][BOUNDARY_AND_INTEROP]

Nullable flow and syntax hints:
    Owner: `NotNullWhen`, `NotNullIfNotNull`, `MemberNotNull`, `DoesNotReturn`, `StringSyntax`, and trim annotations.
    Replace: null-forgiving scatter after `Try*` or guard APIs.
    Gate: trim annotations only when pursuing a NativeAOT owner.

Boundary validation:
    Owner: `ValidationContext`, `Validator.TryValidateObject`, `[Required]`, and `[Range]` for synchronous DTO checks.
    Reject: using data annotations for domain admission; use Thinktecture and LanguageExt.

Compiler and interop:
    Owner: `CallerArgumentExpression`, `MethodImpl`, `CallerMemberName`, custom interpolated string handlers, `CollectionBuilder`, `StructLayout`, `MemoryMarshal`, `NativeMemory`, and `SuppressGCTransition`.
    Gate: `unsafe` interop requires measured proof and a boundary owner.

Drawing:
    Owner: RhinoWIP app-bundle assemblies for Rhino UI drawing.
    Gate: `System.Drawing.Common` package metadata is compile-only where forwarded types require it; runtime drawing claims need host proof.
