# [SYSTEM_APIS]

System APIs replace local machinery only when they own the concern. They do not replace LanguageExt rails, Thinktecture domain shapes, MathNet algorithms, Rhino geometry, or GH2 data semantics.

## [1]-[SMELL_LOOKUP]

This table is a lookup by repeated local smell.

| [INDEX] | [SMELL]                               | [OWNER]                       |
| :-----: | :------------------------------------ | :---------------------------- |
|   [1]   | manual hex loop                       | `Convert.ToHexString`         |
|   [2]   | regex count allocation                | `Regex.Count`                 |
|   [3]   | regex character filter                | `SearchValues<T>`             |
|   [4]   | mutable public payload                | immutable boundary collection |
|   [5]   | primitive branded value               | generated domain shape        |
|   [6]   | matrix algorithm                      | numeric concept page          |
|   [7]   | `DateTime.Now` delta                  | `TimeProvider`                |
|   [8]   | guard-block argument throw            | throw-helper statics          |
|   [9]   | `string.Split` on a parse path        | `MemoryExtensions.Split`      |
|  [10]   | `Encoding.UTF8.GetBytes` on a literal | `u8` literal                  |

## [2]-[TEXT_AND_WIRE]

[REGEX_GRAMMAR]:
- Owner: `[GeneratedRegex]` for stable structural grammar; plain `Regex` with `RegexOptions.NonBacktracking` for runtime grammar.
- Replace: hand-built static regex, regex count via `Matches().Count`, and ad-hoc boolean match loops.
- Gate: route counts to `Regex.Count` and position scans to `Regex.EnumerateMatches`, whose `ValueMatch` carries index and length with no `Match` allocation; set timeout and culture on `[GeneratedRegex]` when the grammar needs them.
- Reject: expecting `[GeneratedRegex]` to emit custom IL for `RegexOptions.Compiled`.

[SEARCH_AND_SPLIT]:
- Owner: `SearchValues<char>`, `SearchValues<byte>`, and `SearchValues<string>` with `MemoryExtensions.ContainsAny`, `ContainsAnyExcept`, `IndexOfAny`, `IndexOfAnyExcept`, `Split`, and `SplitAny`.
- Replace: regex used only for allow/deny character checks, multi-substring scans chained through `IndexOf`, and `string.Split` on parse paths.
- Gate: construct `SearchValues<T>` once as a static field beside the policy; `SearchValues<string>` requires an explicit ordinal-family `StringComparison`.
- Rule: `Split` and `SplitAny` yield `Range` values over the source span, so slicing replaces substring materialization end to end.

[FORMATTING_AND_PARSING]:
- Owner: `CompositeFormat`, `string.Create`, `Span<char>.TryWrite` interpolation, and the `ISpanFormattable` and `ISpanParsable<T>` pair with invariant `TryParse`.
- Replace: repeated format-string parsing, `StringBuilder` for fixed-length construction, and culture-ambient persisted values.
- Gate: use `CultureInfo.InvariantCulture` for wire and persistence.
- Rule: domain types that appear in formatted output implement `ISpanFormattable` and compose into `TryWrite` holes without allocation.

[UTF8_PIPELINE]:
- Owner: `u8` literals, `Utf8.TryWrite` interpolation, the `IUtf8SpanFormattable` and `IUtf8SpanParsable<T>` pair, `Utf8.FromUtf16` and `Utf8.ToUtf16`, and `Encoding.TryGetBytes` and `TryGetChars`.
- Replace: `Encoding.UTF8.GetBytes` on literals, UTF-16 round trips on UTF-8 paths, and throwing encode calls where destination sizing is uncertain.
- Gate: chunked transcoding flows through the `OperationStatus` forms; `IUtf8SpanParsable<T>` is independent of `ISpanParsable<T>` and needs its own constraint.

[JSON]:
- Owner: `[JsonSerializable]`, `JsonSerializerContext`, `JsonSourceGenerationOptions`, and `JsonTypeInfo<T>` overloads.
- Replace: reflection-heavy serializers for known contract types, hand-rolled JSON, and custom converters written only to rename or filter properties.
- Gate: boundary contracts only; runtime property filtering goes through `WithAddedModifier`, foreign-type fallback through `JsonTypeInfoResolver.Combine`, and identifier casing through the `JsonNamingPolicy` statics.
- Rule: strict protocol types set `JsonUnmappedMemberHandling.Disallow` so schema drift surfaces instead of silently dropping fields.
- Reject: using JSON contracts as domain admission.

