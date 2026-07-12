# [RASM_COMPUTE_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` is the single two-tier (L1 in-process + L2
distributed) cache substrate: a stampede-collapsing `GetOrCreateAsync` read-through,
tag-grouped invalidation, and a payload-codec seam (`IHybridCacheSerializer<T>`)
that stacks onto the `CommunityToolkit.HighPerformance` `IBufferWriter<byte>` writers
and the `StackExchange.Redis` L2 backing. One `HybridCache` is registered at the
app root over the `CacheLane` descriptors; the model and lowering lanes compose it —
they never register or instantiate a second cache. The substrate
canonical member catalog is `libs/csharp/.api/api-hybrid-cache.md`; this overlay
carries only the Compute delta — the lane bindings and per-call policy law the
model and lowering pages compose.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-hybrid-cache.md`
- the contract/options/serializer type roster, the operation and registration call-shape tables, and the package/asset facts live on the substrate catalog — this overlay never re-states them
- rail: runtime cache

## [02]-[COMPUTE_BINDINGS]

[COMPUTE_BINDINGS]:
- Every read/write carries `HybridCacheEntryOptions? options = null`, `IEnumerable<string>? tags = null`, and `CancellationToken cancellationToken = default`. The `TState` `GetOrCreateAsync` overload is the dense form the model and lowering lanes compose — passing captured state explicitly lets the `factory` delegate stay static (cached, no per-call closure allocation). The `ReadOnlySpan<char>` and `ref DefaultInterpolatedStringHandler` key overloads avoid a key-string allocation on the population path.
- `HybridCacheEntryOptions` is a sealed `init`-only CLASS, not a record — `with` does not compile; per-call variation constructs a fresh options object seeded from the lane default (`new HybridCacheEntryOptions { Expiration = precision.NegativeTtl.ToTimeSpan(), LocalCacheExpiration = lane.Entry.LocalCacheExpiration, Flags = lane.Entry.Flags }`).
- `HybridCacheEntryFlags` is the per-call tier-bypass — a non-serializable value (a compiled `Delegate` from `Symbolic/lowering#COMPILED_EXPR`) rides `DisableDistributedCache` (full L2 bypass, both read and write) and lives in L1 by reference through an `[ImmutableObject(true)]` carrier, never `DisableDistributedCacheWrite` alone (which leaves every miss probing a permanently-empty L2).
- One `HybridCache` registers at the app root over the `CacheLane` descriptors (`Rasm.Persistence/Query/cache`); the model and lowering lanes compose it — they never register or instantiate a second cache.

## [03]-[IMPLEMENTATION_LAW]

[CACHE_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.DependencyInjection`
- contract root: `HybridCache` (abstract, in the `Abstractions` companion)
- implementation root: `Microsoft.Extensions.Caching.Hybrid` (registered via `AddHybridCache`)
- two tiers: L1 in-process (always) + L2 distributed (a registered `IDistributedCache`, optional); per-call `HybridCacheEntryFlags` and per-options `LocalCacheExpiration` partition the tiers
- registration surface: default (`AddHybridCache`) and keyed (`AddKeyedHybridCache`, 4 overloads) profiles
- policy surface: `HybridCacheOptions` — `DefaultEntryOptions`, `MaximumPayloadBytes` (1 MiB), `MaximumKeyLength` (1024), `DisableCompression`, `ReportTagMetrics`, `DistributedCacheServiceKey`
- entry policy: `HybridCacheEntryOptions` `init`-only `Expiration`/`LocalCacheExpiration`/`Flags`
- serializer surface: `IHybridCacheSerializer<T>` per type + `IHybridCacheSerializerFactory` open-generic discovery
- invalidation surface: `RemoveAsync` (key, single+batch) and `RemoveByTagAsync` (tag, single+batch)

[SERIALIZER_STACKING]:
- `IHybridCacheSerializer<T>.Serialize(T, IBufferWriter<byte>)` writes the L2 payload directly into the `CommunityToolkit.HighPerformance` `ArrayPoolBufferWriter<byte>` (or any `IBufferWriter<byte>`), and `Deserialize(ReadOnlySequence<byte>) → T` reads back from the writer's `WrittenMemory` as a `ReadOnlySequence<byte>` — so a custom codec round-trips with zero intermediate array, the same convergence seam the codec lane uses.
- a value durably serializable across a process boundary stores both tiers from one representation; a non-serializable value (a live `Delegate`) is wrapped in an `[ImmutableObject(true)]` carrier so HybridCache holds it in L1 by reference with no serialize/deserialize round-trip and carries `HybridCacheEntryFlags.DisableDistributedCache` to bypass L2 entirely (both read and write) — HybridCache keeps one representation per key (no separate L2 seed projection), so cross-process reuse is deterministic re-derivation off the content-addressed key, never a serialized delegate.
- compression is on by default unless `DisableCompression` (global or per-call) is set; `MaximumPayloadBytes` rejects an oversized serialized payload.

[L2_BACKING]:
- the L2 distributed tier is whatever `IDistributedCache` is registered; `AddStackExchangeRedisCache(Action<RedisCacheOptions>)` (`Microsoft.Extensions.Caching.StackExchangeRedis` over `StackExchange.Redis`) is the Redis backing, and `DistributedCacheServiceKey` selects a keyed `IDistributedCache` when more than one is registered.
- absent any `IDistributedCache`, the cache runs L1-only with no error; the L2 tier is additive infrastructure, not a contract change at the call site.

[LOCAL_ADMISSION]:
- Hybrid cache is a runtime policy surface, not a domain repository.
- Cache keys, tags, entry options, and serializer policy enter as values; the `CachePolicy` intent is a `[SmartEnum]` row, never a boolean flag.
- Keyed cache registration represents an admitted cache profile, not ad-hoc service lookup.
- Stampede control stays behind the `GetOrCreateAsync` single-flight; a caller that compiles-then-caches in two steps is the rejected form because it duplicates the population lock the read-through already owns.
- The `TState` `GetOrCreateAsync` overload is the default population form (static factory, zero per-call closure); a closure factory is admitted only where no reusable state object exists.
- Tag invalidation is an explicit cache capability and never substitutes for durable store integrity.
- Cache keys derive from the suite `System.IO.Hashing` `XxHash128`/`XxHash3` content identity — never a path-keyed or display-string key.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Caching.Hybrid` (+ `Abstractions`; L2 via `…StackExchangeRedis`/`StackExchange.Redis`)
- Owns: two-tier cache registration, stampede-collapsed read-through, tag invalidation, payload-codec policy
- Accept: cache policy as runtime data, the `TState` static-factory population, `init`-only entry options with per-call flag bypass, an `IBufferWriter<byte>`/`ReadOnlySequence<byte>` custom serializer, an optional Redis L2 backing
- Reject: static cache clients and hidden invalidation channels; a second `HybridCache` registration beside the app-root one; a closure factory where a `TState` form fits; a path-keyed cache key; a duplicated call-site population lock
