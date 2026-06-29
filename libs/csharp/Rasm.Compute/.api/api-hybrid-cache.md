# [RASM_COMPUTE_API_HYBRID_CACHE]

`Microsoft.Extensions.Caching.Hybrid` is the single two-tier (L1 in-process + L2
distributed) cache substrate: a stampede-collapsing `GetOrCreateAsync` read-through,
tag-grouped invalidation, and a payload-codec seam (`IHybridCacheSerializer<T>`)
that stacks onto the `CommunityToolkit.HighPerformance` `IBufferWriter<byte>` writers
and the `StackExchange.Redis` L2 backing. One `HybridCache` is registered at the
app root over the `CacheLane` descriptors; the model and lowering lanes compose it —
they never register or instantiate a second cache.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.Hybrid`
- package: `Microsoft.Extensions.Caching.Hybrid`
- version: `10.7.0`
- assembly: `Microsoft.Extensions.Caching.Hybrid` (the `HybridCache` abstract contract, `HybridCacheEntryOptions`/`Flags`, and `IHybridCacheSerializer<T>` live in the `Microsoft.Extensions.Caching.Abstractions 10.0.9` companion; the implementation, options, and DI extensions live here)
- license: MIT (.NET Foundation)
- bound asset: `lib/net10.0/Microsoft.Extensions.Caching.Hybrid.dll` (consumer-exact; XML docs shipped)
- namespaces: `Microsoft.Extensions.Caching.Hybrid`, `Microsoft.Extensions.DependencyInjection`
- L2 backing: any registered `IDistributedCache` (e.g. `Microsoft.Extensions.Caching.StackExchangeRedis 10.0.9` over `StackExchange.Redis 3.0.7` via `AddStackExchangeRedisCache`); absent an L2, the cache runs L1-only
- asset: runtime library
- rail: runtime cache

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cache contract and entry options
- rail: runtime cache

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]       | [RAIL]                                                        |
| :-----: | :------------------------------ | :------------------ | :----------------------------------------------------------- |
|  [01]   | `HybridCache`                   | cache contract      | abstract; stampede-collapsing `GetOrCreateAsync`, `SetAsync`, `RemoveAsync`, `RemoveByTagAsync` |
|  [02]   | `HybridCacheEntryOptions`       | entry policy value  | sealed; `init`-only `Expiration` / `LocalCacheExpiration` / `Flags` (record-`with` per call) |
|  [03]   | `HybridCacheEntryFlags`         | entry flag value    | `[Flags]`: per-call local/distributed read+write disable, `DisableUnderlyingData`, `DisableCompression` |
|  [04]   | `IHybridCacheSerializer<T>`     | serializer contract | `Serialize(T, IBufferWriter<byte>)` / `Deserialize(ReadOnlySequence<byte>) → T` |
|  [05]   | `IHybridCacheSerializerFactory` | serializer factory  | `TryCreateSerializer<T>(out IHybridCacheSerializer<T>?)` open-generic discovery |

[PUBLIC_TYPE_SCOPE]: registration and implementation options
- rail: runtime cache

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                                        |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `HybridCacheOptions`           | cache option value | `DefaultEntryOptions`, `MaximumPayloadBytes` (1 MiB), `MaximumKeyLength` (1024), `DisableCompression`, `ReportTagMetrics`, `DistributedCacheServiceKey` |
|  [02]   | `IHybridCacheBuilder`          | builder contract   | `AddHybridCache` return; carries `Services` for serializer admission |
|  [03]   | `HybridCacheServiceExtensions` | service extension  | `AddHybridCache` / `AddKeyedHybridCache` registration         |
|  [04]   | `HybridCacheBuilderExtensions` | builder extension  | `AddSerializer` / `AddSerializerFactory` admission            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cache operations
- rail: runtime cache
- Every read/write carries `HybridCacheEntryOptions? options = null`, `IEnumerable<string>? tags = null`, and `CancellationToken cancellationToken = default`. The `TState` `GetOrCreateAsync` overload is the dense form the model and lowering lanes compose — passing captured state explicitly lets the `factory` delegate stay static (cached, no per-call closure allocation). The `ReadOnlySpan<char>` and `ref DefaultInterpolatedStringHandler` key overloads avoid a key-string allocation on the population path.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY]   | [RAIL]                                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CT, ValueTask<T>> factory, …)` | cache read/write | stampede-collapsed population; static factory, zero closure |
|  [02]   | `GetOrCreateAsync<T>(string key, Func<CT, ValueTask<T>> factory, …)`                        | cache read/write | closure-factory form (state captured in the lambda)         |
|  [03]   | `GetOrCreateAsync<…>(ReadOnlySpan<char> key, …)`                                            | cache read/write | span-key population; no key-string allocation               |
|  [04]   | `GetOrCreateAsync<…>(ref DefaultInterpolatedStringHandler key, …)`                          | cache read/write | interpolated-handler key; zero-alloc composed key           |
|  [05]   | `SetAsync<T>(string key, T value, HybridCacheEntryOptions?, IEnumerable<string>? tags, CT)` | cache write      | explicit store with optional tags for grouped invalidation  |
|  [06]   | `RemoveAsync(string key, CT)` / `RemoveAsync(IEnumerable<string> keys, CT)`                 | key invalidation | single or batch key eviction                                |
|  [07]   | `RemoveByTagAsync(string tag, CT)` / `RemoveByTagAsync(IEnumerable<string> tags, CT)`       | tag invalidation | single or batch grouped eviction                            |