[ENCODING]:
- Owner: `Encoding`, `Rune`, `Ascii`, `Base64Url`, `Convert.ToHexString`, and `ToHexStringLower`.
- Replace: manual hex loops, unsafe ASCII checks, URL-base64 helpers, and `char` predicates applied to surrogate halves.
- Gate: validate allowed code points with one text policy, not scattered predicates.
- Rule: codepoint-level work routes through `Rune`; user-perceived characters route through `StringInfo` text elements.

## [3]-[COLLECTIONS_AND_LOOKUP]

[READ_MOSTLY_LOOKUP]:
- Owner: `FrozenDictionary`, `FrozenSet`, and `GetAlternateLookup<ReadOnlySpan<char>>` for span-keyed probes.
- Replace: rebuilt static dictionaries, string-key lookup copied per call, and `ToString` materialization before a keyed probe.
- Gate: pass an explicit comparer; alternate lookup requires an ordinal-family comparer implementing `IAlternateEqualityComparer`, and `TryGetAlternateLookup` is the non-throwing acquisition.
- Rule: the mutable `Dictionary` and `HashSet` alternate lookups also expose `TryAdd` and `Remove`, so span-keyed mutation needs no interim string.

[IMMUTABLE_BOUNDARY_PAYLOAD]:
- Owner: `ImmutableArray`, `ImmutableDictionary`, and `ImmutableHashSet`.
- Replace: mutable public collection payloads outside LanguageExt rails and `ImmutableList<T>` on indexed read paths.
- Rule: `default(ImmutableArray<T>)` is a distinct throwing state guarded by `IsDefaultOrEmpty`; exact-capacity builders hand off through `MoveToImmutable` with zero copy; `AsSpan` reads the backing array without exposing it.
- Boundary: domain sequence identity is not a BCL replacement concern.

[ORDERING_AND_SCHEDULING]:
- Owner: `OrderedDictionary<K,V>` for insertion order and `PriorityQueue<TElement,TPriority>` for heap scheduling.
- Replace: sort-then-pop and manual insertion-order maps.
- Reject: treating `OrderedDictionary<K,V>` as an LRU cache.

[GROUPING_AND_SET_ALGEBRA]:
- Owner: `ToLookup`, `DistinctBy`, `CountBy`, `AggregateBy`, `ExceptBy`, `IntersectBy`, `UnionBy`, `Index`, `LeftJoin`, and `RightJoin`.
- Replace: repeated `GroupBy`, loop dedup, manual index counters, the `GroupJoin`-`SelectMany`-`DefaultIfEmpty` join idiom, and comparer-free keyed set operations.
- Gate: declare key comparer policy when default equality is not the domain rule.
- Rule: the same operator names transpose to `IAsyncEnumerable<T>` through the BCL `AsyncEnumerable` surface; `WhereAwait`-style spellings belong to an external package and are the rejected form.

[COLLECTION_INTERNALS]:
- Owner: `CollectionsMarshal.GetValueRefOrAddDefault`, `GetValueRefOrNullRef`, `AsSpan`, and `SetCount` at measured boundaries.
- Replace: `TryGetValue`-then-`Add` double probes and zero-initialized scratch buffers filled immediately after allocation.
- Gate: pin list size before `AsSpan` because any resizing `Add` invalidates the span; check `GetValueRefOrNullRef` results with `Unsafe.IsNullRef`.
- Reject: domain public identity built from mutable dictionary internals.

## [4]-[EQUALITY_AND_IDENTITY]

[DEFAULT_EQUALITY]:
- Owner: `EqualityComparer<T>.Default`, `EqualityComparer<T>.Create`, `HashCode`, records, and record structs.
- Replace: hand-written comparer classes and XOR or prime hash combining for non-branded data.
- Gate: hash with the same comparer policy used by lookup.
- Reject: persisting `HashCode` output or using it as stable file identity because the output is process-randomized.

[BRANDED_EQUALITY]:
- Owner: Thinktecture value objects, `[KeyMemberEqualityComparer]`, and `[KeyMemberComparer]`.
- Replace: primitive branded string or scalar equality copied across files.
- Boundary: generated domain shapes own branded identity.

[HOST_IDENTITY]:
- Owner: `RuntimeHelpers.GetHashCode(object)` for live host reference identity.
- Reject: persisting `GetHashCode()` output or using it as a stable file key.

## [5]-[NUMERICS]

[SCALAR_AND_GENERIC_MATH]:
- Owner: `Math`, `MathF`, `Half`, BCL `Complex`, and the narrowest generic math constraint such as `INumberBase<T>`, `IBinaryInteger<T>`, or `IFloatingPointIeee754<T>`.
- Replace: hand-rolled per-type scalar math and `Convert`-chain numeric conversions; `CreateChecked`, `CreateSaturating`, and `CreateTruncating` carry the overflow policy in the name.
- Rule: `INumber<T>` subsumes the span parse, span format, UTF-8, arithmetic, and comparison interfaces; constraining to a fragment forecloses the operators a numeric kernel later needs.
- Boundary: matrix and solver algorithms are not scalar replacement concerns.

[VECTOR_AND_TENSOR_PRIMITIVES]:
- Owner: BCL `Vector<T>`, `Vector128/256/512<T>`, `Shuffle`, and `TensorPrimitives`.
- Replace: any flat numeric loop whose body is pure arithmetic, math calls, or element casts; element-wise maps, reductions, argmax tracking, and the `ConvertSaturating` conversion family are owned surfaces.
- Gate: adoption follows measured proof; `System.Numerics.Tensors` requires an explicit package consumer.

[TYPE_COLLISION]:
- Rule: BCL `Vector<T>` is SIMD, MathNet `Vector<T>` is linear algebra, and BCL `Complex` is not MathNet `Complex32`.
- Gate: qualify namespaces at boundaries where both meanings appear.

[HOST_GEOMETRY]:
- Owner: RhinoCommon for geometry, units, tolerances, transforms, and topology.
- Reject: replacing native host geometry semantics with generic numeric helpers.

## [6]-[RUNTIME_AND_OBSERVABILITY]

[TIME]:
- Owner: `TimeProvider`, `PeriodicTimer`, `Stopwatch.GetTimestamp`, and `Stopwatch.GetElapsedTime`.
- Replace: `DateTime.Now` deltas, manual tick math, and timers inside domain logic.
- Rule: `GetUtcNow` and `GetTimestamp` are the only override points, and every time-accepting async primitive — `Task.Delay`, `WaitAsync`, `PeriodicTimer`, `CancellationTokenSource` — follows the injected provider, so time is testable through one seam.
- Gate: `PeriodicTimer` belongs at boundary adapters; it never skips ticks, so overrun policy is the loop body's decision.

[OBSERVABILITY]:
- Owner: `ActivitySource`, `Activity`, `Meter`, counters, histograms, gauges, `TagList`, and `InstrumentAdvice`.
- Replace: ad-hoc `DiagnosticSource`, `TraceSource`, and manual counters.
- Gate: centralize source handles; do not create handler-local telemetry owners.

[LOGGING]:
- Owner: `[LoggerMessage]` and `ILogger` after `Microsoft.Extensions.Logging.Abstractions` has a source-backed owner route.
- Replace: `Console.WriteLine` and string-interpolated logs at runtime boundaries.
- Gate: package adoption is a graph fact, not a replacement decision.
- Boundary: no domain logging rail appears before the boundary owner exists.

[LOCKING]:
- Owner: `Lock`, `Interlocked`, and `Volatile`.
- Replace: private `object` locks, CAS state widened to `int` where narrow integer overloads exist, and full fences where one-directional barriers suffice.
- Rule: the `lock` statement over a `Lock` field lowers to `EnterScope` scopes only when the statement targets the `Lock` instance directly; the sealed type neither boxes nor serves as `SyncRoot`.
- Gate: never hold `Lock` across `await`; `TryEnter` returns `bool`, so timeout paths pair `Enter` with explicit `Exit`.

[CANCELLATION]:
- Owner: `CancellationToken`, `CancellationTokenSource.CreateLinkedTokenSource`, `CancelAsync`, `TryReset`, and `UnsafeRegister`.
- Replace: timeout token scatter, per-cycle source allocation, and `Register` closures that capture only the token.
- Rule: `CancelAsync` dispatches registered callbacks to the thread pool instead of inline on the caller — the re-entrancy-safe form when callbacks launch further work.
- Gate: annotate async streams with `[EnumeratorCancellation]`; `TryReset` reuses a source only when cancellation was never requested.

[TASK_COMPOSITION]:
- Owner: `Task.WhenEach`, the span overloads of `WhenAll` and `WhenAny`, `WaitAsync`, `TaskCompletionSource<T>`, and framework-proven rate limiters.
- Replace: `WhenAny` loops that rebuild task collections per iteration, hand-rolled work queues, and task completions without asynchronous continuations.
- Rule: `WhenEach` yields tasks in completion order; `WaitAsync` bounds the wait, not the work — the underlying task keeps running past timeout or cancellation.