[ENTRYPOINT_SCOPE]: registration and serializer admission
- rail: runtime cache
- `AddHybridCache` returns `IHybridCacheBuilder`; chaining `.AddSerializer*` admits payload codecs. `AddKeyedHybridCache` registers a named cache profile under a DI `serviceKey` (the admitted multi-profile route, never an ad-hoc service lookup). `AddSerializer<T, TImpl>` and `AddSerializerFactory<TImpl>` are the open-generic DI-constructed admission forms.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY]       | [RAIL]                                           |
| :-----: | :------------------------------------------------------------------------- | :------------------- | :----------------------------------------------- |
|  [01]   | `AddHybridCache(this IServiceCollection [, Action<HybridCacheOptions>])`    | service registration | default cache service + builder                  |
|  [02]   | `AddKeyedHybridCache(this IServiceCollection, object? serviceKey [, string optionsName] [, Action<HybridCacheOptions>])` | service registration | keyed cache profile (4 overloads)                |
|  [03]   | `AddSerializer<T>(this IHybridCacheBuilder, IHybridCacheSerializer<T>)`     | builder extension     | concrete serializer instance admission           |
|  [04]   | `AddSerializer<T, TImpl>(this IHybridCacheBuilder) where TImpl : class, IHybridCacheSerializer<T>` | builder extension | DI-constructed typed serializer admission        |
|  [05]   | `AddSerializerFactory(this IHybridCacheBuilder, IHybridCacheSerializerFactory)` / `AddSerializerFactory<TImpl>()` | builder extension | serializer-factory admission (instance + open-generic) |

[ENTRYPOINT_SCOPE]: per-call entry policy values
- rail: runtime cache
- `HybridCacheEntryOptions` is `init`-only; per-call variation is a record-`with` over a lane default (`CacheLane.ModelResult.Entry with { Expiration = precision.NegativeTtl.ToTimeSpan() }`). `HybridCacheEntryFlags` is the per-call tier-bypass — a non-serializable value (a compiled `Delegate`) rides `DisableDistributedCache` (full L2 bypass, both read and write) and lives in L1 by reference through an `[ImmutableObject(true)]` carrier, never `DisableDistributedCacheWrite` alone (which leaves every miss probing a permanently-empty L2).

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]     | [RAIL]                                                  |
| :-----: | :-------------------------------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `HybridCacheEntryOptions.Expiration` / `.LocalCacheExpiration`  | entry policy (init)| total TTL / L1-only TTL                                |
|  [02]   | `HybridCacheEntryOptions.Flags`                                 | entry policy (init)| per-call `HybridCacheEntryFlags` bypass set            |
|  [03]   | `HybridCacheEntryFlags.{DisableLocalCacheRead, …Write, …Cache}` | flag value         | bypass the L1 in-process tier (read/write/both)        |
|  [04]   | `HybridCacheEntryFlags.{DisableDistributedCacheRead, …Write, …Cache}` | flag value   | bypass the L2 distributed tier (read/write/both)       |
|  [05]   | `HybridCacheEntryFlags.DisableUnderlyingData`                   | flag value         | cache-only read; never invoke the factory/data store   |
|  [06]   | `HybridCacheEntryFlags.DisableCompression`                      | flag value         | per-call payload-compression bypass                    |
|  [07]   | `HybridCacheOptions.{MaximumPayloadBytes, MaximumKeyLength}`    | global option      | payload (1 MiB default) and key (1024 default) guards   |
|  [08]   | `HybridCacheOptions.{DisableCompression, ReportTagMetrics}`     | global option      | global compression-off / per-tag eviction metrics      |
|  [09]   | `HybridCacheOptions.DistributedCacheServiceKey`                 | global option      | selects a keyed `IDistributedCache` as the L2 backing  |

## [04]-[IMPLEMENTATION_LAW]

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