[CHANNELS]:
- Owner: `Channel.CreateBounded`, `CreateUnbounded`, `CreateUnboundedPrioritized`, and the reader and writer surfaces through `ReadAllAsync`.
- Replace: `BlockingCollection<T>` and manual `SemaphoreSlim` producer-consumer loops.
- Gate: bounded options carry the backpressure decision through `FullMode` and the `itemDropped` callback; `Complete` terminates `ReadAllAsync` naturally.

## [7]-[IO_BUFFERS_AND_INTEGRITY]

[PATH_AND_FILE_IO]:
- Owner: `Path.Join`, `Path.Exists`, `File.OpenHandle`, `RandomAccess`, `FileStreamOptions`, `ReadExactlyAsync`, `ReadAtLeastAsync`, and `File.ReadLinesAsync`.
- Replace: seek/read loops, ad-hoc `FileStream` construction, bifurcated `File.Exists` and `Directory.Exists` checks, and `Path.Combine` where a rooted right segment must not discard the left.
- Rule: `RandomAccess` is stateless and offset-addressed — position is caller-owned, so concurrent readers on distinct offsets need no lock; a `FileStream` opened only for `Length` or `Flush` is the rejected form.
- Gate: validate wire lengths against bounded caps before allocation; async handle IO requires `FileOptions.Asynchronous` at open, or async calls silently degrade to pool emulation.

[BUFFERS_AND_PIPELINES]:
- Owner: `ArrayPool<T>`, `MemoryPool<T>`, `ReadOnlySpan<T>`, `ReadOnlyMemory<T>`, `ReadOnlySequence<T>`, `SequenceReader<T>`, `BinaryPrimitives`, `PipeReader`, and `PipeWriter`.
- Replace: unbounded `MemoryStream`, `new byte[n]` scratch buffers, and `BitConverter` for endian wire frames.
- Gate: return rented arrays in `finally`; carry `IMemoryOwner<T>` across `await`.

[INTEGRITY]:
- Owner: `CryptographicOperations.HashData` and `HmacData` by algorithm name, the per-algorithm one-shots, `FixedTimeEquals`, `RandomNumberGenerator`, `XxHash3`, `XxHash64`, `Crc32`, `AsnReader`, and `AsnWriter`.
- Replace: constructed hash or HMAC instances used once and discarded, `System.Random` for cryptographic bytes, and `SHA256` for non-cryptographic cache keys.
- Gate: guard `SHA3` and `SHAKE` one-shots behind their `IsSupported` statics; reserve ASN.1 for PKI or signing requirements.
- Rule: `Random.Shared` and `RandomNumberGenerator` expose the same `GetItems`, `GetString`, `Shuffle`, and `Fill` surface, so the owning type is the entire security decision.

## [8]-[BOUNDARY_AND_INTEROP]

[NULLABLE_FLOW_AND_SYNTAX_HINTS]:
- Owner: `NotNullWhen`, `NotNullIfNotNull`, `MemberNotNull`, `DoesNotReturn`, `StringSyntax`, and trim annotations.
- Replace: null-forgiving scatter after `Try*` or guard APIs.
- Gate: trim annotations only when pursuing a NativeAOT owner.

[ARGUMENT_GUARDS]:
- Owner: `ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrEmpty` and `ThrowIfNullOrWhiteSpace`, the `ArgumentOutOfRangeException.ThrowIf` comparison family, and `ObjectDisposedException.ThrowIf`.
- Replace: guard-block `if`-`throw` argument checks and hand-formatted parameter-name messages.
- Rule: `[CallerArgumentExpression]` captures the argument expression into the exception automatically; the numeric guards constrain on `INumberBase<T>`, so signed and unsigned bounds share one spelling.
- Boundary: these guard host and library seams only; domain admission stays on generated owners and typed rails.

[BOUNDARY_VALIDATION]:
- Owner: `ValidationContext`, `Validator.TryValidateObject`, `[Required]`, and `[Range]` for synchronous DTO checks.
- Reject: using data annotations for domain admission; use Thinktecture and LanguageExt.

[COMPILER_AND_INTEROP]:
- Owner: `CallerArgumentExpression`, `MethodImpl`, `CallerMemberName`, custom interpolated string handlers, `CollectionBuilder`, `StructLayout`, `MemoryMarshal`, `NativeMemory`, and `SuppressGCTransition`.
- Gate: `unsafe` interop requires measured proof and a boundary owner.

[DRAWING]:
- Owner: RhinoWIP app-bundle assemblies for Rhino UI drawing.
- Gate: `System.Drawing.Common` package metadata is compile-only where forwarded types require it; runtime drawing claims need host proof.
